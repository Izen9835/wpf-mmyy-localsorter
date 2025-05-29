using FolderMMYYSorter_2.IO;
using FolderMMYYSorter_2.MVVM.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderMMYYSorter_2.MVVM.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        public RelayCommand BackCommand { get; set; }
        public RelayCommand NextCommand { get; set; }
        public RelayCommand CancelCommand { get; set; }

        public FileExplorer _FileExplorer { get; set; }

        private object _currentPage;
        public object CurrentPage
        {
            get => _currentPage;
            set { _currentPage = value; OnPropertyChanged(nameof(CurrentPage)); }
        }

        private readonly List<_baseviewmodel> _pages;
        private int _currentIndex;

        // event handler to reflect changes in ViewModel to UI
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public MainViewModel()
        {
            _FileExplorer = new FileExplorer();

            _pages = new List<_baseviewmodel>
            {
                new P1_srcdir_VM(_FileExplorer),
                new P2_options_VM(_FileExplorer),
                new P3_destdir_VM(_FileExplorer),
                new P4_summary_VM(_FileExplorer),
                new P5_complete_VM(_FileExplorer),
            };
            _currentIndex = 0;
            CurrentPage = _pages[_currentIndex];

            NextCommand = new RelayCommand(_ => GoNext(), _ => _currentIndex < _pages.Count - 1);
            BackCommand = new RelayCommand(_ => GoBack(), _ => _currentIndex > 0);
            CancelCommand = new RelayCommand(_ => Cancel());




            //BrowseCommand = new RelayCommand(o => _FileExplorer.fileDialog());

            //SortCommand = new RelayCommand(
            //    o => _FileExplorer.updateMode(true),
            //    o => (_FileExplorer.CurrentDirectory != "Loading...." &&
            //          _FileExplorer.CurrentDirectory != "Select a folder.")
            //    );
        }
        private void GoNext()
        {
            if (_currentIndex < _pages.Count - 1)
            {
                _currentIndex++;
                CurrentPage = _pages[_currentIndex];
            }

        }

        private void GoBack()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                CurrentPage = _pages[_currentIndex];
            }


        }

        private void Cancel()
        {
            // Handle cancel logic
        }
    }
}
