using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;
using Icon = System.Drawing.Icon;
using System.Windows;

namespace QTTabBarLib {
    /// <summary>
    /// Interaction logic for FileFolderEntryBox.xaml
    /// </summary>
    public partial class FileFolderEntryBox : UserControl {
        private bool file = true;
        private bool watermarkVisible = true;
        private string watermarkText = null;
        private bool showIcon = true;
        private bool updating = false;
        private bool lockText = false;

        public static readonly DependencyProperty SelectedPathProperty =
                DependencyProperty.Register("SelectedPath", typeof(string), typeof(FileFolderEntryBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedPathChanged));

        // Apparently .NET 4 does support using an array type for a DependencyProperty,
        // but using IEnumerable works just as well and allows us to stay on .NET 3.5.
        // We can still bind this to a byte[], and that's what matters.
        public static readonly DependencyProperty SelectedIDLProperty =
                DependencyProperty.Register("SelectedIDL", typeof(IEnumerable<byte>), typeof(FileFolderEntryBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedIDLChanged));

        public string SelectedPath {
            get { return (string)GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        public IEnumerable<byte> SelectedIDL {
            get { return (IEnumerable<byte>)GetValue(SelectedIDLProperty); }
            set {
                SetValue(SelectedIDLProperty, value == null
                  ? null
                  : value as byte[] ?? value.ToArray());
            }
        }

        private static object OnCoerceSelectedIDL(DependencyObject d, object baseValue) {
            var box = (FileFolderEntryBox)d;
            using(IDLWrapper wrapper = new IDLWrapper(box.SelectedPath)) {
                return wrapper.IDL;
            }
        }

        private static void OnSelectedIDLChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            FileFolderEntryBox box = (FileFolderEntryBox)d;
            if(box.updating) return;
            string newPath = "";
            if(box.SelectedIDL != null) {
                IEnumerable<byte> eidl = (IEnumerable<byte>)e.NewValue;
                using(IDLWrapper wrapper = new IDLWrapper(eidl as byte[] ?? eidl.ToArray())) {
                    newPath = wrapper.Path;
                }
            }

            box.updating = true;
            box.SelectedPath = newPath;
            box.updating = false;
            box.Update();
        }

        private static void OnSelectedPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            FileFolderEntryBox box = (FileFolderEntryBox)d;
            if(box.updating) return;
            using(IDLWrapper newIDL = new IDLWrapper((string)e.NewValue)) {
                IEnumerable<byte> eidl = box.SelectedIDL;
                if(eidl == null) {
                    box.updating = true;
                }
                else {
                    using(IDLWrapper oldIDL = new IDLWrapper(eidl as byte[] ?? eidl.ToArray())) {
                        // Don't change the IDL if current one has the same path.
                        box.updating = (oldIDL.Path != newIDL.Path);
                    }
                }

                if(box.updating) {
                    box.SelectedIDL = newIDL.IDL;
                    box.updating = false;
                }
            }
            box.Update();
        }

        // PENDING: the "!File" expression does not explicitly mean using the folder chooser,
        // so I think the class should have both of them
        public bool File {
            get {
                return file;
            }
            set {
                if(file == value) {
                    return;
                }
                file = value;
                Update();
            }
        }

        public bool Folder {
            get {
                return !File;
            }
            set {
                File = !value;
            }
        }

        public bool ShowIcon {
            get {
                return showIcon;
            }
            set {
                if(showIcon == value) {
                    return;
                }
                showIcon = value;
                ShowIconInternal(showIcon);
            }
        }

        // Watermark must be disabled eventually if WatermarkText is an empty text.
        public string WatermarkText {
            get {
                return watermarkText;
            }
            set {
                watermarkText = value;
                if(watermarkVisible) {
                    Update();
                }
            }
        }

        public FileFolderEntryBox() {
            InitializeComponent();
            ShowIcon = false;
            Update();
        }

        // In any case, we must use this method instead of txtLocation.Text
        private void SetTextboxText(string text) {
            updating = true;
            int pos = txtLocation.CaretIndex;
            txtLocation.Text = text;
            txtLocation.CaretIndex = pos;
            updating = false;
        }

        private void ShowIconInternal(bool b) {
            bool visible = (imgIcon.Visibility != Visibility.Collapsed);
            if(visible == b) {
                return;
            }
            Thickness padding = txtLocation.Padding;
            double n = imgIcon.Width + imgIcon.Margin.Left - padding.Top;
            padding.Left += b ? n : -n;
            txtLocation.Padding = padding;
            imgIcon.Visibility = b ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowWatermark(bool b) {
            if(ShowIcon) {
                ShowIconInternal(!b);
            }

            // Allows the first call
            if(!watermarkVisible && !b) {
                return;
            }

            //txtLocation.FontStyle = b ? FontStyles.Italic : FontStyles.Normal;
            txtLocation.Foreground = b ? Brushes.DarkGray : Brushes.Black;
            watermarkVisible = b;

            string text;
            if(b) {
                // TODO: Localize this
                text = WatermarkText ?? string.Format("Choose your {0}", File ? "file" : "folder");
            }
            else {
                text = string.Empty;
            }
            SetTextboxText(text);
        }

        private void Update() {
            if(string.IsNullOrEmpty(SelectedPath)) {
                if(!txtLocation.IsFocused) {
                    ShowWatermark(true);
                }
                imgIcon.Source = null;
                return;
            }

            ShowWatermark(false);

            IDLWrapper wrapper = SelectedIDL == null
                    ? new IDLWrapper(SelectedPath)
                    : new IDLWrapper(SelectedIDL as byte[] ?? SelectedIDL.ToArray());

            using(wrapper) {
                if(!lockText) {
                    string text;
                    if(File || wrapper.IDL == null) {
                        text = wrapper.Path;
                    }
                    else {
                        bool b = File ? wrapper.IsFileSystemFile : wrapper.IsFileSystemFolder;
                        text = b ? wrapper.Path : wrapper.DisplayName;
                    }
                    SetTextboxText(text);
                }

                Icon icon = QTUtility.GetIcon(wrapper.PIDL);
                imgIcon.Source =
                    (ImageSource)new BitmapToImageSourceConverter().Convert(icon.ToBitmap(), null, null, null);
            }
        }

        private bool BrowseForFile() {
            OpenFileDialog ofd = new OpenFileDialog { FileName = SelectedPath };
            if(DialogResult.OK != ofd.ShowDialog()) {
                return false;
            }

            using(IDLWrapper wrapper = new IDLWrapper(ofd.FileName)) {
                if(wrapper.IDL != null) {
                    SelectedIDL = wrapper.IDL;
                }
                else {
                    SelectedPath = wrapper.Path;
                }
            }
            return true;
        }

        private bool BrowseForFolder() {
            FolderBrowserDialogEx fbd = new FolderBrowserDialogEx();
            if(System.Windows.Forms.DialogResult.OK != fbd.ShowDialog()) {
                return false;
            }

            using(IDLWrapper wrapper = new IDLWrapper(fbd.SelectedIDL)) {
                if(wrapper.IDL != null) {
                    SelectedIDL = wrapper.IDL;
                }
                else {
                    SelectedPath = wrapper.Path;
                }
            }
            return true;
        }

        public bool Browse() {
            return File ? BrowseForFile() : BrowseForFolder();
        }

        private void btnBrowse_Click(object sender, System.Windows.RoutedEventArgs e) {
            Browse();
        }

        private void txtLocation_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e) {
            if(watermarkVisible) {
                ShowWatermark(false);
            }
        }

        private void txtLocation_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e) {
            if(string.IsNullOrEmpty(txtLocation.Text)) {
                ShowWatermark(true);
            }
        }

        private void txtLocation_TextChanged(object sender, TextChangedEventArgs e) {
            if(updating) {
                return;
            }

            string path = txtLocation.Text;
            lockText = true;
            if((File && !System.IO.File.Exists(path)) || (Folder && !System.IO.Directory.Exists(path))) {
                SelectedPath = path;
            }
            else {
                using(IDLWrapper wrapper = new IDLWrapper(path)) {
                    if(wrapper.IDL != null) {
                        SelectedIDL = wrapper.IDL;
                    }
                    else {
                        SelectedPath = wrapper.Path;
                    }
                }
            }
            lockText = false;
        }
    }
}
