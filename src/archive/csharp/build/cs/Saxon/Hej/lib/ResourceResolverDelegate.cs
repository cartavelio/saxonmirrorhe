using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public class ResourceResolverDelegate : Saxon.Hej.lib.JJ_ResourceResolver {
        private readonly System.Func<Saxon.Hej.lib.JJ_ResourceRequest,Saxon.Ejavax.xml.transform.Source> lambda;
        public ResourceResolverDelegate(System.Func<Saxon.Hej.lib.JJ_ResourceRequest,Saxon.Ejavax.xml.transform.Source> lambda)  {
            this.lambda = lambda;
        }
        public Saxon.Ejavax.xml.transform.Source resolve(Saxon.Hej.lib.JJ_ResourceRequest request) {
            try {
                return lambda(request);
            } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                throw err.getXPathException();
            }
        }
    
    }
}