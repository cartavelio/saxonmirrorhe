package com.saxonica.ee.bytecode.util;

import net.sf.saxon.om.Item;
import org.objectweb.asm.*;
import org.objectweb.asm.commons.GeneratorAdapter;
import org.objectweb.asm.commons.Method;

import java.lang.reflect.Modifier;

/**
 * This class is an extension of the ASM-supplied GeneratorAdapter library, containing additional
 * utility and shortcut methods for bytecode generation
 */
public class Generator extends GeneratorAdapter {

    private static final String[] exceptionList = {"net/sf/saxon/trans/XPathException"};


    /**
     * Create a code generator for a particular method
     * @param access the visibility of the method, typically PUBLIC
     * @param method the Method object containing details of the argument and return types
     * @param throwsXPathException true if the method throws XPathException on error
     * @param cv the class visitor for the containing class
     */

    public Generator(final int access, final Method method, boolean throwsXPathException, final ClassVisitor cv) {
        super(Opcodes.ASM4,
            cv.visitMethod(access, method.getName(), method.getDescriptor(), null,
                throwsXPathException ? exceptionList : null),
            access, method.getName(), method.getDescriptor());
    }

    /**
     * Generate code to test whether the object on top of stack is an instance of a given class
     *
     * @param theClass the class to be tested
     * @param label    the label to jump to if the current object is an instance of the class. If it
     *                 is not an instance, control passes to the next instruction
     */

    public void ifInstance(Class theClass, LabelInfo label) {
        instanceOf(Type.getType(theClass));
        ifZCmp(NE, label.label());
    }

    /**
     * Generate code to test whether the object on top of stack is NOT an instance of a given class
     *
     * @param theClass the class to be tested
     * @param label    the label to jump to if the current object is an instance of the class. If it
     *                 is not an instance, control passes to the next instruction
     */

    public void ifNotInstance(Class theClass, LabelInfo label) {
        instanceOf(Type.getType(theClass));
        ifZCmp(EQ, label.label());
    }

    /**
     * Generate code to go to a LabelInfo
     *
     * @param labelInfo the label to jump to
     */
    public void goTo(LabelInfo labelInfo) {
        goTo(labelInfo.label());
    }

    /**
     * Push "null" onto the stack
     */

    public void pushNull() {
        push((String) null);
    }

    /**
     * Generate code to test whether the object on top of stack is the boolean true
     *
     * @param label the label to jump to if the value on top of stack is true (non-zero)
     */

    public void ifTrue(LabelInfo label) {
        ifZCmp(NE, label.label());
    }

    /**
     * Generate code to test whether the object on top of stack is NOT an instance of a given class
     *
     * @param label the label to jump to if the current object is false (zero)
     */

    public void ifFalse(LabelInfo label) {
        ifZCmp(EQ, label.label());
    }

    /**
     * Generate code to test whether the two object references on top of stack refer to the same object
     *
     * @param label the label to jump to if the two object references are equal
     */

    public void ifSameObject(LabelInfo label) {
        mv.visitJumpInsn(Opcodes.IF_ACMPEQ, label.label());
    }

    /**
     * Generate code to test whether the two object references on top of stack refer to different objects
     *
     * @param label the label to jump to if the two object references are different
     */

    public void ifNotSameObject(LabelInfo label) {
        mv.visitJumpInsn(Opcodes.IF_ACMPNE, label.label());
    }


    /**
     * Generate a "checking" cast instruction, given a Class to check against.
     *
     * @param theClass the class to be tested
     */

    public void checkClass(Class theClass) {
        if (theClass.isPrimitive()) {
            throw new AssertionError("Cannot do checkClass against a primitive type");
        }
        checkCast(Type.getType(theClass));
    }

    /**
     * Add a comment (in the form of an unused label) to the code listing
     *
     * @param compiler the compiler service
     * @param message  the comment to be added to the code
     */

    public void addComment(CompilerService compiler, String message) {
        if (compiler.isDebugByteCode()) {
            NamedLabel label = new NamedLabel();
            label.setName("%" + message);
            super.visitLabel(label);
        }
    }

    /**
     * Generate code to output a simple diagnostic message at run-time
     *
     * @param compiler the compiler service
     * @param message  the message to be output at run-time
     */

    public void showMessage(CompilerService compiler, String message) {
        if (compiler.isDebugByteCode()) {
            Generator ga = compiler.getCurrentGenerator();
            ga.push(message);
            invokeStaticMethod(ByteCodeTrace.class, "showMessage", String.class);
        }
    }

    /**
     * Generate code to output the value of of the object on the top of the bytecode stack
     *
     * @param compiler the compiler service
     * @param label    a label used to prefix the message to be output at run-time
     */

    public void showObject(CompilerService compiler, String label) {
        if (compiler.isDebugByteCode()) {
            Generator ga = compiler.getCurrentGenerator();
            ga.dup();
            ga.push(label);
            ga.swap();
            ga.dup();
            GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
            LabelInfo nullLab = methodInfo.newLabel("nullCheck");
            LabelInfo exitLab = methodInfo.newLabel("exitShowObject");
            ga.ifNull(nullLab.label());
            ga.invokeInstanceMethod(Object.class, "toString");
            invokeStaticMethod(ByteCodeTrace.class, "showObjectVariable", String.class, String.class);
            ga.goTo(exitLab);
            methodInfo.placeLabel(nullLab);
            ga.pop();
            ga.push("null");
            invokeStaticMethod(ByteCodeTrace.class, "showObjectVariable", String.class, String.class);
            methodInfo.placeLabel(exitLab);
        }
    }

    /**
     * Generate code to output the value of a run-time bytecode variable (which must hold an object)
     *
     * @param compiler the compiler service
     * @param name     a label used to prefix the message to be output at run-time, typically the variable name
     * @param slot     the slot number on the bytecode stack whose value is to be displayed
     */

    public void showObjectVariable(CompilerService compiler, String name, int slot) {
        if (compiler.isDebugByteCode()) {
            Generator ga = compiler.getCurrentGenerator();
            ga.push(name);
            ga.loadArgOrLocal(slot);
            ga.invokeInstanceMethod(Object.class, "toString");
            invokeStaticMethod(ByteCodeTrace.class, "showObjectVariable", String.class, String.class);
        }
    }

    /**
     * Generate code to output the value of a run-time variable (which must hold a long)
     *
     * @param compiler the compiler service
     * @param name     a label used to prefix the message to be output at run-time, typically the variable name
     * @param slot     the slot number on the bytecode stack whose value is to be displayed
     */

    public void showLongVariable(CompilerService compiler, String name, int slot) {
        if (compiler.isDebugByteCode()) {
            Generator ga = compiler.getCurrentGenerator();
            ga.push(name);
            ga.loadLocal(slot);
            invokeStaticMethod(ByteCodeTrace.class, "showLongVariable", String.class, Long.TYPE);
        }
    }

    /**
     * Generate code to output the value of a run-time variable (which must hold an int)
     *
     * @param compiler the compiler service
     * @param name     a label used to prefix the message to be output at run-time, typically the variable name
     * @param slot     the slot number on the bytecode stack whose value is to be displayed
     */

    public void showIntVariable(CompilerService compiler, String name, int slot) {
        if (compiler.isDebugByteCode()) {
            Generator ga = compiler.getCurrentGenerator();
            ga.push(name);
            ga.loadLocal(slot);
            invokeStaticMethod(ByteCodeTrace.class, "showIntVariable", String.class, Integer.TYPE);
        }
    }

    /**
     * Helper method to generate code that gets a static field
     *
     * @param theClass   the class on which the field is defined
     * @param fieldName  the name of the field to be fetched
     * @param fieldClass the class of the field to be fetched
     */

    public void getStaticField(Class theClass, String fieldName, Class fieldClass) {
        getStatic(Type.getType(theClass), fieldName, Type.getType(fieldClass));
    }

    /**
     * Helper method to generate code that invokes a static method
     *
     * @param theClass   the class on which the method is defined
     * @param methodName the name of the method to be invoked
     * @param argTypes   the classes of the arguments to the method
     * @throws AssertionError if no suitable method is defined on this class
     */

    public void invokeStaticMethod(Class<?> theClass, String methodName, Class... argTypes) {
        try {
            java.lang.reflect.Method rMethod = theClass.getMethod(methodName, argTypes);
            if (Modifier.isStatic(rMethod.getModifiers() & Modifier.STATIC)) {
                invokeStatic(Type.getType(theClass), Method.getMethod(rMethod));
            } else {
                throw new NoSuchMethodException();
            }
        } catch (NoSuchMethodException e) {
            throw new AssertionError("Generating call on non-existent method " + methodName + " in class " + theClass.getName());
        }
    }

    /**
     * Helper method to generate code that invokes an instance method
     *
     * @param theClass   the instance class on which the method is defined
     * @param methodName the name of the method to be invoked
     * @param argTypes   the classes of the arguments to the method
     * @throws AssertionError if no suitable method is defined on this interface
     */

    public void invokeInstanceMethod(Class<?> theClass, String methodName, Class... argTypes) {
        if (theClass.isPrimitive()) {
            throw new AssertionError("Cannot invoke instance methods on a primitive type");
        }
        try {
            java.lang.reflect.Method method = theClass.getMethod(methodName, argTypes);
            if (Modifier.isStatic(method.getModifiers() & Modifier.STATIC)) {
                throw new NoSuchMethodException("Method " + methodName + " is static");
            }
            if (theClass.isInterface()) {
                invokeInterface(Type.getType(theClass), Method.getMethod(method));
            } else {
                invokeVirtual(Type.getType(theClass), Method.getMethod(method));
            }
        } catch (NoSuchMethodException e) {
            throw new AssertionError(e);
        }
    }

    /**
     * Helper method to get an instance-level field of an object
     *
     * @param theClass  the instance class on which the method is defined
     * @param fieldName the name of the field to be obtained
     * @throws AssertionError if no suitable method is defined on this interface
     */

    public void getInstanceField(Class theClass, String fieldName) {
        try {
            getField(Type.getType(theClass), fieldName, Type.getType(theClass.getField(fieldName).getType()));
        } catch (NoSuchFieldException e) {
            throw new AssertionError(e);
        }
    }

    /**
     * Helper method for a zero-argument constructor. Generates code to invoke the constructor,
     * leaving the constructed instance on the top of the bytecode stack
     *
     * @param theClass the class of object to be constructed
     */

    public void invokeDefaultConstructor(Class<?> theClass) {
        newInstance(Type.getType(theClass));
        dup();
        try {
            invokeConstructor(Type.getType(theClass), Method.getMethod(theClass.getConstructor(EMPTY_CLASS_ARRAY)));
        } catch (NoSuchMethodException e) {
            throw new AssertionError(e);
        }
    }

    private final static Class[] EMPTY_CLASS_ARRAY = new Class[]{};

    /**
     * Helper method to generate code that creates a new instance of a class
     *
     * @param theClass the class to be instantiated
     */

    public void newInstance(Class theClass) {
        if (theClass.isPrimitive()) {
            throw new AssertionError("Cannot instantiate a primitive type");
        }
        newInstance(Type.getType(theClass));
    }

    /**
     * Helper method to generate code that invokes a constructor.
     * Precondition: the target of the method and all its arguments are on the stack (the target
     * must first be created using the newInstance() instruction).
     * Postcondition: the above items are removed from the stack
     *
     * @param theClass the class on which the method is defined
     * @param argTypes the classes of the arguments to the constructor
     * @throws AssertionError if no suitable constructor is defined on this class
     */

    public void invokeConstructor(Class<?> theClass, Class<?>... argTypes) {
        if (theClass.isPrimitive()) {
            throw new AssertionError("Cannot instantiate a primitive type");
        }
        try {
            invokeConstructor(Type.getType(theClass), Method.getMethod(theClass.getConstructor(argTypes)));
        } catch (NoSuchMethodException e) {
            throw new AssertionError(e);
        }
    }


    /**
     * Helper method to generate a local bytecode variable
     *
     * @param theClass the class of the variable
     * @return the slot number allocated to the variable
     */

    public int newLocal(Class<?> theClass) {
        return newLocal(Type.getType(theClass));
    }

    /**
     * Generates the instruction to load the given local variable on the stack.
     *
     * @param local a local variable identifier, as returned by
     *              {@link org.objectweb.asm.commons.LocalVariablesSorter#newLocal(org.objectweb.asm.Type) newLocal()}.
     */
    @Override
    public void loadLocal(int local) {
        checkLocal(local);
        super.loadLocal(local);
    }

    /**
     * Load a local variable or method argument
     *
     * @param slot the slot number to be loaded, which may represent either a method argument or a local variable
     */

    public void loadArgOrLocal(int slot) {
        if (slot < firstLocal) {
            loadArg(slot);
        } else {
            loadLocal(slot);
        }
    }

    /**
     * Generates the instruction to store the top stack value in the given local
     * variable.
     *
     * @param local a local variable identifier, as returned by
     *              {@link org.objectweb.asm.commons.LocalVariablesSorter#newLocal(org.objectweb.asm.Type) newLocal()}.
     */
    @Override
    public void storeLocal(int local) {
        checkLocal(local);
        super.storeLocal(local);
    }

    /**
     * Generate code to store the value from top of stack into a local variable or method argument slot
     *
     * @param slot the slot number to be used, which may represent either a method argument or a local variable
     */

    public void storeArgOrLocal(int slot) {
        if (slot < firstLocal) {
            storeArg(slot);
        } else {
            storeLocal(slot);
        }
    }


    /**
     * Returns the type of the given local variable.
     *
     * @param local a local variable identifier, as returned by
     *              {@link org.objectweb.asm.commons.LocalVariablesSorter#newLocal(org.objectweb.asm.Type) newLocal()}.
     * @return the type of the given local variable.
     */
    @Override
    public Type getLocalType(int local) {
        checkLocal(local);
        return super.getLocalType(local);
    }

    /**
     * Internal method to check that an integer represents a valid local variable
     *
     * @param local the slot number to be checked
     * @throws IllegalArgumentException if the slot number is invalid
     */

    private void checkLocal(int local) {
        if (local < firstLocal) {
            throw new IllegalArgumentException("local (" + local + ") is < firstLocal (" + firstLocal + ")");
        }
    }

    /**
     * Process a try/catch block
     *
     * @param start   the label for the start of the try block
     * @param end     the label for the end of the try block
     * @param handler the label for the catch block
     * @param type    the type
     */

    public void visitTryCatchBlock(
            final LabelInfo start,
            final LabelInfo end,
            final LabelInfo handler,
            final String type) {
        start.isUsed = true;
        end.isUsed = true;
        handler.isUsed = true;
        visitTryCatchBlock(start.label(), end.label(), handler.label(), type);
    }

    /**
     * Generate code to concatenate a sequence of string objects. The strings must be on the stack, in order. On completion,
     * the concatenated string is on the top of the bytecode stack
     *
     * @param n the number of strings to be concatenated
     */

    public void concatenateStrings(int n) {
        switch (n) {
            case 0:
                // no action
                break;
            case 1:
                // no action
                break;
            case 2:
                // assert stack S1 S2
                swap();
                // assert stack S2 S1
                newInstance(StringBuilder.class);
                // assert stack S2 S1 B
                dupX2();
                // assert stack B S2 S1 B
                swap();
                // assert stack B S2 B S1
                invokeConstructor(StringBuilder.class, String.class);
                // assert stack B S2
                invokeInstanceMethod(StringBuilder.class, "append", String.class);
                // assert stack B
                invokeInstanceMethod(Object.class, "toString");
                // assert stack S
                break;
            case 3:
                // assert stack S1 S2 S3
                dupX2();
                // assert stack S3 S1 S2 S3
                pop();
                // assert stack S3 S1 S2
                swap();
                // assert stack S3 S2 S1
                newInstance(StringBuilder.class);
                // assert stack S3 S2 S1 B
                dup();
                // assert stack S3 S2 S1 B B
                invokeConstructor(StringBuilder.class);
                // assert stack S3 S2 S1 B
                swap();
                // assert stack S3 S2 B S1
                invokeInstanceMethod(StringBuilder.class, "append", String.class);
                // assert stack S3 S2 B
                swap();
                // assert stack S3 B S2
                invokeInstanceMethod(StringBuilder.class, "append", String.class);
                // assert stack S3 B
                swap();
                // assert stack B S3
                invokeInstanceMethod(StringBuilder.class, "append", String.class);
                // assert stack B
                invokeInstanceMethod(Object.class, "toString");
                // assert stack S
                break;

            default:
                newInstance(StringBuilder.class);
                // assert stack S S S S S B
                dupX1();
                // assert stack S S S B S B
                swap();
                // assert stack S S S B B S
                invokeConstructor(StringBuilder.class, String.class);
                // assert stack S S S B
                for (int i = 1; i < n; i++) {
                    swap();
                    // assert stack S S B S
                    push(0);
                    // assert stack S S B S 0
                    swap();
                    // assert stack S S B 0 S
                    invokeInstanceMethod(StringBuilder.class, "insert", Integer.TYPE, String.class);
                    // assert stack S S B
                }
                // assert stack B
                invokeInstanceMethod(Object.class, "toString");
        }

    }

    /**
     * Generate a checkClass instruction if required
     * @param supplied the class of the current value
     * @param required the class required by the context in which the value is used
     * @param forceCast true if a checkClass is to be generated in the case where the supplied type is used
     * @return supplied or required, whichever is more specific
     */
    public Class<? extends Item> downCast(Class<? extends Item> supplied, Class<? extends Item> required, boolean forceCast) throws CannotCompileException {
        if (!required.isAssignableFrom(supplied)) {
            if (!supplied.isAssignableFrom(required)) {
                // Treat as recoverable error (test -s:misc-CombinedErrorCodes -t:XPTY0004_28)
                throw new CannotCompileException("Unrelated types: " + supplied + " / " + required, true);
            }
            checkClass(required);
            return required;
        } else {
            if (forceCast) {
                checkClass(supplied);
            }
            return supplied;
        }
    }

}

// Copyright (c) 2011-2022 Saxonica Limited