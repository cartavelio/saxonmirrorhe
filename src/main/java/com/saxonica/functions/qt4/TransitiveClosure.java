package com.saxonica.functions.qt4;

import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.DocumentOrderIterator;
import net.sf.saxon.expr.sort.GlobalOrderComparer;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.functions.SystemFunction;
import net.sf.saxon.om.FunctionItem;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.ListIterator;
import net.sf.saxon.type.FunctionItemType;
import net.sf.saxon.type.SpecificFunctionType;
import net.sf.saxon.value.EmptySequence;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.SequenceExtent;
import net.sf.saxon.value.SequenceType;

import java.util.ArrayList;
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
            // Find the nodes reachable in exactly "min" steps
            Set<NodeInfo> startSet = new HashSet<>();
            startSet.add(origin);
            for (int i=0; i<min; i++) {
                Set<NodeInfo> endSet = new HashSet<>();
                for (NodeInfo node : startSet) {
                    stepArgs[0] = node;
                    SequenceIterator stepResult = step.call(context, stepArgs).iterate();
                    NodeInfo resultNode;
                    while ((resultNode = (NodeInfo)stepResult.next()) != null) {
                        endSet.add(resultNode);
                    }
                }
                startSet = endSet;
                if (startSet.isEmpty()) {
                    return EmptySequence.getInstance();
                }
            }
            // Find the nodes reachable in up to (max-min) further steps, stopping when no further nodes are found
            Set<NodeInfo> result = startSet;
            int steps = min;
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
                steps++;
            } while (foundMoreNodes && steps < max);

            SequenceIterator resultIter = new ListIterator.Of<NodeInfo>(new ArrayList<NodeInfo>(result));
            DocumentOrderIterator sortedIter = new DocumentOrderIterator(resultIter, new GlobalOrderComparer());
            return SequenceExtent.from(sortedIter);

        }

    }



}

