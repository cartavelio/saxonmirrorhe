package com.saxonica.ee.extfn;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.EmptySequence;

/**
 * Extension function saxon:schema-type
 */

public class SchemaTypeFn extends SystemFunction {

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
     * returned in the form of a {@link net.sf.saxon.tree.iter.SingletonIterator}. If the result is an empty sequence,
     * the method should return <code>EmptyIterator.getInstance()</code>
     * @throws net.sf.saxon.trans.XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                                           code will add information about the error location.
     */

    /*@Nullable*/
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        Item item = arguments[0].head();
        if (item == null) {
            return EmptySequence.getInstance();
        } else if (item instanceof NodeInfo) {
            NodeInfo node = (NodeInfo) item;
            SchemaType type = node.getSchemaType();
            if (type == null) {
                return EmptySequence.getInstance();
            }
            return type.getComponentAsFunction();
        } else if (item instanceof AtomicValue) {
            AtomicType type = ((AtomicValue) item).getItemType();
            return type.getComponentAsFunction();
        } else {
            return EmptySequence.getInstance();
        }
    }


}

// Copyright (c) 2020-2023 Saxonica Limited

