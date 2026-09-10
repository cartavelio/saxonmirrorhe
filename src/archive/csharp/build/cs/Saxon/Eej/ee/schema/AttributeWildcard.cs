using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class AttributeWildcard : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Eej.ee.schema.UserSchemaComponent {
        private Saxon.Eej.ee.schema.Wildcard wildcard;
        public AttributeWildcard(Saxon.Eej.config.EnterpriseConfiguration config)  {
            wildcard = new Saxon.Eej.ee.schema.Wildcard();
            wildcard.setConfiguration(config);
            setConfiguration(config);
        }
        public AttributeWildcard(Saxon.Eej.ee.schema.Wildcard wc)  {
            wildcard = wc;
            setConfiguration(wc.getConfiguration());
        }
        public void setWildcard(Saxon.Eej.ee.schema.Wildcard wildcard) {
            this.wildcard = wildcard;
        }
        public Saxon.Eej.ee.schema.Wildcard getWildcard() {
            return wildcard;
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            if (!(wildcard.validate(compiler))) {
                return false;
            }
            if (wildcard.isInexpressible()) {
                compiler.error("The intersection or union of this wildcard with other wildcards is inexpressible", this);
                return false;
            }
            return true;
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            string id = serializer.getId(wildcard, false);
            serializer.startElement("attributeWildcard");
            serializer.emitAttribute("ref", id);
            serializer.endElement();
        }
        public override Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            return wildcard.getComponentAsFunction();
        }
        public override Saxon.Hej.type.SchemaValidationStatus getValidationStatus() {
            return Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED;
        }
    
    }
}