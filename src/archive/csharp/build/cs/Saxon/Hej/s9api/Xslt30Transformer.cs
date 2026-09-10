using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class Xslt30Transformer : Saxon.Hej.s9api.AbstractXsltTransformer {
        private Saxon.Hej.expr.instruct.GlobalParameterSet globalParameterSet;
        private bool primed = false;
        private Saxon.Hej.om.Item globalContextItem = null;
        private bool alreadyStripped;
        protected internal Xslt30Transformer(Saxon.Hej.s9api.Processor processor, Saxon.Hej.trans.XsltController controller, Saxon.Hej.expr.instruct.GlobalParameterSet staticParameters) : base (processor, controller) {
            globalParameterSet = new Saxon.Hej.expr.instruct.GlobalParameterSet();
        }
        public void setGlobalContextItem(Saxon.Api.XdmItem globalContextItem) {
            setGlobalContextItem(globalContextItem, false);
        }
        public void setGlobalContextItem(Saxon.Api.XdmItem globalContextItem, bool alreadyStripped) { lock(this) {
            if (primed) {
                throw new Saxon.Ejava.lang.IllegalStateException("Stylesheet has already been evaluated");
            }
            this.globalContextItem = ((globalContextItem == null) ? null : ((Saxon.Hej.om.Item)globalContextItem.Unwrap()));
            this.alreadyStripped = alreadyStripped;
        }}
        public Saxon.Api.XdmItem getGlobalContextItem() {
            return Saxon.Api.XdmItem.WrapItem(this.globalContextItem);
        }
        public void setStylesheetParameters<T> (System.Collections.Generic.IDictionary<Saxon.Api.QName,T> parameters) where T : Saxon.Api.XdmValue { lock(this) {
            if (primed) {
                throw new Saxon.Ejava.lang.IllegalStateException("Stylesheet has already been evaluated");
            }
            if ((globalParameterSet == null)) {
                globalParameterSet = new Saxon.Hej.expr.instruct.GlobalParameterSet();
            }
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Api.QName,T> param in parameters) {
                Saxon.Hej.om.StructuredQName name = param.Key.GetStructuredQName();
                Saxon.Api.XdmValue value = param.Value;
                try {
                    globalParameterSet.put(name, ((Saxon.Hej.om.GroundedValue)value.Unwrap()));
                } catch (Saxon.Hej.trans.UncheckedXPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }
        }}
        private void prime() {
            if (!(primed)) {
                if ((globalParameterSet == null)) {
                    globalParameterSet = new Saxon.Hej.expr.instruct.GlobalParameterSet();
                }
                try {
                    controller.setGlobalContextItem(globalContextItem, alreadyStripped);
                    controller.initializeController(globalParameterSet);
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(e);
                }
            }
            primed = true;
        }
        public void setInitialTemplateParameters<T> (System.Collections.Generic.IDictionary<Saxon.Api.QName,T> parameters, bool tunnel) where T : Saxon.Api.XdmValue { lock(this) {
            System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.Sequence> templateParams = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.om.Sequence>();
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Api.QName,T> entry in parameters) {
                Saxon.Api.QName key = entry.Key;
                Saxon.Api.XdmValue value = entry.Value;
                Saxon.Impl.Helpers.MapUtils.Put(templateParams, key.GetStructuredQName(), ((Saxon.Hej.om.GroundedValue)value.Unwrap()));
            }
            controller.setInitialTemplateParameters(templateParams, tunnel);
        }}
        public void applyTemplates(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.s9api.Destination destination) { lock(this) {
            Saxon.Ejava.util.Objects.requireNonNull(destination);
            if ((source == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("No initial match selection supplied", "XTDE0044");
                throw new Saxon.Api.SaxonApiException(err);
            }
            prime();
            try {
                Saxon.Hej.@event.Receiver sOut = getDestinationReceiver(controller, destination);
                applyTemplatesToSource(source, sOut);
                destination.closeAndNotify();
            } catch (Saxon.Hej.trans.XPathException e) {
                if (!(e.hasBeenReported())) {
                    getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(e));
                }
                throw new Saxon.Api.SaxonApiException(e);
            }
        }}
        public Saxon.Api.XdmValue applyTemplates(Saxon.Ejavax.xml.transform.Source source) { lock(this) {
            Saxon.Ejava.util.Objects.requireNonNull(source);
            Saxon.Hej.s9api.RawDestination raw = new Saxon.Hej.s9api.RawDestination();
            applyTemplates(source, raw);
            return raw.getXdmValue();
        }}
        public void transform(Saxon.Ejavax.xml.transform.Source source, Saxon.Hej.s9api.Destination destination) { lock(this) {
            Saxon.Ejava.util.Objects.requireNonNull(destination);
            if ((source == null)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("No initial match selection supplied", "XTDE0044");
                throw new Saxon.Api.SaxonApiException(err);
            }
            if (controller.getInitialMode().isDeclaredStreamable()) {
                throw new Saxon.Api.SaxonApiException("Cannot use the transform() method when the initial mode is streamable");
            }
            prime();
            try {
                Saxon.Hej.om.NodeInfo sourceNode;
                if ((source is Saxon.Hej.om.NodeInfo)) {
                    controller.setGlobalContextItem((Saxon.Hej.om.NodeInfo)(source));
                    sourceNode = (Saxon.Hej.om.NodeInfo)(source);
                } else {
                    sourceNode = controller.makeSourceTree(source, getSchemaValidationMode().getNumber());
                    controller.setGlobalContextItem(sourceNode);
                }
                Saxon.Hej.@event.Receiver sOut = getDestinationReceiver(controller, destination);
                controller.applyTemplates(sourceNode, sOut);
                destination.closeAndNotify();
            } catch (Saxon.Hej.trans.XPathException e) {
                if (!(e.hasBeenReported())) {
                    getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(e));
                }
                throw new Saxon.Api.SaxonApiException(e);
            }
        }}
        public void applyTemplates(Saxon.Api.XdmValue selection, Saxon.Hej.s9api.Destination destination) { lock(this) {
            Saxon.Ejava.util.Objects.requireNonNull(selection);
            Saxon.Ejava.util.Objects.requireNonNull(destination);
            prime();
            try {
                Saxon.Hej.@event.Receiver sOut = getDestinationReceiver(controller, destination);
                if (baseOutputUriWasSet) {
                    sOut.setSystemId(getBaseOutputURI());
                }
                controller.applyTemplates(((Saxon.Hej.om.GroundedValue)selection.Unwrap()), sOut);
                destination.closeAndNotify();
            } catch (Saxon.Hej.trans.XPathException e) {
                if (!(e.hasBeenReported())) {
                    getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(e));
                }
                throw new Saxon.Api.SaxonApiException(e);
            }
        }}
        public Saxon.Api.XdmValue applyTemplates(Saxon.Api.XdmValue selection) { lock(this) {
            Saxon.Ejava.util.Objects.requireNonNull(selection);
            Saxon.Hej.s9api.RawDestination raw = new Saxon.Hej.s9api.RawDestination();
            applyTemplates(selection, raw);
            return raw.getXdmValue();
        }}
        public void callTemplate(Saxon.Api.QName templateName, Saxon.Hej.s9api.Destination destination) { lock(this) {
            Saxon.Ejava.util.Objects.requireNonNull(destination);
            prime();
            if ((templateName == null)) {
                templateName = new Saxon.Api.QName("xsl", Saxon.Hej.lib.NamespaceConstant.XSLT, "initial-template");
            }
            try {
                Saxon.Hej.@event.Receiver sOut = getDestinationReceiver(controller, destination);
                if (baseOutputUriWasSet) {
                    sOut.setSystemId(getBaseOutputURI());
                }
                controller.callTemplate(templateName.GetStructuredQName(), sOut);
                destination.closeAndNotify();
            } catch (Saxon.Hej.trans.XPathException e) {
                destination.closeAndNotify();
                if (!(e.hasBeenReported())) {
                    getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(e));
                }
                throw new Saxon.Api.SaxonApiException(e);
            }
        }}
        public Saxon.Api.XdmValue callTemplate(Saxon.Api.QName templateName) { lock(this) {
            Saxon.Hej.s9api.RawDestination dest = new Saxon.Hej.s9api.RawDestination();
            callTemplate(templateName, dest);
            return dest.getXdmValue();
        }}
        public Saxon.Api.XdmValue callFunction(Saxon.Api.QName function, Saxon.Api.XdmValue[] arguments) { lock(this) {
            Saxon.Ejava.util.Objects.requireNonNull(function);
            Saxon.Ejava.util.Objects.requireNonNull(arguments);
            prime();
            try {
                Saxon.Hej.expr.Component f = getFunctionComponent(function, arguments);
                Saxon.Hej.expr.instruct.UserFunction uf = (Saxon.Hej.expr.instruct.UserFunction)(f.getActor());
                Saxon.Hej.om.Sequence[] vr = typeCheckFunctionArguments(uf, arguments);
                Saxon.Hej.expr.XPathContextMajor context = controller.newXPathContext();
                context.setCurrentComponent(f);
                context.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_FUNCTION);
                context.setCurrentOutputUri(null);
                Saxon.Hej.om.Sequence result = uf.call(context, vr);
                result = result.materialize();
                return Saxon.Api.XdmValue.Wrap(result);
            } catch (Saxon.Hej.trans.XPathException e) {
                if (!(e.hasBeenReported())) {
                    getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(e));
                }
                throw new Saxon.Api.SaxonApiException(e);
            }
        }}
        private Saxon.Hej.expr.Component getFunctionComponent(Saxon.Api.QName function, Saxon.Api.XdmValue[] arguments) { lock(this) {
            Saxon.Hej.trans.SymbolicName fName = new Saxon.Hej.trans.SymbolicName.F(function.GetStructuredQName(), arguments.Length);
            Saxon.Hej.PreparedStylesheet pss = (Saxon.Hej.PreparedStylesheet)(controller.getExecutable());
            Saxon.Hej.expr.Component f = pss.getComponent(fName);
            if ((f == null)) {
                throw new Saxon.Hej.trans.XPathException((((("No public function with name "+function.ClarkName)+" and arity ")+arguments.Length)+" has been declared in the stylesheet"), "XTDE0041");
            } else {
                if (((f.getVisibility() != Saxon.Hej.trans.Visibility.FINAL) && (f.getVisibility() != Saxon.Hej.trans.Visibility.PUBLIC))) {
                    throw new Saxon.Hej.trans.XPathException((("Cannot invoke "+fName)+" externally, because it is not public"), "XTDE0041");
                }
            }
            return f;
        }}
        private Saxon.Hej.om.Sequence[] typeCheckFunctionArguments(Saxon.Hej.expr.instruct.UserFunction uf, Saxon.Api.XdmValue[] arguments) {
            Saxon.Hej.Configuration config = processor.getUnderlyingConfiguration();
            Saxon.Hej.expr.instruct.UserFunctionParameter[] @params = uf.getParameterDefinitions();
            Saxon.Hej.om.GroundedValue[] vr = new Saxon.Hej.om.GroundedValue[arguments.Length];
            for (int i = 0; (i<arguments.Length); i++) {
                Saxon.Hej.value.SequenceType type = @params[i].getRequiredType();
                vr[i] = ((Saxon.Hej.om.GroundedValue)arguments[i].Unwrap());
                if (!(type.matches(vr[i], config.getTypeHierarchy()))) {
                    Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.FUNCTION, uf.getFunctionName().getDisplayName(), i);
                    Saxon.Hej.om.Sequence converted = config.getTypeHierarchy().applyFunctionConversionRules(vr[i], type, role, Saxon.Hej.expr.parser.Loc.NONE);
                    vr[i] = converted.materialize();
                }
            }
            return vr;
        }
        public void callFunction(Saxon.Api.QName function, Saxon.Api.XdmValue[] arguments, Saxon.Hej.s9api.Destination destination) { lock(this) {
            prime();
            try {
                Saxon.Hej.expr.Component f = getFunctionComponent(function, arguments);
                Saxon.Hej.expr.instruct.UserFunction uf = (Saxon.Hej.expr.instruct.UserFunction)(f.getActor());
                Saxon.Hej.om.Sequence[] vr = typeCheckFunctionArguments(uf, arguments);
                Saxon.Hej.expr.XPathContextMajor context = controller.newXPathContext();
                context.setCurrentComponent(f);
                context.setTemporaryOutputState(Saxon.Hej.om.StandardNames.XSL_FUNCTION);
                context.setCurrentOutputUri(null);
                Saxon.Hej.serialize.SerializationProperties @params = controller.getExecutable().getPrimarySerializationProperties();
                Saxon.Hej.@event.Receiver receiver = destination.getReceiver(controller.makePipelineConfiguration(), @params);
                receiver.open();
                uf.process(context, vr, new Saxon.Hej.@event.ComplexContentOutputter(receiver));
                receiver.close();
            } catch (Saxon.Hej.trans.XPathException e) {
                getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(e));
                throw new Saxon.Api.SaxonApiException(e);
            }
            destination.closeAndNotify();
        }}
        public Saxon.Hej.s9api.Destination asDocumentDestination(Saxon.Hej.s9api.Destination finalDestination) {
            return new Inner_AbstractDestination_137(this, finalDestination);
        }
        public Saxon.Hej.s9api.Serializer newSerializer() {
            Saxon.Hej.s9api.Serializer serializer = processor.newSerializer();
            serializer.setOutputProperties(controller.getExecutable().getPrimarySerializationProperties());
            return serializer;
        }
        public Saxon.Hej.s9api.Serializer newSerializer(Saxon.Ejava.io.File file) {
            Saxon.Hej.s9api.Serializer serializer = processor.newSerializer(file);
            serializer.setOutputProperties(controller.getExecutable().getPrimarySerializationProperties());
            setBaseOutputURI(file.toURI().ToString());
            return serializer;
        }
        public Saxon.Hej.s9api.Serializer newSerializer(System.IO.TextWriter writer) {
            Saxon.Hej.s9api.Serializer serializer = newSerializer();
            serializer.setOutputWriter(writer);
            return serializer;
        }
        public Saxon.Hej.s9api.Serializer newSerializer(System.IO.Stream stream) {
            Saxon.Hej.s9api.Serializer serializer = newSerializer();
            serializer.setOutputStream(stream);
            return serializer;
        }
    
        private class Inner_AbstractDestination_137 : Saxon.Hej.s9api.AbstractDestination {
            private readonly Xslt30Transformer outerInstance;
            public Inner_AbstractDestination_137(Xslt30Transformer outerInstance, Saxon.Hej.s9api.Destination finalDestination) {
                this.outerInstance = outerInstance;
                this.finalDestination = finalDestination;
            }
            private Saxon.Hej.s9api.Destination finalDestination;
            private Saxon.Hej.@event.Receiver receiver;
            public override  /* REPLACING public */ Saxon.Hej.@event.Receiver getReceiver(Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.serialize.SerializationProperties @params) {
                Saxon.Hej.@event.Receiver rt = /*guess*/outerInstance.getReceivingTransformer(/*guess*/outerInstance.controller, outerInstance.globalParameterSet, /*extraParam*/finalDestination);
                /*localvar*/rt = new Saxon.Hej.@event.SequenceNormalizerWithSpaceSeparator(/*localvar*/rt);
                /*localvar*/rt.setPipelineConfiguration(/*localparam*/pipe);
                return /*field*/receiver = /*localvar*/rt;
            }
            public override  /* REPLACING public */ void close() {
                try {
                    /*field*/receiver.close();
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Api.SaxonApiException(/*exception*/e);
                }
            }
        }

    }
}