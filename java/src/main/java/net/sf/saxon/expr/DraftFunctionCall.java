////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.expr.parser.XPathParser;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.IContextAccessorFunction;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.registry.XPath31FunctionSet;
import net.sf.saxon.om.*;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.query.XQueryParser;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpUsing;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.QNameValue;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Map;


/**
 * This class represents a call to a function in its raw lexical form, before it has been resolved to an
 * actual function. Used in XQuery only, where function references can be forwards references to functions not
 *  * yet declared.
 */
@CSharpUsing(code="Saxon.Hej.trans")
public class DraftFunctionCall extends DraftExpression {

    private final String lexicalName;
    private final QueryModule queryModule;
    private final Operand[] operanda;
    private final List<Object> placeMarkerArgs;
    private final Map<StructuredQName, Integer> keywordArgs;

    /**
     * Create a function call to a user-written function in a query or stylesheet
     */

    public DraftFunctionCall(String lexicalName,
                             QueryModule queryModule,
                             Expression[] arguments,
                             List<Object> placeMarkerArgs,
                             Map<StructuredQName, Integer> keywordArgs) {
        this.lexicalName = lexicalName;
        this.queryModule = queryModule;
        this.operanda = new Operand[arguments.length];
        int i=0;
        for (Expression arg : arguments) {
            operanda[i++] = new Operand(this, arg, OperandRole.NAVIGATE);
        }
        this.placeMarkerArgs = placeMarkerArgs;
        this.keywordArgs = keywordArgs;
    }

    @Override
    public Iterable<Operand> operands() {
        return Arrays.asList(operanda);
    }

    @Override
    public int getIntrinsicDependencies() {
        return StaticProperty.DEPENDS_ON_USER_FUNCTIONS;
    }

    /**
     * Determine whether this is an updating expression as defined in the XQuery update specification
     *
     * @return true if this is an updating expression
     */

    @Override
    public boolean isUpdatingExpression() {
        return false; // we don't actually know yet
    }

    /**
     * Copy an expression. This makes a deep copy.
     *
     * @param rebindings variable bindings that need to be changed
     * @return the copy of the original expression
     */

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        Expression[] a2 = new Expression[operanda.length];
        int i=0;
        for (Operand operand : operanda) {
            a2[i++] = operand.getChildExpression().copy(rebindings);
        }
        return new DraftFunctionCall(lexicalName, queryModule, a2, placeMarkerArgs, keywordArgs);
    }

    private Expression[] getArguments() {
        Expression[] a2 = new Expression[operanda.length];
        int i = 0;
        for (Operand operand : operanda) {
            a2[i++] = operand.getChildExpression();
        }
        return a2;
    }

    /**
     * Determine the cardinality of the result
     */

    @Override
    protected int computeCardinality() {
        return StaticProperty.ALLOWS_ZERO_OR_MORE;
    }

    /**
     * Call the function, returning the value as an item. This method will be used
     * only when the cardinality is zero or one. If the function is tail recursive,
     * it returns an Object representing the arguments to the next (recursive) call
     */

    @Override
    public Item evaluateItem(XPathContext c) throws XPathException {
        throw cannotExecute();
    }

    /**
     * Call the function, returning an iterator over the results. (But if the function is
     * tail recursive, it returns an iterator over the arguments of the recursive call)
     */

    /*@NotNull*/
    @Override
    public SequenceIterator iterate(XPathContext c) throws XPathException {
        throw cannotExecute();
    }

    /**
     * Process the function call in push mode
     * @param output the destination for the result
     * @param context the XPath dynamic context
     * @throws XPathException if a dynamic error occurs
     */

    @Override
    public void process(Outputter output, XPathContext context) throws XPathException {
        throw cannotExecute();
    }


    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        throw cannotExecute();
    }

    /**
     * Get a name identifying the kind of expression, in terms meaningful to a user.
     *
     * @return a name identifying the kind of expression, in terms meaningful to a user.
     * The name will always be in the form of a lexical XML QName, and should match the name used
     * in explain() output displaying the expression.
     */
    @Override
    public String getExpressionName() {
        return lexicalName;
    }

    @Override
    public int getImplementationMethod() {
        return ITERATE_METHOD;
    }


    @Override
    public Elaborator getElaborator() {
        throw cannotExecute();
    }

    private UnsupportedOperationException cannotExecute() {
        return new UnsupportedOperationException("Internal error. Function call " + toShortString() + " has not been resolved.");
    }

    /**
     * The {@code simplify} method applied to a DraftFunctionCall when the entire query has been parsed, so all user-defined
     * functions are now known, enabling the name to be resolved.
     * @return either a FunctionCall or a CurriedFunction
     * @throws XPathException if resolution fails
     */
    @Override
    public Expression simplify() throws XPathException {
        simplifyChildren();
        StructuredQName qName;
        int arity = operanda.length;
        FunctionLibrary lib = queryModule.getFunctionLibrary();
        NamespaceUri defaultFunctionNamespace = queryModule.getDefaultFunctionNamespace();
        if (NameChecker.isValidNCName(lexicalName)) {
            if (queryModule.getXPathVersion() >= 40) {
                // For an unprefixed function name in 4.0, we look first for a no-namespace name,
                // then for a name in the default function namespace.
                SymbolicName.F sn = new SymbolicName.F(new StructuredQName("",NamespaceUri.NULL, lexicalName), arity);
                if (lib.getFunctionItem(sn, queryModule) != null) {
                    qName = sn.getComponentName();
                } else {
                    qName = new StructuredQName("", defaultFunctionNamespace, lexicalName);
                }
            } else {
                qName = new StructuredQName("", defaultFunctionNamespace, lexicalName);
            }
        } else {
            NamespaceResolver resolver = getRetainedStaticContext();
            QNameParser qp = new QNameParser(resolver)
                    .withAcceptEQName(true, queryModule.getXPathVersion())
                    .withUnescaper(new XQueryParser.Unescaper(queryModule.getConfiguration().getValidCharacterChecker()));
            qName = qp.parse(lexicalName, NamespaceUri.NULL);
        }

        if (queryModule.getConfiguration().isDisabledFunction(qName)) {
            throw new XPathException("Function " + qName.getEQName() + " has been disabled in this Saxon Configuration", "XPST0017")
                    .withLocation(getLocation());
        }

        if (placeMarkerArgs != null && !placeMarkerArgs.isEmpty()) {
            return makeCurriedFunction(qName, getArguments(), placeMarkerArgs);
        }

        SymbolicName.F functionName = new SymbolicName.F(qName, arity);
        List<String> reasons = new ArrayList<>();
        Expression[] args = getArguments();
        Expression exp = lib.bind(functionName, args, keywordArgs, queryModule, reasons);
        if (exp == null) {
            StringBuilder sb = new StringBuilder();
            sb.append("Cannot find a ").append(args.length).append(
                    "-argument function named ").append(functionName.getComponentName().getEQName()).append("()");
            for (String reason : reasons) {
                sb.append(". ").append(reason);
            }
            throw new XPathException(sb.toString(), "XPST0017").asStaticError();
        }
        exp.setRetainedStaticContext(getRetainedStaticContext());
        ExpressionTool.copyLocationInfo(this, exp);
        return exp;
    }

    private Expression makeCurriedFunction(
            StructuredQName name,
            Expression[] args,
            List<Object> placeMarkerArgs) throws XPathException {

        FunctionLibrary lib = queryModule.getFunctionLibrary();
        SymbolicName.F sn = new SymbolicName.F(name, args.length);
        FunctionItem target = lib.getFunctionItem(sn, queryModule);
        if (target == null) {
            String msg = "Cannot find a " + args.length
                    + "-argument function named " + name.getEQName() + "()";
            throw new XPathException(msg, "XPST0017").asStaticError();
        }
        if (target instanceof IContextAccessorFunction) {
            // For a context-dependent function, return a call on function-lookup(), which saves the context
            SystemFunction lookup = XPath31FunctionSet.getInstance().makeFunction("function-lookup", 2);
            lookup.setRetainedStaticContext(queryModule.makeRetainedStaticContext());
            return lookup.makeFunctionCall(Literal.makeLiteral(new QNameValue(name, BuiltInAtomicType.QNAME)),
                                           Literal.makeLiteral(Int64Value.makeIntegerValue(args.length)));

        }
        Expression targetExp = XPathParser.makeNamedFunctionReference(name, target);
        ExpressionTool.copyLocationInfo(this, targetExp);
        return XPathParser.curryFunction(targetExp, args, placeMarkerArgs);
    }


}