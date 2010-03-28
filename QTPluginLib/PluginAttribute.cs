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

namespace QTPlugin {
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PluginAttribute : Attribute {
        public string Author;
        public string Description;
        public string Name;
        public QTPlugin.PluginType PluginType;
        public string Version;

        public PluginAttribute(QTPlugin.PluginType pluginType) {
            this.PluginType = pluginType;
        }

        public PluginAttribute(QTPlugin.PluginType pluginType, Type tProvider)
            : this(pluginType, tProvider, -1) {
        }

        public PluginAttribute(QTPlugin.PluginType pluginType, Type tProvider, int iKey) {
            this.PluginType = pluginType;
            if(tProvider.IsSubclassOf(typeof(LocalizedStringProvider))) {
                try {
                    LocalizedStringProvider provider = (LocalizedStringProvider)Activator.CreateInstance(tProvider);
                    provider.SetKey(iKey);
                    this.Author = provider.Author;
                    this.Name = provider.Name;
                    this.Description = provider.Description;
                }
                catch(MissingMethodException) {
                    this.Author = string.Empty;
                    this.Name = "Name missing";
                    this.Description = "Default constuctor of LocalizedStringProvider is missing. \nContact the author of this plugin.";
                }
            }
            else {
                this.Author = string.Empty;
                this.Name = "Name missing";
                this.Description = "The type is not subclass of LocalizedStringProvider. \nContact the author of this plugin.";
            }
        }

        public PluginAttribute(QTPlugin.PluginType pluginType, string name, string author, string version, string description) {
            this.PluginType = pluginType;
            this.Name = name;
            this.Author = author;
            this.Version = version;
            this.Description = description;
        }
    }
}
