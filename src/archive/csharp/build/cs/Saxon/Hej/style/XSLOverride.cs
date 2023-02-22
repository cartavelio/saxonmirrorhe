using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLOverride : Saxon.Hej.style.StyleElement {
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                checkUnknownAttribute(attName);
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            foreach (Saxon.Hej.om.NodeInfo curr in children()) {
                if ((curr.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                    compileError("Character content is not allowed as a child of xsl:override", "XTSE0010");
                } else {
                    if (((((curr is Saxon.Hej.style.XSLFunction) || (curr is Saxon.Hej.style.XSLTemplate)) || (curr is Saxon.Hej.style.XSLGlobalVariable)) || (curr is Saxon.Hej.style.XSLAttributeSet))) {
                    } else {
                        ((Saxon.Hej.style.StyleElement)(curr)).compileError((("Element "+curr.getDisplayName())+" is not allowed as a child of xsl:override"), "XTSE0010");
                    }
                }
            }
        }
        public override void postValidate() {
            Saxon.Hej.style.XSLUsePackage parent = (Saxon.Hej.style.XSLUsePackage)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()));
            System.Diagnostics.Debug.Assert((parent != null));
            if ((parent.getUsedPackage() != null)) {
                foreach (Saxon.Hej.om.NodeInfo curr in children()) {
                    if (((((curr is Saxon.Hej.style.XSLFunction) || (curr is Saxon.Hej.style.XSLTemplate)) || (curr is Saxon.Hej.style.XSLGlobalVariable)) || (curr is Saxon.Hej.style.XSLAttributeSet))) {
                        Saxon.Hej.style.StylesheetComponent procedure = (Saxon.Hej.style.StylesheetComponent)(curr);
                        Saxon.Hej.trans.SymbolicName name = procedure.getSymbolicName();
                        if ((name == null)) {
                            if ((curr is Saxon.Hej.style.XSLTemplate)) {
                                Saxon.Hej.style.XSLTemplate decl = (Saxon.Hej.style.XSLTemplate)(curr);
                                if ((decl.getMatch() == null)) {
                                    decl.compileError("An overriding template with no name must have a match pattern");
                                }
                                Saxon.Hej.om.StructuredQName[] modeNames = decl.getModeNames();
                                foreach (Saxon.Hej.om.StructuredQName modeName in modeNames) {
                                    if (modeName.Equals(Saxon.Hej.trans.Mode.OMNI_MODE)) {
                                        ((Saxon.Hej.style.StyleElement)(curr)).compileError("An overriding template rule must not specify mode=\"#all\"", "XTSE3440");
                                    } else {
                                        if (modeName.Equals(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME)) {
                                            if (decl.getDefaultMode().Equals(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME)) {
                                                ((Saxon.Hej.style.StyleElement)(curr)).compileError("An overriding template rule must not belong to the unnamed mode", "XTSE3440");
                                            }
                                        }
                                    }
                                }
                            } else {
                                ((Saxon.Hej.style.StyleElement)(curr)).compileError("An overriding component (other than a template rule) must have a name", "XTSE3440");
                                return ;
                            }
                        } else {
                            Saxon.Hej.expr.Component overridden = parent.getUsedPackage().getComponent(name);
                            if ((overridden == null)) {
                                ((Saxon.Hej.style.StyleElement)(curr)).compileError((((("There is no "+Saxon.Hej.om.StandardNames.getLocalName(name.getComponentKind()))+" named ")+name.getShortName())+" in the used package"), "XTSE3058");
                                return ;
                            }
                            Saxon.Hej.trans.Visibility overriddenVis = overridden.getVisibility();
                            if ((overriddenVis == Saxon.Hej.trans.Visibility.UNDEFINED)) {
                                overriddenVis = Saxon.Hej.trans.Visibility.PRIVATE;
                            }
                            if (((overriddenVis == Saxon.Hej.trans.Visibility.FINAL) || (overriddenVis == Saxon.Hej.trans.Visibility.PRIVATE))) {
                                ((Saxon.Hej.style.StyleElement)(curr)).compileError(((((("The "+Saxon.Hej.om.StandardNames.getLocalName(name.getComponentKind()))+" named ")+name.getShortName())+" in the used package cannot be overridden because its visibility is ")+Saxon.Hej.trans.Err.describeVisibility(overriddenVis)), "XTSE3060");
                                return ;
                            }
                            procedure.checkCompatibility(overridden);
                        }
                    }
                }
            }
        }
    
    }
}