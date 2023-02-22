using System;
using System.Collections.Generic;
using System = java.lang.System;
using SysUri = System.Uri;
/**
 * A C# emulation of java.net.URI
 */
 
namespace java.net {
    public class URI {
        protected SysUri uri;
        public URI(string s) {
            uri = new SysUri(s);
        }
        
        public URI(SysUri u) {
            uri = u;
        }

        public URL toURL() {
            throw new NotImplementedException();
        }

        public static URI create(string s) {
            return new URI(s);
        }

        public bool isAbsolute() {
            return uri.IsAbsoluteUri;
        }

        public URI resolve(URI relative) {
            return new URI(new SysUri(this.uri, relative.uri));
        }
        public URI resolve(string relative) {
            return new URI(new SysUri(this.uri, relative));
        }

        public string toASCIIString() {
            return uri.OriginalString; // TODO - probably not correct
        }
        public string getScheme() {
            throw new NotImplementedException();
        }

        public string getPath() {
            throw new NotImplementedException();
        }

        public string getRawQuery() {
            throw new NotImplementedException();
        }

        public string getSchemeSpecificPart() {
            throw new NotImplementedException();
        }

        public string getQuery() {
            throw new NotImplementedException();
        }

        public string getFragment() {
            throw new NotImplementedException();
        }
        //public java.net.URI(java.net.URI base, string href) {
        //    uri = System.Uri(new System.Uri(base.ToString(), href));
        //}
    }

}