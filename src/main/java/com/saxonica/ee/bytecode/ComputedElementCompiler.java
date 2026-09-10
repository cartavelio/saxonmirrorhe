package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.expr.instruct.ComputedElement;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.StandardURIChecker;
import net.sf.saxon.om.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.value.Whitespace;
import org.objectweb.asm.Type;

/**
 * Generate bytecode for a computed element constructor (or xsl:element in XSLT)
 */

public class ComputedElementCompiler extends ElementCreatorCompiler {

    /**
     * Callback from the superclass ElementCreatorCompiler to get the NodeName.
     * Leaves the NodeName for the element name on the stack
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */
    @Override
    protected void pushNodeName(CompilerService compiler, Expression expression) throws CannotCompileException {
        ComputedElement compElement = (ComputedElement) expression;
        Expression nameExpr = compElement.getNameExp();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo notNull = methodInfo.newLabel("notNullEle");
        LabelInfo notNull2 = methodInfo.newLabel("notNullEle2");
        LabelInfo notNull3 = methodInfo.newLabel("notNullEle3");
        LabelInfo uriNotNull = methodInfo.newLabel("uriNotNull");
        LabelInfo notStrInstance = methodInfo.newLabel("notStrInstance");
        LabelInfo notQNameInstance = methodInfo.newLabel("notQNameInstance");
        LabelInfo end = methodInfo.newLabel("endCompElem");
        final LabelInfo end1 = methodInfo.newLabel("end1");
        LabelInfo done = methodInfo.newLabel("doneCompElem");
        LabelInfo test1 = methodInfo.newLabel("test1");
        LabelInfo allocLab = methodInfo.newLabel("allocLab");
        LabelInfo notEqual = methodInfo.newLabel("notEqual");
        LabelInfo notEqual2 = methodInfo.newLabel("notEqual2");
        LabelInfo XMLNSTest = methodInfo.newLabel("XMLNSTest");
        LabelInfo notEQName = methodInfo.newLabel("notEQName");
        int localNameVar = methodInfo.allocateLocal(String.class);
        int uriVar = methodInfo.allocateLocal(String.class);
        ga.pushNull();
        ga.storeLocal(uriVar);
        final int prefixVar = methodInfo.allocateLocal(String.class);
        final int rawNameVar = methodInfo.allocateLocal(String.class);

        Class<? extends Item> operandClass = compiler.compileToItem(nameExpr);
        operandClass = ga.downCast(operandClass, AtomicValue.class, true);
        ga.dup();
        ga.ifNonNull(notNull.label());
        ga.pop();
        compiler.generateDynamicError("Invalid element name (empty sequence)", (compElement.isXSLT() ? "XTDE0820" : "XPTY0004"), compElement.getLocation(), false);
        ga.goTo(end);
        methodInfo.placeLabel(notNull);
        ga.dup();
        ga.ifNotInstance(StringValue.class, notStrInstance);

        ga.invokeInstanceMethod(AtomicValue.class, "getStringValue");
        ga.invokeStaticMethod(Whitespace.class, "trim", String.class);
        ga.storeLocal(rawNameVar);
        ga.loadLocal(rawNameVar);

        ga.dup();
        ga.push("Q{");
        ga.invokeInstanceMethod(String.class, "startsWith", String.class);
        ga.ifFalse(notEQName);
        ga.loadLocal(rawNameVar);
        ga.push(compElement.isXSLT() ? "XTDE0820" : "XQDY0074");
        ga.invokeStaticMethod(Callback.class, "sqNameFromValidatedEQName", String.class, String.class);
        ga.dup();
        ga.invokeInstanceMethod(StructuredQName.class, "getLocalPart");
        ga.storeLocal(localNameVar);
        ga.invokeInstanceMethod(StructuredQName.class, "getURI");
        ga.storeLocal(uriVar);
        ga.push("");
        ga.storeLocal(prefixVar);
        ga.pop();
        ga.goTo(test1);
        methodInfo.placeLabel(notEQName);

        LabelInfo tryBegin = methodInfo.newLabel("tryBegin");
        LabelInfo tryEnd = methodInfo.newLabel("tryEnd");
        LabelInfo catchBegin = methodInfo.newLabel("catchBegin");
        ga.visitTryCatchBlock(tryBegin, tryEnd, catchBegin, "net/sf/saxon/om/QNameException");

        methodInfo.placeLabel(tryBegin);
        ga.invokeStaticMethod(NameChecker.class, "getQNameParts", String.class);
        methodInfo.placeLabel(tryEnd);

        ga.dup();
        //retrieve prefix
        ga.push(0);
        ga.arrayLoad(Type.getType(String.class));
        ga.storeLocal(prefixVar);
        //retrieve localName
        ga.push(1);
        ga.arrayLoad(Type.getType(String.class));
        ga.storeLocal(localNameVar);

        ga.goTo(test1);

        // Catch the QNameException
        methodInfo.placeLabel(catchBegin);
        compiler.generateParameterizedDynamicError(
                new MessageTemplate(ga, "Invalid element name: <{$1}>", rawNameVar),
                compElement.isXSLT() ? "XTDE0820" : "XQDY0074", compElement.getLocation(), false);


        methodInfo.placeLabel(notStrInstance);
        //atomicValue on stack
        if (compElement.isAllowNameAsQName()) {
            ga.dup();
            ga.ifNotInstance(QNameValue.class, notQNameInstance);
            ga.checkCast(Type.getType(QNameValue.class));
            ga.dup();

            ga.invokeInstanceMethod(QNameValue.class, "getLocalName");
            ga.storeLocal(localNameVar);
            ga.dup();
            ga.invokeInstanceMethod(QNameValue.class, "getNamespaceURI");
            ga.storeLocal(uriVar);
            ga.loadLocal(uriVar);
            ga.ifNonNull(notNull2.label());
            ga.push("");
            ga.storeLocal(uriVar);
            methodInfo.placeLabel(notNull2);
            ga.invokeInstanceMethod(QNameValue.class, "getPrefix");
            ga.storeLocal(prefixVar);
            ga.loadLocal(prefixVar);
            ga.push("xmlns");
            ga.invokeInstanceMethod(String.class, "equals", Object.class);
            ga.ifFalse(notEqual);
            compiler.generateDynamicError("Computed element name has prefix xmlns", "XQDY0096", compElement.getLocation(), false);
            ga.goTo(end);
            methodInfo.placeLabel(notEqual);
            ga.goTo(test1);
            methodInfo.placeLabel(notQNameInstance);
            compiler.generateDynamicError("Computed element name has incorrect type",
                (compElement.isXSLT() ? "XTDE0820" : "XPTY0004"), compElement.getLocation(), true);
            ga.goTo(end);
        } else {
            compiler.generateDynamicError("Computed element name has incorrect type",
                (compElement.isXSLT() ? "XTDE0820" : "XPTY0004"), compElement.getLocation(), true);
            ga.goTo(end);
        }
        methodInfo.placeLabel(test1);

        Expression namespace = compElement.getNamespaceExp();

        if (namespace == null) {
            ga.loadLocal(uriVar);
            ga.ifNonNull(uriNotNull.label());
            ExpressionCompiler.allocateStatic(compiler, compElement.getNamespaceResolver());
            ga.loadLocal(prefixVar);
            ga.push(true);
            ga.invokeInstanceMethod(NamespaceResolver.class, "getURIForPrefix", String.class, Boolean.TYPE);
            ga.dup();
            ga.ifNonNull(notNull3.label());
            ga.pop();
            compiler.generateParameterizedDynamicError(
                    new MessageTemplate(ga, "Undeclared prefix in element name: {$1}", prefixVar),
                    compElement.isXSLT() ? "XTDE0830" : "XQDY0074", compElement.getLocation(), false);
            ga.goTo(end);
            methodInfo.placeLabel(notNull3);
            ga.storeLocal(uriVar);
            methodInfo.placeLabel(uriNotNull);
        } else {
            ga.loadLocal(uriVar);
            ga.ifNonNull(uriNotNull.label());
            if (namespace instanceof StringLiteral) {
                ga.push(((StringLiteral) namespace).stringify());
                ga.storeLocal(uriVar);

            } else {
                compiler.compileToPrimitive(namespace, String.class, OnEmpty.RETURN_NULL);
                ga.storeLocal(uriVar);

                ga.invokeStaticMethod(StandardURIChecker.class, "getInstance");
                ga.loadLocal(uriVar);
                ga.invokeInstanceMethod(StandardURIChecker.class, "isValidURI", String.class);
                ga.ifTrue(uriNotNull);
                compiler.generateDynamicError("The value of the namespace attribute must be a valid URI", "XTDE0835", compElement.getLocation(), false);
                ga.goTo(end);
            }
            methodInfo.placeLabel(uriNotNull);
            ga.loadLocal(uriVar);
            ga.invokeInstanceMethod(String.class, "length");
            ga.push(0);
            ga.ifCmp(Type.getType(Integer.TYPE), Generator.NE, notEqual2.label());
            ga.push("");
            ga.storeLocal(prefixVar);

            methodInfo.placeLabel(notEqual2);
            ga.loadLocal(prefixVar);
            ga.push("xmlns");
            ga.invokeInstanceMethod(String.class, "equals", Object.class);
            ga.ifFalse(done);
            // this isn't a legal prefix so we mustn't use it
            ga.push("x-xmlns");
            ga.storeLocal(prefixVar);
        }


        methodInfo.placeLabel(done);
        ga.loadLocal(uriVar);

        ga.push(NamespaceConstant.XMLNS);
        ga.invokeInstanceMethod(String.class, "equals", Object.class);
        ga.ifFalse(XMLNSTest);
        compiler.generateParameterizedDynamicError(
                new MessageTemplate(ga, "Cannot create element in namespace {$1}", uriVar),
                compElement.isXSLT() ? "XTDE0835" : "XQDY0096", compElement.getLocation(), false);
        ga.goTo(end);

        methodInfo.placeLabel(XMLNSTest);
        ga.loadLocal(uriVar);
        ga.push(NamespaceConstant.XML);
        ga.invokeInstanceMethod(String.class, "equals", Object.class);
        ga.loadLocal(prefixVar);
        ga.push("xml");
        ga.invokeInstanceMethod(String.class, "equals", Object.class);
        ga.ifCmp(Type.getType(Boolean.TYPE), Generator.EQ, allocLab.label());

        ga.loadLocal(prefixVar);
        ga.loadLocal(uriVar);
        ga.push(compElement.isXSLT());
        ga.push(compElement.getLocation().getSystemId());
        ga.push(compElement.getLocation().getLineNumber());
        ga.invokeStaticMethod(Callback.class, "makeXmlNamespaceException",
                    String.class, String.class, Boolean.TYPE, String.class, Integer.TYPE);
        ga.throwException();
        //ga.goTo(end);

        methodInfo.placeLabel(allocLab);

        ga.newInstance(FingerprintedQName.class);
        ga.dup();
        ga.loadLocal(prefixVar);
        ga.loadLocal(uriVar);
        ga.invokeInstanceMethod(Object.class, "toString");
        ga.loadLocal(localNameVar);
        ga.invokeConstructor(FingerprintedQName.class, String.class, String.class, String.class);

        methodInfo.placeLabel(end);
        methodInfo.releaseLocal(localNameVar);
        methodInfo.releaseLocal(uriVar);
        methodInfo.releaseLocal(prefixVar);
    }

    @Override
    protected void outputNamespaceNodes(CompilerService compiler, Expression expression) {
    }

    @Override
    public void identifyExpression(CompilerService compiler, Expression expression, String evalMode) {
        visitAnnotation(compiler, "ComputedElement-" + evalMode);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
