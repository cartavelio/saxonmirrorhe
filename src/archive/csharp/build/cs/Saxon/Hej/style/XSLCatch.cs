using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLCatch : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.expr.Expression select;
        private Saxon.Hej.pattern.QNameTest nameTest;
        public override bool isInstruction() {
            return false;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override bool seesAvuncularVariables() {
            return false;
        }
        protected internal override void prepareAttributes() {
            string selectAtt = null;
            string errorAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                string value = att.getValue();
                if (f.Equals("select")) {
                    selectAtt = value;
                    select = makeExpression(selectAtt, att);
                } else {
                    if (f.Equals("errors")) {
                        errorAtt = value;
                    } else {
                        checkUnknownAttribute(attName);
                    }
                }
            }
            if ((errorAtt == null)) {
                nameTest = Saxon.Hej.pattern.AnyNodeTest.getInstance();
            } else {
                System.Collections.Generic.IList<Saxon.Hej.pattern.QNameTest> tests = parseNameTests(errorAtt);
                if ((tests.Count == 0)) {
                    compileError("xsl:catch/@errors must not be empty");
                }
                if ((tests.Count == 1)) {
                    nameTest = tests[0];
                } else {
                    nameTest = new Saxon.Hej.pattern.UnionQNameTest(tests);
                }
            }
        }
        private System.Collections.Generic.IList<Saxon.Hej.pattern.QNameTest> parseNameTests(string elements) {
            System.Collections.Generic.IList<Saxon.Hej.pattern.QNameTest> result = new System.Collections.Generic.List<Saxon.Hej.pattern.QNameTest>();
            Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(elements, " \t\n\r", false);
            while (st.hasMoreTokens()) {
                string s = st.nextToken();
                Saxon.Hej.pattern.QNameTest nt;
                if (s.Equals("*")) {
                    nt = Saxon.Hej.pattern.AnyNodeTest.getInstance();
                    result.Add(nt);
                } else {
                    if (s.EndsWith(":*")) {
                        if ((s.Length == 2)) {
                            compileError("No prefix before ':*'");
                            result.Add(Saxon.Hej.pattern.AnyNodeTest.getInstance());
                        }
                        string prefix = s.Substring(0, (s.Length-2) - 0);
                        Saxon.Hej.om.NamespaceUri uri = getURIForPrefix(prefix, false);
                        nt = new Saxon.Hej.pattern.NamespaceTest(getNamePool(), Saxon.Hej.type.Type.ELEMENT, uri);
                        result.Add(nt);
                    } else {
                        if (s.StartsWith("*:")) {
                            if ((s.Length == 2)) {
                                compileErrorInAttribute("No local name after '*:'", "XTSE0010", "errors");
                                result.Add(Saxon.Hej.pattern.AnyNodeTest.getInstance());
                            }
                            string localname = s.Substring(2);
                            nt = new Saxon.Hej.pattern.LocalNameTest(getNamePool(), Saxon.Hej.type.Type.ELEMENT, localname);
                            result.Add(nt);
                        } else {
                            string prefix;
                            string localName;
                            Saxon.Hej.om.NamespaceUri uri;
                            try {
                                string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(s);
                                prefix = parts[0];
                                if (parts[0].Equals("")) {
                                    uri = Saxon.Hej.om.NamespaceUri.NULL;
                                } else {
                                    uri = getURIForPrefix(prefix, false);
                                    if ((uri == null)) {
                                        undeclaredNamespaceError(prefix, "XTSE0280", "errors");
                                        result.Add(Saxon.Hej.pattern.AnyNodeTest.getInstance());
                                        break ;
                                    }
                                }
                                localName = parts[1];
                            } catch (Saxon.Hej.om.QNameException err) {
                                compileErrorInAttribute((("Error code "+s)+" is not a valid QName"), "XTSE0280", "errors");
                                result.Add(Saxon.Hej.pattern.AnyNodeTest.getInstance());
                                break ;
                            }
                            Saxon.Hej.om.NamePool target = getNamePool();
                            int nameCode = target.allocateFingerprint(uri, localName);
                            nt = new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, nameCode, getNamePool());
                            result.Add(nt);
                        }
                    }
                }
            }
            return result;
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            select = typeCheck("select", select);
            if (((select != null) && hasChildNodes())) {
                compileError("An xsl:catch element with a select attribute must be empty", "XTSE3150");
            }
            if (!(((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLTry)))) {
                compileError("xsl:catch may appear only as a child of xsl:try");
            }
        }
        public override Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl) {
            if ((select == null)) {
                select = compileSequenceConstructor(exec, decl, true);
            }
            ((Saxon.Hej.style.XSLTry)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).addCatchClause(nameTest, select);
            return null;
        }
    
    }
}