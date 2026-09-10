using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLOriginalLibrary : Saxon.Hej.functions.FunctionLibrary {
        private static readonly Saxon.Hej.style.XSLOriginalLibrary THE_INSTANCE = new Saxon.Hej.style.XSLOriginalLibrary();
        public static Saxon.Hej.style.XSLOriginalLibrary getInstance() {
            return THE_INSTANCE;
        }
        public static Saxon.Hej.om.StructuredQName XSL_ORIGINAL = new Saxon.Hej.om.StructuredQName("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "original");
        private XSLOriginalLibrary()  {
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] staticArgs, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            try {
                Saxon.Hej.om.Function target = getFunctionItem(functionName, env);
                if ((target == null)) {
                    return null;
                } else {
                    return new Saxon.Hej.expr.StaticFunctionCall(target, staticArgs);
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                reasons.Add(e.Message);
                return null;
            }
        }
        public bool isAvailable(Saxon.Hej.trans.SymbolicName.F functionName, int languageLevel) {
            return false;
        }
        public Saxon.Hej.functions.FunctionLibrary copy() {
            return this;
        }
        public Saxon.Hej.om.Function getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext env) {
            if (((((functionName.getComponentKind() == Saxon.Hej.om.StandardNames.XSL_FUNCTION) && functionName.getComponentName().hasURI(Saxon.Hej.lib.NamespaceConstant.XSLT)) && functionName.getComponentName().getLocalPart().Equals("original")) && (env is Saxon.Hej.style.ExpressionContext))) {
                Saxon.Hej.style.ExpressionContext expressionContext = (Saxon.Hej.style.ExpressionContext)(env);
                Saxon.Hej.style.StyleElement overridingFunction = expressionContext.getStyleElement();
                while (!(((overridingFunction is Saxon.Hej.style.XSLFunction)))) {
                    Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)overridingFunction.getParent());
                    if (!(((parent is Saxon.Hej.style.StyleElement)))) {
                        return null;
                    }
                    overridingFunction = (Saxon.Hej.style.StyleElement)(parent);
                }
                Saxon.Hej.trans.SymbolicName originalName = ((/*1430*/Saxon.Hej.trans.SymbolicName.F)((Saxon.Hej.style.XSLFunction)(overridingFunction)).getSymbolicName());
                Saxon.Hej.style.XSLOverride @override = (Saxon.Hej.style.XSLOverride)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)overridingFunction.getParent()));
                Saxon.Hej.style.XSLUsePackage use = (Saxon.Hej.style.XSLUsePackage)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)@override.getParent()));
                Saxon.Hej.expr.Component overridden = use.getUsedPackage().getComponent(originalName);
                if ((overridden == null)) {
                    throw new Saxon.Hej.trans.XPathException((("Function "+originalName)+" does not exist in used package"), "XTSE3058");
                }
                return new Saxon.Hej.expr.instruct.OriginalFunction(overridden);
            } else {
                return null;
            }
        }
    
        /* Expanding default method setConfiguration from interface FunctionLibrary */
        public virtual void setConfiguration(Saxon.Hej.Configuration config) {
        }

    }
}