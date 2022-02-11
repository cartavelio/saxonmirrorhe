using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public interface StaticError {
        Saxon.Api.QName getErrorCode();
        string getMessage();
        Saxon.Hej.s9api.Location getLocation();
        string getModuleUri();
        int getColumnNumber();
        int getLineNumber();
        string getInstructionName();
        bool isWarning();
        bool isTypeError();
        string getPath();
        void setTerminationMessage(string message);
        string getTerminationMessage();
    
    }
}