using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public enum Affinity {
        SAME_TYPE,
        SUBSUMES,
        SUBSUMED_BY,
        DISJOINT,
        OVERLAPS,
    }
}