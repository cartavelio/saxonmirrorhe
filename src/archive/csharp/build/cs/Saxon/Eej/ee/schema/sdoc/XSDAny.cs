using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDAny : Saxon.Eej.ee.schema.sdoc.AnnotationParent {
        private Saxon.Eej.ee.schema.ElementWildcard wildcard = null;
        public XSDAny()  {
        }
        protected internal override void prepareAttributes() {
            wildcard = new Saxon.Eej.ee.schema.ElementWildcard(getSchema().getConfiguration());
            wildcard.setLocator(this);
            Saxon.Hej.om.AttributeMap atts = attributes();
            if (((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Eej.ee.schema.sdoc.XSDDefaultOpenContent) || (((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Eej.ee.schema.sdoc.XSDOpenContent))) {
                string[] allowed = {"id", "namespace", "notNamespace", "notQName", "processContents"};
                allowAttributes(atts, allowed);
            } else {
                string[] allowed = {"id", "maxOccurs", "minOccurs", "namespace", "notNamespace", "notQName", "processContents"};
                allowAttributes(atts, allowed);
            }
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
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "maxOccurs");
            if ((attValue != null)) {
                int max = processMaxOccurs(attValue);
                wildcard.setMaxOccurs(max);
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "minOccurs");
            if ((attValue != null)) {
                int minOccurs = processMinOccurs(attValue);
                wildcard.setMinOccurs(minOccurs);
            }
            int maxOccurs = wildcard.getMaxOccurs();
            if (((maxOccurs != -1) && (wildcard.getMinOccurs()>maxOccurs))) {
                error("maxOccurs must not be less than minOccurs");
            }
            attValue = atts.getValue(Saxon.Hej.om.NamespaceUri.NULL, "processContents");
            if ((attValue != null)) {
                if (isProcessName(attValue)) {
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
            if (((getAttributeValue(Saxon.Hej.om.NamespaceUri.NULL, "notQName") != null) && (compiler.getLanguageVersion() != Saxon.Hej.Configuration.XSD11))) {
                error("The notQName attribute requires XSD 1.1 to be enabled");
            }
        }
        public Saxon.Eej.ee.schema.ElementWildcard getElementWildcard() {
            return wildcard;
        }
        protected internal static bool isProcessName(string name) {
            return ((("lax".Equals(name) || "skip".Equals(name)) || "strict".Equals(name)));
        }
    
    }
}