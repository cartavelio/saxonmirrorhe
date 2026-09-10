package com.saxonica.ee.optim;

import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.AtomicComparer;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

/**
 * A representation of a (sequence) value that allows the construction of one or more indexes to provide rapid
 * access to the items in the sequence.
 */

public class IndexedValue extends SearchableValue {

    // There can be more than one index, for different "use" expressions. But usually there's only
    // one. So rather than a full HashMap, we keep two parallel lists: a list of use expressions,
    // and a list of corresponding indexes.

    private final List<Expression> indexedExpressions = new ArrayList<>(3);
    private final List<MultiIndex> indexes = new ArrayList<>(3);
    private Set<NodeInfo> distinctNodes;

    IndexedValue(Item[] array, int end) {
        this.value = array;
        this.end = end;
    }

    /**
     * Retrieve items that match a given index value
     *
     * @param use                   the expression whose value is to be matched
     * @param values                the set of values to be found
     * @param convertUntypedToOther true if untypedAtomic values are to be converted to the
     *                              type of the other operand
     * @param firstOnly             true if only the first matching item is required
     * @param comparer              AtomicComparer used to compare values for equality (encapsulates a collation
     *                              if necessary)
     * @param context               the dynamic evaluation context @return an iterator over the items that match the specified values
     * @return an iterator over the items that matched
     * @throws XPathException if an error occurs
     */

    @Override
    public SequenceIterator findItems(Expression use, SequenceIterator values,
                                                      boolean convertUntypedToOther,
                                                      boolean firstOnly, AtomicComparer comparer, XPathContext context) throws XPathException {
        // first find the index for this expression if there is one
        MultiIndex index = null;
        synchronized(this) {
            // First look for an index for this very same expression
            for (int i = 0; i < indexedExpressions.size(); i++) {
                Expression si = indexedExpressions.get(i);
                if (si == use) {
                    index = indexes.get(i);
                    break;
                }
            }
            // if not found, look for an index for an equivalent expression. This is expensive, but rare.
            if (index == null) {
                for (int i = 0; i < indexedExpressions.size(); i++) {
                    Expression si = indexedExpressions.get(i);
                    if (si.isEqual(use)) {
                        index = indexes.get(i);
                        indexedExpressions.add(use);
                        indexes.add(index);
                        break;
                    }
                }
            }
            if (index == null) {
                index = new MultiIndex(this);
                indexedExpressions.add(use);
                indexes.add(index);
            }
        }
        return index.findItems(use, values, convertUntypedToOther, firstOnly, comparer.getCollator(), context);
    }

    /**
     * Determine whether the value contains a particular node. This implementation
     * of the method builds a representation of the node-sequence as a Java HashSet
     * the first time it is invoked, and subsequently does a fast lookup in this HashSet
     * rather than performing a sequential search.
     * @param sought the sought-after node
     * @return true if the value contains this node
     */

    @Override
    public synchronized boolean containsNode(NodeInfo sought) {
        if (distinctNodes == null) {
            distinctNodes = new HashSet<>();
            SequenceIterator iter = iterate();
            Item it;
            while ((it = iter.next()) != null) {
                if (it instanceof NodeInfo) {
                    distinctNodes.add((NodeInfo)it);
                }
            };
        }
        return distinctNodes.contains(sought);
    }
}

// Copyright (c) 2005-2023 Saxonica Limited
