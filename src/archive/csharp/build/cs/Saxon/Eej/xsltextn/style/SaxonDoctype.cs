using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.xsltextn.style {
    public class SaxonDoctype : Saxon.Hej.style.ExtensionInstruction {
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                checkUnknownAttribute(attName);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            getConfiguration().checkLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION, "saxon:doctype", getPackageData().getLocalLicenseId());
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression content = compileSequenceConstructor(exec, decl, true);
            if ((content == null)) {
                content = Saxon.Hej.expr.Literal.makeEmptySequence();
            }
            Saxon.Hej.expr.instruct.Doctype inst = new Saxon.Hej.expr.instruct.Doctype(content);
            return inst;
        }
    
    }
}