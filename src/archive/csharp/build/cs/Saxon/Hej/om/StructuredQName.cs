using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    #pragma warning disable 659
    public class StructuredQName : Saxon.Hej.om.IdentityComparable {
        private readonly string prefix;
        private readonly Saxon.Hej.om.NamespaceUri uri;
        private readonly string local;
        private int cachedHashCode = -1;
        public StructuredQName(string prefix, Saxon.Hej.om.NamespaceUri uri, string localName)  {
            this.prefix = ((prefix == null) ? "" : prefix);
            this.uri = uri;
            this.local = localName;
        }
        public StructuredQName(string prefix, string uri, string localName)  {
            this.prefix = ((prefix == null) ? "" : prefix);
            this.uri = Saxon.Hej.om.NamespaceUri.of(uri);
            this.local = localName;
        }
        public static Saxon.Hej.om.StructuredQName fromClarkName(string expandedName) {
            string @namespace;
            string localName;
            if (expandedName.StartsWith("Q{")) {
                expandedName = expandedName.Substring(1);
            }
            if ((expandedName[0] == '{')) {
                int closeBrace = expandedName.IndexOf('}');
                if ((closeBrace<0)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("No closing '}' in Clark name");
                }
                @namespace = expandedName.Substring(1, closeBrace - 1);
                if ((closeBrace == expandedName.Length)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Missing local part in Clark name");
                }
                localName = expandedName.Substring((closeBrace+1));
            } else {
                @namespace = "";
                localName = expandedName;
            }
            return new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.of(@namespace), localName);
        }
        public static Saxon.Hej.om.StructuredQName fromLexicalQName(string lexicalName, bool useDefault, bool allowEQName, Saxon.Hej.om.NamespaceResolver resolver) {
            lexicalName = Saxon.Hej.value.Whitespace.trim(lexicalName);
            if ((((allowEQName && (lexicalName.Length>=4)) && (lexicalName[0] == 'Q')) && (lexicalName[1] == '{'))) {
                string name = lexicalName.ToString();
                int endBrace = name.IndexOf('}', 2);
                if ((endBrace<0)) {
                    throw new Saxon.Hej.trans.XPathException("Invalid EQName: closing brace not found", "FOCA0002");
                } else {
                    if ((endBrace == (name.Length-1))) {
                        throw new Saxon.Hej.trans.XPathException("Invalid EQName: local part is missing", "FOCA0002");
                    }
                }
                string uri = name.Substring(2, endBrace - 2);
                if ((uri.IndexOf('{', 0)>=0)) {
                    throw new Saxon.Hej.trans.XPathException("Namespace URI must not contain '{'", "FOCA0002");
                }
                string local = name.Substring((endBrace+1), name.Length - (endBrace+1));
                if (!(Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.str.StringTool.codePoints(local)))) {
                    throw new Saxon.Hej.trans.XPathException("Invalid EQName: local part is not a valid NCName", "FOCA0002");
                }
                return new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.of(uri), local);
            }
            try {
                string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(lexicalName);
                Saxon.Hej.om.NamespaceUri uri = resolver.getURIForPrefix(parts[0], useDefault);
                if ((uri == null)) {
                    if (Saxon.Hej.om.NameChecker.isValidNCName(parts[0])) {
                        Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((("Namespace prefix '"+parts[0])+"' has not been declared"));
                        de.setErrorCode("FONS0004");
                        throw de;
                    } else {
                        Saxon.Hej.trans.XPathException de = new Saxon.Hej.trans.XPathException((("Invalid namespace prefix '"+parts[0])+"'"));
                        de.setErrorCode("FOCA0002");
                        throw de;
                    }
                }
                return new Saxon.Hej.om.StructuredQName(parts[0], uri, parts[1]);
            } catch (Saxon.Hej.om.QNameException e) {
                throw new Saxon.Hej.trans.XPathException(e.Message, "FOCA0002");
            }
        }
        public static Saxon.Hej.om.StructuredQName fromEQName(string eqName) {
            eqName = Saxon.Hej.value.Whitespace.trim(eqName);
            if (((eqName.Length>=4) && eqName.StartsWith("Q{"))) {
                int endBrace = eqName.IndexOf('}');
                if ((endBrace<0)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Invalid EQName: closing brace not found");
                } else {
                    if ((endBrace == (eqName.Length-1))) {
                        throw new Saxon.Ejava.lang.IllegalArgumentException("Invalid EQName: local part is missing");
                    }
                }
                string uri = eqName.Substring(2, endBrace - 2);
                if ((uri.IndexOf('{')>=0)) {
                    throw new Saxon.Ejava.lang.IllegalArgumentException("Invalid EQName: open brace in URI part");
                }
                string local = eqName.Substring((endBrace+1));
                return new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.of(uri), local);
            } else {
                return new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, eqName);
            }
        }
        public string getPrefix() {
            return prefix;
        }
        public Saxon.Hej.om.NamespaceUri getNamespaceUri() {
            return this.uri;
        }
        public string getURI() {
            return this.uri.ToString();
        }
        public bool hasURI(Saxon.Hej.om.NamespaceUri uri) {
            return (this.uri == uri);
        }
        public string getLocalPart() {
            return local;
        }
        public string getDisplayName() {
            if (prefix.Length==0) {
                return local;
            } else {
                return ((prefix+":")+local);
            }
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return this;
        }
        public string getClarkName() {
            if ((uri == Saxon.Hej.om.NamespaceUri.NULL)) {
                return local;
            } else {
                return ((("{"+uri)+"}")+local);
            }
        }
        public string getEQName() {
            if ((uri == Saxon.Hej.om.NamespaceUri.NULL)) {
                return ("Q{}"+local);
            } else {
                return ((("Q{"+uri)+"}")+local);
            }
        }
        public override string ToString() {
            return getDisplayName();
        }
        public override bool Equals(System.Object other) {
            if ((this == other)) {
                return true;
            }
            if ((other is Saxon.Hej.om.StructuredQName)) {
                return (local.Equals(((Saxon.Hej.om.StructuredQName)(other)).local) && (uri == ((Saxon.Hej.om.StructuredQName)(other)).uri));
            } else {
                return false;
            }
        }
        public override int GetHashCode() {
            if ((cachedHashCode == -1)) {
                return cachedHashCode = ((0x5004a00b^local.GetHashCode())^uri.GetHashCode());
            } else {
                return cachedHashCode;
            }
        }
        public static int computeHashCode(Saxon.Hej.om.NamespaceUri uri, string local) {
            return ((0x5004a00b^local.GetHashCode())^uri.GetHashCode());
        }
        public Saxon.Hej.om.NamespaceBinding getNamespaceBinding() {
            return new Saxon.Hej.om.NamespaceBinding(getPrefix(), getNamespaceUri());
        }
        public bool isIdentical(Saxon.Hej.om.IdentityComparable other) {
            return (Equals(other) && ((Saxon.Hej.om.StructuredQName)(other)).getPrefix().Equals(getPrefix()));
        }
        public int identityHashCode() {
            return (GetHashCode()^getPrefix().GetHashCode());
        }
    
    }
}