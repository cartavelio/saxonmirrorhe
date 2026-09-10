using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.s9api {
    internal class XsltExecutable {
        internal Saxon.Hej.s9api.Processor processor;
        internal Saxon.Hej.PreparedStylesheet preparedStylesheet;
        protected internal XsltExecutable(Saxon.Hej.s9api.Processor processor, Saxon.Hej.PreparedStylesheet preparedStylesheet)  {
            this.processor = processor;
            this.preparedStylesheet = preparedStylesheet;
        }
        public Saxon.Hej.s9api.Processor getProcessor() {
            return processor;
        }
        public Saxon.Hej.s9api.XsltTransformer load() {
            Saxon.Hej.s9api.XsltTransformer xt = new Saxon.Hej.s9api.XsltTransformer(processor, preparedStylesheet.newController(), preparedStylesheet.getCompileTimeParams());
            Saxon.Hej.om.StructuredQName initialTemplate = preparedStylesheet.getDefaultInitialTemplateName();
            if ((initialTemplate != null)) {
                xt.setInitialTemplate(new Saxon.Api.QName(initialTemplate));
            }
            return xt;
        }
        public Saxon.Hej.s9api.Xslt30Transformer load30() {
            return new Saxon.Hej.s9api.Xslt30Transformer(processor, preparedStylesheet.newController(), preparedStylesheet.getCompileTimeParams());
        }
        public void explain(Saxon.Hej.s9api.Destination destination) {
            Saxon.Hej.Configuration config = processor.getUnderlyingConfiguration();
            try {
                Saxon.Hej.@event.Receiver @out = destination.getReceiver(config.makePipelineConfiguration(), config.obtainDefaultSerializationProperties());
                preparedStylesheet.explain(new Saxon.Hej.trace.ExpressionPresenter(config, @out));
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public void export(System.IO.Stream destination) {
            string target = preparedStylesheet.getTopLevelPackage().getTargetEdition();
            if ((target == null)) {
                target = getProcessor().getSaxonEdition();
            }
            export(destination, target);
        }
        public void export(System.IO.Stream destination, string target) {
            Saxon.Hej.Configuration config = processor.getUnderlyingConfiguration();
            try {
                Saxon.Hej.style.StylesheetPackage topLevelPackage = preparedStylesheet.getTopLevelPackage();
                Saxon.Hej.trace.ExpressionPresenter presenter = config.newExpressionExporter(target, destination, topLevelPackage);
                presenter.setRelocatable(topLevelPackage.isRelocatable());
                topLevelPackage.export(presenter);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
            try {
                destination.Close();
            } catch (System.IO.IOException e) {
                throw new Saxon.Api.SaxonApiException(e);
            }
        }
        public Saxon.Api.WhitespacePolicy getWhitespaceStrippingPolicy() {
            Saxon.Hej.style.StylesheetPackage top = preparedStylesheet.getTopLevelPackage();
            if (top.isStripsWhitespace()) {
                return new Saxon.Api.WhitespacePolicy(top);
            } else {
                return Saxon.Api.WhitespacePolicy.Unspecified;
            }
        }
        public System.Collections.Generic.Dictionary<Saxon.Api.QName,Saxon.Hej.s9api.XsltExecutable.ParameterDetails> getGlobalParameters() {
            System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.GlobalParam> globals = preparedStylesheet.getGlobalParameters();
            System.Collections.Generic.Dictionary<Saxon.Api.QName,Saxon.Hej.s9api.XsltExecutable.ParameterDetails> @params = new System.Collections.Generic.Dictionary<Saxon.Api.QName, Saxon.Hej.s9api.XsltExecutable.ParameterDetails>();
            foreach (Saxon.Hej.expr.instruct.GlobalParam v in globals.Values) {
                Saxon.Hej.s9api.XsltExecutable.ParameterDetails details = new Saxon.Hej.s9api.XsltExecutable.ParameterDetails(processor, v.getRequiredType(), v.isRequiredParam());
                Saxon.Impl.Helpers.MapUtils.Put(@params, new Saxon.Api.QName(v.getVariableQName()), details);
            }
            return @params;
        }
        public class ParameterDetails {
            private readonly Saxon.Hej.s9api.Processor processor;
            private readonly Saxon.Hej.value.SequenceType type;
            private readonly bool required;
            protected internal ParameterDetails(Saxon.Hej.s9api.Processor processor, Saxon.Hej.value.SequenceType type, bool isRequired)  {
                this.processor = processor;
                this.type = type;
                this.required = isRequired;
            }
            public Saxon.Api.XdmItemType getDeclaredItemType() {
                return new Saxon.Hej.s9api.ConstructedItemType(type.getPrimaryType(), processor.getUnderlyingConfiguration());
            }
            public Saxon.Hej.s9api.OccurrenceIndicator getDeclaredCardinality() {
                return Saxon.Hej.s9api.OccurrenceIndicator.getOccurrenceIndicator(type.getCardinality());
            }
            public Saxon.Hej.value.SequenceType getUnderlyingDeclaredType() {
                return type;
            }
            public bool isRequired() {
                return this.required;
            }
        
        }
        public Saxon.Hej.PreparedStylesheet getUnderlyingCompiledStylesheet() {
            return preparedStylesheet;
        }
    
    }
}