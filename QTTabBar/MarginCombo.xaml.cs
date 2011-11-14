using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace QTTabBarLib {

    // It *really* pisses me off that I can't use the Thickness class instead.
    // The WPF devs forgot to mark it as Serializable!
    [Serializable]
    public struct Margin {
        public int Left     { get; set; }
        public int Top      { get; set; }
        public int Right    { get; set; }
        public int Bottom   { get; set; }

        public Margin(int left, int top, int right, int bottom) : this() {
            Left    = left;
            Top     = top;
            Right   = right;
            Bottom  = bottom;
        }
    }

    /// <summary>
    /// Interaction logic for MarginCombo.xaml
    /// </summary>
    public partial class MarginCombo : UserControl {
        private const int VAL_MAX = 99;

        public MarginCombo() {
            InitializeComponent();

            // Set bindings (so much easier doing it here than in xaml...)
            var boxes = new TextBox[] {txtLeft, txtTop, txtRight, txtBottom};
            for(int i = 0; i < 4; i++) {
                boxes[i].SetBinding(TextBox.TextProperty, new Binding("Value") {
                    Source = this,
                    Converter = new SingleTextConverter(i, this),
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged 
                });
            }

            txtMargin.SetBinding(TextBox.TextProperty, new Binding("Value") {
                    Source = this, 
                    Converter = new JoinedTextConverter(), 
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            });

            txtAll.SetBinding(TextBox.TextProperty, new MultiBinding {
                Converter = new CommonMultiConverter(),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Bindings = {
                    new Binding("Text") { Source = txtLeft   },
                    new Binding("Text") { Source = txtTop    },
                    new Binding("Text") { Source = txtRight  },
                    new Binding("Text") { Source = txtBottom },
                }
            });
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value",
                typeof(Margin), typeof(MarginCombo), new FrameworkPropertyMetadata(new Margin(), 
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Margin Value {
            get {
                return (Margin)GetValue(ValueProperty);
            }
            set {
                SetValue(ValueProperty, value);
            }
        }

        #region Event Handlers

        //# The converter will do the validation for us.
        private void txtMargin_TextChanged(object sender, RoutedEventArgs e) {
            TextBox box = ((TextBox)sender);
            int pos = box.CaretIndex;
            BindingExpression bind = box.GetBindingExpression(TextBox.TextProperty);
            if(bind != null) {
                bind.UpdateTarget();
                box.CaretIndex = pos;
            }
        }

        //# Make sure the text stays numeric and in range.Z
        private void txtLTRB_TextChanged(object sender, RoutedEventArgs e) {
            int i;
            TextBox box = ((TextBox)sender);
            int pos = box.CaretIndex;
            BindingExpression bind = box.GetBindingExpression(TextBox.TextProperty);
            if(box.Text.Length == 0) return;
            if(!int.TryParse(box.Text, out i)) {
                if(bind != null) bind.UpdateTarget();
            }
            else if(i > VAL_MAX) {
                box.Text = VAL_MAX.ToString();
                box.CaretIndex = pos;
            }
            else if(i < 0) {
                box.Text = "0";
                box.CaretIndex = pos;
            }
        }

        //# Allow only digits to be entered.  We still need TextChanged to 
        //# make sure letters don't get in via pasting, drag & drop, etc.
        private void txtMargin_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !e.Text.ToCharArray().All(c => char.IsDigit(c) || c == ' ' || c == ',');
        }

        private void txtLTRB_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !e.Text.ToCharArray().All(char.IsDigit);
        }
        #endregion 

        #region Converters

        [ValueConversion(typeof(Margin), typeof(string))]
        private class JoinedTextConverter : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
                Margin t = (Margin)value;
                return t.Left + ", " + t.Top + ", " + t.Right + ", " + t.Bottom;
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
                int i;
                int[] v = value.ToString().Split(',').Select(
                        s => int.TryParse(s.Trim(), out i) ? Math.Min(Math.Max(i, 0), VAL_MAX) : 0).ToArray();
                Array.Resize(ref v, 4);
                return new Margin(v[0], v[1], v[2], v[3]);
            }
        }

        [ValueConversion(typeof(Margin), typeof(string))]
        private class SingleTextConverter : IValueConverter {
            private int idx;
            private MarginCombo parent;

            public SingleTextConverter(int idx, MarginCombo parent) {
                this.idx = idx;
                this.parent = parent;
            }

            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
                switch(idx) {
                    case 0:  return ((Margin)value).Left.ToString();
                    case 1:  return ((Margin)value).Top.ToString();
                    case 2:  return ((Margin)value).Right.ToString();
                    default: return ((Margin)value).Bottom.ToString();
                }
            }

            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
                int i;
                if(!int.TryParse(value.ToString().Trim(), out i) || i < 0 || i > VAL_MAX) return Binding.DoNothing;
                Margin current = parent.Value;
                switch(idx) {
                    case 0:  current.Left   = i; break;
                    case 1:  current.Top    = i; break;
                    case 2:  current.Right  = i; break;
                    default: current.Bottom = i; break;
                }
                return current;
            }
        }

        private class CommonMultiConverter : IMultiValueConverter {
            public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
                return values.All(o => o.Equals(values[0])) ? values[0] : "";
            }

            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
                int i;
                return Enumerable.Repeat(
                        int.TryParse(value.ToString().Trim(), out i) && i >= 0 ? i.ToString() : Binding.DoNothing,
                        targetTypes.Length).ToArray();
            }
        }

        #endregion
    }
}
