package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.*;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.SequenceType;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;


/**
 * Generate code for a LocalVariableReference
 */
public class LocalVariableReferenceCompiler extends ToIteratorCompiler {


    /**
     * Generate bytecode to get the LocalVariable from the Context as a ValueRepresentation
     * Precondition: none.
     * Postcondition: the stack contains an ValueRepresentation representing the LocalVariable
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return the class of item generated
     */
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        LocalVariableReference varRef = (LocalVariableReference) expression;

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "LocalVariableReference-Item - " + varRef.getDisplayName());
        visitLineNumber(compiler, ga, expression);
        int localSlot = varRef.getSlotNumber();
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getStackFrame");
        ga.invokeInstanceMethod(StackFrame.class, "getStackFrameValues");
        ga.push(localSlot);
        ga.arrayLoad(Type.getType(Sequence.class));
        ga.invokeInstanceMethod(Sequence.class, "head");
        Binding binding = ((VariableReference) expression).getBinding();
        SequenceType declaredType = binding.getRequiredType();
        ItemType itemType = declaredType.getPrimaryType();;
        if (declaredType.equals(SequenceType.SINGLE_ITEM) && binding instanceof Assignation) {
            itemType = ((Assignation) binding).getSequence().getItemType();
        }
        Class<? extends Item> returnClass = VariableReferenceCompiler.classForItemType(itemType);
        visitAnnotation(compiler, "LocalVariableReferenceCompiler, localSlotNumber: " + localSlot);
        return returnClass;
    }


    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        LocalVariableReference varRef = (LocalVariableReference) expression;

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "LocalVariableReference-Push - " + varRef.getDisplayName());
        visitLineNumber(compiler, ga, expression);
        int localSlot = varRef.getSlotNumber();

        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getStackFrame");
        ga.invokeInstanceMethod(StackFrame.class, "getStackFrameValues");
        ga.push(localSlot);
        ga.arrayLoad(Type.getType(Sequence.class));
//        ga.push(localSlot);
//        ga.invokeInstanceMethod(XPathContext.class, "evaluateLocalVariable", Integer.TYPE);

        LabelInfo notInstance = methodInfo.newLabel("notInstance");

        int actualVar = methodInfo.allocateLocal(Sequence.class);
        ga.storeLocal(actualVar);
        ga.loadLocal(actualVar);
        ga.instanceOf(Type.getType(NodeInfo.class));
        ga.ifZCmp(GeneratorAdapter.EQ, notInstance.label());
        ga.newInstance(Type.getType(ZeroOrOne.class));
        ga.dup();
        ga.loadLocal(actualVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeConstructor(ZeroOrOne.class, Item.class);
        ga.storeLocal(actualVar);

        methodInfo.placeLabel(notInstance);
        ga.loadLocal(actualVar);
        ga.checkClass(Sequence.class);
        compiler.generateGetOutputter();
        allocateStatic(compiler, varRef.getLocation());
        ga.invokeStaticMethod(SequenceTool.class, "process", Sequence.class, Outputter.class, Location.class);

        methodInfo.releaseLocal(actualVar);

    }

    /**
     * Generate bytecode to get the LocalVariable from the Context as a ValueRepresentation
     * Precondition: none.
     * Postcondition: the stack contains an ValueRepresentation as a SingletonIterator representing the LocalVariable
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return
     */
    @Override
    public IteratorDetails compileToIterator(CompilerService compiler,
                                             Expression expression) throws CannotCompileException {

        LocalVariableReference varRef = (LocalVariableReference) expression;

        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "LocalVariableReference-Itr - " + varRef.getDisplayName());
        visitLineNumber(compiler, ga, expression);
        int localSlot = varRef.getSlotNumber();

        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getStackFrame");
        ga.invokeInstanceMethod(StackFrame.class, "getStackFrameValues");
        ga.push(localSlot);
        ga.arrayLoad(Type.getType(Sequence.class));
//        ga.push(localSlot);
//        ga.invokeInstanceMethod(XPathContext.class, "evaluateLocalVariable", Integer.TYPE);
        ga.invokeInstanceMethod(Sequence.class, "iterate");
        return new IteratorDetails(SequenceIterator.class, Item.class);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
