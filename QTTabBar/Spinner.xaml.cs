using System;
using System.Collections.Generic;
using System.Globalization;
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
        private bool _doRecurse = false;

        public static readonly DependencyProperty ValueProperty = 
            DependencyProperty.Register( "Value", typeof (int), typeof(Spinner), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault) );
        public static readonly DependencyProperty MaxProperty =
            DependencyProperty.Register("Max", typeof(int), typeof(Spinner), new FrameworkPropertyMetadata(10000, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty MinProperty =
            DependencyProperty.Register("Min", typeof(int), typeof(Spinner), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

        public int Max {
            get { return (int)GetValue(MaxProperty); }
            set { SetValue(MaxProperty, value); }
        }
        public int Min {
            get { return (int)GetValue(MinProperty); }
            set { SetValue(MinProperty, value); }
        }
        public int Value {
            get { return (int)GetValue(ValueProperty); }
            set {
                SetValue( ValueProperty, value );
            }
        }

        public Spinner() {
            InitializeComponent();
        }

        //# Make sure only numbers are entered.
        private void txtValue_TextChanged(object sender, RoutedEventArgs e) {
            if(!_doRecurse) {
                _doRecurse = true;
                return;
            }

            if(!int.TryParse(txtValue.Text, out _value)) {
                txtValue.Text = Value.ToString();
                return;
            }

            _doRecurse = false;
            
            if(int.Parse( txtValue.Text ) > scrollBar.Maximum) {
                txtValue.Text = scrollBar.Maximum.ToString();
            }else if(int.Parse(txtValue.Text) < scrollBar.Minimum) {
                txtValue.Text = scrollBar.Minimum.ToString();
            }

            Value = int.Parse(txtValue.Text);
        }

        //# Increment, or decrement on up, or down arrow key usage.
        private void txtValue_KeyDown(object sender, KeyEventArgs e) {
           if(e.Key == Key.Up) {
               Value++;
           }

           if(e.Key == Key.Down) {
               Value--;
           }
        }

        //# Ensure the txtValue field always reflects the value of the ScrollBar
        private void scrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if(!_doRecurse) {
                _doRecurse = true;
            }
            txtValue.Text = Value.ToString();
        }
    }
}
