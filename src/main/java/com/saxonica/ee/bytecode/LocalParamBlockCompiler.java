package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.GeneratedMethodInfo;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.LocalParam;
import net.sf.saxon.expr.instruct.LocalParamBlock;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.type.TypeHierarchy;

/**
 * Compiler for LocalParam instructions in XSLT (representing an xsl:param in a template)
 */
public class LocalParamBlockCompiler extends ToPushCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {
        LocalParamBlock params = (LocalParamBlock) expression;
        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "LocalParamBlock-push");

//        for (int i=0; i<children.length; i++) {
//            try {
//                LocalParam param = children[i];
//                context.setLocalVariable(param.getSlotNumber(), param.getSelectValue(context));
//            } catch (XPathException e) {
//                e.maybeSetLocation(children[i]);
//                e.maybeSetContext(context);
//                throw e;
//            }
//        }

        for (Operand o : params.operands()) {
            LocalParam param = (LocalParam)o.getChildExpression();
            compiler.generateGetContext();
            ga.push(param.getSlotNumber());
            compiler.compileToSequence(param.getSelectExpression());
            ga.invokeInstanceMethod(XPathContext.class, "setLocalVariable", Integer.TYPE, Sequence.class);
        }

    }
}

// Copyright (c) 2011-2022 Saxonica Limited