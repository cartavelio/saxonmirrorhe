using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public interface ExtensionFunction {
        Saxon.Api.QName getName();
        Saxon.Api.XdmSequenceType getResultType();
        Saxon.Api.XdmSequenceType[] getArgumentTypes();
        Saxon.Api.XdmValue call(Saxon.Api.XdmValue[] arguments);
    
    }
}