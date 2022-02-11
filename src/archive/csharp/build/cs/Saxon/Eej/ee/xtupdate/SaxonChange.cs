using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.xtupdate {
    public class SaxonChange : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select;
        private Saxon.Hej.expr.Expression newValue;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return false;
        }
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            string toAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                if (f.Equals("select")) {
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                } else {
                    if (f.Equals("to")) {
                        toAtt = value;
                        newValue = makeExpression(toAtt, att);
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ((selectAtt == null)) {
                select = new Saxon.Hej.expr.ContextItemExpression();
                select.setLocation(allocateLocation());
            }
            if ((toAtt == null)) {
                reportAbsence("to");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT, "saxon:change", getPackageData().getLocalLicenseId());
            select = typeCheck("select", select);
            newValue = typeCheck("to", newValue);
            /*134*/Saxon.Eej.ee.xtupdate.SaxonUpdate.checkContainment(this);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            return new Saxon.Eej.ee.update.ReplaceValueExpression(select, newValue);
        }
    
    }
}