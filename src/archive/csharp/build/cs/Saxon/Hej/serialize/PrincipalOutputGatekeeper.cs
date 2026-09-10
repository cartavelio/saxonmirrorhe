using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.serialize {
    public class PrincipalOutputGatekeeper : Saxon.Hej.@event.ProxyReceiver {
        private readonly Saxon.Hej.trans.XsltController controller;
        private bool usedAsPrimaryResult = false;
        private bool usedAsSecondaryResult = false;
        private bool opened = false;
        private bool closed = false;
        public PrincipalOutputGatekeeper(Saxon.Hej.trans.XsltController controller, Saxon.Hej.@event.Receiver next) : base (next) {
            this.controller = controller;
        }
        public override void open() {
            if (closed) {
                string uri = (getSystemId().Equals(Saxon.Hej.trans.XsltController.ANONYMOUS_PRINCIPAL_OUTPUT_URI) ? "(no URI supplied)" : getSystemId());
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot write more than one result document to the principal output destination: "+uri));
                err.setErrorCode("XTDE1490");
                throw err;
            }
            base.open();
            opened = true;
        }
        public override void startDocument(int properties) { lock(this) {
            if (!(opened)) {
                open();
            }
            nextReceiver.startDocument(properties);
        }}
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            useAsPrimary();
            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) { lock(this) {
            useAsPrimary();
            nextReceiver.characters(chars, locationId, properties);
        }}
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
            useAsPrimary();
            nextReceiver.processingInstruction(target, data, locationId, properties);
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            useAsPrimary();
            nextReceiver.comment(chars, locationId, properties);
        }
        public override void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int copyNamespaces) {
            useAsPrimary();
            nextReceiver.append(item, locationId, copyNamespaces);
        }
        private void useAsPrimary() { lock(this) {
            if (closed) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot write to the principal output destination as it has already been closed: "+identifySystemId()));
                err.setErrorCode("XTDE1490");
                throw err;
            }
            if (usedAsSecondaryResult) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot write to the principal output destination as it has already been used by xsl:result-document: "+identifySystemId()));
                err.setErrorCode("XTDE1490");
                throw err;
            }
            usedAsPrimaryResult = true;
        }}
        public void useAsSecondary() { lock(this) {
            if (usedAsPrimaryResult) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot use xsl:result-document to write to a destination already used for the principal output: "+identifySystemId()));
                err.setErrorCode("XTDE1490");
                throw err;
            }
            if (usedAsSecondaryResult) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot write more than one xsl:result-document to the principal output destination: "+identifySystemId()));
                err.setErrorCode("XTDE1490");
                throw err;
            }
            usedAsSecondaryResult = true;
        }}
        public Saxon.Hej.@event.Receiver makeReceiver(Saxon.Hej.serialize.SerializationProperties @params) {
            try {
                Saxon.Hej.s9api.Destination dest = controller.getPrincipalDestination();
                if ((dest != null)) {
                    return dest.getReceiver(controller.makePipelineConfiguration(), @params);
                }
            } catch (Saxon.Api.SaxonApiException e) {
                return null;
            }
            return null;
        }
        private string identifySystemId() {
            string uri = getSystemId();
            return ((uri == null) ? "(no URI supplied)" : uri);
        }
        public override void close() {
            closed = true;
            if (usedAsPrimaryResult) {
                nextReceiver.close();
            }
        }
    
    }
}