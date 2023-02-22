using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public interface XmlProcessingError {
        Saxon.Api.HostLanguage getHostLanguage();
        bool isStaticError();
        bool isTypeError();
        Saxon.Api.QName getErrorCode();
        string getMessage();
        Saxon.Hej.s9api.Location getLocation();
        string getModuleUri();
        bool isWarning();
        string getPath();
        System.Exception getCause();
        Saxon.Hej.s9api.XmlProcessingError asWarning();
        void setTerminationMessage(string message);
        string getTerminationMessage();
        bool isAlreadyReported();
        void setAlreadyReported(bool reported);
    
    }
}