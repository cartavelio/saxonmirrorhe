using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class NotationValue : Saxon.Hej.value.QualifiedNameValue {
        public NotationValue(string prefix, Saxon.Hej.om.NamespaceUri uri, string localName, bool check) : base (new Saxon.Hej.om.StructuredQName(prefix, uri, localName), Saxon.Hej.type.BuiltInAtomicType.NOTATION) {
            if ((check && !(Saxon.Hej.om.NameChecker.isValidNCName(localName)))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Malformed local name in NOTATION: '"+localName)+'\''));
                err.setErrorCode("FORG0001");
                throw err;
            }
            prefix = ((prefix == null) ? "" : prefix);
            if (((check && uri.isEmpty()) && (prefix.Length != 0))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("NOTATION has null namespace but non-empty prefix");
                err.setErrorCode("FOCA0002");
                throw err;
            }
        }
        public NotationValue(string prefix, string uri, string localName, bool check) : this (prefix, Saxon.Hej.om.NamespaceUri.of(uri), localName, check) {
        }
        public NotationValue(string prefix, Saxon.Hej.om.NamespaceUri uri, string localName) : base (new Saxon.Hej.om.StructuredQName(prefix, uri, localName), Saxon.Hej.type.BuiltInAtomicType.NOTATION) {
        }
        public NotationValue(string prefix, Saxon.Hej.om.NamespaceUri uri, string localName, Saxon.Hej.type.AtomicType typeLabel) : base (new Saxon.Hej.om.StructuredQName(prefix, uri, localName), typeLabel) {
        }
        public NotationValue(Saxon.Hej.om.StructuredQName qName, Saxon.Hej.type.AtomicType typeLabel) : base (qName, typeLabel) {
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            return new Saxon.Hej.value.NotationValue(getStructuredQName(), typeLabel);
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.NOTATION;
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.value.NotationValue) && qName.Equals(((Saxon.Hej.value.NotationValue)(other)).qName));
        }
        public override int GetHashCode() {
            return qName.GetHashCode();
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return null;
        }
        public override string show() {
            return (("NOTATION("+getClarkName())+')');
        }
    
    }
}