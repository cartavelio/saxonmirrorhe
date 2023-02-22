using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema.sdoc {
    public class XSDSimpleContent : Saxon.Eej.ee.schema.sdoc.SchemaElement {
        public XSDSimpleContent()  {
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string[] allowed = {"id"};
            allowAttributes(atts, allowed);
            processId();
        }
        public override void validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            checkContentModel(stateTerms, stateTargets, false);
        }
        private readonly static int[] state0terms = {Saxon.Hej.om.StandardNames.XS_EXTENSION, Saxon.Hej.om.StandardNames.XS_RESTRICTION, Saxon.Hej.om.StandardNames.XS_ANNOTATION};
        private readonly static int[] state0targets = {1, 1, 2};
        private readonly static int[] state1terms = {};
        private readonly static int[] state1targets = {};
        private readonly static int[] state2terms = {Saxon.Hej.om.StandardNames.XS_EXTENSION, Saxon.Hej.om.StandardNames.XS_RESTRICTION};
        private readonly static int[] state2targets = {1, 1};
        private readonly static int[][] stateTerms = {state0terms, state1terms, state2terms};
        private readonly static int[][] stateTargets = {state0targets, state1targets, state2targets};
        protected internal override bool isFinalState(int state) {
            return (state == 1);
        }
    
    }
}