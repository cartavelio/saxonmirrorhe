using System;

namespace Saxon.Impl.Helpers {
    public class LineNumberReader : System.IO.TextReader {
        private readonly System.IO.TextReader reader;
        private int b;
        private int line;

        public LineNumberReader(System.IO.TextReader reader) {
            this.reader = reader;
        }

        public int getLineNumber() {
            return this.line;
        }

        public override int Peek() {
            return this.reader.Peek();
        }

        public override int Read() {
            try {
                int b = this.reader.Read();
                if ((this.b == '\n') || (this.b == '\r' && b != '\n')) {
                    this.line++;
                }

                return this.b = b;
            }
            catch (System.IO.IOException) {
                throw;
            }
            catch (Exception e) {
                throw new System.IO.IOException(e.Message);
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                this.reader.Dispose();
            }
        }
    }
}

// Copyright (c) 2021 Saxonica Limited