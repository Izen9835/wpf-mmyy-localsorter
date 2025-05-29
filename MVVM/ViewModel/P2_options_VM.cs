using FolderMMYYSorter_2.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderMMYYSorter_2.MVVM.ViewModel
{
    class P2_options_VM : _baseviewmodel
    {


        public ObservableCollection<string> Options { get; } = new ObservableCollection<string>
        {
            "Sort Directory",
            "Sort Subdirectories",
        };

        private string _description = "";

        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        private string _selectedOption;

        public string SelectedOption
        {
            get => _selectedOption;
            set
            {
                if (_selectedOption != value)
                {
                    _selectedOption = value;
                    OnPropertyChanged(nameof(SelectedOption));
                    switch (_selectedOption)
                    {
                        case "Sort Directory":
                            Description = "Sort the folders in the specified directory.";
                            break;
                        case "Sort Subdirectories":
                            Description = "Sort the subfolders/items of all the folders in the specified directory.";
                            break;
                    }
                }
            }
        }

        public P2_options_VM(FileExplorer fileExplorer) : base(fileExplorer)
        {
            Title = "Options";
            Instructions = "Configure which files you want to sort.";

        }
    }
}
