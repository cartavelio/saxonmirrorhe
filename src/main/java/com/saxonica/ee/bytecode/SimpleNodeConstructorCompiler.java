package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.instruct.SimpleNodeConstructor;
import net.sf.saxon.om.FingerprintedQName;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.str.EmptyUnicodeString;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.Cardinality;

public abstract class SimpleNodeConstructorCompiler extends ToPushCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "SimpleNodeConstructorCompiler-Push " + expression.getExpressionName());
        SimpleNodeConstructor simpleNode = (SimpleNodeConstructor) expression;

        // push the receiver onto the stack
        compiler.generateGetOutputter();

        // push the node name if any onto the stack
        compileNodeName(compiler, expression);

        Expression content = simpleNode.getSelect();

        // push the content onto the stack
        visitAnnotation(compiler, "SimpleNodeConstructor-Push-content");
        //System.err.println("Content::: " + content.toShortString() + " --- " + content.getClass());
        compiler.compileToPrimitive(content, UnicodeString.class, OnEmpty.RETURN_NULL);
        LabelInfo done = null;
        if (Cardinality.allowsZero(content.getCardinality())) {
            LabelInfo notNull = methodInfo.newLabel("simpleNodeNotNull");
            ga.dup();
            ga.ifNonNull(notNull.label());
            ga.pop();
            if (isNoNodeIfEmpty(expression)) {
                done = methodInfo.newLabel("doneSimpleNode");
                ga.pop();
                ga.goTo(done);
            } else {
                ga.invokeStaticMethod(EmptyUnicodeString.class, "getInstance");
            }
            methodInfo.placeLabel(notNull);
        }

        // check and adjust the content if necessary
        checkContent(compiler, expression);

        // write the node to the current receiver
        pushNode(compiler, expression);

        if (done != null) {
            methodInfo.placeLabel(done);
        }

    }

    /**
     * Overridable method to ask whether no node should be produced in the case that the content is empty
     *
     * @return true if the rules for this instruction are that when the content is empty, no node is generated
     */

    protected boolean isNoNodeIfEmpty(Expression expr) {
        return false;
    }

    /**
     * Compile code to check the content of the node, and adjust it if necessary
     * On entry the proposed content is on the stack as a UnicodeString; on exit the revised content
     * must be on the stack as a UnicodeString.
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     */

    protected void checkContent(CompilerService compiler, Expression expression) {
        // default implementation does nothing
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

    protected void compileNodeName(CompilerService compiler, Expression expression) throws CannotCompileException {
        // default implementation is suitable for unnamed nodes such as comments
    }

    /**
     * Generate code to write the node to the current receiver. On entry the stack holds the
     * current receiver, the node name (for named node kinds only) and the content (for all nodes),
     * in general as instances of UnicodeString
     *
     * @param compiler      the compiler service
     * @param expression    the expression to be compiled
     */

    protected abstract void pushNode(CompilerService compiler, Expression expression);

    /**
     * Ask whether this instruction generates a kind of node that is named
     *
     * @return true if the instruction generates a kind of node that is named
     */

    protected boolean isNamedNodeKind() {
        return false;
    }

    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "SimpleNodeConstructorCompiler-Push");
        SimpleNodeConstructor simpleNode = (SimpleNodeConstructor) expression;

        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();

        ga.newInstance(Orphan.class);
        ga.dup();
        allocateStatic(compiler, config);
        ga.invokeConstructor(Orphan.class, Configuration.class);

        ga.dup();
        ga.push((short) simpleNode.getItemType().getPrimitiveType());
        ga.invokeInstanceMethod(Orphan.class, "setNodeKind", Short.TYPE);

        if (isNamedNodeKind()) {
            ga.dup();
            ga.newInstance(FingerprintedQName.class);
            ga.dup();
            compileNodeName(compiler, expression);
            //allocateStatic(compiler, config.getNamePool());
            //ga.swap();
            ga.push("");
            ga.swap();
            ga.push("");
            ga.swap();
            // assert: stack contains NamePool, "", "", localName
            //  ga.invokeInstanceMethod(NamePool.class, "allocate", String.class, String.class, String.class);
            ga.invokeConstructor(FingerprintedQName.class, String.class, String.class, String.class);
            ga.invokeInstanceMethod(Orphan.class, "setNodeName", NodeName.class);
        }

        ga.dup();

        Expression content = simpleNode.getSelect();

        // push the content onto the stack
        compiler.compileToPrimitive(content, UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);

        // check and adjust the content if necessary
        checkContent(compiler, expression);

        // set the string value of the Orphan node
        ga.invokeInstanceMethod(Orphan.class, "setStringValue", UnicodeString.class);

        // return with the Orphan node on the top of stack

    }
}

// Copyright (c) 2011-2020 Saxonica Limited