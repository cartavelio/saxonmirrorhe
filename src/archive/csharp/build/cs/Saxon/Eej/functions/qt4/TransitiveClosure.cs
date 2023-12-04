using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class TransitiveClosure : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo origin = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
            if ((origin == null)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            Saxon.Hej.om.FunctionItem step = (Saxon.Hej.om.FunctionItem)(arguments[1].head());
            Saxon.Hej.om.Sequence[] stepArgs = new Saxon.Hej.om.Sequence[1];
            System.Collections.Generic.ISet<Saxon.Hej.om.NodeInfo> startSet = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeInfo>();
            startSet.Add(origin);
            System.Collections.Generic.ISet<Saxon.Hej.om.NodeInfo> resultSet = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeInfo>();
            while (true) {
                System.Collections.Generic.ISet<Saxon.Hej.om.NodeInfo> thisSet = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeInfo>();
                foreach (Saxon.Hej.om.NodeInfo node in startSet) {
                    stepArgs[0] = node;
                    Saxon.Hej.om.SequenceIterator stepResult = step.call(context, stepArgs).iterate();
                    Saxon.Hej.om.NodeInfo resultNode;
                    while (((resultNode = (Saxon.Hej.om.NodeInfo)(stepResult.next())) != null)) {
                        if (!(resultSet.Contains(resultNode))) {
                            thisSet.Add(resultNode);
                        }
                    }
                }
                if (thisSet.Count==0) {
                    break ;
                }
                Saxon.Impl.Helpers.SetUtils.AddAll(resultSet, thisSet);
                startSet = thisSet;
            }
            Saxon.Hej.om.SequenceIterator resultIter = new Saxon.Hej.om.IteratorWrapper(resultSet.GetEnumerator());
            Saxon.Hej.expr.sort.DocumentOrderIterator sortedIter = new Saxon.Hej.expr.sort.DocumentOrderIterator(resultIter, new Saxon.Hej.expr.sort.GlobalOrderComparer());
            return Saxon.Hej.value.SequenceExtent.from(sortedIter);
        }
    
    }
}