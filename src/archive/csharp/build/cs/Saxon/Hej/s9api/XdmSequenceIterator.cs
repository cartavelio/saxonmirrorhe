using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public class XdmSequenceIterator<T>  : System.Collections.Generic.IEnumerator<T> where T : Saxon.Api.XdmItem {
        private readonly Saxon.Hej.tree.iter.LookaheadIterator @base;
        private bool closed = false;
        protected internal XdmSequenceIterator(Saxon.Hej.om.SequenceIterator @base)  {
            try {
                this.@base = Saxon.Hej.tree.iter.LookaheadIteratorImpl.makeLookaheadIterator(@base);
            } catch (Saxon.Hej.trans.UncheckedXPathException uxe) {
                throw new Saxon.Api.SaxonApiException(uxe.getXPathException());
            } catch (Saxon.Hej.trans.XPathException xe) {
                throw new Saxon.Api.SaxonApiException(xe);
            }
        }
        public static System.Collections.Generic.IEnumerator<Saxon.Api.XdmNode> ofNodes(Saxon.Hej.tree.iter.AxisIterator @base) {
            return new Saxon.Hej.s9api.XdmSequenceIterator<Saxon.Api.XdmNode>(@base);
        }
        public static System.Collections.Generic.IEnumerator<Saxon.Api.XdmAtomicValue> ofAtomicValues(Saxon.Hej.om.SequenceIterator @base) {
            return new Saxon.Hej.s9api.XdmSequenceIterator<Saxon.Api.XdmAtomicValue>(@base);
        }
        protected internal static System.Collections.Generic.IEnumerator<Saxon.Api.XdmNode> ofNode(Saxon.Api.XdmNode node) {
            return new Saxon.Hej.s9api.XdmSequenceIterator<Saxon.Api.XdmNode>(Saxon.Hej.tree.iter.SingletonIterator.makeIterator(((Saxon.Hej.om.NodeInfo)node.Unwrap())));
        }
        public bool hasNext() {
            return (!(closed) && @base.hasNext());
        }
        public T next() {
            try {
                Saxon.Hej.om.Item it = @base.next();
                if ((it == null)) {
                    throw new Saxon.Ejava.util.NoSuchElementException();
                } else {
                    return (T)(Saxon.Api.XdmItem.WrapItem(it));
                }
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw new Saxon.Api.SaxonApiException(e.getXPathException());
            }
        }
        public void remove() {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }
        public void close() {
            closed = true;
            @base.close();
        }
        
    /* Add methods to implement IEnumerable */

        object System.Collections.IEnumerator.Current => Current;
        public T Current { get; private set; }
        public bool MoveNext() {
            if (hasNext()) {Current = next(); return true;} else return false;
        }
        public void Reset() {throw new NotImplementedException();}
        public void Dispose() {}
      
    }
}