package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SingletonAtomizer;
import net.sf.saxon.expr.parser.RoleDiagnostic;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.AnyNodeTest;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.ObjectValue;
import net.sf.saxon.value.StringValue;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;


/**
 * Generate code for a SingletonAtomizer. This class combines two functions: atomizing
 * an input sequence, and checking that the atomized sequence is a singleton
 */
public class SingletonAtomizerCompiler extends ToItemCompiler {


    /**
     * Generate bytecode to evaluate the SingletonAtomizer as an Item
     * Precondition: none.
     * Postcondition: the stack contains an Item representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToItem(CompilerService compiler, final Expression expression)
            throws CannotCompileException {

        Expression arg = ((SingletonAtomizer) expression).getBaseExpression();
        if (!Cardinality.allowsMany(arg.getCardinality())) {
            compileSingletonInputToItem(compiler, expression);
        } else {
            Generator ga = compiler.getCurrentGenerator();
            GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
            visitAnnotation(compiler, "SingletonAtomizerCompiler");
            final RoleDiagnostic role = ((SingletonAtomizer) expression).getRole();
            Configuration config = compiler.getConfiguration();
            TypeHierarchy th = config.getTypeHierarchy();
            ItemType baseType = arg.getItemType();

            boolean allowEmpty = ((SingletonAtomizer) expression).isAllowEmpty();

            boolean maybeNode = th.relationship(baseType, AnyNodeTest.getInstance()) != Affinity.DISJOINT;
            boolean maybeAtomic = th.relationship(baseType, BuiltInAtomicType.ANY_ATOMIC) != Affinity.DISJOINT;
            boolean maybeFunction = th.relationship(baseType, AnyFunctionType.getInstance()) != Affinity.DISJOINT;
            boolean maybeExternal = th.relationship(baseType, config.getJavaExternalObjectType(Object.class)) != Affinity.DISJOINT;

            boolean alwaysNode = th.isSubType(baseType, AnyNodeTest.getInstance());
            boolean alwaysAtomic = th.isSubType(baseType, BuiltInAtomicType.ANY_ATOMIC);
            boolean alwaysFunction = th.isSubType(baseType, AnyFunctionType.getInstance());
            boolean alwaysExternal = th.isSubType(baseType, config.getJavaExternalObjectType(Object.class));

            int iterVar;
            compiler.compileToIterator(arg);
            visitLineNumber(compiler, ga, expression);
            iterVar = methodInfo.allocateLocal(SequenceIterator.class);
            ga.storeLocal(iterVar);

            int countVar = ga.newLocal(Type.INT_TYPE);
            ga.push(0);
            ga.storeLocal(countVar);

            int resultVar = methodInfo.allocateLocal(Item.class);
            ga.pushNull();
            ga.storeLocal(resultVar);

            LabelInfo done = methodInfo.newLabel("doneAtomizer");
            LabelInfo isEmpty = methodInfo.newLabel("isEmpty");
            LabelInfo moreThanOne = methodInfo.newLabel("moreThanOne");
            LabelInfo loop = methodInfo.placeNewLabel("loop");
            ga.loadLocal(iterVar);
            ga.invokeInstanceMethod(SequenceIterator.class, "next");

            int itemVar = methodInfo.allocateLocal(Item.class);
            ga.storeLocal(itemVar);
            ga.loadLocal(itemVar);
            ga.ifNull(isEmpty.label());

            if (maybeAtomic) {
                visitAnnotation(compiler, " SingletonAtomizerCompiler-atomic");
                LabelInfo notAtomicLab = methodInfo.newLabel("notAtomicLab");
                if (!alwaysAtomic) {
                    ga.loadLocal(itemVar);
                    ga.ifNotInstance(AtomicValue.class, notAtomicLab);
                }
                ga.iinc(countVar, 1);
                ga.loadLocal(countVar);
                ga.push(1);
                ga.ifICmp(GeneratorAdapter.GT, moreThanOne.label());

                ga.loadLocal(itemVar);
                ga.storeLocal(resultVar);
                ga.goTo(loop);
                methodInfo.placeLabel(notAtomicLab);
            }

            if (maybeExternal) {
                visitAnnotation(compiler, " SingletonAtomizerCompiler-external");
                LabelInfo notExternalLab = methodInfo.newLabel("notExternalLab");
                if (!alwaysExternal) {
                    ga.loadLocal(itemVar);
                    ga.ifNotInstance(ObjectValue.class, notExternalLab);
                }
                ga.iinc(countVar, 1);
                ga.loadLocal(countVar);
                ga.push(1);
                ga.ifICmp(GeneratorAdapter.GT, moreThanOne.label());

                ga.loadLocal(itemVar);
                ga.invokeInstanceMethod(Item.class, "getUnicodeStringValue");
                ga.invokeStaticMethod(StringValue.class, "makeUStringValue", UnicodeString.class);
                ga.storeLocal(resultVar);
                ga.goTo(loop);
                methodInfo.placeLabel(notExternalLab);
            }


            if (maybeFunction) {
                visitAnnotation(compiler, " SingletonAtomizerCompiler-Function");
                LabelInfo notFnItemLab = methodInfo.newLabel("notFnItemLab");
                if (!alwaysFunction) {
                    ga.loadLocal(itemVar);
                    ga.ifNotInstance(Function.class, notFnItemLab);
                }

                // Throw error - function item not allowed
                compiler.generateDynamicError("A function item cannot appear as the " + role.getMessage(), role.getErrorCode(), expression.getLocation(), true);

                methodInfo.placeLabel(notFnItemLab);
            }


            if (maybeNode) {
                //Generate the  bytecode if the above if-condition is true
                visitAnnotation(compiler, " SingletonAtomizerCompiler-Node");
                if (!alwaysNode) {
                    ga.loadLocal(itemVar);
                    ga.ifNotInstance(NodeInfo.class, loop);
                }
                ga.loadLocal(itemVar);
                ga.checkClass(NodeInfo.class);
                ga.invokeInstanceMethod(NodeInfo.class, "atomize");
                int valueVar = methodInfo.allocateLocal(AtomicSequence.class);
                ga.storeLocal(valueVar);
                ga.loadLocal(valueVar);
                ga.invokeInstanceMethod(AtomicSequence.class, "getLength");
                ga.loadLocal(countVar);
                ga.math(GeneratorAdapter.ADD, Type.getType(Integer.TYPE));
                //ga.dup();
                ga.storeLocal(countVar);
                ga.loadLocal(countVar);
                ga.push(1);
                ga.ifICmp(GeneratorAdapter.GT, moreThanOne.label());

                ga.loadLocal(valueVar);
                ga.invokeInstanceMethod(Sequence.class, "head");
                ga.storeLocal(resultVar);
                ga.goTo(loop);

                methodInfo.releaseLocal(valueVar);
            }

            methodInfo.placeLabel(moreThanOne);
            // Throw error - found > 1
            compiler.generateDynamicError("A sequence of more than one item is not allowed as the " + role.getMessage(), role.getErrorCode(), expression.getLocation(), true);

            methodInfo.placeLabel(isEmpty);

            methodInfo.placeLabel(done);

            if (!allowEmpty) {
                LabelInfo notEmpty = methodInfo.newLabel("notEmpty");
                ga.loadLocal(countVar);
                ga.ifZCmp(GeneratorAdapter.NE, notEmpty.label());

                compiler.generateDynamicError("An empty sequence is not allowed as the " + role.getMessage(), role.getErrorCode(), expression.getLocation(), true);
                methodInfo.placeLabel(notEmpty);
            }

            ga.loadLocal(resultVar);

            methodInfo.releaseLocal(iterVar);
            methodInfo.releaseLocal(resultVar);
            methodInfo.releaseLocal(itemVar);

        }
    }

    /**
     * Compile code to evaluate the expression in the case where the input is known to have
     * cardinality zero or one
     *
     * @param compiler   the compiler service
     * @param expression the SingletonAtomizer expression to be compiled
     * @throws CannotCompileException if a failure occurs
     */

    private void compileSingletonInputToItem(CompilerService compiler, final Expression expression)
            throws CannotCompileException {

        Expression arg = ((SingletonAtomizer) expression).getBaseExpression();

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "SingletonAtomizerCompilerS-fromSingleton");
        final RoleDiagnostic role = ((SingletonAtomizer) expression).getRole();
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        ItemType baseType = arg.getItemType();

        boolean allowEmpty = ((SingletonAtomizer) expression).isAllowEmpty();

        boolean maybeNode = th.relationship(baseType, AnyNodeTest.getInstance()) != Affinity.DISJOINT;
        boolean maybeTypedNode = maybeNode && expression.getPackageData().isSchemaAware();
        boolean maybeArray = th.relationship(baseType, ArrayItemType.getInstance()) != Affinity.DISJOINT;

        LabelInfo done = methodInfo.newLabel("doneAtomizer");
        LabelInfo isEmpty = methodInfo.newLabel("isEmpty");

        compiler.compileToItem(arg);
        // Stack: inputItem
        ga.dup();
        // Stack: inputItem inputItem
        ga.ifNull(isEmpty.label());
        // Stack: inputItem
        ga.invokeInstanceMethod(Item.class, "atomize");

        if (maybeTypedNode || maybeArray) {
            // atomizing a single value is capable of returning a sequence; this is a type error
            ga.dup();
            // Stack: AtomicSequence AtomicSequence
            ga.invokeInstanceMethod(AtomicSequence.class, "getLength");
            // Stack: AtomicSequence length
            if (!allowEmpty) {
                LabelInfo notEmpty = methodInfo.newLabel("notEmpty");
                ga.dup();
                // Stack: AtomicSequence length length
                ga.ifZCmp(GeneratorAdapter.NE, notEmpty.label());
                // Throw error - found empty sequence
                // Stack: AtomicSequence length
                ga.pop();
                ga.pop();
                ga.pushNull();
                ga.goTo(isEmpty);

                methodInfo.placeLabel(notEmpty);
                // Stack: AtomicSequence length
            }

            // Stack: AtomicSequence length
            ga.push(1);
            // Stack: AtomicSequence length 1
            LabelInfo isSingleton = methodInfo.newLabel("isSingleton");
            ga.ifICmp(GeneratorAdapter.LE, isSingleton.label());
            // Throw error - found > 1
            compiler.generateDynamicError("A sequence of more than one item is not allowed as the " + role.getMessage(), role.getErrorCode(), expression.getLocation(), true);

            methodInfo.placeLabel(isSingleton);

        }
        ga.invokeInstanceMethod(AtomicSequence.class, "head");
        ga.goTo(done);

//        if (maybeAtomic) {
//            visitAnnotation(compiler, " SingletonAtomizerCompilerS-atomic");
//            if (!alwaysAtomic) {
//                ga.dup();
//                // Stack: inputItem inputItem
//                ga.ifInstance(AtomicValue.class, done);
//                // Stack: inputItem
//            }
//        }
//
//        if (maybeExternal) {
//            visitAnnotation(compiler, " SingletonAtomizerCompilerS-external");
//            LabelInfo notExternalLab = methodInfo.newLabel("notExternalLab");
//            if (!alwaysExternal) {
//                ga.dup();
//                // Stack: inputItem inputItem
//                ga.ifNotInstance(ObjectValue.class, notExternalLab);
//                // Stack: inputItem
//            }
//
//            ga.invokeInstanceMethod(Item.class, "getStringValue");
//            ga.invokeStaticMethod(StringValue.class, "makeStringValue", CharSequence.class);
//            ga.goTo(done);
//            methodInfo.placeLabel(notExternalLab);
//        }
//
//
//        if (maybeFunction) {
//            visitAnnotation(compiler, " SingletonAtomizerCompilerS-Function");
//            LabelInfo notFnItemLab = methodInfo.newLabel("notFnItemLab");
//            if (!alwaysFunction) {
//                ga.dup();
//                // Stack: inputItem inputItem
//                ga.ifNotInstance(Function.class, notFnItemLab);
//                // Stack: inputItem
//            }
//
//            // Throw error - function item not allowed
//            compiler.generateDynamicError(
//                    "A function item cannot appear as the " + role.getMessage(),
//                    role.getErrorCode(), expression, true);
//
//            methodInfo.placeLabel(notFnItemLab);
//        }
//
//
//        if (maybeNode) {
//            //Generate the  bytecode if the above if-condition is true
//            visitAnnotation(compiler, " SingletonAtomizerCompilerS-Node");
//
//            // Assert: if we get here, then the item must be a node
////            if (!alwaysNode) {
////                ga.dup();
////                // Stack: inputItem inputItem
////                ga.ifNotInstance(NodeInfo.class, notFnItemLab);
////                // Stack: inputItem
////            }
//            ga.checkClass(NodeInfo.class);
//            ga.invokeInstanceMethod(NodeInfo.class, "atomize");
//            // Stack: AtomicSequence
//            ga.dup();
//            // Stack: AtomicSequence AtomicSequence
//            ga.invokeInstanceMethod(AtomicSequence.class, "getLength");
//            // Stack: AtomicSequence length
//            if (!allowEmpty) {
//                LabelInfo notEmpty = methodInfo.newLabel("notEmpty");
//                ga.dup();
//                // Stack: AtomicSequence length length
//                ga.ifZCmp(GeneratorAdapter.NE, notEmpty.label());
//                // Throw error - found empty sequence
//                // Stack: AtomicSequence length
//                ga.pop();
//                ga.pop();
//                ga.pushNull();
//                ga.goTo(isEmpty);
//
//                methodInfo.placeLabel(notEmpty);
//                // Stack: AtomicSequence length
//            }
//
//            // Stack: AtomicSequence length
//            ga.push(1);
//            // Stack: AtomicSequence length 1
//            LabelInfo nodeOK = methodInfo.newLabel("nodeOK");
//            ga.ifICmp(GeneratorAdapter.EQ, nodeOK.label());
//            // Throw error - found > 1
//            compiler.generateDynamicError(
//                    "A sequence of more than one item is not allowed as the " + role.getMessage(),
//                    role.getErrorCode(), expression, true);
//
//            methodInfo.placeLabel(nodeOK);
//            // Stack: AtomicSequence
//            ga.invokeInstanceMethod(Sequence.class, "head");
//            // Stack: AtomicValue
//            ga.goTo(done);
//        }

        methodInfo.placeLabel(isEmpty);
        // Stack: null
        if (!allowEmpty) {
            compiler.generateDynamicError("An empty sequence is not allowed as the " + role.getMessage(), role.getErrorCode(), expression.getLocation(), true);
        }

        methodInfo.placeLabel(done);
        // Stack: AtomicValue or null


    }
}

// Copyright (c) 2011-2022 Saxonica Limited
