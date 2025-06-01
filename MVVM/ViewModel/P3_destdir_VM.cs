using FolderMMYYSorter_2.IO;
using FolderMMYYSorter_2.MVVM.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderMMYYSorter_2.MVVM.ViewModel
{
    class P3_destdir_VM : _baseviewmodel
    {

        public RelayCommand BrowseCommand { get; set; }

        public RelayCommand UpdateDirCommand { get; set; }

        public P3_destdir_VM(FileExplorer fileExplorer) : base(fileExplorer)
        {
            Title = "Select Destination";
            Instructions = "Select a destination directory. (where will the output be stored)";

            BrowseCommand = new RelayCommand(o => _FileExplorer.destFileDialog());

            UpdateDirCommand = new RelayCommand(
            async o =>
            {
                await _FileExplorer.updateDisplayedFiles();
                Debug.WriteLine(_FileExplorer.CurrentDirectory);
            },
            o => _FileExplorer.isCurrDirValid()
);

        }
    }
}
