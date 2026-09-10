package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.DocumentInstr;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.SingletonIterator;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.TextFragmentValue;
import org.objectweb.asm.Type;

/**
 * Generate bytecode for the xsl:document instruction in XSLT or the document-node{} constructor in XQuery
 */

public class DocumentInstrCompiler extends ToPushCompiler {


    @Override
    public void compileToPush(final CompilerService compiler, Expression expression) throws CannotCompileException {

        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "DocumentInstrCompiler-Push");
        visitLineNumber(compiler, ga, expression);
        final DocumentInstr instr = (DocumentInstr) expression;

        if (instr.isPreservingTypes() && !instr.isTextOnly()) {
            compiler.generateGetReceiver();
            ga.push(0);
            ga.invokeInstanceMethod(Receiver.class, "startDocument", Integer.TYPE);
            compiler.compileToPush(instr.getContentExpression());
            compiler.generateGetReceiver();
            ga.invokeInstanceMethod(Receiver.class, "endDocument");
        } else {
            LabelInfo nonNull = methodInfo.newLabel("docNonNull");
            LabelInfo docDone = methodInfo.newLabel("docDone");
            compileToItem(compiler, instr);
            ga.dup();
            ga.ifNonNull(nonNull.label());
            ga.pop();
            ga.goTo(docDone);
            methodInfo.placeLabel(nonNull);
            // out.append(item, locationId, NodeInfo.ALL_NAMESPACES);
            compiler.generateGetReceiver();
            ga.swap();
            allocateStatic(compiler, instr.getLocation());
            ga.push(ReceiverOptions.ALL_NAMESPACES);
            ga.invokeInstanceMethod(Receiver.class, "append", Item.class, Location.class, Integer.TYPE);
            methodInfo.placeLabel(docDone);
        }
    }

    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "DocumentInstrCompiler-Item");
        visitLineNumber(compiler, ga, expression);
        final DocumentInstr instr = (DocumentInstr) expression;

        // Note, don't bother with lazy construction - it's semi-obsolete

        if (instr.isTextOnly()) {
//            CharSequence textValue;
            allocateStatic(compiler, compiler.getConfiguration());
            if (instr.getConstantText() != null) {
//                textValue = constantText;
                ga.push(instr.getConstantText().toString());
            } else {
//                FastStringBuffer sb = new FastStringBuffer(FastStringBuffer.SMALL);
//                SequenceIterator iter = content.iterate(context);
                int fsbVar = methodInfo.allocateLocal(FastStringBuffer.class);
                int iterVar = methodInfo.allocateLocal(SequenceIterator.class);

                ga.newInstance(FastStringBuffer.class);
                ga.dup();
                ga.push(FastStringBuffer.C64);
                ga.invokeConstructor(FastStringBuffer.class, Integer.TYPE);
                ga.storeLocal(fsbVar);

                compiler.compileToIterator(instr.getContentExpression());
                ga.storeLocal(iterVar);

                LabelInfo loopDone = methodInfo.newLabel("docLoopDone");
                LabelInfo loop = methodInfo.placeNewLabel("docLoop");
//                while (true) {
//                    Item item = iter.next();
//                    if (item==null) break;
//                    sb.append(item.getStringValueCS());
//                }
                ga.loadLocal(iterVar);
                ga.invokeInstanceMethod(SequenceIterator.class, "next");
                ga.dup();
                ga.ifNull(loopDone.label());
                ga.invokeInstanceMethod(Item.class, "getStringValueCS");
                ga.loadLocal(fsbVar);
                ga.swap();
                ga.invokeInstanceMethod(FastStringBuffer.class, "append", CharSequence.class);
                ga.goTo(loop);

                methodInfo.placeLabel(loopDone);
                ga.pop();
//                textValue = sb.condense();
                ga.loadLocal(fsbVar);
                ga.invokeInstanceMethod(FastStringBuffer.class, "condense");

                methodInfo.releaseLocal(fsbVar);
                methodInfo.releaseLocal(iterVar);
            }
//            root = new TextFragmentValue(config, textValue, getBaseURI());
            ga.push(instr.getStaticBaseURIString());
            ga.invokeStaticMethod(TextFragmentValue.class, "makeTextFragment", Configuration.class, CharSequence.class, String.class);

////            ((TextFragmentValue)root).setConfiguration(controller.getConfiguration());
//            ga.dup();
//            allocateStatic(compiler, compiler.getConfiguration());
//            ga.invokeInstanceMethod(TextFragmentValue.class, "setConfiguration", Configuration.class);

        } else {

//            SequenceReceiver saved = context.getReceiver();
//
//            Builder builder = controller.makeBuilder();
            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getController");
            ga.invokeInstanceMethod(Controller.class, "makeBuilder");
            // assert stack: Builder

            //builder.setUseEventLocation(false);
            ga.dup();
            ga.push(false);
            ga.invokeInstanceMethod(Builder.class, "setUseEventLocation", Boolean.TYPE);
//
//            builder.setBaseURI(getBaseURI());
            ga.dup();
            ga.push(instr.getStaticBaseURIString());
            ga.invokeInstanceMethod(Builder.class, "setBaseURI", String.class);
            // assert stack: Builder

//            builder.setTiming(false);
            ga.dup();
            ga.push(false);
            ga.invokeInstanceMethod(Builder.class, "setTiming", Boolean.TYPE);
            // assert stack: Builder
//
//            PipelineConfiguration pipe = controller.makePipelineConfiguration();
//            pipe.setHostLanguage(getHostLanguage());
//            builder.setPipelineConfiguration(pipe);
            ga.dup();
            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getController");
            ga.invokeInstanceMethod(Controller.class, "makePipelineConfiguration");
            // assert stack: Builder Builder PipelineConfiguration
            ga.dup();
            ga.push(instr.getPackageData().getHostLanguage());
            ga.invokeInstanceMethod(PipelineConfiguration.class, "setHostLanguage", Integer.TYPE);
            // assert stack: Builder Builder PipelineConfiguration
            ga.invokeInstanceMethod(Receiver.class, "setPipelineConfiguration", PipelineConfiguration.class);
            // assert stack: Builder
//
//            context.changeOutputDestination(
//                    builder, validationOptions)
//            ga.dup();
//            compiler.generateGetContext();
//            ga.swap();

            ga.dup();

            // assert stack:  Builder Builder

            allocateStatic(compiler, instr.getValidationOptions());
            // assert stack:  Builder Builder ParseOptions

//            SequenceReceiver out = ComplexContentOutputter.makeComplexContentReceiver(builder, getValidationOptions());
//            context.setReceiver(out);

            ga.invokeStaticMethod(ComplexContentOutputter.class, "makeComplexContentReceiver", Receiver.class, ParseOptions.class);

            // assert stack : Builder SequenceReceiver

            compiler.generateGetContext();

            // assert stack : Builder SequenceReceiver XPathContext

            ga.swap();

            // assert stack : Builder XPathContext SequenceReceiver
            ga.invokeInstanceMethod(XPathContext.class, "setReceiver", Receiver.class);

//            ga.invokeInstanceMethod(XPathContext.class, "changeOutputDestination", Receiver.class, ParseOptions.class);
            // assert stack: Builder

            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getReceiver");
            compiler.pushNewReceiverInfo(ga);
            // assert stack: Builder

//            Receiver out = context.getReceiver();
//            out.open();
//            out.startDocument(0);

            compiler.generateGetReceiver();
            ga.invokeInstanceMethod(Receiver.class, "open");
            compiler.generateGetReceiver();
            ga.push(0);
            ga.invokeInstanceMethod(Receiver.class, "startDocument", Integer.TYPE);
//
//            content.process(context);
            compiler.compileToPush(instr.getContentExpression());
//
//            out.endDocument();
//            out.close();
            compiler.generateGetReceiver();
            ga.invokeInstanceMethod(Receiver.class, "endDocument");
            compiler.generateGetReceiver();
            ga.invokeInstanceMethod(Receiver.class, "close");

            compiler.popReceiverInfo();
//            context.setReceiver(saved);

            // assert stack: Builder 
//            root = (DocumentInfo)builder.getCurrentRoot();
            ga.invokeInstanceMethod(Builder.class, "getCurrentRoot");
        }


    }

    @Override
    public void compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        DocumentInstr documentInstr = (DocumentInstr) expression;
        visitAnnotation(compiler, "DocumentInstrCompiler-Iter");
        int m = documentInstr.getImplementationMethod();
        LabelInfo notNull = methodInfo.newLabel("notNullDocInstr");
        LabelInfo end = methodInfo.newLabel("endDocInstr");
        if ((m & DocumentInstr.EVALUATE_METHOD) != 0) {
            compileToItem(compiler, expression);
            ga.dup();
            ga.ifNonNull(notNull.label());
            ga.pop();
            ga.invokeStaticMethod(EmptyIterator.class, "emptyIterator");
            ga.goTo(end);

            methodInfo.placeLabel(notNull);
            ga.invokeStaticMethod(SingletonIterator.class, "makeIterator", Item.class);
            methodInfo.placeLabel(end);
        } else if ((m & DocumentInstr.ITERATE_METHOD) != 0) {
            ga.throwException(Type.getType(AssertionError.class), "iterate() is not implemented in the subclass ");
        } else {
            /*
            *  Controller controller = context.getController();
        SequenceReceiver saved = context.getReceiver();
        SequenceOutputter seq = controller.allocateSequenceOutputter(20);
        seq.getPipelineConfiguration().setHostLanguage(getHostLanguage());
        context.setReceiver(seq);
        process(context);
        context.setReceiver(saved);
        seq.close();
        return seq.iterate();
            * */
            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getController");
            ga.invokeInstanceMethod(Controller.class, "allocateSequenceOutputter");
            int seqReceiverVar = methodInfo.allocateLocal(SequenceOutputter.class);
            ga.storeLocal(seqReceiverVar);
            ga.loadLocal(seqReceiverVar);
            ga.invokeInstanceMethod(SequenceOutputter.class, "getPipelineConfiguration");
            ga.push(documentInstr.getPackageData().getHostLanguage());
            ga.invokeInstanceMethod(PipelineConfiguration.class, "setHostLanguage", Integer.TYPE);
            ga.loadLocal(seqReceiverVar);
            compiler.pushNewReceiverInfo(ga);
            compileToPush(compiler, documentInstr.getContentExpression());
            compiler.popReceiverInfo();
            ga.loadLocal(seqReceiverVar);
            ga.invokeInstanceMethod(SequenceOutputter.class, "close");
            ga.invokeInstanceMethod(SequenceOutputter.class, "iterate");
        }

    }

}

// Copyright (c) 2011 Saxonica Limited.