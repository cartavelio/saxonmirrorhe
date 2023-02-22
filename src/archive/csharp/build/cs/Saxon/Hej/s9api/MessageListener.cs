using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public interface MessageListener {
        void message(Saxon.Api.XdmNode content, bool terminate, Saxon.Ejavax.xml.transform.SourceLocator locator);
    
    }
}