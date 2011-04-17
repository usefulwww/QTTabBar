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
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using QTPlugin;

namespace QTTabBarLib {
    internal sealed class PluginAssembly : IDisposable {
        private Assembly assembly;
        public string Author;
        public string Description;
        private Dictionary<string, PluginInformation> dicPluginInformations = new Dictionary<string, PluginInformation>();
        public bool Enabled;
        private static string IMGLARGE = "_large";
        private static string IMGSMALL = "_small";
        public string Name;
        public string Path;
        private static string RESNAME = "Resource";
        private static Type T_PLUGINATTRIBUTE = typeof(PluginAttribute);
        public string Title;
        private static string TYPENAME_PLUGINCLIENT = typeof(IPluginClient).FullName;
        public string Version;

        public PluginAssembly(string path) {
            Path = path;
            Title = Author = Description = Version = Name = string.Empty;
            if(File.Exists(path)) {
                try {
                    assembly = Assembly.Load(File.ReadAllBytes(path));
                    AssemblyName name = assembly.GetName();
                    AssemblyTitleAttribute customAttribute = (AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute));
                    AssemblyCompanyAttribute attribute2 = (AssemblyCompanyAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyCompanyAttribute));
                    AssemblyDescriptionAttribute attribute3 = (AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute));
                    Version = name.Version.ToString();
                    if(customAttribute != null) {
                        Title = customAttribute.Title;
                    }
                    if(attribute2 != null) {
                        Author = attribute2.Company;
                    }
                    if(attribute3 != null) {
                        Description = attribute3.Description;
                    }
                    Name = Title + Version + "(" + path.GetHashCode().ToString("X") + ")";
                    foreach(Type type in assembly.GetTypes()) {
                        try {
                            if(ValidateType(type)) {
                                PluginAttribute pluginAtt = Attribute.GetCustomAttribute(type, T_PLUGINATTRIBUTE) as PluginAttribute;
                                if(pluginAtt != null) {
                                    string pluginID = Name + "+" + type.FullName;
                                    PluginInformation info = new PluginInformation(pluginAtt, path, pluginID, type.FullName);
                                    GetImageFromAssembly(assembly, type, info);
                                    dicPluginInformations[pluginID] = info;
                                }
                                else {
                                    QTUtility2.MakeErrorLog(null, "failed attribute");
                                }
                            }
                        }
                        catch {
                        }
                    }
                }
                catch(ReflectionTypeLoadException exception) {
                    QTUtility2.MakeErrorLog(exception, "Failed to load plugin assembly.\r\n" 
                            + exception.LoaderExceptions.StringJoin("\r\n") + "\r\n" + path);
                }
                catch(Exception exception) {
                    QTUtility2.MakeErrorLog(exception, "Failed to load plugin assembly.\r\n" + path);
                }
            }
        }

        public void Dispose() {
            assembly = null;
            foreach(PluginInformation information in dicPluginInformations.Values) {
                information.Dispose();
            }
            dicPluginInformations.Clear();
        }

        private static void GetImageFromAssembly(Assembly asm, Type type, PluginInformation info) {
            try {
                Type type2 = asm.GetType(type.Namespace + "." + RESNAME);
                if(type2 != null) {
                    PropertyInfo property = type2.GetProperty(type.Name + IMGLARGE, BindingFlags.NonPublic | BindingFlags.Static);
                    PropertyInfo info3 = type2.GetProperty(type.Name + IMGSMALL, BindingFlags.NonPublic | BindingFlags.Static);
                    if(property != null) {
                        info.ImageLarge = (Image)property.GetValue(null, null);
                    }
                    if(info3 != null) {
                        info.ImageSmall = (Image)info3.GetValue(null, null);
                    }
                }
            }
            catch {
            }
        }

        public Plugin Load(string pluginID) {
            if(File.Exists(Path)) {
                try {
                    PluginInformation information;
                    if(dicPluginInformations.TryGetValue(pluginID, out information)) {
                        IPluginClient pluginClient = assembly.CreateInstance(information.TypeFullName) as IPluginClient;
                        if(pluginClient != null) {
                            Plugin plugin = new Plugin(pluginClient, information);
                            IBarButton button = pluginClient as IBarButton;
                            if(button != null) {
                                Image imageLarge = information.ImageLarge;
                                Image imageSmall = information.ImageSmall;
                                try {
                                    Image image = button.GetImage(true);
                                    Image image4 = button.GetImage(false);
                                    if(image != null) {
                                        information.ImageLarge = image;
                                        if(imageLarge != null) {
                                            imageLarge.Dispose();
                                        }
                                    }
                                    if(image4 != null) {
                                        information.ImageSmall = image4;
                                        if(imageSmall != null) {
                                            imageSmall.Dispose();
                                        }
                                    }
                                }
                                catch(Exception exception) {
                                    PluginManager.HandlePluginException(exception, IntPtr.Zero, information.Name, "Getting image from pluging.");
                                    throw;
                                }
                            }
                            return plugin;
                        }
                    }
                }
                catch(Exception exception2) {
                    QTUtility2.MakeErrorLog(exception2, null);
                }
            }
            return null;
        }

        public bool TryGetPluginInformation(string pluginID, out PluginInformation info) {
            return dicPluginInformations.TryGetValue(pluginID, out info);
        }

        public void Uninstall() {
            try {
                foreach(Type type in assembly.GetTypes()) {
                    try {
                        if(ValidateType(type)) {
                            MethodInfo method = type.GetMethod("Uninstall", BindingFlags.Public | BindingFlags.Static);
                            if(method != null) {
                                method.Invoke(null, null);
                            }
                        }
                    }
                    catch {
                    }
                }
            }
            catch(Exception exception) {
                QTUtility2.MakeErrorLog(exception, "failed uninstall type");
            }
        }

        private static bool ValidateType(Type t) {
            return (((t.IsClass && t.IsPublic) && !t.IsAbstract) && (t.GetInterface(TYPENAME_PLUGINCLIENT) != null));
        }

        public List<PluginInformation> PluginInformations {
            get {
                return new List<PluginInformation>(dicPluginInformations.Values);
            }
        }

        public bool PluginInfosExist {
            get {
                return (dicPluginInformations.Count > 0);
            }
        }
    }
}
