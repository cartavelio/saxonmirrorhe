using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api.push {
    public interface Element : Saxon.Hej.s9api.push.Container {
        Saxon.Hej.s9api.push.Element attribute(Saxon.Api.QName name, string value);
        Saxon.Hej.s9api.push.Element attribute(string name, string value);
        Saxon.Hej.s9api.push.Element @namespace(string prefix, string uri);
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