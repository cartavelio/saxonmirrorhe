using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLStylesheet : Saxon.Hej.style.XSLModuleRoot {
        public override void initialise(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType elementType, Saxon.Hej.om.AttributeMap atts, Saxon.Hej.om.NodeInfo parent, int sequenceNumber) {
            base.initialise(elemName, elementType, atts, parent, sequenceNumber);
            processDefaultCollationAttribute();
        }
        protected internal override bool mayContainParam() {
            return true;
        }
        protected internal override void prepareAttributes() {
            processDefaultCollationAttribute();
            processDefaultMode();
            string inputTypeAnnotationsAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "version":
                    break ;
                  case "id":
                    break ;
                  case "extension-element-prefixes":
                    break ;
                  case "exclude-result-prefixes":
                    break ;
                  case "input-type-annotations":
                    inputTypeAnnotationsAtt = value;
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if (((version == -1) && (((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) == null) || (((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()).getNodeKind() == Saxon.Hej.type.Type.DOCUMENT))))) {
                reportAbsence("version");
            }
            if ((inputTypeAnnotationsAtt != null)) {
                switch (inputTypeAnnotationsAtt) {
                  case "strip":
                    break ;
                  case "preserve":
                    break ;
                  case "unspecified":
                    break ;
                  default:
                    invalidAttribute("input-type-annotations", "strip|preserve|unspecified");
                    break ;
                }
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            if ((validationError != null)) {
                compileError(validationError);
            }
            if (((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) != null) && (((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()).getNodeKind() != Saxon.Hej.type.Type.DOCUMENT))) {
                compileError((getDisplayName()+" must be the outermost element"), "XTSE0010");
            }
            foreach (Saxon.Hej.om.NodeInfo curr in children()) {
                if ((((curr.getNodeKind() == Saxon.Hej.type.Type.TEXT) || (((curr is Saxon.Hej.style.StyleElement) && ((Saxon.Hej.style.StyleElement)(curr)).isDeclaration()))) || (curr is Saxon.Hej.style.DataElement))) {
                } else {
                    if ((curr is Saxon.Hej.style.StyleElement)) {
                        if ((!(((Saxon.Hej.style.StyleElement)(curr)).isInXsltNamespace()) && !(((Saxon.Hej.style.StyleElement)(curr)).getNodeName().hasURI(Saxon.Hej.om.NamespaceUri.NULL)))) {
                        } else {
                            if (((curr is Saxon.Hej.style.AbsentExtensionElement) && ((Saxon.Hej.style.StyleElement)(curr)).forwardsCompatibleModeIsEnabled())) {
                            } else {
                                if (((Saxon.Hej.style.StyleElement)(curr)).isInXsltNamespace()) {
                                    ((Saxon.Hej.style.StyleElement)(curr)).compileError(((("Element "+curr.getDisplayName())+" must not appear directly within ")+getDisplayName()), "XTSE0010");
                                } else {
                                    ((Saxon.Hej.style.StyleElement)(curr)).compileError((((("Element "+curr.getDisplayName())+" must not appear directly within ")+getDisplayName())+" because it is not in a namespace"), "XTSE0130");
                                }
                            }
                        }
                    }
                }
            }
        }
    
    }
}