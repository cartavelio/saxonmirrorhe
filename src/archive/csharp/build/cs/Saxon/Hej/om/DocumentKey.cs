using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class DocumentKey {
        public readonly static bool CASE_BLIND_FILES = new Saxon.Ejava.io.File("a").Equals(new Saxon.Ejava.io.File("A"));
        private string displayValue;
        private string normalizedValue;
        private string packageName = "";
        private Saxon.Hej.style.PackageVersion packageVersion = Saxon.Hej.style.PackageVersion.ONE;
        public DocumentKey(string uri)  {
            Saxon.Ejava.util.Objects.requireNonNull(uri);
            this.displayValue = uri;
            this.normalizedValue = normalizeURI(uri);
        }
        public DocumentKey(string uri, string packageName, Saxon.Hej.style.PackageVersion version)  {
            Saxon.Ejava.util.Objects.requireNonNull(uri);
            this.displayValue = uri;
            this.normalizedValue = normalizeURI(uri);
            this.packageName = ((packageName == null) ? "" : packageName);
            this.packageVersion = version;
        }
        public string getAbsoluteURI() {
            return displayValue;
        }
        public override string ToString() {
            return displayValue;
        }
        public override bool Equals(System.Object obj) {
            return ((((obj is Saxon.Hej.om.DocumentKey) && normalizedValue.Equals(((Saxon.Hej.om.DocumentKey)(obj)).normalizedValue)) && packageName.Equals(((Saxon.Hej.om.DocumentKey)(obj)).packageName)) && packageVersion.Equals(((Saxon.Hej.om.DocumentKey)(obj)).packageVersion));
        }
        public override int GetHashCode() {
            return normalizedValue.GetHashCode();
        }
        public static string normalizeURI(string uri) {
            if ((uri == null)) {
                return null;
            }
            if (uri.StartsWith("FILE:")) {
                uri = ("file:"+uri.Substring(5));
            }
            if (uri.StartsWith("file:")) {
                if (uri.StartsWith("file:///")) {
                    uri = ("file:/"+uri.Substring(8));
                }
                if (uri.StartsWith("file:/")) {
                    try {
                        string cpath = new Saxon.Ejava.io.File(uri.Substring(5)).getCanonicalPath();
                        uri = ("file:"+cpath);
                    } catch (System.IO.IOException ioe) {
                    }
                }
                if (CASE_BLIND_FILES) {
                    uri = uri.ToLower();
                }
            }
            return uri;
        }
    
    }
}