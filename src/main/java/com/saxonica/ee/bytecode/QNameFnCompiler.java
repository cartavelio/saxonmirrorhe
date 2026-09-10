package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.om.Item;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.om.NameChecker;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.QNameValue;
import org.objectweb.asm.Type;


/**
 * Generate bytecode for a call to fn:QName
 */
public class QNameFnCompiler extends ToItemCompiler {

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {

        final Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo mi = compiler.getCurrentMethod();
        LabelInfo notNull = mi.newLabel("qnameNotNull");
        LabelInfo uriLab = mi.newLabel("uriLab");
        LabelInfo end = mi.newLabel("endTryCatch");
        LabelInfo nameOK = mi.newLabel("nameOK");
        LabelInfo L0 = mi.newLabel("L0");
        LabelInfo L1 = mi.newLabel("L1");
        LabelInfo L0_1 = mi.newLabel("L0_1");
        LabelInfo L1_1 = mi.newLabel("L1_1");
        LabelInfo L2 = mi.newLabel("L2");
        LabelInfo L3 = mi.newLabel("L3");
        final SystemFunctionCall fn = (SystemFunctionCall) expression;
        compiler.compileToItem(fn.getArg(0));
        ga.checkClass(AtomicValue.class);
        ga.dup();
        ga.ifNonNull(notNull.label());
        ga.pop();
        ga.pushNull();
        ga.goTo(uriLab);
        mi.placeLabel(notNull);
        ga.invokeInstanceMethod(AtomicValue.class, "getStringValue");
        mi.placeLabel(uriLab);
        int uriVar = mi.allocateLocal(String.class);
        ga.storeLocal(uriVar);

        ga.visitTryCatchBlock(L0, L1, L2, "net/sf/saxon/om/QNameException");
        ga.visitTryCatchBlock(L0_1, L1_1, L3, "net/sf/saxon/trans/XPathException");
        /* try block*/
        mi.placeLabel(L0);
        mi.placeLabel(L0_1);


        ga.newInstance(QNameValue.class);
        ga.dup();
        compiler.compileToPrimitive(fn.getArg(1), String.class, OnEmpty.RETURN_EMPTY_STRING);

        ga.invokeStaticMethod(NameChecker.class, "getQNameParts", String.class);

        final int partsVar = mi.allocateLocal(String[].class);
        ga.storeLocal(partsVar);

        ga.loadLocal(partsVar);
        ga.push(0);
        ga.arrayLoad(Type.getType(String.class));
        ga.invokeInstanceMethod(String.class, "length");
        ga.ifZCmp(Generator.EQ, nameOK.label());
        ga.loadLocal(partsVar);
        ga.push(0);
        ga.arrayLoad(Type.getType(String.class));
        ga.invokeStaticMethod(NameChecker.class, "isValidNCName", String.class);
        ga.ifTrue(nameOK);

        ga.loadLocal(partsVar);
        ga.push(0);
        ga.arrayLoad(Type.getType(String.class));
        compiler.generateParameterizedDynamicError(
                new MessageTemplate(ga, "Malformed prefix in QName: ($1}", -1),
                "FOCA0002", expression.getLocation(), false);
        ga.goTo(end);


        mi.placeLabel(nameOK);
        ga.loadLocal(partsVar);
        ga.push(0);
        ga.arrayLoad(Type.getType(String.class));
        ga.loadLocal(uriVar);
        ga.loadLocal(partsVar);
        ga.push(1);
        ga.arrayLoad(Type.getType(String.class));
        allocateStatic(compiler, BuiltInAtomicType.QNAME);
        ga.push(1);
        ga.invokeConstructor(QNameValue.class, String.class, String.class, String.class, AtomicType.class, Boolean.TYPE);

        mi.placeLabel(L1);
        mi.placeLabel(L1_1);
        ga.goTo(end.label());

        // catch block for QNameException
        mi.placeLabel(L2);
        ga.invokeInstanceMethod(Exception.class, "getMessage");
        compiler.generateParameterizedDynamicError(MessageFromStack.getInstance(), "FOCA0002", expression.getLocation(), false);
        ga.goTo(end.label());


        // catch block for XPathException
        mi.placeLabel(L3);
        ga.dup();
        allocateStatic(compiler, expression.getLocation());
        ga.invokeInstanceMethod(XPathException.class, "maybeSetLocation", Location.class);
        ga.throwException();

        mi.placeLabel(end);

        return QNameValue.class;


    }

}

// Copyright (c) 2011-2022 Saxonica Limited
