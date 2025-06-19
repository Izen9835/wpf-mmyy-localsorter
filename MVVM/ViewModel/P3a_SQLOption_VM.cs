using FolderMMYYSorter_2.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderMMYYSorter_2.MVVM.ViewModel
{
    class P3a_SQLOption_VM : _baseviewmodel
    {
        public P3a_SQLOption_VM(FileExplorer fileExplorer) : base(fileExplorer)
        {
            Title = "Use SQL DB Data";
            Instructions = "Toggle further sorting based on SQL DB data";
        }
    }
}
