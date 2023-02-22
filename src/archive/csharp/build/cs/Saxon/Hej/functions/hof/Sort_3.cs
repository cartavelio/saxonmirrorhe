using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions.hof {
    public class Sort_3 : Saxon.Hej.functions.Sort_2 {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.Sequence input = arguments[0];
            System.Collections.Generic.List<Saxon.Hej.functions.Sort_1.ItemToBeSorted> inputList = new System.Collections.Generic.List<Saxon.Hej.functions.Sort_1.ItemToBeSorted>();
            int i = 0;
            Saxon.Hej.om.FunctionItem key = (Saxon.Hej.om.FunctionItem)(arguments[2].head());
            Saxon.Hej.om.SequenceIterator iterator = input.iterate();
            Saxon.Hej.om.Item item;
            while (((item = iterator.next()) != null)) {
                Saxon.Hej.functions.Sort_1.ItemToBeSorted member = new Saxon.Hej.functions.Sort_1.ItemToBeSorted();
                member.value = item;
                member.originalPosition = i++;
                member.sortKey = dynamicCall(key, context, new Saxon.Hej.om.Sequence[]{item}).materialize();
                inputList.Add(member);
            }
            return doSort(inputList, getCollation(context, arguments[1]), context);
        }
    
    }
}