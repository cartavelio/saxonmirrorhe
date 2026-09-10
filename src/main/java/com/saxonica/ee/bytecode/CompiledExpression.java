package com.saxonica.ee.bytecode;

import net.sf.saxon.event.Outputter;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.elab.*;
import net.sf.saxon.expr.parser.PathMap;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.SchemaType;

import java.util.Iterator;

/**
 * An expression that has been compiled to Java bytecode. The actual bytecode is contained
 * in a separate object of type {@link GeneratedCode}, to which evaluation is delegated
 */
public class CompiledExpression extends Expression {

    private final GeneratedCode byteCode;
    private final Expression original; // this link is not considered an operand, and cannot be changed
    private String className;

    public CompiledExpression(Expression original, GeneratedCode byteCode) {
        this.original = original;
        this.byteCode = byteCode;
        setRetainedStaticContext(original.getRetainedStaticContext());
    }

    /**
     * Get the original (uncompiled) expression of which this is the compiled form
     * @return the original expression
     */

    @Override
    public Expression getInterpretedExpression() {
        return original;
    }

    public GeneratedCode getGeneratedCode(){
        return byteCode;
    }

    public void setClassName(String cname){
        className = cname;
    }

    public String getClassName(){
        return className;
    }

    @Override
    public String getExpressionName() {
        return getInterpretedExpression().getExpressionName();
    }

    @Override
    public int getImplementationMethod() {
        return getInterpretedExpression().getImplementationMethod();
    }

    @Override
    public int getCardinality() {
        return getInterpretedExpression().getCardinality();
    }

    /*@NotNull*/
    @Override
    public ItemType getItemType() {
        return getInterpretedExpression().getItemType();
    }

    @Override
    public int getDependencies() {
        return getInterpretedExpression().getDependencies();
    }

    @Override
    public void process(Outputter output, XPathContext context) throws XPathException {
        try {
            byteCode.process(output, context);
        } catch (XPathException e) {
            e.maybeSetContext(context);
            e.maybeSetLocation(getInterpretedExpression().getLocation());
            throw e;
        } catch (UncheckedXPathException uxe) {
            XPathException e = uxe.getXPathException();
            e.maybeSetContext(context);
            e.maybeSetLocation(getInterpretedExpression().getLocation());
            throw e;
        }
    }

//    @Override
//    public void evaluatePendingUpdates(XPathContext context, PendingUpdateList pul) throws XPathException {
//        getInterpretedExpression().evaluatePendingUpdates(context, pul);
//    }

    @Override
    public String toString() {
        return getInterpretedExpression().toString();
    }

    @Override
    public void export(/*@NotNull*/ ExpressionPresenter out) throws XPathException {
        getInterpretedExpression().export(out);
    }

    @Override
    public void checkPermittedContents(SchemaType parentType, boolean whole) throws XPathException {
        getInterpretedExpression().checkPermittedContents(parentType, whole);
    }

    @Override
    protected int computeCardinality() {
        return getInterpretedExpression().getCardinality();
    }

    @Override
    protected int computeSpecialProperties() {
        return getInterpretedExpression().getSpecialProperties();
    }

    @Override
    public int computeDependencies() {
        return getInterpretedExpression().getDependencies();
    }

    @Override
    public int getIntrinsicDependencies() {
        return getInterpretedExpression().getIntrinsicDependencies();
    }

    @Override
    public void checkForUpdatingSubexpressions() throws XPathException {
        getInterpretedExpression().checkForUpdatingSubexpressions();
    }

    @Override
    public boolean isUpdatingExpression() {
        return getInterpretedExpression().isUpdatingExpression();
    }

    @Override
    public boolean isVacuousExpression() {
        return getInterpretedExpression().isVacuousExpression();
    }

    /*@NotNull*/
    @Override
    public Expression copy(RebindingMap rebindings) {
        return getInterpretedExpression().copy(rebindings);
    }

    @Override
    public StructuredQName getObjectName() {
        return getInterpretedExpression().getObjectName();
    }

    /**
     * Is this expression the same as another expression?
     *
     * @param other the expression to be compared with this one
     * @return true if the two expressions are statically equivalent
     */
    @Override
    public boolean equals(Object other) {
        return other instanceof CompiledExpression &&
                ((CompiledExpression)other).getInterpretedExpression().isEqual(getInterpretedExpression())
                && ((CompiledExpression)other).byteCode.equals(byteCode);
    }

    /**
     * Hashcode supporting equals()
     */

    @Override
    protected int computeHashCode() {
        int h = 0x833b12a0;
        return h + getInterpretedExpression().hashCode() + byteCode.hashCode();
    }

    @Override
    public Object getProperty(String name) {
        return getInterpretedExpression().getProperty(name);
    }

    @Override
    public Iterator<String> getProperties() {
        return getInterpretedExpression().getProperties();
    }


    @Override
    public PathMap.PathMapNodeSet addToPathMap(PathMap pathMap, PathMap.PathMapNodeSet pathMapNodeSet) {
        return getInterpretedExpression().addToPathMap(pathMap, pathMapNodeSet);
    }

    @Override
    public boolean isSubtreeExpression() {
        return getInterpretedExpression().isSubtreeExpression();
    }

    /*@NotNull*/
    @Override
    public SequenceIterator iterate(XPathContext context) throws XPathException {
        try {
            return byteCode.iterate(context);
        } catch (XPathException e) {
            e.maybeSetLocation(getInterpretedExpression().getLocation());
            throw e;
        } catch (UncheckedXPathException e) {
            XPathException xe = e.getXPathException();
            xe.maybeSetLocation(getInterpretedExpression().getLocation());
            throw e;
        }
    }

    @Override
    public Item evaluateItem(XPathContext context) throws XPathException {
        try {
            return byteCode.evaluateItem(context);
        } catch (XPathException e) {
            e.maybeSetLocation(getInterpretedExpression().getLocation());
            throw e;
        } catch (UncheckedXPathException e) {
            XPathException xe = e.getXPathException();
            xe.maybeSetLocation(getInterpretedExpression().getLocation());
            throw xe;
        }
    }

    @Override
    public boolean effectiveBooleanValue(XPathContext context) throws XPathException {
        try {
            return byteCode.effectiveBooleanValue(context);
        } catch (XPathException e) {
            e.maybeSetLocation(getInterpretedExpression().getLocation());
            throw e;
        } catch (UncheckedXPathException e) {
            XPathException xe = e.getXPathException();
            xe.maybeSetLocation(getInterpretedExpression().getLocation());
            throw e;
        }
    }

    public Elaborator getElaborator() {
        return new CompiledExpressionElaborator();
    }

    private static class CompiledExpressionElaborator extends PushElaborator {

        @Override
        public PushEvaluator elaborateForPush() {
            CompiledExpression expr = (CompiledExpression) getExpression();
            return (output, context) -> {
                try {
                    expr.byteCode.process(output, context);
                    return null;
                } catch (XPathException e) {
                    e.maybeSetContext(context);
                    e.maybeSetLocation(expr.getInterpretedExpression().getLocation());
                    throw e;
                } catch (UncheckedXPathException uxe) {
                    XPathException e = uxe.getXPathException();
                    e.maybeSetContext(context);
                    e.maybeSetLocation(expr.getInterpretedExpression().getLocation());
                    throw e;
                }
            };
        }

        @Override
        public ItemEvaluator elaborateForItem() {
            CompiledExpression expr = (CompiledExpression) getExpression();
            return context -> {
                try {
                    return expr.byteCode.evaluateItem(context);
                } catch (XPathException e) {
                    e.maybeSetContext(context);
                    e.maybeSetLocation(expr.getInterpretedExpression().getLocation());
                    throw e;
                } catch (UncheckedXPathException uxe) {
                    XPathException e = uxe.getXPathException();
                    e.maybeSetContext(context);
                    e.maybeSetLocation(expr.getInterpretedExpression().getLocation());
                    throw e;
                }
            };
        }

        @Override
        public PullEvaluator elaborateForPull() {
            CompiledExpression expr = (CompiledExpression) getExpression();
            return context -> {
                try {
                    return expr.byteCode.iterate(context);
                } catch (XPathException e) {
                    e.maybeSetContext(context);
                    e.maybeSetLocation(expr.getInterpretedExpression().getLocation());
                    throw e;
                } catch (UncheckedXPathException uxe) {
                    XPathException e = uxe.getXPathException();
                    e.maybeSetContext(context);
                    e.maybeSetLocation(expr.getInterpretedExpression().getLocation());
                    throw e;
                }
            };
        }

        @Override
        public BooleanEvaluator elaborateForBoolean() {
            CompiledExpression expr = (CompiledExpression) getExpression();
            return context -> {
                try {
                    return expr.byteCode.effectiveBooleanValue(context);
                } catch (XPathException e) {
                    e.maybeSetContext(context);
                    e.maybeSetLocation(expr.getInterpretedExpression().getLocation());
                    throw e;
                } catch (UncheckedXPathException uxe) {
                    XPathException e = uxe.getXPathException();
                    e.maybeSetContext(context);
                    e.maybeSetLocation(expr.getInterpretedExpression().getLocation());
                    throw e;
                }
            };
        }
    }
}

// Copyright (c) 2011-2022 Saxonica Limited




