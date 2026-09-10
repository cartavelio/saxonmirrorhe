using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public class QNameParser {
        private Saxon.Hej.om.NamespaceResolver resolver;
        private bool acceptEQName = false;
        private string errorOnBadSyntax = "XPST0003";
        private string errorOnUnresolvedPrefix = "XPST0081";
        private Saxon.Hej.query.XQueryParser.Unescaper unescaper = null;
        public QNameParser(Saxon.Hej.om.NamespaceResolver resolver)  {
            this.resolver = resolver;
        }
        public Saxon.Hej.om.QNameParser withNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
            Saxon.Hej.om.QNameParser qp2 = copy();
            qp2.resolver = resolver;
            return qp2;
        }
        public Saxon.Hej.om.QNameParser withAcceptEQName(bool acceptEQName) {
            if ((acceptEQName == this.acceptEQName)) {
                return this;
            }
            Saxon.Hej.om.QNameParser qp2 = copy();
            qp2.acceptEQName = acceptEQName;
            return qp2;
        }
        public Saxon.Hej.om.QNameParser withErrorOnBadSyntax(string code) {
            if (code.Equals(errorOnBadSyntax)) {
                return this;
            }
            Saxon.Hej.om.QNameParser qp2 = copy();
            qp2.errorOnBadSyntax = code;
            return qp2;
        }
        public Saxon.Hej.om.QNameParser withErrorOnUnresolvedPrefix(string code) {
            if (code.Equals(errorOnUnresolvedPrefix)) {
                return this;
            }
            Saxon.Hej.om.QNameParser qp2 = copy();
            qp2.errorOnUnresolvedPrefix = code;
            return qp2;
        }
        public Saxon.Hej.om.QNameParser withUnescaper(Saxon.Hej.query.XQueryParser.Unescaper unescaper) {
            Saxon.Hej.om.QNameParser qp2 = copy();
            qp2.unescaper = unescaper;
            return qp2;
        }
        private Saxon.Hej.om.QNameParser copy() {
            Saxon.Hej.om.QNameParser qp2 = new Saxon.Hej.om.QNameParser(resolver);
            qp2.acceptEQName = acceptEQName;
            qp2.errorOnBadSyntax = errorOnBadSyntax;
            qp2.errorOnUnresolvedPrefix = errorOnUnresolvedPrefix;
            qp2.unescaper = unescaper;
            return qp2;
        }
        public Saxon.Hej.om.StructuredQName parse(string lexicalName, Saxon.Hej.om.NamespaceUri defaultNS) {
            lexicalName = Saxon.Hej.value.Whitespace.trim(lexicalName);
            if ((((acceptEQName && (lexicalName.Length>=4)) && (lexicalName[0] == 'Q')) && (lexicalName[1] == '{'))) {
                int endBrace = lexicalName.IndexOf('}');
                if ((endBrace<0)) {
                    throw new Saxon.Hej.trans.XPathException("Invalid EQName: closing brace not found", errorOnBadSyntax);
                } else {
                    if ((endBrace == (lexicalName.Length-1))) {
                        throw new Saxon.Hej.trans.XPathException("Invalid EQName: local part is missing", errorOnBadSyntax);
                    }
                }
                string uri = Saxon.Hej.value.Whitespace.collapseWhitespace(lexicalName.Substring(2, endBrace - 2));
                if (uri.Contains("{")) {
                    throw new Saxon.Hej.trans.XPathException("Invalid EQName: URI contains opening brace", errorOnBadSyntax);
                }
                if (((unescaper != null) && uri.Contains("&"))) {
                    uri = unescaper.unescape(uri).ToString();
                }
                if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.XMLNS)) {
                    throw new Saxon.Hej.trans.XPathException((("The string '"+Saxon.Hej.lib.NamespaceConstant.XMLNS)+"' cannot be used as a namespace URI"), "XQST0070");
                }
                string local = lexicalName.Substring((endBrace+1));
                checkLocalName(local);
                return new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.of(uri), local);
            }
            try {
                string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(lexicalName);
                checkLocalName(parts[1]);
                if (parts[0].Length==0) {
                    return new Saxon.Hej.om.StructuredQName("", defaultNS, parts[1]);
                }
                Saxon.Hej.om.NamespaceUri uri = resolver.getURIForPrefix(parts[0], false);
                if ((uri == null)) {
                    throw new Saxon.Hej.trans.XPathException((("Namespace prefix '"+parts[0])+"' has not been declared"), errorOnUnresolvedPrefix);
                }
                return new Saxon.Hej.om.StructuredQName(parts[0], uri, parts[1]);
            } catch (Saxon.Hej.om.QNameException e) {
                throw new Saxon.Hej.trans.XPathException(e.Message, errorOnBadSyntax);
            }
        }
        private void checkLocalName(string local) {
            if (!(Saxon.Hej.om.NameChecker.isValidNCName(local))) {
                throw new Saxon.Hej.trans.XPathException("Invalid EQName: local part is not a valid NCName", errorOnBadSyntax);
            }
        }
    
    }
}