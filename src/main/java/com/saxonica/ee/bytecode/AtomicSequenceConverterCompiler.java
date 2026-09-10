package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.AtomicSequenceConverter;
import net.sf.saxon.expr.AtomicSequenceConverter.ToStringMappingFunction;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.ItemMappingFunction;
import net.sf.saxon.expr.ItemMappingIterator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.ConversionResult;
import net.sf.saxon.type.Converter;
import net.sf.saxon.value.*;
import org.objectweb.asm.Type;

/**
 * Generate bytecode for an AtomicSequenceConverter or UntypedSequenceConverter expression, performing run-time type checking
 */
public class AtomicSequenceConverterCompiler extends ToIteratorCompiler {


    /*
      *
      * Item item = operand.evaluateItem(context);
         if (item==null) {
             return null;
         }
         return converter.convert((AtomicValue) item).asAtomic();
      */
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler,
                                               Expression expression) throws CannotCompileException {
        Expression operand = ((AtomicSequenceConverter) expression).getBaseExpression();
        Converter converter = ((AtomicSequenceConverter) expression).getConverter();
        if (converter == null) {
            converter = ((AtomicSequenceConverter) expression).allocateConverter(compiler.getConfiguration(), false);
        }
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "AtomicSequenceConverterCompiler-Item");
        visitLineNumber(compiler, ga, expression);

        LabelInfo end = methodInfo.newLabel("endASC");

        Class<? extends Item> operandClass = compiler.compileToItem(operand);
        visitAnnotation(compiler, "AtomicSequenceConverterCompiler-Item-Returned");
        if (Cardinality.allowsZero(operand.getCardinality())) {
            LabelInfo notNull = methodInfo.newLabel("notNullASC");
            ga.dup();
            ga.ifNonNull(notNull.label());
            ga.pop();
            ga.pushNull();
            ga.goTo(end);
            methodInfo.placeLabel(notNull);
        }
        if (converter instanceof Converter.ToStringConverter) {
            ga.checkClass(AtomicValue.class);
            ga.invokeInstanceMethod(AtomicValue.class, "getUnicodeStringValue");
            ga.invokeStaticMethod(StringValue.class, "makeUStringValue", UnicodeString.class);
            methodInfo.placeLabel(end);
            return StringValue.class;
        } else {
            allocateStatic(compiler, converter);
            ga.swap();
            ga.checkClass(AtomicValue.class);
            ga.invokeInstanceMethod(Converter.class, "convert", AtomicValue.class);
            ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
            methodInfo.placeLabel(end);
            return AtomicValue.class;
        }

    }

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        if (requiredClass == String.class || requiredClass == UnicodeString.class) {
            Expression operand = ((AtomicSequenceConverter) expression).getBaseExpression();
            Generator ga = compiler.getCurrentGenerator();
            GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
            visitAnnotation(compiler, "AtomicSequenceConverterCompiler-Item");
            visitLineNumber(compiler, ga, expression);

            int operandType = operand.getItemType().getPrimitiveType();
            OnEmpty oe2 = onEmpty;
            boolean needToHandleEmpty =
                    Cardinality.allowsZero(operand.getCardinality()) && !(oe2 instanceof OnEmpty.UnwindAndJump);
            LabelInfo handleEmpty = (needToHandleEmpty ? methodInfo.newLabel("handleEmpty") : null);
            if (operandType == StandardNames.XS_INTEGER && compiler.isInRangeForInt(operand)) {
                oe2 = prepareForEmptiness(oe2, needToHandleEmpty, handleEmpty);
                compiler.compileToPrimitive(operand, Integer.TYPE, oe2);
                ga.invokeStaticMethod(Integer.class, "toString", Integer.TYPE);
                postpareForEmptiness(onEmpty, ga, methodInfo, needToHandleEmpty, handleEmpty);
                if (requiredClass == UnicodeString.class) {
                    ga.invokeStaticMethod(BMPString.class, "of", String.class);
                }
            } else if (operandType == StandardNames.XS_INTEGER && compiler.isInRangeForLong(operand)) {
                oe2 = prepareForEmptiness(oe2, needToHandleEmpty, handleEmpty);
                compiler.compileToPrimitive(operand, Long.TYPE, oe2);
                ga.invokeStaticMethod(Long.class, "toString", Long.TYPE);
                postpareForEmptiness(onEmpty, ga, methodInfo, needToHandleEmpty, handleEmpty);
                if (requiredClass == UnicodeString.class) {
                    ga.invokeStaticMethod(BMPString.class, "of", String.class);
                }
            } else if (operandType == StandardNames.XS_DOUBLE) {
                oe2 = prepareForEmptiness(oe2, needToHandleEmpty, handleEmpty);
                compiler.compileToPrimitive(operand, Double.TYPE, oe2);
                ga.invokeStaticMethod(DoubleValue.class, "doubleToString", Double.TYPE);
                postpareForEmptiness(onEmpty, ga, methodInfo, needToHandleEmpty, handleEmpty);
                if (requiredClass == String.class) {
                    ga.invokeInstanceMethod(UnicodeString.class, "toString");
                }
            } else if (operandType == StandardNames.XS_FLOAT) {
                oe2 = prepareForEmptiness(oe2, needToHandleEmpty, handleEmpty);
                compiler.compileToPrimitive(operand, Float.TYPE, oe2);
                ga.invokeStaticMethod(FloatValue.class, "floatToString", Float.TYPE);
                postpareForEmptiness(onEmpty, ga, methodInfo, needToHandleEmpty, handleEmpty);
                if (requiredClass == String.class) {
                    ga.invokeInstanceMethod(UnicodeString.class, "toString");
                }
            } else if (operandType == StandardNames.XS_STRING || operandType == StandardNames.XS_UNTYPED_ATOMIC) {
                compiler.compileToPrimitive(operand, requiredClass, onEmpty);

            } else {

                LabelInfo end = methodInfo.newLabel("endASC");

                Class<? extends Item> operandClass = compiler.compileToItem(operand);
                ga.downCast(operandClass, AtomicValue.class, true);
                visitAnnotation(compiler, "AtomicSequenceConverterCompiler-Primitive-Returned");
                if (Cardinality.allowsZero(operand.getCardinality())) {
                    LabelInfo notNull = methodInfo.newLabel("notNullASC");
                    ga.dup();
                    ga.ifNonNull(notNull.label());
                    handleEmptyStringResult(onEmpty, ga, methodInfo, end);
                    methodInfo.placeLabel(notNull);
                }
                ga.invokeInstanceMethod(operandClass, requiredClass == String.class ? "getStringValue" : "getUnicodeStringValue");
                methodInfo.placeLabel(end);
            }
        } else {
            super.compileToPrimitive(compiler, expression, requiredClass, onEmpty);
        }
    }

    private OnEmpty prepareForEmptiness(OnEmpty oe2, boolean needToHandleEmpty, LabelInfo handleEmpty) {
        if (needToHandleEmpty) {
            oe2 = new OnEmpty.UnwindAndJump(handleEmpty);
        }
        return oe2;
    }

    private void postpareForEmptiness(OnEmpty onEmpty, Generator ga, GeneratedMethodInfo methodInfo, boolean needToHandleEmpty, LabelInfo handleEmpty) {
        if (needToHandleEmpty) {
            LabelInfo exit = methodInfo.newLabel("exit");
            ga.goTo(exit);
            methodInfo.placeLabel(handleEmpty);
            //ga.pop();
            if (onEmpty instanceof OnEmpty.ReturnEmptyString) {
                ga.push("");
            } else if (onEmpty instanceof OnEmpty.ReturnEmptyUnicodeString) {
                ga.invokeStaticMethod(EmptyUnicodeString.class, "getInstance");
            } else if (onEmpty instanceof OnEmpty.ReturnNull) {
                ga.pushNull();
            } else {
                throw new AssertionError();
            }
            methodInfo.placeLabel(exit);
        }
    }

    @Override
    public IteratorDetails compileToIterator(CompilerService compiler,
                                  /*@NotNull*/ Expression expression) throws CannotCompileException {

        Expression operand = ((AtomicSequenceConverter) expression).getBaseExpression();
        Converter converter = ((AtomicSequenceConverter) expression).getConverter();
        if (converter == null) {
            converter = ((AtomicSequenceConverter) expression).allocateConverter(compiler.getConfiguration(), false);
        }
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "AtomicSequenceConverterCompiler-Itr");

        if (converter == Converter.ToStringConverter.INSTANCE) {
            visitAnnotation(compiler, "AtomicSequenceConverterCompiler-string");
            // special-case this because it's the most common case

            // return new ItemMappingIterator(base.iterate(), AtomicSequenceConverter.TO_STRING, true)

            ga.newInstance(Type.getType(ItemMappingIterator.class));
            ga.dup();
            compiler.compileToIterator(operand);
            visitLineNumber(compiler, ga, expression);
            ga.getStaticField(AtomicSequenceConverter.class, "TO_STRING_MAPPER", ToStringMappingFunction.class);
            ga.push(true);
            ga.invokeConstructor(ItemMappingIterator.class, SequenceIterator.class, ItemMappingFunction.class, Boolean.TYPE);
            return new IteratorDetails(ItemMappingIterator.class, StringValue.class);
        } else {

            visitAnnotation(compiler, "AtomicSequenceConverterCompiler-1");
            visitLineNumber(compiler, ga, expression);

            /* AtomicSequenceMappingFunction mapper = new AtomicSequenceMappingFunction();
               mapper.setConverter(converter);
               return new ItemMappingIterator(base, mapper, true);
            */

            AtomicSequenceConverter.AtomicSequenceMappingFunction amf = new AtomicSequenceConverter.AtomicSequenceMappingFunction();
            amf.setConverter(converter);

            ga.newInstance(Type.getType(ItemMappingIterator.class));
            ga.dup();
            compiler.compileToIterator(operand);
            allocateStatic(compiler, amf);
            ga.push(true);
            ga.invokeConstructor(ItemMappingIterator.class, SequenceIterator.class, ItemMappingFunction.class, Boolean.TYPE);
            return new IteratorDetails(ItemMappingIterator.class, AtomicValue.class);
        }
    }


}

// Copyright (c) 2011-2022 Saxonica Limited
