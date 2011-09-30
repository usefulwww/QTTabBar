using System.Windows.Controls;
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
        private byte[] location = null;
        private bool file = true;
        private bool watermarkVisible = true;
        private string watermarkText = null;
        private bool showIcon = true;

        public string SelectedPath {
            get {
                using(IDLWrapper wrapper = new IDLWrapper(location)) {
                    return wrapper.Path;
                }
            }
            set {
                if(string.IsNullOrEmpty(value)) {
                    ClearLocation();
                }
                else {
                    // Note that this cannot select a path that does not exist.
                    using(IDLWrapper wrapper = new IDLWrapper(value)) {
                        SetLocation(wrapper);
                    }
                }
            }
        }
        public byte[] SelectedIDL {
            get {
                return location;
            }
            set {
                location = value;
            }
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
                ClearLocation();
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
                    ClearLocation();
                }
            }
        }

        public static readonly DependencyProperty SelectedPathProperty =
                DependencyProperty.Register("SelectedPath", typeof(string), typeof(FileFolderEntryBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public FileFolderEntryBox() {
            InitializeComponent();

            ClearLocation();
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
                text = (WatermarkText != null)
                    ? WatermarkText : string.Format("Choose your {0}", File ? "file" : "folder");
            }
            else {
                text = string.Empty;
            }
            txtLocation.Text = text;
        }

        private void ClearLocation() {
            if(!txtLocation.IsFocused) {
                ShowWatermark(true);
            }
            imgIcon.Source = null;
            location = null;
        }

        private void SetLocation(IDLWrapper wrapper, bool updateText) {
            ShowWatermark(false);

            if(updateText) {
                if(File) {
                    txtLocation.Text = wrapper.Path;
                }
                else {
                    bool b = File ? wrapper.IsFileSystemFile : wrapper.IsFileSystemFolder;
                    txtLocation.Text = b ? wrapper.Path : wrapper.DisplayName;
                }
            }

            Icon icon = QTUtility.GetIcon(wrapper.PIDL);
            imgIcon.Source =
                (ImageSource)new BitmapToImageSourceConverter().Convert(icon.ToBitmap(), null, null, null);

            location = wrapper.IDL;
        }

        private void SetLocation(IDLWrapper wrapper) {
            SetLocation(wrapper, true);
        }

        private bool BrowseForFile() {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.FileName = SelectedPath;
            if(System.Windows.Forms.DialogResult.OK != ofd.ShowDialog()) {
                return false;
            }

            using(IDLWrapper wrapper = new IDLWrapper(ofd.FileName)) {
                SetLocation(wrapper);
            }
            return true;
        }

        private bool BrowseForFolder() {
            FolderBrowserDialogEx fbd = new FolderBrowserDialogEx();
            if(System.Windows.Forms.DialogResult.OK != fbd.ShowDialog()) {
                return false;
            }

            using(IDLWrapper wrapper = new IDLWrapper(fbd.SelectedIDL)) {
                SetLocation(wrapper);
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
            if(watermarkVisible) {
                return;
            }

            string path = txtLocation.Text;
            if((File && !System.IO.File.Exists(path)) || (Folder && !System.IO.Directory.Exists(path))) {
                ClearLocation();
                return;
            }

            using(IDLWrapper wrapper = new IDLWrapper(path)) {
                SetLocation(wrapper, false);
            }
        }
    }
}
