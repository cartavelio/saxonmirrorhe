package com.saxonica.ee.bytecode.util;

/**
 * Calls to this class are inserted into the bytecode for debugging purposes
 */
public class ByteCodeTrace {

    /**
     * Implement a trace point
     *
     * @param n the number allocated to the trace point
     */

    public static void trace(int n) {
        System.err.println("Byte code trace point " + n);
    }

    /**
     * Output a simple message
     *
     * @param message the message
     */

    public static void showMessage(String message) {
        System.err.println("Message: " + message);
    }

    /**
     * Output a message to identify the contents of a variable (which must hold an Object)
     *
     * @param variable the name of the variable
     * @param value    the value of the variable
     */

    public static void showObjectVariable(String variable, String value) {
        System.err.println("Value of variable " + variable + " = " + value);
    }

    /**
     * Output a message to identify the contents of a variable (which must hold a long)
     *
     * @param variable the name of the variable
     * @param value    the value of the variable
     */

    public static void showLongVariable(String variable, long value) {
        System.err.println("Value of variable " + variable + " = " + value);
    }

    /**
     * Output a message to identify the contents of a variable (which must hold a int)
     *
     * @param variable the name of the variable
     * @param value    the value of the variable
     */

    public static void showIntVariable(String variable, int value) {
        System.err.println("Value of variable " + variable + " = " + value);
    }

    /**
     * Output a message to identify the contents of a variable (which must hold a double)
     *
     * @param variable the name of the variable
     * @param value    the value of the variable
     */

    public static void showDoubleVariable(double value, String variable) {
        System.err.println("Value of variable " + variable + " = " + value);
    }

    /**
     * Ad-hoc method for use as a debugging breakpoint
     *
     * @param obj not currently used
     */

    @SuppressWarnings({"UnusedParameters"})
    public static void breakpoint(Object obj) {
        System.err.println("Breakpoint");
    }
}

// Copyright (c) 2011-2022 Saxonica Limited

