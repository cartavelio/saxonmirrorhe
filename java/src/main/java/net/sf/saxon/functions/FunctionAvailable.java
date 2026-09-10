////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.expr.parser.RetainedStaticContext;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.NumericValue;

/**
 * This class implements the XSLT function-available functions.
 */

public class FunctionAvailable extends SystemFunction {

    @Override
    public Expression makeFunctionCall(Expression... arguments) {
        PackageData pack = getRetainedStaticContext().getPackageData();
        if (pack instanceof StylesheetPackage) {
            ((StylesheetPackage) pack).setRetainUnusedFunctions();
        }
        return super.makeFunctionCall(arguments);
    }

    /**
     * Allow the function to create an optimized call based on the values of the actual arguments
     *
     * @param visitor     the expression visitor
     * @param contextInfo information about the context item
     * @param arguments   the supplied arguments to the function call
     * @return either a function call on this function, or an expression that delivers
     * the same result, or null indicating that no optimization has taken place
     * @throws net.sf.saxon.trans.XPathException if an error is detected
     */
    @Override
    public Expression makeOptimizedFunctionCall(ExpressionVisitor visitor, ContextItemStaticInfo contextInfo, Expression... arguments) throws XPathException {

        // Note, the LATE property is set in the function details to avoid the function being evaluated by the preEvaluate() call.
        // This is because the full static context is needed, not the (smaller) RetainedStaticContext. Instead, pre-evaluation
        // for calls with fixed arguments is done during the optimization phase, which makes the full static context available.

        if (arguments[0] instanceof Literal lit
                && lit.getGroundedValue() instanceof AtomicValue av
                && (arguments.length == 1 || arguments[1] instanceof Literal)) {
            StaticContext env = visitor.getStaticContext();
            StructuredQName qName;
            try {
                qName = processStringOrQNameArgument(av, env.getDefaultFunctionNamespace());
            } catch (XPathException e) {
                throw e.withErrorCode("XTDE1400");
            }
            boolean b = false;

            if (env.getConfiguration().isDisabledFunction(qName)) {
                return Literal.makeLiteral(BooleanValue.FALSE);
            }

            int minArity = 0;
            int maxArity = 20;
            if (getArity() == 2) {
                NumericValue arg2 = ((NumericValue) arguments[1].evaluateItem(env.makeEarlyEvaluationContext()));
                if (arg2 != null) {
                    // empty sequence is allowed in 4.0
                    minArity = (int) ((NumericValue) arguments[1].evaluateItem(env.makeEarlyEvaluationContext())).longValue();
                    maxArity = minArity;
                }
            }

            for (int i = minArity; i <= maxArity; i++) {
                SymbolicName.F sn = new SymbolicName.F(qName, i);
                if (env.getFunctionLibrary().isAvailable(sn, env.getImportedSchema(), env.getXPathVersion())) {
                    b = true;
                    break;
                }
            }

            return Literal.makeLiteral(BooleanValue.get(b));
        } else {
            return null;
        }
    }

    private boolean isFunctionAvailable(StructuredQName functionName, RetainedStaticContext rsc, int arity, XPathContext context) throws XPathException {
        if (arity == -1) {
            for (int i = 0; i < 20; i++) {
                if (isFunctionAvailable(functionName, rsc, i, context)) {
                    return true;
                }
            }
            return false;
        }

        if (context.getConfiguration().isDisabledFunction(functionName)) {
            return false;
        }

        final FunctionLibrary lib = context.getController().getExecutable().getFunctionLibrary();
        SymbolicName.F sn = new SymbolicName.F(functionName, arity);
        // TODO: reinstate something along these lines. Removed because it doesn't build in HE 9.8
//        if (known && qName.hasURI(NamespaceConstant.FN) && !context.getConfiguration().getEditionCode().equals(edition)) {
//            // Target environment differs from compile-time environment: some functions might not be available
//            BuiltInFunctionSet.Entry details = XPath31HOFunctionSet.getInstance().getFunctionDetails(qName.getLocalPart(), sn.getArity());
//            if (details != null) {
//                if (((details.applicability & BuiltInFunctionSet.HOF) != 0) && ("HE".equals(edition) || "JS".equals(edition))) {
//                    return false;
//                }
//                // TODO: some further functions are not available in SaxonJS
//            }
//        }
        return lib.isAvailable(sn, rsc.getImportedSchema(), rsc.getPackageData().getHostLanguageVersion());
    }

    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public BooleanValue call(XPathContext context, Sequence[] arguments) throws XPathException {
        StructuredQName functionName = processStringOrQNameArgument(((AtomicValue)(arguments[0].head())), NamespaceUri.FN);
        int arity = -1;
        if (arguments.length == 2) {
            NumericValue arg2 = ((NumericValue) arguments[1].head());
            if (arg2 != null) {
                // Empty sequence is allowed in 4.0
                arity = (int) arg2.longValue();
            }
        }
        return BooleanValue.get(
                isFunctionAvailable(functionName, getRetainedStaticContext(), arity, context));
    }
}

