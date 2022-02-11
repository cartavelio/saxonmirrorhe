using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api.push {
    public interface Container {
        void setDefaultNamespace(string uri);
        Saxon.Hej.s9api.push.Element element(Saxon.Api.QName name);
        Saxon.Hej.s9api.push.Element element(string name);
        Saxon.Hej.s9api.push.Container text(string value);
        Saxon.Hej.s9api.push.Container comment(string value);
        Saxon.Hej.s9api.push.Container processingInstruction(string name, string value);
        void close();
    
    }
}