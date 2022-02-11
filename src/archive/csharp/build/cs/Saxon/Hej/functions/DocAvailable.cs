using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class DocAvailable : Saxon.Hej.functions.SystemFunction {
        private bool isDocAvailable(Saxon.Hej.value.AtomicValue hrefVal, Saxon.Hej.expr.XPathContext context) {
            if ((hrefVal == null)) {
                return false;
            }
            string href = hrefVal.getStringValue();
            return docAvailable(href, context);
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            return Saxon.Hej.value.BooleanValue.get(isDocAvailable((Saxon.Hej.value.AtomicValue)(arguments[0].head()), context));
        }
        public bool docAvailable(string href, Saxon.Hej.expr.XPathContext context) {
            try {
                Saxon.Hej.expr.PackageData packageData = getRetainedStaticContext().getPackageData();
                Saxon.Hej.om.DocumentKey documentKey = Saxon.Hej.functions.DocumentFn.computeDocumentKey(href, getStaticBaseUriString(), packageData, context);
                Saxon.Hej.om.DocumentPool pool = context.getController().getDocumentPool();
                if (pool.isMarkedUnavailable(documentKey)) {
                    return false;
                }
                Saxon.Hej.om.TreeInfo doc = pool.find(documentKey);
                if ((doc != null)) {
                    return true;
                }
                Saxon.Hej.om.Item item = Saxon.Hej.functions.DocumentFn.makeDoc(href, getStaticBaseUriString(), packageData, null, context, null, true);
                if ((item != null)) {
                    return true;
                } else {
                    pool.markUnavailable(documentKey);
                    return false;
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                return false;
            }
        }
    
    }
}