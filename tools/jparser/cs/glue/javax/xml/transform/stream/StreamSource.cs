using java.io;

namespace javax.xml.transform.stream {
    public class StreamSource : javax.xml.transform.Source {

        private string systemId;

        public StreamSource() {
        }

        public StreamSource(java.io.File file) {}

        public StreamSource(string uri) {
        }
        public StreamSource(Reader reader) {
            
        }
        public StreamSource(Reader reader, string sysId) {
            
        }
        public StreamSource(InputStream stream) {
            
        }
        public StreamSource(InputStream stream, string sysId) {
            
        }

        public string getSystemId() {
            return systemId;
        }

        public void setSystemId(string uri) {
            this.systemId = uri;
        }

        public void setInputStream(InputStream stream) {
            
        }
        public InputStream getInputStream() {
            return null;
        }

        public void setReader(Reader reader) {
            
        }

        public Reader getReader() {
            return null;
        }
        
    }
}