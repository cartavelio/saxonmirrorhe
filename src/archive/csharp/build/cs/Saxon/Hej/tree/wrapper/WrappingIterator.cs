using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.wrapper {
    public class WrappingIterator : Saxon.Hej.tree.iter.AxisIterator {
        internal Saxon.Hej.tree.iter.AxisIterator @base;
        internal Saxon.Hej.tree.wrapper.VirtualNode parent;
        internal Saxon.Hej.om.NodeInfo _current;
        internal bool atomizing = false;
        internal Saxon.Hej.tree.wrapper.WrappingFunction wrappingFunction;
        public WrappingIterator(Saxon.Hej.tree.iter.AxisIterator @base, Saxon.Hej.tree.wrapper.WrappingFunction function, Saxon.Hej.tree.wrapper.VirtualNode parent)  {
            this.@base = @base;
            this.wrappingFunction = function;
            this.parent = parent;
        }
        public Saxon.Hej.om.Item next() {
            Saxon.Hej.om.NodeInfo n = ((/*1430*/Saxon.Hej.om.NodeInfo)@base.next());
            if ((n == null)) {
                return _current = null;
            }
            if (atomizing) {
                _current = n;
            } else {
                _current = wrappingFunction.makeWrapper(n, parent);
            }
            return _current;
        }
        public Saxon.Hej.om.NodeInfo current() {
            return _current;
        }
        public void close() {
            @base.close();
        }
        public void discharge() {
            @base.discharge();
        }
    
    }
}