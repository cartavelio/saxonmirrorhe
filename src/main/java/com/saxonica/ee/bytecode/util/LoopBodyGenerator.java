package com.saxonica.ee.bytecode.util;

import net.sf.saxon.om.Item;

/**
 * LoopBodyGenerator is code to generate the body of a loop, for example of a ForExpression. When the ForExpression
 * is compiled as a loop, this callback specifies how to generate code for the loop body. For example, if the caller
 * is the compiler for the sum() function, the callback will increment the local variable holding the total so far.
 */
public abstract class LoopBodyGenerator {

    /**
     * Generate the code to process each item delivered by the looping expression. On entry to this code,
     * the relevant item will be on the top of the bytecode stack. On exit the stack will be empty (that is,
     * this item will have been removed)
     *
     * @param compiler the compiler service
     * @throws CannotCompileException if the expression cannot be compiled
     */

    public abstract void generateLoopBody(CompilerService compiler) throws CannotCompileException;

    /**
     * Say in what form the loop body generator expects the items to be delivered. This method returns
     * a class which gives the expected type of the item on the top of stack at the point where the
     * loop body code takes over. The default is that the item is on the stack as an instance of
     * {@link net.sf.saxon.om.Item}. The alternative is to ask for it to be placed as a primitive value,
     * for example of type Double.TYPE or Boolean.TYPE, or as a Java String or BigDecimal object
     *
     * @return the class of objects to be delivered
     */

    public Class getExpectedItemClass() {
        return Item.class;
    }

    /**
     * Say whether the amount of code generated is sufficiently small that we don't mind it being
     * generated more than once.
     *
     * @return true if the amount of code to be generated is small
     */

    public boolean isSmallAmountOfCode() {
        return false;
    }
}

//
// Copyright (c) 2011 Saxonica Limited. All rights reserved
