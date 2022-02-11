using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public abstract class PositionAndLast : Saxon.Hej.functions.ContextAccessorFunction {
        private bool contextPossiblyUndefined = true;
        public override int getNetCost() {
            return 0;
        }
        public override Saxon.Hej.om.Function bindContext(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.value.Int64Value value;
            try {
                value = evaluateItem(context);
            } catch (Saxon.Hej.trans.XPathException e) {
                Saxon.Hej.trans.SymbolicName.F name = new Saxon.Hej.trans.SymbolicName.F(getFunctionName(), getArity());
                Saxon.Hej.expr.Callable callable = new Saxon.Hej.expr.CallableDelegate((context1, arguments) => {
                    throw e;
                } );
                return new Saxon.Hej.functions.CallableFunction(name, callable, getFunctionItemType());
            }
            Saxon.Hej.functions.ConstantFunction fn = new Saxon.Hej.functions.ConstantFunction(value);
            fn.setDetails(getDetails());
            fn.setRetainedStaticContext(getRetainedStaticContext());
            return fn;
        }
        public override Saxon.Hej.value.IntegerValue[] getIntegerBounds() {
            return new Saxon.Hej.value.IntegerValue[]{Saxon.Hej.value.Int64Value.PLUS_ONE, Saxon.Hej.expr.Expression.MAX_SEQUENCE_LENGTH};
        }
        public override void supplyTypeInformation(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, Saxon.Hej.expr.Expression[] arguments) {
            base.supplyTypeInformation(visitor, contextInfo, arguments);
            if ((contextInfo.getItemType() == Saxon.Hej.type.ErrorType.getInstance())) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("The context item is absent at this point");
                err.setErrorCode("XPDY0002");
                throw err;
            } else {
                contextPossiblyUndefined = contextInfo.isPossiblyAbsent();
            }
        }
        public bool isContextPossiblyUndefined() {
            return contextPossiblyUndefined;
        }
        public abstract Saxon.Hej.value.Int64Value evaluateItem(Saxon.Hej.expr.XPathContext c);
        public override Saxon.Hej.value.IntegerValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return evaluateItem(context);
        }
        public class Position : Saxon.Hej.functions.PositionAndLast {
            public override Saxon.Hej.value.Int64Value evaluateItem(Saxon.Hej.expr.XPathContext c) {
                Saxon.Hej.om.FocusIterator currentIterator = c.getCurrentIterator();
                if ((currentIterator == null)) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("The context item is absent, so position() is undefined");
                    e.setXPathContext(c);
                    e.setErrorCode("XPDY0002");
                    throw e;
                }
                return Saxon.Hej.value.Int64Value.makeIntegerValue(currentIterator.position());
            }
            public override string getCompilerName() {
                return "PositionCompiler";
            }
        
        }
        public class Last : Saxon.Hej.functions.PositionAndLast {
            public override Saxon.Hej.value.Int64Value evaluateItem(Saxon.Hej.expr.XPathContext c) {
                try {
                    return Saxon.Hej.value.Int64Value.makeIntegerValue(c.getLast());
                } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                }
            }
            public override string getCompilerName() {
                return "LastCompiler";
            }
            public override string getStreamerName() {
                return "Last";
            }
        
        }
    
    }
}