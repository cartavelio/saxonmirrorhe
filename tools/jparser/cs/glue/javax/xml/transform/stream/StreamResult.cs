namespace javax.xml.transform.stream {
    public class StreamResult : Result {
        private java.io.OutputStream _stream;
        private java.io.Writer _writer;
        public StreamResult() {
            // TODO
        }
        public StreamResult(java.io.OutputStream stream) {
            this._stream = stream;
        }
        public StreamResult(java.io.Writer writer) {
            this._writer = writer;
        }

        public void setOutputStream(java.io.OutputStream stream) {
            this._stream = stream;
        }

        public void setWriter(java.io.Writer writer) {
            this._writer = writer;
        }

        public java.io.Writer getWriter() {
            return this._writer;
        }

        public java.io.OutputStream getOutputStream() {
            return _stream;
        }
    }
}