package com.saxonica.ee.validate;

import com.saxonica.ee.schema.Assertion;
import com.saxonica.ee.schema.ElementDecl;
import com.saxonica.ee.schema.TypeAlternative;
import com.saxonica.ee.schema.UserComplexType;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.parser.ExpressionTool;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.sxpath.XPathDynamicContext;
import net.sf.saxon.sxpath.XPathExpression;
import net.sf.saxon.sxpath.XPathVariable;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XmlProcessingIncident;
import net.sf.saxon.transpile.CSharpSimpleEnum;
import net.sf.saxon.type.*;

import java.util.ArrayList;
import java.util.List;
import java.util.Map;


/**
 * This class is an abstract superclass for all the validators.
 * The class includes factory methods for constructing a validator.
 */


public abstract class ContentValidator extends ValidatingFilter {

    @CSharpSimpleEnum
    public enum Nillability {
        /**
         * Constant indicating that xsi:nil is allowed but ignored; this is true when we are validating
         * against a type, rather than against an element declaration
         */
        IGNORED,
        /**
         * Constant indicating that xsi:nil is allowed and active. That is, we are validating against
         * an element declaration that specifies nillable=true
         */
        ALLOWED,
        /**
         * Constant indicating that xsi:nil is disallowed. That is, we are validating against
         * an element declaration that specifies nillable=false
         */
        DISALLOWED}


    // The validator used to validate the current child element of this element
    protected ContentValidator childValidator;

    private StructuredQName containingElement = null;
    private Location containingElementLocation;
    protected boolean nilled = false;
    private Nillability nillability = Nillability.IGNORED;
    private ElementDecl elementDecl;

    private SchemaType annotation = Untyped.getInstance();
    protected List<String> pendingMessages;
    private NamespaceResolver namespaceResolver;


    public ContentValidator(Receiver next) {
        super(next);
    }

    /**
     * Set the name of the containing element (for diagnostics)
     *
     * @param name identifies the name of the containing element
     * @param locationId  identifies the location of the containing element
     */

    public void setContainingElement(StructuredQName name, Location locationId) {
        containingElement = name;
        containingElementLocation = locationId;
    }

    /**
     * Set the element declaration that this validator is validating against
     *
     * @param decl the element declaration
     */

    protected void setElementDeclaration(ElementDecl decl) {
        elementDecl = decl;
    }

    /**
     * Get the element declaration that this validator is validating against
     *
     * @return the element declaration
     */

    protected ElementDecl getElementDeclaration() {
        return elementDecl;
    }


    /**
     * Get the name of the element being validated (for diagnostics).
     *
     * @return the element name, or null if not available.
     */

    public StructuredQName getContainingElement() {
        return containingElement;
    }

    /**
     * Get the location of the element being validated
     *
     * @return the location, or 0 if not available
     */

    public Location getContainingElementLocationId() {
        return containingElementLocation;
    }

    /**
     * Get the containing element name, for use in error messages.
     *
     * @return the name of the containing element (plus surrounding punctuation), or the
     *         string "element" if not available
     */

    protected String getContainingElementName() {
        StructuredQName nn = getContainingElement();
        if (nn == null) {
            return "element";
        } else {
            return "element " + Err.wrap(nn.getDisplayName(), Err.ELEMENT);
        }
    }

    void setNamespaceResolver(NamespaceResolver resolver) {
        this.namespaceResolver = resolver;
    }

    /**
     * Get the schema type against which we are validating, if any
     *
     * @return the schema type that this validator is validating against, or null
     */

    public abstract SchemaType getSchemaType();

    /**
     * Set the type annotation code to be allocated by this validator
     *
     * @param annotation the integer fingerprint of the name of the type
     */

    protected final void setAnnotation(SchemaType annotation) {
        this.annotation = annotation;
    }

    /**
     * Get the type annotation code to be used on elements validated by this validator
     *
     * @return the integer fingerprint of the name of the type
     */

    protected final SchemaType getAnnotation() {
        return annotation;
    }


    /**
     * Get the namespace resolver
     *
     * @return the namespace resolver
     */

    public NamespaceResolver getNamespaceResolver() {
        return namespaceResolver;
    }

    /**
     * Set whether this type permits xsi:nil = true
     *
     * @param nillability one of {@link Nillability#IGNORED}, {@link Nillability#ALLOWED}, {@link Nillability#DISALLOWED}
     */

    public void setNillability(Nillability nillability) {
        this.nillability = nillability;
    }

    /**
     * Determine whether this type permits xsi:nil = true       BlockCom
     *
     * @return one of {@link Nillability#IGNORED}, {@link Nillability#ALLOWED}, {@link Nillability#DISALLOWED}
     *
     */

    public Nillability getNillability() {
        return nillability;
    }

    /**
     * Tell this validator whether xsi:nil="true" was specified on the element
     * being validated
     * @param nilled true if xsi:nil="true" was specified on the element
     * being validated
     */

    public void setNilled(boolean nilled) {
        this.nilled = nilled;
    }

    /**
     * Determine whether xsi:nil = true was set on this element
     *
     * @return true if xsi:nil="true" was set
     */

    public boolean isNilled() {
        return nilled;
    }

    /**
     * Handle the start tag for a child element of the element being validated
     */

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties)
            throws XPathException {
        ContentValidator childValidator = getChildValidator();
        if (getConstraintChecker() != null) {
            ElementDecl element = childValidator.getElementDeclaration();
            if (element != null) {
                getConstraintChecker().setElementDeclaration(element);
            }
        }
        List<NamespaceBinding> additionalNamespaces = new ArrayList<>();
        childValidator.setNamespaceResolver(namespaces);
        AttributeMap validatedAtts = childValidator.validateAttributes(attributes, additionalNamespaces);
        for (NamespaceBinding ns : additionalNamespaces) {
            namespaces = namespaces.put(ns.getPrefix(), ns.getNamespaceUri());
        }
        try {
            nextReceiver.startElement(elemName, type, validatedAtts, namespaces, location, properties);
        } catch (ValidationException ve) {
            reportValidationError(ve.getValidationFailure(), true, location);
        }
    }

    /**
     * Factory method to make a validator for a particular element
     *
     * @param decl              the element declaration to validate against. May be null.
     * @param elementName          the name of the element (used only for diagnostics)
     * @param locationId        the location of the element, or of the stylesheet instruction that generated this element (or zero)
     * @param validationContext information about the validation episode
     * @param governingType     the type identified by xsi:type, if specified; otherwise null
     * @param validation        the validation mode (strict, lax, preserve, strip)
     * @param pipe              the pipeline configuration
     * @param nextReceiver      the next receiver in the pipeline
     * @param failures          a list which the method will populate with ValidationFailure objects
     *                          in the event that a validity error is detected. (This is used to avoid throwing
     *                          exceptions in the case where validation failures are a normal event)
     * @return a Validator suitable for validating the content of this element
     */

    /*@NotNull*/
    public static ContentValidator makeValidator(/*@Nullable*/ ElementDecl decl,
                                                 StructuredQName elementName,
                                                 Location locationId,
                                                 ValidationContext validationContext,
                                                 /*@Nullable*/ SchemaType governingType,
                                                 int validation,
                                                 PipelineConfiguration pipe,
                                                 Receiver nextReceiver,
                                                 List<ValidationFailure> failures) throws MissingComponentException {
        Configuration config = pipe.getConfiguration();
        ContentValidator val;
        Nillability nillability = Nillability.IGNORED;
        if (validation == Validation.STRIP && governingType == null) {
            val = new SkipValidator(nextReceiver);
        } else if (validation == Validation.BY_TYPE) {
            val = makeValidatorForType(null, governingType, pipe, nextReceiver, failures);
            if (!failures.isEmpty()) {
                return null;
            }
        } else if (decl == null) {
            if (governingType != null) {
                val = makeValidatorForType(null, governingType, pipe, nextReceiver, failures);
                if (!failures.isEmpty()) {
                    return null;
                }
            } else {
                if (validation == Validation.STRICT) {
                    String message = "Cannot validate " +
                            Err.wrap(elementName.getStructuredQName().getEQName(), Err.ELEMENT) +
                            ": no element declaration available";
                    ValidationFailure ve = new ValidationFailure(message);
                    ve.setConstraintReference(1, "cvc-elt", "1");
                    ve.setErrorCode(pipe.isXSLT() ? "XTTE1512" : "XQDY0084");
                    failures.add(ve);
                    return null;
                }
                val = new LaxValidator(nextReceiver);
                val.setValidationContext(validationContext);
                val.setUnderlyingReceiver(nextReceiver);
            }
        } else if (decl.isAbstract()) {
            String msg = "Invalid element " +
                    Err.wrap(elementName.getDisplayName(), Err.ELEMENT) +
                    ": the element is declared to be abstract";
            ValidationFailure ve = new ValidationFailure(msg);
            failures.add(ve);
            return null;
        } else {
            // We have a usable element declaration
            nillability = decl.isNillable() ? Nillability.ALLOWED : Nillability.DISALLOWED;
            config.sealNamespace(decl.getTargetNamespace());
            if (governingType != null && governingType.getFingerprint() == StandardNames.XS_UNTYPED_ATOMIC) {
                ValidationFailure ve = new ValidationFailure("Cannot use xs:untypedAtomic as governing type for validation");
                failures.add(ve);
                return null;
            }
            SchemaType elType = decl.getType();
            AttributeInheritor startTagBuffer = validationContext.getAttributeInheritor();
            if (startTagBuffer != null && decl.hasTypeAlternatives()) {
                try {
                    elType = getConditionalType(validationContext, decl);
                } catch (XPathException e) {
                    ValidationFailure ve = new ValidationFailure("Failure during conditional type assignment: " +  e.getMessage());
                    failures.add(ve);
                    return null;
                }
            }

            if (elType != null && governingType != null) {
                // check that the xsi:type requested is consistent with the element type
                int block = decl.getBlock() | elType.getBlock();
                try {
                    config.checkTypeDerivationIsOK(governingType, elType, block);
                } catch (SchemaException err) {
                    ValidationFailure ve = new ValidationFailure(
                            "The xsi:type is not validly derived from the declared type. " + err.getMessage());
                    ve.setConstraintReference(1, "cvc-elt", "4.3");
                    failures.add(ve);
                    return null;
                }
                elType = governingType;
            }
            if (elType == null) {
                // implies xs:anyType
                if (governingType == null) {
                    val = new AnyTypeValidator(nextReceiver);
                } else {
                    val = makeValidatorForType(decl, governingType, pipe, nextReceiver, failures);
                    if (!failures.isEmpty()) {
                        return null;
                    }
                }
            } else if (elType instanceof UserComplexType && ((ComplexType) elType).isAbstract()) {
                // See Saxon bug 2100
                if (governingType == null) {
                    ValidationFailure ve = new ValidationFailure (
                            "Cannot validate content against an abstract type");
                    ve.setConstraintReference(1, "cvc-complex-type", "1");
                    failures.add(ve);
                    return null;
                } else {
                    val = makeValidatorForType(decl, governingType, pipe, nextReceiver, failures);
                    if (!failures.isEmpty()) {
                        return null;
                    }
                }
            } else {
                governingType = elType;
                val = makeValidatorForType(decl, elType, pipe, nextReceiver, failures);
                if (!failures.isEmpty()) {
                    return null;
                }
            }
        }
        assert val != null;
        val.setPipelineConfiguration(pipe);
        val.setContainingElement(elementName.getStructuredQName(), locationId);
        val.setValidationContext(validationContext);
        val.setNillability(nillability);
        val.setElementDeclaration(decl);
        val.setAnnotation(governingType == null ? Untyped.getInstance() : governingType);

        return val;
    }

    /**
     * Evaluate the conditions for conditional type assignment
     *
     * @param validationContext the validation context, which also
     *                          gives access to the buffer containing information about the current element instance and its attributes,
     *                          which forms the data on which CTA operates
     * @param decl              the element declaration, which defines the CTA conditional logic
     * @return the type selected by the conditional type assignment algorithm
     * @throws XPathException for example if construction of the subtree used for XPath evaluation fails
     */

    private static SchemaType getConditionalType(ValidationContext validationContext, ElementDecl decl)
            throws XPathException {
        // Conditional Type Assignment
        AttributeInheritor startTagBuffer = validationContext.getAttributeInheritor();
        SchemaType eltype = decl.getType();
        NodeInfo element = startTagBuffer.createSkeletonElementNode();
        for (TypeAlternative alt : decl.getTypeAlternatives()) {
            XPathExpression exp = alt.getCondition();
            if (exp == null) {
                eltype = alt.getSchemaType();
                break;
            } else {
                Controller controller = validationContext.getController();
                XPathDynamicContext context = exp.createDynamicContext(controller, element);
                Assertion.setDocumentsAndCollections(controller.getConfiguration(), context);
                // Set the values of any saxon:param validation parameters
                ValidationParams actualParams = startTagBuffer.getPipelineConfiguration().getParseOptions().getValidationParams();
                Map<StructuredQName, XPathVariable> declaredParams = alt.getParameters();
                ValidationParams.setValidationParams(declaredParams, actualParams, context);
                try {
                    if (ExpressionTool.effectiveBooleanValue(exp.iterate(context))) {
                        eltype = alt.getSchemaType();
                        break;
                    }
                } catch (XPathException err) {
                    // An error in evaluating the XPath expression is treated as evaluating to false.
                    // However, it could mean something, so we log a warning (and treat it as fatal if
                    // the error listener bombs out)
                    PipelineConfiguration pipe = startTagBuffer.getPipelineConfiguration();
                    XmlProcessingIncident warning = new XmlProcessingIncident(
                            "Dynamic error occurred during evaluation of Conditional Type Alternative test: "
                                + err.getMessage(), SaxonErrorCode.SXWN9047).asWarning();
                    warning.setCause(err);
                    pipe.getErrorReporter().report(warning);
                }
            }
        }
        return eltype;
    }

    /**
     * Get a validator to handle the children of the current element. This
     * is called immediately on return from the startElement call that starts
     * this element.
     *
     * @return the validator to be used to handle the children
     */

    protected abstract ContentValidator getChildValidator();

    /**
     * Factory method to create a validator for elements of a given type
     *
     * @param elementDecl  the element declaration if available, otherwise null
     *                     (there will be no element declaration, for example, when doing lax validation
     *                     against an xsi:type)
     * @param schemaType   the simple or complex type against which the content of the
     *                     element is to be validated
     * @param pipe         the pipeline configuration
     * @param nextReceiver the next receiver in the pipeline
     * @param failures     a list which the method will populate with any validity errors discovered by the method
     * @return a Validator that can sit in the event pipeline to perform
     *         local validation of elements declared to have this type, or null if any validity errors were found
     * @throws MissingComponentException if validation requires components that are missing from the schema. Note that unresolved
     * references are errors only if the component is actually used, so they are detected during validation.
     */

    public static ContentValidator makeValidatorForType(/*@Nullable*/ ElementDecl elementDecl,
                                                        SchemaType schemaType,
                                                        PipelineConfiguration pipe,
                                                        Receiver nextReceiver,
                                                        List<ValidationFailure> failures)
    throws MissingComponentException {
        ContentValidator val;
        if (schemaType instanceof SimpleType) {
            val = new SimpleContentValidator(elementDecl, schemaType, nextReceiver);
        } else if (schemaType instanceof AnyType) {
            val = new AnyTypeValidator(elementDecl, nextReceiver);
        } else if (schemaType.getFingerprint() == StandardNames.XS_UNTYPED_ATOMIC) {
            ValidationFailure ve = new ValidationFailure("Cannot use xs:untypedAtomic as governing type for validation");
            failures.add(ve);
            return null;
        } else {
            ComplexType complexType = (ComplexType) schemaType;
            if (complexType.isSimpleContent()) {
                val = new SimpleContentValidator(elementDecl, complexType, nextReceiver);
            } else if (complexType.isAbstract()) {
                ValidationFailure ve = new ValidationFailure(
                            "Cannot validate content against an abstract type");
                ve.setConstraintReference(1, "cvc-complex-type", "1");
                failures.add(ve);
                return null;
            } else if (complexType.isEmptyContent()) {
                val = new EmptyContentValidator((UserComplexType) complexType, nextReceiver);
            } else if (complexType.isAllContent()) {
                val = new AllElementValidator((UserComplexType) complexType, nextReceiver);
            } else {
                val = new ComplexContentValidator(elementDecl, (UserComplexType) complexType, nextReceiver);
            }
        }
        return val;
    }

    /**
     * Notify the end of a document node
     */

    @Override
    public void endDocument() throws XPathException {
        reportIfInvalid();
        nextReceiver.endDocument();
    }

    /**
     * Make a validator for the children of the element whose start tag is currently being processed. This will
     * always set some kind of validator for the children in the field childValidator, even if the method fails
     *
     * @param decl       the element declaration for this element, if applicable
     * @param elementName   the name of this element, used for diagnostics
     * @param locationId location information, used for diagnostics
     * @throws XPathException if a validation error occurs and if recovery from validation errors
     *                        has not been requested. The error will always have been reported.
     */

    protected void makeChildValidator(ElementDecl decl, StructuredQName elementName, Location locationId, int process) throws XPathException {
        List<ValidationFailure> failures = new ArrayList<>();
        childValidator = makeValidator(decl, elementName, locationId,
                                       getValidationContext(), getValidationContext().getXSIType(),
                                       process, getPipelineConfiguration(), getNextReceiver(), failures);
        for (ValidationFailure failure : failures) {
            reportValidationError(failure, false, locationId);
            if (childValidator == null) {
                childValidator = new SkipValidator(getNextReceiver());
                childValidator.setValidationContext(getValidationContext());
            }
        }
        if (childValidator != null) {
            childValidator.setConstraintChecker(getConstraintChecker());
        }
    }

    public AttributeMap validateAttributes(AttributeMap attributes, List<NamespaceBinding> additionalNamespaces) throws XPathException {
        return attributes;
    }

    /**
     * A child element was encountered; check that this does not conflict with an xsi:nil=true attribute
     * @param location the location to be used in any error report
     * @throws XPathException  if processing should terminate, for example because of too many errors
     */
    protected void checkNoChildrenWhenNil(Location location) throws XPathException {
        if (nilled) {
            reportContentWhenNil("No child elements are allowed when xsi:nil is true", location);
        }
    }

    /**
     * Character content was encountered; check that this does not conflict with an xsi:nil=true attribute
     * @param location the location to be used in any error report
     * @throws XPathException if processing should terminate, for example because of too many errors
     */
    protected void checkNoCharactersWhenNil(Location location) throws XPathException {
        if (nilled) {
            reportContentWhenNil("No character content is allowed when xsi:nil is true", location);
        }
    }

    /**
     * Report that child elements or character content were encountered when xsi:nil=true
     * @param message the message to be reported
     * @param location the location of the error
     * @throws XPathException if processing should terminate, for example because of too many errors
     */

    private void reportContentWhenNil(String message, Location location) throws XPathException {
        ValidationFailure ve = new ValidationFailure(message);
        ve.setConstraintReference(1, "cvc-elt", "3.2.1");
        ve.setSchemaType(getSchemaType());
        reportValidationError(ve, true, location);
    }


}

// Copyright (c) 2004-2023 Saxonica Limited

