using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface ErrorReporter {
        void report(Saxon.Hej.s9api.XmlProcessingError error);
    
    }
}