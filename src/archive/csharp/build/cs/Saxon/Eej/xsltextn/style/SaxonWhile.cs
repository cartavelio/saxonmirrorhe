using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.style {
    public class SaxonWhile : Saxon.Hej.style.ExtensionInstruction {
        private Saxon.Hej.expr.Expression test;
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            test = Saxon.Hej.style.XSLIf.prepareTestAttribute(this);
            if ((test == null)) {
                reportAbsence("test");
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, "saxon:while", getPackageData().getLocalLicenseId());
            test = typeCheck("test", test);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression action = compileSequenceConstructor(exec, decl, true);
            if ((action == null)) {
                action = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            return new Saxon.Eej.xsltextn.instruct.While(test, action);
        }
    
    }
}