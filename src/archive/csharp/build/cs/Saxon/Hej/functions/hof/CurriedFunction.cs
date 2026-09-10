using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class CurriedFunction : Saxon.Hej.functions.AbstractFunction {
        private readonly Saxon.Hej.om.Function targetFunction;
        private readonly Saxon.Hej.om.Sequence[] boundValues;
        private Saxon.Hej.type.FunctionItemType functionType;
        public CurriedFunction(Saxon.Hej.om.Function targetFunction, Saxon.Hej.om.Sequence[] boundValues)  {
            this.targetFunction = targetFunction;
            this.boundValues = boundValues;
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            if ((functionType == null)) {
                Saxon.Hej.type.FunctionItemType baseItemType = targetFunction.getFunctionItemType();
                Saxon.Hej.value.SequenceType resultType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
                if ((baseItemType is Saxon.Hej.type.SpecificFunctionType)) {
                    resultType = baseItemType.getResultType();
                }
                int placeholders = 0;
                foreach (Saxon.Hej.om.Sequence boundArgument in boundValues) {
                    if ((boundArgument == null)) {
                        placeholders++;
                    }
                }
                Saxon.Hej.value.SequenceType[] argTypes = new Saxon.Hej.value.SequenceType[placeholders];
                if ((baseItemType is Saxon.Hej.type.SpecificFunctionType)) {
                    for (int i = 0, j = 0; (i<boundValues.Length); i++) {
                        if ((boundValues[i] == null)) {
                            argTypes[j++] = baseItemType.getArgumentTypes()[i];
                        }
                    }
                } else {
                    Saxon.Ejava.util.Arrays.fill(argTypes, Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
                }
                functionType = new Saxon.Hej.type.SpecificFunctionType(argTypes, resultType);
            }
            return functionType;
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return null;
        }
        public override string getDescription() {
            return ("partially-applied function "+targetFunction.getDescription());
        }
        public override int getArity() {
            int count = 0;
            foreach (Saxon.Hej.om.Sequence v in boundValues) {
                if ((v == null)) {
                    count++;
                }
            }
            return count;
        }
        public override Saxon.Hej.query.AnnotationList getAnnotations() {
            return targetFunction.getAnnotations();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            Saxon.Hej.om.Sequence[] newArgs = new Saxon.Hej.om.Sequence[boundValues.Length];
            for (int i = 0, j = 0; (i<newArgs.Length); i++) {
                if ((boundValues[i] == null)) {
                    newArgs[i] = args[j++];
                } else {
                    newArgs[i] = boundValues[i];
                }
            }
            Saxon.Hej.expr.XPathContext c2 = targetFunction.makeNewContext(context, null);
            if ((targetFunction is Saxon.Hej.expr.instruct.UserFunction)) {
                ((Saxon.Hej.expr.XPathContextMajor)(c2)).setCurrentComponent(((Saxon.Hej.expr.instruct.UserFunction)(targetFunction)).getDeclaringComponent());
            }
            return targetFunction.call(c2, newArgs);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("curriedFunc");
            targetFunction.export(@out);
            @out.startElement("args");
            foreach (Saxon.Hej.om.Sequence seq in boundValues) {
                if ((seq == null)) {
                    @out.startElement("x");
                    @out.endElement();
                } else {
                    Saxon.Hej.expr.Literal.exportValue(seq, @out);
                }
            }
            @out.endElement();
            @out.endElement();
        }
    
    }
}