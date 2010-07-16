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

namespace QTTabBarLib {
    internal sealed class StackDictionary<S, T> {
        private IDictionary<S, T> dictionary;
        private IList<S> lstKeys;

        public StackDictionary() {
            this.lstKeys = new List<S>();
            this.dictionary = new Dictionary<S, T>();
        }

        public T Peek() {
            S local;
            return this.popPeekInternal(false, out local);
        }

        public T Peek(out S key) {
            return this.popPeekInternal(false, out key);
        }

        public T Pop() {
            S local;
            return this.popPeekInternal(true, out local);
        }

        public T Pop(out S key) {
            return this.popPeekInternal(true, out key);
        }

        private T popPeekInternal(bool fPop, out S lastKey) {
            if(this.lstKeys.Count == 0) {
                throw new InvalidOperationException("This StackDictionary is empty.");
            }
            lastKey = this.lstKeys[this.lstKeys.Count - 1];
            T local = this.dictionary[lastKey];
            if(fPop) {
                this.lstKeys.RemoveAt(this.lstKeys.Count - 1);
                this.dictionary.Remove(lastKey);
            }
            return local;
        }

        public void Push(S key, T value) {
            this.lstKeys.Remove(key);
            this.lstKeys.Add(key);
            this.dictionary[key] = value;
        }

        public bool Remove(S key) {
            this.lstKeys.Remove(key);
            return this.dictionary.Remove(key);
        }

        public bool TryGetValue(S key, out T value) {
            return this.dictionary.TryGetValue(key, out value);
        }

        public int Count {
            get {
                return this.lstKeys.Count;
            }
        }

        public ICollection<S> Keys {
            get {
                return this.dictionary.Keys;
            }
        }

        public ICollection<T> Values {
            get {
                return this.dictionary.Values;
            }
        }
    }
}
