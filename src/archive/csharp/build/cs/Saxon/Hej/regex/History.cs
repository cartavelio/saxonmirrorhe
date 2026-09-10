using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.regex {
    public class History {
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.regex.Operation,Saxon.Hej.z.IntSet> zeroLengthMatches = new System.Collections.Generic.Dictionary<Saxon.Hej.regex.Operation,Saxon.Hej.z.IntSet>();
        public bool isDuplicateZeroLengthMatch(Saxon.Hej.regex.Operation op, int position) {
            Saxon.Hej.z.IntSet positions = Saxon.Impl.Helpers.MapUtils.Get(zeroLengthMatches, op);
            if ((positions == null)) {
                positions = new Saxon.Hej.z.IntHashSet(position);
                positions.add(position);
                Saxon.Impl.Helpers.MapUtils.Put(zeroLengthMatches, op, positions);
                return false;
            } else {
                return !(positions.add(position));
            }
        }
    
    }
}