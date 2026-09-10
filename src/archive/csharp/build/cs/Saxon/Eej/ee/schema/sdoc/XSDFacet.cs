using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDFacet : Saxon.Eej.ee.schema.sdoc.AnnotationParent {
        private Saxon.Eej.ee.schema.Facet facet = null;
        public XSDFacet()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            if ((getLocalPart().Equals("enumeration") || getLocalPart().Equals("pattern"))) {
                string[] allowed = {"id", "value"};
                allowAttributes(atts, allowed);
            } else {
                string[] allowed = {"fixed", "id", "value"};
                allowAttributes(atts, allowed);
            }
            string value = atts.getValue("", "value");
            if ((value == null)) {
                if (getLocalPart().Equals("distinct")) {
                    value = "true";
                } else {
                    missingAttribute("value");
                    value = "";
                }
            }
            try {
                facet = /*134*/Saxon.Eej.ee.schema.Facet.makeFacet(getLocalPart(), value, getSchema().getConfiguration(), makeNamespaceContext());
            } catch (Saxon.Hej.type.SchemaException err) {
                error(err.Message);
                return ;
            }
            string message = atts.getValue(Saxon.Hej.lib.NamespaceConstant.SAXON, "message");
            if ((message != null)) {
                facet.setMessage(message);
            }
            if ((facet is Saxon.Eej.ee.schema.PatternFacet)) {
                string flags = atts.getValue(Saxon.Hej.lib.NamespaceConstant.SAXON, "flags");
                if ((flags != null)) {
                    ((Saxon.Eej.ee.schema.PatternFacet)(facet)).setFlags(flags);
                }
                System.Collections.Generic.IList<string> warnings = new System.Collections.Generic.List<string>();
                try {
                    ((Saxon.Eej.ee.schema.PatternFacet)(facet)).compile(warnings);
                } catch (Saxon.Hej.type.SchemaException e) {
                    error(e.Message);
                }
                foreach (string s in warnings) {
                    warning(s);
                }
            }
            string fixedAtt = atts.getValue("", "fixed");
            bool @fixed = false;
            if ((fixedAtt != null)) {
                @fixed = parseBooleanAttribute("fixed", fixedAtt);
            }
            facet.setFixed(@fixed);
            processId();
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            base.validate(compiler);
            if ((compiler.getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                if ((facet is Saxon.Eej.ee.schema.ExplicitTimezoneFacet)) {
                    error("Timezone facet requires XSD 1.1 to be enabled");
                }
            }
        }
        public Saxon.Eej.ee.schema.Facet getFacet() {
            return facet;
        }
    
    }
}