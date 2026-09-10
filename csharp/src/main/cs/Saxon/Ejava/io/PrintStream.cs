using System;
using System.IO;

namespace Saxon.Ejava.io {
    public class PrintStream : OutputStream {
        private System.IO.Stream outputStream;
        private bool autoFlush;

        public PrintStream(Saxon.Ejava.io.File file) {
            throw new NotImplementedException();
        }
        public PrintStream(Stream outputStream) {
            this.outputStream = outputStream;
        }
        public PrintStream(Stream outputStream, bool autoFlush) {
            this.outputStream = outputStream;
            this.autoFlush = autoFlush;
        }
        public PrintStream(string fileName) {
            outputStream = new FileStream(fileName, System.IO.FileMode.Create);
        }

        public System.IO.Stream getOutputStream() {
            return this.outputStream;
        }

        public void Println(string str) {
            var bytes = System.Text.Encoding.UTF8.GetBytes(str + '\n');
            outputStream.Write(bytes, 0, bytes.Length);
        }

        public void flush() {
            outputStream.Flush();
        }

        public void close() {
            outputStream.Close();
        }
    }
}