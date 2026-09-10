package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ComputedAttribute;
import net.sf.saxon.lib.StandardURIChecker;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.SimpleType;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.value.Whitespace;
import org.objectweb.asm.Type;

/**
 * Generate bytecode for a computed attribute constructor
 */

public class ComputedAttributeCompiler extends ToPushCompiler {

    @Override
    public void compileToPush(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        Expression select = ((ComputedAttribute) expression).getSelect();
        visitAnnotation(compiler, "ComputedAttributeCompiler - push");
        visitLineNumber(compiler, ga, expression);

        int opt = ((ComputedAttribute) expression).getOptions();
        Location locationId = expression.getLocation();
        LabelInfo notEqual = methodInfo.newLabel("notEqual");
        LabelInfo done = null;
        // prepare the call on receiver.attribute()
        compiler.generateGetOutputter();
        pushNodeName(compiler, expression);
        int nodeNameVar = ga.newLocal(NodeName.class);
        ga.storeLocal(nodeNameVar);
        ga.loadLocal(nodeNameVar);

        SimpleType schemaType = ((ComputedAttribute) expression).getSchemaType();
        if (schemaType != null) {
            allocateStatic(compiler, schemaType);
        } else {
            ga.getStaticField(BuiltInAtomicType.class, "UNTYPED_ATOMIC", BuiltInAtomicType.class);
        }


        // push the content onto the stack
        compiler.compileToPrimitive(select, UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);

        FixedAttributeCompiler.generateAttributeValidationPushCode(compiler, (ComputedAttribute) expression, null, nodeNameVar);

        ga.loadLocal(nodeNameVar);
        ga.invokeInstanceMethod(NodeName.class, "getFingerprint");
        ga.push(StandardNames.XML_ID);
        ga.ifICmp(Generator.NE, notEqual.label());
        ga.invokeStaticMethod(Whitespace.class, "collapseWhitespace", UnicodeString.class);
        methodInfo.placeLabel(notEqual);
        ga.invokeInstanceMethod(UnicodeString.class, "toString");
        // Complete the call on receiver.attribute()
        allocateStatic(compiler, locationId);
        ga.push(opt);
        visitAnnotation(compiler, "ComputedAttributeCompiler - call attribute()");
        ga.invokeInstanceMethod(Outputter.class, "attribute", NodeName.class, SimpleType.class, String.class, Location.class, Integer.TYPE);
        if (done != null) {
            methodInfo.placeLabel(done);
        }
    }


    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "ComputedAttributeCompiler - item");
        ComputedAttribute instr = (ComputedAttribute) expression;

        Expression select = instr.getSelect();
        compiler.compileToPrimitive(select, UnicodeString.class, OnEmpty.RETURN_EMPTY_UNICODE_STRING);

        int contentVar = methodInfo.allocateLocal(UnicodeString.class);
        ga.storeLocal(contentVar);

        LabelInfo done = null;

        ga.newInstance(Type.getType(Orphan.class));
        ga.dup();
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getConfiguration");
        ga.invokeConstructor(Orphan.class, Configuration.class);
        int oVar = methodInfo.allocateLocal(Orphan.class);
        ga.storeLocal(oVar);
        ga.loadLocal(oVar);
        ga.push(net.sf.saxon.type.Type.ATTRIBUTE);
        ga.invokeInstanceMethod(Orphan.class, "setNodeKind", Short.TYPE);
        ga.loadLocal(oVar);
        ga.loadLocal(contentVar);
        ga.invokeInstanceMethod(Orphan.class, "setStringValue", UnicodeString.class);
        ga.loadLocal(oVar);
        pushNodeName(compiler, expression);
        ga.invokeInstanceMethod(Orphan.class, "setNodeName", NodeName.class);
        ga.loadLocal(oVar);

        FixedAttributeCompiler.validateOrphanAttribute(compiler, instr);

        if (done != null) {
            methodInfo.placeLabel(done);
        }
        methodInfo.releaseLocal(contentVar);
        methodInfo.releaseLocal(oVar);
        return Orphan.class;
    }

    /**
     * Generate code to evaluate the name of the attribute, check that it is valid, and leave the
     * namecode on the bytecode stack
     *
     * @param compiler the compiler service
     * @param expression the computed attribute instruction
     * @throws CannotCompileException if a failure occurs
     */

    private void pushNodeName(CompilerService compiler, Expression expression) throws CannotCompileException {
        ComputedAttribute compAttr = (ComputedAttribute) expression;
        Expression nameExpr = compAttr.getNameExp();
        Expression namespace = compAttr.getNamespaceExp();
        final Generator ga = compiler.getCurrentGenerator();
        final GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo notNull2 = methodInfo.newLabel("notNull2");
        LabelInfo notNull3 = methodInfo.newLabel("notNull3");
        LabelInfo uriNotNull = methodInfo.newLabel("uriNotNull");
        LabelInfo notStrInstance = methodInfo.newLabel("notStrInstance");
        LabelInfo notQNameInstance = methodInfo.newLabel("notQNameInstance");
        LabelInfo end = methodInfo.newLabel("endCompAttr");
        LabelInfo doneAtt = methodInfo.newLabel("doneAtt");
        LabelInfo done1 = methodInfo.newLabel("done1");
        LabelInfo qnameOK = methodInfo.newLabel("qnameOK");
        LabelInfo uriGTZero = methodInfo.newLabel("uriGTZero");
        LabelInfo prefixOK = methodInfo.newLabel("prefixOK");
        LabelInfo test1 = methodInfo.newLabel("test1");
        LabelInfo test2 = methodInfo.newLabel("test2");
        LabelInfo notEqual = methodInfo.newLabel("notEqual");
        LabelInfo notEqual2 = methodInfo.newLabel("notEqual2");
        LabelInfo notEqual_rawName = methodInfo.newLabel("notEqual_rawName");
        LabelInfo notEqual_prefix = methodInfo.newLabel("notEqual_prefix");
        LabelInfo notEQName = methodInfo.newLabel("notEQName");

        int localNameVar = methodInfo.allocateLocal(String.class);
        int uriVar = methodInfo.allocateLocal(NamespaceUri.class);
        ga.pushNull();
        ga.storeLocal(uriVar);
        final int prefixVar = methodInfo.allocateLocal(String.class);
        ga.pushNull();
        ga.storeLocal(prefixVar);

        compiler.compileToItem(nameExpr);
        ga.dup();
        ga.ifNotInstance(StringValue.class, notStrInstance);
        ga.invokeInstanceMethod(Item.class, "getStringValue");
        ga.invokeStaticMethod(Whitespace.class, "trim", String.class);
        // rawName
        int rawNameVar = methodInfo.allocateLocal(String.class);
        ga.storeLocal(rawNameVar);
        ga.loadLocal(rawNameVar);
        // rawName
        if (namespace == null) {
            ga.dup();
            ga.push("xmlns");
            ga.invokeInstanceMethod(String.class, "equals", Object.class);
            ga.ifFalse(notEqual_rawName);
            ga.pop();
            compiler.generateParameterizedDynamicError(
                    new MessageTemplate(ga, "Invalid attribute name: {$1}", rawNameVar),
                    compAttr.isXSLT() ? "XTDE0855" : "XQDY0044", compAttr.getLocation(), false);
            ga.goTo(end);
            methodInfo.placeLabel(notEqual_rawName);
        }
        // rawName
        ga.dup();
        ga.push("Q{");
        ga.invokeInstanceMethod(String.class, "startsWith", String.class);
        ga.ifFalse(notEQName);
        ga.loadLocal(rawNameVar);
        ga.push(compAttr.isXSLT() ? "XTDE0860" : "XQDY0044");
        ga.invokeStaticMethod(Callback.class, "sqNameFromValidatedEQName", String.class, String.class);
        ga.dup();
        ga.invokeInstanceMethod(StructuredQName.class, "getLocalPart");
        ga.storeLocal(localNameVar);
        ga.invokeInstanceMethod(StructuredQName.class, "getURI");
        ga.storeLocal(uriVar);
        ga.push("_");
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
        ga.loadLocal(prefixVar);
        ga.push("xmlns");
        ga.invokeInstanceMethod(String.class, "equals", Object.class);
        ga.ifFalse(notEqual_prefix);
        if (namespace == null) {
            compiler.generateParameterizedDynamicError(
                    new MessageTemplate(ga, "Invalid attribute name: {$1}", rawNameVar),
                    compAttr.isXSLT() ? "XTDE0860" : "XQDY0044", compAttr.getLocation(), false);
            ga.goTo(end);
        } else {
            ga.push("");
            ga.storeLocal(prefixVar);
        }
        methodInfo.placeLabel(notEqual_prefix);

        ga.goTo(test1);

        // Catch the QNameException
        methodInfo.placeLabel(catchBegin);
        compiler.generateParameterizedDynamicError(
                new MessageTemplate(ga, "Invalid attribute name: {$1}", rawNameVar),
                compAttr.isXSLT() ? "XTDE0850" : "XQDY0074", compAttr.getLocation(), false);


        methodInfo.placeLabel(notStrInstance);
        //atomicValue on stack
        if (compAttr.isAllowNameAsQName()) {
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
            allocateStatic(compiler, NamespaceUri.NULL);
            ga.storeLocal(uriVar);
            methodInfo.placeLabel(notNull2);
            ga.loadLocal(localNameVar);
            ga.push("xmlns");
            ga.invokeInstanceMethod(String.class, "equals", Object.class);
            ga.ifFalse(qnameOK);
            ga.loadLocal(uriVar);
            ga.invokeInstanceMethod(NamespaceUri.class,"toString");
            ga.invokeInstanceMethod(String.class, "length");
            ga.push(0);
            ga.ifICmp(Generator.NE, qnameOK.label());
            compiler.generateDynamicError("Invalid attribute name: xmlns", "XQDY0044", compAttr.getLocation(), false);
            ga.goTo(end);
            methodInfo.placeLabel(qnameOK);
            ga.loadLocal(uriVar);
            ga.invokeInstanceMethod(NamespaceUri.class, "toString");
            ga.invokeInstanceMethod(String.class, "length");
            ga.push(0);
            ga.ifICmp(Generator.NE, uriGTZero.label());
            ga.pop();
            ga.push("");
            ga.storeLocal(prefixVar);
            ga.goTo(test2);

            methodInfo.placeLabel(uriGTZero);
            ga.checkCast(Type.getType(QNameValue.class));
            ga.invokeInstanceMethod(QNameValue.class, "getPrefix");
            ga.storeLocal(prefixVar);
            ga.loadLocal(prefixVar);
            ga.invokeInstanceMethod(String.class, "length");
            ga.push(0);
            ga.ifICmp(Generator.NE, prefixOK.label());
            compiler.generateGetContext();
            ga.invokeInstanceMethod(XPathContext.class, "getNamePool");
            ga.loadLocal(uriVar);
            ga.invokeInstanceMethod(NamePool.class, "suggestPrefixForURI", NamespaceUri.class);
            ga.storeLocal(prefixVar);
            ga.loadLocal(prefixVar);
            ga.ifNonNull(prefixOK.label());
            ga.push("ns0");
            ga.storeLocal(prefixVar);
            methodInfo.placeLabel(prefixOK);


            ga.loadLocal(uriVar);
            allocateStatic(compiler, NamespaceUri.XML);
            ga.invokeInstanceMethod(NamespaceUri.class, "equals", Object.class);
            ga.push("xml");
            ga.loadLocal(prefixVar);
            ga.invokeInstanceMethod(String.class, "equals", Object.class);
            ga.ifCmp(Type.getType(Boolean.TYPE), Generator.EQ, test2.label());

            ga.loadLocal(prefixVar);
            ga.push(compAttr.isXSLT());
            ga.push(compAttr.getLocation().getSystemId());
            ga.push(compAttr.getLocation().getLineNumber());
            ga.invokeStaticMethod(Callback.class, "makeXmlNamespaceException",
                                  String.class, Boolean.TYPE, String.class, Integer.TYPE);
            ga.throwException();
            //ga.goTo(end);

            methodInfo.placeLabel(test2);
            ga.push("xmlns");
            ga.loadLocal(prefixVar);
            ga.invokeInstanceMethod(String.class, "equals", Object.class);
            ga.ifFalse(test1);
            compiler.generateDynamicError("Invalid attribute namespace: http://www.w3.org/2000/xmlns/", "XQDY0044", compAttr.getLocation(), false);
            ga.goTo(doneAtt);

            methodInfo.placeLabel(notQNameInstance);
            compiler.generateDynamicError("Attribute name must be either a string or a QName", "XQDY0004", compAttr.getLocation(), false);
            ga.goTo(end);
        } else {
            compiler.generateDynamicError("Attribute name must be either a string or a QName", "XQDY0004", compAttr.getLocation(), false);
            ga.goTo(end);
        }

        methodInfo.placeLabel(test1);


        if (namespace == null) {
            ga.loadLocal(uriVar);
            ga.ifNonNull(uriNotNull.label());
            ga.loadLocal(prefixVar);
            ga.invokeInstanceMethod(String.class, "length");
            ga.push(0);
            ga.ifICmp(Generator.NE, notEqual.label());
            allocateStatic(compiler, NamespaceUri.NULL);
            ga.storeLocal(uriVar);
            methodInfo.placeLabel(notEqual);


            ExpressionCompiler.allocateStatic(compiler, compAttr.getNamespaceResolver());
            ga.loadLocal(prefixVar);
            ga.push(false);
            ga.invokeInstanceMethod(NamespaceResolver.class, "getURIForPrefix", String.class, Boolean.TYPE);
            ga.dup();
            ga.ifNonNull(notNull3.label());
            ga.pop();
            compiler.generateParameterizedDynamicError(
                    new MessageTemplate(ga, "Undeclared prefix in attribute name: {$1}", prefixVar),
                    compAttr.isXSLT() ? "XTDE0860" : "XQDY0074", compAttr.getLocation(), false);
            ga.goTo(end);
            methodInfo.placeLabel(notNull3);
            ga.storeLocal(uriVar);
            methodInfo.placeLabel(uriNotNull);
        } else {
            ga.loadLocal(uriVar);
            ga.ifNonNull(uriNotNull.label());
            if (namespace instanceof StringLiteral) {
                ga.push(((StringLiteral) namespace).stringify());
                ga.invokeStaticMethod(NamespaceUri.class, "of",String.class);
                ga.storeLocal(uriVar);

            } else {
                compiler.compileToPrimitive(namespace, String.class, OnEmpty.RETURN_NULL);
                ga.invokeStaticMethod(NamespaceUri.class, "of", String.class);
                ga.storeLocal(uriVar);

                ga.invokeStaticMethod(StandardURIChecker.class, "getInstance");
                ga.loadLocal(uriVar);
                ga.invokeInstanceMethod(NamespaceUri.class, "toString");
                ga.invokeInstanceMethod(StandardURIChecker.class, "isValidURI", String.class);
                ga.ifTrue(uriNotNull);
                compiler.generateDynamicError("The value of the namespace attribute must be a valid URI", "XTDE0865", compAttr.getLocation(), false);
                ga.goTo(end);
            }
            methodInfo.placeLabel(uriNotNull);
//            ga.loadLocal(uriVar);
//            ga.checkCast(Type.getType(String.class));
//            ga.storeLocal(uriVar);
            ga.loadLocal(uriVar);
            ga.invokeInstanceMethod(NamespaceUri.class, "toString");
            ga.invokeInstanceMethod(String.class, "length");
            ga.push(0);
            ga.ifCmp(Type.getType(Integer.TYPE), Generator.NE, notEqual2.label());
            ga.push("");
            ga.storeLocal(prefixVar);
            ga.goTo(doneAtt);

            methodInfo.placeLabel(notEqual2);
            ga.loadLocal(prefixVar);
            ga.invokeInstanceMethod(String.class, "length");
            ga.ifZCmp(Generator.NE, doneAtt.label());
            // prefix = pool.suggestPrefixForURI(uri);
            allocateStatic(compiler, compiler.getConfiguration().getNamePool());
            ga.loadLocal(uriVar);
            ga.invokeInstanceMethod(NamePool.class, "suggestPrefixForURI", NamespaceUri.class);
            ga.storeLocal(prefixVar);
            ga.loadLocal(prefixVar);
            ga.ifNonNull(doneAtt.label());
            // this isn't a legal prefix so we mustn't use it
            ga.push("ns0");
            ga.storeLocal(prefixVar);

        }
        methodInfo.placeLabel(doneAtt);
        ga.loadLocal(uriVar);
        allocateStatic(compiler, NamespaceUri.XMLNS);
        ga.invokeInstanceMethod(NamespaceUri.class, "equals", Object.class);
        ga.ifFalse(done1);
        compiler.generateParameterizedDynamicError(
                new MessageTemplate(ga, "Cannot create attribute in namespace {$1}", uriVar),
                compAttr.isXSLT() ? "XTDE0865" : "XQDY0044", compAttr.getLocation(), false);
        ga.goTo(end);

        methodInfo.placeLabel(done1);
        ga.newInstance(FingerprintedQName.class);
        ga.dup();

        ga.loadLocal(prefixVar);
        ga.loadLocal(uriVar);
        ga.loadLocal(localNameVar);
        ga.invokeConstructor(FingerprintedQName.class, String.class, NamespaceUri.class, String.class);
        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(localNameVar);
        methodInfo.releaseLocal(uriVar);
        methodInfo.releaseLocal(prefixVar);
        methodInfo.releaseLocal(rawNameVar);
    }

}

// Copyright (c) 2011-2022 Saxonica Limited