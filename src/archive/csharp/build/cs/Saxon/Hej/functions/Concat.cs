using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Concat : Saxon.Hej.functions.SystemFunction, Saxon.Hej.functions.PushableFunction {
        protected internal override Saxon.Hej.om.Sequence resultIfEmpty(int arg) {
            return null;
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            Saxon.Hej.value.SequenceType[] argTypes = new Saxon.Hej.value.SequenceType[getArity()];
            Saxon.Ejava.util.Arrays.fill(argTypes, Saxon.Hej.value.SequenceType.ATOMIC_SEQUENCE);
            return new Saxon.Hej.type.SpecificFunctionType(argTypes, Saxon.Hej.value.SequenceType.SINGLE_STRING);
        }
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            if (Saxon.Hej.expr.oper.OperandArray.every(arguments, arg => ((arg.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE) && (arg.getItemType() == Saxon.Hej.type.BuiltInAtomicType.BOOLEAN)))) {
                visitor.getStaticContext().issueWarning((("Did you intend to apply string concatenation to boolean operands? "+"Perhaps you intended 'or' rather than '||'. ")+"To suppress this warning, use string() on the arguments."), arguments[0].getLocation());
            }
            return new Inner_Optimized_d1047e257(this, this, arguments);
        }
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.str.UnicodeBuilder builder = new Saxon.Hej.str.UnicodeBuilder();
            foreach (Saxon.Hej.om.Sequence arg in arguments) {
                Saxon.Hej.om.SequenceIterator iter = arg.iterate();
                Saxon.Hej.om.Item item;
                while (((item = iter.next()) != null)) {
                    builder.accept(item.getUnicodeStringValue());
                }
            }
            return new Saxon.Hej.value.StringValue(builder.toUnicodeString());
        }
        public void process(Saxon.Hej.@event.Outputter destination, Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.str.UniStringConsumer output = destination.getStringReceiver(false);
            output.open();
            foreach (Saxon.Hej.om.Sequence arg in arguments) {
                Saxon.Hej.om.SequenceIterator iter = arg.iterate();
                Saxon.Hej.om.Item item;
                while (((item = iter.next()) != null)) {
                    output.accept(item.getUnicodeStringValue());
                }
            }
            output.close();
        }
        public override Saxon.Hej.value.SequenceType getRequiredType(int arg) {
            return getDetails().argumentTypes[0];
        }
        public override string getCompilerName() {
            return "ConcatCompiler";
        }
    
        private class Inner_Optimized_d1047e257 : Saxon.Hej.expr.SystemFunctionCall.Optimized {
            private readonly Concat outerInstance;
            public Inner_Optimized_d1047e257(Concat outerInstance, Concat arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override  /* REPLACING public */ Saxon.Hej.str.UnicodeString evaluateAsString(Saxon.Hej.expr.XPathContext context) {
                Saxon.Hej.str.UnicodeBuilder buffer = new Saxon.Hej.str.UnicodeBuilder();
                foreach (Saxon.Hej.expr.Operand o in /*superMethod*/operands()) {
                    Saxon.Hej.om.SequenceIterator iter = /*localvar*/o.getChildExpression().iterate(/*localparam*/context);
                    Saxon.Hej.om.Item item;
                    while (((/*localvar*/item = /*localvar*/iter.next()) != null)) {
                        /*localvar*/buffer.accept(/*localvar*/item.getUnicodeStringValue());
                    }
                }
                return /*localvar*/buffer.toUnicodeString();
            }
            public override  /* REPLACING public */ Saxon.Hej.om.Item evaluateItem(Saxon.Hej.expr.XPathContext context) {
                return new Saxon.Hej.value.StringValue(/*method*/evaluateAsString(/*localparam*/context));
            }
        }

    }
}