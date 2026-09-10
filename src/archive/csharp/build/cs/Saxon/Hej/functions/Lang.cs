using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Lang : Saxon.Hej.functions.SystemFunction {
        public static bool isLang(string arglang, Saxon.Hej.om.NodeInfo target) {
            string doclang = null;
            Saxon.Hej.om.NodeInfo node = target;
            while ((node != null)) {
                doclang = node.getAttributeValue(Saxon.Hej.lib.NamespaceConstant.XML, "lang");
                if ((doclang != null)) {
                    break ;
                }
                node = node.getParent();
                if ((node == null)) {
                    return false;
                }
            }
            if ((doclang == null)) {
                return false;
            }
            while (true) {
                if (Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(arglang, doclang)) {
                    return true;
                }
                int hyphen = doclang.LastIndexOf("-");
                if ((hyphen<0)) {
                    return false;
                }
                doclang = doclang.Substring(0, hyphen - 0);
            }
        }
        public override Saxon.Hej.value.BooleanValue call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            Saxon.Hej.om.NodeInfo target;
            if ((arguments.Length>1)) {
                target = (Saxon.Hej.om.NodeInfo)(arguments[1].head());
            } else {
                target = getAndCheckContextItem(context);
            }
            Saxon.Hej.om.Item arg0Val = arguments[0].head();
            string testLang = ((arg0Val == null) ? "" : arg0Val.getStringValue());
            return Saxon.Hej.value.BooleanValue.get(isLang(testLang, target));
        }
        private Saxon.Hej.om.NodeInfo getAndCheckContextItem(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.NodeInfo target;
            Saxon.Hej.om.Item current = context.getContextItem();
            if ((current == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("The context item for lang() is absent");
                err.setErrorCode("XPDY0002");
                err.setXPathContext(context);
                throw err;
            }
            if (!(((current is Saxon.Hej.om.NodeInfo)))) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("The context item for lang() is not a node");
                err.setErrorCode("XPTY0004");
                err.setXPathContext(context);
                throw err;
            }
            target = (Saxon.Hej.om.NodeInfo)(current);
            return target;
        }
    
    }
}