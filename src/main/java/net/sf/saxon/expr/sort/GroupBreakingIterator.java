////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2020 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr.sort;

import com.saxonica.ee.stream.ManualGroupIterator;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.tree.iter.LookaheadIterator;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.EnumSet;
import java.util.List;

/**
 * A GroupBreakingIterator iterates over a sequence of groups defined by
 * xsl:for-each-group break-when="x". The groups are returned in
 * order of first appearance.
 */

public class GroupBreakingIterator implements LookaheadIterator, GroupIterator {

    private final Expression select;
    private final FocusIterator population;
    private final Function breakWhen;
    private final XPathContext baseContext;
    private final XPathContext runningContext;
    private List<Item> currentMembers;
    private Item next;
    private Item current = null;
    private int position = 0;


    public GroupBreakingIterator(Expression select, Function breakWhen,
                                 XPathContext baseContext)
            throws XPathException {
        this.select = select;
        this.breakWhen = breakWhen;
        this.baseContext = baseContext;
        this.runningContext = baseContext.newMinorContext();
        this.population = runningContext.trackFocus(select.iterate(baseContext));
        next = population.next();
    }

    private void advance() throws XPathException {
        currentMembers = new ArrayList<>(20);
        currentMembers.add(current);
        while (true) {
            Item nextCandidate = population.next();
            if (nextCandidate == null) {
                break;
            }
            BooleanValue result = (BooleanValue)breakWhen.call(runningContext,
                                                               new Sequence[]{
                                                                       SequenceExtent.makeSequenceExtent(currentMembers),
                                                               nextCandidate});
            try {
                if (!result.getBooleanValue()) {
                    currentMembers.add(nextCandidate);
                } else {
                    next = nextCandidate;
                    return;
                }
            } catch (ClassCastException e) {
                String message = "Grouping key values are of non-comparable types";
                XPathException err = new XPathException(message);
                err.setIsTypeError(true);
                err.setXPathContext(runningContext);
                throw err;
            }
        }
        next = null;
    }

    @Override
    public AtomicSequence getCurrentGroupingKey() {
        return null;
    }

    @Override
    public SequenceIterator iterateCurrentGroup() {
        return new ListIterator<>(currentMembers);
    }

    @Override
    public boolean hasNext() {
        return next != null;
    }

    @Override
    public Item next() throws XPathException {
        if (next == null) {
            current = null;
            position = -1;
            return null;
        }
        current = next;
        position++;
        advance();
        return current;
    }

    @Override
    public void close() {
        population.close();
    }

    /**
     * Get properties of this iterator, as a bit-significant integer.
     *
     * @return the properties of this iterator. This will be some combination of
     * properties such as {@link Property#GROUNDED}, {@link Property#LAST_POSITION_FINDER},
     * and {@link Property#LOOKAHEAD}. It is always
     * acceptable to return the value zero, indicating that there are no known special properties.
     * It is acceptable for the properties of the iterator to change depending on its state.
     */

    @Override
    public EnumSet<Property> getProperties() {
        return EnumSet.of(Property.LOOKAHEAD);
    }

    @Override
    public ManualGroupIterator getSnapShot(XPathContext context) {
        throw new UnsupportedOperationException();
        // TODO: implement me. Needed for parallel operation
    }


}

