using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class SchemaException : Saxon.Hej.trans.XPathException {
        public SchemaException(string message, Saxon.Hej.s9api.Location locator) : base (message, null, locator) {
        }
        public SchemaException(string message) : base (message) {
        }
        public SchemaException(System.Exception exception) : base (exception) {
        }
        public SchemaException(string message, System.Exception exception) : base (message, exception) {
        }
    
    }
}