////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions.hof;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.Choose;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.functions.Fold;
import net.sf.saxon.functions.FoldingFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AnyFunctionType;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.ObjectValue;

/**
 * This class implements the function fn:fold-left(), which is a standard function in XPath 3.0
 */

public class FoldLeftFn extends FoldingFunction {

    /**
     * Look for an "early exit" rewrite
     *
     * <p>Consider:</p>
     *
     * <p><code>fold-left(1 to $N, 0, function($sum, $item){
     *      if ($sum gt 1000)
     *      then $sum
     *      else $sum + $item})</code></p>
     *
     * <p>We can rewrite this for early exit as:</p>
     *
     <p><code>fold-left(1 to $N, 0, function($sum, $item){
     *      if ($sum gt 1000)
     *      then saxon:break-fold($sum)
     *      else $sum + $item})</code></p>
     *
     * <p>The saxon:break-fold() function wraps a value in a wrapper recognized by the Saxon implementation
     * as a signal to end the iteration. In principle it is available for use by user code.</p>
     * 
     * <p>In more detail, the optimization is possible if the callback function is an inline
     * function expression whose body is of the form <code>if (C) then (T) else (E)</code>,
     * where C has and T have no dependency on $item. </p>
     *
     * @param visitor     the expression visitor
     * @param contextInfo information about the context item
     * @param arguments   the supplied arguments to the function call. Note: modifying the contents
     *                    of this array should not be attempted, it is likely to have no effect.
     * @return the rewritten expression
     * @throws XPathException should not happen
     */

    public Expression makeOptimizedFunctionCall(
            ExpressionVisitor visitor, ContextItemStaticInfo contextInfo, Expression... arguments)
            throws XPathException {
        if (arguments[2] instanceof FunctionLiteral f &&
                f.getGroundedValue() instanceof InlineFunctionItem ifi &&
                ifi.getArity() == 2 &&
                ifi.getBody() instanceof Choose choice &&
                choice.size() == 2 &&
                !ExpressionTool.dependsOnVariable(choice.getCondition(0), ifi.getParameters()[1]) &&
                (  !ExpressionTool.dependsOnVariable(choice.getAction(0), ifi.getParameters()[1]) ||
                   !ExpressionTool.dependsOnVariable(choice.getAction(1), ifi.getParameters()[1]))) {

            visitor.obtainOptimizer().injectFoldBreak(ifi, visitor, this.getClass());
        }
        return super.makeOptimizedFunctionCall(visitor, contextInfo, arguments);
    }


    @Override
    public Fold getFold(XPathContext context, Sequence... arguments) throws XPathException {
        Sequence arg0 = arguments[0];
        return new FoldLeftFold(context, arg0.materialize(), (FunctionItem)arguments[1].head());
    }

    public static class FoldLeftFold implements Fold {

        private final XPathContext context;
        private final FunctionItem function;
        private Sequence data;
        private final Sequence[] argArray;

        public FoldLeftFold(XPathContext context, GroundedValue zero, FunctionItem function) {
            this.context = context;
            this.function = function;
            this.data = zero;
            argArray = new Sequence[function.getArity()];
        }

        @Override
        public void processItem(Item item) throws XPathException {
            if (argArray.length > 0) {
                argArray[0] = data;
            }
            if (argArray.length > 1) {
                argArray[1] = item;
            }
            data = dynamicCall(function, context, argArray).materialize();
        }

        @Override
        public boolean isFinished() {
            return data instanceof BreakResult;
        }

        @Override
        public Sequence result() {
            return data instanceof BreakResult breakResult
                ? breakResult.getObject() : data;
        }
    }

    /**
     * Get the return type, given knowledge of the actual arguments
     *
     * @param args the actual arguments supplied
     * @return the best available item type that the function will return
     */
    @Override
    public ItemType getResultItemType(Expression[] args) {
        // Item type of the result is the same as the result item type of the argument function
        ItemType functionArgType = args[2].getItemType();
        if (functionArgType instanceof AnyFunctionType) {
            // will always be true once the query has been successfully type-checked
            return ((AnyFunctionType) args[2].getItemType()).getResultType().getPrimaryType();
        } else {
            return AnyItemType.getInstance();
        }
    }

    public static class BreakResult extends ObjectValue<GroundedValue> {
        public BreakResult(GroundedValue result) {
            super(result);
        }
    }
}

// Copyright (c) 2013-2026 Saxonica Limited
