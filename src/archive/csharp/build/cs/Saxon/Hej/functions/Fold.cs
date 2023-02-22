using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public interface Fold {
        void processItem(Saxon.Hej.om.Item item);
        bool isFinished();
        Saxon.Hej.om.Sequence result();
    
    }
}