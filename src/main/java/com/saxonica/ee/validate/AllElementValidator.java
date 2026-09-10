package com.saxonica.ee.validate;

import com.saxonica.ee.schema.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.Whitespace;
import net.sf.saxon.z.IntToIntMap;

import java.util.Iterator;
import java.util.List;

/**
 * This class is validator used to check the content of an element whose complex type
 * uses the "xs:all" construction in XML Schema.
 */

public class AllElementValidator extends AttributeValidator {

    // The complex type that we are validating against
    private final UserComplexType type;

    // The AllModelGroup
    private final AllModelGroup allGroup;

    // Boolean to indicate whether mixed content is allowed. If not, all non-whitespace
    // character data is rejected
    private final boolean mixed;

    // Boolean to indicate whether any child elements have been encountered
    private boolean empty = true;

    // Boolean to indicate whether empty content is allowed
    private final boolean emptiable;

    // Maintains a count for each particle that needs to be counted, including wildcards
    // For wildcards, the n'th wildcard uses the n'th counter.
    // For element particles, the counter map associated with the all model group maps the
    // element fingerprint to a position in the counter array
    private final int[] counters;

    // Boolean to indicate whether whitespace in element-only content is to be stripped
    private boolean ignoreIgnorable;

    // Boolean to indicate that we are processing open content in mode="suffix" and that
    // instances of the "regular" content model are therefore no longer accepted.
    private boolean inOpenSuffix;

    /**
     * Create an AllElementValidator
     *
     * @param type the complex type to be validated
     * @param next the next receiver in the pipeline
     * @throws MissingComponentException if the schema is incomplete
     */

    AllElementValidator(UserComplexType type, Receiver next) throws MissingComponentException {
        super(next);
        this.type = type;
        mixed = type.isMixedContent();
        setAnnotation(type);
        setAttributeGroup(type.getCombinedAttributeGroup());
        ModelGroupParticle allParticle = type.getAllCompositor();
        if (allParticle == null) {
            throw new IllegalStateException("Calling xs:all validator, cannot find xs:all compositor");
        }
        allGroup = (AllModelGroup) allParticle.getGroup();
        emptiable = allParticle.isEmptiable();
        counters = new int[allGroup.getMinima().length];
    }

    @Override
    public void setPipelineConfiguration(/*@NotNull*/ PipelineConfiguration pipe) {
        super.setPipelineConfiguration(pipe);
        ignoreIgnorable = getConfiguration().getParseOptions().getSpaceStrippingRule() != NoElementsSpaceStrippingRule.getInstance();
    }

    /**
     * Get the schema type against which we are validating, if any
     *
     * @return the schema type that this validator is validating against, or null
     */

    @Override
    public SchemaType getSchemaType() {
        return type;
    }

    /**
     * Handle the start tag for a child element of the element being validated
     */

    @Override
    public void startElement(NodeName elemName, SchemaType suppliedType,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {

        Configuration config = getConfiguration();
        NamePool pool = config.getNamePool();
        SchemaType annotation = null;
        int key = elemName.obtainFingerprint(pool);

        empty = false;

        IntToIntMap counterMap = allGroup.getCounterMap();
        int counter = counterMap.get(key);
        boolean foundElementParticle = counter != counterMap.getDefaultValue();
        int count;
        int max;
        boolean exhausted = !foundElementParticle;
        if (foundElementParticle) {
            count = ++counters[counter];
            max = allGroup.getMaxima()[counter];
            exhausted = max != -1 && count > max;
        }
        if (exhausted) {
            // child element doesn't match any element particle: try a wildcard
            List<ElementWildcard> wildcards = allGroup.getWildcards();
            int w = 0;
            for (ElementWildcard wildcard : wildcards) {
                if (wildcard.getWildcard().matches(elemName, true, config, type)) {
                    if (inOpenSuffix) {
                        reportNotAllowedInSuffix(elemName, location);
                    }
                    int found = ++counters[w];
                    int wmax = wildcard.getMaxOccurs();
                    if (wmax != -1 && found > wmax) {
                        reportExcess(elemName, location, wmax);
                        counters[w] = Integer.MIN_VALUE;
                    }
                    annotation = processWildcardTerm(wildcard.getWildcard(), elemName, location);
                    break;
                }
                w++;
            }
            if (annotation == null) {
                // No matching wildcard found; try open content
                Wildcard openWildcard = type.getOpenContentWildcard();
                if (openWildcard != null) {
                    if (openWildcard.matches(elemName, true, config, type)) {
                        annotation = processWildcardTerm(openWildcard, elemName, location);
                        inOpenSuffix = type.getOpenContentMode() == OpenContentVariety.SUFFIX;
                    }
                }
            }
            if (annotation == null) {
                if (foundElementParticle) {
                    reportExcess(elemName, location, allGroup.getMaxima()[counter]);
                    counters[counter] = Integer.MIN_VALUE;  // to prevent multiple errors
                } else {
                    String reason = "";
                    if (wildcards.size() == 1) {
                        Iterator<ElementWildcard> it = wildcards.iterator();
                        ElementWildcard wildcard = it.next();
                        reason = ". " + wildcard.getWildcard().reasonForNonMatch(elemName, true, config, type);
                    }
                    reportNotAllowed(elemName, location, reason);
                }
                childValidator = new AnyTypeValidator(getNextReceiver());
                annotation = AnyType.getInstance();
            }
        } else {
            // found a matching element particle
            if (inOpenSuffix) {
                reportNotAllowedInSuffix(elemName, location);
            }
            ElementDecl contextDeterminedDeclaration = (ElementDecl) allGroup.getDeclarationMap().get(key);
            makeChildValidator(contextDeterminedDeclaration, elemName.getStructuredQName(), location, Validation.DEFAULT);
//            try {
//                childValidator = makeValidator(contextDeterminedDeclaration, nameCode, locationId,
//                        getValidationContext(), getXSIType(), Validation.DEFAULT, getPipelineConfiguration(), getUnderlyingReceiver());
//                childValidator.setContainingElement(nameCode & 0xfffff, locationId);
//                childValidator.setConstraintChecker(getConstraintChecker());
//            } catch (ValidationException err) {
//                reportValidationError(err, true, locationId, 2);
//                childValidator = new AnyTypeValidator(getUnderlyingReceiver());
//            }
            annotation = childValidator.getAnnotation();
        }
        super.startElement(elemName, annotation, attributes, namespaces, location, properties);
    }

    private void reportExcess(NodeName elemName, Location locationId, int maxAllowed) throws XPathException {
        ValidationFailure ve = new ValidationFailure(
                "Element " + Err.wrap(elemName.getDisplayName(), Err.ELEMENT) +
                        " exceeds limits defined in xs:all group: maxOccurs is " + maxAllowed);
        ve.setConstraintReference(1, "cvc-model-group", "1");
        ve.setSchemaType(type);
        reportValidationError(ve, true, locationId);
    }

    private void reportNotAllowed(NodeName elemName, Location locationId, String reason) throws XPathException {
        ValidationFailure ve = new ValidationFailure(
                "Element " + Err.wrap(elemName.getDisplayName(), Err.ELEMENT) +
                        " is not allowed in the content of the xs:all group" + reason);
        ve.setConstraintReference(1, "cvc-complex-content", "1");
        ve.setSchemaType(type);
        reportValidationError(ve, true, locationId);
    }

    private void reportNotAllowedInSuffix(NodeName elemName, Location locationId) throws XPathException {
        ValidationFailure ve = new ValidationFailure (
                "Element " + Err.wrap(elemName.getDisplayName(), Err.ELEMENT) +
                        " cannot appear in the content of the xs:all group after an element that matches open content with mode=suffix");
        ve.setConstraintReference(1, "cvc-complex-content", "2");
        ve.setSchemaType(type);
        reportValidationError(ve, true, locationId);
    }

    /**
     * Get a validator to handle the children of the current element. This
     * is called immediately on return from the startElement call that starts
     * this element.
     */

    @Override
    protected ContentValidator getChildValidator() {
        return childValidator;
    }

    /**
     * Handle character data appearing as a child of the element being validated
     * @param chars      The character content
     * @param locationId the location of the node in the source, or of the instruction that created it
     * @param properties Additional properties  @throws XPathException If this element does not allow character data
     */

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        checkNoCharactersWhenNil(locationId);
        if (mixed) {
            nextReceiver.characters(chars, locationId, properties);
            empty = false;
        } else if (Whitespace.isAllWhite(chars)) {
            if (!ignoreIgnorable) {
                nextReceiver.characters(chars, locationId, properties);
            }
        } else {
            ValidationFailure ve = new ValidationFailure("The content model for the type " +
                    type.getDescription() + " does not allow character content");
            ve.setConstraintReference(1, "cvc-complex-type", "2.3");
            ve.setSchemaType(type);
            reportValidationError(ve, true, locationId);
        }
    }

    /**
     * Validation at the end of the element being validated.
     *
     * @throws XPathException if the element is invalid
     */

    @Override
    public void endElement() throws XPathException {
        if ((empty && emptiable) || isNilled()) {
            // do nothing, all is well
        } else {
            List<ElementWildcard> wildcards = allGroup.getWildcards();
            int[] minima = allGroup.getMinima();
            for (int i = wildcards.size(); i < counters.length; i++) {
                int found = counters[i];
                if (found < minima[i] && found >= 0) { // negative means too many, already reported
                    String name = allGroup.getNameCodes()[i].getDisplayName();
                    String container;
                    if (getContainingElement() == null) {
                        if (type.isAnonymousType()) {
                            StructuredQName nc = type.getContainingDeclarationName();
                            container = "element " + nc.getDisplayName();
                        } else {
                            container = "element of type " + type.getDescription();
                        }
                    } else {
                        container = getContainingElementName();
                    }
                    String msg = "In content of " +
                            container + ": " +
                            "element " + Err.wrap(name, Err.ELEMENT) +
                            (found == 0 ? " is not present" :
                                    " has " + found + " occurrence" + (found == 1 ? "" : "s") +
                                            " but minOccurs is " + allGroup.getMinima()[i]);
                    ValidationFailure ve = new ValidationFailure(msg);
                    ve.setConstraintReference(1, "cvc-model-group", "3");
                    ve.setSchemaType(type);
                    reportValidationError(ve, true, getContainingElementLocationId());
                }
            }
            //  check minOccurs() rules for Element Wildcards are satisfied
            int index = 0;
            for (ElementWildcard wildcard : wildcards) {
                int found = counters[index++];
                int min = wildcard.getMinOccurs();
                if (found < min && found >= 0) {
                    String msg = "In content of " +
                            Err.wrap(getContainingElement().getDisplayName(), Err.ELEMENT) + ": " +
                            (found == 0 ? "No" : "Only " + found) +
                            " child element" + (found == 1 ? "" : "s") +
                            " found to match element wildcard with minOccurs=" + min;
                    ValidationFailure ve = new ValidationFailure(msg);
                    ve.setConstraintReference(1, "cvc-model-group", "3");
                    ve.setSchemaType(type);
                    reportValidationError(ve, true, getContainingElementLocationId());
                }
            }
        }
        nextReceiver.endElement();
    }
}

// Copyright (c) 2004-2022 Saxonica Limited
