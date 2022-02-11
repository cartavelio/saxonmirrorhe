using System.IO;
using Saxon.Ejava.io;

namespace Saxon.Ejavax.xml.transform.stream {
    public class StreamSource : Saxon.Ejavax.xml.transform.Source {

        private string systemId;
        private TextReader reader;
        private Stream inputStream;

        public StreamSource() {
        }

        public StreamSource(Saxon.Ejava.io.File file) {
            this.systemId = file.toURI().ToString();
        }

        public StreamSource(string uri) {
            this.systemId = uri;
        }
        public StreamSource(System.IO.TextReader reader) {
            this.reader = reader;
        }
        public StreamSource(System.IO.TextReader reader, string sysId) {
            this.reader = reader;
            this.systemId = sysId;
        }
        public StreamSource(System.IO.Stream stream) {
            this.inputStream = stream;
        }
        public StreamSource(System.IO.Stream stream, string sysId) {
            this.inputStream = stream;
            this.systemId = sysId;
        }

        public string getSystemId() {
            return systemId;
        }

        public void setSystemId(string uri) {
            this.systemId = uri;
        }

        public void setInputStream(System.IO.Stream stream) {
            this.inputStream = stream;
        }
        public System.IO.Stream getInputStream() {
            return this.inputStream;
        }

        public void setReader(System.IO.TextReader reader) {
            this.reader = reader;
        }

        public System.IO.TextReader getReader() {
            return this.reader;
        }
        
    }
}