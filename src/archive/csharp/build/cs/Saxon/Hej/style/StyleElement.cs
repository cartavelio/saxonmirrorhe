using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public abstract class StyleElement : Saxon.Hej.tree.linked.ElementImpl {
    // Injecting C# code
        
            protected System.Collections.Generic.IEnumerable<Saxon.Hej.om.NodeInfo> children(System.Predicate<Saxon.Hej.om.NodeInfo> filter) {
                return new Saxon.Hej.tree.util.Navigator.ChildrenAsIterable(this, Saxon.Hej.pattern.NodeSelector.of(filter));
            }
    // End of injected C# code
        protected internal string[] extensionNamespaces = null;
        private string[] excludedNamespaces = null;
        protected internal int version = -1;
        protected internal Saxon.Hej.style.ExpressionContext staticContext = null;
        protected internal Saxon.Hej.trans.XmlProcessingIncident validationError = null;
        protected internal Saxon.Hej.style.StyleElement.OnFailure reportingCircumstances = Saxon.Hej.style.StyleElement.OnFailure.REPORT_ALWAYS;
        protected internal string defaultXPathNamespace = null;
        protected internal string defaultCollationName = null;
        protected internal Saxon.Hej.om.StructuredQName defaultMode;
        protected internal bool expandText = false;
        private Saxon.Hej.om.StructuredQName objectName;
        private string baseURI;
        private Saxon.Hej.style.Compilation compilation;
        private Saxon.Hej.expr.parser.Loc savedLocation = null;
        private int defaultValidation = Saxon.Hej.lib.Validation.DEFAULT;
        public enum OnFailure {
            REPORT_ALWAYS,
            REPORT_UNLESS_FORWARDS_COMPATIBLE,
            REPORT_IF_INSTANTIATED,
            REPORT_STATICALLY_UNLESS_FALLBACK_AVAILABLE,
            REPORT_DYNAMICALLY_UNLESS_FALLBACK_AVAILABLE,
            IGNORED_INSTRUCTION,
        }
        protected internal int actionsCompleted = 0;
        public const int ACTION_VALIDATE = 1;
        public const int ACTION_COMPILE = 2;
        public const int ACTION_TYPECHECK = 4;
        public const int ACTION_OPTIMIZE = 8;
        public const int ACTION_FIXUP = 16;
        public const int ACTION_PROCESS_ATTRIBUTES = 32;
        public StyleElement()  {
        }
        public Saxon.Hej.style.Compilation getCompilation() {
            return compilation;
        }
        public virtual void setCompilation(Saxon.Hej.style.Compilation compilation) {
            this.compilation = compilation;
        }
        public Saxon.Hej.style.StylesheetPackage getPackageData() {
            return getPrincipalStylesheetModule().getStylesheetPackage();
        }
        public override Saxon.Hej.Configuration getConfiguration() {
            return compilation.getConfiguration();
        }
        public Saxon.Hej.style.ExpressionContext getStaticContext() {
            if ((staticContext == null)) {
                staticContext = new Saxon.Hej.style.ExpressionContext(this, null);
            }
            return staticContext;
        }
        public Saxon.Hej.style.ExpressionContext getStaticContext(Saxon.Hej.om.StructuredQName attributeName) {
            return new Saxon.Hej.style.ExpressionContext(this, attributeName);
        }
        public override string getBaseURI() {
            if ((baseURI == null)) {
                baseURI = base.getBaseURI();
            }
            return baseURI;
        }
        public Saxon.Hej.expr.parser.ExpressionVisitor makeExpressionVisitor() {
            return Saxon.Hej.expr.parser.ExpressionVisitor.make(getStaticContext());
        }
        public bool isSchemaAware() {
            return getCompilation().isSchemaAware();
        }
        public void substituteFor(Saxon.Hej.style.StyleElement temp) {
            setRawParent(temp.getRawParent());
            setAttributes(temp.attributes());
            setNamespaceMap(temp.getAllNamespaces());
            setNodeName(temp.getNodeName());
            setRawSequenceNumber(temp.getRawSequenceNumber());
            extensionNamespaces = temp.extensionNamespaces;
            excludedNamespaces = temp.excludedNamespaces;
            version = temp.version;
            staticContext = temp.staticContext;
            validationError = temp.validationError;
            reportingCircumstances = temp.reportingCircumstances;
            compilation = temp.compilation;
        }
        public void setValidationError(Saxon.Hej.trans.XmlProcessingIncident reason, Saxon.Hej.style.StyleElement.OnFailure circumstances) {
            validationError = reason;
            reportingCircumstances = circumstances;
        }
        internal void setIgnoreInstruction() {
            reportingCircumstances = Saxon.Hej.style.StyleElement.OnFailure.IGNORED_INSTRUCTION;
        }
        public virtual bool isInstruction() {
            return false;
        }
        public virtual bool isDeclaration() {
            return false;
        }
        public virtual Saxon.Hej.trans.Visibility getVisibility() {
            string vis = getAttributeValue("", "visibility");
            if ((vis == null)) {
                return Saxon.Hej.trans.Visibility.PRIVATE;
            } else {
                return interpretVisibilityValue(vis, "");
            }
        }
        public Saxon.Hej.trans.Visibility getDeclaredVisibility() {
            string vis = getAttributeValue("", "visibility");
            if ((vis == null)) {
                return Saxon.Hej.trans.Visibility.UNDEFINED;
            } else {
                return interpretVisibilityValue(vis, "");
            }
        }
        protected internal virtual bool markTailCalls() {
            return false;
        }
        protected internal virtual bool mayContainSequenceConstructor() {
            return false;
        }
        protected internal virtual bool mayContainFallback() {
            return mayContainSequenceConstructor();
        }
        protected internal virtual bool mayContainParam() {
            return false;
        }
        internal int getDefaultValidation() {
            int v = defaultValidation;
            Saxon.Hej.om.NodeInfo p = this;
            while ((v == Saxon.Hej.lib.Validation.DEFAULT)) {
                p = p.getParent();
                if (!(((p is Saxon.Hej.style.StyleElement)))) {
                    return Saxon.Hej.lib.Validation.STRIP;
                }
                v = ((Saxon.Hej.style.StyleElement)(p)).defaultValidation;
            }
            return v;
        }
        public Saxon.Hej.om.StructuredQName makeQName(string lexicalQName, string errorCode, string attributeName) {
            Saxon.Hej.om.StructuredQName qName;
            try {
                qName = Saxon.Hej.om.StructuredQName.fromLexicalQName((lexicalQName), false, true, this);
            } catch (Saxon.Hej.trans.XPathException e) {
                e.setIsStaticError(true);
                if ((errorCode == null)) {
                    string code = e.getErrorCodeLocalPart();
                    if ("FONS0004".Equals(code)) {
                        e.setErrorCode("XTSE0280");
                    } else {
                        if ("FOCA0002".Equals(code)) {
                            e.setErrorCode("XTSE0020");
                        } else {
                            if ((code == null)) {
                                e.setErrorCode("XTSE0020");
                            }
                        }
                    }
                } else {
                    e.setErrorCode(errorCode);
                }
                if ((attributeName == null)) {
                    e.setLocator(this);
                } else {
                    e.setLocator(new Saxon.Hej.tree.AttributeLocation(this, Saxon.Hej.om.StructuredQName.fromEQName((attributeName))));
                }
                compileError(e);
                qName = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, "error-name");
            }
            if (Saxon.Hej.lib.NamespaceConstant.isReserved(qName.getURI())) {
                if (qName.hasURI(Saxon.Hej.lib.NamespaceConstant.XSLT)) {
                    if ((qName.getLocalPart().Equals("initial-template") && (((this is Saxon.Hej.style.XSLTemplate) || (this is Saxon.Hej.style.XSLCallTemplate))))) {
                        return qName;
                    }
                    if (qName.getLocalPart().Equals("original")) {
                        if ((findAncestorElement(Saxon.Hej.om.StandardNames.XSL_OVERRIDE) != null)) {
                            return qName;
                        }
                    }
                }
                Saxon.Hej.trans.XmlProcessingIncident err = new Saxon.Hej.trans.XmlProcessingIncident((("Namespace prefix "+qName.getPrefix())+" refers to a reserved namespace"), "XTSE0080");
                compileError(err);
                qName = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, "error-name");
            }
            return qName;
        }
        internal Saxon.Hej.style.StyleElement findAncestorElement(int fingerprint) {
            Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            while (true) {
                if ((parent is Saxon.Hej.style.StyleElement)) {
                    if ((parent.getFingerprint() == fingerprint)) {
                        return (Saxon.Hej.style.StyleElement)(parent);
                    } else {
                        parent = parent.getParent();
                    }
                } else {
                    return null;
                }
            }
        }
        public virtual Saxon.Hej.style.StylesheetPackage getUsedPackage() {
            return null;
        }
        public Saxon.Hej.expr.instruct.Actor getXslOriginal(int componentKind) {
            Saxon.Hej.style.StyleElement container = ((componentKind == getFingerprint()) ? this : findAncestorElement(componentKind));
            if (!(((container is Saxon.Hej.style.StylesheetComponent)))) {
                throw new Saxon.Hej.trans.XPathException((("A reference to xsl:original appears within the wrong kind of component: in this case"+", it must be within xsl:")+getNamePool().getLocalName(componentKind)), "XTSE0650", this);
            }
            Saxon.Hej.trans.SymbolicName originalName = ((Saxon.Hej.style.StylesheetComponent)(container)).getSymbolicName();
            Saxon.Hej.style.StyleElement xslOverride = container.findAncestorElement(Saxon.Hej.om.StandardNames.XSL_OVERRIDE);
            if ((xslOverride == null)) {
                throw new Saxon.Hej.trans.XPathException("A reference to xsl:original can be used only within an xsl:override element");
            }
            Saxon.Hej.style.StyleElement usePackage = xslOverride.findAncestorElement(Saxon.Hej.om.StandardNames.XSL_USE_PACKAGE);
            if ((usePackage == null)) {
                throw new Saxon.Hej.trans.XPathException("The parent of xsl:override must be an xsl:use-package element", "XTSE0010", xslOverride);
            }
            Saxon.Hej.expr.Component overridden = usePackage.getUsedPackage().getComponent(originalName);
            if ((overridden == null)) {
                return null;
            }
            return overridden.getActor();
        }
        internal Saxon.Hej.expr.Component getOverriddenComponent() {
            if (!(((this is Saxon.Hej.style.StylesheetComponent)))) {
                return null;
            }
            Saxon.Hej.trans.SymbolicName originalName = ((Saxon.Hej.style.StylesheetComponent)(this)).getSymbolicName();
            Saxon.Hej.style.StyleElement xslOverride = findAncestorElement(Saxon.Hej.om.StandardNames.XSL_OVERRIDE);
            if ((xslOverride == null)) {
                return null;
            }
            Saxon.Hej.style.StyleElement usePackage = xslOverride.findAncestorElement(Saxon.Hej.om.StandardNames.XSL_USE_PACKAGE);
            if ((usePackage == null)) {
                return null;
            }
            return usePackage.getUsedPackage().getComponent(originalName);
        }
        public Saxon.Hej.expr.parser.RetainedStaticContext makeRetainedStaticContext() {
            return getStaticContext().makeRetainedStaticContext();
        }
        internal bool changesRetainedStaticContext() {
            Saxon.Hej.tree.linked.NodeImpl parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            return (((((((parent == null) || !(Saxon.Hej.expr.parser.ExpressionTool.equalOrNull(getBaseURI(), parent.getBaseURI()))) || (defaultCollationName != null)) || (defaultXPathNamespace != null)) || !(((parent is Saxon.Hej.style.StyleElement)))) || (getAllNamespaces() != parent.getAllNamespaces())) || (getEffectiveVersion() != ((Saxon.Hej.style.StyleElement)(parent)).getEffectiveVersion()));
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return this;
        }
        public virtual void processAllAttributes() {
            processDefaultCollationAttribute();
            processDefaultMode();
            staticContext = new Saxon.Hej.style.ExpressionContext(this, null);
            processAttributes();
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child is Saxon.Hej.style.StyleElement)) {
                    ((Saxon.Hej.style.StyleElement)(child)).processAllAttributes();
                } else {
                    if ((child is Saxon.Hej.style.TextValueTemplateNode)) {
                        ((Saxon.Hej.style.TextValueTemplateNode)(child)).parse();
                    }
                }
            }
        }
        public void processStandardAttributes(string @namespace) {
            processExtensionElementAttribute(@namespace);
            processExcludedNamespaces(@namespace);
            processVersionAttribute(@namespace);
            processDefaultXPathNamespaceAttribute(@namespace);
            processDefaultValidationAttribute(@namespace);
            processExpandTextAttribute(@namespace);
        }
        public string getAttributeValue(string clarkName) {
            Saxon.Hej.om.NodeName nn = Saxon.Hej.om.FingerprintedQName.fromClarkName(clarkName);
            return getAttributeValue(nn.getURI(), nn.getLocalPart());
        }
        protected internal void processAttributes() {
            prepareAttributes();
        }
        protected internal void checkUnknownAttribute(Saxon.Hej.om.NodeName nc) {
            string attributeURI = nc.getURI();
            string elementURI = getURI();
            string clarkName = nc.getStructuredQName().getClarkName();
            if (forwardsCompatibleModeIsEnabled()) {
                return ;
            }
            if ((((isInstruction() && attributeURI.Equals(Saxon.Hej.lib.NamespaceConstant.XSLT)) && !(elementURI.Equals(Saxon.Hej.lib.NamespaceConstant.XSLT))) && (((((((((clarkName.EndsWith("}default-collation") || clarkName.EndsWith("}default-mode")) || clarkName.EndsWith("}xpath-default-namespace")) || clarkName.EndsWith("}expand-text")) || clarkName.EndsWith("}extension-element-prefixes")) || clarkName.EndsWith("}exclude-result-prefixes")) || clarkName.EndsWith("}version")) || clarkName.EndsWith("}default-validation")) || clarkName.EndsWith("}use-when"))))) {
                return ;
            }
            if ((elementURI.Equals(Saxon.Hej.lib.NamespaceConstant.XSLT) && (((((((((clarkName.Equals("default-collation") || clarkName.Equals("default-mode")) || clarkName.Equals("expand-text")) || clarkName.Equals("xpath-default-namespace")) || clarkName.Equals("extension-element-prefixes")) || clarkName.Equals("exclude-result-prefixes")) || clarkName.Equals("version")) || clarkName.Equals("default-validation")) || clarkName.Equals("use-when"))))) {
                return ;
            }
            if (("".Equals(attributeURI) || Saxon.Hej.lib.NamespaceConstant.XSLT.Equals(attributeURI))) {
                compileErrorInAttribute(((("Attribute "+Saxon.Hej.trans.Err.wrap(nc.getDisplayName(), Saxon.Hej.trans.Err.ATTRIBUTE))+" is not allowed on element ")+Saxon.Hej.trans.Err.wrap(getDisplayName(), Saxon.Hej.trans.Err.ELEMENT)), "XTSE0090", clarkName);
            } else {
                if (Saxon.Hej.lib.NamespaceConstant.SAXON.Equals(attributeURI)) {
                    compileWarning(("Unrecognized attribute in Saxon namespace: "+nc.getDisplayName()), "XTSE0090");
                }
            }
        }
        protected internal abstract void prepareAttributes();
        internal Saxon.Hej.style.StyleElement getLastChildInstruction() {
            Saxon.Hej.style.StyleElement last = null;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child is Saxon.Hej.style.StyleElement)) {
                    last = (Saxon.Hej.style.StyleElement)(child);
                } else {
                    last = null;
                }
            }
            return last;
        }
        public Saxon.Hej.expr.Expression makeExpression(string expression, Saxon.Hej.om.AttributeInfo att) {
            try {
                Saxon.Hej.expr.StaticContext env = staticContext;
                if ((att != null)) {
                    Saxon.Hej.om.StructuredQName attName = att.getNodeName().getStructuredQName();
                    env = getStaticContext(attName);
                }
                return Saxon.Hej.expr.parser.ExpressionTool.make(expression, env, 0, Saxon.Hej.expr.parser.Token.EOF, getCompilation().getCompilerInfo().getCodeInjector());
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetLocation(allocateLocation());
                if (err.isReportableStatically()) {
                    compileError(err);
                }
                Saxon.Hej.expr.ErrorExpression erexp = new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(err));
                erexp.setRetainedStaticContext(makeRetainedStaticContext());
                erexp.setLocation(allocateLocation());
                return erexp;
            }
        }
        internal Saxon.Hej.pattern.Pattern makePattern(string pattern, string attributeName) {
            try {
                Saxon.Hej.expr.StaticContext env = getStaticContext(new Saxon.Hej.om.StructuredQName("", "", attributeName));
                Saxon.Hej.pattern.Pattern p = Saxon.Hej.pattern.Pattern.make(pattern, env, getCompilation().getPackageData());
                p.setLocation(allocateLocation());
                return p;
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetErrorCode("XTSE0340");
                if ("XPST0003".Equals(err.getErrorCodeLocalPart())) {
                    err.setErrorCode("XTSE0340");
                }
                compileError(err);
                Saxon.Hej.pattern.NodeTestPattern nsp = new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.pattern.AnyNodeTest.getInstance());
                nsp.setLocation(allocateLocation());
                return nsp;
            }
        }
        protected internal Saxon.Hej.expr.Expression makeAttributeValueTemplate(string expression, Saxon.Hej.om.AttributeInfo att) {
            Saxon.Hej.expr.StaticContext env = ((att == null) ? staticContext : getStaticContext(att.getNodeName().getStructuredQName()));
            if ((att != null)) {
                Saxon.Hej.om.StructuredQName attName = att.getNodeName().getStructuredQName();
                env = getStaticContext(attName);
            }
            try {
                return Saxon.Hej.style.AttributeValueTemplate.make(expression, env);
            } catch (Saxon.Hej.trans.XPathException err) {
                compileError(err);
                return new Saxon.Hej.expr.StringLiteral(expression);
            }
        }
        internal void checkAttributeValue(string name, string value, bool avt, string[] allowed) {
            if ((avt && value.Contains("{"))) {
                return ;
            }
            if ((Saxon.Ejava.util.Arrays.binarySearch(allowed, value)<0)) {
                Saxon.Ejava.lang.StringBuilder sb = new Saxon.Ejava.lang.StringBuilder(64);
                sb.append("Invalid value for ");
                sb.append("@");
                sb.append(name);
                sb.append(". Value must be one of (");
                for (int i = 0; (i<allowed.Length); i++) {
                    sb.append(((i == 0) ? "" : "|"));
                    sb.append(allowed[i]);
                }
                sb.append(")");
                compileError(sb.ToString(), "XTSE0020");
            }
        }
        public readonly static string[] YES_NO = {"0", "1", "false", "no", "true", "yes"};
        public bool processBooleanAttribute(string name, string value) {
            string s = Saxon.Hej.value.Whitespace.trim(value);
            if (isYes(s)) {
                return true;
            } else {
                if (isNo(s)) {
                    return false;
                } else {
                    invalidAttribute(name, "yes|no | true|false | 1|0");
                    return false;
                }
            }
        }
        public static bool isYes(string s) {
            return (("yes".Equals(s) || "true".Equals(s)) || "1".Equals(s));
        }
        public static bool isNo(string s) {
            return (("no".Equals(s) || "false".Equals(s)) || "0".Equals(s));
        }
        internal bool processStreamableAtt(string streamableAtt) {
            bool streamable = processBooleanAttribute("streamable", streamableAtt);
            if (streamable) {
                if (!(getConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.ENTERPRISE_XSLT))) {
                    compileWarning("Request for streaming ignored: this Saxon configuration does not support streaming", Saxon.Hej.trans.SaxonErrorCode.SXST0068);
                    return false;
                }
                if ("off".Equals(getConfiguration().getConfigurationProperty(Saxon.Hej.lib.Feature<string>.STREAMABILITY))) {
                    compileWarning("Request for streaming ignored: streaming is disabled in this Saxon configuration", Saxon.Hej.trans.SaxonErrorCode.SXST0068);
                    return false;
                }
            }
            return streamable;
        }
        public Saxon.Hej.value.SequenceType makeSequenceType(string sequenceType) {
            Saxon.Hej.style.ExpressionContext env = getStaticContext();
            int languageLevel = env.getXPathVersion();
            if ((languageLevel == 30)) {
                languageLevel = 305;
            }
            Saxon.Hej.expr.parser.XPathParser parser = getConfiguration().newExpressionParser("XP", false, languageLevel);
            Saxon.Hej.om.QNameParser qp = new Saxon.Hej.om.QNameParser(staticContext.getNamespaceResolver()).withAcceptEQName((staticContext.getXPathVersion()>=30)).withErrorOnBadSyntax("XPST0003").withErrorOnUnresolvedPrefix("XPST0081");
            parser.setQNameParser(qp);
            return parser.parseSequenceType(sequenceType, staticContext);
        }
        internal Saxon.Hej.value.SequenceType makeExtendedSequenceType(string sequenceType) {
            getStaticContext();
            Saxon.Hej.expr.parser.XPathParser parser = getConfiguration().newExpressionParser("XP", false, 31);
            Saxon.Hej.om.QNameParser qp = new Saxon.Hej.om.QNameParser(staticContext.getNamespaceResolver()).withAcceptEQName((staticContext.getXPathVersion()>=30)).withErrorOnBadSyntax("XPST0003").withErrorOnUnresolvedPrefix("XPST0081");
            parser.setQNameParser(qp);
            return parser.parseExtendedSequenceType(sequenceType, staticContext);
        }
        internal void processExtensionElementAttribute(string ns) {
            string ext = getAttributeValue(ns, "extension-element-prefixes");
            if ((ext != null)) {
                int count = 0;
                Saxon.Ejava.util.StringTokenizer st1 = new Saxon.Ejava.util.StringTokenizer(ext, " \t\n\r", false);
                while (st1.hasMoreTokens()) {
                    st1.nextToken();
                    count++;
                }
                extensionNamespaces = new string[count];
                count = 0;
                Saxon.Ejava.util.StringTokenizer st2 = new Saxon.Ejava.util.StringTokenizer(ext, " \t\n\r", false);
                while (st2.hasMoreTokens()) {
                    string s = st2.nextToken();
                    if ("#default".Equals(s)) {
                        s = "";
                    }
                    string uri = getURIForPrefix(s, false);
                    if ((uri == null)) {
                        extensionNamespaces = null;
                        compileError((("Namespace prefix "+s)+" is undeclared"), "XTSE1430");
                    } else {
                        if (Saxon.Hej.lib.NamespaceConstant.isReserved(uri)) {
                            compileError(((("Namespace "+uri)+" is reserved: it cannot be used for extension instructions ")+"(perhaps exclude-result-prefixes was intended)."), "XTSE0085");
                            extensionNamespaces[count++] = uri;
                        } else {
                            extensionNamespaces[count++] = uri;
                        }
                    }
                }
            }
        }
        internal void processExcludedNamespaces(string ns) {
            string ext = getAttributeValue(ns, "exclude-result-prefixes");
            if ((ext != null)) {
                if ("#all".Equals(Saxon.Hej.value.Whitespace.trim(ext))) {
                    System.Collections.Generic.IList<string> excluded = new System.Collections.Generic.List<string>();
                    foreach (Saxon.Hej.om.NamespaceBinding binding in getAllNamespaces()) {
                        excluded.Add(binding.getURI());
                    }
                    excludedNamespaces = excluded.ToArray();
                } else {
                    int count = 0;
                    Saxon.Ejava.util.StringTokenizer st1 = new Saxon.Ejava.util.StringTokenizer(ext, " \t\n\r", false);
                    while (st1.hasMoreTokens()) {
                        st1.nextToken();
                        count++;
                    }
                    excludedNamespaces = new string[count];
                    count = 0;
                    Saxon.Ejava.util.StringTokenizer st2 = new Saxon.Ejava.util.StringTokenizer(ext, " \t\n\r", false);
                    while (st2.hasMoreTokens()) {
                        string s = st2.nextToken();
                        if ("#default".Equals(s)) {
                            s = "";
                        } else {
                            if ("#all".Equals(s)) {
                                compileError("In exclude-result-prefixes, cannot mix #all with other values", "XTSE0020");
                            }
                        }
                        string uri = getURIForPrefix(s, true);
                        if ((uri == null)) {
                            excludedNamespaces = null;
                            compileError((("Namespace prefix "+s)+" is not declared"), "XTSE0808");
                            break ;
                        }
                        excludedNamespaces[count++] = uri;
                        if ((s.Length==0 && uri.Length==0)) {
                            compileError("Cannot exclude the #default namespace when no default namespace is declared", "XTSE0809");
                        }
                    }
                }
            }
        }
        protected internal virtual void processVersionAttribute(string ns) {
            string v = Saxon.Hej.value.Whitespace.trim(getAttributeValue(ns, "version"));
            if ((v != null)) {
                Saxon.Hej.type.ConversionResult val = Saxon.Hej.value.BigDecimalValue.makeDecimalValue(v, true);
                if ((val is Saxon.Hej.type.ValidationFailure)) {
                    version = 30;
                    compileError("The version attribute must be a decimal literal", "XTSE0110");
                } else {
                    version = ((int)(((Saxon.Hej.value.BigDecimalValue)(val)).getDecimalValue()*(Singulink.Numerics.BigDecimal)10));
                    if (((version<20) && (version != 10))) {
                        issueWarning((("Unrecognized version "+val)+": treated as 1.0"), this);
                        version = 10;
                    } else {
                        if (((version>20) && (version<30))) {
                            issueWarning((("Unrecognized version "+val)+": treated as 2.0"), this);
                            version = 20;
                        }
                    }
                }
            }
        }
        internal int getEffectiveVersion() {
            if ((version == -1)) {
                Saxon.Hej.om.NodeInfo node = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
                if ((node is Saxon.Hej.style.StyleElement)) {
                    version = ((Saxon.Hej.style.StyleElement)(node)).getEffectiveVersion();
                } else {
                    return 20;
                }
            }
            return version;
        }
        protected internal int validateValidationAttribute(string value) {
            int code = Saxon.Hej.lib.Validation.getCode(value);
            if ((code == Saxon.Hej.lib.Validation.INVALID)) {
                string prefix = ((this is Saxon.Hej.style.LiteralResultElement) ? "xsl:" : "");
                compileError((((("Invalid value of "+prefix)+"validation attribute: '")+value)+"'"), "XTSE0020");
                code = getDefaultValidation();
            }
            if (!(isSchemaAware())) {
                if ((code == Saxon.Hej.lib.Validation.STRICT)) {
                    compileError("To perform validation, a schema-aware XSLT processor is needed", "XTSE1660");
                }
                code = Saxon.Hej.lib.Validation.STRIP;
            }
            return code;
        }
        protected internal bool isExtensionAttributeAllowed(string attribute) {
            if (getConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                return true;
            } else {
                issueWarning((((("The option "+getDisplayName())+"/@")+attribute)+" is ignored because it requires a Saxon-PE license"), this);
                return false;
            }
        }
        internal bool forwardsCompatibleModeIsEnabled() {
            return (getEffectiveVersion()>30);
        }
        internal bool xPath10ModeIsEnabled() {
            return (getEffectiveVersion()<20);
        }
        internal void processDefaultCollationAttribute() {
            string ns = (getURI().Equals(Saxon.Hej.lib.NamespaceConstant.XSLT) ? "" : Saxon.Hej.lib.NamespaceConstant.XSLT);
            string v = getAttributeValue(ns, "default-collation");
            Saxon.Ejava.lang.StringBuilder reasons = new Saxon.Ejava.lang.StringBuilder();
            if ((v != null)) {
                Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(v, " \t\n\r", false);
                while (st.hasMoreTokens()) {
                    string uri = st.nextToken();
                    if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI)) {
                        defaultCollationName = uri;
                        return ;
                    } else {
                        System.Uri collationURI;
                        try {
                            collationURI = new System.Uri(uri, System.UriKind.RelativeOrAbsolute);
                            if (!(collationURI.IsAbsoluteUri)) {
                                System.Uri @base = new System.Uri(getBaseURI(), System.UriKind.RelativeOrAbsolute);
                                collationURI = new System.Uri(@base, collationURI);
                                uri = collationURI.ToString();
                            }
                        } catch (System.UriFormatException err) {
                            compileError((("default collation '"+uri)+"' is not a valid URI"));
                            uri = Saxon.Hej.lib.NamespaceConstant.CODEPOINT_COLLATION_URI;
                        }
                        try {
                            if ((getConfiguration().getCollation(uri) != null)) {
                                defaultCollationName = uri;
                                return ;
                            } else {
                                if ((reasons.length() != 0)) {
                                    reasons.append("; ");
                                }
                                reasons.append("Collation ").append(uri).append(" is not recognized");
                            }
                        } catch (Saxon.Hej.trans.XPathException e) {
                            if ((reasons.length() != 0)) {
                                reasons.append("; ");
                            }
                            reasons.append("Collation ").append(uri).append(" is not recognized (").append(e.Message).append(")");
                        }
                    }
                }
                string msg = "No recognized collation URI found in default-collation attribute";
                if ((reasons.length() != 0)) {
                    msg += ". ";
                    msg += reasons.ToString();
                }
                compileErrorInAttribute(msg, "XTSE0125", new Saxon.Hej.om.StructuredQName("", ns, "default-collation").getClarkName());
            }
        }
        protected internal string getDefaultCollationName() {
            Saxon.Hej.style.StyleElement e = this;
            while (true) {
                if ((e.defaultCollationName != null)) {
                    return e.defaultCollationName;
                }
                Saxon.Hej.om.NodeInfo p = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)e.getParent());
                if (!(((p is Saxon.Hej.style.StyleElement)))) {
                    break ;
                }
                e = (Saxon.Hej.style.StyleElement)(p);
            }
            return getConfiguration().getDefaultCollationName();
        }
        internal Saxon.Hej.lib.StringCollator findCollation(string name, string baseURI) {
            return getConfiguration().getCollation(name, baseURI);
        }
        internal void processDefaultMode() {
            string ns = (getURI().Equals(Saxon.Hej.lib.NamespaceConstant.XSLT) ? "" : Saxon.Hej.lib.NamespaceConstant.XSLT);
            string v = getAttributeValue(ns, "default-mode");
            if ((v != null)) {
                if (v.Equals("#unnamed")) {
                    defaultMode = Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME;
                } else {
                    defaultMode = makeQName(v, null, "default-mode");
                }
            }
            Saxon.Hej.style.PrincipalStylesheetModule psm = compilation.getPrincipalStylesheetModule();
            Saxon.Hej.om.StructuredQName checkedName = defaultMode;
            if (((psm != null) && psm.isDeclaredModes())) {
                psm.addFixupAction(() => {
                    if ((psm.getRuleManager().obtainMode(checkedName, false) == null)) {
                        Saxon.Hej.trans.XPathException err = new Saxon.Hej.trans.XPathException((("Mode "+checkedName.getDisplayName())+" is not declared in an xsl:mode declaration"), "XTSE3085");
                        err.setLocation(this);
                        throw err;
                    }
                } );
            }
        }
        internal Saxon.Hej.om.StructuredQName getDefaultMode() {
            if ((defaultMode == null)) {
                processDefaultMode();
                if ((defaultMode == null)) {
                    Saxon.Hej.om.NodeInfo p = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
                    if ((p is Saxon.Hej.style.StyleElement)) {
                        return defaultMode = ((Saxon.Hej.style.StyleElement)(p)).getDefaultMode();
                    } else {
                        return defaultMode = Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME;
                    }
                }
            }
            return defaultMode;
        }
        private bool definesExtensionElement(string uri) {
            if ((extensionNamespaces == null)) {
                return false;
            }
            foreach (string extensionNamespace in extensionNamespaces) {
                if (extensionNamespace.Equals(uri)) {
                    return true;
                }
            }
            return false;
        }
        public bool isExtensionNamespace(string uri) {
            Saxon.Hej.om.NodeInfo anc = this;
            while ((anc is Saxon.Hej.style.StyleElement)) {
                if (((Saxon.Hej.style.StyleElement)(anc)).definesExtensionElement(uri)) {
                    return true;
                }
                anc = anc.getParent();
            }
            return false;
        }
        private bool definesExcludedNamespace(string uri) {
            if ((excludedNamespaces == null)) {
                return false;
            }
            foreach (string excludedNamespace in excludedNamespaces) {
                if (excludedNamespace.Equals(uri)) {
                    return true;
                }
            }
            return false;
        }
        internal bool isExcludedNamespace(string uri) {
            if ((uri.Equals(Saxon.Hej.lib.NamespaceConstant.XSLT) || uri.Equals(Saxon.Hej.lib.NamespaceConstant.XML))) {
                return true;
            }
            if (isExtensionNamespace(uri)) {
                return true;
            }
            Saxon.Hej.om.NodeInfo anc = this;
            while ((anc is Saxon.Hej.style.StyleElement)) {
                if (((Saxon.Hej.style.StyleElement)(anc)).definesExcludedNamespace(uri)) {
                    return true;
                }
                anc = anc.getParent();
            }
            return false;
        }
        internal void processDefaultXPathNamespaceAttribute(string ns) {
            string v = getAttributeValue(ns, "xpath-default-namespace");
            if ((v != null)) {
                defaultXPathNamespace = v;
            }
        }
        public string getDefaultXPathNamespace() {
            Saxon.Hej.om.NodeInfo anc = this;
            while ((anc is Saxon.Hej.style.StyleElement)) {
                string x = ((Saxon.Hej.style.StyleElement)(anc)).defaultXPathNamespace;
                if ((x != null)) {
                    return x;
                }
                anc = anc.getParent();
            }
            return compilation.getCompilerInfo().getDefaultElementNamespace();
        }
        internal void processExpandTextAttribute(string ns) {
            string v = getAttributeValue(ns, "expand-text");
            if ((v != null)) {
                expandText = processBooleanAttribute("expand-text", v);
            } else {
                Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
                expandText = ((parent is Saxon.Hej.style.StyleElement) && ((Saxon.Hej.style.StyleElement)(parent)).expandText);
            }
        }
        internal void processDefaultValidationAttribute(string ns) {
            string v = getAttributeValue(ns, "default-validation");
            if ((v != null)) {
                int val = Saxon.Hej.lib.Validation.getCode(v);
                if (((val == Saxon.Hej.lib.Validation.STRIP) || (val == Saxon.Hej.lib.Validation.PRESERVE))) {
                    defaultValidation = val;
                } else {
                    compileErrorInAttribute("@default-validation must be preserve|strip", "XTSE0020", "default-validation");
                }
            }
        }
        internal bool isExpandingText() {
            return expandText;
        }
        public Saxon.Hej.type.SchemaType getSchemaType(string typeAtt) {
            try {
                string uri;
                string lname;
                if (typeAtt.StartsWith("Q{")) {
                    Saxon.Hej.om.StructuredQName q = makeQName(typeAtt, "XTSE1520", "type");
                    uri = q.getURI();
                    lname = q.getLocalPart();
                } else {
                    string[] parts = Saxon.Hej.om.NameChecker.getQNameParts(typeAtt);
                    lname = parts[1];
                    if ("".Equals(parts[0])) {
                        uri = getDefaultXPathNamespace();
                    } else {
                        uri = getURIForPrefix(parts[0], false);
                        if ((uri == null)) {
                            compileError("Namespace prefix for type annotation is undeclared", "XTSE1520");
                            return null;
                        }
                    }
                }
                if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.SCHEMA)) {
                    Saxon.Hej.type.SchemaType t = Saxon.Hej.type.BuiltInType.getSchemaTypeByLocalName(lname);
                    if ((t == null)) {
                        compileError(("Unknown built-in type "+typeAtt), "XTSE1520");
                        return null;
                    }
                    return t;
                }
                if (!(getPrincipalStylesheetModule().isImportedSchema(uri))) {
                    compileError(("There is no imported schema for the namespace of type "+typeAtt), "XTSE1520");
                    return null;
                }
                Saxon.Hej.om.StructuredQName qName = new Saxon.Hej.om.StructuredQName("", uri, lname);
                Saxon.Hej.type.SchemaType stype = getConfiguration().getSchemaType(qName);
                if ((stype == null)) {
                    compileError((("There is no type named "+typeAtt)+" in an imported schema"), "XTSE1520");
                }
                return stype;
            } catch (Saxon.Hej.om.QNameException err) {
                compileError(("Invalid type name. "+err.Message), "XTSE1520");
            }
            return null;
        }
        public Saxon.Hej.type.SimpleType getTypeAnnotation(Saxon.Hej.type.SchemaType schemaType) {
            return (Saxon.Hej.type.SimpleType)(schemaType);
        }
        protected internal Saxon.Hej.expr.Expression arrayToSequence(Saxon.Hej.expr.Expression arrayExpr) {
            try {
                return Saxon.Hej.ma.arrays.ArrayFunctionSet.getInstance().makeFunction("members", 1).makeFunctionCall(arrayExpr);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        protected internal Saxon.Hej.expr.Expression mapToSequence(Saxon.Hej.expr.Expression mapExpr) {
            try {
                return Saxon.Hej.functions.registry.VendorFunctionSetHE.getInstance().makeFunction("map-as-sequence-of-maps", 1).makeFunctionCall(mapExpr);
            } catch (Saxon.Hej.trans.XPathException e) {
                throw new Saxon.Hej.trans.UncheckedXPathException(e);
            }
        }
        public virtual void validate(Saxon.Hej.style.ComponentDeclaration decl) {
        }
        public virtual void postValidate() {
        }
        public virtual void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
        }
        public Saxon.Hej.expr.Expression typeCheck(string name, Saxon.Hej.expr.Expression exp) {
            if ((exp == null)) {
                return null;
            }
            Saxon.Hej.Configuration config = getConfiguration();
            if (config.getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STRICT_STREAMABILITY)) {
                return exp;
            }
            try {
                exp = exp.typeCheck(makeExpressionVisitor(), config.makeContextItemStaticInfo(Saxon.Hej.type.Type.ITEM_TYPE, true));
                exp = Saxon.Hej.expr.parser.ExpressionTool.resolveCallsToCurrentFunction(exp);
                return exp;
            } catch (Saxon.Hej.trans.XPathException err) {
                if (err.isReportableStatically()) {
                    err.setLocation(new Saxon.Hej.tree.AttributeLocation(this, Saxon.Hej.om.StructuredQName.fromClarkName(name)));
                    compileError(err);
                    return exp;
                } else {
                    Saxon.Hej.expr.ErrorExpression erexp = new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(err));
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(exp, erexp);
                    return erexp;
                }
            }
        }
        internal void allocateLocalSlots(Saxon.Hej.expr.Expression exp) {
            Saxon.Hej.expr.instruct.SlotManager slotManager = getContainingSlotManager();
            if ((slotManager == null)) {
                throw new Saxon.Ejava.lang.AssertionError("Slot manager has not been allocated");
            } else {
                int firstSlot = slotManager.getNumberOfVariables();
                int highWater = Saxon.Hej.expr.parser.ExpressionTool.allocateSlots(exp, firstSlot, slotManager);
                if ((highWater>firstSlot)) {
                    slotManager.setNumberOfVariables(highWater);
                }
            }
        }
        public Saxon.Hej.pattern.Pattern typeCheck(string name, Saxon.Hej.pattern.Pattern pattern) {
            if ((pattern == null)) {
                return null;
            }
            try {
                Saxon.Hej.type.ItemType cit = Saxon.Hej.type.Type.ITEM_TYPE;
                pattern = pattern.typeCheck(makeExpressionVisitor(), getConfiguration().makeContextItemStaticInfo(cit, true));
                bool usesCurrent = false;
                foreach (Saxon.Hej.expr.Operand o in pattern.operands()) {
                    Saxon.Hej.expr.Expression filter = o.getChildExpression();
                    if (Saxon.Hej.expr.parser.ExpressionTool.callsFunction(filter, Saxon.Hej.functions.Current.FN_CURRENT, false)) {
                        usesCurrent = true;
                        break ;
                    }
                }
                if (usesCurrent) {
                    Saxon.Hej.pattern.PatternThatSetsCurrent p2 = new Saxon.Hej.pattern.PatternThatSetsCurrent(pattern);
                    pattern.bindCurrent(p2.getCurrentBinding());
                    pattern = p2;
                }
                return pattern;
            } catch (Saxon.Hej.trans.XPathException err) {
                if (err.isReportableStatically()) {
                    Saxon.Hej.trans.XPathException e2 = new Saxon.Hej.trans.XPathException((("Error in "+name)+" pattern"), err);
                    e2.setLocator(this);
                    e2.setErrorCodeQName(err.getErrorCodeQName());
                    throw e2;
                } else {
                    Saxon.Hej.pattern.Pattern p = new Saxon.Hej.pattern.BasePatternWithPredicate(new Saxon.Hej.pattern.NodeTestPattern(Saxon.Hej.type.ErrorType.getInstance()), new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(err)));
                    p.setLocation(allocateLocation());
                    return p;
                }
            }
        }
        public virtual void fixupReferences() {
            foreach (Saxon.Hej.om.NodeInfo child in children(x => (x is Saxon.Hej.style.StyleElement))) {
                ((Saxon.Hej.style.StyleElement)(child)).fixupReferences();
            }
        }
        public Saxon.Hej.expr.instruct.SlotManager getContainingSlotManager() {
            Saxon.Hej.tree.linked.NodeImpl node = this;
            while (true) {
                Saxon.Hej.tree.linked.NodeImpl next = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)node.getParent());
                System.Diagnostics.Debug.Assert((next != null));
                if (((next is Saxon.Hej.style.XSLModuleRoot) || (next.getFingerprint() == Saxon.Hej.om.StandardNames.XSL_OVERRIDE))) {
                    if ((node is Saxon.Hej.style.StylesheetComponent)) {
                        return ((Saxon.Hej.style.StylesheetComponent)(node)).getSlotManager();
                    } else {
                        return null;
                    }
                }
                node = next;
            }
        }
        public virtual void validateSubtree(Saxon.Hej.style.ComponentDeclaration decl, bool excludeStylesheet) {
            if (isActionCompleted(Saxon.Hej.style.StyleElement.ACTION_VALIDATE)) {
                return ;
            }
            setActionCompleted(Saxon.Hej.style.StyleElement.ACTION_VALIDATE);
            if ((validationError != null)) {
                if ((reportingCircumstances == Saxon.Hej.style.StyleElement.OnFailure.REPORT_ALWAYS)) {
                    compileError(validationError);
                } else {
                    if (((reportingCircumstances == Saxon.Hej.style.StyleElement.OnFailure.REPORT_UNLESS_FORWARDS_COMPATIBLE) && !(forwardsCompatibleModeIsEnabled()))) {
                        compileError(validationError);
                    } else {
                        if ((reportingCircumstances == Saxon.Hej.style.StyleElement.OnFailure.REPORT_STATICALLY_UNLESS_FALLBACK_AVAILABLE)) {
                            bool hasFallback = false;
                            foreach (Saxon.Hej.om.NodeInfo child in children(x => (x is Saxon.Hej.style.XSLFallback))) {
                                hasFallback = true;
                                ((Saxon.Hej.style.XSLFallback)(child)).validateSubtree(decl, false);
                            }
                            if (!(hasFallback)) {
                                compileError(validationError);
                            }
                        } else {
                            if ((reportingCircumstances == Saxon.Hej.style.StyleElement.OnFailure.REPORT_DYNAMICALLY_UNLESS_FALLBACK_AVAILABLE)) {
                                foreach (Saxon.Hej.om.NodeInfo child in children(x => (x is Saxon.Hej.style.XSLFallback))) {
                                    ((Saxon.Hej.style.XSLFallback)(child)).validateSubtree(decl, false);
                                }
                            }
                        }
                    }
                }
            } else {
                try {
                    validate(decl);
                } catch (Saxon.Hej.trans.XPathException err) {
                    compileError(err);
                }
                validateChildren(decl, excludeStylesheet);
                if ((getCompilation().getErrorCount() == 0)) {
                    postValidate();
                }
            }
        }
        protected internal virtual void validateChildren(Saxon.Hej.style.ComponentDeclaration decl, bool excludeStylesheet) {
            bool containsInstructions = mayContainSequenceConstructor();
            Saxon.Hej.style.StyleElement lastChild = null;
            bool endsWithTextTemplate = false;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child is Saxon.Hej.style.StyleElement)) {
                    if (!(((excludeStylesheet && (child is Saxon.Hej.style.XSLStylesheet))))) {
                        endsWithTextTemplate = false;
                        if (((containsInstructions && !(((Saxon.Hej.style.StyleElement)(child)).isInstruction())) && !(isPermittedChild((Saxon.Hej.style.StyleElement)(child))))) {
                            ((Saxon.Hej.style.StyleElement)(child)).compileError((((("An "+getDisplayName())+" element must not contain an ")+child.getDisplayName())+" element"), "XTSE0010");
                        }
                        ((Saxon.Hej.style.StyleElement)(child)).validateSubtree(decl, excludeStylesheet);
                        lastChild = (Saxon.Hej.style.StyleElement)(child);
                    }
                } else {
                    endsWithTextTemplate = examineTextNode(child);
                }
            }
            if ((((lastChild is Saxon.Hej.style.XSLLocalVariable) && !(((this is Saxon.Hej.style.XSLStylesheet)))) && !(endsWithTextTemplate))) {
                lastChild.compileWarning("A variable with no following sibling instructions has no effect", Saxon.Hej.trans.SaxonErrorCode.SXWN9001);
            }
        }
        private bool examineTextNode(Saxon.Hej.om.NodeInfo node) {
            if ((node is Saxon.Hej.style.TextValueTemplateNode)) {
                ((Saxon.Hej.style.TextValueTemplateNode)(node)).validate();
                return !(((((Saxon.Hej.style.TextValueTemplateNode)(node)).getContentExpression() is Saxon.Hej.expr.Literal)));
            } else {
                return false;
            }
        }
        protected internal virtual bool isPermittedChild(Saxon.Hej.style.StyleElement child) {
            return false;
        }
        public Saxon.Hej.style.PrincipalStylesheetModule getPrincipalStylesheetModule() {
            return getCompilation().getPrincipalStylesheetModule();
        }
        public Saxon.Hej.style.StylesheetPackage getContainingPackage() {
            Saxon.Hej.style.PrincipalStylesheetModule psm = getPrincipalStylesheetModule();
            return ((psm == null) ? null : psm.getStylesheetPackage());
        }
        internal void checkSortComesFirst(bool sortRequired) {
            bool sortFound = false;
            bool nonSortFound = false;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                if ((child is Saxon.Hej.style.XSLSort)) {
                    if (nonSortFound) {
                        ((Saxon.Hej.style.XSLSort)(child)).compileError((("Within "+getDisplayName())+", xsl:sort elements must come before other instructions"), "XTSE0010");
                    }
                    sortFound = true;
                } else {
                    if ((child.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                        if (!(Saxon.Hej.value.Whitespace.isAllWhite(child.getUnicodeStringValue()))) {
                            nonSortFound = true;
                        }
                    } else {
                        nonSortFound = true;
                    }
                }
            }
            if ((sortRequired && !(sortFound))) {
                compileError((getDisplayName()+" must have at least one xsl:sort child"), "XTSE0010");
            }
        }
        public void checkTopLevel(string errorCode, bool allowOverride) {
            Saxon.Hej.tree.linked.NodeImpl parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            System.Diagnostics.Debug.Assert((parent != null));
            if ((parent.getFingerprint() == Saxon.Hej.om.StandardNames.XSL_OVERRIDE)) {
                if (!(allowOverride)) {
                    compileError((("Element "+getDisplayName())+" is not allowed as a child of xsl:override"));
                }
            } else {
                if (!(isTopLevel())) {
                    compileError((("Element "+getDisplayName())+" must be top-level (a child of xsl:stylesheet, xsl:transform, or xsl:package)"), errorCode);
                }
            }
        }
        public void checkEmpty() {
            if (hasChildNodes()) {
                compileError("Element must be empty", "XTSE0260");
            }
        }
        public void reportAbsence(string attribute) {
            compileError((("Element must have an "+Saxon.Hej.trans.Err.wrap(attribute, Saxon.Hej.trans.Err.ATTRIBUTE))+" attribute"), "XTSE0010");
        }
        public virtual Saxon.Hej.expr.Expression compile(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            return null;
        }
        protected internal virtual bool isWithinDeclaredStreamableConstruct() {
            if (getURI().Equals(Saxon.Hej.lib.NamespaceConstant.XSLT)) {
                string streamableAtt = getAttributeValue("streamable");
                if ((streamableAtt != null)) {
                    return processStreamableAtt(streamableAtt);
                }
            }
            Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            return ((parent is Saxon.Hej.style.StyleElement) && ((Saxon.Hej.style.StyleElement)(parent)).isWithinDeclaredStreamableConstruct());
        }
        protected internal string generateId() {
            Saxon.Ejava.lang.StringBuilder buff = new Saxon.Ejava.lang.StringBuilder(16);
            generateId(buff);
            return buff.ToString();
        }
        public virtual void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
        }
        public virtual Saxon.Hej.expr.Expression compileSequenceConstructor(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl, bool includeParams) {
            bool containsEmptyTest = false;
            foreach (Saxon.Hej.om.NodeInfo child in children()) {
                int fp = child.getFingerprint();
                if (((fp == Saxon.Hej.om.StandardNames.XSL_ON_EMPTY) || (fp == Saxon.Hej.om.StandardNames.XSL_ON_NON_EMPTY))) {
                    containsEmptyTest = true;
                }
            }
            if (containsEmptyTest) {
                System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> vars = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>();
                System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> onEmpties = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>();
                System.Collections.Generic.IList<Saxon.Hej.om.NodeInfo> others = new System.Collections.Generic.List<Saxon.Hej.om.NodeInfo>();
                foreach (Saxon.Hej.om.NodeInfo kid in children()) {
                    int fp = kid.getFingerprint();
                    if (((fp == Saxon.Hej.om.StandardNames.XSL_VARIABLE) || (fp == Saxon.Hej.om.StandardNames.XSL_PARAM))) {
                        vars.Add(kid);
                    } else {
                        if ((fp == Saxon.Hej.om.StandardNames.XSL_ON_EMPTY)) {
                            onEmpties.Add(kid);
                        } else {
                            others.Add(kid);
                        }
                    }
                }
                Saxon.Impl.Helpers.ListUtils.AddAll(vars, others);
                Saxon.Impl.Helpers.ListUtils.AddAll(vars, onEmpties);
                return compileSequenceConstructor(compilation, decl, new Saxon.Hej.tree.iter.NodeListIterator(vars), includeParams);
            } else {
                return compileSequenceConstructor(compilation, decl, iterateAxis(Saxon.Hej.om.AxisInfo.CHILD), includeParams);
            }
        }
        public Saxon.Hej.expr.Expression compileSequenceConstructor(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.om.SequenceIterator iter, bool includeParams) {
            Saxon.Hej.s9api.Location locationId = allocateLocation();
            System.Collections.Generic.IList<Saxon.Hej.expr.Expression> contents = new System.Collections.Generic.List<Saxon.Hej.expr.Expression>(10);
            bool containsSpecials = false;
            Saxon.Hej.om.NodeInfo node;
            while (((node = (Saxon.Hej.om.NodeInfo)(iter.next())) != null)) {
                if ((node.getNodeKind() == Saxon.Hej.type.Type.TEXT)) {
                    if (isExpandingText()) {
                        compileContentValueTemplate((Saxon.Hej.tree.linked.TextImpl)(node), contents);
                    } else {
                        Saxon.Hej.tree.iter.AxisIterator lookahead = node.iterateAxis(Saxon.Hej.om.AxisInfo.FOLLOWING_SIBLING);
                        Saxon.Hej.om.NodeInfo sibling = ((/*1430*/Saxon.Hej.om.NodeInfo)lookahead.next());
                        if (!((((((sibling is Saxon.Hej.style.XSLLocalParam) || (sibling is Saxon.Hej.style.XSLSort)) || (sibling is Saxon.Hej.style.XSLContextItem)) || (sibling is Saxon.Hej.style.XSLOnCompletion))))) {
                            Saxon.Hej.expr.Expression text = new Saxon.Hej.expr.instruct.ValueOf(new Saxon.Hej.expr.StringLiteral(node.getUnicodeStringValue()), false, false);
                            text.setLocation(allocateLocation());
                            contents.Add(text);
                        }
                    }
                } else {
                    if ((node is Saxon.Hej.style.XSLLocalVariable)) {
                        Saxon.Hej.style.XSLLocalVariable var = (Saxon.Hej.style.XSLLocalVariable)(node);
                        Saxon.Hej.style.SourceBinding sourceBinding = var.getSourceBinding();
                        var.compileLocalVariable(compilation, decl);
                        Saxon.Hej.expr.Expression tail = compileSequenceConstructor(compilation, decl, iter, includeParams);
                        if (((tail == null) || Saxon.Hej.expr.Literal.isEmptySequence(tail))) {
                        } else {
                            Saxon.Hej.expr.LetExpression let = new Saxon.Hej.expr.LetExpression();
                            let.setInstruction(true);
                            let.setRequiredType(var.getRequiredType());
                            let.setVariableQName(sourceBinding.getVariableQName());
                            let.setSequence(sourceBinding.getSelectExpression());
                            let.setAction(tail);
                            sourceBinding.fixupBinding(let);
                            locationId = ((Saxon.Hej.style.StyleElement)(node)).allocateLocation();
                            let.setLocation(locationId);
                            contents.Add(let);
                            if (var.changesRetainedStaticContext()) {
                                let.setRetainedStaticContext(makeRetainedStaticContext());
                            }
                        }
                    } else {
                        if ((node is Saxon.Hej.style.StyleElement)) {
                            Saxon.Hej.style.StyleElement snode = (Saxon.Hej.style.StyleElement)(node);
                            int fp = snode.getFingerprint();
                            if (((fp == Saxon.Hej.om.StandardNames.XSL_ON_EMPTY) || (fp == Saxon.Hej.om.StandardNames.XSL_ON_NON_EMPTY))) {
                                containsSpecials = true;
                            }
                            Saxon.Hej.expr.Expression child;
                            if (((snode.validationError != null) && !(((snode is Saxon.Hej.style.AbsentExtensionElement))))) {
                                if ((snode.reportingCircumstances == Saxon.Hej.style.StyleElement.OnFailure.REPORT_IF_INSTANTIATED)) {
                                    child = new Saxon.Hej.expr.ErrorExpression(snode.validationError);
                                } else {
                                    child = fallbackProcessing(compilation, decl, snode);
                                }
                            } else {
                                child = snode.compile(compilation, decl);
                                if ((child != null)) {
                                    if (snode.changesRetainedStaticContext()) {
                                        child.setRetainedStaticContext(snode.makeRetainedStaticContext());
                                    }
                                    setInstructionLocation(snode, child);
                                }
                            }
                            if ((child != null)) {
                                contents.Add(child);
                            }
                        }
                    }
                }
            }
            if (containsSpecials) {
                return new Saxon.Hej.expr.instruct.ConditionalBlock(contents);
            }
            Saxon.Hej.expr.Expression block = Saxon.Hej.expr.instruct.Block.makeBlock(contents);
            if ((block.getLocation() == null)) {
                block.setLocation(locationId);
            }
            if ((block.getLocalRetainedStaticContext() == null)) {
                block.setRetainedStaticContext(makeRetainedStaticContext());
            }
            return block;
        }
        internal void compileContentValueTemplate(Saxon.Hej.tree.linked.TextImpl node, System.Collections.Generic.IList<Saxon.Hej.expr.Expression> contents) {
            if ((node is Saxon.Hej.style.TextValueTemplateNode)) {
                Saxon.Hej.expr.Expression exp = ((Saxon.Hej.style.TextValueTemplateNode)(node)).getContentExpression();
                if ((getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STRICT_STREAMABILITY) && !(((exp is Saxon.Hej.expr.Literal))))) {
                    exp = new Saxon.Hej.expr.instruct.SequenceInstr(exp);
                }
                contents.Add(exp);
            } else {
                contents.Add(new Saxon.Hej.expr.StringLiteral(node.getUnicodeStringValue()));
            }
        }
        protected internal static void setInstructionLocation(Saxon.Hej.style.StyleElement source, Saxon.Hej.expr.Expression child) {
            child.setLocation(source.allocateLocation());
        }
        internal Saxon.Hej.expr.Expression fallbackProcessing(Saxon.Hej.style.Compilation exec, Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.StyleElement instruction) {
            Saxon.Hej.expr.Expression fallback = null;
            foreach (Saxon.Hej.om.NodeInfo child in children(x => (x is Saxon.Hej.style.XSLFallback))) {
                Saxon.Hej.expr.Expression b = ((Saxon.Hej.style.XSLFallback)(child)).compileSequenceConstructor(exec, decl, true);
                if ((b == null)) {
                    b = Saxon.Hej.expr.Literal.makeEmptySequence();
                }
                if ((fallback == null)) {
                    fallback = b;
                } else {
                    fallback = Saxon.Hej.expr.instruct.Block.makeBlock(fallback, b);
                    fallback.setLocation(allocateLocation());
                }
            }
            if ((fallback != null)) {
                return fallback;
            } else {
                return new Saxon.Hej.expr.ErrorExpression(instruction.validationError);
            }
        }
        protected internal Saxon.Hej.s9api.Location allocateLocation() {
            if ((savedLocation == null)) {
                savedLocation = new Saxon.Hej.expr.parser.Loc(this);
            }
            return savedLocation;
        }
        internal Saxon.Hej.expr.sort.SortKeyDefinitionList makeSortKeys(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            int numberOfSortKeys = 0;
            foreach (Saxon.Hej.om.NodeInfo child in children(x => (x is Saxon.Hej.style.XSLSortOrMergeKey))) {
                ((Saxon.Hej.style.XSLSortOrMergeKey)(child)).compile(compilation, decl);
                if ((child is Saxon.Hej.style.XSLSort)) {
                    if (((numberOfSortKeys != 0) && (((Saxon.Hej.style.XSLSort)(child)).getStable() != null))) {
                        compileError("stable attribute may appear only on the first xsl:sort element", "XTSE1017");
                    }
                }
                numberOfSortKeys++;
            }
            if ((numberOfSortKeys>0)) {
                Saxon.Hej.expr.sort.SortKeyDefinition[] keys = new Saxon.Hej.expr.sort.SortKeyDefinition[numberOfSortKeys];
                int k = 0;
                foreach (Saxon.Hej.om.NodeInfo child in children(x => (x is Saxon.Hej.style.XSLSortOrMergeKey))) {
                    keys[k++] = (Saxon.Hej.expr.sort.SortKeyDefinition)(((Saxon.Hej.style.XSLSortOrMergeKey)(child)).getSortKeyDefinition().simplify());
                }
                return new Saxon.Hej.expr.sort.SortKeyDefinitionList(keys);
            } else {
                return null;
            }
        }
        internal Saxon.Hej.om.StructuredQName[] getUsedAttributeSets(string use) {
            System.Collections.Generic.IList<Saxon.Hej.om.StructuredQName> nameList = new System.Collections.Generic.List<Saxon.Hej.om.StructuredQName>(4);
            Saxon.Ejava.util.StringTokenizer st = new Saxon.Ejava.util.StringTokenizer(use, " \t\n\r", false);
            while (st.hasMoreTokens()) {
                string asetname = st.nextToken();
                Saxon.Hej.om.StructuredQName name = makeQName(asetname, "XTSE0710", "use-attribute-sets");
                nameList.Add(name);
            }
            return nameList.ToArray();
        }
        internal Saxon.Hej.trans.Visibility interpretVisibilityValue(string s, string flags) {
            foreach (Saxon.Hej.trans.Visibility v in System.Enum.GetValues(typeof(Saxon.Hej.trans.Visibility))) {
                if (((v.ToString().ToLower().Equals(s) && ((flags.Contains("h") || !(s.Equals("hidden"))))) && ((flags.Contains("a") || !(s.Equals("absent")))))) {
                    return v;
                }
            }
            invalidAttribute("visibility", (("public|final|private|abstract"+((flags.Contains("h") ? "|hidden" : "")))+((flags.Contains("a") ? "|absent" : ""))));
            return Saxon.Hej.trans.Visibility.UNDEFINED;
        }
        public Saxon.Hej.expr.instruct.WithParam[] getWithParamInstructions(Saxon.Hej.expr.Expression parent, Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl, bool tunnel) {
            int count = 0;
            foreach (Saxon.Hej.om.NodeInfo child in children(x => (x is Saxon.Hej.style.XSLWithParam))) {
                Saxon.Hej.style.XSLWithParam wp = (Saxon.Hej.style.XSLWithParam)(child);
                if ((wp.getSourceBinding().hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.TUNNEL) == tunnel)) {
                    count++;
                }
            }
            if ((count == 0)) {
                return Saxon.Hej.expr.instruct.WithParam.EMPTY_ARRAY;
            }
            Saxon.Hej.expr.instruct.WithParam[] array = new Saxon.Hej.expr.instruct.WithParam[count];
            count = 0;
            foreach (Saxon.Hej.om.NodeInfo child in children(x => (x is Saxon.Hej.style.XSLWithParam))) {
                Saxon.Hej.style.XSLWithParam wp = (Saxon.Hej.style.XSLWithParam)(child);
                if ((wp.getSourceBinding().hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.TUNNEL) == tunnel)) {
                    Saxon.Hej.expr.instruct.WithParam p = wp.compileWithParam(parent, compilation, decl);
                    if (((((/*1430*/Saxon.Hej.tree.linked.NodeImpl)wp.getParent()) is Saxon.Hej.style.XSLNextIteration) && wp.hasChildNodes())) {
                        Saxon.Hej.value.SequenceType required = ((Saxon.Hej.style.XSLNextIteration)(((/*1430*/Saxon.Hej.tree.linked.NodeImpl)wp.getParent()))).getDeclaredParamType(wp.getSourceBinding().getVariableQName());
                        wp.checkAgainstRequiredType(required);
                        p.getSelectOperand().setChildExpression(wp.sourceBinding.getSelectExpression());
                    }
                    array[count++] = p;
                }
            }
            return array;
        }
        public void compileError(Saxon.Hej.s9api.XmlProcessingError error) {
            Saxon.Hej.trans.XmlProcessingIncident.maybeSetHostLanguage(error, Saxon.Api.HostLanguage.XSLT);
            if (((error.getLocation() == null) || (((((error.getLocation() is Saxon.Hej.expr.parser.Loc) || (error.getLocation() is Saxon.Hej.expr.Expression))) && !(((this is Saxon.Hej.style.StylesheetComponent))))))) {
                Saxon.Hej.trans.XmlProcessingIncident.maybeSetLocation(error, this);
            }
            getCompilation().reportError(error);
        }
        public void compileError(Saxon.Hej.trans.XPathException err) {
            if ((err.getLocator() == null)) {
                err.setLocation(this);
            }
            Saxon.Hej.trans.XmlProcessingIncident se = new Saxon.Hej.trans.XmlProcessingIncident(err.Message, err.getErrorCodeLocalPart(), err.getLocator());
            se.setHostLanguage(Saxon.Api.HostLanguage.XSLT);
            compileError(se);
        }
        public void compileError(string message) {
            compileError(message, "XTSE0010");
        }
        public void compileError(string message, Saxon.Hej.om.StructuredQName errorCode) {
            Saxon.Hej.trans.XmlProcessingIncident error = new Saxon.Hej.trans.XmlProcessingIncident(message, errorCode.getEQName(), this);
            error.setHostLanguage(Saxon.Api.HostLanguage.XSLT);
            compileError(error);
        }
        public void compileError(string message, string errorCode) {
            compileError(new Saxon.Hej.trans.XPathException(message, errorCode, this));
        }
        public void compileError(string message, string errorCode, Saxon.Hej.s9api.Location loc) {
            compileError(new Saxon.Hej.trans.XPathException(message, errorCode, loc));
        }
        public void compileErrorInAttribute(string message, string errorCode, string attributeName) {
            Saxon.Hej.om.StructuredQName att = Saxon.Hej.om.StructuredQName.fromClarkName(attributeName);
            Saxon.Hej.s9api.Location location = new Saxon.Hej.tree.AttributeLocation(this, att);
            compileError(new Saxon.Hej.trans.XPathException(message, errorCode, location));
        }
        protected internal void invalidAttribute(string attributeName, string allowedValues) {
            compileErrorInAttribute(((((("Attribute "+getDisplayName())+"/@")+attributeName)+" must be ")+allowedValues), "XTSE0020", attributeName);
        }
        protected internal void requireXslt40(string attributeName) {
            if ((compilation.getCompilerInfo().getXsltVersion() != 40)) {
                if ((attributeName == null)) {
                    compileError((("Element "+getDisplayName())+" is allowed only if XSLT 4.0 is enabled"), "XTSE0010");
                } else {
                    compileErrorInAttribute((((("Attribute "+getDisplayName())+"/@")+attributeName)+" is allowed only if XSLT 4.0 is enabled"), "XTSE0020", attributeName);
                }
            }
        }
        internal void undeclaredNamespaceError(string prefix, string errorCode, string attributeName) {
            if ((errorCode == null)) {
                errorCode = "XTSE0280";
            }
            compileErrorInAttribute(("Undeclared namespace prefix "+Saxon.Hej.trans.Err.wrap(prefix)), errorCode, attributeName);
        }
        public void compileWarning(string message, Saxon.Hej.om.StructuredQName errorCode) {
            getCompilation().reportWarning(message, errorCode.getEQName(), this);
        }
        public void compileWarning(string message, string errorCode) {
            getCompilation().reportWarning(message, errorCode, this);
        }
        public void compileWarning(string message, string errorCode, Saxon.Hej.s9api.Location location) {
            getCompilation().reportWarning(message, errorCode, location);
        }
        protected internal void issueWarning(Saxon.Hej.trans.XPathException error) {
            if ((error.getLocator() == null)) {
                error.setLocator(this);
            }
            getCompilation().reportWarning(error);
        }
        protected internal void issueWarning(string message, Saxon.Ejavax.xml.transform.SourceLocator locator) {
            Saxon.Hej.trans.XPathException tce = new Saxon.Hej.trans.XPathException(message);
            if ((locator == null)) {
                tce.setLocator(this);
            } else {
                tce.setLocator(locator);
            }
            issueWarning(tce);
        }
        public bool isTopLevel() {
            return (((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent()) is Saxon.Hej.style.XSLModuleRoot);
        }
        internal bool isConstructingComplexContent() {
            if (!(isInstruction())) {
                return false;
            }
            Saxon.Hej.om.NodeInfo parent = ((/*1430*/Saxon.Hej.tree.linked.NodeImpl)getParent());
            while (true) {
                if (!((((parent is Saxon.Hej.style.StyleElement) && ((Saxon.Hej.style.StyleElement)(parent)).isInstruction())))) {
                    return false;
                }
                if ((parent is Saxon.Hej.style.XSLGeneralVariable)) {
                    return (((Saxon.Hej.style.XSLGeneralVariable)(parent)).getAttributeValue("as") == null);
                }
                if (((((parent is Saxon.Hej.style.XSLElement) || (parent is Saxon.Hej.style.LiteralResultElement)) || (parent is Saxon.Hej.style.XSLDocument)) || (parent is Saxon.Hej.style.XSLCopy))) {
                    return true;
                }
                parent = parent.getParent();
            }
        }
        public virtual Saxon.Hej.style.SourceBinding getBindingInformation(Saxon.Hej.om.StructuredQName name) {
            return null;
        }
        public Saxon.Hej.style.SourceBinding bindVariable(Saxon.Hej.om.StructuredQName qName) {
            Saxon.Hej.style.SourceBinding decl = bindLocalVariable(qName);
            if ((decl != null)) {
                return decl;
            }
            Saxon.Hej.style.SourceBinding binding = getPrincipalStylesheetModule().getGlobalVariableBinding(qName);
            if (((binding == null) || Saxon.Hej.tree.util.Navigator.isAncestorOrSelf(binding.getSourceElement(), this))) {
                return null;
            } else {
                return binding;
            }
        }
        public Saxon.Hej.style.SourceBinding bindLocalVariable(Saxon.Hej.om.StructuredQName qName) {
            Saxon.Hej.om.NodeInfo curr = this;
            Saxon.Hej.om.NodeInfo prev = this;
            Saxon.Hej.style.SourceBinding @implicit = hasImplicitBinding(qName);
            if ((@implicit != null)) {
                return @implicit;
            }
            if (!(isTopLevel())) {
                Saxon.Hej.tree.iter.AxisIterator preceding = curr.iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING);
                while (true) {
                    curr = ((/*1430*/Saxon.Hej.om.NodeInfo)preceding.next());
                    while ((curr == null)) {
                        curr = prev.getParent();
                        if ((curr is Saxon.Hej.style.StyleElement)) {
                            @implicit = ((Saxon.Hej.style.StyleElement)(curr)).hasImplicitBinding(qName);
                            if ((@implicit != null)) {
                                return @implicit;
                            }
                        }
                        while (((curr is Saxon.Hej.style.StyleElement) && !(((Saxon.Hej.style.StyleElement)(curr)).seesAvuncularVariables()))) {
                            curr = curr.getParent();
                        }
                        prev = curr;
                        if ((curr.getParent() is Saxon.Hej.style.XSLModuleRoot)) {
                            break ;
                        }
                        preceding = curr.iterateAxis(Saxon.Hej.om.AxisInfo.PRECEDING_SIBLING);
                        curr = ((/*1430*/Saxon.Hej.om.NodeInfo)preceding.next());
                    }
                    if ((curr.getParent() is Saxon.Hej.style.XSLModuleRoot)) {
                        break ;
                    }
                    if ((curr is Saxon.Hej.style.XSLGeneralVariable)) {
                        Saxon.Hej.style.SourceBinding sourceBinding = ((Saxon.Hej.style.XSLGeneralVariable)(curr)).getBindingInformation(qName);
                        if ((sourceBinding != null)) {
                            return sourceBinding;
                        }
                    }
                }
            }
            return null;
        }
        protected internal virtual bool seesAvuncularVariables() {
            return true;
        }
        protected internal virtual Saxon.Hej.style.SourceBinding hasImplicitBinding(Saxon.Hej.om.StructuredQName name) {
            return null;
        }
        public virtual Saxon.Hej.om.StructuredQName getObjectName() {
            return objectName;
        }
        public void setObjectName(Saxon.Hej.om.StructuredQName qName) {
            objectName = qName;
        }
        public System.Collections.Generic.IEnumerator<string> getProperties() {
            System.Collections.Generic.IList<string> list = new System.Collections.Generic.List<string>(10);
            foreach (Saxon.Hej.om.AttributeInfo att in attributes()) {
                list.Add(att.getNodeName().getStructuredQName().getClarkName());
            }
            return list.GetEnumerator();
        }
        internal bool isActionCompleted(int action) {
            return (((actionsCompleted&action)) != 0);
        }
        internal void setActionCompleted(int action) {
            actionsCompleted |= action;
        }
    
    }
}