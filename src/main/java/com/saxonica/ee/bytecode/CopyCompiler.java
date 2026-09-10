package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.Copy;
import net.sf.saxon.expr.instruct.DummyNamespaceResolver;
import net.sf.saxon.expr.instruct.Instruction;
import net.sf.saxon.expr.instruct.ValidatingInstruction;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.tree.util.NamespaceIterator;
import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.*;
import net.sf.saxon.value.Whitespace;
import org.objectweb.asm.Label;
import org.objectweb.asm.Type;
import org.objectweb.asm.commons.TableSwitchGenerator;

import java.util.ArrayList;
import java.util.List;

/**
 * Generate bytecode for an xsl:copy instruction
 */

public class CopyCompiler extends ToPushCompiler {


    @Override
    public void compileToPush(final CompilerService compiler, Expression expression) throws CannotCompileException {

        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo mi = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        final Copy copyInstr = (Copy) expression;

        ItemType resultItemType = copyInstr.getItemType();
        
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getContextItem");

        final int selectedItemVar = mi.allocateLocal(Item.class);
        ga.storeLocal(selectedItemVar);

        final LabelInfo copyEnd = mi.newLabel("copyEnd");

        if (th.isSubType(resultItemType, NodeKindTest.DOCUMENT)) {
            compileCopyDocument(compiler, copyInstr, selectedItemVar);
        } else if (th.isSubType(resultItemType, NodeKindTest.ELEMENT)) {
            compileCopyElement(compiler, copyInstr, selectedItemVar);
        } else if (th.isSubType(resultItemType, NodeKindTest.ATTRIBUTE)) {
            compileCopyAttribute(compiler, copyInstr, selectedItemVar, copyInstr.isPreservingTypes());
        } else if (th.isSubType(resultItemType, NodeKindTest.TEXT)) {
            compileCopyText(compiler, copyInstr, selectedItemVar);
        } else if (th.isSubType(resultItemType, NodeKindTest.COMMENT)) {
            compileCopyComment(compiler, copyInstr, selectedItemVar);
        } else if (th.isSubType(resultItemType, NodeKindTest.PROCESSING_INSTRUCTION)) {
            compileCopyProcessingInstruction(compiler, copyInstr, selectedItemVar);
        } else if (th.isSubType(resultItemType, NodeKindTest.NAMESPACE)) {
            compileCopyNamespace(compiler, copyInstr, selectedItemVar);
        } else if (th.isSubType(resultItemType, BuiltInAtomicType.ANY_ATOMIC)) {
            compileCopyNonNode(compiler, copyInstr, selectedItemVar);
        } else if (th.isSubType(resultItemType, AnyFunctionType.getInstance())) {
            compileCopyNonNode(compiler, copyInstr, selectedItemVar);
        } else {

            if (th.relationship(resultItemType, BuiltInAtomicType.ANY_ATOMIC) != TypeHierarchy.DISJOINT ||
                    th.relationship(resultItemType, AnyFunctionType.getInstance()) != TypeHierarchy.DISJOINT) {
                LabelInfo copyNode = mi.newLabel("copyNode");
                ga.loadLocal(selectedItemVar);
                ga.ifInstance(NodeInfo.class, copyNode);
                compileCopyNonNode(compiler, copyInstr, selectedItemVar);
                ga.goTo(copyEnd);
                mi.placeLabel(copyNode);
            }

            ga.loadLocal(selectedItemVar);
            ga.checkClass(NodeInfo.class);
            ga.invokeInstanceMethod(NodeInfo.class, "getNodeKind");

            final List<CannotCompileException> errors = new ArrayList<>();

            ga.tableSwitch(
                    new int[]{
                            net.sf.saxon.type.Type.ELEMENT,
                            net.sf.saxon.type.Type.ATTRIBUTE,
                            net.sf.saxon.type.Type.TEXT,
                            net.sf.saxon.type.Type.PROCESSING_INSTRUCTION,
                            net.sf.saxon.type.Type.COMMENT,
                            net.sf.saxon.type.Type.DOCUMENT,
                            net.sf.saxon.type.Type.NAMESPACE},
                    new TableSwitchGenerator() {
                        public void generateCase(int key, Label end) {
                            try {
                                switch (key) {
                                    case net.sf.saxon.type.Type.DOCUMENT:
                                        compileCopyDocument(compiler, copyInstr, selectedItemVar);
                                        ga.goTo(end);
                                        break;
                                    case net.sf.saxon.type.Type.ELEMENT:
                                        compileCopyElement(compiler, copyInstr, selectedItemVar);
                                        ga.goTo(end);
                                        break;
                                    case net.sf.saxon.type.Type.ATTRIBUTE:
                                        compileCopyAttribute(compiler, copyInstr, selectedItemVar, copyInstr.isPreservingTypes());
                                        ga.goTo(end);
                                        break;
                                    case net.sf.saxon.type.Type.TEXT:
                                        compileCopyText(compiler, copyInstr, selectedItemVar);
                                        ga.goTo(end);
                                        break;
                                    case net.sf.saxon.type.Type.COMMENT:
                                        compileCopyComment(compiler, copyInstr, selectedItemVar);
                                        ga.goTo(end);
                                        break;
                                    case net.sf.saxon.type.Type.PROCESSING_INSTRUCTION:
                                        compileCopyProcessingInstruction(compiler, copyInstr, selectedItemVar);
                                        ga.goTo(end);
                                        break;
                                    case net.sf.saxon.type.Type.NAMESPACE:
                                        compileCopyNamespace(compiler, copyInstr, selectedItemVar);
                                        ga.goTo(end);
                                        break;

                                }
                            } catch (CannotCompileException e) {
                                errors.add(e);
                            }
                        }

                        /**
                         * Generates the code for the default switch case.
                         */
                        public void generateDefault() {
                            ga.goTo(copyEnd);
                        }
                    },
                    true // use table switch

            );

            if (!errors.isEmpty()) {
                throw errors.get(0);
            }
        }
        mi.placeLabel(copyEnd);
        mi.releaseLocal(selectedItemVar);

    }

    private void compileCopyDocument(CompilerService compiler, Copy expression, int selectedItemVar) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo mi = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Copy - Document");


        if (expression.isPreservingTypes()) {
            compiler.generateGetReceiver();
            ga.push(0);
            ga.invokeInstanceMethod(Receiver.class, "startDocument", Integer.TYPE);
            compileCopyUnparsedEntities(compiler, selectedItemVar);
            compiler.compileToPush(expression.getContentExpression());
            compiler.generateGetReceiver();
            ga.invokeInstanceMethod(Receiver.class, "endDocument");
        } else {
            compiler.generateGetReceiver();
            ga.invokeInstanceMethod(Receiver.class, "getPipelineConfiguration");
            // assert stack : pipe
//            boolean pop = false;
//            Receiver val = controller.getConfiguration().
//                    getDocumentValidator(out, source.getBaseURI(),
//                            validation, Whitespace.NONE, getSchemaType(), -1);
            allocateStatic(compiler, compiler.getConfiguration());
            compiler.generateGetReceiver();
            ga.loadLocal(selectedItemVar);
            ga.checkClass(NodeInfo.class);
            ga.invokeInstanceMethod(NodeInfo.class, "getBaseURI");

//        ParseOptions options = new ParseOptions();
//        options.setSchemaValidationMode(validation);
//        options.setStripSpace(Whitespace.NONE);
//        options.setTopLevelType(schemaType);

            ga.invokeDefaultConstructor(ParseOptions.class);
            ga.dup();
            ga.push(expression.getValidationAction());
            ga.invokeInstanceMethod(ParseOptions.class, "setSchemaValidationMode", Integer.TYPE);
            ga.dup();
            ga.push(Whitespace.NONE);
            ga.invokeInstanceMethod(ParseOptions.class, "setStripSpace", Integer.TYPE);

            if (expression.getSchemaType() != null) {
                ga.dup();
                allocateStatic(compiler, expression.getSchemaType());
                ga.invokeInstanceMethod(ParseOptions.class, "setTopLevelType", SchemaType.class);
            }
            allocateStatic(compiler, expression.getLocation());
//        call getDocumentValidator (Receiver receiver,
//                String systemId,
//                ParseOptions validationOptions, Location initiatingLocation){

            ga.invokeInstanceMethod(Configuration.class, "getDocumentValidator",
                    Receiver.class, String.class, ParseOptions.class, Location.class);

            // assert stack pipe Receiver(val)
            ga.dup();
            // assert stack pipe Receiver(val) Receiver(val)
            compiler.generateGetReceiver();
            // assert stack pipe Receiver(val) Receiver(val) Receiver(out)
            LabelInfo differentReceiver = mi.newLabel("differentReceiver");
            LabelInfo startDocument = mi.newLabel("startDocument");
            ga.ifNotSameObject(differentReceiver);
            // assert stack pipe Receiver(val)
            ga.swap();
            ga.pop();
            // assert stack Receiver(val)
            ga.goTo(startDocument);

            mi.placeLabel(differentReceiver);
            // assert stack pipe Receiver(val)
//            if (val != out) {
//                SequenceReceiver sr = new TreeReceiver(val);
//                sr.setPipelineConfiguration(out.getPipelineConfiguration());
//                context.setReceiver(sr);
//                pop = true;
//                out = sr;
            ga.newInstance(TreeReceiver.class);
            // assert stack pipe Receiver(val) TreeReceiver
            ga.dupX1();
            // assert stack pipe TreeReceiver Receiver(val) TreeReceiver
            ga.swap();
            // assert stack pipe TreeReceiver TreeReceiver Receiver(val)
            ga.invokeConstructor(TreeReceiver.class, Receiver.class);
            // assert stack pipe TreeReceiver
            ga.swap();
            // assert stack TreeReceiver pipe
            ga.dup2();
            // assert stack TreeReceiver pipe TreeReceiver pipe
            ga.invokeInstanceMethod(Receiver.class, "setPipelineConfiguration", PipelineConfiguration.class);
            // assert stack TreeReceiver pipe
            ga.pop();

            mi.placeLabel(startDocument);
            // assert stack TreeReceiver
            ga.checkClass(SequenceReceiver.class);
            compiler.pushNewReceiverInfo(ga);
            // assert stack (empty)
            compiler.generateGetReceiver();
            // assert stack Receiver

//            out.startDocument(0);
//            content.process(context);
//            out.endDocument();
            ga.push(0);
            ga.invokeInstanceMethod(Receiver.class, "startDocument", Integer.TYPE);
            compileCopyUnparsedEntities(compiler, selectedItemVar);

            compiler.compileToPush(expression.getContentExpression());

            compiler.generateGetReceiver();
            ga.invokeInstanceMethod(Receiver.class, "endDocument");
            compiler.popReceiverInfo();
        }

    }

    private void compileCopyUnparsedEntities(CompilerService compiler, int selectedItemVar) {
        Generator ga = compiler.getCurrentGenerator();
        ga.loadLocal(selectedItemVar);
        compiler.generateGetReceiver();
        ga.invokeStaticMethod(Copy.class, "copyUnparsedEntities", NodeInfo.class, Receiver.class);
    }

    private void compileCopyElement(CompilerService compiler, Copy expression, int selectedItemVar) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Copy - Element");
        Expression content = expression.getContentExpression();
        Location locationId = expression.getLocation();
        int properties = expression.isInheritNamespacesToChildren() ? 0 : ReceiverOptions.DISINHERIT_NAMESPACES;
        if (!expression.isInheritNamespacesFromParent()) {
            properties |= ReceiverOptions.REFUSE_NAMESPACES;
        }

        boolean needsNameCodeVar = true; //!expression.isCopyNamespaces() || !expression.isPreservingTypes();
        int nodeNameVar = ga.newLocal(NodeName.class);
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeStaticMethod(NameOfNode.class, "makeName", NodeInfo.class);
        ga.storeLocal(nodeNameVar);

        if (!expression.isPreservingTypes()) {
//                Receiver validator = context.getConfiguration().getElementValidator(
//                        out, nameCode, locationId, getSchemaType(), validation);
//                if (validator != out) {
//                    context.setReceiver(new TreeReceiver(validator));
//                }

            if (expression.getValidationAction() == Validation.STRIP) {
                // Save existing receiver on stack
                ElementCreatorCompiler.addTypeStripper(compiler);
            } else {

                // ParseOptions options = new ParseOptions();
                // options.setSchemaValidationMode(validation);
                // options.setTopLevelType(schemaType);
                // options.setTopLevelElement(NameOfNode.makeName(source));
                // Configuration.getElementValidator(receiver, options, locationId)
                compiler.generateGetContext();
                ga.invokeInstanceMethod(XPathContext.class, "getConfiguration");
                compiler.generateGetReceiver();

                ga.invokeDefaultConstructor(ParseOptions.class);
                ga.dup();
                ga.push(expression.getValidationAction());
                ga.invokeInstanceMethod(ParseOptions.class, "setSchemaValidationMode", Integer.TYPE);
                if (expression.getSchemaType() != null) {
                    ga.dup();
                    allocateStatic(compiler, expression.getSchemaType());
                    ga.invokeInstanceMethod(ParseOptions.class, "setTopLevelType", SchemaType.class);
                }
                ga.dup();
                ga.loadLocal(selectedItemVar);
                ga.checkClass(NodeInfo.class);
                ga.invokeStaticMethod(Navigator.class, "getNodeName", NodeInfo.class);
                ga.invokeInstanceMethod(ParseOptions.class, "setTopLevelElement", StructuredQName.class);
                allocateStatic(compiler, expression.getLocation());

                ga.invokeInstanceMethod(Configuration.class, "getElementValidator",
                        Receiver.class, ParseOptions.class, Location.class);

                int validatorVar = methodInfo.allocateLocal(Receiver.class);
                ga.storeLocal(validatorVar);

                ga.loadLocal(validatorVar);
                compiler.generateGetReceiver();
                LabelInfo sameReceiver = methodInfo.newLabel("sameReceiver");
                LabelInfo doneReceiver = methodInfo.newLabel("doneReceiver");
                ga.ifSameObject(sameReceiver);
                ga.newInstance(Type.getType(TreeReceiver.class));
                ga.dup();
                ga.loadLocal(validatorVar);
                ga.invokeConstructor(TreeReceiver.class, Receiver.class);
                ga.goTo(doneReceiver);
                methodInfo.placeLabel(sameReceiver);
                compiler.generateGetReceiver();
                methodInfo.placeLabel(doneReceiver);
                compiler.pushNewReceiverInfo(ga);
                methodInfo.releaseLocal(validatorVar);
            }
        }

        LabelInfo nonNullSystemId = methodInfo.newLabel("nonNullSystemId");
        visitAnnotation(compiler, "Copy - Element - Base URI");
        compiler.generateGetReceiver();
        ga.invokeInstanceMethod(Receiver.class, "getSystemId");
        ga.ifNonNull(nonNullSystemId.label());
        compiler.generateGetReceiver();
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getBaseURI");
        ga.invokeInstanceMethod(Receiver.class, "setSystemId", String.class);
        methodInfo.placeLabel(nonNullSystemId);

        compiler.generateGetReceiver();
        visitAnnotation(compiler, "Copy - Element - NameOfNode");
        ga.loadLocal(nodeNameVar);

        SchemaType typeCode =
                expression.getValidationAction() == Validation.PRESERVE ? AnyType.getInstance() : Untyped.getInstance();
        allocateStatic(compiler, typeCode);
        allocateStatic(compiler, locationId);
        ga.push(properties);
        ga.invokeInstanceMethod(Receiver.class, "startElement", NodeName.class, SchemaType.class, Location.class, Integer.TYPE);
        if (expression.isCopyNamespaces()) {
            // NamespaceIterator.sendNamespaces(copiedNode, receiver);
            ga.loadLocal(selectedItemVar);
            compiler.generateGetReceiver();
            ga.invokeStaticMethod(NamespaceIterator.class, "sendNamespaces", NodeInfo.class, Receiver.class);
        } else {
            // receiver.namespace(nodeName.getNamespaceBinding(), 0);
            compiler.generateGetReceiver();
            ga.loadLocal(nodeNameVar);
            ga.invokeInstanceMethod(NodeName.class, "getNamespaceBinding");
            ga.push(0);
            ga.invokeInstanceMethod(Receiver.class, "namespace", NamespaceBindingSet.class, Integer.TYPE);
        }

        compiler.compileToPush(content);

        compiler.generateGetReceiver();
        ga.invokeInstanceMethod(Receiver.class, "endElement");

        if (!expression.isPreservingTypes()) {
            compiler.popReceiverInfo();
        }

        methodInfo.releaseLocal(nodeNameVar);
    }

    protected static void compileCopyAttribute(CompilerService compiler, ValidatingInstruction expression, int selectedItemVar, boolean preserveTypes) {
        Generator ga = compiler.getCurrentGenerator();

        // out.attribute(namecode, typecode, value, locationId, properties)
        GeneratedMethodInfo mi = compiler.getCurrentMethod();
        int nodeNameVar = mi.allocateLocal(NodeName.class);
        visitAnnotation(compiler, "Copy_Attribute");

        LabelInfo nonEmptyFi = mi.newLabel("nonEmptyFi");

        compiler.generateGetReceiver();
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeStaticMethod(NameOfNode.class, "makeName", NodeInfo.class);
        ga.storeLocal(nodeNameVar);
        ga.loadLocal(nodeNameVar);
        SchemaType schemaType = expression.getSchemaType();
        if (schemaType != null) {
            if (schemaType.isSimpleType()) {
                visitAnnotation(compiler, "Copy_Attribute_withType");
                if (((SimpleType) schemaType).isNamespaceSensitive()) {
                    compiler.generateDynamicError("Cannot create a parentless attribute whose " +
                            "type is namespace-sensitive (such as xs:QName)", "XTTE1545", ((Expression)expression).getLocation(), true);
                } else {

                    allocateStatic(compiler, schemaType);
                    ga.loadLocal(selectedItemVar);
                    ga.invokeInstanceMethod(Item.class, "getStringValueCS");
                    allocateStatic(compiler, DummyNamespaceResolver.getInstance());
                    allocateStatic(compiler, compiler.getConfiguration().getConversionRules());
                    ga.invokeInstanceMethod(SimpleType.class, "validateContent", CharSequence.class, NamespaceResolver.class, ConversionRules.class);
                    LabelInfo validationOk = mi.newLabel("validationOk");
                    ga.dup();
                    // assert stack: err err
                    ga.ifNull(validationOk.label());
                    // assert stack: err
                    ga.dup();
                    // assert stack: err err
                    ga.dup();
                    // assert stack: err err err
                    ga.invokeInstanceMethod(ValidationFailure.class, "getMessage");
                    // assert stack: err err string
                    ga.push("Attribute being copied does not match the required type. ");
                    // assert stack: err err string string
                    ga.swap();
                    ga.concatenateStrings(2);
                    // assert stack: err err string
                    ga.invokeInstanceMethod(ValidationFailure.class, "setMessage", String.class);
                    // assert stack: err
                    ga.invokeInstanceMethod(ValidationFailure.class, "makeException");

                    ga.throwException();

                    mi.placeLabel(validationOk);
                    ga.pop();
                    allocateStatic(compiler, schemaType);
                    ga.loadLocal(selectedItemVar);
                    ga.invokeInstanceMethod(Item.class, "getStringValueCS");
                    allocateStatic(compiler, ((Expression) expression).getLocation());
                    ga.push(0);
                    ga.invokeInstanceMethod(Receiver.class, "attribute", NodeName.class, SimpleType.class, CharSequence.class, Location.class, Integer.TYPE);
                }
            } else {
                compiler.generateDynamicError("Cannot validate an attribute against a complex type",
                    "XTTE1535", ((Expression) expression).getLocation(), true);
            }
            return;
        }
        if (preserveTypes) {
            ga.loadLocal(selectedItemVar);
            ga.checkClass(NodeInfo.class);
            ga.invokeInstanceMethod(NodeInfo.class, "getSchemaType");
            ga.dup();
            LabelInfo notSensitive = mi.newLabel("notSensitive");
            ga.getStaticField(BuiltInAtomicType.class, "UNTYPED_ATOMIC", BuiltInAtomicType.class);
            ga.invokeInstanceMethod(Object.class, "equals", Object.class);
            ga.ifTrue(notSensitive);
            ga.dup();
            ga.checkClass(SimpleType.class);
            ga.invokeInstanceMethod(SimpleType.class, "isNamespaceSensitive");
            ga.ifFalse(notSensitive);
            compiler.generateDynamicError("Cannot preserve type annotation when copying an attribute with namespace-sensitive content",
                                          ((Instruction) expression).getPackageData().getHostLanguage() == Configuration.XSLT ? "XTTE0950" : "XQTY0086",
                                          ((Expression) expression).getLocation(), true);
            mi.placeLabel(notSensitive);
            ga.checkClass(SimpleType.class);
        } else {
            ga.getStaticField(BuiltInAtomicType.class, "UNTYPED_ATOMIC", BuiltInAtomicType.class);
        }
        ga.loadLocal(selectedItemVar);
        ga.invokeInstanceMethod(Item.class, "getStringValueCS");
        FixedAttributeCompiler.generateAttributeValidationPushCode(compiler, expression, null, nodeNameVar);
        allocateStatic(compiler, ((Expression) expression).getLocation());
        ga.push(0);
        ga.invokeInstanceMethod(Receiver.class, "attribute", NodeName.class, SimpleType.class, CharSequence.class, Location.class, Integer.TYPE);

    }

    protected static void compileCopyNamespace(CompilerService compiler, Expression expression, int selectedItemVar) {
        // source.copy(out, 0, locationId);
        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "Copy - Namespace");
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        compiler.generateGetReceiver();
        ga.push(0);
        allocateStatic(compiler, expression.getLocation());
        ga.invokeInstanceMethod(NodeInfo.class, "copy", Receiver.class, Integer.TYPE, Location.class);
    }

    protected static void compileCopyText(CompilerService compiler, Expression expression, int selectedItemVar) {
        // out.characters(source.getStringValueCS(), locationId, 0);
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo mi = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Copy - Text");
        compiler.generateGetReceiver();
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getStringValueCS");
        LabelInfo nonEmptyFi = mi.newLabel("nonEmptyFi");
        allocateStatic(compiler, expression.getLocation());
        ga.push(0);
        ga.invokeInstanceMethod(Receiver.class, "characters", CharSequence.class, Location.class, Integer.TYPE);

    }

    protected static void compileCopyComment(CompilerService compiler, Expression expression, int selectedItemVar) {
        // out.comment(source.getStringValueCS(), locationId, 0);
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo mi = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Copy - Comment");
        compiler.generateGetReceiver();
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getStringValueCS");
        allocateStatic(compiler, expression.getLocation());
        ga.push(0);
        ga.invokeInstanceMethod(Receiver.class, "comment", CharSequence.class, Location.class, Integer.TYPE);

    }

    protected static void compileCopyProcessingInstruction(CompilerService compiler, Expression expression, int selectedItemVar) {
        // out.processingInstruction(source.getDisplayName(), source.getStringValueCS(), locationId, 0);
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo mi = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Copy - PI");
        compiler.generateGetReceiver();
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getDisplayName");
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getStringValueCS");
        LabelInfo nonEmptyFi = mi.newLabel("nonEmptyFi");
        allocateStatic(compiler, expression.getLocation());
        ga.push(0);
        ga.invokeInstanceMethod(Receiver.class, "processingInstruction", String.class, CharSequence.class, Location.class, Integer.TYPE);

    }

    protected static void compileCopyNonNode(CompilerService compiler, Expression expression, int selectedItemVar) {
        // out.append(item, locationId, NodeInfo.ALL_NAMESPACES);
        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "Copy_NonNode");
        compiler.generateGetReceiver();
        ga.loadLocal(selectedItemVar);
        allocateStatic(compiler, expression.getLocation());
        ga.push(ReceiverOptions.ALL_NAMESPACES);
        ga.invokeInstanceMethod(Receiver.class, "append", Item.class, Location.class, Integer.TYPE);
    }

}

// Copyright (c) 2011 Saxonica Limited.