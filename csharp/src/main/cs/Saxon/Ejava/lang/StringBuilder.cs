using System;
using Saxon.Hej.serialize.charcode;
using Saxon.Hej.trans.packages;
using SysText = System.Text;

namespace Saxon.Ejava.lang {
    public class StringBuilder {
        private SysText.StringBuilder _builder;

        public StringBuilder() {
            this._builder = new SysText.StringBuilder();
        }

        public StringBuilder(int capacity) {
            this._builder = new SysText.StringBuilder(capacity);
        }

        public StringBuilder(string str) {
            this._builder = new SysText.StringBuilder(str);
        }

        public StringBuilder append(string str) {
            this._builder.Append(str);
            return this;
        }
        
        public StringBuilder append(StringBuilder str) {
            this._builder.Append(str);
            return this;
        }
        
        public StringBuilder append(string str, int start, int end) {
            this._builder.Append(str, start, end-start);
            return this;
        }
        
        public StringBuilder append(char[] str, int start, int length) {
            this._builder.Append(str, start, length);
            return this;
        }

        public StringBuilder append(char ch) {
            this._builder.Append(("" + ch));
            return this;
        }
        
        public StringBuilder append(int i) {
            this._builder.Append(("" + i));
            return this;
        }
        
        public StringBuilder append(long i) {
            this._builder.Append(("" + i));
            return this;
        }

        public StringBuilder append(object obj) {
            this._builder.Append(obj);
            return this;
        }

        public StringBuilder appendCodePoint(int cp) {
            if (cp >= UTF16CharacterSet.NONBMP_MIN) {
                this._builder.Append(UTF16CharacterSet.highSurrogate(cp));
                this._builder.Append(UTF16CharacterSet.lowSurrogate(cp));
            }
            else {
                this._builder.Append((char) cp);
            }

            return this;
        }

        public int indexOf(string str) {
            return this._builder.ToString().IndexOf(str, StringComparison.InvariantCulture);
        }

        public StringBuilder insert(int index, string str) {
            this._builder.Insert(index, str);
            return this;
        }
        
        public StringBuilder insert(int index, char ch) {
            this._builder.Insert(index, ch);
            return this;
        }
        
        public StringBuilder insert(int index, char[] chars) {
            this._builder.Insert(index, chars);
            return this;
        }

        public StringBuilder setCharAt(int index, char replacement) {
            this._builder[index] = replacement;
            return this;
        }

        public StringBuilder deleteCharAt(int index) {
            this._builder.Remove(index, 1);
            return this;
        }
        public int length() {
            return this._builder.Length;
        }

        public override string ToString() {
            return _builder.ToString();
        }

        public char charAt(int i) {
            return _builder[i];
        }
        
        public string substring(int start) {
            return _builder.ToString().Substring(start);
        }
        
        public string substring(int start, int end) {
            return _builder.ToString().Substring(start, end-start);
        }

        public void setLength(int len) {
            _builder.Length = len;
        }
    }
}