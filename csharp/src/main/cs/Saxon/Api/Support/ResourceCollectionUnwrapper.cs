using System.Linq;
using SysCollGen = System.Collections.Generic;
using Saxon.Hej.expr;
using Saxon.Impl.Overrides;

namespace Saxon.Api.Support {
    internal class ResourceCollectionUnwrapper : Saxon.Hej.lib.ResourceCollection {

        private readonly SysCollGen.IEnumerable<IResource> resources;
        private readonly string collectionUri;

        public ResourceCollectionUnwrapper(string collectionUri, SysCollGen.IEnumerable<IResource> resources) {
            this.resources = resources;
            this.collectionUri = collectionUri;
        }

        public string getCollectionURI() {
            return collectionUri;
        }

        public SysCollGen.IEnumerator<string> getResourceURIs(XPathContext context) {
            return resources.Select(resource => resource.ResourceUri.ToString()).GetEnumerator();
        }

        public SysCollGen.IEnumerator<Hej.lib.Resource> getResources(XPathContext context) {
            return resources.Select(ResourceUnwrapper.Unwrap).GetEnumerator();
        }

        public bool isStable(XPathContext context) {
            return true; // optimistically...
        }
        
        
    }
}

// Copyright (c) 2021 - 2023 Saxonica Limited