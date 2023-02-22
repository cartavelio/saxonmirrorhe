using SysIO = System.IO;
namespace java.io {
    public class StringWriter : Writer {
        private SysIO.StringWriter _sw;

        public StringWriter() {
            this._sw = new SysIO.StringWriter();
        }

        public void write(string str) {
            _sw.Write(str);
        }

        public void write(char ch) {
            _sw.Write(ch);
        }

        public string ToString() {
            return this._sw.ToString();
        }
    }
}