using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class TextValueTemplateContext : Saxon.Hej.style.ExpressionContext {
        internal Saxon.Hej.style.TextValueTemplateNode textNode;
        public TextValueTemplateContext(Saxon.Hej.style.StyleElement parent, Saxon.Hej.style.TextValueTemplateNode textNode) : base (parent, null) {
            this.textNode = textNode;
        }
        public override Saxon.Hej.expr.Expression bindVariable(Saxon.Hej.om.StructuredQName qName) {
            Saxon.Hej.style.SourceBinding siblingVar = bindLocalVariable(qName);
            if ((siblingVar == null)) {
                return base.bindVariable(qName);
            } else {
                Saxon.Hej.expr.VariableReference var = new Saxon.Hej.expr.LocalVariableReference(qName);
                siblingVar.registerReference(var);
                return var;
            }
        }
        private Saxon.Hej.style.SourceBinding bindLocalVariable(Saxon.Hej.om.StructuredQName qName) {
            Saxon.Hej.om.NodeInfo curr = textNode;
            Saxon.Hej.tree.iter.AxisIterator preceding = curr.iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING);
            while (((curr = ((/*1430*/Saxon.Hej.om.NodeInfo)preceding.next())) != null)) {
                if ((curr is Saxon.Hej.style.XSLGeneralVariable)) {
                    Saxon.Hej.style.SourceBinding sourceBinding = ((Saxon.Hej.style.XSLGeneralVariable)(curr)).getBindingInformation(qName);
                    if ((sourceBinding != null)) {
                        return sourceBinding;
                    }
                }
            }
            return null;
        }
    
    }
}