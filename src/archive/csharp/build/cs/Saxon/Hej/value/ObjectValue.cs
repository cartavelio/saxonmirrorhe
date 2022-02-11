using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.value {
    public class ObjectValue<T>  : Saxon.Hej.value.AnyExternalObject {
        private readonly T value;
        public ObjectValue(T @object)  {
            value = Saxon.Ejava.util.Objects.requireNonNull(@object, "External object cannot wrap a Java null");
        }
        public Saxon.Hej.om.Genre getGenre() {
            return Saxon.Hej.om.Genre.EXTERNAL;
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            return Saxon.Hej.str.StringView.of(value.ToString()).tidy();
        }
        public Saxon.Hej.om.AtomicSequence atomize() {
            return new Saxon.Hej.value.StringValue(getUnicodeStringValue().tidy());
        }
        public Saxon.Hej.type.ItemType getItemType(Saxon.Hej.type.TypeHierarchy th) {
            return Saxon.Hej.type.AnyExternalObjectType.THE_INSTANCE;
        }
        public static string displayTypeName(System.Object value) {
            return ("java-type:"+value.GetType().Name);
        }
        public bool effectiveBooleanValue() {
            return true;
        }
        public T getObject() {
            return value;
        }
        public System.Object getWrappedObject() {
            return value;
        }
        public override bool Equals(System.Object other) {
            if ((other is Saxon.Hej.value.ObjectValue<object>)) {
                System.Object o = ((Saxon.Hej.value.ObjectValue<object>)(other)).value;
                return value.Equals(o);
            } else {
                return false;
            }
        }
        public override int GetHashCode() {
            return value.GetHashCode();
        }
        public string toShortString() {
            string v = value.ToString();
            if (v.StartsWith(value.GetType().Name)) {
                return v;
            } else {
                return ((("("+value.GetType().Name)+")")+Saxon.Hej.trans.Err.truncate30(Saxon.Hej.str.StringView.tidy(value.ToString())));
            }
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
            return new Saxon.Hej.om.GroundedValueAsIterable<Saxon.Hej.om.Item>(this);
        }

        /* Expanding default method containsNode from interface GroundedValue */
        public virtual bool containsNode(Saxon.Hej.om.NodeInfo sought) {
            return Saxon.Hej.expr.SingletonIntersectExpression.containsNode(iterate(), sought);
        }

        /* Expanding default method concatenate from interface GroundedValue */
        public virtual Saxon.Hej.om.GroundedValue concatenate(params Saxon.Hej.om.GroundedValue[]  others) {
            Saxon.Hej.ma.zeno.ZenoSequence chain = new Saxon.Hej.ma.zeno.ZenoSequence();
            chain.appendSequence(this);
            foreach (Saxon.Hej.om.GroundedValue seq in others) {
                chain.appendSequence(seq);
            }
            return chain;
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
            return new Saxon.Hej.tree.iter.SingletonIterator(this);
        }

        /* Expanding default method isStreamed from interface Item */
        public virtual bool isStreamed() {
            return false;
        }

        /* Expanding default method makeRepeatable from interface Sequence */
        public virtual Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

    }
}