using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class XsltTransformer : Saxon.Hej.s9api.AbstractXsltTransformer, Saxon.Hej.s9api.Destination {
        private Saxon.Api.QName initialTemplateName;
        private Saxon.Hej.expr.instruct.GlobalParameterSet parameters;
        private Saxon.Ejavax.xml.transform.Source initialSource;
        private Saxon.Hej.s9api.Destination destination;
        private readonly Saxon.Hej.s9api.DestinationHelper destinationHelper;
        private System.Uri destinationBaseUri;
        protected internal XsltTransformer(Saxon.Hej.s9api.Processor processor, Saxon.Hej.trans.XsltController controller, Saxon.Hej.expr.instruct.GlobalParameterSet staticParameters) : base (processor, controller) {
            parameters = new Saxon.Hej.expr.instruct.GlobalParameterSet();
            destinationHelper = new Saxon.Hej.s9api.DestinationHelper(this);
        }
        public void setDestinationBaseURI(System.Uri baseURI) {
            this.destinationBaseUri = baseURI;
        }
        public System.Uri getDestinationBaseURI() {
            return destinationBaseUri;
        }
        public void onClose(Saxon.Hej.s9api.Action listener) {
            destinationHelper.onClose(listener);
        }
        public void closeAndNotify() {
            destinationHelper.closeAndNotify();
        }
        public void setInitialTemplate(Saxon.Api.QName templateName) {
            initialTemplateName = templateName;
        }
        public Saxon.Api.QName getInitialTemplate() {
            return initialTemplateName;
        }
        public void setSource(Saxon.Ejavax.xml.transform.Source source) { lock(this) {
            if ((source is Saxon.Hej.om.NodeInfo)) {
                setInitialContextNode(new Saxon.Api.XdmNode((Saxon.Hej.om.NodeInfo)(source)));
            } else {
                initialSource = source;
            }
        }}
        public void setInitialContextNode(Saxon.Api.XdmNode node) { lock(this) {
            try {
                if ((node == null)) {
                    initialSource = null;
                    controller.setGlobalContextItem(null);
                } else {
                    initialSource = ((Saxon.Hej.om.NodeInfo)node.Unwrap()).asActiveSource();
                    controller.setGlobalContextItem(((Saxon.Hej.om.NodeInfo)node.Unwrap()).getRoot());
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }}
        public Saxon.Api.XdmNode getInitialContextNode() {
            if ((initialSource is Saxon.Hej.om.NodeInfo)) {
                return (Saxon.Api.XdmNode)(Saxon.Api.XdmValue.Wrap((Saxon.Hej.om.NodeInfo)(initialSource)));
            } else {
                return null;
            }
        }
        public void setParameter(Saxon.Api.QName name, Saxon.Api.XdmValue value) { lock(this) {
            try {
                parameters.put(name.GetStructuredQName(), ((value == null) ? null : ((Saxon.Hej.om.Sequence)(((Saxon.Hej.om.GroundedValue)value.Unwrap()))).materialize()));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }}
        public void clearParameters() { lock(this) {
            parameters = new Saxon.Hej.expr.instruct.GlobalParameterSet();
        }}
        public Saxon.Api.XdmValue getParameter(Saxon.Api.QName name) { lock(this) {
            Saxon.Hej.om.Sequence oval = parameters.get(name.GetStructuredQName());
            return ((oval == null) ? null : Saxon.Api.XdmValue.Wrap(oval));
        }}
        public void setDestination(Saxon.Hej.s9api.Destination destination) {
            this.destination = destination;
        }
        public Saxon.Hej.s9api.Destination getDestination() {
            return destination;
        }
        public void transform() { lock(this) {
            Saxon.Ejavax.xml.transform.Source initialSelection = initialSource;
            bool reset = false;
            if ((destination == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException("No destination has been supplied");
            }
            try {
                Saxon.Hej.@event.Receiver @out = getDestinationReceiver(controller, destination);
                Saxon.Hej.expr.instruct.GlobalContextRequirement gcr = controller.getExecutable().getGlobalContextRequirement();
                if (((((gcr == null) || !(gcr.isAbsentFocus()))) && (initialSelection != null))) {
                    if ((initialSelection is Saxon.Hej.om.NodeInfo)) {
                        reset = maybeSetGlobalContextItem((Saxon.Hej.om.NodeInfo)(initialSelection));
                    } else {
                        Saxon.Hej.om.NodeInfo node = controller.makeSourceTree(initialSelection, getSchemaValidationMode().getNumber());
                        reset = maybeSetGlobalContextItem(node);
                        initialSelection = node.asActiveSource();
                    }
                }
                if (baseOutputUriWasSet) {
                    @out.setSystemId(getBaseOutputURI());
                }
                controller.initializeController(parameters);
                if ((initialTemplateName != null)) {
                    controller.callTemplate(initialTemplateName.GetStructuredQName(), @out);
                } else {
                    if ((initialSelection != null)) {
                        applyTemplatesToSource(initialSelection, @out);
                    } else {
                        Saxon.Api.QName entryPoint = new Saxon.Api.QName("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "initial-template");
                        controller.callTemplate(entryPoint.GetStructuredQName(), @out);
                    }
                }
                destination.closeAndNotify();
            } catch (Saxon.Hej.trans.XPathException e) {
                if (!(e.hasBeenReported())) {
                    getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(e));
                    e.setHasBeenReported(true);
                }
                throw new Saxon.Api.SaxonApiException(e);
            } finally {
                if (reset) {
                    controller.clearGlobalContextItem();
                }
            }
        }}
        private bool maybeSetGlobalContextItem(Saxon.Hej.om.Item item) {
            if ((controller.getGlobalContextItem() == null)) {
                controller.setGlobalContextItem(item, true);
                return true;
            } else {
                return false;
            }
        }
        public Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.SerializationProperties @params) {
            if ((destination == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException("No destination has been supplied");
            }
            Saxon.Hej.@event.Receiver rt = getReceivingTransformer(controller, parameters, destination);
            rt = new Saxon.Hej.@event.SequenceNormalizerWithSpaceSeparator(rt);
            rt.setPipelineConfiguration(pipe);
            return rt;
        }
        public void close() {
        }
    
    }
}