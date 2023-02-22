using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class StylesheetFunctionLibrary : Saxon.Hej.functions.FunctionLibrary {
        private readonly Saxon.Hej.style.StylesheetPackage pack;
        private readonly bool overrideExtensionFunction;
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
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] staticArgs, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.instruct.UserFunction fn = pack.getFunction(functionName);
            if ((fn == null)) {
                return null;
            }
            if ((fn.isOverrideExtensionFunction() != this.overrideExtensionFunction)) {
                return null;
            }
            Saxon.Hej.expr.UserFunctionCall fc = new Saxon.Hej.expr.UserFunctionCall();
            fc.setFunctionName(functionName.getComponentName());
            fc.setArguments(staticArgs);
            fc.setFunction(fn);
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
        public Saxon.Hej.om.Function getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            return pack.getFunction(functionName);
        }
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName) {
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