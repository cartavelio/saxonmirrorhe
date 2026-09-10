using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class CurrentGroupingKeyCall : Saxon.Hej.expr.Expression, Saxon.Hej.expr.Callable {
        public override Saxon.Hej.expr.Expression getScopingExpression() {
            return Saxon.Hej.functions.CurrentGroupCall.findControllingInstruction(this);
        }
        protected internal override int computeCardinality() {
            return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_MORE;
        }
        public override int getImplementationMethod() {
            return ITERATE_METHOD;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.BuiltInAtomicType.ANY_ATOMIC;
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("currentGroupingKey");
            @out.endElement();
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return new Saxon.Hej.functions.CurrentGroupingKeyCall();
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CURRENT_GROUP;
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext c) {
            Saxon.Hej.expr.sort.GroupIterator gi = c.getCurrentGroupIterator();
            Saxon.Hej.om.AtomicSequence result = ((gi == null) ? null : gi.getCurrentGroupingKey());
            if ((result == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("There is no current grouping key", "XTDE1071");
                err.setLocation(getLocation());
                throw err;
            }
            return ((/*1430*/Saxon.Hej.tree.iter.AtomicIterator)result.iterate());
        }
        public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.om.SequenceTool.toLazySequence(iterate(context));
        }
    
    }
}