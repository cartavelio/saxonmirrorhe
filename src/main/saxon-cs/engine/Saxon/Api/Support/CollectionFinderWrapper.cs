using net.sf.saxon.expr;

namespace Saxon.Api.Support {
    public class CollectionFinderWrapper {
        private readonly ICollectionFinder finder;

        public CollectionFinderWrapper(ICollectionFinder finder) {
            this.finder = finder;
        }

        public net.sf.saxon.lib.ResourceCollection findCollection(XPathContext context, string collectionURI) {
            IResourceCollection rc = finder(new DynamicContext(context), collectionURI);
            return new ResourceCollectionUnwrapper(rc);
        }

        public ICollectionFinder Unwrap() {
            return finder;
        }
    }
}