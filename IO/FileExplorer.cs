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
                if (_CurrentDirectory != value)
                {
                    _CurrentDirectory = value;
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

                }
            }
        }


        public bool isModeSorting { get; set; }
        // as opposed to emptying mode

        public List<DirFileModel> filesList;
        public List<DirFileModel> dirsList;

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

        private async Task updateDisplayedFiles()
        {
            // list directories first
            string[] dirs = await Task.Run(() => Directory.GetDirectories(CurrentDirectory));
            dirsList = await Task.Run(() => dirs.Select(
                dirPath => new DirFileModel
            {
                name = System.IO.Path.GetFileName(dirPath),
                CreationDate = new FileInfo(dirPath).CreationTime,
                isFolder = true,
                Path = dirPath,
            }).ToList());


            // list files next
            string[] files = await Task.Run(() => Directory.GetFiles(CurrentDirectory));
            filesList = await Task.Run(() => files.Select(
                filePath => new DirFileModel
                {
                    name = System.IO.Path.GetFileName(filePath),
                    CreationDate = new FileInfo(filePath).CreationTime,
                    isFolder = false,
                    Path = filePath,
                }).ToList());


            // do as a batch to save resources
            // display all directories and files
            Win32.Application.Current.Dispatcher.Invoke(() =>
            {
                DispFiles.Clear();

                foreach (var dir in dirsList)
                    DispFiles.Add(dir);

                foreach (var file in filesList)
                    DispFiles.Add(file);
            });

        }

        public void updateMode(bool input)
        {
            isModeSorting = input;
            Task.Run(() => updateHelpText());
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
            switch (isModeSorting)
            {
                case true: // Sorting
                    HelpText = "Loading....";
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

                    foreach (var dir in DispFiles.Where(d => d.isFolder && IsValidMMYYFolder(d.name)))
                    {
                        mmYYFolderCount++;
                        totalItemsInMMYYFolders += GetDirectoryItemCount(dir.Path);
                    }
                    HelpText = $"Emptying out {mmYYFolderCount} folders that contain a total of {totalItemsInMMYYFolders} items."; // how many folders there are. (how many items inside all total?)
                    break;
            }
        }

        public async Task execute()
        {
            HelpText = "Processing....";
            switch (isModeSorting)
            {
                case true: // Sorting
                    foreach (var group in mmYYGroups)
                    {
                        string mmYY = group.Key;
                        string targetDir = Path.Combine(CurrentDirectory, mmYY);
                        Directory.CreateDirectory(targetDir);
                        foreach (var file in group)
                        {
                            string targetPath = Path.Combine(targetDir, Path.GetFileName(file.Path));
                            if (file.isFolder)
                                Directory.Move(file.Path, targetPath);
                            else
                                File.Move(file.Path, targetPath); // file already exists causes issues
                        }
                    }
                    break;

                case false: // Emptying
                    foreach (var dir in DispFiles.Where(d => d.isFolder && IsValidMMYYFolder(d.name)))
                    {
                        foreach (var item in Directory.GetFileSystemEntries(dir.Path))
                        {
                            var targetDir = Path.Combine(CurrentDirectory, Path.GetFileName(item));
                            if (File.Exists(item))
                                File.Move(item, targetDir);
                            else if (Directory.Exists(item))
                                Directory.Move(item, targetDir);
                        }
                        Directory.Delete(dir.Path, recursive: true); // perms might fail
                    }
                    break;
            }
            await updateDisplayedFiles();
            HelpText = "Executed Successfully.";
        }
    }
}
