using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public interface QNameTest {
        bool matches(Saxon.Hej.om.StructuredQName qname);
        bool matchesFingerprint(Saxon.Hej.om.NamePool namePool, int fp);
        string exportQNameTest();
    
    }
}