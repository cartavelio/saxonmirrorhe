package com.saxonica.functions.extfn;

import net.sf.saxon.om.Sequence;
import net.sf.saxon.value.ObjectValue;


/**
 * This class implements the result of the saxon:array-member() extension function
 * or the saxon:array-member instruction
 */

public class ArrayMemberValue extends ObjectValue<Sequence> {

    public ArrayMemberValue(Sequence<?> sequence) {
        super(sequence);
    }

}


// Copyright (c) 2018 Saxonica Limited.
