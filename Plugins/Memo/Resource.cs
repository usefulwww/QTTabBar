namespace QuizoPlugins {
    using System;
    using System.CodeDom.Compiler;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.CompilerServices;

    [CompilerGenerated, DebuggerNonUserCode, GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
    internal class Resource {
        private static CultureInfo resourceCulture;
        private static System.Resources.ResourceManager resourceMan;

        internal Resource() {
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        internal static System.Resources.ResourceManager ResourceManager {
            get {
                if(object.ReferenceEquals(resourceMan, null)) {
                    System.Resources.ResourceManager manager = new System.Resources.ResourceManager("QuizoPlugins.Resource", typeof(Resource).Assembly);
                    resourceMan = manager;
                }
                return resourceMan;
            }
        }

        internal static string str_en {
            get {
                return ResourceManager.GetString("str_en", resourceCulture);
            }
        }

        internal static string str_ja {
            get {
                return ResourceManager.GetString("str_ja", resourceCulture);
            }
        }
    }
}
