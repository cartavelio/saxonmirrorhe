using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class ChainedResourceResolver : Saxon.Hej.lib.JJ_ResourceResolver {
        private readonly Saxon.Hej.lib.JJ_ResourceResolver first;
        private readonly Saxon.Hej.lib.JJ_ResourceResolver second;
        public ChainedResourceResolver(Saxon.Hej.lib.JJ_ResourceResolver first, Saxon.Hej.lib.JJ_ResourceResolver second)  {
            Saxon.Ejava.util.Objects.requireNonNull(first);
            Saxon.Ejava.util.Objects.requireNonNull(second);
            this.first = first;
            this.second = second;
        }
        public Saxon.Ejavax.xml.transform.Source resolve(Saxon.Hej.lib.JJ_ResourceRequest request) {
            Saxon.Ejavax.xml.transform.Source @is = first.resolve(request);
            if ((@is == null)) {
                @is = second.resolve(request);
            }
            return @is;
        }
    
    }
}