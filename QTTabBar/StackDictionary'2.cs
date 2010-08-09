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
            lstKeys = new List<S>();
            dictionary = new Dictionary<S, T>();
        }

        public T Peek() {
            S local;
            return popPeekInternal(false, out local);
        }

        public T Peek(out S key) {
            return popPeekInternal(false, out key);
        }

        public T Pop() {
            S local;
            return popPeekInternal(true, out local);
        }

        public T Pop(out S key) {
            return popPeekInternal(true, out key);
        }

        private T popPeekInternal(bool fPop, out S lastKey) {
            if(lstKeys.Count == 0) {
                throw new InvalidOperationException("This StackDictionary is empty.");
            }
            lastKey = lstKeys[lstKeys.Count - 1];
            T local = dictionary[lastKey];
            if(fPop) {
                lstKeys.RemoveAt(lstKeys.Count - 1);
                dictionary.Remove(lastKey);
            }
            return local;
        }

        public void Push(S key, T value) {
            lstKeys.Remove(key);
            lstKeys.Add(key);
            dictionary[key] = value;
        }

        public bool Remove(S key) {
            lstKeys.Remove(key);
            return dictionary.Remove(key);
        }

        public bool TryGetValue(S key, out T value) {
            return dictionary.TryGetValue(key, out value);
        }

        public int Count {
            get {
                return lstKeys.Count;
            }
        }

        public ICollection<S> Keys {
            get {
                return dictionary.Keys;
            }
        }

        public ICollection<T> Values {
            get {
                return dictionary.Values;
            }
        }
    }
}
