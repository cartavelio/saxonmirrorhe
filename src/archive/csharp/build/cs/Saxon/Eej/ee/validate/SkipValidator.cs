using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class SkipValidator : Saxon.Eej.ee.validate.ContentValidator {
        private bool stripIdProperty = false;
        public SkipValidator(Saxon.Hej.@event.Receiver next) : base (next) {
            setPipelineConfiguration(next.getPipelineConfiguration());
        }
        public override void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            base.setPipelineConfiguration(pipe);
            stripIdProperty = (pipe.getHostLanguage() == Saxon.Api.HostLanguage.XQUERY);
        }
        public override Saxon.Hej.type.SchemaType getSchemaType() {
            return null;
        }
        protected internal override Saxon.Eej.ee.validate.ConstraintChecker getConstraintChecker() {
            return null;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            int p2 = properties;
            if (((!(stripIdProperty) && (type != Saxon.Hej.type.Untyped.getInstance())) && (!(Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.ID_IDREF_CHECKED))))) {
                if (type.isIdType()) {
                    p2 |= (Saxon.Hej.@event.ReceiverOption.IS_ID);
                } else {
                    if (type.isIdRefType()) {
                        p2 |= (Saxon.Hej.@event.ReceiverOption.IS_IDREF);
                    }
                }
                p2 |= (Saxon.Hej.@event.ReceiverOption.ID_IDREF_CHECKED);
            }
            p2 &= ~(Saxon.Hej.@event.ReceiverOption.NILLED_ELEMENT);
            Saxon.Hej.om.AttributeMap atts2;
            try {
                atts2 = attributes.apply(att => {
                    int prop = att.getProperties();
                    if ((!(stripIdProperty) && (!(Saxon.Hej.@event.ReceiverOption.contains(properties, Saxon.Hej.@event.ReceiverOption.ID_IDREF_CHECKED))))) {
                        Saxon.Hej.type.SimpleType attType = att.getType();
                        if (attType.isIdType()) {
                            prop |= (Saxon.Hej.@event.ReceiverOption.IS_ID);
                        } else {
                            if (attType.isIdRefType()) {
                                prop |= (Saxon.Hej.@event.ReceiverOption.IS_IDREF);
                            }
                        }
                        prop |= (Saxon.Hej.@event.ReceiverOption.ID_IDREF_CHECKED);
                    }
                    if (stripIdProperty) {
                        prop &= ~(((Saxon.Hej.@event.ReceiverOption.IS_ID|Saxon.Hej.@event.ReceiverOption.IS_IDREF)));
                        prop |= Saxon.Hej.@event.ReceiverOption.ID_IDREF_CHECKED;
                    }
                    return new Saxon.Hej.om.AttributeInfo(att.getNodeName(), Saxon.Hej.type.BuiltInAtomicType.UNTYPED_ATOMIC, att.getValue(), att.getLocation(), prop);
                } );
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw e.getXPathException();
            }
            getNextReceiver().startElement(elemName, Saxon.Hej.type.Untyped.getInstance(), atts2, namespaces, location, p2);
        }
        protected internal override Saxon.Eej.ee.validate.ContentValidator getChildValidator() {
            return this;
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                decompose(item, locationId, copyNamespaces);
            } else {
                nextReceiver.append(item, locationId, copyNamespaces);
            }
        }
    
    }
}