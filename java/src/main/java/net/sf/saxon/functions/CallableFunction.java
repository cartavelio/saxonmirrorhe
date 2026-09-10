////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.query.AnnotationList;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.SpecificFunctionType;

/**
 * A function item that wraps a Callable
 */

public class CallableFunction extends AbstractFunction {

    private final CallableDelegate.Lambda callable;
    private final SymbolicName.F name;
    private FunctionItemType type;
    private AnnotationList annotations;
    private String description;

    public CallableFunction(SymbolicName.F name, CallableDelegate.Lambda callable, FunctionItemType type) {
        this.name = name;
        this.callable = callable;
        this.type = type;
    }

    public CallableFunction(CallableDelegate.Lambda callable, SpecificFunctionType type) {
        this.name = new SymbolicName.F(NamespaceUri.ANONYMOUS.qName("anon"), type.getArity());
        this.callable = callable;
        this.type = type;
    }

    public CallableFunction withDescription(String decription) {
        this.description = decription;
        return this;
    }

    /**
     * Get the item type of the function item
     *
     * @return the function item's type
     */
    @Override
    public FunctionItemType getFunctionItemType() {
//        if (type == AnyFunctionType.getInstance() && callable instanceof XQueryFunctionLibrary.UnresolvedCallable) {
//            UserFunction uf = ((XQueryFunctionLibrary.UnresolvedCallable) callable).getFunction();
//            if (uf != null) {
//                // the previously unresolved function reference is now resolved
//                if (uf.isVariadic()) {
//                    SequenceType[] argTypes = new SequenceType[getArity()];
//                    int last = uf.getArity() - 1;
//                    System.arraycopy(uf.getFunctionItemType().getArgumentTypes(), 0, argTypes, 0, last);
//                    SequenceType lastArgType = uf.getFunctionItemType().getArgumentTypes()[last];
//                    Arrays.fill(argTypes, last, argTypes.length, lastArgType);
//                    return new SpecificFunctionType(argTypes, uf.getResultType());
//                }
//                type = uf.getFunctionItemType();
//            }
//        }
        return type;
    }

    /**
     * Get the name of the function, or null if it is anonymous
     *
     * @return the function name, or null for an anonymous inline function
     */
    @Override
    public StructuredQName getFunctionName() {
        return name.getComponentName();
    }

    /**
     * Get a description of this function for use in error messages. For named functions, the description
     * is the function name (as a lexical QName). For others, it might be, for example, "inline function",
     * or "partially-applied ends-with function".
     *
     * @return a description of the function for use in error messages
     */
    @Override
    public String getDescription() {
        if (description != null) {
            return description;
        }
        if (getFunctionName() != null) {
            return "function " + getFunctionName();
        }
        return "anonymous function of type " + getFunctionItemType();
    }

    /**
     * Get the arity of the function
     *
     * @return the number of arguments in the function signature
     */
    @Override
    public int getArity() {
        return name.getArity();
    }

    public void setAnnotations(AnnotationList annotations) {
        this.annotations = annotations;
    }

    @Override
    public AnnotationList getAnnotations() {
        return annotations;
    }

    /**
     * Invoke the function
     *
     * @param context the XPath dynamic evaluation context
     * @param args    the actual arguments to be supplied
     * @return the result of invoking the function
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic error occurs within the function
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] args) throws XPathException {
        return callable.call(context, args);
    }


    /**
     * Output information about this function item to the diagnostic explain() output
     *
     * @param out the destination for the output
     */
    @Override
    public void export(ExpressionPresenter out) {
        throw new UnsupportedOperationException("A CallableFunction is a transient value that cannot be exported");
    }
}
