using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.tree.tiny {
    public class TreeStatistics {
        public readonly Saxon.Hej.tree.tiny.Statistics SOURCE_DOCUMENT_STATISTICS = new Saxon.Hej.tree.tiny.Statistics();
        public readonly Saxon.Hej.tree.tiny.Statistics TEMPORARY_TREE_STATISTICS = new Saxon.Hej.tree.tiny.Statistics();
        public readonly Saxon.Hej.tree.tiny.Statistics RESULT_TREE_STATISTICS = new Saxon.Hej.tree.tiny.Statistics();
        public readonly Saxon.Hej.tree.tiny.Statistics ASSERTION_TREE_STATISTICS = new Saxon.Hej.tree.tiny.Statistics();
        public readonly Saxon.Hej.tree.tiny.Statistics FN_PARSE_STATISTICS = new Saxon.Hej.tree.tiny.Statistics();
    
    }
}