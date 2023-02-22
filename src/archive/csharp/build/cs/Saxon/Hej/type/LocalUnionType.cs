using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class LocalUnionType : Saxon.Hej.type.PlainType, Saxon.Hej.type.UnionType {
        private System.Collections.Generic.IList<Saxon.Hej.type.AtomicType> memberTypes;
        public virtual Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.ATOMIC;
        }
        public virtual Saxon.Hej.om.StructuredQName getTypeName() {
            return new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.ANONYMOUS, ("U"+GetHashCode()));
        }
        public LocalUnionType(System.Collections.Generic.IList<Saxon.Hej.type.AtomicType> memberTypes)  {
            this.memberTypes = memberTypes;
        }
        public LocalUnionType(params Saxon.Hej.type.AtomicType[]  memberTypes)  {
            this.memberTypes = new System.Collections.Generic.List<Saxon.Hej.type.AtomicType>();
            Saxon.Impl.Helpers.ListUtils.AddAll(this.memberTypes, Saxon.Ejava.util.Arrays.asList(memberTypes));
        }
        public System.Collections.Generic.IList<Saxon.Hej.type.AtomicType> getMemberTypes() {
            return memberTypes;
        }
        public virtual bool isAtomicType() {
            return false;
        }
        public virtual bool containsListType() {
            return false;
        }
        public virtual bool isPlainType() {
            return true;
        }
        public virtual Saxon.Hej.value.SequenceType getResultTypeOfCast() {
            return Saxon.Hej.value.SequenceType.makeSequenceType(this, Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE);
        }
        private bool someMemberTypeSatisfies(System.Predicate<Saxon.Hej.type.AtomicType> condition) {
            foreach (Saxon.Hej.type.AtomicType member in memberTypes) {
                if (condition(member)) {
                    return true;
                }
            }
            return false;
        }
        public virtual bool isIdType() {
            return someMemberTypeSatisfies(x => x.isIdType() /* method reference */);
        }
        public virtual bool isIdRefType() {
            return someMemberTypeSatisfies(x => x.isIdRefType() /* method reference */);
        }
        public virtual bool isBuiltInType() {
            return false;
        }
        public virtual bool isListType() {
            return false;
        }
        public virtual bool isUnionType() {
            return true;
        }
        public virtual Saxon.Hej.type.UType getUType() {
            Saxon.Hej.type.UType u = Saxon.Hej.type.UType.VOID;
            foreach (Saxon.Hej.type.AtomicType at in memberTypes) {
                u = u.union(at.getUType());
            }
            return u;
        }
        public virtual string getBasicAlphaCode() {
            return "A";
        }
        public virtual bool isNamespaceSensitive() {
            return someMemberTypeSatisfies(x => x.isNamespaceSensitive() /* method reference */);
        }
        public virtual Saxon.Hej.type.ValidationFailure validateContent(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.lib.ConversionRules rules) {
            foreach (Saxon.Hej.type.AtomicType at in memberTypes) {
                Saxon.Hej.type.ValidationFailure err = at.validateContent(value, nsResolver, rules);
                if ((err == null)) {
                    return null;
                }
            }
            return new Saxon.Hej.type.ValidationFailure(((("Value "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" does not match any member of union type ")+ToString()));
        }
        public virtual Saxon.Hej.type.ValidationFailure checkAgainstFacets(Saxon.Hej.value.AtomicValue value, Saxon.Hej.lib.ConversionRules rules) {
            return null;
        }
        public virtual Saxon.Hej.om.AtomicSequence getTypedValue(Saxon.Hej.str.UnicodeString value, Saxon.Hej.om.NamespaceResolver resolver, Saxon.Hej.lib.ConversionRules rules) {
            foreach (Saxon.Hej.type.AtomicType type in memberTypes) {
                Saxon.Hej.type.StringConverter converter = rules.makeStringConverter(type);
                converter.setNamespaceResolver(resolver);
                Saxon.Hej.type.ConversionResult outcome = converter.convertString(value);
                if ((outcome is Saxon.Hej.value.AtomicValue)) {
                    return (Saxon.Hej.value.AtomicValue)(outcome);
                }
            }
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(((("Value "+Saxon.Hej.trans.Err.wrap(value, Saxon.Hej.trans.Err.VALUE))+" does not match any member of union type ")+ToString()));
            throw ve.makeException();
        }
        public virtual bool matches(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            if ((item is Saxon.Hej.value.AtomicValue)) {
                return someMemberTypeSatisfies(at => at.matches(item, th));
            } else {
                return false;
            }
        }
        public virtual Saxon.Hej.type.ItemType getPrimitiveItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
        }
        public virtual int getPrimitiveType() {
            return Saxon.Hej.om.StandardNames.XS_ANY_ATOMIC_TYPE;
        }
        public virtual Saxon.Hej.type.PlainType getAtomizedItemType() {
            return this;
        }
        public virtual bool isAtomizable(Saxon.Hej.type.TypeHierarchy th) {
            return true;
        }
        public virtual System.Collections.Generic.IList<Saxon.Hej.type.PlainType> getPlainMemberTypes() {
            return new System.Collections.Generic.List<Saxon.Hej.type.PlainType>(memberTypes);
        }
        public virtual double getDefaultPriority() {
            double result = 1;
            foreach (Saxon.Hej.type.AtomicType t in memberTypes) {
                result *= t.getDefaultPriority();
            }
            return result;
        }
        public override string ToString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            fsb.append("union(");
            foreach (Saxon.Hej.type.AtomicType at in memberTypes) {
                string member = at.getDisplayName();
                fsb.append(member);
                fsb.append(", ");
            }
            fsb.setLength((fsb.length()-2));
            fsb.append(")");
            return fsb.ToString();
        }
        public virtual string toExportString() {
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            fsb.append("union(");
            foreach (Saxon.Hej.type.AtomicType at in memberTypes) {
                fsb.append(at.toExportString());
                fsb.append(", ");
            }
            fsb.setLength((fsb.length()-2));
            fsb.append(")");
            return fsb.ToString();
        }
    
        /* Expanding default method getStructuredQName from interface UnionType */
        public virtual Saxon.Hej.om.StructuredQName getStructuredQName() {
            return getTypeName();
        }

        /* Expanding default method explainMismatch from interface ItemType */
        public virtual Saxon.Ejava.util.Optional<string> explainMismatch(Saxon.Hej.om.Item item, Saxon.Hej.type.TypeHierarchy th) {
            return Saxon.Ejava.util.Optional<string>.empty();
        }

        /* Expanding default method getDescription from interface UnionType */
        public virtual string getDescription() {
            if ((this is Saxon.Hej.type.SimpleType)) {
                return ((Saxon.Hej.type.SimpleType)(this)).getDescription();
            } else {
                return ToString();
            }
        }

        /* Expanding default method getNormalizedDefaultPriority from interface ItemType */
        public virtual double getNormalizedDefaultPriority() {
            return (((getDefaultPriority()+1))/2);
        }

        /* Expanding default method getFullAlphaCode from interface ItemType */
        public virtual string getFullAlphaCode() {
            return getBasicAlphaCode();
        }

    }
}