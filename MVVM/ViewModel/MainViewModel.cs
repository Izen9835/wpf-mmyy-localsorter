using FolderMMYYSorter_2.IO;
using FolderMMYYSorter_2.MVVM.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderMMYYSorter_2.MVVM.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public RelayCommand BrowseCommand { get; set; }
        public RelayCommand SortCommand { get; set; }
        public RelayCommand EmptyCommand { get; set; }
        public RelayCommand ExecuteCommand { get; set; }

        public FileExplorer _FileExplorer { get; set; }

        // event handler to reflect changes in ViewModel to UI
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public MainViewModel()
        {
            _FileExplorer = new FileExplorer();

            BrowseCommand = new RelayCommand(o => _FileExplorer.fileDialog());

            SortCommand = new RelayCommand(
                o => _FileExplorer.updateMode(true),
                o => (_FileExplorer.CurrentDirectory != "Loading...." &&
                      _FileExplorer.CurrentDirectory != "Select a folder.")
                );

            EmptyCommand = new RelayCommand(
                o => _FileExplorer.updateMode(false),
                o => (_FileExplorer.CurrentDirectory != "Loading...." &&
                      _FileExplorer.CurrentDirectory != "Select a folder.")
                );

            ExecuteCommand = new RelayCommand(
                async o => await _FileExplorer.execute(),
                o => (_FileExplorer.CurrentDirectory != "Loading...." &&
                      _FileExplorer.CurrentDirectory != "Select a folder." &&
                      _FileExplorer.HelpText != "Select a mode." &&
                      _FileExplorer.HelpText != "Loading....")
                );

        }
    }
}
