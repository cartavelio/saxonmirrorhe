using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.arrays {
    public abstract class ArrayItem : Saxon.Hej.om.FunctionItem {
        public bool isArray() {
            return true;
        }
        public bool isMap() {
            return false;
        }
        public abstract Saxon.Hej.om.GroundedValue get(int index);
        public abstract Saxon.Hej.ma.arrays.ArrayItem put(int index, Saxon.Hej.om.GroundedValue newValue);
        public abstract int arrayLength();
        public virtual bool isEmpty() {
            return (arrayLength() == 0);
        }
        public abstract System.Collections.Generic.IEnumerable<Saxon.Hej.om.GroundedValue> members();
        public virtual Saxon.Hej.om.SequenceIterator parcels() {
            return new Saxon.Hej.tree.iter.SequenceIteratorOverJavaIterator<Saxon.Hej.om.GroundedValue>(members().GetEnumerator(), member => new Saxon.Hej.ma.Parcel(member));
        }
        public abstract Saxon.Hej.ma.arrays.ArrayItem append(Saxon.Hej.om.GroundedValue newMember);
        public abstract Saxon.Hej.ma.arrays.ArrayItem concat(Saxon.Hej.ma.arrays.ArrayItem other);
        public abstract Saxon.Hej.ma.arrays.ArrayItem remove(int index);
        public abstract Saxon.Hej.ma.arrays.ArrayItem removeSeveral(Saxon.Hej.z.IntSet positions);
        public abstract Saxon.Hej.ma.arrays.ArrayItem subArray(int start, int end);
        public abstract Saxon.Hej.ma.arrays.ArrayItem insert(int position, Saxon.Hej.om.GroundedValue member);
        public abstract Saxon.Hej.value.SequenceType getMemberType(Saxon.Hej.type.TypeHierarchy th);
        public virtual string toShortString() {
            Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder();
            sb.append("array{");
            int count = 0;
            foreach (Saxon.Hej.om.GroundedValue member in members()) {
                if ((count++>2)) {
                    sb.append(" ...");
                    break ;
                }
                sb.append(member.toShortString());
                sb.append(", ");
            }
            sb.append("}");
            return sb.ToString();
        }
        public Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.ARRAY;
        }
    
        /* Expanding default method isSequenceVariadic from interface FunctionItem */
        public virtual bool isSequenceVariadic() {
            return false;
        }

        /* Expanding default method effectiveBooleanValue from interface GroundedValue */
        public virtual bool effectiveBooleanValue() {
            return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(iterate());
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

        /* Expanding default method head from interface Item */
        public virtual Saxon.Hej.om.Item head() {
            return this;
        }

        /* Expanding default method getStringValue from interface Item */
        public virtual string getStringValue() {
            return getUnicodeStringValue().ToString();
        }

        /* Expanding default method itemAt from interface Item */
        public virtual Saxon.Hej.om.Item itemAt(int n) {
            return ((n == 0) ? head() : null);
        }

        /* Expanding default method subsequence from interface Item */
        public virtual Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            if (((start<=0) && (((start+length))>0))) {
                return this;
            } else {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
        }

        /* Expanding default method getLength from interface Item */
        public virtual int getLength() {
            return 1;
        }

        /* Expanding default method iterate from interface Item */
        public virtual Saxon.Hej.om.SequenceIterator iterate() {
            return (Saxon.Hej.tree.iter.SingletonIterator)(Saxon.Hej.tree.iter.SingletonIterator.makeIterator(this));
        }

        /* Expanding default method isStreamed from interface Item */
        public virtual bool isStreamed() {
            return false;
        }

        /* Creating stub method getFunctionItemType to satisfy interface FunctionItem */
        public virtual Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getFunctionName to satisfy interface FunctionItem */
        public virtual Saxon.Hej.om.StructuredQName getFunctionName() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getArity to satisfy interface FunctionItem */
        public virtual int getArity() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getOperandRoles to satisfy interface FunctionItem */
        public virtual Saxon.Hej.expr.OperandRole[] getOperandRoles() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getAnnotations to satisfy interface FunctionItem */
        public virtual Saxon.Hej.query.AnnotationList getAnnotations() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method makeNewContext to satisfy interface FunctionItem */
        public virtual Saxon.Hej.expr.XPathContext makeNewContext(Saxon.Hej.expr.XPathContext arg1, Saxon.Hej.expr.ContextOriginator arg2) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method deepEquals to satisfy interface FunctionItem */
        public virtual bool deepEquals(Saxon.Hej.om.FunctionItem arg1, Saxon.Hej.expr.XPathContext arg2, Saxon.Hej.expr.sort.AtomicComparer arg3, int arg4) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getDescription to satisfy interface FunctionItem */
        public virtual string getDescription() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method export to satisfy interface FunctionItem */
        public virtual void export(Saxon.Hej.trace.ExpressionPresenter arg1) {
            throw new System.NotImplementedException();
        }

        /* Creating stub method isTrustedResultType to satisfy interface FunctionItem */
        public virtual bool isTrustedResultType() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method getUnicodeStringValue to satisfy interface GroundedValue */
        /* (Also inherited from Item) */
        public virtual Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method atomize to satisfy interface Item */
        public virtual Saxon.Hej.om.AtomicSequence atomize() {
            throw new System.NotImplementedException();
        }

        /* Creating stub method call to satisfy interface Callable */
        public virtual Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext arg1, Saxon.Hej.om.Sequence[] arg2) {
            throw new System.NotImplementedException();
        }

    }
}