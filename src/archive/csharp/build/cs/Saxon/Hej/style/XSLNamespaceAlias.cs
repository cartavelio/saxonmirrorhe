using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLNamespaceAlias : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.om.NamespaceUri stylesheetURI;
        private Saxon.Hej.om.NamespaceBinding resultNamespaceBinding;
        public override bool isDeclaration() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string stylesheetPrefix = null;
            string resultPrefix = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("stylesheet-prefix")) {
                    stylesheetPrefix = Saxon.Hej.value.Whitespace.trim(value);
                } else {
                    if (f.Equals("result-prefix")) {
                        resultPrefix = Saxon.Hej.value.Whitespace.trim(value);
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ((stylesheetPrefix == null)) {
                reportAbsence("stylesheet-prefix");
                stylesheetPrefix = "";
            }
            if (stylesheetPrefix.Equals("#default")) {
                stylesheetPrefix = "";
            }
            if ((resultPrefix == null)) {
                reportAbsence("result-prefix");
                resultPrefix = "";
            }
            if (resultPrefix.Equals("#default")) {
                resultPrefix = "";
            }
            stylesheetURI = getURIForPrefix(stylesheetPrefix, true);
            if ((stylesheetURI == null)) {
                compileError((("stylesheet-prefix "+stylesheetPrefix)+" has not been declared"), "XTSE0812");
                stylesheetURI = Saxon.Hej.om.NamespaceUri.NULL;
                resultNamespaceBinding = Saxon.Hej.om.NamespaceBinding.DEFAULT_UNDECLARATION;
                return ;
            }
            Saxon.Hej.om.NamespaceUri resultURI = getURIForPrefix(resultPrefix, true);
            if ((resultURI == null)) {
                compileError((("result-prefix "+resultPrefix)+" has not been declared"), "XTSE0812");
                stylesheetURI = Saxon.Hej.om.NamespaceUri.NULL;
                resultURI = Saxon.Hej.om.NamespaceUri.NULL;
            }
            resultNamespaceBinding = new Saxon.Hej.om.NamespaceBinding(resultPrefix, resultURI);
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkTopLevel("XTSE0010", false);
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            return null;
        }
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            top.addNamespaceAlias(decl);
        }
        public Saxon.Hej.om.NamespaceUri getStylesheetURI() {
            return stylesheetURI;
        }
        public Saxon.Hej.om.NamespaceBinding getResultNamespaceBinding() {
            return resultNamespaceBinding;
        }
    
    }
}