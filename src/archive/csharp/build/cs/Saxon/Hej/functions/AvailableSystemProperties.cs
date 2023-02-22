using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class AvailableSystemProperties : Saxon.Hej.functions.SystemFunction {
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            System.Collections.Generic.IList<Saxon.Hej.value.AtomicValue> myList = new System.Collections.Generic.List<Saxon.Hej.value.AtomicValue>();
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "version"));
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "vendor"));
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "vendor-url"));
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "product-name"));
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "product-version"));
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "is-schema-aware"));
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "supports-serialization"));
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "supports-backwards-compatibility"));
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "supports-namespace-axis"));
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "supports-streaming"));
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "supports-dynamic-evaluation"));
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "supports-higher-order-functions"));
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "xpath-version"));
            myList.Add(new Saxon.Hej.value.QNameValue("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "xsd-version"));
            myList.Add(new Saxon.Hej.value.QNameValue("bin", Saxon.Eej.functions.extfn.EXPathBinaryFunctionSet.NAMESPACE, "version"));
            myList.Add(new Saxon.Hej.value.QNameValue("file", Saxon.Eej.functions.extfn.EXPathFileFunctionSet.NAMESPACE, "version"));
            if (context.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_EXTERNAL_FUNCTIONS)) {
                foreach (string s in Saxon.Ejava.lang.System.getProperties().stringPropertyNames()) {
                    myList.Add(new Saxon.Hej.value.QNameValue("", "", s));
                }
            }
            return new Saxon.Hej.om.AtomicArray(myList);
        }
        public override Saxon.Hej.expr.Expression makeFunctionCall(Saxon.Hej.expr.Expression[] arguments) {
            return new Inner_SystemFunctionCall_d990e459(this, this, arguments);
        }
    
        private class Inner_SystemFunctionCall_d990e459 : Saxon.Hej.expr.SystemFunctionCall {
            private readonly AvailableSystemProperties outerInstance;
            public Inner_SystemFunctionCall_d990e459(AvailableSystemProperties outerInstance, AvailableSystemProperties arg1, /*AA*/Saxon.Hej.expr.Expression[] arg2) : base(arg1, arg2) {
                this.outerInstance = outerInstance;
            }
            public override Saxon.Hej.expr.Expression preEvaluate(Saxon.Hej.expr.parser.ExpressionVisitor visitor) {
                return this;
            }
        }

    }
}