using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.stream.feed {
    public class ElementCreatorFeed : Saxon.Eej.ee.stream.feed.ComplexNodeEventFeed {
        public ElementCreatorFeed(Saxon.Hej.expr.Expression exp, Saxon.Eej.ee.stream.feed.ItemFeed parentExpressionFeed, Saxon.Hej.expr.XPathContext context) : base (exp, parentExpressionFeed, context) {
        }
        protected internal override void processLeft(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.expr.instruct.ElementCreator instr = (Saxon.Hej.expr.instruct.ElementCreator)(getExpression());
            try {
                Saxon.Hej.om.NodeName elemName = instr.getElementName(context, null);
                Saxon.Hej.type.SchemaType typeCode = ((instr.getValidationAction() == Saxon.Hej.lib.Validation.PRESERVE) ? Saxon.Hej.type.AnyType.getInstance() : Saxon.Hej.type.Untyped.getInstance());
                if (!(instr.isPreservingTypes())) {
                    Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions(instr.getValidationOptions());
                    options.setTopLevelElement(elemName.getStructuredQName());
                    Saxon.Hej.@event.Receiver validator = context.getConfiguration().getElementValidator(next, options, instr.getLocation());
                    next = new Saxon.Hej.@event.ComplexContentOutputter(validator);
                }
                if ((next.getSystemId() == null)) {
                    next.setSystemId(instr.getNewBaseURI(context, null));
                }
                bool condition = !(instr.isBequeathNamespacesToChildren());
                int properties = (condition ? Saxon.Hej.@event.ReceiverOption.DISINHERIT_NAMESPACES : Saxon.Hej.@event.ReceiverOption.NONE);
                next.startElement(elemName, typeCode, instr.getLocation(), properties);
                instr.outputNamespaceNodes(next, elemName, null);
            } catch (Saxon.Hej.trans.XPathException e) {
                e.maybeSetLocation(instr.getLocation());
                e.maybeSetContext(context);
                throw;
            }
        }
        protected internal override void processRight(Saxon.Hej.expr.XPathContext context) {
            try {
                endElement();
            } catch (Saxon.Hej.trans.XPathException e) {
                dynamicError(e);
            }
        }
        public override string ToString() {
            return getExpression().toShortString();
        }
    
    }
}