using System.IO;
using System.Text;

namespace Saxon.Ejava.io {
    public class BufferedWriter : TextWriter {
        
        // TODO: make this do some actual buffering...
        
        private TextWriter output;
        public BufferedWriter(TextWriter output) {
            this.output = output;
        }

        public override void Write(string str) {
            output.Write(str);
        }

        public override void Write(char ch) {
            output.Write(ch);
        }

        public override void Write(char[] chars) {
            output.Write(chars);
        }

        public override void Write(char[] chars, int start, int length) {
            output.Write(chars, start, length);
        }

        public override void Flush() {
            output.Flush();
        }

        public override void Close() {
            output.Close();
        }

        public override Encoding Encoding => output.Encoding;
    }
}