using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class PublicStylesheetFunctionLibrary : Saxon.Hej.functions.FunctionLibrary {
        private readonly Saxon.Hej.functions.FunctionLibrary @base;
        public PublicStylesheetFunctionLibrary(Saxon.Hej.functions.FunctionLibrary @base)  {
            this.@base = @base;
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] staticArgs, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> keywords, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            Saxon.Hej.expr.Expression baseCall = @base.bind(functionName, staticArgs, null, env, reasons);
            if ((baseCall is Saxon.Hej.expr.UserFunctionCall)) {
                Saxon.Hej.expr.Component target = ((Saxon.Hej.expr.UserFunctionCall)(baseCall)).getTarget();
                Saxon.Hej.trans.Visibility v = target.getVisibility();
                if (((v == Saxon.Hej.trans.Visibility.PUBLIC) || (v == Saxon.Hej.trans.Visibility.FINAL))) {
                    return baseCall;
                } else {
                    reasons.Add("The function exists, but does not have public visibility");
                }
            }
            return null;
        }
        public Saxon.Hej.om.FunctionItem getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext staticContext) {
            Saxon.Hej.om.FunctionItem baseFunction = @base.getFunctionItem(functionName, staticContext);
            if ((baseFunction is Saxon.Hej.expr.instruct.UserFunction)) {
                Saxon.Hej.trans.Visibility v = ((Saxon.Hej.expr.instruct.UserFunction)(baseFunction)).getDeclaredVisibility();
                if (((v == Saxon.Hej.trans.Visibility.PUBLIC) || (v == Saxon.Hej.trans.Visibility.FINAL))) {
                    return baseFunction;
                }
            }
            return null;
        }
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName, int languageLevel) {
            if ((@base is Saxon.Hej.style.StylesheetFunctionLibrary)) {
                Saxon.Hej.style.StylesheetPackage pack = ((Saxon.Hej.style.StylesheetFunctionLibrary)(@base)).getStylesheetPackage();
                Saxon.Hej.expr.instruct.UserFunction fn = pack.getFunction(functionName);
                if ((fn != null)) {
                    Saxon.Hej.trans.Visibility v = fn.getDeclaredVisibility();
                    return ((v == Saxon.Hej.trans.Visibility.PUBLIC) || (v == Saxon.Hej.trans.Visibility.FINAL));
                } else {
                    return false;
                }
            } else {
                return @base.isAvailable(functionName, languageLevel);
            }
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            return this;
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}