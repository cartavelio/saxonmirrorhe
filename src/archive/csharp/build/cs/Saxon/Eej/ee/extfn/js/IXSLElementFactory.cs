using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn.js {
    public abstract class IXSLElementFactory {
        public readonly static Saxon.Eej.xsltextn.ExtensionElementCreator FACTORY = localname => {
            switch (localname) {
              case "schedule-action":
                return new Saxon.Eej.ee.extfn.js.IXSLScheduleAction();
              case "set-attribute":
                return new Saxon.Eej.ee.extfn.js.IXSLSetAttribute();
              case "remove-attribute":
                return new Saxon.Eej.ee.extfn.js.IXSLRemoveAttribute();
              case "set-property":
                return new Saxon.Eej.ee.extfn.js.IXSLSetProperty();
              case "remove-property":
                return new Saxon.Eej.ee.extfn.js.IXSLRemoveProperty();
              case "set-style":
                return new Saxon.Eej.ee.extfn.js.IXSLSetStyle();
            }
            return null;
        } ;
    
    }
}