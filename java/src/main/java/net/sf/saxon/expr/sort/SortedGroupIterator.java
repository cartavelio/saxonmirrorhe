////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.expr.sort;

import com.saxonica.ee.stream.ManualGroupIterator;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.XPathContextMajor;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.ObjectValue;

import java.util.ArrayList;

/**
 * A SortedGroupIterator is a modified SortedIterator. It sorts a sequence of groups,
 * and is itself a GroupIterator. The modifications retain extra information about
 * the items being sorted. The items are each the leading item of a group, and as well
 * as the item itself, the iterator preserves information about the group: specifically,
 * an iterator over the items in the group, and the value of the grouping key (if any).
 */

public class SortedGroupIterator extends SortedIterator implements GroupIterator {

    public SortedGroupIterator(XPathContext context,
                               GroupIterator base,
                               SortKeyEvaluator sortKeyEvaluator,
                               AtomicComparer[] comparators
    ) {
        super(context, base, sortKeyEvaluator, comparators, true);
        setHostLanguage(HostLanguage.XSLT);
    }

    public record Group (GroundedValue content, AtomicSequence groupingKey) {};

    public static class GroupParcel extends ObjectValue<Group> {
        public GroupParcel(GroundedValue content, AtomicSequence groupingKey) {
            super(new Group(content, groupingKey));
        }
        public GroundedValue getContent() {
            return getObject().content();
        }
        public AtomicSequence getGroupingKey() {
            return getObject().groupingKey();
        }
    }

    /**
     * Override the method that builds the array of values and sort keys.
     *
     * @throws XPathException in the event of a dynamic error
     */

    @Override
    protected void buildArray() throws XPathException {
        SequenceIterator base = getBaseIterator();
        int allocated = SequenceTool.supportsGetLength(base) ? SequenceTool.getLength(base) : 100;

        values = new ArrayList<>(allocated);
        count = 0;

        XPathContextMajor c2 = context.newContext();
        c2.setCurrentIterator((FocusIterator)base);
        GroupIterator groupIter = (GroupIterator)((FocusTrackingIterator)base).getUnderlyingIterator();
        c2.setCurrentGroupIterator(groupIter);

        // initialise the array with data

        Item item;
        while ((item = base.next()) != null) {

            AtomicSequence[] sortKeyValues = new AtomicValue[comparators.length];
            for (int n = 0; n < comparators.length; n++) {
                sortKeyValues[n] = sortKeyEvaluator.evaluateSortKey(n, c2);
            }
            GroupParcel parcel = new GroupParcel(groupIter.currentGroup(), groupIter.getCurrentGroupingKey());
            ThingToBeSorted gtbs = new ThingToBeSorted(parcel, sortKeyValues);
            values.add(gtbs);
        }
    }

    @Override
    public Item next() {
        GroupParcel parcel = (GroupParcel)super.next();
        return parcel == null ? null : parcel.getContent().head();
    }

    /*@Nullable*/
    @Override
    public AtomicSequence getCurrentGroupingKey() {
        return ((GroupParcel) values.get(position - 1).thing()).getGroupingKey();
    }

    @Override
    public GroundedValue currentGroup() throws XPathException {
        return ((GroupParcel) values.get(position - 1).thing()).getContent();
    }

    //#if EE==true

    @Override
    public ManualGroupIterator getSnapShot(XPathContext context) {
        GroupParcel thisGroup = (GroupParcel)values.get(position - 1).thing();
        ManualGroupIterator iter = new ManualGroupIterator(thisGroup, position);
        iter.setCurrentGroup(thisGroup.getContent());
        iter.setCurrentGroupingKey(thisGroup.getGroupingKey());
        iter.setLengthFinder(() -> values.size());
        return iter;
    }


    //#endif

}

