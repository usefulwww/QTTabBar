using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControl = System.Windows.Controls.UserControl;

namespace QTTabBarLib {
    /// <summary>
    /// Interaction logic for ApplicationsNode.xaml
    /// </summary>
    public partial class ApplicationsNode : UserControl {
        public ApplicationsNode() {
            InitializeComponent();
        }

        private void btnPath_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog d = new OpenFileDialog();

            if(d.ShowDialog() == DialogResult.OK) {
                txtPath.Text = d.FileName;
            }
        }

        private void btnArguments_Click(object sender, RoutedEventArgs e) {

        }

        private void btnWorkingDir_Click(object sender, RoutedEventArgs e) {

        }
    }
}
