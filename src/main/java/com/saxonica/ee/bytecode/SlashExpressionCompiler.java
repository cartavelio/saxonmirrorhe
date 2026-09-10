package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.map.CompiledAxisMappingFunction;
import com.saxonica.ee.bytecode.map.CompiledContextMappingFunction;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.ForEach;
import net.sf.saxon.om.FocusIterator;
import net.sf.saxon.om.FocusTrackingIterator;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.AxisIterator;
import org.objectweb.asm.ClassVisitor;
import org.objectweb.asm.ClassWriter;
import org.objectweb.asm.Opcodes;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.Method;

/**
 * Generate bytecode for a call to a SlashExpression or an xsl:for-each instruction
 * (These have the same run-time semantics)
 */
public class SlashExpressionCompiler extends ToIteratorCompiler {


    /**
     * Generate bytecode to evaluate the SlashExpression as a ContextMappingIterator
     * Precondition: none.
     * Postcondition: the stack contains an iterator representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {
        if (expression instanceof ForEach && ((ForEach)expression).getSeparatorExpression() != null) {
            throw new CannotCompileException("Separator attribute", true);
        }
        Expression start = ((ContextSwitchingExpression) expression).getSelectExpression();
        Expression step = ((ContextSwitchingExpression) expression).getActionExpression();
        Generator ga = compiler.getCurrentGenerator();
        //GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "SlashExpressionCompiler-Iterator");
        visitLineNumber(compiler, ga, expression);

        if (expression instanceof SlashExpression
                && ((SlashExpression)expression).isContextFree()
                && step instanceof AxisExpression) {
//            return new MappingIterator<>((SequenceIterator<? extends NodeInfo>) getStart().iterate(context),
//                                         ((AxisExpression) getRhsExpression())::iterate);
            ga.newInstance(MappingIterator.class);
            ga.dup();
            compiler.compileToIterator(start);
            CompiledAxisMappingFunction fn = generateAxisMappingFunction(compiler, (AxisExpression) step);
            allocateStatic(compiler, fn);
            ga.invokeConstructor(MappingIterator.class, SequenceIterator.class, MappingFunction.class);
        } else {
            /*
               *
              SequenceIterator result = start.iterate(context);
              XPathContext context2 = context.newMinorContext();
              context2.setCurrentIterator(result);
              return new ContextMappingIterator(this, context2);
               * */


            ga.newInstance(ContextMappingIterator.class);
            ga.dup();

            // assert stack: ContextMappingIterator ContextMappingIterator

            CompiledContextMappingFunction contextMapFunc = generateMappingFunction(compiler,
                                                                                    ((ContextSwitchingExpression) expression).getActionExpression());

            allocateStatic(compiler, contextMapFunc);

            // assert stack: ContextMappingIterator ContextMappingIterator ContextMappingFunction

            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "newMinorContext");
            ga.dup();

            // assert stack: ContextMappingIterator ContextMappingIterator ContextMappingFunction XPathContext XPathContext

            ga.newInstance(FocusTrackingIterator.class);
            ga.dup();
            compiler.compileToIterator(start);
            ga.invokeConstructor(FocusTrackingIterator.class, SequenceIterator.class);
            ga.invokeInstanceMethod(XPathContext.class, "setCurrentIterator", FocusIterator.class);

            // assert stack: ContextMappingIterator ContextMappingIterator ContextMappingFunction XPathContext

            ga.invokeConstructor(ContextMappingIterator.class, net.sf.saxon.expr.ContextMappingFunction.class, XPathContext.class);

            // assert stack: ContextMappingIterator
        }

    }


    /**
     * Generate a mapping function for a given step expression
     *
     * @param compiler the compiler service
     * @param step     the step expression to form the body of the mapping action
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */
    public static CompiledContextMappingFunction generateMappingFunction(CompilerService compiler, Expression step) throws CannotCompileException {

        compiler.checkMaxClassesLimit();
        ClassWriter cw = new ClassWriter(compiler.getFlags());
        ClassVisitor cv = cw;
        String className = "gen_SlashContextMappingFunction_" + CompilerService.getUniqueNumber();
        cv = compiler.makeAnnotatedTraceClassVisitor(cv, className);

        cv.visitSource(step.getLocation().getSystemId(), null);
        cv.visit(Opcodes.V1_5, Opcodes.ACC_PUBLIC, className, null,
                "com/saxonica/ee/bytecode/map/CompiledContextMappingFunction", new String[]{});
        compiler.pushNewClassInfo(className, CompiledContextMappingFunction.class, cw);

        Method m = Method.getMethod("void <init> ()");
        Generator ga = new Generator(Opcodes.ACC_PUBLIC, m, false, cv);
        ga.loadThis();
        ga.invokeConstructor(Type.getType(CompiledContextMappingFunction.class), m);
        ga.returnValue();
        ga.endMethod();

        m = Method.getMethod("net.sf.saxon.om.SequenceIterator map(net.sf.saxon.expr.XPathContext)");
        visitAnnotation(compiler, "SlashExpressionCompiler - map");
        ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
        compiler.pushNewMethodInfo(ga, true, 0, -1);
        visitLineNumber(compiler, ga, step);
        compiler.compileToIterator(step);
        ga.returnValue();
        ga.endMethod();
        compiler.popCurrentMethodInfo();
        //pw.flush();
        cv.visitEnd();
        verify(cw, "Step expression at line " + step.getLocation().getLineNumber(), compiler.isDebugByteCode());

        Class mappingFunction = compiler.makeClass(cw, className);
        try {
            return (CompiledContextMappingFunction) mappingFunction.newInstance();
        } catch (Exception e) {
            e.printStackTrace();
            throw new CannotCompileException(step, e.getMessage());
        }
    }

    /**
     * Generate a mapping function for a given step expression
     *
     * @param compiler the compiler service
     * @param step     the step expression to form the body of the mapping action
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     * @return the mapping function
     */
    private static CompiledAxisMappingFunction generateAxisMappingFunction(CompilerService compiler, AxisExpression step) throws CannotCompileException {

        compiler.checkMaxClassesLimit();
        ClassWriter cw = new ClassWriter(compiler.getFlags());
        ClassVisitor cv = cw;
        String className = "gen_AxisMappingFunction_" + CompilerService.getUniqueNumber();
        cv = compiler.makeAnnotatedTraceClassVisitor(cv, className);

        cv.visitSource(step.getLocation().getSystemId(), null);
        cv.visit(Opcodes.V1_5, Opcodes.ACC_PUBLIC, className, null,
                 "com/saxonica/ee/bytecode/map/CompiledAxisMappingFunction", new String[]{});
        compiler.pushNewClassInfo(className, CompiledAxisMappingFunction.class, cw);

        Method m = Method.getMethod("void <init> ()");
        Generator ga = new Generator(Opcodes.ACC_PUBLIC, m, false, cv);
        ga.loadThis();
        ga.invokeConstructor(Type.getType(CompiledAxisMappingFunction.class), m);
        ga.returnValue();
        ga.endMethod();

        m = Method.getMethod("net.sf.saxon.tree.iter.AxisIterator map(net.sf.saxon.om.Item)");
        visitAnnotation(compiler, "SlashExpressionCompiler - axisMap");
        ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
        compiler.pushNewMethodInfo(ga, false, 0, -1);
        visitLineNumber(compiler, ga, step);
        ga.loadArg(0);
        AxisExpressionCompiler.iterate(compiler, step.getAxis(), step.getNodeTest());
        ga.checkClass(AxisIterator.class);
        ga.returnValue();
        ga.endMethod();
        compiler.popCurrentMethodInfo();
        //pw.flush();
        cv.visitEnd();
        verify(cw, "Step expression at line " + step.getLocation().getLineNumber(), compiler.isDebugByteCode());

        Class mappingFunction = compiler.makeClass(cw, className);
        try {
            return (CompiledAxisMappingFunction) mappingFunction.newInstance();
        } catch (Exception e) {
            e.printStackTrace();
            throw new CannotCompileException(step, e.getMessage());
        }
    }

    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        if (expression instanceof ForEach && ((ForEach) expression).getSeparatorExpression() != null) {
            throw new CannotCompileException("Separator attribute", true);
        }
        Expression select = ((ContextSwitchingExpression) expression).getSelectExpression();
        Expression action = ((ContextSwitchingExpression) expression).getActionExpression();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "SlashExpressionCompiler-Push");
        visitLineNumber(compiler, ga, expression);
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "newMinorContext");
        int context2Var = methodInfo.allocateLocal(XPathContext.class);
        visitAnnotation(compiler, "SlashExpressionCompiler - newMinorContext");
        ga.storeLocal(context2Var);


        ga.loadLocal(context2Var);
        ga.newInstance(FocusTrackingIterator.class);
        ga.dup();
        compiler.compileToIterator(select);
        ga.invokeConstructor(FocusTrackingIterator.class, SequenceIterator.class);

        int iterVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.dup();
        ga.storeLocal(iterVar);

        ga.invokeInstanceMethod(XPathContext.class, "setCurrentIterator", FocusIterator.class);

        methodInfo.pushContextVariableInfo(context2Var, false);

        LabelInfo forEachDone = methodInfo.newLabel("forEachDone");
        LabelInfo loop = methodInfo.placeNewLabel("forEachLoop");
        ga.loadLocal(iterVar);
        ga.showObjectVariable(compiler, "forEachSelectIterator", iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.ifNull(forEachDone.label());

        visitAnnotation(compiler, "SlashExpressionCompiler - compileAction");
        compiler.compileToPush(action);
        ga.goTo(loop);

        methodInfo.placeLabel(forEachDone);
        methodInfo.popContextVariableInfo();

        methodInfo.releaseLocal(context2Var);
        methodInfo.releaseLocal(iterVar);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
