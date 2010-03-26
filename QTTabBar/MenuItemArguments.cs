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
    using System;

    internal sealed class MenuItemArguments {
        public string Argument;
        public MenuGenre Genre;
        public int Index;
        public bool IsBack;
        public int KeyShortcut;
        public string OriginalArgument;
        public string OriginalWorkingDirectory;
        public string Path;
        public MenuTarget Target;
        public bool TokenReplaced;
        public string WorkingDirectory;

        public MenuItemArguments(string path, MenuTarget target, MenuGenre genre) {
            this.Path = path;
            this.Genre = genre;
            this.Target = target;
        }

        public MenuItemArguments(string path, bool fback, int index, MenuGenre genre) {
            this.Path = path;
            this.IsBack = fback;
            this.Index = index;
            this.Genre = genre;
        }

        public MenuItemArguments(string path, string arg, string work, int keyShortcut, MenuGenre genre) {
            this.Path = path;
            this.Argument = this.OriginalArgument = arg;
            this.WorkingDirectory = this.OriginalWorkingDirectory = work;
            this.KeyShortcut = keyShortcut;
            this.Genre = genre;
        }

        public void RestoreOriginalArgs() {
            this.Argument = this.OriginalArgument;
            this.WorkingDirectory = this.OriginalWorkingDirectory;
        }
    }
}
