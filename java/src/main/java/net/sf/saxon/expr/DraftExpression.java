package net.sf.saxon.expr;

import net.sf.saxon.type.AnyItemType;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.SequenceType;

/**
 * A DraftExpression is a temporary expression constructed in the course of parsing, that must
 * be substituted by a real expression before it can be evaluated. It is used, for example, for
 * functions calls whose names cannot yet be resolved to an actual function
 */
public abstract class DraftExpression extends Expression {

    @Override
    public ItemType getItemType() {
        return AnyItemType.getInstance();
    }

    @Override
    public SequenceType getStaticType() {
        return SequenceType.ANY_SEQUENCE;
    }

    @Override
    public UType getStaticUType(UType contextItemType) {
        return UType.ANY;
    }

    /**
     * Compute the special properties of this expression. These properties are denoted by a bit-significant
     * integer, possible values are in class {@link StaticProperty}. The "special" properties are properties
     * other than cardinality and dependencies, and most of them relate to properties of node sequences, for
     * example whether the nodes are in document order.
     *
     * @return the special properties, as a bit-significant integer
     */

    @Override
    protected int computeSpecialProperties() {
        return 0;
    }
}

