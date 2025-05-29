using FolderMMYYSorter_2.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderMMYYSorter_2.MVVM.ViewModel
{
    class _baseviewmodel : INotifyPropertyChanged
    {
        protected FileExplorer _FileExplorer { get; set; }

        // event handler to reflect changes in ViewModel to UI
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Title { get; set; }
        public string Instructions { get; set; }


        public _baseviewmodel(FileExplorer fileExplorer)
        {
            // use same instance of file explorer for every sub page
            _FileExplorer = fileExplorer;

            // default title and instructions
            Title = "Default Title Text";
            Instructions = "lorem ipsum default instructions text";
        }
    }
}
