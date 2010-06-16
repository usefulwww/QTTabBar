using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;

using QTPlugin;
using QTPlugin.Interop;

namespace QuizoPlugins {
    /// <summary>
    /// Cut button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Cut", Version = "1.0.0.0", Description = "Cut files")]
    public class CutButton : IBarButton {
        private IPluginServer pluginServer;
        private IShellBrowser shellBrowser;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;
            this.shellBrowser = shellBrowser;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out this.ResStr)) {
                this.ResStr = new string[] { StringResources.ButtonNames[0] };
            }

            this.pluginServer.SelectionChanged += new PluginEventHandler(pluginServer_SelectionChanged);
            this.pluginServer.NavigationComplete += new PluginEventHandler(pluginServer_NavigationComplete);
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = null;
            return false;
        }

        public void Close(EndCode code) {
            this.pluginServer = null;
            this.shellBrowser = null;
        }

        public void OnShortcutKeyPressed(int index) {
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        #endregion


        #region IBarButton Members

        public void InitializeItem() {
        }

        public Image GetImage(bool fLarge) {
            return fLarge ? Resource.CutButton_large : Resource.CutButton_small;
        }

        public void OnButtonClick() {
            FileOps.FileOperation(FileOpActions.Cut, this.pluginServer.ExplorerHandle, this.shellBrowser);
        }

        public bool ShowTextLabel {
            get {
                return true;
            }
        }

        public string Text {
            get {
                return this.ResStr[0];
            }
        }

        #endregion


        private void pluginServer_NavigationComplete(object sender, PluginEventArgs e) {
            this.Update();
        }

        private void pluginServer_SelectionChanged(object sender, PluginEventArgs e) {
            this.Update();
        }

        private void Update() {
            Address[] addresses;
            if(this.pluginServer.TryGetSelection(out addresses)) {
                this.pluginServer.UpdateItem(this, addresses.Length > 0, false);
            }
        }
    }

    /// <summary>
    /// Copy button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Copy", Version = "1.0.0.0", Description = "Copy files")]
    public class CopyButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out this.ResStr)) {
                this.ResStr = new string[] { StringResources.ButtonNames[1] };
            }

            this.pluginServer.NavigationComplete += new PluginEventHandler(pluginServer_NavigationComplete);
            this.pluginServer.SelectionChanged += new PluginEventHandler(pluginServer_SelectionChanged);
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = null;
            return false;
        }

        public void Close(EndCode code) {
            this.pluginServer = null;
        }

        public void OnShortcutKeyPressed(int index) {
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        #endregion


        #region IBarButton Members

        public void InitializeItem() {
        }

        public Image GetImage(bool fLarge) {
            return fLarge ? Resource.CopyButton_large : Resource.CopyButton_small;
        }

        public void OnButtonClick() {
            FileOps.FileOperation(FileOpActions.Copy, this.pluginServer.ExplorerHandle, null);
        }

        public bool ShowTextLabel {
            get {
                return true;
            }
        }

        public string Text {
            get {
                return this.ResStr[0];
            }
        }

        #endregion


        private void pluginServer_NavigationComplete(object sender, PluginEventArgs e) {
            this.Update();
        }

        private void pluginServer_SelectionChanged(object sender, PluginEventArgs e) {
            this.Update();
        }

        private void Update() {
            Address[] addresses;
            if(this.pluginServer.TryGetSelection(out addresses)) {
                this.pluginServer.UpdateItem(this, addresses.Length > 0, false);
            }
        }
    }

    /// <summary>
    /// Paste Button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Paste", Version = "1.0.0.0", Description = "Paste files")]
    public class PasteButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;

        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out this.ResStr)) {
                this.ResStr = new string[] { StringResources.ButtonNames[2] };
            }
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = null;
            return false;
        }

        public void Close(EndCode code) {
            this.pluginServer = null;
        }

        public void OnShortcutKeyPressed(int index) {
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {

        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        #endregion

        #region IBarButton Members

        public void InitializeItem() {
        }

        public Image GetImage(bool fLarge) {
            return fLarge ? Resource.PasteButton_large : Resource.PasteButton_small;
        }

        public void OnButtonClick() {
            bool fFilesInClipboard = false;
            try {
                fFilesInClipboard = Clipboard.ContainsFileDropList();
            }
            catch {
            }

            if(fFilesInClipboard)
                FileOps.FileOperation(FileOpActions.Paste, this.pluginServer.ExplorerHandle, null);
            else
                System.Media.SystemSounds.Beep.Play();
        }

        public bool ShowTextLabel {
            get {
                return true;
            }
        }

        public string Text {
            get {
                return this.ResStr[0];
            }
        }

        #endregion
    }

    /// <summary>
    /// Delete button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Delete", Version = "1.0.0.0", Description = "Delete files")]
    public class DeleteButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out this.ResStr)) {
                this.ResStr = new string[] { StringResources.ButtonNames[3] };
            }

            this.pluginServer.NavigationComplete += new PluginEventHandler(pluginServer_NavigationComplete);
            this.pluginServer.SelectionChanged += new PluginEventHandler(pluginServer_SelectionChanged);
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = null;
            return false;
        }

        public void Close(EndCode code) {
            this.pluginServer = null;
        }

        public void OnShortcutKeyPressed(int index) {
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        #endregion


        #region IBarButton Members

        public void InitializeItem() {
        }

        public Image GetImage(bool fLarge) {
            return fLarge ? Resource.DeleteButton_large : Resource.DeleteButton_small;
        }

        public void OnButtonClick() {
            FileOps.FileOperation(FileOpActions.Delete, this.pluginServer.ExplorerHandle, null);
        }

        public bool ShowTextLabel {
            get {
                return true;
            }
        }

        public string Text {
            get {
                return this.ResStr[0];
            }
        }

        #endregion


        private void pluginServer_NavigationComplete(object sender, PluginEventArgs e) {
            this.Update();
        }

        private void pluginServer_SelectionChanged(object sender, PluginEventArgs e) {
            this.Update();
        }

        private void Update() {
            Address[] addresses;
            if(this.pluginServer.TryGetSelection(out addresses)) {
                this.pluginServer.UpdateItem(this, addresses.Length > 0, false);
            }
        }
    }

    /// <summary>
    /// CopyTo button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Copy To Folder", Version = "1.0.0.0", Description = "Open copy-to-folder dialog")]
    public class CopyToButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out this.ResStr)) {
                this.ResStr = new string[] { StringResources.ButtonNames[4] };
            }

            this.pluginServer.SelectionChanged += new PluginEventHandler(pluginServer_SelectionChanged);
            this.pluginServer.NavigationComplete += new PluginEventHandler(pluginServer_NavigationComplete);
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = new string[] { this.ResStr[0] };
            return true;
        }

        public void Close(EndCode code) {
            this.pluginServer = null;
        }

        public void OnShortcutKeyPressed(int index) {
            FileOps.FileOperation(FileOpActions.CopyTo, this.pluginServer.ExplorerHandle, null);
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        #endregion


        #region IBarButton Members

        public void InitializeItem() {
        }

        public Image GetImage(bool fLarge) {
            return fLarge ? Resource.CopyToButton_large : Resource.CopyToButton_small;
        }

        public void OnButtonClick() {
            FileOps.FileOperation(FileOpActions.CopyTo, this.pluginServer.ExplorerHandle, null);
        }

        public bool ShowTextLabel {
            get {
                return true;
            }
        }

        public string Text {
            get {
                return this.ResStr[0];
            }
        }

        #endregion


        private void pluginServer_NavigationComplete(object sender, PluginEventArgs e) {
            this.Update();
        }

        private void pluginServer_SelectionChanged(object sender, PluginEventArgs e) {
            this.Update();
        }

        private void Update() {
            Address[] addresses;
            if(this.pluginServer.TryGetSelection(out addresses)) {
                this.pluginServer.UpdateItem(this, addresses.Length > 0, false);
            }
        }
    }

    /// <summary>
    /// MoveTo button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Move To Folder", Version = "1.0.0.0", Description = "Open move-to-folder dialog")]
    public class MoveToButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out this.ResStr)) {
                this.ResStr = new string[] { StringResources.ButtonNames[5] };
            }

            this.pluginServer.SelectionChanged += new PluginEventHandler(pluginServer_SelectionChanged);
            this.pluginServer.NavigationComplete += new PluginEventHandler(pluginServer_NavigationComplete);
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = new string[] { this.ResStr[0] };
            return true;
        }

        public void Close(EndCode code) {
            this.pluginServer = null;
        }

        public void OnShortcutKeyPressed(int index) {
            FileOps.FileOperation(FileOpActions.MoveTo, this.pluginServer.ExplorerHandle, null);
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        #endregion


        #region IBarButton Members

        public void InitializeItem() {
        }

        public Image GetImage(bool fLarge) {
            return fLarge ? Resource.MoveToButton_large : Resource.MoveToButton_small;
        }

        public void OnButtonClick() {
            FileOps.FileOperation(FileOpActions.MoveTo, this.pluginServer.ExplorerHandle, null);
        }

        public bool ShowTextLabel {
            get {
                return true;
            }
        }

        public string Text {
            get {
                return this.ResStr[0];
            }
        }

        #endregion


        private void pluginServer_NavigationComplete(object sender, PluginEventArgs e) {
            this.Update();
        }

        private void pluginServer_SelectionChanged(object sender, PluginEventArgs e) {
            this.Update();
        }

        private void Update() {
            Address[] addresses;
            if(this.pluginServer.TryGetSelection(out addresses)) {
                this.pluginServer.UpdateItem(this, addresses.Length > 0, false);
            }
        }
    }

    /// <summary>
    /// Undo Button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Undo", Version = "1.0.0.0", Description = "Undo operation")]
    public class UndoButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;

        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out this.ResStr)) {
                this.ResStr = new string[] { StringResources.ButtonNames[6] };
            }
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = null;
            return false;
        }

        public void Close(EndCode code) {
            this.pluginServer = null;
        }

        public void OnShortcutKeyPressed(int index) {

        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {

        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        #endregion

        #region IBarButton Members

        public void InitializeItem() {
        }

        public Image GetImage(bool fLarge) {
            return fLarge ? Resource.UndoButton_large : Resource.UndoButton_small;
        }

        public void OnButtonClick() {
            FileOps.FileOperation(FileOpActions.Undo, this.pluginServer.ExplorerHandle, null);
        }

        public bool ShowTextLabel {
            get {
                return true;
            }
        }

        public string Text {
            get {
                return this.ResStr[0];
            }
        }

        #endregion
    }

    /// <summary>
    /// Send up button
    /// </summary>
    [Plugin(PluginType.Background, Author = "Quizo", Name = "Send To Parent", Version = "1.0.0.1", Description = "Send files to parent folder. This copies when Ctrl key is down.")]
    public class SendToParentButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out this.ResStr)) {
                this.ResStr = new string[] { StringResources.ButtonNames[7] };
            }

            this.pluginServer.SelectionChanged += new PluginEventHandler(pluginServer_SelectionChanged);
            this.pluginServer.NavigationComplete += new PluginEventHandler(pluginServer_NavigationComplete);
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = new string[] { this.ResStr[0] };
            return true;
        }

        public void Close(EndCode code) {
            if(code != EndCode.Hidden)
                this.pluginServer = null;
        }

        public void OnShortcutKeyPressed(int index) {
            if(!FileOps.MoveSelectedToParent(this.pluginServer))
                System.Media.SystemSounds.Beep.Play();
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        #endregion


        #region IBarButton Members

        public void InitializeItem() {
        }

        public Image GetImage(bool fLarge) {
            return fLarge ? Resource.SendToParentButton_large : Resource.SendToParentButton_small;
        }

        public void OnButtonClick() {
            if(!FileOps.MoveSelectedToParent(this.pluginServer))
                System.Media.SystemSounds.Beep.Play();
        }

        public bool ShowTextLabel {
            get {
                return true;
            }
        }

        public string Text {
            get {
                return this.ResStr[0];
            }
        }

        #endregion


        private void pluginServer_NavigationComplete(object sender, PluginEventArgs e) {
            this.Update();
        }

        private void pluginServer_SelectionChanged(object sender, PluginEventArgs e) {
            this.Update();
        }

        private void Update() {
            try {
                string currentPath = this.pluginServer.SelectedTab.Address.Path;

                Address[] addresses;
                if(this.pluginServer.TryGetSelection(out addresses)) {
                    bool fEnabled = currentPath != null && currentPath.Length > 3 && !currentPath.StartsWith("::") && addresses.Length > 0;

                    this.pluginServer.UpdateItem(this, fEnabled, false);
                }
            }
            catch {
                this.pluginServer.UpdateItem(this, false, false);
            }
        }
    }

    /// <summary>
    /// Properties button
    /// </summary>
    [Plugin(PluginType.Interactive, Author = "Quizo", Name = "Properties", Version = "1.0.0.1", Description = "Show file properties")]
    public class PropertiesButton : IBarButton {
        private IPluginServer pluginServer;
        private string[] ResStr;


        #region IPluginClient Members

        public void Open(IPluginServer pluginServer, IShellBrowser shellBrowser) {
            this.pluginServer = pluginServer;

            if(!pluginServer.TryGetLocalizedStrings(this, 1, out this.ResStr)) {
                this.ResStr = new string[] { StringResources.ButtonNames[8] };
            }
        }

        public bool QueryShortcutKeys(out string[] actions) {
            actions = new string[] { this.ResStr[0] };
            return true;
        }

        public void Close(EndCode code) {
            this.pluginServer = null;
        }

        public void OnShortcutKeyPressed(int index) {
            FileOps.FileOperation(FileOpActions.Properties, this.pluginServer.ExplorerHandle, null);
        }

        public void OnMenuItemClick(MenuType menuType, string menuText, ITab tab) {
        }

        public bool HasOption {
            get {
                return false;
            }
        }

        public void OnOption() {
        }

        #endregion


        #region IBarButton Members

        public void InitializeItem() {
        }

        public Image GetImage(bool fLarge) {
            return fLarge ? Resource.PropertiesButton_large : Resource.PropertiesButton_small;
        }

        public void OnButtonClick() {
            Address[] addresses;
            if(this.pluginServer.TryGetSelection(out addresses)) {
                if(addresses.Length > 0)
                    FileOps.FileOperation(FileOpActions.Properties, this.pluginServer.ExplorerHandle, null);
                else
                    FileOps.ShowProperties(this.pluginServer);
            }
        }

        public bool ShowTextLabel {
            get {
                return true;
            }
        }

        public string Text {
            get {
                return this.ResStr[0];
            }
        }

        #endregion

    }


    class StringResources {
        public static string[] ButtonNames;
        static StringResources() {
            if(System.Globalization.CultureInfo.CurrentCulture.Parent.Name == "ja") {
                ButtonNames = Resource.str_ja.Split(new char[] { ';' });
            }
            else {
                ButtonNames = Resource.str.Split(new char[] { ';' });
            }
        }
    }
}
