using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    abstract internal class AbstractXsltTransformer {
        protected internal Saxon.Hej.s9api.Processor processor;
        protected internal Saxon.Hej.trans.XsltController controller;
        protected internal bool baseOutputUriWasSet = false;
        internal AbstractXsltTransformer(Saxon.Hej.s9api.Processor processor, Saxon.Hej.trans.XsltController controller)  {
            this.processor = processor;
            this.controller = controller;
        }
        public void setBaseOutputURI(string uri) { lock(this) {
            controller.setBaseOutputURI(uri);
            baseOutputUriWasSet = (uri != null);
        }}
        public string getBaseOutputURI() {
            return controller.getBaseOutputURI();
        }
        public void setResourceResolver(Saxon.Hej.lib.JJ_ResourceResolver resolver) {
            controller.setResourceResolver(resolver);
        }
        public Saxon.Hej.lib.JJ_ResourceResolver getResourceResolver() {
            return controller.getResourceResolver();
        }
        public void setUnparsedTextResolver(Saxon.Hej.lib.UnparsedTextURIResolver resolver) {
            controller.setUnparsedTextURIResolver(resolver);
        }
        public Saxon.Hej.lib.UnparsedTextURIResolver getUnparsedTextURIResolver() {
            return controller.getUnparsedTextURIResolver();
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            controller.setErrorReporter(reporter);
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            return controller.getErrorReporter();
        }
        public void setResultDocumentHandler(System.Func<System.Uri,Saxon.Hej.s9api.Destination> handler) {
            controller.setResultDocumentResolver(new Inner_ResultDocumentResolver_5(handler));
        }
        public void setAssertionsEnabled(bool enabled) {
            controller.setAssertionsEnabled(enabled);
        }
        public bool isAssertionsEnabled() {
            return controller.isAssertionsEnabled();
        }
        public void setTraceListener(Saxon.Hej.lib.TraceListener listener) {
            controller.setTraceListener(listener);
        }
        public Saxon.Hej.lib.TraceListener getTraceListener() {
            return controller.getTraceListener();
        }
        public void setTraceFunctionDestination(Saxon.Hej.lib.Logger stream) {
            controller.setTraceFunctionDestination(stream);
        }
        public Saxon.Hej.lib.Logger getTraceFunctionDestination() {
            return controller.getTraceFunctionDestination();
        }
        protected internal void applyTemplatesToSource(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.@event.Receiver @out) {
            Saxon.Ejava.util.Objects.requireNonNull(source);
            Saxon.Ejava.util.Objects.requireNonNull(@out);
            if (controller.getInitialMode().isDeclaredStreamable()) {
                controller.applyStreamingTemplates(source, @out);
            } else {
                Saxon.Hej.om.NodeInfo node;
                if ((source is Saxon.Hej.om.NodeInfo)) {
                    node = (Saxon.Hej.om.NodeInfo)(source);
                } else {
                    node = controller.makeSourceTree(source, controller.getSchemaValidationMode());
                }
                controller.applyTemplates(node, @out);
            }
        }
        protected internal bool isStreamableSource(Saxon.Ejavax.xml.transform.Source source) {
            if ((source is Saxon.Hej.lib.AugmentedSource)) {
                return isStreamableSource(((Saxon.Hej.lib.AugmentedSource)(source)).getContainedSource());
            }
            Saxon.Hej.Configuration config = controller.getConfiguration();
            try {
                source = config.getSourceResolver().resolveSource(source, config);
            } catch (Saxon.Hej.trans.XPathException e) {
                return false;
            }
            return (((source is Saxon.Ejavax.xml.transform.sax.SAXSource) || (source is Saxon.Ejavax.xml.transform.stream.StreamSource)) || (source is Saxon.Hej.@event.EventSource));
        }
        public void setSchemaValidationMode(Saxon.Hej.s9api.ValidationMode mode) {
            if ((mode != null)) {
                controller.setSchemaValidationMode(mode.getNumber());
            }
        }
        public Saxon.Hej.s9api.ValidationMode getSchemaValidationMode() {
            return Saxon.Hej.s9api.ValidationMode.get(controller.getSchemaValidationMode());
        }
        public void setInitialMode(Saxon.Api.QName modeName) {
            try {
                controller.setInitialMode(((modeName == null) ? null : modeName.GetStructuredQName()));
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Api.QName getInitialMode() {
            Saxon.Hej.om.StructuredQName mode = controller.getInitialModeName();
            if ((mode == null)) {
                return null;
            } else {
                return new Saxon.Api.QName(mode);
            }
        }
        public Saxon.Hej.trans.XsltController getUnderlyingController() {
            return controller;
        }
        public Saxon.Hej.@event.Receiver getDestinationReceiver(Saxon.Hej.trans.XsltController controller, Saxon.Hej.s9api.Destination destination) {
            Saxon.Hej.@event.Receiver receiver;
            controller.setPrincipalDestination(destination);
            Saxon.Hej.@event.PipelineConfiguration pipe = controller.makePipelineConfiguration();
            Saxon.Hej.serialize.SerializationProperties @params = controller.getExecutable().getPrimarySerializationProperties();
            receiver = destination.getReceiver(pipe, @params);
            if (Saxon.Hej.Configuration.isAssertionsEnabled()) {
                receiver = new Saxon.Hej.@event.RegularSequenceChecker(receiver, true);
            }
            receiver.getPipelineConfiguration().setController(controller);
            if (baseOutputUriWasSet) {
                try {
                    if ((destination.getDestinationBaseURI() == null)) {
                        destination.setDestinationBaseURI(new System.Uri(controller.getBaseOutputURI(), System.UriKind.RelativeOrAbsolute));
                    }
                } catch (System.UriFormatException e) {
                }
            } else {
                if ((destination.getDestinationBaseURI() != null)) {
                    controller.setBaseOutputURI(destination.getDestinationBaseURI().OriginalString);
                }
            }
            receiver.setSystemId(controller.getBaseOutputURI());
            return receiver;
        }
        protected internal Saxon.Hej.@event.Receiver getReceivingTransformer(Saxon.Hej.trans.XsltController controller, Saxon.Hej.expr.instruct.GlobalParameterSet parameters, Saxon.Hej.s9api.Destination finalDestination) {
            Saxon.Hej.Configuration config = controller.getConfiguration();
            if (controller.getInitialMode().isDeclaredStreamable()) {
                Saxon.Hej.@event.Receiver sOut = getDestinationReceiver(controller, finalDestination);
                try {
                    controller.initializeController(parameters);
                    return controller.getStreamingReceiver(controller.getInitialMode(), sOut);
                } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            } else {
                Saxon.Hej.@event.Builder sourceTreeBuilder = controller.makeBuilder();
                sourceTreeBuilder.setDurability(Saxon.Hej.om.Durability.LASTING);
                if ((sourceTreeBuilder is Saxon.Hej.tree.tiny.TinyBuilder)) {
                    ((Saxon.Hej.tree.tiny.TinyBuilder)(sourceTreeBuilder)).setStatistics(config.getTreeStatistics().SOURCE_DOCUMENT_STATISTICS);
                }
                Saxon.Hej.@event.Receiver stripper = controller.makeStripper(sourceTreeBuilder);
                if (controller.isStylesheetStrippingTypeAnnotations()) {
                    stripper = controller.getConfiguration().getAnnotationStripper(stripper);
                }
                return makeTreeReceiver(controller, parameters, finalDestination, sourceTreeBuilder, stripper);
            }
        }
        private Saxon.Hej.@event.TreeReceiver makeTreeReceiver(Saxon.Hej.trans.XsltController controller, Saxon.Hej.expr.instruct.GlobalParameterSet parameters, Saxon.Hej.s9api.Destination finalDestination, Saxon.Hej.@event.Builder sourceTreeBuilder, Saxon.Hej.@event.Receiver stripper) {
            return new Inner_TreeReceiver_80(this, stripper, sourceTreeBuilder, controller, finalDestination, parameters);
        }
    
        private class Inner_ResultDocumentResolver_5 : Saxon.Hej.lib.ResultDocumentResolver {
            public Inner_ResultDocumentResolver_5(System.Func<System.Uri,Saxon.Hej.s9api.Destination> handler) {
                this.handler = handler;
            }
            private System.Func<System.Uri,Saxon.Hej.s9api.Destination> handler;
            public Saxon.Hej.@event.Receiver resolve(Saxon.Hej.expr.XPathContext context, string href, string baseUri, Saxon.Hej.serialize.SerializationProperties properties) {
                try {
                    System.Uri abs = Saxon.Hej.functions.ResolveURI.makeAbsolute(/*localparam*/href, /*localparam*/baseUri);
                    Saxon.Hej.s9api.Destination destination;
                    try {
                        /*localvar*/destination = /*extraParam*/handler(/*localvar*/abs);
                    } catch (Saxon.Api.SaxonApiException e) {
                        Saxon.Hej.trans.XPathException xe = Saxon.Hej.trans.XPathException.makeXPathException(/*exception*/e);
                        /*localvar*/xe.maybeSetErrorCode("SXRD0001");
                        throw /*localvar*/xe;
                    }
                    try {
                        Saxon.Hej.@event.PipelineConfiguration pipe = /*localparam*/context.getController().makePipelineConfiguration();
                        return /*localvar*/destination.getReceiver(/*localvar*/pipe, /*localparam*/properties);
                    } catch (Saxon.Api.SaxonApiException e) {
                        throw Saxon.Hej.trans.XPathException.makeXPathException(/*exception*/e);
                    }
                } catch (System.UriFormatException e) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(/*exception*/e);
                }
            }
        }

        private class Inner_TreeReceiver_80 : Saxon.Hej.@event.TreeReceiver {
            private readonly AbstractXsltTransformer outerInstance;
            public Inner_TreeReceiver_80(AbstractXsltTransformer outerInstance, /*AA*/Saxon.Hej.@event.Receiver arg1, Saxon.Hej.@event.Builder sourceTreeBuilder, Saxon.Hej.trans.XsltController controller, Saxon.Hej.s9api.Destination finalDestination, Saxon.Hej.expr.instruct.GlobalParameterSet parameters) : base(arg1) {
                this.outerInstance = outerInstance;
                this.sourceTreeBuilder = sourceTreeBuilder;
                this.controller = controller;
                this.finalDestination = finalDestination;
                this.parameters = parameters;
                closed = false;
            }
            private Saxon.Hej.@event.Builder sourceTreeBuilder;
            private Saxon.Hej.trans.XsltController controller;
            private Saxon.Hej.s9api.Destination finalDestination;
            private Saxon.Hej.expr.instruct.GlobalParameterSet parameters;
            internal bool closed;
            public override void close() {
                if (!(/*field*/closed)) {
                    try {
                        Saxon.Hej.om.NodeInfo doc = /*extraParam*/sourceTreeBuilder.getCurrentRoot();
                        if ((/*localvar*/doc != null)) {
                            /*localvar*/doc.getTreeInfo().setSpaceStrippingRule(/*extraParam*/controller.getSpaceStrippingRule());
                            Saxon.Hej.@event.Receiver result = outerInstance.getDestinationReceiver(/*extraParam*/controller, /*extraParam*/finalDestination);
                            try {
                                /*extraParam*/controller.setGlobalContextItem(/*localvar*/doc);
                                /*extraParam*/controller.initializeController(/*extraParam*/parameters);
                                /*extraParam*/controller.applyTemplates(/*localvar*/doc, /*localvar*/result);
                            } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                                throw new Saxon.Api.SaxonApiException(/*exception*/e);
                            }
                        }
                    } catch (Saxon.Api.SaxonApiException e) {
                        throw Saxon.Hej.trans.XPathException.makeXPathException(/*exception*/e);
                    }
                    /*field*/closed = true;
                }
            }
        }

    }
}