using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.extfn {
    public class Evaluate : Saxon.Hej.functions.SystemFunction {
        protected internal Saxon.Hej.sxpath.IndependentContext makeStaticContext(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.NodeInfo namespaceOrigin, string baseUri) {
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = getRetainedStaticContext();
            Saxon.Hej.Configuration config = context.getConfiguration();
            Saxon.Hej.sxpath.IndependentContext env = new Saxon.Hej.sxpath.IndependentContext(config);
            env.setBaseURI(baseUri);
            env.setExecutable(context.getController().getExecutable());
            env.setXPathLanguageLevel(31);
            if ((namespaceOrigin != null)) {
                env.setNamespaces(namespaceOrigin);
            } else {
                env.setNamespaceResolver(rsc);
                env.setDefaultElementNamespace(rsc.getDefaultElementNamespace());
            }
            Saxon.Hej.expr.PackageData pack = getRetainedStaticContext().getPackageData();
            if ((pack is Saxon.Hej.style.StylesheetPackage)) {
                env.setFunctionLibrary(((Saxon.Hej.style.StylesheetPackage)(pack)).getFunctionLibrary());
            } else {
                env.setFunctionLibrary(context.getController().getExecutable().getFunctionLibrary());
            }
            env.setDecimalFormatManager(getRetainedStaticContext().getDecimalFormatManager());
            return env;
        }
        protected internal Saxon.Eej.functions.extfn.Evaluate.PreparedExpression prepareExpression(string exprText, Saxon.Hej.om.NodeInfo namespaceOrigin, string baseUri, string functionName, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Configuration config = context.getConfiguration();
            Saxon.Hej.sxpath.IndependentContext env = makeStaticContext(context, namespaceOrigin, baseUri);
            Saxon.Hej.expr.instruct.Executable exec = context.getController().getExecutable();
            env.setExecutable(exec);
            env.setXPathLanguageLevel(30);
            env.setAllowUndeclaredVariables(true);
            Saxon.Eej.functions.extfn.Evaluate.PreparedExpression pexpr = new Saxon.Eej.functions.extfn.Evaluate.PreparedExpression();
            pexpr.expStaticContext = env;
            pexpr.variables = new Saxon.Hej.sxpath.XPathVariable[10];
            for (int i = 1; (i<10); i++) {
                pexpr.variables[(i-1)] = env.declareVariable(Saxon.Hej.om.NamespaceUri.NULL, ("p"+i));
            }
            Saxon.Hej.expr.Expression expr;
            try {
                expr = Saxon.Hej.expr.parser.ExpressionTool.make(exprText, env, 0, Saxon.Hej.expr.parser.Token.EOF, null);
            } catch (Saxon.Hej.trans.XPathException e) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(((("Static error in XPath expression supplied to "+functionName)+": ")+e.Message.Trim()));
                err.setErrorCodeQName(e.getErrorCodeQName());
                err.setXPathContext(context);
                throw err;
            }
            Saxon.Hej.type.ItemType contextItemType = Saxon.Hej.type.AnyItemType.getInstance();
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(env);
            expr = Saxon.Hej.expr.parser.ExpressionTool.resolveCallsToCurrentFunction(expr);
            expr = expr.typeCheck(visitor, config.makeContextItemStaticInfo(contextItemType, true));
            pexpr.stackFrameMap = env.getStackFrameMap();
            Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(expr, pexpr.stackFrameMap.getNumberOfVariables(), pexpr.stackFrameMap);
            pexpr.expression = expr;
            return pexpr;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.expr.XPathContextMajor c2 = context.newCleanContext();
            string exprText = arguments[0].head().getStringValue();
            Saxon.Eej.functions.extfn.Evaluate.PreparedExpression pexpr = prepareExpression(exprText, null, getStaticBaseUriString(), "saxon:evaluate", c2);
            c2.setOrigin(pexpr);
            c2.openStackFrame(pexpr.stackFrameMap);
            for (int i = 1; (i<arguments.Length); i++) {
                int slot = pexpr.variables[(i-1)].getLocalSlotNumber();
                c2.setLocalVariable(slot, arguments[i].materialize());
            }
            c2.setCurrentIterator(context.getCurrentIterator());
            return Saxon.Hej.om.SequenceTool.toLazySequence(pexpr.expression.iterate(c2));
        }
        public class PreparedExpression : Saxon.Hej.expr.ContextOriginator {
            public Saxon.Hej.sxpath.IndependentContext expStaticContext;
            public Saxon.Hej.expr.Expression expression;
            public Saxon.Hej.sxpath.XPathVariable[] variables;
            public Saxon.Hej.expr.instruct.SlotManager stackFrameMap;
        
        }
    
    }
}