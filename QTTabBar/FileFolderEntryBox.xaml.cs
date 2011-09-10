using System.Drawing;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace QTTabBarLib {
    /// <summary>
    /// Interaction logic for FileFolderEntryBox.xaml
    /// </summary>
    public partial class FileFolderEntryBox : UserControl {
        private byte[] location = null;

        // TODO: Expose location
        public string SelectedPath { get; private set; }
        public byte[] SelectedIDL { get; private set; }

        // PENDING: the "!Folder" expression does not directly describe about using the file chooser,
        // so I think the class should have both of them
        public bool Folder { get; set; }
        public bool File {
            get {
                return !Folder;
            }
            set {
                Folder = !value;
            }
        }

        public FileFolderEntryBox() {
            InitializeComponent();
        }

        private void SetLocation(IDLWrapper wrapper) {
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
    }
}
