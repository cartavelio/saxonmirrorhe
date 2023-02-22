using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface GroundedValue : Saxon.Hej.om.Sequence {
        /* implicit =====
        Saxon.Hej.om.SequenceIterator iterate();
        ===== implicit */
        Saxon.Hej.om.Item itemAt(int n);
        /* implicit =====
        Saxon.Hej.om.Item head();
        ===== implicit */
        Saxon.Hej.om.GroundedValue subsequence(int start, int length);
        int getLength();
        bool effectiveBooleanValue();
        Saxon.Hej.str.UnicodeString getUnicodeStringValue();
        string getStringValue();
        Saxon.Hej.om.GroundedValue reduce();
        string toShortString();
        System.Collections.Generic.IEnumerable<Saxon.Hej.om.Item> asIterable();
        bool containsNode(Saxon.Hej.om.NodeInfo sought);
        Saxon.Hej.om.GroundedValue concatenate(params Saxon.Hej.om.GroundedValue[]  others);
    
    }
}