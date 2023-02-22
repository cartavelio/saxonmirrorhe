using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public class ShallowCopyRuleSet : Saxon.Hej.trans.rules.BuiltInRuleSet {
        private static readonly Saxon.Hej.trans.rules.ShallowCopyRuleSet THE_INSTANCE = new Saxon.Hej.trans.rules.ShallowCopyRuleSet();
        public static Saxon.Hej.trans.rules.ShallowCopyRuleSet getInstance() {
            return THE_INSTANCE;
        }
        private ShallowCopyRuleSet()  {
        }
        public void process(Saxon.Hej.om.Item item, Saxon.Hej.expr.instruct.ParameterSet parameters, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.@event.Outputter @out, Saxon.Hej.expr.XPathContext context, Saxon.Hej.s9api.Location locationId) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                switch (node.getNodeKind()) {
                  case Saxon.Hej.type.Type.DOCUMENT:
                    {
                        Saxon.Hej.@event.PipelineConfiguration pipe = @out.getPipelineConfiguration();
                        if ((@out.getSystemId() == null)) {
                            @out.setSystemId(node.getBaseURI());
                        }
                        @out.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                        Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                        c2.setOrigin(this);
                        c2.trackFocus(node.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD));
                        c2.setCurrentComponent(c2.getCurrentMode());
                        pipe.setXPathContext(c2);
                        Saxon.Hej.expr.instruct.TailCall tc = context.getCurrentMode().getActor().applyTemplates(parameters, tunnelParams, null, @out, c2, locationId);
                        while ((tc != null)) {
                            tc = tc.processLeavingTail();
                        }
                        @out.endDocument();
                        pipe.setXPathContext(context);
                        return ;
                    }
                  case Saxon.Hej.type.Type.ELEMENT:
                    {
                        bool schemaAware = context.getController().getExecutable().isSchemaAware();
                        Saxon.Hej.@event.PipelineConfiguration pipe = @out.getPipelineConfiguration();
                        if ((@out.getSystemId() == null)) {
                            @out.setSystemId(node.getBaseURI());
                        }
                        Saxon.Hej.om.NodeName fqn = Saxon.Hej.om.NameOfNode.makeName(node);
                        @out.startElement(fqn, (schemaAware ? Saxon.Hej.type.AnyType.getInstance() : Saxon.Hej.type.Untyped.getInstance()), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                        foreach (Saxon.Hej.om.NamespaceBinding ns in node.getAllNamespaces()) {
                            @out.@namespace(ns.getPrefix(), ns.getURI(), Saxon.Hej.@event.ReceiverOption.NONE);
                        }
                        Saxon.Hej.expr.XPathContextMajor c2 = context.newContext();
                        c2.setCurrentComponent(c2.getCurrentMode());
                        pipe.setXPathContext(c2);
                        Saxon.Hej.tree.iter.AxisIterator attributes = node.iterateAxis(Saxon.Hej.om.AxisInfo.ATTRIBUTE);
                        if ((attributes != Saxon.Hej.tree.iter.EmptyIterator.ofNodes())) {
                            c2.setOrigin(this);
                            c2.trackFocus(attributes);
                            Saxon.Hej.expr.instruct.TailCall tc = c2.getCurrentMode().getActor().applyTemplates(parameters, tunnelParams, null, @out, c2, locationId);
                            while ((tc != null)) {
                                tc = tc.processLeavingTail();
                            }
                        }
                        if (node.hasChildNodes()) {
                            c2.trackFocus(node.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD));
                            Saxon.Hej.expr.instruct.TailCall tc = c2.getCurrentMode().getActor().applyTemplates(parameters, tunnelParams, null, @out, c2, locationId);
                            while ((tc != null)) {
                                tc = tc.processLeavingTail();
                            }
                        }
                        @out.endElement();
                        pipe.setXPathContext(context);
                        return ;
                    }
                  case Saxon.Hej.type.Type.TEXT:
                    @out.characters(node.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    return ;
                  case Saxon.Hej.type.Type.COMMENT:
                    @out.comment(node.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    return ;
                  case Saxon.Hej.type.Type.PROCESSING_INSTRUCTION:
                    @out.processingInstruction(node.getLocalPart(), node.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    return ;
                  case Saxon.Hej.type.Type.ATTRIBUTE:
                    @out.attribute(Saxon.Hej.om.NameOfNode.makeName(node), (Saxon.Hej.type.SimpleType)(node.getSchemaType()), node.getStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
                    return ;
                  case Saxon.Hej.type.Type.NAMESPACE:
                    @out.@namespace(node.getLocalPart(), node.getStringValue(), Saxon.Hej.@event.ReceiverOption.NONE);
                    return ;
                  default:
                    break;
                }
            } else {
                @out.append(item, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
            }
        }
        public string getName() {
            return "shallow-copy";
        }
        public Saxon.Hej.trans.rules.BuiltInRules[] getActionForParentNodes(int nodeKind) {
            return new Saxon.Hej.trans.rules.BuiltInRules[]{Saxon.Hej.trans.rules.BuiltInRules.SHALLOW_COPY, Saxon.Hej.trans.rules.BuiltInRules.APPLY_TEMPLATES_TO_ATTRIBUTES, Saxon.Hej.trans.rules.BuiltInRules.APPLY_TEMPLATES_TO_CHILDREN};
        }
    
    }
}