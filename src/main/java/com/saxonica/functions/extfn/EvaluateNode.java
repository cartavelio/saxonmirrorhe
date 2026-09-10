package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;

/**
 * This class implements the saxon:evaluate-node() extension function
 */

public class EvaluateNode extends Evaluate {


//        protected PreparedExpression prepareExpression(NodeInfo node, String functionName, XPathContext context) throws XPathException {
//            Configuration config = context.getConfiguration();
//            IndependentContext env = new IndependentContext(staticContext);
//            Executable exec = context.getController().getExecutable();
//            PreparedExpression pexpr = new PreparedExpression();
//            pexpr.expStaticContext = env;
//            env.setBaseURI(node.getBaseURI());
//            env.setFunctionLibrary(exec.getFunctionLibrary());
//            env.setNamespaces(node);
//
//
//            String exprText = node.getStringValue();
//            Expression expr;
//            try {
//                expr = ExpressionTool.make(exprText, env, 0, Token.EOF, null);
//                //expr.setContainer(env);
//            } catch (XPathException e) {
//                XPathException err = new XPathException("Static error in XPath expression supplied to " +
//                        functionName + ": " + e.getMessage().trim());
//                err.setXPathContext(context);
//                throw err;
//            }
//            ItemType contextItemType = Type.ITEM_TYPE;
//            ExpressionVisitor visitor = ExpressionVisitor.make(env);
//            expr = expr.typeCheck(visitor, config.makeContextItemStaticInfo(contextItemType, true));
//            pexpr.stackFrameMap = env.getStackFrameMap();
//            ExpressionTool.allocateSlots(expr, pexpr.stackFrameMap.getNumberOfVariables(), pexpr.stackFrameMap);
//            pexpr.expression = expr;
//            //expr.setContainer(env);
//            return pexpr;
//
//        }
//

        public Sequence<?> call(XPathContext context, Sequence[] arguments) throws XPathException {
            XPathContextMajor c2 = context.newCleanContext();
            NodeInfo node = (NodeInfo) arguments[0].head();
            Evaluate.PreparedExpression pexpr = prepareExpression(
                    node.getStringValue(), node, node.getBaseURI(), "saxon:evaluate-node", c2);

            c2.setOrigin(pexpr);
            c2.openStackFrame(pexpr.stackFrameMap);
            ManualIterator<?> single = new ManualIterator<>(node);
            c2.setCurrentIterator(single);
            return SequenceTool.toLazySequence(pexpr.expression.iterate(c2));
        }
//    }

}

// Copyright (c) 2009 Saxonica Limited.
