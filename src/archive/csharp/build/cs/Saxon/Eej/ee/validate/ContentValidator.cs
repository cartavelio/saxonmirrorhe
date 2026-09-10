using System;
using System.Collections.Generic;
using System.Linq;
namespace Saxon.Eej.ee.validate {
    public abstract class ContentValidator : Saxon.Eej.ee.validate.ValidatingFilter {
        public enum Nillability {
            IGNORED,
            ALLOWED,
            DISALLOWED,
        }
        protected internal Saxon.Eej.ee.validate.ContentValidator childValidator;
        private Saxon.Hej.om.StructuredQName containingElement = null;
        private Saxon.Hej.s9api.Location containingElementLocation;
        protected internal bool nilled = false;
        private Saxon.Eej.ee.validate.ContentValidator.Nillability nillability = Saxon.Eej.ee.validate.ContentValidator.Nillability.IGNORED;
        private Saxon.Eej.ee.schema.ElementDecl elementDecl;
        private Saxon.Hej.type.SchemaType annotation = Saxon.Hej.type.Untyped.getInstance();
        protected internal System.Collections.Generic.IList<string> pendingMessages;
        private Saxon.Hej.om.NamespaceResolver namespaceResolver;
        public ContentValidator(Saxon.Hej.@event.Receiver next) : base (next) {
        }
        public void setContainingElement(Saxon.Hej.om.StructuredQName name, Saxon.Hej.s9api.Location locationId) {
            containingElement = name;
            containingElementLocation = locationId;
        }
        protected internal void setElementDeclaration(Saxon.Eej.ee.schema.ElementDecl decl) {
            elementDecl = decl;
        }
        protected internal Saxon.Eej.ee.schema.ElementDecl getElementDeclaration() {
            return elementDecl;
        }
        public Saxon.Hej.om.StructuredQName getContainingElement() {
            return containingElement;
        }
        public Saxon.Hej.s9api.Location getContainingElementLocationId() {
            return containingElementLocation;
        }
        protected internal string getContainingElementName() {
            Saxon.Hej.om.StructuredQName nn = getContainingElement();
            if ((nn == null)) {
                return "element";
            } else {
                return ("element "+Saxon.Hej.trans.Err.wrap(nn.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT));
            }
        }
        internal void setNamespaceResolver(Saxon.Hej.om.NamespaceResolver resolver) {
            this.namespaceResolver = resolver;
        }
        public abstract Saxon.Hej.type.SchemaType getSchemaType();
        protected internal void setAnnotation(Saxon.Hej.type.SchemaType annotation) {
            this.annotation = annotation;
        }
        protected internal Saxon.Hej.type.SchemaType getAnnotation() {
            return annotation;
        }
        public Saxon.Hej.om.NamespaceResolver getNamespaceResolver() {
            return namespaceResolver;
        }
        public void setNillability(Saxon.Eej.ee.validate.ContentValidator.Nillability nillability) {
            this.nillability = nillability;
        }
        public Saxon.Eej.ee.validate.ContentValidator.Nillability getNillability() {
            return nillability;
        }
        public void setNilled(bool nilled) {
            this.nilled = nilled;
        }
        public bool isNilled() {
            return nilled;
        }
        public override void startElement(Saxon.Hej.om.NodeName elemName, Saxon.Hej.type.SchemaType type, Saxon.Hej.om.AttributeMap attributes, Saxon.Hej.om.NamespaceMap namespaces, Saxon.Hej.s9api.Location location, int properties) {
            Saxon.Eej.ee.validate.ContentValidator childValidator = getChildValidator();
            if ((getConstraintChecker() != null)) {
                Saxon.Eej.ee.schema.ElementDecl element = childValidator.getElementDeclaration();
                if ((element != null)) {
                    getConstraintChecker().setElementDeclaration(element);
                }
            }
            System.Collections.Generic.IList<Saxon.Hej.om.NamespaceBinding> additionalNamespaces = new System.Collections.Generic.List<Saxon.Hej.om.NamespaceBinding>();
            childValidator.setNamespaceResolver(namespaces);
            Saxon.Hej.om.AttributeMap validatedAtts = childValidator.validateAttributes(attributes, additionalNamespaces);
            foreach (Saxon.Hej.om.NamespaceBinding ns in additionalNamespaces) {
                namespaces = namespaces.put(ns.getPrefix(), ns.getNamespaceUri());
            }
            try {
                nextReceiver.startElement(elemName, type, validatedAtts, namespaces, location, properties);
            } catch (Saxon.Hej.type.ValidationException ve) {
                reportValidationError(ve.getValidationFailure(), true, location);
            }
        }
        public static Saxon.Eej.ee.validate.ContentValidator makeValidator(Saxon.Eej.ee.schema.ElementDecl decl, Saxon.Hej.om.StructuredQName elementName, Saxon.Hej.s9api.Location locationId, Saxon.Eej.ee.validate.ValidationContext validationContext, Saxon.Hej.type.SchemaType governingType, int validation, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.@event.Receiver nextReceiver, System.Collections.Generic.IList<Saxon.Hej.type.ValidationFailure> failures) {
            Saxon.Hej.Configuration config = pipe.getConfiguration();
            Saxon.Eej.ee.validate.ContentValidator val;
            Saxon.Eej.ee.validate.ContentValidator.Nillability nillability = Saxon.Eej.ee.validate.ContentValidator.Nillability.IGNORED;
            if (((validation == Saxon.Hej.lib.Validation.STRIP) && (governingType == null))) {
                val = new Saxon.Eej.ee.validate.SkipValidator(nextReceiver);
            } else {
                if ((validation == Saxon.Hej.lib.Validation.BY_TYPE)) {
                    val = makeValidatorForType(null, governingType, pipe, nextReceiver, failures);
                    if (!(failures.Count==0)) {
                        return null;
                    }
                } else {
                    if ((decl == null)) {
                        if ((governingType != null)) {
                            val = makeValidatorForType(null, governingType, pipe, nextReceiver, failures);
                            if (!(failures.Count==0)) {
                                return null;
                            }
                        } else {
                            if ((validation == Saxon.Hej.lib.Validation.STRICT)) {
                                string message = (("Cannot validate "+Saxon.Hej.trans.Err.wrap(elementName.getStructuredQName().getEQName(), Saxon.Hej.trans.Err.ELEMENT))+": no element declaration available");
                                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
                                ve.setConstraintReference(1, "cvc-elt", "1");
                                ve.setErrorCode((pipe.isXSLT() ? "XTTE1512" : "XQDY0084"));
                                failures.Add(ve);
                                return null;
                            }
                            val = new Saxon.Eej.ee.validate.LaxValidator(nextReceiver);
                            val.setUnderlyingReceiver(nextReceiver);
                        }
                    } else {
                        if (decl.isAbstract()) {
                            string msg = (("Invalid element "+Saxon.Hej.trans.Err.wrap(elementName.getDisplayName(), Saxon.Hej.trans.Err.ELEMENT))+": the element is declared to be abstract");
                            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(msg);
                            failures.Add(ve);
                            return null;
                        } else {
                            nillability = (decl.isNillable() ? Saxon.Eej.ee.validate.ContentValidator.Nillability.ALLOWED : Saxon.Eej.ee.validate.ContentValidator.Nillability.DISALLOWED);
                            config.sealNamespace(decl.getTargetNamespace());
                            if (((governingType != null) && (governingType.getFingerprint() == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC))) {
                                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure("Cannot use xs:untypedAtomic as governing type for validation");
                                failures.Add(ve);
                                return null;
                            }
                            Saxon.Hej.type.SchemaType elType = decl.getType();
                            Saxon.Eej.ee.validate.AttributeInheritor startTagBuffer = validationContext.getAttributeInheritor();
                            if (((startTagBuffer != null) && decl.hasTypeAlternatives())) {
                                try {
                                    elType = getConditionalType(validationContext, decl);
                                } catch (Saxon.Hej.trans.XPathException e) {
                                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(("Failure during conditional type assignment: "+e.Message));
                                    failures.Add(ve);
                                    return null;
                                }
                            }
                            if (((elType != null) && (governingType != null))) {
                                int block = (decl.getBlock()|elType.getBlock());
                                try {
                                    config.checkTypeDerivationIsOK(governingType, elType, block);
                                } catch (Saxon.Hej.type.SchemaException err) {
                                    Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(("The xsi:type is not validly derived from the declared type. "+err.Message));
                                    ve.setConstraintReference(1, "cvc-elt", "4.3");
                                    failures.Add(ve);
                                    return null;
                                }
                                elType = governingType;
                            }
                            if ((elType == null)) {
                                if ((governingType == null)) {
                                    val = new Saxon.Eej.ee.validate.AnyTypeValidator(nextReceiver);
                                } else {
                                    val = makeValidatorForType(decl, governingType, pipe, nextReceiver, failures);
                                    if (!(failures.Count==0)) {
                                        return null;
                                    }
                                }
                            } else {
                                if (((elType is Saxon.Eej.ee.schema.UserComplexType) && ((Saxon.Hej.type.ComplexType)(elType)).isAbstract())) {
                                    if ((governingType == null)) {
                                        Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure("Cannot validate content against an abstract type");
                                        ve.setConstraintReference(1, "cvc-complex-type", "1");
                                        failures.Add(ve);
                                        return null;
                                    } else {
                                        val = makeValidatorForType(decl, governingType, pipe, nextReceiver, failures);
                                        if (!(failures.Count==0)) {
                                            return null;
                                        }
                                    }
                                } else {
                                    governingType = elType;
                                    val = makeValidatorForType(decl, elType, pipe, nextReceiver, failures);
                                    if (!(failures.Count==0)) {
                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            System.Diagnostics.Debug.Assert((val != null));
            val.setPipelineConfiguration(pipe);
            val.setContainingElement(elementName.getStructuredQName(), locationId);
            val.setValidationContext(validationContext);
            val.setNillability(nillability);
            val.setElementDeclaration(decl);
            val.setAnnotation(((governingType == null) ? Saxon.Hej.type.Untyped.getInstance() : governingType));
            return val;
        }
        private static Saxon.Hej.type.SchemaType getConditionalType(Saxon.Eej.ee.validate.ValidationContext validationContext, Saxon.Eej.ee.schema.ElementDecl decl) {
            Saxon.Eej.ee.validate.AttributeInheritor startTagBuffer = validationContext.getAttributeInheritor();
            Saxon.Hej.type.SchemaType eltype = decl.getType();
            Saxon.Hej.om.NodeInfo element = startTagBuffer.createSkeletonElementNode();
            foreach (Saxon.Eej.ee.schema.TypeAlternative alt in decl.getTypeAlternatives()) {
                Saxon.Hej.sxpath.XPathExpression exp = alt.getCondition();
                if ((exp == null)) {
                    eltype = alt.getSchemaType();
                    break ;
                } else {
                    Saxon.Hej.Controller controller = validationContext.getController();
                    Saxon.Hej.sxpath.XPathDynamicContext context = exp.createDynamicContext(controller, element);
                    /*134*/Saxon.Eej.ee.schema.Assertion.setDocumentsAndCollections(controller.getConfiguration(), context);
                    Saxon.Hej.type.ValidationParams actualParams = startTagBuffer.getPipelineConfiguration().getParseOptions().getValidationParams();
                    System.Collections.Generic.IDictionary<Saxon.Hej.om.StructuredQName,Saxon.Hej.sxpath.XPathVariable> declaredParams = alt.getParameters();
                    Saxon.Hej.type.ValidationParams.setValidationParams(declaredParams, actualParams, context);
                    try {
                        if (Saxon.Hej.expr.parser.ExpressionTool.effectiveBooleanValue(exp.iterate(context))) {
                            eltype = alt.getSchemaType();
                            break ;
                        }
                    } catch (Saxon.Hej.trans.XPathException err) {
                        Saxon.Hej.@event.PipelineConfiguration pipe = startTagBuffer.getPipelineConfiguration();
                        Saxon.Hej.trans.XmlProcessingIncident warning = ((/*1430*/Saxon.Hej.trans.XmlProcessingIncident)new Saxon.Hej.trans.XmlProcessingIncident(("Dynamic error occurred during evaluation of Conditional Type Alternative test: "+err.Message), Saxon.Hej.trans.SaxonErrorCode.SXWN9047).asWarning());
                        warning.setCause(err);
                        pipe.getErrorReporter().report(warning);
                    }
                }
            }
            return eltype;
        }
        protected internal abstract Saxon.Eej.ee.validate.ContentValidator getChildValidator();
        public static Saxon.Eej.ee.validate.ContentValidator makeValidatorForType(Saxon.Eej.ee.schema.ElementDecl elementDecl, Saxon.Hej.type.SchemaType schemaType, Saxon.Hej.@event.PipelineConfiguration pipe, Saxon.Hej.@event.Receiver nextReceiver, System.Collections.Generic.IList<Saxon.Hej.type.ValidationFailure> failures) {
            Saxon.Eej.ee.validate.ContentValidator val;
            if ((schemaType is Saxon.Hej.type.SimpleType)) {
                val = new Saxon.Eej.ee.validate.SimpleContentValidator(elementDecl, schemaType, nextReceiver);
            } else {
                if ((schemaType is Saxon.Hej.type.AnyType)) {
                    val = new Saxon.Eej.ee.validate.AnyTypeValidator(elementDecl, nextReceiver);
                } else {
                    if ((schemaType.getFingerprint() == Saxon.Hej.om.StandardNames.XS_UNTYPED_ATOMIC)) {
                        Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure("Cannot use xs:untypedAtomic as governing type for validation");
                        failures.Add(ve);
                        return null;
                    } else {
                        Saxon.Hej.type.ComplexType complexType = (Saxon.Hej.type.ComplexType)(schemaType);
                        if (complexType.isSimpleContent()) {
                            val = new Saxon.Eej.ee.validate.SimpleContentValidator(elementDecl, complexType, nextReceiver);
                        } else {
                            if (complexType.isAbstract()) {
                                Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure("Cannot validate content against an abstract type");
                                ve.setConstraintReference(1, "cvc-complex-type", "1");
                                failures.Add(ve);
                                return null;
                            } else {
                                if (complexType.isEmptyContent()) {
                                    val = new Saxon.Eej.ee.validate.EmptyContentValidator((Saxon.Eej.ee.schema.UserComplexType)(complexType), nextReceiver);
                                } else {
                                    if (complexType.isAllContent()) {
                                        val = new Saxon.Eej.ee.validate.AllElementValidator((Saxon.Eej.ee.schema.UserComplexType)(complexType), nextReceiver);
                                    } else {
                                        val = new Saxon.Eej.ee.validate.ComplexContentValidator(elementDecl, (Saxon.Eej.ee.schema.UserComplexType)(complexType), nextReceiver);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return val;
        }
        public override void endDocument() {
            reportIfInvalid();
            nextReceiver.endDocument();
        }
        protected internal void makeChildValidator(Saxon.Eej.ee.schema.ElementDecl decl, Saxon.Hej.om.StructuredQName elementName, Saxon.Hej.s9api.Location locationId, int process) {
            System.Collections.Generic.IList<Saxon.Hej.type.ValidationFailure> failures = new System.Collections.Generic.List<Saxon.Hej.type.ValidationFailure>();
            childValidator = makeValidator(decl, elementName, locationId, getValidationContext(), getValidationContext().getXSIType(), process, getPipelineConfiguration(), getNextReceiver(), failures);
            foreach (Saxon.Hej.type.ValidationFailure failure in failures) {
                reportValidationError(failure, false, locationId);
                if ((childValidator == null)) {
                    childValidator = new Saxon.Eej.ee.validate.SkipValidator(getNextReceiver());
                    childValidator.setValidationContext(getValidationContext());
                }
            }
            if ((childValidator != null)) {
                childValidator.setConstraintChecker(getConstraintChecker());
            }
        }
        public virtual Saxon.Hej.om.AttributeMap validateAttributes(Saxon.Hej.om.AttributeMap attributes, System.Collections.Generic.IList<Saxon.Hej.om.NamespaceBinding> additionalNamespaces) {
            return attributes;
        }
        protected internal void checkNoChildrenWhenNil(Saxon.Hej.s9api.Location location) {
            if (nilled) {
                reportContentWhenNil("No child elements are allowed when xsi:nil is true", location);
            }
        }
        protected internal void checkNoCharactersWhenNil(Saxon.Hej.s9api.Location location) {
            if (nilled) {
                reportContentWhenNil("No character content is allowed when xsi:nil is true", location);
            }
        }
        private void reportContentWhenNil(string message, Saxon.Hej.s9api.Location location) {
            Saxon.Hej.type.ValidationFailure ve = new Saxon.Hej.type.ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-elt", "3.2.1");
            ve.setSchemaType(getSchemaType());
            reportValidationError(ve, true, location);
        }
    
    }
}