using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class TransitiveClosure : Saxon.Hej.functions.SystemFunction {
        public static readonly Saxon.Hej.type.FunctionItemType resultType = new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.OPTIONAL_NODE}, Saxon.Hej.value.SequenceType.NODE_SEQUENCE);
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.FunctionItem step = (Saxon.Hej.om.FunctionItem)(arguments[0].head());
            int min = 1;
            if ((arguments.Length>1)) {
                Saxon.Hej.value.IntegerValue supplied = ((Saxon.Hej.value.IntegerValue)(arguments[1].head()));
                if ((supplied != null)) {
                    min = (int)(supplied.longValue());
                }
            }
            int max = System.Int32.MaxValue;
            if ((arguments.Length>2)) {
                Saxon.Hej.value.IntegerValue supplied = ((Saxon.Hej.value.IntegerValue)(arguments[2].head()));
                if ((supplied != null)) {
                    max = (int)(supplied.longValue());
                }
            }
            if ((max<min)) {
                throw new Saxon.Hej.trans.XPathException("In call to transitive-closure, max<min", "FORG0013");
            }
            return new Saxon.Hej.functions.CallableFunction(1, new Saxon.Eej.functions.qt4.TransitiveClosure.TransitiveClosureResult(step, min, max), resultType);
        }
        public class TransitiveClosureResult : Saxon.Hej.expr.Callable {
            private readonly Saxon.Hej.om.FunctionItem step;
            private readonly int min;
            private readonly int max;
            public TransitiveClosureResult(Saxon.Hej.om.FunctionItem step, int min, int max)  {
                this.step = step;
                this.min = min;
                this.max = max;
            }
            public Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
                Saxon.Hej.om.NodeInfo origin = (Saxon.Hej.om.NodeInfo)(arguments[0].head());
                if ((origin == null)) {
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
                Saxon.Hej.om.Sequence[] stepArgs = new Saxon.Hej.om.Sequence[1];
                System.Collections.Generic.ISet<Saxon.Hej.om.NodeInfo> startSet = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeInfo>();
                startSet.Add(origin);
                System.Collections.Generic.ISet<Saxon.Hej.om.NodeInfo> resultSet = new System.Collections.Generic.HashSet<Saxon.Hej.om.NodeInfo>();
                if ((min == 0)) {
                    resultSet.Add(origin);
                }
                for (int i = 1; (i<=max); i++) {
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
                    if ((i>=min)) {
                        Saxon.Impl.Helpers.SetUtils.AddAll(resultSet, thisSet);
                    }
                    startSet = thisSet;
                }
                Saxon.Hej.om.SequenceIterator resultIter = new Saxon.Hej.om.IteratorWrapper(resultSet.GetEnumerator());
                Saxon.Hej.expr.sort.DocumentOrderIterator sortedIter = new Saxon.Hej.expr.sort.DocumentOrderIterator(resultIter, new Saxon.Hej.expr.sort.GlobalOrderComparer());
                return Saxon.Hej.value.SequenceExtent.from(sortedIter);
            }
        
        }
    
    }
}