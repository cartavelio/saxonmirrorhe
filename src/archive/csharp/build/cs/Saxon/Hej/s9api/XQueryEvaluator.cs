using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class XQueryEvaluator : Saxon.Hej.s9api.AbstractDestination, System.Collections.Generic.IEnumerable<Saxon.Api.XdmItem> {
        private readonly Saxon.Hej.s9api.Processor processor;
        private readonly Saxon.Hej.query.XQueryExpression expression;
        private readonly Saxon.Hej.query.DynamicQueryContext context;
        private Saxon.Hej.Controller controller;
        private Saxon.Hej.s9api.Destination destination;
        private System.Collections.Generic.ISet<Saxon.Api.XdmNode> updatedDocuments;
        private Saxon.Hej.@event.Builder sourceTreeBuilder;
        protected internal XQueryEvaluator(Saxon.Hej.s9api.Processor processor, Saxon.Hej.query.XQueryExpression expression)  {
            this.processor = processor;
            this.expression = expression;
            this.context = new Saxon.Hej.query.DynamicQueryContext(expression.getConfiguration());
        }
        public void setSchemaValidationMode(Saxon.Hej.s9api.ValidationMode mode) {
            if ((mode != null)) {
                context.setSchemaValidationMode(mode.getNumber());
            }
        }
        public Saxon.Hej.s9api.ValidationMode getSchemaValidationMode() {
            return Saxon.Hej.s9api.ValidationMode.get(context.getSchemaValidationMode());
        }
        public void setSource(Saxon.Ejavax.xml.transform.Source source) {
            if ((source is Saxon.Hej.om.NodeInfo)) {
                setContextItem(new Saxon.Api.XdmNode((Saxon.Hej.om.NodeInfo)(source)));
            } else {
                setContextItem(processor.newDocumentBuilder().build(source));
            }
        }
        public void setContextItem(Saxon.Api.XdmItem item) {
            if ((item != null)) {
                Saxon.Hej.expr.instruct.GlobalContextRequirement gcr = expression.getExecutable().getGlobalContextRequirement();
                if (((gcr != null) && !(gcr.isExternal()))) {
                    throw new Saxon.Api.SaxonApiException("The context item for the query is not defined as external");
                }
                context.setContextItem(((Saxon.Hej.om.Item)item.Unwrap()));
            }
        }
        public Saxon.Api.XdmItem getContextItem() {
            Saxon.Hej.om.Item item = context.getContextItem();
            if ((item == null)) {
                return null;
            }
            return (Saxon.Api.XdmItem)(Saxon.Api.XdmValue.Wrap(item));
        }
        public void setExternalVariable(Saxon.Api.QName name, Saxon.Api.XdmValue value) {
            try {
                context.setParameter(name.GetStructuredQName(), ((value == null) ? null : ((Saxon.Hej.om.Sequence)(((Saxon.Hej.om.GroundedValue)value.Unwrap()))).materialize()));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Api.XdmValue getExternalVariable(Saxon.Api.QName name) {
            Saxon.Hej.om.GroundedValue oval = context.getParameter(name.GetStructuredQName());
            if ((oval == null)) {
                return null;
            }
            return Saxon.Api.XdmValue.Wrap(oval);
        }
        public void setURIResolver(Saxon.Ejavax.xml.transform.URIResolver resolver) {
            context.setURIResolver(resolver);
        }
        public Saxon.Ejavax.xml.transform.URIResolver getURIResolver() {
            return context.getURIResolver();
        }
        public void setUnparsedTextResolver(Saxon.Hej.lib.UnparsedTextURIResolver resolver) {
            context.setUnparsedTextURIResolver(resolver);
        }
        public Saxon.Hej.lib.UnparsedTextURIResolver getUnparsedTextURIResolver() {
            return context.getUnparsedTextURIResolver();
        }
        public void setTraceListener(Saxon.Hej.lib.TraceListener listener) {
            context.setTraceListener(listener);
        }
        public Saxon.Hej.lib.TraceListener getTraceListener() {
            return context.getTraceListener();
        }
        public void setTraceFunctionDestination(Saxon.Hej.lib.Logger stream) {
            context.setTraceFunctionDestination(stream);
        }
        public Saxon.Hej.lib.Logger getTraceFunctionDestination() {
            return context.getTraceFunctionDestination();
        }
        public void setDestination(Saxon.Hej.s9api.Destination destination) {
            this.destination = destination;
        }
        public void run() {
            try {
                if (expression.isUpdateQuery()) {
                    System.Collections.Generic.ISet<Saxon.Hej.om.MutableNodeInfo> docs = expression.runUpdate(context);
                    updatedDocuments = new System.Collections.Generic.HashSet<Saxon.Api.XdmNode>();
                    foreach (Saxon.Hej.om.MutableNodeInfo doc in docs) {
                        updatedDocuments.Add(Saxon.Api.XdmItem.WrapItem(doc));
                    }
                } else {
                    if ((destination == null)) {
                        throw new Saxon.Ejava.lang.IllegalStateException("No destination supplied");
                    }
                    run(destination);
                }
            } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public void run(Saxon.Hej.s9api.Destination destination) {
            if (expression.isUpdateQuery()) {
                throw new Saxon.Ejava.lang.IllegalStateException("Query is updating");
            }
            try {
                Saxon.Hej.@event.Receiver @out = getDestinationReceiver(destination);
                expression.run(context, @out, null);
                destination.closeAndNotify();
            } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public void runStreamed(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.s9api.Destination destination) {
            if (expression.isUpdateQuery()) {
                throw new Saxon.Ejava.lang.IllegalStateException("Query is updating; cannot run with streaming");
            }
            Saxon.Hej.Configuration config = context.getConfiguration();
            if (config.isTiming()) {
                string systemId = source.getSystemId();
                if ((systemId == null)) {
                    systemId = "";
                }
                config.getLogger().info(("Processing streamed input "+systemId));
            }
            try {
                Saxon.Hej.serialize.SerializationProperties @params = expression.getExecutable().getPrimarySerializationProperties();
                Saxon.Hej.@event.Receiver receiver = destination.getReceiver(config.makePipelineConfiguration(), @params);
                expression.runStreamed(context, source, receiver, null);
            } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Api.XdmValue evaluate() {
            if (expression.isUpdateQuery()) {
                throw new Saxon.Ejava.lang.IllegalStateException("Query is updating");
            }
            try {
                Saxon.Hej.om.SequenceIterator iter = expression.iterator(context);
                return Saxon.Api.XdmValue.Wrap(Saxon.Hej.om.SequenceTool.toGroundedValue(iter));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw new Saxon.Api.SaxonApiException(e.getXPathException());
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Api.XdmItem evaluateSingle() {
            try {
                Saxon.Hej.om.SequenceIterator iter = expression.iterator(context);
                Saxon.Hej.om.Item next = iter.next();
                return ((next == null) ? null : (Saxon.Api.XdmItem)(Saxon.Api.XdmValue.Wrap(next)));
            } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                throw new Saxon.Api.SaxonApiException(e.getXPathException());
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public System.Collections.Generic.IEnumerator<Saxon.Api.XdmItem> GetEnumerator() {
            if (expression.isUpdateQuery()) {
                throw new Saxon.Ejava.lang.IllegalStateException("Query is updating");
            }
            try {
                return new Saxon.Hej.s9api.XdmSequenceIterator<Saxon.Api.XdmItem>(expression.iterator(context));
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        // Extra method to make the class IEnumerable as well as IEnumerable<T>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        private Saxon.Hej.@event.Receiver getDestinationReceiver(Saxon.Hej.s9api.Destination destination) {
            Saxon.Hej.expr.instruct.Executable exec = expression.getExecutable();
            Saxon.Hej.@event.PipelineConfiguration pipe = expression.getConfiguration().makePipelineConfiguration();
            Saxon.Hej.@event.Receiver @out = destination.getReceiver(pipe, exec.getPrimarySerializationProperties());
            if (Saxon.Hej.Configuration.isAssertionsEnabled()) {
                return new Saxon.Hej.@event.RegularSequenceChecker(@out, true);
            } else {
                return @out;
            }
        }
        public override Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.SerializationProperties @params) {
            if ((destination == null)) {
                throw new Saxon.Ejava.lang.IllegalStateException("No destination has been supplied");
            }
            try {
                if ((controller == null)) {
                    controller = expression.newController(context);
                } else {
                    context.initializeController(controller);
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
            sourceTreeBuilder = controller.makeBuilder();
            if ((sourceTreeBuilder is Saxon.Hej.tree.tiny.TinyBuilder)) {
                ((Saxon.Hej.tree.tiny.TinyBuilder)(sourceTreeBuilder)).setStatistics(context.getConfiguration().getTreeStatistics().SOURCE_DOCUMENT_STATISTICS);
            }
            Saxon.Hej.@event.Receiver @out = controller.makeStripper(sourceTreeBuilder);
            Saxon.Hej.@event.SequenceNormalizer sn = @params.makeSequenceNormalizer(@out);
            sn.onClose(() => {
                Saxon.Hej.om.NodeInfo doc = sourceTreeBuilder.getCurrentRoot();
                if ((doc == null)) {
                    throw new Saxon.Api.SaxonApiException("No source document has been built by the previous pipeline stage");
                }
                doc.getTreeInfo().setSpaceStrippingRule(controller.getSpaceStrippingRule());
                setSource(doc.asActiveSource());
                sourceTreeBuilder = null;
                run(destination);
                destination.closeAndNotify();
            } );
            return sn;
        }
        public override void close() {
        }
        public System.Collections.Generic.IEnumerator<Saxon.Api.XdmNode> getUpdatedDocuments() {
            return updatedDocuments.GetEnumerator();
        }
        public Saxon.Api.XdmValue callFunction(Saxon.Api.QName function, params Saxon.Api.XdmValue[]  arguments) {
            Saxon.Hej.expr.instruct.UserFunction fn = expression.getMainModule().getUserDefinedFunction(function.Uri, function.LocalName, arguments.Length);
            if ((fn == null)) {
                throw new Saxon.Api.SaxonApiException((((("No function with name "+function.EQName)+" and arity ")+arguments.Length)+" has been declared in the query"));
            }
            try {
                if ((controller == null)) {
                    controller = expression.newController(context);
                } else {
                    context.initializeController(controller);
                }
                Saxon.Hej.Configuration config = processor.getUnderlyingConfiguration();
                Saxon.Hej.type.TypeHierarchy th = config.getTypeHierarchy();
                Saxon.Hej.om.Sequence[] vr = Saxon.Hej.om.SequenceTool.makeSequenceArray(arguments.Length);
                for (int i = 0; (i<arguments.Length); i++) {
                    Saxon.Hej.value.SequenceType type = fn.getParameterDefinitions()[i].getRequiredType();
                    Saxon.Hej.om.GroundedValue gVal = ((Saxon.Hej.om.GroundedValue)arguments[i].Unwrap());
                    if (!(type.matches(gVal, th))) {
                        Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, function.GetStructuredQName().getDisplayName(), i);
                        gVal = th.applyFunctionConversionRules(gVal, type, role, Saxon.Hej.expr.parser.Loc.NONE);
                    }
                    vr[i] = gVal;
                }
                Saxon.Hej.om.Sequence result = fn.call(vr, controller);
                return Saxon.Api.XdmValue.Wrap(result);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Hej.query.DynamicQueryContext getUnderlyingQueryContext() {
            return context;
        }
    
    }
}