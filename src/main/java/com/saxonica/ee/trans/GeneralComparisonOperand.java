package com.saxonica.ee.trans;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.ItemConsumer;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.z.IntEmptySet;
import net.sf.saxon.z.IntHashSet;
import net.sf.saxon.z.IntSet;
import net.sf.saxon.z.IntSingletonSet;

/**
 * Given an expression, this characteristic evaluates the set of hash keys
 * corresponding to the string values of the items selected by that expression
 */
public class GeneralComparisonOperand extends Characteristic {

    Expression exp;
    boolean multiValued;
    StringCollator collator;

    public GeneralComparisonOperand(StringCollator collator, Expression exp) {
        this.collator = collator;
        this.exp = exp;
        this.multiValued = Cardinality.allowsMany(exp.getCardinality());
    }

    @Override
    public IntSet evaluate(XPathContext context) throws XPathException {
        if (multiValued) {
            SequenceIterator iter = exp.iterate(context);
            IntHashSet keys = new IntHashSet();
            SequenceTool.supply(iter, (ItemConsumer<? super Item>) item -> {
                keys.add(collator.getCollationKey(item.getUnicodeStringValue()).hashCode());
            });
            return keys;
        } else {
            Item item = exp.evaluateItem(context);
            if (item == null) {
                return IntEmptySet.getInstance();
            } else {
                return new IntSingletonSet(collator.getCollationKey(item.getUnicodeStringValue()).hashCode());
            }
        }
    }

    @Override
    public int hashCode() {
        return exp.hashCode();
    }

    @Override
    public boolean equals(Object obj) {
        return obj instanceof GeneralComparisonOperand &&
                exp.isEqual(((GeneralComparisonOperand)obj).exp) &&
                collator.equals(((GeneralComparisonOperand) obj).collator);
    }

    @Override
    public String toString() {
        return exp.toString();
    }

}

// Copyright (c) 2009-2023 Saxonica Limited
