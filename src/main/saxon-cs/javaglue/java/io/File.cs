using System;
using System.Text;
using SysUri = System.Uri;
using SysIO = System.IO;
using Microsoft.VisualBasic;
using SysIOFile = System.IO.File;
using System = java.lang.System;

namespace java.io {
    public class File {
        private readonly string fileName;
        public File(string name) {
            this.fileName = name;
        }

        public File(SysUri uri) {
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
            return new File(SysIO.Path.GetFullPath(SysIO.Path.Combine(dir.fileName, localName)));
        }

        public SysUri toURI() {
            string abs = SysIO.Path.GetFullPath(fileName);
            return new Uri(abs); 
        }
        
        public SysUri toUri() { // method on java.nio.Path
            return toURI();
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
            return SysIOFile.GetAttributes(fileName).HasFlag(SysIO.FileAttributes.Normal);
        }

        public bool canWrite() {
            return !SysIOFile.GetAttributes(fileName).HasFlag(SysIO.FileAttributes.ReadOnly);
        }

        public bool canExecute() {
            return false;
        }

        public long lastModified() {
            return new SysIO.FileInfo(fileName).LastWriteTime.ToFileTimeUtc();
        }

        public long length() {
            return new SysIO.FileInfo(fileName).Length;
        }

        public File getParentFile() {
            return new File(SysIO.Path.GetDirectoryName(fileName));
        }
        
        public string getParent() {
            return SysIO.Path.GetDirectoryName(fileName);
        }

        public bool isDirectory() {
            return SysIOFile.GetAttributes(SysIO.Path.GetFullPath(fileName)).HasFlag(SysIO.FileAttributes.Directory);
        }
        
        public bool isFile() {
            return !isDirectory();
        }
        
        public bool isAbsolute() {
            return SysIO.Path.IsPathRooted(fileName);
        }

        public File getCanonicalFile() {
            throw new NotImplementedException();
        }

        public bool isHidden() {
            return SysIOFile.GetAttributes(SysIO.Path.GetFullPath(fileName)).HasFlag(SysIO.FileAttributes.Hidden);
        }

        public bool delete() {
            throw new NotImplementedException();
        }
        
        public void deleteOnExit() {
            throw new NotImplementedException();
        }

        public void createNewFile() {
            SysIO.Stream stream = SysIOFile.Create(fileName); // TODO: use the returned stream...
            stream.Close();
        }
        
        public void mkdir() {
            throw new NotImplementedException();
        }

        public bool mkdirs() {
            SysIO.Directory.CreateDirectory(fileName);
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
            return SysIO.Directory.GetFiles(fileName);
        }
        
        public File[] listFiles() {
            throw new NotImplementedException();
        }

        public File[] listFiles(FilenameFilter filter) {
            throw new NotImplementedException();
        }

        public SysIO.FileStream makeAppendOutputStream() {
            return new(fileName, SysIO.FileMode.Append, SysIO.FileAccess.Write);
        }

        public SysIO.TextReader getInputStreamReader(string encoding) {
            SysIO.Stream stream = SysIO.File.OpenRead(fileName);
            return new SysIO.StreamReader(stream, Encoding.GetEncoding(encoding));
        }

        public java.io.File toPath() {
            // Probably not good enough in the long term, but for now we can ignore the
            // distinction between java.io.File and java.nio.Path.
            return this;
        }
    }
}