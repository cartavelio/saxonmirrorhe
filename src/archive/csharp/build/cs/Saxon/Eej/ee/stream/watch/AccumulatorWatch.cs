using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.watch {
    public class AccumulatorWatch : Saxon.Eej.ee.stream.watch.PatternWatch {
        private readonly Saxon.Hej.expr.accum.Accumulator accumulator;
        private readonly Saxon.Hej.trans.SimpleMode preDescentMode;
        private readonly Saxon.Hej.trans.SimpleMode postDescentMode;
        private readonly Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.om.Sequence> preDescentStack = new Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.om.Sequence>();
        private readonly System.Collections.Generic.Stack<Saxon.Hej.@event.ComplexContentOutputter> captureStack = new System.Collections.Generic.Stack<Saxon.Hej.@event.ComplexContentOutputter>();
        private Saxon.Hej.om.Sequence currentValue;
        private Saxon.Eej.ee.stream.om.FleetingNode lastPredescentNode = null;
        private Saxon.Eej.ee.stream.om.FleetingNode lastPostdescentNode = null;
        private Saxon.Hej.functions.AccumulatorFn.Phase beingComputed = Saxon.Hej.functions.AccumulatorFn.Phase.UNSPECIFIED;
        private Saxon.Hej.expr.XPathContextMajor context;
        private Saxon.Eej.ee.stream.watch.WatchManager watchManager;
        private System.Collections.Generic.Stack<Saxon.Eej.ee.stream.om.FleetingNode> nodeStack;
        public AccumulatorWatch(Saxon.Hej.expr.accum.Accumulator accumulator)  {
            this.accumulator = accumulator;
            this.preDescentMode = accumulator.getPreDescentRules();
            this.postDescentMode = accumulator.getPostDescentRules();
            setSelection(new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.pattern.AnyNodeTest.getInstance()));
        }
        public Saxon.Hej.expr.accum.Accumulator getAccumulator() {
            return accumulator;
        }
        public Saxon.Hej.expr.XPathContext getContext() {
            return context;
        }
        public override void open(Saxon.Eej.ee.stream.watch.Terminator terminator) {
            context = watchManager.getXPathContext().newCleanContext();
            context.setCurrentComponent(accumulator.getDeclaringComponent());
            Saxon.Hej.expr.instruct.SlotManager slotManager = accumulator.getSlotManagerForInitialValueExpression();
            context.setStackFrame(slotManager, Saxon.Hej.om.SequenceTool.makeSequenceArray(slotManager.getNumberOfVariables()));
            context.setCurrentIterator(new Saxon.Hej.tree.iter.ManualIterator());
            try {
                currentValue = Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluate(accumulator.getInitialValueExpression(), getContext());
            } catch (Saxon.Hej.trans.XPathException err) {
                currentValue = new Saxon.Hej.value.ObjectValue<Saxon.Hej.trans.XPathException>(err);
            }
        }
        public void setNodeStack(System.Collections.Generic.Stack<Saxon.Eej.ee.stream.om.FleetingNode> stack) {
            this.nodeStack = stack;
        }
        public void setWatchManager(Saxon.Eej.ee.stream.watch.WatchManager watchManager) {
            this.watchManager = watchManager;
        }
        public override Saxon.Hej.@event.Receiver startSelectedParentNode(Saxon.Eej.ee.stream.om.FleetingParentNode node, Saxon.Hej.s9api.Location locationId) {
            int nodeNr = node.getNodeNumber();
            if (((lastPredescentNode != null) && (nodeNr == lastPredescentNode.getNodeNumber()))) {
                return null;
            }
            lastPredescentNode = node;
            Saxon.Hej.tree.iter.ManualIterator iter = (Saxon.Hej.tree.iter.ManualIterator)(context.getCurrentIterator());
            iter.setContextItem(node);
            iter.incrementPosition();
            Saxon.Hej.trans.rules.Rule rule = preDescentMode.getRule(node, context);
            Saxon.Hej.om.Sequence value = currentValue;
            if ((rule != null)) {
                beingComputed = Saxon.Hej.functions.AccumulatorFn.Phase.BEFORE;
                value = applyRule(node, rule);
                beingComputed = Saxon.Hej.functions.AccumulatorFn.Phase.UNSPECIFIED;
                currentValue = value;
                if (accumulator.isTracing()) {
                    context.getConfiguration().getLogger().info(((((accumulator.getAccumulatorName().getDisplayName()+" BEFORE ")+Saxon.Hej.tree.util.Navigator.getPath(node))+": ")+Saxon.Hej.trans.Err.depictSequence(value)));
                }
            }
            Saxon.Hej.trans.rules.Rule capture = postDescentMode.getRule(node, context);
            if (((capture != null) && ((Saxon.Hej.expr.accum.AccumulatorRule)(capture.getAction())).isCapturing())) {
                Saxon.Hej.@event.SequenceCollector so = new Saxon.Hej.@event.SequenceCollector(getPipelineConfiguration());
                Saxon.Hej.@event.ComplexContentOutputter cco = new Saxon.Hej.@event.ComplexContentOutputter(so);
                Saxon.Eej.ee.stream.feed.ItemFeed feed = new Saxon.Eej.ee.stream.feed.DecomposingFeed(cco, context);
                Saxon.Eej.ee.stream.watch.Watch snapshotWatch = /*134*/Saxon.Eej.ee.stream.feed.SnapshotFeed.getWatch(feed, Saxon.Hej.pattern.AnchorPattern.getInstance(), watchManager, context);
                watchManager.addWatch(snapshotWatch, true);
                captureStack.Push(cco);
            }
            int depth = nodeStack.Count;
            setAtDepth(preDescentStack, (depth-1), value);
            return null;
        }
        private static bool valueIsException(Saxon.Hej.om.Sequence value) {
            return ((value is Saxon.Hej.value.AnyExternalObject) && (((Saxon.Hej.value.AnyExternalObject)(value)).getWrappedObject() is Saxon.Hej.trans.XPathException));
        }
        private static void detectErrorValue(Saxon.Hej.om.Sequence value) {
            if (valueIsException(value)) {
                throw (Saxon.Hej.trans.XPathException)(((Saxon.Hej.value.AnyExternalObject)(value)).getWrappedObject());
            }
        }
        private Saxon.Hej.om.Sequence applyRule(Saxon.Eej.ee.stream.om.FleetingNode node, Saxon.Hej.trans.rules.Rule rule) {
            if (valueIsException(currentValue)) {
                return currentValue;
            }
            Saxon.Hej.expr.accum.AccumulatorRule target = (Saxon.Hej.expr.accum.AccumulatorRule)(rule.getAction());
            Saxon.Hej.expr.Expression delta = target.getNewValueExpression();
            Saxon.Hej.expr.XPathContextMajor c2 = context.newCleanContext();
            Saxon.Hej.Controller controller = c2.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.om.NodeInfo targetNode = node;
            if ((target.isPostDescent() && target.isCapturing())) {
                Saxon.Hej.@event.ComplexContentOutputter cco = captureStack.Pop();
                Saxon.Hej.@event.SequenceCollector so = (Saxon.Hej.@event.SequenceCollector)(cco.getReceiver());
                targetNode = (Saxon.Hej.om.NodeInfo)(so.getFirstItem());
            }
            Saxon.Hej.tree.iter.ManualIterator initialNode = new Saxon.Hej.tree.iter.ManualIterator(targetNode);
            c2.setCurrentIterator(initialNode);
            c2.openStackFrame(target.getStackFrameMap());
            c2.setLocalVariable(0, currentValue);
            c2.setCurrentComponent(accumulator.getDeclaringComponent());
            c2.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_ACCUMULATOR_RULE);
            try {
                return Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluate(delta, c2);
            } catch (Saxon.Hej.trans.XPathException e) {
                return new Saxon.Hej.value.ObjectValue<Saxon.Hej.trans.XPathException>(e);
            }
        }
        private static void setAtDepth(Saxon.Hej.tree.util.IndexedStack<Saxon.Hej.om.Sequence> stack, int depth, Saxon.Hej.om.Sequence value) {
            if ((depth>=stack.size())) {
                stack.push(value);
            } else {
                stack.set(depth, value);
            }
        }
        public bool isBeingComputed(Saxon.Hej.functions.AccumulatorFn.Phase phase) {
            return (beingComputed == phase);
        }
        public override void processItem(Saxon.Hej.om.Item item) {
            Saxon.Eej.ee.stream.om.FleetingNode node = (Saxon.Eej.ee.stream.om.FleetingNode)(item);
            if ((node.getNodeKind() == Saxon.Hej.type.Type.ATTRIBUTE)) {
                return ;
            }
            Saxon.Hej.tree.iter.ManualIterator iter = (Saxon.Hej.tree.iter.ManualIterator)(context.getCurrentIterator());
            iter.setContextItem(item);
            iter.incrementPosition();
            int nodeNr = node.getNodeNumber();
            if (((lastPredescentNode != null) && (nodeNr == lastPredescentNode.getNodeNumber()))) {
                return ;
            }
            lastPredescentNode = node;
            Saxon.Hej.trans.rules.Rule rule = preDescentMode.getRule(item, context);
            if ((rule != null)) {
                beingComputed = Saxon.Hej.functions.AccumulatorFn.Phase.BEFORE;
                currentValue = applyRule(node, rule);
                beingComputed = Saxon.Hej.functions.AccumulatorFn.Phase.UNSPECIFIED;
                if (accumulator.isTracing()) {
                    context.getConfiguration().getLogger().info(((((accumulator.getAccumulatorName().getDisplayName()+" BEFORE ")+Saxon.Hej.tree.util.Navigator.getPath(node))+": ")+Saxon.Hej.trans.Err.depictSequence(currentValue)));
                }
            }
            int depth = nodeStack.Count;
            setAtDepth(preDescentStack, depth, currentValue);
            rule = postDescentMode.getRule(item, context);
            if ((rule != null)) {
                beingComputed = Saxon.Hej.functions.AccumulatorFn.Phase.AFTER;
                currentValue = applyRule(node, rule);
                beingComputed = Saxon.Hej.functions.AccumulatorFn.Phase.UNSPECIFIED;
                if (accumulator.isTracing()) {
                    context.getConfiguration().getLogger().info(((((accumulator.getAccumulatorName().getDisplayName()+" AFTER ")+Saxon.Hej.tree.util.Navigator.getPath(node))+": ")+Saxon.Hej.trans.Err.depictSequence(currentValue)));
                }
            }
        }
        public override void endSelectedParentNode(Saxon.Hej.s9api.Location locationId) {
            Saxon.Eej.ee.stream.om.FleetingNode node = nodeStack.Peek();
            int nodeNr = node.getNodeNumber();
            if (((lastPostdescentNode != null) && (nodeNr == lastPostdescentNode.getNodeNumber()))) {
                return ;
            }
            lastPostdescentNode = node;
            Saxon.Hej.trans.rules.Rule rule = postDescentMode.getRule(node, context);
            if ((rule != null)) {
                beingComputed = Saxon.Hej.functions.AccumulatorFn.Phase.AFTER;
                currentValue = applyRule(node, rule);
                beingComputed = Saxon.Hej.functions.AccumulatorFn.Phase.UNSPECIFIED;
                if (accumulator.isTracing()) {
                    context.getConfiguration().getLogger().info(((((accumulator.getAccumulatorName().getDisplayName()+" AFTER ")+Saxon.Hej.tree.util.Navigator.getPath(node))+": ")+Saxon.Hej.trans.Err.depictSequence(currentValue)));
                }
            }
        }
        public override void close() {
        }
        public Saxon.Hej.om.Sequence getPreDescentValue(Saxon.Eej.ee.stream.om.FleetingNode node) {
            if (((lastPredescentNode == null) || (node.getNodeNumber()>lastPredescentNode.getNodeNumber()))) {
                if ((node is Saxon.Eej.ee.stream.om.FleetingParentNode)) {
                    startSelectedParentNode((Saxon.Eej.ee.stream.om.FleetingParentNode)(node), Saxon.Hej.expr.parser.Loc.NONE);
                } else {
                    processItem(node);
                }
            }
            detectErrorValue(currentValue);
            Saxon.Hej.om.Sequence value = preDescentStack.get(node.getDepth());
            detectErrorValue(value);
            return value;
        }
        public Saxon.Hej.om.Sequence getPreDescentValueAtDepth(int depth) {
            Saxon.Hej.om.Sequence value = preDescentStack.get(depth);
            detectErrorValue(value);
            return value;
        }
        public Saxon.Hej.om.Sequence getPostDescentValue(Saxon.Eej.ee.stream.om.FleetingNode node) {
            if ((node == null)) {
                node = nodeStack.Peek();
            }
            if (((lastPostdescentNode == null) || (node.getNodeNumber()>lastPostdescentNode.getNodeNumber()))) {
                if ((node is Saxon.Eej.ee.stream.om.FleetingParentNode)) {
                    endSelectedParentNode(Saxon.Hej.expr.parser.Loc.NONE);
                } else {
                    processItem(node);
                }
            }
            Saxon.Hej.om.Sequence value = currentValue;
            detectErrorValue(value);
            return value;
        }
    
    }
}