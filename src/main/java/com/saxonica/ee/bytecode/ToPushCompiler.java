package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.event.SequenceOutputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;

public class ToPushCompiler extends ToIteratorCompiler {


    @Override
    public void compileToIterator(CompilerService compiler,
                                  Expression expression) throws CannotCompileException {

        /*
          SequenceReceiver saved = context.getReceiver();
            SequenceOutputter outputter = allocateSequenceOutputter(context, expression.getHostLanguage());
          process(context);
          context.setReceiver(saved);
          outputter.close();
          return outputter.iterate();
           * */

        Generator ga = compiler.getCurrentGenerator();
        compiler.generateGetContext();
        ga.push(compiler.getHostLanguage());
        ga.invokeStaticMethod(SequenceOutputter.class, "allocateSequenceOutputter", XPathContext.class, Integer.TYPE);
        compiler.pushNewReceiverInfo(ga);

        compileToPush(compiler, expression);

        compiler.generateGetReceiver();
        ga.checkClass(SequenceOutputter.class);
        ga.dup();
        ga.invokeInstanceMethod(SequenceOutputter.class, "iterate");
        ga.swap();
        ga.invokeInstanceMethod(SequenceOutputter.class, "reset");
        compiler.popReceiverInfo();
    }


    @Override
    public void compileToItem(CompilerService compiler,
                              Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        compiler.generateGetContext();
        ga.push(compiler.getHostLanguage());
        ga.invokeStaticMethod(SequenceOutputter.class, "allocateSequenceOutputter", XPathContext.class, Integer.TYPE);
        compiler.pushNewReceiverInfo(ga);

        compileToPush(compiler, expression);

        compiler.generateGetReceiver();
        ga.checkClass(SequenceOutputter.class);
        ga.dup();
        ga.invokeInstanceMethod(SequenceOutputter.class, "popLastItem");
        ga.swap();
        ga.invokeInstanceMethod(SequenceOutputter.class, "reset");
        compiler.popReceiverInfo();
    }

}

// Copyright (c) 2011 Saxonica Limited.
