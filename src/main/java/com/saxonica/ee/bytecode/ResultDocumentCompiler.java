package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ResultDocument;

/**
 * Generate bytecode for the xsl:result-document instruction
 */

public class ResultDocumentCompiler extends ToPushCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        // The strategy here is unorthodox. We first compile the body of the xsl:result-document instruction as
        // a free-standing expression (we can do this because variables are all available in the context heap in the
        // usual way). Then we create a copy of the xsl;result-document instruction that uses this compiled content;
        // and then we generate code to call the interpreter, via the ResultDocument.process() method. So all the code
        // for assembling serialization parameters, etc, is interpreted, but the body of the instruction is compiled.
        Generator ga = compiler.getCurrentGenerator();
        ResultDocument instr = (ResultDocument) expression;
        Expression content = instr.getContentExpression();
        Expression compiledContent = compiler.compileToByteCode(content, "resultDocument", Expression.PROCESS_METHOD);
        if (compiledContent == null) {
            throw new CannotCompileException(content);
        }
        allocateStatic(compiler, instr);
        allocateStatic(compiler, compiledContent);
        compiler.generateGetContext();
        ga.invokeInstanceMethod(ResultDocument.class, "process", Expression.class, XPathContext.class);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited