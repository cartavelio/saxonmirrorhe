package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.GeneratedMethodInfo;
import com.saxonica.ee.bytecode.util.Generator;
import com.saxonica.ee.bytecode.util.LabelInfo;
import net.sf.saxon.expr.Calculator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.type.ConversionResult;
import net.sf.saxon.type.Converter;
import net.sf.saxon.type.Converter.FloatToInteger;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.value.*;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.GeneratorAdapter;

import javax.xml.transform.SourceLocator;
import java.math.BigDecimal;
import java.math.BigInteger;

/**
 * This class generates bytecode for arithmetic expressions; it acts as a helper class to the ArithmeticCompiler
 * class. There are many static subclasses for the different kinds of arithmetic expression
 */
public abstract class CalculatorCompiler {


    CompilerService compiler;

    Expression oper0;
    Expression oper1;

    public void setCompilerService(CompilerService compiler) {
        this.compiler = compiler;
    }

    public void setExpressions(Expression oper0, Expression oper1) {
        this.oper0 = oper0;
        this.oper1 = oper1;
    }

    /**
     * Generate bytecode to compute the result of the arithmetic expression
     * @param ga the code generator
     * @param locator the location of the arithmetic expression
     */

    public abstract void compute(Generator ga, SourceLocator locator);


    public static class IntegerPlusInteger extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "IntPlusCalculator-0");
            ga.checkClass(IntegerValue.class);
            ga.swap();
            ga.checkClass(IntegerValue.class);
            ga.invokeInstanceMethod(IntegerValue.class, "plus", IntegerValue.class);
        }

    }

    public static class IntegerMinusInteger extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "IntMinusCalculator-0");
            ga.checkClass(IntegerValue.class);
            ga.swap();
            ga.checkClass(IntegerValue.class);
            ga.swap();
            ga.invokeInstanceMethod(IntegerValue.class, "minus", IntegerValue.class);
        }
    }

    public static class IntegerTimesInteger extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "IntTimesCalculator-0");
            ga.checkClass(IntegerValue.class);
            ga.swap();
            ga.checkClass(IntegerValue.class);
            ga.invokeInstanceMethod(IntegerValue.class, "times", IntegerValue.class);
        }
    }

    public static class IntegerDivInteger extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "IntDivCalculator-0");
            ga.checkClass(IntegerValue.class);
            ga.swap();
            ga.checkClass(IntegerValue.class);
            ga.swap();
            ExpressionCompiler.allocateStatic(compiler, locator);
            ga.invokeInstanceMethod(IntegerValue.class, "div", IntegerValue.class, Location.class);
        }
    }

    public static class IntegerModInteger extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "IntModCalculator-0");
            ga.checkClass(IntegerValue.class);
            ga.swap();
            ga.checkClass(IntegerValue.class);
            ga.swap();
            ExpressionCompiler.allocateStatic(compiler, locator);
            ga.invokeInstanceMethod(IntegerValue.class, "mod", IntegerValue.class, Location.class);
        }
    }

    public static class IntegerIdivInteger extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "IntIdivCalculator-0");
            ga.checkClass(IntegerValue.class);
            ga.swap();
            ga.checkClass(IntegerValue.class);
            ga.swap();
            ExpressionCompiler.allocateStatic(compiler, locator);
            ga.invokeInstanceMethod(IntegerValue.class, "idiv", IntegerValue.class, Location.class);
        }
    }

    /**
     * Generic class for double arithmetic
     *
     * @param ga       the generator adapter
     * @param operator the operator, as a constant in the GeneratorAdapter class
     */

    private static void doubleArithmetic(Generator ga, int operator) {
        // assert stack DoubleValue0 DoubleValue1
        ga.checkClass(NumericValue.class);
        ga.invokeInstanceMethod(NumericValue.class, "getDoubleValue");
        // assert stack DoubleValue double1A double1B (a double occupies two slots on the stack)
        ga.dup2X1();
        // assert stack double1A double1B DoubleValue double1A double1B
        ga.pop2();
        // assert stack double1A double1B DoubleValue
        ga.checkClass(NumericValue.class);
        ga.invokeInstanceMethod(NumericValue.class, "getDoubleValue");
        // assert stack double1A double1B double0A double0B
        ga.dup2X2();
        // assert stack double0A double0B double1A double1B double0A double0B
        ga.pop2();
        // assert stack double0A double0B double1A double1B
        ga.math(operator, Type.DOUBLE_TYPE);
        // assert stack double2A double2B
        ga.newInstance(Type.getType(DoubleValue.class));
        // assert stack double2A double2B DoubleValue
        ga.dupX2();
        // assert stack DoubleValue double2A double2B DoubleValue
        ga.dupX2();
        // assert stack DoubleValue DoubleValue double2A double2B DoubleValue
        ga.pop();
        // assert stack DoubleValue DoubleValue double2A double2B
        ga.invokeConstructor(DoubleValue.class, Double.TYPE);
        // assert stack DoubleValue
    }

    /**
     * Arithmetic: double + double (including types that promote to double)
     */
    public static class DoublePlusDouble extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "DoublePlusDoubleCalculator-0");
            doubleArithmetic(ga, GeneratorAdapter.ADD);
        }
    }

    /**
     * Arithmetic: double - double (including types that promote to double)
     */
    public static class DoubleMinusDouble extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "DoubleMinusDoubleCalculator-0");
            doubleArithmetic(ga, GeneratorAdapter.SUB);
        }
    }

    /**
     * Arithmetic: double * double (including types that promote to double)
     */
    public static class DoubleTimesDouble extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "DoubleTimesDoubleCalculator-0");
            doubleArithmetic(ga, GeneratorAdapter.MUL);
        }
    }

    /**
     * Arithmetic: double / double (including types that promote to double)
     */
    public static class DoubleDivDouble extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "DoubleDivDoubleCalculator-0");
            doubleArithmetic(ga, GeneratorAdapter.DIV);
        }
    }

    /**
     * Arithmetic: double % double (including types that promote to double)
     */
    public static class DoubleModDouble extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "DoubleModDoubleCalculator-0");
            doubleArithmetic(ga, GeneratorAdapter.REM);
        }
    }

    /**
     * Generic code for arithmetic involving two float operands
     *
     * @param ga The code generator
     * @param operator the arithmetic operator in ASM terms, for example {@link GeneratorAdapter#DIV}
     */

    private static void floatArithmetic(Generator ga, int operator) {
        // see doubleArithmetic for the logic
        ga.checkClass(NumericValue.class);
        ga.invokeInstanceMethod(NumericValue.class, "getFloatValue");
        ga.swap();
        ga.checkClass(NumericValue.class);
        ga.invokeInstanceMethod(NumericValue.class, "getFloatValue");
        ga.swap();
        ga.math(operator, Type.FLOAT_TYPE);
        ga.newInstance(Type.getType(FloatValue.class));
        ga.dupX1();
        ga.swap();
        ga.invokeConstructor(FloatValue.class, Float.TYPE);
    }

    /**
     * Arithmetic: float + float (including types that promote to float))
     */
    public static class FloatPlusFloat extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            floatArithmetic(ga, GeneratorAdapter.ADD);
        }
    }

    /**
     * Arithmetic: float - float (including types that promote to float)
     */
    public static class FloatMinusFloat extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            floatArithmetic(ga, GeneratorAdapter.SUB);
        }
    }

    /**
     * Arithmetic: float * float (including types that promote to float)
     */
    public static class FloatTimesFloat extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            floatArithmetic(ga, GeneratorAdapter.MUL);
        }
    }

    /**
     * Arithmetic: float div float (including types that promote to float)
     */
    public static class FloatDivFloat extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            floatArithmetic(ga, GeneratorAdapter.DIV);
        }
    }

    /**
     * Arithmetic: float mod float (including types that promote to float)
     */
    public static class FloatModFloat extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            floatArithmetic(ga, GeneratorAdapter.REM);
        }
    }


    /**
     * Arithmetic: float idiv float (including types that promote to float)
     */
    public static class FloatIdivFloat extends CalculatorCompiler {

        @Override
        public void compute(Generator ga, SourceLocator locator) {

            GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
            ExpressionCompiler.visitAnnotation(compiler, "FloatIdivFloatCalculator");
            LabelInfo notZero = methodInfo.newLabel("notZero");
            LabelInfo notNan = methodInfo.newLabel("notNan");
            LabelInfo end = methodInfo.newLabel("end");
            LabelInfo bOK = methodInfo.newLabel("bOK");
            LabelInfo aOK = methodInfo.newLabel("aOK");
            int oper0Var = methodInfo.allocateLocal(NumericValue.class);
            int oper1Var = methodInfo.allocateLocal(NumericValue.class);
            //Stack: oper0 oper1
            ga.checkClass(NumericValue.class);
            ga.storeLocal(oper1Var);
            ga.checkClass(NumericValue.class);
            ga.storeLocal(oper0Var);

            ga.loadLocal(oper0Var);
            ga.invokeInstanceMethod(NumericValue.class, "getFloatValue");
            int floatA = methodInfo.allocateLocal(Float.TYPE);
            ga.storeLocal(floatA);

            ga.loadLocal(oper1Var);
            ga.invokeInstanceMethod(NumericValue.class, "getFloatValue");
            int floatB = methodInfo.allocateLocal(Float.TYPE);
            ga.storeLocal(floatB);

            ga.loadLocal(floatB);
            ga.push((float) 0.0);
            ga.ifCmp(Type.getType(Float.TYPE), Generator.NE, notZero.label());
            compiler.generateDynamicError("Integer division by zero", "FOAR0001", locator, false);
            ga.goTo(end.label());
            methodInfo.placeLabel(notZero);


            ga.loadLocal(floatA);
            ga.invokeStaticMethod(Float.class, "isNaN", Float.TYPE);
            ga.ifFalse(notNan);
            compiler.generateDynamicError("First operand of idiv is NaN or infinity", "FOAR0002", locator, false);
            ga.goTo(end.label());
            methodInfo.placeLabel(notNan);
            ga.loadLocal(floatA);
            ga.invokeStaticMethod(Float.class, "isInfinite", Float.TYPE);
            ga.ifFalse(aOK);
            compiler.generateDynamicError("First operand of idiv is NaN or infinity", "FOAR0002", locator, false);
            ga.goTo(end.label());
            methodInfo.placeLabel(aOK);

            ga.loadLocal(floatB);
            ga.invokeStaticMethod(Float.class, "isNaN", Float.TYPE);
            ga.ifFalse(bOK);
            compiler.generateDynamicError("Second operand of idiv is NaN or infinity", "FOAR0002", locator, false);
            ga.goTo(end.label());
            methodInfo.placeLabel(bOK);

            ga.newInstance(FloatValue.class);
            ga.dup();
            ga.loadLocal(floatA);
            ga.loadLocal(floatB);
            ga.math(GeneratorAdapter.DIV, Type.FLOAT_TYPE);
            ga.invokeConstructor(FloatValue.class, Float.TYPE);
            ExpressionCompiler.allocateStatic(compiler, Converter.FloatToInteger.INSTANCE);
            ga.swap();
            ga.invokeInstanceMethod(FloatToInteger.class, "convert", AtomicValue.class);
            ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
            methodInfo.placeLabel(end);
            methodInfo.releaseLocal(floatA);
            methodInfo.releaseLocal(floatB);
            methodInfo.releaseLocal(oper0Var);
            methodInfo.releaseLocal(oper1Var);
        }
    }

    /**
     * Generic method for decimal arithmetic
     */

    private static void decimalArithmetic(CompilerService compiler, Generator ga, String integerMethod, String decimalMethod) {
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo notInteger = methodInfo.newLabel("notInteger");
        LabelInfo end = methodInfo.newLabel("endCalc");

        int oper0Var = methodInfo.allocateLocal(NumericValue.class);
        int oper1Var = methodInfo.allocateLocal(NumericValue.class);
        ga.checkClass(NumericValue.class);
        ga.storeLocal(oper1Var);
        ga.checkClass(NumericValue.class);
        ga.storeLocal(oper0Var);

        ga.loadLocal(oper0Var);
        ga.ifNotInstance(IntegerValue.class, notInteger);

        ga.loadLocal(oper1Var);
        ga.ifNotInstance(IntegerValue.class, notInteger);

        ga.loadLocal(oper0Var);
        ga.checkClass(IntegerValue.class);
        ga.loadLocal(oper1Var);
        ga.checkClass(IntegerValue.class);
        ga.invokeInstanceMethod(IntegerValue.class, integerMethod, IntegerValue.class);
        ga.goTo(end);

        methodInfo.placeLabel(notInteger);
        ga.newInstance(Type.getType(BigDecimalValue.class));
        ga.dup();
        ga.loadLocal(oper0Var);
        ga.checkClass(NumericValue.class);
        ga.invokeInstanceMethod(NumericValue.class, "getDecimalValue");
        ga.loadLocal(oper1Var);
        ga.checkClass(NumericValue.class);
        ga.invokeInstanceMethod(NumericValue.class, "getDecimalValue");
        ga.invokeInstanceMethod(BigDecimal.class, decimalMethod, BigDecimal.class);
        ga.invokeConstructor(BigDecimalValue.class, BigDecimal.class);

        methodInfo.placeLabel(end);
    }

    /**
     * Arithmetic: decimal + decimal (including types that promote to decimal, that is, integer)
     */

    public static class DecimalPlusDecimal extends CalculatorCompiler {

        @Override
        public void compute(Generator ga, SourceLocator locator) {
            decimalArithmetic(compiler, ga, "plus", "add");

        }
    }


    /**
     * Arithmetic: decimal - decimal (including types that promote to decimal, that is, integer)
     */

    public static class DecimalMinusDecimal extends CalculatorCompiler {

        @Override
        public void compute(Generator ga, SourceLocator locator) {
            decimalArithmetic(compiler, ga, "minus", "subtract");
        }
    }


    /**
     * Arithmetic: decimal * decimal (including types that promote to decimal, that is, integer)
     */

    public static class DecimalTimesDecimal extends CalculatorCompiler {

        @Override
        public void compute(Generator ga, SourceLocator locator) {
            decimalArithmetic(compiler, ga, "times", "multiply");
        }
    }


    /**
     * Arithmetic: decimal / decimal (including types that promote to decimal, that is, integer)
     */

    public static class DecimalDivDecimal extends CalculatorCompiler {

        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ga.checkClass(NumericValue.class);
            ga.swap();
            ga.checkClass(NumericValue.class);
            ga.swap();
            //TODO: check - might need to expand the decimalDivide method
            ga.invokeStaticMethod(Calculator.class, "decimalDivide", NumericValue.class, NumericValue.class);
        }

    }


    /**
     * Arithmetic: decimal % decimal (including types that promote to decimal, that is, integer)
     */

    public static class DecimalModDecimal extends CalculatorCompiler {

        @Override
        public void compute(Generator ga, SourceLocator locator) {

            GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
            int oper0Var = methodInfo.allocateLocal(NumericValue.class);
            int oper1Var = methodInfo.allocateLocal(NumericValue.class);
            ga.checkClass(NumericValue.class);
            ga.storeLocal(oper1Var);
            ga.checkClass(NumericValue.class);
            ga.storeLocal(oper0Var);

            LabelInfo notInstance = methodInfo.newLabel("notInstance");
            LabelInfo end = methodInfo.newLabel("endCalc");
            LabelInfo notZero = methodInfo.newLabel("notZero");
            ga.loadLocal(oper0Var);
            ga.instanceOf(Type.getType(IntegerValue.class));
            ga.ifZCmp(GeneratorAdapter.EQ, notInstance.label());
            ga.loadLocal(oper1Var);
            ga.instanceOf(Type.getType(IntegerValue.class));
            ga.ifZCmp(GeneratorAdapter.EQ, notInstance.label());
            ga.loadLocal(oper0Var);
            ga.checkClass(IntegerValue.class);
            ga.loadLocal(oper1Var);
            ga.checkClass(IntegerValue.class);
            ga.invokeInstanceMethod(IntegerValue.class, "mod", IntegerValue.class);
            ga.goTo(end);

            methodInfo.placeLabel(notInstance);
            LabelInfo L0 = methodInfo.newLabel("L0");
            LabelInfo L1 = methodInfo.newLabel("L1");
            LabelInfo L2 = methodInfo.newLabel("L2");
            ga.visitTryCatchBlock(L0, L1, L2, "java/lang/ArithmeticException");
            /* try block*/
            methodInfo.placeLabel(L0);
            ga.newInstance(Type.getType(BigDecimalValue.class));
            ga.dup();
            ga.loadLocal(oper0Var);
            ga.checkClass(NumericValue.class);
            ga.invokeInstanceMethod(NumericValue.class, "getDecimalValue");
            ga.loadLocal(oper1Var);
            ga.checkClass(NumericValue.class);
            ga.invokeInstanceMethod(NumericValue.class, "getDecimalValue");
            ga.invokeInstanceMethod(BigDecimal.class, "remainder", BigDecimal.class);
            ga.invokeConstructor(BigDecimalValue.class, BigDecimal.class);
            methodInfo.placeLabel(L1);
            ga.goTo(end);
            /*catch block*/
            methodInfo.placeLabel(L2);
            ga.loadLocal(oper1Var);
            ga.checkClass(NumericValue.class);
            ga.push((long) 0);
            ga.invokeInstanceMethod(NumericValue.class, "compareTo", Long.TYPE);
            ga.ifZCmp(Generator.NE, notZero.label());
            compiler.generateDynamicError("Decimal modulo zero", "FOAR0001", locator, false);
            ga.goTo(end.label());
            methodInfo.placeLabel(notZero);
            ga.throwException();

            methodInfo.placeLabel(end);
        }
    }


    /**
     * Arithmetic: decimal % decimal (including types that promote to decimal, that is, integer)
     */

    public static class DecimalIdivDecimal extends CalculatorCompiler {

        @Override
        public void compute(Generator ga, SourceLocator locator) {

            GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
            int oper0Var = methodInfo.allocateLocal(NumericValue.class);
            int oper1Var = methodInfo.allocateLocal(NumericValue.class);
            ga.checkClass(NumericValue.class);
            ga.storeLocal(oper1Var);
            ga.checkClass(NumericValue.class);
            ga.storeLocal(oper0Var);

            LabelInfo notInstance = methodInfo.newLabel("notInstance");
            LabelInfo end = methodInfo.newLabel("endCalc");
            LabelInfo notNeg = methodInfo.newLabel("notNeg");
            ga.loadLocal(oper0Var);
            ga.instanceOf(Type.getType(IntegerValue.class));
            ga.ifZCmp(GeneratorAdapter.EQ, notInstance.label());
            ga.loadLocal(oper1Var);
            ga.instanceOf(Type.getType(IntegerValue.class));
            ga.ifZCmp(GeneratorAdapter.EQ, notInstance.label());
            ga.loadLocal(oper0Var);
            ga.checkClass(IntegerValue.class);
            ga.loadLocal(oper1Var);
            ga.checkClass(IntegerValue.class);
            ga.invokeInstanceMethod(IntegerValue.class, "idiv", IntegerValue.class);
            ga.goTo(end);

            methodInfo.placeLabel(notInstance);
            ga.loadLocal(oper0Var);
            ga.checkClass(NumericValue.class);
            ga.invokeInstanceMethod(NumericValue.class, "getDecimalValue");
            ga.loadLocal(oper1Var);
            ga.checkClass(NumericValue.class);
            ga.invokeInstanceMethod(NumericValue.class, "getDecimalValue");
            ga.dup();
            ga.invokeInstanceMethod(BigDecimal.class, "signum");
            ga.ifZCmp(GeneratorAdapter.NE, notNeg.label());
            compiler.generateDynamicError("Integer division by zero", "FOAR0001", locator, true);
            ga.goTo(end);

            methodInfo.placeLabel(notNeg);
            ga.invokeInstanceMethod(BigDecimal.class, "divideToIntegralValue", BigDecimal.class);
            ga.invokeInstanceMethod(BigDecimal.class, "toBigInteger");
            ga.invokeStaticMethod(BigIntegerValue.class, "makeIntegerValue", BigInteger.class);

            methodInfo.placeLabel(end);
        }
    }


    /**
     * Generic code to compile an arithmetic expression where the types have not been resolved at compile time
     *
     * @param compiler the compiler service
     * @param ga       the generator adaptor
     * @param operator a constant from the Calculator class
     * @param opName   name of the operator for diagnostics
     * @param locator  expression location
     */

    private static void anyAnyArithmetic(CompilerService compiler, Generator ga, int operator, String opName, SourceLocator locator) {
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        // assert stack: oper0 oper1
        ga.swap();
        // assert stack: oper1 oper0
        ga.dup2();
        // assert stack: oper1 oper0 oper1 oper0
        ga.invokeInstanceMethod(AtomicValue.class, "getItemType");
        ga.invokeInstanceMethod(ItemType.class, "getPrimitiveType");
        // assert stack: oper1 oper0 oper1 type0
        ga.swap();
        // assert stack: oper1 oper0 type0 oper1
        ga.invokeInstanceMethod(AtomicValue.class, "getItemType");
        ga.invokeInstanceMethod(ItemType.class, "getPrimitiveType");
        // assert stack: oper1 oper0 type0 type1
        ga.push(operator);
        ga.push(true);
        // assert stack: oper1 oper0 type0 type1 operator true
        ga.invokeStaticMethod(Calculator.class, "getCalculator", Integer.TYPE, Integer.TYPE, Integer.TYPE, Boolean.TYPE);
        // assert stack: oper1 oper0 calc
        ga.dupX2();
        // assert stack: calc oper1 oper0 calc
        LabelInfo notNullCalc = methodInfo.newLabel("notNullCalc");
        ga.ifNonNull(notNullCalc.label());
        compiler.generateDynamicError("Unsuitable types for " + opName + " operation", "XPTY0004", locator, true);

        methodInfo.placeLabel(notNullCalc);
        // assert stack: calc oper1 oper0
        ga.swap();
        // assert stack: calc oper0 oper1
        compiler.generateGetContext();
        // assert stack: calc oper0 oper1 context
        ga.invokeInstanceMethod(Calculator.class, "compute", AtomicValue.class, AtomicValue.class, XPathContext.class);
        // assert stack: result (as atomicValue)
    }

    /**
     * Arithmetic: anyAtomicType + AnyAtomicType
     */

    public static class AnyPlusAny extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "AnyPlusCalculator-0");
            anyAnyArithmetic(compiler, ga, Calculator.PLUS, "plus (+)", locator);
        }
    }

    /**
     * Arithmetic: anyAtomicType - AnyAtomicType
     */

    public static class AnyMinusAny extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "AnyMinusCalculator-0");
            anyAnyArithmetic(compiler, ga, Calculator.MINUS, "minus (-)", locator);
        }
    }

    /**
     * Arithmetic: anyAtomicType * AnyAtomicType
     */

    public static class AnyTimesAny extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "AnyTimesCalculator-0");
            anyAnyArithmetic(compiler, ga, Calculator.TIMES, "multiply (*)", locator);
        }
    }

    /**
     * Arithmetic: anyAtomicType idiv AnyAtomicType
     */

    public static class AnyIdivAny extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "AnyIdivAnyCalculator");
            anyAnyArithmetic(compiler, ga, Calculator.IDIV, "idiv", locator);
        }
    }

    /**
     * Arithmetic: anyAtomicType div AnyAtomicType
     */

    public static class AnyDivAny extends CalculatorCompiler {
        @Override
        public void compute(Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "AnyDivAnyCalculator");
            anyAnyArithmetic(compiler, ga, Calculator.DIV, "div", locator);
        }
    }


    public static class AnyModAny extends CalculatorCompiler {
        @Override
        public void compute(/*@NotNull*/ Generator ga, SourceLocator locator) {
            ExpressionCompiler.visitAnnotation(compiler, "AnyModAnyCalculator");
            anyAnyArithmetic(compiler, ga, Calculator.MOD, "mod", locator);
        }
    }
}

// Copyright (c) 2011-2022 Saxonica Limited