using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLOutput : Saxon.Hej.style.StyleElement {
        private Saxon.Hej.om.StructuredQName outputFormatName;
        private readonly string method = null;
        private readonly string outputVersion = null;
        private string useCharacterMaps = null;
        private readonly System.Collections.Generic.IDictionary<string,string> serializationAttributes = new System.Collections.Generic.Dictionary<string,string>(10);
        private System.Collections.Generic.Dictionary<string,string> userAttributes = null;
        public override bool isDeclaration() {
            return true;
        }
        protected internal override void prepareAttributes() {
            string nameAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string value = att.getValue();
                string f = attName.getStructuredQName().getClarkName();
                if (f.Equals("name")) {
                    nameAtt = Saxon.Hej.value.Whitespace.trim(value);
                } else {
                    if (f.Equals("version")) {
                        string outputVersion = Saxon.Hej.value.Whitespace.trim(value);
                        Saxon.Impl.Helpers.MapUtils.Put(serializationAttributes, f, outputVersion);
                    } else {
                        if (f.Equals("use-character-maps")) {
                            useCharacterMaps = value;
                        } else {
                            if (f.Equals("parameter-document")) {
                                string val = Saxon.Hej.value.Whitespace.trim(value);
                                try {
                                    val = Saxon.Hej.functions.ResolveURI.makeAbsolute(val, getBaseURI()).OriginalString;
                                } catch (System.UriFormatException e) {
                                    compileError(Saxon.Hej.trans.XPathException.makeXPathException(e));
                                }
                                Saxon.Impl.Helpers.MapUtils.Put(serializationAttributes, f, val);
                            } else {
                                if ((Saxon.Hej.style.XSLResultDocument.fans.Contains(f) && !(f.Equals("output-version")))) {
                                    string val = value;
                                    if ((!(f.Equals(Saxon.Hej.lib.SaxonOutputKeys.ITEM_SEPARATOR)) && !(f.Equals(Saxon.Hej.lib.SaxonOutputKeys.NEWLINE)))) {
                                        val = Saxon.Hej.value.Whitespace.trim(val);
                                    }
                                    Saxon.Impl.Helpers.MapUtils.Put(serializationAttributes, f, val);
                                } else {
                                    string attributeURI = attName.getURI();
                                    if ((("".Equals(attributeURI) || Saxon.Hej.lib.NamespaceConstant.XSLT.Equals(attributeURI)) || Saxon.Hej.lib.NamespaceConstant.SAXON.Equals(attributeURI))) {
                                        checkUnknownAttribute(attName);
                                    } else {
                                        string name = ((('{'+attributeURI)+'}')+attName.getLocalPart());
                                        if ((userAttributes == null)) {
                                            userAttributes = new System.Collections.Generic.Dictionary<string, string>(5);
                                        }
                                        Saxon.Impl.Helpers.MapUtils.Put(userAttributes, name, value);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if ((nameAtt != null)) {
                outputFormatName = makeQName(nameAtt, "XTSE1570", "name");
            }
        }
        public Saxon.Hej.om.StructuredQName getFormatQName() {
            return outputFormatName;
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            checkTopLevel("XTSE0010", false);
            checkEmpty();
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
        }
        protected internal override void processVersionAttribute(string ns) {
            version = ((Saxon.Hej.style.StyleElement)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()))).getEffectiveVersion();
        }
        protected internal void gatherOutputProperties(Saxon.Ejava.util.Properties details, System.Collections.Generic.Dictionary<string,int> precedences, int thisPrecedence) {
            Saxon.Hej.lib.SerializerFactory sf = getConfiguration().getSerializerFactory();
            if ((method != null)) {
                if (((((("xml".Equals(method) || "html".Equals(method)) || "text".Equals(method)) || "xhtml".Equals(method)) || "json".Equals(method)) || "adaptive".Equals(method))) {
                    checkAndPut(sf, Saxon.Ejavax.xml.transform.OutputKeys.METHOD, method, details, precedences, thisPrecedence);
                } else {
                    string[] parts;
                    try {
                        parts = Saxon.Hej.om.NameChecker.getQNameParts(method);
                        string prefix = parts[0];
                        if (prefix.Length==0) {
                            compileError("method must be xml, html, xhtml, text, json, adaptive, or a prefixed name", "XTSE1570");
                        } else {
                            string uri = getURIForPrefix(prefix, false);
                            if ((uri == null)) {
                                undeclaredNamespaceError(prefix, "XTSE0280", "method");
                            }
                            checkAndPut(sf, Saxon.Ejavax.xml.transform.OutputKeys.METHOD, ((('{'+uri)+'}')+parts[1]), details, precedences, thisPrecedence);
                        }
                    } catch (Saxon.Hej.om.QNameException e) {
                        compileError(("Invalid method name. "+e.Message), "XTSE1570");
                    }
                }
            }
            foreach (System.Collections.Generic.KeyValuePair<string,string> entry in serializationAttributes) {
                checkAndPut(sf, entry.Key, entry.Value, details, precedences, thisPrecedence);
            }
            if (serializationAttributes.ContainsKey(Saxon.Hej.lib.SaxonOutputKeys.NEXT_IN_CHAIN)) {
                checkAndPut(sf, Saxon.Hej.lib.SaxonOutputKeys.NEXT_IN_CHAIN_BASE_URI, getSystemId(), details, precedences, thisPrecedence);
            }
            if ((useCharacterMaps != null)) {
                string s = prepareCharacterMaps(this, useCharacterMaps, details);
                details.setProperty(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS, s);
            }
            if ((userAttributes != null)) {
                foreach (System.Collections.Generic.KeyValuePair<string,string> e in userAttributes) {
                    details.setProperty(e.Key, e.Value);
                }
            }
        }
        private void checkAndPut(Saxon.Hej.lib.SerializerFactory sf, string property, string value, Saxon.Ejava.util.Properties props, System.Collections.Generic.Dictionary<string,int> precedences, int thisPrecedence) {
            try {
                if (isListOfNames(property)) {
                    bool useDefaultNS = !(property.Equals(Saxon.Hej.lib.SaxonOutputKeys.ATTRIBUTE_ORDER));
                    bool allowStar = property.Equals(Saxon.Hej.lib.SaxonOutputKeys.ATTRIBUTE_ORDER);
                    value = Saxon.Hej.lib.SaxonOutputKeys.parseListOfNodeNames(value, this, useDefaultNS, false, allowStar, "XTSE0280");
                }
                if ((isQName(property) && value.Contains(":"))) {
                    value = Saxon.Hej.functions.ResolveQName.resolveQName(value, this).getEQName();
                }
                value = sf.checkOutputProperty(property, value);
            } catch (Saxon.Hej.trans.XPathException err) {
                string code = (property.Equals("method") ? "XTSE1570" : "XTSE0020");
                if (property.Contains("{")) {
                    compileError(err.Message, code);
                } else {
                    compileErrorInAttribute(err.Message, code, property);
                }
                return ;
            }
            string old = props.getProperty(property);
            if ((old == null)) {
                props.setProperty(property, value);
                Saxon.Impl.Helpers.MapUtils.Put(precedences, property, thisPrecedence);
            } else {
                if (old.Equals(value)) {
                } else {
                    if (isListOfNames(property)) {
                        props.setProperty(property, ((old+" ")+value));
                        Saxon.Impl.Helpers.MapUtils.Put(precedences, property, thisPrecedence);
                    } else {
                        int oldPrec = Saxon.Impl.Helpers.MapUtils.GetOrDefault(precedences, property, System.Int32.MinValue);
                        if ((oldPrec == System.Int32.MinValue)) {
                            return ;
                        }
                        if ((oldPrec>thisPrecedence)) {
                        } else {
                            if ((oldPrec == thisPrecedence)) {
                                compileError(("Conflicting values for output property "+property), "XTSE1560");
                            } else {
                                throw new Saxon.Ejava.lang.IllegalStateException("Output properties must be processed in decreasing precedence order");
                            }
                        }
                    }
                }
            }
        }
        private static bool isListOfNames(string property) {
            return (((property.Equals(Saxon.Ejavax.xml.transform.OutputKeys.CDATA_SECTION_ELEMENTS) || property.Equals(Saxon.Hej.lib.SaxonOutputKeys.SUPPRESS_INDENTATION)) || property.Equals(Saxon.Hej.lib.SaxonOutputKeys.ATTRIBUTE_ORDER)) || property.Equals(Saxon.Hej.lib.SaxonOutputKeys.DOUBLE_SPACE));
        }
        private static bool isQName(string property) {
            return (property.Equals(Saxon.Ejavax.xml.transform.OutputKeys.METHOD) || property.Equals(Saxon.Hej.lib.SaxonOutputKeys.JSON_NODE_OUTPUT_METHOD));
        }
        public static string prepareCharacterMaps(Saxon.Hej.style.StyleElement element, string useCharacterMaps, Saxon.Ejava.util.Properties details) {
            Saxon.Hej.style.PrincipalStylesheetModule psm = element.getPrincipalStylesheetModule();
            string existing = details.getProperty(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS);
            if ((existing == null)) {
                existing = "";
            }
            Saxon.Ejava.lang.StringBuilder s = new Saxon.Ejava.lang.StringBuilder();
            Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(useCharacterMaps, " \t\n\r", false);
            while (st.hasMoreTokens()) {
                string displayname = st.nextToken();
                Saxon.Hej.om.StructuredQName qName = element.makeQName(displayname, null, "use-character-maps");
                Saxon.Hej.style.ComponentDeclaration decl = psm.getCharacterMap(qName);
                if ((decl == null)) {
                    element.compileErrorInAttribute((("No character-map named '"+displayname)+"' has been defined"), "XTSE1590", "use-character-maps");
                }
                s.append(" ").append(qName.getClarkName());
            }
            existing = (s+existing);
            return existing;
        }
    
    }
}