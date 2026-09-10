package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.ProcessingInstruction;
import net.sf.saxon.om.NameChecker;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.value.AnyURIValue;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.value.Whitespace;

public class ProcessingInstructionCompiler extends SimpleNodeConstructorCompiler {

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
     * (namespace nodes), an entry is always added to the stack, which will be null if the particular node has
     * no name.
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     * @throws CannotCompileException if the expression cannot be compiled to bytecode
     */
    @Override
    protected void compileNodeName(CompilerService compiler, Expression expression) throws CannotCompileException {
        final Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        ProcessingInstruction procInstr = (ProcessingInstruction) expression;
        if (((ProcessingInstruction) expression).isXSLT()) {
            // Value is always a string in XSLT
            compiler.compileToPrimitive(procInstr.getNameExp(), String.class, OnEmpty.RETURN_EMPTY_STRING);
        } else {
            // In XQuery, need to check the returned type
            compiler.compileToItem(procInstr.getNameExp());
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
            compiler.generateDynamicError("Processing instruction name is not an xs:string or xs:untypedAtomic", "XPTY0004", expression.getLocation(), true);

            methodInfo.placeLabel(isString);
        }
        ga.invokeStaticMethod(Whitespace.class, "trim", String.class);
        LabelInfo validNCName = methodInfo.newLabel("validNCName");
        LabelInfo notEqualXML = methodInfo.newLabel("PINotEqualXML");
        ga.dup();
        ga.invokeStaticMethod(NameChecker.class, "isValidNCName", String.class);
        ga.ifTrue(validNCName);
        compiler.generateParameterizedDynamicError(
                new MessageTemplate(ga, "Invalid processing instruction name: {$1}", -1),
                ((ProcessingInstruction) expression).isXSLT() ? "XTDE0890" : "XQDY0041", expression.getLocation(), false);

        methodInfo.placeLabel(validNCName);

        //PI Name on top of Stack
        ga.dup();
        ga.push("xml");
        ga.invokeInstanceMethod(String.class, "equalsIgnoreCase", String.class);
        ga.ifFalse(notEqualXML);
        ga.pop();
        String errorCode = ((ProcessingInstruction) expression).isXSLT() ? "XTDE0890" : "XQDY0064";
        compiler.generateDynamicError("Processing instructions cannot be named 'xml' in any combination of upper/lower case", errorCode, expression.getLocation(), false);
        methodInfo.placeLabel(notEqualXML);

    }

    @Override
    public void checkContent(CompilerService compiler, Expression expression) {

        ProcessingInstruction procInstr = (ProcessingInstruction) expression;
        Generator ga = compiler.getCurrentGenerator();
        if (procInstr.isXSLT()) {
            ga.invokeStaticMethod(ProcessingInstruction.class, "checkContentXSLT", UnicodeString.class);
        } else {
            ga.invokeStaticMethod(ProcessingInstruction.class, "checkContentXQuery", UnicodeString.class);
        }
    }

    /**
     * Generate code to write the node to the current receiver. On entry the stack holds the
     * current receiver, the node name (for named node kinds only) and the content (for all nodes)
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     */
    @Override
    protected void pushNode(CompilerService compiler, Expression expression) {
        Generator ga = compiler.getCurrentGenerator();
        allocateStatic(compiler, expression.getLocation());
        ga.push(ReceiverOption.NONE);
        ga.invokeInstanceMethod(Outputter.class, "processingInstruction", String.class, UnicodeString.class, Location.class, Integer.TYPE);
    }


}

// Copyright (c) 2011-2020 Saxonica Limited
