package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.*;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.value.*;
import org.objectweb.asm.Type;

/**
 * Generate byte code for a literal expression in the expression tree
 */
public class LiteralCompiler extends ExpressionCompiler {

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "LiteralCompiler-item");
        visitLineNumber(compiler, ga, expression);
        Item item;
        try {
            item = SequenceTool.asItem(((Literal) expression).getGroundedValue());
        } catch (XPathException err) {
            throw new CannotCompileException(expression, "compileItem on non-singleton");
        }
        if (item == null) {
            ga.pushNull();
            return Item.class;
        } else if (item instanceof AtomicValue) {
            int type = ((AtomicValue) item).getItemType().getFingerprint();
            switch (type) {
                case StandardNames.XS_BOOLEAN:
                    ga.push(((BooleanValue) item).getBooleanValue());
                    ga.invokeStaticMethod(BooleanValue.class, "get", Boolean.TYPE);
                    return BooleanValue.class;
                case StandardNames.XS_DOUBLE:
                    ga.newInstance(Type.getType(DoubleValue.class));
                    ga.dup();
                    ga.push(((DoubleValue) item).getDoubleValue());
                    ga.invokeConstructor(DoubleValue.class, Double.TYPE);
                    return DoubleValue.class;
                case StandardNames.XS_FLOAT:
                    ga.newInstance(Type.getType(FloatValue.class));
                    ga.dup();
                    ga.push(((FloatValue) item).getFloatValue());
                    ga.invokeConstructor(FloatValue.class, Float.TYPE);
                    return FloatValue.class;
                case StandardNames.XS_INTEGER:
                    allocateStatic(compiler, item);
                    ga.checkClass(IntegerValue.class);
                    return item.getClass();
                case StandardNames.XS_STRING:
                    String s = item.getUnicodeStringValue().toString();
                    if (s.isEmpty()) {
                        ga.getStaticField(StringValue.class, "EMPTY_STRING", StringValue.class);
                    } else {
                        /*if(new StringValue(s).containsSurrogatePairs()){
                           allocateStatic(compiler, s);
                       }else{
                           ga.push(s);
                       } */
                        // code changed because of ASM bug - pushing a UTF8 string handled incorrectly
                        allocateStatic(compiler, new StringValue(s));
                        //ga.invokeStaticMethod(StringValue.class, "makeStringValue", CharSequence.class);
                    }
                    return StringValue.class;
            }
        }
        // All other values
        allocateStatic(compiler, item);
        return item.getClass();
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
     * @param onEmpty       action to be performed if the value of the expression is an empty sequence
     */
    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "LiteralCompiler-Primitive - " + (((Literal) expression).toString()));
        visitLineNumber(compiler, ga, expression);
        Literal literal = (Literal) expression;
        if (Literal.isEmptySequence(expression)) {
            onEmpty.generate(ga);
        } else if (requiredClass == Double.TYPE) {
            ga.push(((DoubleValue) literal.getGroundedValue()).getDoubleValue());
        } else if (requiredClass == Float.TYPE) {
            ga.push(((FloatValue) literal.getGroundedValue()).getFloatValue());
        } else if (requiredClass == String.class) {
            GroundedValue value = ((Literal) expression).getGroundedValue();
            if (value instanceof StringValue) {
                ga.push(((StringValue) value).getStringValue());
            } else {
                compileToItem(compiler, expression);
                ga.checkClass(StringValue.class);
                ga.invokeInstanceMethod(StringValue.class, "getStringValue");
            }
        } else if (requiredClass == UnicodeString.class) {
            GroundedValue value = ((Literal) expression).getGroundedValue();
            if (value instanceof StringValue) {
                allocateStatic(compiler, ((StringValue) value).getUnicodeStringValue());
            } else {
                compileToItem(compiler, expression);
                ga.checkClass(UnicodeString.class);
                ga.invokeInstanceMethod(StringValue.class, "getUnicodeStringValue");
            }
        } else if (requiredClass == Long.TYPE) {
            try {
                GroundedValue value = ((Literal) expression).getGroundedValue();
                ga.push(((IntegerValue) value).longValue());
            } catch (XPathException e) {
                throw new CannotCompileException();
            }
        } else if (requiredClass == Integer.TYPE) {
            try {
                GroundedValue value = ((Literal) expression).getGroundedValue();
                ga.push(((IntegerValue) value).longValue());
                ga.cast(Type.LONG_TYPE, Type.INT_TYPE);
            } catch (XPathException e) {
                throw new CannotCompileException();
            }
        } else {
            throw new CannotCompileException();
        }
    }

    @Override
    public IteratorDetails compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "LiteralCompiler-itr");
        visitLineNumber(compiler, ga, expression);
        GroundedValue value = ((Literal) expression).getGroundedValue();
        if (value.getLength() == 0) {
            ga.invokeStaticMethod(EmptyIterator.class, "getInstance");
            return new IteratorDetails(EmptyIterator.class, Item.class);
        } else if (value instanceof Item) {
            Class<? extends Item> itemClass = compileToItem(compiler, expression);
            ga.invokeStaticMethod(SingletonIterator.class, "makeIterator", Item.class);
            return new IteratorDetails(SequenceIterator.class, itemClass);
        } else {
            //	allocateStatic(compiler, value);
            ExpressionCompiler.allocateStatic(compiler, value);
            ga.invokeInstanceMethod(Sequence.class, "iterate");
            return new IteratorDetails(SequenceIterator.class, Item.class);
        }

    }

    @Override
    public void compileToBoolean(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        visitLineNumber(compiler, ga, expression);
        GroundedValue value = ((Literal) expression).getGroundedValue();
        try {
            boolean b = ExpressionTool.effectiveBooleanValue(value.iterate());
            ga.push(b);
        } catch (XPathException e) {
            throw new CannotCompileException(expression);
        }
    }

    @Override
    public void compileToPush(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        compiler.generateGetOutputter();
        int outVar = methodInfo.allocateLocal(Outputter.class);
        ga.storeLocal(outVar);
//		compileToItem(compiler, expression);
//		ga.invokeInstanceMethod(SequenceReceiver.class , "append", Item.class);
        GroundedValue value = ((Literal) expression).getGroundedValue();
        ExpressionCompiler.allocateStatic(compiler, value);
        ga.invokeInstanceMethod(Sequence.class, "iterate");
        int seqVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(seqVar);

        LabelInfo end = methodInfo.newLabel("endLit");
        LabelInfo loop = methodInfo.placeNewLabel("loop");
        ga.loadLocal(seqVar);
        ga.invokeInstanceMethod(SequenceIterator.class, "next");
        ga.dup();
        ga.ifNull(end.label());
        ga.loadLocal(outVar);
        ga.swap();
        allocateStatic(compiler, Loc.NONE);
        ga.push(ReceiverOption.ALL_NAMESPACES);
        ga.invokeInstanceMethod(Outputter.class, "append", Item.class, Location.class, Integer.TYPE);
        ga.goTo(loop);


        methodInfo.placeLabel(end);
        ga.pop();

        methodInfo.releaseLocal(outVar);
        methodInfo.releaseLocal(seqVar);

        /*
           * while (true) {
              Item it = iter.next();
              if (it==null) break;
              out.append(it, 0, NodeInfo.ALL_NAMESPACES);
          }
           * */

    }


}

// Copyright (c) 2011-2022 Saxonica Limited
