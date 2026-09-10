package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.DocumentInstr;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeBuilder;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.iter.EmptyIterator;
import net.sf.saxon.tree.iter.SingletonIterator;
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
            compiler.generateGetOutputter();
            ga.push(ReceiverOption.NONE);
            ga.invokeInstanceMethod(Outputter.class, "startDocument", Integer.TYPE);
            compiler.compileToPush(instr.getContentExpression());
            compiler.generateGetOutputter();
            ga.invokeInstanceMethod(Outputter.class, "endDocument");
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
            compiler.generateGetOutputter();
            ga.swap();
            allocateStatic(compiler, instr.getLocation());
            ga.push(ReceiverOption.ALL_NAMESPACES);
            ga.invokeInstanceMethod(Outputter.class, "append", Item.class, Location.class, Integer.TYPE);
            methodInfo.placeLabel(docDone);
        }
    }

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "DocumentInstrCompiler-Item");
        visitLineNumber(compiler, ga, expression);
        final DocumentInstr instr = (DocumentInstr) expression;

        // Note, don't bother with lazy construction - it's semi-obsolete

        if (instr.isTextOnly()) {
//            CharSequence textValue;
//            ga.newInstance(TextFragmentValue.class);
//            ga.dup();
            allocateStatic(compiler, compiler.getConfiguration());
            // Stack: config
            if (instr.getConstantText() != null) {
//                textValue = constantText;
                allocateStatic(compiler, instr.getConstantText());
                // Stack: config UniString
            } else {
//                StringBuilder sb = new StringBuilder(StringBuilder.SMALL);
//                SequenceIterator iter = content.iterate(context);
                //int fsbVar = methodInfo.allocateLocal(UnicodeBuilder.class);

                ga.newInstance(UnicodeBuilder.class);
                ga.dup();
                ga.invokeConstructor(UnicodeBuilder.class);
                //ga.storeLocal(fsbVar);
                // Stack: config UniBuilder
                IteratorDetails contentIter = compiler.compileToIterator(instr.getContentExpression());
                int iterVar = methodInfo.allocateLocal(contentIter.iteratorClass);
                ga.storeLocal(iterVar);

                LabelInfo loopDone = methodInfo.newLabel("docLoopDone");
                LabelInfo loop = methodInfo.placeNewLabel("docLoop");
                // Stack: config UniBuilder
//                while (true) {
//                    Item item = iter.next();
//                    if (item==null) break;
//                    sb.append(item.getStringValueCS());
//                }
                ga.loadLocal(iterVar);
                ga.invokeInstanceMethod(contentIter.iteratorClass, "next");
                // Stack: config UniBuilder Item
                ga.dup();
                ga.ifNull(loopDone.label());
                // Stack: config UniBuilder Item
                ga.invokeInstanceMethod(Item.class, "getUnicodeStringValue");
                // Stack: config UniBuilder UniString
                ga.invokeInstanceMethod(UnicodeBuilder.class, "append", UnicodeString.class);
                // Stack: config UniBuilder
                ga.goTo(loop);

                methodInfo.placeLabel(loopDone);
                // Stack: config UniBuilder Item
                ga.pop();
                // Stack: config UniBuilder
//                textValue = sb.condense();

                methodInfo.releaseLocal(iterVar);
                ga.invokeInstanceMethod(UnicodeBuilder.class, "toUnicodeString");
                // Stack: config UniString
            }
//            root = TextFragmentValue.makeTextFragment(config, textValue, getBaseURI());
            ga.push(instr.getStaticBaseURIString());
            // Stack: config UniString String
            ga.invokeStaticMethod(TextFragmentValue.class, "makeTextFragment", Configuration.class, UnicodeString.class, String.class);
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
            allocateStatic(compiler, instr.getPackageData().getHostLanguage());
            ga.invokeInstanceMethod(PipelineConfiguration.class, "setHostLanguage", HostLanguage.class);
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

            // assert stack : Builder ComplexContentOutputter

//            compiler.generateGetContext();
//
//            // assert stack : Builder ComplexContentOutputter XPathContext
//
//            ga.swap();
//
//            // assert stack : Builder XPathContext ComplexContentOutputter
//            ga.invokeInstanceMethod(XPathContext.class, "setReceiver", ComplexContentOutputter.class);

            // assert stack: Builder

//            compiler.generateGetContext();
//            ga.invokeInstanceMethod(XPathContext.class, "getReceiver");
            compiler.pushNewOutputterInfo(ga);
            // assert stack: Builder

//            Receiver out = context.getReceiver();
//            out.open();
//            out.startDocument(0);

            compiler.generateGetOutputter();
            ga.invokeInstanceMethod(Outputter.class, "open");
            compiler.generateGetOutputter();
            ga.push(ReceiverOption.NONE);
            ga.invokeInstanceMethod(Outputter.class, "startDocument", Integer.TYPE);
//
//            content.process(context);
            compiler.compileToPush(instr.getContentExpression());
//
//            out.endDocument();
//            out.close();
            compiler.generateGetOutputter();
            ga.invokeInstanceMethod(Outputter.class, "endDocument");
            compiler.generateGetOutputter();
            ga.invokeInstanceMethod(Outputter.class, "close");

            compiler.popOutputterInfo();
//            context.setReceiver(saved);

            // assert stack: Builder 
//            root = (DocumentInfo)builder.getCurrentRoot();
            ga.invokeInstanceMethod(Builder.class, "getCurrentRoot");
        }
        return NodeInfo.class;

    }

    @Override
    public IteratorDetails compileToIterator(CompilerService compiler, Expression expression) throws CannotCompileException {
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
            return new IteratorDetails(SequenceIterator.class, NodeInfo.class);
        } else if ((m & DocumentInstr.ITERATE_METHOD) != 0) {
            ga.throwException(Type.getType(AssertionError.class), "iterate() is not implemented in the subclass ");
            return null;
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
            int seqCollectorVar = methodInfo.allocateLocal(SequenceCollector.class);
            ga.storeLocal(seqCollectorVar);
            ga.loadLocal(seqCollectorVar);
            ga.invokeInstanceMethod(SequenceCollector.class, "getPipelineConfiguration");
            allocateStatic(compiler, documentInstr.getPackageData().getHostLanguage());
            ga.invokeInstanceMethod(PipelineConfiguration.class, "setHostLanguage", HostLanguage.class);
            ga.loadLocal(seqCollectorVar);
            compiler.pushNewOutputterInfo(ga);
            compileToPush(compiler, documentInstr.getContentExpression());
            compiler.popOutputterInfo();
            ga.loadLocal(seqCollectorVar);
            ga.invokeInstanceMethod(SequenceCollector.class, "close");
            ga.loadLocal(seqCollectorVar);
            ga.invokeInstanceMethod(SequenceCollector.class, "iterate");
            return new IteratorDetails(SequenceIterator.class, NodeInfo.class);
        }

    }

}

// Copyright (c) 2011-2022 Saxonica Limited