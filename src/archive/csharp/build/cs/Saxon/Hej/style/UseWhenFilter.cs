using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class UseWhenFilter : Saxon.Hej.@event.ProxyReceiver {
        private int depthOfHole = 0;
        private bool emptyStylesheetElement = false;
        private readonly System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceUri> defaultNamespaceStack = new System.Collections.Generic.Stack<Saxon.Hej.om.NamespaceUri>();
        private readonly System.Collections.Generic.Stack<int> versionStack = new System.Collections.Generic.Stack<int>();
        private readonly Saxon.Hej.value.DateTimeValue currentDateTime = Saxon.Hej.value.DateTimeValue.getCurrentDateTime(null);
        private readonly Saxon.Hej.style.Compilation compilation;
        private readonly System.Collections.Generic.Stack<string> systemIdStack = new System.Collections.Generic.Stack<string>();
        private readonly System.Collections.Generic.Stack<System.Uri> baseUriStack = new System.Collections.Generic.Stack<System.Uri>();
        private readonly Saxon.Hej.value.NestedIntegerValue precedence;
        private int importCount = 0;
        private bool dropUnderscoredAttributes;
        private readonly Saxon.Hej.tree.linked.LinkedTreeBuilder treeBuilder;
        public UseWhenFilter(Saxon.Hej.style.Compilation compilation, Saxon.Hej.@event.Receiver next, Saxon.Hej.value.NestedIntegerValue precedence) : base (next) {
            this.compilation = compilation;
            this.precedence = precedence;
            System.Diagnostics.Debug.Assert(((next is Saxon.Hej.tree.linked.LinkedTreeBuilder)));
            treeBuilder = (Saxon.Hej.tree.linked.LinkedTreeBuilder)(next);
        }
        public override void open() {
            nextReceiver.open();
            string sysId = getSystemId();
            if ((sysId == null)) {
                sysId = "";
            }
            systemIdStack.Push(sysId);
            try {
                baseUriStack.Push(new System.Uri(sysId, System.UriKind.RelativeOrAbsolute));
            } catch (System.UriFormatException e) {
                try {
                    baseUriStack.Push(new Saxon.Ejava.io.File(sysId).toURI());
                } catch (System.Exception ex) {
                }
            }
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            int fp = elemName.obtainFingerprint(getNamePool());
            bool inXsltNamespace = elemName.hasURI(Saxon.Hej.om.NamespaceUri.XSLT);
            Saxon.Hej.om.NamespaceUri stdAttUri = (inXsltNamespace ? Saxon.Hej.om.NamespaceUri.NULL : Saxon.Hej.om.NamespaceUri.XSLT);
            Saxon.Hej.tree.linked.DocumentImpl includedDoc = null;
            Saxon.Hej.style.UseWhenFilter.ParsedAttributes pa = startElementProcessAttributes(elemName, attributes, inXsltNamespace, stdAttUri);
            defaultNamespaceStack.Push(pa.xpathDefaultNamespaceAtt);
            if (emptyStylesheetElement) {
                depthOfHole++;
                return ;
            }
            if ((depthOfHole == 0)) {
                System.Uri baseUri = processBaseUri(location, pa.xmlBaseAtt);
                bool ignore = false;
                int version = getVersion(pa, fp);
                versionStack.Push(version);
                if ((((inXsltNamespace && (defaultNamespaceStack.Count == 2)) && (version>30)) && !(Saxon.Hej.functions.ElementAvailable.isXslt30Element(fp)))) {
                    ignore = true;
                }
                if ((pa.hasShadowAttributes && !(ignore))) {
                    attributes = processShadowAttributes(elemName, attributes, namespaces, location, baseUri);
                    string uw = attributes.getValue(stdAttUri, "use-when");
                    if ((uw != null)) {
                        pa.useWhenAtt = uw;
                    }
                }
                if (!(ignore)) {
                    if (checkUseEvaluateWhen(pa, fp, location, baseUri, namespaces, elemName, stdAttUri)) {
                        return ;
                    }
                }
                if (inXsltNamespace) {
                    includedDoc = handleInXsltNamespace(elemName, baseUri, fp, pa, attributes, namespaces, location);
                }
                dropUnderscoredAttributes = inXsltNamespace;
                nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
                checkTargetDocument(includedDoc);
            } else {
                depthOfHole++;
            }
        }
        private void checkTargetDocument(Saxon.Hej.tree.linked.DocumentImpl includedDoc) {
            if ((includedDoc != null)) {
                Saxon.Hej.style.XSLGeneralIncorporate node = (Saxon.Hej.style.XSLGeneralIncorporate)(treeBuilder.getCurrentParentNode());
                node.setTargetDocument(includedDoc);
            }
        }
        private bool checkUseEvaluateWhen(Saxon.Hej.style.UseWhenFilter.ParsedAttributes pa, int fp, Saxon.Hej.s9api.Location location, System.Uri baseUri, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.om.NodeName elemName, Saxon.Hej.om.NamespaceUri stdAttUri) {
            if ((pa.useWhenAtt != null)) {
                Saxon.Hej.tree.AttributeLocation attLoc = new Saxon.Hej.tree.AttributeLocation(elemName.getStructuredQName(), new Saxon.Hej.om.StructuredQName("", stdAttUri, "use-when"), location);
                if (!(evaluateUseWhen(pa.useWhenAtt, attLoc, baseUri.ToString(), namespaces))) {
                    if ((((fp == Saxon.Hej.om.StandardNames.XSL_STYLESHEET) || (fp == Saxon.Hej.om.StandardNames.XSL_TRANSFORM)) || (fp == Saxon.Hej.om.StandardNames.XSL_PACKAGE))) {
                        emptyStylesheetElement = true;
                    } else {
                        depthOfHole = 1;
                        return true;
                    }
                }
            }
            return false;
        }
        private int getVersion(Saxon.Hej.style.UseWhenFilter.ParsedAttributes pa, int fp) {
            int version = System.Int32.MinValue;
            if (((pa.versionAtt != null) && (fp != Saxon.Hej.om.StandardNames.XSL_OUTPUT))) {
                version = processVersionAttribute(pa.versionAtt);
            }
            if ((version == System.Int32.MinValue)) {
                version = ((versionStack.Count==0) ? 30 : versionStack.Peek());
            }
            return version;
        }
        private Saxon.Hej.tree.linked.DocumentImpl processIncludeImport(Saxon.Hej.om.NodeName elemName, Saxon.Hej.s9api.Location location, System.Uri baseUri, string href, bool isImport) {
            if ((href == null)) {
                throw new Saxon.Hej.trans.XPathException(("Missing href attribute on "+elemName.getDisplayName()), "XTSE0010");
            }
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.lib.JJ_ResourceResolver resolver = compilation.getCompilerInfo().getResourceResolver();
            string baseUriStr = baseUri.ToString();
            Saxon.Hej.om.DocumentKey key = Saxon.Hej.functions.DocumentFn.computeDocumentKey(href, baseUriStr, compilation.getPackageData(), false);
            System.Collections.Generic.IDictionary<Saxon.Hej.om.DocumentKey,Saxon.Hej.om.TreeInfo> map = compilation.getStylesheetModules();
            if (map.ContainsKey(key)) {
                return (Saxon.Hej.tree.linked.DocumentImpl)(Saxon.Impl.Helpers.MapUtils.Get(map, key));
            } else {
                Saxon.Hej.lib.JJ_ResourceRequest request = new Saxon.Hej.lib.JJ_ResourceRequest();
                request.relativeUri = href;
                request.baseUri = baseUriStr;
                request.uri = key.getAbsoluteURI();
                request.nature = Saxon.Hej.lib.JJ_ResourceRequest.XSLT_NATURE;
                request.purpose = Saxon.Hej.lib.JJ_ResourceRequest.ANY_PURPOSE;
                Saxon.Ejavax.xml.transform.Source source = request.resolve(resolver, config.getResourceResolver(), new Saxon.Callbacks.DirectResourceResolver(config));
                if ((source == null)) {
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException(((("Unable to resolve "+elemName.getDisplayName())+" stylesheet URI ")+href), "XTSE0165");
                    e.setLocation(location);
                    throw e;
                }
                if ((source is Saxon.Hej.lib.EmptySource)) {
                    source = new Saxon.Ejavax.xml.transform.stream.StreamSource(new System.IO.StringReader("<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'/>"));
                }
                Saxon.Hej.value.NestedIntegerValue newPrecedence = precedence;
                if (isImport) {
                    newPrecedence = precedence.getStem().append((precedence.getLeaf()-1)).append((2*++importCount));
                }
                try {
                    Saxon.Hej.tree.linked.DocumentImpl includedDoc = Saxon.Hej.style.StylesheetModule.loadStylesheetModule(source, false, compilation, newPrecedence);
                    Saxon.Impl.Helpers.MapUtils.Put(map, key, includedDoc);
                    return includedDoc;
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.maybeSetLocation(location);
                    e.maybeSetErrorCode("XTSE0165");
                    if ("XTSE0180".Equals(e.getErrorCodeLocalPart())) {
                        if (isImport) {
                            e.setErrorCode("XTSE0210");
                        }
                    }
                    if (!(e.hasBeenReported())) {
                        compilation.reportError(e);
                    }
                    throw;
                }
            }
        }
        private class ParsedAttributes {
            internal Saxon.Hej.om.NamespaceUri xpathDefaultNamespaceAtt = null;
            internal string versionAtt = null;
            internal string xmlBaseAtt = null;
            internal string useWhenAtt = null;
            internal string staticAtt = null;
            internal bool hasShadowAttributes = false;
        
        }
        private Saxon.Hej.style.UseWhenFilter.ParsedAttributes startElementProcessAttributes(Saxon.Hej.om.NodeName elemName, Saxon.Hej.om.AttributeMap attributes, bool inXsltNamespace, Saxon.Hej.om.NamespaceUri stdAttUri) {
            bool inSaxonNamespace = elemName.hasURI(Saxon.Hej.om.NamespaceUri.SAXON);
            Saxon.Hej.style.UseWhenFilter.ParsedAttributes pa = new Saxon.Hej.style.UseWhenFilter.ParsedAttributes();
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                attName.obtainFingerprint(getNamePool());
                Saxon.Hej.om.NamespaceUri uri = attName.getNamespaceUri();
                string local = attName.getLocalPart();
                if (uri.Equals(stdAttUri)) {
                    processAttributeLocal(att, local, pa);
                    if (((local.StartsWith("_") && uri.Equals(Saxon.Hej.om.NamespaceUri.NULL)) && ((inXsltNamespace || inSaxonNamespace)))) {
                        pa.hasShadowAttributes = true;
                    }
                } else {
                    if ((inSaxonNamespace || uri.Equals(Saxon.Hej.om.NamespaceUri.SAXON))) {
                        if (local.StartsWith("_")) {
                            pa.hasShadowAttributes = true;
                        }
                    } else {
                        if (uri.Equals(Saxon.Hej.om.NamespaceUri.XML)) {
                            if (local.Equals("base")) {
                                pa.xmlBaseAtt = att.getValue();
                            }
                        }
                    }
                }
            }
            return pa;
        }
        private void processAttributeLocal(Saxon.Hej.om.AttributeInfo att, string local, Saxon.Hej.style.UseWhenFilter.ParsedAttributes pa) {
            switch (local) {
              case "xpath-default-namespace":
                pa.xpathDefaultNamespaceAtt = Saxon.Hej.om.NamespaceUri.of(att.getValue());
                break ;
              case "version":
                pa.versionAtt = att.getValue();
                break ;
              case "use-when":
                pa.useWhenAtt = att.getValue();
                break ;
              case "static":
                pa.staticAtt = att.getValue();
                break ;
            }
        }
        private Saxon.Hej.tree.linked.DocumentImpl handleInXsltNamespace(Saxon.Hej.om.NodeName elemName, System.Uri baseUri, int fp, Saxon.Hej.style.UseWhenFilter.ParsedAttributes pa, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location) {
            Saxon.Hej.tree.linked.DocumentImpl includedDoc = null;
            if ((defaultNamespaceStack.Count == 2)) {
                switch (fp) {
                  case Saxon.Hej.om.StandardNames.XSL_VARIABLE:
                  case Saxon.Hej.om.StandardNames.XSL_PARAM:
                    if (pa.hasShadowAttributes) {
                        pa.staticAtt = attributes.getValue(Saxon.Hej.om.NamespaceUri.NULL, "static");
                    }
                    if ((pa.staticAtt != null)) {
                        string staticStr = Saxon.Hej.value.Whitespace.trim(pa.staticAtt);
                        if (Saxon.Hej.style.StyleElement.isYes(staticStr)) {
                            processStaticVariable(elemName, attributes, namespaces, location, baseUri, precedence);
                        }
                    }
                    break ;
                  case Saxon.Hej.om.StandardNames.XSL_INCLUDE:
                  case Saxon.Hej.om.StandardNames.XSL_IMPORT:
                    string href = attributes.getValue(Saxon.Hej.om.NamespaceUri.NULL, "href");
                    includedDoc = processIncludeImport(elemName, location, baseUri, href, (fp == Saxon.Hej.om.StandardNames.XSL_IMPORT));
                    break ;
                  case Saxon.Hej.om.StandardNames.XSL_IMPORT_SCHEMA:
                    compilation.setSchemaAware(true);
                    break ;
                  case Saxon.Hej.om.StandardNames.XSL_USE_PACKAGE:
                    if ((precedence.getDepth()>1)) {
                        throw new Saxon.Hej.trans.XPathException("xsl:use-package cannot appear in an imported stylesheet", "XTSE3008");
                    }
                    string name = attributes.getValue(Saxon.Hej.om.NamespaceUri.NULL, "name");
                    string pversion = attributes.getValue(Saxon.Hej.om.NamespaceUri.NULL, "package-version");
                    if ((name != null)) {
                        try {
                            Saxon.Hej.trans.packages.UsePack use = new Saxon.Hej.trans.packages.UsePack(name, pversion, location.saveLocation());
                            compilation.registerPackageDependency(use);
                        } catch (Saxon.Hej.trans.XPathException err) {
                        }
                    }
                    break ;
                }
            }
            return includedDoc;
        }
        private void processStaticVariable(Saxon.Hej.om.NodeName elemName, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.s9api.Location location, System.Uri baseUri, Saxon.Hej.value.NestedIntegerValue precedence) {
            string nameStr = attributes.getValue(Saxon.Hej.om.NamespaceUri.NULL, "name");
            string asStr = attributes.getValue(Saxon.Hej.om.NamespaceUri.NULL, "as");
            string requiredStr = Saxon.Hej.value.Whitespace.trim(attributes.getValue(Saxon.Hej.om.NamespaceUri.NULL, "required"));
            bool isRequired = Saxon.Hej.style.StyleElement.isYes(requiredStr);
            Saxon.Hej.style.UseWhenStaticContext staticContext = new Saxon.Hej.style.UseWhenStaticContext(compilation, nsResolver);
            staticContext.setBaseURI(baseUri.ToString());
            staticContext.setContainingLocation(new Saxon.Hej.tree.AttributeLocation(elemName.getStructuredQName(), new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, "as"), location));
            Saxon.Hej.value.SequenceType requiredType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
            int languageLevel = compilation.getConfiguration().getConfigurationProperty(Saxon.Hej.lib.Feature<int>.XPATH_VERSION_FOR_XSLT);
            if ((languageLevel == 30)) {
                languageLevel = 305;
            }
            staticContext.setXPathLanguageLevel(languageLevel);
            if ((asStr != null)) {
                Saxon.Hej.expr.parser.XPathParser parser = compilation.getConfiguration().newExpressionParser("XP", false);
                requiredType = parser.parseSequenceType(asStr, staticContext);
            }
            Saxon.Hej.om.StructuredQName varName;
            try {
                varName = Saxon.Hej.om.StructuredQName.fromLexicalQName((nameStr), false, true, nsResolver);
            } catch (Saxon.Hej.trans.XPathException err) {
                throw createXPathException(((("Invalid variable name:"+nameStr)+". ")+err.Message), err.getErrorCodeLocalPart(), location);
            }
            bool isVariable = elemName.getLocalPart().Equals("variable");
            bool isParam = elemName.getLocalPart().Equals("param");
            bool isSupplied = (isParam && compilation.getParameters().containsKey(varName));
            Saxon.Hej.tree.AttributeLocation attLoc = new Saxon.Hej.tree.AttributeLocation(elemName.getStructuredQName(), new Saxon.Hej.om.StructuredQName("", Saxon.Hej.om.NamespaceUri.NULL, "select"), location);
            if (isParam) {
                if ((isRequired && !(isSupplied))) {
                    string selectStr = attributes.getValue(Saxon.Hej.om.NamespaceUri.NULL, "select");
                    if ((selectStr != null)) {
                        throw createXPathException("Cannot supply a default value when required='yes'", "XTSE0010", attLoc);
                    } else {
                        throw createXPathException(("No value was supplied for the required static parameter $"+varName.getDisplayName()), "XTDE0050", location);
                    }
                }
                if (isSupplied) {
                    Saxon.Hej.om.Sequence suppliedValue = compilation.getParameters().convertParameterValue(varName, requiredType, true, staticContext.makeEarlyEvaluationContext());
                    compilation.declareStaticVariable(varName, suppliedValue.materialize(), precedence, isParam);
                }
            }
            if ((isVariable || !(isSupplied))) {
                string selectStr = attributes.getValue(Saxon.Hej.om.NamespaceUri.NULL, "select");
                Saxon.Hej.om.GroundedValue value;
                if ((selectStr == null)) {
                    if (isVariable) {
                        throw createXPathException("The select attribute is required for a static global variable", "XTSE0010", location);
                    } else {
                        if (!(Saxon.Hej.value.Cardinality.allowsZero(requiredType.getCardinality()))) {
                            throw createXPathException(("The parameter is implicitly required because it does not accept an "+"empty sequence, but no value has been supplied"), "XTDE0700", location);
                        } else {
                            if ((asStr == null)) {
                                value = Saxon.Hej.value.StringValue.EMPTY_STRING;
                            } else {
                                value = Saxon.Hej.value.EmptySequence.getInstance();
                            }
                            compilation.declareStaticVariable(varName, value, precedence, isParam);
                        }
                    }
                } else {
                    try {
                        staticContext.setContainingLocation(attLoc);
                        Saxon.Hej.om.Sequence sequence = evaluateStatic(selectStr, location, staticContext);
                        value = sequence.materialize();
                    } catch (Saxon.Hej.trans.XPathException e) {
                        throw createXPathException(((("Error in "+elemName.getLocalPart())+" expression. ")+e.Message), e.getErrorCodeLocalPart(), attLoc);
                    }
                }
                Saxon.Ejava.util.function.Supplier<Saxon.Hej.expr.parser.RoleDiagnostic> role = () => new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, varName.getDisplayName(), 0, "XTDE0050");
                Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
                Saxon.Hej.om.Sequence seq = th.applyFunctionConversionRules(value, requiredType, role, attLoc);
                value = seq.materialize();
                try {
                    compilation.declareStaticVariable(varName, value, precedence, isParam);
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw createXPathException(e.Message, e.getErrorCodeLocalPart(), attLoc);
                }
            }
        }
        private Saxon.Hej.om.AttributeMap processShadowAttributes(Saxon.Hej.om.NodeName elemName, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.s9api.Location location, System.Uri baseUri) {
            System.Collections.Generic.IDictionary<Saxon.Hej.om.NodeName,Saxon.Hej.om.AttributeInfo> attMap = new System.Collections.Generic.Dictionary<Saxon.Hej.om.NodeName, Saxon.Hej.om.AttributeInfo>();
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                Saxon.Impl.Helpers.MapUtils.Put(attMap, attName, att);
            }
            foreach (Saxon.Hej.om.AttributeInfo att in attributes) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string local = attName.getLocalPart();
                Saxon.Hej.om.NamespaceUri uri = attName.getNamespaceUri();
                if (((local.StartsWith("_") && ((uri.isEmpty() || uri.Equals(Saxon.Hej.om.NamespaceUri.SAXON)))) && (local.Length>=2))) {
                    string value = att.getValue();
                    Saxon.Hej.tree.AttributeLocation attLocation = new Saxon.Hej.tree.AttributeLocation(elemName.getStructuredQName(), attName.getStructuredQName(), location);
                    string newValue = processShadowAttribute(value, baseUri.ToString(), nsResolver, attLocation);
                    string plainName = local.Substring(1);
                    Saxon.Hej.om.NodeName newName;
                    if (uri.isEmpty()) {
                        newName = new Saxon.Hej.om.NoNamespaceName(plainName);
                    } else {
                        newName = new Saxon.Hej.om.FingerprintedQName(attName.getPrefix(), Saxon.Hej.om.NamespaceUri.SAXON, plainName);
                    }
                    Saxon.Hej.om.AttributeInfo newAtt = new Saxon.Hej.om.AttributeInfo(newName, att.getType(), newValue, att.getLocation(), Saxon.Hej.@event.ReceiverOption.NONE);
                    Saxon.Impl.Helpers.MapUtils.Put(attMap, newName, newAtt);
                    attMap.Remove(attName);
                }
            }
            Saxon.Hej.om.AttributeMap resultAtts = Saxon.Hej.om.EmptyAttributeMap.getInstance();
            foreach (Saxon.Hej.om.AttributeInfo att in attMap.Values) {
                resultAtts = resultAtts.put(new Saxon.Hej.om.AttributeInfo(att.getNodeName(), att.getType(), att.getValue(), att.getLocation(), att.getProperties()));
            }
            return resultAtts;
        }
        private System.Uri processBaseUri(Saxon.Hej.s9api.Location location, string xmlBaseAtt) {
            string systemId = location.getSystemId();
            if ((systemId == null)) {
                systemId = getSystemId();
            }
            System.Uri baseUri;
            if (((systemId == null) || systemId.Equals(systemIdStack.Peek()))) {
                baseUri = baseUriStack.Peek();
            } else {
                try {
                    baseUri = new System.Uri(systemId, System.UriKind.RelativeOrAbsolute);
                } catch (System.UriFormatException e) {
                    throw new Saxon.Hej.trans.XPathException(("Invalid URI for stylesheet entity: "+systemId));
                }
            }
            if ((xmlBaseAtt != null)) {
                try {
                    baseUri = new System.Uri(baseUri, xmlBaseAtt);
                } catch (Saxon.Ejava.lang.IllegalArgumentException iae) {
                    throw new Saxon.Hej.trans.XPathException(((("Invalid URI in xml:base attribute: "+xmlBaseAtt)+". ")+iae.Message));
                }
            }
            baseUriStack.Push(baseUri);
            systemIdStack.Push(systemId);
            return baseUri;
        }
        private int processVersionAttribute(string version) {
            if ((version != null)) {
                Saxon.Hej.type.ConversionResult cr = Saxon.Hej.value.BigDecimalValue.makeDecimalValue(version, true);
                if ((cr is Saxon.Hej.type.ValidationFailure)) {
                    throw new Saxon.Hej.trans.XPathException(("Invalid version number: "+version), "XTSE0110");
                }
                Saxon.Hej.value.BigDecimalValue d = (Saxon.Hej.value.BigDecimalValue)(cr.asAtomic());
                return ((int)(d.getDecimalValue()*(Singulink.Numerics.BigDecimal)10));
            } else {
                return System.Int32.MinValue;
            }
        }
        private string processShadowAttribute(string expression, string baseUri, Saxon.Hej.om.NamespaceResolver nsResolver, Saxon.Hej.tree.AttributeLocation loc) {
            Saxon.Hej.style.UseWhenStaticContext staticContext = new Saxon.Hej.style.UseWhenStaticContext(compilation, nsResolver);
            staticContext.setBaseURI(baseUri);
            staticContext.setContainingLocation(loc);
            setNamespaceBindings(staticContext);
            Saxon.Hej.expr.Expression expr = Saxon.Hej.style.AttributeValueTemplate.make(expression, staticContext);
            expr = typeCheck(expr, staticContext);
            Saxon.Hej.expr.instruct.SlotManager stackFrameMap = allocateSlots(expression, expr);
            Saxon.Hej.expr.XPathContext dynamicContext = makeDynamicContext(staticContext);
            ((Saxon.Hej.expr.XPathContextMajor)(dynamicContext)).openStackFrame(stackFrameMap);
            return expr.evaluateAsString(dynamicContext).ToString();
        }
        private Saxon.Hej.trans.XPathException createXPathException(string message, string errorCode, Saxon.Hej.s9api.Location location) {
            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(message);
            err.setErrorCode(errorCode);
            err.setIsStaticError(true);
            err.setLocator(location.saveLocation());
            getPipelineConfiguration().getErrorReporter().report(new Saxon.Hej.trans.XmlProcessingException(err));
            err.setHasBeenReported(true);
            return err;
        }
        public override void endElement() {
            defaultNamespaceStack.Pop();
            if ((depthOfHole>0)) {
                depthOfHole--;
            } else {
                systemIdStack.Pop();
                baseUriStack.Pop();
                versionStack.Pop();
                nextReceiver.endElement();
            }
        }
        public override void characters(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
            if ((depthOfHole == 0)) {
                nextReceiver.characters(chars, locationId, properties);
            }
        }
        public override void processingInstruction(string target, Saxon.Hej.str.UnicodeString data, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        public override void comment(Saxon.Hej.str.UnicodeString chars, Saxon.Hej.s9api.Location locationId, int properties) {
        }
        private bool evaluateUseWhen(string expression, Saxon.Hej.tree.AttributeLocation location, string baseUri, Saxon.Hej.om.NamespaceResolver nsResolver) {
            Saxon.Hej.style.UseWhenStaticContext staticContext = new Saxon.Hej.style.UseWhenStaticContext(compilation, nsResolver);
            staticContext.setBaseURI(baseUri);
            staticContext.setContainingLocation(location);
            setNamespaceBindings(staticContext);
            Saxon.Hej.expr.Expression expr = Saxon.Hej.expr.parser.ExpressionTool.make(expression, staticContext, 0, Saxon.Hej.expr.parser.Token.EOF, null);
            expr.setRetainedStaticContext(staticContext.makeRetainedStaticContext());
            expr = typeCheck(expr, staticContext);
            Saxon.Hej.expr.instruct.SlotManager stackFrameMap = allocateSlots(expression, expr);
            Saxon.Hej.expr.XPathContext dynamicContext = makeDynamicContext(staticContext);
            ((Saxon.Hej.expr.XPathContextMajor)(dynamicContext)).openStackFrame(stackFrameMap);
            return expr.effectiveBooleanValue(dynamicContext);
        }
        private Saxon.Hej.expr.instruct.SlotManager allocateSlots(string expression, Saxon.Hej.expr.Expression expr) {
            Saxon.Hej.expr.instruct.SlotManager stackFrameMap = getPipelineConfiguration().getConfiguration().makeSlotManager();
            if ((expression.IndexOf('$')>=0)) {
                Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(expr, stackFrameMap.getNumberOfVariables(), stackFrameMap);
            }
            return stackFrameMap;
        }
        private void setNamespaceBindings(Saxon.Hej.style.UseWhenStaticContext staticContext) {
            staticContext.setDefaultElementNamespace(Saxon.Hej.om.NamespaceUri.NULL);
            foreach (Saxon.Hej.om.NamespaceUri uri in defaultNamespaceStack) {
                if ((uri != null)) {
                    staticContext.setDefaultElementNamespace(uri);
                    break ;
                }
            }
        }
        private Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.Expression expr, Saxon.Hej.style.UseWhenStaticContext staticContext) {
            Saxon.Hej.type.ItemType contextItemType = Saxon.Hej.type.Type.ITEM_TYPE;
            Saxon.Hej.expr.parser.ContextItemStaticInfo cit = getConfiguration().makeContextItemStaticInfo(contextItemType, true);
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(staticContext);
            return expr.typeCheck(visitor, cit);
        }
        private Saxon.Hej.expr.XPathContext makeDynamicContext(Saxon.Hej.style.UseWhenStaticContext staticContext) {
            Saxon.Hej.Controller controller = new Saxon.Hej.Controller(getConfiguration());
            controller.getExecutable().setFunctionLibrary((Saxon.Hej.functions.FunctionLibraryList)(staticContext.getFunctionLibrary()));
            if ((staticContext.getXPathVersion()<30)) {
                controller.setResourceResolver(new Saxon.Hej.lib.ResourceResolverDelegate(request => {
                    throw new Saxon.Hej.trans.UncheckedXPathException("No external documents are available within an [xsl]use-when expression");
                } ));
            }
            controller.setCurrentDateTime(currentDateTime);
            Saxon.Hej.expr.XPathContext dynamicContext = controller.newXPathContext();
            dynamicContext = dynamicContext.newCleanContext();
            return dynamicContext;
        }
        public Saxon.Hej.om.Sequence evaluateStatic(string expression, Saxon.Hej.s9api.Location locationId, Saxon.Hej.style.UseWhenStaticContext staticContext) {
            setNamespaceBindings(staticContext);
            Saxon.Hej.expr.Expression expr = Saxon.Hej.expr.parser.ExpressionTool.make(expression, staticContext, 0, Saxon.Hej.expr.parser.Token.EOF, null);
            expr = typeCheck(expr, staticContext);
            Saxon.Hej.expr.instruct.SlotManager stackFrameMap = getPipelineConfiguration().getConfiguration().makeSlotManager();
            Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(expr, stackFrameMap.getNumberOfVariables(), stackFrameMap);
            Saxon.Hej.expr.XPathContext dynamicContext = makeDynamicContext(staticContext);
            ((Saxon.Hej.expr.XPathContextMajor)(dynamicContext)).openStackFrame(stackFrameMap);
            return Saxon.Hej.om.SequenceTool.toGroundedValue(expr.iterate(dynamicContext));
        }
    
    }
}