package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.AttributeCreator;
import net.sf.saxon.expr.instruct.DummyNamespaceResolver;
import net.sf.saxon.expr.instruct.FixedAttribute;
import net.sf.saxon.expr.instruct.ValidatingInstruction;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.value.Whitespace;

/**
 * Generate bytecode for a fixed attribute constructor
 */

public class FixedAttributeCompiler extends ToPushCompiler {


    @Override
    public void compileToPush(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        FixedAttribute instr = (FixedAttribute) expression;
        Expression select = instr.getSelect();
        visitLineNumber(compiler, ga, expression);
        NodeName nodeName = instr.evaluateNodeName(null);
        int opt = instr.getOptions();
        Location locationId = instr.getLocation();

        // prepare the call on Outputter.attribute()
        compiler.generateGetOutputter();
        // Stack: Outputter
        allocateStatic(compiler, nodeName);
        // Stack: Outputter nodeName
        SimpleType schemaType = instr.getSchemaType();
        if (schemaType != null) {
            allocateStatic(compiler, schemaType);
        } else {
            ga.getStaticField(BuiltInAtomicType.class, "UNTYPED_ATOMIC", BuiltInAtomicType.class);
        }
        // Stack: Outputter nodeName schemaType

        // push the content onto the stack
        compiler.compileToPrimitive(select, UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);

        // Stack: Outputter nodeName schemaType UniString
        generateAttributeValidationPushCode(compiler, instr, nodeName, -1);
        // Stack: Outputter nodeName schemaType UniString
        ga.invokeInstanceMethod(UnicodeString.class, "toString");
        // Stack: Outputter nodeName schemaType String
        if (nodeName.getFingerprint() == StandardNames.XML_ID) {
            ga.invokeStaticMethod(Whitespace.class, "collapseWhitespace", String.class);
        }

        // Complete the call on receiver.attribute()
        allocateStatic(compiler, locationId);
        // Stack: Outputter nodeName schemaType String
        ga.push(opt);
        // Stack: Outputter nodeName schemaType String int
        visitAnnotation(compiler, "FixedAttributeCompiler - call attribute()");
        ga.invokeInstanceMethod(Outputter.class, "attribute", NodeName.class, SimpleType.class, String.class, Location.class, Integer.TYPE);
    }

    /**
     * Generate code to validate the contents of the attribute against schema definitions.
     * Both on entry and exit, the bytecode stack holds: SequenceReceiver(out), NodeName(nodeName) int(annotation),
     * UnicodeString(value)
     * As well as validating the value, the method may modify the type annotation that is held on the stack.
     *
     * @param compiler    the compiler service
     * @param instr       the instruction (fixed or computed attribute)
     * @param nodeName    the (fixed) nodeName of the attribute, or null if the name is computed
     * @param nodeNameVar the slot number of the variable holding the computed node name, or -1 if the name is fixed
     */

    protected static void generateAttributeValidationPushCode(
        CompilerService compiler, ValidatingInstruction instr, NodeName nodeName, int nodeNameVar) {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        if (instr.getSchemaType() != null) {
            // test whether the value actually conforms to the given type

//            ValidationFailure err = schemaType.validateContent(
//                    value, DummyNamespaceResolver.getInstance(), context.getConfiguration().getConversionRules());
            ga.dup();
            allocateStatic(compiler, instr.getSchemaType());
            ga.swap();
            allocateStatic(compiler, DummyNamespaceResolver.getInstance());
            allocateStatic(compiler, compiler.getConfiguration().getConversionRules());
            ga.invokeInstanceMethod(SimpleType.class, "validateContent", UnicodeString.class, NamespaceResolver.class, ConversionRules.class);

            LabelInfo valFail = methodInfo.newLabel("valFail");
            LabelInfo valSuccess = methodInfo.newLabel("valSuccess");
            ga.ifNonNull(valFail.label());
            ga.goTo(valSuccess.label());

            methodInfo.placeLabel(valFail);

            compiler.generateDynamicError("Invalid attribute", "XTTE1540", ((Expression)instr).getLocation(), true);
            // TODO: should be a ValidationException, get the message right

//            if (err != null) {
//                ValidationException ve = new ValidationException(
//                        "Attribute value " + Err.wrap(value, Err.VALUE) +
//                                           " does not match the required type " +
//                                           schemaType.getDescription() + ". " +
//                                           err.getMessage());
//                ve.setErrorCode("XTTE1540");
//                throw ve;
//            }
            methodInfo.placeLabel(valSuccess);
            ga.showMessage(compiler, "Attribute validation succeeded");

        } else if (instr.getValidationAction() == Validation.STRICT ||
                instr.getValidationAction() == Validation.LAX) {
//            try {
//                ann = context.getConfiguration().validateAttribute(nameCode, value, validationAction);
//            } catch (ValidationException e) {
//                    ....
//            }
            // assert stack: out nodeName ann value
            ga.dup();
            // assert stack: out nodeName ann value value
            allocateStatic(compiler, compiler.getConfiguration());
            // assert stack: out nodeName ann value value config
            ga.swap();
            // assert stack: out nodeName ann value config value
            if (nodeName == null) {
                ga.loadLocal(nodeNameVar);
                //allocateStatic(compiler, compiler.getConfiguration().getNamePool());
                //ga.invokeInstanceMethod(NodeName.class, "allocateNameCode", NamePool.class);
            } else {
                allocateStatic(compiler, nodeName);
            }
            ga.invokeInstanceMethod(NodeName.class, "getStructuredQName");
            // assert stack: out nodeName ann value config value sQName
            ga.swap();
            // assert stack: out nodeName ann value config sQName value
            ga.push(instr.getValidationAction());
            // assert stack: out nodeName ann value config sQName value validation

            LabelInfo L0 = methodInfo.newLabel("L0");
            LabelInfo L1 = methodInfo.newLabel("L1");
            LabelInfo L2 = methodInfo.newLabel("L2");
            ga.visitTryCatchBlock(L0, L1, L2, "net/sf/saxon/type/ValidationException");
            /* try block*/
            methodInfo.placeLabel(L0);
            ga.invokeInstanceMethod(Configuration.class, "validateAttribute", StructuredQName.class, UnicodeString.class, Integer.TYPE);
            methodInfo.placeLabel(L1);
            // assert stack: out nodeName ann value ann2
            ga.dupX2();
            // assert stack: out nodeName ann2 ann value ann2
            ga.pop();
            // assert stack: out nodeName ann2 ann value
            ga.swap();
            // assert stack: out nodeName ann2 value ann
            ga.pop();
            // assert stack: out nodeName ann2 value
            LabelInfo end = methodInfo.newLabel("end");
            ga.goTo(end);

            /* catch block */
            methodInfo.placeLabel(L2);
//                XPathException err = XPathException.makeXPathException(e);
//                err.maybeSetErrorCode((validationAction== Validation.STRICT ? "XTTE1510" : "XTTE1515"));
//                err.setXPathContext(context);
//                err.maybesetLocation(getLocation());
//                err.setIsTypeError(true);
//                throw err;
            ga.invokeStaticMethod(XPathException.class, "makeXPathException", Exception.class);

            ga.dup();
            ga.push((instr.getValidationAction() == Validation.STRICT ? "XTTE1510" : "XTTE1515"));
            ga.invokeInstanceMethod(XPathException.class, "maybeSetErrorCode", String.class);

            ga.dup();
            allocateStatic(compiler, ((Expression)instr).getLocation());
            ga.invokeInstanceMethod(XPathException.class, "maybeSetLocation", Location.class);

            ga.dup();
            ga.push(true);
            ga.invokeInstanceMethod(XPathException.class, "setIsTypeError", Boolean.TYPE);

            ga.throwException();

            methodInfo.placeLabel(end);

        }
    }

    private void pushNodeName(CompilerService compiler, Expression expression) throws CannotCompileException {
        allocateStatic(compiler, ((FixedAttribute) expression).getAttributeName());
    }

    @Override
    public void compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getConfiguration");
        int configVar = methodInfo.allocateLocal(Configuration.class);
        ga.storeLocal(configVar);
        FixedAttribute instr = (FixedAttribute) expression;
        Expression expr = instr.getSelect();


        ga.newInstance(Orphan.class);
        ga.dup();
        allocateStatic(compiler, compiler.getConfiguration());
        ga.invokeConstructor(Orphan.class, Configuration.class);

        // set the node kind

        ga.dup();
        ga.push(net.sf.saxon.type.Type.ATTRIBUTE);
        ga.invokeInstanceMethod(Orphan.class, "setNodeKind", Short.TYPE);

        // set the name code

        ga.dup();
        pushNodeName(compiler, expression);
        ga.invokeInstanceMethod(Orphan.class, "setNodeName", NodeName.class);

        // set the string value

        ga.dup();
        compiler.compileToPrimitive(expr, UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);
        ga.invokeInstanceMethod(Orphan.class, "setStringValue", UnicodeString.class);

        validateOrphanAttribute(compiler, instr);


    }

    static void validateOrphanAttribute(CompilerService compiler, AttributeCreator instr) {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        if (instr.getSchemaType() != null) {
            if (instr.getSchemaType().isNamespaceSensitive()) {
                compiler.generateDynamicError("Cannot validate a parentless attribute whose content is namespace-sensitive",
                        "XTTE1545", instr.getLocation(), true);
            } else {
                ga.dup();
                ga.invokeInstanceMethod(NodeInfo.class, "getUnicodeStringValue");
                allocateStatic(compiler, instr.getSchemaType());
                ga.swap();
                allocateStatic(compiler, DummyNamespaceResolver.getInstance());
                allocateStatic(compiler, compiler.getConfiguration().getConversionRules());
                ga.invokeInstanceMethod(SimpleType.class, "validateContent", UnicodeString.class, NamespaceResolver.class, ConversionRules.class);

//            ValidationFailure err = schemaType.validateContent(
//                    o.getStringValueCS(), DummyNamespaceResolver.getInstance(), rules);
//            if (err != null) {
//                throw new ValidationException("Attribute value " + Err.wrap(o.getStringValueCS(), Err.VALUE) +
//                                           " does not the match the required type " +
//                                           schemaType.getDescription() + ". " +
//                                           err.getMessage());
//            }
//            o.setTypeAnnotation(schemaType.getFingerprint());

                LabelInfo valFail = methodInfo.newLabel("valFail");
                LabelInfo valSuccess = methodInfo.newLabel("valSuccess");
                ga.ifNonNull(valFail.label());
                ga.goTo(valSuccess.label());

                methodInfo.placeLabel(valFail);

                compiler.generateDynamicError("Invalid attribute", "XTTE1540", instr.getLocation(), true);
                // TODO: should be a ValidationException, get the message right
                methodInfo.placeLabel(valSuccess);
                ga.showMessage(compiler, "Attribute validation succeeded");

                ga.dup();
                allocateStatic(compiler, instr.getSchemaType());
                ga.invokeInstanceMethod(Orphan.class, "setTypeAnnotation", SchemaType.class);
            }
            //
        } else if (instr.getValidationAction() == Validation.STRICT ||
                instr.getValidationAction() == Validation.LAX) {
            //            int ann = context.getController().getConfiguration().validateAttribute(
            //                    nameCode, o.getStringValueCS(), validationAction);
            //            o.setTypeAnnotation(ann);
            // assert stack: Orphan
            ga.dup();
            // assert stack: Orphan Orphan
            ga.dup();
            // assert stack: Orphan Orphan Orphan
            allocateStatic(compiler, compiler.getConfiguration());
            // assert stack: Orphan Orphan Orphan Configuration
            ga.swap();
            // assert stack: Orphan Orphan Configuration Orphan
            ga.dup();
            // assert stack: Orphan Orphan Configuration Orphan Orphan
            ga.invokeInstanceMethod(NodeInfo.class, "getNameCode");
            // assert stack: Orphan Orphan Configuration Orphan nameCode
            ga.swap();
            // assert stack: Orphan Orphan Configuration nameCode Orphan
            ga.invokeInstanceMethod(NodeInfo.class, "getUnicodeStringValue");
            // assert stack: Orphan Orphan Configuration nameCode stringValue
            ga.push(instr.getValidationAction());
            // assert stack: Orphan Orphan Configuration nameCode stringValue int
            ga.invokeInstanceMethod(Configuration.class, "validateAttribute", Integer.TYPE, UnicodeString.class, Integer.TYPE);
            // assert stack: Orphan Orphan SchemaType
            ga.invokeInstanceMethod(Orphan.class, "setTypeAnnotation", SchemaType.class);
            // assert stack: Orphan

        } else {
            ga.dup();
            if (instr.getSchemaType() != null) {
                allocateStatic(compiler, instr.getSchemaType());
            } else {
                ga.pushNull();
            }

            ga.invokeInstanceMethod(Orphan.class, "setTypeAnnotation", SchemaType.class);
        }
    }

}

// Copyright (c) 2011-2020 Saxonica Limited