using System;

namespace Saxon.Api.Support {
    public class ResourceWrapper : IResource {

        private readonly Saxon.Hej.lib.Resource resource;
        public ResourceWrapper(Saxon.Hej.lib.Resource resource) {
            this.resource = resource;
        }

        public Uri ResourceUri => new Uri(resource.getResourceURI());

        public XdmItem GetXdmItem(DynamicContext context) {
            return XdmItem.WrapItem(resource.getItem(context.context));
        }

        public string ContentType => resource.getContentType();
        
    }
}

// Copyright (c) 2021 Saxonica Limited