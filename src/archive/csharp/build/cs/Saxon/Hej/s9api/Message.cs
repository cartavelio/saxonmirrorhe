using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    public class Message {
        private readonly Saxon.Api.XdmNode _content;
        private readonly Saxon.Api.QName _errorCode;
        private readonly bool _terminate;
        private readonly Saxon.Hej.s9api.Location _location;
        public Message(Saxon.Api.XdmNode content, Saxon.Api.QName errorCode, bool terminate, Saxon.Hej.s9api.Location location)  {
            this._content = content;
            this._errorCode = errorCode;
            this._terminate = terminate;
            this._location = location;
        }
        public Saxon.Api.XdmNode getContent() {
            return _content;
        }
        public Saxon.Api.QName getErrorCode() {
            return _errorCode;
        }
        public bool isTerminate() {
            return _terminate;
        }
        public Saxon.Hej.s9api.Location getLocation() {
            return _location;
        }
    
    }
}