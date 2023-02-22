using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDAnyAttribute : Saxon.Eej.ee.schema.sdoc.AnnotationParent {
        private Saxon.Eej.ee.schema.AttributeWildcard wildcard = null;
        public XSDAnyAttribute()  {
        }
        protected internal override void prepareAttributes() {
            wildcard = new Saxon.Eej.ee.schema.AttributeWildcard(getSchema().getConfiguration());
            wildcard.setLocator(this);
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "namespace", "notNamespace", "notQName", "processContents"};
            allowAttributes(atts, allowed);
            checkMutuallyExclusiveAttributes("namespace", "notNamespace");
            string attValue;
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "namespace");
            if ((attValue != null)) {
                processAllowedNamespaces(attValue, wildcard.getWildcard());
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "notNamespace");
            if ((attValue != null)) {
                processDisallowedNamespaces(attValue, wildcard.getWildcard());
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "notQName");
            if ((attValue != null)) {
                processDisallowedQNames(attValue, wildcard.getWildcard());
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "processContents");
            if ((attValue != null)) {
                if (/*134*/Saxon.Eej.ee.schema.sdoc.XSDAny.isProcessName(attValue)) {
                    wildcard.getWildcard().setProcessContents(attValue);
                } else {
                    invalidAttributeValue("processContents", attValue, null);
                }
            }
            processId();
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            base.validate(compiler);
            if (((getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "notNamespace") != null) && (compiler.getLanguageVersion() != Saxon.Hej.Configuration.XSD11))) {
                error("The notNamespace attribute requires XSD 1.1 to be enabled");
            }
        }
        public Saxon.Eej.ee.schema.AttributeWildcard getWildcard() {
            return wildcard;
        }
    
    }
}