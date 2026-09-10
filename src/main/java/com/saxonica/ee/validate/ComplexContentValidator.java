package com.saxonica.ee.validate;

import com.saxonica.ee.schema.*;
import com.saxonica.ee.schema.fsa.*;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.AtomicSequence;
import net.sf.saxon.om.NoElementsSpaceStrippingRule;
import net.sf.saxon.om.NodeName;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.FastStringBuffer;
import net.sf.saxon.type.*;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.Whitespace;

import java.util.*;

/**
 * This class is a filter that is used to validate the content of an element with a
 * complex type. As each child element is encountered, it is used to compute a transition
 * in the finite state machine compiled for the given complex type.
 */

public class ComplexContentValidator extends AttributeValidator {

    // The complex type that we are validating against
    UserComplexType type;

    // Boolean to indicate whether mixed content is allowed. If not, all non-whitespace
    // character data is rejected
    boolean mixed;

    // The fixed value, if any, converted to a string
    String fixedValue = null;

    // The default value, if any
    String defaultValue = null;

    // The current state in the finite state machine
    State state;

    // The previous element encountered
    int previousElement = -1;

    // Boolean to indicate whether whitespace in element-only content is to be stripped
    boolean ignoreIgnorable;

    // Buffer for accumulating character content
    FastStringBuffer buffer;

    // Boolean to indicate whether any child elements have been found
    boolean foundChildren = false;


    /**
     * Create a ComplexContentValidator for a given type
     *
     * @param declaration the element declaration, if available, or null otherwise.
     * @param type        the complex type
     * @param next        the next receiver in the pipeline
     * @throws MissingComponentException if the schema is incomplete or invalid
     */

    public ComplexContentValidator(/*@Nullable*/ ElementDecl declaration, UserComplexType type, Receiver next) throws MissingComponentException {
        super(next);
        setSchemaType(type);
        if (declaration != null) {
            defaultValue = declaration.getDefaultValueLexicalForm();
            AtomicSequence fixed = declaration.getFixedValue();
            if (fixed != null) {
                fixedValue = fixed.getStringValue();
            }
        }
    }

    public void setPipelineConfiguration(/*@NotNull*/ PipelineConfiguration pipe) {
        super.setPipelineConfiguration(pipe);
        ignoreIgnorable = getConfiguration().getParseOptions().getSpaceStrippingRule() != NoElementsSpaceStrippingRule.getInstance();
    }

    /**
     * Set the schema type against which this validator is to validate the instance element
     *
     * @param type the schema type used for validation
     * @throws MissingComponentException if there is a problem with the schema
     */

    public void setSchemaType(UserComplexType type) throws MissingComponentException {
        this.type = type;
        mixed = type.isMixedContent();
        state = type.getInitialState();
        setAnnotation(type);
        setAttributeGroup(type.getCombinedAttributeGroup());
    }

    public SchemaType getSchemaType() {
        return type;
    }

    /**
     * Handle the start tag for a child element of the element being validated
     */

    public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties)
            throws XPathException {
        if (locallyInvalid) {
            // already found errors and warned about them. But because of EDC, we can still try to
            // find a governing type for the element, to tease out more validation errors.
            recover(elemName, location, properties);
            return;
        }

        SchemaType annotation;

        if (fixedValue != null) {
            String message = "Element " +
                    Err.wrap(elemName.getDisplayName(), Err.ELEMENT) +
                    " cannot appear here, because the containing " +
                    getContainingElementName() + " element has fixed content";
            ValidationFailure ve = new ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-elt", "5.2.2.1");
            ve.setSchemaType(type);
            reportValidationError(ve, true, location);
            locallyInvalid = true;
            recover(elemName, location, properties);
            return;
        }

        foundChildren = true;

        final int fingerprint = elemName.obtainFingerprint(getNamePool());
        final Edge edge = state.getTransition(fingerprint, type);
        if (edge == null) {

            FiniteStateMachine machine = type.getFiniteStateMachine();
            Wildcard openContentWildcard = machine.getOpenContentWildcard();
            if (openContentWildcard != null &&
                    openContentWildcard.matches(elemName, true, getConfiguration(), type)) {
                if (machine.isOpenContentInterleaved()) {
                    // interleaved open content: leave the state unchanged
                    childValidator = new LaxValidator(getNextReceiver());
                    annotation = processWildcardTerm(openContentWildcard, elemName, location);
                    super.startElement(elemName, annotation, location, properties);
                    return;
                } else if (state.isFinalState()) {
                    // suffix wildcard: from now on we can only match the open content wildcard
                    state = SuffixState.getInstance();
                    childValidator = new LaxValidator(getNextReceiver());
                    annotation = processWildcardTerm(openContentWildcard, elemName, location);
                    super.startElement(elemName, annotation, location, properties);
                    return;
                }
            }

            String message = supplyReasonForNonMatch(elemName);
            ValidationFailure ve = new ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-complex-type", "2.4");
            ve.setSchemaType(type);
            reportValidationError(ve, true, location);
            locallyInvalid = true;
            recover(elemName, location, properties);
            return;
        }
        try {
            previousElement = fingerprint;
            state = edge.makeTransition(state);
        } catch (ValidationException e) {
            String message = e.getMessage();
            if (getContainingElement() != null) {
                message = "In content of " + getContainingElementName() + ": " + message;
            }
            ValidationFailure ve = e.getValidationFailure();
            ve.setSchemaType(type);
            reportValidationError(ve, true, location);
            locallyInvalid = true;
            recover(elemName, location, properties);
            return;

        }
        final Term term = edge.getTerm();

        if (term instanceof ElementDecl) {
            ElementDecl decl = (ElementDecl) term;
            makeChildValidator(decl, elemName.getStructuredQName(), location, Validation.DEFAULT);
            childValidator.setContainingElement(decl.getComponentName(), location);
            annotation = childValidator.getAnnotation();
        } else {
            // It's a wildcard
            Wildcard card = (Wildcard) term;
            annotation = processWildcardTerm(card, elemName, location);
            if (childValidator == null) {
                childValidator = new LaxValidator(getNextReceiver());
            }
        }
        super.startElement(elemName, annotation, location, properties);
    }

    /**
     * Construct a string to use in error messages containing an explanation of why the new child element
     * is not valid in the current state
     *
     * @param nodeName the name of the new child element, or null if we reached the end of the children prematurely
     * @return a string to use in error messages. The string will contain one or more complete English sentences,
     * starting with a capital letter and ending with a full stop and trailing space.
     */

    private String supplyReasonForNonMatch(NodeName nodeName) {
        int fingerprint = nodeName == null ? -1 : nodeName.obtainFingerprint(getNamePool());
        FastStringBuffer fsb = new FastStringBuffer(FastStringBuffer.C64);

        if (getContainingElement() != null) {
            fsb.append("In content of " + getContainingElementName() + ": ");
        }

        boolean explained = false;
        if (nodeName == null) {
            if (previousElement == -1) {
                fsb.append("Empty content is not allowed. ");
            } else {
                fsb.append("The content is incomplete. ");
            }
        } else {
            if (state instanceof SuffixState) {
                fsb.append("The content model does not allow element " +
                        Err.wrap(nodeName.getStructuredQName().getEQName(), Err.ELEMENT) +
                        " to appear after an element that matches the suffix open content wildcard. ");
                explained = true;
            } else {
                fsb.append("The content model does not allow element ");
                fsb.append(Err.wrap(nodeName.getStructuredQName().getEQName(), Err.ELEMENT));
                if (previousElement == -1) {
                    fsb.append(" to appear as the first child. ");
                } else if (previousElement == fingerprint) {
                    try {
                        if (Cardinality.allowsMany(type.getElementParticleCardinality(fingerprint, false))) {
                            fsb.append(" to appear this many times. ");
                        } else {
                            fsb.append(" to appear more than once. ");
                        }
                    } catch (MissingComponentException e) {
                        fsb.append(" to appear more than once. ");
                    }
                    explained = true;
                } else {
                    fsb.append(" to appear immediately after element ");
                    fsb.append(Err.wrap(getNamePool().getClarkName(previousElement), Err.ELEMENT));
                    fsb.append(". ");
                }

                try {
                    if (!explained && type.getElementParticleType(fingerprint, false) != null) {
                        // The element is allowed by the content model, but not here
                        // See if the error can be attributed to the absence of an element
                        StructuredQName omission = null;
                        HashSet<StructuredQName> possibleOmissions = new HashSet<StructuredQName>();
                        StructuredQName firstOmission = null;
                        for (Iterator<Edge> edges = state.getEdges(); edges.hasNext(); ) {
                            Edge e = edges.next();
                            if (e.getParticle() instanceof ElementParticle) {
                                omission = e.getParticle().getTargetComponentName();
                                int omittedfp = ((ElementParticle) e.getParticle()).getDeclaration().getFingerprint();
                                AutomatonState s = state.getTransition(omittedfp, type).getTargetState();
                                if (s.getTransition(fingerprint, type) != null) {
                                    possibleOmissions.add(omission);
                                    if (firstOmission == null) {
                                        firstOmission = omission;
                                    }
                                }
                            }
                        }
                        if (!possibleOmissions.isEmpty()) {
                            if (possibleOmissions.size() == 1) {
                                fsb.append("It must be preceded by " + Err.wrap(firstOmission.getEQName(), Err.ELEMENT) + ". ");
                            } else if (possibleOmissions.size() > 5) {
                                fsb.append("It must be preceded by another element, for example " + Err.wrap(firstOmission.getEQName(), Err.ELEMENT) + ". ");
                            } else {
                                fsb.append("It must be preceded by one of: ");
                                boolean first = true;
                                for (StructuredQName poss : possibleOmissions) {
                                    if (!first) {
                                        fsb.append(", ");
                                    }
                                    if (poss.getURI().equals(nodeName.getURI())) {
                                        fsb.append(Err.wrap(poss.getLocalPart(), Err.ELEMENT));
                                    } else {
                                        fsb.append(Err.wrap(poss.getEQName(), Err.ELEMENT));
                                    }
                                    first = false;
                                }
                                fsb.append(". ");
                            }
                            explained = true;
                        }
                    }
                } catch (MissingComponentException e) {
                    // no action
                }
                if (!explained) {
                    // See if there is an element with the same local name that would be OK here
                    for (Iterator<Edge> edges = state.getEdges(); edges.hasNext(); ) {
                        String foundLocal = nodeName.getLocalPart();
                        String foundURI = nodeName.getURI();
                        Edge e = edges.next();
                        if (e.getParticle() instanceof ElementParticle) {
                            String local = ((ElementParticle) e.getParticle()).getName();
                            if (local.equals(foundLocal)) {
                                String uri = ((ElementParticle) e.getParticle()).getNamespaceURI();
                                fsb.append("The element is in " +
                                        ("".equals(foundURI) ? "no namespace" : "namespace " + foundURI) +
                                        " but it should be in " +
                                        ("".equals(uri) ? "no namespace" : "namespace " + uri) + ". ");
                                explained = true;
                                break;
                            }
                        }
                    }
                }
                if (!explained) {
                    Wildcard openContentWildcard = type.getOpenContentWildcard();
                    if (openContentWildcard != null) {
                        String s = openContentWildcard.reasonForNonMatch(nodeName, true, getConfiguration(), type);
                        if (s != null) {
                            fsb.append("It does not match the open content wildcard: ");
                            fsb.append(s);
                        }
                    }
                }
            }
        }

        if (!explained) {
            Set<String> specificTransitions = new HashSet<String>();
            StructuredQName expected = null;
            Set<String> specificURIs = new HashSet<String>();
            String namespace = null;
            for (Iterator<Edge> ii = state.getEdges(); ii.hasNext(); ) {
                Edge e = ii.next();
                if (e.getParticle() instanceof ElementParticle) {
                    expected = e.getParticle().getTargetComponentName();
                    specificTransitions.add(((ElementParticle) e.getParticle()).getDisplayName());
                    namespace = ((ElementParticle) e.getParticle()).getNamespaceURI();
                    specificURIs.add(namespace);
                }
            }
            Set<Wildcard> wildcards = new HashSet<Wildcard>();
            for (Iterator<Edge> ii = state.getEdges(); ii.hasNext(); ) {
                Edge e = ii.next();
                if (e.getParticle() instanceof ElementWildcard) {
                    wildcards.add(((ElementWildcard) e.getParticle()).getWildcard());
                }
            }

            if (specificTransitions.isEmpty() && wildcards.isEmpty()) {
                if (previousElement == -1) {
                    fsb.append("In fact, nothing is allowed: the type has no valid instances. ");
                } else {
                    fsb.append("No further elements are allowed at this point. ");
                }
            } else {
                if (!specificTransitions.isEmpty()) {
                    if (specificTransitions.size() == 1) {
                        fsb.append("Expected ");
                        fsb.append(Err.wrap(expected.getEQName(), Err.ELEMENT));
                        if (state.isFinalState()) {
                            fsb.append(" or nothing");
                        }
                        fsb.append(". ");
                    } else {
                        boolean uniform = specificURIs.size() == 1;
                        fsb.append("The following elements would be valid here");
                        if (uniform) {
                            fsb.append(", all in ");
                            fsb.append("".equals(namespace) ? "no namespace" : "namespace " + namespace);
                        }
                        fsb.append(": ");
                        int i = 0;
                        for (String s : specificTransitions) {
                            if (i >= 10) {
                                fsb.append(", and " + (specificTransitions.size() - 10) + " others");
                                break;
                            }
                            if (i++ > 0) {
                                fsb.append(", ");
                            }
                            if (uniform) {
                                int c = s.indexOf(':');
                                if (c < 0) {
                                    fsb.append(s);
                                } else {
                                    fsb.append(s.substring(c + 1));
                                }
                            } else {
                                fsb.append(s);
                            }
                        }
                        if (state.isFinalState()) {
                            fsb.append(" (or nothing)");
                        }
                        fsb.append(". ");
                    }
                }
                if (nodeName == null) {
                    if (!wildcards.isEmpty()) {
                        if (!specificTransitions.isEmpty()) {
                            fsb.append("Alternatively, an element that matches ");
                        } else {
                            fsb.append("Expected an element that matches ");
                        }
                        if (wildcards.size() == 1) {
                            fsb.append("the");
                        } else {
                            fsb.append("an");
                        }
                        fsb.append(" <xs:any> wildcard. ");
                    }
                } else {
                    if (wildcards.size() == 1) {
                        for (Wildcard w : wildcards) {
                            fsb.append("There is an <xs:any> wildcard, but it does not match: ");
                            fsb.append(w.reasonForNonMatch(nodeName, true, getConfiguration(), type));
                        }
                    } else if (wildcards.size() > 1) {
                        fsb.append("There are several <xs:any> wildcards, but none of them match. ");
                    }
                }
            }
        }
        return fsb.toString();
    }

    private void recover(NodeName elemName, Location locationId, int properties) throws XPathException {
        childValidator = null;
        SchemaType childType = type.getContextDeterminedTypeForElement(elemName.getStructuredQName());
        if (childType != null && !(childType.isComplexType() && ((ComplexType) childType).isAbstract())) {
            List<ValidationFailure> failures = new ArrayList<ValidationFailure>(1);
            childValidator = makeValidator(null, elemName.getStructuredQName(), locationId, getValidationContext(), childType,
                    Validation.BY_TYPE, getPipelineConfiguration(), nextReceiver, failures);
            for (ValidationFailure failure : failures) {
                reportValidationError(failure, false, locationId);
            }
        }
        if (childValidator == null){
            childType = AnyType.getInstance();
            childValidator = new AnyTypeValidator(nextReceiver);
        }
        nextReceiver.startElement(elemName, childType, locationId, properties);
    }

    /**
     * Get a validator to handle the children of the current element. This
     * is called immediately on return from the startElement call that starts
     * this element.
     */

    protected ContentValidator getChildValidator() {
        return childValidator;
    }

    /**
     * Handle character data appearing as a child of the element being validated
     *
     * @param chars      The character content
     * @param locationId the location of the node in the source, or of the instruction that created it
     * @param properties Additional properties  @throws XPathException If this element does not allow character data
     */

    public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
        checkNoCharactersWhenNil(locationId);
        if (mixed) {
            if (fixedValue != null || defaultValue != null) {
                if (buffer == null) {
                    buffer = new FastStringBuffer(chars.length());
                }
                buffer.append(chars);
            }
            nextReceiver.characters(chars, locationId, properties);
        } else {
            if (Whitespace.isWhite(chars)) {
                if (ignoreIgnorable) {
                    // strip the whitespace
                } else {
                    nextReceiver.characters(chars, locationId, properties);
                }
            } else {
                String message = "The content model for " + getContainingElementName() +
                        " does not allow character content";
                ValidationFailure ve = new ValidationFailure(message);
                ve.setSchemaType(type);
                ve.setConstraintReference(1, "cvc-complex-type", "2.3");
                reportValidationError(ve, true, locationId);
            }
        }
    }

    /**
     * Validation at the end of the element being validated. (Note this is a different element from
     * the one passed to startElement).
     *
     * @throws XPathException if the element is invalid
     */

    public void endElement() throws XPathException {
        if (fixedValue != null) {
            if (isNil()) {
                String message = "The " + getContainingElementName() +
                        " has a fixed value so it must not be nil";
                ValidationFailure ve = new ValidationFailure(message);
                ve.setConstraintReference(1, "cvc-elt", "3.2.2");
                ve.setSchemaType(type);
                reportValidationError(ve, true, getContainingElementLocationId());

                nextReceiver.endElement();
                return;
            }
            if (buffer == null || buffer.isEmpty()) {
                characters(fixedValue, ExplicitLocation.UNKNOWN_LOCATION, 0);
            } else if (!buffer.toString().equals(fixedValue)) {
                String message = "The content of " + getContainingElementName() +
                        " differs from the fixed value defined in the schema. Fixed value is " +
                        Err.wrap(fixedValue, Err.VALUE) +
                        ", actual value is " + Err.wrap(buffer, Err.VALUE);
                ValidationFailure ve = new ValidationFailure(message);
                ve.setSchemaType(type);
                ve.setConstraintReference(1, "cvc-elt", "5.2.2.2");
                reportValidationError(ve, true, getContainingElementLocationId());
            }
        }
        if (defaultValue != null && !foundChildren) {
            if (buffer == null || buffer.isEmpty()) {
                characters(defaultValue, ExplicitLocation.UNKNOWN_LOCATION, 0);
            }
        }
        if (!state.isFinalState() && !isNil() && !locallyInvalid) {
            String message = supplyReasonForNonMatch(null);
            ValidationFailure ve = new ValidationFailure(message);
            ve.setSchemaType(type);
            ve.setConstraintReference(1, "cvc-complex-type", "2.4");
            reportValidationError(ve, true, getContainingElementLocationId());
        }
        nextReceiver.endElement();
    }
}

// Copyright � 2004-2012 Saxonica Limited. No warranty.
