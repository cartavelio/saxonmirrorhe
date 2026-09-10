using System;
using SysCollGen = System.Collections.Generic;
using System.Text;
using Saxon.Hej.functions;
using SysUri = System.Uri;
using SysIO = System.IO;
using SysIOFile = System.IO.File;
using SysIODirectory = System.IO.Directory;
using SysIOPath = System.IO.Path;
using JLSystem = Saxon.Ejava.lang.System;

namespace Saxon.Ejava.io {
    public class File {
        private readonly string fileName;
        private readonly string fullPath;
        public File(string name) {
            if (name.EndsWith(separator)) {
                name = name.Substring(0, name.Length - separator.Length);
            }
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
        
        public File(string dir, string name) : this(new File(dir), name) {
        }

        public static string separator = "/";

        public static File Resolve(File dir, string localName) {
            return new File(SysIO.Path.GetFullPath(SysIO.Path.Combine(dir.fileName, localName)));
        }

        public SysUri toURI() {
            // This is horrible, but the simple "new Uri(fullPath)" generates a Uri whose OriginalString property
            // is say "/folder/file.xml" rather than "file:///folder/file.xml". Really, we shouldn't be generating
            // calls on OriginalString in the transpiler, but changing that just before release is a bit scary.
            return new Uri(new Uri(fullPath).AbsoluteUri);
            //return new Uri(fullPath); 
        }
        
        public SysUri toUri() { // method on Saxon.Ejava.nio.Path
            return toURI();
        }

        public File getAbsoluteFile() {
            throw new NotImplementedException();
        }

        public string getName() {
            if (isDirectory()) {
                return new SysIO.DirectoryInfo(fileName).Name;
            }
            else {
                return new SysIO.FileInfo(fileName).Name;
            }
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

        /**
         * @return A long value representing the time the file was last modified, measured in milliseconds since the epoch (00:00:00 GMT, January 1, 1970),
         * or 0L if the file does not exist or if an I/O error occurs
         */
        public long lastModified() {
            if (!exists()) {
                return 0L;
            }

            DateTime dt = new SysIO.FileInfo(fileName).LastWriteTime;
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = dt.ToUniversalTime() - origin;
            return (long) diff.TotalMilliseconds;
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
            return exists() && SysIOFile.GetAttributes(SysIO.Path.GetFullPath(fileName)).HasFlag(SysIO.FileAttributes.Directory);
        }
        
        public bool isFile() {
            return exists() && !isDirectory();
        }
        
        public bool isAbsolute() {
            return SysIO.Path.IsPathRooted(fileName);
        }

        public File getCanonicalFile() {
            return new File(getCanonicalPath());
        }

        public bool isHidden() {
            return SysIOFile.GetAttributes(SysIO.Path.GetFullPath(fileName)).HasFlag(SysIO.FileAttributes.Hidden);
        }

        public bool delete() {
            try {
                if (isDirectory()) {
                    SysIODirectory.Delete(fullPath);
                }
                else {
                    SysIOFile.SetAttributes(fullPath, SysIO.FileAttributes.Normal);
                    SysIOFile.Delete(fullPath);
                }
                

                return true;
            }
            catch (Exception) {
                return false;
            }
        }
        
        public void deleteOnExit() {
            throw new NotImplementedException();
        }

        public void createNewFile() {
            SysIO.Stream stream = SysIOFile.Create(fileName); // TODO: use the returned stream...
            stream.Close();
        }
        
        public void mkdir() {
            mkdirs();
        }

        public bool mkdirs() {
            SysIO.Directory.CreateDirectory(fileName);
            return true;
        }

        public static File createTempFile(string prefix, string suffix) {
            return createTempFile(prefix, suffix, null);
        }
        
        public static File createTempFile(string prefix, string suffix, File dir) {
            if (suffix == null) {
                suffix = ".tmp";
            }

            if (dir == null) {
                dir = new File(JLSystem.getProperty("java.io.tmpdir"));
            }

            String mid = "_" + new Random().Next();
            File file = new File(dir, prefix + mid + suffix);
            file.createNewFile();
            return file;
        }

        public bool renameTo(File file) {
            if (isDirectory() || file.isDirectory()) {
                SysIO.Directory.Move(fullPath, file.fullPath);
            }
            else {
                SysIO.File.Move(fullPath, file.fullPath);
            }

            return true;
        }

        public string[] list() {
            return SysIO.Directory.GetFiles(fileName);
        }
        
        public File[] listFiles() {
            SysCollGen.IList<string> files = new SysCollGen.List<string>();
            foreach (string dir in SysIODirectory.EnumerateDirectories(fullPath)) {
                files.Add(dir);
            }
            foreach (string file in SysIODirectory.EnumerateFiles(fullPath)) {
                files.Add(file);
            }

            File[] result = new File[files.Count];
            int i = 0;
            foreach (string f in files) {
                result[i++] = new File(f);
            }

            return result;
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

        public SysIO.TextReader getInputStreamReader(string encodingName) {
            SysIO.Stream stream = SysIO.File.OpenRead(fileName);
            Encoding encoding = Encoding.GetEncoding(encodingName, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
            return new SysIO.StreamReader(stream, encoding);
        }

        public Saxon.Ejava.io.File toPath() {
            // Probably not good enough in the long term, but for now we can ignore the
            // distinction between Saxon.Ejava.io.File and Saxon.Ejava.nio.Path.
            return this;
        }

        public override string ToString() {
            return fileName;
        }
    }
}