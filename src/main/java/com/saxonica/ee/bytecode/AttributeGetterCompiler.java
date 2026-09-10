package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.AttributeGetter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.FingerprintedQName;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.tree.tiny.TinyElementImpl;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.UntypedAtomicValue;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate bytecode for evaluation of an AxisExpression
 */
public class AttributeGetterCompiler extends ToItemCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        final FingerprintedQName attributeName = ((AttributeGetter) expression).getAttributeName();
        final Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "AttGetterCompiler - primitive");
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getContextItem");
        // item
        LabelInfo end = methodInfo.newLabel("endAttGet");
        int requiredChecks = ((AttributeGetter) expression).getRequiredChecks();
//        if ((requiredChecks & AttributeGetter.CHECK_CONTEXT_ITEM_PRESENT) != 0) {
//            LabelInfo notUndefined = methodInfo.newLabel("notUndefined");
//            ga.dup();
//            // item item
//            ga.ifNonNull(notUndefined.label());
//            // Throw error - context item is undefined
//            compiler.generateDynamicError("The context item for @" + attributeName.getDisplayName() + " is absent", "XPDY0002", expression.getLocation(), false);
//            // item
//            methodInfo.placeLabel(notUndefined);
//        }

        // Fast path if the node is an element in a tiny tree
        LabelInfo notTiny = methodInfo.newLabel("notTiny");
        ga.dup();
        // item item
        ga.ifNotInstance(TinyElementImpl.class, notTiny);
        // item
        ga.checkClass(TinyElementImpl.class);
        ga.push(attributeName.getFingerprint());
        ga.invokeInstanceMethod(TinyElementImpl.class, "getAttributeValue", Integer.TYPE);
        // string or null
        ga.goTo(end);

        methodInfo.placeLabel(notTiny);
        // item
        if ((requiredChecks & AttributeGetter.CHECK_CONTEXT_ITEM_IS_NODE) != 0) {
            ga.dup();
            // item item
            LabelInfo isNode = methodInfo.newLabel("isNode");
            ga.ifInstance(NodeInfo.class, isNode);
            // item
            compiler.generateDynamicError("The context item for @" + attributeName.getDisplayName() + " is not a node", "XPDY0002", expression.getLocation(), false);
            ga.pop();
            ga.pushNull();
            ga.goTo(end);

            methodInfo.placeLabel(isNode);
        }
        // item
        ga.dup();
        // item item
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getNodeKind");
        // item kind
        LabelInfo isElement = methodInfo.newLabel("isElement");
        ga.push((int) Type.ELEMENT);
        ga.ifICmp(GeneratorAdapter.EQ, isElement.label());
        // item
        ga.pop();
        ga.pushNull();
        ga.goTo(end);

        methodInfo.placeLabel(isElement);
        // item
        ga.push(attributeName.getURI());
        ga.push(attributeName.getLocalPart());
        ga.invokeInstanceMethod(NodeInfo.class, "getAttributeValue", String.class, String.class);
        // string or null

        methodInfo.placeLabel(end);
        // string or null
        ga.dup();
        LabelInfo exists = methodInfo.newLabel("exists");
        ga.ifNonNull(exists.label());
        // string
        ga.pop();
        onEmpty.generate(ga);
        methodInfo.placeLabel(exists);
    }

    @Override
    public void compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        final FingerprintedQName attributeName = ((AttributeGetter)expression).getAttributeName();
        final Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "AttGetterCompiler - item");
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getContextItem");
        // item
        LabelInfo end = methodInfo.newLabel("endAttGet");
        int requiredChecks = ((AttributeGetter) expression).getRequiredChecks();
//        if ((requiredChecks & AttributeGetter.CHECK_CONTEXT_ITEM_PRESENT) != 0) {
//            LabelInfo notUndefined = methodInfo.newLabel("notUndefined");
//            ga.dup();
//            // item item
//            ga.ifNonNull(notUndefined.label());
//            // Throw error - context item is undefined
//            compiler.generateDynamicError("The context item for @" + attributeName.getDisplayName() + " is absent", "XPDY0002", expression.getLocation(), false);
//            // item
//            methodInfo.placeLabel(notUndefined);
//        }

        // Fast path if the node is an element in a tiny tree
        LabelInfo notTiny = methodInfo.newLabel("notTiny");
        ga.dup();
        // item item
        ga.ifNotInstance(TinyElementImpl.class, notTiny);
        // item
        ga.checkClass(TinyElementImpl.class);
        ga.push(attributeName.getFingerprint());
        ga.invokeInstanceMethod(TinyElementImpl.class, "getAttributeValue", Integer.TYPE);
        // string
        ga.dup();
        // string string
        ga.ifNull(end.label());
        // string
        ga.newInstance(UntypedAtomicValue.class);
        // string UAV
        ga.dupX1();
        // UAV string UAV
        ga.swap();
        // UAV UAV string
        ga.invokeConstructor(UntypedAtomicValue.class, CharSequence.class);
        // UAV
        ga.goTo(end);

        methodInfo.placeLabel(notTiny);
        if ((requiredChecks & AttributeGetter.CHECK_CONTEXT_ITEM_IS_NODE) != 0) {
            // item
            ga.dup();
            // item item
            LabelInfo isNode = methodInfo.newLabel("isNode");
            ga.ifInstance(NodeInfo.class, isNode);
            // item
            compiler.generateDynamicError("The context item for @" + attributeName.getDisplayName() + " is not a node", "XPDY0002", expression.getLocation(), false);
            ga.pop();
            ga.pushNull();
            ga.goTo(end);

            methodInfo.placeLabel(isNode);
        }
        // item
        ga.dup();
        // item item
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getNodeKind");
        // item kind
        LabelInfo isElement = methodInfo.newLabel("isElement");
        ga.push((int) Type.ELEMENT);
        ga.ifICmp(GeneratorAdapter.EQ, isElement.label());
        // item
        ga.pop();
        ga.pushNull();
        ga.goTo(end);

        methodInfo.placeLabel(isElement);
        // item
        ga.push(attributeName.getURI());
        ga.push(attributeName.getLocalPart());
        ga.invokeInstanceMethod(NodeInfo.class, "getAttributeValue", String.class, String.class);
        // string
        ga.dup();
        // string string
        ga.ifNull(end.label());
        // string
        ga.newInstance(UntypedAtomicValue.class);
        // string UAV
        ga.dupX1();
        // UAV string UAV
        ga.swap();
        // UAV UAV string
        ga.invokeConstructor(UntypedAtomicValue.class, CharSequence.class);
        // UAV
        ga.goTo(end);

        methodInfo.placeLabel(end);
    }



}

// Copyright (c) 2018 Saxonica Limited.


