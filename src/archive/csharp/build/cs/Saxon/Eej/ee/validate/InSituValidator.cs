using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class InSituValidator {
        private readonly Saxon.Hej.om.MutableNodeInfo[] nodeBuffer = new Saxon.Hej.om.MutableNodeInfo[16];
        private readonly Saxon.Hej.om.MutableNodeInfo root;
        private readonly int validationMode;
        public InSituValidator(Saxon.Hej.om.MutableNodeInfo root, int validationMode)  {
            this.root = root;
            this.validationMode = validationMode;
        }
        public void validate() {
            Saxon.Hej.Configuration config = root.getConfiguration();
            Saxon.Hej.@event.PipelineConfiguration pipe = config.makePipelineConfiguration();
            pipe.setHostLanguage(Saxon.Api.HostLanguage.XML_SCHEMA);
            Saxon.Hej.@event.SequenceReceiver endPoint = new Saxon.Eej.ee.validate.InSituValidator.NodeUpdater(nodeBuffer, pipe);
            Saxon.Hej.lib.ParseOptions options = new Saxon.Hej.lib.ParseOptions().withSchemaValidationMode(validationMode).withSpaceStrippingRule(null);
            Saxon.Hej.@event.Receiver validator;
            if ((root.getNodeKind() == Saxon.Hej.type.Type.DOCUMENT)) {
                validator = config.getDocumentValidator(endPoint, root.getSystemId(), options, null);
            } else {
                options = options.withTopLevelElement(Saxon.Hej.om.NameOfNode.makeName(root).getStructuredQName());
                validator = config.getElementValidator(endPoint, options, Saxon.Hej.expr.parser.Loc.NONE);
            }
            Saxon.Eej.ee.validate.InSituValidator.NodeCounter nodeCounter = new Saxon.Eej.ee.validate.InSituValidator.NodeCounter(nodeBuffer);
            pipe.setCopyInformee(nodeCounter.notifyElementNode /* instance method reference */);
            root.copy(validator, Saxon.Hej.om.CopyOptions.ALL_NAMESPACES, Saxon.Hej.expr.parser.Loc.NONE);
        }
        private class NodeCounter : Saxon.Hej.@event.CopyInformee {
            internal int counter = 0;
            readonly Saxon.Hej.om.MutableNodeInfo[] nodeBuffer;
            public NodeCounter(Saxon.Hej.om.MutableNodeInfo[] nodeBuffer)  {
                this.nodeBuffer = nodeBuffer;
            }
            public System.Object notifyElementNode(Saxon.Hej.om.NodeInfo element) {
                nodeBuffer[(counter%16)] = (Saxon.Hej.om.MutableNodeInfo)(element);
                return new Saxon.Hej.expr.parser.Loc(element.getSystemId(), counter++, 998877);
            }
        
        }
        private class NodeUpdater : Saxon.Hej.@event.ProxyReceiver {
            private Saxon.Hej.om.MutableNodeInfo currentElement;
            readonly Saxon.Hej.om.MutableNodeInfo[] nodeBuffer;
            public NodeUpdater(Saxon.Hej.om.MutableNodeInfo[] nodeBuffer, Saxon.Hej.@event.PipelineConfiguration pipe) : base (new Saxon.Hej.@event.Sink(pipe)) {
                this.nodeBuffer = nodeBuffer;
            }
            public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
                Saxon.Hej.om.MutableNodeInfo node = nodeBuffer[(location.getLineNumber()%16)];
                node.setTypeAnnotation(type);
                currentElement = node;
                node.setAttributes(attributes);
            }
            public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
                if (Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.DEFAULTED_VALUE)) {
                    Saxon.Hej.tree.util.Orphan child = new Saxon.Hej.tree.util.Orphan(getConfiguration());
                    child.setNodeKind(Saxon.Hej.type.Type.TEXT);
                    child.setStringValue(chars.tidy());
                    currentElement.insertChildren(new Saxon.Hej.om.NodeInfo[]{child}, true, true);
                }
                base.characters(chars, locationId, properties);
            }
            public override bool usesTypeAnnotations() {
                return true;
            }
        
        }
    
    }
}