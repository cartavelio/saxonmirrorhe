using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    #pragma warning disable 659
    public class StructuredQName : Saxon.Hej.om.IdentityComparable {
        private readonly char[] content;
        private readonly int localNameStart;
        private readonly int prefixStart;
        private int cachedHashCode = -1;
        private StructuredQName(char[] content, int localNameStart, int prefixStart)  {
            this.content = content;
            this.localNameStart = localNameStart;
            this.prefixStart = prefixStart;
        }
        public StructuredQName(string prefix, string uri, string localName)  {
            if ((uri == null)) {
                uri = "";
            }
            int plen = prefix.Length;
            int ulen = uri.Length;
            int llen = localName.Length;
            localNameStart = ulen;
            prefixStart = (ulen+llen);
            content = new char[((ulen+llen)+plen)];
            Saxon.Impl.Helpers.StringUtils.stringToCharArray(uri, 0, ulen, content, 0);
            Saxon.Impl.Helpers.StringUtils.stringToCharArray(localName, 0, llen, content, ulen);
            Saxon.Impl.Helpers.StringUtils.stringToCharArray(prefix, 0, plen, content, (ulen+llen));
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
            return new Saxon.Hej.om.StructuredQName("", @namespace, localName);
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
                return new Saxon.Hej.om.StructuredQName("", uri, local);
            }
            try {
                string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(lexicalName);
                string uri = resolver.getURIForPrefix(parts[0], useDefault);
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
                return new Saxon.Hej.om.StructuredQName("", uri, local);
            } else {
                return new Saxon.Hej.om.StructuredQName("", "", eqName);
            }
        }
        public string getPrefix() {
            return new string(content, prefixStart, (content.Length-prefixStart));
        }
        public string getURI() {
            if ((localNameStart == 0)) {
                return "";
            }
            return new string(content, 0, localNameStart);
        }
        public bool hasURI(string uri) {
            if ((localNameStart != uri.Length)) {
                return false;
            }
            for (int i = (localNameStart-1); (i>=0); i--) {
                if ((content[i] != uri[i])) {
                    return false;
                }
            }
            return true;
        }
        public string getLocalPart() {
            return new string(content, localNameStart, (prefixStart-localNameStart));
        }
        public string getDisplayName() {
            if ((prefixStart == content.Length)) {
                return getLocalPart();
            } else {
                Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(((content.Length-localNameStart)+1));
                buff.append(content, prefixStart, (content.Length-prefixStart));
                buff.append(':');
                buff.append(content, localNameStart, (prefixStart-localNameStart));
                return buff.ToString();
            }
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return this;
        }
        public string getClarkName() {
            Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(((content.Length-prefixStart)+2));
            if ((localNameStart>0)) {
                buff.append('{');
                buff.append(content, 0, localNameStart);
                buff.append('}');
            }
            buff.append(content, localNameStart, (prefixStart-localNameStart));
            return buff.ToString();
        }
        public string getEQName() {
            Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(((content.Length-prefixStart)+2));
            buff.append("Q{");
            if ((localNameStart>0)) {
                buff.append(content, 0, localNameStart);
            }
            buff.append('}');
            buff.append(content, localNameStart, (prefixStart-localNameStart));
            return buff.ToString();
        }
        public override string ToString() {
            return getDisplayName();
        }
        public override bool Equals(System.Object other) {
            if ((this == other)) {
                return true;
            }
            if ((other is Saxon.Hej.om.StructuredQName)) {
                int c = ((Saxon.Hej.om.StructuredQName)(other)).cachedHashCode;
                if (((c != -1) && (c != GetHashCode()))) {
                    return false;
                }
                Saxon.Hej.om.StructuredQName sq2 = (Saxon.Hej.om.StructuredQName)(other);
                if (((localNameStart != sq2.localNameStart) || (prefixStart != sq2.prefixStart))) {
                    return false;
                }
                for (int i = (prefixStart-1); (i>=0); i--) {
                    if ((content[i] != sq2.content[i])) {
                        return false;
                    }
                }
                return true;
            } else {
                return false;
            }
        }
        public override int GetHashCode() {
            if ((cachedHashCode == -1)) {
                int h = 0x5004a00b;
                h ^= prefixStart;
                h ^= localNameStart;
                for (int i = localNameStart; (i<prefixStart); i++) {
                    h ^= (content[i]<<((i&0x1f)));
                }
                return cachedHashCode = h;
            } else {
                return cachedHashCode;
            }
        }
        public static int computeHashCode(string uri, string local) {
            int h = 0x5004a00b;
            int localLen = local.Length;
            int uriLen = uri.Length;
            int totalLen = (localLen+uriLen);
            h ^= totalLen;
            h ^= uriLen;
            for (int i = 0, j = uriLen; (i<localLen); i++, j++) {
                h ^= (local[i]<<((j&0x1f)));
            }
            return h;
        }
        public Saxon.Hej.om.NamespaceBinding getNamespaceBinding() {
            return Saxon.Hej.om.NamespaceBinding.makeNamespaceBinding(getPrefix(), getURI());
        }
        public bool isIdentical(Saxon.Hej.om.IdentityComparable other) {
            return (Equals(other) && ((Saxon.Hej.om.StructuredQName)(other)).getPrefix().Equals(getPrefix()));
        }
        public int identityHashCode() {
            return (GetHashCode()^getPrefix().GetHashCode());
        }
    
    }
}