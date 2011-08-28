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

namespace QTTabBarLib {
    /// <summary>
    /// Interaction logic for Spinner.xaml
    /// </summary>
    public partial class Spinner : UserControl {
        private int _value = 0;
        public int Value {
            get { return (int)scrollBar.Value; }
            set { 
                scrollBar.Value = value;
                txtValue.Text = value.ToString();
            }
        }

        public Spinner() {
            InitializeComponent();
        }

        //# Make sure only numbers are entered.
        private void txtValue_TextChanged(object sender, RoutedEventArgs e) {
            if(!int.TryParse(txtValue.Text, out _value)) {
                txtValue.Text = scrollBar.Value.ToString();
                return;
            }

            //# Why does this crash Explorer? This should not recurse...

            /*if(int.Parse( txtValue.Text ) > scrollBar.Maximum) {
                txtValue.Text = scrollBar.Maximum.ToString();
            }else if(int.Parse(txtValue.Text) < scrollBar.Minimum) {
                txtValue.Text = scrollBar.Minimum.ToString();
            }*/
        }

        //# Increment, or decrement on up, or down arrow key usage.
        private void txtValue_KeyUp(object sender, KeyEventArgs e) {
           if(e.Key == Key.Up) {
               scrollBar.Value++;
           }

           if(e.Key == Key.Down) {
               scrollBar.Value--;
           }
        }

        //# Ensure the txtValue field always reflects the value of the ScrollBar
        private void scrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            txtValue.Text = scrollBar.Value.ToString();
        }

        //# Make sure that when the mouse leaves the txtValue field, that the value inside the TextBox is stored as the ScrollBar value
        private void txtValue_MouseLeave(object sender, MouseEventArgs e) {
            Value = int.Parse( txtValue.Text );
        }
    }


}
