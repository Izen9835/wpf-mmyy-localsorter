using FolderMMYYSorter_2.MVVM.Model;
using System;
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
                    updateDisplayedFiles();
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
                }
            }
        }

        private string _HelpText = "Select a folder above.";
        public string HelpText
        {
            get { return _HelpText; }
            set
            {
                if (_HelpText != value)
                {
                    _HelpText = value;
                    OnPropertyChanged(nameof(HelpText)); // triggers the UI update
                    CommandManager.InvalidateRequerySuggested(); // Force command refresh

                }
            }
        }


        public bool isModeSorting { get; set; }
        // as opposed to emptying mode

        public bool isModeSubFolder { get; set; }

        public List<DirFileModel> baseDirsList;
        public List<DirFileModel> subDirsList;


        public ObservableCollection<DirFileModel> DispFiles { get; set; }


        private IEnumerable<IGrouping<string, DirFileModel>> mmYYGroups;

        // event handler to clear message bar when send message
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        public FileExplorer()
        {
            OnPropertyChanged(nameof(CurrentDirectory)); // triggers the UI update

            DispFiles = new ObservableCollection<DirFileModel>();
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

                await updateDisplayedFiles();
                OnPropertyChanged(nameof(CurrentDirectory)); // triggers the UI update
                HelpText = "Select a mode.";
                
            }
        }

        public async void destFileDialog()
        {
            FolderBrowserDialog openFoldDlg = new FolderBrowserDialog();

            openFoldDlg.InitialDirectory = @"C:\Temp\";

            openFoldDlg.Description =
                "Select the directory that you want to perform actions on.";

            // Do not allow the user to create new files via the FolderBrowserDialog.
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
        public async Task updateDisplayedFiles()
        {
            // list directories first
            string[] dirs = await Task.Run(() => Directory.GetDirectories(CurrentDirectory));
            baseDirsList = await Task.Run(() => dirs.Select(
                dirPath => new DirFileModel
            {
                name = System.IO.Path.GetFileName(dirPath),
                CreationDate = new FileInfo(dirPath).LastWriteTime,
                isFolder = true,
                Path = dirPath,
            }).ToList());


            // list files next
            string[] files = await Task.Run(() => Directory.GetFiles(CurrentDirectory));
            baseDirsList.AddRange(await Task.Run(() => files.Select( // considering using EnumerateFiles EnumerateDirectories instead
                filePath => new DirFileModel
                {
                    name = System.IO.Path.GetFileName(filePath),
                    CreationDate = new FileInfo(filePath).LastWriteTime,
                    isFolder = false,
                    Path = filePath,
                }).ToList()));


            // list subdirectories folders
            var subDirs = await Task.Run(() => dirs.SelectMany(dir => Directory.GetDirectories(dir).ToList()));
            subDirsList = await Task.Run(() => subDirs.Select(
                dirPath => new DirFileModel
                {
                    name = System.IO.Path.GetFileName(dirPath),
                    CreationDate = new FileInfo(dirPath).LastWriteTime,
                    isFolder = true,
                    Path = dirPath,
                }).ToList());

            // list subdirectories files next
            var subFiles = await Task.Run(() => dirs.SelectMany(file => Directory.GetFiles(file).ToList()));
            subDirsList.AddRange(await Task.Run(() => subFiles.Select( // considering using EnumerateFiles EnumerateDirectories instead
                filePath => new DirFileModel
                {
                    name = System.IO.Path.GetFileName(filePath),
                    CreationDate = new FileInfo(filePath).LastWriteTime,
                    isFolder = false,
                    Path = filePath,
                }).ToList()));


            // do as a batch to save resources
            // display all directories and files
            Win32.Application.Current.Dispatcher.Invoke(() =>
            {
                DispFiles.Clear();

                foreach (var dir in baseDirsList)
                    DispFiles.Add(dir);

            });

        }

        public bool isCurrDirValid()
        {
            return Directory.Exists(CurrentDirectory);
        }

        public void updateMode(bool input)
        {
            isModeSorting = input;
            Task.Run(() => updateHelpText());
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

        private bool IsValidMMYYFolder(string folderName)
        {
            if (folderName.Length != 4 || !folderName.All(char.IsDigit))
                return false;
            int month = int.Parse(folderName.Substring(0, 2));
            int year = int.Parse(folderName.Substring(2, 2));
            return month >= 1 && month <= 12 && year >= 0 && year <= 99;
        }

        private int GetDirectoryItemCount(string path)
        {
            try { return Directory.GetFileSystemEntries(path).Length; }
            catch { return 0; }
        }

        private async Task updateHelpText()
        {
            HelpText = "Loading....";
            switch (isModeSorting)
            {
                case true: // Sorting
                    mmYYGroups = await Task.Run(
                        () => DispFiles
                                .Where(f => !IsValidMMYYFolder(f.name)) // do not sort files that are in MMYY format
                                .GroupBy(f => f.CreationDate.ToString("MMyy"))
                                .ToList()
                                );
                    HelpText = $"Sort above {DispFiles.Count} files into {mmYYGroups.Count()} folders. \nMMYY files are ignored. \nPress Execute."; // how many files there are, how many folders they will be sorted into
                    break;


                case false: // Emptying
                    int mmYYFolderCount = 0;
                    int totalItemsInMMYYFolders = 0;

                    await Task.Run(() =>
                    {
                        foreach (var dir in DispFiles.Where(d => d.isFolder && IsValidMMYYFolder(d.name)))
                        {
                            mmYYFolderCount++;
                            totalItemsInMMYYFolders += GetDirectoryItemCount(dir.Path);
                        }
                    });

                    HelpText = $"Emptying out {mmYYFolderCount} folders that contain a total of {totalItemsInMMYYFolders} items."; // how many folders there are. (how many items inside all total?)
                    break;
            }

            Win32.Application.Current.Dispatcher.Invoke(() => CommandManager.InvalidateRequerySuggested());

        }



        public async Task<bool> execute(IProgress<int> ProgressValue = null, IProgress<string> CurrentItem = null)
        {
            // if isModeSubFolder then access from subDirsList
            // else access from baseDirsList

            List<DirFileModel> src = [];
            if (isModeSubFolder)
                src = subDirsList;
            else
                src = baseDirsList;

            List<string> errors = [];

            if (src == null || src.Count == 0) errors.Add("No items found in source directory");

            // can add other checks also

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


            IEnumerable<IGrouping<string, DirFileModel>> yyMMGroups;
            yyMMGroups = src
                       .GroupBy(f => f.CreationDate.ToString("yyMM"))
                       .ToList();

            foreach (var group in yyMMGroups)
            {
                string yyMM = group.Key;
                string targetDir = Path.Combine(DestDirectory, yyMM);
                Directory.CreateDirectory(targetDir);
                foreach (var file in group)
                {
                    Debug.WriteLine($"processing {file.name}");

                    processedItems++;
                    ProgressValue?.Report((processedItems * 100) / totalItems);
                    CurrentItem?.Report(file.name);

                    string targetPath = Path.Combine(targetDir, file.name);
                    // Move individual file operations to background thread
                    // this allows the ProgressBar UI to update
                    await Task.Run(() =>
                    {
                        if (file.isFolder)
                            CopyDirectory(file.Path, targetPath, true);
                        else
                            File.Copy(file.Path, targetPath);
                    });
                }

            }

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
                file.CopyTo(targetFilePath);
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
