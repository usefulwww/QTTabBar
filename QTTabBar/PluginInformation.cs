//    This file is part of QTTabBar, a shell extension for Microsoft
//    Windows Explorer.
//    Copyright (C) 2007-2010  Quizo, Paul Accisano
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

using System;
using System.Drawing;
using QTPlugin;

namespace QTTabBarLib {
    internal sealed class PluginInformation : IDisposable {
        public string Author;
        public string Description;
        public bool Enabled;
        public Image ImageLarge;
        public Image ImageSmall;
        public int Index;
        public string Name;
        public string Path;
        public string PluginID;
        public PluginType PluginType;
        public string[] ShortcutKeyActions;
        public string TypeFullName;
        public string Version;

        public PluginInformation(PluginAttribute pluginAtt, string path, string pluginID, string typeFullName) {
            Author = pluginAtt.Author;
            Name = pluginAtt.Name;
            Version = pluginAtt.Version;
            Description = pluginAtt.Description;
            PluginType = pluginAtt.PluginType;
            Path = path;
            PluginID = pluginID;
            TypeFullName = typeFullName;
        }

        public PluginInformation Clone(int index) {
            PluginInformation information = (PluginInformation)MemberwiseClone();
            information.Index = index;
            return information;
        }

        public void Dispose() {
            if(ImageLarge != null) {
                ImageLarge.Dispose();
                ImageLarge = null;
            }
            if(ImageSmall != null) {
                ImageSmall.Dispose();
                ImageSmall = null;
            }
        }
    }
}
