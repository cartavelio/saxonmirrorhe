using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.schema {
    public class SchemaCompiler {
        private readonly Saxon.Eej.config.EnterpriseConfiguration config;
        private Saxon.Eej.ee.schema.PreparedSchema preparedSchema;
        private int[] occurrenceLimits;
        private readonly Saxon.Hej.lib.SchemaURIResolver schemaURIResolver;
        private Saxon.Hej.lib.ErrorReporter errorReporter;
        private System.Collections.Generic.Dictionary<Saxon.Eej.ee.schema.IdentityConstraintReference,Saxon.Hej.type.SchemaType> identityConstraintsAwaitingTypeChecking = null;
        private int errorCount = 0;
        private int xsdlVersion = Saxon.Hej.Configuration.XSD10;
        private readonly bool allowDanglingReferences;
        private readonly System.Collections.Generic.Dictionary<string,Saxon.Eej.ee.schema.sdoc.XSDSchema> documentsAlreadyRead = new System.Collections.Generic.Dictionary<string, Saxon.Eej.ee.schema.sdoc.XSDSchema>(10);
        private readonly System.Collections.Generic.HashSet<string> documentsBeingRead = new System.Collections.Generic.HashSet<string>(10);
        private Saxon.Hej.@event.PipelineConfiguration pipelineConfiguration;
        private Saxon.Eej.ee.schema.sdoc.SchemaDocumentTransformer chameleonTransformer = null;
        private Saxon.Eej.ee.schema.sdoc.SchemaDocumentTransformer overrideTransformer = null;
        private readonly System.Collections.Generic.ISet<string> componentsReportedMissing = new System.Collections.Generic.HashSet<string>();
        private System.Collections.Generic.ISet<Saxon.Eej.ee.schema.UserComplexType> scheduledForAutomaton = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.UserComplexType>();
        private System.Collections.Generic.ISet<Saxon.Eej.ee.schema.UserComplexType[]> scheduledForAutomatonCheck = new System.Collections.Generic.HashSet<Saxon.Eej.ee.schema.UserComplexType[]>();
        public SchemaCompiler(Saxon.Eej.config.EnterpriseConfiguration config)  {
            this.config = config;
            errorReporter = config.makeErrorReporter();
            schemaURIResolver = config.getSchemaURIResolver();
            occurrenceLimits = config.getOccurrenceLimits();
            allowDanglingReferences = config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_UNRESOLVED_SCHEMA_COMPONENTS);
        }
        public Saxon.Eej.config.EnterpriseConfiguration getConfiguration() {
            return config;
        }
        public Saxon.Hej.om.NamePool getNamePool() {
            return config.getNamePool();
        }
        public void setLanguageVersion(int version) {
            xsdlVersion = version;
        }
        public int getLanguageVersion() {
            return xsdlVersion;
        }
        public void setPreparedSchema(Saxon.Eej.ee.schema.PreparedSchema schema) {
            preparedSchema = schema;
        }
        public Saxon.Eej.ee.schema.PreparedSchema getPreparedSchema() {
            return preparedSchema;
        }
        public Saxon.Hej.lib.ErrorReporter getErrorReporter() {
            if ((errorReporter == null)) {
                errorReporter = config.makeErrorReporter();
            }
            return errorReporter;
        }
        public void setErrorReporter(Saxon.Hej.lib.ErrorReporter reporter) {
            this.errorReporter = reporter;
        }
        public int getErrorCount() {
            return errorCount;
        }
        public void setOccurrenceLimits(int minimum, int maximum) {
            if (((minimum<1) || (minimum>=maximum))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException("Invalid occurrence limits: min<1 or max<min");
            }
            occurrenceLimits = new int[]{minimum, maximum};
        }
        public int[] getOccurrenceLimits() {
            return occurrenceLimits;
        }
        public void setBeingRead(string systemId) {
            documentsBeingRead.Add(systemId);
        }
        public bool isBeingRead(string systemId) {
            return documentsBeingRead.Contains(systemId);
        }
        public void markAsRead(string systemId, Saxon.Eej.ee.schema.sdoc.XSDSchema schema) {
            documentsBeingRead.Remove(systemId);
            Saxon.Impl.Helpers.MapUtils.Put(documentsAlreadyRead, ((systemId+"##")+schema.getTargetNamespace()), schema);
        }
        public bool isAllowDanglingReferences() {
            return allowDanglingReferences;
        }
        public Saxon.Eej.ee.schema.sdoc.XSDSchema getExistingSchemaDocument(string systemId, string targetNamespace) {
            return Saxon.Impl.Helpers.MapUtils.Get(documentsAlreadyRead, ((systemId+"##")+targetNamespace));
        }
        public Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipelineConfiguration;
        }
        public void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipelineConfiguration) {
            this.pipelineConfiguration = pipelineConfiguration;
        }
        public Saxon.Hej.lib.SchemaURIResolver getSchemaURIResolver() {
            return schemaURIResolver;
        }
        public string readSchema(Saxon.Hej.@event.PipelineConfiguration pipe, string baseURI, string schemaLocation, string expected) {
            Saxon.Ejavax.xml.transform.Source source = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.getSource(baseURI, schemaLocation, pipe, expected);
            if ((source == null)) {
                throw new Saxon.Hej.type.SchemaException(("No schema could be loaded at location "+schemaLocation));
            }
            Saxon.Eej.ee.schema.sdoc.SchemaDocument schemadoc = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.read(source, this, pipe, null);
            if (schemadoc.isCompiledSchemaDocument()) {
                try {
                    getConfiguration().importComponents(schemadoc.getDocument().getRootNode().asActiveSource());
                    return expected;
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw new Saxon.Hej.type.SchemaException(e);
                }
            } else {
                return processSchemaDocument(schemadoc, schemaLocation, expected);
            }
        }
        private string processSchemaDocument(Saxon.Eej.ee.schema.sdoc.SchemaDocument schemadoc, string schemaLocation, string expected) {
            if (schemadoc.isCompiledSchemaDocument()) {
                return expected;
            }
            string targetNamespace = schemadoc.getTargetNamespace();
            if (((expected != null) && !(expected.Equals(targetNamespace)))) {
                Saxon.Hej.type.SchemaException exception = new Saxon.Hej.type.SchemaException((((((("Schema "+(((schemaLocation == null) ? "" : (("at location "+schemaLocation)+" "))))+"has target namespace \"")+targetNamespace)+"\" but requested namespace was \"")+expected)+'\"'));
                if ((schemaLocation == null)) {
                    exception.setErrorCode("XTSE0215");
                }
                throw exception;
            }
            Saxon.Eej.ee.schema.PreparedSchema ps = schemadoc.getXSDSchema().getSchema();
            preparedSchema = ps;
            validate(schemadoc);
            preparedSchema = null;
            System.Collections.Generic.ICollection<Saxon.Hej.expr.instruct.GlobalParam> globalParams = schemadoc.getXSDSchema().getGlobalParams();
            foreach (Saxon.Hej.expr.instruct.GlobalParam param in globalParams) {
                ps.addGlobalParam(param);
            }
            return targetNamespace;
        }
        private void retainSchemaComponents(Saxon.Eej.ee.schema.PreparedSchema ps) {
            if ((preparedSchema == null)) {
                preparedSchema = ps;
            } else {
                preparedSchema = /*134*/Saxon.Eej.ee.schema.PreparedSchema.merge(preparedSchema, ps);
            }
        }
        public void readMultipleSchemas(Saxon.Hej.@event.PipelineConfiguration pipe, string baseURI, System.Collections.Generic.IList<string> schemaLocations, string expected) {
            string[] locations = new string[schemaLocations.Count];
            int loc = 0;
            foreach (string schemaLocation in schemaLocations) {
                locations[loc++] = schemaLocation;
            }
            Saxon.Ejavax.xml.transform.Source[] sources = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.getSources(baseURI, locations, pipe, expected);
            if ((sources.Length == 0)) {
                return ;
            }
            bool success = false;
            Saxon.Hej.type.SchemaException lastException = null;
            Saxon.Eej.ee.schema.PreparedSchema mergedSchema = null;
            for (int i = 0; (i<sources.Length); i++) {
                Saxon.Eej.ee.schema.sdoc.SchemaDocument schemadoc;
                try {
                    Saxon.Ejavax.xml.transform.Source source = sources[i];
                    schemadoc = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.read(source, this, pipe, null);
                    if (schemadoc.isCompiledSchemaDocument()) {
                        throw new Saxon.Hej.type.SchemaException("Only source schema documents (not SCM compiled schemas) are accepted here");
                    }
                    success = true;
                    Saxon.Eej.ee.schema.PreparedSchema ps = schemadoc.getXSDSchema().getSchema();
                    if ((mergedSchema == null)) {
                        mergedSchema = ps;
                    } else {
                        mergedSchema = /*134*/Saxon.Eej.ee.schema.PreparedSchema.merge(mergedSchema, ps);
                    }
                    string targetNamespace = schemadoc.getTargetNamespace();
                    if (((expected != null) && !(expected.Equals(targetNamespace)))) {
                        throw new Saxon.Hej.type.SchemaException((((((("Schema at "+locations[i])+" has target namespace \"")+targetNamespace)+"\" but requested namespace was \"")+expected)+'\"'));
                    }
                } catch (Saxon.Hej.type.SchemaException e) {
                    if ((e.getException() is System.IO.IOException)) {
                        Saxon.Hej.trans.XmlProcessingException ee = new Saxon.Hej.trans.XmlProcessingException(e);
                        ee.setWarning(true);
                        pipe.getErrorReporter().report(ee);
                    } else {
                        lastException = e;
                    }
                }
            }
            if ((lastException != null)) {
                throw lastException;
            }
            if (!(success)) {
                throw new Saxon.Hej.type.SchemaException("No valid schema was found");
            }
            preparedSchema = mergedSchema;
            mergedSchema.validate(this);
            makeAllAutomata();
            checkAllAutomata();
            preparedSchema = null;
        }
        public string readInlineSchema(Saxon.Hej.om.NodeInfo root, string expected, Saxon.Hej.lib.ErrorReporter errorReporter) {
            Saxon.Hej.lib.ErrorReporter listener = ((errorReporter == null) ? getErrorReporter() : errorReporter);
            setErrorReporter(listener);
            Saxon.Hej.@event.PipelineConfiguration pipe = config.makePipelineConfiguration();
            pipe.setErrorReporter(listener);
            pipe.setHostLanguage(Saxon.Api.HostLanguage.XML_SCHEMA);
            Saxon.Eej.ee.schema.sdoc.SchemaDocument schemadoc = /*134*/Saxon.Eej.ee.schema.sdoc.SchemaReader.read(root.asActiveSource(), this, pipe, null);
            return processSchemaDocument(schemadoc, null, expected);
        }
        public void compile() {
            bool valid = preparedSchema.validate(this);
            if (!(valid)) {
                throw new Saxon.Hej.type.SchemaException("The schema is invalid");
            }
        }
        public Saxon.Eej.ee.schema.PreparedSchema validate(Saxon.Eej.ee.schema.sdoc.SchemaDocument doc) {
            if (doc.isCompiledSchemaDocument()) {
                throw new Saxon.Hej.type.SchemaException("Only source schema documents (not SCM compiled schemas) are accepted here");
            }
            Saxon.Eej.ee.schema.sdoc.XSDSchema schemaRoot = doc.getXSDSchema();
            preparedSchema = schemaRoot.getSchema();
            int errors = schemaRoot.getErrorCount();
            if ((errors>0)) {
                string msg = ((errors == 1) ? "One error was" : (errors+" errors were"));
                throw new Saxon.Hej.type.SchemaException((msg+" found while processing the schema"));
            }
            foreach (Saxon.Hej.expr.instruct.GlobalParam param in schemaRoot.getGlobalParams()) {
                preparedSchema.addGlobalParam(param);
            }
            compile();
            makeAllAutomata();
            checkAllAutomata();
            Saxon.Eej.ee.schema.PreparedSchema ps = preparedSchema;
            preparedSchema = null;
            return ps;
        }
        public bool reportMissingComponent(string componentType, Saxon.Hej.om.StructuredQName componentName) {
            string key = ((componentType+" ")+componentName.getEQName());
            return componentsReportedMissing.Add(key);
        }
        public void error(string message, Saxon.Hej.s9api.Location locator) {
            errorCount++;
            getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingIncident(message, Saxon.Hej.trans.SaxonErrorCode.SXXP0006, locator));
        }
        public void warning(string message, Saxon.Hej.s9api.Location locator) {
            if (((locator.getSystemId() != null) && locator.getSystemId().EndsWith("override.xsl"))) {
                locator = new Saxon.Hej.expr.parser.Loc("schema_document_resulting_from_override_processing", -1, -1);
            }
            Saxon.Hej.trans.XmlProcessingIncident exception = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(message, Saxon.Hej.trans.SaxonErrorCode.SXWN9000, locator).asWarning());
            getErrorReporter().report(exception);
        }
        public void addPendingTypeCheck(Saxon.Eej.ee.schema.IdentityConstraintReference constraint, Saxon.Hej.type.SchemaType contextItemType) {
            if ((identityConstraintsAwaitingTypeChecking == null)) {
                identityConstraintsAwaitingTypeChecking = new System.Collections.Generic.Dictionary<Saxon.Eej.ee.schema.IdentityConstraintReference,Saxon.Hej.type.SchemaType>(20);
            }
            Saxon.Impl.Helpers.MapUtils.Put(identityConstraintsAwaitingTypeChecking, constraint, contextItemType);
        }
        public void typeCheckIdentityConstraints() {
            try {
                if ((identityConstraintsAwaitingTypeChecking != null)) {
                    foreach (System.Collections.Generic.KeyValuePair<Saxon.Eej.ee.schema.IdentityConstraintReference,Saxon.Hej.type.SchemaType> e in identityConstraintsAwaitingTypeChecking) {
                        Saxon.Eej.ee.schema.IdentityConstraint constraint = ((/*1430*/Saxon.Eej.ee.schema.IdentityConstraint)e.Key.getTarget());
                        Saxon.Hej.type.SchemaType contextItemType = e.Value;
                        constraint.typeCheck(this, contextItemType);
                    }
                    identityConstraintsAwaitingTypeChecking = null;
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.type.SchemaException(e);
            }
        }
        public Saxon.Eej.ee.schema.sdoc.SchemaDocument doChameleonTransformation(Saxon.Hej.om.NodeInfo input, Saxon.Eej.ee.schema.sdoc.SchemaElement referrer) {
            if ((chameleonTransformer == null)) {
                chameleonTransformer = new Saxon.Eej.ee.schema.sdoc.SchemaDocumentTransformer(config, "chameleon.xsl");
            }
            System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.Sequence> @params = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.om.Sequence>();
            Saxon.Impl.Helpers.MapUtils.Put(@params, new Saxon.Hej.om.StructuredQName("", "", "newTargetNamespace"), new Saxon.Hej.value.AnyURIValue((referrer.getXSDSchema().getTargetNamespace())));
            Saxon.Hej.expr.instruct.GlobalParameterSet parameterSet = new Saxon.Hej.expr.instruct.GlobalParameterSet();
            parameterSet.put(new Saxon.Hej.om.StructuredQName("", "", "newTargetNamespace"), new Saxon.Hej.value.AnyURIValue((referrer.getXSDSchema().getTargetNamespace())));
            Saxon.Hej.om.TreeInfo doc = chameleonTransformer.transform(this, pipelineConfiguration, referrer, input, parameterSet);
            return new Saxon.Eej.ee.schema.sdoc.SchemaDocument(doc);
        }
        public Saxon.Eej.ee.schema.sdoc.SchemaDocument doOverrideTransformation(Saxon.Hej.om.NodeInfo input, Saxon.Eej.ee.schema.sdoc.SchemaElement referrer) {
            if ((overrideTransformer == null)) {
                overrideTransformer = new Saxon.Eej.ee.schema.sdoc.SchemaDocumentTransformer(config, "override.xsl");
            }
            Saxon.Hej.expr.instruct.GlobalParameterSet @params = new Saxon.Hej.expr.instruct.GlobalParameterSet();
            @params.put(new Saxon.Hej.om.StructuredQName("", "", "overrideElement"), referrer);
            Saxon.Hej.om.TreeInfo doc = overrideTransformer.transform(this, pipelineConfiguration, referrer, input, @params);
            return new Saxon.Eej.ee.schema.sdoc.SchemaDocument(doc);
        }
        public void scheduleAutomaton(Saxon.Eej.ee.schema.UserComplexType type) {
            scheduledForAutomaton.Add(type);
        }
        public void scheduleAutomatonCheck(Saxon.Eej.ee.schema.UserComplexType @base, Saxon.Eej.ee.schema.UserComplexType sub) {
            scheduledForAutomatonCheck.Add(new Saxon.Eej.ee.schema.UserComplexType[]{@base, sub});
        }
        public void makeAllAutomata() {
            foreach (Saxon.Eej.ee.schema.UserComplexType type in scheduledForAutomaton) {
                type.makeAutomaton(this);
            }
        }
        public void checkAllAutomata() {
            foreach (Saxon.Eej.ee.schema.UserComplexType[] pair in scheduledForAutomatonCheck) {
                Saxon.Eej.ee.schema.UserComplexType sub = pair[1];
                Saxon.Eej.ee.schema.UserComplexType @base = pair[0];
                string subsumeError = /*134*/Saxon.Eej.ee.schema.fsa.FiniteStateMachine.subsumesMachine(@base.getFiniteStateMachine(), sub.getFiniteStateMachine(), this);
                if ((subsumeError != null)) {
                    string message;
                    if (subsumeError.StartsWith("?")) {
                        message = (((((("Saxon is unable to determine whether or not the"+" content model of the complex type ")+sub.getDescription())+" is a valid restriction of the content model of the type ")+@base.getDescription())+". ")+subsumeError.Substring(1));
                    } else {
                        message = ((((("The content model of the complex type "+sub.getDescription())+" is not a valid restriction of the content model of the type ")+@base.getDescription())+". ")+subsumeError);
                    }
                    error(message, sub);
                    throw new Saxon.Hej.type.SchemaException(message);
                }
            }
        }
    
    }
}