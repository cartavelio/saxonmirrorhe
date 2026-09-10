package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;
import org.objectweb.asm.Type;

/**
 * Compile the fn:number() function to Java bytecode
 */
public class NumberFnCompiler extends ToItemCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        if (requiredClass != Double.TYPE) {
            throw new IllegalArgumentException();
        }
        visitAnnotation(compiler, "NumberFn-Dbl");
        boolean compiled = compileFromPrimitive(compiler, ((SystemFunctionCall) expression).getArg(0));
        if (!compiled) {
            super.compileToPrimitive(compiler, expression, requiredClass, onEmpty);
        }

    }

    private boolean compileFromPrimitive(CompilerService compiler, Expression arg) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        int type = arg.getItemType().getPrimitiveType();
        boolean allowEmpty = Cardinality.allowsZero(arg.getCardinality());
        visitAnnotation(compiler, "NumberFn");
        LabelInfo exit = methodInfo.newLabel("exit");
        LabelInfo returnNaN = methodInfo.newLabel("returnNaN");
        OnEmpty.UnwindAndJump onEmptyReturnNaN = new OnEmpty.UnwindAndJump(returnNaN);
        boolean compiled = true;

        if (type == StandardNames.XS_BOOLEAN && !allowEmpty) {
            compiler.compileToBoolean(arg);
            ga.cast(Type.INT_TYPE, Type.DOUBLE_TYPE);

        } else if (type == StandardNames.XS_DOUBLE) {
            compiler.compileToPrimitive(arg, Double.TYPE, onEmptyReturnNaN);

        } else if (type == StandardNames.XS_FLOAT) {
            compiler.compileToPrimitive(arg, Float.TYPE, onEmptyReturnNaN);
            ga.cast(Type.FLOAT_TYPE, Type.DOUBLE_TYPE);

        } else if (type == StandardNames.XS_INTEGER && compiler.isInRangeForLong(arg)) {
            compiler.compileToPrimitive(arg, Long.TYPE, onEmptyReturnNaN);
            ga.cast(Type.LONG_TYPE, Type.DOUBLE_TYPE);

        } else if (type == StandardNames.XS_STRING || type == StandardNames.XS_UNTYPED_ATOMIC) {
            compiler.compileToPrimitive(arg, UnicodeString.class, onEmptyReturnNaN);
            StringToDouble s2d = compiler.getConfiguration().getConversionRules().getStringToDoubleConverter();
            allocateStatic(compiler, s2d);
            ga.swap();
            ga.invokeStaticMethod(Callback.class, "stringToNumberOrNaN", StringToDouble.class, UnicodeString.class);
        } else {
            compiled = false;
        }

        if (compiled) {
            ga.goTo(exit);
            methodInfo.placeLabel(returnNaN);
            ga.push(Double.NaN);
            methodInfo.placeLabel(exit);
        }
        return compiled;
    }

    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        TypeHierarchy th = compiler.getConfiguration().getTypeHierarchy();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "NumberFn-Item");

        Expression arg = ((SystemFunctionCall) expression).getArg(0);
        boolean compiled = compileFromPrimitive(compiler, arg);
        if (compiled) {
            ga.invokeStaticMethod(DoubleValue.class, "makeDoubleValue", Double.TYPE);
            return;
        }


        LabelInfo numberFnReturn = methodInfo.newLabel("numberFnReturn");
        compiler.compileToItem(arg);
        int valueVar = methodInfo.allocateLocal(Item.class);
        ga.storeLocal(valueVar);
        if (Cardinality.allowsZero(arg.getCardinality())) {
            LabelInfo nonNull = methodInfo.newLabel("nonNull");
            ga.loadLocal(valueVar);
            ga.ifNonNull(nonNull.label());
            ga.getStaticField(DoubleValue.class, "NaN", DoubleValue.class);
            ga.goTo(numberFnReturn);
            methodInfo.placeLabel(nonNull);
        }
        ItemType type = arg.getItemType();
        if (th.relationship(type, BuiltInAtomicType.BOOLEAN) != Affinity.DISJOINT) {
            LabelInfo nonBoolean = methodInfo.newLabel("nonBoolean");
            ga.loadLocal(valueVar);
            ga.ifNotInstance(BooleanValue.class, nonBoolean);
            ga.newInstance(DoubleValue.class);
            ga.dup();
            ga.loadLocal(valueVar);
            ga.checkClass(BooleanValue.class);
            ga.invokeInstanceMethod(BooleanValue.class, "getBooleanValue");
            ga.cast(Type.INT_TYPE, Type.DOUBLE_TYPE);
            ga.invokeConstructor(DoubleValue.class, Double.TYPE);
            ga.goTo(numberFnReturn);
            methodInfo.placeLabel(nonBoolean);
        }
        if (th.relationship(type, BuiltInAtomicType.DOUBLE) != Affinity.DISJOINT) {
            LabelInfo nonDouble = methodInfo.newLabel("nonDouble");
            ga.loadLocal(valueVar);
            ga.ifNotInstance(DoubleValue.class, nonDouble);
            ga.loadLocal(valueVar);
            ga.goTo(numberFnReturn);
            methodInfo.placeLabel(nonDouble);
        }
        if (th.relationship(type, NumericType.getInstance()) != Affinity.DISJOINT) {
            LabelInfo nonNumeric = methodInfo.newLabel("nonNumeric");
            ga.loadLocal(valueVar);
            ga.ifNotInstance(NumericValue.class, nonNumeric);
            ga.newInstance(DoubleValue.class);
            ga.dup();
            ga.loadLocal(valueVar);
            ga.checkClass(NumericValue.class);
            ga.invokeInstanceMethod(NumericValue.class, "getDoubleValue");
            ga.invokeConstructor(DoubleValue.class, Double.TYPE);
            ga.goTo(numberFnReturn);
            methodInfo.placeLabel(nonNumeric);
        }

        if (th.relationship(type, BuiltInAtomicType.STRING) != Affinity.DISJOINT ||
                th.relationship(type, BuiltInAtomicType.UNTYPED_ATOMIC) != Affinity.DISJOINT) {
            LabelInfo nonString = methodInfo.newLabel("nonString");
            ga.loadLocal(valueVar);
            ga.ifNotInstance(StringValue.class, nonString);
            ga.loadLocal(valueVar);
            ga.ifInstance(AnyURIValue.class, nonString);
//            StringConverter sc = StringConverter.getStringConverter(BuiltInAtomicType.DOUBLE, rules);
//            ConversionResult cr = sc.convert((AtomicValue)arg0);
//            if (cr instanceof ValidationFailure) {
//                return DoubleValue.NaN;
//            } else {
//                return (AtomicValue)cr;
//            }
            allocateStatic(compiler, BuiltInAtomicType.DOUBLE.getStringConverter(compiler.getConfiguration().getConversionRules()));
            ga.loadLocal(valueVar);
            ga.checkClass(AtomicValue.class);
            ga.invokeInstanceMethod(Converter.class, "convert", AtomicValue.class);
            ga.dup();
            LabelInfo notFailure = methodInfo.newLabel("notFailure");
            ga.ifNotInstance(ValidationFailure.class, notFailure);
            ga.pop();
            ga.getStaticField(DoubleValue.class, "NaN", DoubleValue.class);
            ga.goTo(numberFnReturn);
            methodInfo.placeLabel(notFailure);
            ga.checkClass(DoubleValue.class);
            ga.goTo(numberFnReturn);
            methodInfo.placeLabel(nonString);
        }
        ga.getStaticField(DoubleValue.class, "NaN", DoubleValue.class);
        methodInfo.placeLabel(numberFnReturn);

        methodInfo.releaseLocal(valueVar);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited


