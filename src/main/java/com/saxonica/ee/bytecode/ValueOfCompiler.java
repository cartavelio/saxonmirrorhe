package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.ValueOf;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.om.Item;
import net.sf.saxon.tree.util.Orphan;
import net.sf.saxon.type.BuiltInAtomicType;
import net.sf.saxon.type.TypeHierarchy;
import net.sf.saxon.value.Cardinality;
import org.objectweb.asm.Type;

/**
 * Generate code for a ValueOf instruction
 */
public class ValueOfCompiler extends SimpleNodeConstructorCompiler {


    /**
     * Generate bytecode to get the ValueOf text from the instruction
     * Precondition: none.
     * Postcondition: the stack contains an item from evaluating this instruction
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    public void compileToItem(CompilerService compiler, Expression expression)
            throws CannotCompileException {
        Configuration config = compiler.getConfiguration();
        TypeHierarchy th = config.getTypeHierarchy();
        ValueOf valueOf = (ValueOf) expression;
        Expression select = valueOf.getSelect();
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitAnnotation(compiler, "ValueOfCompiler");

        LabelInfo notNull = methodInfo.newLabel("notNullValOf");
        LabelInfo done = methodInfo.newLabel("doneValOf");
        LabelInfo end = methodInfo.newLabel("endValOf");
        int valVar = methodInfo.allocateLocal(CharSequence.class);
        if (th.isSubType(select.getItemType(), BuiltInAtomicType.STRING) &&
                !Cardinality.allowsZero(select.getCardinality())) {
            compiler.compileToPrimitive(select, CharSequence.class, OnEmpty.RETURN_EMPTY_STRING);
            ga.storeLocal(valVar);
        } else {
            compiler.compileToItem(select);
            if (Cardinality.allowsZero(select.getCardinality())) {
                ga.dup();
                ga.ifNonNull(notNull.label());
                ga.pop();
                if (valueOf.isNoNodeIfEmpty()) {
                    ga.pushNull();
                    ga.goTo(end);
                } else {
                    ga.push("");
                    ga.storeLocal(valVar);
                    ga.goTo(done);
                }
                methodInfo.placeLabel(notNull);
            }
            visitLineNumber(compiler, ga, expression);
            ga.invokeInstanceMethod(Item.class, "getStringValueCS");
            ga.storeLocal(valVar);
            methodInfo.placeLabel(done);
        }
        ga.newInstance(Type.getType(Orphan.class));
        ga.dup();
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getConfiguration");
        ga.invokeConstructor(Orphan.class, Configuration.class);
        int oVar = methodInfo.allocateLocal(Orphan.class);
        ga.storeLocal(oVar);
        ga.loadLocal(oVar);
        ga.push(net.sf.saxon.type.Type.TEXT);
        ga.invokeInstanceMethod(Orphan.class, "setNodeKind", Short.TYPE);
        ga.loadLocal(oVar);
        ga.loadLocal(valVar);
        ga.invokeInstanceMethod(Orphan.class, "setStringValue", CharSequence.class);
        ga.loadLocal(oVar);

        methodInfo.placeLabel(end);

        methodInfo.releaseLocal(valVar);
        methodInfo.releaseLocal(oVar);
    }

    /**
     * Overridable method to ask whether no node should be produced in the case that the content is empty
     *
     * @return true if the rules for this instruction are that when the content is empty, no node is generated
     */

    protected boolean isNoNodeIfEmpty(Expression expr) {
        return ((ValueOf) expr).isNoNodeIfEmpty();
    }

    /**
     * Generate code to write the node to the current receiver. On entry the stack holds the
     * current receiver, the node name (for named node kinds only) and the content (for all nodes)
     *
     * @param compiler   the compiler service
     * @param expression the expression to be compiled
     */
    @Override
    protected void pushNode(CompilerService compiler, Expression expression) {
        ValueOf expr = (ValueOf) expression;
        Generator ga = compiler.getCurrentGenerator();
        allocateStatic(compiler, expr.getLocation());
        ga.push(expr.getOptions());
        ga.invokeInstanceMethod(Receiver.class, "characters", CharSequence.class, Location.class, Integer.TYPE);
    }
}

//Copyright (c) 2011 Saxonica Limited.