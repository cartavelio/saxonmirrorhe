using System;

namespace java.io {
    public class File {
        public File(string name) {
            
        }

        public File(System.Uri uri) {
            
        }
        
        public File(File dir, string name) {
            
        }

        public System.Uri toURI() {
            return null;
        }

        public File getAbsoluteFile() {
            throw new NotImplementedException();
        }

        public string getName() {
            return null;
        }
        public string getCanonicalPath() {
            return null;
        }

        public bool exists() {
            return false;
        }

        public File getParentFile() {
            return null;
        }

        public bool isDirectory() {
            return false;
        }

        public bool enameTo(File newName) {
            throw new NotImplementedException();
        }
        
        public void createNewFile() {}

        public bool renameTo(File file) {
            return false;
        }
    }
}