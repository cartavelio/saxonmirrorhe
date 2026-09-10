using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.accum {
    public class PathMappedAccumulatorData : Saxon.Hej.expr.accum.IAccumulatorData {
        private readonly Saxon.Hej.expr.accum.IAccumulatorData originalData;
        private readonly Saxon.Hej.om.NodeInfo origin;
        internal PathMappedAccumulatorData(Saxon.Hej.expr.accum.IAccumulatorData original, Saxon.Hej.om.NodeInfo origin)  {
            this.originalData = original;
            this.origin = origin;
        }
        public Saxon.Hej.expr.accum.Accumulator getAccumulator() {
            return null;
        }
        public Saxon.Hej.om.Sequence getValue(Saxon.Hej.om.NodeInfo node, bool postDescent) {
            return originalData.getValue(map(node), postDescent);
        }
        private Saxon.Hej.om.NodeInfo map(Saxon.Hej.om.NodeInfo node) {
            if (((origin is Saxon.Hej.tree.tiny.TinyNodeImpl) && (node is Saxon.Hej.tree.tiny.TinyNodeImpl))) {
                int nodeNrInSubtree = ((Saxon.Hej.tree.tiny.TinyNodeImpl)(node)).getNodeNumber();
                return ((/*1430*/Saxon.Hej.tree.tiny.TinyNodeImpl)((Saxon.Hej.tree.tiny.TinyNodeImpl)(origin)).getTree().getNode((nodeNrInSubtree+((Saxon.Hej.tree.tiny.TinyNodeImpl)(origin)).getNodeNumber())));
            } else {
                System.Collections.Generic.Stack<int> path = new System.Collections.Generic.Stack<int>();
                Saxon.Hej.om.NodeInfo ancestor = node;
                while ((ancestor != null)) {
                    path.Push(Saxon.Hej.tree.util.Navigator.getSiblingPosition(ancestor, Saxon.Hej.pattern.AnyNodeTest.getInstance(), System.Int32.MaxValue));
                    ancestor = ancestor.getParent();
                }
                Saxon.Hej.om.NodeInfo target = origin;
                while (!((path.Count==0))) {
                    int pos = path.Pop();
                    Saxon.Hej.tree.iter.AxisIterator kids = target.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
                    while ((pos-->0)) {
                        target = ((/*1430*/Saxon.Hej.om.NodeInfo)kids.next());
                        System.Diagnostics.Debug.Assert(((target != null)));
                    }
                }
                return target;
            }
        }
    
    }
}