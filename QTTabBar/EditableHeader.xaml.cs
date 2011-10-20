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

        private bool preparing = false;

        public string Text {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public UIElement Container {
            get { return (UIElement)GetValue(ContainerProperty); }
            set { SetValue(ContainerProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(EditableHeader),
            new FrameworkPropertyMetadata("New Header", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty ContainerProperty =
            DependencyProperty.Register("Container", typeof(UIElement), typeof(EditableHeader),
            new FrameworkPropertyMetadata(null));

        public EditableHeader(string initialName, TreeViewItem item = null) {
            InitializeComponent();

            if(initialName != null) {
                Text = initialName;
            }

            //# TODO Find a way to set the mouse focus on instantiation...
        }

        public EditableHeader() : this(null) {
        }

        private void txtHeaderEdit_LostFocus(object sender, RoutedEventArgs e) {
            //txtHeader.Text = txtHeaderEdit.Text;
            txtHeaderEdit.Visibility = Visibility.Hidden;
        }

        private void txtHeaderEdit_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Enter) {
                //txtHeader.Text = txtHeaderEdit.Text;
                txtHeaderEdit.Visibility = Visibility.Hidden;
            }
        }

        private void txtHeader_MouseDown(object sender, MouseButtonEventArgs e) {
            preparing = (Container != null)
                ? Container.IsFocused
                : true;
        }

        private void txtHeader_MouseUp(object sender, MouseButtonEventArgs e) {
            // Does not open the text box if editable area has got focus by clicking right now.
            if(!preparing) {
                return;
            }
            txtHeaderEdit.Visibility = Visibility.Visible;
            txtHeaderEdit.CaptureMouse();
            txtHeaderEdit.Focus();
            txtHeaderEdit.ReleaseMouseCapture();
        }
    }
}
