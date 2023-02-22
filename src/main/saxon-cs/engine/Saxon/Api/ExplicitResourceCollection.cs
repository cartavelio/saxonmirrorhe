using System.Collections.Generic;
using net.sf.saxon.resource;
using Saxon.Impl.Overrides;

namespace Saxon.Api {
    /// <summary>
    /// A simple implementation of <c>IResourceCollection<c> that implements a resource
    /// collection as a list of <c>IResource</c> objects.
    /// </summary>
    public class ExplicitResourceCollection : IResourceCollection {
        private readonly List<IResource> resources;

        /// <summary>
        /// Construct a resource collection from a list of resources.
        /// </summary>
        /// <param name="collectionUri">The collection URI</param>
        /// <param name="resources">The list of resources</param>
        public ExplicitResourceCollection(string collectionUri, List<IResource> resources) {
            this.CollectionURI = collectionUri; 
            this.resources = resources;
        }

        public string CollectionURI { get; }

        public IEnumerator<string> GetResourceURIs(DynamicContext context) {
            return new MappingEnumerator<IResource, string>(resources.GetEnumerator(), r => r.ResourceURI);
        }

        public IEnumerator<IResource> GetResources(DynamicContext context) {
            return resources.GetEnumerator();
        }
    }
}