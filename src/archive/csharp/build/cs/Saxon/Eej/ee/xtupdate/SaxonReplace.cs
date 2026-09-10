using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.xtupdate {
    public class SaxonReplace : Saxon.Hej.style.ExtensionInstruction {
        private Saxon.Hej.expr.Expression select;
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                if (f.Equals("select")) {
                    selectAtt = att.getValue();
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
            getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT, "saxon:replace", getPackageData().getLocalLicenseId());
            select = typeCheck("select", select);
            /*134*/Saxon.Eej.ee.xtupdate.SaxonUpdate.checkContainment(this);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression content = compileSequenceConstructor(exec, decl, false);
            return new Saxon.Eej.ee.update.ReplaceNodeExpression(select, content, Saxon.Hej.lib.Validation.PRESERVE, true);
        }
    
    }
}