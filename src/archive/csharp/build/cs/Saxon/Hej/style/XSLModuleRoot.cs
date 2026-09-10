using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public abstract class XSLModuleRoot : Saxon.Hej.style.StyleElement {
        public const int ANNOTATION_UNSPECIFIED = 0;
        public const int ANNOTATION_STRIP = 1;
        public const int ANNOTATION_PRESERVE = 2;
        public virtual bool isDeclaredModes() {
            return false;
        }
        public override void processAllAttributes() {
            prepareAttributes();
            foreach (Saxon.Hej.om.NodeInfo node in children(x => (x is Saxon.Hej.style.StyleElement))) {
                try {
                    ((Saxon.Hej.style.StyleElement)(node)).processAllAttributes();
                } catch (Saxon.Hej.trans.XPathException err) {
                    ((Saxon.Hej.style.StyleElement)(node)).compileError(err);
                }
            }
        }
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            compileError((getDisplayName()+" can appear only as the outermost element"), "XTSE0010");
        }
        public int getInputTypeAnnotationsAttribute() {
            string inputTypeAnnotationsAtt = getAttributeValue("", "input-type-annotations");
            if ((inputTypeAnnotationsAtt != null)) {
                switch (inputTypeAnnotationsAtt) {
                  case "strip":
                    return ANNOTATION_STRIP;
                  case "preserve":
                    return ANNOTATION_PRESERVE;
                  case "unspecified":
                    return ANNOTATION_UNSPECIFIED;
                  default:
                    compileError(("Invalid value for input-type-annotations attribute. "+"Permitted values are (strip, preserve, unspecified)"), "XTSE0020");
                    return ANNOTATION_UNSPECIFIED;
                }
            }
            return -1;
        }
    
    }
}