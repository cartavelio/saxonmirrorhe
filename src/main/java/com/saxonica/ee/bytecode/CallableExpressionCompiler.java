package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.*;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.EmptySequence;
import org.objectweb.asm.Type;

import java.util.ArrayList;
import java.util.List;

/**
 * Compiler for a CallableExpression. This merely generates a call on the expression's "call" method,
 * which executes the interpreter code for the expression.
 */

public class CallableExpressionCompiler extends ToIteratorCompiler {

    @Override
    public IteratorDetails compileToIterator(CompilerService compiler,
                                  /*@NotNull*/ Expression expression) throws CannotCompileException {
        List<Expression> subExpressions = new ArrayList<Expression>(5);
        for (Operand o: expression.operands()) {
            subExpressions.add(o.getChildExpression());
        }
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "CallableExpressionCompiler - " + expression.getExpressionName());
        visitLineNumber(compiler, ga, expression);

        int arrVar = buildArgumentArray(compiler, ga, methodInfo, subExpressions);

        allocateStatic(compiler, expression);
        compiler.generateGetContext();
        ga.loadLocal(arrVar);
        ga.invokeInstanceMethod(Callable.class, "call", XPathContext.class, Sequence[].class);
        ga.invokeInstanceMethod(Sequence.class, "iterate");
        visitAnnotation(compiler, "CallableCompiler-Done");
        return new IteratorDetails(SequenceIterator.class, Item.class);
    }

    public static int buildArgumentArray(CompilerService compiler, Generator ga, GeneratedMethodInfo methodInfo, List<Expression> subExpressions) throws CannotCompileException {
        int len = subExpressions.size();
        ga.push(len);
        ga.newArray(Type.getType(Sequence.class));
        int arrVar = methodInfo.allocateLocal(Sequence[].class);
        ga.storeLocal(arrVar);
        visitAnnotation(compiler, "CallableCompiler-ArrayCreated");
        int i = 0;
        for (Expression expr : subExpressions) {
            ga.loadLocal(arrVar);
            ga.push(i++);
            if (!Cardinality.allowsMany(expr.getCardinality())) {
                compiler.compileToItem(expr);
                if (Cardinality.allowsZero(expr.getCardinality())) {
                    ga.dup();
                    LabelInfo notNull = methodInfo.newLabel("notNull");
                    ga.ifNonNull(notNull.label());
                    ga.pop();
                    allocateStatic(compiler, EmptySequence.getInstance());
                    methodInfo.placeLabel(notNull);
                }
            } else if (expr instanceof VariableReference) {
                VariableReferenceCompiler.compileToSequence(compiler, (VariableReference) expr);
            } else {
                compiler.compileToIterator(expr);
                ga.invokeStaticMethod(SequenceTool.class, "toLazySequence", SequenceIterator.class);
            }
            visitAnnotation(compiler, "CallableCompiler-SetArg" + (i - 1));
            ga.arrayStore(Type.getType(Sequence.class));
        }
        return arrVar;
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
