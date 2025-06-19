using FolderMMYYSorter_2.MVVM.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Win32 = System.Windows;

namespace FolderMMYYSorter_2.IO
{
    class FileExplorer : INotifyPropertyChanged
    {
        private string _CurrentDirectory = "Select a folder.";
        public string CurrentDirectory
        {
            get { return _CurrentDirectory; }
            set
            {
                if (Directory.Exists(value) && (_CurrentDirectory != value))
                {
                    // Only update this property if the input 'value' is valid
                    _CurrentDirectory = value;
                }
            }
        }

        private string _Summary = "placeholder summary";
        public string Summary
        {
            get { return _Summary; }
            set
            {
                if (_Summary != value)
                {
                    // Only update this property if the input 'value' is valid
                    _Summary = value;
                }
            }
        }

        private string _FolderName = "";
        public string FolderName
        {
            get { return _FolderName; }
            set
            {
                if (_FolderName != value)
                {
                    // Only update this property if the input 'value' is valid
                    _FolderName = value;
                    generateSummary();
                }
            }
        }


        private string _DestDirectory = "Select a folder.";
        public string DestDirectory
        {
            get { return _DestDirectory; }
            set
            {
                if (Directory.Exists(value) && (_DestDirectory != value))
                {
                    // Only update this property if the input 'value' is valid
                    _DestDirectory = value;
                    generateSummary();
                }
            }
        }

        private bool _isUsingSQLDB = false;
        public bool isUsingSQLDB
        {
            get { return _isUsingSQLDB; }
            set
            {
                if (_isUsingSQLDB != value)
                {
                    // Only update this property if the input 'value' is valid
                    _isUsingSQLDB = value;
                    OnPropertyChanged(nameof(isUsingSQLDB));
                    if (_isUsingSQLDB)
                    {
                        Debug.WriteLine(_SqlHelper.GetDatabaseNames("localhost"));
                    }
                }
            }
        }


        public bool isModeSorting { get; set; }
        // as opposed to emptying mode

        public bool isModeSubFolder { get; set; }

        public List<DirFileModel> baseDirsList;
        public List<DirFileModel> subDirsList;

        public bool isExecuting = false;


        public ObservableCollection<DirFileModel> DispFiles { get; set; }


        // event handler to clear message bar when send message
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public SqlHelper _SqlHelper { get; set; }



        public FileExplorer()
        {
            OnPropertyChanged(nameof(CurrentDirectory)); // triggers the UI update

            DispFiles = new ObservableCollection<DirFileModel>();

            _SqlHelper = new SqlHelper();
        }


        public void generateSummary()
        {
            var destDir = Path.Combine(DestDirectory, FolderName);
            var _sum = "";

            _sum += "Source:\n";
            _sum += "Data to copy: ";
            _sum += Double.Round(getSourceSize(),2) + " GB";

            _sum += "\n\n";
            _sum += "Output Directory:\n";
            _sum += destDir;

            Summary = _sum;
        }


        private double getSourceSize()
        {
            List<DirFileModel> src = [];
            if (isModeSubFolder)
                src = subDirsList;
            else
                src = baseDirsList;

            double output = 0;

            // src is empty
            if (src == null || src.Count == 0) return 0;

            foreach (var item in src){
                if (item.isFolder) output += DirSize(new DirectoryInfo(item.Path));
                else output += new FileInfo(item.Path).Length;
            }

            return output/1000000000.0;
        }

        public static long DirSize(DirectoryInfo d)
        {
            long size = 0;
            // Add file sizes.
            FileInfo[] fis = d.GetFiles();
            foreach (FileInfo fi in fis)
            {
                size += fi.Length;
            }
            // Add subdirectory sizes.
            DirectoryInfo[] dis = d.GetDirectories();
            foreach (DirectoryInfo di in dis)
            {
                size += DirSize(di);
            }
            return size;
        }


        public async void fileDialog()
        {
            FolderBrowserDialog openFoldDlg = new FolderBrowserDialog();

            openFoldDlg.InitialDirectory = @"C:\Temp\";

            openFoldDlg.Description =
                "Select the directory that you want to perform actions on.";

            // Do not allow the user to create new files via the FolderBrowserDialog.
            openFoldDlg.ShowNewFolderButton = false;

            // Show dialog and check result
            System.Windows.Forms.DialogResult result = openFoldDlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Dialog was accepted
                CurrentDirectory = "Loading...."; // display loading while awaiting
                OnPropertyChanged(nameof(CurrentDirectory)); // triggers the UI update
                CurrentDirectory = openFoldDlg.SelectedPath;

                await UpdateDisplayedFilesAsync();
                OnPropertyChanged(nameof(CurrentDirectory)); // triggers the UI update
                
            }
        }

        public async void destFileDialog()
        {
            FolderBrowserDialog openFoldDlg = new FolderBrowserDialog();

            openFoldDlg.InitialDirectory = @"C:\Temp\";

            openFoldDlg.Description =
                "Select the output directory";

            openFoldDlg.ShowNewFolderButton = true;

            // Show dialog and check result
            System.Windows.Forms.DialogResult result = openFoldDlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                // Dialog was accepted
                DestDirectory = openFoldDlg.SelectedPath;
                OnPropertyChanged(nameof(DestDirectory)); // triggers the UI update

            }
        }

        /* TODO: rename restructure this code */
        // function is named update displayed files
        // but main function is to read CurrentDir
        // updating the DispFiles could be a separate function 
        // that is also called by P2_options page
        public async Task UpdateDisplayedFilesAsync()
        {
            if (isExecuting) return;
            isExecuting = true;

            baseDirsList = new List<DirFileModel>();
            subDirsList = new List<DirFileModel>();

            var baseBag = new ConcurrentBag<DirFileModel>();
            var subBag = new ConcurrentBag<DirFileModel>();

            await Task.Run(() =>
            {
                // 1. Process all files and folders in the chosen directory (top level)
                var baseEntries = new DirectoryInfo(CurrentDirectory).EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);
                Parallel.ForEach(baseEntries, info =>
                {
                    baseBag.Add(new DirFileModel
                    {
                        name = info.Name,
                        CreationDate = info.CreationTime,
                        isFolder = info is DirectoryInfo,
                        Path = info.FullName
                    });
                });

                // 2. Process all files and folders one layer into each folder of the chosen directory
                var subDirs = new DirectoryInfo(CurrentDirectory).EnumerateDirectories("*", SearchOption.TopDirectoryOnly);
                Parallel.ForEach(subDirs, subDir =>
                {
                    var subEntries = subDir.EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly);
                    foreach (var info in subEntries)
                    {
                        subBag.Add(new DirFileModel
                        {
                            name = info.Name,
                            CreationDate = info.CreationTime,
                            isFolder = info is DirectoryInfo,
                            Path = info.FullName
                        });
                    }
                });
            });

            baseDirsList = baseBag.ToList();
            subDirsList = subBag.ToList();

            isExecuting = false;

            generateSummary();

            // Update UI
            Win32.Application.Current.Dispatcher.Invoke(() =>
            {
                DispFiles.Clear();

                foreach (var item in baseDirsList)
                {
                    DispFiles.Add(item);
                }
            });
        }

        public bool isCurrDirValid()
        {
            return Directory.Exists(CurrentDirectory);
        }

        public void updateIsSubFolder(bool input)
        {
            isModeSubFolder = input;

            if (subDirsList == null && baseDirsList == null) return;

            if (isModeSubFolder)
            {
                Win32.Application.Current.Dispatcher.Invoke(() =>
                {
                    DispFiles.Clear();

                    foreach (var dir in subDirsList)
                        DispFiles.Add(dir);
                });
            } else
            {
                Win32.Application.Current.Dispatcher.Invoke(() =>
                {
                    DispFiles.Clear();

                    foreach (var dir in baseDirsList)
                        DispFiles.Add(dir);
                });
            }
        }


        public async Task<bool> execute(IProgress<int> ProgressValue = null, IProgress<string> CurrentItem = null)
        {
            // if isModeSubFolder then access from subDirsList
            // else access from baseDirsList

            List<DirFileModel> src = isModeSubFolder 
                                        ? subDirsList 
                                        : baseDirsList;

            var dst = Path.Combine(DestDirectory, FolderName);

            List<string> errors = [];

            if (src == null || src.Count == 0) errors.Add("No items found in source directory");
            if (!Directory.Exists(DestDirectory)) errors.Add("Destination is invalid");
            if (FolderName == "") errors.Add("Fill in a folder name");
            if (isUsingSQLDB && !_SqlHelper.hasSQLData()) errors.Add("no SQL data found");

            if (errors.Count > 0)
            {
                MessageBox.Show(
                    $"Missing requirements:\n\n{string.Join("\n• ", errors)}",
                    "Action Cannot Proceed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return false;
            }



            // for progress bar
            int totalItems = src.Count;
            int processedItems = 0;

            var exceptions = new ConcurrentBag<Exception>();

            int Gov = 0; int IC = 0; int Unknown = 0;


            // Process in parallel
            await Task.Run(() =>
            {
                Parallel.ForEach(src, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, file =>
                {
                    try
                    {
                        var yyMM = file.CreationDate.ToString("yyMM");

                        // get fileType from Sql server data
                        var fileType = isUsingSQLDB 
                                        ? _SqlHelper.filetypeOf(file.name) 
                                        : ""; // "" safe for Path.Combine

                        // remove after debug
                        switch (fileType)
                        {
                            case "IC":
                                IC += 1;
                                break;
                            case "Gov":
                                Gov += 1;
                                break;
                            case "Unknown":
                                Unknown += 1;
                                break;
                            default:
                                break;
                        }
                        // remove after debug


                        var targetDir = Path.Combine(dst, fileType, yyMM);

                        // Ensure the directory exists (safe to call repeatedly)
                        Directory.CreateDirectory(targetDir);

                        string targetPath = Path.Combine(targetDir, file.name);

                        if (file.isFolder)
                            CopyDirectory(file.Path, targetPath, true);
                        else
                            File.Copy(file.Path, targetPath, overwrite: true);

                        // update UI periodically (not with every object)
                        // otherwise UI will simply seize
                        int reportFrequency = 10;
                        int newCount = Interlocked.Increment(ref processedItems);
                        if (newCount % reportFrequency == 0 || newCount == totalItems)
                        {
                            ProgressValue?.Report((newCount * 100) / totalItems);
                            CurrentItem?.Report(file.name);
                        }

                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                });
            });

            // Handle any exceptions
            if (!exceptions.IsEmpty)
            {
                throw new AggregateException(exceptions);
            }

            Debug.WriteLine("Total Files copied");
            Debug.WriteLine("IC: " + IC);
            Debug.WriteLine("Gov: " + Gov);
            Debug.WriteLine("Unknown: " + Unknown);

            return true;
        }


        // copied from C# .NET documentation
        // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            // Get information about the source directory
            var dir = new DirectoryInfo(sourceDir);

            // Check if the source directory exists
            if (!dir.Exists)
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            // Cache directories before we start copying
            DirectoryInfo[] dirs = dir.GetDirectories();

            // Create the destination directory
            Directory.CreateDirectory(destinationDir);

            // Get the files in the source directory and copy to the destination directory
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, overwrite:true);
            }

            // If recursive and copying subdirectories, recursively call this method
            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
