using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trace {
    public class XSLTTraceCodeInjector : Saxon.Hej.trace.TraceCodeInjector {
        protected internal override bool isApplicable(Saxon.Hej.expr.Expression exp) {
            if ((traceLevel == Saxon.Hej.trace.TraceLevel.HIGH)) {
                return !(((((exp is Saxon.Hej.expr.instruct.TraceExpression) || (exp is Saxon.Hej.expr.instruct.OnEmptyExpr)) || (exp is Saxon.Hej.expr.instruct.OnNonEmptyExpr))));
            }
            return isTraceableExpression(exp);
        }
        public static bool isTraceableExpression(Saxon.Hej.expr.Expression exp) {
            if (Saxon.Hej.expr.Literal.isEmptySequence(exp)) {
                return false;
            }
            if (((exp is Saxon.Hej.expr.instruct.OnEmptyExpr) || (exp is Saxon.Hej.expr.instruct.OnNonEmptyExpr))) {
                return false;
            }
            Saxon.Hej.expr.Expression parent = exp.getParentExpression();
            if ((parent is Saxon.Hej.expr.instruct.TraceExpression)) {
                parent = parent.getParentExpression();
            }
            if (exp.isCallOn(typeof(Saxon.Hej.functions.TransformFn))) {
                return true;
            }
            if (((parent != null) && (parent.getLocation() is Saxon.Hej.expr.parser.XPathParser.NestedLocation))) {
                return false;
            }
            Saxon.Hej.s9api.Location loc = exp.getLocation();
            if ((loc is Saxon.Hej.expr.parser.XPathParser.NestedLocation)) {
                loc = ((Saxon.Hej.expr.parser.XPathParser.NestedLocation)(loc)).getContainingLocation();
            }
            if ((loc is Saxon.Hej.tree.AttributeLocation)) {
                Saxon.Hej.om.StructuredQName elementName = ((Saxon.Hej.tree.AttributeLocation)(loc)).getElementName();
                return (elementName.hasURI(Saxon.Hej.om.NamespaceUri.XSLT) && elementName.getLocalPart().Equals("sequence"));
            }
            return (((loc != null) && (loc.getLineNumber() != -1)) && !(((((parent != null) && (loc == parent.getLocation())) && !((((parent is Saxon.Hej.expr.instruct.Block) || (parent is Saxon.Hej.expr.instruct.ComponentTracer))))))));
        }
    
    }
}