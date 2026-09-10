using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class String_1 : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            try {
                return new Saxon.Hej.value.StringValue(arg.getUnicodeStringValue());
            } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                throw err.getXPathException();
            }
        }
        public override Saxon.Hej.om.Sequence resultWhenEmpty() {
            return Saxon.Hej.value.StringValue.EMPTY_STRING;
        }
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.type.TypeHierarchy th = visitor.getConfiguration().getTypeHierarchy();
            Saxon.Hej.expr.Expression arg = arguments[0];
            if ((th.isSubType(arg.getItemType(), Saxon.Hej.type.BuiltInAtomicType.STRING) && (arg.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE))) {
                return arg;
            }
            if (((arg is Saxon.Hej.expr.instruct.SimpleNodeConstructor) && (arg.getCardinality() == Saxon.Hej.expr.StaticProperty.EXACTLY_ONE))) {
                return ((Saxon.Hej.expr.instruct.SimpleNodeConstructor)(arg)).getSelect();
            }
            return null;
        }
        public override string getCompilerName() {
            return "StringFnCompiler";
        }
        public override string getStreamerName() {
            return "StringFn";
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.functions.String_1.StringFnElaborator();
        }
        public class StringFnElaborator : Saxon.Hej.expr.elab.StringElaborator {
            public override Saxon.Hej.expr.elab.UnicodeStringEvaluator elaborateForUnicodeString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                Saxon.Hej.expr.elab.UnicodeStringEvaluator argEval = arg.makeElaborator().elaborateForUnicodeString(true);
                return context => {
                    try {
                        return /*Delegate.eval()*/argEval(context);
                    } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                        throw err.getXPathException();
                    }
                } ;
            }
            public override Saxon.Hej.expr.elab.StringEvaluator elaborateForString(bool zeroLengthWhenAbsent) {
                Saxon.Hej.expr.SystemFunctionCall fnc = (Saxon.Hej.expr.SystemFunctionCall)(getExpression());
                Saxon.Hej.expr.Expression arg = fnc.getArg(0);
                Saxon.Hej.expr.elab.StringEvaluator argEval = arg.makeElaborator().elaborateForString(true);
                return context => {
                    try {
                        return /*Delegate.eval()*/argEval(context);
                    } catch (Saxon.Hej.trans.UncheckedXPathException err) {
                        throw err.getXPathException();
                    }
                } ;
            }
        
        }
    
    }
}