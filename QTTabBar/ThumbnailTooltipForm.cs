//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2010  Paul Accisano
//
//    QTTabBar is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    QTTabBar is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with QTTabBar.  If not, see <http://www.gnu.org/licenses/>.

namespace QTTabBarLib {
    using BandObjectLib;
    using QTTabBarLib.Interop;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;

    internal sealed class ThumbnailTooltipForm : Form {
        private IContainer components;
        private const string EMPTYFILE = "  *empty file";
        private bool fFontAsigned;
        private bool fIsShownByKey;
        private ImageCacheStore imageCacheStore;
        private const string IOERROR_CANNOTACCESS = "  *Access Error!";
        private static IOException ioException;
        private bool isShowing;
        private Label lblInfo;
        private Label lblText;
        private List<string> lstPathFailedThumbnail;
        private const int MAX_CACHE_LENGTH = 0x80;
        private const int MAX_TEXT_LENGTH = 0x400;
        public const int MAX_THUMBNAIL_HEIGHT = 0x4b0;
        public const int MAX_THUMBNAIL_WIDTH = 0x780;
        private int maxHeight = QTUtility.PreviewMaxHeight;
        private int maxWidth = QTUtility.PreviewMaxWidth;
        private PictureBox pictureBox1;
        private static string supportedImages;
        private static string supportedMovies = ".asf;.asx;.avi;.dvr-ms;.m1v;.wmv;.mpg;.mpeg;.mp2;.flv";

        public event QEventHandler ThumbnailVisibleChanged;

        public ThumbnailTooltipForm() {
            this.InitializeComponent();
            this.lstPathFailedThumbnail = new List<string>();
            this.imageCacheStore = new ImageCacheStore(0x80);
        }

        public void ClearCache() {
            this.imageCacheStore.Clear();
        }

        private bool CreateThumbnail(string path, ref Size formSize) {
            string ext = Path.GetExtension(path).ToLower();
            if(ExtIsImage(ext)) {
                FileInfo info = new FileInfo(path);
                if(!info.Exists || (info.Length <= 0L)) {
                    goto Label_0635;
                }
                bool flag = false;
                bool thumbnail = false;
                bool fCached = false;
                Bitmap bitmap = null;
                ImageData item = null;
                Size empty = Size.Empty;
                Size sizeActual = Size.Empty;
                this.lblInfo.Text = string.Empty;
                string toolTipText = null;
                if((this.maxWidth != QTUtility.PreviewMaxWidth) || (this.maxHeight != QTUtility.PreviewMaxHeight)) {
                    this.maxWidth = QTUtility.PreviewMaxWidth;
                    this.maxHeight = QTUtility.PreviewMaxHeight;
                    this.pictureBox1.Image = null;
                    this.imageCacheStore.Clear();
                    this.lstPathFailedThumbnail.Clear();
                }
                foreach(ImageData data2 in this.imageCacheStore) {
                    if(string.Equals(data2.Path, path, StringComparison.OrdinalIgnoreCase)) {
                        if(data2.ModifiedDate == info.LastWriteTime) {
                            bitmap = data2.Bitmap;
                            thumbnail = data2.Thumbnail;
                            empty = data2.RawSize;
                            sizeActual = data2.ZoomedSize;
                            toolTipText = data2.TooltipText;
                            flag = true;
                        }
                        else {
                            item = data2;
                        }
                        break;
                    }
                }
                if(item != null) {
                    this.imageCacheStore.Remove(item);
                }
                if(!flag) {
                    try {
                        ImageData data3 = null;
                        if(!ExtIsDefaultImage(ext)) {
                            if(this.lstPathFailedThumbnail.Contains(path)) {
                                return false;
                            }
                            thumbnail = true;
                            if(QTUtility.IsVista) {
                                data3 = LoadThumbnail(path, info.LastWriteTime, out empty, out sizeActual, out toolTipText, out fCached);
                            }
                            else {
                                data3 = LoadThumbnail2(path, info.LastWriteTime, out empty, out sizeActual, out toolTipText, out fCached);
                            }
                        }
                        else {
                            data3 = LoadImageFile(path, info.LastWriteTime, out empty, out sizeActual);
                        }
                        if(data3 == null) {
                            this.lstPathFailedThumbnail.Add(path);
                            return false;
                        }
                        bitmap = data3.Bitmap;
                        this.imageCacheStore.Add(data3);
                    }
                    catch {
                        return false;
                    }
                }
                int width = 0x9e;
                if(width < sizeActual.Width) {
                    width = sizeActual.Width;
                }
                bool flag4 = false;
                if(!QTUtility.CheckConfig(14, 8)) {
                    SizeF ef;
                    string text = Path.GetFileName(path) + "\r\n";
                    if(thumbnail && (toolTipText != null)) {
                        text = text + toolTipText;
                    }
                    else {
                        bool flag5 = sizeActual == empty;
                        text = text + FormatSize(info.Length);
                        if(!thumbnail) {
                            object obj2 = text;
                            text = string.Concat(new object[] { obj2, "    ( ", empty.Width, " x ", empty.Height, " )", flag5 ? string.Empty : "*" });
                        }
                        text = text + "\r\n" + info.LastWriteTime.ToString();
                    }
                    using(Graphics graphics = this.lblInfo.CreateGraphics()) {
                        ef = graphics.MeasureString(text, this.lblInfo.Font, (int)(width - 8));
                    }
                    this.lblInfo.SuspendLayout();
                    this.lblInfo.Text = text;
                    this.lblInfo.Width = width;
                    this.lblInfo.Height = (int)(ef.Height + 8f);
                    this.lblInfo.ResumeLayout();
                    formSize = new Size(width + 8, (sizeActual.Height + this.lblInfo.Height) + 8);
                }
                else {
                    flag4 = true;
                    formSize = new Size(width + 8, sizeActual.Height + 8);
                }
                try {
                    base.SuspendLayout();
                    if(flag4) {
                        this.lblInfo.Dock = DockStyle.None;
                    }
                    else {
                        this.lblInfo.Dock = DockStyle.Bottom;
                        this.lblInfo.BringToFront();
                    }
                    this.pictureBox1.SuspendLayout();
                    this.pictureBox1.SizeMode = (sizeActual != bitmap.Size) ? PictureBoxSizeMode.Zoom : PictureBoxSizeMode.CenterImage;
                    this.pictureBox1.Image = bitmap;
                    this.pictureBox1.ResumeLayout();
                    this.pictureBox1.BringToFront();
                    base.ResumeLayout();
                    return true;
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, null);
                    goto Label_0635;
                }
            }
            if(ExtIsText(ext)) {
                FileInfo info2 = new FileInfo(path);
                if(info2.Exists) {
                    try {
                        SizeF ef2;
                        bool fLoadedAll = false;
                        bool flag7 = false;
                        string str4 = string.Empty;
                        ioException = null;
                        if(info2.Length > 0L) {
                            str4 = LoadTextFile(path, out fLoadedAll);
                        }
                        else {
                            flag7 = true;
                            str4 = "  *empty file";
                        }
                        this.lblText.ForeColor = (ioException != null) ? Color.Red : (flag7 ? SystemColors.GrayText : SystemColors.InfoText);
                        if(!string.IsNullOrEmpty(QTUtility.PreviewFontName)) {
                            if((this.lblText.Font.Name != QTUtility.PreviewFontName) || (this.lblText.Font.Size != QTUtility.PreviewFontSize)) {
                                this.fFontAsigned = true;
                                try {
                                    this.lblText.Font = new Font(QTUtility.PreviewFontName, QTUtility.PreviewFontSize);
                                }
                                catch {
                                }
                            }
                        }
                        else if(this.fFontAsigned) {
                            this.fFontAsigned = false;
                            this.lblText.Font = null;
                        }
                        int num2 = 0x100;
                        if(this.fFontAsigned) {
                            num2 = Math.Max((int)(num2 * (QTUtility.PreviewFontSize / Control.DefaultFont.Size)), 0x80);
                            formSize.Width = num2;
                        }
                        using(Graphics graphics2 = this.lblText.CreateGraphics()) {
                            ef2 = graphics2.MeasureString(str4, this.lblText.Font, num2);
                        }
                        if((ef2.Height < 512f) || fLoadedAll) {
                            formSize.Height = (int)(ef2.Height + 8f);
                        }
                        else {
                            formSize.Height = 0x200;
                        }
                        base.SuspendLayout();
                        this.lblInfo.Dock = DockStyle.None;
                        this.lblText.Text = str4;
                        this.lblText.BringToFront();
                        base.ResumeLayout();
                        return true;
                    }
                    catch(Exception exception2) {
                        QTUtility2.MakeErrorLog(exception2, null);
                        return false;
                    }
                }
            }
        Label_0635:
            return false;
        }

        protected override void Dispose(bool disposing) {
            this.imageCacheStore.Clear();
            if(disposing && (this.components != null)) {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private static bool ExtIsDefaultImage(string ext) {
            return GetGDIPSupportedImages().Contains(ext.ToLower());
        }

        private static bool ExtIsImage(string ext) {
            return (((ext.Length != 0) && QTUtility.PreviewExtsList_Img.Contains(ext.ToLower())) && (ext != ".ico"));
        }

        public static bool ExtIsSupported(string ext) {
            if(!ExtIsImage(ext)) {
                return ExtIsText(ext);
            }
            return true;
        }

        private static bool ExtIsText(string ext) {
            return ((ext.Length != 0) && QTUtility.PreviewExtsList_Txt.Contains(ext.ToLower()));
        }

        private static string FormatSize(long size) {
            string str = size + " bytes";
            if(size >= 0x400L) {
                str = Math.Round((double)(((double)size) / 1024.0), 1) + " KB";
            }
            if(size >= 0x100000L) {
                str = Math.Round((double)(((double)size) / 1048576.0), 1) + " MB";
            }
            return str;
        }

        private static string GetGDIPSupportedImages() {
            if(supportedImages == null) {
                ImageCodecInfo[] imageDecoders = ImageCodecInfo.GetImageDecoders();
                StringBuilder builder = new StringBuilder();
                foreach(ImageCodecInfo info in imageDecoders) {
                    builder.Append(info.FilenameExtension + ";");
                }
                supportedImages = builder.ToString().ToLower().Replace("*", string.Empty).Replace(".ico;", string.Empty);
            }
            return supportedImages;
        }

        public bool HideToolTip() {
            if(this.fIsShownByKey) {
                this.fIsShownByKey = false;
                return false;
            }
            this.isShowing = false;
            PInvoke.ShowWindow(base.Handle, 0);
            this.pictureBox1.Image = null;
            if(this.ThumbnailVisibleChanged != null) {
                this.ThumbnailVisibleChanged(this, new QEventArgs(ArrowDirection.Down));
            }
            return true;
        }

        private void InitializeComponent() {
            this.pictureBox1 = new PictureBox();
            this.lblText = new Label();
            this.lblInfo = new Label();
            ((ISupportInitialize)this.pictureBox1).BeginInit();
            base.SuspendLayout();
            this.lblInfo.ForeColor = SystemColors.InfoText;
            this.lblInfo.BackColor = Color.Transparent;
            this.lblInfo.Dock = DockStyle.Bottom;
            this.lblInfo.Padding = new Padding(4);
            this.lblInfo.Size = new Size(0x10, 50);
            this.lblInfo.UseMnemonic = false;
            this.pictureBox1.BackColor = Color.Transparent;
            this.pictureBox1.Dock = DockStyle.Fill;
            this.pictureBox1.Location = new Point(0, 0);
            this.pictureBox1.Padding = new Padding(4);
            this.pictureBox1.Size = new Size(0x100, 0x80);
            this.pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            this.pictureBox1.TabStop = false;
            this.lblText.AutoEllipsis = true;
            this.lblText.ForeColor = SystemColors.InfoText;
            this.lblText.BackColor = Color.Transparent;
            this.lblText.Dock = DockStyle.Fill;
            this.lblText.Location = new Point(0, 0);
            this.lblText.Padding = new Padding(4);
            this.lblText.Size = new Size(0x100, 0x80);
            this.lblText.UseMnemonic = false;
            base.AutoScaleDimensions = new SizeF(6f, 13f);
            base.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = SystemColors.Info;
            base.ClientSize = new Size(0x100, 0x80);
            base.Controls.Add(this.lblText);
            base.Controls.Add(this.pictureBox1);
            base.Controls.Add(this.lblInfo);
            base.FormBorderStyle = FormBorderStyle.None;
            base.ShowInTaskbar = false;
            base.StartPosition = FormStartPosition.Manual;
            ((ISupportInitialize)this.pictureBox1).EndInit();
            base.ResumeLayout(false);
        }

        private static ImageData LoadImageFile(string path, DateTime dtLastWriteTime, out Size sizeRaw, out Size sizeActual) {
            sizeRaw = sizeActual = Size.Empty;
            using(Bitmap bitmap = new Bitmap(path)) {
                if(bitmap != null) {
                    sizeRaw = bitmap.Size;
                    int width = sizeRaw.Width;
                    int height = sizeRaw.Height;
                    if((height > QTUtility.PreviewMaxHeight) || (width > QTUtility.PreviewMaxWidth)) {
                        if(height > QTUtility.PreviewMaxHeight) {
                            width = (int)((((double)QTUtility.PreviewMaxHeight) / ((double)height)) * width);
                            height = QTUtility.PreviewMaxHeight;
                            if(width > QTUtility.PreviewMaxWidth) {
                                height = (int)((((double)QTUtility.PreviewMaxWidth) / ((double)width)) * height);
                                width = QTUtility.PreviewMaxWidth;
                            }
                        }
                        else {
                            height = (int)((((double)QTUtility.PreviewMaxWidth) / ((double)width)) * height);
                            width = QTUtility.PreviewMaxWidth;
                        }
                        sizeActual = new Size(width, height);
                        if(ImageAnimator.CanAnimate(bitmap)) {
                            MemoryStream stream = new MemoryStream();
                            bitmap.Save(stream, bitmap.RawFormat);
                            return new ImageData(new Bitmap(stream), stream, path, dtLastWriteTime, sizeRaw, sizeActual);
                        }
                        return new ImageData(new Bitmap(bitmap, width, height), null, path, dtLastWriteTime, sizeRaw, sizeActual);
                    }
                    sizeActual = sizeRaw;
                    MemoryStream stream2 = new MemoryStream();
                    bitmap.Save(stream2, bitmap.RawFormat);
                    return new ImageData(new Bitmap(stream2), stream2, path, dtLastWriteTime, sizeRaw, sizeRaw);
                }
            }
            return null;
        }

        private static string LoadTextFile(string path, out bool fLoadedAll) {
            byte[] buffer;
            int count = 0x400;
            string str = string.Empty;
            fLoadedAll = false;
            try {
                using(FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                    if(stream.Length < count) {
                        fLoadedAll = true;
                        count = (int)stream.Length;
                    }
                    buffer = new byte[count];
                    stream.Read(buffer, 0, count);
                }
            }
            catch(IOException exception) {
                ioException = exception;
                return "  *Access Error!";
            }
            if(buffer.Length <= 0) {
                return str;
            }
            Encoding encoding = null;
            if(PluginManager.IEncodingDetector != null) {
                try {
                    encoding = PluginManager.IEncodingDetector.GetEncoding(ref buffer);
                }
                catch(Exception exception2) {
                    PluginManager.HandlePluginException(exception2, IntPtr.Zero, "Unknown IEncodingDetector", "Getting Enconding object.");
                    QTUtility2.MakeErrorLog(exception2, null, false);
                }
            }
            if(encoding == null) {
                encoding = TxtEnc.GetEncoding(ref buffer);
                if((encoding == null) || ((((Encoding.Default.CodePage != 0x3a4) && (encoding.CodePage != 0xfde8)) && ((encoding.CodePage != 0xfde9) && (encoding.CodePage != 0x4b0))) && (encoding.CodePage != 0x2ee0))) {
                    encoding = Encoding.Default;
                }
            }
            return encoding.GetString(buffer);
        }

        private static ImageData LoadThumbnail(string path, DateTime dtLastWriteTime, out Size sizeRaw, out Size sizeActual, out string toolTipText, out bool fCached) {
            sizeRaw = sizeActual = Size.Empty;
            toolTipText = null;
            fCached = false;
            IntPtr zero = IntPtr.Zero;
            QTTabBarLib.Interop.IShellItem ppsi = null;
            ISharedBitmap ppvThumb = null;
            LocalThumbnailCache o = null;
            try {
                zero = PInvoke.ILCreateFromPath(path);
                if((zero != IntPtr.Zero) && (PInvoke.SHCreateShellItem(IntPtr.Zero, null, zero, out ppsi) == 0)) {
                    o = new LocalThumbnailCache();
                    IThumbnailCache cache2 = (IThumbnailCache)o;
                    uint flags = 0;
                    uint pOutFlags = 0;
                    WTS_THUMBNAILID pThumbnailID = new WTS_THUMBNAILID();
                    uint cxyRequestedThumbSize = (uint)Math.Min(0x400, Math.Min(QTUtility.PreviewMaxWidth, QTUtility.PreviewMaxHeight));
                    if(cache2.GetThumbnail(ppsi, cxyRequestedThumbSize, flags, out ppvThumb, ref pOutFlags, ref pThumbnailID) == 0) {
                        IntPtr ptr2;
                        if((pOutFlags & 2) == 2) {
                            fCached = true;
                        }
                        if(ppvThumb.Detach(out ptr2) == 0) {
                            Bitmap bmp = Image.FromHbitmap(ptr2);
                            Size size = bmp.Size;
                            sizeRaw = sizeActual = size;
                            ImageData data = new ImageData(bmp, null, path, dtLastWriteTime, size, size);
                            data.Thumbnail = true;
                            try {
                                toolTipText = data.TooltipText = ShellMethods.GetShellInfoTipText(zero, false);
                            }
                            catch {
                            }
                            return data;
                        }
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
            finally {
                if(zero != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(zero);
                }
                if(ppsi != null) {
                    Marshal.ReleaseComObject(ppsi);
                }
                if(ppvThumb != null) {
                    Marshal.ReleaseComObject(ppvThumb);
                }
                if(o != null) {
                    Marshal.ReleaseComObject(o);
                }
            }
            return null;
        }

        private static ImageData LoadThumbnail2(string path, DateTime dtLastWriteTime, out Size sizeRaw, out Size sizeActual, out string toolTipText, out bool fCached) {
            sizeRaw = sizeActual = Size.Empty;
            toolTipText = null;
            fCached = false;
            IntPtr zero = IntPtr.Zero;
            IShellFolder ppv = null;
            object obj2 = null;
            try {
                IntPtr ptr3;
                zero = PInvoke.ILCreateFromPath(path);
                if((zero != IntPtr.Zero) && (PInvoke.SHBindToParent(zero, ExplorerGUIDs.IID_IShellFolder, out ppv, out ptr3) == 0)) {
                    uint rgfReserved = 0;
                    Guid riid = ExplorerGUIDs.IID_IExtractImage;
                    IntPtr[] apidl = new IntPtr[] { ptr3 };
                    if(ppv.GetUIObjectOf(IntPtr.Zero, 1, apidl, ref riid, ref rgfReserved, out obj2) == 0) {
                        IntPtr ptr2;
                        IExtractImage image = (IExtractImage)obj2;
                        StringBuilder pszPathBuffer = new StringBuilder(260);
                        int pdwPriority = 0;
                        Size prgSize = new Size(QTUtility.PreviewMaxWidth, QTUtility.PreviewMaxHeight);
                        int pdwFlags = 0x60;
                        if(((image.GetLocation(pszPathBuffer, pszPathBuffer.Capacity, ref pdwPriority, ref prgSize, 0x18, ref pdwFlags) == 0) && (image.Extract(out ptr2) == 0)) && (ptr2 != IntPtr.Zero)) {
                            Bitmap bmp = Image.FromHbitmap(ptr2);
                            Size size = bmp.Size;
                            sizeRaw = sizeActual = size;
                            ImageData data = new ImageData(bmp, null, path, dtLastWriteTime, size, size);
                            data.Thumbnail = true;
                            try {
                                toolTipText = data.TooltipText = ShellMethods.GetShellInfoTipText(zero, false);
                            }
                            catch {
                            }
                            return data;
                        }
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, null);
            }
            finally {
                if(zero != IntPtr.Zero) {
                    PInvoke.CoTaskMemFree(zero);
                }
                if(ppv != null) {
                    Marshal.ReleaseComObject(ppv);
                }
                if(obj2 != null) {
                    Marshal.ReleaseComObject(obj2);
                }
            }
            return null;
        }

        internal static List<string> MakeDefaultImgExts() {
            StringBuilder builder = new StringBuilder();
            builder.Append(GetGDIPSupportedImages());
            builder.Append(supportedMovies);
            return new List<string>(builder.ToString().Split(QTUtility.SEPARATOR_CHAR));
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            if(QTUtility.IsVista && VisualStyleRenderer.IsSupported) {
                new VisualStyleRenderer(VisualStyleElement.ToolTip.Standard.Normal).DrawBackground(e.Graphics, new Rectangle(0, 0, base.Width, base.Height));
            }
            else {
                base.OnPaintBackground(e);
                e.Graphics.DrawRectangle(SystemPens.InfoText, new Rectangle(0, 0, base.Width - 1, base.Height - 1));
            }
        }

        public bool ShowToolTip(string path, Point pnt) {
            Size formSize = new Size(0x100, 0x80);
            if(!this.CreateThumbnail(path, ref formSize)) {
                return false;
            }
            Rectangle workingArea = Screen.FromPoint(pnt).WorkingArea;
            int num = ((formSize.Width + pnt.X) - workingArea.Right) + 8;
            int num2 = ((formSize.Height + pnt.Y) - workingArea.Bottom) + 0x10;
            bool flag = (workingArea.Right - pnt.X) < (pnt.X - workingArea.Left);
            bool flag2 = (workingArea.Bottom - pnt.Y) < (pnt.Y - workingArea.Top);
            bool flag3 = false;
            if((num > 0) && (num2 > 0)) {
                if(flag) {
                    pnt.X -= ((formSize.Width + 0x18) + 0x20) + 0x10;
                }
                if(flag2) {
                    pnt.Y -= formSize.Height + 0x20;
                    flag3 = true;
                }
            }
            else if(num > 0) {
                if(flag) {
                    pnt.X -= num + 0x20;
                }
            }
            else if(num2 > 0) {
                pnt.Y -= num2 + 0x10;
                flag3 = true;
            }
            if(pnt.X < workingArea.X) {
                pnt.X = workingArea.X + 8;
                if(!flag3) {
                    pnt.Y += 8;
                }
            }
            this.isShowing = true;
            PInvoke.SetWindowPos(base.Handle, (IntPtr)(-1), pnt.X + 0x18, pnt.Y + 0x10, formSize.Width, formSize.Height, 0x10);
            PInvoke.ShowWindow(base.Handle, 4);
            if(this.ThumbnailVisibleChanged != null) {
                this.ThumbnailVisibleChanged(this, new QEventArgs(ArrowDirection.Up));
            }
            return true;
        }

        public bool ShowToolTip(string path, Rectangle rctMenuItem) {
            Size formSize = new Size(0x100, 0x80);
            if(!this.CreateThumbnail(path, ref formSize)) {
                return false;
            }
            Point point = new Point(rctMenuItem.Right + 8, rctMenuItem.Bottom);
            Rectangle workingArea = Screen.FromPoint(rctMenuItem.Location).WorkingArea;
            bool flag = (workingArea.Right - point.X) < (point.X - workingArea.Left);
            int bottom = workingArea.Bottom;
            int y = point.Y;
            int num3 = point.Y;
            int top = workingArea.Top;
            bool flag2 = false;
            if(((((formSize.Width + point.X) - workingArea.Right) + 8) > 0) && flag) {
                point.X = (rctMenuItem.X - formSize.Width) - 8;
            }
            if((((formSize.Height + point.Y) - workingArea.Bottom) + 8) > 0) {
                point.Y = (workingArea.Bottom - formSize.Height) - 0x10;
                flag2 = true;
            }
            if(point.X < workingArea.X) {
                point.X = workingArea.X + 8;
                if(!flag2) {
                    point.Y += 8;
                }
            }
            this.isShowing = true;
            PInvoke.SetWindowPos(base.Handle, (IntPtr)(-1), point.X, point.Y, formSize.Width, formSize.Height, 0x10);
            PInvoke.ShowWindow(base.Handle, 4);
            return true;
        }

        protected override System.Windows.Forms.CreateParams CreateParams {
            get {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.ClassStyle |= 0x20000;
                return createParams;
            }
        }

        public bool IsShowing {
            get {
                return this.isShowing;
            }
        }

        public bool IsShownByKey {
            get {
                return this.fIsShownByKey;
            }
            set {
                this.fIsShownByKey = value;
            }
        }

        private sealed class ImageCacheStore : Collection<ThumbnailTooltipForm.ImageData> {
            private int max_cache_length;
            private object syncObject = new object();

            public ImageCacheStore(int max_cache_length) {
                this.max_cache_length = max_cache_length;
            }

            protected override void ClearItems() {
                lock(this.syncObject) {
                    foreach(ThumbnailTooltipForm.ImageData data in this) {
                        data.Dispose();
                    }
                    base.ClearItems();
                }
            }

            protected override void InsertItem(int index, ThumbnailTooltipForm.ImageData item) {
                lock(this.syncObject) {
                    base.InsertItem(index, item);
                    if(base.Count > this.max_cache_length) {
                        base[0].Dispose();
                        base.RemoveItem(0);
                    }
                }
            }

            protected override void RemoveItem(int index) {
                lock(this.syncObject) {
                    base[index].Dispose();
                    base.RemoveItem(index);
                }
            }

            protected override void SetItem(int index, ThumbnailTooltipForm.ImageData item) {
                lock(this.syncObject) {
                    base.SetItem(index, item);
                }
            }
        }

        private sealed class ImageData : IDisposable {
            public System.Drawing.Bitmap Bitmap;
            public DateTime ModifiedDate;
            public MemoryStream ms;
            public string Path;
            public Size RawSize;
            public bool Thumbnail;
            public string TooltipText;
            public Size ZoomedSize;

            public ImageData(System.Drawing.Bitmap bmp, MemoryStream memoryStream, string path, DateTime dtModified, Size sizeRaw, Size sizeZoomed) {
                this.Bitmap = bmp;
                this.ms = memoryStream;
                this.Path = path;
                this.ModifiedDate = dtModified;
                this.RawSize = sizeRaw;
                this.ZoomedSize = sizeZoomed;
            }

            public void Dispose() {
                try {
                    if(this.Bitmap != null) {
                        this.Bitmap.Dispose();
                        this.Bitmap = null;
                    }
                    if(this.ms != null) {
                        this.ms.Dispose();
                        this.ms = null;
                    }
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, null);
                }
            }
        }
    }
}
