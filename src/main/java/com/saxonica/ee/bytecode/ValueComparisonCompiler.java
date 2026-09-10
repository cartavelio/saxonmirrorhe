package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.ValueComparison;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.expr.sort.CodepointCollatingComparer;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.expr.sort.ContextFreeAtomicComparer;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.Affinity;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.StringValue;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

import java.util.Stack;

/**
 * Generate code for a ValueComparison
 */
public class ValueComparisonCompiler extends ToBooleanCompiler {

    /**
     * Generate bytecode to evaluate the expression as an Item
     * Precondition: none.
     * Postcondition: the stack contains either an item (the result of the expression), or null
     * (representing an empty sequence).
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return
     */

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        ValueComparison expr = (ValueComparison) expression;
        compile(compiler, expression, expr.getLhsExpression(), expr.getRhsExpression(), expr.getOperator(), expr.getAtomicComparer(), true);
        return BooleanValue.class;
    }

    /**
     * Generate bytecode to evaluate the ValueComparison as a boolean value
     * Precondition: none.
     * Postcondition: the stack contains an boolean value representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToBoolean(CompilerService compiler,
                                 final Expression expression) throws CannotCompileException {
        ValueComparison expr = (ValueComparison) expression;
        compile(compiler, expression, expr.getLhsExpression(), expr.getRhsExpression(), expr.getOperator(), expr.getAtomicComparer(), false);
    }

    /**
     * This method handles both compileToBoolean and compileToItem. The difference is that in some circumstances
     * compileToItem returns an empty sequence when compileToBoolean returns false;
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @param lhs        the two operands of the comparison
     * @param rhs        the two operands of the comparison
     * @param op         the Operator
     * @param comparer   the AtomicComparer used to compare the two atomic values
     * @param toItem     true if compiling to an Item rather than to a boolean
     * @throws CannotCompileException if the expression cannot be compiled
     */

    private static void compile(CompilerService compiler, final Expression expression, Expression lhs, Expression rhs, int op, AtomicComparer comparer, boolean toItem)
            throws CannotCompileException {

        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        final Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, expression instanceof ValueComparison ? "ValueComparison-compare" : "SingletonComparison-compare");

        BooleanValue resultWhenEmpty = null;
        boolean needsRuntime = false;
        if (expression instanceof ValueComparison) {
            resultWhenEmpty = ((ValueComparison) expression).getResultWhenEmpty();
            needsRuntime = ((ValueComparison) expression).needsRuntimeComparabilityCheck();
        }

        boolean maybeEmpty0 = Cardinality.allowsZero(lhs.getCardinality());
        boolean maybeEmpty1 = Cardinality.allowsZero(rhs.getCardinality());
        ItemType type0 = lhs.getItemType();
        if (type0 == BuiltInAtomicType.UNTYPED_ATOMIC) {
            type0 = BuiltInAtomicType.STRING;
        }
        ItemType type1 = rhs.getItemType();
        if (type1 == BuiltInAtomicType.UNTYPED_ATOMIC) {
            type1 = BuiltInAtomicType.STRING;
        }
        LabelInfo done = methodInfo.newLabel("doneVC");
        LabelInfo returnFalse = methodInfo.newLabel("returnFalseValueComp");
        LabelInfo returnTrue = methodInfo.newLabel("returnTrueValueComp");
        //LabelInfo returnNullItem = methodInfo.newLabel("returnNullItem");
        LabelInfo returnEmpty = null;
        Stack<Class<?>> stack = new Stack<>();


        boolean finished = false;

        // Shortcut code (to avoid unnecessary boxing) for comparing two booleans or two simple integers
        // or two simple doubles or floats

        if (type0 == type1) {

            LabelInfo emptyOperandReturn;
            if (resultWhenEmpty == null && toItem) {
                returnEmpty = methodInfo.newLabel("returnEmpty");
                emptyOperandReturn = returnEmpty;
            } else if (resultWhenEmpty!= null && resultWhenEmpty.getBooleanValue()) {
                emptyOperandReturn = returnTrue;
            } else {
                emptyOperandReturn = returnFalse;
            }
            OnEmpty onEmptyBoolean = new OnEmpty.UnwindAndJump(stack, emptyOperandReturn);

            if (type0 == BuiltInAtomicType.BOOLEAN && !maybeEmpty0 && !maybeEmpty1) {
                compiler.compileToBoolean(lhs);
                compiler.compileToBoolean(rhs);
                ga.ifCmp(Type.BOOLEAN_TYPE, translateOperator(op), returnTrue.label());
                ga.goTo(returnFalse);
                finished = true;
            } else if (type0 == BuiltInAtomicType.INTEGER &&
                    compiler.isInRangeForInt(lhs) && compiler.isInRangeForInt(rhs)) {
                compiler.compileToPrimitive(lhs, Integer.TYPE, onEmptyBoolean);
                stack.push(Integer.TYPE);
                compiler.compileToPrimitive(rhs, Integer.TYPE, onEmptyBoolean);
                ga.ifCmp(Type.INT_TYPE, translateOperator(op), returnTrue.label());
                ga.goTo(returnFalse);
                finished = true;
            } else if (type0 == BuiltInAtomicType.INTEGER &&
                    compiler.isInRangeForLong(lhs) && compiler.isInRangeForLong(rhs)) {
                compiler.compileToPrimitive(lhs, Long.TYPE, onEmptyBoolean);
                stack.push(Long.TYPE);
                compiler.compileToPrimitive(rhs, Long.TYPE, onEmptyBoolean);
                ga.ifCmp(Type.LONG_TYPE, translateOperator(op), returnTrue.label());
                ga.goTo(returnFalse);
                finished = true;
            } else if (type0 == BuiltInAtomicType.DOUBLE) {
                compiler.compileToPrimitive(lhs, Double.TYPE, onEmptyBoolean);
                stack.push(Double.TYPE);
                compiler.compileToPrimitive(rhs, Double.TYPE, onEmptyBoolean);
                ga.ifCmp(Type.DOUBLE_TYPE, translateOperator(op), returnTrue.label());
                ga.goTo(returnFalse);
                finished = true;
            } else if (type0 == BuiltInAtomicType.FLOAT) {
                compiler.compileToPrimitive(lhs, Float.TYPE, onEmptyBoolean);
                stack.push(Float.TYPE);
                compiler.compileToPrimitive(rhs, Float.TYPE, onEmptyBoolean);
                ga.ifCmp(Type.FLOAT_TYPE, translateOperator(op), returnTrue.label());
                ga.goTo(returnFalse);
                finished = true;
//            } else if (type0 == BuiltInAtomicType.STRING && comparer instanceof CodepointCollatingComparer) {
//                switch (op) {
//                    case Token.FEQ:
//                    case Token.FNE:
//                        compiler.compileToPrimitive(lhs, String.class, toItem ? onEmptyItem : onEmptyBoolean);
//                        stack.push(String.class);
//                        compiler.compileToPrimitive(rhs, String.class, toItem ? onEmptyItem : onEmptyBoolean);
//                        ga.invokeInstanceMethod(String.class, "equals", Object.class);
//                        if (op == Token.FNE) {
//                            ga.not();
//                        }
//                        ga.goTo(done);
//                        break;
//                    case Token.FGT:
//                    case Token.FLT:
//                    case Token.FGE:
//                    case Token.FLE:
//                        compiler.compileToPrimitive(lhs, CharSequence.class, toItem ? onEmptyItem : onEmptyBoolean);
//                        stack.push(CharSequence.class);
//                        compiler.compileToPrimitive(rhs, CharSequence.class, toItem ? onEmptyItem : onEmptyBoolean);
//                        ga.invokeStaticMethod(CodepointCollator.class, "compareCS", CharSequence.class, CharSequence.class);
//                        ga.ifZCmp(translateOperator(op), returnTrue.label());
//                        ga.goTo(returnFalse);
//
//                        break;
//                    default:
//                        throw new IllegalStateException("unknown operator");
//
//                }
//                finished = true;

            }
/*            if (toItem && finished) {
                methodInfo.placeLabel(returnNullItem);
                returnEmpty = methodInfo.newLabel("returnEmpty");
                ga.goTo(returnEmpty);
            } */
        }

        if (!finished) {
            Class<? extends Item> lhsClass = compiler.compileToItem(lhs);
            lhsClass = ga.downCast(lhsClass, AtomicValue.class);
            final int op0Var = methodInfo.allocateLocal(lhsClass);
            ga.storeLocal(op0Var);

            if (maybeEmpty0) {
                ga.loadLocal(op0Var);
                if (toItem) {
                    if (resultWhenEmpty == null) {
                        returnEmpty = methodInfo.newLabel("returnEmpty");
                        ga.ifNull(returnEmpty.label());
                    } else {
                        ga.ifNull(resultWhenEmpty.getBooleanValue() ? returnTrue.label() : returnFalse.label());
                    }
                } else {
                    ga.ifNull(resultWhenEmpty == BooleanValue.TRUE ? returnTrue.label() : returnFalse.label());
                }
            }

            Class<? extends Item> rhsClass = compiler.compileToItem(rhs);
            rhsClass = ga.downCast(rhsClass, AtomicValue.class);
            final int op1Var = methodInfo.allocateLocal(rhsClass);
            ga.storeLocal(op1Var);

            if (maybeEmpty1) {
                ga.loadLocal(op1Var);
                if (toItem) {
                    if (resultWhenEmpty == null) {
                        if (returnEmpty == null) {
                            returnEmpty = methodInfo.newLabel("returnEmpty");
                        }
                        ga.ifNull(returnEmpty.label());
                    } else {
                        ga.ifNull(resultWhenEmpty.getBooleanValue() ? returnTrue.label() : returnFalse.label());
                    }
                } else {
                    ga.ifNull(resultWhenEmpty == BooleanValue.TRUE ? returnTrue.label() : returnFalse.label());
                }
            }

            if (needsRuntime) {
                visitLineNumber(compiler, ga, expression);
                LabelInfo isCompLab = methodInfo.newLabel("isCompLab");
                ga.loadLocal(op0Var);
                //ga.checkClass(AtomicValue.class);
                ga.invokeInstanceMethod(lhsClass, "getPrimitiveType");
                ga.loadLocal(op1Var);
                //ga.checkClass(AtomicValue.class);
                ga.invokeInstanceMethod(rhsClass, "getPrimitiveType");
                ga.push(Token.isOrderedOperator(op));
                ga.invokeStaticMethod(net.sf.saxon.type.Type.class, "isGuaranteedComparable", BuiltInAtomicType.class, BuiltInAtomicType.class, Boolean.TYPE);
                ga.ifZCmp(GeneratorAdapter.NE, isCompLab.label());
                // Throw error - types not comparable
                ga.loadLocal(op0Var);
                //ga.checkClass(AtomicValue.class);
                ga.loadLocal(op1Var);
                //ga.checkClass(AtomicValue.class);
                ga.push(expression.getLocation().getSystemId());
                ga.push(expression.getLocation().getLineNumber());
                ga.invokeStaticMethod(Callback.class, "makeNonComparableException", AtomicValue.class, AtomicValue.class, String.class, Integer.TYPE);
                ga.throwException();
                methodInfo.placeLabel(isCompLab);
            }

            /*
                *  if (v0.isNaN() || v1.isNaN())
                *   return (op == Token.FNE);
                */

            Affinity r0d = th.relationship(lhs.getItemType(), BuiltInAtomicType.DOUBLE);
            Affinity r0f = th.relationship(lhs.getItemType(), BuiltInAtomicType.FLOAT);
            Affinity r1d = th.relationship(rhs.getItemType(), BuiltInAtomicType.DOUBLE);
            Affinity r1f = th.relationship(rhs.getItemType(), BuiltInAtomicType.FLOAT);

            if (r0d != Affinity.DISJOINT || r0f != Affinity.DISJOINT) {
                // The first operand might be a NaN: generate code to test whether it actually is
                ga.loadLocal(op0Var);
                ga.checkClass(AtomicValue.class);
                ga.invokeInstanceMethod(AtomicValue.class, "isNaN");
                ga.ifZCmp(GeneratorAdapter.NE, op == Token.FNE ? returnTrue.label() : returnFalse.label());
            }

            if (r1d != Affinity.DISJOINT || r1f != Affinity.DISJOINT) {
                // The second operand might be a NaN: generate code to test whether it actually is
                ga.loadLocal(op1Var);
                ga.checkClass(AtomicValue.class);
                ga.invokeInstanceMethod(AtomicValue.class, "isNaN");
                ga.ifZCmp(GeneratorAdapter.NE, op == Token.FNE ? returnTrue.label() : returnFalse.label());
            }

            // We generate explicit code for some commonly-used comparers; others are handled
            // by calling the comparer at run-time
            LabelInfo L0 = methodInfo.newLabel("L0");
            LabelInfo endTryCatch = methodInfo.newLabel("EndTryCatch_ValueCom");
            LabelInfo L1 = methodInfo.newLabel("L1");
            LabelInfo L2 = methodInfo.newLabel("L2");
            ga.visitTryCatchBlock(L0, L1, L2, "java/lang/ClassCastException");
            /* try block*/
            methodInfo.placeLabel(L0);
            visitLineNumber(compiler, ga, expression);
            if (comparer instanceof ContextFreeAtomicComparer) {
                switch (op) {
                    case Token.FEQ:
                    case Token.FNE:
                        ga.loadLocal(op0Var);
                        ga.loadLocal(op1Var);
                        ga.invokeInstanceMethod(AtomicValue.class, "equals", Object.class);
                        if (op == Token.FNE) {
                            ga.not();
                        }
                        ga.goTo(done);
                        break;
                    case Token.FGT:
                    case Token.FLT:
                    case Token.FGE:
                    case Token.FLE:
                        ga.loadLocal(op0Var);
                        ga.checkClass(Comparable.class);
                        ga.loadLocal(op1Var);
                        ga.invokeInstanceMethod(Comparable.class, "compareTo", Object.class);
                        ga.ifZCmp(translateOperator(op), returnTrue.label());
                        ga.goTo(returnFalse);
                        break;
                    default:
                        throw new IllegalStateException("unknown operator");

                }
            } else if (comparer instanceof CodepointCollatingComparer) {
                switch (op) {
                    case Token.FEQ:
                    case Token.FNE:
                        ga.loadLocal(op0Var);
                        ga.checkClass(StringValue.class);
                        ga.loadLocal(op1Var);
                        ga.checkClass(StringValue.class);
                        ga.invokeInstanceMethod(Object.class, "equals", Object.class);
                        if (op == Token.FNE) {
                            ga.not();
                        }
                        ga.goTo(done);
                        break;
                    case Token.FGT:
                    case Token.FLT:
                    case Token.FGE:
                    case Token.FLE:
                        ga.invokeStaticMethod(CodepointCollator.class, "getInstance");
                        ga.loadLocal(op0Var);
                        ga.invokeInstanceMethod(Item.class, "getUnicodeStringValue");
                        ga.loadLocal(op1Var);
                        ga.invokeInstanceMethod(Item.class, "getUnicodeStringValue");
                        ga.invokeInstanceMethod(CodepointCollator.class, "compareStrings",
                                                UnicodeString.class, UnicodeString.class);
                        ga.ifZCmp(translateOperator(op), returnTrue.label());
                        ga.goTo(returnFalse);
                        break;
                    default:
                        throw new IllegalStateException("unknown operator");

                }
            } else {
                allocateStatic(compiler, comparer);
                compiler.generateGetContext();
                ga.invokeInstanceMethod(AtomicComparer.class, "provideContext", XPathContext.class);
                int comparerVar = methodInfo.allocateLocal(AtomicComparer.class);
                ga.storeLocal(comparerVar);
                switch (op) {
                    case Token.FEQ:
                    case Token.FNE:
                        ga.loadLocal(comparerVar);
                        ga.loadLocal(op0Var);
                        ga.loadLocal(op1Var);
                        ga.invokeInstanceMethod(AtomicComparer.class, "comparesEqual", AtomicValue.class, AtomicValue.class);
                        if (op == Token.FNE) {
                            ga.not();
                        }
                        ga.goTo(done);
                        break;
                    case Token.FGT:
                    case Token.FLT:
                    case Token.FGE:
                    case Token.FLE:
                        ga.loadLocal(comparerVar);
                        ga.loadLocal(op0Var);
                        ga.loadLocal(op1Var);
                        ga.invokeInstanceMethod(AtomicComparer.class, "compareAtomicValues", AtomicValue.class, AtomicValue.class);
                        ga.ifZCmp(translateOperator(op), returnTrue.label());
                        ga.goTo(returnFalse);
                        break;
                    default:
                        // Throw error - unknown operator
                        throw new IllegalStateException("unknown operator");
                }
                methodInfo.releaseLocal(comparerVar);
            }
            methodInfo.releaseLocal(op0Var);
            methodInfo.releaseLocal(op1Var);
            methodInfo.placeLabel(L1);
            ga.goTo(endTryCatch.label());

            /*catch block*/
            methodInfo.placeLabel(L2);
            visitAnnotation(compiler, "Catch");
            ga.loadLocal(op0Var);
            ga.checkClass(AtomicValue.class);
            ga.loadLocal(op1Var);
            ga.checkClass(AtomicValue.class);
            ga.push(expression.getLocation().getSystemId());
            ga.push(expression.getLocation().getLineNumber());
            ga.invokeStaticMethod(Callback.class, "makeNonComparableException", AtomicValue.class, AtomicValue.class, String.class, Integer.TYPE);
            ga.throwException();

            methodInfo.placeLabel(endTryCatch);
        }


        if (returnTrue.isUsed()) {
            methodInfo.placeLabel(returnTrue);
            ga.push(true);
            if (returnFalse.isUsed()) {
                ga.goTo(done);
            }
        }
        if (returnFalse.isUsed()) {
            methodInfo.placeLabel(returnFalse);
            ga.showMessage(compiler, "ValueComp-returnFalse returnEmpty:" + returnEmpty);
            ga.push(false);
        }


        methodInfo.placeLabel(done);

        // if compiling to an Item, wrap the boolean result as a BooleanValue

        if (toItem) {
            ga.showMessage(compiler, "ValueComp-toItem");
            ga.invokeStaticMethod(BooleanValue.class, "get", Boolean.TYPE);
        }

        if (returnEmpty != null) {
            LabelInfo reallyDone = methodInfo.newLabel("reallyDone");
            ga.goTo(reallyDone);
            methodInfo.placeLabel(returnEmpty);
            ga.showMessage(compiler, "ValueComp-returnEmpty");
            ga.pushNull();
            //ga.push(true);
            methodInfo.placeLabel(reallyDone);
        }
    }

    public static int translateOperator(int op) {
        switch (op) {
            case Token.FEQ:
                return GeneratorAdapter.EQ;
            case Token.FNE:
                return GeneratorAdapter.NE;
            case Token.FGT:
                return GeneratorAdapter.GT;
            case Token.FLT:
                return GeneratorAdapter.LT;
            case Token.FGE:
                return GeneratorAdapter.GE;
            case Token.FLE:
                return GeneratorAdapter.LE;
            default:
                throw new UnsupportedOperationException("Unknown operator " + op);
        }
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
