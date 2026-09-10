using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLOriginalLibrary : Saxon.Hej.functions.FunctionLibrary {
        private static readonly Saxon.Hej.style.XSLOriginalLibrary THE_INSTANCE = new Saxon.Hej.style.XSLOriginalLibrary();
        public static Saxon.Hej.style.XSLOriginalLibrary getInstance() {
            return THE_INSTANCE;
        }
        public static Saxon.Hej.om.StructuredQName XSL_ORIGINAL = new Saxon.Hej.om.StructuredQName("xsl", Saxon.Hej.om.NamespaceUri.XSLT, "original");
        private XSLOriginalLibrary()  {
        }
        public Saxon.Hej.expr.Expression bind(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.Expression[] staticArgs, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,int> keywords, Saxon.Hej.expr.StaticContext env, System.Collections.Generic.IList<string> reasons) {
            try {
                Saxon.Hej.om.FunctionItem target = getFunctionItem(functionName, env);
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
        public Saxon.Hej.om.FunctionItem getFunctionItem(Saxon.Hej.trans.SymbolicName.F functionName, Saxon.Hej.expr.StaticContext env) {
            if (((((functionName.getComponentKind() == Saxon.Hej.om.StandardNames.XSL_FUNCTION) && functionName.getComponentName().hasURI(Saxon.Hej.om.NamespaceUri.XSLT)) && functionName.getComponentName().getLocalPart().Equals("original")) && (env is Saxon.Hej.style.ExpressionContext))) {
                Saxon.Hej.style.ExpressionContext expressionContext = (Saxon.Hej.style.ExpressionContext)(env);
                Saxon.Hej.style.StyleElement containingElement = expressionContext.getStyleElement();
                Saxon.Hej.style.XSLFunction overridingFunction = (Saxon.Hej.style.XSLFunction)(containingElement.findAncestorElement(Saxon.Hej.om.StandardNames.XSL_FUNCTION));
                if ((overridingFunction == null)) {
                    throw new Saxon.Hej.trans.XPathException("Function name xsl:original can only be used within xsl:function", "XTSE3058");
                }
                Saxon.Hej.trans.SymbolicName originalName = ((/*1430*/Saxon.Hej.trans.SymbolicName.F)overridingFunction.getSymbolicName());
                Saxon.Hej.style.StyleElement @override = (Saxon.Hej.style.StyleElement)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)overridingFunction.getParent()));
                if (!(((@override is Saxon.Hej.style.XSLOverride)))) {
                    throw new Saxon.Hej.trans.XPathException("Function name xsl:original can only be used within xsl:override", "XPST0017");
                }
                Saxon.Hej.style.XSLUsePackage use = (Saxon.Hej.style.XSLUsePackage)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)@override.getParent()));
                System.Diagnostics.Debug.Assert((use != null));
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