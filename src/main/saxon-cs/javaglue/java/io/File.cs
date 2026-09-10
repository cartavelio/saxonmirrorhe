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
        
        public File(string dir, string name) {
            throw new NotImplementedException();
        }

        public static string separator = "/";

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
            return this.fileName;
        }
        public string getCanonicalPath() {
            return this.fileName;
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
            return new File(Path.GetDirectoryName(fileName));
        }
        
        public string getParent() {
            return Path.GetDirectoryName(fileName);
        }

        public bool isDirectory() {
            return SysIOFile.GetAttributes(Path.GetFullPath(fileName)).HasFlag(FileAttributes.Directory);
        }
        
        public bool isFile() {
            return !isDirectory();
        }
        
        public bool isAbsolute() {
            return Path.IsPathRooted(fileName);
        }

        public File getCanonicalFile() {
            throw new NotImplementedException();
        }

        public bool isHidden() {
            return SysIOFile.GetAttributes(Path.GetFullPath(fileName)).HasFlag(FileAttributes.Hidden);
        }

        public bool delete() {
            throw new NotImplementedException();
        }
        
        public void deleteOnExit() {
            throw new NotImplementedException();
        }

        public void createNewFile() {
            Stream stream = SysIOFile.Create(fileName); // TODO: use the returned stream...
            stream.Close();
        }
        
        public void mkdir() {
            throw new NotImplementedException();
        }

        public bool mkdirs() {
            Directory.CreateDirectory(fileName);
            return true;
        }

        public static File createTempFile(string prefix, string suffix) {
            throw new NotImplementedException();
        }
        
        public static File createTempFile(string prefix, string suffix, File dir) {
            throw new NotImplementedException();
        }

        public bool renameTo(File file) {
            return false;
        }

        public string[] list() {
            return Directory.GetFiles(fileName);
        }
        
        public File[] listFiles() {
            throw new NotImplementedException();
        }

        public File[] listFiles(FilenameFilter filter) {
            throw new NotImplementedException();
        }

        public System.IO.FileStream makeAppendOutputStream() {
            return new(fileName, FileMode.Append, FileAccess.Write);
        }
    }
}