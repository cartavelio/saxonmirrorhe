using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface CollationURIResolver {
        Saxon.Hej.lib.StringCollator resolve(string collationURI, Saxon.Hej.Configuration config);
    
    }
}