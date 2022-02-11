using Saxon.Hej.expr;
using Saxon.Hej.om;

namespace Saxon.Api.Support {
    
    /// <summary>
    /// Converts an <c>Saxon.Api.IResource</c> (the external Saxon-CS representation) to an <c>Saxon.Hej.lib.Resource</c>
    /// (the internal representation).
    /// </summary>
    public class ResourceUnwrapper : Saxon.Hej.lib.Resource {

        private readonly IResource resource;
        private ResourceUnwrapper(IResource resource) {
            this.resource = resource;
        }

        public static Saxon.Hej.lib.Resource Unwrap(IResource resource) {
            return new ResourceUnwrapper(resource);
        }

        public string getResourceURI() {
            return resource.ResourceUri.ToString();
        }

        public Item getItem() {
            return (Item)resource.GetXdmItem().value;
        }

        public string getContentType() {
            return resource.ContentType;
        }
    }
}

// Copyright (c) 2021 Saxonica Limited