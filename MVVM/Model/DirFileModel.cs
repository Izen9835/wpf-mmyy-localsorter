using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderMMYYSorter_2.MVVM.Model
{
    class DirFileModel
    {

        public string name { get; set; }

        public DateTime CreationDate { get; set; }


        private bool _isFolder;
        public bool isFolder
        {
            get => _isFolder;
            set
            {
                _isFolder = value;
                IconName = value ? "/Resource/foldericon.png" : "/Resource/fileicon.png";
            }
        }

        public string IconName { get; set; }

        public string Path { get; set; }


    }
}
