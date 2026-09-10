package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.converter.*;
import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.OnEmpty;
import net.sf.saxon.expr.CastExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.PlainType;


/**
 * Generate code for a CastExpression
 */
public class CastExpressionCompiler extends ToItemCompiler {

    /**
     * Generate bytecode to evaluate the CastExpression as an Item
     * Precondition: none.
     * Postcondition: the stack contains an Item representing the result of the expression
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {

        final Expression base = ((CastExpression) expression).getBaseExpression();
        final AtomicType targetType = ((CastExpression) expression).getTargetType();
        visitAnnotation(compiler, "CastExpression-Item");
        PlainType sourceType = (PlainType) base.getItemType();
        ConverterCompiler converterCompiler = getConverterCompiler(sourceType, targetType);
        converterCompiler.compileToItem(compiler, expression);

    }

    /**
     * Generate bytecode to evaluate the expression leaving a plain Java value on the stack. The static type
     * of the expression must be such that the value is known to be representable by a value of the specified class;
     * the cardinality must be either exactly-one or zero-or-one.
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     * @param requiredClass the class of the Java value that is to be left on the stack if the generated code
     *                      exits normally. This must be one of Integer.TYPE, Double.TYPE, Float.TYPE, BigDecimal.class,
     *                      UnicodeString.class.
     * @param onEmpty       action to be taken if the result of the expression is an empty sequence
     */
    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        final CastExpression cast = (CastExpression) expression;
        final Expression base = cast.getBaseExpression();
        final AtomicType targetType = cast.getTargetType();
        PlainType sourceType = (PlainType) base.getItemType();
        visitAnnotation(compiler, "CastExpression-Primitive");

        ConverterCompiler converterCompiler = getConverterCompiler(sourceType, targetType);
        if (converterCompiler instanceof ConverterCompiler.GenericConverterCompiler) {
            super.compileToPrimitive(compiler, expression, requiredClass, onEmpty);
        } else {
            converterCompiler.compileToPrimitive(compiler, expression, requiredClass, onEmpty);
        }
    }


    public static ConverterCompiler getConverterCompiler(PlainType sourceType, /*@NotNull*/ AtomicType targetType) throws CannotCompileException {

        if (sourceType.isAtomicType()) {
            int tt = targetType.getFingerprint();
            int st = sourceType.getPrimitiveType();

            switch (tt) {
                case StandardNames.XS_UNTYPED_ATOMIC:
                    switch (st) {
                        case StandardNames.XS_STRING:
                            return new StringToUntypedAtomicCompiler();
                        case StandardNames.XS_DOUBLE:
                            return new DoubleToUntypedAtomicCompiler();
                        case StandardNames.XS_FLOAT:
                            return new FloatToUntypedAtomicCompiler();
                        case StandardNames.XS_INTEGER:
                            return new IntegerToUntypedAtomicCompiler();
                        case StandardNames.XS_BOOLEAN:
                            return new BooleanToUntypedAtomicCompiler();
                        default:
                            return new ConverterCompiler.GenericConverterCompiler();
                    }
                case StandardNames.XS_STRING:
                    switch (st) {
                        case StandardNames.XS_DOUBLE:
                            return new DoubleToStringCompiler();
                        case StandardNames.XS_FLOAT:
                            return new FloatToStringCompiler();
                        case StandardNames.XS_INTEGER:
                            return new IntegerToStringCompiler();
                        case StandardNames.XS_BOOLEAN:
                            return new BooleanToStringCompiler();
                        default:
                            return new AnyTypeToStringConverterCompiler();
                    }
                case StandardNames.XS_FLOAT:
                    switch (st) {
                        case StandardNames.XS_UNTYPED_ATOMIC:
                        case StandardNames.XS_STRING:
                            return new StringToFloatCompiler();
                        case StandardNames.XS_DOUBLE:
                            return new DoubleToFloatCompiler();
                        case StandardNames.XS_INTEGER:
                            return new IntegerToFloatCompiler();
                        case StandardNames.XS_BOOLEAN:
                            return new BooleanToFloatCompiler();
                        default:
                            return new ConverterCompiler.GenericConverterCompiler();
                    }
                case StandardNames.XS_DOUBLE:
                    switch (st) {
                        case StandardNames.XS_UNTYPED_ATOMIC:
                        case StandardNames.XS_STRING:
                            return new StringToDoubleCompiler();
                        case StandardNames.XS_FLOAT:
                            return new FloatToDoubleCompiler();
                        case StandardNames.XS_INTEGER:
                            return new IntegerToDoubleCompiler();
                        case StandardNames.XS_BOOLEAN:
                            return new BooleanToDoubleCompiler();
                        default:
                            return new ConverterCompiler.GenericConverterCompiler();
                    }

                case StandardNames.XS_INTEGER:
                    switch (st) {
                        case StandardNames.XS_UNTYPED_ATOMIC:
                        case StandardNames.XS_STRING:
                            return new StringToIntegerCompiler();
                        case StandardNames.XS_FLOAT:
                            return new FloatToIntegerCompiler();
                        case StandardNames.XS_DOUBLE:
                            return new DoubleToIntegerCompiler();
                        case StandardNames.XS_BOOLEAN:
                            return new BooleanToIntegerCompiler();
                        default:
                            return new ConverterCompiler.GenericConverterCompiler();
                    }

                case StandardNames.XS_BOOLEAN:
                    switch (st) {
                        case StandardNames.XS_UNTYPED_ATOMIC:
                        case StandardNames.XS_STRING:
                            return new StringToBooleanCompiler();
                        case StandardNames.XS_FLOAT:
                            return new FloatToBooleanCompiler();
                        case StandardNames.XS_DOUBLE:
                            return new DoubleToBooleanCompiler();
                        case StandardNames.XS_INTEGER:
                            return new IntegerToBooleanCompiler();
                        default:
                            return new ConverterCompiler.GenericConverterCompiler();
                    }

                default:
                    return new ConverterCompiler.GenericConverterCompiler();
            }
        } else {
            return new ConverterCompiler.GenericConverterCompiler();
        }
    }
}

// Copyright (c) 2011-2022 Saxonica Limited

