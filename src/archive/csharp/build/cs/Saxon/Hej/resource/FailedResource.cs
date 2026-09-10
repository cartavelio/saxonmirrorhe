using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class FailedResource : Saxon.Hej.lib.Resource {
        private readonly string uri;
        private readonly Saxon.Hej.trans.XPathException error;
        public FailedResource(string uri, Saxon.Hej.trans.XPathException error)  {
            this.uri = uri;
            this.error = error;
        }
        public string getContentType() {
            return null;
        }
        public string getResourceURI() {
            return uri;
        }
        public Saxon.Hej.om.Item getItem() {
            throw error;
        }
        public Saxon.Hej.trans.XPathException getError() {
            return error;
        }
    
    }
}