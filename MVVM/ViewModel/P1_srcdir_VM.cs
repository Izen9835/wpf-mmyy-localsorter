using FolderMMYYSorter_2.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderMMYYSorter_2.MVVM.ViewModel
{
    class P1_srcdir_VM : _baseviewmodel
    {
        public P1_srcdir_VM(FileExplorer fileExplorer) : base(fileExplorer)
        {
            Title = "Select Source";
            Instructions = "Select a source directory";


        }
    }
}
