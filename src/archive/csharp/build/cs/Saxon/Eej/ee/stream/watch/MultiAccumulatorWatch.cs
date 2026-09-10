using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public class MultiAccumulatorWatch : Saxon.Eej.ee.stream.watch.PatternWatch {
        private readonly System.Collections.Generic.Stack<Saxon.Eej.ee.stream.om.FleetingNode> nodeStack = new System.Collections.Generic.Stack<Saxon.Eej.ee.stream.om.FleetingNode>();
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Eej.ee.stream.watch.AccumulatorWatch> accumulatorMap;
        public MultiAccumulatorWatch(System.Collections.Generic.IDictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Eej.ee.stream.watch.AccumulatorWatch> accumulatorMap)  {
            this.accumulatorMap = accumulatorMap;
            setSelection(new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.pattern.AnyNodeTest.getInstance()));
        }
        public System.Collections.Generic.IDictionary<Saxon.Hej.expr.accum.Accumulator,Saxon.Eej.ee.stream.watch.AccumulatorWatch> getAccumulatorMap() {
            return accumulatorMap;
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            foreach (Saxon.Eej.ee.stream.watch.AccumulatorWatch watch in accumulatorMap.Values) {
                watch.open(terminator);
                watch.setNodeStack(nodeStack);
            }
        }
        public void setWatchManager(Saxon.Eej.ee.stream.watch.WatchManager watchManager) {
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            nodeStack.Push(node);
            foreach (Saxon.Eej.ee.stream.watch.AccumulatorWatch watch in accumulatorMap.Values) {
                watch.startSelectedParentNode(node, locationId);
            }
            return null;
        }
        public override void processItem(Saxon.Hej.om.Item item) {
            Saxon.Eej.ee.stream.om.FleetingNode node = (Saxon.Eej.ee.stream.om.FleetingNode)(item);
            if ((node.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE)) {
                return ;
            }
            foreach (Saxon.Eej.ee.stream.watch.AccumulatorWatch watch in accumulatorMap.Values) {
                watch.processItem(node);
            }
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            foreach (Saxon.Eej.ee.stream.watch.AccumulatorWatch watch in accumulatorMap.Values) {
                watch.endSelectedParentNode(locationId);
            }
            nodeStack.Pop();
        }
        public override void close() {
        }
    
    }
}