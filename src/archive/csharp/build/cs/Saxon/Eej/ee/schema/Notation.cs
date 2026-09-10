using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class Notation : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Eej.ee.schema.UserSchemaComponent, Saxon.Eej.ee.schema.SerializableSchemaComponent {
        private string notationSystemId;
        private Saxon.Hej.om.StructuredQName notationName;
        private string notationPublicId;
        public Notation(Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Ejavax.xml.transform.SourceLocator locator)  {
            setConfiguration(config);
            setLocator(locator);
        }
        public void setNotationName(Saxon.Hej.om.StructuredQName nameCode) {
            this.notationName = nameCode;
        }
        public Saxon.Hej.om.StructuredQName getNotationName() {
            return notationName;
        }
        public void setNotationSystemId(string systemId) {
            this.notationSystemId = systemId;
        }
        public void setNotationPublicId(string publicId) {
            this.notationPublicId = publicId;
        }
        public string getNotationSystemId() {
            return notationSystemId;
        }
        public string getNotationPublicId() {
            return notationPublicId;
        }
        public string getName() {
            return notationName.getLocalPart();
        }
        public string getTargetNamespace() {
            return notationName.getURI();
        }
        public override bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            return true;
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.VALIDATED);
            return true;
        }
        public bool isSameDeclaration(Saxon.Eej.ee.schema.Notation other) {
            return ((this == other) || ((getNotationName().Equals(other.getNotationName()) && hasSameLocation(other))));
        }
        public override void elaborate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            string id = serializer.getId(this, true);
            serializer.startElement("notation");
            serializer.emitAttribute("id", id);
            serializer.emitAttribute("name", getName());
            if (((getTargetNamespace() != null) && !(getTargetNamespace().Length==0))) {
                serializer.emitAttribute("targetNamespace", getTargetNamespace());
            }
            if ((notationPublicId != null)) {
                serializer.emitAttribute("publicId", notationPublicId);
            }
            if ((notationSystemId != null)) {
                serializer.emitAttribute("systemId", notationSystemId);
            }
            serializer.endElement();
        }
        public override Saxon.Hej.om.Function getComponentAsFunction() {
            Saxon.Eej.ee.schema.Notation thisNotation = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Notation Declaration");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.Notation>(thisNotation);
                  case "name":
                    return new Saxon.Hej.value.StringValue(getName(), Saxon.Hej.type.BuiltInAtomicType.NCNAME);
                  case "target namespace":
                    return new Saxon.Hej.value.AnyURIValue(getTargetNamespace());
                  case "system identifier":
                    if ((getNotationSystemId() == null)) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        return new Saxon.Hej.value.AnyURIValue(getNotationSystemId());
                    }
                  case "public identifier":
                    if ((getNotationPublicId() == null)) {
                        return Saxon.Hej.value.EmptySequence.getInstance();
                    } else {
                        return new Saxon.Hej.value.AnyURIValue(getNotationSystemId());
                    }
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}