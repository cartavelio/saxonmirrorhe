package com.saxonica.ee.trans;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.NumericValue;

/**
 * Given an expression, this characteristic evaluates the set of hash keys
 * corresponding to the string values of the items selected by that expression
 */
public class NumericComparisonOperand extends Characteristic {

    Expression exp;

    public NumericComparisonOperand(Expression exp) {
        this.exp = exp;
    }

    @Override
    public NumericValue evaluate(XPathContext context) throws XPathException {
        return (NumericValue)exp.evaluateItem(context);
    }

    @Override
    public int hashCode() {
        return exp.hashCode();
    }

    @Override
    public boolean equals(Object obj) {
        return obj instanceof NumericComparisonOperand &&
                exp.isEqual(((NumericComparisonOperand)obj).exp);
    }

    @Override
    public String toString() {
        return exp.toString();
    }

}

// Copyright (c) 2009-2022 Saxonica Limited