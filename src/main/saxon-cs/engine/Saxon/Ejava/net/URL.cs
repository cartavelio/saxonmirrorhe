using System;
using System.Net;
using SysUri = System.Uri;

namespace Saxon.Ejava.net {
    
    /// <summary>
    /// A partial emulation of the java.net.URL class, implemented as a wrapper around a .NET System.Uri
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

        public System.IO.Stream openStream() {
            return new WebClient().OpenRead(uri);
        } 

        
    }
}