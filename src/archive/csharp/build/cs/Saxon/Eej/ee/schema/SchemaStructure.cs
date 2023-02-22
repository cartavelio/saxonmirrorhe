using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public abstract class SchemaStructure : Saxon.Eej.ee.schema.UserSchemaComponent, Saxon.Hej.s9api.Location {
        private string systemId;
        private int lineNumber;
        private string generatedId;
        private string schemaDocumentURI;
        private Saxon.Hej.type.SchemaValidationStatus fixupStatus = Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED;
        private Saxon.Hej.type.SchemaValidationStatus validationStatus = Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED;
        private Saxon.Eej.config.EnterpriseConfiguration config;
        private int redefinitionLevel = 0;
        public string getSystemId() {
            return systemId;
        }
        public string getPublicId() {
            return null;
        }
        public int getLineNumber() {
            return lineNumber;
        }
        public int getColumnNumber() {
            return -1;
        }
        public Saxon.Hej.s9api.Location saveLocation() {
            return this;
        }
        public void setLocator(Saxon.Ejavax.xml.transform.SourceLocator locator) {
            if ((locator != null)) {
                systemId = locator.getSystemId();
                lineNumber = locator.getLineNumber();
                if ((locator is Saxon.Hej.om.NodeInfo)) {
                    generatedId = Saxon.Hej.tree.util.Navigator.getPath((Saxon.Hej.om.NodeInfo)(locator));
                }
            }
        }
        public void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public void setLineNumber(int lineNumber) {
            this.lineNumber = lineNumber;
        }
        public void setSchemaDocumentURI(string uri) {
            this.schemaDocumentURI = uri;
        }
        public string getSchemaDocumentURI() {
            return this.schemaDocumentURI;
        }
        public virtual void setGeneratedId(string generatedId) {
            this.generatedId = generatedId;
        }
        public string getGeneratedId() {
            return generatedId;
        }
        public virtual bool validate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            return true;
        }
        public virtual Saxon.Hej.om.FunctionItem getComponentAsFunction() {
            return null;
        }
        public virtual Saxon.Hej.type.SchemaValidationStatus getValidationStatus() {
            return validationStatus;
        }
        public bool isValidationNeeded() {
            return (validationStatus == Saxon.Hej.type.SchemaValidationStatus.UNVALIDATED);
        }
        public void setValidationStatus(Saxon.Hej.type.SchemaValidationStatus status) {
            validationStatus = status;
        }
        public Saxon.Hej.type.SchemaValidationStatus getFixupStatus() {
            return fixupStatus;
        }
        public void setFixupStatus(Saxon.Hej.type.SchemaValidationStatus status) {
            fixupStatus = status;
        }
        public void setConfiguration(Saxon.Eej.config.EnterpriseConfiguration config) {
            this.config = config;
        }
        public Saxon.Eej.config.EnterpriseConfiguration getConfiguration() {
            return config;
        }
        public void setRedefinitionLevel(int level) {
            redefinitionLevel = level;
        }
        public int getRedefinitionLevel() {
            return redefinitionLevel;
        }
        public virtual void lookForCycles(System.Collections.Generic.Stack<Saxon.Hej.type.SchemaComponent> references, Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        public virtual bool fixup(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
            return true;
        }
        public virtual void elaborate(Saxon.Eej.ee.schema.SchemaCompiler compiler) {
        }
        protected internal static Saxon.Hej.om.FunctionItem makeXPathExpressionPropertyRecord(string conditionText, Saxon.Hej.om.NamespaceResolver nsResolver, string baseURI) {
            Saxon.Hej.expr.CallableDelegate callable = new Saxon.Hej.expr.CallableDelegate((context, arguments) => {
                string key = arguments[0].head().getStringValue();
                switch (key) {
                  case "namespace bindings":
                    System.Collections.Generic.IList<Saxon.Hej.om.Item> bindings = new System.Collections.Generic.List<Saxon.Hej.om.Item>();
                    System.Collections.Generic.IEnumerator<string> prefixes = nsResolver.iteratePrefixes();
                    while (prefixes.MoveNext() /*UNSAFE CONVERSION*/) {
                        string prefix = prefixes.Current;
                        Saxon.Hej.expr.CallableDelegate binding = new Saxon.Hej.expr.CallableDelegate((context1, arguments1) => {
                            string key1 = arguments1[0].head().getStringValue();
                            switch (key1) {
                              case "prefix":
                                return new Saxon.Hej.value.StringValue(prefix);
                              case "namespace":
                                return new Saxon.Hej.value.AnyURIValue((nsResolver.getURIForPrefix(prefix, false)).toUnicodeString());
                              default:
                                return Saxon.Hej.value.EmptySequence.getInstance();
                            }
                        } );
                        bindings.Add(new Saxon.Hej.functions.CallableFunction(1, binding, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE));
                    }
                    return Saxon.Hej.value.SequenceExtent.makeSequenceExtent(bindings);
                  case "default namespace":
                    return new Saxon.Hej.value.AnyURIValue((nsResolver.getURIForPrefix("", true)).toUnicodeString());
                  case "base URI":
                    return new Saxon.Hej.value.AnyURIValue((baseURI));
                  case "expression":
                    return new Saxon.Hej.value.StringValue(conditionText);
                  default:
                    return Saxon.Hej.value.EmptySequence.getInstance();
                }
            } );
            return new Saxon.Hej.functions.CallableFunction(1, callable, Saxon.Hej.type.SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
        }
        public bool hasSameLocation(Saxon.Eej.ee.schema.SchemaStructure other) {
            if (!(/*134*/Saxon.Eej.ee.schema.UserDefinedType.isSameURI(this.getSystemId(), other.getSystemId()))) {
                return false;
            }
            if (((this.getLineNumber()>=0) && (other.getLineNumber()>=0))) {
                return ((this.getLineNumber() == other.getLineNumber()) && (this.getColumnNumber() == other.getColumnNumber()));
            }
            return (((this.generatedId != null) && (other.generatedId != null)) && this.generatedId.Equals(other.generatedId));
        }
    
    }
}