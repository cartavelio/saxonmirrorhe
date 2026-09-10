package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CannotCompileException;
import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.GeneratedMethodInfo;
import com.saxonica.ee.bytecode.util.Generator;
import net.sf.saxon.Controller;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.SequenceCollector;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;
import net.sf.saxon.s9api.HostLanguage;

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

        Generator ga = compilePushToSequenceCollector(compiler, expression);
        ga.invokeInstanceMethod(SequenceCollector.class, "iterate");
    }

    public static Generator compilePushToSequenceCollector(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getController");
        ga.invokeInstanceMethod(Controller.class, "allocateSequenceOutputter");
        int seqCollectorVar = methodInfo.allocateLocal(SequenceCollector.class);
        ga.storeLocal(seqCollectorVar);
        ga.loadLocal(seqCollectorVar);
        ga.invokeInstanceMethod(SequenceCollector.class, "getPipelineConfiguration");
        allocateStatic(compiler, compiler.getHostLanguage());
        ga.invokeInstanceMethod(PipelineConfiguration.class, "setHostLanguage", HostLanguage.class);
        ga.newInstance(ComplexContentOutputter.class);
        ga.dup();
        ga.loadLocal(seqCollectorVar);
        ga.invokeConstructor(ComplexContentOutputter.class, Receiver.class);
        compiler.pushNewOutputterInfo(ga);
        compiler.compileToPush(expression);
        compiler.popOutputterInfo();
        ga.loadLocal(seqCollectorVar);
        ga.invokeInstanceMethod(SequenceCollector.class, "close");
        ga.loadLocal(seqCollectorVar);
        return ga;
    }


    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler,
                                               Expression expression) throws CannotCompileException {

        Generator ga = compilePushToSequenceCollector(compiler, expression);
        ga.invokeInstanceMethod(SequenceCollector.class, "getFirstItem");
        return Item.class;
    }

}

// Copyright (c) 2011-2022 Saxonica Limited
