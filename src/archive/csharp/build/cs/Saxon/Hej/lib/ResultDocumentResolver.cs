using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.lib {
    public interface ResultDocumentResolver {
        Saxon.Hej.@event.Receiver resolve(Saxon.Hej.expr.XPathContext context, string href, string baseUri, Saxon.Hej.serialize.SerializationProperties properties);
    
    }
}