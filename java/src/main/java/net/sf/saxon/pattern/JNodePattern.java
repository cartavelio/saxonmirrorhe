package net.sf.saxon.pattern;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.parser.RebindingMap;
import net.sf.saxon.ma.jnode.AnyJNodeType;
import net.sf.saxon.ma.jnode.JNode;
import net.sf.saxon.om.Item;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ItemType;
import net.sf.saxon.type.UType;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.SequenceType;

public class JNodePattern extends Pattern {

    private final AtomicValue selector;
    private final SequenceType contentType;

    public JNodePattern(AtomicValue selector, SequenceType contentType) {
        this.selector = selector;
        this.contentType = contentType;
    }

    @Override
    public boolean matches(Item item, XPathContext context) throws XPathException {
        return item instanceof JNode jnode &&
                (selector == null || jnode.getSelector().asMapKey(40).equals(selector.asMapKey(40))) &&
                contentType.matches(jnode.getContent());
    }

    @Override
    public UType getUType() {
        return UType.JNODE;
    }

    @Override
    public ItemType getItemType() {
        return AnyJNodeType.getInstance();
    }


    @Override
    public void export(ExpressionPresenter presenter) throws XPathException {
        throw new UnsupportedOperationException(); // TODO: implement this
    }

    @Override
    public double getDefaultPriority() {
        /*
            If the pattern has one or more predicates, then the default priority is +0.5.
            The default priority of the pattern jnode(*, *) is -1 (minus one).
            The default priority of the equivalent pattern jnode(*, item()*) is also -1 (minus one).
            The default priority of the pattern jnode(S, *), where S is any constant, is 0 (zero).
            The default priority of the equivalent pattern jnode(S, item()*), where S is any constant, is also 0 (zero).
            The default priority of the pattern jnode(*, T), where T is any sequence type other than item()*, is 0 (zero).
            The default priority of the pattern jnode(S, T), where S is any constant, and T is any sequence type other than item()*, is +0.25.
        */
        if (selector == null) {
            if (contentType.equals(SequenceType.ANY_SEQUENCE)) {
                return -1;
            } else {
                return 0;
            }
        } else if (contentType.equals(SequenceType.ANY_SEQUENCE)) {
            return 0;
        } else {
            return 0.25;
        }
    }

    @Override
    public Pattern copy(RebindingMap rebindings) {
        return this;
    }
}

