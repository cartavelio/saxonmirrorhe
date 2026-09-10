package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.type.AnyFunctionType;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.InstanceOfExpression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Function;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate bytecode for a call to the "instance of" expression
 */
public class InstanceOfCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        InstanceOfExpression exp = (InstanceOfExpression) expression;
        Expression arg = exp.getBaseExpression();
        int requiredCardinality = exp.getRequiredCardinality();
        ItemType requiredType = exp.getRequiredItemType();

        // iterVar = arg[0].iterator()

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "compileToBoolean(instanceOf())");

        boolean anyItem = requiredType == AnyItemType.getInstance();
        boolean anyNode = requiredType == AnyNodeTest.getInstance();
        boolean anyAtomic = requiredType == BuiltInAtomicType.ANY_ATOMIC;
        boolean anyFunction = requiredType == AnyFunctionType.getInstance();
        boolean specificType = (!(anyItem || anyNode || anyAtomic || anyFunction));

        boolean itemTypeOK = th.isSubType(arg.getItemType(), requiredType);
        boolean cardinalityOK = Cardinality.subsumes(requiredCardinality, arg.getCardinality());

        int itemTypeVar = -1;
        int configVar = -1;
        if (specificType && !itemTypeOK) {

            ExpressionCompiler.allocateStatic(compiler, requiredType);
            itemTypeVar = methodInfo.allocateLocal(ItemType.class);
            ga.storeLocal(itemTypeVar);

            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getConfiguration");
            configVar = methodInfo.allocateLocal(Configuration.class);
            ga.storeLocal(configVar);
        }

        LabelInfo returnFalse = methodInfo.newLabel("returnFalse");
        LabelInfo returnTrue = methodInfo.newLabel("returnTrue");
        LabelInfo testEmpty = methodInfo.newLabel("testEmpty");
        LabelInfo done = methodInfo.newLabel("doneInstOf");

        if (Cardinality.allowsMany(arg.getCardinality())) {

            LabelInfo closeAndReturnFalse = methodInfo.newLabel("closeAndReturnFalse");

            // static cardinality allows more than one item

            IteratorDetails argIter = compiler.compileToIterator(arg);
            int iterVar = methodInfo.allocateLocal(argIter.iteratorClass);
            ga.storeLocal(iterVar);
            visitLineNumber(compiler, ga, expression);

            // count = -1;  // the actual number of items in the sequence minus one

            int countVar = -1;
            if (!cardinalityOK) {
                countVar = ga.newLocal(Type.INT_TYPE);
                ga.push(-1);
                ga.storeLocal(countVar);
            }

            // loop:
            //   if (iterVar.next() == null) {
            //       if (countVar < 0 && !allowsZero) return false;
            //       goto done;
            //   }
            //   if (!iterVar.current().matches(itemType) return false;
            //   countVar++
            //   if (countVar > 0 && !allowsMany) return false;
            //   goto loop:
            // done:
            //   return true;

            LabelInfo loop = methodInfo.placeNewLabel("loop");
            ga.loadLocal(iterVar);
            ga.invokeInstanceMethod(argIter.iteratorClass, "next");
            if (!itemTypeOK) {
                ga.dup();
            }
            ga.ifNull(testEmpty.label());

            if (!itemTypeOK) {
                // Unless we know statically that the item type is OK, check it now
                if (anyNode) {
                    ga.instanceOf(Type.getType(NodeInfo.class));
                    ga.ifZCmp(GeneratorAdapter.EQ, closeAndReturnFalse.label());
                } else if (anyAtomic) {
                    ga.instanceOf(Type.getType(AtomicValue.class));
                    ga.ifZCmp(GeneratorAdapter.EQ, closeAndReturnFalse.label());
                } else if (anyFunction) {
                    ga.instanceOf(Type.getType(Function.class));
                    ga.ifZCmp(GeneratorAdapter.EQ, closeAndReturnFalse.label());
                } else if (specificType) {
                    ga.loadLocal(itemTypeVar);
                    ga.swap();
                    allocateStatic(compiler, th);
                    ga.invokeInstanceMethod(ItemType.class, "matches", Item.class, TypeHierarchy.class);
                    ga.ifZCmp(GeneratorAdapter.EQ, closeAndReturnFalse.label());
                }
            }

            if (!cardinalityOK) {
                ga.iinc(countVar, 1);
                if (!Cardinality.allowsMany(requiredCardinality)) {
                    ga.loadLocal(countVar);
                    // The count variable is the actual count minus one, so any value > 0 is one too many...
                    ga.ifZCmp(GeneratorAdapter.GT, closeAndReturnFalse.label());
                }
            }
            ga.goTo(loop);

            methodInfo.placeLabel(testEmpty);
            if (!itemTypeOK) {
                ga.pop();
            }
            if (!cardinalityOK && !Cardinality.allowsZero(requiredCardinality)) {
                ga.loadLocal(countVar);
                // The count variable is the actual count minus one, so any value < 0 is one too few...
                ga.ifZCmp(GeneratorAdapter.LT, returnFalse.label());
            }

            methodInfo.placeLabel(returnTrue);
            ga.push(true);
            ga.goTo(done);
            methodInfo.placeLabel(closeAndReturnFalse);
            ga.loadLocal(iterVar);
            ga.invokeInstanceMethod(SequenceIterator.class, "close");
            methodInfo.placeLabel(returnFalse);
            ga.push(false);
            methodInfo.placeLabel(done);

            methodInfo.releaseLocal(iterVar);

        } else {

            // static cardinality is 0 or 1

            compiler.compileToItem(arg);
            visitLineNumber(compiler, ga, expression);
            // if an empty sequence is possible, generate code to test for it, and return true or false
            // based on whether the required type permits an empty sequence

            if (Cardinality.allowsZero(arg.getCardinality())) {
                LabelInfo notNull = methodInfo.newLabel("notNullIO");
                ga.dup();
                ga.ifNonNull(notNull.label());
                ga.pop();
                ga.goTo(Cardinality.allowsZero(requiredCardinality) ? returnTrue.label() : returnFalse.label());
                methodInfo.placeLabel(notNull);
            }

            // if the item type isn't known statically to be OK, generate code to test it at run-time

            if (itemTypeOK) {
                ga.pop();
            } else {
                if (anyNode) {
                    ga.instanceOf(Type.getType(NodeInfo.class));
                    ga.ifZCmp(GeneratorAdapter.EQ, returnFalse.label());
                } else if (anyAtomic) {
                    ga.instanceOf(Type.getType(AtomicValue.class));
                    ga.ifZCmp(GeneratorAdapter.EQ, returnFalse.label());
                } else if (anyFunction) {
                    ga.instanceOf(Type.getType(Function.class));
                    ga.ifZCmp(GeneratorAdapter.EQ, returnFalse.label());
                } else if (specificType) {
                    ga.loadLocal(itemTypeVar);
                    ga.swap();
                    allocateStatic(compiler, th);
                    ga.invokeInstanceMethod(ItemType.class, "matches", Item.class, TypeHierarchy.class);
                    ga.ifZCmp(GeneratorAdapter.EQ, returnFalse.label());
                }
            }
            methodInfo.placeLabel(returnTrue);
            ga.push(true);
            ga.goTo(done);
            methodInfo.placeLabel(returnFalse);
            ga.push(false);
            methodInfo.placeLabel(done);
        }
        methodInfo.releaseLocal(itemTypeVar);
        methodInfo.releaseLocal(configVar);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited


