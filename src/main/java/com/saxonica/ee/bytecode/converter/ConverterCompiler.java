package com.saxonica.ee.bytecode.converter;

import com.saxonica.ee.bytecode.ExpressionCompiler;
import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.ConversionResult;
import net.sf.saxon.type.Converter;
import net.sf.saxon.value.AtomicValue;

/**
 * This class generates code to implement a casting conversion.
 */
public abstract class ConverterCompiler {

    public abstract Class<? extends Item> compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException;

    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        throw new CannotCompileException();
    }

    public static class GenericConverterCompiler extends ConverterCompiler {

        @Override
        public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty) throws CannotCompileException {
            throw new UnsupportedOperationException("compileToPrimitive for generic target type");

        }

        @Override
        public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
            CastExpression cast = (CastExpression) expression;
            final Expression base = cast.getBaseExpression();
            final AtomicType targetType = cast.getTargetType();
            final Converter converter = cast.getConverter();
            final Generator ga = compiler.getCurrentGenerator();
            final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
            ExpressionCompiler.visitAnnotation(compiler, "GenericConverterCompiler-Item");
            ExpressionCompiler.visitLineNumber(compiler, ga, expression);
            LabelInfo end = methodInfo.newLabel("endCast");
            LabelInfo notNull = methodInfo.newLabel("notNull");
            compiler.compileToItem(base);
            ga.dup();
            ga.ifNonNull(notNull.label());
            ga.pop();
            ga.push((String) null);
            ga.goTo(end);
            methodInfo.placeLabel(notNull);
            ga.checkClass(AtomicValue.class);
            int valueVar = methodInfo.allocateLocal(AtomicValue.class);
            ga.storeLocal(valueVar);
            if (converter == null) {
                LabelInfo weHaveAConverter = methodInfo.newLabel("weHaveAConverter");
                ConversionRules conversionRules = compiler.getConfiguration().getConversionRules();
                ExpressionCompiler.allocateStatic(compiler, conversionRules);
                ga.loadLocal(valueVar);
                ga.invokeInstanceMethod(AtomicValue.class, "getPrimitiveType");
                ExpressionCompiler.allocateStatic(compiler, targetType);
                ga.invokeInstanceMethod(ConversionRules.class, "getConverter", AtomicType.class, AtomicType.class);
                ga.dup();
                ga.ifNonNull(weHaveAConverter.label());

                ga.pop();
                compiler.generateParameterizedDynamicError(
                        new MessageTemplate(ga, "Casting {$1} to " + targetType + " is not permitted", valueVar),
                        "XPTY0004", base.getLocation(), true);

                methodInfo.placeLabel(weHaveAConverter);
                // TODO: check that this conversion is allowed in XPath 2.0 if applicable

                if (((CastExpression) expression).getNamespaceResolver() != null) {
                    //converter.setNamespaceResolver(nsResolver);
                    //ga.dup();
                    ExpressionCompiler.allocateStatic(compiler, ((CastExpression) expression).getNamespaceResolver());
                    ga.invokeInstanceMethod(Converter.class, "setNamespaceResolver", NamespaceResolver.class);
                }

            } else {
                // The converter was allocated statically, but we haven't got around to generating custom code for it
                // So we load the converter and call it.
                ExpressionCompiler.allocateStatic(compiler, converter);
            }
            // At this point the converter is on top-of-stack, so we can do the conversion
            ga.loadLocal(valueVar);
            ga.invokeInstanceMethod(Converter.class, "convert", AtomicValue.class);
            // some converters always succeed: no need to check the result
            if (converter != null && converter.isAlwaysSuccessful()) {
                ga.checkClass(AtomicValue.class);
            } else {
                ga.invokeInstanceMethod(ConversionResult.class, "asAtomic");
            }
            methodInfo.placeLabel(end);
            methodInfo.releaseLocal(valueVar);
            return AtomicValue.class;
        }

    }


}

// Copyright (c) 2011-2022 Saxonica Limited