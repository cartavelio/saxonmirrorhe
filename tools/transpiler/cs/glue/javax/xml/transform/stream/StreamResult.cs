namespace javax.xml.transform.stream {
    public class StreamResult : Result {
        private java.io.OutputStream _stream;
        private java.io.Writer _writer;
        private string sysId;
        
        public StreamResult() {
            // TODO
        }
        public StreamResult(java.io.OutputStream stream) {
            this._stream = stream;
        }
        public StreamResult(java.io.Writer writer) {
            this._writer = writer;
        }

        public StreamResult(java.io.File file) {
            
        }
        public string getSystemId() {
            return sysId;
        }

        public void setSystemId(string systemId) {
            this.sysId = systemId;
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