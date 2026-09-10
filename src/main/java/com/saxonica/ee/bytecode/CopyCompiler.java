package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.ComplexContentOutputter;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.Copy;
import net.sf.saxon.expr.instruct.DummyNamespaceResolver;
import net.sf.saxon.expr.instruct.Instruction;
import net.sf.saxon.expr.instruct.ValidatingInstruction;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.type.*;
import org.objectweb.asm.Label;
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

        LabelInfo contextItemNotNull = mi.newLabel("contextItemNotNull");
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getContextItem");
        
        ga.dup();
        ga.ifNonNull(contextItemNotNull.label());
        compiler.generateDynamicError("Context item for xsl:copy is null",
                                      "XTTE0945", expression.getLocation(), true);
        ga.pop();
        mi.placeLabel(contextItemNotNull);

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

            if (th.relationship(resultItemType, BuiltInAtomicType.ANY_ATOMIC) != Affinity.DISJOINT ||
                    th.relationship(resultItemType, AnyFunctionType.getInstance()) != Affinity.DISJOINT) {
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
                        @Override
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
                                    case 123:
                                        throw new CannotCompileException();

                                }
                            } catch (CannotCompileException e) {
                                errors.add(e);
                            }
                        }

                        /**
                         * Generates the code for the default switch case.
                         */
                        @Override
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
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Copy - Document");

//        if (!preservingTypes) {
//            ParseOptions options = new ParseOptions(getValidationOptions());
//            options.setSpaceStrippingRule(NoElementsSpaceStrippingRule.getInstance());
//            controller.getConfiguration().prepareValidationReporting(context, options);
//            Receiver val = controller.getConfiguration().
//                    getDocumentValidator(out, source.getBaseURI(), options, getLocation());
//            out = new ComplexContentOutputter(val);
//        }
//        if (out.getSystemId() == null) {
//            out.setSystemId(source.getBaseURI());
//        }
//        out.startDocument(ReceiverOption.NONE);
//        copyUnparsedEntities(source, out);
//        getContentExpression().process(out, context);
//        out.endDocument();


        if (!expression.isPreservingTypes()) {
            ga.newInstance(ParseOptions.class);
            //-- ParseOptions
            ga.dup();
            //-- ParseOptions ParseOptions
            allocateStatic(compiler, expression.getValidationOptions());
            //-- ParseOptions ParseOptions ValidationOptions
            ga.invokeConstructor(ParseOptions.class, ParseOptions.class);
            //-- ParseOptions
            int optionsVar = ga.newLocal(ParseOptions.class);
            ga.storeLocal(optionsVar);
            ga.loadLocal(optionsVar);
            //-- ParseOptions
            allocateStatic(compiler, NoElementsSpaceStrippingRule.getInstance());
            //-- ParseOptions SpaceStrippingRule
            ga.invokeInstanceMethod(ParseOptions.class,"setSpaceStrippingRule", SpaceStrippingRule.class);
            compiler.generateGetContext();
            //-- context
            ga.dup();
            //-- context
            ga.invokeInstanceMethod(XPathContext.class, "getConfiguration");
            //-- context config
            ga.swap();
            //-- config context
            ga.loadLocal(optionsVar);
            //-- config context options
            // TODO: this calls a method that doesn't actually do anything...
            ga.invokeInstanceMethod(Configuration.class, "prepareValidationReporting", XPathContext.class, ParseOptions.class);
            //Receiver val = controller.getConfiguration().
            //          getDocumentValidator(out, source.getBaseURI(), options, getLocation());
            compiler.generateGetContext();
            //-- context
            ga.invokeInstanceMethod(XPathContext.class, "getConfiguration");
            //-- config
            compiler.generateGetOutputter();
            //-- config out
            ga.loadLocal(selectedItemVar);
            //-- config out source
            ga.invokeInstanceMethod(NodeInfo.class, "getBaseURI" );
            //-- config out baseURI
            ga.loadLocal(optionsVar);
            //-- config out baseURI options
            allocateStatic(compiler, expression.getLocation());
            //-- config out baseURI options location
            ga.invokeInstanceMethod(Configuration.class, "getDocumentValidator",
                                    Receiver.class, String.class, ParseOptions.class, Location.class);
            //-- receiver
            ga.newInstance(ComplexContentOutputter.class);
            //-- receiver CCO
            ga.dupX1();
            //-- CCO receiver CCO
            ga.swap();
            //-- CCO CCO receiver
            ga.invokeConstructor(ComplexContentOutputter.class, Receiver.class);
            //-- CCO
            compiler.pushNewOutputterInfo(ga);
            
        }

//        if (out.getSystemId() == null) {
////            out.setSystemId(source.getBaseURI());
////        }

        LabelInfo baseUriOk = methodInfo.newLabel("baseUriOk");
        compiler.generateGetOutputter();
        ga.invokeInstanceMethod(Outputter.class, "getSystemId");
        ga.ifNonNull(baseUriOk.label());
        compiler.generateGetOutputter();
        ga.loadLocal(selectedItemVar);
        ga.invokeInstanceMethod(NodeInfo.class, "getBaseURI");
        ga.invokeInstanceMethod(Outputter.class, "setSystemId", String.class);
        methodInfo.placeLabel(baseUriOk);
        
        compiler.generateGetOutputter();
        ga.push(ReceiverOption.NONE);
        ga.invokeInstanceMethod(Outputter.class, "startDocument", Integer.TYPE);
        compileCopyUnparsedEntities(compiler, selectedItemVar);
        compiler.compileToPush(expression.getContentExpression());
        compiler.generateGetOutputter();
        ga.invokeInstanceMethod(Outputter.class, "endDocument");
            
        if (!expression.isPreservingTypes()) {
            compiler.popOutputterInfo();
        }

    }

    private void compileCopyUnparsedEntities(CompilerService compiler, int selectedItemVar) {
        Generator ga = compiler.getCurrentGenerator();
        ga.loadLocal(selectedItemVar);
        compiler.generateGetOutputter();
        ga.invokeStaticMethod(Copy.class, "copyUnparsedEntities", NodeInfo.class, Outputter.class);
    }

    private void compileCopyElement(CompilerService compiler, Copy expression, int selectedItemVar) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Copy - Element");
        Expression content = expression.getContentExpression();
        Location locationId = expression.getLocation();

        int properties = ReceiverOption.NONE;
        if (!expression.isBequeathNamespacesToChildren()) {
            properties |= ReceiverOption.DISINHERIT_NAMESPACES;
        }
        if (!expression.isInheritNamespacesFromParent()) {
            properties |= ReceiverOption.REFUSE_NAMESPACES;
        }
        properties |= ReceiverOption.ALL_NAMESPACES;

        int nodeNameVar = ga.newLocal(NodeName.class);
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeStaticMethod(NameOfNode.class, "makeName", NodeInfo.class);
        ga.storeLocal(nodeNameVar);

        int elemOutVar = ga.newLocal(Receiver.class);
        compiler.generateGetOutputter();
        ga.storeLocal(elemOutVar);

        if (!expression.isPreservingTypes()) {
//            ParseOptions options = new ParseOptions(getValidationOptions());
//            options.setTopLevelElement(elemName.getStructuredQName());
//            context.getConfiguration().prepareValidationReporting(context, options);
//            Receiver validator = context.getConfiguration().getElementValidator(elemOut, options, getLocation());
//
//            if (validator != elemOut) {
//                out = new ComplexContentOutputter(validator);
//            }
//            elemOut = validator;


            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getConfiguration");
            //-- config
            compiler.generateGetOutputter();
            //-- config outputter
            ga.newInstance(ParseOptions.class);
            ga.dup();
            allocateStatic(compiler, expression.getValidationOptions());
            //-- config outputter options options validationOptions
            ga.invokeConstructor(ParseOptions.class, ParseOptions.class);
            //-- config outputter options
            ga.dup();
            //-- config outputter options options
            ga.loadLocal(nodeNameVar);
            //-- config outputter options options nodename
            ga.invokeInstanceMethod(NodeName.class, "getStructuredQName");
            //-- config outputter options options qname
            ga.invokeInstanceMethod(ParseOptions.class, "setTopLevelElement", StructuredQName.class);
            allocateStatic(compiler, locationId);
            //-- config outputter options location
            ga.invokeInstanceMethod(Configuration.class, "getElementValidator",
                                    Receiver.class, ParseOptions.class, Location.class);
            //-- validator
            int validatorVar = methodInfo.allocateLocal(Receiver.class);
            ga.storeLocal(validatorVar);
            //-- (empty)

            ga.loadLocal(validatorVar);
            ga.loadLocal(elemOutVar);
            LabelInfo sameReceiver = methodInfo.newLabel("sameReceiver");
            ga.ifSameObject(sameReceiver);
            //-- (empty)
            ga.newInstance(ComplexContentOutputter.class);
            ga.dup();
            ga.loadLocal(validatorVar);
            ga.invokeConstructor(ComplexContentOutputter.class, Receiver.class);
            ga.storeLocal(elemOutVar);
            methodInfo.placeLabel(sameReceiver);
            //-- (empty)
            ga.loadLocal(elemOutVar);
            compiler.pushNewOutputterInfo(ga);
        }

        LabelInfo nonNullSystemId = methodInfo.newLabel("nonNullSystemId");
        visitAnnotation(compiler, "Copy - Element - Base URI");
        compiler.generateGetOutputter();
        ga.invokeInstanceMethod(Outputter.class, "getSystemId");
        ga.ifNonNull(nonNullSystemId.label());
        compiler.generateGetOutputter();
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getBaseURI");
        ga.invokeInstanceMethod(Outputter.class, "setSystemId", String.class);
        methodInfo.placeLabel(nonNullSystemId);

        compiler.generateGetOutputter();
        visitAnnotation(compiler, "Copy - Element - NameOfNode");
        ga.loadLocal(nodeNameVar);

        SchemaType typeCode =
                expression.getValidationAction() == Validation.PRESERVE ? AnyType.getInstance() : Untyped.getInstance();
        allocateStatic(compiler, typeCode);
        allocateStatic(compiler, locationId);
        ga.push(properties);
        ga.invokeInstanceMethod(Outputter.class, "startElement", NodeName.class, SchemaType.class, Location.class, Integer.TYPE);
        if (expression.isCopyNamespaces()) {
//            receiver.namespaces(copiedNode.getAllNamespaces(), ReceiverOption.NAMESPACE_OK);
            compiler.generateGetOutputter();
            ga.loadLocal(selectedItemVar);
            ga.invokeInstanceMethod(NodeInfo.class, "getAllNamespaces");
            ga.push(ReceiverOption.NONE);
            ga.invokeInstanceMethod(Outputter.class, "namespaces", NamespaceBindingSet.class, Integer.TYPE);
        } else {
//            NamespaceBinding ns = nodeName.getNamespaceBinding();
//            if (!ns.isDefaultUndeclaration()) {
//                receiver.namespace(ns.getPrefix(), ns.getURI(), ReceiverOption.NONE);
//            }
            compiler.generateGetOutputter();
            //-- out
            LabelInfo defaultUndecl = methodInfo.newLabel("defaultUndecl");
            LabelInfo fi = methodInfo.newLabel("fi");
            ga.loadLocal(nodeNameVar);
            //-- out name
            ga.invokeInstanceMethod(NodeName.class, "getNamespaceBinding");
            //-- out binding
            ga.dup();
            //-- out binding binding
            ga.invokeInstanceMethod(NamespaceBinding.class, "isDefaultUndeclaration");
            //-- out binding boolean
            ga.ifTrue(defaultUndecl);
            //-- out binding
            ga.push(ReceiverOption.NONE);
            //-- out binding int
            ga.invokeInstanceMethod(Outputter.class, "namespaces", NamespaceBindingSet.class, Integer.TYPE);
            //-- (empty)
            ga.goTo(fi);
            methodInfo.placeLabel(defaultUndecl);
            //-- out binding
            ga.pop2();
            //-- (empty)
            methodInfo.placeLabel(fi);
        }

        compiler.compileToPush(content);

        compiler.generateGetOutputter();
        ga.invokeInstanceMethod(Outputter.class, "endElement");

        if (!expression.isPreservingTypes()) {
            compiler.popOutputterInfo();
        }

        methodInfo.releaseLocal(nodeNameVar);
    }

    private static void compileCopyAttribute(CompilerService compiler, ValidatingInstruction expression, int selectedItemVar, boolean preserveTypes) {
        Generator ga = compiler.getCurrentGenerator();

        // out.attribute(namecode, typecode, value, locationId, properties)
        GeneratedMethodInfo mi = compiler.getCurrentMethod();
        int nodeNameVar = mi.allocateLocal(NodeName.class);
        visitAnnotation(compiler, "Copy_Attribute");

        compiler.generateGetOutputter();
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
                    ga.invokeInstanceMethod(Item.class, "getUnicodeStringValue");
                    allocateStatic(compiler, DummyNamespaceResolver.getInstance());
                    allocateStatic(compiler, compiler.getConfiguration().getConversionRules());
                    ga.invokeInstanceMethod(SimpleType.class, "validateContent", UnicodeString.class, NamespaceResolver.class, ConversionRules.class);
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
                    ga.invokeInstanceMethod(Item.class, "getStringValue");
                    allocateStatic(compiler, ((Expression) expression).getLocation());
                    ga.push(ReceiverOption.NONE);
                    ga.invokeInstanceMethod(Outputter.class, "attribute", NodeName.class, SimpleType.class,
                                            String.class, Location.class, Integer.TYPE);
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
                                          ((Instruction) expression).getPackageData().isXSLT() ? "XTTE0950" : "XQTY0086",
                                          ((Expression) expression).getLocation(), true);
            mi.placeLabel(notSensitive);
            ga.checkClass(SimpleType.class);
        } else {
            ga.getStaticField(BuiltInAtomicType.class, "UNTYPED_ATOMIC", BuiltInAtomicType.class);
        }
        ga.loadLocal(selectedItemVar);
        ga.invokeInstanceMethod(Item.class, "getUnicodeStringValue");
        FixedAttributeCompiler.generateAttributeValidationPushCode(compiler, expression, null, nodeNameVar);
        ga.invokeInstanceMethod(UnicodeString.class, "toString" );
        allocateStatic(compiler, ((Expression) expression).getLocation());
        ga.push(ReceiverOption.NONE);
        ga.invokeInstanceMethod(Outputter.class, "attribute", NodeName.class, SimpleType.class,
                                String.class, Location.class, Integer.TYPE);

    }

    protected static void compileCopyNamespace(CompilerService compiler, Expression expression, int selectedItemVar) {

        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "Copy - Namespace");
//        ga.loadLocal(selectedItemVar);
//        ga.checkClass(NodeInfo.class);
        compiler.generateGetOutputter();
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.dup();
        // Outputter NodeInfo NodeInfo
        ga.invokeInstanceMethod(NodeInfo.class, "getLocalPart");
        // Outputter NodeInfo String
        ga.swap();
        // Outputter String NodeInfo
        ga.invokeInstanceMethod(NodeInfo.class, "getStringValue");
        // Outputter String String
        ga.invokeStaticMethod(NamespaceUri.class, "of", String.class);
        // Outputter String NamespaceUri
        ga.push(ReceiverOption.NONE);
        // Outputter String NamespaceUri int
        ga.invokeInstanceMethod(Outputter.class, "namespace", String.class, NamespaceUri.class, Integer.TYPE);


    }

    protected static void compileCopyText(CompilerService compiler, Expression expression, int selectedItemVar) {
        // out.characters(source.getStringValueCS(), locationId, 0);
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo mi = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Copy - Text");
        compiler.generateGetOutputter();
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getUnicodeStringValue");
        LabelInfo nonEmptyFi = mi.newLabel("nonEmptyFi");
        allocateStatic(compiler, expression.getLocation());
        ga.push(ReceiverOption.NONE);
        ga.invokeInstanceMethod(Outputter.class, "characters", UnicodeString.class, Location.class, Integer.TYPE);

    }

    private static void compileCopyComment(CompilerService compiler, Expression expression, int selectedItemVar) {
        // out.comment(source.getStringValueCS(), locationId, 0);
        final Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "Copy - Comment");
        compiler.generateGetOutputter();
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getUnicodeStringValue");
        allocateStatic(compiler, expression.getLocation());
        ga.push(ReceiverOption.NONE);
        ga.invokeInstanceMethod(Outputter.class, "comment", UnicodeString.class, Location.class, Integer.TYPE);

    }

    private static void compileCopyProcessingInstruction(CompilerService compiler, Expression expression, int selectedItemVar) {
        // out.processingInstruction(source.getDisplayName(), source.getStringValueCS(), locationId, 0);
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo mi = compiler.getCurrentMethod();
        visitAnnotation(compiler, "Copy - PI");
        compiler.generateGetOutputter();
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getDisplayName");
        ga.loadLocal(selectedItemVar);
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getUnicodeStringValue");
        allocateStatic(compiler, expression.getLocation());
        ga.push(ReceiverOption.NONE);
        ga.invokeInstanceMethod(Outputter.class, "processingInstruction", String.class, UnicodeString.class, Location.class, Integer.TYPE);

    }

    private static void compileCopyNonNode(CompilerService compiler, Expression expression, int selectedItemVar) {
        // out.append(item, locationId, NodeInfo.ALL_NAMESPACES);
        Generator ga = compiler.getCurrentGenerator();
        visitAnnotation(compiler, "Copy_NonNode");
        compiler.generateGetOutputter();
        ga.loadLocal(selectedItemVar);
        allocateStatic(compiler, expression.getLocation());
        ga.push(ReceiverOption.ALL_NAMESPACES);
        ga.invokeInstanceMethod(Outputter.class, "append", Item.class, Location.class, Integer.TYPE);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited