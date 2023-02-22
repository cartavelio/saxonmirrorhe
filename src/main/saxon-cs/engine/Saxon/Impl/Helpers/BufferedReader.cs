using System.IO;

namespace Saxon.Impl.Helpers {
    public class BufferedReader : System.IO.TextReader {
        
        private readonly System.IO.TextReader reader;
        private int b;
        private int line;

        public BufferedReader(System.IO.TextReader reader)
        {
            this.reader = reader;
        }
        
        public BufferedReader(System.IO.Stream inputStream)
        {
            if (inputStream is BufferedStream) {
                this.reader = new StreamReader(inputStream);
            } else {
                this.reader = new StreamReader(new BufferedStream(inputStream));
            }
        }

        public override int Peek()
        {
            return this.reader.Peek();
        }

        public override int Read()
        {
            int b = this.reader.Read();
            if ((this.b == '\n') || (this.b == '\r' && b != '\n')) {
                this.line++;
            }
            return this.b = b;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) {
                this.reader.Dispose();
            }
        }
    
    }
}