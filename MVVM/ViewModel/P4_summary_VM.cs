using FolderMMYYSorter_2.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Win32 = System.Windows;


namespace FolderMMYYSorter_2.MVVM.ViewModel
{
    class P4_summary_VM : _baseviewmodel
    {
       
        private int _progressValue;
        private string _currentItemText;
        private string _progressText;


        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                OnPropertyChanged(nameof(ProgressValue));
            }
        }
        public string ProgressText
        {
            get => _progressText;
            set
            {
                _progressText = value;
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public string CurrentItemText
        {
            get => _currentItemText;
            set
            {
                _currentItemText = value;
                OnPropertyChanged(nameof(CurrentItemText));
            }
        }


        public P4_summary_VM(FileExplorer fileExplorer) : base(fileExplorer)
        {
            Title = "Summary";
            Instructions = "Double check instructions and press Execute.";
            nextButtonText = "Execute";

            //Summary = "Instructions Summary:\r\n- Select the target directory for file processing.\r\n- Specify the number of files to include in this operation.\r\n- Review the list of files to be changed, added, or removed.\r\n- Confirm all parameters are correct.\r\n- Press 'Execute' to begin the operation.\r\n";

            CurrentItemText = "Pending execution.";

        }

        public async Task<bool> Execute_w_Prog_Bar()
        {
            if (_FileExplorer.isExecuting == true)
            {
                Debug.WriteLine("cannot start. execution in progress!");
                return false;
            }
            var progress = new Progress<int>(percent =>
            {
                Win32.Application.Current.Dispatcher.Invoke(() =>
                {
                    ProgressValue = percent;
                    ProgressText = $"{percent}%";
                });
            });

            var currentItemProgress = new Progress<string>(item =>
            {
                Win32.Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentItemText = item;
                });
            });

            Debug.WriteLine("awaiting execute now");
            bool success = await _FileExplorer.execute(progress, currentItemProgress);

            return success;
        }

    }
}
