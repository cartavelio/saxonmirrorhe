package com.saxonica.ee.trans;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.lib.StringCollator;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.om.SequenceTool;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.Whitespace;
import net.sf.saxon.z.IntHashSet;
import net.sf.saxon.z.IntSet;

/**
 * Given an expression, this characteristic evaluates the set of hash keys
 * corresponding to the whitespace-separated tokens in the string values of the
 * items selected by that expression.
 * <p>It is used typically in optimizing patterns in DITA stylesheets, where the pattern
 * takes the form *[@class = ' topic/li '] or similar. In this example the characteristic
 * will be obtained by tokenizing the value of @class on whitespace boundaries, and finding
 * the hash code of the collation key of each of these tokens. This set of hash codes is
 * then compared with the hashcode of the constant string ' topic/li '. The benefit comes
 * only when a rule chain contains many rules with this same general structure, but different
 * constant strings; the same set of hash codes will then be compared against each of the
 * string constants in turn.</p>
 */
public class TokenMatchOperand extends Characteristic {

    Expression exp;

    /**
     * Create a token match characteristic
     * @param exp the expression to be tokenized. In the case
     *            of DITA-OT stylesheets, this expression will generally be "@class".
     */

    public TokenMatchOperand(Expression exp) {
        this.exp = exp;
    }

    @Override
    public IntSet evaluate(XPathContext context) throws XPathException {
        SequenceIterator iter = exp.iterate(context);
        IntHashSet keys = new IntHashSet();
        StringCollator collator = CodepointCollator.getInstance();
        SequenceTool.supply(iter, item -> {
            Whitespace.Tokenizer seq = new Whitespace.Tokenizer(item.getUnicodeStringValue());
            Item token;
            while ((token = seq.next()) != null) {
                keys.add(collator.getCollationKey(token.getUnicodeStringValue()).hashCode());
            }
        });
        return keys;
    }

    @Override
    public int hashCode() {
        return exp.hashCode();
    }

    @Override
    public boolean equals(Object obj) {
        return obj instanceof TokenMatchOperand &&
                exp.isEqual(((TokenMatchOperand)obj).exp);
    }

    @Override
    public String toString() {
        return exp.toString();
    }

}

// Copyright (c) 2018-2022 Saxonica Limited