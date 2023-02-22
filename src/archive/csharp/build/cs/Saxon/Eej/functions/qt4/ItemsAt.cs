using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.functions.qt4 {
    public class ItemsAt : Saxon.Hej.functions.SystemFunction {
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return arguments[0].getSpecialProperties();
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.GroundedValue input = arguments[0].materialize();
            if ((input.getLength() == 0)) {
                return Saxon.Hej.value.EmptySequence.getInstance();
            }
            Saxon.Hej.ma.zeno.ZenoSequence result = new Saxon.Hej.ma.zeno.ZenoSequence();
            Saxon.Hej.om.SequenceIterator positions = arguments[1].iterate();
            for (Saxon.Hej.om.Item it; ((it = positions.next()) != null); ) {
                int offset = (int)(((Saxon.Hej.value.IntegerValue)(it)).longValue());
                Saxon.Hej.om.Item selected = input.itemAt((offset-1));
                if ((selected != null)) {
                    result = result.append(selected);
                }
            }
            return result;
        }
    
    }
}