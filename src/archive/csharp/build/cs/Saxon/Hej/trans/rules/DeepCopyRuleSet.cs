using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans.rules {
    public class DeepCopyRuleSet : Saxon.Hej.trans.rules.BuiltInRuleSet {
        private static readonly Saxon.Hej.trans.rules.DeepCopyRuleSet THE_INSTANCE = new Saxon.Hej.trans.rules.DeepCopyRuleSet();
        public static Saxon.Hej.trans.rules.DeepCopyRuleSet getInstance() {
            return THE_INSTANCE;
        }
        private DeepCopyRuleSet()  {
        }
        public void process(Saxon.Hej.om.Item item, Saxon.Hej.expr.instruct.ParameterSet parameters, Saxon.Hej.expr.instruct.ParameterSet tunnelParams, Saxon.Hej.@event.Outputter @out, Saxon.Hej.expr.XPathContext context, Saxon.Hej.s9api.Location locationId) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                Saxon.Hej.om.NodeInfo node = (Saxon.Hej.om.NodeInfo)(item);
                switch (node.getNodeKind()) {
                  case Saxon.Hej.type.Type.DOCUMENT:
                  case Saxon.Hej.type.Type.ELEMENT:
                    {
                        if ((@out.getSystemId() == null)) {
                            @out.setSystemId(node.getBaseURI());
                        }
                        Saxon.Hej.tree.util.Navigator.copy(node, @out, (Saxon.Hej.om.CopyOptions.ALL_NAMESPACES|Saxon.Hej.om.CopyOptions.TYPE_ANNOTATIONS), locationId);
                        return ;
                    }
                  case Saxon.Hej.type.Type.TEXT:
                    @out.characters(item.getUnicodeStringValue(), locationId, Saxon.Hej.@event.ReceiverOption.NONE);
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
                    @out.@namespace(node.getLocalPart(), Saxon.Hej.om.NamespaceUri.of(node.getStringValue()), Saxon.Hej.@event.ReceiverOption.NONE);
                    return ;
                  default:
                    break;
                }
            } else {
                @out.append(item, locationId, Saxon.Hej.@event.ReceiverOption.NONE);
            }
        }
        public Saxon.Hej.trans.rules.BuiltInRules[] getActionForParentNodes(int nodeKind) {
            return new Saxon.Hej.trans.rules.BuiltInRules[]{Saxon.Hej.trans.rules.BuiltInRules.DEEP_COPY};
        }
        public string getName() {
            return "deep-copy";
        }
    
    }
}