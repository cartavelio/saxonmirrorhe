package com.saxonica.functions.qt4;

import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.DocumentOrderIterator;
import net.sf.saxon.expr.sort.GlobalOrderComparer;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.SequenceExtent;

import java.util.HashSet;
import java.util.Set;

/**
 * Function fn:transitive-closure, proposed for 4.0 (revised simplified 2-argument form in PR 761)
 */
public class TransitiveClosure extends SystemFunction {


    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        NodeInfo origin = (NodeInfo) arguments[0].head();
        if (origin == null) {
            return EmptySequence.getInstance();
        }
        FunctionItem step = (FunctionItem) arguments[1].head();
        Sequence[] stepArgs = new Sequence[1];
        // startSet holds the input nodes to the current iteration
        Set<NodeInfo> startSet = new HashSet<>();
        startSet.add(origin);

        // resultSet contains nodes that are to form part of the final result
        Set<NodeInfo> resultSet = new HashSet<>();

        while (true) {
            // thisSet holds the nodes reached in this step that were not already reached
            // in an earlier step
            Set<NodeInfo> thisSet = new HashSet<>();
            for (NodeInfo node : startSet) {
                stepArgs[0] = node;
                SequenceIterator stepResult = step.call(context, stepArgs).iterate();
                NodeInfo resultNode;
                while ((resultNode = (NodeInfo) stepResult.next()) != null) {
                    if (!resultSet.contains(resultNode)) {
                        thisSet.add(resultNode);
                    }
                }
            }
            if (thisSet.isEmpty()) {
                // No new nodes found, we're done
                break;
            }

            // Add the nodes found in this step to the result set if we have made "min" steps
            resultSet.addAll(thisSet);

            // Proceed to the next step
            startSet = thisSet;
        }

        // Finally, sort the set of nodes into document order
        SequenceIterator resultIter = new IteratorWrapper(resultSet.iterator());
        DocumentOrderIterator sortedIter = new DocumentOrderIterator(resultIter, new GlobalOrderComparer());
        return SequenceExtent.from(sortedIter); 

    }



}

