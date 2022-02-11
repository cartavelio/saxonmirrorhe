using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class FunctionAvailable : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.expr.Expression makeFunctionCall(params Saxon.Hej.expr.Expression[]  arguments) {
            Saxon.Hej.expr.PackageData pack = getRetainedStaticContext().getPackageData();
            if ((pack is Saxon.Hej.style.StylesheetPackage)) {
                ((Saxon.Hej.style.StylesheetPackage)(pack)).setRetainUnusedFunctions();
            }
            return base.makeFunctionCall(arguments);
        }
        public override Saxon.Hej.expr.Expression makeOptimizedFunctionCall(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo, params Saxon.Hej.expr.Expression[]  arguments) {
            if (((arguments[0] is Saxon.Hej.expr.Literal) && (((arguments.Length == 1) || (arguments[1] is Saxon.Hej.expr.Literal))))) {
                string lexicalQName = ((Saxon.Hej.expr.StringLiteral)(arguments[0])).stringify();
                Saxon.Hej.expr.StaticContext env = visitor.getStaticContext();
                bool b = false;
                Saxon.Hej.om.QNameParser qp = new Saxon.Hej.om.QNameParser(getRetainedStaticContext()).withAcceptEQName(true).withErrorOnBadSyntax("XTDE1400").withErrorOnUnresolvedPrefix("XTDE1400");
                Saxon.Hej.om.StructuredQName functionName = qp.parse(lexicalQName, env.getDefaultFunctionNamespace());
                int minArity = 0;
                int maxArity = 20;
                if ((getArity() == 2)) {
                    minArity = (int)(((Saxon.Hej.value.NumericValue)(arguments[1].evaluateItem(env.makeEarlyEvaluationContext()))).longValue());
                    maxArity = minArity;
                }
                for (int i = minArity; (i<=maxArity); i++) {
                    Saxon.Hej.trans.SymbolicName.F sn = new Saxon.Hej.trans.SymbolicName.F(functionName, i);
                    if (env.getFunctionLibrary().isAvailable(sn)) {
                        b = true;
                        break ;
                    }
                }
                return Saxon.Hej.expr.Literal.makeLiteral(Saxon.Hej.value.BooleanValue.get(b));
            } else {
                return null;
            }
        }
        private bool isFunctionAvailable(string lexicalName, string edition, int arity, Saxon.Hej.expr.XPathContext context) {
            if ((arity == -1)) {
                for (int i = 0; (i<20); i++) {
                    if (isFunctionAvailable(lexicalName, edition, i, context)) {
                        return true;
                    }
                }
                return false;
            }
            Saxon.Hej.om.StructuredQName qName;
            try {
                if (Saxon.Hej.om.NameChecker.isValidNCName(Saxon.Hej.str.StringTool.codePoints(lexicalName))) {
                    string uri = Saxon.Hej.lib.NamespaceConstant.FN;
                    qName = new Saxon.Hej.om.StructuredQName("", uri, lexicalName);
                } else {
                    qName = Saxon.Hej.om.StructuredQName.fromLexicalQName(lexicalName, false, true, getRetainedStaticContext());
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                e.setErrorCode("XTDE1400");
                e.setXPathContext(context);
                throw;
            }
            Saxon.Hej.functions.FunctionLibrary lib = context.getController().getExecutable().getFunctionLibrary();
            Saxon.Hej.trans.SymbolicName.F sn = new Saxon.Hej.trans.SymbolicName.F(qName, arity);
            return lib.isAvailable(sn);
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            string lexicalQName = arguments[0].head().getStringValue();
            int arity = -1;
            if ((arguments.Length == 2)) {
                arity = (int)(((Saxon.Hej.value.NumericValue)(arguments[1].head())).longValue());
            }
            return Saxon.Hej.value.BooleanValue.get(isFunctionAvailable(lexicalQName, getRetainedStaticContext().getPackageData().getTargetEdition(), arity, context));
        }
    
    }
}