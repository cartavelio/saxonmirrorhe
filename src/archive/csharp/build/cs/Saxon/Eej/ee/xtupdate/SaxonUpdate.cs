using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.xtupdate {
    public class SaxonUpdate : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("select")) {
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                } else {
                    checkUnknownAttribute(attName);
                }
            }
            if ((selectAtt == null)) {
                select = new Saxon.Hej.expr.ContextItemExpression();
                select.setLocation(allocateLocation());
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT, "saxon:modify", getPackageData().getLocalLicenseId());
            select = typeCheck("select", select);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression action = compileSequenceConstructor(exec, decl, true);
            if ((action == null)) {
                action = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Eej.ee.update.CopyModifyExpression exp = new Saxon.Eej.ee.update.CopyModifyExpression(action, new Saxon.Hej.expr.ContextItemExpression(), Saxon.Hej.lib.Validation.STRIP);
            Saxon.Hej.expr.instruct.CopyOf copy = new Saxon.Hej.expr.instruct.CopyOf(select, true, Saxon.Hej.lib.Validation.STRIP, null, false);
            copy.setCopyForUpdate(true);
            Saxon.Hej.expr.instruct.ForEach forEach = new Saxon.Hej.expr.instruct.ForEach(copy, exp);
            return forEach;
        }
        public static void checkContainment(Saxon.Hej.style.StyleElement child) {
            Saxon.Hej.om.NodeInfo s = child;
            while (true) {
                s = s.getParent();
                if ((s is Saxon.Eej.ee.xtupdate.SaxonUpdate)) {
                    return ;
                }
                if (!(((((((s is Saxon.Hej.style.XSLIf) || (s is Saxon.Hej.style.XSLChoose)) || (s is Saxon.Hej.style.XSLWhen)) || (s is Saxon.Hej.style.XSLOtherwise)) || (s is Saxon.Hej.style.XSLForEach))))) {
                    child.compileError((("The "+child.getDisplayName())+" instruction must be contained within saxon:update; only xsl:if, xsl:choose, and xsl:for-each may intervene"));
                    return ;
                }
            }
        }
    
    }
}