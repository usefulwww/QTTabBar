using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace QTTabBarLib
{
    /// <summary>
    /// Interaction logic for EditableHeader.xaml
    /// </summary>
    public partial class EditableHeader : UserControl {
        public EditableHeader(string initialName = "", TreeViewItem item = null) {
            InitializeComponent();
            txtHeaderEdit.Text = txtHeader.Text = initialName;

            //# TODO Find a way to set the mouse focus on instantiation...
        }

        private void txtHeaderEdit_LostFocus(object sender, RoutedEventArgs e) {
            txtHeader.Text = txtHeaderEdit.Text;
            txtHeaderEdit.Visibility = Visibility.Hidden;
        }

        private void txtHeaderEdit_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                txtHeader.Text = txtHeaderEdit.Text;
                txtHeaderEdit.Visibility = Visibility.Hidden;
            }
        }

        private void txtHeader_MouseUp(object sender, MouseButtonEventArgs e) {
            txtHeaderEdit.Visibility = Visibility.Visible;
            txtHeaderEdit.CaptureMouse();
            txtHeaderEdit.Focus();
            txtHeaderEdit.ReleaseMouseCapture();
        }
    }
}
