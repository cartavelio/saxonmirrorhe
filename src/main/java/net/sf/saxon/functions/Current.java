////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;

/**
 * Implement the XSLT current() function
 */

public class Current extends SystemFunction {

    private ItemType contextItemType;

    /**
     * The name of the Current function
     */

    /*@NotNull*/ public static final StructuredQName FN_CURRENT =
        NamespaceUri.FN.qName("current");

    //@Override
    public FunctionItem bindContext(XPathContext context) {
//        Int64Value value;
//        try {
//            value = evaluateItem(context);
//        } catch (final XPathException e) {
//            // This happens when we do a dynamic lookup of position() or last() when there is no context item
//            SymbolicName.F name = new SymbolicName.F(getFunctionName(), getArity());
//            Callable callable = new CallableDelegate((context1, arguments) -> {
//                throw e;
//            });
//            return new CallableFunction(name, callable, getFunctionItemType());
//        }
//        ConstantFunction fn = new ConstantFunction(value);
//        fn.setDetails(getDetails());
//        fn.setRetainedStaticContext(getRetainedStaticContext());
//        return fn;
        return null;
    }

    /**
     * Method called during static type checking. This method may be implemented in subclasses so that functions
     * can take advantage of knowledge of the types of the arguments that will be supplied.
     *
     * @param visitor         an expression visitor, providing access to the static context and configuration
     * @param contextInfo information about whether the context item is set, and what its type is
     * @param arguments       the expressions appearing as arguments in the function call
     * @throws XPathException if an error is detected
     */
    @Override
    public void supplyTypeInformation(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo, Expression[] arguments) throws XPathException {
        this.contextItemType = contextInfo.getItemType();
    }

    /**
     * Determine the item type of the value returned by the function
     *
     * @return the item type of the result
     */
    @Override
    public ItemType getResultItemType(Expression[] args) {
        if (contextItemType != null) {
            return contextItemType;
        } else {
            return super.getResultItemType();
        }
    }

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as Sequences
     * @return the result of the evaluation, in the form of a Sequence
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        throw new XPathException("Dynamic evaluation of the current() function is not supported", "XTDE1360");
    }
}

