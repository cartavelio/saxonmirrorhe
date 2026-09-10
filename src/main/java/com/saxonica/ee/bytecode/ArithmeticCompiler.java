package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.ArithmeticExpression;
import net.sf.saxon.expr.Calculator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.om.Item;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.DoubleValue;
import net.sf.saxon.value.NumericValue;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

/**
 * Generate bytecode for a call to the ArithmeticExpression
 */
public class ArithmeticCompiler extends ToItemCompiler {

    /**
     * Generate bytecode to evaluate the ArithmeticExpression
     * Precondition: none.
     * Postcondition: the stack contains an item representing the result of the expression
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return the class of item generated
     */
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        ArithmeticExpression exp = (ArithmeticExpression) expression;

        visitAnnotation(compiler, "ArithmeticCompiler");
        LabelInfo end = methodInfo.newLabel("endArith");

        Class<? extends Item> lhsClass = compiler.compileToItem(exp.getLhsExpression());
        lhsClass = ga.downCast(lhsClass, AtomicValue.class, true);

        visitAnnotation(compiler, "ArithmeticCompiler-doneArg-0");

        if (Cardinality.allowsZero(exp.getLhsExpression().getCardinality())) {
            LabelInfo notNull0 = methodInfo.newLabel("notNull0");
            ga.dup();
            ga.ifNonNull(notNull0.label());
            ga.pop();
            ga.push((String) null);
            ga.goTo(end);
            methodInfo.placeLabel(notNull0);
        }
        Class<? extends Item> rhsClass = compiler.compileToItem(exp.getRhsExpression());
        rhsClass = ga.downCast(rhsClass, AtomicValue.class, true);

        visitAnnotation(compiler, "ArithmeticCompiler-doneArg-1");

        if (Cardinality.allowsZero(exp.getRhsExpression().getCardinality())) {
            LabelInfo notNull1 = methodInfo.newLabel("notNull1");
            ga.dup();
            ga.ifNonNull(notNull1.label());
            ga.pop2();
            ga.pushNull();
            ga.goTo(end);
            //Stack: oper0 oper1
            methodInfo.placeLabel(notNull1);
        }

        visitLineNumber(compiler, ga, expression);
        Calculator calc = exp.getCalculator();
        Class<? extends CalculatorCompiler> myCalcCompiler = map(calc);

        Class<? extends AtomicValue> returnClass;
        if (myCalcCompiler != null) {
            CalculatorCompiler myCalc;
            try {
                myCalc = myCalcCompiler.newInstance();
            } catch (InstantiationException | IllegalAccessException e) {
                throw new CannotCompileException(expression, e.getMessage());
            }
            myCalc.setCompilerService(compiler);
            myCalc.setExpressions(exp.getLhsExpression(), lhsClass, exp.getRhsExpression(), rhsClass);
            returnClass = myCalc.compute(ga, expression.getLocation());
        } else {
            // Fallback implementation invokes the relevant Calculator object
            // assert stack oper0 oper1
            allocateStatic(compiler, calc);
            // assert stack oper0 oper1 calc
            ga.dupX2();
            // assert stack calc oper0 oper1 calc
            ga.pop();
            // assert stack calc oper0 oper1
            compiler.generateGetContext();
            // assert stack calc oper0 oper1 context
            ga.invokeInstanceMethod(Calculator.class, "compute", AtomicValue.class, AtomicValue.class, XPathContext.class);
            returnClass = AtomicValue.class;
        }
        methodInfo.placeLabel(end);
        return returnClass;
        //methodInfo.releaseLocal(oper0Var);
        //methodInfo.releaseLocal(oper1Var);
    }

    /**
     * Generate bytecode to evaluate the expression leaving a plain Java value on the stack. The static type
     * of the expression must be such that the value is known to be representable by a value of the specified class;
     * the cardinality must be either exactly-one or zero-or-one.
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     * @param requiredClass the class of the Java value that is to be left on the stack if the generated code
     *                      exits normally. This must be one of Integer.TYPE, Double.TYPE, Float.TYPE, BigDecimal.class, CharSequence.class.
     * @param onEmpty       a label to which control will be transferred (with nothing on the stack) if the result of the
     */
    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        ArithmeticExpression exp = (ArithmeticExpression) expression;

        visitAnnotation(compiler, "ArithmeticCompiler");

        ItemType t0 = exp.getLhsExpression().getItemType();
        ItemType t1 = exp.getRhsExpression().getItemType();
        if (requiredClass == String.class || requiredClass == UnicodeString.class) {
            compiler.compileToItem(exp.getLhsExpression());
            ga.checkClass(NumericValue.class);
            ga.invokeInstanceMethod(NumericValue.class, "getDoubleValue");
            compiler.compileToItem(exp.getLhsExpression());
            ga.checkClass(NumericValue.class);
            ga.invokeInstanceMethod(NumericValue.class, "getDoubleValue");
            generateDoubleOperator((ArithmeticExpression) expression, ga);
            ga.invokeStaticMethod(DoubleValue.class, "doubleToString", Double.TYPE);
            if (requiredClass == String.class) {
                ga.invokeInstanceMethod(UnicodeString.class, "toString");
            }

        } else if (requiredClass == Double.TYPE) {
            if (th.isSubType(t0, BuiltInAtomicType.DOUBLE)) {
                compiler.compileToPrimitive(exp.getLhsExpression(), Double.TYPE, onEmpty);
            } else {
                //TODO - if primitive operand of type is known then use compileToPrimitive
                compiler.compileToItem(exp.getLhsExpression());
                ga.checkClass(NumericValue.class);
                ga.invokeInstanceMethod(NumericValue.class, "getDoubleValue");
            }
            if (onEmpty instanceof OnEmpty.UnwindAndJump) {
                ((OnEmpty.UnwindAndJump) onEmpty).getUnwindStack().push(Double.TYPE);
            }
            if (th.isSubType(t1, BuiltInAtomicType.DOUBLE)) {
                compiler.compileToPrimitive(exp.getRhsExpression(), Double.TYPE, onEmpty);
            } else {
                compiler.compileToItem(exp.getRhsExpression());
                ga.checkClass(NumericValue.class);
                ga.invokeInstanceMethod(NumericValue.class, "getDoubleValue");
            }
            generateDoubleOperator((ArithmeticExpression) expression, ga);
            if (onEmpty instanceof OnEmpty.UnwindAndJump) {
                ((OnEmpty.UnwindAndJump) onEmpty).getUnwindStack().pop();
            }
        } else if (requiredClass == Float.TYPE) {
            if (th.isSubType(t0, BuiltInAtomicType.FLOAT)) {
                compiler.compileToPrimitive(exp.getLhsExpression(), Float.TYPE, onEmpty);
            } else {
                compiler.compileToItem(exp.getLhsExpression());
                ga.checkClass(NumericValue.class);
                ga.invokeInstanceMethod(NumericValue.class, "getFloatValue");
            }
            if (!(onEmpty instanceof OnEmpty.ReturnNull)) {
                ((OnEmpty.UnwindAndJump) onEmpty).getUnwindStack().push(Float.TYPE);
            }
            if (th.isSubType(t1, BuiltInAtomicType.FLOAT)) {
                compiler.compileToPrimitive(exp.getRhsExpression(), Float.TYPE, onEmpty);
            } else {
                compiler.compileToItem(exp.getRhsExpression());
                ga.checkClass(NumericValue.class);
                ga.invokeInstanceMethod(NumericValue.class, "getFloatValue");
            }
            switch (((ArithmeticExpression) expression).getOperator()) {
                case Token.PLUS:
                    ga.math(GeneratorAdapter.ADD, Type.FLOAT_TYPE);
                    break;
                case Token.MINUS:
                    ga.math(GeneratorAdapter.SUB, Type.FLOAT_TYPE);
                    break;
                case Token.MULT:
                    ga.math(GeneratorAdapter.MUL, Type.FLOAT_TYPE);
                    break;
                case Token.DIV:
                    ga.math(GeneratorAdapter.DIV, Type.FLOAT_TYPE);
                    break;
                case Token.MOD:
                    ga.math(GeneratorAdapter.REM, Type.FLOAT_TYPE);
                    break;
                case Token.IDIV:
                    ga.math(GeneratorAdapter.DIV, Type.FLOAT_TYPE);
                    break;
                default:
                    throw new CannotCompileException();
            }
            if (!(onEmpty instanceof OnEmpty.ReturnNull)) {
                ((OnEmpty.UnwindAndJump) onEmpty).getUnwindStack().pop();
            }
        } else if (requiredClass == Long.TYPE || requiredClass == Integer.TYPE) {
            if (th.isSubType(t0, BuiltInAtomicType.INTEGER)) {
                compiler.compileToPrimitive(exp.getLhsExpression(), Long.TYPE, onEmpty);
            } else {
                compiler.compileToItem(exp.getLhsExpression());
                ga.checkClass(NumericValue.class);
                ga.invokeInstanceMethod(NumericValue.class, "longValue");
            }
            if (onEmpty instanceof OnEmpty.UnwindAndJump) {
                ((OnEmpty.UnwindAndJump) onEmpty).getUnwindStack().push(Long.TYPE);
            }
            if (th.isSubType(t1, BuiltInAtomicType.INTEGER)) {
                compiler.compileToPrimitive(exp.getRhsExpression(), Long.TYPE, onEmpty);
            } else {
                compiler.compileToItem(exp.getRhsExpression());
                ga.checkClass(NumericValue.class);
                ga.invokeInstanceMethod(NumericValue.class, "longValue");
            }
            switch (((ArithmeticExpression) expression).getOperator()) {
                case Token.PLUS:
                    ga.math(GeneratorAdapter.ADD, Type.LONG_TYPE);
                    break;
                case Token.MINUS:
                    ga.math(GeneratorAdapter.SUB, Type.LONG_TYPE);
                    break;
                case Token.MULT:
                    ga.math(GeneratorAdapter.MUL, Type.LONG_TYPE);
                    break;
                case Token.DIV:
                    ga.math(GeneratorAdapter.DIV, Type.LONG_TYPE);
                    break;
                case Token.MOD:
                    ga.math(GeneratorAdapter.REM, Type.LONG_TYPE);
                    break;
                case Token.IDIV:
                    ga.math(GeneratorAdapter.DIV, Type.LONG_TYPE);
                    break;
                default:
                    throw new CannotCompileException();
            }
            if (onEmpty instanceof OnEmpty.UnwindAndJump) {
                ((OnEmpty.UnwindAndJump) onEmpty).getUnwindStack().pop();
            }
            if (requiredClass == Integer.TYPE) {
                ga.cast(Type.LONG_TYPE, Type.INT_TYPE);
            }
        } else {
            throw new CannotCompileException();
        }
    }

    private void generateDoubleOperator(ArithmeticExpression expression, Generator ga) throws CannotCompileException {
        switch (expression.getOperator()) {
            case Token.PLUS:
                ga.math(GeneratorAdapter.ADD, Type.DOUBLE_TYPE);
                break;
            case Token.MINUS:
                ga.math(GeneratorAdapter.SUB, Type.DOUBLE_TYPE);
                break;
            case Token.MULT:
                ga.math(GeneratorAdapter.MUL, Type.DOUBLE_TYPE);
                break;
            case Token.DIV:
                ga.math(GeneratorAdapter.DIV, Type.DOUBLE_TYPE);
                break;
            case Token.MOD:
                ga.math(GeneratorAdapter.REM, Type.DOUBLE_TYPE);
                break;
            case Token.IDIV:
                ga.math(GeneratorAdapter.DIV, Type.DOUBLE_TYPE);
                break;
            default:
                throw new CannotCompileException();
        }
    }


    public static Map<Class<? extends Calculator>, Class<? extends CalculatorCompiler>> map =
            new ConcurrentHashMap<>(60);

    public static Class<? extends CalculatorCompiler> map(Calculator calc) {
        return map.get(calc.getClass());
    }

    static {
        synchronized (ArithmeticCompiler.class) {
            def(Calculator.IntegerPlusInteger.class, CalculatorCompiler.IntegerPlusInteger.class);
            def(Calculator.IntegerMinusInteger.class, CalculatorCompiler.IntegerMinusInteger.class);
            def(Calculator.IntegerTimesInteger.class, CalculatorCompiler.IntegerTimesInteger.class);
            def(Calculator.IntegerDivInteger.class, CalculatorCompiler.IntegerDivInteger.class);
            def(Calculator.IntegerModInteger.class, CalculatorCompiler.IntegerModInteger.class);
            def(Calculator.IntegerIdivInteger.class, CalculatorCompiler.IntegerIdivInteger.class);

            def(Calculator.AnyPlusAny.class, CalculatorCompiler.AnyPlusAny.class);
            def(Calculator.AnyMinusAny.class, CalculatorCompiler.AnyMinusAny.class);
            def(Calculator.AnyTimesAny.class, CalculatorCompiler.AnyTimesAny.class);
            def(Calculator.AnyDivAny.class, CalculatorCompiler.AnyDivAny.class);
            def(Calculator.AnyModAny.class, CalculatorCompiler.AnyModAny.class);
            def(Calculator.AnyIdivAny.class, CalculatorCompiler.AnyIdivAny.class);

            def(Calculator.DoublePlusDouble.class, CalculatorCompiler.DoublePlusDouble.class);
            def(Calculator.DoubleMinusDouble.class, CalculatorCompiler.DoubleMinusDouble.class);
            def(Calculator.DoubleTimesDouble.class, CalculatorCompiler.DoubleTimesDouble.class);
            def(Calculator.DoubleModDouble.class, CalculatorCompiler.DoubleModDouble.class);
            def(Calculator.DoubleDivDouble.class, CalculatorCompiler.DoubleDivDouble.class);
            // TODO: add DoubleIdivDouble (but fallback works fine)

            def(Calculator.DecimalPlusDecimal.class, CalculatorCompiler.DecimalPlusDecimal.class);
            def(Calculator.DecimalMinusDecimal.class, CalculatorCompiler.DecimalMinusDecimal.class);
            def(Calculator.DecimalTimesDecimal.class, CalculatorCompiler.DecimalTimesDecimal.class);
            def(Calculator.DecimalDivDecimal.class, CalculatorCompiler.DecimalDivDecimal.class);
            def(Calculator.DecimalModDecimal.class, CalculatorCompiler.DecimalModDecimal.class);
            def(Calculator.DecimalIdivDecimal.class, CalculatorCompiler.DecimalIdivDecimal.class);

            def(Calculator.FloatPlusFloat.class, CalculatorCompiler.FloatPlusFloat.class);
            def(Calculator.FloatMinusFloat.class, CalculatorCompiler.FloatMinusFloat.class);
            def(Calculator.FloatTimesFloat.class, CalculatorCompiler.FloatTimesFloat.class);
            def(Calculator.FloatDivFloat.class, CalculatorCompiler.FloatDivFloat.class);
            def(Calculator.FloatModFloat.class, CalculatorCompiler.FloatModFloat.class);
            def(Calculator.FloatIdivFloat.class, CalculatorCompiler.FloatIdivFloat.class);

        }
    }

    private static void def(Class<? extends Calculator> c, Class<? extends CalculatorCompiler> cg) {
        map.put(c, cg);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
