using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface OutputURIResolver {
        Saxon.Hej.lib.OutputURIResolver newInstance();
        Saxon.Ejavax.xml.transform.Result resolve(string href, string @base);
        void close(Saxon.Ejavax.xml.transform.Result result);
    
    }
}