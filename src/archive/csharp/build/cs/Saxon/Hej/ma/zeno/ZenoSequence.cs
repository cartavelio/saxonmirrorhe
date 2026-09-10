using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.ma.zeno {
    public class ZenoSequence : Saxon.Hej.om.GroundedValue {
        private Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item> chain;
        public ZenoSequence()  {
            chain = new Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item>();
        }
        public static Saxon.Hej.ma.zeno.ZenoSequence fromList(System.Collections.Generic.IList<Saxon.Hej.om.Item> items) {
            Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item> chain = new Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item>().addAll(items);
            return new Saxon.Hej.ma.zeno.ZenoSequence(chain);
        }
        private ZenoSequence(Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item> chain)  {
            this.chain = chain;
        }
        public Saxon.Hej.om.SequenceIterator iterate() {
            return new Saxon.Hej.ma.zeno.ZenoSequence.ZenoSequenceIterator(this);
        }
        public Saxon.Hej.om.Item itemAt(int n) {
            try {
                return chain.get(n);
            } catch (System.IndexOutOfRangeException e) {
                return null;
            }
        }
        public Saxon.Hej.om.Item head() {
            return (chain.isEmpty() ? null : chain.get(0));
        }
        public Saxon.Hej.om.GroundedValue subsequence(int start, int length) {
            if ((length == 1)) {
                return itemAt(start);
            }
            if ((start<0)) {
                start = 0;
            }
            return new Saxon.Hej.ma.zeno.ZenoSequence(chain.subList(start, (start+length)));
        }
        public int getLength() {
            return chain.size();
        }
        public Saxon.Hej.str.UnicodeString getUnicodeStringValue() {
            switch (getLength()) {
              case 0:
                return Saxon.Hej.str.EmptyUnicodeString.getInstance();
              case 1:
                return this.head().getUnicodeStringValue();
              default:
                Saxon.Hej.str.UnicodeBuilder builder = new Saxon.Hej.str.UnicodeBuilder();
                Saxon.Hej.str.UnicodeString separator = Saxon.Hej.str.EmptyUnicodeString.getInstance();
                foreach (Saxon.Hej.om.Item item in chain) {
                    builder.append(separator);
                    separator = Saxon.Hej.str.StringConstants.SINGLE_SPACE;
                    builder.append(item.getStringValue());
                }
                return builder.toUnicodeString();
            }
        }
        public string getStringValue() {
            switch (getLength()) {
              case 0:
                return "";
              case 1:
                return this.head().getStringValue();
              default:
                Saxon.Ejava.lang.StringBuilder builder = new Saxon.Ejava.lang.StringBuilder();
                string separator = "";
                foreach (Saxon.Hej.om.Item item in chain) {
                    builder.append(separator);
                    separator = " ";
                    builder.append(item.getStringValue());
                }
                return builder.ToString();
            }
        }
        public void append(Saxon.Hej.om.Item item) {
            chain = chain.add(item);
        }
        public void appendSequence(Saxon.Hej.om.GroundedValue items) {
            if ((items is Saxon.Hej.ma.zeno.ZenoSequence)) {
                int len = this.getLength();
                switch (len) {
                  case 0:
                    chain = ((Saxon.Hej.ma.zeno.ZenoSequence)(items)).chain;
                    return ;
                  case 1:
                    chain = ((Saxon.Hej.ma.zeno.ZenoSequence)(items)).chain.prepend(head());
                    return ;
                  default:
                    chain = chain.addAll(((Saxon.Hej.ma.zeno.ZenoSequence)(items)).chain);
                    return ;
                }
            } else {
                chain = chain.addAll(items.asIterable());
            }
        }
        public static Saxon.Hej.ma.zeno.ZenoSequence join(System.Collections.Generic.IList<Saxon.Hej.om.GroundedValue> segments) {
            Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item> list = new Saxon.Hej.ma.zeno.ZenoChain<Saxon.Hej.om.Item>();
            foreach (Saxon.Hej.om.GroundedValue val in segments) {
                if ((val is Saxon.Hej.ma.zeno.ZenoSequence)) {
                    list = list.concat(((Saxon.Hej.ma.zeno.ZenoSequence)(val)).chain);
                } else {
                    foreach (Saxon.Hej.om.Item item in val.asIterable()) {
                        list = list.add(item);
                    }
                }
            }
            return new Saxon.Hej.ma.zeno.ZenoSequence(list);
        }
        public class ZenoSequenceIterator : Saxon.Hej.tree.iter.GroundedIterator {
            private Saxon.Hej.ma.zeno.ZenoSequence sequence;
            private System.Collections.Generic.IEnumerator<Saxon.Hej.om.Item> chainIterator;
            private int position = 0;
            public ZenoSequenceIterator(Saxon.Hej.ma.zeno.ZenoSequence sequence)  {
                this.sequence = sequence;
                this.chainIterator = sequence.chain.GetEnumerator();
            }
            public Saxon.Hej.om.Item next() {
                position++;
                return (chainIterator.MoveNext() /*UNSAFE CONVERSION*/ ? chainIterator.Current : null);
            }
            public bool isActuallyGrounded() {
                return true;
            }
            public Saxon.Hej.om.GroundedValue getResidue() {
                return sequence.subsequence(position, System.Int32.MaxValue);
            }
            public Saxon.Hej.om.GroundedValue materialize() {
                return sequence;
            }
            public bool supportsHasNext() {
                return false;
            }
            public bool hasNext() {
                throw new InvalidOperationException();
            }
        
            /* Expanding default method close from interface SequenceIterator */
            public virtual void close() {
            }

        }
    
        /* Expanding default method effectiveBooleanValue from interface GroundedValue */
        public virtual bool effectiveBooleanValue() {
            return Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(iterate());
        }

        /* Expanding default method reduce from interface GroundedValue */
        public virtual Saxon.Hej.om.GroundedValue reduce() {
            return this;
        }

        /* Expanding default method materialize from interface Sequence */
        public virtual Saxon.Hej.om.GroundedValue materialize() {
            return Saxon.Hej.om.SequenceTool.toGroundedValue(iterate());
        }

        /* Expanding default method toShortString from interface GroundedValue */
        public virtual string toShortString() {
            return Saxon.Hej.trans.Err.depictSequence(this).ToString();
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

        /* Expanding default method makeRepeatable from interface Sequence */
        public virtual Saxon.Hej.om.Sequence makeRepeatable() {
            return this;
        }

    }
}