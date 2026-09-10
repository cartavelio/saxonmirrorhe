using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDNotation : Saxon.Eej.ee.schema.sdoc.AnnotationParent {
        private Saxon.Eej.ee.schema.Notation notation;
        public XSDNotation()  {
        }
        protected internal override void prepareAttributes() {
            notation = new Saxon.Eej.ee.schema.Notation(getSchema().getConfiguration(), this);
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "name", "public", "system"};
            allowAttributes(atts, allowed);
            processId();
            string name = atts.getValue("", "name");
            if ((name == null)) {
                missingAttribute("name");
                name = ("unnamed-notation-"+GetHashCode());
            }
            Saxon.Hej.om.StructuredQName nc;
            try {
                nc = getComponentName(name, REQUIRE_TARGET_NS);
            } catch (Saxon.Hej.type.SchemaException err) {
                error(err.Message);
                nc = Saxon.Hej.om.StandardNames.SQ_XS_INVALID_NAME;
            }
            notation.setNotationName(nc);
            string publicAtt = atts.getValue("", "public");
            if ((publicAtt != null)) {
                notation.setNotationPublicId(publicAtt);
            }
            string systemAtt = atts.getValue("", "system");
            if ((systemAtt != null)) {
                notation.setNotationSystemId(systemAtt);
            }
            if (((publicAtt == null) && (systemAtt == null))) {
                error("A notation must have either a public identifier or a system identifier");
            }
        }
        public Saxon.Eej.ee.schema.Notation getNotation() {
            return notation;
        }
    
    }
}