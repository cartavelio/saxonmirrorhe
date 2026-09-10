using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public abstract class QualifiedNameValue : Saxon.Hej.value.AtomicValue, Saxon.Hej.expr.sort.AtomicMatchKey {
        protected internal readonly Saxon.Hej.om.StructuredQName qName;
        public QualifiedNameValue(Saxon.Hej.om.StructuredQName qName, Saxon.Hej.type.AtomicType typeLabel) : base (typeLabel) {
            Saxon.Ejava.util.Objects.requireNonNull(qName);
            this.qName = qName;
        }
        public static Saxon.Hej.value.AtomicValue makeQName(string prefix, Saxon.Hej.om.NamespaceUri uri, string local, Saxon.Hej.type.AtomicType targetType, Saxon.Hej.str.UnicodeString lexicalForm, Saxon.Hej.lib.ConversionRules rules) {
            if ((targetType.getFingerprint() == Saxon.Hej.om.StandardNames.XS_QNAME)) {
                return new Saxon.Hej.value.QNameValue(prefix, uri, local, Saxon.Hej.type.BuiltInAtomicType.QNAME, true);
            } else {
                Saxon.Hej.value.QualifiedNameValue qnv;
                if ((targetType.getPrimitiveType() == Saxon.Hej.om.StandardNames.XS_QNAME)) {
                    qnv = new Saxon.Hej.value.QNameValue(prefix, uri, local, targetType, true);
                } else {
                    qnv = new Saxon.Hej.value.NotationValue(prefix, uri, local, targetType);
                }
                Saxon.Hej.type.ValidationFailure vf = targetType.validate(qnv, lexicalForm, rules);
                if ((vf != null)) {
                    throw vf.makeException();
                }
                return qnv;
            }
        }
        public override Saxon.Hej.str.UnicodeString getPrimitiveStringValue() {
            return Saxon.Hej.str.StringView.of(qName.getDisplayName()).tidy();
        }
        public string getClarkName() {
            return qName.getClarkName();
        }
        public string getEQName() {
            return qName.getEQName();
        }
        public string getLocalName() {
            return qName.getLocalPart();
        }
        public Saxon.Hej.om.NamespaceUri getNamespaceURI() {
            return qName.getNamespaceUri();
        }
        public string getPrefix() {
            return qName.getPrefix();
        }
        public override Saxon.Hej.expr.sort.AtomicMatchKey getXPathMatchKey(Saxon.Hej.lib.StringCollator collator, int implicitTimezone) {
            return this;
        }
        public override int GetHashCode() {
            return qName.GetHashCode();
        }
        public override bool isIdentical(Saxon.Hej.value.AtomicValue v) {
            return (base.isIdentical(v) && qName.getPrefix().Equals(((Saxon.Hej.value.QualifiedNameValue)(v)).getPrefix()));
        }
        public override int identityHashCode() {
            return qName.identityHashCode();
        }
        public override string show() {
            return (((("QName(\""+getNamespaceURI())+"\", \"")+getLocalName())+"\")");
        }
        public Saxon.Hej.om.StructuredQName getStructuredQName() {
            return qName;
        }
    
    }
}