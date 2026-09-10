package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.CastableExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate bytecode for evaluation of a CastableExpression
 */
public class CastableExpressionCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        CastableExpression castableExpr = (CastableExpression) expression;
        Expression operand = castableExpr.getBaseExpression();
        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        ItemType itemType = operand.getItemType();
        boolean maybeNode = th.relationship(itemType, AnyNodeTest.getInstance()) != Affinity.DISJOINT;
        boolean maybeAtomic = th.relationship(itemType, BuiltInAtomicType.ANY_ATOMIC) != Affinity.DISJOINT;

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "CastableExpression-boolean");

        LabelInfo end = methodInfo.newLabel("endCastable");
        LabelInfo done = methodInfo.newLabel("doneCastable");
        LabelInfo zeroLab = methodInfo.newLabel("zeroLab");
        LabelInfo notNull = methodInfo.newLabel("notNullCastable");
        LabelInfo notInstance = methodInfo.newLabel("notInstanceCastable");
        LabelInfo notGTLab = methodInfo.newLabel("GTLab");
        LabelInfo GTLab2 = methodInfo.newLabel("GTLab2");
        IteratorDetails opIter = compiler.compileToIterator(operand);
        int itrVar = methodInfo.allocateLocal(opIter.iteratorClass);
        ga.storeLocal(itrVar);
        ga.push(0);
        int countVar = ga.newLocal(Type.INT_TYPE);
        ga.storeLocal(countVar);
        int atomicVar = methodInfo.allocateLocal(AtomicValue.class);

        //TODO: Do not need to iterate if static cardinality is zero or one
        LabelInfo loop = methodInfo.placeNewLabel("loop");
        ga.loadLocal(itrVar);
        ga.invokeInstanceMethod(opIter.iteratorClass, "next");
        ga.dup();
        ga.ifNonNull(notNull.label());
        ga.pop();
        ga.goTo(done);
        methodInfo.placeLabel(notNull);

        if (maybeNode) {
            ga.dup();
            ga.instanceOf(Type.getType(NodeInfo.class));
            ga.ifZCmp(GeneratorAdapter.EQ, notInstance.label());
            ga.checkClass(NodeInfo.class);
            ga.invokeInstanceMethod(NodeInfo.class, "atomize");
            int atomizedValueVar = methodInfo.allocateLocal(AtomicSequence.class);
            ga.storeLocal(atomizedValueVar);
            ga.loadLocal(atomizedValueVar);
            ga.invokeInstanceMethod(AtomicSequence.class, "getLength");
            int lenVar = ga.newLocal(Type.INT_TYPE);
            ga.storeLocal(lenVar);
            ga.loadLocal(countVar);
            ga.loadLocal(lenVar);
            ga.math(GeneratorAdapter.ADD, Type.INT_TYPE);
            ga.storeLocal(countVar);
            ga.loadLocal(countVar);
            ga.push(1);
            ga.ifICmp(GeneratorAdapter.LE, notGTLab.label());
            ga.push(false);
            ga.goTo(end);

            methodInfo.placeLabel(notGTLab);
            ga.loadLocal(lenVar);
            ga.ifZCmp(GeneratorAdapter.EQ, loop.label());
            ga.loadLocal(atomizedValueVar);
            ga.push(0);
            ga.invokeInstanceMethod(AtomicSequence.class, "itemAt", Integer.TYPE);
            ga.checkClass(AtomicValue.class);
            ga.storeLocal(atomicVar);
            compileIsCastable(compiler, castableExpr, atomicVar);
            ga.ifZCmp(GeneratorAdapter.NE, loop.label());
            ga.push(false);
            ga.goTo(end);

            methodInfo.placeLabel(notInstance);
            //ga.pop();
            methodInfo.releaseLocal(atomizedValueVar);
        }
        if (maybeAtomic) {
            /*
                *  AtomicValue av = (AtomicValue)item;
                   count++;
                   if (count > 1) {
                       return false;
                   }
                   if (!isCastable(av, getTargetType(), context)) {
                       return false;
                   }
                * */
            ga.checkClass(AtomicValue.class);
            ga.storeLocal(atomicVar);
            ga.loadLocal(countVar);
            ga.push(1);
            ga.math(GeneratorAdapter.ADD, Type.INT_TYPE);
            ga.storeLocal(countVar);
            ga.loadLocal(countVar);
            ga.push(1);
            ga.ifICmp(GeneratorAdapter.LE, GTLab2.label());
            ga.push(false);
            ga.goTo(end);
            methodInfo.placeLabel(GTLab2);
            compileIsCastable(compiler, castableExpr, atomicVar);
            ga.ifZCmp(GeneratorAdapter.NE, loop.label());
            ga.push(false);
            ga.goTo(end);

            //ga.mark(methodInfo.markLabel(isCastable));
        } else {
            ga.pop();
        }
        ga.goTo(loop);
        methodInfo.placeLabel(done);

        if (castableExpr.allowsEmpty()) {
            ga.push(true);
            methodInfo.placeLabel(end);
            return;
        }
        ga.loadLocal(countVar);
        ga.ifZCmp(GeneratorAdapter.EQ, zeroLab.label());
        ga.push(true);
        ga.goTo(end);
        methodInfo.placeLabel(zeroLab);
        ga.push(false);

        methodInfo.placeLabel(end);
        methodInfo.releaseLocal(itrVar);
        methodInfo.releaseLocal(atomicVar);
    }


    public void compileIsCastable(CompilerService compiler, /*@NotNull*/ CastableExpression castableExpr, int atomicVar) {
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        Generator ga = compiler.getCurrentGenerator();
        Converter converter = castableExpr.getConverter();
        LabelInfo end = methodInfo.newLabel("endCastable");
        LabelInfo notSuccessful = methodInfo.newLabel("notSuccessful");
        if (converter == null) {
            ConversionRules conversionRules = compiler.getConfiguration().getConversionRules();
            ExpressionCompiler.allocateStatic(compiler, conversionRules);
            ga.loadLocal(atomicVar);
            ga.invokeInstanceMethod(AtomicValue.class, "getPrimitiveType");
            allocateStatic(compiler, castableExpr.getTargetType());
            ga.invokeInstanceMethod(ConversionRules.class, "getConverter", AtomicType.class, AtomicType.class);
            int converterVar = methodInfo.allocateLocal(Converter.class);
            ga.storeLocal(converterVar);
            ga.loadLocal(converterVar);
            LabelInfo notNull1 = methodInfo.newLabel("notNull1");
            LabelInfo notLabel = methodInfo.newLabel("notLabel");
            ga.ifNonNull(notNull1.label());
            ga.push(false);
            ga.goTo(end);

            methodInfo.placeLabel(notNull1);
            ga.loadLocal(converterVar);
            ga.invokeInstanceMethod(Converter.class, "isAlwaysSuccessful");
            ga.ifZCmp(GeneratorAdapter.EQ, notSuccessful.label());
            ga.push(true);
            ga.goTo(end);

            methodInfo.placeLabel(notSuccessful);

            if (castableExpr.getNamespaceResolver() != null) {
                ga.loadLocal(converterVar);
                allocateStatic(compiler, castableExpr.getNamespaceResolver());
                ga.invokeInstanceMethod(Converter.class, "setNamespaceResolver", NamespaceResolver.class);
                ga.storeLocal(converterVar);
            }
            ga.loadLocal(converterVar);
            methodInfo.releaseLocal(converterVar);
        } else {
            ExpressionCompiler.allocateStatic(compiler, converter);
        }
        ga.loadLocal(atomicVar);
        ga.invokeInstanceMethod(Converter.class, "convert", AtomicValue.class);
        ga.instanceOf(Type.getType(ValidationFailure.class));
        ga.not();

        methodInfo.placeLabel(end);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
