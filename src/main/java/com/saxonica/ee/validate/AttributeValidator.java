package com.saxonica.ee.validate;

import com.saxonica.ee.schema.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.AttributeLocation;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Whitespace;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntIterator;
import net.sf.saxon.z.IntToIntMap;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;


/**
 * This class is an abstract superclass for all the receivers that do element content
 * validation. Since all kinds of element content can be associated with attributes,
 * the attribute validation is handled at this level.
 */


public abstract class AttributeValidator extends ContentValidator {

    private IntHashMap<AttributeUse> declaredAttributes;
    private IntToIntMap attributeCounterMap;
    private boolean[] found;
    private AttributeWildcard attributeWildcard = null;
    private int idAttributes = 0;

    // The schema type used to validate the most recent attribute of this element, if any
    SimpleType latestAttributeType = null;

    // The validateAttributes method is called twice when assertions are present. So
    // we remember the previous result (bug 5737)

    private AttributeMap previousAttributeMap;
    private AttributeMap previousValidatedAttributes;
    private List<NamespaceBinding> previousNamespaceBindings;

    /**
     * Create an AttributeValidator
     *
     * @param next the next receiver in the pipeline
     */

    public AttributeValidator(Receiver next) {
        super(next);
    }

    /**
     * Identify the set of attributes permitted by the complex type being validated. This is called
     * during initialization of the AttributeValidator. It is not called if the type is a simple type;
     * in such cases any attribute is an error, except for the xsi: attributes permitted on all elements.
     *
     * @param attributeGroup the set of attributes declared for this complex type
     * @throws MissingComponentException if an unresolved reference is found
     */

    public void setAttributeGroup(AttributeGroupDecl attributeGroup) {
        if (attributeGroup != null) {
            attributeWildcard = attributeGroup.getAttributeWildcard();
            declaredAttributes = attributeGroup.getDeclaredAttributes();
            attributeCounterMap = attributeGroup.getCounterMap();
            found = new boolean[attributeCounterMap.size()];
        }
    }


    /**
     * Handle the attributes
     *
     * @param in                   the supplied attribute collection
     * @param additionalNamespaces a mutable list to which additional namespace bindings can be added
     *                             in the event that validation of attributes requires new namespaces
     *                             (to support fixed or default values of QName-valued attributes)
     * @return the validated attribute collection
     */

    @Override
    @SuppressWarnings({"ThrowableResultOfMethodCallIgnored"})
    public AttributeMap validateAttributes(AttributeMap in, List<NamespaceBinding> additionalNamespaces) throws XPathException {

        if (in == previousAttributeMap) {
            additionalNamespaces.addAll(previousNamespaceBindings);
            return previousValidatedAttributes;
        }
        PipelineConfiguration pipe = getPipelineConfiguration();
        AttributeMap result = EmptyAttributeMap.getInstance();

        for (AttributeInfo att : in) {
            //System.err.println("AttValidator " + this + "attribute " + attName.getDisplayName());
            NodeName attName = att.getNodeName();
            String value = att.getValue();
            Location location = att.getLocation();
            SimpleType type = att.getType();
            int properties = att.getProperties();
            int fingerprint = att.getNodeName().obtainFingerprint(getNamePool());

            if (fingerprint == StandardNames.XSI_NIL) {
                if (getNillability() == Nillability.DISALLOWED) {
                    reportNotNillable(attName, location);
                }
                // TODO: following code is probably redundant; we've already made the checks in XSIAttributeHandler
                String b = Whitespace.trim(value);
                if (getNillability() != Nillability.IGNORED && (b.equals("true") || b.equals("1"))) {
                    nilled = true;
                }
            }
            SimpleType annotation = type;

            AttributeUse use = declaredAttributes == null ? null : declaredAttributes.get(fingerprint);
            AttributeDecl decl = null;
            if (use != null && !use.isProhibited()) {
                decl = (AttributeDecl) use.getTarget();
                int counter = attributeCounterMap.get(fingerprint);
                if (counter != attributeCounterMap.getDefaultValue()) {
                    found[counter] = true;
                }
            } else {
                // There is no declaration of this attribute.
                // Certain XSI attributes are always allowed...
                boolean implicitlyAllowed = false;
                NamespaceUri uri = attName.getNamespaceUri();
                if (uri.equals(NamespaceUri.SCHEMA_INSTANCE) &&
                        (fingerprint == StandardNames.XSI_TYPE ||
                                 fingerprint == StandardNames.XSI_NIL ||
                                 fingerprint == StandardNames.XSI_SCHEMA_LOCATION ||
                                 fingerprint == StandardNames.XSI_NO_NAMESPACE_SCHEMA_LOCATION)) {
                    implicitlyAllowed = true;
                }
                if (!implicitlyAllowed) {
                    if (attributeWildcard != null) {
                        // See if the attribute matches a wildcard
                        PreparedSchema schema = getValidationContext().getSchema();
                        if (!attributeWildcard.getWildcard().matches(attName, false, schema, null)) {
                            attributeDoesNotMatchWildcard(attName, schema, location);
                        }
                        String process = attributeWildcard.getWildcard().getProcessContents();
                        if (process.equals("skip")) {
                            // do nothing
                        } else {
                            StructuredQName attributeName = attName.getStructuredQName();
                            //noinspection RedundantCast
                            decl = (AttributeDecl)getValidationContext().getSchema().getAttributeDecl(attributeName);
                            if (decl == null && process.equals("strict")) {
                                noAttributeDeclarationAvailable(attName, location);
                            }
                        }
                    } else {
                        attributeIsNotAllowed(attName, location);
                    }
                }
            }
            // We have found an attribute declaration (either from the declared attributes for this type,
            // or a global attribute declaration matching a wildcard with process = strict
            if (decl != null) {
                SimpleType declaredType = decl.getSimpleType();
                latestAttributeType = declaredType;
                if (declaredType.getBuiltInBaseType().getFingerprint() == StandardNames.XS_ID) {
                    idAttributes++;
                    if (idAttributes == 2 && getConfiguration().getXsdVersion() == Configuration.XSD10) {
                        // we can't always detect this as a schema validity error, because multiple attributes
                        // may be IDs as a result of wildcard matching
                        multipleIdAttributes(attName, location);
                    }
                }
                ConversionRules rules = getValidationContext();
                if (declaredType.getBuiltInBaseType().getFingerprint() == StandardNames.XS_NOTATION) {
                    if (declaredType.getFingerprint() == StandardNames.XS_NOTATION) {
                        notationIsAbstract(location);
                    }
                }
                ValidationFailure err = declaredType.validateContent(
                        StringView.of(value).tidy(), getNamespaceResolver(), rules);
                if (err != null) {
                    err.setConstraintReference(2, "cvc-complex-type", "3");
                    Location attLocation = makeAttributeLocation(attName, location);
                    reportValidationError(err, false, attLocation);
                } else {
                    // if the attribute declaration has an enumeration facet, economize on storage by
                    // passing on the reference to the value already held in the schema.
                    if (declaredType instanceof UserAtomicType) {
                        value = ((UserAtomicType) declaredType).getSharedInstance(value);
                    }

                    annotation = declaredType;

                    // Check the value if it is fixed

                    boolean matchesFixed = true;
                    if (use != null && use.getFixedValueConstraint() != null) {
                        matchesFixed = use.getFixedValueConstraint().testFixedValue(
                                getConfiguration(), StringView.of(value).tidy(), declaredType, getNamespaceResolver());
                    }
                    if (decl.getFixedValueConstraint() != null) {
                        matchesFixed = matchesFixed && decl.getFixedValueConstraint().testFixedValue(
                                getConfiguration(), StringView.of(value).tidy(), declaredType, getNamespaceResolver());
                    }

                    if (!matchesFixed) {
                        mismatchWithFixedValue(attName, value, location, use, decl);
                    }
                }
            }

            if ((use != null && use.isInheritable()) || use == null && decl != null && decl.isInheritable()) {
                getAttributeInheritor().notifyInheritableAttribute(attName, value, location, properties);
            }

            result = result.put(new AttributeInfo(attName, annotation, value, location, properties));
            //nextReceiver.attribute(attName, annotation, value, location, properties);
        }

        if (declaredAttributes != null) {
            IntIterator iter2 = declaredAttributes.keyIterator();
            while (iter2.hasNext()) {
                int key = iter2.next();
                int counter = attributeCounterMap.get(key);
                if (!found[counter]) {
                    AttributeUse use = declaredAttributes.get(key);
                    if (use.isRequired()) {
                        String name = use.getTargetComponentName().getEQName();
                        String message = "Required attribute " + Err.wrap(name, Err.ATTRIBUTE) +
                                " is missing on " + getContainingElementName();
                        ValidationFailure ve = new ValidationFailure(message);
                        ve.setConstraintReference(1, "cvc-complex-type", "4");
                        ve.setSchemaType(getSchemaType());
                        reportValidationError(ve, true, getContainingElementLocationId());
                    } else if (pipe.getParseOptions().isExpandAttributeDefaults()) {
                        // see if there is a default or fixed value
                        UnicodeString value = use.getUnderlyingDefaultValue();
                        if (value == null) {
                            AtomicSequence fixed = use.getUnderlyingFixedValue();
                            value = fixed == null ? null : fixed.getUnicodeStringValue();
                        }
                        if (value != null) {
                            if (use.getAttributeDeclaration().getSimpleType().isNamespaceSensitive()) {
                                // If the default value is namespace-sensitive, we need to validate it
                                checkNamespaceSensitiveDefaultValue(use, value);
                            }
                            // If the attribute is in a namespace, we need to ensure that the name has
                            // a prefix and that the prefix is declared
                            NamePool pool = getNamePool();
                            NamespaceUri uri = use.getTargetComponentName().getNamespaceUri();
                            String prefix = null;
                            if (uri.isEmpty()) {
                                prefix = "";
                            } else {
                                NamespaceResolver resolver = getNamespaceResolver();
                                // See if there is a prefix bound to the namespace
                                Iterator<String> iter = resolver.iteratePrefixes();
                                while (iter.hasNext()) {
                                    String p = iter.next();
                                    if (!p.isEmpty() && uri.equals(resolver.getURIForPrefix(p, false))) {
                                        prefix = p;
                                        break;
                                    }
                                }
                                if (prefix == null) {
                                    prefix = use.getTargetComponentName().getPrefix();
                                    if (prefix.isEmpty()) {
                                        prefix = pool.suggestPrefixForURI(uri);
                                    }
                                    if (prefix == null) {
                                        prefix = "ns";
                                    }
                                    while (true) {
                                        NamespaceUri u = getNamespaceResolver().getURIForPrefix(prefix, false);
                                        if (u == null || u.equals(uri)) {
                                            break;
                                        }
                                        prefix = prefix + "1";
                                    }
                                    additionalNamespaces.add(new NamespaceBinding(prefix, uri));
                                }
                            }
                            result = result.put(new AttributeInfo(new CodedName(key, prefix, getNamePool()),
                                                use.getAttributeDeclaration().getSimpleType(),
                                                value.toString(),
                                                Loc.NONE,
                                                ReceiverOption.DEFAULTED_VALUE));
                        }
                    }
                }
            }
        }

        previousAttributeMap = in;
        previousValidatedAttributes = result;
        previousNamespaceBindings = additionalNamespaces;

        return result;
    }

    private void checkNamespaceSensitiveDefaultValue(AttributeUse use, UnicodeString value) throws XPathException {
        ValidationFailure err =
                use.getAttributeDeclaration().getSimpleType().validateContent(
                        value,
                        getNamespaceResolver(),
                        getConfiguration().getConversionRules());
        if (err != null) {
            String message = "The namespace-sensitive default value is invalid in the " +
                    "namespace context where it is being used";
            ValidationFailure ve = new ValidationFailure(message);
            ve.setSchemaType(getSchemaType());
            // Note: Schema 1.0 says nothing about this case. 1.1 leans towards performing
            // namespace fixup, that is, generating a namespace declaration to make the
            // default value valid. This implies that the default is to be treated as a typed
            // value, not as a string. See 3.4.5.1 Attribute Default Value
            reportValidationError(ve, true, getContainingElementLocationId());
        }
    }

    private void mismatchWithFixedValue(NodeName attName, String value, Location location, AttributeUse use, AttributeDecl decl) throws XPathException {
        AtomicSequence fixed;
        if (use != null) {
            fixed = use.getUnderlyingFixedValue();
        } else {
            fixed = decl.getFixedValue();
        }
        String message = "The actual value " + Err.wrap(value, Err.VALUE) +
                " of attribute " + Err.wrap(attName.getDisplayName(), Err.ATTRIBUTE) +
                " does not match the required fixed value " +
                (fixed instanceof AtomicValue ? Err.wrap(fixed.getUnicodeStringValue(), Err.VALUE) : "");
        ValidationFailure ve = new ValidationFailure(message);
        ve.setConstraintReference(1, "cvc-au", "0");
        ve.setSchemaType(getSchemaType());
        reportValidationError(ve, false, makeAttributeLocation(attName, location));
    }

    private void notationIsAbstract(Location location) throws XPathException {
        String message = "Cannot validate against type xs:NOTATION (only against a subtype defining an enumeration of values)";
        ValidationFailure ve = new ValidationFailure(message);
        ve.setConstraintReference(1, "enumeration-required-notation", "0");
        ve.setSchemaType(getSchemaType());
        reportValidationError(ve, false, location);
    }

    private void multipleIdAttributes(NodeName attName, Location location) throws XPathException {
        String message = "Element has more than one ID attribute";
        ValidationFailure ve = new ValidationFailure(message);
        ve.setConstraintReference(1, "cvc-complex-type", "5");
        ve.setSchemaType(getSchemaType());
        reportValidationError(ve, false, makeAttributeLocation(attName, location));
        // Note: in XSD 1.0, cvc-complex-type rule 5 also says that if the type includes a declared
        // attribute of type ID, and this attribute is omitted in the instance, then the element must
        // not have an ID attribute supplied by means of a wildcard.
    }

    private void attributeIsNotAllowed(NodeName attName, Location location) throws XPathException {
        String possibleNamespaceMessage = "";
        if (declaredAttributes != null) {
            for (Iterator<AttributeUse> ii = declaredAttributes.valueIterator(); ii.hasNext(); ) {
                AttributeUse au = ii.next();
                if (au.getTargetComponentName().getLocalPart().equals(attName.getLocalPart())) {
                    NamespaceUri ns = au.getTargetComponentName().getNamespaceUri();
                    if (ns.isEmpty()) {
                        possibleNamespaceMessage = " (it would be allowed in no namespace)";
                    } else {
                        possibleNamespaceMessage = " (it would be allowed in namespace " + ns + ")";
                    }
                    break;
                }
            }
        }
        String message = "Attribute " + Err.wrap(attName.getDisplayName(), Err.ATTRIBUTE) +
                " is not allowed on " + getContainingElementName() + possibleNamespaceMessage;
        ValidationFailure ve = new ValidationFailure(message);
        ve.setConstraintReference(1, "cvc-complex-type", "3");
        ve.setSchemaType(getSchemaType());
        reportValidationError(ve, true, makeAttributeLocation(attName, location));
    }

    private void reportNotNillable(NodeName attName, Location location) throws XPathException {
        String message = "The element is not nillable, so xsi:nil must not be present";
        ValidationFailure ve = new ValidationFailure(message);
        ve.setConstraintReference(1, "cvc-elt", "3.1");
        ve.setSchemaType(getSchemaType());
        reportValidationError(ve, true, makeAttributeLocation(attName, location));
    }

    private void noAttributeDeclarationAvailable(NodeName attName, Location location) throws XPathException {
        String message = "The <xs:anyAttribute> that matches attribute " +
                Err.wrap(attName.getDisplayName(), Err.ATTRIBUTE) +
                " requires strict validation, but no attribute declaration is available";
        ValidationFailure ve = new ValidationFailure(message);
        ve.setConstraintReference(1, "cvc-wildcard", "2");
        ve.setSchemaType(getSchemaType());
        reportValidationError(ve, false, makeAttributeLocation(attName, location));
    }

    private void attributeDoesNotMatchWildcard(NodeName attName, PreparedSchema schema, Location location) throws XPathException {
        String reason = attributeWildcard.getWildcard().reasonForNonMatch(
                attName, false, schema, null);
        String message = "Attribute " +
                Err.wrap(attName.getDisplayName(), Err.ATTRIBUTE) +
                " is not allowed on " + getContainingElementName() + ". " + reason;
        ValidationFailure ve = new ValidationFailure(message);
        ve.setConstraintReference(1, "cvc-complex-type", "3.2.1");
        ve.setSchemaType(getSchemaType());
        reportValidationError(ve, false, makeAttributeLocation(attName, location));
    }

    private AttributeLocation makeAttributeLocation(NodeName attName, Location location) {
        return new AttributeLocation(getContainingElement(), attName.getStructuredQName(), location);
    }

    /**
     * Get the SimpleType used for validating the most recent attribute, if any
     *
     * @return the SimpleType of the most recent attribute
     */

    /*@Nullable*/
    public SimpleType getMostRecentAttributeType() {
        return latestAttributeType;
    }

    /**
     * Handle the start tag for a child element of the element being validated
     */

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {

        idAttributes = 0;
        checkNoChildrenWhenNil(location);

        if (childValidator instanceof SkipValidator) {
            properties |= ReceiverOption.SKIP_VALIDATION;
        }
        super.startElement(elemName, type, attributes, namespaces, location, properties);

        if (locallyInvalid && pendingMessages != null) {
            for (String pendingMessage : pendingMessages) {
                nextReceiver.comment(StringView.of(pendingMessage), Loc.NONE, ReceiverOption.NONE);
            }
        }
    }


    /**
     * Handle character data appearing as a child of the element being validated
     * @param chars      The character content
     * @param locationId the location of the node in the source, or of the instruction that created it
     * @param properties Additional properties  @throws net.sf.saxon.trans.XPathException
     */

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        checkNoCharactersWhenNil(locationId);
        nextReceiver.characters(chars, locationId, properties);
    }

    /**
     * Process a wildcard term found in complex content or "all" content groups.
     * This method is called when the validator is processing a child element of the relevant group
     * and has established that it matches a wildcard term. The purpose of the method is (a) to
     * determine whether the "processContents=strict" condition is satisfied (that is, there must
     * be a matching global element declaration), and (b) to allocate a childValidator for the
     * child element.
     *
     * @param card        the Wildcard that has been matched
     * @param elementName the name of the child element
     * @param locationId  the location of the child element
     * @return the type annotation to be used for the child element
     * @throws net.sf.saxon.trans.XPathException if the content is invalid
     */

    protected SchemaType processWildcardTerm(Wildcard card, NodeName elementName, Location locationId) throws XPathException {
        SchemaType annotation;
        StructuredQName elementQName = elementName.getStructuredQName();
        int fingerprint = elementName.getFingerprint();
        Configuration config = getConfiguration();
        PipelineConfiguration pipe = getPipelineConfiguration();
        String mode = card.getProcessContents();
        SchemaType xsiType = getValidationContext().getXSIType();
        switch (mode) {
            case "strict": {
                @SuppressWarnings("RedundantCast")
                ElementDecl decl = (ElementDecl)getValidationContext().getSchema().getElementDecl(fingerprint);
                if (decl == null) {
                    if (xsiType == null) {
                        String message = "No element declaration found for element " +
                                Err.wrap(elementName.getDisplayName(), Err.ELEMENT);
                        if (getContainingElement() != null) {
                            message = "In content of " + getContainingElementName() + ": " + message;
                        }
                        ValidationFailure ve = new ValidationFailure(message);
                        ve.setConstraintReference(1, "cvc-wildcard", "2");
                        ve.setSchemaType(getSchemaType());
                        reportValidationError(ve, true, locationId);
                        annotation = AnyType.getInstance();
                        return annotation;
                    } else {
                        makeChildValidator(null, elementQName, locationId, Validation.DEFAULT);
                    }
                } else {
                    makeChildValidator(decl, elementQName, locationId, Validation.DEFAULT);
                }
                childValidator.setContainingElement(elementName.getStructuredQName(), locationId);
                annotation = childValidator.getAnnotation();

                break;
            }
            case "lax": {
                @SuppressWarnings("RedundantCast")
                ElementDecl decl = (ElementDecl)getValidationContext().getSchema().getElementDecl(fingerprint);
                if (decl == null) {
                    if (xsiType != null) {
                        List<ValidationFailure> failures = new ArrayList<>(1);
                        childValidator = makeValidator(null, elementQName, locationId,
                                                       getValidationContext(), xsiType, Validation.DEFAULT, pipe, getNextReceiver(), failures);
                        if (!failures.isEmpty()) {
                            reportValidationError(failures.get(0), true, locationId);
                            childValidator = new SkipValidator(getNextReceiver());
                            annotation = AnyType.getInstance();
                        } else {
                            annotation = childValidator.getAnnotation();
                        }
                    } else {
                        childValidator = new LaxValidator(getNextReceiver());
                        childValidator.setContainingElement(elementQName, locationId);
                        annotation = AnyType.getInstance();
                    }
                } else {
                    makeChildValidator(decl, elementQName, locationId, Validation.DEFAULT);
                    childValidator.setContainingElement(elementQName, locationId);
                    annotation = childValidator.getAnnotation();
                }
                break;
            }
            default:
                // SKIP validation
                annotation = AnyType.getInstance();
                childValidator = new SkipValidator(getNextReceiver());
                childValidator.setValidationContext(getValidationContext());
                break;
        }

        //childValidator.setConstraintChecker(getConstraintChecker());

        if (!(childValidator instanceof SkipValidator)) {
            // In XSD 1.1, the governing type declaration must be a subtype of the context-determined type
            // See rule 5 of Element Locally Valid (Complex Type)
            UserComplexType containingType = (UserComplexType) getSchemaType();
            CompiledComplexType compiledContainingType =
                    getValidationContext().getSchema().getCompiledComplexType(containingType);
            if (getValidationContext().getSchema().getXsdVersion() != Configuration.XSD10) {
                SchemaType contextDeterminedType = compiledContainingType.getContextDeterminedTypeForElement(elementQName);
                SchemaType governingType = childValidator.getSchemaType();
                if (contextDeterminedType != null && governingType != null) {
                    try {
                        config.checkTypeDerivationIsOK(governingType, contextDeterminedType, 0);
                    } catch (SchemaException err) {
                        String message = "The governing type (" + governingType.getDescription() +
                                ") of the " + Err.wrap(elementName.getDisplayName(), Err.ELEMENT) +
                                " element which matches against a wildcard is not substitutable for the " +
                                "context-determined type of the element particle named " +
                                elementName.getDisplayName() +
                                " within the content model of type " + containingType.getDescription() + ". " +
                                err.getMessage();
                        ValidationFailure verr = new ValidationFailure(message);
                        verr.setConstraintReference(1, "cvc-complex-type", "5");
                        verr.setSchemaType(getSchemaType());
                        reportValidationError(verr, true, locationId);
                    }
                }
            }
        }
        return annotation;
    }

}

// Copyright (c) 2004-2023 Saxonica Limited No warranty.
