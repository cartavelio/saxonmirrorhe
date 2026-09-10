using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class XSLFunction : Saxon.Hej.style.StyleElement, Saxon.Hej.style.StylesheetComponent {
        private bool doneAttributes = false;
        private string nameAtt = null;
        private string asAtt = null;
        private string extraAsAtt = null;
        private Saxon.Hej.value.SequenceType resultType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
        private Saxon.Hej.expr.instruct.SlotManager stackFrameMap;
        private bool memoFunction = false;
        private string overrideExtensionFunctionAtt = null;
        private bool overrideExtensionFunction = true;
        private int numberOfArguments = -1;
        private Saxon.Hej.expr.instruct.UserFunction compiledFunction;
        private Saxon.Hej.trans.Visibility visibility = Saxon.Hej.trans.Visibility.UNDEFINED;
        private Saxon.Hej.trans.FunctionStreamability streamability;
        private Saxon.Hej.expr.instruct.UserFunction.Determinism determinism = Saxon.Hej.expr.instruct.UserFunction.Determinism.PROACTIVE;
        private bool explaining;
        public Saxon.Hej.expr.instruct.Actor getActor() {
            return compiledFunction;
        }
        public override bool isDeclaration() {
            return true;
        }
        protected internal override void prepareAttributes() {
            if (doneAttributes) {
                return ;
            }
            doneAttributes = true;
            Saxon.Hej.om.AttributeMap atts = attributes();
            overrideExtensionFunctionAtt = null;
            string visibilityAtt = null;
            string cacheAtt = null;
            string newEachTimeAtt = null;
            string streamabilityAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in atts) {
                Saxon.Hej.om.NodeName name = att.getNodeName();
                string uri = name.getURI();
                string local = name.getLocalPart();
                if ("".Equals(uri)) {
                    switch (local) {
                      case "name":
                        nameAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                        System.Diagnostics.Debug.Assert((nameAtt != null));
                        Saxon.Hej.om.StructuredQName functionName = makeQName(nameAtt, null, "name");
                        if (functionName.hasURI("")) {
                            functionName = new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, functionName.getLocalPart());
                            compileError("Function name must be in a namespace", "XTSE0740");
                        }
                        setObjectName(functionName);
                        break ;
                      case "as":
                        asAtt = att.getValue();
                        break ;
                      case "visibility":
                        visibilityAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                        break ;
                      case "streamability":
                        streamabilityAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                        break ;
                      case "override":
                        string overrideAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                        bool @override = processBooleanAttribute("override", overrideAtt);
                        if ((overrideExtensionFunctionAtt != null)) {
                            if ((@override != overrideExtensionFunction)) {
                                compileError("Attributes override-extension-function and override are both used, but do not match", "XTSE0020");
                            }
                        } else {
                            overrideExtensionFunctionAtt = overrideAtt;
                            overrideExtensionFunction = @override;
                        }
                        compileWarning("The xsl:function/@override attribute is deprecated; use override-extension-function", Saxon.Hej.trans.SaxonErrorCode.SXWN9014);
                        break ;
                      case "override-extension-function":
                        string overrideExtAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                        bool overrideExt = processBooleanAttribute("override-extension-function", overrideExtAtt);
                        if ((overrideExtensionFunctionAtt != null)) {
                            if ((overrideExt != overrideExtensionFunction)) {
                                compileError("Attributes override-extension-function and override are both used, but do not match", "XTSE0020");
                            }
                        } else {
                            overrideExtensionFunctionAtt = overrideExtAtt;
                            overrideExtensionFunction = overrideExt;
                        }
                        if (local.Equals("override")) {
                            compileWarning("The xsl:function/@override attribute is deprecated; use override-extension-function", Saxon.Hej.trans.SaxonErrorCode.SXWN9014);
                        }
                        break ;
                      case "cache":
                        cacheAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                        break ;
                      case "new-each-time":
                        newEachTimeAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                        break ;
                      default:
                        checkUnknownAttribute(name);
                        break ;
                    }
                } else {
                    if (uri.Equals(Saxon.Hej.lib.NamespaceConstant.SAXON)) {
                        if (isExtensionAttributeAllowed(att.getNodeName().getDisplayName())) {
                            if (local.Equals("memo-function")) {
                                compileWarning("saxon:memo-function is deprecated: use cache='yes'", Saxon.Hej.trans.SaxonErrorCode.SXWN9014);
                                if (getConfiguration().isLicensedFeature(Saxon.Hej.Configuration.LicenseFeature.PROFESSIONAL_EDITION)) {
                                    memoFunction = processBooleanAttribute("saxon:memo-function", att.getValue());
                                }
                            } else {
                                if (local.Equals("as")) {
                                    isExtensionAttributeAllowed(name.getDisplayName());
                                    extraAsAtt = att.getValue();
                                } else {
                                    if ((local.Equals("explain") && isYes(Saxon.Hej.value.Whitespace.trim(att.getValue())))) {
                                        explaining = true;
                                    }
                                }
                            }
                        }
                    } else {
                        checkUnknownAttribute(name);
                    }
                }
            }
            if ((nameAtt == null)) {
                reportAbsence("name");
                nameAtt = ("xsl:unnamed-function-"+generateId());
            }
            if ((asAtt != null)) {
                try {
                    resultType = makeSequenceType(asAtt);
                } catch (Saxon.Hej.trans.XPathException e) {
                    compileErrorInAttribute(e.Message, e.getErrorCodeLocalPart(), "as");
                }
            }
            if ((extraAsAtt != null)) {
                Saxon.Hej.value.SequenceType extraResultType = null;
                try {
                    extraResultType = makeExtendedSequenceType(extraAsAtt);
                } catch (Saxon.Hej.trans.XPathException e) {
                    compileErrorInAttribute(e.Message, e.getErrorCodeLocalPart(), "saxon:as");
                    extraResultType = resultType;
                }
                if ((asAtt != null)) {
                    Saxon.Hej.type.Affinity rel = getConfiguration().getTypeHierarchy().sequenceTypeRelationship(extraResultType, resultType);
                    if (((rel == Saxon.Hej.type.Affinity.SAME_TYPE) || (rel == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                        resultType = extraResultType;
                    } else {
                        compileErrorInAttribute("When both are present, @saxon:as must be a subtype of @as", "SXER7TBA", "saxon:as");
                    }
                } else {
                    resultType = extraResultType;
                }
            }
            if ((visibilityAtt == null)) {
                visibility = Saxon.Hej.trans.Visibility.PRIVATE;
            } else {
                visibility = interpretVisibilityValue(visibilityAtt, "");
            }
            if ((streamabilityAtt == null)) {
                streamability = Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED;
            } else {
                streamability = getStreamabilityValue(streamabilityAtt);
                if (streamability.isStreaming()) {
                    bool streamable = processStreamableAtt("yes");
                    if (!(streamable)) {
                        streamability = Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED;
                    }
                }
            }
            if ((newEachTimeAtt != null)) {
                if ("maybe".Equals(newEachTimeAtt)) {
                    determinism = Saxon.Hej.expr.instruct.UserFunction.Determinism.ELIDABLE;
                } else {
                    bool b = processBooleanAttribute("new-each-time", newEachTimeAtt);
                    determinism = (b ? Saxon.Hej.expr.instruct.UserFunction.Determinism.PROACTIVE : Saxon.Hej.expr.instruct.UserFunction.Determinism.DETERMINISTIC);
                }
            }
            bool cache = false;
            if ((cacheAtt != null)) {
                cache = processBooleanAttribute("cache", cacheAtt);
            }
            if (((determinism == Saxon.Hej.expr.instruct.UserFunction.Determinism.DETERMINISTIC) || cache)) {
                memoFunction = true;
            }
        }
        private Saxon.Hej.trans.FunctionStreamability getStreamabilityValue(string s) {
            if (s.Contains(":")) {
                makeQName(s, null, "streamability");
                return Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED;
            }
            try {
                return Saxon.Hej.trans.FunctionStreamability.of(s);
            } catch (Saxon.Ejava.lang.IllegalArgumentException ill) {
                invalidAttribute("streamability", "unclassified|absorbing|inspection|filter|shallow-descent|deep-descent|ascent");
                return null;
            }
        }
        public override Saxon.Hej.om.StructuredQName getObjectName() {
            Saxon.Hej.om.StructuredQName qn = base.getObjectName();
            if ((qn == null)) {
                nameAtt = Saxon.Hej.value.Whitespace.trim(getAttributeValue("", "name"));
                if ((nameAtt == null)) {
                    return new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, ("badly-named-function"+generateId()));
                }
                qn = makeQName(nameAtt, null, "name");
                setObjectName(qn);
            }
            return qn;
        }
        protected internal override bool mayContainSequenceConstructor() {
            return true;
        }
        protected internal override bool mayContainParam() {
            return true;
        }
        protected internal override bool isPermittedChild(Saxon.Hej.style.StyleElement child) {
            return (child is Saxon.Hej.style.XSLLocalParam);
        }
        public override Saxon.Hej.trans.Visibility getVisibility() {
            if ((visibility == Saxon.Hej.trans.Visibility.UNDEFINED)) {
                string vAtt = getAttributeValue("", "visibility");
                return ((vAtt == null) ? Saxon.Hej.trans.Visibility.PRIVATE : interpretVisibilityValue(Saxon.Hej.value.Whitespace.trim(vAtt), ""));
            }
            return visibility;
        }
        public Saxon.Hej.trans.SymbolicName getSymbolicName() {
            return new Saxon.Hej.trans.SymbolicName.F(getObjectName(), getNumberOfArguments());
        }
        public void checkCompatibility(Saxon.Hej.expr.Component component) {
            if ((compiledFunction == null)) {
                getCompiledFunction();
            }
            Saxon.Hej.type.TypeHierarchy th = getConfiguration().getTypeHierarchy();
            Saxon.Hej.expr.instruct.UserFunction other = (Saxon.Hej.expr.instruct.UserFunction)(component.getActor());
            if (!(compiledFunction.getSymbolicName().Equals(other.getSymbolicName()))) {
                compileError(((("The overriding xsl:function "+nameAtt)+" does not match the overridden function: ")+"the function name/arity does not match"), "XTSE3070");
            }
            if (!(compiledFunction.getDeclaredResultType().isSameType(other.getDeclaredResultType(), th))) {
                compileError(((("The overriding xsl:function "+nameAtt)+" does not match the overridden function: ")+"the return type does not match"), "XTSE3070");
            }
            if (!(compiledFunction.getDeclaredStreamability().Equals(other.getDeclaredStreamability()))) {
                compileError(((("The overriding xsl:function "+nameAtt)+" does not match the overridden function: ")+"the streamability category does not match"), "XTSE3070");
            }
            if (!(compiledFunction.getDeterminism().Equals(other.getDeterminism()))) {
                compileError(((("The overriding xsl:function "+nameAtt)+" does not match the overridden function: ")+"the new-each-time attribute does not match"), "XTSE3070");
            }
            for (int i = 0; (i<getNumberOfArguments()); i++) {
                if (!(compiledFunction.getArgumentType(i).isSameType(other.getArgumentType(i), th))) {
                    compileError(((((("The overriding xsl:function "+nameAtt)+" does not match the overridden function: ")+"the type of the ")+Saxon.Hej.expr.parser.RoleDiagnostic.ordinal((i+1)))+" argument does not match"), "XTSE3070");
                }
            }
        }
        public bool isOverrideExtensionFunction() {
            if ((overrideExtensionFunctionAtt == null)) {
                prepareAttributes();
            }
            return overrideExtensionFunction;
        }
        public override void index(Saxon.Hej.style.ComponentDeclaration decl, Saxon.Hej.style.PrincipalStylesheetModule top) {
            getCompiledFunction();
            top.indexFunction(decl);
        }
        public override void validate(Saxon.Hej.style.ComponentDeclaration decl) {
            stackFrameMap = getConfiguration().makeSlotManager();
            checkTopLevel("XTSE0010", true);
            int arity = getNumberOfArguments();
            if (((arity == 0) && (streamability != Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED))) {
                compileError("A function with no arguments must have streamability=unclassified", "XTSE3155");
            }
        }
        public override void compileDeclaration(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            Saxon.Hej.expr.Expression exp = compileSequenceConstructor(compilation, decl, false);
            if ((exp == null)) {
                exp = Saxon.Hej.expr.Literal.makeEmptySequence();
            } else {
                if (Saxon.Hej.expr.Literal.isEmptySequence(exp)) {
                } else {
                    if ((visibility == Saxon.Hej.trans.Visibility.ABSTRACT)) {
                        compileError("A function defined with visibility='abstract' must have no body");
                    }
                    exp = exp.simplify();
                }
            }
            Saxon.Hej.expr.instruct.UserFunction fn = getCompiledFunction();
            fn.setBody(exp);
            fn.setStackFrameMap(stackFrameMap);
            bindParameterDefinitions(fn);
            fn.setRetainedStaticContext(makeRetainedStaticContext());
            fn.setOverrideExtensionFunction(overrideExtensionFunction);
            if ((compilation.getCompilerInfo().getCodeInjector() != null)) {
                compilation.getCompilerInfo().getCodeInjector().process(fn);
            }
            Saxon.Hej.expr.Component overridden = getOverriddenComponent();
            if ((overridden != null)) {
                checkCompatibility(overridden);
            }
        }
        public void optimize(Saxon.Hej.style.ComponentDeclaration declaration) {
            Saxon.Hej.expr.Expression exp = compiledFunction.getBody();
            Saxon.Hej.expr.parser.ExpressionTool.resetPropertiesWithinSubtree(exp);
            Saxon.Hej.expr.parser.ExpressionVisitor visitor = makeExpressionVisitor();
            Saxon.Hej.expr.Expression exp2 = exp.typeCheck(visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo.ABSENT);
            if (streamability.isStreaming()) {
                visitor.setOptimizeForStreaming(true);
            }
            exp2 = Saxon.Hej.expr.parser.ExpressionTool.optimizeComponentBody(exp2, getCompilation(), visitor, Saxon.Hej.expr.parser.ContextItemStaticInfo.ABSENT, true);
            setInstructionLocation(this, exp2);
            compiledFunction.setBody(exp2);
            Saxon.Hej.expr.parser.Optimizer optimizer = visitor.getConfiguration().obtainOptimizer();
            if (streamability.isStreaming()) {
                optimizer.assessFunctionStreamability(this, compiledFunction);
            }
            allocateLocalSlots(exp2);
            if ((exp2 != exp)) {
                compiledFunction.setBody(exp2);
            }
            Saxon.Hej.expr.parser.OptimizerOptions options = getCompilation().getCompilerInfo().getOptimizerOptions();
            if ((options.isSet(Saxon.Hej.expr.parser.OptimizerOptions.TAIL_CALLS) && !(streamability.isStreaming()))) {
                int tailCalls = Saxon.Hej.expr.parser.ExpressionTool.markTailFunctionCalls(exp2, getObjectName(), getNumberOfArguments());
                if ((tailCalls != 0)) {
                    compiledFunction.setTailRecursive((tailCalls>0), (tailCalls>1));
                    exp2 = compiledFunction.getBody();
                    compiledFunction.setBody(new Saxon.Hej.expr.TailCallLoop(compiledFunction, exp2));
                }
            }
            compiledFunction.computeEvaluationMode();
            if (streamability.isStreaming()) {
                compiledFunction.prepareForStreaming();
            } else {
                if (visitor.getConfiguration().isDeferredByteCode(Saxon.Api.HostLanguage.XSLT)) {
                    int evaluationModes = (Saxon.Hej.expr.Expression.ITERATE_METHOD|Saxon.Hej.expr.Expression.PROCESS_METHOD);
                    compiledFunction.setBody(getConfiguration().obtainOptimizer().makeByteCodeCandidate(compiledFunction, compiledFunction.getBody(), nameAtt, evaluationModes));
                }
            }
            if (explaining) {
                exp2.explain(getConfiguration().getLogger());
            }
        }
        public void generateByteCode(Saxon.Hej.expr.parser.Optimizer opt) {
        }
        public Saxon.Hej.expr.instruct.SlotManager getSlotManager() {
            return stackFrameMap;
        }
        public Saxon.Hej.value.SequenceType getResultType() {
            if ((resultType == null)) {
                string asAtt = getAttributeValue("", "as");
                if ((asAtt != null)) {
                    try {
                        resultType = makeSequenceType(asAtt);
                    } catch (Saxon.Hej.trans.XPathException err) {
                    }
                }
            }
            return ((resultType == null) ? Saxon.Hej.value.SequenceType.ANY_SEQUENCE : resultType);
        }
        public int getNumberOfArguments() {
            if ((numberOfArguments == -1)) {
                numberOfArguments = 0;
                foreach (Saxon.Hej.om.NodeInfo child in children()) {
                    if ((child is Saxon.Hej.style.XSLLocalParam)) {
                        numberOfArguments++;
                    } else {
                        return numberOfArguments;
                    }
                }
            }
            return numberOfArguments;
        }
        public void setParameterDefinitions(Saxon.Hej.expr.instruct.UserFunction fn) {
            Saxon.Hej.expr.instruct.UserFunctionParameter[] @params = new Saxon.Hej.expr.instruct.UserFunctionParameter[getNumberOfArguments()];
            fn.setParameterDefinitions(@params);
            int count = 0;
            foreach (Saxon.Hej.om.NodeInfo node in children()) {
                if ((node is Saxon.Hej.style.XSLLocalParam)) {
                    Saxon.Hej.expr.instruct.UserFunctionParameter param = new Saxon.Hej.expr.instruct.UserFunctionParameter();
                    @params[count] = param;
                    param.setRequiredType(((Saxon.Hej.style.XSLLocalParam)(node)).getRequiredType());
                    param.setVariableQName(((Saxon.Hej.style.XSLLocalParam)(node)).getVariableQName());
                    param.setSlotNumber(((Saxon.Hej.style.XSLLocalParam)(node)).getSlotNumber());
                    if (((count == 0) && (streamability != Saxon.Hej.trans.FunctionStreamability.UNCLASSIFIED))) {
                        param.setFunctionStreamability(streamability);
                    }
                    count++;
                } else {
                    break ;
                }
            }
        }
        private void bindParameterDefinitions(Saxon.Hej.expr.instruct.UserFunction fn) {
            Saxon.Hej.expr.instruct.UserFunctionParameter[] @params = fn.getParameterDefinitions();
            int count = 0;
            foreach (Saxon.Hej.om.NodeInfo node in children(x => (x is Saxon.Hej.style.XSLLocalParam))) {
                Saxon.Hej.expr.instruct.UserFunctionParameter param = @params[count++];
                param.setRequiredType(((Saxon.Hej.style.XSLLocalParam)(node)).getRequiredType());
                param.setVariableQName(((Saxon.Hej.style.XSLLocalParam)(node)).getVariableQName());
                param.setSlotNumber(((Saxon.Hej.style.XSLLocalParam)(node)).getSlotNumber());
                ((Saxon.Hej.style.XSLLocalParam)(node)).getSourceBinding().fixupBinding(param);
            }
        }
        public Saxon.Hej.value.SequenceType[] getArgumentTypes() {
            Saxon.Hej.value.SequenceType[] types = new Saxon.Hej.value.SequenceType[getNumberOfArguments()];
            int count = 0;
            foreach (Saxon.Hej.om.NodeInfo node in children(x => (x is Saxon.Hej.style.XSLLocalParam))) {
                types[count++] = ((Saxon.Hej.style.XSLLocalParam)(node)).getRequiredType();
            }
            return types;
        }
        public Saxon.Hej.expr.instruct.UserFunction getCompiledFunction() {
            if ((compiledFunction == null)) {
                prepareAttributes();
                Saxon.Hej.expr.instruct.UserFunction fn = getConfiguration().newUserFunction(memoFunction, streamability);
                fn.setPackageData(getCompilation().getPackageData());
                fn.setFunctionName(getObjectName());
                setParameterDefinitions(fn);
                fn.setResultType(getResultType());
                fn.setLineNumber(getLineNumber());
                fn.setSystemId(getSystemId());
                fn.obtainDeclaringComponent(this);
                fn.setDeclaredVisibility(getDeclaredVisibility());
                fn.setDeclaredStreamability(streamability);
                fn.setDeterminism(determinism);
                System.Collections.Generic.IList<Saxon.Hej.query.Annotation> annotations = new System.Collections.Generic.List<Saxon.Hej.query.Annotation>();
                if (memoFunction) {
                    annotations.Add(new Saxon.Hej.query.Annotation(new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, "memo-function")));
                }
                fn.setAnnotations(new Saxon.Hej.query.AnnotationList(annotations));
                fn.setOverrideExtensionFunction(overrideExtensionFunction);
                compiledFunction = fn;
            }
            return compiledFunction;
        }
    
    }
}