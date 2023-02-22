using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public class XSIAttributeHandler : Saxon.Eej.ee.validate.ValidatingFilter {
        private readonly System.Collections.Generic.HashSet<string> namespacesEncountered;
        private readonly System.Collections.Generic.Dictionary<string,System.Uri> schemaDocumentsRead;
        private bool useXsiSchemaLocation = true;
        private readonly System.Collections.Generic.IList<Saxon.Hej.type.ValidationFailure> xsiValidationErrors = new System.Collections.Generic.List<Saxon.Hej.type.ValidationFailure>(4);
        public XSIAttributeHandler(Saxon.Eej.ee.validate.ValidationStack @out) : base (@out) {
            namespacesEncountered = new System.Collections.Generic.HashSet<string>(10);
            schemaDocumentsRead = new System.Collections.Generic.Dictionary<string, System.Uri>(10);
            useXsiSchemaLocation = getPipelineConfiguration().getParseOptions().isUseXsiSchemaLocation();
        }
        public override void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            base.setPipelineConfiguration(pipe);
            useXsiSchemaLocation = pipe.getParseOptions().isUseXsiSchemaLocation();
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            bool nilled = false;
            xsiValidationErrors.Clear();
            Saxon.Hej.type.SchemaType xsiType = null;
            bool hasXsiAttributes = false;
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                string uri = att.getNodeName().getURI();
                if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE)) {
                    hasXsiAttributes = true;
                    break ;
                }
            }
            Saxon.Hej.om.AttributeMap attMap2 = attributes;
            if (hasXsiAttributes) {
                System.Collections.Generic.IList<Saxon.Hej.om.AttributeInfo> list = new System.Collections.Generic.List<Saxon.Hej.om.AttributeInfo>(attributes.size());
                foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                    Saxon.Hej.om.NodeName attName = att.getNodeName();
                    Saxon.Hej.type.SimpleType attType = att.getType();
                    string uri = attName.getURI();
                    if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.SCHEMA_INSTANCE)) {
                        string value = att.getValue();
                        int fp = attName.obtainFingerprint(getNamePool());
                        if ((fp == Saxon.Hej.om.StandardNames.XSI_NO_NAMESPACE_SCHEMA_LOCATION)) {
                            attType = Saxon.Hej.type.BuiltInAtomicType.ANY_URI;
                            if (useXsiSchemaLocation) {
                                processXsiNoNamespaceSchemaLocation(value, location);
                            }
                        } else {
                            if ((fp == Saxon.Hej.om.StandardNames.XSI_SCHEMA_LOCATION)) {
                                attType = Saxon.Hej.type.BuiltInListType.ANY_URIS;
                                if (useXsiSchemaLocation) {
                                    processXsiSchemaLocation(value, location);
                                }
                            } else {
                                if ((fp == Saxon.Hej.om.StandardNames.XSI_TYPE)) {
                                    attType = Saxon.Hej.type.BuiltInAtomicType.QNAME;
                                    xsiType = processXsiType(value, namespaces);
                                } else {
                                    if ((fp == Saxon.Hej.om.StandardNames.XSI_NIL)) {
                                        attType = Saxon.Hej.type.BuiltInAtomicType.BOOLEAN;
                                        nilled = processXsiNil(value);
                                    }
                                }
                            }
                        }
                        list.Add(new Saxon.Hej.om.AttributeInfo(att.getNodeName(), attType, att.getValue(), att.getLocation(), att.getProperties()));
                    } else {
                        list.Add(att);
                    }
                }
                attMap2 = Saxon.Hej.om.SequenceTool.attributeMapFromList(list);
            }
            if ((nextReceiver is Saxon.Eej.ee.validate.ValidationStack)) {
                ((Saxon.Eej.ee.validate.ValidationStack)(nextReceiver)).setValidationContext(getValidationContext());
                ((Saxon.Eej.ee.validate.ValidationStack)(nextReceiver)).setIsNilled(nilled);
                ((Saxon.Eej.ee.validate.ValidationStack)(nextReceiver)).setXsiValidationErrors(xsiValidationErrors);
            }
            namespacesEncountered.Add(elemName.getURI());
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                string uri = att.getNodeName().getURI();
                namespacesEncountered.Add(uri);
            }
            getValidationContext().setXSIType(xsiType);
            nextReceiver.startElement(elemName, type, attMap2, namespaces, location, properties);
        }
        private void processXsiSchemaLocation(string value, Saxon.Hej.s9api.Location locationId) {
            Saxon.Ejava.util.StringTokenizer tokenizer = new Saxon.Ejava.util.StringTokenizer(value, " \t\n\r", false);
            string targetNS = null;
            while (tokenizer.hasMoreTokens()) {
                string tok = tokenizer.nextToken();
                if ((targetNS == null)) {
                    targetNS = tok;
                } else {
                    bool ignore;
                    System.Uri absoluteLocation = null;
                    try {
                        absoluteLocation = Saxon.Hej.functions.ResolveURI.makeAbsolute(tok, locationId.getSystemId());
                        ignore = absoluteLocation.Equals(Saxon.Impl.Helpers.MapUtils.Get(schemaDocumentsRead, targetNS));
                        Saxon.Impl.Helpers.MapUtils.Put(schemaDocumentsRead, targetNS, absoluteLocation);
                    } catch (System.UriFormatException err) {
                        ignore = false;
                    }
                    if (!(ignore)) {
                        if (namespacesEncountered.Contains(targetNS)) {
                            if ((getConfiguration().getXsdVersion() == Saxon.Hej.Configuration.XSD11)) {
                            } else {
                                string message = (("xsi:schemaLocation namespace "+Saxon.Hej.trans.Err.wrap(targetNS))+" has already appeared earlier in the instance document");
                                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                                ve.setConstraintReference(1, "schema-loc", "4");
                                reportValidationError(ve, false, locationId);
                            }
                        }
                        loadSchema(absoluteLocation, targetNS);
                    }
                    targetNS = null;
                }
            }
            if ((targetNS != null)) {
                string message = "xsi:schemaLocation must contain an even number of URIs";
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                reportValidationError(ve, false, locationId);
            }
            tokenizer = new Saxon.Ejava.util.StringTokenizer(value, " \t\n\r", false);
            targetNS = null;
            while (tokenizer.hasMoreTokens()) {
                string tok = tokenizer.nextToken();
                if ((targetNS == null)) {
                    targetNS = tok;
                } else {
                    getConfiguration().sealNamespace(targetNS);
                    targetNS = null;
                }
            }
        }
        private void processXsiNoNamespaceSchemaLocation(string value, Saxon.Hej.s9api.Location locationId) {
            bool ignore;
            System.Uri absoluteLocation = null;
            try {
                absoluteLocation = Saxon.Hej.functions.ResolveURI.makeAbsolute(value, locationId.getSystemId());
                ignore = absoluteLocation.Equals(Saxon.Impl.Helpers.MapUtils.Get(schemaDocumentsRead, ""));
                Saxon.Impl.Helpers.MapUtils.Put(schemaDocumentsRead, "", absoluteLocation);
            } catch (System.UriFormatException err) {
                ignore = false;
            }
            if (!(ignore)) {
                if (namespacesEncountered.Contains("")) {
                    string message = ("xsi:noNamespaceSchemaLocation cannot appear after the "+"first no-namespace element or attribute");
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                    ve.setConstraintReference(1, "schema-loc", "4");
                    reportValidationError(ve, false, locationId);
                }
                loadSchema(absoluteLocation, "");
            }
        }
        private bool processXsiNil(string value) {
            Saxon.Hej.type.ConversionResult v = Saxon.Hej.type.StringConverter.StringToBoolean.INSTANCE.convertString(Saxon.Hej.str.StringView.of(value).tidy());
            if ((v is Saxon.Hej.type.ValidationFailure)) {
                string message = "The value of xsi:nil must be a boolean";
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                xsiValidationErrors.Add(ve);
            }
            return (v == Saxon.Hej.value.BooleanValue.TRUE);
        }
        private Saxon.Hej.type.SchemaType processXsiType(string value, Saxon.Hej.om.NamespaceResolver nsResolver) {
            Saxon.Hej.Configuration config = getConfiguration();
            try {
                Saxon.Hej.om.StructuredQName qName = Saxon.Hej.om.StructuredQName.fromLexicalQName((value), true, false, nsResolver);
                Saxon.Hej.type.SchemaType type = config.getSchemaType(qName);
                if ((type == null)) {
                    string message = (("Unknown type "+Saxon.Hej.trans.Err.wrap(value))+" specified in xsi:type attribute");
                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                    ve.setConstraintReference(1, "cvc-elt", "4.2");
                    xsiValidationErrors.Add(ve);
                } else {
                    if (((type is Saxon.Hej.type.ComplexType) && ((Saxon.Hej.type.ComplexType)(type)).isAbstract())) {
                        string message = (("Type "+Saxon.Hej.trans.Err.wrap(value))+" specified in xsi:type attribute is abstract");
                        Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                        ve.setConstraintReference(1, "cvc-type", "2");
                        xsiValidationErrors.Add(ve);
                    } else {
                        return type;
                    }
                }
            } catch (Saxon.Hej.trans.XPathException de) {
                string message = ((("Invalid QName "+Saxon.Hej.trans.Err.wrap(value))+" specified in xsi:type attribute: ")+de.Message);
                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                ve.setConstraintReference(1, "cvc-elt", "4.1");
                xsiValidationErrors.Add(ve);
            }
            return null;
        }
        public override void endDocument() {
            base.endDocument();
            reportIfInvalid();
        }
        private void loadSchema(System.Uri value, string targetNS) {
            int status = ((Saxon.Eej.config.EnterpriseConfiguration)(getConfiguration())).getNamespaceStatus(targetNS);
            if (((status != Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_UNKNOWN) && (status != Saxon.Eej.ee.schema.PreparedSchema.NAMESPACE_UNDER_CONSTRUCTION))) {
                return ;
            }
            Saxon.Hej.@event.PipelineConfiguration pipe;
            try {
                pipe = new Saxon.Hej.@event.PipelineConfiguration(getPipelineConfiguration());
                Saxon.Hej.lib.InvalidityHandler handler = getValidationContext().getInvalidityHandler();
                if ((handler is Saxon.Hej.lib.InvalidityHandlerWrappingErrorReporter)) {
                    Saxon.Hej.lib.ErrorReporter listener = ((Saxon.Hej.lib.InvalidityHandlerWrappingErrorReporter)(handler)).getErrorReporter();
                    pipe.setErrorReporter(listener);
                }
                Saxon.Eej.config.EnterpriseConfiguration config = (Saxon.Eej.config.EnterpriseConfiguration)(pipe.getConfiguration());
                config.readSchema(pipe, null, value.ToString(), targetNS);
            } catch (Saxon.Hej.type.SchemaException e) {
                try {
                    pipe = new Saxon.Hej.@event.PipelineConfiguration(getPipelineConfiguration());
                    Saxon.Eej.config.EnterpriseConfiguration config = (Saxon.Eej.config.EnterpriseConfiguration)(pipe.getConfiguration());
                    Saxon.Eej.ee.schema.SchemaCompiler compiler = config.newSchemaCompiler();
                    compiler.setPreparedSchema(config.getSuperSchema());
                    config.getSuperSchema().validate(compiler);
                    Saxon.Hej.lib.ErrorReporter reporter = pipe.getErrorReporter();
                    reporter.report(new Saxon.Hej.trans.XmlProcessingException(e).asWarning());
                    reporter.report(((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(("Validation will continue without the schema at "+value)).asWarning()));
                } catch (Saxon.Hej.type.SchemaException err) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                }
            }
        }
    
    }
}