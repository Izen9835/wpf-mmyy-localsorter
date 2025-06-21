using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FolderMMYYSorter_2.MVVM.View
{
    /// <summary>
    /// Interaction logic for P3_destdir.xaml
    /// </summary>
    public partial class P3_destdir : System.Windows.Controls.UserControl
    {
        public P3_destdir()
        {
            InitializeComponent();
        }


        // The following code highlights the full textbox when you mouse (/keyboard(?)) click on the textbox
        // To facilitate easy copy pasting of directories
        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is System.Windows.Controls.TextBox tb)
                tb.SelectAll();
        }
        private void TextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as System.Windows.Controls.TextBox;
            if (tb != null && !tb.IsKeyboardFocusWithin)
            {
                tb.Focus();
                e.Handled = true; // Prevents default caret placement
            }
        }
    }
}
