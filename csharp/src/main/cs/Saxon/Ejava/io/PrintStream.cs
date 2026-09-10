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
        public PrintStream(String fileName) {
            outputStream = new FileStream(fileName, System.IO.FileMode.Create);
        }

        public void Println(string str) {
            Console.Write(str);
        }

        public void flush() {
            throw new NotImplementedException();
        }

        public void close() {
            throw new NotImplementedException();
        }
    }
}