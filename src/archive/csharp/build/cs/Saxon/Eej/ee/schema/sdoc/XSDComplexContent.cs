using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDComplexContent : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        private Saxon.Ejava.util.Optional<bool> mixed = Saxon.Ejava.util.Optional<bool>.empty();
        public XSDComplexContent()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id", "mixed"};
            allowAttributes(atts, allowed);
            processId();
            string mixedAtt = atts.getValue("", "mixed");
            if ((mixedAtt != null)) {
                mixed = Saxon.Ejava.util.Optional<bool>.of(parseBooleanAttribute("mixed", mixedAtt));
            }
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            checkContentModel(stateTerms, stateTargets, false);
        }
        private readonly static int[] state0terms = {Saxon.Hej.om.StandardNames.XS_RESTRICTION, Saxon.Hej.om.StandardNames.XS_EXTENSION, Saxon.Hej.om.StandardNames.XS_ANNOTATION};
        private readonly static int[] state0targets = {1, 1, 2};
        private readonly static int[] state1terms = {};
        private readonly static int[] state1targets = {};
        private readonly static int[] state2terms = {Saxon.Hej.om.StandardNames.XS_RESTRICTION, Saxon.Hej.om.StandardNames.XS_EXTENSION};
        private readonly static int[] state2targets = {1, 1};
        private readonly static int[][] stateTerms = {state0terms, state1terms, state2terms};
        private readonly static int[][] stateTargets = {state0targets, state1targets, state2targets};
        protected internal override bool isFinalState(int state) {
            return (state == 1);
        }
        public Saxon.Ejava.util.Optional<bool> getMixedAttribute() {
            return mixed;
        }
    
    }
}