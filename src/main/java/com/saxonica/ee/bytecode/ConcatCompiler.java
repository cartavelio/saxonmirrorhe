package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.instruct.Block;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Cardinality;
import org.objectweb.asm.Type;

/**
 * Generate bytecode for evaluation of a Concat function
 */
public class ConcatCompiler extends ToStringCompiler {

    @Override
    public void compileToUnicodeString(CompilerService compiler, Expression expression, OnEmpty onEmpty)
            throws CannotCompileException {
        // In Saxon 11, the arguments to concat() are reduced to a single argument, which is a Block
        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        SystemFunctionCall fnc = (SystemFunctionCall)expression;
        Operand[] operanda;
        if (((SystemFunctionCall) expression).getArg(0) instanceof Block) {
            operanda = ((Block) ((SystemFunctionCall) expression).getArg(0)).getOperanda();
        } else {
            operanda = fnc.getOperanda().copy();
        }
        //Block block = (Block) ((SystemFunctionCall) expression).getArg(0);
        //Expression[] arguments = ((Concat) expression).getArguments();
        int numArgs = operanda.length;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Concat-item");
        int valVar = methodInfo.allocateLocal(AtomicValue.class);
        ga.newInstance(Type.getType(UnicodeBuilder.class));
        ga.dup();
        ga.push(64);
        ga.invokeConstructor(UnicodeBuilder.class, Integer.TYPE);
        for (int i = 0; i < numArgs; i++) {
            Expression arg = operanda[i].getChildExpression();
            if (th.isSubType(arg.getItemType(), BuiltInAtomicType.STRING) &&
                    arg.getCardinality() == StaticProperty.EXACTLY_ONE) {
                // this path is particularly useful for literal arguments
                compiler.compileToPrimitive(arg, UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
                ga.invokeInstanceMethod(UnicodeBuilder.class, "append", UnicodeString.class);
            } else if (Cardinality.allowsMany(arg.getCardinality())) {
                compiler.compileToIterator(arg);
                ga.invokeInstanceMethod(UnicodeBuilder.class, "appendAll", SequenceIterator.class);
            } else {
                compiler.compileToItem(arg);
                ga.checkClass(AtomicValue.class);
                ga.storeLocal(valVar);
                LabelInfo nullLab = null;
                if (Cardinality.allowsZero(arg.getCardinality()) || arg instanceof LocalVariableReference) {
                    nullLab = methodInfo.newLabel("notNullCc" + i);
                    ga.loadLocal(valVar);
                    ga.ifNull(nullLab.label());
                }
                ga.loadLocal(valVar);
                ga.invokeInstanceMethod(AtomicValue.class, "getUnicodeStringValue");
                ga.invokeInstanceMethod(UnicodeBuilder.class, "append", UnicodeString.class);
                if (nullLab != null) {
                    methodInfo.placeLabel(nullLab);
                }
            }
        }
        ga.invokeInstanceMethod(UnicodeBuilder.class, "toUnicodeString");
        methodInfo.releaseLocal(valVar);
    }

}

// Copyright (c) 2011-2020 Saxonica Limited
