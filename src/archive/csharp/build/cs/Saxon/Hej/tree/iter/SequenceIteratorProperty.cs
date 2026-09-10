using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.iter {
    [System.Flags] public enum SequenceIteratorProperty {
        NONE = 0,
        GROUNDED = 1,
        LAST_POSITION_FINDER = 2,
        LOOKAHEAD = 4,
        ATOMIZING = 8,
    }
}