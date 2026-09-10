package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.AbstractBlockIterator;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.om.Item;
import net.sf.saxon.tree.iter.EmptyIterator;
import org.objectweb.asm.*;
import org.objectweb.asm.commons.Method;
import org.objectweb.asm.commons.TableSwitchGenerator;


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
    @Override
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
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return
     */
    //@Override
    @Override
    public IteratorDetails compileToIterator(CompilerService compiler,
                                             Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        Operand[] children = ((Block) expression).getOperanda();
        visitAnnotation(compiler, "BlockCompiler-Itr");
        if (children.length == 0) {
            ga.invokeStaticMethod(EmptyIterator.class, "getInstance");
            return new IteratorDetails(EmptyIterator.class, Item.class);
        } else if (children.length == 1) {
            return compiler.compileToIterator(children[0].getChildExpression());
        } else {
            visitLineNumber(compiler, ga, expression);
            //Class blockIterator = generateBlockIterator(compiler, expression, children);
            Class<?> blockIterator = compiler.getCompiledClass(expression);
            if (blockIterator == null) {
                blockIterator = generateBlockIterator(compiler, expression);
                compiler.setCompiledClass(expression, blockIterator);
            }
            allocateStatic(compiler, blockIterator);
            ga.invokeInstanceMethod(Class.class, "newInstance");
            int compiledBlockVar = methodInfo.allocateLocal(AbstractBlockIterator.class);
            ga.storeLocal(compiledBlockVar);
            ga.loadLocal(compiledBlockVar);
            ga.checkClass(AbstractBlockIterator.class);
            ga.push(children.length);
            compiler.generateGetContext();
            ga.invokeInstanceMethod(AbstractBlockIterator.class, "init", Integer.TYPE, XPathContext.class);
            ga.loadLocal(compiledBlockVar);
            ga.checkClass(AbstractBlockIterator.class);
            return new IteratorDetails(AbstractBlockIterator.class, Item.class);
        }
    }

    /**
     * Generate a BlockIterator subclass of the BlockExpression with a next method
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */
    private Class<? extends AbstractBlockIterator> generateBlockIterator(
            final CompilerService compiler, final Expression expression) throws CannotCompileException {

        compiler.checkMaxClassesLimit();
        ClassWriter cw = new ClassWriter(compiler.getFlags());
        ClassVisitor cv = cw;
        String className = "gen_CompiledBlockIterator_" + CompilerService.getUniqueNumber();
        cv = compiler.makeAnnotatedTraceClassVisitor(cv, className);

        cv.visitSource(expression.getLocation().getSystemId(), null);
        cv.visit(Opcodes.V1_5, Opcodes.ACC_PUBLIC, className, null,
                 "net/sf/saxon/expr/instruct/AbstractBlockIterator", new String[]{});

        compiler.pushNewClassInfo(className, AbstractBlockIterator.class, cw);

        makeInitMethod(cv);
        makeNextIteratorMethod(compiler, expression, cv);

        //pw.flush();
        cv.visitEnd();
        verify(cw, "BlockIterator at line " + expression.getLocation().getLineNumber(), compiler.isDebugByteCode());
        return (Class<? extends AbstractBlockIterator>) compiler.makeClass(cw, className);
    }

    private void makeNextIteratorMethod(final CompilerService compiler, /*@NotNull*/ final Expression expression, ClassVisitor cv) throws CannotCompileException {
        final Operand[] children = ((Block) expression).getOperanda();
        Method m = Method.getMethod("net.sf.saxon.om.SequenceIterator getNthChildIterator(int)");
        visitAnnotation(compiler, "BlockCompiler - nthIterator");
        final Generator ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        int contextVar = ga.newLocal(Type.getType(XPathContext.class));
        compiler.pushNewMethodInfo(ga, false, contextVar, -1);
        visitLineNumber(compiler, ga, expression);

        ga.loadThis();
        ga.getField(Type.getType(AbstractBlockIterator.class), "context", Type.getType(XPathContext.class));
        compiler.initNewMethod(ga, true);
        
        TableSwitchGenerator tabSwitch = new TableSwitchGenerator() {

            @Override
            public void generateCase(int key, Label end) {
                Expression child = ((Block) expression).getOperanda()[key].getChildExpression();
                try {
                    visitAnnotation(compiler, "BlockCompiler - operand " + key);
                    compiler.compileToIterator(child);
                    ga.goTo(end);
                } catch (CannotCompileException e) {
                    throw new IllegalStateException(e);  // wrap the checked exception in an unchecked one
                }
            }

            @Override
            public void generateDefault() {
                visitAnnotation(compiler, "BlockCompiler - default ");
                ga.pushNull();
            }
        };

        int[] states = new int[children.length];
        for (int i = 0; i < children.length; i++) {
            states[i] = i;
        }
        ga.loadArg(0);
        try {
            ga.tableSwitch(states, tabSwitch);
        } catch (IllegalStateException e) {
            if (e.getCause() instanceof CannotCompileException) {
                throw (CannotCompileException)e.getCause();
            } else {
                throw e;
            }
        }
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
        ga.invokeConstructor(Type.getType(AbstractBlockIterator.class), m);
        ga.returnValue();
        ga.endMethod();
    }


}

// Copyright (c) 2011-2022 Saxonica Limited
