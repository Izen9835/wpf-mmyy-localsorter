using FolderMMYYSorter_2.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderMMYYSorter_2.MVVM.ViewModel
{
    class P3_destdir_VM : _baseviewmodel
    {
        public P3_destdir_VM(FileExplorer fileExplorer) : base(fileExplorer)
        {
            Title = "Select Destination";
            Instructions = "Select a destination directory. (where will the output be stored)";

        }
    }
}
