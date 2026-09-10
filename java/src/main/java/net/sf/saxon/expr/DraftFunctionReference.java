////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.elab.Elaborator;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.om.*;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpUsing;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.util.Map;


/**
 * This class represents a call to a function in its raw form, before it has been resolved to an
 * actual function.
 */
@CSharpUsing(code="Saxon.Hej.trans")
public class DraftFunctionCall extends DraftExpression {

    private final String lexicalName;
    private final NamespaceResolver resolver;
    private final NamespaceUri defaultFunctionNamespace;
    private final Operand[] operanda;
    private Map<StructuredQName, Integer> keywordArgs;

    /**
     * Create a function call to a user-written function in a query or stylesheet
     */

    public DraftFunctionCall(String lexicalName,
                             NamespaceResolver resolver,
                             NamespaceUri defaultFunctionNamespace,
                             Expression[] arguments) {
        this.lexicalName = lexicalName;
        this.resolver = resolver;
        this.defaultFunctionNamespace = defaultFunctionNamespace;
        this.operanda = new Operand[arguments.length];
        int i=0;
        for (Expression arg : arguments) {
            operanda[i++] = new Operand(this, arg, OperandRole.NAVIGATE);
        }
        this.keywordArgs = null; // TODO: implement this
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
        return new DraftFunctionCall(lexicalName, resolver, defaultFunctionNamespace, a2);
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
        throw new UnsupportedOperationException();
    }

    /**
     * Call the function, returning an iterator over the results. (But if the function is
     * tail recursive, it returns an iterator over the arguments of the recursive call)
     */

    /*@NotNull*/
    @Override
    public SequenceIterator iterate(XPathContext c) throws XPathException {
        throw new UnsupportedOperationException();
    }

    /**
     * Process the function call in push mode
     * @param output the destination for the result
     * @param context the XPath dynamic context
     * @throws XPathException if a dynamic error occurs
     */

    @Override
    public void process(Outputter output, XPathContext context) throws XPathException {
        throw new UnsupportedOperationException();
    }


    /**
     * Diagnostic print of expression structure. The abstract expression tree
     * is written to the supplied output destination.
     */

    @Override
    public void export(ExpressionPresenter out) throws XPathException {
        throw new UnsupportedOperationException();
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
        throw new UnsupportedOperationException();
    }

    @Override
    public Expression simplify(StaticContext env) throws XPathException {
        simplifyChildren(env);
        StructuredQName qName;
        int arity = operanda.length;
        FunctionLibrary lib = env.getFunctionLibrary();
        if (NameChecker.isValidNCName(lexicalName)) {
            if (env.getXPathVersion() >= 40) {
                // For an unprefixed function name in 4.0, we look first for a no-namespace name,
                // then for a name in the default function namespace.
                SymbolicName.F sn = new SymbolicName.F(new StructuredQName("",NamespaceUri.NULL, lexicalName), arity);
                if (lib.getFunctionItem(sn, env) != null) {
                    qName = sn.getComponentName();
                } else {
                    qName = new StructuredQName("", env.getDefaultFunctionNamespace(), lexicalName);
                }
            } else {
                qName = new StructuredQName("", env.getDefaultFunctionNamespace(), lexicalName);
            }
        } else {
            QNameParser qp = new QNameParser(resolver);
            qName = qp.parse(lexicalName, NamespaceUri.NULL);
        }

        if (env.getConfiguration().isDisabledFunction(qName)) {
            throw new XPathException("Function " + qName.getEQName() + " has been disabled in this Saxon Configuration", "XPST0017")
                    .withLocation(getLocation());
        }

        SymbolicName.F functionName = new SymbolicName.F(qName, arity);
        List<String> reasons = new ArrayList<>();
        Expression[] args = new Expression[operanda.length];
        for (int i=0; i<args.length; i++) {
            args[i] = operanda[i].getChildExpression();
        }
        Expression exp = lib.bind(functionName, args, keywordArgs, env, reasons);
        if (exp == null) {
            StringBuilder sb = new StringBuilder();
            sb.append("Cannot find a ").append(args.length).append(
                    "-argument function named ").append(functionName.getComponentName().getEQName()).append("()");
            for (String reason : reasons) {
                sb.append(". ").append(reason);
            }
            throw new XPathException(sb.toString(), "XPST0017").asStaticError();
        }
        return exp;
    }

}