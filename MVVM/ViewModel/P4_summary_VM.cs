using FolderMMYYSorter_2.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderMMYYSorter_2.MVVM.ViewModel
{
    class P4_summary_VM : _baseviewmodel
    {
        public P4_summary_VM(FileExplorer fileExplorer) : base(fileExplorer)
        {
            Title = "Summary";
            Instructions = "Double check instructions and press Execute.";

        }
    }
}
