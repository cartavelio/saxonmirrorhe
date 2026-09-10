using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class WherePopulated : Saxon.Hej.expr.UnaryExpression, Saxon.Hej.expr.ItemMappingFunction {
        public WherePopulated(Saxon.Hej.expr.Expression @base) : base (@base) {
        }
        public override bool isInstruction() {
            return true;
        }
        protected internal override Saxon.Hej.expr.OperandRole getOperandRole() {
            return new Saxon.Hej.expr.OperandRole(0, Saxon.Hej.expr.OperandUsage.TRANSMISSION);
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            return new Saxon.Hej.expr.instruct.WherePopulated(getBaseExpression().copy(rebindings));
        }
        public override int getImplementationMethod() {
            return (ITERATE_METHOD|PROCESS_METHOD);
        }
        protected internal override int computeCardinality() {
            return (base.computeCardinality()|Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO);
        }
        public override Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.expr.XPathContext context) {
            return new Saxon.Hej.expr.ItemMappingIterator(getBaseExpression().iterate(context), this);
        }
        public override void process(Saxon.Hej.@event.Outputter output, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.@event.WherePopulatedOutputter filter = new Saxon.Hej.@event.WherePopulatedOutputter(output);
            getBaseExpression().process(filter, context);
        }
        public Saxon.Hej.om.Item mapItem(Saxon.Hej.om.Item item) {
            return (isDeemedEmpty(item) ? null : item);
        }
        public static bool isDeemedEmpty(Saxon.Hej.om.Item item) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                int kind = ((Saxon.Hej.om.NodeInfo)(item)).getNodeKind();
                switch (kind) {
                  case Saxon.Hej.type.Type.DOCUMENT:
                  case Saxon.Hej.type.Type.ELEMENT:
                    return !(((Saxon.Hej.om.NodeInfo)(item)).hasChildNodes());
                  default:
                    return (item.getUnicodeStringValue().length() == 0);
                }
            } else {
                if ((((item is Saxon.Hej.value.StringValue) || (item is Saxon.Hej.value.HexBinaryValue)) || (item is Saxon.Hej.value.Base64BinaryValue))) {
                    return (item.getUnicodeStringValue().length() == 0);
                } else {
                    if ((item is Saxon.Hej.ma.map.MapItem)) {
                        return ((Saxon.Hej.ma.map.MapItem)(item)).isEmpty();
                    } else {
                        return false;
                    }
                }
            }
        }
        public override string getExpressionName() {
            return "wherePop";
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("condCont", this);
            getBaseExpression().export(@out);
            @out.endElement();
        }
        public override string getStreamerName() {
            return "WherePopulated";
        }
    
    }
}