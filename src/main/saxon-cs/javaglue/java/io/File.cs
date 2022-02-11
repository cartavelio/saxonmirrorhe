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

        public string getPath() {
            return null;
        }

        public string getAbsolutePath() {
            return null;
        }
        public string getCanonicalPath() {
            return null;
        }

        public bool exists() {
            return false;
        }

        public bool canRead() {
            return true;
        }

        public bool canWrite() {
            return true;
        }

        public bool canExecute() {
            return false;
        }

        public long lastModified() {
            return 0;
        }

        public long length() {
            return 0;
        }

        public File getParentFile() {
            return null;
        }

        public bool isDirectory() {
            return false;
        }

        public bool isHidden() {
            return false;
        }

        public bool enameTo(File newName) {
            throw new NotImplementedException();
        }
        
        public void createNewFile() {}

        public void mkdirs() {
        }

        public bool renameTo(File file) {
            return false;
        }

        public string[] list() {
            return new string[0];
        }

        public File[] listFiles(FilenameFilter filter) {
            return new File[0];
        }
    }
}