using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class StringJoin : Saxon.Hej.functions.FoldingFunction, Saxon.Hej.functions.PushableFunction {
        private bool returnEmptyIfEmpty;
        public void setReturnEmptyIfEmpty(bool option) {
            returnEmptyIfEmpty = option;
        }
        public bool isReturnEmptyIfEmpty() {
            return returnEmptyIfEmpty;
        }
        public override int getCardinality(Saxon.Hej.expr.Expression[] arguments) {
            if (returnEmptyIfEmpty) {
                return Saxon.Hej.expr.StaticProperty.ALLOWS_ZERO_OR_ONE;
            } else {
                return Saxon.Hej.expr.StaticProperty.EXACTLY_ONE;
            }
        }
        public override bool Equals(System.Object o) {
            return ((((o is Saxon.Hej.functions.StringJoin)) && base.Equals(o)) && (returnEmptyIfEmpty == ((Saxon.Hej.functions.StringJoin)(o)).returnEmptyIfEmpty));
        }
        public override int GetHashCode() {
            return (base.GetHashCode()|((returnEmptyIfEmpty ? 0x05000000 : 0)));
        }
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.expr.Expression e2 = base.makeOptimizedFunctionCall(visitor, contextInfo, arguments);
            if ((e2 != null)) {
                return e2;
            }
            int card = arguments[0].getCardinality();
            if (!(Saxon.Hej.value.Cardinality.allowsMany(card))) {
                if ((Saxon.Hej.value.Cardinality.allowsZero(card) || (arguments[0].getItemType().getPrimitiveItemType() != Saxon.Hej.type.BuiltInAtomicType.STRING))) {
                    if (returnEmptyIfEmpty) {
                        return new Saxon.Hej.expr.CastExpression(arguments[0], Saxon.Hej.type.BuiltInAtomicType.STRING, true);
                    } else {
                        return Saxon.Hej.functions.SystemFunction.makeCall("string", getRetainedStaticContext(), arguments[0]);
                    }
                } else {
                    return arguments[0];
                }
            }
            return null;
        }
        public override Saxon.Hej.functions.Fold getFold(Saxon.Hej.expr.XPathContext context, params Saxon.Hej.om.Sequence[]  additionalArguments) {
            Saxon.Hej.str.UnicodeString separator = Saxon.Hej.str.EmptyUnicodeString.getInstance();
            if ((additionalArguments.Length>0)) {
                separator = ((Saxon.Hej.om.GroundedValue)(additionalArguments[0].head())).getUnicodeStringValue();
            }
            return new Saxon.Hej.functions.StringJoin.StringJoinFold(separator, returnEmptyIfEmpty);
        }
        public void process(Saxon.Hej.@event.Outputter destination, Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.str.UnicodeString separator = ((arguments.Length>1) ? ((Saxon.Hej.om.GroundedValue)(arguments[1].head())).getUnicodeStringValue() : Saxon.Hej.str.EmptyUnicodeString.getInstance());
            Saxon.Hej.str.UniStringConsumer output = destination.getStringReceiver(false);
            output.open();
            bool first = true;
            Saxon.Hej.om.SequenceIterator iter = arguments[0].iterate();
            Saxon.Hej.om.Item it;
            try {
                while (((it = iter.next()) != null)) {
                    if (first) {
                        first = false;
                    } else {
                        output.accept(separator);
                    }
                    output.accept(it.getUnicodeStringValue());
                }
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
            output.close();
        }
        private class StringJoinFold : Saxon.Hej.functions.Fold {
            private int position = 0;
            private readonly Saxon.Hej.str.UnicodeString separator;
            private readonly Saxon.Hej.str.UnicodeBuilder data;
            private readonly bool returnEmptyIfEmpty;
            public StringJoinFold(Saxon.Hej.str.UnicodeString separator, bool returnEmptyIfEmpty)  {
                this.separator = separator;
                this.data = new Saxon.Hej.str.UnicodeBuilder();
                this.returnEmptyIfEmpty = returnEmptyIfEmpty;
            }
            public void processItem(Saxon.Hej.om.Item item) {
                if ((position == 0)) {
                    data.accept(item.getUnicodeStringValue());
                    position = 1;
                } else {
                    ((/*1430*/Saxon.Hej.str.UnicodeBuilder)data.accept(separator)).accept(item.getUnicodeStringValue());
                }
            }
            public bool isFinished() {
                return false;
            }
            public Saxon.Hej.om.Sequence result() {
                if (((position == 0) && returnEmptyIfEmpty)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                } else {
                    return new Saxon.Hej.value.StringValue(data.toUnicodeString());
                }
            }
        
        }
        public override string getCompilerName() {
            return "StringJoinCompiler";
        }
    
    }
}