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
        private bool watermark = true;

        // TODO: Expose location
        public string SelectedPath { get; private set; }
        public byte[] SelectedIDL { get; private set; }

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

        // TODO:
        public bool ShowIcon { set; get; }

        public FileFolderEntryBox() {
            InitializeComponent();

            ClearLocation();
        }

        private void EnableWatermark(bool b) {
            // TODO: hide the icon area if b == true

            //txtLocation.FontStyle = b ? FontStyles.Italic : FontStyles.Normal;
            txtLocation.Foreground = b ? Brushes.DarkGray : Brushes.Black;
            watermark = b;
        }

        private void ClearLocation() {
            EnableWatermark(true);

            // TODO: localize this
            txtLocation.Text = string.Format("Choose your {0}...", File ? "file" : "folder");
            imgIcon.Source = null;
            location = null;
        }

        private void SetLocation(IDLWrapper wrapper) {
            EnableWatermark(false);

            txtLocation.Text =
                wrapper.IsFileSystemFolder ? wrapper.Path : wrapper.DisplayName;

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
            // TODO: delete this
            Folder = true;

            Browse();
        }

        private void txtLocation_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e) {
            if(watermark) {
                EnableWatermark(false);
                txtLocation.Text = string.Empty;
            }
        }

        private void txtLocation_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e) {
            if(string.IsNullOrEmpty(txtLocation.Text)) {
                ClearLocation();
            }
        }
    }
}
