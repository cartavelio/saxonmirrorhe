using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLPreserveSpace : Saxon.Hej.style.StyleElement {
        private string elements;
        public override bool isDeclaration() {
            return true;
        }
        protected internal override void prepareAttributes() {
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                if (f.Equals("elements")) {
                    elements = att.getValue();
                } else {
                    checkUnknownAttribute(attName);
                }
            }
            if ((elements == null)) {
                reportAbsence("elements");
                elements = "*";
            }
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkEmpty();
            checkTopLevel("XTSE0010", false);
        }
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            if ((getFingerprint() == Saxon.Hej.om.StandardNames.XSL_STRIP_SPACE)) {
                if ((getFingerprint() == Saxon.Hej.om.StandardNames.XSL_STRIP_SPACE)) {
                    string elements = getAttributeValue("", "elements");
                    if (((elements != null) && !(elements.Trim().Length==0))) {
                        top.getStylesheetPackage().setStripsWhitespace(true);
                    }
                }
            }
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.@event.Stripper.StripRuleTarget preserve = ((getFingerprint() == Saxon.Hej.om.StandardNames.XSL_PRESERVE_SPACE) ? Saxon.Hej.@event.Stripper.PRESERVE : Saxon.Hej.@event.Stripper.STRIP);
            Saxon.Hej.style.PrincipalStylesheetModule psm = getCompilation().getPrincipalStylesheetModule();
            Saxon.Hej.om.SpaceStrippingRule stripperRules = psm.getStylesheetPackage().getStripperRules();
            if (!(((stripperRules is Saxon.Hej.om.SelectedElementsSpaceStrippingRule)))) {
                stripperRules = new Saxon.Hej.om.SelectedElementsSpaceStrippingRule(true);
                psm.getStylesheetPackage().setStripperRules(stripperRules);
            }
            Saxon.Hej.om.SelectedElementsSpaceStrippingRule rules = (Saxon.Hej.om.SelectedElementsSpaceStrippingRule)(stripperRules);
            Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(elements, " \t\n\r", false);
            try {
                while (st.hasMoreTokens()) {
                    string s = st.nextToken();
                    Saxon.Hej.pattern.NodeTest nt;
                    if (s.Equals("*")) {
                        nt = Saxon.Hej.pattern.NodeKindTest.ELEMENT;
                        rules.addRule(nt, preserve, decl.getModule(), decl.getSourceElement().getLineNumber());
                    } else {
                        if (s.StartsWith("Q{")) {
                            int brace = s.IndexOf('}');
                            if ((brace<0)) {
                                compileError("No closing '}' in EQName");
                            } else {
                                if ((brace == (s.Length-1))) {
                                    compileError("Missing local part in EQName");
                                } else {
                                    string uri = s.Substring(2, brace - 2);
                                    string local = s.Substring((brace+1));
                                    if (local.Equals("*")) {
                                        nt = new Saxon.Hej.pattern.NamespaceTest(getNamePool(), Saxon.Hej.type.Type.ELEMENT, uri);
                                    } else {
                                        nt = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, uri, local, getNamePool());
                                    }
                                    rules.addRule(nt, preserve, decl.getModule(), decl.getSourceElement().getLineNumber());
                                }
                            }
                        } else {
                            if (s.EndsWith(":*")) {
                                if ((s.Length == 2)) {
                                    compileError("No prefix before ':*'");
                                }
                                string prefix = s.Substring(0, (s.Length-2) - 0);
                                string uri = getURIForPrefix(prefix, false);
                                if ((uri == null)) {
                                    undeclaredNamespaceError(prefix, "XTSE0280", "elements");
                                }
                                nt = new Saxon.Hej.pattern.NamespaceTest(getNamePool(), Saxon.Hej.type.Type.ELEMENT, uri);
                                rules.addRule(nt, preserve, decl.getModule(), decl.getSourceElement().getLineNumber());
                            } else {
                                if (s.StartsWith("*:")) {
                                    if ((s.Length == 2)) {
                                        compileErrorInAttribute("No local name after '*:'", "XTSE0010", "elements");
                                    }
                                    string localname = s.Substring(2);
                                    nt = new Saxon.Hej.pattern.LocalNameTest(getNamePool(), Saxon.Hej.type.Type.ELEMENT, localname);
                                    rules.addRule(nt, preserve, decl.getModule(), decl.getSourceElement().getLineNumber());
                                } else {
                                    string prefix;
                                    string localName;
                                    string uri;
                                    try {
                                        string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(s);
                                        prefix = parts[0];
                                        if (parts[0].Equals("")) {
                                            uri = getDefaultXPathNamespace();
                                        } else {
                                            uri = getURIForPrefix(prefix, false);
                                            if ((uri == null)) {
                                                undeclaredNamespaceError(prefix, "XTSE0280", "elements");
                                            }
                                        }
                                        localName = parts[1];
                                    } catch (Saxon.Hej.om.QNameException err) {
                                        compileError((("Element name "+s)+" is not a valid QName"), "XTSE0280");
                                        return ;
                                    }
                                    Saxon.Hej.om.NamePool target = getNamePool();
                                    int nameCode = target.allocateFingerprint(uri, localName);
                                    nt = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, nameCode, getNamePool());
                                    rules.addRule(nt, preserve, decl.getModule(), decl.getSourceElement().getLineNumber());
                                }
                            }
                        }
                    }
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(allocateLocation());
                compileError(e);
            }
        }
    
    }
}