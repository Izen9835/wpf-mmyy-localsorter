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
    class P1_srcdir_VM : _baseviewmodel
    {
        public RelayCommand BrowseCommand { get; set; }
        public RelayCommand UpdateDirCommand { get; set; }

        public P1_srcdir_VM(FileExplorer fileExplorer) : base(fileExplorer)
        {
            Title = "Select Source";
            Instructions = "Select a source directory. Only copy pasting a directory works, press Enter when pasted.";

            BrowseCommand = new RelayCommand(o => _FileExplorer.fileDialog());

            UpdateDirCommand = new RelayCommand(
            async o =>
            {
                await _FileExplorer.UpdateDisplayedFilesAsync();
                Debug.WriteLine(_FileExplorer.CurrentDirectory);
            },
            o => _FileExplorer.isCurrDirValid()
            );

        }
    }
}
