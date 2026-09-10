package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import com.saxonica.ee.validate.SkipValidator;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ElementCreator;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.type.AnyType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.Untyped;
import org.objectweb.asm.Type;


/**
 * Generate bytecode. An instruction that creates an element node.
 * There are two subtypes, FixedElement for use where the name is
 * known statically, and ComputedElement where it is computed
 * dynamically.
 */
public abstract class ElementCreatorCompiler extends ToPushCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression) throws CannotCompileException {

        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        identifyExpression(compiler, expression, "push");
        visitAnnotation(compiler, "ElementCreator-push");
        visitLineNumber(compiler, ga, expression);
        ElementCreator elementCreator = (ElementCreator) expression;
        SchemaType typeCode = elementCreator.getValidationAction() == Validation.PRESERVE
            ? AnyType.getInstance()
            : Untyped.getInstance();
        int resetReceiver = 0;
        int nodeNameVar = -1;

        if (!elementCreator.isPreservingTypes()) {
            resetReceiver++;
            if (elementCreator.getValidationAction() == Validation.STRIP) {
                addTypeStripper(compiler);
            } else {
                // ParseOptions options = new ParseOptions(validationOptions)
                // options.setTopLevelElement(elemName);
                // validator = config.getElementValidator(receiver, options, locationId)
                compiler.generateGetContext();
                ga.invokeInstanceMethod(XPathContext.class, "getConfiguration");
                compiler.generateGetReceiver();
                ga.newInstance(ParseOptions.class);
                ga.dup();
                allocateStatic(compiler, elementCreator.getValidationOptions());
                ga.invokeConstructor(ParseOptions.class, ParseOptions.class);
                ga.dup();
                pushNodeName(compiler, elementCreator);
                nodeNameVar = methodInfo.allocateLocal(NodeName.class);
                ga.dup();
                ga.storeLocal(nodeNameVar);
                ga.invokeInstanceMethod(NodeName.class, "getStructuredQName");
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

        // Generate code to set the base URI if necessary

        LabelInfo nonNullSystemId = methodInfo.newLabel("nonNullSystemId");
        if (elementCreator.getStaticBaseURIString() != null) {
            compiler.generateGetReceiver();
            ga.showObject(compiler, "Receiver: ");
            ga.invokeInstanceMethod(Receiver.class, "getSystemId");
            ga.ifNonNull(nonNullSystemId.label());
            visitAnnotation(compiler, "ElementCreator-setting system ID to " + elementCreator.getStaticBaseURIString());
            compiler.generateGetReceiver();
            ga.push(elementCreator.getStaticBaseURIString());
            ga.invokeInstanceMethod(Receiver.class, "setSystemId", String.class);
            methodInfo.placeLabel(nonNullSystemId);
        }

        // Generate the call on startElement()

        Expression content = elementCreator.getContentExpression();
        Location locationId = expression.getLocation();
        //int typeCode = (elementCreator.getValidationAction() == Validation.PRESERVE ? StandardNames.XS_ANY_TYPE : StandardNames.XS_UNTYPED);
        int properties = (elementCreator.isInheritNamespacesToChildren() ? 0 : ReceiverOptions.DISINHERIT_NAMESPACES);
        if (!elementCreator.isInheritNamespacesFromParent()) {
            properties |= ReceiverOptions.REFUSE_NAMESPACES;
        }
        compiler.generateGetReceiver();
        if (nodeNameVar == -1) {
            pushNodeName(compiler, expression);
        } else {
            ga.loadLocal(nodeNameVar);
        }
        //ga.push(typeCode);
        allocateStatic(compiler, typeCode);
        allocateStatic(compiler, locationId);
        ga.push(properties);
        ga.invokeInstanceMethod(Receiver.class, "startElement", NodeName.class, SchemaType.class, Location.class, Integer.TYPE);

        outputNamespaceNodes(compiler, expression);

        if (!Literal.isEmptySequence(content)) {
            compiler.compileToPush(content);
        }
        compiler.generateGetReceiver();
        ga.invokeInstanceMethod(Receiver.class, "endElement");

        for (int i = 0; i < resetReceiver; i++) {
            compiler.popReceiverInfo();
        }
        methodInfo.releaseLocal(nodeNameVar);
    }

    /**
     * Callback for subclasses to implement: generate code to output namespace nodes
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     */

    protected void outputNamespaceNodes(CompilerService compiler, Expression expression) {
    }

    /**
     * Callback from the superclass ElementCreatorCompiler to get the name of the element.
     * Leaves the name code for the element name on the stack
     * for the element name
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */
    protected abstract void pushNodeName(CompilerService compiler, Expression expression) throws CannotCompileException;

    /**
     * Add a filter to the output pipeline that removes type annotations. This method adds a new entry to
     * the receiver stack, whether it creates a new filter or not.
     *
     * @param compiler the compiler service
     */

    protected static void addTypeStripper(CompilerService compiler) {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        LabelInfo noStripNeeded = methodInfo.newLabel("noStripNeeded");
        LabelInfo returnReceiver = methodInfo.newLabel("returnReceiver");
        compiler.generateGetReceiver();

        ga.invokeInstanceMethod(Receiver.class, "usesTypeAnnotations");
        ga.ifFalse(noStripNeeded);

        ga.newInstance(TreeReceiver.class);
        ga.dup();

        ga.newInstance(SkipValidator.class);
        ga.dup();
        compiler.generateGetReceiver();
        ga.invokeConstructor(SkipValidator.class, Receiver.class);
        ga.invokeConstructor(TreeReceiver.class, Receiver.class);
        ga.goTo(returnReceiver);
        methodInfo.placeLabel(noStripNeeded);
        // if we haven't created a new receiver, we put a copy of the old one one the stack, allowing an
        // unconditional pop() later.
        compiler.generateGetReceiver();
        methodInfo.placeLabel(returnReceiver);
        // assert stack newReceiver
        compiler.pushNewReceiverInfo(ga);
    }

    public abstract void identifyExpression(CompilerService compiler, Expression expression, String evalMode);

}

//Copyright (c) 2011-2015 Saxonica Limited.
