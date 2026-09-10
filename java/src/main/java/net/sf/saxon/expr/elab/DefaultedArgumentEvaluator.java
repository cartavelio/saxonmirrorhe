////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2023-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr.elab;

import net.sf.saxon.Controller;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.instruct.Executable;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.functions.Current;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ManualIterator;

/**
 * A {@link SequenceEvaluator} for default function arguments that depend on
 * the value of current(), which is defined to return the context item of the
 * function call
 */

public class DefaultedArgumentEvaluator implements SequenceEvaluator {

    final UserFunction target;
    final Expression defaultArgumentExpression;

    public DefaultedArgumentEvaluator(UserFunction target, Expression expr) {
        this.defaultArgumentExpression = expr;
        this.target = target;
    }

    /**
     * Evaluate a construct to produce a value (which might be a lazily evaluated Sequence)
     *
     * @param context the evaluation context
     * @return a Sequence (not necessarily grounded)
     * @throws XPathException if a dynamic error occurs during the evaluation.
     */
    @Override
    public Sequence evaluate(XPathContext context) throws XPathException {
        // Special case for the expression current()
        if (defaultArgumentExpression.isCallOn(Current.class)) {
            return context.getContextItem();
        }

        Controller controller = context.getController();
        Executable exec = controller.getExecutable();
        boolean hasAccessToGlobalContext = true;
//        if (exec instanceof PreparedStylesheet) {
//            hasAccessToGlobalContext = target == null || target.getDeclaringPackage() == ((PreparedStylesheet) exec).getTopLevelPackage();
//        }
        XPathContextMajor c2 = context.newCleanContext();
        if (hasAccessToGlobalContext) {
            Item globalContextItem = context.getController().getGlobalContextItem();
            ManualIterator mi = new ManualIterator(globalContextItem);
            c2.setCurrentIterator(mi);
            c2.setCurrentValue(globalContextItem);
        } else {
            c2.setCurrentIterator(null);
        }
//        if (getStackFrameMap() != null) {
//            c2.openStackFrame(getStackFrameMap());
//        }
//        c2.setCurrentComponent(target);
//        int savedOutputState = c2.getTemporaryOutputState();
        c2.setTemporaryOutputState(StandardNames.XSL_VARIABLE);
        c2.setCurrentOutputUri(null);
        c2.setCurrentValue(context.getContextItem());
        return defaultArgumentExpression.makeElaborator().eagerly().evaluate(c2);
    }




}
