using System;
using net.sf.saxon.expr;
using Saxon.Api.Support;

namespace Saxon.Api {
    internal class CollectionFinderWrapper
        //: net.sf.saxon.lib.CollectionFinder
    {

        ICollectionFinder collectionFinder;

        public CollectionFinderWrapper(ICollectionFinder cf) {
            collectionFinder = cf;
        }

        public net.sf.saxon.lib.ResourceCollection findCollection(XPathContext xpc, string str)
        {
            throw new NotImplementedException();
            // IResourceCollection rcollection = collectionFinder.FindCollection(new DynamicContext(xpc), str);
            // return new ResourceCollectionWrapper(rcollection);
        }
    }
}