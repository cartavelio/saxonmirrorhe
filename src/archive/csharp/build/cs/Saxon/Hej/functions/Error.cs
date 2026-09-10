using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Error : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        public override int getSpecialProperties(Saxon.Hej.expr.Expression[] arguments) {
            return (base.getSpecialProperties(arguments)&~(Saxon.Hej.expr.StaticProperty.NO_NODES_NEWLY_CREATED));
        }
        public bool isVacuousExpression() {
            return true;
        }
        public Saxon.Hej.om.Item error(Saxon.Hej.expr.XPathContext context, Saxon.Hej.value.QNameValue errorCode, Saxon.Hej.value.StringValue desc, Saxon.Hej.om.SequenceIterator errObject) {
            Saxon.Hej.value.QNameValue qname = null;
            if ((getArity()>0)) {
                qname = errorCode;
            }
            if ((qname == null)) {
                qname = new Saxon.Hej.value.QNameValue("err", Saxon.Hej.lib.NamespaceConstant.ERR, ((getArity() == 1) ? "FOTY0004" : "FOER0000"), Saxon.Hej.type.BuiltInAtomicType.QNAME, false);
            }
            string description;
            if ((getArity()>1)) {
                description = ((desc == null) ? "" : desc.getStringValue());
            } else {
                description = "Error signalled by application call on error()";
            }
            Saxon.Hej.trans.XPathException e = new Saxon.Hej.functions.Error.UserDefinedXPathException(description);
            e.setErrorCodeQName(qname.getStructuredQName());
            e.setXPathContext(context);
            if (((getArity()>2) && (errObject != null))) {
                Saxon.Hej.om.GroundedValue errorObject = Saxon.Hej.om.SequenceTool.toGroundedValue(errObject);
                if ((errorObject.getLength() == 1)) {
                    Saxon.Hej.om.Item root = errorObject.head();
                    if ((((root is Saxon.Hej.om.NodeInfo)) && (((Saxon.Hej.om.NodeInfo)(root)).getNodeKind() == Saxon.Hej.type.Type.DOCUMENT))) {
                        Saxon.Hej.tree.iter.AxisIterator iter = ((Saxon.Hej.om.NodeInfo)(root)).iterateAxis(Saxon.Hej.om.AxisInfo.CHILD, new Saxon.Hej.pattern.NameTest(Saxon.Hej.type.Type.ELEMENT, "", "error", context.getConfiguration().getNamePool()));
                        Saxon.Hej.om.NodeInfo errorElement = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next());
                        if ((errorElement != null)) {
                            string module = errorElement.getAttributeValue("", "module");
                            string lineVal = errorElement.getAttributeValue("", "line");
                            int line;
                            try {
                                line = ((lineVal == null) ? -1 : Saxon.Impl.Helpers.Utils.ParseInt(lineVal));
                            } catch (Saxon.Ejava.lang.NumberFormatException ex) {
                                line = -1;
                            }
                            string columnVal = errorElement.getAttributeValue("", "column");
                            int col;
                            try {
                                col = ((columnVal == null) ? -1 : Saxon.Impl.Helpers.Utils.ParseInt(columnVal));
                            } catch (Saxon.Ejava.lang.NumberFormatException ex) {
                                col = -1;
                            }
                            Saxon.Hej.expr.parser.Loc locator = new Saxon.Hej.expr.parser.Loc(module, line, col);
                            e.setLocator(locator);
                        }
                    }
                }
                e.setErrorObject(errorObject);
            }
            throw e;
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            int len = arguments.Length;
            switch (len) {
              case 0:
                return error(context, null, null, null);
              case 1:
                Saxon.Hej.value.QNameValue arg0 = (Saxon.Hej.value.QNameValue)(arguments[0].head());
                if ((arg0 == null)) {
                    arg0 = new Saxon.Hej.value.QNameValue("err", Saxon.Hej.lib.NamespaceConstant.ERR, "FOER0000");
                }
                return error(context, arg0, null, null);
              case 2:
                return error(context, (Saxon.Hej.value.QNameValue)(arguments[0].head()), (Saxon.Hej.value.StringValue)(arguments[1].head()), null);
              case 3:
                return error(context, (Saxon.Hej.value.QNameValue)(arguments[0].head()), (Saxon.Hej.value.StringValue)(arguments[1].head()), arguments[2].iterate());
              default:
                return null;
            }
        }
        public class UserDefinedXPathException : Saxon.Hej.trans.XPathException {
            public UserDefinedXPathException(string message) : base (message) {
            }
        
        }
    
    }
}