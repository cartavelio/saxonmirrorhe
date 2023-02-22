using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn.js {
    public class JSFunctionLibrary : Saxon.Hej.functions.FunctionLibrary {
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName, int languageLevel) {
            return Saxon.Hej.om.NamespaceUri.GLOBAL_JS.Equals(functionName.getComponentName().getNamespaceUri());
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] staticArgs, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> keywords, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.om.StructuredQName fName = functionName.getComponentName();
            if (Saxon.Hej.om.NamespaceUri.GLOBAL_JS.Equals(fName.getNamespaceUri())) {
                try {
                    Saxon.Hej.expr.Expression windowCall = /*134*/Saxon.Eej.ee.extfn.js.IXSLFunctionSet.getInstance().makeFunction("window", 0).makeFunctionCall();
                    Saxon.Hej.expr.Expression callCall = /*134*/Saxon.Eej.ee.extfn.js.IXSLFunctionSet.getInstance().makeFunction("call", 3).makeFunctionCall(windowCall, new Saxon.Hej.expr.StringLiteral(fName.getLocalPart()), new Saxon.Hej.ma.arrays.SquareArrayConstructor(Saxon.Ejava.util.Arrays.asList(staticArgs)));
                    callCall.setRetainedStaticContext(env.makeRetainedStaticContext());
                    return callCall;
                } catch (Saxon.Hej.trans.XPathException e) {
                    reasons.Add(e.Message);
                    return null;
                }
            }
            return null;
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            return this;
        }
        public Saxon.Hej.om.FunctionItem getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            return null;
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}