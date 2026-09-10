using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.style {
    public class SaxonDeepUpdate : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression root;
        private Saxon.Hej.expr.Expression select;
        private Saxon.Hej.expr.Expression action;
        public override bool isInstruction() {
            return true;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return false;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "root":
                    root = makeExpression(value, att);
                    break ;
                  case "select":
                    select = makeExpression(value, att);
                    break ;
                  case "action":
                    action = makeExpression(value, att);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((root == null)) {
                reportAbsence("root");
            }
            if ((select == null)) {
                reportAbsence("select");
            }
            if ((action == null)) {
                reportAbsence("action");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, "saxon:deep-update", getPackageData().getLocalLicenseId());
            root = typeCheck("root", root);
            select = typeCheck("select", select);
            action = typeCheck("action", action);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            return new Saxon.Eej.xsltextn.instruct.DeepUpdate(root, select, action);
        }
    
    }
}