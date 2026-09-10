package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.ComparisonExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.GeneralComparison;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.sort.*;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.util.Stack;


/**
 * Generate code for a GeneralComparison
 */
public class GeneralComparisonCompiler extends ToBooleanCompiler {

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        switch (((GeneralComparison) expression).getComparisonCardinality()) {
            case ONE_TO_ONE:
                compileToBooleanOneToOne(compiler, (GeneralComparison) expression);
                break;
            case MANY_TO_ONE:
                compileToBooleanManyToOne(compiler, (GeneralComparison) expression);
                break;
            case MANY_TO_MANY:
                compileToBooleanManyToMany(compiler, (GeneralComparison) expression);
                break;
        }
    }

    private void compileToBooleanOneToOne(CompilerService compiler, GeneralComparison comparison) throws CannotCompileException {
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        final Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "GeneralComparison-OneToOne-boolean");

        Expression arg0 = comparison.getLhsExpression();
        Expression arg1 = comparison.getRhsExpression();

        AtomicType type0 = (AtomicType) arg0.getItemType().getPrimitiveItemType();
        AtomicType type1 = (AtomicType) arg1.getItemType().getPrimitiveItemType();

        LabelInfo returnTrue = methodInfo.newLabel("returnTrue");
        LabelInfo returnFalse = methodInfo.newLabel("returnFalse");
        LabelInfo end = methodInfo.newLabel("sngCompEnd");

        // special-case comparison of two values known statically to be single strings

        if ((type0.equals(BuiltInAtomicType.UNTYPED_ATOMIC) || th.isSubType(type0, BuiltInAtomicType.STRING)) &&
                (type1.equals(BuiltInAtomicType.UNTYPED_ATOMIC) || th.isSubType(type1, BuiltInAtomicType.STRING))) {
            Stack<Class<?>> unwindStack = new Stack<>();
            compiler.compileToPrimitive(arg0, String.class, new OnEmpty.UnwindAndJump(unwindStack, returnFalse));
            unwindStack.add(String.class);
            compiler.compileToPrimitive(arg1, String.class, new OnEmpty.UnwindAndJump(unwindStack, returnFalse));
            if (comparison.getAtomicComparer() instanceof CodepointCollatingComparer) {
                switch (comparison.getSingletonOperator()) {
                    case Token.FEQ:
                        ga.invokeInstanceMethod(Object.class, "equals", Object.class);
                        ga.goTo(end);
                        break;
                    case Token.FNE:
                        ga.invokeInstanceMethod(Object.class, "equals", Object.class);
                        ga.not();
                        ga.goTo(end);
                        break;
                    default:
                        ga.invokeStaticMethod(CodepointCollator.class, "getInstance");
                        ga.dupX2();
                        ga.pop();
                        ga.invokeInstanceMethod(CodepointCollator.class, "compareStrings", UnicodeString.class, UnicodeString.class);
                        ga.ifZCmp(ValueComparisonCompiler.translateOperator(comparison.getSingletonOperator()), returnTrue.label());
                        ga.goTo(returnFalse);
                }
            } else {
                allocateStatic(compiler, comparison.getAtomicComparer().getCollator());
                ga.dupX2();
                ga.pop();
                ga.invokeInstanceMethod(StringCollator.class, "compareStrings", UnicodeString.class, UnicodeString.class);
                ga.ifZCmp(ValueComparisonCompiler.translateOperator(comparison.getSingletonOperator()), returnTrue.label());
                ga.goTo(returnFalse);
            }

            // return true
            methodInfo.placeLabel(returnTrue);
            ga.push(true);
            ga.goTo(end);

            // return false
            methodInfo.placeLabel(returnFalse);
            ga.push(false);

            methodInfo.placeLabel(end);

        } else {


            //        AtomicValue v1 = (AtomicValue)operand0.evaluateItem(context);
            //        if (v1==null) {
            //            return false;
            //        }

            compiler.compileToItem(arg0);
            if (Cardinality.allowsZero(arg0.getCardinality())) {
                LabelInfo nonNullArg0 = methodInfo.newLabel("nonNullArg0");
                ga.dup();
                ga.ifNonNull(nonNullArg0.label());
                ga.pop();
                ga.goTo(returnFalse);
                methodInfo.placeLabel(nonNullArg0);
            }

            //        AtomicValue v2 = (AtomicValue)operand1.evaluateItem(context);
            //        if (v2==null) {
            //            return false;
            //        }

            compiler.compileToItem(arg1);
            if (Cardinality.allowsZero(arg1.getCardinality())) {
                LabelInfo nonNullArg1 = methodInfo.newLabel("nonNullArg1");
                ga.dup();
                ga.ifNonNull(nonNullArg1.label());
                ga.pop2();
                ga.goTo(returnFalse);
                methodInfo.placeLabel(nonNullArg1);
            }

            // assert stack: item0 item1

            final int item0Var = methodInfo.allocateLocal(AtomicValue.class);
            final int item1Var = methodInfo.allocateLocal(AtomicValue.class);
            ga.checkClass(AtomicValue.class);
            ga.storeLocal(item1Var);
            ga.checkClass(AtomicValue.class);
            ga.storeLocal(item0Var);

            boolean checkTypes = comparison.needsRuntimeCheck();

            GeneralComparisonCompiler.generateComparisonCode(compiler, comparison,
                    returnTrue, returnFalse, end,
                    item0Var, item1Var,
                    type0, type1, checkTypes);

            // return true
            methodInfo.placeLabel(returnTrue);
            ga.push(true);
            ga.goTo(end);

            // return false
            methodInfo.placeLabel(returnFalse);
            ga.push(false);

            // return (with result on stack)
            methodInfo.placeLabel(end);

            methodInfo.releaseLocal(item0Var);
            methodInfo.releaseLocal(item1Var);
        }

    }

    private void compileToBooleanManyToOne(CompilerService compiler, GeneralComparison comparison) throws CannotCompileException {
        final Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "GeneralComparison-ManyToOne-boolean");

        Expression arg0 = comparison.getLhsExpression();
        Expression arg1 = comparison.getRhsExpression();

        AtomicType type0 = (AtomicType) arg0.getItemType().getPrimitiveItemType();
        AtomicType type1 = (AtomicType) arg1.getItemType().getPrimitiveItemType();

        LabelInfo returnTrue = methodInfo.newLabel("returnTrue");
        LabelInfo returnFalse = methodInfo.newLabel("returnFalse");
        LabelInfo end = methodInfo.newLabel("genCompEnd");

        //        AtomicValue v1 = (AtomicValue)operand1.evaluateItem(context);
        //        if (v1==null) {
        //            return false;
        //        }

        compiler.compileToItem(arg1);
        if (Cardinality.allowsZero(arg1.getCardinality())) {
            LabelInfo nonNullArg1 = methodInfo.newLabel("nonNullArg1");
            ga.dup();
            ga.ifNonNull(nonNullArg1.label());
            ga.pop();
            ga.goTo(returnFalse);
            methodInfo.placeLabel(nonNullArg1);
        }

        int item0Var = methodInfo.allocateLocal(AtomicValue.class);
        int item1Var = methodInfo.allocateLocal(AtomicValue.class);
        ga.checkClass(AtomicValue.class);
        ga.storeLocal(item1Var);

        compiler.compileToIterator(arg0);
        int iter0Var = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(iter0Var);

        LabelInfo loop = methodInfo.placeNewLabel("compLoop");
        LabelInfo out = methodInfo.newLabel("compout");

        ga.loadLocal(iter0Var);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.checkClass(AtomicValue.class);
        ga.storeLocal(item0Var);
        ga.loadLocal(item0Var);
        ga.ifNull(returnFalse.label());
        boolean checkTypes = comparison.needsRuntimeCheck();
        GeneralComparisonCompiler.generateComparisonCode(compiler, comparison,
                returnTrue, loop, out,
                item0Var, item1Var,
                type0, type1, checkTypes);

        // exit from comparison with result on stack
        methodInfo.placeLabel(out);
        ga.ifTrue(returnTrue);
        ga.goTo(loop);

        // return true
        methodInfo.placeLabel(returnTrue);
        ga.loadLocal(iter0Var);
        ga.invokeInstanceMethod(SequenceIterator.class, "close");
        ga.push(true);
        ga.goTo(end);

        // return false
        methodInfo.placeLabel(returnFalse);
        ga.push(false);

        // return (with result on stack)
        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(item0Var);
        methodInfo.releaseLocal(item1Var);
        methodInfo.releaseLocal(iter0Var);

    }

    public void compileToBooleanManyToMany(CompilerService compiler, GeneralComparison comparison)
            throws CannotCompileException {

        final Generator ga = compiler.getCurrentGenerator();

        // evaluate using the interpreter
        // TODO: generate code for this case (though it's uncommon)
        Expression arg0 = comparison.getLhsExpression();
        Expression arg1 = comparison.getRhsExpression();
        allocateStatic(compiler, comparison);
        compiler.compileToIterator(arg0);
        compiler.compileToIterator(arg1);
        compiler.generateGetContext();
        ga.invokeInstanceMethod(GeneralComparison.class, "evaluateManyToMany", SequenceIterator.class, SequenceIterator.class, XPathContext.class);

    }


    /**
     * Generate code to compare two items, leaving a boolean result on the top of the bytecode stack
     *
     * @param compiler    the compiler service
     * @param comparison  the comparison expression being compiled
     * @param returnTrue  label to jump to to return true
     * @param returnFalse label to jump to to return false
     * @param end         label to jump to to exit, with the result on stack
     * @param item0Var    position of local variable (or if negative, argument) holding the first item
     * @param item1Var    position of local variable (or if negative, argument) holding the second item
     * @param type0       static type of the first item
     * @param type1       static type of the second item
     * @param checkTypes  if true, code needs to be generated to check the items are comparable
     */

    public static void generateComparisonCode(CompilerService compiler, ComparisonExpression comparison,
                                              LabelInfo returnTrue, LabelInfo returnFalse, LabelInfo end,
                                              int item0Var, int item1Var,
                                              AtomicType type0, AtomicType type1,
                                              boolean checkTypes) {
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        ConversionRules rules = compiler.getConfiguration().getConversionRules();
        visitAnnotation(compiler, "generateComparisonCode-GeneralComp");
        LabelInfo notUntyped0 = methodInfo.newLabel("notUntyped0");
        LabelInfo notNumeric0 = methodInfo.newLabel("notNumeric0");
        LabelInfo doneConversion0 = methodInfo.newLabel("doneConversion0");
        LabelInfo notUntyped1 = methodInfo.newLabel("notUntyped1");
        LabelInfo notNumeric1 = methodInfo.newLabel("notNumeric1");
        LabelInfo doneConversion1 = methodInfo.newLabel("doneConversion1");
        int checkTypesVar = methodInfo.allocateLocal(Boolean.TYPE);
        ga.push(checkTypes);
        ga.storeLocal(checkTypesVar);

        //        boolean u0 = (a0 instanceof UntypedAtomicValue);
        //        boolean u1 = (a1 instanceof UntypedAtomicValue);
        //        if (u0 != u1) {
        //            // one value untyped, the other not
        //            if (u0) {
        //                // a0 is untyped atomic
        //                if (a1 instanceof NumericValue) {
        //                    return quickCompare((UntypedAtomicValue)a0, (NumericValue)a1, operator, rules);
        //                } else {
        //                    a0 = Converter.convert(a0, a1.getPrimitiveType(), rules).asAtomic();
        //                }
        //            } else {
        //                // a1 is untyped atomic
        //                if (a0 instanceof NumericValue) {
        //                    return quickCompare((UntypedAtomicValue)a1, (NumericValue)a0, Token.inverse(operator), rules);
        //                } else {
        //                    a1 = Converter.convert(a1, a0.getPrimitiveType(), rules).asAtomic();
        //                }
        //            }
        //            checkTypes = false; // No further checking needed if conversion succeeded
        //        }


        // Test if arg0 is untyped
        if (th.relationship(type0, BuiltInAtomicType.UNTYPED_ATOMIC) != Affinity.DISJOINT) {
            ga.loadArgOrLocal(item0Var);
            visitAnnotation(compiler, "generateComparisonCode-testing item0 untyped");
            ga.invokeInstanceMethod(AtomicValue.class, "isUntypedAtomic");
            ga.ifFalse(notUntyped0);
            ga.loadArgOrLocal(item1Var);
            visitAnnotation(compiler, "generateComparisonCode-testing item1 untyped");
            ga.invokeInstanceMethod(AtomicValue.class, "isUntypedAtomic");
            ga.ifTrue(doneConversion1);
            // a0 is untypedAtomic, a1 is not
            if (th.relationship(type1, NumericType.getInstance()) != Affinity.DISJOINT) {
                ga.loadArgOrLocal(item1Var);
                ga.ifNotInstance(NumericValue.class, notNumeric0);
                visitAnnotation(compiler, "generateComparisonCode-item1 numeric");
                ga.loadArgOrLocal(item0Var);
                ga.checkClass(StringValue.class);
                ga.loadArgOrLocal(item1Var);
                ga.checkClass(NumericValue.class);
                ga.push(comparison.getSingletonOperator());
                allocateStatic(compiler, compiler.getConfiguration().getConversionRules());
//                quickCompare(
//                        StringItem a0, NumericValue a1, int operator, ConversionRules rules)
                ga.invokeStaticMethod(UntypedNumericComparer.class, "quickCompare",
                                      StringValue.class, NumericValue.class, Integer.TYPE, ConversionRules.class);
                ga.goTo(end);

//                Converter untypedToDouble = rules.getConverter(BuiltInAtomicType.UNTYPED_ATOMIC, BuiltInAtomicType.DOUBLE);
//                allocateStatic(compiler, untypedToDouble);
//                ga.loadArgOrLocal(item0Var);
//                ga.invokeInstanceMethod(Converter.class, "convert", AtomicValue.class);
//                ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
//                ga.storeArgOrLocal(item0Var);
//                ga.goTo(doneConversion0);


                methodInfo.placeLabel(notNumeric0);
            }
            if (type1 == BuiltInAtomicType.ANY_ATOMIC) {
                allocateStatic(compiler, rules);
                ga.getStaticField(BuiltInAtomicType.class, "UNTYPED_ATOMIC", BuiltInAtomicType.class);
                ga.loadArgOrLocal(item1Var);
                ga.invokeInstanceMethod(AtomicValue.class, "getPrimitiveType");
                ga.invokeInstanceMethod(ConversionRules.class, "getConverter", AtomicType.class, AtomicType.class);
            } else {
                Converter untypedToOther = rules.getConverter(BuiltInAtomicType.UNTYPED_ATOMIC, type1);
                allocateStatic(compiler, untypedToOther);
            }
            ga.loadArgOrLocal(item0Var);
            ga.invokeInstanceMethod(Converter.class, "convert", AtomicValue.class);
            ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
            ga.storeArgOrLocal(item0Var);
            ga.push(false);
            ga.storeLocal(checkTypesVar);
            methodInfo.placeLabel(doneConversion0);
            methodInfo.placeLabel(notUntyped0);
            visitAnnotation(compiler, "generateComparisonCode-label notUntyped0");
        }

        // Test if arg1 is untyped

        if (th.relationship(type1, BuiltInAtomicType.UNTYPED_ATOMIC) != Affinity.DISJOINT) {
            if (type1 != BuiltInAtomicType.UNTYPED_ATOMIC) {
                ga.loadArgOrLocal(item1Var);
                ga.invokeInstanceMethod(AtomicValue.class, "isUntypedAtomic");
                ga.ifFalse(notUntyped1);
            }

            // a1 is untypedAtomic, a0 is not
            if (th.relationship(type0, NumericType.getInstance()) != Affinity.DISJOINT) {
                ga.loadArgOrLocal(item0Var);
                ga.ifNotInstance(NumericValue.class, notNumeric1);

                ga.loadArgOrLocal(item1Var);
                ga.checkClass(StringValue.class);
                ga.loadArgOrLocal(item0Var);
                ga.checkClass(NumericValue.class);
                ga.push(Token.inverse(comparison.getSingletonOperator()));
                allocateStatic(compiler, compiler.getConfiguration().getConversionRules());
                // static boolean quickCompare(StringValue a0, NumericValue a1, int operator, ConversionRules rules)
                ga.invokeStaticMethod(UntypedNumericComparer.class, "quickCompare",
                                      StringValue.class, NumericValue.class, Integer.TYPE, ConversionRules.class);
                ga.goTo(end);
//                Converter untypedToDouble = rules.getConverter(BuiltInAtomicType.UNTYPED_ATOMIC, BuiltInAtomicType.DOUBLE);
//                allocateStatic(compiler, untypedToDouble);
//                ga.loadArgOrLocal(item1Var);
//                ga.invokeInstanceMethod(Converter.class, "convert", AtomicValue.class);
//                ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
//                //ga.checkClass(AtomicValue.class);
//                ga.storeArgOrLocal(item1Var);
//                ga.goTo(doneConversion1);
                methodInfo.placeLabel(notNumeric1);
            }
            if (type0 == BuiltInAtomicType.ANY_ATOMIC) {
                allocateStatic(compiler, rules);
                ga.getStaticField(BuiltInAtomicType.class, "UNTYPED_ATOMIC", BuiltInAtomicType.class);
                ga.loadArgOrLocal(item0Var);
                ga.invokeInstanceMethod(AtomicValue.class, "getPrimitiveType");
                ga.invokeInstanceMethod(ConversionRules.class, "getConverter", AtomicType.class, AtomicType.class);
                ga.loadArgOrLocal(item1Var);
                ga.invokeInstanceMethod(Converter.class, "convert", AtomicValue.class);
                //ga.checkClass(AtomicValue.class);
                ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
                ga.storeArgOrLocal(item1Var);
                ga.push(false);
                ga.storeLocal(checkTypesVar);
                ga.goTo(doneConversion1);
            } else {
                if (type0 != BuiltInAtomicType.STRING) {
                    Converter untypedToOther = rules.getConverter(BuiltInAtomicType.UNTYPED_ATOMIC, type0);
                    allocateStatic(compiler, untypedToOther);
                    ga.loadArgOrLocal(item1Var);
                    ga.invokeInstanceMethod(Converter.class, "convert", AtomicValue.class);
                    ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
                    //ga.checkClass(AtomicValue.class);
                    ga.storeArgOrLocal(item1Var);
                    ga.push(false);
                    ga.storeLocal(checkTypesVar);
                    ga.goTo(doneConversion1);
                }
            }

        }


        methodInfo.placeLabel(doneConversion1);
        methodInfo.placeLabel(notUntyped1);

        //        if (checkTypes &&
        //                    !Type.isComparable(v0.getPrimitiveType(), v1.getPrimitiveType(), Token.isOrderedOperator(op))) {
        //            XPathException e2 = new XPathException("Cannot compare " + Type.displayTypeName(v0) +
        //                " to " + Type.displayTypeName(v1));
        //            e2.setErrorCode("XPTY0004");
        //            e2.setIsTypeError(true);
        //            throw e2;
        //        }

        if (checkTypes) {
            LabelInfo doneCheckingTypes = methodInfo.newLabel("doneCheckingTypes");
            ga.loadLocal(checkTypesVar);
            ga.ifFalse(doneCheckingTypes);
            ga.loadArgOrLocal(item0Var);
            ga.invokeInstanceMethod(AtomicValue.class, "getPrimitiveType");
            ga.loadArgOrLocal(item1Var);
            ga.invokeInstanceMethod(AtomicValue.class, "getPrimitiveType");
            ga.push(Token.isOrderedOperator(comparison.getSingletonOperator()));
            ga.invokeStaticMethod(Type.class, "isGenerallyComparable", BuiltInAtomicType.class, BuiltInAtomicType.class, Boolean.TYPE);
            ga.ifTrue(doneCheckingTypes);
            ga.loadArgOrLocal(item0Var);
            ga.loadArgOrLocal(item1Var);
            ga.push(((Expression) comparison).getLocation().getSystemId());
            ga.push(((Expression) comparison).getLocation().getLineNumber());
            ga.invokeStaticMethod(Callback.class, "makeNonComparableException", AtomicValue.class, AtomicValue.class, String.class, Integer.TYPE);
            ga.throwException();
            methodInfo.placeLabel(doneCheckingTypes);
        }

        //        if (v0.isNaN() || v1.isNaN()) {
        //            return (op == Token.FNE);
        //        }

        if (th.relationship(type0, BuiltInAtomicType.DOUBLE) != Affinity.DISJOINT ||
                th.relationship(type0, BuiltInAtomicType.FLOAT) != Affinity.DISJOINT) {
            ga.loadArgOrLocal(item0Var);
            ga.invokeInstanceMethod(AtomicValue.class, "isNaN");
            ga.ifTrue(comparison.getSingletonOperator() == Token.FNE ? returnTrue : returnFalse);
        }
        if (th.relationship(type1, BuiltInAtomicType.DOUBLE) != Affinity.DISJOINT ||
                th.relationship(type1, BuiltInAtomicType.FLOAT) != Affinity.DISJOINT) {
            ga.loadArgOrLocal(item1Var);
            ga.invokeInstanceMethod(AtomicValue.class, "isNaN");
            ga.ifTrue(comparison.getSingletonOperator() == Token.FNE ? returnTrue : returnFalse);
        }

        int op = comparison.getSingletonOperator();
        if (comparison.getAtomicComparer() instanceof ContextFreeAtomicComparer) {
            switch (op) {
                case Token.FEQ:
                case Token.FNE:
                    ga.loadLocal(item0Var);
                    ga.loadLocal(item1Var);
                    ga.invokeInstanceMethod(AtomicValue.class, "equals", Object.class);
                    if (op == Token.FNE) {
                        ga.not();
                    }
                    ga.goTo(end);
                    break;
                case Token.FGT:
                case Token.FLT:
                case Token.FGE:
                case Token.FLE:
                    ga.loadLocal(item0Var);
                    ga.checkClass(Comparable.class);
                    ga.loadLocal(item1Var);
                    ga.invokeInstanceMethod(Comparable.class, "compareTo", Object.class);
                    ga.ifZCmp(ValueComparisonCompiler.translateOperator(op), returnTrue.label());
                    ga.goTo(returnFalse);
                    break;
                default:
                    throw new IllegalStateException("unknown operator");

            }
        } else if (comparison.getAtomicComparer() instanceof CodepointCollatingComparer) {
            switch (op) {
                case Token.FEQ:
                case Token.FNE:
                    ga.loadLocal(item0Var);
                    ga.checkClass(StringValue.class);
                    ga.loadLocal(item1Var);
                    ga.checkClass(StringValue.class);
                    ga.invokeInstanceMethod(StringValue.class, "equals", Object.class);
                    if (op == Token.FNE) {
                        ga.not();
                    }
                    ga.goTo(end);
                    break;
                case Token.FGT:
                case Token.FLT:
                case Token.FGE:
                case Token.FLE:

                    ga.invokeStaticMethod(CodepointCollator.class, "getInstance");
                    ga.loadLocal(item0Var);
                    ga.invokeInstanceMethod(AtomicValue.class, "getUnicodeStringValue");
                    ga.loadLocal(item1Var);
                    ga.invokeInstanceMethod(AtomicValue.class, "getUnicodeStringValue");
                    ga.invokeInstanceMethod(CodepointCollator.class, "compareStrings", UnicodeString.class, UnicodeString.class);

                    ga.ifZCmp(ValueComparisonCompiler.translateOperator(op), returnTrue.label());
                    ga.goTo(returnFalse);

                    break;
                default:
                    throw new IllegalStateException("unknown operator");

            }
        } else {
            // comparer = comparer.provideContext(context);

            allocateStatic(compiler, comparison.getAtomicComparer());
            compiler.generateGetContext();

            // prepare for call to the comparer

            ga.invokeInstanceMethod(AtomicComparer.class, "provideContext", XPathContext.class);
            ga.loadArgOrLocal(item0Var);
            ga.loadArgOrLocal(item1Var);

            // try - catch for ClassCastException errors
            LabelInfo L0 = methodInfo.newLabel("L0");
            LabelInfo L1 = methodInfo.newLabel("L1");
            LabelInfo L2 = methodInfo.newLabel("L2");
            ga.visitTryCatchBlock(L0, L1, L2, "java/lang/ClassCastException");
            methodInfo.placeLabel(L0);
            switch (comparison.getSingletonOperator()) {
                case Token.FEQ:
                    //return comparer.comparesEqual(v0, v1);
                    //methodInfo.placeLabel(L0);
                    ga.invokeInstanceMethod(AtomicComparer.class, "comparesEqual", AtomicValue.class, AtomicValue.class);
                    methodInfo.placeLabel(L1);
                    ga.goTo(end);
                    break;
                case Token.FNE:
                    //return !comparer.comparesEqual(v0, v1);
                    //methodInfo.placeLabel(L0);
                    ga.invokeInstanceMethod(AtomicComparer.class, "comparesEqual", AtomicValue.class, AtomicValue.class);
                    methodInfo.placeLabel(L1);
                    ga.not();
                    ga.goTo(end);
                    break;
                case Token.FGT:
                case Token.FLT:
                case Token.FGE:
                case Token.FLE:
                    //return comparer.compareAtomicValues(v0, v1) > 0;
                    //methodInfo.placeLabel(L0);
                    ga.invokeInstanceMethod(AtomicComparer.class, "compareAtomicValues", AtomicValue.class, AtomicValue.class);
                    methodInfo.placeLabel(L1);
                    ga.ifZCmp(ValueComparisonCompiler.translateOperator(comparison.getSingletonOperator()), returnTrue.label());
                    ga.goTo(returnFalse);
                    break;

                default:
                    throw new UnsupportedOperationException("Unknown operator");
            }


            methodInfo.releaseLocal(checkTypesVar);


            // catch block
            methodInfo.placeLabel(L2);
            ga.pop();
            ga.loadLocal(item0Var);
            ga.loadLocal(item1Var);
            ga.push(((Expression) comparison).getLocation().getSystemId());
            ga.push(((Expression) comparison).getLocation().getLineNumber());
            ga.invokeStaticMethod(Callback.class, "makeNonComparableException", AtomicValue.class, AtomicValue.class, String.class, Integer.TYPE);
            ga.throwException();
            ga.goTo(end.label());
        }
    }


}

// Copyright (c) 2011-2022 Saxonica Limited