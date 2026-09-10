package com.saxonica.functions.qt4;

import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.DocumentOrderIterator;
import net.sf.saxon.expr.sort.GlobalOrderComparer;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.SequenceType;

import java.util.HashSet;
import java.util.Set;

/**
 * Function fn:transitive-closure, proposed for 4.0
 */
public class TransitiveClosure extends SystemFunction {

    public static final FunctionItemType resultType =
            new SpecificFunctionType(new SequenceType[]{SequenceType.OPTIONAL_NODE}, SequenceType.NODE_SEQUENCE);


    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        FunctionItem step = (FunctionItem) arguments[0].head();
        int min = 1;
        if (arguments.length > 1) {
            IntegerValue supplied = ((IntegerValue) arguments[1].head());
            if (supplied != null) {
                min = (int) supplied.longValue(); // TODO: value range
            }
        }
        int max = Integer.MAX_VALUE;
        if (arguments.length > 2) {
            IntegerValue supplied = ((IntegerValue) arguments[2].head());
            if (supplied != null) {
                max = (int) supplied.longValue(); // TODO: value range
            }
        }
        if (max < min) {
            throw new XPathException("In call to transitive-closure, max<min", "FORG0013");
        }
        return new CallableFunction(1, new TransitiveClosureResult(step, min, max), resultType);
    }

    public static class TransitiveClosureResult implements Callable {

        private final FunctionItem step;
        private final int min;
        private final int max;

        public TransitiveClosureResult(FunctionItem step, int min, int max) {
            this.step = step;
            this.min = min;
            this.max = max;
        }

        @Override
        public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
            NodeInfo origin = (NodeInfo)arguments[0].head();
            if (origin == null) {
                return EmptySequence.getInstance();
            }
            Sequence[] stepArgs = new Sequence[1];
            // startSet holds the input nodes to the current iteration
            Set<NodeInfo> startSet = new HashSet<>();
            startSet.add(origin);

            // resultSet contains nodes that are to form part of the final result
            Set<NodeInfo> resultSet = new HashSet<>();
            if (min == 0) {
                resultSet.add(origin);
            }

            for (int i=1; i<=max; i++) {
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
                if (i >= min) {
                    // Add the nodes found in this step to the result set if we have made "min" steps
                    resultSet.addAll(thisSet);
                }
                // Proceed to the next step
                startSet = thisSet;
            }

            // Finally, sort the set of nodes into document order
            SequenceIterator resultIter = new IteratorWrapper(resultSet.iterator());
            DocumentOrderIterator sortedIter = new DocumentOrderIterator(resultIter, new GlobalOrderComparer());
            return SequenceExtent.from(sortedIter);

        }

    }



}

