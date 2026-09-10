using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLTry : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select;
        private bool rollbackOutput = true;
        private readonly System.Collections.Generic.IList<Saxon.Hej.pattern.QNameTest> catchTests = new System.Collections.Generic.List<Saxon.Hej.pattern.QNameTest>();
        private readonly System.Collections.Generic.IList<Saxon.Hej.expr.Expression> catchExprs = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>();
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            string rollbackOutputAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "select":
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                    break ;
                  case "rollback-output":
                    rollbackOutputAtt = value;
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((rollbackOutputAtt != null)) {
                rollbackOutput = processBooleanAttribute("rollback-output", rollbackOutputAtt);
            }
        }
        protected internal override bool isPermittedChild(Saxon.Hej.style.StyleElement child) {
            return (child is Saxon.Hej.style.XSLCatch);
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            select = typeCheck("select", select);
            bool foundCatch = false;
            foreach (Saxon.Hej.om.NodeInfo kid in children()) {
                if ((kid is Saxon.Hej.style.XSLCatch)) {
                    foundCatch = true;
                } else {
                    if ((kid is Saxon.Hej.style.XSLFallback)) {
                    } else {
                        if (foundCatch) {
                            compileError("xsl:catch elements must come after all other children of xsl:try (excepting xsl:fallback)", "XTSE0010");
                        }
                        if ((select != null)) {
                            compileError((("An "+getDisplayName())+" element with a select attribute must be empty"), "XTSE3140");
                        }
                    }
                }
            }
            if (!(foundCatch)) {
                compileError("xsl:try must have at least one xsl:catch child element", "XTSE0010");
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression content = compileSequenceConstructor(exec, decl, true);
            if ((select == null)) {
                select = content;
            }
            Saxon.Hej.expr.TryCatch expr = new Saxon.Hej.expr.TryCatch(select);
            for (int i = 0; (i<catchTests.Count); i++) {
                expr.addCatchExpression(catchTests[i], catchExprs[i]);
            }
            expr.setRollbackOutput(rollbackOutput);
            return expr;
        }
        public void addCatchClause(Saxon.Hej.pattern.QNameTest nameTest, Saxon.Hej.expr.Expression catchExpr) {
            catchTests.Add(nameTest);
            catchExprs.Add(catchExpr);
        }
    
    }
}