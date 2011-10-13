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
        private bool updating = false;

        public string SelectedPath {
            get {
                return (string)GetValue(SelectedPathProperty);
            }
            set {
                using(IDLWrapper wrapper = new IDLWrapper(value)) {
                    SetLocation(wrapper);
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
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedPathChanged));

        public FileFolderEntryBox() {
            InitializeComponent();

            ClearLocation();
        }

        private static void OnSelectedPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            FileFolderEntryBox this_ = (FileFolderEntryBox)d;
            if(this_.updating) {
                return;
            }
            using(IDLWrapper wrapper = new IDLWrapper((string)e.NewValue)) {
                this_.SetLocation(wrapper);
            }
        }

        // In any case, we must use this method instead of txtLocation.Text
        private void SetSelectedPath(string path, string displayName) {
            updating = true;

            SetValue(SelectedPathProperty, path);
            txtLocation.Text = displayName;

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
                text = (WatermarkText != null)
                    ? WatermarkText : string.Format("Choose your {0}", File ? "file" : "folder");
            }
            else {
                text = string.Empty;
            }
            SetSelectedPath(string.Empty, text);
        }

        private void ClearLocation() {
            if(!txtLocation.IsFocused) {
                ShowWatermark(true);
            }
            imgIcon.Source = null;
            location = null;
        }

        private void SetLocation(IDLWrapper wrapper) {
            if(string.IsNullOrEmpty(wrapper.Path)) {
                ClearLocation();
                return;
            }

            ShowWatermark(false);

            string text;
            if(wrapper.IDL == null) {
                text = wrapper.Path;
            }
            else if(File) {
                text = wrapper.Path;
            }
            else {
                bool b = File ? wrapper.IsFileSystemFile : wrapper.IsFileSystemFolder;
                text = b ? wrapper.Path : wrapper.DisplayName;
            }
            SetSelectedPath(wrapper.Path, text);

            Icon icon = QTUtility.GetIcon(wrapper.PIDL);
            imgIcon.Source =
                (ImageSource)new BitmapToImageSourceConverter().Convert(icon.ToBitmap(), null, null, null);

            location = wrapper.IDL;
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
            if(updating) {
                return;
            }

            string path = txtLocation.Text;
            if((File && !System.IO.File.Exists(path)) || (Folder && !System.IO.Directory.Exists(path))) {
                ClearLocation();
                SetSelectedPath(path, path);
                return;
            }

            using(IDLWrapper wrapper = new IDLWrapper(path)) {
                SetLocation(wrapper);
            }
        }
    }
}
