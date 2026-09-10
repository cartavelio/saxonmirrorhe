using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class ContextItemAccessorFunction : Saxon.Hej.functions.ContextAccessorFunction {
        public override Saxon.Hej.om.Function bindContext(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item ci = context.getContextItem();
            if ((ci == null)) {
                Saxon.Hej.expr.Callable callable = new Saxon.Hej.expr.CallableDelegate((context1, arguments) => {
                    throw new Saxon.Hej.trans.XPathException((("Context item for "+getFunctionName().getDisplayName())+" is absent"), "XPDY0002");
                } );
                Saxon.Hej.type.FunctionItemType fit = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{}, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
                return new Saxon.Hej.functions.CallableFunction(0, callable, fit);
            }
            Saxon.Hej.functions.ConstantFunction fn = new Saxon.Hej.functions.ConstantFunction(evaluate(ci, context));
            fn.setDetails(getDetails());
            fn.setRetainedStaticContext(getRetainedStaticContext());
            return fn;
        }
        public virtual Saxon.Hej.om.GroundedValue evaluate(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.functions.SystemFunction f = Saxon.Hej.functions.SystemFunction.makeFunction(getDetails().name.getLocalPart(), getRetainedStaticContext(), 1);
            return f.call(context, new Saxon.Hej.om.Sequence[]{item}).materialize();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return evaluate(context.getContextItem(), context);
        }
        public override  /* REPLACING public override */ Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            Saxon.Hej.expr.Expression arg = new Saxon.Hej.expr.ContextItemExpression();
            if (getFunctionName().hasURI(Saxon.Hej.lib.NamespaceConstant.SAXON)) {
                Saxon.Hej.functions.registry.BuiltInFunctionSet.Entry entry = getDetails();
                try {
                    return entry.functionSet.makeFunction(entry.name.getLocalPart(), 1).makeFunctionCall(arg);
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.trans.UncheckedXPathException(e);
                }
            } else {
                return Saxon.Hej.functions.SystemFunction.makeCall(getFunctionName().getLocalPart(), getRetainedStaticContext(), arg);
            }
        }
        public Saxon.Hej.expr.Expression makeContextItemExplicit() {
            Saxon.Hej.expr.Expression[] args = new Saxon.Hej.expr.Expression[]{new Saxon.Hej.expr.ContextItemExpression()};
            return Saxon.Hej.functions.SystemFunction.makeCall(getFunctionName().getLocalPart(), getRetainedStaticContext(), args);
        }
        public class StringAccessor : Saxon.Hej.functions.ContextItemAccessorFunction {
            public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
                Saxon.Hej.expr.Expression ci = new Saxon.Hej.expr.ContextItemExpression();
                Saxon.Hej.expr.Expression sv = Saxon.Hej.functions.SystemFunction.makeCall("string", getRetainedStaticContext(), ci);
                return Saxon.Hej.functions.SystemFunction.makeCall(getFunctionName().getLocalPart(), getRetainedStaticContext(), sv);
            }
            public override Saxon.Hej.om.GroundedValue evaluate(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.functions.SystemFunction f = Saxon.Hej.functions.SystemFunction.makeFunction(getDetails().name.getLocalPart(), getRetainedStaticContext(), 1);
                Saxon.Hej.value.StringValue val = new Saxon.Hej.value.StringValue(item.getUnicodeStringValue());
                return f.call(context, new Saxon.Hej.om.Sequence[]{val}).materialize();
            }
        
        }
        public class Number_0 : Saxon.Hej.functions.ContextItemAccessorFunction {
            public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
                Saxon.Hej.expr.Expression ci = new Saxon.Hej.expr.ContextItemExpression();
                Saxon.Hej.expr.Expression sv = Saxon.Hej.functions.SystemFunction.makeCall("data", getRetainedStaticContext(), ci);
                return Saxon.Hej.functions.SystemFunction.makeCall(getFunctionName().getLocalPart(), getRetainedStaticContext(), sv);
            }
            public override Saxon.Hej.om.GroundedValue evaluate(Saxon.Hej.om.Item item, Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.functions.SystemFunction f = Saxon.Hej.functions.SystemFunction.makeFunction(getDetails().name.getLocalPart(), getRetainedStaticContext(), 1);
                Saxon.Hej.om.AtomicSequence val = item.atomize();
                switch (val.getLength()) {
                  case 0:
                    return Saxon.Hej.value.DoubleValue.NaN;
                  case 1:
                    return f.call(context, new Saxon.Hej.om.Sequence[]{((/*1430*/Saxon.Hej.value.AtomicValue)val.head())}).materialize();
                  default:
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("When number() is called with no arguments, the atomized value of the context node must "+"not be a sequence of several atomic values"), "XPTY0004");
                    err.setIsTypeError(true);
                    throw err;
                }
            }
        
        }
    
    }
}