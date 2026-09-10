using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class TypeAlternative : Saxon.Eej.ee.schema.SchemaStructure, Saxon.Hej.type.SchemaComponent {
        private readonly Saxon.Hej.sxpath.XPathExpression condition;
        private readonly string conditionText;
        private Saxon.Hej.om.NamespaceResolver namespaceContext;
        private string baseURI;
        private string defaultNamespace;
        private System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable> declaredParams;
        private Saxon.Eej.ee.schema.TypeReference typeReference;
        public TypeAlternative(Saxon.Eej.config.EnterpriseConfiguration config, Saxon.Hej.sxpath.XPathExpression condition, string text)  {
            setConfiguration(config);
            this.condition = condition;
            this.conditionText = text;
        }
        public Saxon.Hej.sxpath.XPathExpression getCondition() {
            return condition;
        }
        public string getConditionText() {
            return conditionText;
        }
        public void setDefaultElementNamespace(string @namespace) {
            defaultNamespace = @namespace;
        }
        public string getDefaultElementNamespace() {
            return defaultNamespace;
        }
        public void setNamespaceContext(Saxon.Hej.om.NamespaceResolver resolver) {
            this.namespaceContext = resolver;
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceContext() {
            return namespaceContext;
        }
        public void setBaseURI(string baseURI) {
            this.baseURI = baseURI;
        }
        public string getBaseURI() {
            return baseURI;
        }
        public void setTypeReference(Saxon.Eej.ee.schema.TypeReference type) {
            typeReference = type;
        }
        public Saxon.Eej.ee.schema.TypeReference getTypeReference() {
            return typeReference;
        }
        public void setParameters(System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable> parameters) {
            declaredParams = parameters;
        }
        public System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable> getParameters() {
            return declaredParams;
        }
        public Saxon.Hej.type.SchemaType getSchemaType() {
            return (Saxon.Hej.type.SchemaType)(typeReference.getTarget());
        }
        public override bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            Saxon.Hej.type.SchemaType type = (Saxon.Hej.type.SchemaType)(/*134*/Saxon.Eej.ee.schema.PreparedSchema.validateReference(typeReference, compiler, false));
            if ((type == null)) {
                setFixupStatus(/*explicit enum*/ Saxon.Hej.type.SchemaValidationStatus.INVALID);
                return false;
            }
            return (!(((type is Saxon.Eej.ee.schema.UserSchemaComponent))) || ((Saxon.Eej.ee.schema.UserSchemaComponent)(type)).fixup(compiler));
        }
        public void serialize(Saxon.Eej.ee.schema.SchemaModelSerializer serializer) {
            serializer.startElement("alternativeType");
            serializer.emitNamespaceContext(getNamespaceContext());
            serializer.emitAttribute("test", ((conditionText == null) ? "true()" : conditionText));
            serializer.emitAttribute("type", serializer.getTypeLink((Saxon.Hej.type.SchemaType)(typeReference.getTarget())));
            serializer.emitAttribute("defaultNamespace", getDefaultElementNamespace());
            serializer.emitAttribute("xml:base", getBaseURI());
            serializer.endElement();
        }
        public override Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            Saxon.Eej.ee.schema.TypeAlternative thisTypeAlternative = this;
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "class":
                    return Saxon.Hej.value.StringValue.bmp("Type Alternative");
                  case "implementation":
                    return new Saxon.Hej.value.ObjectValue<Saxon.Eej.ee.schema.TypeAlternative>(thisTypeAlternative);
                  case "test":
                    return /*134*/Saxon.Eej.ee.schema.SchemaStructure.makeXPathExpressionPropertyRecord(conditionText, namespaceContext, baseURI);
                  case "type definition":
                    return getTypeReference().getTarget().getComponentAsFunction();
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
    
    }
}