using System;
using System.IO;

namespace java.io {
    public class PrintStream : OutputStream {
        private System.IO.Stream outputStream;

        public PrintStream(java.io.File file) {
            throw new NotImplementedException();
        }
        public PrintStream(Stream outputStream) {
            this.outputStream = outputStream;
        }
        public PrintStream(String fileName) {
            outputStream = new FileStream(fileName, System.IO.FileMode.OpenOrCreate);
        }

        public void Println(string str) {
            Console.Write(str);
        }

    }
}