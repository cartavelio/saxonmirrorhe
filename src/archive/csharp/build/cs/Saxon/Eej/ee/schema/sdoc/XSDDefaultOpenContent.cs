using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDDefaultOpenContent : Saxon.Eej.ee.schema.sdoc.AnnotationParent {
        private bool appliesToEmpty = false;
        private string mode = "interleave";
        public XSDDefaultOpenContent()  {
        }
        protected internal override void prepareAttributes() {
            if ((getXSDSchema().getSchemaCompiler().getLanguageVersion() == Saxon.Hej.Configuration.XSD10)) {
                error("The xs:defaultOpenContent element requires XSD 1.1 to be enabled");
            }
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"appliesToEmpty", "id", "mode"};
            allowAttributes(atts, allowed);
            processId();
            string attValue = atts.getValue("", "appliesToEmpty");
            if ((attValue != null)) {
                appliesToEmpty = parseBooleanAttribute("appliesToEmpty", attValue);
            }
            attValue = Saxon.Hej.value.Whitespace.trim(atts.getValue("", "mode"));
            if ((attValue != null)) {
                if ((attValue.Equals("interleave") || attValue.Equals("suffix"))) {
                    mode = attValue;
                } else {
                    invalidAttributeValue("mode", attValue, "Must be 'interleave' or 'suffix'");
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
                        Saxon.Eej.ee.schema.Wildcard wildcard = ((Saxon.Eej.ee.schema.sdoc.XSDAny)(child)).getElementWildcard().getWildcard();
                        getXSDSchema().setDefaultOpenContent(wildcard, mode, appliesToEmpty);
                        break ;
                    }
                }
            }
        }
        private readonly static int[] state0terms = {Saxon.Hej.om.StandardNames.XS_ANY, Saxon.Hej.om.StandardNames.XS_ANNOTATION};
        private readonly static int[] state0targets = {1, 2};
        private readonly static int[] state1terms = {};
        private readonly static int[] state1targets = {};
        private readonly static int[] state2terms = {Saxon.Hej.om.StandardNames.XS_ANY};
        private readonly static int[] state2targets = {1};
        private readonly static int[][] stateTerms = {state0terms, state1terms, state2terms};
        private readonly static int[][] stateTargets = {state0targets, state1targets, state2targets};
        protected internal override bool isFinalState(int state) {
            return (state == 1);
        }
    
    }
}