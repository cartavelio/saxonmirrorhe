using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class CoercedFunction : Saxon.Hej.functions.AbstractFunction {
        private Saxon.Hej.om.FunctionItem targetFunction;
        private readonly Saxon.Hej.type.SpecificFunctionType requiredType;
        public CoercedFunction(Saxon.Hej.om.FunctionItem targetFunction, Saxon.Hej.type.SpecificFunctionType requiredType)  {
            if ((targetFunction.getArity() != requiredType.getArity())) {
                throw new Saxon.Hej.trans.XPathException(wrongArityMessage(targetFunction, requiredType.getArity()), "XPTY0004");
            }
            this.targetFunction = targetFunction;
            this.requiredType = requiredType;
        }
        public CoercedFunction(Saxon.Hej.type.SpecificFunctionType requiredType)  {
            this.requiredType = requiredType;
        }
        public void setTargetFunction(Saxon.Hej.om.FunctionItem targetFunction) {
            if ((targetFunction.getArity() != requiredType.getArity())) {
                throw new Saxon.Hej.trans.XPathException(wrongArityMessage(targetFunction, requiredType.getArity()), "XPTY0004");
            }
            this.targetFunction = targetFunction;
        }
        public override void typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextItemType) {
            if ((targetFunction is Saxon.Hej.functions.AbstractFunction)) {
                ((Saxon.Hej.functions.AbstractFunction)(targetFunction)).typeCheck(visitor, contextItemType);
            }
        }
        public override Saxon.Hej.type.FunctionItemType getFunctionItemType() {
            return requiredType;
        }
        public override Saxon.Hej.om.StructuredQName getFunctionName() {
            return targetFunction.getFunctionName();
        }
        public override string getDescription() {
            return ("coerced "+targetFunction.getDescription());
        }
        public override int getArity() {
            return targetFunction.getArity();
        }
        public override Saxon.Hej.query.AnnotationList getAnnotations() {
            return targetFunction.getAnnotations();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] args) {
            Saxon.Hej.type.SpecificFunctionType req = requiredType;
            Saxon.Hej.value.SequenceType[] argTypes = targetFunction.getFunctionItemType().getArgumentTypes();
            Saxon.Hej.type.TypeHierarchy th = context.getConfiguration().getTypeHierarchy();
            Saxon.Hej.om.Sequence[] targetArgs = new Saxon.Hej.om.Sequence[args.Length];
            for (int i = 0; (i<args.Length); i++) {
                Saxon.Hej.om.GroundedValue gVal = args[i].materialize();
                if (argTypes[i].matches(gVal, th)) {
                    targetArgs[i] = gVal;
                } else {
                    int pos = i;
                    Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, targetFunction.getDescription(), pos);
                    targetArgs[i] = th.applyFunctionConversionRules(gVal, argTypes[i], role, Saxon.Hej.expr.parser.Loc.NONE);
                }
            }
            Saxon.Hej.om.GroundedValue rawResult = targetFunction.call(context, targetArgs).materialize();
            if (req.getResultType().matches(rawResult, th)) {
                return rawResult;
            } else {
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION_RESULT, targetFunction.getDescription(), 0);
                return th.applyFunctionConversionRules(rawResult, req.getResultType(), role, Saxon.Hej.expr.parser.Loc.NONE);
            }
        }
        private static string wrongArityMessage(Saxon.Hej.om.FunctionItem supplied, int expected) {
            return ((((("The supplied function ("+supplied.getDescription())+") has ")+Saxon.Hej.expr.FunctionCall.pluralArguments(supplied.getArity()))+" - expected ")+expected);
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("coercedFn");
            @out.emitAttribute("type", requiredType.toExportString());
            new Saxon.Hej.functions.hof.FunctionLiteral(targetFunction).export(@out);
            @out.endElement();
        }
    
    }
}