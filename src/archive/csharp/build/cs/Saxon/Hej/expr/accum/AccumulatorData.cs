using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.accum {
    public class AccumulatorData : Saxon.Hej.expr.accum.IAccumulatorData {
        private readonly Saxon.Hej.expr.accum.Accumulator accumulator;
        private readonly System.Collections.Generic.IList<Saxon.Hej.expr.accum.AccumulatorData.DataPoint> values = new System.Collections.Generic.List<Saxon.Hej.expr.accum.AccumulatorData.DataPoint>();
        private bool building = false;
        public AccumulatorData(Saxon.Hej.expr.accum.Accumulator acc)  {
            this.accumulator = acc;
        }
        public Saxon.Hej.expr.accum.Accumulator getAccumulator() {
            return accumulator;
        }
        public void buildIndex(Saxon.Hej.om.NodeInfo doc, Saxon.Hej.expr.XPathContext context) {
            if (building) {
                throw new Saxon.Hej.trans.XPathException((("Accumulator "+accumulator.getAccumulatorName().getDisplayName())+" requires access to its own value"), "XTDE3400");
            }
            building = true;
            Saxon.Hej.expr.Expression initialValue = accumulator.getInitialValueExpression();
            Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
            Saxon.Hej.expr.instruct.SlotManager sf = accumulator.getSlotManagerForInitialValueExpression();
            Saxon.Hej.om.Sequence[] slots = new Saxon.Hej.om.Sequence[sf.getNumberOfVariables()];
            c2.setStackFrame(sf, slots);
            c2.setCurrentIterator(new Saxon.Hej.tree.iter.ManualIterator(doc));
            Saxon.Hej.om.Sequence val = Saxon.Hej.om.SequenceTool.toGroundedValue(initialValue.iterate(c2));
            values.Add(new Saxon.Hej.expr.accum.AccumulatorData.DataPoint(new Saxon.Hej.expr.accum.AccumulatorData.Visit(doc, false), val));
            val = visit(doc, val, c2);
            values.Add(new Saxon.Hej.expr.accum.AccumulatorData.DataPoint(new Saxon.Hej.expr.accum.AccumulatorData.Visit(doc, true), val));
            Saxon.Impl.Helpers.ListUtils.TrimToSize(((System.Collections.Generic.List<Saxon.Hej.expr.accum.AccumulatorData.DataPoint>)(values)));
            building = false;
        }
        private Saxon.Hej.om.Sequence visit(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.Sequence value, Saxon.Hej.expr.XPathContext context) {
            try {
                ((Saxon.Hej.tree.iter.ManualIterator)(context.getCurrentIterator())).setContextItem(node);
                Saxon.Hej.trans.rules.Rule rule = accumulator.getPreDescentRules().getRule(node, context);
                if ((rule != null)) {
                    value = processRule(rule, node, false, value, context);
                    logChange(node, value, context, " BEFORE ");
                }
                foreach (Saxon.Hej.om.NodeInfo kid in node.children()) {
                    value = visit(kid, value, context);
                }
                ((Saxon.Hej.tree.iter.ManualIterator)(context.getCurrentIterator())).setContextItem(node);
                rule = accumulator.getPostDescentRules().getRule(node, context);
                if ((rule != null)) {
                    value = processRule(rule, node, true, value, context);
                    logChange(node, value, context, " AFTER ");
                }
                return value;
            } catch (System.StackOverflowException e) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException.StackOverflow("Too many nested accumulator evaluations. The accumulator definition may have cyclic dependencies", "XTDE3400", accumulator);
                err.setXPathContext(context);
                throw err;
            }
        }
        private void logChange(Saxon.Hej.om.NodeInfo node, Saxon.Hej.om.Sequence value, Saxon.Hej.expr.XPathContext context, string phase) {
            if (accumulator.isTracing()) {
                context.getConfiguration().getLogger().info(((((accumulator.getAccumulatorName().getDisplayName()+phase)+Saxon.Hej.tree.util.Navigator.getPath(node))+": ")+Saxon.Hej.trans.Err.depictSequence(value)));
            }
        }
        private Saxon.Hej.om.Sequence processRule(Saxon.Hej.trans.rules.Rule rule, Saxon.Hej.om.NodeInfo node, bool isPostDescent, Saxon.Hej.om.Sequence value, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.accum.AccumulatorRule target = (Saxon.Hej.expr.accum.AccumulatorRule)(rule.getAction());
            Saxon.Hej.expr.Expression delta = target.getNewValueExpression();
            Saxon.Hej.expr.XPathContextMajor c2 = context.newCleanContext();
            Saxon.Hej.Controller controller = c2.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.tree.iter.ManualIterator initialNode = new Saxon.Hej.tree.iter.ManualIterator(node);
            c2.setCurrentIterator(initialNode);
            c2.openStackFrame(target.getStackFrameMap());
            c2.setLocalVariable(0, value);
            c2.setCurrentComponent(accumulator.getDeclaringComponent());
            c2.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_ACCUMULATOR_RULE);
            value = Saxon.Hej.expr.parser.ExpressionTool.eagerEvaluate(delta, c2);
            if ((((node.getParent() == null) && !(isPostDescent)) && (values.Count == 1))) {
                values.Clear();
            }
            values.Add(new Saxon.Hej.expr.accum.AccumulatorData.DataPoint(new Saxon.Hej.expr.accum.AccumulatorData.Visit(node, isPostDescent), value));
            return value;
        }
        public Saxon.Hej.om.Sequence getValue(Saxon.Hej.om.NodeInfo node, bool postDescent) {
            Saxon.Hej.expr.accum.AccumulatorData.Visit visit = new Saxon.Hej.expr.accum.AccumulatorData.Visit(node, postDescent);
            return search(0, values.Count, visit);
        }
        private Saxon.Hej.om.Sequence search(int start, int end, Saxon.Hej.expr.accum.AccumulatorData.Visit sought) {
            if ((start == end)) {
                int rel = sought.compareTo(values[start].visit);
                if ((rel<0)) {
                    return values[(start-1)].value;
                } else {
                    return values[start].value;
                }
            }
            int mid = (((start+end))/2);
            if ((sought.compareTo(values[mid].visit)<=0)) {
                return search(start, mid, sought);
            } else {
                return search((mid+1), end, sought);
            }
        }
        private class Visit : Saxon.Ejava.lang.Comparable<Saxon.Hej.expr.accum.AccumulatorData.Visit> {
            public Saxon.Hej.om.NodeInfo node;
            public bool isPostDescent;
            public Visit(Saxon.Hej.om.NodeInfo node, bool isPostDescent)  {
                this.node = node;
                this.isPostDescent = isPostDescent;
            }
            public int compareTo(Saxon.Hej.expr.accum.AccumulatorData.Visit other) {
                int relation = Saxon.Hej.tree.util.Navigator.comparePosition(node, other.node);
                switch (relation) {
                  case Saxon.Hej.om.AxisInfo.SELF:
                    if ((isPostDescent == other.isPostDescent)) {
                        return 0;
                    } else {
                        return (isPostDescent ? +(1) : -1);
                    }
                  case Saxon.Hej.om.AxisInfo.PRECEDING:
                    return -1;
                  case Saxon.Hej.om.AxisInfo.FOLLOWING:
                    return +(1);
                  case Saxon.Hej.om.AxisInfo.ANCESTOR:
                    return (isPostDescent ? +(1) : -1);
                  case Saxon.Hej.om.AxisInfo.DESCENDANT:
                    return (other.isPostDescent ? -1 : +(1));
                  default:
                    throw new Saxon.Ejava.lang.IllegalStateException();
                }
            }
        
        }
        private class DataPoint {
            public Saxon.Hej.expr.accum.AccumulatorData.Visit visit;
            public Saxon.Hej.om.Sequence value;
            public DataPoint(Saxon.Hej.expr.accum.AccumulatorData.Visit visit, Saxon.Hej.om.Sequence value)  {
                this.visit = visit;
                this.value = value;
            }
        
        }
    
    }
}