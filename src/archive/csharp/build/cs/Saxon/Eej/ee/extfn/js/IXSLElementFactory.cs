using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.extfn.js {
    public class IXSLElementFactory : Saxon.Eej.xsltextn.ExtensionElementFactory {
        public System.Type getExtensionClass(string localname) {
            switch (localname) {
              case "schedule-action":
                return typeof(Saxon.Eej.ee.extfn.js.IXSLScheduleAction);
              case "set-attribute":
                return typeof(Saxon.Eej.ee.extfn.js.IXSLSetAttribute);
              case "remove-attribute":
                return typeof(Saxon.Eej.ee.extfn.js.IXSLRemoveAttribute);
              case "set-property":
                return typeof(Saxon.Eej.ee.extfn.js.IXSLSetProperty);
              case "remove-property":
                return typeof(Saxon.Eej.ee.extfn.js.IXSLRemoveProperty);
              case "set-style":
                return typeof(Saxon.Eej.ee.extfn.js.IXSLSetStyle);
            }
            return null;
        }
    
    }
}