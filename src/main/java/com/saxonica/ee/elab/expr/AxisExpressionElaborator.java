package com.saxonica.ee.elab.expr;

import com.saxonica.ee.elab.eval.PullEvaluator;
import com.saxonica.ee.elab.gen.PullElaborator;
import net.sf.saxon.expr.AxisExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Genre;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.trans.XPathException;

/**
 * Elaborator for an AxisExpression
 */

public class AxisExpressionElaborator extends PullElaborator {

    private void reportDoesNotExist(Expression expression, XPathContext context) throws XPathException {
        XPathException err = new XPathException("The context item for axis step " +
                                                        expression + " is absent");
        err.setErrorCode("XPDY0002");
        err.setXPathContext(context);
        err.setLocation(expression.getLocation());
        err.setIsTypeError(true);
        throw err;
    }

    private void reportIsNotNode(Expression expression, XPathContext context) throws XPathException {
        XPathException err = new XPathException("The context item for axis step " +
                                                        expression + " is not a node");
        err.setErrorCode("XPTY0020");
        err.setXPathContext(context);
        err.setLocation(expression.getLocation());
        err.setIsTypeError(true);
        throw err;
    }

    @SuppressWarnings("DuplicatedCode")
    @Override
    public PullEvaluator elaborateForPull() {
        AxisExpression axisExpression = (AxisExpression) getExpression();
        NodeTest test = axisExpression.getNodeTest();
        int axis = axisExpression.getAxis();
        // These variables are computed in the hope that the optimizer will remove runtime error tests
        // that aren't needed because the condition cannot occur
        boolean checkContextItemExists = axisExpression.isContextPossiblyUndefined();
        boolean checkContextItemIsNode = axisExpression.getContextItemType().getGenre() != Genre.NODE;
        if (test instanceof AnyNodeTest) {
            return context -> {
                Item item = context.getContextItem();
                if (checkContextItemExists && item == null) {
                    reportDoesNotExist(axisExpression, context);
                }
                if (checkContextItemIsNode && !(item instanceof NodeInfo)) {
                    reportIsNotNode(axisExpression, context);
                }
                assert item != null;
                return ((NodeInfo)item).iterateAxis(axis);
            };
        } else {
            return context -> {
                Item item = context.getContextItem();
                if (checkContextItemExists && item == null) {
                    reportDoesNotExist(axisExpression, context);
                }
                if (checkContextItemIsNode && !(item instanceof NodeInfo)) {
                    reportIsNotNode(axisExpression, context);
                }
                assert item != null;
                return ((NodeInfo) item).iterateAxis(axis, test);
            };
        }
    }


}

