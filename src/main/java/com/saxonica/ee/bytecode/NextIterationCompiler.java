package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.GeneratedMethodInfo;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.instruct.NextIteration;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.expr.instruct.WithParam;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.type.TypeHierarchy;
import org.objectweb.asm.Type;

/**
 * Compiler for xsl:next-iteration within xsl:iterate (XSLT 3.0)
 */
public class NextIterationCompiler extends ToPushCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        NextIteration instr = (NextIteration) expression;
        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "NextIteration-push");

        WithParam[] actualParams = instr.getParameters();
        if (actualParams.length == 1) {
//            cm.setLocalVariable(actualParams[0].getSlotNumber(), actualParams[0].getSelectValue(context));
            compiler.generateGetContext();
            ga.checkClass(XPathContextMajor.class);
            ga.push(actualParams[0].getSlotNumber());
            compiler.compileToSequence(actualParams[0].getSelectExpression());
            ga.invokeInstanceMethod(XPathContextMajor.class, "setLocalVariable", Integer.TYPE, Sequence.class);
        } else {
//            // we can't overwrite any of the parameters until we've evaluated all of them: test iterate012
//            ValueRepresentation[] oldVars = cm.getAllVariableValues();
//            ValueRepresentation[] newVars = new ValueRepresentation[oldVars.length];
//            System.arraycopy(oldVars, 0, newVars, 0, oldVars.length);
//            for (int i = 0; i < actualParams.length; i++) {
//                WithParam wp = actualParams[i];
//                newVars[wp.getSlotNumber()] = wp.getSelectValue(context);
//            }
//            cm.resetAllVariableValues(newVars);
            compiler.generateGetContext();
            ga.checkClass(XPathContextMajor.class);
            ga.invokeInstanceMethod(XPathContextMajor.class, "getAllVariableValues");
            int oldVarsVar = methodInfo.allocateLocal(Sequence[].class);
            ga.storeLocal(oldVarsVar);
            ga.loadLocal(oldVarsVar);
            ga.arrayLength();
            ga.newArray(Type.getType(Sequence.class));
            int newVarsVar = methodInfo.allocateLocal(Sequence[].class);
            ga.storeLocal(newVarsVar);
            ga.loadLocal(oldVarsVar);
            ga.push(0);
            ga.loadLocal(newVarsVar);
            ga.push(0);
            ga.loadLocal(oldVarsVar);
            ga.arrayLength();
            ga.invokeStaticMethod(System.class, "arraycopy", Object.class, Integer.TYPE, Object.class, Integer.TYPE, Integer.TYPE);
            for (int i = 0; i < actualParams.length; i++) {
                WithParam wp = actualParams[i];
//                newVars[wp.getSlotNumber()] = wp.getSelectValue(context);
                ga.loadLocal(newVarsVar);
                ga.push(wp.getSlotNumber());
                compiler.compileToSequence(wp.getSelectExpression());
                ga.arrayStore(Type.getType(Sequence.class));
            }

            compiler.generateGetContext();
            ga.checkClass(XPathContextMajor.class);
            ga.loadLocal(newVarsVar);
            ga.invokeInstanceMethod(XPathContextMajor.class, "resetAllVariableValues", Sequence[].class);

        }
//        cm.requestTailCall(nextIterationFunction, emptyArgs);

        ga.goTo(methodInfo.getXslIterateLoopLabelStack().peek());

    }

}

// Copyright (c) 2011-2022 Saxonica Limited