using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public sealed class NotationValue : Saxon.Hej.value.QualifiedNameValue {
        public NotationValue(string prefix, string uri, string localName, bool check)  {
            if ((check && !(Saxon.Hej.om.NameChecker.isValidNCName(localName)))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Malformed local name in NOTATION: '"+localName)+'\''));
                err.setErrorCode("FORG0001");
                throw err;
            }
            prefix = ((prefix == null) ? "" : prefix);
            uri = ((uri == null) ? "" : uri);
            if (((check && uri.Length==0) && (prefix.Length != 0))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("NOTATION has null namespace but non-empty prefix");
                err.setErrorCode("FOCA0002");
                throw err;
            }
            qName = new Saxon.Hej.om.StructuredQName(prefix, uri, localName);
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.NOTATION;
        }
        public NotationValue(string prefix, string uri, string localName)  {
            qName = new Saxon.Hej.om.StructuredQName(prefix, uri, localName);
            typeLabel = Saxon.Hej.type.BuiltInAtomicType.NOTATION;
        }
        public NotationValue(string prefix, string uri, string localName, Saxon.Hej.type.AtomicType typeLabel)  {
            qName = new Saxon.Hej.om.StructuredQName(prefix, uri, localName);
            this.typeLabel = typeLabel;
        }
        public NotationValue(Saxon.Hej.om.StructuredQName qName, Saxon.Hej.type.AtomicType typeLabel)  {
            if ((qName == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("qName");
            }
            if ((typeLabel == null)) {
                throw new Saxon.Ejava.lang.NullPointerException("typeLabel");
            }
            this.qName = qName;
            this.typeLabel = typeLabel;
        }
        public override Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel) {
            Saxon.Hej.value.NotationValue v = new Saxon.Hej.value.NotationValue(getPrefix(), getNamespaceURI(), getLocalName());
            v.typeLabel = typeLabel;
            return v;
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