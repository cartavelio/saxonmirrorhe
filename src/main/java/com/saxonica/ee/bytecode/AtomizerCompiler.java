package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Atomizer;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;


/**
 * Generate code for Atomizer
 */
public class AtomizerCompiler extends ToIteratorCompiler {


    /**
     * Generate bytecode to evaluate the Atomizer as an Item
     * Precondition: none.
     * Postcondition: the stack contains an Item representing the result of the expression
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return the class of item generated
     */
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        Atomizer atomizer = (Atomizer) expression;
        Expression arg = ((Atomizer) expression).getBaseExpression();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "AtomizerCompiler-Item");
        visitLineNumber(compiler, ga, expression);

        Class<? extends Item> operandClass = compiler.compileToItem(arg);

        // assert stack : item

        boolean allowEmpty = Cardinality.allowsZero(atomizer.getCardinality());

        LabelInfo end = methodInfo.newLabel("endAtomizer");
        if (allowEmpty) {
            // generate "if (null) return null"
            ga.dup();
            ga.ifNull(end.label());
        }

        // assert stack : item

        ga.invokeInstanceMethod(operandClass, "atomize");
        ga.invokeInstanceMethod(AtomicSequence.class, "head");
        methodInfo.placeLabel(end);
        return AtomicValue.class;
    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        if ((requiredClass == String.class || requiredClass == UnicodeString.class) && ((Atomizer) expression).isUntyped()) {
            Atomizer atomizer = (Atomizer) expression;
            Expression arg = ((Atomizer) expression).getBaseExpression();
            Generator ga = compiler.getCurrentGenerator();
            GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
            visitAnnotation(compiler, "AtomizerCompiler-String");
            visitLineNumber(compiler, ga, expression);

            boolean allowEmpty = Cardinality.allowsZero(atomizer.getCardinality());

            Class<? extends Item> operandClass = compiler.compileToItem(arg);
            operandClass = ga.downCast(operandClass, Item.class, true);
            LabelInfo exit = null;
            if (allowEmpty) {
                LabelInfo notNull = methodInfo.newLabel("ACstrNotNull");
                ga.dup();
                ga.ifNonNull(notNull.label());
                exit = methodInfo.newLabel("itemToStringExit");
                handleEmptyStringResult(onEmpty, ga, methodInfo, exit);
                methodInfo.placeLabel(notNull);
            }

            ga.invokeInstanceMethod(operandClass, requiredClass == String.class ? "getStringValue" : "getUnicodeStringValue");

            if (exit != null) {
                methodInfo.placeLabel(exit);
            }

        } else {
            super.compileToPrimitive(compiler, expression, requiredClass, onEmpty);
        }
    }

    /**
     * Generate bytecode to evaluate the Atomizer as a MappingIterator
     * (i.e. return new MappingIterator(base, AtomizingFunction.getInstance()))
     * Precondition: none.
     * Postcondition: the stack contains a MappingIterator representing the result of the expression
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return
     */
    @Override
    public IteratorDetails compileToIterator(CompilerService compiler,
                                  /*@NotNull*/ Expression expression) throws CannotCompileException {

        Expression arg = ((Atomizer) expression).getBaseExpression();
        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "AtomizerCompiler-Itr");
        boolean untyped = ((Atomizer) expression).isUntyped();

        compiler.compileToIterator(arg);
        ga.push(untyped && arg.getItemType() instanceof NodeTest);
        ga.invokeStaticMethod(Atomizer.class, "getAtomizingIterator", SequenceIterator.class, Boolean.TYPE);
        return new IteratorDetails(SequenceIterator.class, AtomicValue.class);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
