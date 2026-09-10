using System;
using SysUri = System.Uri;

namespace java.net {
    
    /// <summary>
    /// A partial emulation of the java.net.URI class, implemented as a wrapper around a .NET System.Uri
    /// </summary>
    public class URL {

        private readonly SysUri uri;
        
        public URL(string str) {
            uri = new SysUri(str);
        }
        
        public URL(URL baseURL, string href) {
            uri = new SysUri(baseURL.uri, href);
        }
        
        public string getPath() {
            return uri.AbsolutePath;
        }

        public string getProtocol() {
            return uri.Scheme;
        }
        
        public System.Uri toURI() {
            return uri;
        }

        
    }
}