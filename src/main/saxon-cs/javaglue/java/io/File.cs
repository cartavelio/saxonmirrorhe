using System;
using System.IO;
using Microsoft.VisualBasic;
using SysIOFile = System.IO.File;

namespace java.io {
    public class File {
        private readonly string fileName;
        public File(string name) {
            this.fileName = name;
        }

        public File(System.Uri uri) {
            if (!uri.IsFile) {
                throw new ArgumentException("URI " + uri + " is not a file URI");
            }
            fileName = uri.LocalPath;
        }
        
        public File(File dir, string name) {
            throw new NotImplementedException(); 
            // transpiler should convert to call on static factory method Resolve
        }

        public static File Resolve(File dir, string localName) {
            return new File(Path.GetFullPath(Path.Combine(dir.fileName, localName)));
        }

        public System.Uri toURI() {
            string abs = Path.GetFullPath(fileName);
            return new Uri(abs); 
        }

        public File getAbsoluteFile() {
            throw new NotImplementedException();
        }

        public string getName() {
            return fileName;
        }

        public string getPath() {
            return fileName;
        }

        public string getAbsolutePath() {
            return null;
        }
        public string getCanonicalPath() {
            return null;
        }

        public bool exists() {
            return SysIOFile.Exists(this.fileName);
        }

        public bool canRead() {
            return SysIOFile.GetAttributes(fileName).HasFlag(FileAttributes.Normal);
        }

        public bool canWrite() {
            return !SysIOFile.GetAttributes(fileName).HasFlag(FileAttributes.ReadOnly);
        }

        public bool canExecute() {
            return false;
        }

        public long lastModified() {
            return new FileInfo(fileName).LastWriteTime.ToFileTimeUtc();
        }

        public long length() {
            return new FileInfo(fileName).Length;
        }

        public File getParentFile() {
            return null;
        }

        public bool isDirectory() {
            return SysIOFile.GetAttributes(Path.GetFullPath(fileName)).HasFlag(FileAttributes.Directory);
        }

        public bool isHidden() {
            return SysIOFile.GetAttributes(Path.GetFullPath(fileName)).HasFlag(FileAttributes.Hidden);
        }

        public void createNewFile() {}

        public void mkdirs() {
        }

        public bool renameTo(File file) {
            return false;
        }

        public string[] list() {
            return Directory.GetFiles(fileName);
        }

        public File[] listFiles(FilenameFilter filter) {
            return new File[0];
        }
    }
}