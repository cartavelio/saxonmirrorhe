package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.Instruction;
import net.sf.saxon.expr.instruct.NamespaceConstructor;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.StandardURIChecker;
import net.sf.saxon.om.NameChecker;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.*;
import org.objectweb.asm.commons.GeneratorAdapter;


/**
 * Generate bytecode for the xsl:namespace Instruction
 */
public class NamespaceConstructorCompiler extends SimpleNodeConstructorCompiler {

    /**
     * Ask whether this instruction generates a kind of node that is named
     *
     * @return true if the instruction generates a kind of node that is named
     */

    @Override
    protected boolean isNamedNodeKind() {
        return true;
    }

    /**
     * Compile code to generate the node name (if any) and leave it on the top of the stack. For node kinds that
     * are always unnamed (such as comments), the stack is not changed. For node kinds that may be named or unnamed
     * (namespace nodes), an entry is always added to the stack, which will be "" if the particular node has
     * no name.
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */
    @Override
    protected void compileNodeName(CompilerService compiler, Expression expression) throws CannotCompileException {
        NamespaceConstructor instr = (NamespaceConstructor) expression;
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo nameOK = methodInfo.newLabel("nameOK");
        if (((NamespaceConstructor) expression).isXSLT()) {
            // Value is always a string in XSLT
            compiler.compileToPrimitive(instr.getNameExp(), String.class, OnEmpty.RETURN_NULL);
        } else {
            // In XQuery, need to check the returned type
            compiler.compileToItem(instr.getNameExp());
            if (Cardinality.allowsZero(instr.getNameExp().getCardinality())) {
                LabelInfo nameNotNull = methodInfo.newLabel("nameNotNull");
                ga.dup();
                ga.ifNonNull(nameNotNull.label());
                ga.pop();
                ga.push("");
                ga.goTo(nameOK);
                methodInfo.placeLabel(nameNotNull);
            }
            ga.checkClass(AtomicValue.class);
            LabelInfo isString = methodInfo.newLabel("isString");
            LabelInfo isNotString = methodInfo.newLabel("isNotString");
            ga.dup();
            ga.ifNotInstance(StringValue.class, isNotString);
            ga.dup();
            ga.ifInstance(AnyURIValue.class, isNotString);
            ga.invokeInstanceMethod(AtomicValue.class, "getStringValue");
            ga.goTo(isString);

            methodInfo.placeLabel(isNotString);
            compiler.generateDynamicError("Namespace prefix is not an xs:string or xs:untypedAtomic",
                    "XPTY0004", expression.getLocation(), true);

            methodInfo.placeLabel(isString);
        }

        // assert stack: prefix

        ga.invokeStaticMethod(Whitespace.class, "trim", String.class);

        ga.dup();
        ga.invokeInstanceMethod(String.class, "length");
        ga.ifZCmp(GeneratorAdapter.EQ, nameOK.label());

        ga.dup();
        ga.invokeStaticMethod(NameChecker.class, "isValidNCName", String.class);
        LabelInfo validNCName = methodInfo.newLabel("validNCName");
        ga.ifTrue(validNCName);
        compiler.generateDynamicError("Namespace prefix is invalid",
                instr.isXSLT() ? "XTDE0920" : "XQDY0074",
                instr.getLocation(), false);

        methodInfo.placeLabel(validNCName);
        ga.dup();
        ga.push("xmlns");
        ga.invokeInstanceMethod(Object.class, "equals", Object.class);
        ga.ifFalse(nameOK);
        compiler.generateDynamicError("Namespace prefix must not be 'xmlns'",
                instr.isXSLT() ? "XTDE0920" : "XQDY0101",
                instr.getLocation(), false);

        methodInfo.placeLabel(nameOK);
    }

    /**
     * Generate code to write the node to the current receiver. On entry the stack holds the
     * current receiver, the node name (for named node kinds only) and the content (for all nodes),
     * as a UnicodeString
     *
     * @param compiler  the bytecode compiler
     * @param expression the expression to be compiled
     */
    @Override
    protected void pushNode(CompilerService compiler, Expression expression) {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();

        // assert stack : Receiver prefix (as String) uri (as UnicodeString)
        LabelInfo xmlTestOk = methodInfo.newLabel("xmltestok");
        LabelInfo xmlnsOk = methodInfo.newLabel("xmlnsOk");
        LabelInfo validURI = methodInfo.newLabel("validURI");
        ga.invokeInstanceMethod(UnicodeString.class, "toString");
        // assert stack : Receiver prefix (as String) uri (as String)
        ga.dup2();
        // assert stack : Receiver prefix uri prefix uri
        ga.dup();
        // assert stack : Receiver prefix uri prefix uri uri
        ga.invokeInstanceMethod(String.class, "length");
        // assert stack : Receiver prefix uri prefix uri length
        LabelInfo nonEmpty = methodInfo.newLabel("nonEmptyUri");
        ga.ifZCmp(GeneratorAdapter.NE, nonEmpty.label());
        // assert stack : Receiver prefix uri prefix uri
        compiler.generateDynamicError("Namespace URI must not be zero-length",
                (((Instruction) expression).isXSLT() ? "XTDE0930" : "XQDY0101"), expression.getLocation(), false);

        methodInfo.placeLabel(nonEmpty);
        // assert stack : Receiver prefix uri prefix uri
        ga.push(NamespaceConstant.XML);
        // assert stack : Receiver prefix uri prefix uri uri
        ga.invokeInstanceMethod(Object.class, "equals", Object.class);
        // assert stack : Receiver prefix uri prefix boolean
        ga.swap();
        // assert stack : Receiver prefix uri boolean prefix
        ga.push("xml");
        ga.invokeInstanceMethod(Object.class, "equals", Object.class);
        // assert stack : Receiver prefix uri boolean boolean
        ga.ifICmp(GeneratorAdapter.EQ, xmlTestOk.label());
        compiler.generateDynamicError("Namespace prefix 'xml' and namespace uri " + NamespaceConstant.XML +
                " must only be used together",
                (((Instruction) expression).isXSLT() ? "XTDE0925" : "XQDY0101"), expression.getLocation(), false);

        // assert stack : Receiver prefix uri
        methodInfo.placeLabel(xmlTestOk);
        // check for uri==xmlns namespace
        ga.invokeInstanceMethod(CharSequence.class, "toString");
        ga.dup();
        ga.push(NamespaceConstant.XMLNS);
        ga.invokeInstanceMethod(Object.class, "equals", Object.class);
        // assert stack : Receiver prefix uri boolean
        ga.ifFalse(xmlnsOk);
        compiler.generateDynamicError("Namespace URI must not be " + NamespaceConstant.XMLNS,
                (((Instruction) expression).isXSLT() ? "XTDE0905" : "XQDY0101"), expression.getLocation(), false);
        methodInfo.placeLabel(xmlnsOk);

        if (compiler.getConfiguration().getXsdVersion() == Configuration.XSD10) {
            // check that namespace URI is a valid URI  (W3C bug 30180)
            ga.dup();
            // StandardURIChecker.getInstance().isValidURI(uri)
            ga.invokeStaticMethod(StandardURIChecker.class, "getInstance");
            ga.swap();
            ga.invokeInstanceMethod(StandardURIChecker.class, "isValidURI", String.class);
            ga.ifTrue(validURI);
            compiler.generateDynamicError("The string value of the constructed namespace node must be a valid URI", "XTDE0905", expression.getLocation(), false);
            methodInfo.placeLabel(validURI);
        }

        ga.push(ReceiverOption.REJECT_DUPLICATES);
        ga.invokeInstanceMethod(Outputter.class, "namespace", String.class, String.class, Integer.TYPE);
    }
}

// Copyright (c) 2011-2022 Saxonica Limited