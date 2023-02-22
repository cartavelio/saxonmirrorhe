using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public abstract class AtomicValue : Saxon.Hej.om.Item, Saxon.Hej.om.AtomicSequence, Saxon.Hej.type.ConversionResult, Saxon.Hej.om.IdentityComparable {
        protected internal readonly Saxon.Hej.type.AtomicType typeLabel;
        public AtomicValue(Saxon.Hej.type.AtomicType typeLabel)  {
            Saxon.Ejava.util.Objects.requireNonNull(typeLabel);
            this.typeLabel = typeLabel;
        }
        public Saxon.Hej.om.AtomicSequence atomize() {
            return this;
        }
        public Saxon.Hej.om.Item head() {
            return this;
        }
        public int getLength() {
            return 1;
        }
        public bool isUntypedAtomic() {
            return (typeLabel == Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC);
        }
        public abstract Saxon.Hej.expr.sort.AtomicMatchKey getXPathMatchKey(Saxon.Hej.lib.StringCollator collator, int implicitTimezone);
        public abstract Saxon.Hej.expr.sort.XPathComparable getXPathComparable(Saxon.Hej.lib.StringCollator collator, int implicitTimezone);
        public virtual Saxon.Hej.expr.sort.AtomicMatchKey asMapKey() {
            try {
                return getXPathMatchKey(Saxon.Hej.expr.sort.CodepointCollator.getInstance(), Saxon.Hej.value.CalendarValue.NO_TIMEZONE);
            } catch (Saxon.Hej.trans.NoDynamicContextException e) {
                throw new Saxon.Ejava.lang.IllegalStateException("No implicit timezone available");
            }
        }
        public override bool Equals(System.Object o) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("equals() not implemented");
        }
        public override int GetHashCode() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("hashCode() not implemented");
        }
        public virtual bool isIdentical(Saxon.Hej.value.AtomicValue v) {
            return Saxon.Hej.expr.sort.SimpleTypeComparison.getInstance().equal(this, v);
        }
        public bool isIdentical(Saxon.Hej.om.IdentityComparable other) {
            return ((other is Saxon.Hej.value.AtomicValue) && isIdentical((Saxon.Hej.value.AtomicValue)(other)));
        }
        public virtual int identityHashCode() {
            return GetHashCode();
        }
        public virtual Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            Saxon.Hej.str.UnicodeString cs = getPrimitiveStringValue();
            try {
                return typeLabel.postprocess(cs);
            } catch (Saxon.Hej.trans.XPathException err) {
                return cs;
            }
        }
        public virtual Saxon.Hej.str.UnicodeString getCanonicalLexicalRepresentation() {
            return this.getUnicodeStringValue();
        }
        public Saxon.Hej.om.Item itemAt(int n) {
            return ((n == 0) ? ((/*1430*/Saxon.Hej.value.AtomicValue)head()) : null);
        }
        public Saxon.Hej.type.AtomicType getItemType() {
            return typeLabel;
        }
        public abstract Saxon.Hej.type.BuiltInAtomicType getPrimitiveType();
        public Saxon.Hej.type.UType getUType() {
            return getItemType().getUType();
        }
        public int getCardinality() {
            return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
        }
        public abstract Saxon.Hej.value.AtomicValue copyAsSubType(Saxon.Hej.type.AtomicType typeLabel);
        public virtual bool isNaN() {
            return false;
        }
        public abstract Saxon.Hej.str.UnicodeString getPrimitiveStringValue();
        public virtual bool effectiveBooleanValue() {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Effective boolean value is not defined for an atomic value of type "+Saxon.Hej.type.Type.displayTypeName(this)));
            err.setIsTypeError(true);
            err.setErrorCode("FORG0006");
            throw err;
        }
        public virtual Saxon.Hej.value.AtomicValue getComponent(Saxon.Hej.functions.AccessorFn.Component component) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException("Data type does not support component extraction");
        }
        public void checkPermittedContents(Saxon.Hej.type.SchemaType parentType, Saxon.Hej.expr.StaticContext env, bool whole) {
            if (whole) {
                Saxon.Hej.type.SimpleType stype = null;
                if ((parentType is Saxon.Hej.type.SimpleType)) {
                    stype = (Saxon.Hej.type.SimpleType)(parentType);
                } else {
                    if (((parentType is Saxon.Hej.type.ComplexType) && ((Saxon.Hej.type.ComplexType)(parentType)).isSimpleContent())) {
                        stype = ((Saxon.Hej.type.ComplexType)(parentType)).getSimpleContentType();
                    }
                }
                if (((stype != null) && !(stype.isNamespaceSensitive()))) {
                    Saxon.Hej.type.ValidationFailure err = stype.validateContent(this.getUnicodeStringValue(), null, env.getConfiguration().getConversionRules());
                    if ((err != null)) {
                        throw err.makeException();
                    }
                    return ;
                }
            }
            if (((((parentType is Saxon.Hej.type.ComplexType) && !(((Saxon.Hej.type.ComplexType)(parentType)).isSimpleContent())) && !(((Saxon.Hej.type.ComplexType)(parentType)).isMixedContent())) && !(Saxon.Hej.value.Whitespace.isAllWhite(this.getUnicodeStringValue())))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((("Complex type "+parentType.getDescription())+" does not allow text content ")+Saxon.Hej.trans.Err.wrap(this.getUnicodeStringValue())));
                err.setIsTypeError(true);
                throw err;
            }
        }
        public virtual void checkValidInJavascript() {
        }
        public virtual Saxon.Hej.value.AtomicValue asAtomic() {
            return this;
        }
        public override string ToString() {
            return getStringValue();
        }
        public virtual string toShortString() {
            return show();
        }
        public virtual string show() {
            return (((typeLabel+"(\"")+this.getUnicodeStringValue())+"\")");
        }
        public Saxon.Hej.om.SequenceIterator iterate() {
            return (Saxon.Hej.tree.iter.SingleAtomicIterator)(Saxon.Hej.tree.iter.SingleAtomicIterator.makeIterator(this));
        }
        public System.Collections.Generic.IEnumerator<Saxon.Hej.value.AtomicValue> GetEnumerator() {
            return new Saxon.Hej.tree.jiter.MonoIterator<Saxon.Hej.value.AtomicValue>(this);
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        public Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.ATOMIC;
        }
    
        /* Expanding default method reduce from interface Item */
        public virtual Saxon.Hej.om.GroundedValue reduce() {
            return this;
        }

        /* Expanding default method materialize from interface Sequence */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }

        /* Expanding default method asIterable from interface GroundedValue */
        public virtual System.Collections.Generic.IEnumerable<Saxon.Hej.om.Item> asIterable() {
            return new Saxon.Hej.om.GroundedValueAsIterable(this);
        }

        /* Expanding default method containsNode from interface GroundedValue */
        public virtual bool containsNode(Saxon.Hej.om.NodeInfo sought) {
            return Saxon.Hej.expr.SingletonIntersectExpression.containsNode(iterate(), sought);
        }

        /* Expanding default method concatenate from interface GroundedValue */
        public virtual Saxon.Hej.om.GroundedValue concatenate(params Saxon.Hej.om.GroundedValue[]  others) {
            Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item> chain = new Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item>();
            chain = chain.addAll(asIterable());
            foreach (Saxon.Hej.om.GroundedValue val in others) {
                chain = chain.addAll(val.asIterable());
            }
            return new Saxon.Hej.ma.zeno.ZenoSequence(chain);
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public virtual Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

        /* Expanding default method getStringValue from interface Item */
        public virtual string getStringValue() {
            return getUnicodeStringValue().ToString();
        }

        /* Expanding default method subsequence from interface Item */
        public virtual Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            if (((start<=0) && (((start+length))>0))) {
                return this;
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }

        /* Expanding default method isStreamed from interface Item */
        public virtual bool isStreamed() {
            return false;
        }

    }
}