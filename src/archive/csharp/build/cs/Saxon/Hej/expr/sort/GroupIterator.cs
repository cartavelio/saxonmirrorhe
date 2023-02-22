using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.sort {
    public interface GroupIterator : Saxon.Hej.om.SequenceIterator {
        Saxon.Hej.om.AtomicSequence getCurrentGroupingKey();
        Saxon.Hej.om.GroundedValue currentGroup();
        Saxon.Eej.ee.stream.ManualGroupIterator getSnapShot(Saxon.Hej.expr.XPathContext context);
    
    }
}