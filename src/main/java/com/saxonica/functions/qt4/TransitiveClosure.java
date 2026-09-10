package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.DocumentOrderIterator;
import net.sf.saxon.expr.sort.GlobalOrderComparer;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.SequenceExtent;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.Set;

/**
 * Function fn:transitive-closure, proposed for 4.0
 */
public class TransitiveClosure extends SystemFunction {

    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        SequenceIterator iterator = arguments[0].iterate();
        FunctionItem step = (FunctionItem)arguments[1].head();
        Set<NodeInfo> result = new HashSet<>();

        // Prime the result with the immediate neighbours
        NodeInfo node;
        Sequence[] stepArgs = new Sequence[1];
        while ((node = (NodeInfo)iterator.next()) != null) {
            stepArgs[0] = node;
            SequenceIterator stepResult = step.call(context, stepArgs).iterate();
            NodeInfo target;
            while ((target = (NodeInfo)stepResult.next()) != null) {
                result.add(target);
            }
        }

        if (result.isEmpty()) {
            return EmptySequence.getInstance();
        }

        // Rinse and repeat
        boolean foundMoreNodes;
        do {
            int size = result.size();
            Set<NodeInfo> nextResult = new HashSet<>(result);
            for (NodeInfo n : result) {
                stepArgs[0] = n;
                SequenceIterator stepResult = step.call(context, stepArgs).iterate();
                NodeInfo target;
                while ((target = (NodeInfo) stepResult.next()) != null) {
                    nextResult.add(target);
                }
            }
            result = nextResult;
            foundMoreNodes = nextResult.size() > size;
        } while (foundMoreNodes);

        SequenceIterator resultIter = new ListIterator.Of<NodeInfo>(new ArrayList<NodeInfo>(result));
        DocumentOrderIterator sortedIter = new DocumentOrderIterator(resultIter, new GlobalOrderComparer());
        return SequenceExtent.from(sortedIter);
    }

}

