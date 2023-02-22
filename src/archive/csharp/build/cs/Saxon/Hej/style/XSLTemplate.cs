using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public sealed class XSLTemplate : Saxon.Hej.style.StyleElement, Saxon.Hej.style.StylesheetComponent {
        private string matchAtt = null;
        private string modeAtt = null;
        private string nameAtt = null;
        private string priorityAtt = null;
        private string asAtt = null;
        private string visibilityAtt = null;
        private Saxon.Hej.om.StructuredQName[] modeNames;
        private string diagnosticId;
        private Saxon.Hej.pattern.Pattern match;
        private bool prioritySpecified;
        private double priority;
        private Saxon.Hej.expr.instruct.SlotManager stackFrameMap;
        private Saxon.Hej.expr.instruct.NamedTemplate compiledNamedTemplate;
        private readonly System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.TemplateRule> compiledTemplateRules = new System.Collections.Generic.Dictionary<Saxon.Hej.om.StructuredQName, Saxon.Hej.expr.instruct.TemplateRule>();
        private Saxon.Hej.value.SequenceType requiredType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
        private bool declaresRequiredType = false;
        private bool hasRequiredParams = false;
        private bool isTailRecursive = false;
        private Saxon.Hej.trans.Visibility visibility = Saxon.Hej.trans.Visibility.PRIVATE;
        private Saxon.Hej.type.ItemType requiredContextItemType = Saxon.Hej.type.AnyItemType.getInstance();
        private bool mayOmitContextItem = true;
        private bool absentFocus = false;
        private bool jitCompilationDone = false;
        private bool explaining;
        public Saxon.Hej.expr.instruct.Actor getActor() {
            return compiledNamedTemplate;
        }
        public override void setCompilation(Saxon.Hej.style.Compilation compilation) {
            base.setCompilation(compilation);
        }
        public override bool isDeclaration() {
            return true;
        }
        public bool isDeferredCompilation(Saxon.Hej.style.Compilation compilation) {
            return ((compilation.isPreScan() && (getTemplateName() == null)) && !(compilation.isLibraryPackage()));
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override bool mayContainParam() {
            return true;
        }
        protected internal override bool isWithinDeclaredStreamableConstruct() {
            try {
                foreach (Saxon.Hej.trans.Mode m in getApplicableModes()) {
                    if (m.isDeclaredStreamable()) {
                        return true;
                    }
                }
            } catch (Saxon.Hej.trans.XPathException e) {
                return false;
            }
            return false;
        }
        public void setContextItemRequirements(Saxon.Hej.type.ItemType type, bool mayBeOmitted, bool absentFocus) {
            requiredContextItemType = type;
            mayOmitContextItem = mayBeOmitted;
            this.absentFocus = absentFocus;
        }
        protected internal override bool isPermittedChild(Saxon.Hej.style.StyleElement child) {
            return ((child is Saxon.Hej.style.XSLLocalParam) || (child.getFingerprint() == Saxon.Hej.om.StandardNames.XSL_CONTEXT_ITEM));
        }
        public Saxon.Hej.om.StructuredQName getTemplateName() {
            if ((getObjectName() == null)) {
                string nameAtt = getAttributeValue("", "name");
                if ((nameAtt != null)) {
                    setObjectName(makeQName(nameAtt, null, "name"));
                }
            }
            return getObjectName();
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicName() {
            if ((getTemplateName() == null)) {
                return null;
            } else {
                return new Saxon.Hej.trans.SymbolicName(Saxon.Hej.om.StandardNames.XSL_TEMPLATE, getTemplateName());
            }
        }
        public Saxon.Hej.type.ItemType getRequiredContextItemType() {
            return requiredContextItemType;
        }
        public bool isMayOmitContextItem() {
            return mayOmitContextItem;
        }
        public void checkCompatibility(Saxon.Hej.expr.Component component) {
            Saxon.Hej.expr.instruct.NamedTemplate other = (Saxon.Hej.expr.instruct.NamedTemplate)(component.getActor());
            if (!(getSymbolicName().Equals(other.getSymbolicName()))) {
                throw new Saxon.Ejava.lang.IllegalArgumentException();
            }
            Saxon.Hej.value.SequenceType req = ((requiredType == null) ? Saxon.Hej.value.SequenceType.ANY_SEQUENCE : requiredType);
            if (!(req.Equals(other.getRequiredType()))) {
                compileError("The overriding template has a different required type from the overridden template", "XTSE3070");
                return ;
            }
            if (((!(requiredContextItemType.Equals(other.getRequiredContextItemType())) || (mayOmitContextItem != other.isMayOmitContextItem())) || (absentFocus != other.isAbsentFocus()))) {
                compileError("The required context item for the overriding template differs from that of the overridden template", "XTSE3070");
                return ;
            }
            System.Collections.Generic.IList<Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo> otherParams = other.getLocalParamDetails();
            System.Collections.Generic.ISet<Saxon.Hej.om.StructuredQName> overriddenParams = new System.Collections.Generic.HashSet<Saxon.Hej.om.StructuredQName>();
            foreach (Saxon.Hej.expr.instruct.NamedTemplate.LocalParamInfo lp0 in otherParams) {
                Saxon.Hej.style.XSLLocalParam lp1 = getParam(lp0.name);
                if ((lp1 == null)) {
                    if (!(lp0.isTunnel)) {
                        compileError((("The overridden template declares a parameter "+lp0.name.getDisplayName())+" which is not declared in the overriding template"), "XTSE3070");
                    }
                    return ;
                }
                if (!(lp1.getRequiredType().Equals(lp0.requiredType))) {
                    lp1.compileError((("The parameter "+lp0.name.getDisplayName())+" has a different required type in the overridden template"), "XTSE3070");
                    return ;
                }
                if (((lp1.isRequiredParam() != lp0.isRequired) && !(lp0.isTunnel))) {
                    lp1.compileError((((((("The parameter "+lp0.name.getDisplayName())+" is ")+((lp1.isRequiredParam() ? "required" : "optional")))+" in the overriding template, but ")+((lp0.isRequired ? "required" : "optional")))+" in the overridden template"), "XTSE3070");
                    return ;
                }
                if ((lp1.isTunnelParam() != lp0.isTunnel)) {
                    lp1.compileError((((((("The parameter "+lp0.name.getDisplayName())+" is a ")+((lp1.isTunnelParam() ? "tunnel" : "non-tunnel")))+" parameter in the overriding template, but ")+((lp0.isTunnel ? "tunnel" : "non-tunnel")))+" parameter in the overridden template"), "XTSE3070");
                    return ;
                }
                overriddenParams.Add(lp0.name);
            }
            foreach (Saxon.Hej.om.NodeInfo param in children(x => (x is Saxon.Hej.style.XSLLocalParam))) {
                if ((!(overriddenParams.Contains(((Saxon.Hej.style.XSLLocalParam)(param)).getObjectName())) && ((Saxon.Hej.style.XSLLocalParam)(param)).isRequiredParam())) {
                    ((Saxon.Hej.style.XSLLocalParam)(param)).compileError("An overriding template cannot introduce a required parameter that is not declared in the overridden template", "XTSE3070");
                }
            }
        }
        public Saxon.Hej.style.XSLLocalParam getParam(Saxon.Hej.om.StructuredQName name) {
            foreach (Saxon.Hej.om.NodeInfo param in children(x => (x is Saxon.Hej.style.XSLLocalParam))) {
                if (name.Equals(((Saxon.Hej.style.XSLLocalParam)(param)).getObjectName())) {
                    return (Saxon.Hej.style.XSLLocalParam)(param);
                }
            }
            return null;
        }
        protected internal override void prepareAttributes() {
            Saxon.Hej.om.AttributeMap atts = attributes();
            string extraAsAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in atts) {
                Saxon.Hej.om.NodeName name = att.getNodeName();
                string f = name.getDisplayName();
                if (f.Equals("mode")) {
                    modeAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                } else {
                    if (f.Equals("name")) {
                        nameAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                    } else {
                        if (f.Equals("match")) {
                            matchAtt = att.getValue();
                        } else {
                            if (f.Equals("priority")) {
                                priorityAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                            } else {
                                if (f.Equals("as")) {
                                    asAtt = att.getValue();
                                } else {
                                    if (f.Equals("visibility")) {
                                        visibilityAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                                    } else {
                                        if (name.hasURI(Saxon.Hej.lib.NamespaceConstant.SAXON)) {
                                            isExtensionAttributeAllowed(name.getDisplayName());
                                            if (name.getLocalPart().Equals("as")) {
                                                extraAsAtt = att.getValue();
                                            } else {
                                                if (name.getLocalPart().Equals("explain")) {
                                                    explaining = isYes(Saxon.Hej.value.Whitespace.trim(att.getValue()));
                                                }
                                            }
                                        } else {
                                            checkUnknownAttribute(name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            try {
                if ((modeAtt == null)) {
                    if ((matchAtt != null)) {
                        Saxon.Hej.om.StructuredQName defaultMode = getDefaultMode();
                        if ((defaultMode == null)) {
                            defaultMode = Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME;
                        }
                        modeNames = new Saxon.Hej.om.StructuredQName[1];
                        modeNames[0] = defaultMode;
                    }
                } else {
                    if ((matchAtt == null)) {
                        compileError("The mode attribute must be absent if the match attribute is absent", "XTSE0500");
                    }
                    getModeNames();
                }
            } catch (Saxon.Hej.trans.XPathException err) {
                err.maybeSetErrorCode("XTSE0280");
                if (err.getErrorCodeLocalPart().Equals("XTSE0020")) {
                    err.setErrorCode("XTSE0550");
                }
                err.setIsStaticError(true);
                compileError(err);
            }
            if ((nameAtt != null)) {
                if ((getObjectName() == null)) {
                    Saxon.Hej.om.StructuredQName qName = makeQName(nameAtt, "XTSE0280", "name");
                    setObjectName(qName);
                }
                if ((compiledNamedTemplate != null)) {
                    compiledNamedTemplate.setTemplateName(getObjectName());
                }
                diagnosticId = nameAtt;
            }
            prioritySpecified = (priorityAtt != null);
            if (prioritySpecified) {
                if ((matchAtt == null)) {
                    compileError("The priority attribute must be absent if the match attribute is absent", "XTSE0500");
                }
                try {
                    if (!(Saxon.Hej.value.BigDecimalValue.castableAsDecimal(priorityAtt))) {
                        compileError((("Invalid numeric value for priority ("+priority)+')'), "XTSE0530");
                    }
                    priority = System.Double.Parse(priorityAtt);
                } catch (Saxon.Ejava.lang.NumberFormatException err) {
                    compileError((("Invalid numeric value for priority ("+priority)+')'), "XTSE0530");
                }
            }
            if ((matchAtt != null)) {
                match = makePattern(matchAtt, "match");
                if ((diagnosticId == null)) {
                    diagnosticId = (("match=\""+matchAtt)+'\"');
                    if ((modeAtt != null)) {
                        diagnosticId += ((" mode=\""+modeAtt)+'\"');
                    }
                }
            }
            if (((match == null) && (nameAtt == null))) {
                compileError("xsl:template must have a name or match attribute (or both)", "XTSE0500");
            }
            if ((asAtt != null)) {
                try {
                    requiredType = makeSequenceType(asAtt);
                    declaresRequiredType = true;
                } catch (Saxon.Hej.trans.XPathException e) {
                    compileErrorInAttribute(e.Message, e.getErrorCodeLocalPart(), "as");
                }
            }
            if ((extraAsAtt != null)) {
                Saxon.Hej.value.SequenceType extraResultType;
                declaresRequiredType = true;
                try {
                    extraResultType = makeExtendedSequenceType(extraAsAtt);
                } catch (Saxon.Hej.trans.XPathException e) {
                    compileErrorInAttribute(e.Message, e.getErrorCodeLocalPart(), "saxon:as");
                    extraResultType = requiredType;
                }
                if ((asAtt != null)) {
                    Saxon.Hej.type.Affinity rel = getConfiguration().getTypeHierarchy().sequenceTypeRelationship(extraResultType, requiredType);
                    if (((rel == Saxon.Hej.type.Affinity.SAME_TYPE) || (rel == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                        requiredType = extraResultType;
                    } else {
                        compileErrorInAttribute("When both are present, @saxon:as must be a subtype of @as", "SXER7TBA", "saxon:as");
                    }
                } else {
                    requiredType = extraResultType;
                }
            }
            if ((visibilityAtt != null)) {
                visibility = interpretVisibilityValue(visibilityAtt, "");
                if ((nameAtt == null)) {
                    compileError("xsl:template/@visibility can be specified only if the template has a @name attribute", "XTSE0020");
                } else {
                    compiledNamedTemplate.setDeclaredVisibility(getVisibility());
                }
            }
        }
        public override void processAllAttributes() {
            if (!(isDeferredCompilation(getCompilation()))) {
                base.processAllAttributes();
            } else {
                processDefaultCollationAttribute();
                processDefaultMode();
                staticContext = new Saxon.Hej.style.ExpressionContext(this, null);
                processAttributes();
            }
        }
        public Saxon.Hej.om.StructuredQName[] getModeNames() {
            if ((modeNames == null)) {
                if ((modeAtt == null)) {
                    modeAtt = getAttributeValue("mode");
                    if ((modeAtt == null)) {
                        modeAtt = "#default";
                    }
                }
                bool allModes = false;
                string[] tokens = System.Text.RegularExpressions.Regex.Split(Saxon.Hej.value.Whitespace.trim(modeAtt), "[ \t\n\r]+");
                int count = tokens.Length;
                modeNames = new Saxon.Hej.om.StructuredQName[count];
                count = 0;
                foreach (string s in tokens) {
                    Saxon.Hej.om.StructuredQName mname;
                    if ("#default".Equals(s)) {
                        mname = getDefaultMode();
                        if ((mname == null)) {
                            mname = Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME;
                        }
                    } else {
                        if ("#unnamed".Equals(s)) {
                            mname = Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME;
                        } else {
                            if ("#all".Equals(s)) {
                                allModes = true;
                                mname = Saxon.Hej.trans.Mode.OMNI_MODE;
                            } else {
                                mname = makeQName(s, "XTSE0550", "mode");
                            }
                        }
                    }
                    for (int e = 0; (e<count); e++) {
                        if (modeNames[e].Equals(mname)) {
                            compileError((("In the list of modes, the value "+s)+" is duplicated"), "XTSE0550");
                        }
                    }
                    modeNames[count++] = mname;
                }
                if ((allModes && ((count>1)))) {
                    compileError("mode='#all' cannot be combined with other modes", "XTSE0550");
                }
            }
            return modeNames;
        }
        public System.Collections.Generic.ISet<Saxon.Hej.trans.Mode> getApplicableModes() {
            Saxon.Hej.om.StructuredQName[] names = getModeNames();
            System.Collections.Generic.ISet<Saxon.Hej.trans.Mode> modes = new System.Collections.Generic.HashSet<Saxon.Hej.trans.Mode>(names.Length);
            Saxon.Hej.trans.rules.RuleManager mgr = getPrincipalStylesheetModule().getRuleManager();
            foreach (Saxon.Hej.om.StructuredQName name in names) {
                if (name.Equals(Saxon.Hej.trans.Mode.OMNI_MODE)) {
                    modes.Add(mgr.getUnnamedMode());
                    Saxon.Impl.Helpers.SetUtils.AddAll(modes, mgr.getAllNamedModes());
                } else {
                    Saxon.Hej.trans.Mode mode = mgr.obtainMode(name, false);
                    if ((mode != null)) {
                        modes.Add(mode);
                    }
                }
            }
            return modes;
        }
        public bool isOmniMode() {
            foreach (Saxon.Hej.om.StructuredQName name in getModeNames()) {
                if (name.Equals(Saxon.Hej.trans.Mode.OMNI_MODE)) {
                    return true;
                }
            }
            return false;
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            stackFrameMap = getConfiguration().makeSlotManager();
            checkTopLevel("XTSE0010", true);
            if ((match != null)) {
                match = typeCheck("match", match);
                if ((match.getItemType() is Saxon.Hej.type.ErrorType)) {
                    issueWarning(new Saxon.Hej.trans.XPathException("Pattern will never match anything", Saxon.Hej.trans.SaxonErrorCode.SXWN9015, this));
                }
                if (getPrincipalStylesheetModule().isDeclaredModes()) {
                    Saxon.Hej.trans.rules.RuleManager manager = getPrincipalStylesheetModule().getRuleManager();
                    if ((modeNames != null)) {
                        foreach (Saxon.Hej.om.StructuredQName name in modeNames) {
                            if ((name.Equals(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME) && !(manager.isUnnamedModeExplicit()))) {
                                compileError("The unnamed mode has not been declared in an xsl:mode declaration", "XTSE3085");
                            }
                            if ((manager.obtainMode(name, false) == null)) {
                                compileError((("Mode name "+name.getDisplayName())+" has not been declared in an xsl:mode declaration"), "XTSE3085");
                            }
                        }
                    } else {
                        if (!(manager.isUnnamedModeExplicit())) {
                            compileError("The unnamed mode has not been declared in an xsl:mode declaration", "XTSE3085");
                        }
                    }
                }
                if ((visibility == Saxon.Hej.trans.Visibility.ABSTRACT)) {
                    compileError("An abstract template must have no match attribute");
                }
            }
            bool hasContent = false;
            foreach (Saxon.Hej.om.NodeInfo param in children(x => (x is Saxon.Hej.style.StyleElement))) {
                if ((param.getFingerprint() == Saxon.Hej.om.StandardNames.XSL_CONTEXT_ITEM)) {
                } else {
                    if ((param is Saxon.Hej.style.XSLLocalParam)) {
                        if (((Saxon.Hej.style.XSLLocalParam)(param)).isRequiredParam()) {
                            hasRequiredParams = true;
                        }
                    } else {
                        hasContent = true;
                    }
                }
            }
            if (((visibility == Saxon.Hej.trans.Visibility.ABSTRACT) && hasContent)) {
                compileError("A template with visibility='abstract' must have no body");
            }
        }
        public override void validateSubtree(Saxon.Hej.style.ComponentDeclaration decl, bool excludeStylesheet) {
            if (!(isDeferredCompilation(getCompilation()))) {
                base.validateSubtree(decl, excludeStylesheet);
            } else {
                try {
                    validate(decl);
                } catch (Saxon.Hej.trans.XPathException err) {
                    compileError(err);
                }
            }
        }
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            if ((getTemplateName() != null)) {
                if ((compiledNamedTemplate == null)) {
                    compiledNamedTemplate = new Saxon.Hej.expr.instruct.NamedTemplate(getTemplateName());
                }
                top.indexNamedTemplate(decl);
            }
        }
        protected internal override bool markTailCalls() {
            Saxon.Hej.style.StyleElement last = getLastChildInstruction();
            return ((last != null) && last.markTailCalls());
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            if (isDeferredCompilation(compilation)) {
                createSkeletonTemplate(compilation, decl);
                return ;
            }
            if (compilation.getCompilerInfo().getOptimizerOptions().isSet(Saxon.Hej.expr.parser.OptimizerOptions.TAIL_CALLS)) {
                isTailRecursive = markTailCalls();
            }
            Saxon.Hej.expr.Expression body = compileSequenceConstructor(compilation, decl, true);
            body.restoreParentPointers();
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = makeRetainedStaticContext();
            if ((body.getRetainedStaticContext() == null)) {
                body.setRetainedStaticContext(rsc);
            }
            if ((((match != null) && compilation.getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STRICT_STREAMABILITY)) && isWithinDeclaredStreamableConstruct())) {
                checkStrictStreamability(body);
            }
            if ((getTemplateName() != null)) {
                compileNamedTemplate(compilation, body, decl);
            }
            if ((match != null)) {
                compileTemplateRule(compilation, body, decl);
            }
        }
        private void checkStrictStreamability(Saxon.Hej.expr.Expression body) {
            getConfiguration().checkStrictStreamability(this, body);
        }
        private void compileNamedTemplate(Saxon.Hej.style.Compilation compilation, Saxon.Hej.expr.Expression body, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.parser.RetainedStaticContext rsc = body.getRetainedStaticContext();
            compiledNamedTemplate.setPackageData(rsc.getPackageData());
            compiledNamedTemplate.setBody(body);
            compiledNamedTemplate.setStackFrameMap(stackFrameMap);
            compiledNamedTemplate.setSystemId(getSystemId());
            compiledNamedTemplate.setLineNumber(getLineNumber());
            compiledNamedTemplate.setHasRequiredParams(hasRequiredParams);
            compiledNamedTemplate.setRequiredType(requiredType);
            compiledNamedTemplate.setContextItemRequirements(requiredContextItemType, mayOmitContextItem, absentFocus);
            compiledNamedTemplate.setRetainedStaticContext(rsc);
            compiledNamedTemplate.setDeclaredVisibility(getDeclaredVisibility());
            Saxon.Hej.expr.Component overridden = getOverriddenComponent();
            if ((overridden != null)) {
                checkCompatibility(overridden);
            }
            Saxon.Hej.expr.parser.ContextItemStaticInfo cisi = getConfiguration().makeContextItemStaticInfo(requiredContextItemType, mayOmitContextItem);
            Saxon.Hej.expr.Expression body2 = refineTemplateBody(body, cisi);
            compiledNamedTemplate.setBody(body2);
            if ((getCompilation().getCompilerInfo().getCodeInjector() != null)) {
                getCompilation().getCompilerInfo().getCodeInjector().process(compiledNamedTemplate);
            }
        }
        private Saxon.Hej.expr.Expression refineTemplateBody(Saxon.Hej.expr.Expression body, Saxon.Hej.expr.parser.ContextItemStaticInfo cisi) {
            Saxon.Hej.expr.Expression old = body;
            try {
                body = body.simplify();
            } catch (Saxon.Hej.trans.XPathException e) {
                if (e.isReportableStatically()) {
                    compileError(e);
                } else {
                    body = new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(e));
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(old, body);
                }
            }
            Saxon.Hej.Configuration config = getConfiguration();
            if ((visibility != Saxon.Hej.trans.Visibility.ABSTRACT)) {
                try {
                    if (((requiredType != null) && (requiredType != Saxon.Hej.value.SequenceType.ANY_SEQUENCE))) {
                        Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.TEMPLATE_RESULT, diagnosticId, 0);
                        role.setErrorCode("XTTE0505");
                        body = config.getTypeChecker(false).staticTypeCheck(body, requiredType, role, makeExpressionVisitor());
                    }
                } catch (Saxon.Hej.trans.XPathException err) {
                    if (err.isReportableStatically()) {
                        compileError(err);
                    }
                    body = new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(err));
                    Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(old, body);
                }
            }
            try {
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = makeExpressionVisitor();
                body = body.typeCheck(visitor, cisi);
            } catch (Saxon.Hej.trans.XPathException e) {
                compileError(e);
            }
            return body;
        }
        public void compileTemplateRule(Saxon.Hej.style.Compilation compilation, Saxon.Hej.expr.Expression body, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.Configuration config = getConfiguration();
            if ((getTemplateName() != null)) {
                body = body.copy(new Saxon.Hej.expr.parser.RebindingMap());
            }
            Saxon.Hej.type.ItemType contextItemType;
            Saxon.Hej.expr.parser.ContextItemStaticInfo cisi;
            contextItemType = match.getItemType();
            if (contextItemType.Equals(Saxon.Hej.type.ErrorType.getInstance())) {
                contextItemType = Saxon.Hej.type.AnyItemType.getInstance();
            }
            cisi = config.makeContextItemStaticInfo(contextItemType, false);
            body = refineTemplateBody(body, cisi);
            bool first = true;
            foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.TemplateRule> kvp in compiledTemplateRules) {
                if (!(kvp.Key.Equals(Saxon.Hej.trans.Mode.OMNI_MODE))) {
                    Saxon.Hej.expr.instruct.TemplateRule rule = kvp.Value;
                    if (first) {
                        rule.setMatchPattern(match);
                        rule.setBody(body);
                        if ((compilation.getCompilerInfo().getCodeInjector() != null)) {
                            compilation.getCompilerInfo().getCodeInjector().process(rule);
                            body = rule.getBody();
                        }
                        first = false;
                    } else {
                        if ((rule.getBody() == null)) {
                            body = body.copy(new Saxon.Hej.expr.parser.RebindingMap());
                        } else {
                            body = rule.getBody();
                        }
                    }
                    setCompiledTemplateRuleProperties(rule, body);
                    rule.updateSlaveCopies();
                }
            }
        }
        private void createSkeletonTemplate(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.om.StructuredQName[] modes = modeNames;
            if (isOmniMode()) {
                System.Collections.Generic.IList<Saxon.Hej.om.StructuredQName> all = new System.Collections.Generic.List<Saxon.Hej.om.StructuredQName>();
                all.Add(Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME);
                Saxon.Hej.trans.rules.RuleManager mgr = getCompilation().getPrincipalStylesheetModule().getRuleManager();
                foreach (Saxon.Hej.trans.Mode m in mgr.getAllNamedModes()) {
                    all.Add(m.getModeName());
                }
                modes = all.ToArray();
            }
            foreach (Saxon.Hej.om.StructuredQName modeName in modes) {
                Saxon.Hej.expr.instruct.TemplateRule templateRule = Saxon.Impl.Helpers.MapUtils.Get(compiledTemplateRules, modeName);
                if ((templateRule == null)) {
                    templateRule = getConfiguration().makeTemplateRule();
                }
                templateRule.prepareInitializer(compilation, decl, modeName);
                Saxon.Impl.Helpers.MapUtils.Put(compiledTemplateRules, modeName, templateRule);
                Saxon.Hej.expr.parser.RetainedStaticContext rsc = makeRetainedStaticContext();
                templateRule.setPackageData(rsc.getPackageData());
                setCompiledTemplateRuleProperties(templateRule, null);
            }
        }
        private void setCompiledTemplateRuleProperties(Saxon.Hej.expr.instruct.TemplateRule templateRule, Saxon.Hej.expr.Expression body) {
            templateRule.setMatchPattern(match);
            templateRule.setBody(body);
            templateRule.setStackFrameMap(stackFrameMap);
            templateRule.setSystemId(getSystemId());
            templateRule.setLineNumber(getLineNumber());
            templateRule.setColumnNumber(getColumnNumber());
            templateRule.setHasRequiredParams(hasRequiredParams);
            templateRule.setRequiredType(requiredType);
            templateRule.setContextItemRequirements(requiredContextItemType, absentFocus);
        }
        public void jitCompile(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) { lock(this) {
            if (!(jitCompilationDone)) {
                jitCompilationDone = true;
                compilation.setPreScan(false);
                processAllAttributes();
                checkForJitCompilationErrors(compilation);
                validateSubtree(decl, false);
                checkForJitCompilationErrors(compilation);
                compileDeclaration(compilation, decl);
                checkForJitCompilationErrors(compilation);
            }
        }}
        private void checkForJitCompilationErrors(Saxon.Hej.style.Compilation compilation) {
            if ((compilation.getErrorCount()>0)) {
                Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException((("Errors were reported during JIT compilation of template rule with match=\""+matchAtt)+"\""), Saxon.Hej.trans.SaxonErrorCode.SXST0001, this);
                e.setHasBeenReported(true);
                throw e;
            }
        }
        public void register(Saxon.Hej.style.ComponentDeclaration declaration) {
            if ((match != null)) {
                Saxon.Hej.style.StylesheetModule module = declaration.getModule();
                Saxon.Hej.trans.rules.RuleManager mgr = getCompilation().getPrincipalStylesheetModule().getRuleManager();
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = Saxon.Hej.expr.parser.ExpressionVisitor.make(getStaticContext());
                foreach (Saxon.Hej.om.StructuredQName modeName in getModeNames()) {
                    Saxon.Hej.trans.Mode mode = mgr.obtainMode(modeName, false);
                    if ((mode == null)) {
                        if (mgr.existsOmniMode()) {
                            Saxon.Hej.trans.Mode omniMode = mgr.obtainMode(Saxon.Hej.trans.Mode.OMNI_MODE, true);
                            mode = mgr.obtainMode(modeName, true);
                            Saxon.Hej.trans.SimpleMode.copyRules(omniMode.getActivePart(), mode.getActivePart());
                        } else {
                            mode = mgr.obtainMode(modeName, true);
                        }
                    } else {
                        bool ok = getPrincipalStylesheetModule().checkAcceptableModeForPackage(this, mode);
                        if (!(ok)) {
                            return ;
                        }
                    }
                    Saxon.Hej.pattern.Pattern match1 = match.copy(new Saxon.Hej.expr.parser.RebindingMap());
                    string typed = mode.getActivePart().getPropertyValue("typed");
                    if (("strict".Equals(typed) || "lax".Equals(typed))) {
                        Saxon.Hej.pattern.Pattern match2;
                        try {
                            match2 = match1.convertToTypedPattern(typed);
                        } catch (Saxon.Hej.trans.XPathException e) {
                            e.maybeSetLocation(this);
                            throw;
                        }
                        if ((match2 != match1)) {
                            Saxon.Hej.expr.parser.ContextItemStaticInfo info = getConfiguration().makeContextItemStaticInfo(Saxon.Hej.type.AnyItemType.getInstance(), false);
                            Saxon.Hej.expr.parser.ExpressionTool.copyLocationInfo(match, match2);
                            match2.setOriginalText(match.ToString());
                            match2 = match2.typeCheck(visitor, info);
                            match1 = match2;
                        }
                        if ((modeNames.Length == 1)) {
                            match = match2;
                        }
                    }
                    Saxon.Hej.expr.instruct.TemplateRule rule = Saxon.Impl.Helpers.MapUtils.Get(compiledTemplateRules, modeName);
                    if ((rule == null)) {
                        rule = getConfiguration().makeTemplateRule();
                        Saxon.Impl.Helpers.MapUtils.Put(compiledTemplateRules, modeName, rule);
                    }
                    double prio = (prioritySpecified ? priority : System.Double.NaN);
                    mgr.registerRule(match1, rule, mode, module, prio, mgr.allocateSequenceNumber(), 0);
                    if (mode.isDeclaredStreamable()) {
                        rule.setDeclaredStreamable(true);
                        if (!(match1.isMotionless())) {
                            bool fallback = getConfiguration().getBooleanProperty(Saxon.Hej.lib.Feature<bool>.STREAMING_FALLBACK);
                            string message = "Template rule is declared streamable but the match pattern is not motionless";
                            if (fallback) {
                                message += "\n  * Falling back to non-streaming implementation";
                                getStaticContext().issueWarning(message, this);
                                rule.setDeclaredStreamable(false);
                                getCompilation().setFallbackToNonStreaming(true);
                            } else {
                                throw new Saxon.Hej.trans.XPathException(message, "XTSE3430", this);
                            }
                        }
                    }
                    if (((mode.getDefaultResultType() != null) && !(declaresRequiredType))) {
                        rule.setRequiredType(mode.getDefaultResultType());
                    }
                    if (mode.getModeName().Equals(Saxon.Hej.trans.Mode.OMNI_MODE)) {
                        Saxon.Impl.Helpers.MapUtils.Put(compiledTemplateRules, Saxon.Hej.trans.Mode.UNNAMED_MODE_NAME, rule);
                        mgr.registerRule(match1, rule, mgr.getUnnamedMode(), module, prio, mgr.allocateSequenceNumber(), 0);
                        foreach (Saxon.Hej.trans.Mode m in mgr.getAllNamedModes()) {
                            if ((m is Saxon.Hej.trans.SimpleMode)) {
                                Saxon.Hej.expr.instruct.TemplateRule ruleCopy = rule.copy();
                                if (m.isDeclaredStreamable()) {
                                    ruleCopy.setDeclaredStreamable(true);
                                }
                                Saxon.Impl.Helpers.MapUtils.Put(compiledTemplateRules, m.getModeName(), ruleCopy);
                                mgr.registerRule(match1.copy(new Saxon.Hej.expr.parser.RebindingMap()), ruleCopy, m, module, prio, mgr.allocateSequenceNumber(), 0);
                            }
                        }
                    }
                }
            }
        }
        public void allocatePatternSlotNumbers() {
            if ((match != null)) {
                foreach (Saxon.Hej.expr.instruct.TemplateRule templateRule in compiledTemplateRules.Values) {
                    foreach (Saxon.Hej.trans.rules.Rule r in templateRule.getRules()) {
                        Saxon.Hej.pattern.Pattern match = r.getPattern();
                        int nextFree = 0;
                        if ((((match.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_CURRENT_ITEM)) != 0)) {
                            nextFree = 1;
                        }
                        int slots = match.allocateSlots(getSlotManager(), nextFree);
                        if (((slots == 0) && ((((match.getDependencies()&Saxon.Hej.expr.StaticProperty.DEPENDS_ON_USER_FUNCTIONS)) != 0)))) {
                            slots = 1;
                        }
                        if ((slots>0)) {
                            Saxon.Hej.trans.rules.RuleManager mgr = getCompilation().getPrincipalStylesheetModule().getRuleManager();
                            bool appliesToAll = false;
                            foreach (Saxon.Hej.om.StructuredQName nc in modeNames) {
                                if (nc.Equals(Saxon.Hej.trans.Mode.OMNI_MODE)) {
                                    appliesToAll = true;
                                    break ;
                                }
                                Saxon.Hej.trans.Mode mode = mgr.obtainMode(nc, true);
                                mode.getActivePart().allocatePatternSlots(slots);
                            }
                            if (appliesToAll) {
                                foreach (Saxon.Hej.trans.Mode m in mgr.getAllNamedModes()) {
                                    m.getActivePart().allocatePatternSlots(slots);
                                }
                                mgr.getUnnamedMode().getActivePart().allocatePatternSlots(slots);
                            }
                        }
                    }
                }
            }
        }
        public void optimize(Saxon.Hej.style.ComponentDeclaration declaration) {
            Saxon.Hej.Configuration config = getConfiguration();
            if ((compiledNamedTemplate != null)) {
                Saxon.Hej.expr.Expression body = compiledNamedTemplate.getBody();
                Saxon.Hej.expr.parser.ContextItemStaticInfo cisi = getConfiguration().makeContextItemStaticInfo(requiredContextItemType, mayOmitContextItem);
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = makeExpressionVisitor();
                body = body.typeCheck(visitor, cisi);
                body = Saxon.Hej.expr.parser.ExpressionTool.optimizeComponentBody(body, getCompilation(), visitor, cisi, true);
                compiledNamedTemplate.setBody(body);
                allocateLocalSlots(body);
                if (explaining) {
                    Saxon.Hej.lib.Logger err = getConfiguration().getLogger();
                    err.info((((("Optimized expression tree for named template at line "+getLineNumber())+" in ")+getSystemId())+':'));
                    body.explain(err);
                }
                body.restoreParentPointers();
                if ((config.isDeferredByteCode(Saxon.Api.HostLanguage.XSLT) && !(isTailRecursive))) {
                    Saxon.Hej.expr.parser.Optimizer opt = config.obtainOptimizer();
                    int evaluationModes = (Saxon.Hej.expr.Expression.ITERATE_METHOD|Saxon.Hej.expr.Expression.PROCESS_METHOD);
                    compiledNamedTemplate.setBody(opt.makeByteCodeCandidate(compiledNamedTemplate, body, diagnosticId, evaluationModes));
                }
            }
            if ((match != null)) {
                Saxon.Hej.type.ItemType contextItemType = getContextItemTypeForTemplateRule();
                Saxon.Hej.expr.parser.ContextItemStaticInfo cisi = config.makeContextItemStaticInfo(contextItemType, false);
                cisi.setContextPostureStriding();
                Saxon.Hej.expr.parser.ExpressionVisitor visitor = makeExpressionVisitor();
                match.resetLocalStaticProperties();
                match = match.optimize(visitor, cisi);
                if (!(isDeferredCompilation(getCompilation()))) {
                    Saxon.Hej.expr.parser.Optimizer opt = getConfiguration().obtainOptimizer();
                    try {
                        foreach (System.Collections.Generic.KeyValuePair<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.TemplateRule> entry in compiledTemplateRules) {
                            if (!(entry.Key.Equals(Saxon.Hej.trans.Mode.OMNI_MODE))) {
                                Saxon.Hej.expr.instruct.TemplateRule compiledTemplateRule = entry.Value;
                                Saxon.Hej.expr.Expression templateRuleBody = compiledTemplateRule.getBody();
                                visitor.setOptimizeForStreaming(compiledTemplateRule.isDeclaredStreamable());
                                templateRuleBody = templateRuleBody.typeCheck(visitor, cisi);
                                templateRuleBody = Saxon.Hej.expr.parser.ExpressionTool.optimizeComponentBody(templateRuleBody, getCompilation(), visitor, cisi, true);
                                compiledTemplateRule.setBody(templateRuleBody);
                                opt.checkStreamability(this, compiledTemplateRule);
                                allocateLocalSlots(templateRuleBody);
                                foreach (Saxon.Hej.trans.rules.Rule r in compiledTemplateRule.getRules()) {
                                    Saxon.Hej.pattern.Pattern match = r.getPattern();
                                    Saxon.Hej.expr.parser.ContextItemStaticInfo info = getConfiguration().makeContextItemStaticInfo(match.getItemType(), false);
                                    info.setContextPostureStriding();
                                    Saxon.Hej.pattern.Pattern m2 = match.optimize(visitor, info);
                                    if ((compiledTemplateRules.Count>1)) {
                                        m2 = m2.copy(new Saxon.Hej.expr.parser.RebindingMap());
                                    }
                                    if ((m2 != match)) {
                                        r.setPattern(m2);
                                    }
                                }
                                if ((visitor.getConfiguration().isDeferredByteCode(Saxon.Api.HostLanguage.XSLT) && !(isTailRecursive))) {
                                    int evaluationModes = (Saxon.Hej.expr.Expression.ITERATE_METHOD|Saxon.Hej.expr.Expression.PROCESS_METHOD);
                                    compiledTemplateRule.setBody(opt.makeByteCodeCandidate(compiledTemplateRule, templateRuleBody, diagnosticId, evaluationModes));
                                }
                                if (explaining) {
                                    Saxon.Hej.lib.Logger err = getConfiguration().getLogger();
                                    err.info((((("Optimized expression tree for template rule at line "+getLineNumber())+" in ")+getSystemId())+':'));
                                    templateRuleBody.explain(err);
                                }
                            }
                        }
                    } catch (Saxon.Hej.trans.XPathException e) {
                        e.maybeSetLocation(this);
                        compileError(e);
                    }
                }
            }
        }
        public Saxon.Hej.type.ItemType getContextItemTypeForTemplateRule() {
            Saxon.Hej.Configuration config = getConfiguration();
            Saxon.Hej.type.ItemType contextItemType = match.getItemType();
            if (contextItemType.Equals(Saxon.Hej.type.ErrorType.getInstance())) {
                contextItemType = Saxon.Hej.type.AnyItemType.getInstance();
            }
            if ((requiredContextItemType != Saxon.Hej.type.AnyItemType.getInstance())) {
                Saxon.Hej.type.Affinity rel = config.getTypeHierarchy().relationship(contextItemType, requiredContextItemType);
                switch (rel) {
                  case Saxon.Hej.type.Affinity.DISJOINT:
                    Saxon.Hej.trans.XPathException e = new Saxon.Hej.trans.XPathException("The declared context item type is inconsistent with the match pattern", "XPTY0004", this);
                    e.setIsTypeError(true);
                    throw e;
                  case Saxon.Hej.type.Affinity.SUBSUMED_BY:
                  case Saxon.Hej.type.Affinity.OVERLAPS:
                  case Saxon.Hej.type.Affinity.SAME_TYPE:
                    break ;
                  case Saxon.Hej.type.Affinity.SUBSUMES:
                    contextItemType = requiredContextItemType;
                    break ;
                }
            }
            return contextItemType;
        }
        public void generateByteCode(Saxon.Hej.expr.parser.Optimizer opt) {
        }
        public Saxon.Hej.expr.instruct.SlotManager getSlotManager() {
            return stackFrameMap;
        }
        public Saxon.Hej.expr.instruct.NamedTemplate getCompiledNamedTemplate() {
            return compiledNamedTemplate;
        }
        public Saxon.Hej.pattern.Pattern getMatch() {
            return match;
        }
        public System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.expr.instruct.TemplateRule> getTemplateRulesByMode() {
            return compiledTemplateRules;
        }
    
    }
}