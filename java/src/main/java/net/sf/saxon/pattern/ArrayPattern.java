package net.sf.saxon.pattern;

import net.sf.saxon.expr.Operand;
import net.sf.saxon.expr.OperandRole;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.ArrayItemType;
import net.sf.saxon.om.GroundedValue;
import net.sf.saxon.om.Item;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.UType;

/**
 * A pattern of the form array(SequencePattern), introduced in 4.0
 */

public class ArrayPattern extends Pattern {

    private final SequenceMatcher sequenceMatcher;

    public ArrayPattern(SequenceMatcher matcher) {
        this.sequenceMatcher = matcher;
    }

    @Override
    public Iterable<Operand> operands() {
        return new Operand(this, sequenceMatcher.getItemPattern(), OperandRole.SAME_FOCUS_ACTION);
    }

    @Override
    public boolean matches(Item item, XPathContext context) throws XPathException {
        if (!(item instanceof ArrayItem arrayItem)) {
            return false;
        }
        for (GroundedValue member : arrayItem.members()) {
            if (!sequenceMatcher.matches(member, context)) {
                return false;
            }
        }
        return true;
    }

    @Override
    public UType getUType() {
        return UType.FUNCTION;
    }

    @Override
    public ItemType getItemType() {
        return ArrayItemType.ANY_ARRAY_TYPE; // TODO: can do better
    }

    @Override
    public double getDefaultPriority() {
        return sequenceMatcher.getItemPattern().getDefaultPriority();
    }

    @Override
    public void export(ExpressionPresenter presenter) throws XPathException {
        throw new UnsupportedOperationException("Not yet implemented");
    }

    @Override
    public Pattern copy(RebindingMap rebindings) {
        return new ArrayPattern(sequenceMatcher.copy(rebindings));
    }
}

