using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.expr.instruct {
    public class ResultDocument : Saxon.Hej.expr.instruct.Instruction, Saxon.Hej.expr.instruct.ValidatingInstruction, Saxon.Hej.expr.instruct.InstructionWithComplexContent, Saxon.Hej.expr.ContextOriginator {
        private Saxon.Hej.expr.Operand hrefOp;
        private Saxon.Hej.expr.Operand formatOp;
        private Saxon.Hej.expr.Operand contentOp;
        private bool async = false;
        private readonly Saxon.Ejava.util.Properties globalProperties;
        private readonly Saxon.Ejava.util.Properties localProperties;
        private Saxon.Hej.lib.ParseOptions validationOptions;
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.Operand> serializationAttributes;
        private bool resolveAgainstStaticBase = false;
        private readonly Saxon.Hej.serialize.CharacterMapIndex characterMapIndex;
        public ResultDocument(Saxon.Ejava.util.Properties globalProperties, Saxon.Ejava.util.Properties localProperties, Saxon.Hej.expr.Expression href, Saxon.Hej.expr.Expression formatExpression, int validationAction, Saxon.Hej.type.SchemaType schemaType, System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.Expression> serializationAttributes, Saxon.Hej.serialize.CharacterMapIndex characterMapIndex)  {
            this.globalProperties = globalProperties;
            this.localProperties = localProperties;
            if ((href != null)) {
                hrefOp = new Saxon.Hej.expr.Operand(this, href, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            if ((formatExpression != null)) {
                formatOp = new Saxon.Hej.expr.Operand(this, formatExpression, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
            }
            setValidationAction(validationAction, schemaType);
            this.serializationAttributes = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.expr.Operand>(serializationAttributes.Count);
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.Expression> entry in serializationAttributes) {
                Saxon.Impl.Helpers.MapUtils.Put(this.serializationAttributes, entry.Key, new Saxon.Hej.expr.Operand(this, entry.Value, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC));
            }
            this.characterMapIndex = characterMapIndex;
            foreach (Saxon.Hej.expr.Expression e in serializationAttributes.Values) {
                adoptChildExpression(e);
            }
        }
        public void setContentExpression(Saxon.Hej.expr.Expression content) {
            contentOp = new Saxon.Hej.expr.Operand(this, content, Saxon.Hej.expr.OperandRole.SINGLE_ATOMIC);
        }
        public void setSchemaType(Saxon.Hej.type.SchemaType type) {
            if ((validationOptions == null)) {
                validationOptions = new Saxon.Hej.lib.ParseOptions();
            }
            validationOptions = validationOptions.withSchemaValidationMode(Saxon.Hej.lib.Validation.BY_TYPE).withTopLevelType(type);
        }
        public Saxon.Hej.type.SchemaType getSchemaType() {
            return ((validationOptions == null) ? null : validationOptions.getTopLevelType());
        }
        public bool isResolveAgainstStaticBase() {
            return resolveAgainstStaticBase;
        }
        public Saxon.Hej.lib.ParseOptions getValidationOptions() {
            return validationOptions;
        }
        public void setValidationAction(int mode, Saxon.Hej.type.SchemaType schemaType) {
            bool preservingTypes = ((mode == Saxon.Hej.lib.Validation.PRESERVE) && (schemaType == null));
            if (!(preservingTypes)) {
                if ((validationOptions == null)) {
                    validationOptions = new Saxon.Hej.lib.ParseOptions().withSchemaValidationMode(mode).withTopLevelType(schemaType);
                }
            }
        }
        public int getValidationAction() {
            return ((validationOptions == null) ? Saxon.Hej.lib.Validation.PRESERVE : validationOptions.getSchemaValidationMode());
        }
        public Saxon.Hej.expr.Expression getFormatExpression() {
            return ((formatOp == null) ? null : formatOp.getChildExpression());
        }
        public void setUseStaticBaseUri(bool staticBase) {
            resolveAgainstStaticBase = staticBase;
        }
        public void setAsynchronous(bool async) {
            this.async = async;
        }
        public bool isAsynchronous() {
            return async;
        }
        public override bool isMultiThreaded(Saxon.Hej.Configuration config) {
            return ((isAsynchronous() && config.isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.SCHEMA_VALIDATION)) && config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.ALLOW_MULTITHREADING));
        }
        public override Saxon.Hej.expr.Expression typeCheck(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            typeCheckChildren(visitor, contextInfo);
            string method = getStaticSerializationProperty(Saxon.Hej.style.XSLResultDocument.METHOD);
            bool contentDependentMethod = (((method == null) && (formatOp == null)) && !(serializationAttributes.ContainsKey(Saxon.Hej.style.XSLResultDocument.METHOD)));
            bool buildTree = "yes".Equals(getStaticSerializationProperty(Saxon.Hej.style.XSLResultDocument.BUILD_TREE));
            if ((((((buildTree || contentDependentMethod) || "xml".Equals(method)) || "html".Equals(method)) || "xhtml".Equals(method)) || "text".Equals(method))) {
                try {
                    Saxon.Hej.expr.instruct.DocumentInstr.checkContentSequence(visitor.getStaticContext(), contentOp, validationOptions);
                } catch (Saxon.Hej.trans.XPathException err) {
                    err.maybeSetLocation(getLocation());
                    throw;
                }
            }
            return this;
        }
        public override int getIntrinsicDependencies() {
            return Saxon.Hej.expr.StaticProperty.HAS_SIDE_EFFECTS;
        }
        public override Saxon.Hej.expr.Expression optimize(Saxon.Hej.expr.parser.ExpressionVisitor visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo contextInfo) {
            optimizeChildren(visitor, contextInfo);
            if (isAsynchronous()) {
                Saxon.Hej.expr.Expression e = getParentExpression();
                while ((e != null)) {
                    if (((e is Saxon.Hej.expr.LetExpression) && Saxon.Hej.expr.parser.ExpressionTool.dependsOnVariable(getContentExpression(), new Saxon.Hej.expr.Binding[]{(Saxon.Hej.expr.LetExpression)(e)}))) {
                        ((Saxon.Hej.expr.LetExpression)(e)).setNeedsEagerEvaluation(true);
                    }
                    e = e.getParentExpression();
                }
            }
            return this;
        }
        public override Saxon.Hej.expr.Expression copy(Saxon.Hej.expr.parser.RebindingMap rebindings) {
            System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.Expression> map = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.expr.Expression>();
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.Operand> entry in serializationAttributes) {
                Saxon.Impl.Helpers.MapUtils.Put(map, entry.Key, entry.Value.getChildExpression().copy(rebindings));
            }
            Saxon.Hej.expr.instruct.ResultDocument r = new Saxon.Hej.expr.instruct.ResultDocument(globalProperties, localProperties, ((getHref() == null) ? null : getHref().copy(rebindings)), ((getFormatExpression() == null) ? null : getFormatExpression().copy(rebindings)), getValidationAction(), getSchemaType(), map, characterMapIndex);
            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(this, r);
            r.setContentExpression(getContentExpression().copy(rebindings));
            r.resolveAgainstStaticBase = resolveAgainstStaticBase;
            r.async = async;
            return r;
        }
        public override int getInstructionNameCode() {
            return Saxon.Hej.om.StandardNames.XSL_RESULT_DOCUMENT;
        }
        public override Saxon.Hej.type.ItemType getItemType() {
            return Saxon.Hej.type.ErrorType.getInstance();
        }
        public override System.Collections.Generic.IEnumerable<Saxon.Hej.expr.Operand> operands() {
            System.Collections.Generic.List<Saxon.Hej.expr.Operand> list = new System.Collections.Generic.List<Saxon.Hej.expr.Operand>(6);
            list.Add(contentOp);
            if ((hrefOp != null)) {
                list.Add(hrefOp);
            }
            if ((formatOp != null)) {
                list.Add(formatOp);
            }
            Saxon.Impl.Helpers.ListUtils.AddAll(list, serializationAttributes.Values);
            return list;
        }
        public override Saxon.Hej.expr.parser.PathMap.PathMapNodeSet addToPathMap(Saxon.Hej.expr.parser.PathMap pathMap, Saxon.Hej.expr.parser.PathMap.PathMapNodeSet pathMapNodeSet) {
            Saxon.Hej.expr.parser.PathMap.PathMapNodeSet result = base.addToPathMap(pathMap, pathMapNodeSet);
            result.setReturnable(false);
            return new Saxon.Hej.expr.parser.PathMap.PathMapNodeSet(pathMap.makeNewRoot(this));
        }
        public void process(Saxon.Hej.expr.elab.PushEvaluator content, Saxon.Hej.expr.XPathContext context) {
            checkNotTemporaryOutputState(context);
            context.getConfiguration().processResultDocument(this, content, context);
        }
        public void processInstruction(Saxon.Hej.expr.elab.PushEvaluator content, Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.trans.XsltController controller = (Saxon.Hej.trans.XsltController)(context.getController());
            System.Diagnostics.Debug.Assert((controller != null));
            string savedOutputUri = context.getCurrentOutputUri();
            Saxon.Hej.@event.ComplexContentOutputter @out = processLeft(context);
            bool failed = false;
            try {
                Saxon.Hej.expr.instruct.TailCall tc = /*Delegate.processLeavingTail()*/content(@out, context);
                dispatchTailCall(tc);
            } catch (Saxon.Hej.trans.XPathException err) {
                failed = true;
                err.maybeSetContext(context);
                err.maybeSetLocation(getLocation());
                throw;
            } finally {
                try {
                    @out.close();
                } catch (Saxon.Hej.trans.XPathException e) {
                    if (!(failed)) {
                        throw;
                    }
                }
            }
            context.setCurrentOutputUri(savedOutputUri);
        }
        public Saxon.Hej.@event.ComplexContentOutputter processLeft(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.trans.XsltController controller = (Saxon.Hej.trans.XsltController)(context.getController());
            Saxon.Hej.Configuration config = controller.getConfiguration();
            checkNotTemporaryOutputState(context);
            Saxon.Ejava.util.Properties computedLocalProps = gatherOutputProperties(context);
            if (((computedLocalProps.getProperty(Saxon.Hej.lib.SaxonOutputKeys.PARAMETER_DOCUMENT) != null) && (getStaticBaseURIString() != null))) {
                try {
                    string abs = Saxon.Hej.functions.ResolveURI.makeAbsolute(computedLocalProps.getProperty(Saxon.Hej.lib.SaxonOutputKeys.PARAMETER_DOCUMENT), getStaticBaseURIString()).OriginalString;
                    computedLocalProps.setProperty(Saxon.Hej.lib.SaxonOutputKeys.PARAMETER_DOCUMENT, abs);
                } catch (System.UriFormatException e) {
                    throw Saxon.Hej.trans.XPathException.makeXPathException(e);
                }
            }
            Saxon.Hej.serialize.SerializationProperties serParams = new Saxon.Hej.serialize.SerializationProperties(computedLocalProps, characterMapIndex);
            if (((validationOptions != null) && (validationOptions.getSchemaValidationMode() != Saxon.Hej.lib.Validation.PRESERVE))) {
                serParams.setValidationFactory(output => {
                    Saxon.Hej.@event.NamespaceReducer reducer = new Saxon.Hej.@event.NamespaceReducer(output);
                    return config.getDocumentValidator(reducer, output.getSystemId(), validationOptions, getLocation());
                } );
            }
            Saxon.Hej.@event.Receiver @out = null;
            Saxon.Hej.lib.ResultDocumentResolver resolver;
            string hrefValue = "";
            if ((getHref() != null)) {
                Saxon.Hej.expr.elab.UnicodeStringEvaluator hrefEval = getHref().makeElaborator().elaborateForUnicodeString(true);
                hrefValue = Saxon.Hej.functions.IriToUri.iriToUri(/*Delegate.eval()*/hrefEval(context)).ToString();
            }
            if ((hrefValue.Length==0 || hrefValue.Equals(controller.getBaseOutputURI()))) {
                Saxon.Hej.serialize.PrincipalOutputGatekeeper gateKeeper = controller.getGatekeeper();
                if ((gateKeeper != null)) {
                    gateKeeper.useAsSecondary();
                    @out = gateKeeper.makeReceiver(serParams);
                }
            }
            if ((@out == null)) {
                try {
                    resolver = controller.getResultDocumentResolver();
                    if ((resolver == null)) {
                        resolver = Saxon.Hej.lib.StandardResultDocumentResolver.getInstance();
                    }
                    @out = makeReceiver(hrefValue, getStaticBaseURIString(), context, resolver, serParams, resolveAgainstStaticBase);
                    traceDestination(context, @out);
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.maybeSetLocation(getLocation());
                    e.maybeSetContext(context);
                    throw;
                }
            }
            @out.getPipelineConfiguration().setController(controller);
            string systemId = @out.getSystemId();
            Saxon.Hej.@event.NamespaceReducer nr = new Saxon.Hej.@event.NamespaceReducer(@out);
            Saxon.Hej.@event.ComplexContentOutputter cco = new Saxon.Hej.@event.ComplexContentOutputter(nr);
            cco.setSystemId(systemId);
            context.setCurrentOutputUri(systemId);
            cco.open();
            return cco;
        }
        public Saxon.Hej.serialize.CharacterMapIndex getCharacterMapIndex() {
            return characterMapIndex;
        }
        private void checkNotTemporaryOutputState(Saxon.Hej.expr.XPathContext context) {
            if ((context.getTemporaryOutputState() != 0)) {
                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot execute xsl:result-document while evaluating xsl:"+context.getNamePool().getLocalName(context.getTemporaryOutputState())));
                err.setErrorCode("XTDE1480");
                err.setLocation(getLocation());
                throw err;
            }
        }
        public static Saxon.Hej.@event.Receiver makeReceiver(string hrefValue, string baseURI, Saxon.Hej.expr.XPathContext context, Saxon.Hej.lib.ResultDocumentResolver resolver, Saxon.Hej.serialize.SerializationProperties @params, bool resolveAgainstStaticBase) {
            Saxon.Hej.Controller controller = context.getController();
            try {
                string @base;
                if (resolveAgainstStaticBase) {
                    @base = baseURI;
                } else {
                    @base = controller.getBaseOutputURI();
                }
                try {
                    Saxon.Hej.@event.Receiver @out = resolver.resolve(context, hrefValue, @base, @params);
                    string systemId = @out.getSystemId();
                    if ((systemId == null)) {
                        systemId = Saxon.Hej.functions.ResolveURI.makeAbsolute(hrefValue, @base).OriginalString;
                        @out.setSystemId(systemId);
                    }
                    checkAcceptableUri(context, systemId);
                    return @out;
                } catch (Saxon.Hej.trans.XPathException e) {
                    throw;
                } catch (System.Exception err) {
                    System.Console.Error.WriteLine(err);
                    throw new Saxon.Hej.trans.XPathException("Exception thrown by output resolver", err);
                }
            } catch (Saxon.Ejavax.xml.transform.TransformerException e) {
                throw Saxon.Hej.trans.XPathException.makeXPathException(e);
            }
        }
        public static void traceDestination(Saxon.Hej.expr.XPathContext context, Saxon.Ejavax.xml.transform.Result result) {
            Saxon.Hej.Configuration config = context.getConfiguration();
            bool timing = config.isTiming();
            if (timing) {
                string dest = result.getSystemId();
                if ((dest == null)) {
                    if ((result is Saxon.Ejavax.xml.transform.stream.StreamResult)) {
                        dest = "anonymous output stream";
                    } else {
                        if ((result is Saxon.Ejavax.xml.transform.sax.SAXResult)) {
                            dest = "SAX2 ContentHandler";
                        } else {
                            if ((result is Saxon.Ejavax.xml.transform.dom.DOMResult)) {
                                dest = "DOM tree";
                            } else {
                                dest = result.GetType().Name;
                            }
                        }
                    }
                }
                config.getLogger().info(("Writing to "+dest));
            }
        }
        public static void checkAcceptableUri(Saxon.Hej.expr.XPathContext context, string uri) {
            Saxon.Hej.trans.XsltController controller = (Saxon.Hej.trans.XsltController)(context.getController());
            System.Diagnostics.Debug.Assert((controller != null));
            if ((uri != null)) {
                if ((controller.getDocumentPool().find(uri) != null)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot write to a URI that has already been read: "+((uri.Equals(Saxon.Hej.Controller.ANONYMOUS_PRINCIPAL_OUTPUT_URI) ? "(implicit output URI)" : uri))));
                    err.setXPathContext(context);
                    err.setErrorCode("XTDE1500");
                    throw err;
                }
                Saxon.Hej.om.DocumentKey documentKey = new Saxon.Hej.om.DocumentKey(uri);
                lock (controller) {
                    if (!(controller.checkUniqueOutputDestination(documentKey))) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Cannot write more than one result document to the same URI: "+((uri.Equals(Saxon.Hej.Controller.ANONYMOUS_PRINCIPAL_OUTPUT_URI) ? "(implicit output URI)" : uri))));
                        err.setXPathContext(context);
                        err.setErrorCode("XTDE1490");
                        throw err;
                    } else {
                        controller.addUnavailableOutputDestination(documentKey);
                    }
                }
            }
        }
        public Saxon.Ejava.util.Properties gatherOutputProperties(Saxon.Hej.expr.XPathContext context) {
            Saxon.Hej.Controller controller = context.getController();
            System.Diagnostics.Debug.Assert((controller != null));
            Saxon.Hej.Configuration config = context.getConfiguration();
            Saxon.Ejava.util.Properties computedGlobalProps = globalProperties;
            Saxon.Hej.om.NamespaceResolver nsResolver = getRetainedStaticContext();
            System.Diagnostics.Debug.Assert((nsResolver != null));
            if ((getFormatExpression() != null)) {
                Saxon.Hej.om.StructuredQName qName;
                string format = getFormatExpression().evaluateAsString(context).ToString();
                if (format.StartsWith("Q{")) {
                    qName = Saxon.Hej.om.StructuredQName.fromEQName(format);
                } else {
                    string[] parts;
                    try {
                        parts = Saxon.Hej.om.NameChecker.getQNameParts(format);
                    } catch (Saxon.Hej.om.QNameException e) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("The requested output format "+Saxon.Hej.trans.Err.wrap(format))+" is not a valid QName"));
                        err.maybeSetLocation(getFormatExpression().getLocation());
                        err.setErrorCode("XTDE1460");
                        err.setXPathContext(context);
                        throw err;
                    }
                    Saxon.Hej.om.NamespaceUri uri = nsResolver.getURIForPrefix(parts[0], false);
                    if ((uri == null)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("The namespace prefix in the format name "+format)+" is undeclared"));
                        err.maybeSetLocation(getFormatExpression().getLocation());
                        err.setErrorCode("XTDE1460");
                        err.setXPathContext(context);
                        throw err;
                    }
                    qName = new Saxon.Hej.om.StructuredQName(parts[0], uri, parts[1]);
                }
                computedGlobalProps = ((Saxon.Hej.style.StylesheetPackage)(getRetainedStaticContext().getPackageData())).getNamedOutputProperties(qName);
                if ((computedGlobalProps == null)) {
                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("There is no xsl:output format named "+format));
                    err.setErrorCode("XTDE1460");
                    err.setXPathContext(context);
                    throw err;
                }
            }
            Saxon.Ejava.util.Properties computedLocalProps = new Saxon.Ejava.util.Properties(computedGlobalProps);
            foreach (string key in localProperties.stringPropertyNames()) {
                Saxon.Hej.om.StructuredQName qName = Saxon.Hej.om.StructuredQName.fromClarkName(key);
                try {
                    setSerializationProperty(computedLocalProps, qName.getNamespaceUri(), qName.getLocalPart(), localProperties.getProperty(key), nsResolver, true, config);
                } catch (Saxon.Hej.trans.XPathException e) {
                    e.setErrorCode("XTDE0030");
                    e.maybeSetLocation(getLocation());
                    throw;
                }
            }
            if (!(serializationAttributes.Count==0)) {
                foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.Operand> entry in serializationAttributes) {
                    string value = entry.Value.getChildExpression().evaluateAsString(context).ToString();
                    string lname = entry.Key.getLocalPart();
                    Saxon.Hej.om.NamespaceUri uri = entry.Key.getNamespaceUri();
                    try {
                        setSerializationProperty(computedLocalProps, uri, lname, value, nsResolver, false, config);
                    } catch (Saxon.Hej.trans.XPathException e) {
                        e.setErrorCode("XTDE0030");
                        e.maybeSetLocation(getLocation());
                        e.maybeSetContext(context);
                        if ((Saxon.Hej.om.NamespaceUri.SAXON.Equals(e.getErrorCodeNamespace()) && "SXWN".Equals(e.getErrorCodeLocalPart().Substring(0, 4 - 0)))) {
                            Saxon.Hej.trans.XmlProcessingException ee = new Saxon.Hej.trans.XmlProcessingException(e);
                            ee.setWarning(true);
                            controller.getErrorReporter().report(ee);
                        } else {
                            throw;
                        }
                    }
                }
            }
            computedLocalProps.setProperty(Saxon.Hej.lib.SaxonOutputKeys.STYLESHEET_VERSION, "30");
            return computedLocalProps;
        }
        public string getStaticSerializationProperty(Saxon.Hej.om.StructuredQName name) {
            string clarkName = name.getClarkName();
            string local = localProperties.getProperty(clarkName);
            if ((local != null)) {
                return local;
            }
            if (serializationAttributes.ContainsKey(name)) {
                return null;
            }
            return globalProperties.getProperty(clarkName);
        }
        public static void setSerializationProperty(Saxon.Ejava.util.Properties details, Saxon.Hej.om.NamespaceUri uri, string lname, string value, Saxon.Hej.om.NamespaceResolver nsResolver, bool prevalidated, Saxon.Hej.Configuration config) {
            Saxon.Hej.lib.SerializerFactory sf = config.getSerializerFactory();
            string clarkName = lname;
            if (!(uri.isEmpty())) {
                clarkName = ((("{"+uri)+"}")+lname);
            }
            if ((uri.isEmpty() || Saxon.Hej.om.NamespaceUri.SAXON.Equals(uri))) {
                switch (clarkName) {
                  case "method":
                    value = Saxon.Hej.value.Whitespace.trim(value);
                    if ((value.StartsWith("Q{}") && (value.Length>3))) {
                        value = value.Substring(3);
                    }
                    if ((((((((value.Equals("xml") || value.Equals("html")) || value.Equals("text")) || value.Equals("xhtml")) || value.Equals("json")) || value.Equals("adaptive")) || prevalidated) || value.StartsWith("{"))) {
                        details.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD, value);
                    } else {
                        if (value.StartsWith("Q{")) {
                            details.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD, value.Substring(1));
                        } else {
                            string[] parts;
                            try {
                                parts = Saxon.Hej.om.NameChecker.getQNameParts(value);
                                string prefix = parts[0];
                                if (prefix.Length==0) {
                                    Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException("method must be xml, html, xhtml, text, json, adaptive, or a prefixed name");
                                    err.setErrorCode("SEPM0016");
                                    err.setIsStaticError(true);
                                    throw err;
                                } else {
                                    if ((nsResolver != null)) {
                                        Saxon.Hej.om.NamespaceUri muri = nsResolver.getURIForPrefix(prefix, false);
                                        if ((muri == null)) {
                                            Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Namespace prefix '"+prefix)+"' has not been declared"));
                                            err.setErrorCode("SEPM0016");
                                            err.setIsStaticError(true);
                                            throw err;
                                        }
                                        details.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD, ((('{'+muri.ToString())+'}')+parts[1]));
                                    } else {
                                        details.setProperty(Saxon.Ejavax.xml.transform.OutputKeys.METHOD, value);
                                    }
                                }
                            } catch (Saxon.Hej.om.QNameException e) {
                                Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException(("Invalid method name. "+e.Message));
                                err.setErrorCode("SEPM0016");
                                err.setIsStaticError(true);
                                throw err;
                            }
                        }
                    }
                    break ;
                  case "use-character-maps":
                    string existing = details.getProperty(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS);
                    if ((existing == null)) {
                        existing = "";
                    }
                    details.setProperty(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS, (existing+value));
                    break ;
                  case "cdata-section-elements":
                    processListOfNodeNames(details, clarkName, value, nsResolver, true, prevalidated, false);
                    break ;
                  case "suppress-indentation":
                    processListOfNodeNames(details, clarkName, value, nsResolver, true, prevalidated, false);
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.DOUBLE_SPACE:
                    processListOfNodeNames(details, clarkName, value, nsResolver, true, prevalidated, false);
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.ATTRIBUTE_ORDER:
                    processListOfNodeNames(details, clarkName, value, nsResolver, false, prevalidated, true);
                    break ;
                  case Saxon.Hej.lib.SaxonOutputKeys.NEXT_IN_CHAIN:
                    break ;
                  default:
                    if (clarkName.Equals("output-version")) {
                        clarkName = "version";
                    }
                    if (!(prevalidated)) {
                        try {
                            if (!(Saxon.Hej.lib.SaxonOutputKeys.isUnstrippedProperty(clarkName))) {
                                value = Saxon.Hej.value.Whitespace.trim(value);
                            }
                            value = sf.checkOutputProperty(clarkName, value);
                        } catch (Saxon.Hej.trans.XPathException err) {
                            err.maybeSetErrorCode("SEPM0016");
                            throw;
                        }
                    }
                    details.setProperty(clarkName, value);
                    break ;
                }
            } else {
                details.setProperty(((('{'+uri.ToString())+'}')+lname), value);
            }
        }
        private static void processListOfNodeNames(Saxon.Ejava.util.Properties details, string key, string value, Saxon.Hej.om.NamespaceResolver nsResolver, bool useDefaultNS, bool prevalidated, bool allowStar) {
            string existing = details.getProperty(key);
            if ((existing == null)) {
                existing = "";
            }
            string s = Saxon.Hej.lib.SaxonOutputKeys.parseListOfNodeNames(value, nsResolver, useDefaultNS, prevalidated, allowStar, "SEPM0016");
            details.setProperty(key, (existing+s));
        }
        public override void export(Saxon.Hej.trace.ExpressionPresenter @out) {
            @out.startElement("resultDoc", this);
            @out.emitAttribute("global", exportProperties(globalProperties));
            @out.emitAttribute("local", exportProperties(localProperties));
            if (((getValidationAction() != Saxon.Hej.lib.Validation.SKIP) && (getValidationAction() != Saxon.Hej.lib.Validation.BY_TYPE))) {
                @out.emitAttribute("validation", Saxon.Hej.lib.Validation.describe(getValidationAction()));
            }
            Saxon.Hej.type.SchemaType schemaType = getSchemaType();
            if ((schemaType != null)) {
                @out.emitAttribute("type", schemaType.getStructuredQName());
            }
            if (async) {
                @out.emitAttribute("flags", "a");
            }
            if ((getHref() != null)) {
                @out.setChildRole("href");
                getHref().export(@out);
            }
            if ((getFormatExpression() != null)) {
                @out.setChildRole("format");
                getFormatExpression().export(@out);
            }
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.Operand> p in serializationAttributes) {
                Saxon.Hej.om.StructuredQName name = p.Key;
                Saxon.Hej.expr.Expression value = p.Value.getChildExpression();
                @out.setChildRole(name.getEQName());
                value.export(@out);
            }
            @out.setChildRole("content");
            getContentExpression().export(@out);
            @out.endElement();
        }
        private string exportProperties(Saxon.Ejava.util.Properties props) {
            Saxon.Ejava.lang.StringBuilder writer = new Saxon.Ejava.lang.StringBuilder();
            foreach (string key in props.stringPropertyNames()) {
                string val = props.getProperty(key);
                if ((key.Equals(Saxon.Hej.lib.SaxonOutputKeys.ITEM_SEPARATOR) || key.Equals(Saxon.Hej.lib.SaxonOutputKeys.NEWLINE))) {
                    val = Saxon.Hej.trace.ExpressionPresenter.jsEscape(val);
                }
                if ((key.Equals(Saxon.Hej.lib.SaxonOutputKeys.USE_CHARACTER_MAPS) || key.Equals(Saxon.Ejavax.xml.transform.OutputKeys.METHOD))) {
                    val = val.Replace("{", "Q{");
                }
                string adjustedKey = (key.StartsWith("{") ? ("Q"+key) : key);
                writer.append(adjustedKey).append("=").append(val).append("\n");
            }
            return writer.ToString();
        }
        public static void processXslOutputElement(Saxon.Hej.om.NodeInfo element, Saxon.Ejava.util.Properties props, Saxon.Hej.expr.XPathContext c) {
            Saxon.Hej.om.NamespaceResolver resolver = element.getAllNamespaces();
            foreach (Saxon.Hej.om.AttributeInfo att in element.attributes()) {
                Saxon.Hej.om.NamespaceUri uri = att.getNodeName().getNamespaceUri();
                string local = att.getNodeName().getLocalPart();
                string val = Saxon.Hej.value.Whitespace.trim(att.getValue());
                setSerializationProperty(props, uri, local, val, resolver, false, c.getConfiguration());
            }
        }
        public override string getStreamerName() {
            return "ResultDocument";
        }
        public Saxon.Hej.expr.Expression getHref() {
            return ((hrefOp == null) ? null : hrefOp.getChildExpression());
        }
        public void setHref(Saxon.Hej.expr.Expression href) {
            hrefOp.setChildExpression(href);
        }
        public void setFormatExpression(Saxon.Hej.expr.Expression formatExpression) {
            formatOp.setChildExpression(formatExpression);
        }
        public Saxon.Hej.expr.Expression getContentExpression() {
            return contentOp.getChildExpression();
        }
        public override Saxon.Hej.expr.elab.Elaborator getElaborator() {
            return new Saxon.Hej.expr.instruct.ResultDocument.ResultDocumentElaborator();
        }
        private class ResultDocumentElaborator : Saxon.Hej.expr.elab.PushElaborator {
            public override Saxon.Hej.expr.elab.PushEvaluator elaborateForPush() {
                Saxon.Hej.expr.instruct.ResultDocument expr = (Saxon.Hej.expr.instruct.ResultDocument)(getExpression());
                Saxon.Hej.expr.elab.PushEvaluator contentPush = expr.getContentExpression().makeElaborator().elaborateForPush();
                return (output, context) => {
                    expr.checkNotTemporaryOutputState(context);
                    context.getConfiguration().processResultDocument(expr, contentPush, context);
                    return null;
                } ;
            }
        
        }
    
    }
}