using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pattern {
    public interface QNameTest {
        bool matches(Saxon.Hej.om.StructuredQName qname);
        string exportQNameTest();
        string generateJavaScriptNameTest(int targetVersion);
    
    }
}