package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.*;
import net.sf.saxon.expr.parser.Token;
import net.sf.saxon.expr.sort.DocumentOrderIterator;
import net.sf.saxon.expr.sort.GlobalOrderComparer;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import org.objectweb.asm.Type;

import java.util.Comparator;

public class VennExpressionCompiler extends ToIteratorCompiler {

    @Override
    public IteratorDetails compileToIterator(CompilerService compiler,
                                             Expression expression) throws CannotCompileException {
        /*
           *  SequenceIterator i1 = operand0.iterate(c);
          if ((operand0.getSpecialProperties() & StaticProperty.ORDERED_NODESET) == 0) {
              i1 = new DocumentOrderIterator(i1, GlobalOrderComparer.getInstance());
          }
          SequenceIterator i2 = operand1.iterate(c);
          if ((operand1.getSpecialProperties() & StaticProperty.ORDERED_NODESET) == 0) {
              i2 = new DocumentOrderIterator(i2, GlobalOrderComparer.getInstance());
          }
          switch (operator) {
              case Token.UNION:
                  return new UnionEnumeration(i1, i2,
                                              GlobalOrderComparer.getInstance());
              case Token.INTERSECT:
                  return new IntersectionEnumeration(i1, i2,
                                              GlobalOrderComparer.getInstance());
              case Token.EXCEPT:
                  return new DifferenceEnumeration(i1, i2,
                                              GlobalOrderComparer.getInstance());
          }
          throw new UnsupportedOperationException("Unknown operator in Venn Expression");
           * */
        VennExpression expr = (VennExpression) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        Expression operand0 = expr.getLhsExpression();
        Expression operand1 = expr.getRhsExpression();
        int operator = expr.getOperator();

        compiler.compileToIterator(operand0);
        int iter0Var = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(iter0Var);
        if (!operand0.hasSpecialProperty(StaticProperty.ORDERED_NODESET)) {
            ga.newInstance(Type.getType(DocumentOrderIterator.class));
            ga.dup();
            ga.loadLocal(iter0Var);
            ga.invokeStaticMethod(GlobalOrderComparer.class, "getInstance");
            ga.invokeConstructor(DocumentOrderIterator.class, SequenceIterator.class, Comparator.class);
            ga.storeLocal(iter0Var);
        }

        compiler.compileToIterator(operand1);
        int iter1Var = methodInfo.allocateLocal(SequenceIterator.class);
        ga.storeLocal(iter1Var);
        if (!operand1.hasSpecialProperty(StaticProperty.ORDERED_NODESET)) {
            ga.newInstance(Type.getType(DocumentOrderIterator.class));
            ga.dup();
            ga.loadLocal(iter1Var);
            ga.invokeStaticMethod(GlobalOrderComparer.class, "getInstance");
            ga.invokeConstructor(DocumentOrderIterator.class, SequenceIterator.class, Comparator.class);
            ga.storeLocal(iter1Var);
        }

        switch (operator) {
            case Token.UNION:
                ga.newInstance(Type.getType(UnionEnumeration.class));
                ga.dup();
                ga.loadLocal(iter0Var);
                ga.loadLocal(iter1Var);
                ga.invokeStaticMethod(GlobalOrderComparer.class, "getInstance");
                ga.invokeConstructor(UnionEnumeration.class, SequenceIterator.class, SequenceIterator.class, Comparator.class);
                methodInfo.releaseLocal(iter0Var);
                methodInfo.releaseLocal(iter1Var);
                return new IteratorDetails(UnionEnumeration.class, NodeInfo.class);
            case Token.INTERSECT:
                ga.newInstance(Type.getType(IntersectionEnumeration.class));
                ga.dup();
                ga.loadLocal(iter0Var);
                ga.loadLocal(iter1Var);
                ga.invokeStaticMethod(GlobalOrderComparer.class, "getInstance");
                ga.invokeConstructor(IntersectionEnumeration.class, SequenceIterator.class, SequenceIterator.class, Comparator.class);
                methodInfo.releaseLocal(iter0Var);
                methodInfo.releaseLocal(iter1Var);
                return new IteratorDetails(IntersectionEnumeration.class, NodeInfo.class);
            case Token.EXCEPT:
                ga.newInstance(Type.getType(DifferenceEnumeration.class));
                ga.dup();
                ga.loadLocal(iter0Var);
                ga.loadLocal(iter1Var);
                ga.invokeStaticMethod(GlobalOrderComparer.class, "getInstance");
                ga.invokeConstructor(DifferenceEnumeration.class, SequenceIterator.class, SequenceIterator.class, Comparator.class);
                methodInfo.releaseLocal(iter0Var);
                methodInfo.releaseLocal(iter1Var);
                return new IteratorDetails(DifferenceEnumeration.class, NodeInfo.class);
            default:
                throw new CannotCompileException("Unknown Venn operator", false);
        }
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
