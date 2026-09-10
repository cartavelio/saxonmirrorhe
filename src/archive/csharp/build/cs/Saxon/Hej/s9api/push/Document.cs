using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api.push {
    public interface Document : Saxon.Hej.s9api.push.Container {
        /* implicit =====
        Saxon.Hej.s9api.push.Container text(string value);
        ===== implicit */
        /* implicit =====
        Saxon.Hej.s9api.push.Container comment(string value);
        ===== implicit */
        /* implicit =====
        Saxon.Hej.s9api.push.Container processingInstruction(string name, string value);
        ===== implicit */
    
    }
}