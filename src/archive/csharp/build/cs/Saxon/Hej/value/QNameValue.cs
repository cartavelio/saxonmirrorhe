using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class QNameValue : Saxon.Hej.value.QualifiedNameValue {
        public QNameValue(string prefix, string uri, string localName) : this (prefix, uri, localName, Saxon.Hej.type.BuiltInAtomicType.QNAME) {
        }
        public QNameValue(string prefix, string uri, string localName, Saxon.Hej.type.AtomicType type)  {
            qName = new Saxon.Hej.om.StructuredQName(prefix, uri, localName);
            if ((type == null)) {
                type = Saxon.Hej.type.BuiltInAtomicType.QNAME;
            }
            typeLabel = type;
        }
        public QNameValue(string prefix, string uri, string localName, Saxon.Hej.type.AtomicType type, bool check)  {
            if (!(Saxon.Hej.om.NameChecker.isValidNCName(localName))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Malformed local name in QName: '"+localName)+'\''));
                err.setErrorCode("FORG0001");
                throw err;
            }
            prefix = ((prefix == null) ? "" : prefix);
            uri = ("".Equals(uri) ? null : uri);
            if (((check && (uri == null)) && (prefix.Length != 0))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("QName has null namespace but non-empty prefix");
                err.setErrorCode("FOCA0002");
                throw err;
            }
            qName = new Saxon.Hej.om.StructuredQName(prefix, uri, localName);
            typeLabel = type;
        }
        public QNameValue(Saxon.Hej.om.StructuredQName qName, Saxon.Hej.type.AtomicType typeLabel)  {
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
            return new Saxon.Hej.value.QNameValue(qName, typeLabel);
        }
        public override Saxon.Hej.type.BuiltInAtomicType getPrimitiveType() {
            return Saxon.Hej.type.BuiltInAtomicType.QNAME;
        }
        public override Saxon.Hej.value.AtomicValue getComponent(Saxon.Hej.functions.AccessorFn.Component part) {
            switch (part) {
              case Saxon.Hej.functions.AccessorFn.Component.LOCALNAME:
                return new Saxon.Hej.value.StringValue(getLocalName(), Saxon.Hej.type.BuiltInAtomicType.NCNAME);
              case Saxon.Hej.functions.AccessorFn.Component.NAMESPACE:
                return new Saxon.Hej.value.AnyURIValue((getNamespaceURI()));
              case Saxon.Hej.functions.AccessorFn.Component.PREFIX:
                string prefix = getPrefix();
                if (prefix.Length==0) {
                    return null;
                } else {
                    return new Saxon.Hej.value.StringValue(prefix, Saxon.Hej.type.BuiltInAtomicType.NCNAME);
                }
              default:
                throw new Saxon.Ejava.lang.UnsupportedOperationException("Component of QName must be URI, Local Name, or Prefix");
            }
        }
        public override bool Equals(System.Object other) {
            return ((other is Saxon.Hej.value.QNameValue) && qName.Equals(((Saxon.Hej.value.QNameValue)(other)).qName));
        }
        public override int GetHashCode() {
            return qName.GetHashCode();
        }
        public override Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return null;
        }
    
    }
}