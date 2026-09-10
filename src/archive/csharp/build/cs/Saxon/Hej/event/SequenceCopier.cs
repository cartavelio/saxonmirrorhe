using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class SequenceCopier {
        private SequenceCopier()  {
        }
        public static void copySequence(Saxon.Hej.om.SequenceIterator @in, Saxon.Hej.@event.Receiver @out) {
            @out.open();
            try {
                Saxon.Hej.om.SequenceTool.supply(@in, (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)(it => @out.append(it) /* converted from instance method reference */));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
            @out.close();
        }
    
    }
}