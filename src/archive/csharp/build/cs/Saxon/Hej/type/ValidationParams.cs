using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.type {
    public class ValidationParams : System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.Sequence> {
        public ValidationParams() : base (20) {
        }
        public static void setValidationParams(System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable> declaredParams, Saxon.Hej.type.ValidationParams actualParams, Saxon.Hej.sxpath.XPathDynamicContext context) {
            foreach (Saxon.Hej.om.StructuredQName p in Saxon.Impl.Helpers.MapUtils.KeySet(declaredParams)) {
                Saxon.Hej.sxpath.XPathVariable var = Saxon.Impl.Helpers.MapUtils.Get(declaredParams, p);
                Saxon.Hej.om.Sequence paramValue = Saxon.Impl.Helpers.MapUtils.Get(((System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.Sequence>)(actualParams)), p);
                if ((paramValue != null)) {
                    context.setVariable(var, paramValue);
                } else {
                    context.setVariable(var, var.getDefaultValue());
                }
            }
        }
    
    }
}