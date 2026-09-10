using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class OpFn : Saxon.Hej.functions.SystemFunction {

        static OpFn() {
            string[] ops = {",", "and", "or", "+", "-", "*", "div", "idiv", "mod", "=", "<", "<=", ">", ">=", "!=", "eq", "lt", "le", "gt", "ge", "ne", "<<", ">>", "is", "||", "|", "union", "except", "intersect", "to", "otherwise"};
            Saxon.Impl.Helpers.SetUtils.AddAll(allowedOperators, Saxon.Ejava.util.Arrays.asList(ops));
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string op = arguments[0].head().getStringValue();
            string collation = getRetainedStaticContext().getDefaultCollationName();
            string cacheKey = ("fn:op~"+collation);
            Saxon.Hej.om.FunctionItem fn = (Saxon.Hej.om.FunctionItem)(context.getController().getUserData(cacheKey, op));
            if ((fn == null)) {
                validateOperator(op);
                string xpath = (("function($x, $y) {$x "+op)+" $y}");
                Saxon.Hej.sxpath.XPathEvaluator eval = new Saxon.Hej.sxpath.XPathEvaluator(context.getConfiguration());
                Saxon.Hej.sxpath.IndependentContext env = new Saxon.Hej.sxpath.IndependentContext(context.getConfiguration());
                env.setDefaultCollationName(collation);
                env.setXPathLanguageLevel(40);
                eval.setStaticContext(env);
                Saxon.Hej.sxpath.XPathExpression cexp = eval.createExpression(xpath);
                Saxon.Hej.sxpath.XPathDynamicContext dc = cexp.createDynamicContext();
                fn = (Saxon.Hej.om.FunctionItem)(cexp.evaluateSingle(dc));
                context.getController().setUserData(cacheKey, op, fn);
            }
            return fn;
        }
        private readonly static System.Collections.Generic.HashSet<string> allowedOperators = new System.Collections.Generic.HashSet<string>();
        private static void validateOperator(string op) {
            if (!(allowedOperators.Contains(op))) {
                throw new Saxon.Hej.trans.XPathException((("Unsupported operator '"+op)+"'"), "XPTY0004");
            }
        }
    
    }
}