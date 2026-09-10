using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pull {
    public class UnparsedEntity {
        private string name;
        private string systemId;
        private string publicId;
        private string baseURI;
        public string getName() {
            return name;
        }
        public void setName(string name) {
            this.name = name;
        }
        public string getSystemId() {
            return systemId;
        }
        public void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public string getPublicId() {
            return publicId;
        }
        public void setPublicId(string publicId) {
            this.publicId = publicId;
        }
        public string getBaseURI() {
            return baseURI;
        }
        public void setBaseURI(string baseURI) {
            this.baseURI = baseURI;
        }
    
    }
}