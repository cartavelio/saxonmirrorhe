using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class AccumulatorFn : Saxon.Hej.functions.SystemFunction {
        public enum Phase {
            AFTER,
            BEFORE,
            UNSPECIFIED,
        }
        public abstract Saxon.Hej.functions.AccumulatorFn.Phase getPhase();
        private Saxon.Hej.om.Sequence getAccumulatorValue(string name, Saxon.Hej.functions.AccumulatorFn.Phase phase, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.accum.AccumulatorRegistry registry = getRetainedStaticContext().getPackageData().getAccumulatorRegistry();
            Saxon.Hej.expr.accum.Accumulator accumulator = getAccumulator(name, registry);
            Saxon.Hej.om.Item node = context.getContextItem();
            if ((node == null)) {
                throw new Saxon.Hej.trans.XPathException("No context item for evaluation of accumulator function", "XTDE3350", context);
            }
            if (!(((node is Saxon.Hej.om.NodeInfo)))) {
                throw new Saxon.Hej.trans.XPathException("Context item for evaluation of accumulator function must be a node", "XTTE3360", context);
            }
            int kind = ((Saxon.Hej.om.NodeInfo)(node)).getNodeKind();
            if (((kind == Saxon.Hej.type.Type.ATTRIBUTE) || (kind == Saxon.Hej.type.Type.NAMESPACE))) {
                throw new Saxon.Hej.trans.XPathException("Context item for evaluation of accumulator function must not be an attribute or namespace node", "XTTE3360", context);
            }
            Saxon.Hej.om.Sequence streamedAccVal = registry.getStreamingAccumulatorValue((Saxon.Hej.om.NodeInfo)(node), accumulator, phase);
            if ((streamedAccVal != null)) {
                return streamedAccVal;
            }
            Saxon.Hej.om.TreeInfo root = ((Saxon.Hej.om.NodeInfo)(node)).getTreeInfo();
            Saxon.Hej.trans.XsltController controller = (Saxon.Hej.trans.XsltController)(context.getController());
            if ((!(accumulator.isUniversallyApplicable()) && !(controller.getAccumulatorManager().isApplicable(root, accumulator)))) {
                throw new Saxon.Hej.trans.XPathException((("Accumulator "+name)+" is not applicable to the current document"), "XTDE3362");
            }
            Saxon.Hej.expr.accum.AccumulatorManager manager = controller.getAccumulatorManager();
            Saxon.Hej.expr.accum.IAccumulatorData data = manager.getAccumulatorData(root, accumulator, context);
            return data.getValue((Saxon.Hej.om.NodeInfo)(node), (phase == Saxon.Hej.functions.AccumulatorFn.Phase.AFTER));
        }
        private Saxon.Hej.expr.accum.Accumulator getAccumulator(string name, Saxon.Hej.expr.accum.AccumulatorRegistry registry) {
            Saxon.Hej.om.StructuredQName qName;
            try {
                qName = Saxon.Hej.om.StructuredQName.fromLexicalQName(name, false, true, getRetainedStaticContext());
            } catch (Saxon.Hej.trans.XPathException err) {
                throw new Saxon.Hej.trans.XPathException(("Invalid accumulator name: "+err.Message), "XTDE3340");
            }
            Saxon.Hej.expr.accum.Accumulator accumulator = ((registry == null) ? null : registry.getAccumulator(qName));
            if ((accumulator == null)) {
                throw new Saxon.Hej.trans.XPathException((("Accumulator "+name)+" has not been declared"), "XTDE3340");
            }
            return accumulator;
        }
        public override Saxon.Hej.type.ItemType getResultItemType(Saxon.Hej.expr.Expression[] args) {
            try {
                if ((args[0] is Saxon.Hej.expr.StringLiteral)) {
                    Saxon.Hej.expr.accum.AccumulatorRegistry registry = getRetainedStaticContext().getPackageData().getAccumulatorRegistry();
                    Saxon.Hej.expr.accum.Accumulator accumulator = getAccumulator(((Saxon.Hej.expr.StringLiteral)(args[0])).stringify(), registry);
                    return accumulator.getType().getPrimaryType();
                }
            } catch (System.Exception e) {
            }
            return base.getResultItemType(args);
        }
        public override int getCardinality(Saxon.Hej.expr.Expression[] args) {
            try {
                if ((args[0] is Saxon.Hej.expr.StringLiteral)) {
                    Saxon.Hej.expr.accum.AccumulatorRegistry registry = getRetainedStaticContext().getPackageData().getAccumulatorRegistry();
                    Saxon.Hej.expr.accum.Accumulator accumulator = getAccumulator(((Saxon.Hej.expr.StringLiteral)(args[0])).stringify(), registry);
                    return accumulator.getType().getCardinality();
                }
            } catch (System.Exception e) {
            }
            return base.getCardinality(args);
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string name = arguments[0].head().getStringValue();
            return getAccumulatorValue(name, getPhase(), context);
        }
        public class AccumulatorBefore : Saxon.Hej.functions.AccumulatorFn {
            public AccumulatorBefore()  {
            }
            public override Saxon.Hej.functions.AccumulatorFn.Phase getPhase() {
                return Saxon.Hej.functions.AccumulatorFn.Phase.BEFORE;
            }
        
        }
        public class AccumulatorAfter : Saxon.Hej.functions.AccumulatorFn {
            public override Saxon.Hej.functions.AccumulatorFn.Phase getPhase() {
                return Saxon.Hej.functions.AccumulatorFn.Phase.AFTER;
            }
            public override string getStreamerName() {
                return "AccumulatorAfter";
            }
        
        }
    
    }
}