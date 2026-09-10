package com.saxonica.ee.bytecode;

import com.saxonica.ee.bytecode.util.CompilerService;
import com.saxonica.ee.bytecode.util.GeneratedMethodInfo;
import com.saxonica.ee.bytecode.util.Generator;
import com.saxonica.ee.bytecode.util.LabelInfo;
import net.sf.saxon.expr.ContextItemExpression;
import net.sf.saxon.expr.CurrentItemExpression;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.Item;

/**
 * Generate code for a context item expression (that is, "."), and also for an XSLT
 * CurrentItemExpression (current()), which differs only in the error code it returns
 */
public class ContextItemCompiler extends ToItemCompiler {

    @Override
    public Class<? extends Item> compileToItem(CompilerService compiler, final Expression expression) {
        Generator ga = compiler.getCurrentGenerator();
        GeneratedMethodInfo methodInfo = compiler.getCurrentMethod();
        visitLineNumber(compiler, ga, expression);
        compiler.generateGetContext();
        ga.invokeInstanceMethod(XPathContext.class, "getContextItem");
        if (((ContextItemExpression) expression).isContextPossiblyUndefined()) {
            LabelInfo lab = methodInfo.newLabel("lab");
            ga.dup();
            ga.ifNonNull(lab.label());
            if (expression instanceof CurrentItemExpression) {
                compiler.generateDynamicError("There is no context item for current()", "XTDE1360", expression.getLocation(), false);
            } else {
                compiler.generateDynamicError("There is no context item", "XPDY0002", expression.getLocation(), false);
            }
            methodInfo.placeLabel(lab);
        }
        return Item.class;
    }

}

// Copyright (c) 2011-2022 Saxonica Limited


