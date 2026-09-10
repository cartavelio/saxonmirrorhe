package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StandardNames;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.z.IntHashSet;
import net.sf.saxon.z.IntSet;

/**
 * An xs:restriction element in an XML Schema Document, whose parent element is an xs:simpleType
 */

public class XSDSimpleTypeRestriction extends SchemaElement {

    /**
     * The simpleType we are constructing
     */
    /*@Nullable*/ private SimpleTypeDefinition typeDefinition = null;

    private boolean foundBase = false;

    /**
     * Create a restriction of a simple type
     */

    public XSDSimpleTypeRestriction() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        typeDefinition = getContainingSimpleType();

        AttributeMap atts = attributes();
        String[] allowed = {"base", "id"};
        allowAttributes(atts, allowed);

        //-- @id
        processId();

        //-- @base
        String base = atts.getValue("", "base");
        foundBase = (base != null);
        if (foundBase) {
            int baseFingerprint;
            try {
                baseFingerprint = getFingerprint(base, USE_DEFAULT_NS);
            } catch (SchemaException err) {
                error(err.getMessage());
                return;
            }
            checkPermittedType(baseFingerprint, base, "a base type");
            TypeReference ref = new TypeReference(baseFingerprint, typeDefinition.getConfiguration(), this);
            typeDefinition.setBaseTypeReference(ref);
        }
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler used for error reporting
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {

        checkContentModel(stateTerms, stateTargets, false);

        boolean foundSimpleType = false;
        for (NodeInfo child : children()) {
            int fp = child.getFingerprint();
            if (fp == StandardNames.XS_SIMPLE_TYPE) {
                foundSimpleType = true;
                if (foundBase) {
                    mutuallyExclusiveElementAndAttribute("simpleType", "base");
                    return;
                }
            }
        }
        if (!foundBase && !foundSimpleType) {
            error("A restriction element must either have a 'base' attribute or contain a 'simpleType' definition");
        }
    }

    // Generated using Transform s4s11.scm generate-s4s-code.xsl type=C266
    // where C266 is the id of the type referenced from &lt;scm:element name="restriction"&gt;

    // and modified by hand to allow saxon:preprocess and saxon:separator

    private final static int[] state0terms = {StandardNames.XS_MIN_LENGTH, StandardNames.XS_FRACTION_DIGITS,
            StandardNames.XS_MAX_LENGTH, StandardNames.XS_SIMPLE_TYPE, StandardNames.XS_ANNOTATION,
            StandardNames.XS_PATTERN, StandardNames.XS_ENUMERATION, StandardNames.XS_MAX_INCLUSIVE,
            StandardNames.XS_MIN_INCLUSIVE, StandardNames.XS_LENGTH, StandardNames.XS_WHITE_SPACE,
            StandardNames.XS_MAX_EXCLUSIVE, StandardNames.XS_MIN_EXCLUSIVE, StandardNames.XS_EXPLICIT_TIMEZONE,
            StandardNames.XS_TOTAL_DIGITS, StandardNames.XS_ASSERTION, StandardNames.SAXON_PREPROCESS,
            StandardNames.SAXON_DISTINCT, StandardNames.SAXON_ORDER,
            StandardNames.XS_MIN_SCALE, StandardNames.XS_MAX_SCALE};

    private final static int[] state0targets = {1, 1, 1, 2, 3,  1, 1, 1, 1, 1,  1, 1, 1, 1, 1,  1, 1, 1, 1, 1, 1};

    private final static int[] state1terms = {StandardNames.XS_MIN_LENGTH, StandardNames.XS_FRACTION_DIGITS,
            StandardNames.XS_MAX_LENGTH, StandardNames.XS_PATTERN, StandardNames.XS_ENUMERATION,
            StandardNames.XS_MAX_INCLUSIVE, StandardNames.XS_MIN_INCLUSIVE, StandardNames.XS_LENGTH,
            StandardNames.XS_WHITE_SPACE, StandardNames.XS_MAX_EXCLUSIVE, StandardNames.XS_MIN_EXCLUSIVE,
            StandardNames.XS_EXPLICIT_TIMEZONE, StandardNames.XS_TOTAL_DIGITS, StandardNames.XS_ASSERTION,
            StandardNames.XS_MIN_SCALE, StandardNames.XS_MAX_SCALE, StandardNames.SAXON_PREPROCESS,
            StandardNames.SAXON_DISTINCT, StandardNames.SAXON_ORDER,};

    private final static int[] state1targets = {1, 1, 1, 1, 1,  1, 1, 1, 1, 1,  1, 1, 1, 1, 1,  1, 1, 1, 1};

    private final static int[] state2terms = {StandardNames.XS_MIN_LENGTH, StandardNames.XS_FRACTION_DIGITS,
            StandardNames.XS_MAX_LENGTH, StandardNames.XS_PATTERN, StandardNames.XS_ENUMERATION,
            StandardNames.XS_MAX_INCLUSIVE, StandardNames.XS_MIN_INCLUSIVE, StandardNames.XS_LENGTH,
            StandardNames.XS_WHITE_SPACE, StandardNames.XS_MAX_EXCLUSIVE, StandardNames.XS_MIN_EXCLUSIVE,
            StandardNames.XS_EXPLICIT_TIMEZONE, StandardNames.XS_TOTAL_DIGITS, StandardNames.XS_ASSERTION,
            StandardNames.XS_MIN_SCALE, StandardNames.XS_MAX_SCALE, StandardNames.SAXON_PREPROCESS,
            StandardNames.SAXON_DISTINCT, StandardNames.SAXON_ORDER,};

    private final static int[] state2targets = {1, 1, 1, 1, 1,  1, 1, 1, 1, 1,  1, 1, 1, 1, 1,  1, 1, 1, 1};

    private final static int[] state3terms = {StandardNames.XS_MIN_LENGTH, StandardNames.XS_FRACTION_DIGITS,
            StandardNames.XS_MAX_LENGTH, StandardNames.XS_SIMPLE_TYPE, StandardNames.XS_PATTERN,
            StandardNames.XS_ENUMERATION, StandardNames.XS_MAX_INCLUSIVE, StandardNames.XS_MIN_INCLUSIVE,
            StandardNames.XS_LENGTH, StandardNames.XS_WHITE_SPACE, StandardNames.XS_MAX_EXCLUSIVE,
            StandardNames.XS_MIN_EXCLUSIVE, StandardNames.XS_EXPLICIT_TIMEZONE, StandardNames.XS_TOTAL_DIGITS,
            StandardNames.XS_ASSERTION, StandardNames.XS_MIN_SCALE, StandardNames.XS_MAX_SCALE,
            StandardNames.SAXON_PREPROCESS, StandardNames.SAXON_DISTINCT, StandardNames.SAXON_ORDER,};

    private final static int[] state3targets = {1, 1, 1, 2, 1,  1, 1, 1, 1, 1,  1, 1, 1, 1, 1,  1, 1, 1, 1, 1};

    private final static int[][] stateTerms = {
            state0terms, state1terms, state2terms, state3terms};
    private final static int[][] stateTargets = {
            state0targets, state1targets, state2targets, state3targets};

    @Override
    protected boolean isFinalState(int state) {
        return state == 0 || state == 1 || state == 2 || state == 3;
    }

    /**
     * Process child elements after validation
     *
     * @param compiler used for error reporting
     */
    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {

        IntSet facetsSeen = new IntHashSet();
        for (NodeInfo child : children()) {
            if (typeDefinition != null) {
                int fp = child.getFingerprint();
                switch (fp) {
                    case StandardNames.XS_ANNOTATION:
                        break;
                    case StandardNames.XS_SIMPLE_TYPE:
                        SimpleTypeDefinition type = ((XSDSimpleType) child).getSimpleTypeDefinition();
                        TypeReference ref = new TypeReference(type.getFingerprint(), compiler.getConfiguration(), this);
                        typeDefinition.setBaseTypeReference(ref);
                        break;
                    case StandardNames.XS_ATTRIBUTE:
                    case StandardNames.XS_ANY_ATTRIBUTE:
                    case StandardNames.XS_ATTRIBUTE_GROUP:
                        // no action
                        break;
                    case StandardNames.XS_ASSERTION:
                        if (compiler.getLanguageVersion() == Configuration.XSD10) {
                            error("To use xs:assertion, you must set xsdversion=1.1");
                        }
                        Assertion assertion = ((XSDAssert) child).getAssertion();
                        Facet af = new AssertionFacet(assertion);
                        af.setMessage(assertion.getMessage());
                        typeDefinition.addFacet(af);
                        break;
                    case StandardNames.XS_MIN_LENGTH:
                    case StandardNames.XS_FRACTION_DIGITS:
                    case StandardNames.XS_MAX_LENGTH:
                    case StandardNames.XS_MAX_INCLUSIVE:
                    case StandardNames.XS_MIN_INCLUSIVE:
                    case StandardNames.XS_LENGTH:
                    case StandardNames.XS_WHITE_SPACE:
                    case StandardNames.XS_MAX_EXCLUSIVE:
                    case StandardNames.XS_MIN_EXCLUSIVE:
                    case StandardNames.XS_MAX_SCALE:
                    case StandardNames.XS_MIN_SCALE:
                    case StandardNames.XS_EXPLICIT_TIMEZONE:
                    case StandardNames.XS_TOTAL_DIGITS:
                    case StandardNames.SAXON_DISTINCT:
                    case StandardNames.SAXON_ORDER:
                        if (facetsSeen.contains(fp)) {
                            error("The " + child.getDisplayName() + " facet must not appear more than once");
                        }
                        facetsSeen.add(fp);
                        CSharp.emitCode("goto case Saxon.Hej.om.StandardNames.XS_PATTERN;");
                        // drop through
                    case StandardNames.XS_PATTERN:
                    case StandardNames.XS_ENUMERATION:
                        Facet facet = ((XSDFacet) child).getFacet();
                        if (facet != null) {
                            typeDefinition.addFacet(facet);
                        }
                        break;
                    case StandardNames.SAXON_PREPROCESS:
                        if (compiler.getLanguageVersion() == Configuration.XSD10) {
                            error("To use saxon:preprocess, you must set xsdversion=1.1");
                        }
                        Assertion action = ((SaxonPreprocess) child).getAction();
                        Assertion reverseAction = ((SaxonPreprocess) child).getReverseAction();
                        Facet af2 = new PreprocessFacet(action, reverseAction);
                        typeDefinition.addFacet(af2);
                        break;
                    default:
                        // error case, already reported
                }

            }
            // otherwise, the error was already reported
        }
    }

}

// Copyright (c) 2004-2022 Saxonica Limited
