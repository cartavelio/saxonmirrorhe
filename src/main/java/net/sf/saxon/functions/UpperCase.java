////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.elab.StringEvaluator;
import net.sf.saxon.expr.elab.UnicodeStringEvaluator;
import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.elab.StringElaborator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.ToUpper;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.StringValue;


/**
 * This class implements the fn:upper-case() function
 */

public class UpperCase extends ScalarSystemFunction {

    @Override
    public AtomicValue evaluate(Item arg, XPathContext context) {
        return StringValue.makeUStringValue(ToUpper.toUpper(arg.getUnicodeStringValue()));
    }

    @Override
    public Sequence resultWhenEmpty() {
        return StringValue.EMPTY_STRING;
    }

    @Override
    public String getCompilerName() {
        return "ForceCaseCompiler";
    }

    /**
     * Make an elaborator for a system function call on this function
     *
     * @return a suitable elaborator; or null if no custom elaborator is available
     */
    @Override
    public Elaborator getElaborator() {
        return new ForceCaseFnElaborator();
    }


    public static class ForceCaseFnElaborator extends StringElaborator {

        public UnicodeStringEvaluator elaborateForUnicodeString(boolean zeroLengthWhenAbsent) {
            final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
            final Expression arg = fnc.getArg(0);
            final UnicodeStringEvaluator argEval = makeElaborator(arg).elaborateForUnicodeString(false);
            final boolean nullable = Cardinality.allowsZero(arg.getCardinality());
            if (fnc.getFunctionName().getLocalPart().equals("lower-case")) {
                return context -> {
                    UnicodeString s0 = argEval.eval(context);
                    if (nullable && s0 == null) {
                        return handleNullUnicodeString(zeroLengthWhenAbsent);
                    }
                    return StringView.of(s0.toString().toLowerCase());
                };
            } else {
                return context -> {
                    UnicodeString s0 = argEval.eval(context);
                    if (nullable && s0 == null) {
                        return handleNullUnicodeString(zeroLengthWhenAbsent);
                    }
                    return StringView.of(s0.toString().toUpperCase());
                };
            }
        }

        public StringEvaluator elaborateForString(boolean zeroLengthWhenAbsent) {
            final SystemFunctionCall fnc = (SystemFunctionCall) getExpression();
            final Expression arg = fnc.getArg(0);
            final StringEvaluator argEval = makeElaborator(arg).elaborateForString(false);
            final boolean nullable = Cardinality.allowsZero(arg.getCardinality());
            if (fnc.getFunctionName().getLocalPart().equals("lower-case")) {
                return context -> {
                    String s0 = argEval.eval(context);
                    if (nullable && s0 == null) {
                        return handleNullString(zeroLengthWhenAbsent);
                    }
                    return s0.toLowerCase();
                };
            } else {
                return context -> {
                    String s0 = argEval.eval(context);
                    if (nullable && s0 == null) {
                        return handleNullString(zeroLengthWhenAbsent);
                    }
                    return s0.toUpperCase();
                };
            }
        }
    }
}

