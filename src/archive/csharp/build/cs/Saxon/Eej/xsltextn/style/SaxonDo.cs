using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.style {
    public class SaxonDo : Saxon.Hej.style.ExtensionInstruction {
        private Saxon.Hej.expr.Expression action;
        protected internal override bool mayContainSequenceConstructor() {
            return false;
        }
        protected internal override void prepareAttributes() {
            string actionAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("action")) {
                    actionAtt = value;
                    action = makeExpression(actionAtt, att);
                } else {
                    checkUnknownAttribute(attName);
                }
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, "saxon:while", getPackageData().getLocalLicenseId());
            action = typeCheck("action", action);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            return new Saxon.Eej.xsltextn.instruct.DoInstr(action);
        }
    
    }
}