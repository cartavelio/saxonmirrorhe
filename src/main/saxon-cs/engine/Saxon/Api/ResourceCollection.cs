using System;
using System.Collections.Generic;

namespace Saxon.Api {
    public class ResourceCollection : IResourceCollection
    {
        protected net.sf.saxon.lib.ResourceCollection resourceCollection;

        internal ResourceCollection()
        {
        }


        internal ResourceCollection(net.sf.saxon.lib.ResourceCollection rc) {
            resourceCollection = rc;
        }

        public string CollectionURI()
        {
            return resourceCollection.getCollectionURI();
        }

        public List<string> GetResourceURIs(DynamicContext context)
        {
            throw new NotImplementedException();
            // List<string> list = new List<string>();
            // var jIter = resourceCollection.getResourceURIs(context.context);
            // while(jIter.hasNext()) {
            //
            //     string uri = (string)jIter.next();
            //     list.Add(uri);
            // }
            // return list;
        }

        public List<IResource> GetResources(DynamicContext context)
        {
            throw new NotImplementedException();
            // List<IResource> list = new List<IResource>();
            // var jIter = resourceCollection.getResources(context.context);
            // while (jIter.hasNext())
            // {
            //
            //     net.sf.saxon.lib.Resource jresource = (net.sf.saxon.lib.Resource)jIter.next();
            //     list.Add(new Resource(jresource));
            // }
            // return list;
        }

        public bool IsStable(DynamicContext context)
        {
            return resourceCollection.isStable(context.context);
        }


    }
}