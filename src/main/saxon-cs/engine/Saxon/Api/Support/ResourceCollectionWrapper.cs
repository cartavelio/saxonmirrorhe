namespace Saxon.Api.Support {
    internal class ResourceCollectionWrapper {
       

            private IResourceCollection resourceCollection;

            public ResourceCollectionWrapper(IResourceCollection rc) {
                resourceCollection = rc;
            }

            public string getCollectionURI()
            {
                return resourceCollection.CollectionURI();
            }

            public Iterator getResources(XPathContext xpc)
            {
                java.util.List iter = new java.util.ArrayList();
                List<IResource> list = resourceCollection.GetResources(new DynamicContext(xpc));
                foreach (IResource src in list)
                {
                    iter.add(new ResourceWrapper(src));
                }

                return iter.iterator();

            }

            public Iterator getResourceURIs(XPathContext xpc)
            {
                java.util.List iter = new java.util.ArrayList();
                List<string> list = resourceCollection.GetResourceURIs(new DynamicContext(xpc));
                foreach (string src in list)
                {
                    iter.add(src);
                }

                return iter.iterator();
            }

            public bool isStable(XPathContext xpc)
            {
                return false;
            }

            public bool stripWhitespace(net.sf.saxon.om.SpaceStrippingRule ssr)
            {
                return false;
            }
        }
    }
}