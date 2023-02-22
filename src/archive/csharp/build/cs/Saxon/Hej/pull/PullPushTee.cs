using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.pull {
    public class PullPushTee : Saxon.Hej.pull.PullFilter {
        private readonly Saxon.Hej.@event.Receiver branch;
        internal bool previousAtomic = false;
        private readonly System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap> nsStack = new System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceMap>();
        public PullPushTee(Saxon.Hej.pull.PullProvider @base, Saxon.Hej.@event.Receiver branch) : base (@base) {
            this.branch = branch;
        }
        public Saxon.Hej.@event.Receiver getReceiver() {
            return branch;
        }
        public override Saxon.Hej.pull.PullEvent next() {
            currentEvent = base.next();
            copyEvent(currentEvent);
            return currentEvent;
        }
        private void copyEvent(Saxon.Hej.pull.PullEvent @event) {
            Saxon.Hej.pull.PullProvider @in = getUnderlyingProvider();
            Saxon.Hej.s9api.Location loc = @in.getSourceLocator();
            if ((loc == null)) {
                loc = Saxon.Hej.expr.parser.Loc.NONE;
            }
            Saxon.Hej.@event.Receiver @out = branch;
            switch (@event) {
              case Saxon.Hej.pull.PullEvent.START_DOCUMENT:
                @out.startDocument(Saxon.Hej.@event.ReceiverOption.NONE);
                break ;
              case Saxon.Hej.pull.PullEvent.START_ELEMENT:
                Saxon.Hej.om.NamespaceBinding[] bindings = @in.getNamespaceDeclarations();
                Saxon.Hej.om.NamespaceMap nsMap = ((nsStack.Count==0) ? Saxon.Hej.om.NamespaceMap.emptyMap() : nsStack.Peek());
                foreach (Saxon.Hej.om.NamespaceBinding binding in bindings) {
                    if ((binding == null)) {
                        break ;
                    }
                    if (!(((binding.getPrefix().Length==0 && binding.getURI().Equals(nsMap.getDefaultNamespace()))))) {
                        nsMap = nsMap.put(binding.getPrefix(), binding.getURI());
                    }
                }
                nsStack.Push(nsMap);
                @out.startElement(@in.getNodeName(), @in.getSchemaType(), @in.getAttributes(), nsMap, loc, Saxon.Hej.@event.ReceiverOption.NAMESPACE_OK);
                break ;
              case Saxon.Hej.pull.PullEvent.TEXT:
                @out.characters(@in.getStringValue(), loc, Saxon.Hej.@event.ReceiverOption.WHOLE_TEXT_NODE);
                break ;
              case Saxon.Hej.pull.PullEvent.COMMENT:
                @out.comment(@in.getStringValue(), loc, Saxon.Hej.@event.ReceiverOption.NONE);
                break ;
              case Saxon.Hej.pull.PullEvent.PROCESSING_INSTRUCTION:
                @out.processingInstruction(@in.getNodeName().getLocalPart(), @in.getStringValue(), loc, Saxon.Hej.@event.ReceiverOption.NONE);
                break ;
              case Saxon.Hej.pull.PullEvent.END_ELEMENT:
                @out.endElement();
                nsStack.Pop();
                break ;
              case Saxon.Hej.pull.PullEvent.END_DOCUMENT:
                System.Collections.Generic.IList<Saxon.Hej.pull.UnparsedEntity> entities = @in.getUnparsedEntities();
                if ((entities != null)) {
                    foreach (System.Object entity in entities) {
                        Saxon.Hej.pull.UnparsedEntity ue = (Saxon.Hej.pull.UnparsedEntity)(entity);
                        @out.setUnparsedEntity(ue.getName(), ue.getSystemId(), ue.getPublicId());
                    }
                }
                @out.endDocument();
                break ;
              case Saxon.Hej.pull.PullEvent.END_OF_INPUT:
                @in.close();
                break ;
              case Saxon.Hej.pull.PullEvent.ATOMIC_VALUE:
                if ((@out is Saxon.Hej.@event.SequenceReceiver)) {
                    @out.append(base.getAtomicValue(), loc, Saxon.Hej.@event.ReceiverOption.NONE);
                } else {
                    if (previousAtomic) {
                        @out.characters(Saxon.Hej.str.StringConstants.SINGLE_SPACE, loc, Saxon.Hej.@event.ReceiverOption.NONE);
                    }
                    @out.characters(@in.getStringValue(), loc, Saxon.Hej.@event.ReceiverOption.NONE);
                }
                break ;
              case Saxon.Hej.pull.PullEvent.ATTRIBUTE:
                if ((@out is Saxon.Hej.@event.SequenceReceiver)) {
                    Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(@in.getPipelineConfiguration().getConfiguration());
                    o.setNodeName(getNodeName());
                    o.setNodeKind(Saxon.Hej.type.Type.ATTRIBUTE);
                    o.setStringValue(getStringValue().tidy());
                    @out.append(o, loc, Saxon.Hej.@event.ReceiverOption.NONE);
                }
                break ;
              case Saxon.Hej.pull.PullEvent.NAMESPACE:
                if ((@out is Saxon.Hej.@event.SequenceReceiver)) {
                    Saxon.Hej.tree.util.Orphan o = new Saxon.Hej.tree.util.Orphan(@in.getPipelineConfiguration().getConfiguration());
                    o.setNodeName(getNodeName());
                    o.setNodeKind(Saxon.Hej.type.Type.NAMESPACE);
                    o.setStringValue(getStringValue().tidy());
                    @out.append(o, loc, Saxon.Hej.@event.ReceiverOption.NONE);
                }
                break ;
              default:
                throw new Saxon.Ejava.lang.UnsupportedOperationException((""+@event));
            }
            previousAtomic = (@event == Saxon.Hej.pull.PullEvent.ATOMIC_VALUE);
        }
    
    }
}