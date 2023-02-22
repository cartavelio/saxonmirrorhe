using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.trans {
    public class ConfigurationReader : Saxon.Hej.@event.Receiver {
        private int level = 0;
        private string section = null;
        private string subsection = null;
        private readonly Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(100);
        protected internal Saxon.Hej.Configuration targetConfig;
        private Saxon.Ejava.lang.ClassLoader classLoader = null;
        private readonly System.Collections.Generic.IList<Saxon.Hej.s9api.XmlProcessingError> errors = new System.Collections.Generic.List<Saxon.Hej.s9api.XmlProcessingError>();
        private Saxon.Hej.trans.packages.PackageLibrary packageLibrary;
        private Saxon.Hej.trans.packages.PackageDetails currentPackage;
        private Saxon.Hej.Configuration baseConfiguration;
        private Saxon.Hej.@event.PipelineConfiguration pipe;
        private readonly System.Collections.Generic.Stack<string> localNameStack = new System.Collections.Generic.Stack<string>();
        private string systemId = null;
        public ConfigurationReader()  {
        }
        public void setPipelineConfiguration(Saxon.Hej.@event.PipelineConfiguration pipe) {
            this.pipe = pipe;
        }
        public Saxon.Hej.@event.PipelineConfiguration getPipelineConfiguration() {
            return pipe;
        }
        public void setSystemId(string systemId) {
            this.systemId = systemId;
        }
        public void open() {
        }
        public void startDocument(int properties) {
        }
        public void setUnparsedEntity(string name, string systemID, string publicID) {
        }
        public void processingInstruction(string name, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location location, int properties) {
        }
        public void comment(Saxon.Hej.str.UnicodeString content, Saxon.Hej.s9api.Location location, int properties) {
        }
        public void close() {
        }
        public string getSystemId() {
            return systemId;
        }
        public void setClassLoader(Saxon.Ejava.lang.ClassLoader classLoader) {
            this.classLoader = classLoader;
        }
        public void setBaseConfiguration(Saxon.Hej.Configuration @base) {
            this.baseConfiguration = @base;
        }
        public Saxon.Hej.Configuration makeConfiguration(Saxon.Ejavax.xml.transform.Source source) {
            Saxon.Hej.Configuration localConfig = baseConfiguration;
            if ((localConfig == null)) {
                if ((pipe == null)) {
                    localConfig = new Saxon.Hej.Configuration();
                    setPipelineConfiguration(localConfig.makePipelineConfiguration());
                } else {
                    localConfig = pipe.getConfiguration();
                }
            } else {
                if ((pipe == null)) {
                    pipe = localConfig.makePipelineConfiguration();
                }
            }
            setSystemId(source.getSystemId());
            Saxon.Hej.lib.ActiveSource activeSource = Saxon.Hej.Version.platform.resolveSource(source, localConfig);
            activeSource.deliver(this, new Saxon.Hej.lib.ParseOptions());
            if (!(errors.Count==0)) {
                Saxon.Hej.lib.ErrorReporter reporter;
                if ((targetConfig == null)) {
                    reporter = new Saxon.Hej.lib.StandardErrorReporter();
                } else {
                    reporter = targetConfig.makeErrorReporter();
                }
                foreach (Saxon.Hej.s9api.XmlProcessingError err in errors) {
                    reporter.report(err.asWarning());
                }
                throw Saxon.Hej.trans.XPathException.fromXmlProcessingError(errors[0]);
            }
            if ((baseConfiguration != null)) {
                targetConfig.importLicenseDetails(baseConfiguration);
            }
            return targetConfig;
        }
        public void endDocument() {
            if ((targetConfig != null)) {
                targetConfig.getDefaultXsltCompilerInfo().setPackageLibrary(packageLibrary);
            }
        }
        public void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap atts, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            string uri = elemName.getURI();
            string localName = elemName.getLocalPart();
            localNameStack.Push(localName);
            buffer.setLength(0);
            if (Saxon.Hej.lib.NamespaceConstant.SAXON_CONFIGURATION.Equals(uri)) {
                if ((level == 0)) {
                    if (!("configuration".Equals(localName))) {
                        error(localName, null, null, "configuration");
                    }
                    string edition = atts.getValue("edition");
                    if ((edition == null)) {
                        edition = "HE";
                    }
                    string editionEE = "EE";
                    edition = editionEE;
                    switch (edition) {
                      case "HE":
                        targetConfig = new Saxon.Hej.Configuration();
                        break ;
                      case "PE":
                        targetConfig = Saxon.Hej.Configuration.makeLicensedConfiguration(classLoader, "com.saxonica.config.ProfessionalConfiguration");
                        break ;
                      case "EE":
                        targetConfig = Saxon.Hej.Configuration.makeLicensedConfiguration(classLoader, "com.saxonica.config.EnterpriseConfiguration");
                        break ;
                      default:
                        error("configuration", "edition", edition, "HE|PE|EE");
                        targetConfig = new Saxon.Hej.Configuration();
                        break ;
                    }
                    if ((baseConfiguration != null)) {
                        targetConfig.setNamePool(baseConfiguration.getNamePool());
                        targetConfig.setDocumentNumberAllocator(baseConfiguration.getDocumentNumberAllocator());
                    }
                    packageLibrary = new Saxon.Hej.trans.packages.PackageLibrary(targetConfig.getDefaultXsltCompilerInfo());
                    string licenseLoc = atts.getValue("licenseFileLocation");
                    if ((((licenseLoc != null) && !(edition.Equals("HE"))) && !(targetConfig.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)))) {
                        string @base = getSystemId();
                        try {
                            System.Uri absoluteLoc = Saxon.Hej.functions.ResolveURI.makeAbsolute(licenseLoc, @base);
                            targetConfig.setConfigurationProperty(Saxon.Hej.lib.FeatureKeys.LICENSE_FILE_LOCATION, absoluteLoc.ToString());
                        } catch (System.Exception err) {
                            Saxon.Hej.trans.XmlProcessingIncident incident = new Saxon.Hej.trans.XmlProcessingIncident(("Failed to process license at "+licenseLoc));
                            incident.setCause(err);
                            errors.Add(incident);
                        }
                    }
                    string targetEdition = atts.getValue("targetEdition");
                    if ((targetEdition != null)) {
                        packageLibrary.getCompilerInfo().setTargetEdition(targetEdition);
                    }
                    string label = atts.getValue("label");
                    if ((label != null)) {
                        targetConfig.setLabel(label);
                    }
                    targetConfig.getDynamicLoader().setClassLoader(classLoader);
                }
                if ((level == 1)) {
                    section = localName;
                    if ("global".Equals(localName)) {
                        readGlobalElement(atts);
                    } else {
                        if ("serialization".Equals(localName)) {
                            readSerializationElement(atts, namespaces);
                        } else {
                            if ("xquery".Equals(localName)) {
                                readXQueryElement(atts);
                            } else {
                                if ("xslt".Equals(localName)) {
                                    readXsltElement(atts);
                                } else {
                                    if ("xsltPackages".Equals(localName)) {
                                    } else {
                                        if ("xsd".Equals(localName)) {
                                            readXsdElement(atts);
                                        } else {
                                            if ("resources".Equals(localName)) {
                                            } else {
                                                if ("collations".Equals(localName)) {
                                                } else {
                                                    if ("localizations".Equals(localName)) {
                                                        readLocalizationsElement(atts);
                                                    } else {
                                                        error(localName, null, null, null);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                } else {
                    if ((level == 2)) {
                        subsection = localName;
                        switch (section) {
                          case "resources":
                            if ("fileExtension".Equals(localName)) {
                                readFileExtension(atts);
                            }
                            break ;
                          case "collations":
                            if (!("collation".Equals(localName))) {
                                error(localName, null, null, "collation");
                            } else {
                                readCollation(atts);
                            }
                            break ;
                          case "localizations":
                            if (!("localization".Equals(localName))) {
                                error(localName, null, null, "localization");
                            } else {
                                readLocalization(atts);
                            }
                            break ;
                          case "xslt":
                            if ("extensionElement".Equals(localName)) {
                                readExtensionElement(atts);
                            } else {
                                error(localName, null, null, null);
                            }
                            break ;
                          case "xsltPackages":
                            if ("package".Equals(localName)) {
                                readXsltPackage(atts);
                            }
                            break ;
                        }
                    } else {
                        if ((level == 3)) {
                            if ("package".Equals(subsection)) {
                                if ("withParam".Equals(localName)) {
                                    readWithParam(atts, namespaces);
                                } else {
                                    error(localName, null, null, null);
                                }
                            }
                        }
                    }
                }
            } else {
                Saxon.Hej.trans.XmlProcessingIncident incident = new Saxon.Hej.trans.XmlProcessingIncident(("Configuration elements must be in namespace "+Saxon.Hej.lib.NamespaceConstant.SAXON_CONFIGURATION));
                errors.Add(incident);
            }
            level++;
        }
        private void readGlobalElement(Saxon.Hej.om.AttributeMap atts) {
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
            foreach (Saxon.Hej.om.AttributeInfo a in atts) {
                string name = a.getNodeName().getLocalPart();
                string value = a.getValue();
                if ((!(value.Length==0) && a.getNodeName().getURI().Length==0)) {
                    props.setProperty(name, value);
                }
            }
            props.setProperty("#element", "global");
            applyProperty(props, "allowedProtocols", Saxon.Hej.lib.FeatureCode.ALLOWED_PROTOCOLS);
            applyProperty(props, "allowExternalFunctions", Saxon.Hej.lib.FeatureCode.ALLOW_EXTERNAL_FUNCTIONS);
            applyProperty(props, "allowMultiThreading", Saxon.Hej.lib.FeatureCode.ALLOW_MULTITHREADING);
            applyProperty(props, "allowOldJavaUriFormat", Saxon.Hej.lib.FeatureCode.ALLOW_OLD_JAVA_URI_FORMAT);
            applyProperty(props, "allowSyntaxExtensions", Saxon.Hej.lib.FeatureCode.ALLOW_SYNTAX_EXTENSIONS);
            applyProperty(props, "collationUriResolver", Saxon.Hej.lib.FeatureCode.COLLATION_URI_RESOLVER_CLASS);
            applyProperty(props, "collectionFinder", Saxon.Hej.lib.FeatureCode.COLLECTION_FINDER_CLASS);
            applyProperty(props, "compileWithTracing", Saxon.Hej.lib.FeatureCode.COMPILE_WITH_TRACING);
            applyProperty(props, "debugByteCode", Saxon.Hej.lib.FeatureCode.DEBUG_BYTE_CODE);
            applyProperty(props, "debugByteCodeDirectory", Saxon.Hej.lib.FeatureCode.DEBUG_BYTE_CODE_DIR);
            applyProperty(props, "defaultCollation", Saxon.Hej.lib.FeatureCode.DEFAULT_COLLATION);
            applyProperty(props, "defaultCollection", Saxon.Hej.lib.FeatureCode.DEFAULT_COLLECTION);
            applyProperty(props, "defaultRegexEngine", Saxon.Hej.lib.FeatureCode.DEFAULT_REGEX_ENGINE);
            applyProperty(props, "displayByteCode", Saxon.Hej.lib.FeatureCode.DISPLAY_BYTE_CODE);
            applyProperty(props, "dtdValidation", Saxon.Hej.lib.FeatureCode.DTD_VALIDATION);
            applyProperty(props, "dtdValidationRecoverable", Saxon.Hej.lib.FeatureCode.DTD_VALIDATION_RECOVERABLE);
            applyProperty(props, "eagerEvaluation", Saxon.Hej.lib.FeatureCode.EAGER_EVALUATION);
            applyProperty(props, "entityResolver", Saxon.Hej.lib.FeatureCode.ENTITY_RESOLVER_CLASS);
            applyProperty(props, "errorListener", Saxon.Hej.lib.FeatureCode.ERROR_LISTENER_CLASS);
            applyProperty(props, "environmentVariableResolver", Saxon.Hej.lib.FeatureCode.ENVIRONMENT_VARIABLE_RESOLVER_CLASS);
            applyProperty(props, "expandAttributeDefaults", Saxon.Hej.lib.FeatureCode.EXPAND_ATTRIBUTE_DEFAULTS);
            applyProperty(props, "generateByteCode", Saxon.Hej.lib.FeatureCode.GENERATE_BYTE_CODE);
            applyProperty(props, "ignoreSAXSourceParser", Saxon.Hej.lib.FeatureCode.IGNORE_SAX_SOURCE_PARSER);
            applyProperty(props, "lineNumbering", Saxon.Hej.lib.FeatureCode.LINE_NUMBERING);
            applyProperty(props, "markDefaultedAttributes", Saxon.Hej.lib.FeatureCode.MARK_DEFAULTED_ATTRIBUTES);
            applyProperty(props, "maxCompiledClasses", Saxon.Hej.lib.FeatureCode.MAX_COMPILED_CLASSES);
            applyProperty(props, "monitorHotSpotByteCode", Saxon.Hej.lib.FeatureCode.MONITOR_HOT_SPOT_BYTE_CODE);
            applyProperty(props, "optimizationLevel", Saxon.Hej.lib.FeatureCode.OPTIMIZATION_LEVEL);
            applyProperty(props, "parser", Saxon.Hej.lib.FeatureCode.SOURCE_PARSER_CLASS);
            applyProperty(props, "preEvaluateDoc", Saxon.Hej.lib.FeatureCode.PRE_EVALUATE_DOC_FUNCTION);
            applyProperty(props, "recognizeUriQueryParameters", Saxon.Hej.lib.FeatureCode.RECOGNIZE_URI_QUERY_PARAMETERS);
            applyProperty(props, "retainNodeForDiagnostics", Saxon.Hej.lib.FeatureCode.RETAIN_NODE_FOR_DIAGNOSTICS);
            applyProperty(props, "schemaValidation", Saxon.Hej.lib.FeatureCode.SCHEMA_VALIDATION_MODE);
            applyProperty(props, "serializerFactory", Saxon.Hej.lib.FeatureCode.SERIALIZER_FACTORY_CLASS);
            applyProperty(props, "sourceResolver", Saxon.Hej.lib.FeatureCode.SOURCE_RESOLVER_CLASS);
            applyProperty(props, "stableCollectionUri", Saxon.Hej.lib.FeatureCode.STABLE_COLLECTION_URI);
            applyProperty(props, "stableUnparsedText", Saxon.Hej.lib.FeatureCode.STABLE_UNPARSED_TEXT);
            applyProperty(props, "standardErrorOutputFile", Saxon.Hej.lib.FeatureCode.STANDARD_ERROR_OUTPUT_FILE);
            applyProperty(props, "streamability", Saxon.Hej.lib.FeatureCode.STREAMABILITY);
            applyProperty(props, "streamingFallback", Saxon.Hej.lib.FeatureCode.STREAMING_FALLBACK);
            applyProperty(props, "stripSpace", Saxon.Hej.lib.FeatureCode.STRIP_WHITESPACE);
            applyProperty(props, "styleParser", Saxon.Hej.lib.FeatureCode.STYLE_PARSER_CLASS);
            applyProperty(props, "suppressEvaluationExpiryWarning", Saxon.Hej.lib.FeatureCode.SUPPRESS_EVALUATION_EXPIRY_WARNING);
            applyProperty(props, "suppressXPathWarnings", Saxon.Hej.lib.FeatureCode.SUPPRESS_XPATH_WARNINGS);
            applyProperty(props, "suppressXsltNamespaceCheck", Saxon.Hej.lib.FeatureCode.SUPPRESS_XSLT_NAMESPACE_CHECK);
            applyProperty(props, "thresholdForHotspotByteCode", Saxon.Hej.lib.FeatureCode.THRESHOLD_FOR_HOTSPOT_BYTE_CODE);
            applyProperty(props, "timing", Saxon.Hej.lib.FeatureCode.TIMING);
            applyProperty(props, "traceExternalFunctions", Saxon.Hej.lib.FeatureCode.TRACE_EXTERNAL_FUNCTIONS);
            applyProperty(props, "traceListener", Saxon.Hej.lib.FeatureCode.TRACE_LISTENER_CLASS);
            applyProperty(props, "traceListenerOutputFile", Saxon.Hej.lib.FeatureCode.TRACE_LISTENER_OUTPUT_FILE);
            applyProperty(props, "traceOptimizerDecisions", Saxon.Hej.lib.FeatureCode.TRACE_OPTIMIZER_DECISIONS);
            applyProperty(props, "treeModel", Saxon.Hej.lib.FeatureCode.TREE_MODEL_NAME);
            applyProperty(props, "unparsedTextUriResolver", Saxon.Hej.lib.FeatureCode.UNPARSED_TEXT_URI_RESOLVER_CLASS);
            applyProperty(props, "uriResolver", Saxon.Hej.lib.FeatureCode.URI_RESOLVER_CLASS);
            applyProperty(props, "usePiDisableOutputEscaping", Saxon.Hej.lib.FeatureCode.USE_PI_DISABLE_OUTPUT_ESCAPING);
            applyProperty(props, "useTypedValueCache", Saxon.Hej.lib.FeatureCode.USE_TYPED_VALUE_CACHE);
            applyProperty(props, "validationComments", Saxon.Hej.lib.FeatureCode.VALIDATION_COMMENTS);
            applyProperty(props, "validationWarnings", Saxon.Hej.lib.FeatureCode.VALIDATION_WARNINGS);
            applyProperty(props, "versionOfXml", Saxon.Hej.lib.FeatureCode.XML_VERSION);
            applyProperty(props, "xInclude", Saxon.Hej.lib.FeatureCode.XINCLUDE);
            applyProperty(props, "zipUriPattern", Saxon.Hej.lib.FeatureCode.ZIP_URI_PATTERN);
        }
        private void applyProperty(Saxon.Ejava.util.Properties props, string attributeName, int featureCode) {
            string value = props.getProperty(attributeName);
            if ((value != null)) {
                try {
                    targetConfig.setConfigurationProperty(Saxon.Hej.lib.FeatureIndex.getData(featureCode).uri, value);
                } catch (Saxon.Ejava.lang.IllegalArgumentException e) {
                    string message = e.Message;
                    if (message.StartsWith(attributeName)) {
                        message = message.Replace(attributeName, "Value");
                    }
                    if (message.StartsWith("Unknown configuration property")) {
                        message = ((("Property "+attributeName)+" not available in Saxon-")+targetConfig.getEditionCode());
                    }
                    error(props.getProperty("#element"), attributeName, value, message);
                }
            }
        }
        private void readSerializationElement(Saxon.Hej.om.AttributeMap atts, Saxon.Hej.om.NamespaceMap nsMap) {
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
            foreach (Saxon.Hej.om.AttributeInfo a in atts) {
                string uri = a.getNodeName().getURI();
                string name = a.getNodeName().getLocalPart();
                string value = a.getValue();
                if (value.Length==0) {
                    continue ;
                }
                try {
                    Saxon.Hej.expr.instruct.ResultDocument.setSerializationProperty(props, uri, name, value, nsMap, false, targetConfig);
                } catch (Saxon.Hej.trans.XPathException e) {
                    errors.Add(new Saxon.Hej.trans.XmlProcessingException(e));
                }
            }
            targetConfig.setDefaultSerializationProperties(props);
        }
        private void readCollation(Saxon.Hej.om.AttributeMap atts) {
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
            string collationUri = null;
            foreach (Saxon.Hej.om.AttributeInfo a in atts) {
                string uri = a.getNodeName().getURI();
                string name = a.getNodeName().getLocalPart();
                string value = a.getValue();
                if (uri.Length==0) {
                    if (value.Length==0) {
                        continue ;
                    }
                    if ("uri".Equals(name)) {
                        collationUri = value;
                    } else {
                        props.setProperty(name, value);
                    }
                }
            }
            if ((collationUri == null)) {
                errors.Add(new Saxon.Hej.trans.XmlProcessingIncident("collation specified with no uri"));
            }
            Saxon.Hej.lib.StringCollator collator = null;
            try {
                collator = Saxon.Hej.Version.platform.makeCollation(targetConfig, props, collationUri);
            } catch (Saxon.Hej.trans.XPathException e) {
                errors.Add(new Saxon.Hej.trans.XmlProcessingIncident(e.Message));
            }
            targetConfig.registerCollation(collationUri, collator);
        }
        private void readLocalizationsElement(Saxon.Hej.om.AttributeMap atts) {
            foreach (Saxon.Hej.om.AttributeInfo a in atts) {
                string uri = a.getNodeName().getURI();
                string name = a.getNodeName().getLocalPart();
                string value = a.getValue();
                if (uri.Length==0) {
                    if (("defaultLanguage".Equals(name) && !(value.Length==0))) {
                        targetConfig.setConfigurationProperty(Saxon.Hej.lib.FeatureKeys.DEFAULT_LANGUAGE, value);
                    }
                    if (("defaultCountry".Equals(name) && !(value.Length==0))) {
                        targetConfig.setConfigurationProperty(Saxon.Hej.lib.FeatureKeys.DEFAULT_COUNTRY, value);
                    }
                }
            }
        }
        private void readLocalization(Saxon.Hej.om.AttributeMap atts) {
            string lang = null;
            Saxon.Ejava.util.Properties properties = new Saxon.Ejava.util.Properties();
            foreach (Saxon.Hej.om.AttributeInfo a in atts) {
                string uri = a.getNodeName().getURI();
                string name = a.getNodeName().getLocalPart();
                string value = a.getValue();
                if (uri.Length==0) {
                    if (("lang".Equals(name) && !(value.Length==0))) {
                        lang = value;
                    } else {
                        if (!(value.Length==0)) {
                            properties.setProperty(name, value);
                        }
                    }
                }
            }
            if ((lang != null)) {
                Saxon.Hej.lib.LocalizerFactory factory = targetConfig.getLocalizerFactory();
                if ((factory != null)) {
                    factory.setLanguageProperties(lang, properties);
                }
            }
        }
        private void readFileExtension(Saxon.Hej.om.AttributeMap atts) {
            string extension = atts.getValue("extension");
            string mediaType = atts.getValue("mediaType");
            if ((extension == null)) {
                error("fileExtension", "extension", null, null);
            }
            if ((mediaType == null)) {
                error("fileExtension", "mediaType", null, null);
            }
            targetConfig.registerFileExtension(extension, mediaType);
        }
        protected internal virtual void readExtensionElement(Saxon.Hej.om.AttributeMap atts) {
            Saxon.Hej.trans.XmlProcessingIncident err = new Saxon.Hej.trans.XmlProcessingIncident("Extension elements are not available in Saxon-HE");
            errors.Add(err);
        }
        protected internal void readXsltPackage(Saxon.Hej.om.AttributeMap atts) {
            string name = atts.getValue("name");
            if ((name == null)) {
                string attName = "exportLocation";
                string location = atts.getValue("exportLocation");
                System.Uri uri = null;
                if ((location == null)) {
                    attName = "sourceLocation";
                    location = atts.getValue("sourceLocation");
                }
                if ((location == null)) {
                    error("package", attName, null, null);
                }
                try {
                    uri = Saxon.Hej.functions.ResolveURI.makeAbsolute(location, getSystemId());
                } catch (System.UriFormatException e) {
                    error("package", attName, location, "Requires a valid URI.");
                }
                Saxon.Ejava.io.File file = new Saxon.Ejava.io.File(uri);
                try {
                    packageLibrary.addPackage(file);
                } catch (Saxon.Hej.trans.XPathException e) {
                    error(e);
                }
            } else {
                string version = atts.getValue("version");
                if ((version == null)) {
                    version = "1";
                }
                Saxon.Hej.trans.packages.VersionedPackageName vpn = null;
                Saxon.Hej.trans.packages.PackageDetails details = new Saxon.Hej.trans.packages.PackageDetails();
                try {
                    vpn = new Saxon.Hej.trans.packages.VersionedPackageName(name, version);
                } catch (Saxon.Hej.trans.XPathException err) {
                    error("package", "version", version, null);
                }
                details.nameAndVersion = vpn;
                currentPackage = details;
                string sourceLoc = atts.getValue("sourceLocation");
                Saxon.Ejavax.xml.transform.stream.StreamSource source = null;
                if ((sourceLoc != null)) {
                    try {
                        source = new Saxon.Ejavax.xml.transform.stream.StreamSource(Saxon.Hej.functions.ResolveURI.makeAbsolute(sourceLoc, getSystemId()).ToString());
                    } catch (System.UriFormatException e) {
                        error("package", "sourceLocation", sourceLoc, "Requires a valid URI.");
                    }
                    details.sourceLocation = source;
                }
                string exportLoc = atts.getValue("exportLocation");
                if ((exportLoc != null)) {
                    try {
                        source = new Saxon.Ejavax.xml.transform.stream.StreamSource(Saxon.Hej.functions.ResolveURI.makeAbsolute(exportLoc, getSystemId()).ToString());
                    } catch (System.UriFormatException e) {
                        error("package", "exportLocation", exportLoc, "Requires a valid URI.");
                    }
                    details.exportLocation = source;
                }
                string priority = atts.getValue("priority");
                if ((priority != null)) {
                    try {
                        details.priority = System.Int32.Parse(priority);
                    } catch (Saxon.Ejava.lang.NumberFormatException err) {
                        error("package", "priority", priority, "Requires an integer.");
                    }
                }
                details.baseName = atts.getValue("base");
                details.shortName = atts.getValue("shortName");
                packageLibrary.addPackage(details);
            }
        }
        protected internal void readWithParam(Saxon.Hej.om.AttributeMap atts, Saxon.Hej.om.NamespaceMap nsMap) {
            if ((currentPackage.exportLocation != null)) {
                error("withParam", null, null, "Not allowed when @exportLocation exists");
            }
            string name = atts.getValue("name");
            if ((name == null)) {
                error("withParam", "name", null, null);
            }
            Saxon.Hej.om.QNameParser qp = new Saxon.Hej.om.QNameParser(nsMap).withAcceptEQName(true);
            Saxon.Hej.om.StructuredQName qName = null;
            try {
                qName = qp.parse(name, "");
            } catch (Saxon.Hej.trans.XPathException e) {
                error("withParam", "name", name, "Requires valid QName");
            }
            string select = atts.getValue("select");
            if ((select == null)) {
                error("withParam", "select", null, null);
            }
            Saxon.Hej.sxpath.IndependentContext env = new Saxon.Hej.sxpath.IndependentContext(targetConfig);
            env.setNamespaceResolver(nsMap);
            Saxon.Hej.expr.parser.XPathParser parser = new Saxon.Hej.expr.parser.XPathParser();
            Saxon.Hej.om.GroundedValue value = null;
            try {
                Saxon.Hej.expr.Expression exp = parser.parse(select, 0, Saxon.Hej.expr.parser.Token.EOF, env);
                value = Saxon.Hej.om.SequenceTool.toGroundedValue(exp.iterate(env.makeEarlyEvaluationContext()));
            } catch (Saxon.Hej.trans.XPathException e) {
                error(e);
            }
            if ((currentPackage.staticParams == null)) {
                currentPackage.staticParams = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.om.GroundedValue>();
            }
            Saxon.Impl.Helpers.MapUtils.Put(currentPackage.staticParams, qName, value);
        }
        private void readXQueryElement(Saxon.Hej.om.AttributeMap atts) {
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
            foreach (Saxon.Hej.om.AttributeInfo a in atts) {
                string uri = a.getNodeName().getURI();
                string name = a.getNodeName().getLocalPart();
                string value = a.getValue();
                if ((!(value.Length==0) && uri.Length==0)) {
                    props.setProperty(name, value);
                }
            }
            props.setProperty("#element", "xquery");
            applyProperty(props, "allowUpdate", Saxon.Hej.lib.FeatureCode.XQUERY_ALLOW_UPDATE);
            applyProperty(props, "constructionMode", Saxon.Hej.lib.FeatureCode.XQUERY_CONSTRUCTION_MODE);
            applyProperty(props, "defaultElementNamespace", Saxon.Hej.lib.FeatureCode.XQUERY_DEFAULT_ELEMENT_NAMESPACE);
            applyProperty(props, "defaultFunctionNamespace", Saxon.Hej.lib.FeatureCode.XQUERY_DEFAULT_FUNCTION_NAMESPACE);
            applyProperty(props, "emptyLeast", Saxon.Hej.lib.FeatureCode.XQUERY_EMPTY_LEAST);
            applyProperty(props, "inheritNamespaces", Saxon.Hej.lib.FeatureCode.XQUERY_INHERIT_NAMESPACES);
            applyProperty(props, "moduleUriResolver", Saxon.Hej.lib.FeatureCode.MODULE_URI_RESOLVER_CLASS);
            applyProperty(props, "preserveBoundarySpace", Saxon.Hej.lib.FeatureCode.XQUERY_PRESERVE_BOUNDARY_SPACE);
            applyProperty(props, "preserveNamespaces", Saxon.Hej.lib.FeatureCode.XQUERY_PRESERVE_NAMESPACES);
            applyProperty(props, "requiredContextItemType", Saxon.Hej.lib.FeatureCode.XQUERY_REQUIRED_CONTEXT_ITEM_TYPE);
            applyProperty(props, "schemaAware", Saxon.Hej.lib.FeatureCode.XQUERY_SCHEMA_AWARE);
            applyProperty(props, "staticErrorListener", Saxon.Hej.lib.FeatureCode.XQUERY_STATIC_ERROR_LISTENER_CLASS);
            applyProperty(props, "version", Saxon.Hej.lib.FeatureCode.XQUERY_VERSION);
        }
        private void readXsltElement(Saxon.Hej.om.AttributeMap atts) {
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
            foreach (Saxon.Hej.om.AttributeInfo a in atts) {
                string uri = a.getNodeName().getURI();
                string name = a.getNodeName().getLocalPart();
                string value = a.getValue();
                if ((!(value.Length==0) && uri.Length==0)) {
                    props.setProperty(name, value);
                }
            }
            props.setProperty("#element", "xslt");
            applyProperty(props, "disableXslEvaluate", Saxon.Hej.lib.FeatureCode.DISABLE_XSL_EVALUATE);
            applyProperty(props, "enableAssertions", Saxon.Hej.lib.FeatureCode.XSLT_ENABLE_ASSERTIONS);
            applyProperty(props, "initialMode", Saxon.Hej.lib.FeatureCode.XSLT_INITIAL_MODE);
            applyProperty(props, "initialTemplate", Saxon.Hej.lib.FeatureCode.XSLT_INITIAL_TEMPLATE);
            applyProperty(props, "messageEmitter", Saxon.Hej.lib.FeatureCode.MESSAGE_EMITTER_CLASS);
            applyProperty(props, "outputUriResolver", Saxon.Hej.lib.FeatureCode.OUTPUT_URI_RESOLVER_CLASS);
            applyProperty(props, "recoveryPolicy", Saxon.Hej.lib.FeatureCode.RECOVERY_POLICY_NAME);
            applyProperty(props, "resultDocumentThreads", Saxon.Hej.lib.FeatureCode.RESULT_DOCUMENT_THREADS);
            applyProperty(props, "schemaAware", Saxon.Hej.lib.FeatureCode.XSLT_SCHEMA_AWARE);
            applyProperty(props, "staticErrorListener", Saxon.Hej.lib.FeatureCode.XSLT_STATIC_ERROR_LISTENER_CLASS);
            applyProperty(props, "staticUriResolver", Saxon.Hej.lib.FeatureCode.XSLT_STATIC_URI_RESOLVER_CLASS);
            applyProperty(props, "strictStreamability", Saxon.Hej.lib.FeatureCode.STRICT_STREAMABILITY);
            applyProperty(props, "styleParser", Saxon.Hej.lib.FeatureCode.STYLE_PARSER_CLASS);
            applyProperty(props, "version", Saxon.Hej.lib.FeatureCode.XSLT_VERSION);
        }
        private void readXsdElement(Saxon.Hej.om.AttributeMap atts) {
            Saxon.Ejava.util.Properties props = new Saxon.Ejava.util.Properties();
            foreach (Saxon.Hej.om.AttributeInfo a in atts) {
                string uri = a.getNodeName().getURI();
                string name = a.getNodeName().getLocalPart();
                string value = a.getValue();
                if ((!(value.Length==0) && uri.Length==0)) {
                    props.setProperty(name, value);
                }
            }
            props.setProperty("#element", "xsd");
            applyProperty(props, "assertionsCanSeeComments", Saxon.Hej.lib.FeatureCode.ASSERTIONS_CAN_SEE_COMMENTS);
            applyProperty(props, "implicitSchemaImports", Saxon.Hej.lib.FeatureCode.IMPLICIT_SCHEMA_IMPORTS);
            applyProperty(props, "multipleSchemaImports", Saxon.Hej.lib.FeatureCode.MULTIPLE_SCHEMA_IMPORTS);
            applyProperty(props, "occurrenceLimits", Saxon.Hej.lib.FeatureCode.OCCURRENCE_LIMITS);
            applyProperty(props, "schemaUriResolver", Saxon.Hej.lib.FeatureCode.SCHEMA_URI_RESOLVER_CLASS);
            applyProperty(props, "thresholdForCompilingTypes", Saxon.Hej.lib.FeatureCode.THRESHOLD_FOR_COMPILING_TYPES);
            applyProperty(props, "useXsiSchemaLocation", Saxon.Hej.lib.FeatureCode.USE_XSI_SCHEMA_LOCATION);
            applyProperty(props, "version", Saxon.Hej.lib.FeatureCode.XSD_VERSION);
        }
        private void error(string element, string attribute, string actual, string required) {
            Saxon.Hej.trans.XmlProcessingIncident err;
            if ((attribute == null)) {
                err = new Saxon.Hej.trans.XmlProcessingIncident(("Invalid configuration element "+element));
            } else {
                if ((actual == null)) {
                    err = new Saxon.Hej.trans.XmlProcessingIncident(((("Missing configuration property "+element)+"/@")+attribute));
                } else {
                    err = new Saxon.Hej.trans.XmlProcessingIncident((((((((("Invalid configuration property "+element)+"/@")+attribute)+". Supplied value: '")+actual)+"'; required: '")+required)+"'"));
                }
            }
            errors.Add(err);
        }
        protected internal void error(Saxon.Hej.trans.XPathException err) {
            errors.Add(new Saxon.Hej.trans.XmlProcessingException(err));
        }
        protected internal void errorClass(string element, string attribute, string actual, System.Type required, System.Exception cause) {
            Saxon.Hej.trans.XmlProcessingIncident err = new Saxon.Hej.trans.XmlProcessingIncident(((((((("Invalid configuration property "+element)+(((attribute == null) ? "" : ("/@"+attribute))))+". Supplied value '")+actual)+"', required value is the name of a class that implements '")+required.Name)+"'"));
            err.setCause(cause);
            errors.Add(err);
        }
        public void endElement() {
            string localName = localNameStack.Pop();
            if (((level == 3) && "resources".Equals(section))) {
                string content = buffer.ToString();
                if (!(content.Length==0)) {
                    if ("externalObjectModel".Equals(localName)) {
                    } else {
                        if ("extensionFunction".Equals(localName)) {
                            try {
                                Saxon.Hej.lib.ExtensionFunctionDefinition model = (Saxon.Hej.lib.ExtensionFunctionDefinition)(targetConfig.getInstance(content));
                                targetConfig.registerExtensionFunction(model);
                            } catch (Exception e) when (e is Saxon.Hej.trans.XPathException || e is System.InvalidCastException || e is Saxon.Ejava.lang.IllegalArgumentException) {
                                errorClass("extensionFunction", null, content, typeof(Saxon.Hej.lib.ExtensionFunctionDefinition), e);
                            }
                        } else {
                            if ("schemaDocument".Equals(localName)) {
                                try {
                                    Saxon.Ejavax.xml.transform.Source source = getInputSource(content);
                                    targetConfig.addSchemaSource(source);
                                } catch (Saxon.Hej.trans.XPathException e) {
                                    errors.Add(new Saxon.Hej.trans.XmlProcessingException(e));
                                }
                            } else {
                                if ("schemaComponentModel".Equals(localName)) {
                                    try {
                                        Saxon.Ejavax.xml.transform.Source source = getInputSource(content);
                                        targetConfig.importComponents(source);
                                    } catch (Saxon.Hej.trans.XPathException e) {
                                        errors.Add(new Saxon.Hej.trans.XmlProcessingException(e));
                                    }
                                } else {
                                    if ("fileExtension".Equals(localName)) {
                                    } else {
                                        error(localName, null, null, null);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            level--;
            buffer.setLength(0);
        }
        private Saxon.Ejavax.xml.transform.Source getInputSource(string href) {
            try {
                string @base = getSystemId();
                System.Uri abs = Saxon.Hej.functions.ResolveURI.makeAbsolute(href, @base);
                return new Saxon.Ejavax.xml.transform.stream.StreamSource(abs.ToString());
            } catch (System.UriFormatException e) {
                throw new Saxon.Hej.trans.XPathException(e);
            }
        }
        public void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location location, int properties) {
            buffer.append(chars.ToString());
        }
    
        /* Expanding default method append from interface Receiver */
        public virtual void append(Saxon.Hej.om.Item item, Saxon.Hej.s9api.Location locationId, int properties) {
            throw new Saxon.Ejava.lang.UnsupportedOperationException();
        }

        /* Expanding default method append from interface Receiver */
        public virtual void append(Saxon.Hej.om.Item item) {
            append(item, Saxon.Hej.expr.parser.Loc.NONE, Saxon.Hej.@event.ReceiverOption.ALL_NAMESPACES);
        }

        /* Expanding default method usesTypeAnnotations from interface Receiver */
        public virtual bool usesTypeAnnotations() {
            return false;
        }

        /* Expanding default method handlesAppend from interface Receiver */
        public virtual bool handlesAppend() {
            return false;
        }

    }
}