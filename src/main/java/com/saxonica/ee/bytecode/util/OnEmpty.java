package com.saxonica.ee.bytecode.util;

import net.sf.saxon.str.EmptyUnicodeString;

import java.util.Stack;

/**
 * The OnEmpty class represents an action to be taken by the bytecode generator when compiling an expression
 * to return a "primitive" (unboxed) value such as a double float int long or String (treated as primitive for this
 * purpose). There are a number of defined subclasses and these are known to users of the class; it is not possible
 * to introduce new subclasses without careful examination of existing code.
 */
public abstract class OnEmpty {

    public static final OnEmpty RETURN_EMPTY_STRING = new ReturnEmptyString();
    public static final OnEmpty RETURN_EMPTY_UNICODE_STRING = new ReturnEmptyUnicodeString();
    public static final OnEmpty RETURN_NULL = new ReturnNull();

    public abstract void generate(Generator ga);

    /**
     * The UnwindAndJump subclass causes the code generator, when an expression evaluates to the empty sequence,
     * to generate a "goto" instruction to a defined label; but before executing the jump, the stack will be unwound
     * to a known position.
     */

    public static class UnwindAndJump extends OnEmpty {

        LabelInfo label;
        /*@Nullable*/ Stack<Class<?>> theUnwindStack;

        /**
         * @param unwindStack Information about the classes of object that will be on the bytecode stack and must be
         *                    popped off the stack before jumping to the onEmpty label: the generated code must pop the items described
         *                    on this stack before jumping to the label.
         * @param label       a label to which control will be transferred (with nothing on the stack) if the result of the
         *                    evaluation is an empty sequence. This mechanism supports the design pattern whereby many expressions return
         *                    empty if one of there arguments is empty; rather than repeatedly checking for empty after evaluating each
         *                    subexpression, there is a "longjump" treating this as an exception case.
         */

        public UnwindAndJump(Stack<Class<?>> unwindStack, LabelInfo label) {
            this.label = label;
            this.theUnwindStack = unwindStack;
        }

        /**
         * Generate an UnwindAndStack action using an empty stack (no unwinding needed)
         *
         * @param label the label to jump to
         */

        public UnwindAndJump(LabelInfo label) {
            this.label = label;
            this.theUnwindStack = null;
        }

        /**
         * Generate code. (This method is not extensively used)
         *
         * @param ga the code generator
         */

        @Override
        public void generate(Generator ga) {
            Stack<Class<?>> u = theUnwindStack;
            if (u != null) {
                for (int i = u.size() - 1; i >= 0; i--) {
                    Class<?> c = u.get(i);
                    if (c == Double.TYPE || c == Long.TYPE) {
                        ga.pop2();
                    } else {
                        ga.pop();
                    }
                }
            }
            ga.goTo(label);
        }

        /**
         * Get the label acting as the jump destination
         *
         * @return the label
         */

        public LabelInfo getLabel() {
            return label;
        }

        /**
         * Get information about the stack that needs to be unwound before making the jump. This contains
         * information about the class of object at each position, because doubles and longs occupy two words
         * and need a pop2 instruction to remove them
         *
         * @return details of the stack to be unwound
         */

        /*@NotNull*/
        public Stack<Class<?>> getUnwindStack() {
            if (theUnwindStack == null) {
                theUnwindStack = new Stack<>();
            }
            return theUnwindStack;
        }

    }

    /**
     * The ReturnNull subclass indicates that when an expression evaluates to the empty sequence, the value should
     * be returned as a Java null. This works only for types represented as Java objects, that is, String and
     * CharSequence.
     */

    public static class ReturnNull extends OnEmpty {
        @Override
        public void generate(Generator ga) {
            ga.pushNull();
        }
    }

    /**
     * The ReturnEmptyString subclass indicates that when an expression evaluates to the empty sequence, the value should
     * be returned as a zero-length string. This works only for expressions that evaluate to a string.
     */

    public static class ReturnEmptyString extends OnEmpty {
        @Override
        public void generate(Generator ga) {
            ga.push("");
        }
    }

    /**
     * The ReturnEmptyString subclass indicates that when an expression evaluates to the empty sequence, the value should
     * be returned as a zero-length string. This works only for expressions that evaluate to a string.
     */

    public static class ReturnEmptyUnicodeString extends OnEmpty {
        @Override
        public void generate(Generator ga) {
            ga.invokeStaticMethod(EmptyUnicodeString.class, "getInstance");
        }
    }
}

// Copyright (c) 2011-2022 Saxonica Limited


