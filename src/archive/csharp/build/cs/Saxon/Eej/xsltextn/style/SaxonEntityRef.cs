using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.style {
    public class SaxonEntityRef : Saxon.Hej.style.ExtensionInstruction {
        internal string nameAttribute;
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getDisplayName();
                if (f.Equals("name")) {
                    nameAttribute = Saxon.Hej.value.Whitespace.trim(value);
                } else {
                    checkUnknownAttribute(attName);
                }
            }
            if ((nameAttribute == null)) {
                reportAbsence("name");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkEmpty();
            getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, "saxon:entity-ref", getPackageData().getLocalLicenseId());
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            return new Saxon.Hej.expr.instruct.ValueOf(new Saxon.Hej.expr.StringLiteral((('&'+nameAttribute)+';')), true, false);
        }
    
    }
}