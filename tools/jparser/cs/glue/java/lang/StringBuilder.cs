using System;
using net.sf.saxon.trans.packages;
using SysText = System.Text;

namespace java.lang {
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

        public StringBuilder append(char ch) {
            this._builder.Append(("" + ch));
            return this;
        }

        public StringBuilder appendCodePoint(int cp) {
            this._builder.Append(Char.ConvertFromUtf32(cp));
            return this;
        }

        public int length() {
            return this._builder.Length;
        }

        public string toString() {
            return _builder.ToString();
        }

        public void setLength(int len) {
            _builder.Length = len;
        }
    }
}