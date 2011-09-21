using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using Point = System.Drawing.Point;
using UserControl = System.Windows.Controls.UserControl;

namespace QTTabBarLib {
    /// <summary>
    /// Interaction logic for ApplicationsNode.xaml
    /// </summary>
    public partial class ApplicationsNode : UserControl {
        public ApplicationsNode() {
            InitializeComponent();
        }

        private void btnPath_Click( object sender, RoutedEventArgs e ) {
            OpenFileDialog d = new OpenFileDialog();

            if ( d.ShowDialog() == DialogResult.OK ) {
                txtPath.Text = d.FileName;
            }
        }



        private void btnPath2_Click(object sender, RoutedEventArgs e) {
            FolderBrowserDialogEx d = new FolderBrowserDialogEx();

            if (d.ShowDialog() == DialogResult.OK) {
                txtWorkingDir.Text = d.SelectedPath;
            }
        }

        private void btnArguments_Click( object sender, RoutedEventArgs e ) {
            ContextMenu cm = new ContextMenu();
            Separator sep = new Separator();

            MenuItem header = new MenuItem();
            header.IsEnabled = false;
            header.Header = "The following keywords are replaced with double-quoted path strings at runtime.";

            MenuItem f = new MenuItem();
            f.Header = "%f% - Selected files.";
            f.Click += delegate( object o, RoutedEventArgs a ) { txtArguments.AppendText( "%f%" ); };

            MenuItem d = new MenuItem();
            d.Header = "%d% - Selected folders.";
            d.Click += delegate( object o, RoutedEventArgs a ) { txtArguments.AppendText( "%d%" ); };

            MenuItem s = new MenuItem();
            s.Header = "%s% - Selected files and folders.";
            s.Click += delegate( object o, RoutedEventArgs a ) { txtArguments.AppendText( "%s%" ); };

            MenuItem c = new MenuItem();
            c.Header = "%c% - Current folder.";
            c.Click += delegate( object o, RoutedEventArgs a ) { txtArguments.AppendText( "%c%" ); };

            MenuItem cd = new MenuItem();
            cd.Header = "%cd% - Current folder if no selection, otherwise selected folder.";
            cd.Click += delegate( object o, RoutedEventArgs a ) { txtArguments.AppendText( "%cd%" ); };

            MenuItem rip = new MenuItem();
            cd.Header = "%r.i.p.% - Rest in peace, Mami -- you will be missed.";
            cd.Click += delegate(object o, RoutedEventArgs a) { txtArguments.AppendText("%r.i.p.%"); };

            cm.Items.Add( header );
            cm.Items.Add( sep );
            cm.Items.Add( f );
            cm.Items.Add( d );
            cm.Items.Add( s );
            cm.Items.Add( c );
            cm.Items.Add( cd );
            cm.Items.Add( rip );

            cm.PlacementTarget = btnArguments;
            cm.IsOpen = true;
        }

        private void btnWorkingDir_Click( object sender, RoutedEventArgs e ) {
            ContextMenu cm = new ContextMenu();
            Separator sep = new Separator();

            MenuItem header = new MenuItem();
            header.IsEnabled = false;
            header.Header = "The following keywords are replaced with double-quoted path strings at runtime.";

            MenuItem cd = new MenuItem();
            cd.Header = "%cd% - Current folder if no selection, otherwise selected folder.";
            cd.Click += delegate( object o, RoutedEventArgs a ) { txtWorkingDir.AppendText( "%cd%" ); };

            cm.Items.Add( header );
            cm.Items.Add( sep );
            cm.Items.Add( cd );

            cm.PlacementTarget = btnWorkingDir;
            cm.IsOpen = true;
        }

        private void chkShortcutKey_Checked(object sender, RoutedEventArgs e) {
            if(chkShortcutKey.IsChecked == true) {
                txtShortcutKey.IsEnabled = true;
            }else {
                txtShortcutKey.IsEnabled = false;
            }
        }
    }
}
