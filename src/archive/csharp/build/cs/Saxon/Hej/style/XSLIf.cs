using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLIf : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression test;
        private Saxon.Hej.expr.Expression thenExp;
        private Saxon.Hej.expr.Expression elseExp;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                switch (f) {
                  case "test":
                    test = makeExpression(att.getValue(), att);
                    break ;
                  case "then":
                    requireXslt40("then");
                    thenExp = makeExpression(att.getValue(), att);
                    break ;
                  case "else":
                    requireXslt40("else");
                    elseExp = makeExpression(att.getValue(), att);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((test == null)) {
                reportAbsence("test");
            }
        }
        public static Saxon.Hej.expr.Expression prepareTestAttribute(Saxon.Hej.style.StyleElement se) {
            Saxon.Hej.om.AttributeInfo testAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in se.attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                if (f.Equals("test")) {
                    testAtt = att;
                } else {
                    se.checkUnknownAttribute(attName);
                }
            }
            if ((testAtt == null)) {
                return null;
            } else {
                return se.makeExpression(testAtt.getValue(), testAtt);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            test = typeCheck("test", test);
            if (((thenExp != null) && hasChildNodes())) {
                compileError("xsl:if element must be empty if @then is present", "XTSE0010");
            }
        }
        protected internal override bool markTailCalls() {
            Saxon.Hej.style.StyleElement last = getLastChildInstruction();
            return ((last != null) && last.markTailCalls());
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((test is Saxon.Hej.expr.Literal)) {
                Saxon.Hej.om.GroundedValue testVal = ((Saxon.Hej.expr.Literal)(test)).getGroundedValue();
                try {
                    if (testVal.effectiveBooleanValue()) {
                        return compileSequenceConstructor(exec, decl, true);
                    } else {
                        return null;
                    }
                } catch (Saxon.Hej.trans.XPathException err) {
                }
            }
            Saxon.Hej.expr.Expression action = compileSequenceConstructor(exec, decl, true);
            if ((action == null)) {
                return null;
            }
            Saxon.Hej.expr.Expression[] conditions;
            Saxon.Hej.expr.Expression[] actions;
            if ((elseExp == null)) {
                conditions = new Saxon.Hej.expr.Expression[]{test};
                actions = new Saxon.Hej.expr.Expression[]{action};
            } else {
                conditions = new Saxon.Hej.expr.Expression[]{test, Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE)};
                actions = new Saxon.Hej.expr.Expression[]{action, elseExp};
            }
            Saxon.Hej.expr.instruct.Choose choose = new Saxon.Hej.expr.instruct.Choose(conditions, actions);
            choose.setInstruction(true);
            return choose;
        }
        public override Saxon.Hej.expr.Expression compileSequenceConstructor(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl, bool includeParams) {
            if ((thenExp == null)) {
                return base.compileSequenceConstructor(compilation, decl, includeParams);
            } else {
                return thenExp;
            }
        }
    
    }
}