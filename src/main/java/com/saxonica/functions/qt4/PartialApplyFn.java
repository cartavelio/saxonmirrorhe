////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.hof.CurriedFunction;
import net.sf.saxon.ma.map.KeyValuePair;
import net.sf.saxon.ma.map.MapItem;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.coercion.CoercionRules;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.SequenceType;

import java.util.function.Supplier;

/**
 * This class implements the function fn:partial-apply(), which is a standard function in XQuery 3.1.
 * The fn:apply function is also used internally to implement dynamic function calls.
 */

public class PartialApplyFn extends SystemFunction {

    // This flag is nonNull when the fn:apply() call actually implements a dynamic function call.
    // It only affects the error code/message if the call fails
    private String dynamicFunctionCall;

    public PartialApplyFn() {
    }
    

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments. The first argument is the function item
     *                  to be called; the second argument is a map containing
     *                  the arguments to be bound in that function, before conversion.
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        CoercionRules coercionRules = CoercionRules.forVersion(
                context.getConfiguration(),
                getRetainedStaticContext().getPackageData().getHostLanguageVersion());
        final FunctionItem function = (FunctionItem) arguments[0].head();
        final FunctionItemType signature = function.getFunctionItemType();
        final MapItem suppliedValues = (MapItem) arguments[1].head();
        Sequence[] boundArgs = new Sequence[function.getArity()];
        int numberOfBoundArgs = 0;
        for (KeyValuePair pair : suppliedValues.keyValuePairs()) {
            long key = ((IntegerValue)pair.key).longValue();
            if (key > 0 && key <= boundArgs.length) {
                int pos = (int)key - 1;
                SequenceType expected = signature.getArgumentTypes()[pos];
                Supplier<RoleDiagnostic> role = () -> new RoleDiagnostic(RoleDiagnostic.FUNCTION, "fn:apply", pos + 1);
                Sequence converted = coercionRules.coerce(
                        pair.value, expected, SequenceType.ANY_SEQUENCE, context.getConfiguration(), role, Loc.NONE);
                boundArgs[pos] = converted;
                numberOfBoundArgs++;
            }
        }
        int[] unboundArgs = new int[function.getArity() - numberOfBoundArgs];
        for (int i = 0; i<unboundArgs.length; i++) {
            unboundArgs[i] = i; // indicating that there is no re-ordering of unbound arguments
        }

        return new CurriedFunction(function, boundArgs, unboundArgs);

    }


}

