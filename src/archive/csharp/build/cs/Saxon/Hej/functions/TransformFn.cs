using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.functions {
    public class TransformFn : Saxon.Hej.functions.SystemFunction, Saxon.Hej.expr.Callable {
        private static readonly string[] transformOptionNames30 = new string[]{"package-name", "package-version", "package-node", "package-location", "static-params", "global-context-item", "template-params", "tunnel-params", "initial-function", "function-params"};
        private const string dummyBaseOutputUriScheme = "dummy";
        private bool isTransformOptionName30(string str) {
            foreach (string s in transformOptionNames30) {
                if (s.Equals(str)) {
                    return true;
                }
            }
            return false;
        }
        public static Saxon.Hej.functions.OptionsParameter makeOptionsParameter() {
            Saxon.Hej.functions.OptionsParameter op = new Saxon.Hej.functions.OptionsParameter();
            op.addAllowedOption("xslt-version", Saxon.Hej.value.SequenceType.SINGLE_DECIMAL);
            op.addAllowedOption("stylesheet-location", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("stylesheet-node", Saxon.Hej.value.SequenceType.SINGLE_NODE);
            op.addAllowedOption("stylesheet-text", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("stylesheet-base-uri", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("base-output-uri", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("stylesheet-params", Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
            op.addAllowedOption("source-node", Saxon.Hej.value.SequenceType.SINGLE_NODE);
            op.addAllowedOption("source-location", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("initial-mode", Saxon.Hej.value.SequenceType.SINGLE_QNAME);
            op.addAllowedOption("initial-match-selection", Saxon.Hej.value.SequenceType.ANY_SEQUENCE);
            op.addAllowedOption("initial-template", Saxon.Hej.value.SequenceType.SINGLE_QNAME);
            op.addAllowedOption("delivery-format", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("serialization-params", Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
            op.addAllowedOption("vendor-options", Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
            op.addAllowedOption("cache", Saxon.Hej.value.SequenceType.SINGLE_BOOLEAN);
            op.addAllowedOption("package-name", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("package-version", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("package-node", Saxon.Hej.value.SequenceType.SINGLE_NODE);
            op.addAllowedOption("package-location", Saxon.Hej.value.SequenceType.SINGLE_STRING);
            op.addAllowedOption("static-params", Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
            op.addAllowedOption("global-context-item", Saxon.Hej.value.SequenceType.SINGLE_ITEM);
            op.addAllowedOption("template-params", Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
            op.addAllowedOption("tunnel-params", Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
            op.addAllowedOption("initial-function", Saxon.Hej.value.SequenceType.SINGLE_QNAME);
            op.addAllowedOption("function-params", Saxon.Hej.ma.arrays.ArrayItemType.SINGLE_ARRAY);
            op.addAllowedOption("requested-properties", Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.ma.map.MapType.ANY_MAP_TYPE, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
            op.addAllowedOption("post-process", Saxon.Hej.value.SequenceType.makeSequenceType(new Saxon.Hej.type.SpecificFunctionType(new Saxon.Hej.value.SequenceType[]{Saxon.Hej.value.SequenceType.SINGLE_STRING, Saxon.Hej.value.SequenceType.ANY_SEQUENCE}, Saxon.Hej.value.SequenceType.ANY_SEQUENCE), Saxon.Hej.expr.StaticProperty.EXACTLY_ONE));
            return op;
        }
        private void checkTransformOptions(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options, Saxon.Hej.expr.XPathContext context, int languageVersion) {
            if (options.Count==0) {
                throw new Saxon.Hej.trans.XPathException("No transformation options supplied", "FOXT0002");
            }
            foreach (string keyName in Saxon.Impl.Helpers.MapUtils.KeySet(options)) {
                if ((isTransformOptionName30(keyName) && (languageVersion<30))) {
                    throw new Saxon.Hej.trans.XPathException((("The transform option "+keyName)+" is only available when using an XSLT 3.0 processor"), "FOXT0002");
                }
            }
        }
        private string checkStylesheetMutualExclusion(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> map) {
            return exactlyOneOf(map, "stylesheet-location", "stylesheet-node", "stylesheet-text");
        }
        private string checkStylesheetMutualExclusion30(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> map) {
            string styleOption = exactlyOneOf(map, "stylesheet-location", "stylesheet-node", "stylesheet-text", "package-name", "package-node", "package-location");
            if (styleOption.Equals("package-location")) {
                throw new Saxon.Hej.trans.XPathException((("The transform option "+styleOption)+" is not implemented in Saxon"), "FOXT0002");
            }
            return styleOption;
        }
        private string checkInvocationMutualExclusion(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options) {
            return oneOf(options, "initial-mode", "initial-template");
        }
        private string oneOf(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> map, params string[]  keys) {
            string found = null;
            foreach (string s in keys) {
                if ((Saxon.Impl.Helpers.MapUtils.Get(map, s) != null)) {
                    if ((found != null)) {
                        throw new Saxon.Hej.trans.XPathException(("The following transform options are mutually exclusive: "+enumerate(keys)), "FOXT0002");
                    } else {
                        found = s;
                    }
                }
            }
            return found;
        }
        private string exactlyOneOf(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> map, params string[]  keys) {
            string found = oneOf(map, keys);
            if ((found == null)) {
                throw new Saxon.Hej.trans.XPathException(("One of the following transform options must be present: "+enumerate(keys)));
            }
            return found;
        }
        private string enumerate(params string[]  keys) {
            bool first = true;
            Saxon.Ejava.lang.StringBuilder buffer = new Saxon.Ejava.lang.StringBuilder(256);
            foreach (string k in keys) {
                if (first) {
                    first = false;
                } else {
                    buffer.append(" | ");
                }
                buffer.append(k);
            }
            return buffer.ToString();
        }
        private string checkInvocationMutualExclusion30(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> map) {
            return oneOf(map, "initial-mode", "initial-template", "initial-function");
        }
        private void unsuitable(string option, string value) {
            throw new Saxon.Hej.trans.XPathException(((("No XSLT processor is available with xsl:"+option)+" = ")+value), "FOXT0001");
        }
        private bool asBoolean(Saxon.Hej.value.AtomicValue value) {
            if ((value is Saxon.Hej.value.BooleanValue)) {
                return ((Saxon.Hej.value.BooleanValue)(value)).getBooleanValue();
            } else {
                if ((value is Saxon.Hej.value.StringValue)) {
                    string s = Saxon.Hej.value.Whitespace.normalizeWhitespace(value.getUnicodeStringValue()).ToString();
                    if (((s.Equals("yes") || s.Equals("true")) || s.Equals("1"))) {
                        return true;
                    } else {
                        if (((s.Equals("no") || s.Equals("false")) || s.Equals("0"))) {
                            return false;
                        }
                    }
                }
            }
            throw new Saxon.Hej.trans.XPathException(("Unrecognized boolean value "+value), "FOXT0002");
        }
        private void setRequestedProperties(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options, Saxon.Hej.s9api.Processor processor) {
            Saxon.Hej.ma.map.MapItem requestedProps = (Saxon.Hej.ma.map.MapItem)(Saxon.Impl.Helpers.MapUtils.Get(options, "requested-properties").head());
            Saxon.Hej.tree.iter.AtomicIterator optionIterator = requestedProps.keys();
            while (true) {
                Saxon.Hej.value.AtomicValue option = ((/*1430*/Saxon.Hej.value.AtomicValue)optionIterator.next());
                if ((option != null)) {
                    Saxon.Hej.om.StructuredQName optionName = ((Saxon.Hej.value.QNameValue)(((/*1430*/Saxon.Hej.value.AtomicValue)option.head()))).getStructuredQName();
                    Saxon.Hej.value.AtomicValue value = (Saxon.Hej.value.AtomicValue)(requestedProps.get(option).head());
                    if (optionName.hasURI(Saxon.Hej.om.NamespaceUri.XSLT)) {
                        string localName = optionName.getLocalPart();
                        string val = value.getStringValue();
                        switch (localName) {
                          case "vendor-url":
                            if (!(((val.Contains("saxonica.com") || value.getStringValue().Equals("Saxonica"))))) {
                                unsuitable("vendor-url", val);
                            }
                            break ;
                          case "product-name":
                            if (!(val.Equals("SAXON"))) {
                                unsuitable("vendor-url", val);
                            }
                            break ;
                          case "product-version":
                            if (!(Saxon.Hej.Version.getProductVersion().StartsWith(val))) {
                                unsuitable("product-version", val);
                            }
                            break ;
                          case "is-schema-aware":
                            {
                                bool b = asBoolean(value);
                                if (b) {
                                    if (processor.getUnderlyingConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT)) {
                                        processor.setConfigurationProperty(Saxon.Hej.lib.Feature<bool>.XSLT_SCHEMA_AWARE, true);
                                    } else {
                                        unsuitable("is-schema-aware", val);
                                    }
                                } else {
                                    if (processor.getUnderlyingConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT)) {
                                        unsuitable("is-schema-aware", val);
                                    }
                                }
                                break ;
                            }
                          case "supports-serialization":
                            {
                                bool b = asBoolean(value);
                                if (!(b)) {
                                    unsuitable("supports-serialization", val);
                                }
                                break ;
                            }
                          case "supports-backwards-compatibility":
                            {
                                bool b = asBoolean(value);
                                if (!(b)) {
                                    unsuitable("supports-backwards-compatibility", val);
                                }
                                break ;
                            }
                          case "supports-namespace-axis":
                            {
                                bool b = asBoolean(value);
                                if (!(b)) {
                                    unsuitable("supports-namespace-axis", val);
                                }
                                break ;
                            }
                          case "supports-streaming":
                            {
                                bool b = asBoolean(value);
                                if (b) {
                                    if (!(processor.getUnderlyingConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT))) {
                                        unsuitable("supports-streaming", val);
                                    }
                                } else {
                                    if (processor.getUnderlyingConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT)) {
                                        processor.setConfigurationProperty(Saxon.Hej.lib.Feature<string>.STREAMABILITY, "off");
                                    }
                                }
                                break ;
                            }
                          case "supports-dynamic-evaluation":
                            {
                                bool b = asBoolean(value);
                                if (!(b)) {
                                    processor.setConfigurationProperty(Saxon.Hej.lib.Feature<bool>.DISABLE_XSL_EVALUATE, true);
                                }
                                break ;
                            }
                          case "supports-higher-order-functions":
                            {
                                bool b = asBoolean(value);
                                if (!(b)) {
                                    unsuitable("supports-higher-order-functions", val);
                                }
                                break ;
                            }
                          case "xpath-version":
                            {
                                try {
                                    if ((Saxon.Impl.Helpers.Utils.ParseDouble(val)>3.1)) {
                                        unsuitable("xpath-version", val);
                                    }
                                } catch (Saxon.Ejava.lang.NumberFormatException nfe) {
                                    unsuitable("xpath-version", val);
                                }
                                break ;
                            }
                          case "xsd-version":
                            {
                                try {
                                    if ((Saxon.Impl.Helpers.Utils.ParseDouble(val)>1.1)) {
                                        unsuitable("xsd-version", val);
                                    }
                                } catch (Saxon.Ejava.lang.NumberFormatException nfe) {
                                    unsuitable("xsd-version", val);
                                }
                                break ;
                            }
                        }
                    }
                } else {
                    break ;
                }
            }
        }
        private void setStaticParams(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options, Saxon.Hej.s9api.XsltCompiler xsltCompiler, bool allowTypedNodes) {
            Saxon.Hej.ma.map.MapItem staticParamsMap = (Saxon.Hej.ma.map.MapItem)(Saxon.Impl.Helpers.MapUtils.Get(options, "static-params").head());
            Saxon.Hej.tree.iter.AtomicIterator paramIterator = staticParamsMap.keys();
            while (true) {
                Saxon.Hej.value.AtomicValue param = ((/*1430*/Saxon.Hej.value.AtomicValue)paramIterator.next());
                if ((param != null)) {
                    if (!(((param is Saxon.Hej.value.QNameValue)))) {
                        throw new Saxon.Hej.trans.XPathException("Parameter names in static-params must be supplied as QNames", "FOXT0002");
                    }
                    Saxon.Api.QName paramName = new Saxon.Api.QName(((Saxon.Hej.value.QNameValue)(param)).getStructuredQName());
                    Saxon.Hej.om.GroundedValue value = staticParamsMap.get(param);
                    if (!(allowTypedNodes)) {
                        checkSequenceIsUntyped(value);
                    }
                    Saxon.Api.XdmValue paramVal = Saxon.Api.XdmValue.Wrap(value);
                    xsltCompiler.setParameter(paramName, paramVal);
                } else {
                    break ;
                }
            }
        }
        private Saxon.Hej.s9api.XsltExecutable getStylesheet(System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options, Saxon.Hej.s9api.XsltCompiler xsltCompiler, string styleOptionStr, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.om.Item styleOptionItem = Saxon.Impl.Helpers.MapUtils.Get(options, styleOptionStr).head();
            System.Uri stylesheetBaseUri = null;
            Saxon.Hej.om.Sequence seq;
            if (((seq = Saxon.Impl.Helpers.MapUtils.Get(options, "stylesheet-base-uri")) != null)) {
                string styleBaseUri = seq.head().getStringValue();
                stylesheetBaseUri = new System.Uri(styleBaseUri, System.UriKind.RelativeOrAbsolute);
                if (!(stylesheetBaseUri.IsAbsoluteUri)) {
                    System.Uri staticBase = getRetainedStaticContext().getStaticBaseUri();
                    stylesheetBaseUri = new System.Uri(staticBase, styleBaseUri);
                }
            }
            System.Collections.Generic.IList<Saxon.Hej.s9api.XmlProcessingError> compileErrors = new System.Collections.Generic.List<Saxon.Hej.s9api.XmlProcessingError>();
            Saxon.Hej.lib.ErrorReporter originalReporter = xsltCompiler.getErrorReporter();
            xsltCompiler.setErrorReporter(new Saxon.Hej.functions.TransformFn.TransformErrorReporter(compileErrors, originalReporter));
            bool cacheable = (Saxon.Impl.Helpers.MapUtils.Get(options, "static-params") == null);
            if ((Saxon.Impl.Helpers.MapUtils.Get(options, "cache") != null)) {
                cacheable &= ((Saxon.Hej.value.BooleanValue)(Saxon.Impl.Helpers.MapUtils.Get(options, "cache").head())).getBooleanValue();
            }
            Saxon.Hej.trans.StylesheetCache cache = context.getController().getStylesheetCache();
            Saxon.Hej.s9api.XsltExecutable executable = null;
            Saxon.Hej.Configuration config = xsltCompiler.getProcessor().getUnderlyingConfiguration();
            switch (styleOptionStr) {
              case "stylesheet-location":
                string stylesheetLocation = styleOptionItem.getStringValue();
                if (cacheable) {
                    executable = cache.getStylesheetByLocation(stylesheetLocation);
                }
                if ((executable == null)) {
                    Saxon.Ejavax.xml.transform.Source style = null;
                    try {
                        string @base = getStaticBaseUriString();
                        Saxon.Hej.lib.JJ_ResourceRequest request = new Saxon.Hej.lib.JJ_ResourceRequest();
                        request.baseUri = @base;
                        request.relativeUri = stylesheetLocation;
                        request.uri = Saxon.Hej.functions.ResolveURI.makeAbsolute(stylesheetLocation, @base).ToString();
                        request.nature = Saxon.Hej.lib.JJ_ResourceRequest.XSLT_NATURE;
                        request.purpose = Saxon.Hej.lib.JJ_ResourceRequest.ANY_PURPOSE;
                        style = request.resolve(xsltCompiler.getResourceResolver(), config.getResourceResolver(), new Saxon.Callbacks.DirectResourceResolver(config));
                    } catch (System.UriFormatException e) {
                        throw new Saxon.Hej.trans.XPathException(("Failed to resolve stylesheet-location in fn:transform: "+e.Message));
                    } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                        throw new Saxon.Hej.trans.XPathException(e);
                    }
                    try {
                        executable = xsltCompiler.compile(style);
                    } catch (Saxon.Api.SaxonApiException e) {
                        return reportCompileError(e, compileErrors);
                    }
                    if (cacheable) {
                        cache.setStylesheetByLocation(stylesheetLocation, executable);
                    }
                }
                break ;
              case "stylesheet-node":
              case "package-node":
                Saxon.Hej.om.NodeInfo stylesheetNode = (Saxon.Hej.om.NodeInfo)(styleOptionItem);
                if (((stylesheetBaseUri != null) && !(stylesheetNode.getBaseURI().Equals(stylesheetBaseUri.OriginalString)))) {
                    string newBaseUri = stylesheetBaseUri.OriginalString;
                    Saxon.Hej.tree.wrapper.RebasedDocument rebased = new Saxon.Hej.tree.wrapper.RebasedDocument(stylesheetNode.getTreeInfo(), node => newBaseUri, node => newBaseUri);
                    stylesheetNode = rebased.getRootNode();
                }
                if (cacheable) {
                    executable = cache.getStylesheetByNode(stylesheetNode);
                }
                if ((executable == null)) {
                    Saxon.Ejavax.xml.transform.Source source = stylesheetNode.asActiveSource();
                    if ((stylesheetBaseUri != null)) {
                        source = Saxon.Hej.lib.AugmentedSource.makeAugmentedSource(source);
                        source.setSystemId(stylesheetBaseUri.OriginalString);
                    }
                    try {
                        executable = xsltCompiler.compile(source);
                    } catch (Saxon.Api.SaxonApiException e) {
                        reportCompileError(e, compileErrors);
                    }
                    if (cacheable) {
                        cache.setStylesheetByNode(stylesheetNode, executable);
                    }
                }
                break ;
              case "stylesheet-text":
                string stylesheetText = styleOptionItem.getStringValue();
                if (cacheable) {
                    executable = cache.getStylesheetByText(stylesheetText);
                }
                if ((executable == null)) {
                    System.IO.StringReader sr = new System.IO.StringReader(stylesheetText);
                    Saxon.Ejavax.xml.transform.stream.StreamSource style = new Saxon.Ejavax.xml.transform.stream.StreamSource(sr);
                    if ((stylesheetBaseUri != null)) {
                        style.setSystemId(stylesheetBaseUri.OriginalString);
                    }
                    try {
                        executable = xsltCompiler.compile(style);
                    } catch (Saxon.Api.SaxonApiException e) {
                        reportCompileError(e, compileErrors);
                    }
                    if (cacheable) {
                        cache.setStylesheetByText(stylesheetText, executable);
                    }
                }
                break ;
              case "package-name":
                string packageName = Saxon.Hej.value.Whitespace.trim(styleOptionItem.getStringValue());
                string packageVersion = null;
                if ((Saxon.Impl.Helpers.MapUtils.Get(options, "package-version") != null)) {
                    packageVersion = Saxon.Impl.Helpers.MapUtils.Get(options, "package-version").head().getStringValue();
                }
                try {
                    Saxon.Hej.s9api.XsltPackage pack = xsltCompiler.obtainPackage(packageName, packageVersion);
                    if ((pack == null)) {
                        throw new Saxon.Hej.trans.XPathException(((("Cannot locate package "+packageName)+" version ")+packageVersion), "FOXT0002");
                    }
                    executable = pack.link();
                } catch (Saxon.Api.SaxonApiException e) {
                    if ((e.InnerException is Saxon.Hej.trans.XPathException)) {
                        throw (Saxon.Hej.trans.XPathException)(e.InnerException);
                    } else {
                        throw new Saxon.Hej.trans.XPathException(e);
                    }
                }
                break ;
            }
            return executable;
        }
        private class TransformErrorReporter : Saxon.Hej.lib.ErrorReporter {
            private readonly System.Collections.Generic.IList<Saxon.Hej.s9api.XmlProcessingError> compileErrors;
            private readonly Saxon.Hej.lib.ErrorReporter originalReporter;
            public TransformErrorReporter(System.Collections.Generic.IList<Saxon.Hej.s9api.XmlProcessingError> compileErrors, Saxon.Hej.lib.ErrorReporter originalReporter)  {
                this.compileErrors = compileErrors;
                this.originalReporter = originalReporter;
            }
            public void report(Saxon.Hej.s9api.XmlProcessingError error) {
                if (!(error.isWarning())) {
                    compileErrors.Add(error);
                }
                originalReporter.report(error);
            }
        
        }
        private Saxon.Hej.s9api.XsltExecutable reportCompileError(Saxon.Api.SaxonApiException e, System.Collections.Generic.IList<Saxon.Hej.s9api.XmlProcessingError> compileErrors) {
            foreach (Saxon.Hej.s9api.XmlProcessingError te in compileErrors) {
                Saxon.Hej.trans.XPathException xe = Saxon.Hej.trans.XPathException.fromXmlProcessingError(te);
                xe.maybeSetErrorCode("FOXT0002");
                throw xe;
            }
            if ((e.InnerException is Saxon.Hej.trans.XPathException)) {
                throw (Saxon.Hej.trans.XPathException)(e.InnerException);
            } else {
                throw new Saxon.Hej.trans.XPathException(e);
            }
        }
        public override Saxon.Hej.om.Sequence call(Saxon.Hej.expr.XPathContext context, Saxon.Hej.om.Sequence[] arguments) {
            System.Collections.Generic.IDictionary<string,Saxon.Hej.om.Sequence> options = getDetails().optionDetails.processSuppliedOptions((Saxon.Hej.ma.map.MapItem)(arguments[0].head()), context);
            Saxon.Hej.om.Sequence vendorOptionsValue = Saxon.Impl.Helpers.MapUtils.Get(options, "vendor-options");
            Saxon.Hej.ma.map.MapItem vendorOptions = ((vendorOptionsValue == null) ? null : (Saxon.Hej.ma.map.MapItem)(vendorOptionsValue.head()));
            Saxon.Hej.Configuration targetConfig = context.getConfiguration();
            bool allowTypedNodes = true;
            bool tracing = targetConfig.isTiming();
            int schemaValidation = Saxon.Hej.lib.Validation.DEFAULT;
            if ((vendorOptions != null)) {
                Saxon.Hej.om.Sequence optionValue = vendorOptions.get(new Saxon.Hej.value.QNameValue("", Saxon.Hej.om.NamespaceUri.SAXON, "configuration"));
                if ((optionValue != null)) {
                    Saxon.Hej.om.NodeInfo configFile = (Saxon.Hej.om.NodeInfo)(optionValue.head());
                    targetConfig = Saxon.Hej.Configuration.readConfiguration(configFile.asActiveSource(), targetConfig);
                    allowTypedNodes = false;
                    if (!(context.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_EXTERNAL_FUNCTIONS))) {
                        targetConfig.setBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_EXTERNAL_FUNCTIONS, false);
                    }
                }
                optionValue = vendorOptions.get(new Saxon.Hej.value.QNameValue("", Saxon.Hej.om.NamespaceUri.SAXON, "schema-validation"));
                if ((optionValue != null)) {
                    string valOption = optionValue.head().getStringValue();
                    schemaValidation = Saxon.Hej.lib.Validation.getCode(valOption);
                }
            }
            Saxon.Hej.s9api.Processor processor = new Saxon.Hej.s9api.Processor(true);
            processor.setConfigurationProperty(Saxon.Hej.lib.Feature<Saxon.Hej.Configuration>.CONFIGURATION, targetConfig);
            int languageVersion = getRetainedStaticContext().getPackageData().getHostLanguageVersion();
            checkTransformOptions(options, context, languageVersion);
            if ((Saxon.Impl.Helpers.MapUtils.Get(options, "xslt-version") != null)) {
                double targetVersion = ((Saxon.Hej.value.BigDecimalValue)(Saxon.Impl.Helpers.MapUtils.Get(options, "xslt-version").head())).getDoubleValue();
                if (((targetVersion*10)>languageVersion)) {
                    throw new Saxon.Hej.trans.XPathException("The transform option xslt-version is higher than the language version supported by this processor", "FOXT0002");
                }
            }
            string principalInput = oneOf(options, "source-node", "source-location", "initial-match-selection");
            string invocationOption;
            string invocationName = "invocation";
            string styleOption;
            invocationOption = checkInvocationMutualExclusion30(options);
            if ((invocationOption != null)) {
                invocationName = invocationOption;
            }
            if (((!(invocationName.Equals("initial-template")) && !(invocationName.Equals("initial-function"))) && (principalInput == null))) {
                invocationName = "initial-template";
                Saxon.Impl.Helpers.MapUtils.Put(options, "initial-template", new Saxon.Hej.value.QNameValue("", Saxon.Hej.om.NamespaceUri.XSLT, "initial-template"));
            }
            if ((invocationName.Equals("initial-function") && (Saxon.Impl.Helpers.MapUtils.Get(options, "function-params") == null))) {
                throw new Saxon.Hej.trans.XPathException("Use of the transform option initial-function requires the function parameters to be supplied using the option function-params", "FOXT0002");
            }
            if ((!(invocationName.Equals("initial-function")) && (Saxon.Impl.Helpers.MapUtils.Get(options, "function-params") != null))) {
                throw new Saxon.Hej.trans.XPathException("The transform option function-params can only be used if the option initial-function is also used", "FOXT0002");
            }
            styleOption = checkStylesheetMutualExclusion30(options);
            if ((Saxon.Impl.Helpers.MapUtils.Get(options, "requested-properties") != null)) {
                setRequestedProperties(options, processor);
            }
            Saxon.Hej.s9api.XsltCompiler xsltCompiler = processor.newXsltCompiler();
            xsltCompiler.setResourceResolver(context.getResourceResolver());
            xsltCompiler.setJustInTimeCompilation(false);
            xsltCompiler.setErrorReporter(context.getErrorReporter());
            if ((Saxon.Impl.Helpers.MapUtils.Get(options, "static-params") != null)) {
                setStaticParams(options, xsltCompiler, allowTypedNodes);
            }
            Saxon.Hej.s9api.XsltExecutable sheet = getStylesheet(options, xsltCompiler, styleOption, context);
            Saxon.Hej.s9api.Xslt30Transformer transformer = sheet.load30();
            transformer.setErrorReporter(context.getErrorReporter());
            string deliveryFormat = "document";
            Saxon.Hej.om.NodeInfo sourceNode = null;
            string sourceLocation = null;
            Saxon.Api.XdmValue initialMatchSelection = null;
            Saxon.Api.QName initialTemplate = null;
            Saxon.Api.QName initialMode = null;
            string baseOutputUri = null;
            System.Collections.Generic.IDictionary<Saxon.Api.QName,Saxon.Api.XdmValue> stylesheetParams = new System.Collections.Generic.Dictionary<Saxon.Api.QName, Saxon.Api.XdmValue>();
            Saxon.Hej.ma.map.MapItem serializationParamsMap = null;
            Saxon.Api.XdmItem globalContextItem = null;
            System.Collections.Generic.IDictionary<Saxon.Api.QName,Saxon.Api.XdmValue> templateParams = new System.Collections.Generic.Dictionary<Saxon.Api.QName, Saxon.Api.XdmValue>();
            System.Collections.Generic.IDictionary<Saxon.Api.QName,Saxon.Api.XdmValue> tunnelParams = new System.Collections.Generic.Dictionary<Saxon.Api.QName, Saxon.Api.XdmValue>();
            Saxon.Api.QName initialFunction = null;
            Saxon.Api.XdmValue[] functionParams = null;
            Saxon.Hej.om.FunctionItem postProcessor = null;
            string principalResultKey = "output";
            foreach (string name in Saxon.Impl.Helpers.MapUtils.KeySet(options)) {
                Saxon.Hej.om.Sequence value = Saxon.Impl.Helpers.MapUtils.Get(options, name);
                Saxon.Hej.om.Item head = value.head();
                switch (name) {
                  case "source-node":
                    sourceNode = (Saxon.Hej.om.NodeInfo)(head);
                    if (!(allowTypedNodes)) {
                        checkSequenceIsUntyped(sourceNode);
                    }
                    break ;
                  case "source-location":
                    sourceLocation = head.getStringValue();
                    break ;
                  case "initial-template":
                    initialTemplate = new Saxon.Api.QName(((Saxon.Hej.value.QNameValue)(head)).getStructuredQName());
                    break ;
                  case "initial-mode":
                    initialMode = new Saxon.Api.QName(((Saxon.Hej.value.QNameValue)(head)).getStructuredQName());
                    break ;
                  case "initial-match-selection":
                    initialMatchSelection = Saxon.Api.XdmValue.Wrap(value);
                    if (!(allowTypedNodes)) {
                        checkSequenceIsUntyped(value);
                    }
                    break ;
                  case "delivery-format":
                    deliveryFormat = head.getStringValue();
                    if (((!(deliveryFormat.Equals("document")) && !(deliveryFormat.Equals("serialized"))) && !(deliveryFormat.Equals("raw")))) {
                        throw new Saxon.Hej.trans.XPathException("The transform option delivery-format should be one of: document|serialized|raw ", "FOXT0002");
                    }
                    break ;
                  case "base-output-uri":
                    baseOutputUri = head.getStringValue();
                    principalResultKey = baseOutputUri;
                    break ;
                  case "serialization-params":
                    serializationParamsMap = (Saxon.Hej.ma.map.MapItem)(head);
                    break ;
                  case "stylesheet-params":
                    {
                        Saxon.Hej.ma.map.MapItem @params = (Saxon.Hej.ma.map.MapItem)(head);
                        processParams(@params, stylesheetParams, allowTypedNodes);
                        break ;
                    }
                  case "global-context-item":
                    if (((!(allowTypedNodes) && (head is Saxon.Hej.om.NodeInfo)) && ((Saxon.Hej.om.NodeInfo)(head)).getTreeInfo().isTyped())) {
                        throw new Saxon.Hej.trans.XPathException("Schema-validated nodes cannot be passed to fn:transform() when it runs under a different Saxon Configuration", "FOXT0002");
                    }
                    globalContextItem = (Saxon.Api.XdmItem)(Saxon.Api.XdmValue.Wrap(head));
                    break ;
                  case "template-params":
                    {
                        Saxon.Hej.ma.map.MapItem @params = (Saxon.Hej.ma.map.MapItem)(head);
                        processParams(@params, templateParams, allowTypedNodes);
                        break ;
                    }
                  case "tunnel-params":
                    {
                        Saxon.Hej.ma.map.MapItem @params = (Saxon.Hej.ma.map.MapItem)(head);
                        processParams(@params, tunnelParams, allowTypedNodes);
                        break ;
                    }
                  case "initial-function":
                    initialFunction = new Saxon.Api.QName(((Saxon.Hej.value.QNameValue)(head)).getStructuredQName());
                    break ;
                  case "function-params":
                    Saxon.Hej.ma.arrays.ArrayItem functionParamsArray = (Saxon.Hej.ma.arrays.ArrayItem)(head);
                    functionParams = new Saxon.Api.XdmValue[functionParamsArray.arrayLength()];
                    for (int i = 0; (i<functionParams.Length); i++) {
                        Saxon.Hej.om.Sequence argVal = functionParamsArray.get(i);
                        if (!(allowTypedNodes)) {
                            checkSequenceIsUntyped(argVal);
                        }
                        functionParams[i] = Saxon.Api.XdmValue.Wrap(argVal);
                    }
                    break ;
                  case "post-process":
                    postProcessor = (Saxon.Hej.om.FunctionItem)(head);
                    break ;
                }
            }
            if ((baseOutputUri == null)) {
                baseOutputUri = getStaticBaseUriString();
            } else {
                try {
                    System.Uri @base = new System.Uri(baseOutputUri, System.UriKind.RelativeOrAbsolute);
                    if (!(@base.IsAbsoluteUri)) {
                        @base = new System.Uri(getRetainedStaticContext().getStaticBaseUri(), baseOutputUri);
                        baseOutputUri = @base.OriginalString;
                    }
                } catch (System.UriFormatException err) {
                    throw new Saxon.Hej.trans.XPathException(("Invalid base output URI "+baseOutputUri), "FOXT0002");
                }
            }
            Saxon.Hej.functions.TransformFn.Deliverer deliverer = Saxon.Hej.functions.TransformFn.Deliverer.makeDeliverer(processor, deliveryFormat);
            deliverer.setTransformer(transformer);
            deliverer.setBaseOutputUri(baseOutputUri);
            deliverer.setPrincipalResultKey(principalResultKey);
            deliverer.setPostProcessor(postProcessor, context);
            Saxon.Hej.trans.XsltController controller = transformer.getUnderlyingController();
            controller.setResultDocumentResolver(deliverer);
            Saxon.Hej.s9api.Destination destination = deliverer.getPrimaryDestination(serializationParamsMap);
            Saxon.Hej.om.Sequence result;
            try {
                transformer.setStylesheetParameters(stylesheetParams);
                transformer.setBaseOutputURI(baseOutputUri);
                transformer.setInitialTemplateParameters(templateParams, false);
                transformer.setInitialTemplateParameters(tunnelParams, true);
                if (((schemaValidation == Saxon.Hej.lib.Validation.STRICT) || (schemaValidation == Saxon.Hej.lib.Validation.LAX))) {
                    if ((sourceNode != null)) {
                        sourceNode = validate(sourceNode, targetConfig, schemaValidation);
                    } else {
                        if ((sourceLocation != null)) {
                            try {
                                string @base = getStaticBaseUriString();
                                Saxon.Hej.lib.JJ_ResourceRequest rr = new Saxon.Hej.lib.JJ_ResourceRequest();
                                rr.relativeUri = sourceLocation;
                                rr.baseUri = @base;
                                rr.nature = Saxon.Hej.lib.JJ_ResourceRequest.XML_NATURE;
                                rr.purpose = Saxon.Hej.lib.JJ_ResourceRequest.ANY_PURPOSE;
                                try {
                                    rr.uri = Saxon.Hej.functions.ResolveURI.makeAbsolute(sourceLocation, @base).ToString();
                                } catch (System.UriFormatException err) {
                                    throw new Saxon.Hej.trans.XPathException(("Unresolvable sourceLocation URI "+sourceLocation), "FOXT0003");
                                }
                                Saxon.Ejavax.xml.transform.Source ss = rr.resolve(xsltCompiler.getResourceResolver(), targetConfig.getResourceResolver(), new Saxon.Callbacks.DirectResourceResolver(targetConfig));
                                Saxon.Hej.lib.ParseOptions parseOptions = targetConfig.getParseOptions().withSchemaValidationMode(schemaValidation);
                                Saxon.Hej.om.TreeInfo tree = targetConfig.buildDocumentTree(ss, parseOptions);
                                sourceNode = tree.getRootNode();
                                sourceLocation = null;
                            } catch (Saxon.Hej.trans.XPathException e) {
                                e.maybeSetErrorCode("FOXT0003");
                                throw;
                            }
                        }
                    }
                    if ((globalContextItem is Saxon.Api.XdmNode)) {
                        Saxon.Hej.om.NodeInfo v = validate(((Saxon.Hej.om.NodeInfo)((Saxon.Api.XdmNode)(globalContextItem)).Unwrap()), targetConfig, schemaValidation);
                        globalContextItem = (Saxon.Api.XdmNode)(Saxon.Api.XdmValue.Wrap(v));
                    }
                }
                if (((sourceNode != null) && (globalContextItem == null))) {
                    transformer.setGlobalContextItem(new Saxon.Api.XdmNode(sourceNode.getRoot()));
                }
                if ((globalContextItem != null)) {
                    transformer.setGlobalContextItem(globalContextItem);
                }
                if (tracing) {
                    System.Object stylesheetId = Saxon.Impl.Helpers.MapUtils.Get(options, styleOption);
                    targetConfig.getLogger().info((("Calling fn:transform("+(((stylesheetId == null) ? "" : stylesheetId.ToString())))+")"));
                }
                if ((initialTemplate != null)) {
                    transformer.callTemplate(initialTemplate, destination);
                    result = deliverer.getPrimaryResult();
                } else {
                    if ((initialFunction != null)) {
                        transformer.callFunction(initialFunction, functionParams, destination);
                        result = deliverer.getPrimaryResult();
                    } else {
                        if ((initialMode != null)) {
                            transformer.setInitialMode(initialMode);
                        }
                        if (((initialMatchSelection == null) && (sourceNode != null))) {
                            initialMatchSelection = Saxon.Api.XdmValue.Wrap(sourceNode);
                        }
                        if (((initialMatchSelection == null) && (sourceLocation != null))) {
                            Saxon.Ejavax.xml.transform.stream.StreamSource stream = new Saxon.Ejavax.xml.transform.stream.StreamSource(sourceLocation);
                            if (transformer.getUnderlyingController().getInitialMode().isDeclaredStreamable()) {
                                transformer.applyTemplates(stream, destination);
                            } else {
                                transformer.transform(stream, destination);
                            }
                            result = deliverer.getPrimaryResult();
                        } else {
                            transformer.applyTemplates(initialMatchSelection, destination);
                            result = deliverer.getPrimaryResult();
                        }
                    }
                }
                if (tracing) {
                    targetConfig.getLogger().info("Returning from fn:transform()");
                }
            } catch (Saxon.Api.SaxonApiException e) {
                Saxon.Hej.trans.XPathException e2;
                if ((e.InnerException is Saxon.Hej.trans.XPathException)) {
                    e2 = (Saxon.Hej.trans.XPathException)(e.InnerException);
                    e2.setIsGlobalError(false);
                    throw e2;
                } else {
                    throw new Saxon.Hej.trans.XPathException(e);
                }
            }
            Saxon.Hej.ma.map.MapItem resultMap = new Saxon.Impl.DataModel.ImmMap();
            resultMap = deliverer.populateResultMap(resultMap);
            if ((result != null)) {
                Saxon.Hej.value.AtomicValue resultKey = new Saxon.Hej.value.StringValue(principalResultKey);
                resultMap = resultMap.addEntry(resultKey, result.materialize());
            }
            return resultMap;
        }
        private void processParams(Saxon.Hej.ma.map.MapItem suppliedParams, System.Collections.Generic.IDictionary<Saxon.Api.QName,Saxon.Api.XdmValue> checkedParams, bool allowTypedNodes) {
            Saxon.Hej.tree.iter.AtomicIterator paramIterator = suppliedParams.keys();
            while (true) {
                Saxon.Hej.value.AtomicValue param = ((/*1430*/Saxon.Hej.value.AtomicValue)paramIterator.next());
                if ((param != null)) {
                    if (!(((param is Saxon.Hej.value.QNameValue)))) {
                        throw new Saxon.Hej.trans.XPathException("The names of parameters must be supplied as QNames", "FOXT0002");
                    }
                    Saxon.Api.QName paramName = new Saxon.Api.QName(((Saxon.Hej.value.QNameValue)(param)).getStructuredQName());
                    Saxon.Hej.om.Sequence value = suppliedParams.get(param);
                    if (!(allowTypedNodes)) {
                        checkSequenceIsUntyped(value);
                    }
                    Saxon.Api.XdmValue paramVal = Saxon.Api.XdmValue.Wrap(value);
                    Saxon.Impl.Helpers.MapUtils.Put(checkedParams, paramName, paramVal);
                } else {
                    break ;
                }
            }
        }
        private void checkSequenceIsUntyped(Saxon.Hej.om.Sequence value) {
            Saxon.Hej.om.SequenceIterator iter = value.iterate();
            Saxon.Hej.om.Item item;
            while (((item = iter.next()) != null)) {
                if (((item is Saxon.Hej.om.NodeInfo) && ((Saxon.Hej.om.NodeInfo)(item)).getTreeInfo().isTyped())) {
                    throw new Saxon.Hej.trans.XPathException("Schema-validated nodes cannot be passed to fn:transform() when it runs under a different Saxon Configuration", "FOXT0002");
                }
            }
        }
        private static Saxon.Hej.om.NodeInfo validate(Saxon.Hej.om.NodeInfo node, Saxon.Hej.Configuration config, int validation) {
            Saxon.Hej.lib.ParseOptions options = config.getParseOptions().withSchemaValidationMode(validation);
            return config.buildDocumentTree(node.asActiveSource(), options).getRootNode();
        }
        private abstract class Deliverer : Saxon.Hej.lib.ResultDocumentResolver {
            protected internal Saxon.Hej.s9api.Xslt30Transformer transformer;
            protected internal string baseOutputUri;
            protected internal string principalResultKey;
            protected internal Saxon.Hej.om.FunctionItem postProcessor;
            protected internal Saxon.Hej.expr.XPathContext context;
            protected internal Saxon.Impl.DataModel.ImmMap resultMap = new Saxon.Impl.DataModel.ImmMap();
            public static Saxon.Hej.functions.TransformFn.Deliverer makeDeliverer(Saxon.Hej.s9api.Processor processor, string deliveryFormat) {
                switch (deliveryFormat) {
                  case "document":
                    return new Saxon.Hej.functions.TransformFn.DocumentDeliverer();
                  case "serialized":
                    return new Saxon.Hej.functions.TransformFn.SerializedDeliverer(processor);
                  case "raw":
                    return new Saxon.Hej.functions.TransformFn.RawDeliverer();
                  default:
                    throw new Saxon.Ejava.lang.IllegalArgumentException("delivery-format");
                }
            }
            public void setTransformer(Saxon.Hej.s9api.Xslt30Transformer transformer) {
                this.transformer = transformer;
            }
            public void setPrincipalResultKey(string key) {
                this.principalResultKey = key;
            }
            public void setBaseOutputUri(string uri) {
                this.baseOutputUri = uri;
            }
            public void setPostProcessor(Saxon.Hej.om.FunctionItem postProcessor, Saxon.Hej.expr.XPathContext context) {
                this.postProcessor = postProcessor;
                this.context = context;
            }
            protected internal System.Uri getAbsoluteUri(string href, string baseUri) {
                System.Uri absolute;
                try {
                    absolute = Saxon.Hej.functions.ResolveURI.makeAbsolute(href, baseUri);
                } catch (System.UriFormatException e) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                }
                return absolute;
            }
            public abstract Saxon.Hej.ma.map.MapItem populateResultMap(Saxon.Hej.ma.map.MapItem resultMap);
            public abstract Saxon.Hej.s9api.Destination getPrimaryDestination(Saxon.Hej.ma.map.MapItem serializationParamsMap);
            protected internal Saxon.Hej.s9api.Serializer makeSerializer(Saxon.Hej.s9api.Processor processor, Saxon.Hej.ma.map.MapItem serializationParamsMap) {
                Saxon.Hej.s9api.Serializer serializer = processor.newSerializer();
                if ((serializationParamsMap != null)) {
                    Saxon.Hej.tree.iter.AtomicIterator paramIterator = serializationParamsMap.keys();
                    Saxon.Hej.value.AtomicValue param;
                    while (((param = ((/*1430*/Saxon.Hej.value.AtomicValue)paramIterator.next())) != null)) {
                        Saxon.Api.QName paramName;
                        if ((param is Saxon.Hej.value.StringValue)) {
                            paramName = new Saxon.Api.QName(param.getStringValue());
                        } else {
                            if ((param is Saxon.Hej.value.QNameValue)) {
                                paramName = new Saxon.Api.QName(((Saxon.Hej.value.QNameValue)(((/*1430*/Saxon.Hej.value.AtomicValue)param.head()))).getStructuredQName());
                            } else {
                                throw new Saxon.Hej.trans.XPathException("Serialization parameters must be strings or QNames", "XPTY0004");
                            }
                        }
                        string paramValue = null;
                        Saxon.Hej.om.GroundedValue supplied = serializationParamsMap.get(param);
                        if ((supplied.getLength()>0)) {
                            if ((supplied.getLength() == 1)) {
                                Saxon.Hej.om.Item val = supplied.itemAt(0);
                                if ((val is Saxon.Hej.value.StringValue)) {
                                    paramValue = val.getStringValue();
                                } else {
                                    if ((val is Saxon.Hej.value.BooleanValue)) {
                                        paramValue = (((Saxon.Hej.value.BooleanValue)(val)).getBooleanValue() ? "yes" : "no");
                                    } else {
                                        if ((val is Saxon.Hej.value.DecimalValue)) {
                                            paramValue = val.getStringValue();
                                        } else {
                                            if ((val is Saxon.Hej.value.QNameValue)) {
                                                paramValue = ((Saxon.Hej.value.QNameValue)(val)).getStructuredQName().getEQName();
                                            } else {
                                                if (((val is Saxon.Hej.ma.map.MapItem) && paramName.ClarkName.Equals(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS))) {
                                                    Saxon.Hej.serialize.CharacterMap charMap = Saxon.Hej.functions.Serialize.toCharacterMap((Saxon.Hej.ma.map.MapItem)(val));
                                                    Saxon.Hej.serialize.CharacterMapIndex charMapIndex = new Saxon.Hej.serialize.CharacterMapIndex();
                                                    charMapIndex.putCharacterMap(charMap.getName(), charMap);
                                                    serializer.setCharacterMap(charMapIndex);
                                                    string existing = serializer.getOutputProperty(Saxon.Hej.s9api.Serializer.Property.USE_CHARACTER_MAPS);
                                                    if ((existing == null)) {
                                                        serializer.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.USE_CHARACTER_MAPS, charMap.getName().getEQName());
                                                    } else {
                                                        serializer.setOutputProperty(Saxon.Hej.s9api.Serializer.Property.USE_CHARACTER_MAPS, ((existing+" ")+charMap.getName().getEQName()));
                                                    }
                                                    continue ;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if ((paramValue == null)) {
                                Saxon.Hej.om.SequenceIterator iter = supplied.iterate();
                                Saxon.Hej.om.Item it;
                                paramValue = "";
                                while (((it = iter.next()) != null)) {
                                    if ((it is Saxon.Hej.value.QNameValue)) {
                                        paramValue += (" "+((Saxon.Hej.value.QNameValue)(it)).getStructuredQName().getEQName());
                                    } else {
                                        throw new Saxon.Hej.trans.XPathException((("Value of serialization parameter "+paramName.EQName)+" not recognized"), "XPTY0004");
                                    }
                                }
                            }
                            Saxon.Hej.s9api.Serializer.Property prop = Saxon.Hej.s9api.Serializer.getProperty(paramName);
                            if ((paramName.ClarkName.Equals(Saxon.Ejavax.xml.transform.OutputKeys.CDATA_SECTION_ELEMENTS) || paramName.ClarkName.Equals(Saxon.Hej.lib.SaxonOutputKeys.SUPPRESS_INDENTATION))) {
                                string existing = serializer.getOutputProperty(paramName);
                                if ((existing == null)) {
                                    serializer.setOutputProperty(prop, paramValue);
                                } else {
                                    serializer.setOutputProperty(prop, (existing+paramValue));
                                }
                            } else {
                                serializer.setOutputProperty(prop, paramValue);
                            }
                        }
                    }
                }
                return serializer;
            }
            public abstract Saxon.Hej.om.Sequence getPrimaryResult();
            public Saxon.Hej.om.GroundedValue postProcess(string uri, Saxon.Hej.om.Sequence result) {
                if ((postProcessor != null)) {
                    result = postProcessor.call(context.newCleanContext(), new Saxon.Hej.om.Sequence[]{new Saxon.Hej.value.StringValue(uri), result});
                }
                return result.materialize();
            }
        
            /* Creating stub method resolve to satisfy interface ResultDocumentResolver */
            public virtual Saxon.Hej.@event.Receiver resolve(Saxon.Hej.expr.XPathContext arg1, string arg2, string arg3, Saxon.Hej.serialize.SerializationProperties arg4) {
                throw new System.NotImplementedException();
            }

        }
        private class DocumentDeliverer : Saxon.Hej.functions.TransformFn.Deliverer {
            private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.om.GroundedValue> results = new System.Collections.Concurrent.ConcurrentDictionary<string, Saxon.Hej.om.GroundedValue>();
            private readonly Saxon.Hej.s9api.XdmDestination destination = new Saxon.Hej.s9api.XdmDestination();
            public DocumentDeliverer()  {
            }
            public override Saxon.Hej.s9api.Destination getPrimaryDestination(Saxon.Hej.ma.map.MapItem serializationParamsMap) {
                return destination;
            }
            public override Saxon.Hej.om.Sequence getPrimaryResult() {
                Saxon.Api.XdmNode node = destination.getXdmNode();
                return ((node == null) ? null : postProcess(baseOutputUri, ((Saxon.Hej.om.NodeInfo)node.Unwrap())));
            }
            public override Saxon.Hej.@event.Receiver resolve(Saxon.Hej.expr.XPathContext context, string href, string baseUri, Saxon.Hej.serialize.SerializationProperties properties) {
                System.Uri absolute = getAbsoluteUri(href, baseUri);
                Saxon.Hej.s9api.XdmDestination destination = new Saxon.Hej.s9api.XdmDestination();
                destination.setDestinationBaseURI(absolute);
                destination.onClose(() => {
                    try {
                        Saxon.Api.XdmNode root = destination.getXdmNode();
                        Saxon.Hej.om.GroundedValue result = postProcess(absolute.OriginalString, ((Saxon.Hej.om.NodeInfo)root.Unwrap()));
                        Saxon.Impl.Helpers.MapUtils.Put(results, absolute.OriginalString, result);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.UncheckedXPathException(e);
                    }
                } );
                Saxon.Hej.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
                return destination.getReceiver(pipe, properties);
            }
            public override Saxon.Hej.ma.map.MapItem populateResultMap(Saxon.Hej.ma.map.MapItem resultMap) {
                foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.om.GroundedValue> entry in results) {
                    string uri = entry.Key;
                    resultMap = resultMap.addEntry(new Saxon.Hej.value.StringValue(uri), entry.Value);
                }
                return resultMap;
            }
        
        }
        private class SerializedDeliverer : Saxon.Hej.functions.TransformFn.Deliverer {
            private readonly Saxon.Hej.s9api.Processor processor;
            private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.om.GroundedValue> results = new System.Collections.Concurrent.ConcurrentDictionary<string, Saxon.Hej.om.GroundedValue>();
            private System.IO.StringWriter primaryWriter;
            public SerializedDeliverer(Saxon.Hej.s9api.Processor processor)  {
                this.processor = processor;
            }
            public override Saxon.Hej.s9api.Destination getPrimaryDestination(Saxon.Hej.ma.map.MapItem serializationParamsMap) {
                Saxon.Hej.s9api.Serializer serializer = makeSerializer(processor, serializationParamsMap);
                primaryWriter = new System.IO.StringWriter();
                serializer.setOutputWriter(primaryWriter);
                return serializer;
            }
            public override Saxon.Hej.om.Sequence getPrimaryResult() {
                string str = primaryWriter.ToString();
                if (str.Length==0) {
                    return null;
                }
                return postProcess(baseOutputUri, new Saxon.Hej.value.StringValue(str));
            }
            public override Saxon.Hej.@event.Receiver resolve(Saxon.Hej.expr.XPathContext context, string href, string baseUri, Saxon.Hej.serialize.SerializationProperties properties) {
                System.Uri absolute = getAbsoluteUri(href, baseUri);
                if (absolute.Scheme.Equals(dummyBaseOutputUriScheme)) {
                    throw new Saxon.Hej.trans.XPathException("The location of output documents is undefined: use the transform option base-output-uri", "FOXT0002");
                }
                System.IO.StringWriter writer = new System.IO.StringWriter();
                Saxon.Hej.s9api.Serializer serializer = makeSerializer(processor, null);
                serializer.setCharacterMap(properties.getCharacterMapIndex());
                serializer.setOutputWriter(writer);
                serializer.onClose(() => {
                    try {
                        Saxon.Hej.om.GroundedValue result = postProcess(absolute.OriginalString, new Saxon.Hej.value.StringValue(writer.ToString()));
                        Saxon.Impl.Helpers.MapUtils.Put(results, absolute.OriginalString, result);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.UncheckedXPathException(e);
                    }
                } );
                try {
                    Saxon.Hej.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
                    Saxon.Hej.@event.Receiver @out = serializer.getReceiver(pipe, properties);
                    @out.setSystemId(absolute.OriginalString);
                    return @out;
                } catch (Saxon.Api.SaxonApiException e) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                }
            }
            public override Saxon.Hej.ma.map.MapItem populateResultMap(Saxon.Hej.ma.map.MapItem resultMap) {
                foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.om.GroundedValue> entry in results) {
                    string uri = entry.Key;
                    resultMap = resultMap.addEntry(new Saxon.Hej.value.StringValue(uri), entry.Value);
                }
                return resultMap;
            }
        
        }
        private class RawDeliverer : Saxon.Hej.functions.TransformFn.Deliverer {
            private readonly System.Collections.Generic.IDictionary<string,Saxon.Hej.om.GroundedValue> results = new System.Collections.Concurrent.ConcurrentDictionary<string, Saxon.Hej.om.GroundedValue>();
            private readonly Saxon.Hej.s9api.RawDestination primaryDestination = new Saxon.Hej.s9api.RawDestination();
            public RawDeliverer()  {
            }
            public override Saxon.Hej.s9api.Destination getPrimaryDestination(Saxon.Hej.ma.map.MapItem serializationParamsMap) {
                return primaryDestination;
            }
            public override Saxon.Hej.om.Sequence getPrimaryResult() {
                Saxon.Hej.om.Sequence actualResult = ((Saxon.Hej.om.GroundedValue)primaryDestination.getXdmValue().Unwrap());
                return postProcess(baseOutputUri, actualResult);
            }
            public override Saxon.Hej.@event.Receiver resolve(Saxon.Hej.expr.XPathContext context, string href, string baseUri, Saxon.Hej.serialize.SerializationProperties properties) {
                System.Uri absolute = getAbsoluteUri(href, baseUri);
                Saxon.Hej.s9api.RawDestination destination = new Saxon.Hej.s9api.RawDestination();
                destination.onClose(() => {
                    try {
                        destination.close();
                        Saxon.Hej.om.GroundedValue result = postProcess(absolute.OriginalString, ((Saxon.Hej.om.GroundedValue)destination.getXdmValue().Unwrap()));
                        Saxon.Impl.Helpers.MapUtils.Put(results, absolute.OriginalString, result);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw new Saxon.Hej.trans.UncheckedXPathException(e);
                    }
                } );
                Saxon.Hej.@event.PipelineConfiguration pipe = context.getController().makePipelineConfiguration();
                return destination.getReceiver(pipe, properties);
            }
            public override Saxon.Hej.ma.map.MapItem populateResultMap(Saxon.Hej.ma.map.MapItem resultMap) {
                foreach (System.Collections.Generic.KeyValuePair<string,Saxon.Hej.om.GroundedValue> entry in results) {
                    string uri = entry.Key;
                    resultMap = resultMap.addEntry(new Saxon.Hej.value.StringValue(uri), entry.Value);
                }
                return resultMap;
            }
        
        }
    
    }
}