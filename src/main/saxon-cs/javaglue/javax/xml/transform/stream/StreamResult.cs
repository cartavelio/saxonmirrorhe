using System.IO;

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
            if (file.getParent() is null or "") {
                throw new java.io.IOException("Cannot create output file " + file + ": parent directory not known");
            }
            java.io.File dir = new java.io.File(file.getParent());
            if (dir.isFile()) {
                throw new java.io.IOException("Cannot create output file: parent " + dir +
                                              " exists but is not a directory");
            }

            if (!dir.exists()) {
                dir.mkdirs();
            }
            this._stream = new FileStream(file.getPath(), FileMode.Create);
            this.sysId = file.toUri().ToString();
        }

        public StreamResult(string sysId) {
            setSystemId(sysId);
        }
        
        public string getSystemId() {
            return sysId;
        }

        public void setSystemId(string systemId) {
            this.sysId = systemId;
        }
        
        public void setSystemId(java.io.File file) {
            this.sysId = file.toURI().ToString();
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