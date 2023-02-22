using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class NoOpenStartTagException : Saxon.Hej.trans.XPathException {
        public static Saxon.Hej.@event.NoOpenStartTagException makeNoOpenStartTagException(int nodeKind, string name, Saxon.Api.HostLanguage hostLanguage, bool parentIsDocument, Saxon.Hej.s9api.Location startElementLocationId) {
            string message;
            string errorCode;
            if (parentIsDocument) {
                string kind = ((nodeKind == Saxon.Hej.type.Type.ATTRIBUTE) ? "an attribute" : "a namespace");
                message = (((("Cannot create "+kind)+" node (")+name)+") whose parent is a document node");
                errorCode = ((hostLanguage == Saxon.Api.HostLanguage.XSLT) ? "XTDE0420" : "XPTY0004");
            } else {
                string kind = ((nodeKind == Saxon.Hej.type.Type.ATTRIBUTE) ? "An attribute" : "A namespace");
                message = (((kind+" node (")+name)+") cannot be created after a child of the containing element");
                errorCode = ((hostLanguage == Saxon.Api.HostLanguage.XSLT) ? "XTDE0410" : "XQTY0024");
            }
            if (((startElementLocationId != null) && (startElementLocationId.getLineNumber() != -1))) {
                message += (((". Most recent element start tag was output at line "+startElementLocationId.getLineNumber())+" of module ")+new Saxon.Hej.lib.StandardDiagnostics().abbreviateLocationURI(startElementLocationId.getSystemId()));
            }
            Saxon.Hej.@event.NoOpenStartTagException err = new Saxon.Hej.@event.NoOpenStartTagException(message);
            err.setErrorCode(errorCode);
            return err;
        }
        public NoOpenStartTagException(string message) : base (message) {
        }
    
    }
}