using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public interface MessageListener2 {
        void message(Saxon.Api.XdmNode content, Saxon.Api.QName errorCode, bool terminate, Saxon.Ejavax.xml.transform.SourceLocator locator);
    
    }
}