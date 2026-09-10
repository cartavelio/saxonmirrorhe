package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.GeneratedMethodInfo;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.StaticProperty;
import net.sf.saxon.expr.oper.OperandArray;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.ma.arrays.SquareArrayConstructor;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;

import java.util.ArrayList;
import java.util.List;


/**
 * Generate bytecode for an SquareArrayConstructor expression. This typically originates
 * as an array constructor in square brackets, for example [a,b,c,d].
 */
public class SquareArrayConstructorCompiler extends ToItemCompiler {


    /**
     * Generate bytecode to evaluate the ArrayBlockExpression as an (array) item
     * Precondition: none.
     * Postcondition: the stack contains an item representing the constructed array
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return the class of item generated
     */
    //@Override
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler,
                                               Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        OperandArray operanda = ((SquareArrayConstructor) expression).getOperanda();
        visitAnnotation(compiler, "ArrayBlockCompiler");
        if (operanda.getNumberOfOperands() == 0) {
            allocateStatic(compiler, SimpleArrayItem.EMPTY_ARRAY);
        } else {
            visitLineNumber(compiler, ga, expression);
            int listVar = methodInfo.allocateLocal(ArrayList.class);
            ga.newInstance(SimpleArrayItem.class);
            ga.dup();
            ga.newInstance(ArrayList.class);
            ga.dup();
            ga.invokeConstructor(ArrayList.class);
            ga.storeLocal(listVar);
            for (Operand o : expression.operands()) {
                Expression child = o.getChildExpression();
                ga.loadLocal(listVar);
                if (child instanceof Literal) {
                    allocateStatic(compiler, ((Literal)child).getGroundedValue());
                } else if (child.getCardinality() == StaticProperty.EXACTLY_ONE) {
                    compiler.compileToItem(child);
                } else {
                    compiler.compileToIterator(child);
                    ga.invokeStaticMethod(SequenceTool.class, "toGroundedValue", SequenceIterator.class);
                }
                ga.invokeInstanceMethod(List.class, "add", Object.class);
                ga.pop(); // List.add() returns true
            }
            ga.loadLocal(listVar);
            ga.invokeConstructor(SimpleArrayItem.class, List.class);
        }
        return SimpleArrayItem.class;
    }


}

// Copyright (c) 2018-2022 Saxonica Limited
