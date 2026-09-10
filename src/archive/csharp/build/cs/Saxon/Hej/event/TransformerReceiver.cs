using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.@event {
    public class TransformerReceiver : Saxon.Hej.@event.ProxyReceiver {
        private readonly Saxon.Hej.trans.XsltController controller;
        private Saxon.Hej.@event.Builder builder;
        private Saxon.Hej.@event.Receiver destination;
        public TransformerReceiver(Saxon.Hej.trans.XsltController controller) : base (controller.makeBuilder()) {
            this.controller = controller;
            this.builder = (Saxon.Hej.@event.Builder)(getNextReceiver());
            this.builder.setUseEventLocation(false);
        }
        public override void open() {
            builder.setSystemId(systemId);
            Saxon.Hej.@event.Receiver stripper = controller.makeStripper(builder);
            if (controller.isStylesheetStrippingTypeAnnotations()) {
                stripper = controller.getConfiguration().getAnnotationStripper(stripper);
            }
            setUnderlyingReceiver(stripper);
            nextReceiver.open();
        }
        public Saxon.Hej.Controller getController() {
            return controller;
        }
        public override void setSystemId(string systemId) {
            base.setSystemId(systemId);
            controller.setBaseOutputURI(systemId);
        }
        public void setDestination(Saxon.Hej.@event.Receiver destination) {
            this.destination = destination;
        }
        public Saxon.Hej.@event.Receiver getDestination() {
            return destination;
        }
        public override void close() {
            if ((builder == null)) {
            } else {
                nextReceiver.close();
                Saxon.Hej.om.NodeInfo doc = builder.getCurrentRoot();
                builder.reset();
                builder = null;
                if ((doc == null)) {
                    throw new Saxon.Hej.trans.XPathException("No source document has been built");
                }
                doc.getTreeInfo().setSpaceStrippingRule(controller.getSpaceStrippingRule());
                if ((destination == null)) {
                    throw new Saxon.Hej.trans.XPathException("No output destination has been supplied");
                }
                try {
                    controller.setGlobalContextItem(doc);
                    controller.applyTemplates(doc, destination);
                } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                }
            }
        }
    
    }
}