using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class IntegratedFunctionLibrary : Saxon.Hej.functions.FunctionLibrary {
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.lib.ExtensionFunctionDefinition> functions = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.lib.ExtensionFunctionDefinition>();
        public void registerFunction(Saxon.Hej.lib.ExtensionFunctionDefinition function) {
            Saxon.Impl.Helpers.MapUtils.Put(functions, function.getFunctionQName(), function);
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] staticArgs, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.lib.ExtensionFunctionDefinition defn = Saxon.Impl.Helpers.MapUtils.Get(functions, functionName.getComponentName());
            if ((defn == null)) {
                return null;
            }
            return makeFunctionCall(defn, staticArgs);
        }
        public static Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.lib.ExtensionFunctionDefinition defn, Saxon.Hej.expr.Expression[] staticArgs) {
            Saxon.Hej.lib.ExtensionFunctionCall f = defn.makeCallExpression();
            f.setDefinition(defn);
            Saxon.Hej.functions.IntegratedFunctionCall fc = new Saxon.Hej.functions.IntegratedFunctionCall(defn.getFunctionQName(), f);
            fc.setArguments(staticArgs);
            return fc;
        }
        public Saxon.Hej.om.Function getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            Saxon.Hej.lib.ExtensionFunctionDefinition defn = Saxon.Impl.Helpers.MapUtils.Get(functions, functionName.getComponentName());
            if ((defn == null)) {
                return null;
            }
            try {
                return defn.asFunction();
            } catch (System.Exception err) {
                throw new Saxon.Hej.trans.XPathException(("Failed to create call to extension function "+functionName.getComponentName().getDisplayName()), err);
            }
        }
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName) {
            Saxon.Hej.lib.ExtensionFunctionDefinition defn = Saxon.Impl.Helpers.MapUtils.Get(functions, functionName.getComponentName());
            int arity = functionName.getArity();
            return (((defn != null) && (defn.getMaximumNumberOfArguments()>=arity)) && (defn.getMinimumNumberOfArguments()<=arity));
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            Saxon.Hej.functions.IntegratedFunctionLibrary lib = new Saxon.Hej.functions.IntegratedFunctionLibrary();
            lib.functions = copyHashMap(functions);
            return lib;
        }
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.lib.ExtensionFunctionDefinition> copyHashMap(System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.lib.ExtensionFunctionDefinition> functions) {
            return new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.lib.ExtensionFunctionDefinition>(functions);
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}