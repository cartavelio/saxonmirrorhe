using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface UnparsedTextURIResolver {
        System.IO.TextReader resolve(System.Uri absoluteURI, string encoding, Saxon.Hej.Configuration config);
    
    }
}