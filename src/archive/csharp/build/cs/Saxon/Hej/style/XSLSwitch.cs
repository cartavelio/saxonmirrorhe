using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLSwitch : Saxon.Hej.style.XSLChooseOrSwitch {
        private Saxon.Hej.expr.Expression select;
        private Saxon.Hej.expr.LetExpression switchVar;
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("select")) {
                    select = makeExpression(value, att);
                } else {
                    checkUnknownAttribute(attName);
                }
            }
            if ((select == null)) {
                reportAbsence("select");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            requireXslt40("switch");
            select = typeCheck("select", select);
            base.validate(decl);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.LetExpression var = new Saxon.Hej.expr.LetExpression();
            var.setVariableQName(new Saxon.Hej.om.StructuredQName("vv", Saxon.Hej.om.NamespaceUri.SAXON_GENERATED_VARIABLE, ("v"+GetHashCode())));
            var.setSequence(select);
            var.setRequiredType(Saxon.Hej.value.SequenceType.SINGLE_ATOMIC);
            switchVar = var;
            Saxon.Hej.expr.Expression choose = base.compile(exec, decl);
            switchVar.setAction(choose);
            return switchVar;
        }
        protected internal override void compileConditions(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.expr.Expression[] conditions) {
            int w = 0;
            foreach (Saxon.Hej.om.NodeInfo curr in children()) {
                if ((curr is Saxon.Hej.style.XSLWhen)) {
                    Saxon.Hej.expr.Expression values = ((Saxon.Hej.style.XSLWhen)(curr)).getCondition();
                    conditions[w] = new Saxon.Hej.expr.GeneralComparison20(new Saxon.Hej.expr.LocalVariableReference(switchVar), Saxon.Hej.expr.parser.Token.EQUALS, values);
                    w++;
                } else {
                    if ((curr is Saxon.Hej.style.XSLOtherwise)) {
                        Saxon.Hej.expr.Expression otherwise = Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.TRUE);
                        otherwise.setRetainedStaticContext(makeRetainedStaticContext());
                        conditions[w] = otherwise;
                        w++;
                    }
                }
            }
        }
    
    }
}