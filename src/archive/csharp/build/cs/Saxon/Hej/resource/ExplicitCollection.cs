using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.resource {
    public class ExplicitCollection : Saxon.Hej.resource.AbstractResourceCollection {
        private System.Collections.Generic.IList<Saxon.Hej.lib.Resource> resources;
        private Saxon.Hej.om.SpaceStrippingRule whitespaceRules;
        public ExplicitCollection(Saxon.Hej.Configuration config, string collectionURI, System.Collections.Generic.IList<Saxon.Hej.lib.Resource> resources) : base (config) {
            if ((collectionURI == null)) {
                throw new Saxon.Ejava.lang.NullPointerException();
            }
            this.resources = resources;
            this.collectionURI = collectionURI;
        }
        public override bool stripWhitespace(Saxon.Hej.om.SpaceStrippingRule rules) {
            this.whitespaceRules = rules;
            return false;
        }
        public override System.Collections.Generic.IEnumerator<string> getResourceURIs(Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Impl.Overrides.MappingEnumerator<Saxon.Hej.lib.Resource,string>(resources.GetEnumerator(), r => r.getResourceURI());
        }
        public override System.Collections.Generic.IEnumerator<Saxon.Hej.lib.Resource> getResources(Saxon.Hej.expr.XPathContext context) {
            return resources.GetEnumerator();
        }
    
    }
}