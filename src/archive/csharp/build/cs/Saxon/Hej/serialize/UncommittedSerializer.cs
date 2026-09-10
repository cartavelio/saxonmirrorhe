using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class UncommittedSerializer : Saxon.Hej.@event.ProxyReceiver {
        private bool committed = false;
        private Saxon.Hej.@event.EventBuffer pending = null;
        private readonly Saxon.Ejavax.xml.transform.Result finalResult;
        private readonly Saxon.Hej.serialize.SerializationProperties properties;
        public UncommittedSerializer(Saxon.Ejavax.xml.transform.Result finalResult, Saxon.Hej.@event.Receiver next, Saxon.Hej.serialize.SerializationProperties @params) : base (next) {
            this.finalResult = finalResult;
            this.properties = @params;
        }
        public override void open() {
            committed = false;
        }
        public override void close() {
            if (!(committed)) {
                switchToMethod("xml");
            }
            getNextReceiver().close();
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (committed) {
                getNextReceiver().characters(chars, locationId, properties);
            } else {
                if ((pending == null)) {
                    pending = new Saxon.Hej.@event.EventBuffer(getPipelineConfiguration());
                }
                pending.characters(chars, locationId, properties);
                if (!(Saxon.Hej.value.Whitespace.isAllWhite(chars))) {
                    switchToMethod("xml");
                }
            }
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            if (committed) {
                getNextReceiver().processingInstruction(target, data, locationId, properties);
            } else {
                if ((pending == null)) {
                    pending = new Saxon.Hej.@event.EventBuffer(getPipelineConfiguration());
                }
                pending.processingInstruction(target, data, locationId, properties);
            }
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if (committed) {
                getNextReceiver().comment(chars, locationId, properties);
            } else {
                if ((pending == null)) {
                    pending = new Saxon.Hej.@event.EventBuffer(getPipelineConfiguration());
                }
                pending.comment(chars, locationId, properties);
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            if (!(committed)) {
                string name = elemName.getLocalPart();
                string uri = elemName.getURI();
                if ((Saxon.Impl.Helpers.StringUtils.stringEqualsIgnoreCase(name, "html") && uri.Length==0)) {
                    switchToMethod("html");
                } else {
                    if ((name.Equals("html") && uri.Equals(Saxon.Hej.lib.NamespaceConstant.XHTML))) {
                        string version = this.properties.getProperties().getProperty(Saxon.Hej.lib.SaxonOutputKeys.STYLESHEET_VERSION);
                        if ("10".Equals(version)) {
                            switchToMethod("xml");
                        } else {
                            switchToMethod("xhtml");
                        }
                    } else {
                        switchToMethod("xml");
                    }
                }
            }
            getNextReceiver().startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void startDocument(int properties) {
            if (committed) {
                getNextReceiver().startDocument(properties);
            } else {
                if ((pending == null)) {
                    pending = new Saxon.Hej.@event.EventBuffer(getPipelineConfiguration());
                }
                pending.startDocument(properties);
            }
        }
        public override void endDocument() {
            if (!(committed)) {
                switchToMethod("xml");
            }
            getNextReceiver().endDocument();
        }
        private void switchToMethod(string method) {
            Saxon.Ejava.util.Properties newProperties = new Saxon.Ejava.util.Properties(properties.getProperties());
            newProperties.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD, method);
            Saxon.Hej.lib.SerializerFactory sf = getConfiguration().getSerializerFactory();
            Saxon.Hej.serialize.SerializationProperties newParams = new Saxon.Hej.serialize.SerializationProperties(newProperties, properties.getCharacterMapIndex());
            newParams.setValidationFactory(properties.getValidationFactory());
            Saxon.Hej.@event.Receiver target = sf.getReceiver(finalResult, newParams, getPipelineConfiguration());
            committed = true;
            target.open();
            if ((pending != null)) {
                pending.replay(target);
                pending = null;
            }
            setUnderlyingReceiver(target);
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            if ((item is Saxon.Hej.om.NodeInfo)) {
                ((Saxon.Hej.om.NodeInfo)(item)).copy(this, Saxon.Hej.om.CopyOptions.ALL_NAMESPACES, locationId);
            } else {
                if (!(committed)) {
                    switchToMethod("xml");
                }
                getNextReceiver().append(item);
            }
        }
    
    }
}