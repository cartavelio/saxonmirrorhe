using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Reverse : Saxon.Hej.functions.SystemFunction {
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            int baseProps = arguments[0].getSpecialProperties();
            if ((((baseProps&Saxon.Hej.expr.StaticProperty.REVERSE_DOCUMENT_ORDER)) != 0)) {
                return (((baseProps&~(Saxon.Hej.expr.StaticProperty.REVERSE_DOCUMENT_ORDER)))|Saxon.Hej.expr.StaticProperty.ORDERED_NODESET);
            } else {
                if ((((baseProps&Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)) != 0)) {
                    return (((baseProps&~(Saxon.Hej.expr.StaticProperty.ORDERED_NODESET)))|Saxon.Hej.expr.StaticProperty.REVERSE_DOCUMENT_ORDER);
                } else {
                    return baseProps;
                }
            }
        }
        public static Saxon.Hej.om.SequenceIterator getReverseIterator(Saxon.Hej.om.SequenceIterator forwards) {
            if ((forwards is Saxon.Hej.tree.iter.ReversibleIterator)) {
                return ((Saxon.Hej.tree.iter.ReversibleIterator)(forwards)).getReverseIterator();
            } else {
                return Saxon.Hej.value.SequenceExtent.from(forwards).reverseIterate();
            }
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.SequenceExtent input;
            if ((arguments[0] is Saxon.Hej.value.SequenceExtent)) {
                input = (Saxon.Hej.value.SequenceExtent)(arguments[0]);
            } else {
                input = Saxon.Hej.value.SequenceExtent.from(arguments[0].iterate());
            }
            return Saxon.Hej.om.SequenceTool.toLazySequence(input.reverseIterate());
        }
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            if ((arguments[0].getCardinality() == Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE)) {
                return arguments[0];
            }
            return base.makeOptimizedFunctionCall(visitor, contextInfo, arguments);
        }
        public static Saxon.Hej.om.SequenceIterator reverseIterator<T> (System.Collections.Generic.IList<T> list) where T : Saxon.Hej.om.Item {
            return new Saxon.Impl.Helpers.ReverseListIterator<T>(list);
        }
        public override string getStreamerName() {
            return "Reverse";
        }
    
    }
}