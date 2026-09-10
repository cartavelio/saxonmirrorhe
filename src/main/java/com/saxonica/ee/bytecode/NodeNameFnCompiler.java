package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.SystemFunctionCall;
import net.sf.saxon.functions.NodeName_1;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.type.AtomicType;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.QNameValue;
import org.objectweb.asm.Type;

/**
 * Generate bytecode for evaluation of the node-name() function
 */
public class NodeNameFnCompiler extends ToItemCompiler {

    /**
     * Generate bytecode to evaluate the expression as an Item
     * Precondition: none.
     * Postcondition: at execution time, the stack contains either an item (the result of the expression), or null
     * (representing an empty sequence).
     *  @param compiler   the compiler service
     * @param expression the expression to be compiled
     * @return the class of item generated
     */
    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, Expression expression) throws CannotCompileException {
        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        LabelInfo nodeNameReturn = methodInfo.newLabel("nodeNameReturn");
        SystemFunctionCall fnc = (SystemFunctionCall) expression;
        final NodeName_1 fn = (NodeName_1) fnc.getTargetFunction();
        ItemType argType = fnc.getArg(0).getItemType();
        boolean alwaysNamed = th.isSubType(argType, NodeKindTest.ELEMENT) ||
                th.isSubType(argType, NodeKindTest.ATTRIBUTE);
        Class<? extends Item> argClass = compiler.compileToItem(fnc.getArg(0));
        if (Cardinality.allowsZero(fnc.getArg(0).getCardinality())) {
            LabelInfo nodeNotNull = methodInfo.newLabel("nodeNotNull");
            ga.dup();
            ga.ifNonNull(nodeNotNull.label());
            ga.goTo(nodeNameReturn);
            methodInfo.placeLabel(nodeNotNull);
        }
        argClass = ga.downCast(argClass, NodeInfo.class, true);
        int nodeVar = ga.newLocal(argClass);
        ga.storeLocal(nodeVar);

//        if (node.getLocalPart().isEmpty()) {
//            return null;
//        }
//        return new QNameValue(node.getPrefix(), node.getURI(), node.getLocalPart(), BuiltInAtomicType.QNAME);


        int lnameVar = ga.newLocal(String.class);
        ga.loadLocal(nodeVar);
        ga.invokeInstanceMethod(argClass, "getLocalPart");
        ga.storeLocal(lnameVar);
        if (!alwaysNamed) {
            LabelInfo nodeIsNamed = methodInfo.newLabel("nodeIsNamed");
            ga.loadLocal(lnameVar);
            ga.invokeInstanceMethod(String.class, "isEmpty");
            ga.ifFalse(nodeIsNamed);
            ga.pushNull();
            ga.goTo(nodeNameReturn);
            methodInfo.placeLabel(nodeIsNamed);
        }

        ga.newInstance(Type.getType(QNameValue.class));
        ga.dup();
        ga.loadLocal(nodeVar);
        ga.invokeInstanceMethod(argClass, "getPrefix");
        ga.loadLocal(nodeVar);
        ga.invokeInstanceMethod(argClass, "getURI");
        ga.loadLocal(lnameVar);
        allocateStatic(compiler, BuiltInAtomicType.QNAME);

        ga.invokeConstructor(QNameValue.class, String.class, String.class, String.class, AtomicType.class);

        methodInfo.placeLabel(nodeNameReturn);

        methodInfo.releaseLocal(nodeVar);
        methodInfo.releaseLocal(lnameVar);
        return QNameValue.class;
    }
}

// Copyright (c) 2011-2022 Saxonica Limited
