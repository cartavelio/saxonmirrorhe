package com.saxonica.ee.bytecode;


import com.saxonica.ee.bytecode.util.*;
import com.saxonica.ee.bytecode.util.CompilerService.StaticVariableInfo;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.*;
import org.objectweb.asm.ClassReader;
import org.objectweb.asm.ClassVisitor;
import org.objectweb.asm.ClassWriter;
import org.objectweb.asm.Type;
import org.objectweb.asm.util.CheckClassAdapter;

import java.io.*;

/**
 * This abstract class represents the compiler (that is, Java bytecode generator)
 * for a particular kind of expression on the expression tree. The expression compiler
 * is called by the {@link com.saxonica.ee.bytecode.util.CompilerService} working its way down the expression tree in a top
 * down fashion: there is a one-to-one correspondence between the classes implementing the
 * expression on the expression tree and the compiler object used to generate Java code fragments.
 * Various methods are supplied to compile expressions; exactly one of them is called, depending
 * on the context in which the expression appears.
 */
public abstract class ExpressionCompiler {

    /**
     * Generate bytecode to evaluate the expression as an Item
     * Precondition: none.
     * Postcondition: at execution time, the stack contains either an item (the result of the expression), or null
     * (representing an empty sequence).
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */

    public abstract void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException;

    /**
     * Generate bytecode to evaluate the expression as a {@link net.sf.saxon.om.SequenceIterator}
     * Precondition: none.
     * Postcondition: at execution time, the stack contains a SequenceIterator representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */

    public abstract void compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException;

    /**
     * Generate bytecode to evaluate the expression as a boolean
     * Precondition: none.
     * Postcondition: at execution time, the stack contains an integer (0=false, 1=true) representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */

    public abstract void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException;

    /**
     * Generate bytecode to evaluate the expression leaving a plain Java value on the stack. This method
     * must only be called if the static type of the expressionis such that the value is known to be representable
     * by a value of the specified class; the cardinality must be either exactly-one or zero-or-one.
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     * @param requiredClass the class of the Java value that is to be left on the stack if the generated code
     *                      exits normally. This must be one of Integer.TYPE, Double.TYPE, Float.TYPE, BigDecimal.class,
     *                      String.class, UnicodeString.class.
     * @param onEmpty       defines the action to be taken if the expression evaluates to the empty sequence.
     *                      May be null if the expression is known statically not to evaluate to an empty sequence. If the return
     *                      class is a primitive type (double, float, etc) this must be an instance of {@link com.saxonica.ee.bytecode.util.OnEmpty.UnwindAndJump}
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */

    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass,
                                   OnEmpty onEmpty) throws CannotCompileException {
        // This is a fallback implementation that uses compileToItem - which should not normally be
        // used because it incurs the overhead of boxing and unboxing
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        compileToItem(compiler, expression);
        if (Item.class.isAssignableFrom(requiredClass)) {
            if (Cardinality.allowsZero(expression.getCardinality())) {
                LabelInfo notNull = methodInfo.newLabel("notNullECItem");
                ga.dup();
                ga.ifNonNull(notNull.label());
                OnEmpty.UnwindAndJump longJump = (OnEmpty.UnwindAndJump) onEmpty;
                longJump.getUnwindStack().push(requiredClass);
                longJump.generate(ga);
                methodInfo.placeLabel(notNull);
                longJump.getUnwindStack().pop();
            }
            ga.checkClass(requiredClass);
        } else if (requiredClass == Double.TYPE) {
            if (Cardinality.allowsZero(expression.getCardinality())) {
                LabelInfo notNull = methodInfo.newLabel("notNullECDbl");
                ga.dup();
                ga.ifNonNull(notNull.label());
                OnEmpty.UnwindAndJump longJump = (OnEmpty.UnwindAndJump) onEmpty;
                longJump.getUnwindStack().push(DoubleValue.class);
                longJump.generate(ga);
                methodInfo.placeLabel(notNull);
                longJump.getUnwindStack().pop();
            }
            ga.checkClass(DoubleValue.class);
            ga.invokeInstanceMethod(DoubleValue.class, "getDoubleValue");
        } else if (requiredClass == Float.TYPE) {
            if (Cardinality.allowsZero(expression.getCardinality())) {
                LabelInfo notNull = methodInfo.newLabel("notNullECFlt");
                ga.dup();
                ga.ifNonNull(notNull.label());
                OnEmpty.UnwindAndJump longJump = (OnEmpty.UnwindAndJump) onEmpty;
                longJump.getUnwindStack().push(FloatValue.class);
                longJump.generate(ga);
                methodInfo.placeLabel(notNull);
                longJump.getUnwindStack().pop();
            }
            ga.checkClass(FloatValue.class);
            ga.invokeInstanceMethod(FloatValue.class, "getFloatValue");
        } else if (requiredClass == Integer.TYPE || requiredClass == Long.TYPE) {
            if (Cardinality.allowsZero(expression.getCardinality())) {
                LabelInfo notNull = methodInfo.newLabel("notNullECFlt");
                ga.dup();
                ga.ifNonNull(notNull.label());
                OnEmpty.UnwindAndJump longJump = (OnEmpty.UnwindAndJump) onEmpty;
                longJump.getUnwindStack().push(Int64Value.class);
                longJump.generate(ga);
                methodInfo.placeLabel(notNull);
                longJump.getUnwindStack().pop();

            }
            ga.checkClass(IntegerValue.class);
            ga.invokeInstanceMethod(IntegerValue.class, "longValue");
            if (requiredClass == Integer.TYPE) {
                ga.cast(Type.LONG_TYPE, Type.INT_TYPE);
            }
        } else if (requiredClass == String.class) {
            LabelInfo exit = null;
            if (Cardinality.allowsZero(expression.getCardinality())) {
                LabelInfo notNull = methodInfo.newLabel("ECstrNotNull");
                ga.dup();
                ga.ifNonNull(notNull.label());
                exit = methodInfo.newLabel("itemToStringExit");
                handleEmptyStringResult(onEmpty, ga, methodInfo, exit);
                methodInfo.placeLabel(notNull);
            }
            ga.checkClass(StringValue.class);
            ga.invokeInstanceMethod(StringValue.class, "getStringValue");
            if (exit != null) {
                methodInfo.placeLabel(exit);
            }
        } else if (requiredClass == UnicodeString.class) {
            LabelInfo exit = null;
            if (Cardinality.allowsZero(expression.getCardinality())) {
                LabelInfo notNull = methodInfo.newLabel("ECstrNotNull");
                ga.dup();
                ga.ifNonNull(notNull.label());
                exit = methodInfo.newLabel("itemToStringExit");
                handleEmptyStringResult(onEmpty, ga, methodInfo, exit);
                methodInfo.placeLabel(notNull);
            }
            ga.checkClass(StringValue.class);
            ga.invokeInstanceMethod(StringValue.class, "getUnicodeStringValue");
            if (exit != null) {
                methodInfo.placeLabel(exit);
            }

        } else {
            throw new CannotCompileException("compilePrimitive requiredClass = " + requiredClass, false);
        }
    }

    public static void handleEmptyStringResult(OnEmpty onEmpty, Generator ga, GeneratedMethodInfo methodInfo, LabelInfo exit) {
        if (onEmpty instanceof OnEmpty.ReturnEmptyString) {
            ga.pop();
            ga.push("");
            ga.goTo(exit);
        } else if (onEmpty instanceof OnEmpty.ReturnEmptyUnicodeString) {
            ga.pop();
            ga.invokeStaticMethod(EmptyUnicodeString.class, "getInstance");
            ga.goTo(exit);
        } else if (onEmpty instanceof OnEmpty.ReturnNull) {
            ga.pop();
            ga.pushNull();
            ga.goTo(exit);
        } else if (onEmpty instanceof OnEmpty.UnwindAndJump) {
            OnEmpty.UnwindAndJump longJump = (OnEmpty.UnwindAndJump) onEmpty;
            longJump.getUnwindStack().push(StringValue.class);
            longJump.generate(ga);
            longJump.getUnwindStack().pop();
        } else {
            throw new IllegalArgumentException();
        }
    }

    /**
     * Generate bytecode to evaluate the expression in push mode
     * Precondition: none.
     * Postcondition: at execution time, the stack is unchanged, and the value of the expression has been
     * written to the current receiver
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */

    public abstract void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException;
    
    /**
     * Helper method to implement compileFromItem when the class implements compileToPrimitive returning an integer
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */

    protected final void compileItemFromInt(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "compileItemFromInt");

        OnEmpty onEmpty = null;
        LabelInfo returnNull = methodInfo.newLabel("returnNull");
        if (Cardinality.allowsZero(expression.getCardinality())) {
            onEmpty = new OnEmpty.UnwindAndJump(returnNull);
        }
        compileToPrimitive(compiler, expression, Long.TYPE, onEmpty);
        ga.newInstance(Type.getType(Int64Value.class));
        ga.dupX2();
        //Stack: Int64Value LongA LongB Int64Value
        ga.dupX2();
        //Stack: Int64Value Int64Value LongA LongB Int64Value
        ga.pop();
        ga.invokeConstructor(Int64Value.class, Long.TYPE);
        if (onEmpty != null) {
            LabelInfo exit = methodInfo.newLabel("exitItemFromInt");
            ga.goTo(exit);
            methodInfo.placeLabel(returnNull);
            ga.pushNull();
            methodInfo.placeLabel(exit);
        }
        // on completion, the Int64Value (or null) is on the stack
    }

    /**
     * Generate annotations in the bytecode, for diagnostic purposes
     *
     * @param compiler the compiler service
     * @param message  the message to be inserted in the bytecode
     */

    public static void visitAnnotation(CompilerService compiler, String message) {
        if (compiler.isDebugByteCode()) {
            Generator ga = compiler.getCurrentGenerator();
            ga.addComment(compiler, message);
            ga.push(message);
            ga.invokeStaticMethod(ByteCodeTrace.class, "showMessage", String.class);
        }
    }

    /**
     * Print the top entry on the bytecode value stack, for diagnostic purposes
     * @param compiler  the bytecode compiler service to use
     *
     */
    public static void printStackTop(CompilerService compiler) {
        if (compiler.isDebugByteCode()) {
            Generator ga = compiler.getCurrentGenerator();
            ga.dup();
            ga.invokeInstanceMethod(Object.class, "toString");
            ga.push("STACK top:");
            ga.swap();
            ga.invokeInstanceMethod(String.class, "concat", String.class);
            ga.invokeStaticMethod(ByteCodeTrace.class, "showMessage", String.class);
        }
    }

    /**
     * Generate code to allocate static field in widget and to push field on stack
     *
     * @param compiler the compiler service
     * @param value    the Object of the field
     */
    public static void allocateStatic(CompilerService compiler, Object value) {
        if (value == null) {
            compiler.getCurrentGenerator().pushNull();
        } else {
            StaticVariableInfo varInfo = compiler.allocateStaticVariableInfo(value);
            compiler.getCurrentGenerator().getStatic(Type.getType(compiler.getCurrentClass().getTypeDescriptor()), varInfo.name, Type.getType(varInfo.classVar));
        }
    }

    /**
     * Verify the generated bytecode, for diagnostic purposes
     *
     * @param cw         the ClassWriter
     * @param objectName
     * @param debug      true if bytecode tracing is required
     */
    public static void verify(ClassWriter cw, String objectName, boolean debug) {
        if (debug) {
            StringWriter sw = new StringWriter();
            CheckClassAdapter.verify(new ClassReader(cw.toByteArray()), false, new PrintWriter(sw));
            if (sw.toString().length() != 0) {
                System.err.println("Verify Output for " + objectName + ":");
//                if (sw.toString().length() <= 50000) {
                //Test Write to file
                try {
                    BufferedWriter out = new BufferedWriter(new FileWriter("ByteCodeOutput.txt"));
                    out.write(sw.toString());
                    out.close();
                } catch (IOException e) {
                    System.out.println("Exception ");

                }
                //end of test
                System.err.println(sw);
//                } else {
//                    StringBuffer sb = sw.getBuffer();
//                    sb.setLength(50000);
//                    System.err.println(sb);
//                }
                throw new IllegalStateException("Bytecode failed verification");
            }
        }
    }


    /**
     * Generate line number of expression in byte-code
     *
     * @param compiler the compiler service
     * @param ga       the generator adapter
     * @param expr     Expression
     */
    public static void visitLineNumber(CompilerService compiler, Generator ga, Expression expr) {
        int line = expr.getLocation().getLineNumber();
        if (line != compiler.mostRecentLineNumber) {
            ga.visitLineNumber(line, ga.newLabel());
            compiler.mostRecentLineNumber = line;
        }
    }

    public void generateMethod(CompilerService compiler, Expression expression, ClassVisitor cv) {
    }


}

// Copyright (c) 2011-2020 Saxonica Limited

