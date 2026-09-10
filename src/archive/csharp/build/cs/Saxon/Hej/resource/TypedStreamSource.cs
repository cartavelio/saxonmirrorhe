using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class TypedStreamSource : Saxon.Ejavax.xml.transform.stream.StreamSource {
        private string contentType;
        public void setContentType(string contentType) {
            this.contentType = contentType;
        }
        public string getContentType() {
            return contentType;
        }
    
    }
}