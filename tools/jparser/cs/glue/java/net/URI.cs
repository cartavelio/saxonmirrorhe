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

        public bool isAbsolute() {
            return uri.IsAbsoluteUri;
        }

        public URI resolve(URI relative) {
            return new URI(new SysUri(this.uri, relative.uri));
        }
        //public java.net.URI(java.net.URI base, string href) {
        //    uri = System.Uri(new System.Uri(base.ToString(), href));
        //}
    }

}