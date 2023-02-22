using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class StylesheetFunctionLibrary : Saxon.Hej.functions.FunctionLibrary {
        private readonly Saxon.Hej.style.StylesheetPackage pack;
        private readonly bool overrideExtensionFunction;
        private System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,System.Collections.Generic.IList<Saxon.Hej.expr.Component>> functionIndex = null;
        public StylesheetFunctionLibrary(Saxon.Hej.style.StylesheetPackage sheet, bool overrideExtensionFunction)  {
            this.pack = sheet;
            this.overrideExtensionFunction = overrideExtensionFunction;
        }
        public bool isOverrideExtensionFunction() {
            return overrideExtensionFunction;
        }
        public Saxon.Hej.style.StylesheetPackage getStylesheetPackage() {
            return pack;
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] staticArgs, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> keywords, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.Component c = getFunction(functionName.getComponentName(), staticArgs.Length);
            if ((c == null)) {
                return null;
            }
            Saxon.Hej.expr.instruct.UserFunction fn = (Saxon.Hej.expr.instruct.UserFunction)(c.getActor());
            fn.incrementReferenceCount();
            if ((fn.isOverrideExtensionFunction() != this.overrideExtensionFunction)) {
                return null;
            }
            Saxon.Hej.expr.UserFunctionCall fc = new Saxon.Hej.expr.UserFunctionCall();
            fc.setFunction(fn);
            fc.setFunctionName(fn.getFunctionName());
            int maxArity = fn.getArity();
            if (((staticArgs.Length == maxArity) && (((keywords == null) || keywords.Count==0)))) {
                fc.setArguments(staticArgs);
            } else {
                Saxon.Hej.expr.Expression[] expandedArgs = Saxon.Hej.expr.instruct.UserFunction.makeExpandedArgumentArray(staticArgs, keywords, fn);
                fc.setArguments(expandedArgs);
            }
            if ((env is Saxon.Hej.style.ExpressionContext)) {
                Saxon.Hej.style.PrincipalStylesheetModule psm = ((Saxon.Hej.style.ExpressionContext)(env)).getStyleElement().getCompilation().getPrincipalStylesheetModule();
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(env);
                psm.addFixupAction(() => {
                    if ((fc.getFunction() == null)) {
                        Saxon.Hej.expr.Component target = psm.getComponent(fc.getSymbolicName());
                        Saxon.Hej.expr.instruct.UserFunction fn1 = (Saxon.Hej.expr.instruct.UserFunction)(target.getActor());
                        if ((fn1 != null)) {
                            fc.allocateArgumentEvaluators();
                            fc.setStaticType(fn1.getResultType());
                        } else {
                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((((("There is no available function named "+fc.getDisplayName())+" with ")+fc.getArity())+" arguments"), "XPST0017");
                            err.setLocator(fc.getLocation());
                            throw err;
                        }
                    }
                } );
            } else {
            }
            return fc;
        }
        private void buildFunctionIndex() {
            System.Collections.Generic.Dictionary<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> allComponents = pack.getComponentIndex();
            functionIndex = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, System.Collections.Generic.IList<Saxon.Hej.expr.Component>>();
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.trans.SymbolicName,Saxon.Hej.expr.Component> entry in allComponents) {
                if ((entry.Value.getComponentKind() == Saxon.Hej.om.StandardNames.XSL_FUNCTION)) {
                    Saxon.Hej.expr.instruct.UserFunction uf = (Saxon.Hej.expr.instruct.UserFunction)(entry.Value.getActor());
                    Saxon.Hej.om.StructuredQName functionName = entry.Key.getComponentName();
                    if (functionIndex.ContainsKey(functionName)) {
                        Saxon.Impl.Helpers.MapUtils.Get(functionIndex, functionName).Add(entry.Value);
                    } else {
                        System.Collections.Generic.IList<Saxon.Hej.expr.Component> functionList = new System.Collections.Generic.List<Saxon.Hej.expr.Component>();
                        functionList.Add(entry.Value);
                        Saxon.Impl.Helpers.MapUtils.Put(functionIndex, functionName, functionList);
                    }
                }
            }
        }
        private Saxon.Hej.expr.Component getFunction(Saxon.Hej.om.StructuredQName name, int actualArgs) {
            if ((functionIndex == null)) {
                buildFunctionIndex();
            }
            System.Collections.Generic.IList<Saxon.Hej.expr.Component> candidates = Saxon.Impl.Helpers.MapUtils.Get(functionIndex, name);
            if ((candidates == null)) {
                return null;
            }
            foreach (Saxon.Hej.expr.Component c in candidates) {
                Saxon.Hej.expr.instruct.UserFunction fn = (Saxon.Hej.expr.instruct.UserFunction)(c.getActor());
                if (((fn.getMinimumArity()<=actualArgs) && (fn.getArity()>=actualArgs))) {
                    return c;
                }
            }
            return null;
        }
        public Saxon.Hej.om.FunctionItem getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            return pack.getFunction(functionName);
        }
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName, int languageLevel) {
            return (pack.getFunction(functionName) != null);
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            return this;
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}