using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.om {
    public interface AtomicSequence : Saxon.Hej.om.GroundedValue, System.Collections.Generic.IEnumerable<Saxon.Hej.value.AtomicValue> {
        /* implicit =====
        Saxon.Hej.om.Item head();
        ===== implicit */
        /* implicit =====
        Saxon.Hej.om.SequenceIterator iterate();
        ===== implicit */
        /* implicit =====
        Saxon.Hej.om.Item itemAt(int n);
        ===== implicit */
        /* implicit =====
        int getLength();
        ===== implicit */
        /* implicit =====
        Saxon.Hej.str.UnicodeString getUnicodeStringValue();
        ===== implicit */
        Saxon.Hej.str.UnicodeString getCanonicalLexicalRepresentation();
    
    }
}