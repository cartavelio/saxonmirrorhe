package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.AttributeGetter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.FingerprintedQName;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.tiny.TinyElementImpl;
import net.sf.saxon.type.Type;
import net.sf.saxon.value.StringValue;
import org.objectweb.asm.commons.GeneratorAdapter;

/**
 * Generate bytecode for evaluation of an AttributeGetter - an expression that gets a named attribute
 * of the context node and returns its value as a string
 */
public class AttributeGetterCompiler extends ToItemCompiler {

    @Override
    public void compileToPrimitive(CompilerService compiler, Expression expression, Class<?> requiredClass, OnEmpty onEmpty) throws CannotCompileException {
        final FingerprintedQName attributeName = ((AttributeGetter) expression).getAttributeName();
        final Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        visitAnnotation(compiler, "AttGetterCompiler - primitive");
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getContextItem");
        // item
        LabelInfo testAbsence = methodInfo.newLabel("attTestAbsence");
        LabelInfo exists = methodInfo.newLabel("attExists");
        LabelInfo absent = methodInfo.newLabel("attAbsent");
        LabelInfo done = methodInfo.newLabel("attDone");
        int requiredChecks = ((AttributeGetter) expression).getRequiredChecks();

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
        ga.goTo(testAbsence);

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
            ga.goTo(absent);
            methodInfo.placeLabel(isNode);
        }
        // item
        ga.dup();
        // item item
        ga.checkClass(NodeInfo.class);
        ga.invokeInstanceMethod(NodeInfo.class, "getNodeKind");
        // item kind
        LabelInfo isElement = methodInfo.newLabel("isElement");
        ga.push(Type.ELEMENT);
        ga.ifICmp(GeneratorAdapter.EQ, isElement.label());
        // item
        ga.pop();
        ga.goTo(absent);

        methodInfo.placeLabel(isElement);
        // item
        allocateStatic(compiler, attributeName.getURI());
        ga.push(attributeName.getLocalPart());
        ga.invokeInstanceMethod(NodeInfo.class, "getAttributeValue", NamespaceUri.class, String.class);
        // string or null

        methodInfo.placeLabel(testAbsence);
        // string or null
        ga.dup();
        ga.ifNonNull(exists.label());
        // null
        ga.pop();
        methodInfo.placeLabel(absent);
        onEmpty.generate(ga);
        ga.goTo(done);
        methodInfo.placeLabel(exists);
        if (requiredClass == UnicodeString.class) {
            ga.invokeStaticMethod(StringView.class, "of", String.class);
        }
        methodInfo.placeLabel(done);
    }

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
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
        ga.invokeStaticMethod(StringView.class,"tidy", String.class);
        // UnicodeString
        ga.invokeStaticMethod(StringValue.class, "makeUntypedAtomic", UnicodeString.class);
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
        ga.push(Type.ELEMENT);
        ga.ifICmp(GeneratorAdapter.EQ, isElement.label());
        // item
        ga.pop();
        ga.pushNull();
        ga.goTo(end);

        methodInfo.placeLabel(isElement);
        // item
        allocateStatic(compiler, attributeName.getURI());
        ga.push(attributeName.getLocalPart());
        ga.invokeInstanceMethod(NodeInfo.class, "getAttributeValue", NamespaceUri.class, String.class);
        // string
        ga.dup();
        // string string
        ga.ifNull(end.label());
        // string
        ga.invokeStaticMethod(StringView.class, "tidy", String.class);
        // UnicodeString
        ga.invokeStaticMethod(StringValue.class, "makeUntypedAtomic", UnicodeString.class);
        // UAV
        ga.goTo(end);

        methodInfo.placeLabel(end);
        return StringValue.class;
    }



}

// Copyright (c) 2018-2022 Saxonica Limited


