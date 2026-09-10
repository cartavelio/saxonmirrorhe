package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.IntegerRangeTest;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate bytecode for an expression of the form ($I = $J to $K)
 */
public class IntegerRangeTestCompiler extends ToBooleanCompiler {

    /**
     * Generate bytecode to evaluate the expression as a boolean
     * Precondition: none.
     * Postcondition: at execution time, the stack contains an integer (0=false, 1=true) representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {

        IntegerRangeTest test = (IntegerRangeTest) expression;
        Expression valExp = test.getValue();
        Expression minExp = test.getMin();
        Expression maxExp = test.getMax();
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        boolean maybeUntypedAtomic = th.relationship(valExp.getItemType(), BuiltInAtomicType.UNTYPED_ATOMIC) != Affinity.DISJOINT;


        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        visitAnnotation(compiler, "IntegerRangeTestCompiler");
        visitLineNumber(compiler, ga, expression);
        LabelInfo done = methodInfo.newLabel("IRTdone");

        int valueVar = 0;
        valueVar = methodInfo.allocateLocal(NumericValue.class);

        compiler.compileToIterator(valExp);


        int iterVar = methodInfo.allocateLocal(SequenceIterator.class);

        ga.storeLocal(iterVar);

        LabelInfo intRangeLoop = methodInfo.newLabel("intRange_loop");
        LabelInfo falseLabel = methodInfo.newLabel("intRange_FalseLabel");

        compiler.compileToItem(minExp);
        int minVar = methodInfo.allocateLocal(IntegerValue.class);
        ga.checkClass(IntegerValue.class);
        if (Cardinality.allowsZero(minExp.getCardinality())) {
            ga.dup();
            ga.ifNull(falseLabel.label());
        }
        ga.storeLocal(minVar);

        compiler.compileToItem(maxExp);
        int maxVar = methodInfo.allocateLocal(IntegerValue.class);
        ga.checkClass(IntegerValue.class);
        if (Cardinality.allowsZero(maxExp.getCardinality())) {
            ga.dup();
            ga.ifNull(falseLabel.label());
        }
        ga.storeLocal(maxVar);

        Configuration config = compiler.getConfiguration();

        StringConverter toDouble = BuiltInAtomicType.DOUBLE.getStringConverter(config.getConversionRules());

        int toDoubleVar = methodInfo.allocateLocal(StringConverter.class);
        allocateStatic(compiler, toDouble);
        ga.storeLocal(toDoubleVar);
        //loop

        methodInfo.placeLabel(intRangeLoop);
        ga.loadLocal(iterVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.dup();
        ga.ifNull(falseLabel.label());
        ga.checkClass(AtomicValue.class);
        int atomicValueVar = methodInfo.allocateLocal(AtomicValue.class);
        ga.storeLocal(atomicValueVar);


        LabelInfo not_untypedAtomicValueLb = methodInfo.newLabel("untypedAtomicType_label");
        LabelInfo numericInstanceLb = methodInfo.newLabel("numericInstance_label");
        LabelInfo checkWholeNumber_lb = methodInfo.newLabel("checkWholeNumber_lb");

        if (maybeUntypedAtomic) {
            ga.loadLocal(atomicValueVar);
            ga.invokeInstanceMethod(AtomicValue.class, "isUntypedAtomic");
            ga.ifFalse(not_untypedAtomicValueLb);
            untypedAtomicChecker(test, ga, methodInfo, valueVar, toDoubleVar, atomicValueVar, checkWholeNumber_lb);
            methodInfo.placeLabel(not_untypedAtomicValueLb);
        }
        ga.loadLocal(atomicValueVar);
        ga.ifInstance(NumericValue.class, numericInstanceLb);

        //throw error
        ga.push("Cannot compare value of type ");
        ga.loadLocal(atomicValueVar);
        ga.invokeInstanceMethod(AtomicValue.class, "getUType");
        ga.invokeInstanceMethod(UType.class, "toString");
        ga.push(" to xs:integer");
        ga.concatenateStrings(3);
        ga.push("XPTY0004");
        ga.push(test.getLocation().getSystemId());
        ga.push(test.getLocation().getLineNumber());
        ga.push(true);
        ga.invokeStaticMethod(Callback.class, "makeXPathException", String.class, String.class, String.class, Integer.TYPE, Boolean.TYPE);
        ga.throwException();


        methodInfo.placeLabel(numericInstanceLb);
        ga.loadLocal(atomicValueVar);
        ga.checkClass(NumericValue.class);
        ga.storeLocal(valueVar);


        methodInfo.placeLabel(checkWholeNumber_lb);
        if (!Type.isSubType((AtomicType)valExp.getItemType(), BuiltInAtomicType.INTEGER)) {
            ga.loadLocal(valueVar);
            ga.invokeInstanceMethod(NumericValue.class, "isWholeNumber");
            ga.ifFalse(intRangeLoop);
        }

        ga.loadLocal(valueVar);
        ga.loadLocal(minVar);
        ga.invokeInstanceMethod(NumericValue.class, "compareTo", Object.class);
        LabelInfo geMin = methodInfo.newLabel("IRTgeMin");
        ga.ifZCmp(GeneratorAdapter.GE, geMin.label());
        ga.goTo(intRangeLoop);
        methodInfo.placeLabel(geMin);

        ga.loadLocal(valueVar);
        compiler.compileToItem(maxExp);
        ga.invokeInstanceMethod(NumericValue.class, "compareTo", Object.class);
        LabelInfo leMax = methodInfo.newLabel("IRTleMax");
        ga.ifZCmp(GeneratorAdapter.LE, leMax.label());
        ga.goTo(intRangeLoop);
        methodInfo.placeLabel(leMax);

        ga.push(true);
        ga.goTo(done);

        // methodInfo.releaseLocal(valueVar);


        ga.goTo(intRangeLoop);

        methodInfo.placeLabel(falseLabel);
        ga.pop();
        ga.push(false);


        methodInfo.placeLabel(done);

        methodInfo.releaseLocal(valueVar);
        methodInfo.releaseLocal(toDoubleVar);
        methodInfo.releaseLocal(minVar);
        methodInfo.releaseLocal(maxVar);
        methodInfo.releaseLocal(toDoubleVar);
        methodInfo.releaseLocal(atomicValueVar);


    }

    private void untypedAtomicChecker(IntegerRangeTest test, Generator ga, GeneratedMethodInfo methodInfo, int valueVar, int toDoubleVar, int atomicValueVar, LabelInfo checkWholeNumber_lb) {
        ga.loadLocal(toDoubleVar);
        ga.loadLocal(atomicValueVar);
        ga.invokeInstanceMethod(AtomicValue.class, "getUnicodeStringValue");
        ga.invokeInstanceMethod(StringConverter.class, "convertString", UnicodeString.class);
        ga.dup();
        LabelInfo notValidationFailure_lb = methodInfo.newLabel("notValidationFailure_label");
        ga.ifNotInstance(ValidationFailure.class, notValidationFailure_lb);
        ga.pop();
        ga.push("Failed to convert untypedAtomic value {");
        ga.loadLocal(atomicValueVar);
        ga.invokeInstanceMethod(AtomicValue.class, "getStringValue");
        ga.checkClass(String.class);
        ga.push("}  to xs:integer");

        ga.concatenateStrings(3);

        ga.push("FORG0001");

        ga.push(test.getLocation().getSystemId());
        ga.push(test.getLocation().getLineNumber());

        ga.push(true);
        ga.invokeStaticMethod(Callback.class, "makeXPathException", String.class, String.class, String.class, Integer.TYPE, Boolean.TYPE);
        ga.throwException();

        methodInfo.placeLabel(notValidationFailure_lb);
        ga.checkClass(DoubleValue.class);
        ga.invokeInstanceMethod(AtomicValue.class, "asAtomic");
        ga.checkClass(DoubleValue.class);
        ga.storeLocal(valueVar);
        ga.goTo(checkWholeNumber_lb);
    }
}

//
// Copyright (c) 2001-2020 Saxonica Limited