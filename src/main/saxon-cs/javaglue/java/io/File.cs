using System;
using SysCollGen = System.Collections.Generic;
using System.Text;
using SysUri = System.Uri;
using SysIO = System.IO;
using Microsoft.VisualBasic;
using SysIOFile = System.IO.File;
using SysIODirectory = System.IO.Directory;
using SysIOPath = System.IO.Path;
using System = java.lang.System;

namespace java.io {
    public class File {
        private readonly string fileName;
        private readonly string fullPath;
        public File(string name) {
            this.fileName = name;
            this.fullPath = SysIO.Path.GetFullPath(name);
        }

        public File(SysUri uri) {
            if (SysUri.IsWellFormedUriString(uri.ToString(), UriKind.Relative)) {
                this.fileName = uri.ToString();
                this.fullPath = SysIO.Path.GetFullPath(this.fileName);
            }
            else {
                if (!uri.IsFile) {
                    throw new ArgumentException("URI " + uri + " is not a file URI");
                }

                this.fileName = uri.LocalPath;
                this.fullPath = SysIO.Path.GetFullPath(this.fileName);
            }
        }
        
        public File(File dir, string name) {
            string combined = SysIOPath.Combine(dir.fullPath, name);
            this.fileName = this.fullPath = combined;
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
            return this.fullPath;
        }

        public string getAbsolutePath() {
            return this.fullPath;
        }
        public string getCanonicalPath() {
            return this.fullPath;
        }

        public bool exists() {
            return SysIOFile.Exists(this.fullPath) || SysIODirectory.Exists(this.fullPath);
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
            SysCollGen.IList<string> files = new SysCollGen.List<string>();
            foreach (string dir in SysIODirectory.EnumerateDirectories(fullPath)) {
                if (filter.accept(this, SysIOPath.GetFileName(dir))) {
                    files.Add(dir);
                }
            }
            foreach (string file in SysIODirectory.EnumerateFiles(fullPath)) {
                if (filter.accept(this, SysIOPath.GetFileName(file))) {
                    files.Add(file);
                }
            }

            File[] result = new File[files.Count];
            int i = 0;
            foreach (string f in files) {
                result[i++] = new File(f);
            }

            return result;
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

        public override string ToString() {
            return fileName;
        }
    }
}