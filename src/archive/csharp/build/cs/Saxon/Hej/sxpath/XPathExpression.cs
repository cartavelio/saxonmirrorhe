using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.sxpath {
    public class XPathExpression {
        private readonly Saxon.Hej.expr.StaticContext env;
        private readonly Saxon.Hej.expr.Expression expression;
        private Saxon.Hej.expr.instruct.SlotManager stackFrameMap;
        private readonly Saxon.Hej.expr.instruct.Executable executable;
        private int numberOfExternalVariables;
        protected internal XPathExpression(Saxon.Hej.expr.StaticContext env, Saxon.Hej.expr.Expression exp, Saxon.Hej.expr.instruct.Executable exec)  {
            expression = exp;
            this.env = env;
            this.executable = exec;
        }
        public Saxon.Hej.expr.instruct.Executable getExecutable() {
            return executable;
        }
        protected internal void setStackFrameMap(Saxon.Hej.expr.instruct.SlotManager map, int numberOfExternalVariables) {
            stackFrameMap = map;
            this.numberOfExternalVariables = numberOfExternalVariables;
        }
        public Saxon.Hej.sxpath.XPathDynamicContext createDynamicContext() {
            Saxon.Hej.expr.XPathContextMajor context = new Saxon.Hej.expr.XPathContextMajor(null, executable);
            context.openStackFrame(stackFrameMap);
            return new Saxon.Hej.sxpath.XPathDynamicContext(env.getRequiredContextItemType(), context, stackFrameMap);
        }
        public Saxon.Hej.sxpath.XPathDynamicContext createDynamicContext(Saxon.Hej.om.Item contextItem) {
            checkContextItemType(contextItem);
            Saxon.Hej.expr.XPathContextMajor context = new Saxon.Hej.expr.XPathContextMajor(contextItem, executable);
            context.openStackFrame(stackFrameMap);
            return new Saxon.Hej.sxpath.XPathDynamicContext(env.getRequiredContextItemType(), context, stackFrameMap);
        }
        public Saxon.Hej.sxpath.XPathDynamicContext createDynamicContext(Saxon.Hej.Controller controller, Saxon.Hej.om.Item contextItem) {
            checkContextItemType(contextItem);
            if ((controller == null)) {
                return createDynamicContext(contextItem);
            } else {
                Saxon.Hej.expr.XPathContextMajor context = controller.newXPathContext();
                context.openStackFrame(stackFrameMap);
                Saxon.Hej.sxpath.XPathDynamicContext dc = new Saxon.Hej.sxpath.XPathDynamicContext(env.getRequiredContextItemType(), context, stackFrameMap);
                if ((contextItem != null)) {
                    dc.setContextItem(contextItem);
                }
                return dc;
            }
        }
        private void checkContextItemType(Saxon.Hej.om.Item contextItem) {
            if ((contextItem != null)) {
                Saxon.Hej.type.ItemType type = env.getRequiredContextItemType();
                Saxon.Hej.type.TypeHierarchy th = env.getConfiguration().getTypeHierarchy();
                if (!(type.matches(contextItem, th))) {
                    throw new Saxon.Hej.trans.XPathException(("Supplied context item does not match required context item type "+type));
                }
            }
        }
        public Saxon.Hej.om.SequenceIterator iterate(Saxon.Hej.sxpath.XPathDynamicContext context) {
            context.checkExternalVariables(stackFrameMap, numberOfExternalVariables);
            return expression.iterate(context.getXPathContextObject());
        }
        public System.Collections.Generic.IList<Saxon.Hej.om.Item> evaluate(Saxon.Hej.sxpath.XPathDynamicContext context) {
            System.Collections.Generic.IList<Saxon.Hej.om.Item> list = new System.Collections.Generic.List<Saxon.Hej.om.Item>(20);
            Saxon.Hej.om.SequenceTool.supply(expression.iterate(context.getXPathContextObject()), (Saxon.Hej.om.ItemConsumer<Saxon.Hej.om.Item>)((item => list.Add(item))));
            return list;
        }
        public Saxon.Hej.om.Item evaluateSingle(Saxon.Hej.sxpath.XPathDynamicContext context) {
            Saxon.Hej.om.SequenceIterator iter = expression.iterate(context.getXPathContextObject());
            Saxon.Hej.om.Item result = iter.next();
            iter.close();
            return result;
        }
        public bool effectiveBooleanValue(Saxon.Hej.sxpath.XPathDynamicContext context) {
            return expression.effectiveBooleanValue(context.getXPathContextObject());
        }
        public Saxon.Hej.expr.Expression getInternalExpression() {
            return expression;
        }
    
    }
}