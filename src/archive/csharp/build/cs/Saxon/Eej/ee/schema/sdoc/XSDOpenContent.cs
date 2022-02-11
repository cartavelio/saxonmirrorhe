using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDOpenContent : Saxon.Eej.ee.schema.sdoc.AnnotationParent {
        private string mode = "interleave";
        private Saxon.Eej.ee.schema.ElementWildcard wildcard;
        public XSDOpenContent()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "mode"};
            allowAttributes(atts, allowed);
            processId();
            string attValue = Saxon.Hej.value.Whitespace.trim(atts.getValue("", "mode"));
            if ((attValue != null)) {
                if (((attValue.Equals("interleave") || attValue.Equals("suffix")) || attValue.Equals("none"))) {
                    mode = attValue;
                } else {
                    invalidAttributeValue("mode", attValue, "Must be 'interleave' or 'suffix' or 'none");
                }
            }
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            checkContentModel(stateTerms, stateTargets, false);
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                switch (fp) {
                  case Saxon.Hej.om.StandardNames.XS_ANY:
                    {
                        if ("none".Equals(mode)) {
                            error("When mode='none', the <xs:openContent> element must not have an <xs:any> child element");
                        } else {
                            wildcard = ((Saxon.Eej.ee.schema.sdoc.XSDAny)(child)).getElementWildcard();
                        }
                        break ;
                    }
                }
            }
            if (((wildcard == null) && !("none".Equals(mode)))) {
                error("Except when mode='none', the <xs:openContent> element must have an <xs:any> child element");
                mode = "none";
            }
        }
        private readonly static int[] state0terms = {Saxon.Hej.om.StandardNames.XS_ANNOTATION, Saxon.Hej.om.StandardNames.XS_ANY};
        private readonly static int[] state0targets = {1, 2};
        private readonly static int[] state1terms = {Saxon.Hej.om.StandardNames.XS_ANY};
        private readonly static int[] state1targets = {2};
        private readonly static int[] state2terms = {};
        private readonly static int[] state2targets = {};
        private readonly static int[][] stateTerms = {state0terms, state1terms, state2terms};
        private readonly static int[][] stateTargets = {state0targets, state1targets, state2targets};
        protected internal override bool isFinalState(int state) {
            return (((state == 0) || (state == 1)) || (state == 2));
        }
        public string getMode() {
            return mode;
        }
        public Saxon.Eej.ee.schema.ElementWildcard getElementWildcard() {
            return wildcard;
        }
    
    }
}