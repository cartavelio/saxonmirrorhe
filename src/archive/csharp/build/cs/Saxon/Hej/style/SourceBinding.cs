using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Hej.style {
    public class SourceBinding {
        private readonly Saxon.Hej.style.StyleElement sourceElement;
        private Saxon.Hej.om.StructuredQName name;
        private Saxon.Hej.expr.Expression select = null;
        private Saxon.Hej.value.SequenceType declaredType = null;
        private Saxon.Hej.value.SequenceType inferredType = null;
        protected internal Saxon.Hej.expr.instruct.SlotManager slotManager = null;
        private Saxon.Hej.trans.Visibility visibility;
        private Saxon.Hej.om.GroundedValue constantValue = null;
        [System.Flags] public enum BindingProperty {
            NONE = 0,
            PRIVATE = 1,
            GLOBAL = 2,
            PARAM = 4,
            TUNNEL = 8,
            REQUIRED = 16,
            IMPLICITLY_REQUIRED = 32,
            ASSIGNABLE = 64,
            SELECT = 128,
            AS = 256,
            DISALLOWS_CONTENT = 512,
            STATIC = 1024,
            VISIBILITY = 2048,
            IMPLICITLY_DECLARED = 4096,
        }
        private Saxon.Hej.style.SourceBinding.BindingProperty properties = Saxon.Hej.style.SourceBinding.BindingProperty.NONE;
        private readonly System.Collections.Generic.IList<Saxon.Hej.expr.BindingReference> references = new System.Collections.Generic.List<Saxon.Hej.expr.BindingReference>(10);
        public SourceBinding(Saxon.Hej.style.StyleElement sourceElement)  {
            this.sourceElement = sourceElement;
        }
        public void prepareAttributes(Saxon.Hej.style.SourceBinding.BindingProperty permittedAttributes) {
            Saxon.Hej.om.AttributeMap atts = sourceElement.attributes();
            Saxon.Hej.om.AttributeInfo selectAtt = null;
            string asAtt = null;
            string extraAsAtt = null;
            string requiredAtt = null;
            string tunnelAtt = null;
            string assignableAtt = null;
            string staticAtt = null;
            string visibilityAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in atts) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                if (f.Equals("name")) {
                    if (((name == null) || name.Equals(errorName()))) {
                        processVariableName(att.getValue());
                    }
                } else {
                    if (f.Equals("select")) {
                        if (permittedAttributes.HasFlag(/*explicit enum*/ Saxon.Hej.style.SourceBinding.BindingProperty.SELECT)) {
                            selectAtt = att;
                        } else {
                            sourceElement.compileErrorInAttribute("The select attribute is not permitted on a function parameter", "XTSE0760", "select");
                        }
                    } else {
                        if ((f.Equals("as") && permittedAttributes.HasFlag(Saxon.Hej.style.SourceBinding.BindingProperty.AS))) {
                            asAtt = att.getValue();
                        } else {
                            if ((f.Equals("required") && permittedAttributes.HasFlag(Saxon.Hej.style.SourceBinding.BindingProperty.REQUIRED))) {
                                requiredAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                            } else {
                                if (f.Equals("tunnel")) {
                                    tunnelAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                                } else {
                                    if ((f.Equals("static") && permittedAttributes.HasFlag(Saxon.Hej.style.SourceBinding.BindingProperty.STATIC))) {
                                        staticAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                                    } else {
                                        if ((f.Equals("visibility") && permittedAttributes.HasFlag(Saxon.Hej.style.SourceBinding.BindingProperty.VISIBILITY))) {
                                            visibilityAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                                        } else {
                                            if (Saxon.Hej.lib.NamespaceConstant.SAXON.Equals(attName.getURI())) {
                                                if (sourceElement.isExtensionAttributeAllowed(attName.getDisplayName())) {
                                                    if ((attName.getLocalPart().Equals("assignable") && permittedAttributes.HasFlag(Saxon.Hej.style.SourceBinding.BindingProperty.ASSIGNABLE))) {
                                                        assignableAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                                                    } else {
                                                        if (attName.getLocalPart().Equals("as")) {
                                                            extraAsAtt = att.getValue();
                                                        } else {
                                                            sourceElement.checkUnknownAttribute(att.getNodeName());
                                                        }
                                                    }
                                                }
                                            } else {
                                                sourceElement.checkUnknownAttribute(att.getNodeName());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if ((name == null)) {
                sourceElement.reportAbsence("name");
                name = errorName();
            }
            if ((selectAtt != null)) {
                select = sourceElement.makeExpression(selectAtt.getValue(), selectAtt);
            }
            if ((requiredAtt != null)) {
                bool required = sourceElement.processBooleanAttribute("required", requiredAtt);
                setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.REQUIRED, required);
                if ((required && (select != null))) {
                    sourceElement.compileError("xsl:param: cannot supply a default value when required='yes'");
                }
            }
            if ((tunnelAtt != null)) {
                bool tunnel = sourceElement.processBooleanAttribute("tunnel", tunnelAtt);
                if ((tunnel && !(permittedAttributes.HasFlag(Saxon.Hej.style.SourceBinding.BindingProperty.TUNNEL)))) {
                    sourceElement.compileErrorInAttribute("The only permitted value of the 'tunnel' attribute is 'no'", "XTSE0020", "tunnel");
                }
                setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.TUNNEL, tunnel);
            }
            if ((assignableAtt != null)) {
                bool assignable = sourceElement.processBooleanAttribute("saxon:assignable", assignableAtt);
                setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.ASSIGNABLE, assignable);
            }
            if ((staticAtt != null)) {
                bool statick = sourceElement.processBooleanAttribute("static", staticAtt);
                setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.STATIC, statick);
                if (statick) {
                    setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.DISALLOWS_CONTENT, true);
                }
                if ((statick && !(hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.GLOBAL)))) {
                    sourceElement.compileErrorInAttribute("Only global declarations can be static", "XTSE0020", "static");
                }
            }
            declaredType = combineTypeDeclarations(asAtt, extraAsAtt);
            if ((visibilityAtt != null)) {
                if (hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.PARAM)) {
                    sourceElement.compileErrorInAttribute("The visibility attribute is not allowed on xsl:param", "XTSE0020", "visibility");
                } else {
                    visibility = sourceElement.interpretVisibilityValue(visibilityAtt, "");
                }
                if (!(hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.GLOBAL))) {
                    sourceElement.compileErrorInAttribute("The visibility attribute is allowed only on global declarations", "XTSE0020", "visibility");
                }
            }
            if (((hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.STATIC) && (visibility != Saxon.Hej.trans.Visibility.PRIVATE)) && (visibilityAtt != null))) {
                sourceElement.compileErrorInAttribute("A static variable or parameter must be private", "XTSE0020", "static");
            }
        }
        public void prepareTemplateSignatureAttributes() {
            Saxon.Hej.om.AttributeMap atts = sourceElement.attributes();
            string asAtt = null;
            string extraAsAtt = null;
            foreach (Saxon.Hej.om.AttributeInfo att in atts) {
                Saxon.Hej.om.NodeName attName = att.getNodeName();
                string f = attName.getDisplayName();
                if (f.Equals("name")) {
                    if (((name == null) || name.Equals(errorName()))) {
                        processVariableName(att.getValue());
                    }
                } else {
                    if (f.Equals("as")) {
                        asAtt = att.getValue();
                    } else {
                        if (f.Equals("required")) {
                            string requiredAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                            bool required = sourceElement.processBooleanAttribute("required", requiredAtt);
                            setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.REQUIRED, required);
                        } else {
                            if (f.Equals("tunnel")) {
                                string tunnelAtt = Saxon.Hej.value.Whitespace.trim(att.getValue());
                                bool tunnel = sourceElement.processBooleanAttribute("tunnel", tunnelAtt);
                                setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.TUNNEL, tunnel);
                            } else {
                                if (Saxon.Hej.lib.NamespaceConstant.SAXON.Equals(attName.getURI())) {
                                    if (attName.getLocalPart().Equals("as")) {
                                        extraAsAtt = att.getValue();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if ((name == null)) {
                sourceElement.reportAbsence("name");
                name = errorName();
            }
            declaredType = combineTypeDeclarations(asAtt, extraAsAtt);
        }
        private Saxon.Hej.value.SequenceType combineTypeDeclarations(string asAtt, string extraAsAtt) {
            Saxon.Hej.value.SequenceType declaredType = null;
            if ((asAtt != null)) {
                try {
                    declaredType = sourceElement.makeSequenceType(asAtt);
                } catch (Saxon.Hej.trans.XPathException e) {
                    sourceElement.compileErrorInAttribute(e.Message, e.getErrorCodeLocalPart(), "as");
                }
            }
            if ((extraAsAtt != null)) {
                Saxon.Hej.value.SequenceType extraResultType = null;
                try {
                    extraResultType = sourceElement.makeExtendedSequenceType(extraAsAtt);
                } catch (Saxon.Hej.trans.XPathException e) {
                    sourceElement.compileErrorInAttribute(e.Message, e.getErrorCodeLocalPart(), "saxon:as");
                    extraResultType = Saxon.Hej.value.SequenceType.ANY_SEQUENCE;
                }
                if ((asAtt != null)) {
                    Saxon.Hej.type.Affinity rel = sourceElement.getConfiguration().getTypeHierarchy().sequenceTypeRelationship(extraResultType, declaredType);
                    if (((rel == Saxon.Hej.type.Affinity.SAME_TYPE) || (rel == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                        declaredType = extraResultType;
                    } else {
                        sourceElement.compileErrorInAttribute("When both are present, @saxon:as must be a subtype of @as", "SXER7TBA", "as");
                    }
                } else {
                    declaredType = extraResultType;
                }
            }
            return declaredType;
        }
        public Saxon.Hej.style.StyleElement getSourceElement() {
            return sourceElement;
        }
        public void setVariableQName(Saxon.Hej.om.StructuredQName name) {
            this.name = name;
        }
        public void setDeclaredType(Saxon.Hej.value.SequenceType declaredType) {
            this.declaredType = declaredType;
        }
        private void processVariableName(string nameAttribute) {
            if ((nameAttribute != null)) {
                if (nameAttribute.StartsWith("$")) {
                    sourceElement.compileErrorInAttribute("Invalid variable name (no '$' sign needed)", "XTSE0020", "name");
                    nameAttribute = nameAttribute.Substring(1);
                }
                name = sourceElement.makeQName(nameAttribute, null, "name");
            }
        }
        private Saxon.Hej.om.StructuredQName errorName() {
            return new Saxon.Hej.om.StructuredQName("saxon", Saxon.Hej.lib.NamespaceConstant.SAXON, "error-variable-name");
        }
        public void validate() {
            if (((select != null) && sourceElement.hasChildNodes())) {
                sourceElement.compileError((("An "+sourceElement.getDisplayName())+" element with a select attribute must be empty"), "XTSE0620");
            }
            if ((hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.DISALLOWS_CONTENT) && sourceElement.hasChildNodes())) {
                if (isStatic()) {
                    sourceElement.compileError("A static variable or parameter must have no content", "XTSE0010");
                } else {
                    sourceElement.compileError("Within xsl:function, an xsl:param element must have no content", "XTSE0620");
                }
            }
            if (((visibility == Saxon.Hej.trans.Visibility.ABSTRACT) && (((select != null) || sourceElement.hasChildNodes())))) {
                sourceElement.compileError("An abstract variable must have no select attribute and no content", "XTSE0620");
            }
        }
        public void postValidate() {
            checkAgainstRequiredType(declaredType);
            if ((((select == null) && !(hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.DISALLOWS_CONTENT))) && (visibility != Saxon.Hej.trans.Visibility.ABSTRACT))) {
                Saxon.Hej.tree.iter.AxisIterator kids = sourceElement.iterateAxis(Saxon.Hej.om.AxisInfo.CHILD);
                Saxon.Hej.om.NodeInfo first = ((/*1430*/Saxon.Hej.om.NodeInfo)kids.next());
                if ((first == null)) {
                    if ((declaredType == null)) {
                        select = new Saxon.Hej.expr.StringLiteral(Saxon.Hej.value.StringValue.EMPTY_STRING);
                        select.setRetainedStaticContext(sourceElement.makeRetainedStaticContext());
                    } else {
                        if (((sourceElement is Saxon.Hej.style.XSLLocalParam) || (sourceElement is Saxon.Hej.style.XSLGlobalParam))) {
                            if (!(hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.REQUIRED))) {
                                if (Saxon.Hej.value.Cardinality.allowsZero(declaredType.getCardinality())) {
                                    select = Saxon.Hej.expr.Literal.makeEmptySequence();
                                    select.setRetainedStaticContext(sourceElement.makeRetainedStaticContext());
                                } else {
                                    setProperty(Saxon.Hej.style.SourceBinding.BindingProperty.IMPLICITLY_REQUIRED, true);
                                }
                            }
                        } else {
                            if (Saxon.Hej.value.Cardinality.allowsZero(declaredType.getCardinality())) {
                                select = Saxon.Hej.expr.Literal.makeEmptySequence();
                                select.setRetainedStaticContext(sourceElement.makeRetainedStaticContext());
                            } else {
                                sourceElement.compileError("The implicit value () is not valid for the declared type", "XTTE0570");
                            }
                        }
                    }
                }
            }
            select = sourceElement.typeCheck("select", select);
        }
        public bool isStatic() {
            return hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.STATIC);
        }
        public void checkAgainstRequiredType(Saxon.Hej.value.SequenceType required) {
            if ((visibility != Saxon.Hej.trans.Visibility.ABSTRACT)) {
                try {
                    if ((required != null)) {
                        if ((select != null)) {
                            int category = Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE;
                            string errorCode = "XTTE0570";
                            if ((sourceElement is Saxon.Hej.style.XSLLocalParam)) {
                                category = Saxon.Hej.expr.parser.RoleDiagnostic.PARAM;
                                errorCode = "XTTE0600";
                            } else {
                                if (((sourceElement is Saxon.Hej.style.XSLWithParam) || (sourceElement is Saxon.Hej.style.XSLGlobalParam))) {
                                    category = Saxon.Hej.expr.parser.RoleDiagnostic.PARAM;
                                    errorCode = "XTTE0590";
                                }
                            }
                            Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(category, name.getDisplayName(), 0);
                            role.setErrorCode(errorCode);
                            select = sourceElement.getConfiguration().getTypeChecker(false).staticTypeCheck(select, required, role, sourceElement.makeExpressionVisitor());
                        } else {
                        }
                    }
                } catch (Saxon.Hej.trans.XPathException err) {
                    err.setLocator(sourceElement);
                    sourceElement.compileError(err);
                    select = new Saxon.Hej.expr.ErrorExpression(new Saxon.Hej.trans.XmlProcessingException(err));
                }
            }
        }
        public Saxon.Hej.om.StructuredQName getVariableQName() {
            if ((name == null)) {
                processVariableName(sourceElement.getAttributeValue("", "name"));
            }
            return name;
        }
        public void setProperty(Saxon.Hej.style.SourceBinding.BindingProperty prop, bool flag) {
            if (flag) {
                properties |= (prop);
            } else {
                properties &= ~(prop);
            }
        }
        public bool hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty prop) {
            return properties.HasFlag(prop);
        }
        public System.Collections.Generic.IList<Saxon.Hej.expr.BindingReference> getReferences() {
            return references;
        }
        public Saxon.Hej.expr.instruct.SlotManager getSlotManager() {
            return slotManager;
        }
        public void handleSequenceConstructor(Saxon.Hej.style.Compilation compilation, Saxon.Hej.style.ComponentDeclaration decl) {
            if (sourceElement.hasChildNodes()) {
                if ((declaredType == null)) {
                    Saxon.Hej.expr.Expression b = sourceElement.compileSequenceConstructor(compilation, decl, true);
                    if ((b == null)) {
                        b = Saxon.Hej.expr.Literal.makeEmptySequence();
                    }
                    bool textonly = Saxon.Hej.type.UType.TEXT.subsumes(b.getItemType().getUType());
                    Saxon.Hej.str.UnicodeString constant = null;
                    if (((textonly && (b is Saxon.Hej.expr.instruct.ValueOf)) && (((Saxon.Hej.expr.instruct.ValueOf)(b)).getSelect() is Saxon.Hej.expr.StringLiteral))) {
                        constant = ((Saxon.Hej.expr.StringLiteral)(((Saxon.Hej.expr.instruct.ValueOf)(b)).getSelect())).getString();
                    }
                    Saxon.Hej.expr.instruct.DocumentInstr doc = new Saxon.Hej.expr.instruct.DocumentInstr(textonly, constant);
                    doc.setContentExpression(b);
                    doc.setRetainedStaticContext(sourceElement.makeRetainedStaticContext());
                    select = doc;
                } else {
                    select = sourceElement.compileSequenceConstructor(compilation, decl, true);
                    if ((select == null)) {
                        select = Saxon.Hej.expr.Literal.makeEmptySequence();
                    }
                    try {
                        Saxon.Hej.expr.parser.RoleDiagnostic role = new Saxon.Hej.expr.parser.RoleDiagnostic(Saxon.Hej.expr.parser.RoleDiagnostic.VARIABLE, name.getDisplayName(), 0);
                        role.setErrorCode("XTTE0570");
                        select = select.simplify();
                        select = sourceElement.getConfiguration().getTypeChecker(false).staticTypeCheck(select, declaredType, role, sourceElement.makeExpressionVisitor());
                    } catch (Saxon.Hej.trans.XPathException err) {
                        err.setLocator(sourceElement);
                        Saxon.Hej.s9api.XmlProcessingError error = new Saxon.Hej.trans.XmlProcessingException(err);
                        sourceElement.compileError(error);
                        select = new Saxon.Hej.expr.ErrorExpression(error);
                    }
                }
            }
        }
        public Saxon.Hej.value.SequenceType getDeclaredType() {
            if ((declaredType == null)) {
                string asAtt = sourceElement.getAttributeValue("", "as");
                if ((asAtt == null)) {
                    return null;
                } else {
                    try {
                        declaredType = sourceElement.makeSequenceType(asAtt);
                    } catch (Saxon.Hej.trans.XPathException err) {
                    }
                }
            }
            return declaredType;
        }
        public Saxon.Hej.expr.Expression getSelectExpression() {
            return select;
        }
        public Saxon.Hej.value.SequenceType getInferredType(bool useContentRules) {
            if ((inferredType != null)) {
                return inferredType;
            }
            Saxon.Hej.trans.Visibility visibility = sourceElement.getVisibility();
            if (((hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.PARAM) || hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.ASSIGNABLE)) || !((((visibility == Saxon.Hej.trans.Visibility.PRIVATE) || (visibility == Saxon.Hej.trans.Visibility.FINAL)))))) {
                Saxon.Hej.value.SequenceType declared = getDeclaredType();
                return inferredType = ((declared == null) ? Saxon.Hej.value.SequenceType.ANY_SEQUENCE : declared);
            }
            if ((select != null)) {
                Saxon.Hej.type.TypeHierarchy th = sourceElement.getConfiguration().getTypeHierarchy();
                if (Saxon.Hej.expr.Literal.isEmptySequence(select)) {
                    return inferredType = ((declaredType == null) ? Saxon.Hej.value.SequenceType.ANY_SEQUENCE : declaredType);
                }
                Saxon.Hej.type.ItemType actual = select.getItemType();
                int card = select.getCardinality();
                if ((declaredType != null)) {
                    if (!(th.isSubType(actual, declaredType.getPrimaryType()))) {
                        actual = declaredType.getPrimaryType();
                    }
                    if (!(Saxon.Hej.value.Cardinality.subsumes(declaredType.getCardinality(), card))) {
                        card = declaredType.getCardinality();
                    }
                }
                inferredType = Saxon.Hej.value.SequenceType.makeSequenceType(actual, card);
                return inferredType;
            }
            if (useContentRules) {
                if (sourceElement.hasChildNodes()) {
                    if ((declaredType == null)) {
                        return Saxon.Hej.value.SequenceType.makeSequenceType(Saxon.Hej.pattern.NodeKindTest.DOCUMENT, Saxon.Hej.expr.StaticProperty.EXACTLY_ONE);
                    } else {
                        return declaredType;
                    }
                } else {
                    if ((declaredType == null)) {
                        return Saxon.Hej.value.SequenceType.SINGLE_STRING;
                    } else {
                        return declaredType;
                    }
                }
            }
            return declaredType;
        }
        public void registerReference(Saxon.Hej.expr.BindingReference @ref) {
            references.Add(@ref);
        }
        public Saxon.Hej.om.GroundedValue getConstantValue() {
            if ((constantValue == null)) {
                Saxon.Hej.value.SequenceType type = getInferredType(true);
                Saxon.Hej.type.TypeHierarchy th = sourceElement.getConfiguration().getTypeHierarchy();
                if (((!(hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.ASSIGNABLE)) && !(hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.PARAM))) && !((((visibility == Saxon.Hej.trans.Visibility.PUBLIC) || (visibility == Saxon.Hej.trans.Visibility.ABSTRACT)))))) {
                    if ((select is Saxon.Hej.expr.Literal)) {
                        Saxon.Hej.type.Affinity relation = th.relationship(select.getItemType(), type.getPrimaryType());
                        if (((relation == Saxon.Hej.type.Affinity.SAME_TYPE) || (relation == Saxon.Hej.type.Affinity.SUBSUMED_BY))) {
                            constantValue = ((Saxon.Hej.expr.Literal)(select)).getGroundedValue();
                        }
                    }
                }
            }
            return constantValue;
        }
        public void fixupReferences(Saxon.Hej.expr.instruct.GlobalVariable compiledGlobalVariable) {
            Saxon.Hej.value.SequenceType type = getInferredType(true);
            int properties = 0;
            if (((!(hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.ASSIGNABLE)) && !(hasProperty(Saxon.Hej.style.SourceBinding.BindingProperty.PARAM))) && !((((visibility == Saxon.Hej.trans.Visibility.PUBLIC) || (visibility == Saxon.Hej.trans.Visibility.ABSTRACT)))))) {
                if ((select != null)) {
                    properties = select.getSpecialProperties();
                }
            }
            foreach (Saxon.Hej.expr.BindingReference reference in references) {
                if ((compiledGlobalVariable != null)) {
                    reference.fixup(compiledGlobalVariable);
                }
                reference.setStaticType(type, getConstantValue(), properties);
            }
        }
        protected internal void fixupBinding(Saxon.Hej.expr.Binding binding) {
            foreach (Saxon.Hej.expr.BindingReference reference in references) {
                reference.fixup(binding);
            }
        }
    
    }
}