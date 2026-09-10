package com.saxonica.functions.extfn;

import net.sf.saxon.expr.ErrorExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.functions.registry.BuiltInFunctionSet;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.ZeroOrOne;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XmlProcessingException;
import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.value.StringItem;


/**
 * Implementation of the file:base-dir function in the EXPath file library.
 * Other functions in this library are implemented as reflexive extension
 * functions, but this one can't be implemented this way because it needs
 * access to the static context.
 */
public class EXPathFileFunctionSet extends BuiltInFunctionSet {

    private static final EXPathFileFunctionSet THE_INSTANCE = new EXPathFileFunctionSet();

    public static EXPathFileFunctionSet getInstance() {
        return THE_INSTANCE;
    }

    private EXPathFileFunctionSet() {
        init();
    }

    private void init() {

        register("base-dir", 0, BaseDir.class, AnyItemType.getInstance(), OPT, 0);

    }

    @Override
    public String getNamespace() {
        return EXPathFile.NAMESPACE;
    }

    @Override
    public String getConventionalPrefix() {
        return "file";
    }

    public static class BaseDir extends SystemFunction {

        /**
         * Make an expression that either calls this function, or that is equivalent to a call
         * on this function
         *
         * @param arguments the supplied arguments to the function call
         * @return either a function call on this function, or an expression that delivers
         * the same result
         */
        @Override
        public Expression makeFunctionCall(Expression... arguments) {
            if (getRetainedStaticContext().getPackageData().isRelocatable()) {
                return super.makeFunctionCall(arguments);
            } else {
                try {
                    return Literal.makeLiteral(makeResult(getStaticBaseUriString()));
                } catch (XPathException e) {
                    return new ErrorExpression(new XmlProcessingException(e));
                }
            }
        }

        /**
         * Invoke the function
         *
         * @param context the XPath dynamic evaluation context
         * @param args    the actual arguments to be supplied
         * @return the result of invoking the function
         * @throws XPathException if a dynamic error occurs within the function
         */
        @Override
        public Sequence call(XPathContext context, Sequence[] args) throws XPathException {
            return makeResult(getStaticBaseUriString());
        }

        private static ZeroOrOne<StringItem> makeResult(String baseURI) throws XPathException {
            if (baseURI == null) {
                return ZeroOrOne.empty();
            }
            String parent = EXPathFile._parent(baseURI);
            if (parent == null) {
                return ZeroOrOne.empty();
            }
            return new ZeroOrOne<>(StringItem.makeStringValue(parent));
        }

    }



}

// Copyright (c) 2018-2020 Saxonica Limited