using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class Path_1 : Saxon.Hej.functions.ScalarSystemFunction {
        public override Saxon.Hej.value.AtomicValue evaluate(Saxon.Hej.om.Item arg, Saxon.Hej.expr.XPathContext context) {
            return makePath((Saxon.Hej.om.NodeInfo)(arg), context);
        }
        public static Saxon.Hej.value.StringValue makePath(Saxon.Hej.om.NodeInfo node, Saxon.Hej.expr.XPathContext context) {
            if ((node.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                return Saxon.Hej.value.StringValue.makeStringValue("/");
            }
            Saxon.Ejava.lang.StringBuilder fsb = new Saxon.Ejava.lang.StringBuilder(256);
            Saxon.Hej.tree.iter.AxisIterator iter = node.iterateAxis(Saxon.Hej.om.AxisInfo.ANCESTOR_OR_SELF);
            Saxon.Hej.om.NodeInfo n;
            while (((n = ((/*1430*/Saxon.Hej.om.NodeInfo)iter.next())) != null)) {
                if ((n.getParent() == null)) {
                    if ((n.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                        return new Saxon.Hej.value.StringValue(fsb.ToString());
                    } else {
                        fsb.insert(0, "Q{http://www.w3.org/2005/xpath-functions}root()");
                        return new Saxon.Hej.value.StringValue(fsb.ToString());
                    }
                }
                Saxon.Ejava.lang.StringBuilder fsb2 = new Saxon.Ejava.lang.StringBuilder(256);
                switch (n.getNodeKind()) {
                  case Saxon.Hej.type.Type.DOCUMENT:
                    return new Saxon.Hej.value.StringValue(fsb.ToString());
                  case Saxon.Hej.type.Type.ELEMENT:
                    fsb2.append("/Q{").append(n.getNamespaceUri()).append("}");
                    fsb2.append(n.getLocalPart());
                    fsb2.append("[").append(Saxon.Hej.tree.util.Navigator.getNumberSimple(n, context)).append("]");
                    fsb2.append(fsb);
                    fsb = fsb2;
                    break ;
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                    fsb2.append("/@");
                    string attURI = n.getNamespaceUri().ToString();
                    if (!("".Equals(attURI))) {
                        fsb2.append("Q{").append(attURI).append("}");
                    }
                    fsb2.append(n.getLocalPart());
                    fsb2.append(fsb);
                    fsb = fsb2;
                    break ;
                  case Saxon.Hej.type.Type.TEXT:
                    fsb2.append("/text()[").append((Saxon.Hej.tree.util.Navigator.getNumberSimple(n, context)+"]")).append(fsb);
                    fsb = fsb2;
                    break ;
                  case Saxon.Hej.type.Type.COMMENT:
                    fsb2.append("/comment()[").append(Saxon.Hej.tree.util.Navigator.getNumberSimple(n, context)).append("]");
                    fsb2.append(fsb);
                    fsb = fsb2;
                    break ;
                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                    fsb2.append("/processing-instruction(").append(n.getLocalPart()).append(")[");
                    fsb2.append(Saxon.Hej.tree.util.Navigator.getNumberSimple(n, context)).append("]");
                    fsb2.append(fsb);
                    fsb = fsb2;
                    break ;
                  case Saxon.Hej.type.Type.NAMESPACE:
                    fsb2.append("/namespace::");
                    if (n.getLocalPart().Length==0) {
                        fsb2.append((("*[Q{"+Saxon.Hej.lib.NamespaceConstant.FN)+"}local-name()=\"\"]"));
                    } else {
                        fsb.append(n.getLocalPart());
                    }
                    fsb2.append(fsb);
                    fsb = fsb2;
                    break ;
                  default:
                    throw new Saxon.Ejava.lang.AssertionError();
                }
            }
            fsb.insert(0, "Q{http://www.w3.org/2005/xpath-functions}root()");
            return new Saxon.Hej.value.StringValue(fsb.ToString());
        }
    
    }
}