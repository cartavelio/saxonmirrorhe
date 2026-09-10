using System.IO;

namespace Saxon.Ejavax.xml.transform.stream {
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

        public StreamResult(Saxon.Ejava.io.File file) {
            if (file.getParent() is null or "") {
                throw new Saxon.Ejava.io.IOException("Cannot create output file " + file + ": parent directory not known");
            }
            Saxon.Ejava.io.File dir = new Saxon.Ejava.io.File(file.getParent());
            if (dir.isFile()) {
                throw new Saxon.Ejava.io.IOException("Cannot create output file: parent " + dir +
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
        
        public void setSystemId(Saxon.Ejava.io.File file) {
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