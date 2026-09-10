package com.saxonica.ee.schema;

import net.sf.saxon.expr.parser.ContextItemStaticInfo;
import net.sf.saxon.expr.parser.ExpressionVisitor;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;

/**
 * The field selector for an IdentityConstraint
 */


public class IdentityField extends FieldOrSelector {

    private BuiltInAtomicType builtInBaseType;
    private boolean listValued;
    private boolean ascending;

    /**
     * Creates a new IdentityField.
     *
     * @param xpath the xpath for the IdentityField. Must not be null.
     */
    public IdentityField(String xpath) {
        setXPath(xpath);
    }

    public BuiltInAtomicType getBuiltInBaseType() {
        return builtInBaseType;
    }

    public boolean isListValued() {
        return listValued;
    }

    public void setBuiltInBaseType(BuiltInAtomicType builtInBaseType, boolean listValued) {
        this.builtInBaseType = builtInBaseType;
        this.listValued = listValued;
    }

    public void setAscending(boolean upwards) {
        this.ascending = upwards;
    }

    public boolean isAscending() {
        return ascending;
    }
    /**
     * Get the static cardinality of the expression, that is the number of nodes the field xpath will select
     * for each node selected by the selector xpath.
     *
     * @param visitor         the expression visitor
     * @param contextItemType the type of the context item, and information about whether a context item will
     *                        always be provided or not
     * @return the static cardinality
     * @throws net.sf.saxon.trans.XPathException
     *          if an error is found during the evaluation
     */

    public int getCardinality(ExpressionVisitor visitor, ContextItemStaticInfo contextItemType) throws XPathException {
        selectExpression = selectExpression.simplify()
                .typeCheck(visitor, contextItemType)
                .optimize(visitor, contextItemType);
        return selectExpression.getCardinality();
    }

}

// Copyright (c) 2004-2022 Saxonica Limited