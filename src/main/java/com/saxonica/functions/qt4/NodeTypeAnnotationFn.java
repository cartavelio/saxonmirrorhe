package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.value.AtomicValue;

/**
 * This class supports the proposed QT4 function fn:schema-type.
 */

public class AtomicTypeAnnotationFn extends SystemFunction {


    /**
     * Evaluate this function call at run-time
     *
     * @param context   The XPath dynamic evaluation context
     * @param arguments The values of the arguments to the function call.
     * @return the results of the function, as a sequence of arrays
     * @throws XPathException if a dynamic error occurs during evaluation of the function. The Saxon run-time
     *                        code will add information about the error location.
     */

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        AtomicValue item = (AtomicValue) arguments[0].head();
        AtomicType annotation = item.getItemType();
        return SchemaTypeFn.makeSchemaRecord(annotation);
    }


}

// Copyright (c) 2024 Saxonica Limited
