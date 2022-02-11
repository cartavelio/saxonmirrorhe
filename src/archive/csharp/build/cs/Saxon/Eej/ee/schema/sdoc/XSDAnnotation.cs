using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDAnnotation : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        public XSDAnnotation()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id"};
            allowAttributes(atts, allowed);
            processId();
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                if (((fp == Saxon.Hej.om.StandardNames.XS_APPINFO) || (fp == Saxon.Hej.om.StandardNames.XS_DOCUMENTATION))) {
                } else {
                    illegalElement(child);
                }
            }
        }
    
    }
}