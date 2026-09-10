package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.map.ForItemMappingAction;
import com.saxonica.ee.bytecode.map.ForSequenceMappingAction;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.*;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.value.Cardinality;
import org.objectweb.asm.ClassVisitor;
import org.objectweb.asm.ClassWriter;
import org.objectweb.asm.Opcodes;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.Method;


/**
 * Generate bytecode for a call to the ForExpression()
 */
public class ForExpressionCompiler extends ToIteratorCompiler {


    /**
     * Generate bytecode to evaluate the ForExpression in push mode
     * Precondition: none.
     * Postcondition: the stack is empty
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */

    @Override
    public void compileToPush(CompilerService compiler, final Expression expression) throws CannotCompileException {
        Expression sequence = ((ForExpression) expression).getSequence();
        Expression action = ((ForExpression) expression).getAction();
        int slotNum = ((ForExpression) expression).getLocalSlotNumber();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "ForExpression-push");
        compiler.compileToIterator(sequence);
        int iterVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(iterVar);
        LabelInfo done = methodInfo.newLabel("doneFor");
        LabelInfo loop = methodInfo.placeNewLabel("loopFor");
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.dup();
        ga.ifNull(done.label());
        compiler.generateGetContext();
        ga.swap();
        ga.push(slotNum);
        ga.swap();
        ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);
        compiler.compileToPush(action);
        ga.goTo(loop);

        methodInfo.placeLabel(done);
        ga.pop();

        methodInfo.releaseLocal(iterVar);
    }


    /**
     * Generate bytecode to evaluate the ForExpression as an iterator
     * Precondition: none.
     * Postcondition: the stack contains an iterator representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {

        Expression sequence = ((ForExpression) expression).getSequence();
        Expression action = ((ForExpression) expression).getAction();
        visitAnnotation(compiler, "Forexpression-itr");
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        int localSlotNum = ((ForExpression) expression).getLocalSlotNumber();

        if (!Cardinality.allowsMany(sequence.getCardinality())) {

            compiler.compileToItem(sequence);
            ga.dup();
            LabelInfo nullLabel = methodInfo.newLabel("nullLabel");
            LabelInfo endLabel = methodInfo.newLabel("endLabel");
            ga.ifNull(nullLabel.label());
            compiler.generateGetContext();
            ga.swap();
            int slotNumber = ((ForExpression) expression).getLocalSlotNumber();
            ga.push(slotNumber);
            ga.swap();
            ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);
            compiler.compileToIterator(action);
            ga.goTo(endLabel);
            methodInfo.placeLabel(nullLabel);
            ga.pop();
            allocateStatic(compiler, EmptyIterator.emptyIterator());
            methodInfo.placeLabel(endLabel);

        } else if (Cardinality.allowsMany(action.getCardinality())) {

            // return new MappingIterator(base, map);

            ga.newInstance(MappingIterator.class);
            ga.dup();

            // assert stack : MappingIterator MappingIterator

            compiler.compileToIterator(sequence);

            // assert stack : MappingIterator MappingIterator BaseIterator

            Class mappingActionClass = compiler.getCompiledClass(expression);
            if (mappingActionClass == null) {
                mappingActionClass = generateSequenceMappingAction(compiler, (ForExpression) expression, action, localSlotNum);
                compiler.setCompiledClass(expression, mappingActionClass);
            }
            allocateStatic(compiler, mappingActionClass);
            ga.invokeInstanceMethod(Class.class, "newInstance");
            ga.checkClass(ForSequenceMappingAction.class);

            // assert stack : MappingIterator MappingIterator BaseIterator MappingFunction

            ga.dup();
            compiler.generateGetContext();
            ga.invokeInstanceMethod(ForSequenceMappingAction.class, "setContext", XPathContext.class);

            // assert stack : MappingIterator MappingIterator BaseIterator MappingFunction

            ga.invokeConstructor(MappingIterator.class, SequenceIterator.class, MappingFunction.class);

            // assert stack : MappingIterator

        } else {

            // return new ItemMappingIterator(base, map, oneToOne);

            ga.newInstance(ItemMappingIterator.class);
            ga.dup();

            // assert stack : MappingIterator MappingIterator

            compiler.compileToIterator(sequence);

            // assert stack : MappingIterator MappingIterator BaseIterator


            Class mappingActionClass = compiler.getCompiledClass(expression);
            if (mappingActionClass == null) {
                mappingActionClass = generateItemMappingAction(compiler, (ForExpression) expression, action, localSlotNum);
                compiler.setCompiledClass(expression, mappingActionClass);
            }
            allocateStatic(compiler, mappingActionClass);
            ga.invokeInstanceMethod(Class.class, "newInstance");
            ga.checkClass(ForItemMappingAction.class);

            // assert stack : MappingIterator MappingIterator BaseIterator MappingFunction

            ga.dup();
            compiler.generateGetContext();
            ga.invokeInstanceMethod(ForItemMappingAction.class, "setContext", XPathContext.class);

            // assert stack : MappingIterator MappingIterator BaseIterator MappingFunction

            ga.push(!Cardinality.allowsZero(action.getCardinality()));
            ga.invokeConstructor(ItemMappingIterator.class, SequenceIterator.class, ItemMappingFunction.class, Boolean.TYPE);

            // assert stack : MappingIterator

        }
    }


    /**
     * Generate a mapping function with a map() method that returns an iterator over the results of
     * one iteration of the loop
     *
     * @param compiler   the CompilerService
     * @param expression the "for" expression itself
     * @param action     the body of the "for" loop, which represents the expression to be compiled
     * @param slotNumber the slot number
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */

    public static Class generateSequenceMappingAction(CompilerService compiler, ForExpression expression, Expression action, int slotNumber) throws CannotCompileException {

        compiler.checkMaxClassesLimit();
        ClassWriter cw = new ClassWriter(compiler.getFlags());
        ClassVisitor cv = cw;
        String className = "gen_ForSequenceMappingAction_line" + expression.getLocation().getLineNumber()
                + "_" + CompilerService.getUniqueNumber();
        cv = compiler.makeAnnotatedTraceClassVisitor(cv, className);

        cv.visitSource(expression.getLocation().getSystemId(), null);
        cv.visit(Opcodes.V1_5, Opcodes.ACC_PUBLIC, className, null,
                "com/saxonica/ee/bytecode/map/ForSequenceMappingAction", new String[]{});
        compiler.pushNewClassInfo(className, ForSequenceMappingAction.class, cw);

        Method m = Method.getMethod("void <init> ()");
        Generator ga = new Generator(Opcodes.ACC_PUBLIC, m, false, cv);
        ga.loadThis();
        ga.invokeConstructor(Type.getType(ForSequenceMappingAction.class), m);
        ga.returnValue();
        ga.endMethod();

        m = Method.getMethod("net.sf.saxon.om.SequenceIterator map(net.sf.saxon.om.Item)");
        ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
        int contextVar = ga.newLocal(XPathContext.class);
        compiler.pushNewMethodInfo(ga, false, contextVar, -1);
        visitLineNumber(compiler, ga, expression);
        ga.loadThis();
        ga.getField(Type.getType(ForSequenceMappingAction.class), "context", Type.getType(XPathContext.class));
        compiler.initNewMethod(ga, false);
        ga.loadLocal(contextVar);
        ga.push(slotNumber);
        ga.loadArg(0);
        ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);

        compiler.compileToIterator(action);
        ga.returnValue();
        ga.endMethod();
        compiler.popCurrentMethodInfo();
        //pw.flush();
        cv.visitEnd();
        verify(cw, "For expression at line " + expression.getLocation().getLineNumber(), compiler.isDebugByteCode());
        return compiler.makeClass(cw, className);
    }

    /**
     * Generate a mapping function with a mapItem() method that returns an item being the result of
     * one iteration of the loop
     *
     * @param compiler   the CompilerService
     * @param expression the "for" expression itself
     * @param action     the body of the "for" loop, which represents the expression to be compiled
     * @throws CannotCompileException in the event of failure
     */
    private Class generateItemMappingAction(CompilerService compiler, ForExpression expression, Expression action, int slotNumber) throws CannotCompileException {

        compiler.checkMaxClassesLimit();
        ClassWriter cw = new ClassWriter(compiler.getFlags());
        ClassVisitor cv = cw;
        int lineNumber = expression.getLocation().getLineNumber();
        String className = "gen_ForItemMappingAction_line" + (lineNumber > 0 ? lineNumber : 0)
                + "_" + CompilerService.getUniqueNumber();
        cv = compiler.makeAnnotatedTraceClassVisitor(cv, className);

        cv.visitSource(expression.getLocation().getSystemId(), null);
        cv.visit(Opcodes.V1_5, Opcodes.ACC_PUBLIC, className, null,
                "com/saxonica/ee/bytecode/map/ForItemMappingAction", new String[]{});
        compiler.pushNewClassInfo(className, ForItemMappingAction.class, cw);

        Method m = Method.getMethod("void <init> ()");
        Generator ga = new Generator(Opcodes.ACC_PUBLIC, m, false, cv);
        ga.loadThis();
        ga.invokeConstructor(Type.getType(ForItemMappingAction.class), m);
        ga.returnValue();
        ga.endMethod();

        m = Method.getMethod("net.sf.saxon.om.Item mapItem(net.sf.saxon.om.Item)");
        ga = new Generator(Opcodes.ACC_PUBLIC, m, true, cv);
        int contextVar = ga.newLocal(XPathContext.class);
        compiler.pushNewMethodInfo(ga, false, contextVar, -1);
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);

        // diagnostic code
        LabelInfo itemNotNull = methodInfo.newLabel("itemNotNull");
        ga.loadArg(0);
        ga.ifNonNull(itemNotNull.label());
        ga.showMessage(compiler, "Item is null");
        methodInfo.placeLabel(itemNotNull);
        ga.showObjectVariable(compiler, "itemArg", 0);
        // end diagnostic code

        ga.loadThis();
        ga.getField(Type.getType(ForItemMappingAction.class), "context", Type.getType(XPathContext.class));
        //ga.storeLocal(contextVar);
        compiler.initNewMethod(ga, false);
        ga.loadLocal(contextVar);
        ga.push(slotNumber);
        ga.loadArg(0);
        ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);

        compiler.compileToItem(action);
        ga.showMessage(compiler, "Returning");
        ga.returnValue();
        ga.endMethod();
        compiler.popCurrentMethodInfo();
        //pw.flush();
        cv.visitEnd();
        verify(cw, "For expression at line " + expression.getLocation().getLineNumber(), compiler.isDebugByteCode());
        return compiler.makeClass(cw, className);
    }


}

// Copyright (c) 2011-2022 Saxonica Limited
