package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.ma.arrays.ArrayItem;
import net.sf.saxon.ma.arrays.SimpleArrayItem;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.value.BooleanValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.List;

/**
 * Function fn:partition, new in 4.0
 */
public class Partition extends SystemFunction {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        SequenceIterator iterator = arguments[0].iterate();
        List<ArrayItem> completePartitions = new ArrayList<>();
        List<Item> lastPartition = new ArrayList<>();
        FunctionItem breakWhen = (FunctionItem) arguments[1].head();
        Item next;
        int pos = 1;
        while ((next = iterator.next()) != null) {
            if (lastPartition.isEmpty()) {
                // this is the first item in the sequence
                lastPartition.add(next);
            } else {
                BooleanValue val = (BooleanValue)dynamicCall(
                        breakWhen, context, SequenceExtent.makeSequenceExtent(lastPartition), next, Int64Value.makeIntegerValue(pos)).head();
                if (val != null && val.getBooleanValue()) {
                    completePartitions.add(SimpleArrayItem.makeSimpleArrayItem(new ListIterator.Of<>(lastPartition)));
                    lastPartition.clear();
                }
                lastPartition.add(next);
            }
            pos++;
        }
        if (!lastPartition.isEmpty()) {
            completePartitions.add(SimpleArrayItem.makeSimpleArrayItem(new ListIterator.Of<>(lastPartition)));
        }
        return SequenceExtent.makeSequenceExtent(completePartitions);
    }

}

//
// The contents of this file are subject to the Mozilla Public License Version 1.0 (the "License");
// you may not use this file except in compliance with the License. You may obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Saxonica Limited
//
// Portions created by (your name) are Copyright (C) (your legal entity). All Rights Reserved.
//
// Contributor(s): none.
//
