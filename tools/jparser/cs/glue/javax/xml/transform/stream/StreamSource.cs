using java.io;

namespace javax.xml.transform.stream {
    public class StreamSource : javax.xml.transform.Source {

        private string systemId;
        public StreamSource(java.io.File file) {}

        public StreamSource(string uri) {
        }

        public StreamSource(InputStream stream, string sysId) {
            
        }

        public string getSystemId() {
            return systemId;
        }

        public void setSystemId(string uri) {
            this.systemId = uri;
        }

        public InputStream getInputStream() {
            return null;
        }

        public Reader getReader() {
            return null;
        }
        
    }
}