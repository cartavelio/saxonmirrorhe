using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public class StreamingFunctionArgumentPattern : Saxon.Hej.pattern.Pattern {
        private static readonly Saxon.Hej.pattern.StreamingFunctionArgumentPattern THE_INSTANCE = new Saxon.Hej.pattern.StreamingFunctionArgumentPattern();
        public static Saxon.Hej.pattern.StreamingFunctionArgumentPattern getInstance() {
            return THE_INSTANCE;
        }
        protected internal StreamingFunctionArgumentPattern()  {
        }
        public override Saxon.Hej.type.UType getUType() {
            return Saxon.Hej.type.UType.ANY_NODE;
        }
        public override Saxon.Hej.pattern.Pattern typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            return this;
        }
        public override bool matches(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Sequence arg = context.getStackFrame().getStackFrameValues()[0];
            Saxon.Hej.om.SequenceIterator iter = arg.iterate();
            Saxon.Hej.om.Item j;
            while (((j = iter.next()) != null)) {
                if ((j == item)) {
                    return true;
                }
            }
            return false;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.pattern.AnyNodeTest.getInstance();
        }
        public override string reconstruct() {
            return "$$";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter presenter) {
            presenter.startElement("p.streamingArg");
            presenter.endElement();
        }
        public override Saxon.Hej.pattern.Pattern copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return this;
        }
    
    }
}