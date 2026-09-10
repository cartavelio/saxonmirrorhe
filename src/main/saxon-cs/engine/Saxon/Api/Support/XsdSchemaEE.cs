using Saxon.Eej.ee.schema;

namespace Saxon.Api.Support {
    internal class XsdSchemaEE : Saxon.Api.XsdSchema {

        private readonly Saxon.Api.Processor processor;
        private readonly Saxon.Hej.s9api.XsdSchema schemaJ;
        
        internal XsdSchemaEE(Saxon.Api.Processor processor, Saxon.Hej.s9api.XsdSchema schemaJ) {
            this.processor = processor;
            this.schemaJ = schemaJ;
        }

        public override Saxon.Api.SchemaValidator NewValidator() {
            Saxon.Hej.s9api.SchemaValidator validatorJ = schemaJ.newValidator();
            return new Saxon.Api.Support.XsdValidatorEE(validatorJ, processor);
        }

        public override XsdSchema Combine(XsdSchema other) {
            return new Saxon.Api.Support.XsdSchemaEE(processor,
                schemaJ.combine(((Saxon.Api.Support.XsdSchemaEE)other).schemaJ));
        }

        public Saxon.Hej.type.Schema UnderlyingSchema => schemaJ.getUnderlyingSchema();

        public override void ExportComponents(IDestination destination) {
            schemaJ.exportComponents(destination.UnderlyingDestination);
        }
    }
}