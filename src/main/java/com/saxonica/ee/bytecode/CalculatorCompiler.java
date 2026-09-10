package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Calculator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
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
    Class<? extends Item> class0;
    Class<? extends Item> class1;

    public void setCompilerService(CompilerService compiler) {
        this.compiler = compiler;
    }

    public void setExpressions(Expression oper0, Class<? extends Item> class0,
                               Expression oper1, Class<? extends Item> class1) {
        this.oper0 = oper0;
        this.oper1 = oper1;
        this.class0 = class0;
        this.class1 = class1;
    }

    protected void forceIntegerOperands(Generator ga) {
        if (!IntegerValue.class.isAssignableFrom(class0)) {
            class0 = IntegerValue.class;
            ga.checkClass(IntegerValue.class);
        }
        if (!IntegerValue.class.isAssignableFrom(class0)) {
            class0 = IntegerValue.class;
            ga.checkClass(IntegerValue.class);
        }
    }

    /**
     * Generate bytecode to compute the result of the arithmetic expression
     * @param ga the code generator
     * @param locator the location of the arithmetic expression
     * @return the class of Item returned by the computation
     */

    public abstract Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException;


    public static class IntegerPlusInteger extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "IntPlusCalculator-0");
            class1 = ga.downCast(class1, IntegerValue.class, false);
            ga.swap();
            class0 = ga.downCast(class0, IntegerValue.class, false);
            ga.invokeInstanceMethod(class1, "plus", IntegerValue.class);
            return IntegerValue.class;
        }

    }

    public static class IntegerMinusInteger extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "IntMinusCalculator-0");
            class1 = ga.downCast(class1, IntegerValue.class, false);
            ga.swap();
            class0 = ga.downCast(class0, IntegerValue.class, false);
            ga.swap();
            ga.invokeInstanceMethod(class0, "minus", IntegerValue.class);
            return IntegerValue.class;
        }
    }

    public static class IntegerTimesInteger extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "IntTimesCalculator-0");
            class1 = ga.downCast(class1, IntegerValue.class, false);
            ga.swap();
            class0 = ga.downCast(class0, IntegerValue.class, false);
            ga.invokeInstanceMethod(class1, "times", IntegerValue.class);
            return IntegerValue.class;
        }
    }

    public static class IntegerDivInteger extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "IntDivCalculator-0");
            class1 = ga.downCast(class1, IntegerValue.class, false);
            ga.swap();
            class0 = ga.downCast(class0, IntegerValue.class, false);
            ga.swap();
            ExpressionCompiler.allocateStatic(compiler, locator);
            ga.invokeInstanceMethod(class0, "div", IntegerValue.class, Location.class);
            return NumericValue.class;
        }
    }

    public static class IntegerModInteger extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "IntModCalculator-0");
            class1 = ga.downCast(class1, IntegerValue.class, false);
            ga.swap();
            class0 = ga.downCast(class0, IntegerValue.class, false);
            ga.swap();
            ExpressionCompiler.allocateStatic(compiler, locator);
            ga.invokeInstanceMethod(class0, "mod", IntegerValue.class, Location.class);
            return IntegerValue.class;
        }
    }

    public static class IntegerIdivInteger extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "IntIdivCalculator-0");
            class1 = ga.downCast(class1, IntegerValue.class, false);
            ga.swap();
            class0 = ga.downCast(class0, IntegerValue.class, false);
            ga.swap();
            ExpressionCompiler.allocateStatic(compiler, locator);
            ga.invokeInstanceMethod(class0, "idiv", IntegerValue.class, Location.class);
            return IntegerValue.class;
        }
    }

    /**
     * Generic class for double arithmetic
     *
     * @param ga       the generator adapter
     * @param operator the operator, as a constant in the GeneratorAdapter class
     */

    protected void doubleArithmetic(Generator ga, int operator) throws CannotCompileException {
        // assert stack DoubleValue0 DoubleValue1
        class1 = ga.downCast(class1, NumericValue.class, false);
        ga.invokeInstanceMethod(class1, "getDoubleValue");
        // assert stack DoubleValue0 double1A double1B (a double occupies two slots on the stack)
        ga.dup2X1();
        // assert stack double1A double1B DoubleValue0 double1A double1B
        ga.pop2();
        // assert stack double1A double1B DoubleValue0
        class0 = ga.downCast(class0, NumericValue.class, false);
        ga.invokeInstanceMethod(class0, "getDoubleValue");
        // assert stack double1A double1B double0A double0B
        if (!isCommutative(operator)) {
            ga.dup2X2();
            // assert stack double0A double0B double1A double1B double0A double0B
            ga.pop2();
            // assert stack double0A double0B double1A double1B
        }
        ga.math(operator, Type.DOUBLE_TYPE);
        // assert stack double2A double2B
        ga.invokeStaticMethod(DoubleValue.class, "makeDoubleValue", Double.TYPE);
        // assert stack DoubleValue
    }

    /**
     * Arithmetic: double + double (including types that promote to double)
     */
    public static class DoublePlusDouble extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "DoublePlusDoubleCalculator-0");
            doubleArithmetic(ga, GeneratorAdapter.ADD);
            return DoubleValue.class;
        }
    }

    /**
     * Arithmetic: double - double (including types that promote to double)
     */
    public static class DoubleMinusDouble extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "DoubleMinusDoubleCalculator-0");
            doubleArithmetic(ga, GeneratorAdapter.SUB);
            return DoubleValue.class;
        }
    }

    /**
     * Arithmetic: double * double (including types that promote to double)
     */
    public static class DoubleTimesDouble extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "DoubleTimesDoubleCalculator-0");
            doubleArithmetic(ga, GeneratorAdapter.MUL);
            return DoubleValue.class;
        }
    }

    /**
     * Arithmetic: double / double (including types that promote to double)
     */
    public static class DoubleDivDouble extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "DoubleDivDoubleCalculator-0");
            doubleArithmetic(ga, GeneratorAdapter.DIV);
            return DoubleValue.class;
        }
    }

    /**
     * Arithmetic: double % double (including types that promote to double)
     */
    public static class DoubleModDouble extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "DoubleModDoubleCalculator-0");
            doubleArithmetic(ga, GeneratorAdapter.REM);
            return DoubleValue.class;
        }
    }

    /**
     * Generic code for arithmetic involving two float operands
     *
     * @param ga The code generator
     * @param operator the arithmetic operator in ASM terms, for example {@link GeneratorAdapter#DIV}
     */

    protected void floatArithmetic(Generator ga, int operator) throws CannotCompileException {
        // see doubleArithmetic for the logic
        class1 = ga.downCast(class1, NumericValue.class, false);
        ga.invokeInstanceMethod(class1, "getFloatValue");
        ga.swap();
        class0 = ga.downCast(class0, NumericValue.class, false);
        ga.invokeInstanceMethod(class0, "getFloatValue");
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
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            floatArithmetic(ga, GeneratorAdapter.ADD);
            return FloatValue.class;
        }
    }

    /**
     * Arithmetic: float - float (including types that promote to float)
     */
    public static class FloatMinusFloat extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            floatArithmetic(ga, GeneratorAdapter.SUB);
            return FloatValue.class;
        }
    }

    /**
     * Arithmetic: float * float (including types that promote to float)
     */
    public static class FloatTimesFloat extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            floatArithmetic(ga, GeneratorAdapter.MUL);
            return FloatValue.class;
        }
    }

    /**
     * Arithmetic: float div float (including types that promote to float)
     */
    public static class FloatDivFloat extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            floatArithmetic(ga, GeneratorAdapter.DIV);
            return FloatValue.class;
        }
    }

    /**
     * Arithmetic: float mod float (including types that promote to float)
     */
    public static class FloatModFloat extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            floatArithmetic(ga, GeneratorAdapter.REM);
            return FloatValue.class;
        }
    }


    /**
     * Arithmetic: float idiv float (including types that promote to float)
     */
    public static class FloatIdivFloat extends CalculatorCompiler {

        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {

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
            return IntegerValue.class;
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
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            decimalArithmetic(compiler, ga, "plus", "add");
            return DecimalValue.class;

        }
    }


    /**
     * Arithmetic: decimal - decimal (including types that promote to decimal, that is, integer)
     */

    public static class DecimalMinusDecimal extends CalculatorCompiler {

        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            decimalArithmetic(compiler, ga, "minus", "subtract");
            return DecimalValue.class;
        }
    }


    /**
     * Arithmetic: decimal * decimal (including types that promote to decimal, that is, integer)
     */

    public static class DecimalTimesDecimal extends CalculatorCompiler {

        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            decimalArithmetic(compiler, ga, "times", "multiply");
            return DecimalValue.class;
        }
    }


    /**
     * Arithmetic: decimal / decimal (including types that promote to decimal, that is, integer)
     */

    public static class DecimalDivDecimal extends CalculatorCompiler {

        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ga.checkClass(NumericValue.class);
            ga.swap();
            ga.checkClass(NumericValue.class);
            ga.swap();
            //TODO: check - might need to expand the decimalDivide method
            ga.invokeStaticMethod(Calculator.class, "decimalDivide", NumericValue.class, NumericValue.class);
            return DecimalValue.class;
        }

    }


    /**
     * Arithmetic: decimal % decimal (including types that promote to decimal, that is, integer)
     */

    public static class DecimalModDecimal extends CalculatorCompiler {

        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {

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
            return DecimalValue.class;
        }
    }


    /**
     * Arithmetic: decimal % decimal (including types that promote to decimal, that is, integer)
     */

    public static class DecimalIdivDecimal extends CalculatorCompiler {

        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {

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
            return IntegerValue.class;
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
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "AnyPlusCalculator-0");
            anyAnyArithmetic(compiler, ga, Calculator.PLUS, "plus (+)", locator);
            return AtomicValue.class;
        }
    }

    /**
     * Arithmetic: anyAtomicType - AnyAtomicType
     */

    public static class AnyMinusAny extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "AnyMinusCalculator-0");
            anyAnyArithmetic(compiler, ga, Calculator.MINUS, "minus (-)", locator);
            return AtomicValue.class;
        }
    }

    /**
     * Arithmetic: anyAtomicType * AnyAtomicType
     */

    public static class AnyTimesAny extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "AnyTimesCalculator-0");
            anyAnyArithmetic(compiler, ga, Calculator.TIMES, "multiply (*)", locator);
            return AtomicValue.class;
        }
    }

    /**
     * Arithmetic: anyAtomicType idiv AnyAtomicType
     */

    public static class AnyIdivAny extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "AnyIdivAnyCalculator");
            anyAnyArithmetic(compiler, ga, Calculator.IDIV, "idiv", locator);
            return AtomicValue.class;
        }
    }

    /**
     * Arithmetic: anyAtomicType div AnyAtomicType
     */

    public static class AnyDivAny extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "AnyDivAnyCalculator");
            anyAnyArithmetic(compiler, ga, Calculator.DIV, "div", locator);
            return AtomicValue.class;
        }
    }


    public static class AnyModAny extends CalculatorCompiler {
        @Override
        public Class<? extends AtomicValue> compute(/*@NotNull*/ Generator ga, SourceLocator locator) throws CannotCompileException {
            ExpressionCompiler.visitAnnotation(compiler, "AnyModAnyCalculator");
            anyAnyArithmetic(compiler, ga, Calculator.MOD, "mod", locator);
            return AtomicValue.class;
        }
    }

    protected static boolean isCommutative(int operator) {
        return operator == GeneratorAdapter.ADD || operator == GeneratorAdapter.MUL;
    }
}

// Copyright (c) 2011-2022 Saxonica Limited