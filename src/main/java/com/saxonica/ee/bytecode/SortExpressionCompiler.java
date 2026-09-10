package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.*;
import net.sf.saxon.om.SequenceIterator;
import org.objectweb.asm.Type;

public class SortExpressionCompiler extends ToIteratorCompiler {

    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {
        SortExpression sorter = (SortExpression) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        visitAnnotation(compiler, "SortExpressionCompiler");
        visitLineNumber(compiler, ga, expression);

        Expression baseExpr = sorter.getBaseExpression();
        compiler.compileToIterator(baseExpr);
        int baseIterVar = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(baseIterVar);

        AtomicComparer[] comparators = sorter.getComparators();
        int compsVar = -1;
        if (comparators == null) {
            SortKeyDefinitionList definitions = sorter.getSortKeyDefinitionList();
            compsVar = compileSortKeyDefinitions(compiler, definitions);

        }

        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "newMinorContext");
        int context2Var = methodInfo.allocateLocal(XPathContext.class);
        ga.storeLocal(context2Var);
        //mi.pushContextVariableInfo(context2Var, false);

        ga.newInstance(Type.getType(SortedIterator.class));
        ga.dup();
        ga.loadLocal(context2Var);
        ga.loadLocal(baseIterVar);
        allocateStatic(compiler, sorter);
        if (compsVar == -1) {
            allocateStatic(compiler, comparators);
        } else {
            ga.loadLocal(compsVar);
        }
        ga.push(sorter.getSortKeyDefinition(0).isSetContextForSortKey());
        ga.invokeConstructor(SortedIterator.class, XPathContext.class, SequenceIterator.class, SortKeyEvaluator.class,
                AtomicComparer[].class, Boolean.TYPE);

        methodInfo.releaseLocal(baseIterVar);
        methodInfo.releaseLocal(context2Var);
        methodInfo.releaseLocal(compsVar);

    }

    /**
     * Generate code to evaluate variable parts of the sort key definitions
     *
     * @param compiler
     * @param definitions the array of sort key definitions
     * @return the identifier of a variable which will contain the run-time atomic comparers
     */

    public static int compileSortKeyDefinitions(CompilerService compiler, SortKeyDefinitionList definitions) {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        int compsVar;//           comps = new AtomicComparer[sortKeyDefinitions.length];
//            for (int s = 0; s < sortKeyDefinitions.length; s++) {
//                AtomicComparer comp = sortKeyDefinitions[s].getFinalComparator();
//                if (comp == null) {
//                    comp = sortKeyDefinitions[s].makeComparator(xpc);
//                }
//                comps[s] = comp;
//            }
        ga.push(definitions.size());
        ga.newArray(Type.getType(AtomicComparer.class));
        compsVar = methodInfo.allocateLocal(AtomicComparer[].class);
        ga.storeLocal(compsVar);
        for (int s = 0; s < definitions.size(); s++) {
            ga.loadLocal(compsVar);
            // assert stack AtomicComparer[]
            ga.push(s);
            // assert stack AtomicComparer[] int
            SortKeyDefinition skd = definitions.getSortKeyDefinition(s);
            allocateStatic(compiler, skd);
            ga.invokeInstanceMethod(SortKeyDefinition.class, "getFinalComparator");
            ga.dup();
            // assert stack AtomicComparer[] int AtomicComparer AtomicComparer
            LabelInfo nonNullComparator = methodInfo.newLabel("nonNullComparator");
            ga.ifNonNull(nonNullComparator.label());
            ga.pop();
            // assert stack AtomicComparer[] int
            allocateStatic(compiler, skd);
            compiler.generateGetContext();
            // assert stack AtomicComparer[] int SortKeyDefinition context
            ga.invokeInstanceMethod(SortKeyDefinition.class, "makeComparator", XPathContext.class);
            methodInfo.placeLabel(nonNullComparator);
            // assert stack AtomicComparer[] AtomicComparer
            ga.arrayStore(Type.getType(AtomicComparer.class));
        }
        return compsVar;
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
