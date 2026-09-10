package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.iter.CompiledBlockIterator;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.value.Cardinality;
import org.objectweb.asm.*;
import org.objectweb.asm.commons.Method;
import org.objectweb.asm.commons.TableSwitchGenerator;

import java.util.ArrayList;
import java.util.List;


/**
 * Generate bytecode for a call to a Block expression. A block typically represents
 * either an expression of the form (a, b, c, d), or an XSLT sequence constructor.
 */
public class BlockCompiler extends ToIteratorCompiler {


    /**
     * Generate bytecode to evaluate the BlockExpression in push mode
     * Precondition: none.
     * Postcondition: the stack is empty
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    public void compileToPush(CompilerService compiler,
                              Expression expression) throws CannotCompileException {
        Operand[] children = ((Block) expression).getOperanda();
        visitAnnotation(compiler, "BlockCompiler-Push");
        for (Operand o : children) {
            compiler.compileToPush(o.getChildExpression());
        }
    }

    /**
     * Generate bytecode to evaluate the BlockExpression as an iterator
     * Precondition: none.
     * Postcondition: the stack contains an iterator representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    //@Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        Operand[] children = ((Block) expression).getOperanda();
        visitAnnotation(compiler, "BlockCompiler-Itr");
        if (children.length == 0) {
            ga.invokeStaticMethod(EmptyIterator.class, "getInstance");
        } else if (children.length == 1) {
            compiler.compileToIterator(children[0].getChildExpression());
        } else {
            visitLineNumber(compiler, ga, expression);
            //Class blockIterator = generateBlockIterator(compiler, expression, children);
            Class blockIterator = compiler.getCompiledClass(expression);
            if (blockIterator == null) {
                blockIterator = generateBlockIterator(compiler, expression);
                compiler.setCompiledClass(expression, blockIterator);
            }
            allocateStatic(compiler, blockIterator);
            ga.invokeInstanceMethod(Class.class, "newInstance");
            int compiledBlockVar = methodInfo.allocateLocal(CompiledBlockIterator.class);
            ga.storeLocal(compiledBlockVar);
            ga.loadLocal(compiledBlockVar);
            ga.checkClass(CompiledBlockIterator.class);
            compiler.generateGetContext();
            ga.invokeInstanceMethod(CompiledBlockIterator.class, "setContext", XPathContext.class);
            ga.loadLocal(compiledBlockVar);
            ga.checkClass(CompiledBlockIterator.class);
        }
    }

    /**
     * Generate a BlockIterator subclass of the BlockExpression with a next method
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */
    private Class<? extends CompiledBlockIterator> generateBlockIterator(
        final CompilerService compiler, final Expression expression) throws CannotCompileException {

        compiler.checkMaxClassesLimit();
        ClassWriter cw = new ClassWriter(compiler.getFlags());
        ClassVisitor cv = cw;
        String className = "gen_CompiledBlockIterator_" + CompilerService.getUniqueNumber();
        cv = compiler.makeAnnotatedTraceClassVisitor(cv, className);

        cv.visitSource(expression.getLocation().getSystemId(), null);
        cv.visit(Opcodes.V1_5, Opcodes.ACC_PUBLIC, className, null,
                "com/saxonica/ee/bytecode/iter/CompiledBlockIterator", new String[]{});

        compiler.pushNewClassInfo(className, CompiledBlockIterator.class, cw);

        makeInitMethod(cv);
        makeNextMethod(compiler, expression, cv);

        //pw.flush();
        cv.visitEnd();
        verify(cw, "BlockIterator at line " + expression.getLocation().getLineNumber(), compiler.isDebugByteCode());
        return (Class<? extends CompiledBlockIterator>)compiler.makeClass(cw, className);
    }

    private void makeNextMethod(final CompilerService compiler, /*@NotNull*/ final Expression expression, ClassVisitor cv) throws CannotCompileException {
        final Operand[] children = ((Block) expression).getOperanda();
        Method m = Method.getMethod("net.sf.saxon.om.Item next()");
        visitAnnotation(compiler, "BlockCompiler - next");
        final Generator ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);

        int contextVar = ga.newLocal(Type.getType(XPathContext.class));
        final int currentVar = ga.newLocal(Item.class);
        compiler.pushNewMethodInfo(ga, false, contextVar);
        visitLineNumber(compiler, ga, expression);
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        ga.loadThis();
        ga.getField(Type.getType(CompiledBlockIterator.class), "context", Type.getType(XPathContext.class));
        compiler.initNewMethod(ga, true);
        //ga1.storeLocal(contextVar);

        ga.pushNull();
        ga.storeLocal(currentVar);

        LabelInfo end = methodInfo.newLabel("endBlock");
        final LabelInfo next = methodInfo.newLabel("next");
        LabelInfo nonNullItem = methodInfo.newLabel("nonNullItem");
        LabelInfo nonNullItr = methodInfo.newLabel("nonNullItr");

        final LabelInfo loop = methodInfo.placeNewLabel("loop");

        ga.loadThis();
        ga.getField(Type.getType(CompiledBlockIterator.class), "currentIterator", Type.getType(SequenceIterator.class));
        ga.dup();
        ga.ifNonNull(nonNullItr.label());
        ga.pop();
        ga.goTo(next);

        methodInfo.placeLabel(nonNullItr);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.dup();

//        ga.loadThis();
//        ga.swap();
//        ga.putField(Type.getType(CompiledBlockIterator.class), "current", Type.getType(Item.class));
        ga.storeLocal(currentVar);

        ga.ifNonNull(nonNullItem.label());

        ga.loadThis();
        ga.pushNull();
        ga.putField(Type.getType(CompiledBlockIterator.class), "currentIterator", Type.getType(SequenceIterator.class));
        ga.goTo(next);

        methodInfo.placeLabel(nonNullItem);
//        ga.loadThis();
//        ga.dup();
//        ga.getField(Type.getType(CompiledBlockIterator.class), "position", Type.INT_TYPE);
//        ga.push(1);
//        ga.math(Generator.ADD, Type.INT_TYPE);
//        ga.putField(Type.getType(CompiledBlockIterator.class), "position", Type.INT_TYPE);
        ga.goTo(end);

        methodInfo.placeLabel(next);

        final List<CannotCompileException> errors = new ArrayList<>();

        TableSwitchGenerator tabSwitch = new TableSwitchGenerator() {

            public void generateCase(int key, Label end) {
                visitAnnotation(compiler, "BlockCompiler - caseStatement");
                if (key == children.length) {
                    visitAnnotation(compiler, "BlockCompiler - default");
                    ga.pushNull();
                    ga.storeLocal(currentVar);
//                    ga.loadThis();
//                    ga.dup();
//                    ga.push(-1);
//                    ga.putField(Type.getType(CompiledBlockIterator.class), "position", Type.INT_TYPE);
//                    ga.pushNull();
//                    ga.putField(Type.getType(CompiledBlockIterator.class), "current", Type.getType(Item.class));
                    ga.goTo(end);

                } else {
                    ga.loadThis();
//                    ga.dup();
//                    ga.getField(Type.getType(CompiledBlockIterator.class), "state", Type.INT_TYPE);
//                    ga.push(1);
//                    ga.math(Generator.ADD, Type.INT_TYPE);
                    ga.push(key + 1);
                    ga.putField(Type.getType(CompiledBlockIterator.class), "state", Type.INT_TYPE);

                    if (Cardinality.allowsMany(children[key].getChildExpression().getCardinality())) {
                        try {
                            visitAnnotation(compiler, "BlockCompiler - caseStatement" + key);
                            compiler.compileToIterator(children[key].getChildExpression());
                        } catch (CannotCompileException e) {
                            errors.add(e);
                            return;
                        }
                        ga.loadThis();
                        ga.swap();
                        ga.putField(Type.getType(CompiledBlockIterator.class), "currentIterator", Type.getType(SequenceIterator.class));
                        ga.goTo(loop);
                    } else {

                        try {
                            visitAnnotation(compiler, "BlockCompiler - caseStatement-asItem" + key);
                            compiler.compileToItem(children[key].getChildExpression());
                        } catch (CannotCompileException e) {
                            errors.add(e);
                            return;
                        }


                        if (Cardinality.allowsZero(children[key].getChildExpression().getCardinality())) {
                            ga.dup();
//                            ga.loadThis();
//                            ga.swap();
//                            ga.putField(Type.getType(CompiledBlockIterator.class), "current", Type.getType(Item.class));
                            ga.storeLocal(currentVar);
                            ga.ifNull(next.label());
                        } else {
//                            ga.loadThis();
//                            ga.swap();
//                            ga.putField(Type.getType(CompiledBlockIterator.class), "current", Type.getType(Item.class));
                            ga.storeLocal(currentVar);
                        }

//                        ga.loadThis();
//                        ga.dup();
//                        ga.getField(Type.getType(CompiledBlockIterator.class), "position", Type.INT_TYPE);
//                        ga.push(1);
//                        ga.math(Generator.ADD, Type.INT_TYPE);
//                        ga.putField(Type.getType(CompiledBlockIterator.class), "position", Type.INT_TYPE);
                        ga.goTo(end);
                    }
                }
            }

            public void generateDefault() {
            }
        };

        int[] states = new int[children.length + 1];
        for (int i = 0; i < children.length + 1; i++) {
            states[i] = i;
        }
        ga.loadThis();
        ga.getField(Type.getType(CompiledBlockIterator.class), "state", Type.INT_TYPE);
        ga.tableSwitch(states, tabSwitch);
        if (!errors.isEmpty()) {
            throw errors.get(0);
        }

        methodInfo.placeLabel(end);
        ga.loadLocal(currentVar);
//        ga.loadThis();
//        ga.getField(Type.getType(CompiledBlockIterator.class), "current", Type.getType(Item.class));
        ga.returnValue();
        try {
            ga.endMethod();
        } catch (Exception e) {
            e.printStackTrace();
            System.err.println("*** end method failed");
        }
        compiler.popCurrentMethodInfo();
    }

    private void makeInitMethod(ClassVisitor cv) {
        Method m = Method.getMethod("void <init> ()");
        Generator ga = new Generator(Opcodes.ACC_PUBLIC, m, false, cv);
        ga.loadThis();
        ga.invokeConstructor(Type.getType(CompiledBlockIterator.class), m);
        ga.returnValue();
        ga.endMethod();
    }

}

//Copyright (c) 2011 Saxonica Limited.
