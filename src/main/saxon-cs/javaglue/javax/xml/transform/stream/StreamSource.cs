using java.io;

namespace javax.xml.transform.stream {
    public class StreamSource : javax.xml.transform.Source {

        private string systemId;

        public StreamSource() {
        }

        public StreamSource(java.io.File file) {}

        public StreamSource(string uri) {
        }
        public StreamSource(System.IO.TextReader reader) {
            
        }
        public StreamSource(System.IO.TextReader reader, string sysId) {
            
        }
        public StreamSource(System.IO.Stream stream) {
            
        }
        public StreamSource(System.IO.Stream stream, string sysId) {
            
        }

        public string getSystemId() {
            return systemId;
        }

        public void setSystemId(string uri) {
            this.systemId = uri;
        }

        public void setInputStream(System.IO.Stream stream) {
            
        }
        public System.IO.Stream getInputStream() {
            return null;
        }

        public void setReader(System.IO.TextReader reader) {
            
        }

        public System.IO.TextReader getReader() {
            return null;
        }
        
    }
}