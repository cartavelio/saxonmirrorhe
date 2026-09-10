// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

public class JNodePattern extends Pattern {

    private final AtomicValue selector;
    private final SequenceMatcher contentPattern;

    public JNodePattern(AtomicValue selector, SequenceMatcher contentPattern) {
        this.selector = selector;
        this.contentPattern = contentPattern;
    }

    @Override
    public boolean matches(Item item, XPathContext context) throws XPathException {
        return item instanceof JNode jnode &&
                (selector == null || (jnode.getSelector() != null && jnode.getSelector().asMapKey(40).equals(selector.asMapKey(40))))
                && contentPattern.matches(jnode.getContent(), context);
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
        presenter.startElement("JNODE-PATTERN");   // TODO
        presenter.endElement();
    }

    @Override
    public double getDefaultPriority() {
        /*
           If the pattern contains at least one predicate, then +0.5 [handled elsewhere].
           If the JNodeSelectorPattern is *, then the default priority of the contained pattern, ignoring any occurrence indicator.
           For example, the default priority of the pattern jnode(*, P*) is the default priority of P.
           Otherwise, the default priority of the contained pattern, ignoring any occurrence indicator, or zero, whichever is higher.
        */
        if (selector == null) {
            return contentPattern.getItemPattern().getDefaultPriority();
        } else {
            return Double.max(contentPattern.getItemPattern().getDefaultPriority(), 0);
        }
    }

    @Override
    public Pattern copy(RebindingMap rebindings) {
        return this;
    }
}

