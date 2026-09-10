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
import net.sf.saxon.type.ItemType;

public class AxisExpressionElaborator extends PullElaborator {

    public void reportDoesNotExist(Expression expression, XPathContext context) throws XPathException {
        XPathException err = new XPathException("The context item for axis step " +
                                                        expression + " is absent");
        err.setErrorCode("XPDY0002");
        err.setXPathContext(context);
        err.setLocation(expression.getLocation());
        err.setIsTypeError(true);
        throw err;
    }

    public void reportIsNotNode(Expression expression, XPathContext context) throws XPathException {
        XPathException err = new XPathException("The context item for axis step " +
                                                        expression + " is not a node");
        err.setErrorCode("XPTY0020");
        err.setXPathContext(context);
        err.setLocation(expression.getLocation());
        err.setIsTypeError(true);
        throw err;
    }

    @Override
    public PullEvaluator getPullEvaluator() {
        AxisExpression axisExpression = (AxisExpression) getExpression();
        NodeTest test = axisExpression.getNodeTest();
        int axis = axisExpression.getAxis();
        boolean checkContextItemExists = axisExpression.isContextPossiblyUndefined();
        ItemType contextItemType = axisExpression.getContextItemType();
        boolean checkContextItemIsNode = contextItemType.getGenre() != Genre.NODE;
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

