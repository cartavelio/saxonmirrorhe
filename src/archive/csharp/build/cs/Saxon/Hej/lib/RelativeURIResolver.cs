using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface RelativeURIResolver : Saxon.Ejavax.xml.transform.URIResolver {
        string makeAbsolute(string href, string @base);
        Saxon.Ejavax.xml.transform.Source dereference(string uri);
    
    }
}