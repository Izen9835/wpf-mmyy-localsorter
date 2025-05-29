using FolderMMYYSorter_2.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderMMYYSorter_2.MVVM.ViewModel
{
    class P5_complete_VM : _baseviewmodel
    {
        public P5_complete_VM(FileExplorer fileExplorer) : base(fileExplorer)
        {
            Title = "Task Completed";
            Instructions = "";
            nextButtonText = "Finish";

        }
    }
}
