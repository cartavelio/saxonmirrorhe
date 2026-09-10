using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class NormalizeUnicode : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.value.StringValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.value.StringValue sv = (Saxon.Hej.value.StringValue)(arguments[0].head());
            if ((sv == null)) {
                return Saxon.Hej.value.StringValue.EMPTY_STRING;
            }
            string nf = ((arguments.Length == 1) ? "NFC" : Saxon.Hej.value.Whitespace.trim(arguments[1].head().getStringValue()));
            return new Saxon.Hej.value.StringValue(normalize(sv.getStringValue(), nf, context));
        }
        public static string normalize(string sv, string form, Saxon.Hej.expr.XPathContext c) {
            Saxon.Ejava.text.Normalizer.Form fb;
            if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(form, "NFC")) {
                fb = Saxon.Ejava.text.Normalizer.Form.NFC;
            } else {
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(form, "NFD")) {
                    fb = Saxon.Ejava.text.Normalizer.Form.NFD;
                } else {
                    if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(form, "NFKC")) {
                        fb = Saxon.Ejava.text.Normalizer.Form.NFKC;
                    } else {
                        if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(form, "NFKD")) {
                            fb = Saxon.Ejava.text.Normalizer.Form.NFKD;
                        } else {
                            if (form.Length==0) {
                                return sv;
                            } else {
                                string msg = (("Normalization form "+form)+" is not supported");
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(msg);
                                err.setErrorCode("FOCH0003");
                                err.setXPathContext(c);
                                throw err;
                            }
                        }
                    }
                }
            }
            return Saxon.Ejava.text.Normalizer.normalize(sv, fb);
        }
    
    }
}