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

        private string _Summary;

        public string Summary
        {
            get => _Summary;
            set
            {
                if (_Summary != value)
                {
                    _Summary = value;
                    OnPropertyChanged(nameof(Summary));
                }
            }
        }
        public P4_summary_VM(FileExplorer fileExplorer) : base(fileExplorer)
        {
            Title = "Summary";
            Instructions = "Double check instructions and press Execute.";
            nextButtonText = "Execute";

            //Summary = "Instructions Summary:\r\n- Select the target directory for file processing.\r\n- Specify the number of files to include in this operation.\r\n- Review the list of files to be changed, added, or removed.\r\n- Confirm all parameters are correct.\r\n- Press 'Execute' to begin the operation.\r\n";

            Summary = "// under construction //";
            OnPropertyChanged(nameof(Summary));

        }
    }
}
