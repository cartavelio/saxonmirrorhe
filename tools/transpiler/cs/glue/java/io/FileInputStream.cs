using System;

namespace java.io {
    public class FileInputStream : InputStream, System.IDisposable {
        public FileInputStream(string fileName) {
            
        }
        public FileInputStream(File file) {
            
        }

        public void Dispose() {
            throw new NotImplementedException();
        }
    }
}