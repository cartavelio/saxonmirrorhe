using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class ExecutableFunctionLibrary : Saxon.Hej.functions.FunctionLibrary {
        private readonly Saxon.Hej.Configuration config;
        private System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.instruct.UserFunction> functions = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName, Saxon.Hej.expr.instruct.UserFunction>(20);
        public ExecutableFunctionLibrary(Saxon.Hej.Configuration config)  {
            this.config = config;
        }
        public virtual void addFunction(Saxon.Hej.expr.instruct.UserFunction fn) {
            Saxon.Impl.Helpers.MapUtils.Put(functions, fn.getSymbolicName(), fn);
        }
        public virtual Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] staticArgs, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.instruct.UserFunction fn = Saxon.Impl.Helpers.MapUtils.Get(functions, functionName);
            if ((fn == null)) {
                return null;
            }
            Saxon.Hej.expr.UserFunctionCall fc = new Saxon.Hej.expr.UserFunctionCall();
            fc.setFunctionName(functionName.getComponentName());
            fc.setArguments(staticArgs);
            fc.setFunction(fn);
            fc.setStaticType(fn.getResultType());
            return fc;
        }
        public virtual Saxon.Hej.om.Function getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            Saxon.Hej.expr.instruct.UserFunction fn = Saxon.Impl.Helpers.MapUtils.Get(functions, functionName);
            if (((fn != null) && fn.isUpdating())) {
                throw new Saxon.Hej.trans.XPathException("Cannot bind a function item to an updating function");
            }
            return fn;
        }
        public virtual bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName, int languageLevel) {
            return (Saxon.Impl.Helpers.MapUtils.Get(functions, functionName) != null);
        }
        public virtual Saxon.Hej.functions.FunctionLibrary copy() {
            Saxon.Hej.functions.ExecutableFunctionLibrary efl = new Saxon.Hej.functions.ExecutableFunctionLibrary(config);
            efl.functions = new System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName, Saxon.Hej.expr.instruct.UserFunction>(functions);
            return efl;
        }
        public System.Collections.Generic.IEnumerable<Saxon.Hej.expr.instruct.UserFunction> getAllFunctions() {
            return functions.Values;
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}