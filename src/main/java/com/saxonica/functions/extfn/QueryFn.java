package com.saxonica.functions.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.query.DynamicQueryContext;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.AnyExternalObject;

/**
 * This class implements the saxon:query() extension function
 */

public class QueryFn extends SystemFunction {


        /**
         * Evaluate this function call at run-time
         *
         * @param context   The XPath dynamic evaluation context
         * @param arguments The values of the arguments to the function call. Each argument value (which is in general
         *                  a sequence) is supplied in the form of an iterator over the items in the sequence. If required, the
         *                  supplied sequence can be materialized by calling, for example, <code>new SequenceExtent(arguments[i])</code>.
         *                  If the argument is always a singleton, then the single item may be obtained by calling
         *                  <code>arguments[i].next()</code>. The implementation is not obliged to read all the items in each
         *                  <code>SequenceIterator</code> if they are not required to compute the result; but if any SequenceIterator is not read
         *                  to completion, it is good practice to call its close() method.
         * @return an iterator over the results of the function. If the result is a single item, it can be
         *         returned in the form of a {@link net.sf.saxon.tree.iter.SingletonIterator}. If the result is an empty sequence,
         *         the method should return <code>EmptyIterator.getInstance()</code>
         * @throws net.sf.saxon.trans.XPathException
         *          if a dynamic error occurs during evaluation of the function. The Saxon run-time
         *          code will add information about the error location.
         */

        /*@Nullable*/
        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
           AnyExternalObject queryExpression = (AnyExternalObject) arguments[0].head();
            if (queryExpression == null) {
                return null;
            }
            XQueryExpression exp = (XQueryExpression) queryExpression.getWrappedObject();
            DynamicQueryContext dqc = new DynamicQueryContext(context.getConfiguration());

            if (arguments.length >= 2) {
                Item source = arguments[1].head();
                if (source != null) {
                    dqc.setContextItem(source);
                }
            } else {
                Item co = context.getContextItem();
                if (co != null) {
                    dqc.setContextItem(context.getContextItem());
                }
            }

            if (arguments.length == 3) {
                SequenceIterator iter = arguments[2].iterate();
                Item param;
                while ((param = iter.next()) != null) {
                    if (param instanceof NodeInfo) {
                        switch (((NodeInfo) param).getNodeKind()) {
                            case Type.ELEMENT:
                            case Type.ATTRIBUTE:
                                AtomicSequence val = param.atomize();
                                dqc.setParameter(NameOfNode.makeName((NodeInfo) param).getStructuredQName(), val);
                                break;
                            case Type.DOCUMENT:
                                for (NodeInfo kid : ((NodeInfo) param).children(NodeKindTest.ELEMENT)) {
                                    AtomicSequence val2 = kid.atomize();
                                    dqc.setParameter(NameOfNode.makeName(kid).getStructuredQName(), val2);
                                }
                                break;
                            default:
                                throw new XPathException(
                                        "Parameters passed to saxon:query() must be element, attribute, or document nodes");
                        }

                    } else {
                        throw new XPathException("Parameters passed to saxon:query() must be nodes");
                    }
                }
            }
            return SequenceTool.toLazySequence(exp.iterator(dqc));
        }


}

// Copyright (c) 2009-2023 Saxonica Limited
