namespace javax.xml.transform.stream {
    public class StreamResult : Result {
        private System.IO.Stream _stream;
        private System.IO.TextWriter _writer;
        private string sysId;
        
        public StreamResult() {
            // TODO
        }
        public StreamResult(System.IO.Stream stream) {
            this._stream = stream;
        }
        public StreamResult(System.IO.TextWriter writer) {
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

        public void setOutputStream(System.IO.Stream stream) {
            this._stream = stream;
        }

        public void setWriter(System.IO.TextWriter writer) {
            this._writer = writer;
        }

        public System.IO.TextWriter getWriter() {
            return this._writer;
        }

        public System.IO.Stream getOutputStream() {
            return _stream;
        }
    }
}