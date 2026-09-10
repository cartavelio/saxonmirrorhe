using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public abstract class XSLAcceptExpose : Saxon.Hej.style.StyleElement {
        private readonly System.Collections.Generic.ISet<Saxon.Hej.trans.ComponentTest> explicitComponentTests = new System.Collections.Generic.HashSet<Saxon.Hej.trans.ComponentTest>();
        private readonly System.Collections.Generic.ISet<Saxon.Hej.trans.ComponentTest> wildcardComponentTests = new System.Collections.Generic.HashSet<Saxon.Hej.trans.ComponentTest>();
        private Saxon.Hej.trans.Visibility visibility = Saxon.Hej.trans.Visibility.UNDEFINED;
        public override Saxon.Hej.trans.Visibility getVisibility() {
            return visibility;
        }
        public System.Collections.Generic.ISet<Saxon.Hej.trans.ComponentTest> getExplicitComponentTests() {
            prepareAttributes();
            return explicitComponentTests;
        }
        public System.Collections.Generic.ISet<Saxon.Hej.trans.ComponentTest> getWildcardComponentTests() {
            prepareAttributes();
            return wildcardComponentTests;
        }
        protected internal override void prepareAttributes() {
            if ((visibility != Saxon.Hej.trans.Visibility.UNDEFINED)) {
                return ;
            }
            string componentAtt = null;
            string namesAtt = null;
            string visibilityAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                switch (f) {
                  case "names":
                    namesAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "component":
                    componentAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  case "visibility":
                    visibilityAtt = Saxon.Hej.value.Whitespace.trim(value);
                    break ;
                  default:
                    checkUnknownAttribute(attName);
                    break ;
                }
            }
            if ((visibilityAtt == null)) {
                reportAbsence("visibility");
                visibility = Saxon.Hej.trans.Visibility.PRIVATE;
            } else {
                visibility = interpretVisibilityValue(visibilityAtt, ((this is Saxon.Hej.style.XSLAccept) ? "ha" : ""));
                if ((visibility == Saxon.Hej.trans.Visibility.UNDEFINED)) {
                    visibility = Saxon.Hej.trans.Visibility.PRIVATE;
                }
            }
            int componentTypeCode = Saxon.Hej.om.StandardNames.XSL_FUNCTION;
            if ((componentAtt == null)) {
                reportAbsence("component");
            } else {
                string local = Saxon.Hej.value.Whitespace.trim(componentAtt);
                switch (local) {
                  case "function":
                    componentTypeCode = Saxon.Hej.om.StandardNames.XSL_FUNCTION;
                    break ;
                  case "template":
                    componentTypeCode = Saxon.Hej.om.StandardNames.XSL_TEMPLATE;
                    break ;
                  case "variable":
                    componentTypeCode = Saxon.Hej.om.StandardNames.XSL_VARIABLE;
                    break ;
                  case "attribute-set":
                    componentTypeCode = Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE_SET;
                    break ;
                  case "mode":
                    componentTypeCode = Saxon.Hej.om.StandardNames.XSL_MODE;
                    break ;
                  case "*":
                    componentTypeCode = -1;
                    break ;
                  default:
                    compileError("The component type is not one of the allowed names (function, template, variable, attribute-set, or mode)", "XTSE0020");
                    return ;
                }
            }
            if ((namesAtt == null)) {
                reportAbsence("names");
                namesAtt = "";
            }
            Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(namesAtt, " \t\r\n");
            while (st.hasMoreTokens()) {
                string tok = st.nextToken();
                Saxon.Hej.pattern.QNameTest test;
                int hash = tok.LastIndexOf('#');
                if (((hash>0) && (tok.IndexOf('}', hash)<0))) {
                    if ((componentTypeCode == -1)) {
                        compileErrorInAttribute("When component='*' is specified, all names must be wildcards", ((this is Saxon.Hej.style.XSLAccept) ? "XTSE3032" : "XTSE3022"), "names");
                    } else {
                        if ((componentTypeCode == Saxon.Hej.om.StandardNames.XSL_FUNCTION)) {
                            Saxon.Hej.om.StructuredQName name = makeQName(tok.Substring(0, hash - 0), null, "names");
                            test = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, name.getURI(), name.getLocalPart(), getNamePool());
                            int arity = 0;
                            try {
                                arity = System.Int32.Parse(tok.Substring((hash+1)));
                            } catch (System.Exception err) {
                                compileErrorInAttribute((("Malformed function arity in '"+tok)+"'"), "XTSE0020", "names");
                            }
                            explicitComponentTests.Add(new Saxon.Hej.trans.ComponentTest(componentTypeCode, test, arity));
                        } else {
                            compileErrorInAttribute("Cannot specify arity for components other than functions", "XTSE3020", "names");
                        }
                    }
                } else {
                    if (tok.Equals("*")) {
                        test = Saxon.Hej.pattern.AnyNodeTest.getInstance();
                        addWildCardTest(componentTypeCode, test);
                    } else {
                        if (tok.EndsWith(":*")) {
                            if ((tok.Length == 2)) {
                                compileErrorInAttribute("No prefix before ':*'", "XTSE0020", "names");
                            }
                            string prefix = tok.Substring(0, (tok.Length-2) - 0);
                            string uri = getURIForPrefix(prefix, false);
                            if ((uri == null)) {
                                compileErrorInAttribute(("Undeclared prefix "+prefix), "XTSE0020", "names");
                                uri = Saxon.Hej.lib.NamespaceConstant.ANONYMOUS;
                            }
                            test = new Saxon.Hej.pattern.NamespaceTest(getNamePool(), Saxon.Hej.type.Type.ELEMENT, uri);
                            addWildCardTest(componentTypeCode, test);
                        } else {
                            if ((tok.StartsWith("Q{") && tok.EndsWith("}*"))) {
                                string uri = tok.Substring(2, (tok.Length-2) - 2);
                                test = new Saxon.Hej.pattern.NamespaceTest(getNamePool(), Saxon.Hej.type.Type.ELEMENT, uri);
                                wildcardComponentTests.Add(new Saxon.Hej.trans.ComponentTest(componentTypeCode, test, -1));
                            } else {
                                if (tok.StartsWith("*:")) {
                                    if ((tok.Length == 2)) {
                                        compileErrorInAttribute("No local name after '*:'", "XTSE0020", "names");
                                    }
                                    string localname = tok.Substring(2);
                                    test = new Saxon.Hej.pattern.LocalNameTest(getNamePool(), Saxon.Hej.type.Type.ELEMENT, localname);
                                    addWildCardTest(componentTypeCode, test);
                                } else {
                                    if ((componentTypeCode == -1)) {
                                        compileErrorInAttribute("When component='*' is specified, all names must be wildcards", ((this is Saxon.Hej.style.XSLAccept) ? "XTSE3032" : "XTSE3022"), "names");
                                    } else {
                                        if ((componentTypeCode == Saxon.Hej.om.StandardNames.XSL_FUNCTION)) {
                                            compileErrorInAttribute("When the name identifies a function, the arity must be given (XSLT 3.0 erratum E36)", "XTSE3020", "names");
                                        } else {
                                            Saxon.Hej.om.StructuredQName name = makeQName(tok, null, "names");
                                            test = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, name.getURI(), name.getLocalPart(), getNamePool());
                                            explicitComponentTests.Add(new Saxon.Hej.trans.ComponentTest(componentTypeCode, test, -1));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        private void addWildCardTest(int componentTypeCode, Saxon.Hej.pattern.QNameTest test) {
            if ((componentTypeCode == -1)) {
                wildcardComponentTests.Add(new Saxon.Hej.trans.ComponentTest(Saxon.Hej.om.StandardNames.XSL_FUNCTION, test, -1));
                wildcardComponentTests.Add(new Saxon.Hej.trans.ComponentTest(Saxon.Hej.om.StandardNames.XSL_TEMPLATE, test, -1));
                wildcardComponentTests.Add(new Saxon.Hej.trans.ComponentTest(Saxon.Hej.om.StandardNames.XSL_VARIABLE, test, -1));
                wildcardComponentTests.Add(new Saxon.Hej.trans.ComponentTest(Saxon.Hej.om.StandardNames.XSL_ATTRIBUTE_SET, test, -1));
                wildcardComponentTests.Add(new Saxon.Hej.trans.ComponentTest(Saxon.Hej.om.StandardNames.XSL_MODE, test, -1));
            } else {
                wildcardComponentTests.Add(new Saxon.Hej.trans.ComponentTest(componentTypeCode, test, -1));
            }
        }
    
    }
}