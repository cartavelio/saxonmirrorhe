using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class Partition : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.SequenceIterator iterator = arguments[0].iterate();
            System.Collections.Generic.IList<Saxon.Hej.ma.arrays.ArrayItem> completePartitions = new System.Collections.Generic.List<Saxon.Hej.ma.arrays.ArrayItem>();
            System.Collections.Generic.IList<Saxon.Hej.om.Item> lastPartition = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
            Saxon.Hej.om.FunctionItem breakWhen = (Saxon.Hej.om.FunctionItem)(arguments[1].head());
            Saxon.Hej.om.Item next;
            while (((next = iterator.next()) != null)) {
                if (lastPartition.Count==0) {
                    lastPartition.Add(next);
                } else {
                    Saxon.Hej.om.Sequence val = dynamicCall(breakWhen, context, Saxon.Hej.value.SequenceExtent.makeSequenceExtent(lastPartition), next);
                    if (((Saxon.Hej.value.BooleanValue)(val.head())).getBooleanValue()) {
                        completePartitions.Add(Saxon.Hej.ma.arrays.SimpleArrayItem.makeSimpleArrayItem(new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(lastPartition)));
                        lastPartition.Clear();
                    }
                    lastPartition.Add(next);
                }
            }
            if (!(lastPartition.Count==0)) {
                completePartitions.Add(Saxon.Hej.ma.arrays.SimpleArrayItem.makeSimpleArrayItem(new Saxon.Hej.tree.iter.ListIterator.Of<Saxon.Hej.om.Item>(lastPartition)));
            }
            return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(completePartitions);
        }
    
    }
}