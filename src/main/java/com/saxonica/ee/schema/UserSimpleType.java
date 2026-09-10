package com.saxonica.ee.schema;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.lib.ConversionRules;
import net.sf.saxon.om.*;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.SaxonErrorCode;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATING;


/**
 * This class represents a used-defined simple type, as defined in the XML Schema specification.
 * This may be an atomic type, a list type, or a union type. In the non-schema-aware version of
 * Saxon, a simple type will always be a built-in type. With the schema-aware product,
 * it may also be a user-defined simple type.
 */

public abstract class UserSimpleType extends UserDefinedType
        implements SimpleType, UserSchemaComponent, SerializableSchemaComponent {

    /**
     * The constraining facets of this type
     */

    private List<Facet> facetList = new ArrayList<>(10);

    /**
     * The complete facet list including facets inherited from supertypes (but excluding any that
     * are redundant)
     */

    private List<Facet> extendedFacetList;

    /**
     * The schema component that acts as the context for this component, if this component
     * is an anonymous simple type definition
     */

    private SchemaComponent contextComponent;

    /**
     * Construct a UserSimpleType
     */

    public UserSimpleType() {
    }


    /**
     * Returns true if this SchemaType is a SimpleType
     *
     * @return true (always)
     */

    @Override
    public final boolean isSimpleType() {
        return true;
    }

    /**
     * Test whether this Simple Type is an atomic type
     *
     * @return the default implementation returns false
     */

    @Override
    public boolean isAtomicType() {
        return false;
    }

    /**
     * Ask whether this type is an ID type. This is defined to be any simple type
     * who typed value may contain atomic values of type xs:ID: that is, it includes types derived
     * from ID by restriction, list, or union. Note that for a node to be treated
     * as an ID, its typed value must be a *single* atomic value of type ID; the type of the
     * node, however, can still allow a list.
     */

    @Override
    public boolean isIdType() throws MissingComponentException {
        return false;
    }

    /**
     * Ask whether this type is an IDREF or IDREFS type. This is defined to be any simple type
     * who typed value may contain atomic values of type xs:IDREF: that is, it includes types derived
     * from IDREF or IDREFS by restriction, list, or union
     */

    @Override
    public boolean isIdRefType() throws MissingComponentException {
        return false;
    }

    /**
     * Returns true if this type is derived by list, or if it is derived by restriction
     * from a list type, or if it is a union that contains a list as one of its members
     *
     * @return true if this is a list type
     */

    @Override
    public boolean isListType() {
        return false;
    }

    /**
     * Return true if this type is a union type (that is, if its variety is union),
     * which will be true if is derived by union, or if it is derived by restriction from
     * a type whose variety is union.
     *
     * @return true for a union type
     */

    @Override
    public boolean isUnionType() {
        return false;
    }

    /**
     * Get the component that contains the definition of this anonymous simple type definition
     *
     * @return the containing component, or null of the type is not anonymous
     */

    public SchemaComponent getContextComponent() {
        return contextComponent;
    }

    /**
     * Set the component that contains the definition of this anonymous simple type definition
     *
     * @param contextComponent the containing component, or null of the type is not anonymous
     */


    public void setContextComponent(SchemaComponent contextComponent) {
        this.contextComponent = contextComponent;
    }

    /**
     * Get the extended facet list, creating it if necessary. The extended facet
     * list includes facets inherited from supertypes, unless they are redundant. This
     * includes all constraining facets (but not fundamental facets). It does not
     * include facets from built-in base types.
     *
     * @return the extended facet list
     */

    public List<Facet> getExtendedFacetList() {
        if (extendedFacetList != null) {
            return extendedFacetList;
        }
        extendedFacetList = addInheritedFacets(facetList);
        return extendedFacetList;
    }

    protected List<Facet> addInheritedFacets(List<Facet> localFacets) {
        SchemaType baseType = getBaseType();
        if (baseType instanceof UserSimpleType) {
            List<Facet> extendedFacetList = new ArrayList<>(10);
            Set<String> facetsFound = new HashSet<>();
            for (Facet facet : localFacets) {
                extendedFacetList.add(facet);
                facetsFound.add(facet.getName());
            }

            List<Facet> baseFacetList = ((UserSimpleType) baseType).getExtendedFacetList();
            for (Facet facet : baseFacetList) {
                if (facet instanceof PatternFacet || facet instanceof AssertionFacet ||
                        !facetsFound.contains(facet.getName())) {
                    extendedFacetList.add(facet);
                    facetsFound.add(facet.getName());
                }
            }
            return extendedFacetList;
        } else {
            return localFacets;
        }
    }

    /**
     * Get the name of this type as an EQName, that is, a string in the format Q{uri}local.
     *
     * @return an EQName identifying the type. In the case of an anonymous type, an internally-generated
     *         name is returned
     */
    @Override
    public String getEQName() {
        return "Q{" + getTargetNamespace() + "}" + getName();
    }

    /**
     * Add the given Facet to this Simpletype.
     *
     * @param facet the Facet to add to this Simpletype
     */

    public void addFacet(Facet facet) {
        facetList.add(facet);
    }

    /**
     * Merge enumeration facets into a single enumerationSet pseudo-facet.
     * This method also merges multiple pattern facets into a single pattern facet with
     * multiple branches.
     *
     * @param compiler used for error reporting
     * @throws net.sf.saxon.type.SchemaException
     *          if a schema validity error is found
     * @throws MissingComponentException
     *          if an unresolved reference to a schema component is found
     */

    public void mergeEnumerationFacets(SchemaCompiler compiler) throws SchemaException, MissingComponentException {
        boolean enumFound = false;
        EnumerationFacetSet efs = new EnumerationFacetSet();
        PatternFacet pf = null;
        ConversionRules rules = getConfiguration().getConversionRules();
        // The Configuration does not hold the schema components until we have finished processing them,
        // so validation of notations is against the set of notations known to the compiler
        rules = rules.copy();
        rules.setNotationSet(compiler.getPreparedSchema());
        for (int i = facetList.size() - 1; i >= 0; i--) {
            if (facetList.get(i) instanceof EnumerationFacet) {
                EnumerationFacet e = (EnumerationFacet) facetList.get(i);
                //if (this instanceof UserAtomicType) {
                SchemaType base = getBaseType();
                AtomicSequence val;
                if (base instanceof SimpleType) {
                    try {
                        val = e.getTypedValue((SimpleType) base, rules);
                    } catch (SchemaException se) {
                        compiler.error("Enumeration value does not match base type. " +
                                se.getMessage(), this);
                        setValidationStatus(INVALID);
                        return;
                    }
                } else {
                    // base type must be xs:any (if this can happen at all???)
                    val = new StringValue(e.getValue());
                }
//                // Check the value against all the other facets. It's not an error if it's invalid,
//                // but it makes the enumeration value useless
                if (val.getLength() == 1) {
                    AtomicValue atom = val.head();
                    for (Facet facet : facetList) {
                        if (!(facet instanceof EnumerationFacet || facet instanceof EnumerationFacetSet ||
                                facet instanceof AssertionFacet || facet instanceof WhitespaceFacet)) {
                            if (!facet.testAtomicValue(atom)) {
                                UnicodeString value = atom.getUnicodeStringValue();
                                String message = "Enumeration value " + Err.wrap(value, Err.VALUE) +
                                        " is not a valid instance of the type: " +
                                        "it contravenes the " + facet.getName() + " facet";
                                String userMessage = facet.getMessage();
                                if (userMessage != null) {
                                    message += ". " + userMessage;
                                }
                                compiler.warning(message, SaxonErrorCode.SXSD1010, this);
                            }
                        }
                    }
                    // Don't bother checking in other cases - too complicated
                }
                efs.addEnumerationValue(val, e.getValue().toString());
                facetList.remove(i);
                enumFound = true;
            } else if (facetList.get(i) instanceof PatternFacet && !((PatternFacet) facetList.get(i)).isDoNotMerge()) {
                if (pf == null) {
                    pf = (PatternFacet) facetList.get(i);
                } else {
                    UnicodeString pat = facetList.get(i).getValue();
                    pf.addBranch(pat);
                    facetList.remove(i);
                }
            }
        }
        if (enumFound) {
            facetList.add(efs);
        }
        SchemaType baseType = getBaseType();
        if (baseType instanceof UserSimpleType) {
            ((UserSimpleType) baseType).mergeEnumerationFacets(compiler);
        }
    }

    /**
     * Returns the first facet associated with the given name, defined either on this type
     * or on a type from which it is derived
     *
     * @param name the required class of facet
     * @return the first facet associated with the given name
     */

    public Facet getFacet(Class<? extends Facet> name) {
        for (Facet f : getExtendedFacetList()) {
            if (f.getClass() == name) {
                return f;
            }
        }
        return null;
    }

    /**
     * Set the local facet list for this simple type
     *
     * @param facetList the list of facets
     */

    public void setFacetList(List<Facet> facetList) {
        this.facetList = facetList;
    }

    /**
     * Convert the values of facets to the target data type
     *
     * @throws net.sf.saxon.type.SchemaException
     *          if conversion fails
     */

    public void convertFacetValues() throws SchemaException {
        for (Facet facet : facetList) {
            if (facet instanceof ValueRangeFacet) {
                ((ValueRangeFacet) facet).convertToTargetType((SimpleType) getBaseType(), getConfiguration());
            }
        }
    }

    /**
     * Determine the whitespace normalization required for values of this type
     *
     * @return one of PRESERVE, REPLACE, COLLAPSE
     * @throws IllegalStateException if the simple type is not fully validated and the
     *                               whitespace facet is not yet known.
     */

    @Override
    public int getWhitespaceAction() {
        int action = -1;
        Facet ws = getFacet(WhitespaceFacet.class);
        if (ws != null) {
            action = ws.getWhitespaceAction();
        }
        // If no explicit facets were found, the decision depends on the primitive type
        if (action == -1) {
            if (this instanceof UserAtomicType && ((UserAtomicType) this).getPrimitiveType() == StandardNames.XS_STRING) {
                TypeHierarchy th = getConfiguration().getTypeHierarchy();
                if (th.isSubType((UserAtomicType) this, BuiltInAtomicType.TOKEN)) {
                    return Whitespace.COLLAPSE;
                } else if (th.isSubType((UserAtomicType) this, BuiltInAtomicType.NORMALIZED_STRING)) {
                    return Whitespace.REPLACE;
                } else {
                    return Whitespace.PRESERVE;
                }
            } else if (isUnionType()) {
                return Whitespace.PRESERVE;
            } else {
                return Whitespace.COLLAPSE;
            }
        }
        return action;
    }

    /**
     * Apply the whitespace normalization rules for this simple type
     *
     * @param value the string before whitespace normalization
     * @return the string after whitespace normalization
     */

    public UnicodeString applyWhitespaceNormalization(UnicodeString value) {
        final int action = getWhitespaceAction();
        return Whitespace.applyWhitespaceNormalization(action, value);
    }

    /**
     * Returns the built-in base type this type is derived from.
     *
     * @return the first built-in type found when searching up the type hierarchy
     */
    /*@NotNull*/
    @Override
    public SchemaType getBuiltInBaseType() {
        SchemaType base = this;
        do {
            base = base.getBaseType();
            assert base != null;
        } while (base.getFingerprint() > 1023);
        return base;
    }


    /**
     * Check that this type is validly derived from a given type
     *
     * @param type  the type from which this type is derived
     * @param block the derivations that are blocked by the relevant element declaration
     * @throws net.sf.saxon.type.SchemaException
     *          if the derivation is not allowed
     */

    @Override
    public void checkTypeDerivationIsOK(SchemaType type, int block) throws SchemaException {
        String s = isTypeDerivationOK(this, type, block);
        if (s != null) {
            throw new SchemaException(s);
        }
    }

    /**
     * Checks the validity of this SimpleType definition.
     *
     * @param compiler used for error reporting
     * @throws SchemaException if this SimpleType definition is invalid.
     */

    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {

        // check for previous validation
        if (!isValidationNeeded()) {
            return true;
        }
        setValidationStatus(VALIDATING);

        // Check that the base type is a simple type

        if (getFingerprint() < 1023) {
            // built-in type
            setValidationStatus(VALIDATED);
            return true;
        }

        SchemaType base = getBaseType();
        if (base == null) {
            TypeReference ref = new TypeReference(StandardNames.XS_ANY_SIMPLE_TYPE, getConfiguration(), this);
            ref.setTarget(AnySimpleType.getInstance());
            setBaseTypeReference(ref);
        } else if (base instanceof UserSimpleType) {
            ((UserSimpleType) base).validate(compiler);
        } else if (!base.isSimpleType()) {
            compiler.error("The base type of the simple type " +
                    getDescription() +
                    " is not a simple type", this);
            setValidationStatus(INVALID);
            return false;
        }
        setValidationStatus(VALIDATED);
        return true;
    }

    /**
     * Analyze an expression to see whether the expression is capable of delivering a value of this
     * type.
     *
     * @param expression the expression that delivers the content
     * @param kind       the node kind whose content is being delivered: {@link Type#ELEMENT},
     *                   {@link Type#ATTRIBUTE}, or {@link Type#DOCUMENT}
     * @param schema
     * @throws net.sf.saxon.trans.XPathException if the expression will never deliver a value of the correct type
     */

    @Override
    public void analyzeContentExpression(Expression expression, int kind, Schema schema) throws XPathException {
        BuiltInAtomicType.analyzeContentExpression(this, expression, kind);
    }

    /**
     * Get the typed value of a node that is annotated with this schema type.
     *
     * @param node the node whose typed value is required
     * @return the typed value.
     * @since 8.5
     */

    @Override
    public AtomicSequence atomize(NodeInfo node) throws XPathException {
        if (node.isNilled()) {
            return EmptyAtomicSequence.getInstance();
        }
        try {
            NamespaceResolver resolver = null;
            if (isNamespaceSensitive()) {
                resolver = node.getNodeKind() == Type.ELEMENT ? node.getAllNamespaces() : node.getParent().getAllNamespaces();
            }
            return getTypedValue(node.getUnicodeStringValue(), resolver, getConfiguration().getConversionRules());
        } catch (ValidationException err) {
            throw new XPathException("Internal error: value doesn't match its type annotation. " + err.getMessage());
        }
    }


    /**
     * Check whether type derivation is allowed. This implements the
     * Schema Component Constraint: Type Derivation OK (Simple). This type
     * is D (the derived type); the type supplied as an argument is B (the
     * base type), and the argument (a subset (extension, restriction, list, union))
     * is provided in the second argument. If type derivation is allowed, the
     * method return null; otherwise it returns a string that can be used as an error
     * message
     *
     * @param d          the derived type
     * @param b          the base type
     * @param derivation the disallowed derivations, as a bit-significant integer
     * @return null if type derivation is OK; otherwise a string that can be used as an error message
     */

    public static String isTypeDerivationOK(SimpleType d, SchemaType b, int derivation) throws MissingComponentException {

        // rule 1
        if (d == b) {
            return null;
        }
        // rule 2.1 (first half)
        if ((derivation & Derivation.DERIVATION_RESTRICTION) != 0) {
            return "Simple type derivation is not OK: " +
                    "'restriction' is in the set of disallowed derivations";
        }

        // rule 2.1 (second half)
        SchemaType actualBase = d.getBaseType();
        if (actualBase != null && !actualBase.allowsDerivation(Derivation.DERIVATION_RESTRICTION)) {
            return "Simple type derivation is not OK: " +
                    "the base type of type " + d.getDescription() + " does not allow derivation by restriction";
        }

        // rule 2.2.1
        //noinspection ConstantConditions
        if (actualBase == b || actualBase.getFingerprint() == b.getFingerprint()) {
            // the second condition arises when handling the schema for schemas
            return null;
        }

        // rule 2.2.2
        String reason = null;
        if (actualBase != AnyType.getInstance()) {
            if (actualBase instanceof SimpleType) {
                reason = isTypeDerivationOK((SimpleType) actualBase, b, derivation);
                if (reason == null) {
                    return null;
                }
            } else {
                try {
                    UserComplexType.checkTypeDerivation((ComplexType) actualBase, b, derivation);
                    return null;
                } catch (SchemaException err) {
                    reason = err.getMessage();
                }
            }
        }

        // rule 2.2.3
        if (d.isListType() || d.isUnionType()) {
            if (b == AnySimpleType.getInstance()) {
                return null;
            }
        }

        // rule 2.2.4
        // We augment this with a rule that the union type must not contain any restricting facets
        // (patterns or enumerations). This is a bug in the 1.0 spec (MS test stZ073b; bugzilla 4065),
        // fixed in XSD 1.1 (see rule 2.2.4.3)
        if (b.isSimpleType() && ((SimpleType) b).isUnionType() && b != ErrorType.getInstance() &&
                /*b.getDerivationMethod() != SchemaType.DERIVATION_RESTRICTION*/
                ((UserUnionType) b).getExtendedFacetList().isEmpty()) {
            for (TypeReference ref : ((UserUnionType) b).getMemberTypeReferences()) {
                SimpleType member = (SimpleType) ref.getTarget();
                reason = isTypeDerivationOK(d, member, derivation);
                if (reason == null) {
                    return null;
                }
            }
        }

        return "The type " + d.getDescription() + " is not validly derived from the type " + b.getDescription()
                + (reason == null || reason.contains(" is not validly derived from the type ") ? "" : ". " + reason);

    }

    /**
     * Return all the facets for this type
     *
     * @return the list of facets defined on this simple type (this doesn't include inherited facets)
     */

    public List<Facet> getLocalFacetList() {
        return facetList;
    }

    /**
     * Apply any pre-lexical facets, other than whitespace. At the moment the only such
     * facet is saxon:preprocess
     *
     * @param input the value to be preprocessed
     * @return the value after preprocessing
     */

    @Override
    public UnicodeString preprocess(UnicodeString input) throws ValidationException {
        for (Facet f : getExtendedFacetList()) {
            if (f instanceof PreprocessFacet) {
                input = ((PreprocessFacet) f).preprocess(input);
            }
        }
        return input;
    }

    /**
     * Reverse any pre-lexical facets, other than whitespace. At the moment the only such
     * facet is saxon:preprocess. This is called when converting a value of this type to
     * a string
     *
     * @param input the value to be postprocessed: this is the "ordinary" result of converting
     *              the value to a string
     * @return the value after postprocessing
     */

    @Override
    public UnicodeString postprocess(UnicodeString input) throws ValidationException {
        for (Facet f : getExtendedFacetList()) {
            if (f instanceof PreprocessFacet) {
                input = ((PreprocessFacet) f).postprocess(input);
            }
        }
        return input;
    }

    /**
     * Validate that the value conforms to all the constraining facets for the given type.
     *
     * @param primitiveValue the typed value to be checked, as an instance of its primitive type
     * @param lexicalValue   The supplied value; this must be normalized using the whitespace
     *                       facet, and then checked against the pattern facet.
     *                       May be null; if so the actual value is converted to a string for checking
     *                       against the pattern.
     * @param facets         List of the facets to be checked: can be obtained using one of the various
     *                       getFacets() methods.
     * @param rules          Validation context (including values of parameters declared using saxon:param)
     * @return null if the value is OK, otherwise a ValidationFailure. Note that
     *         the exception is returned rather than being thrown. This is designed to
     *         make the "castable" operator more efficient.
     */

    public ValidationFailure checkAgainstFacets(
            AtomicSequence primitiveValue, UnicodeString lexicalValue, List<Facet> facets, ConversionRules rules) {
        UnicodeString normalizedValue = null;
        for (Facet f : facets) {

            if (f instanceof PatternFacet) {
                if (normalizedValue == null) {
                    if (lexicalValue == null) {
                        normalizedValue = primitiveValue.getUnicodeStringValue();
                    } else {
                        normalizedValue = applyWhitespaceNormalization(lexicalValue);
//                        if (primitiveValue instanceof StringValue) {
//                            ((StringValue) primitiveValue).setStringValueCS(normalizedValue.getStringValueCS());
//                        }
                    }
                }
                if (!f.testAtomicValue(new StringValue(normalizedValue))) {
                    return makeValidationFailure(primitiveValue, this, f);
                }
            } else if (f instanceof AssertionFacet) {
                if (primitiveValue instanceof AtomicValue) {
                    AtomicValue av = (AtomicValue) primitiveValue;
                    if (!((AssertionFacet) f).testAtomicValue(av, rules)) {
                        return makeValidationFailure(av, this, f);
                    }
                } else {
                    if (!((AssertionFacet) f).testListValue(primitiveValue, rules)) {
                        return makeValidationFailure(primitiveValue, this, f);
                    }
                }
            } else {
                if (primitiveValue instanceof AtomicValue) {
                    AtomicValue av = (AtomicValue) primitiveValue;
                    if (!f.testAtomicValue(av)) {
                        return makeValidationFailure(av, this, f);
                    }
                } else {
                    if (!f.testListValue(primitiveValue)) {
                        return makeValidationFailure(primitiveValue, this, f);
                    }
                }
            }
        }

        return null;
    }

    public static ValidationFailure makeValidationFailure(AtomicSequence primitiveValue, SimpleType type, Facet f) {
        String message = "Value " + Err.wrap(displayValue(primitiveValue), Err.VALUE) + " contravenes the " +
                f.getName() + " facet " +
                (f.getValue() == null ? "" : Err.wrap(f.getValue(), Err.VALUE) + ' ') +
                "of the type " + type.getDescription();
        if (f.getMessage() != null) {
            message += ". " + f.getMessage();
        }
        ValidationFailure ve = new ValidationFailure(message);
        ve.setConstraintReference(2, f.getConstraintName(), "1");
        ve.setErrorCode("FORG0001");
        return ve;
    }

    /**
     * Display a value in a form suitable for error messages
     *
     * @param primitiveValue the value to be displayed
     * @return a displayable representation of the value (truncated if it is long)
     */

    private static String displayValue(AtomicSequence primitiveValue) {
        String v = primitiveValue.getUnicodeStringValue().toString();
        if (v.length() > 30) {
            v = v.substring(0, 30) + "...";
        }
        return v;
    }

    /**
     * Elaborate the schema component: after reloading a serialized schema component model,
     * this expands the component with derived information needed during validation episodes.
     * The model is assumed to be valid.
     *
     * @param compiler the schema compiler
     * @throws SchemaException if a fatal error occurs
     */

    @Override
    public void elaborate(SchemaCompiler compiler) throws SchemaException {
        if (getValidationStatus() == SchemaValidationStatus.UNVALIDATED) {
            if (getBaseType() instanceof UserSchemaComponent) {
                ((UserSchemaComponent) getBaseType()).elaborate(compiler);
            }
            for (Facet facet : facetList) {
                if (facet instanceof ValueRangeFacet) {
                    ((ValueRangeFacet) facet).convertToTargetType((SimpleType) getBuiltInBaseType(), getConfiguration());
                }
            }
            mergeEnumerationFacets(compiler);
            setValidationStatus(SchemaValidationStatus.VALIDATED);
        }
    }

    /**
     * Serialize the schema component
     */
    @Override
    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        String id = serializer.getId(this, true);
        serializer.startElement("simpleType");
        serializer.emitAttribute("id", id);
        if (!isAnonymousType()) {
            serializer.emitAttribute("name", getName());
            if (!getTargetNamespace().isEmpty()) {
                serializer.emitAttribute("targetNamespace", getTargetNamespace().toString());
            }
        }
        serializer.emitAttribute("base", serializer.getTypeLink(getBaseType()));
        if (finalProhibitions != 0) {
            String ex = "";
            if ((finalProhibitions & Derivation.DERIVATION_RESTRICTION) != 0) {
                ex = "restriction";
            }
            if ((finalProhibitions & Derivation.DERIVATION_LIST) != 0) {
                ex += (ex.isEmpty() ? "" : " ") + "list";
            }
            if ((finalProhibitions & Derivation.DERIVATION_UNION) != 0) {
                ex += (ex.isEmpty() ? "" : " ") + "union";
            }
            serializer.emitAttribute("final", ex);
        }
        serializeVariety(serializer);
        boolean seenEnumeration = false;
        for (Facet f : getExtendedFacetList()) {
            if (f instanceof EnumerationFacetSet) {
                // only output the first enumeration (omit enumeration facets on supertypes)
                if (seenEnumeration) {
                    continue;
                }
                seenEnumeration = true;
            }
            f.serializeFacet(serializer);
        }
        serializer.endElement();
    }

    protected abstract void serializeVariety(SchemaModelSerializer serializer) throws XPathException;

    @Override
    public FunctionItem getComponentAsFunction() {
        return getComponentAsFunction(this);
    }

    /**
     * Get schema information for a simple type in the form of a function item
     *
     * @param type The type whose component information is required
     * @return a function item representing a function whose argument is a property
     *         name and whose result is the value of that property
     */

    public static FunctionItem getComponentAsFunction(final SimpleType type) {
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return StringValue.bmp("Simple Type Definition");
                case "implementation":
                    return new ObjectValue<>(type);
                case "name":
                    if (type.isAnonymousType()) {
                        return EmptySequence.getInstance();
                    } else {
                        return new StringValue(type.getName(), BuiltInAtomicType.NCNAME);
                    }
                case "target namespace":
                    if (type.isAnonymousType()) {
                        return EmptySequence.getInstance();
                    } else {
                        return new AnyURIValue((type.getTargetNamespace().toUnicodeString()));
                    }
                case "final": {
                    List<AtomicValue> list = new ArrayList<>();
                    int f = type.getFinalProhibitions();
                    if ((f & Derivation.DERIVATION_RESTRICTION) != 0) {
                        list.add(StringValue.bmp("restriction"));
                    }
                    if ((f & Derivation.DERIVATION_EXTENSION) != 0) {
                        list.add(StringValue.bmp("extension"));
                    }
                    if ((f & Derivation.DERIVATION_LIST) != 0) {
                        list.add(StringValue.bmp("list"));
                    }
                    if ((f & Derivation.DERIVATION_UNION) != 0) {
                        list.add(StringValue.bmp("union"));
                    }
                    return new AtomicArray(list);
                }
                case "context":
                    if (type.isAnonymousType()) {
                        UserSimpleType stype = (UserSimpleType) type;
                        SchemaComponent contextComponent = stype.getContextComponent();
                        if (contextComponent == null) {
                            return EmptySequence.getInstance();
                        } else {
                            return contextComponent.getComponentAsFunction();
                        }
                    } else {
                        return EmptySequence.getInstance();
                    }
                case "base type definition":
                    return type.getBaseType().getComponentAsFunction();
                case "facets": {
                    if (type instanceof UserSimpleType) {
                        List<Item> list = new ArrayList<>();
                        for (Facet f : ((UserSimpleType) type).getExtendedFacetList()) {
                            list.add(f.getFacetAsFunctionItem());
                        }
                        return SequenceExtent.makeSequenceExtent(list);
                    } else if (type instanceof BuiltInAtomicType) {
                        List<Facet> facetList = getExtendedFacetList((BuiltInAtomicType) type, context.getConfiguration());
                        List<Item> list = new ArrayList<>();
                        for (Facet f : facetList) {
                            list.add(f.getFacetAsFunctionItem());
                        }
                        return SequenceExtent.makeSequenceExtent(list);
                    } else {
                        // TODO: facets on a built-in list type; whiteSpace facet
                        return EmptySequence.getInstance();
                    }
                }
                case "fundamental facets":
                    // TODO: not implemented
                    return EmptySequence.getInstance();
                case "variety":
                    String variety = "atomic";
                    if (type.isListType()) {
                        variety = "list";
                    } else if (type.isUnionType()) {
                        variety = "union";
                    }
                    return new StringValue(variety);
                case "primitive type definition":
                    if (type == BuiltInAtomicType.ANY_ATOMIC || !type.isAtomicType()) {
                        return EmptySequence.getInstance();
                    } else {
                        return getComponentAsFunction((AtomicType)(((AtomicType)type).getPrimitiveItemType()));
                    }
                case "item type definition":
                    if (type.isListType()) {
                        return ((ListType) type).getItemType().getComponentAsFunction();
                    } else {
                        return EmptySequence.getInstance();
                    }
                case "member type definitions":
                    if (type.isUnionType()) {
                        List<Item> list = new ArrayList<>();
                        for (TypeReference member : ((UserUnionType) type).getMemberTypeReferences()) {
                            list.add(member.getTarget().getComponentAsFunction());
                        }
                        return SequenceExtent.makeSequenceExtent(list);
                    } else {
                        return EmptySequence.getInstance();
                    }
                default:
                    return EmptySequence.getInstance();
            }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }

    /**
     * Get the list of constraining facets for a built-in type.
     * Constraining facets for built-in types are not used operationally
     * by Saxon, but can be generated on demand where they are needed.
     *
     * @param type   a built-in type
     * @param config the Saxon configuration (needed for regex handling)
     * @return the list of constraining facets for a type. For primitive types this will be an
     *         empty list
     */

    private static List<Facet> getExtendedFacetList(BuiltInAtomicType type, Configuration config) {
        List<Facet> list = new ArrayList<>(4);
        int prim = type.getPrimitiveType();
        int fp = type.getFingerprint();
        if (prim == StandardNames.XS_INTEGER) {
            IntegerValue min = Int64Value.getMinInclusive(type);
            if (min != null) {
                MinInclusiveFacet facet = new MinInclusiveFacet();
                facet.setRequiredValue(min);
                list.add(facet);
            }
            IntegerValue max = Int64Value.getMaxInclusive(type);
            if (max != null) {
                MaxInclusiveFacet facet = new MaxInclusiveFacet();
                facet.setRequiredValue(max);
                list.add(facet);
            }
        } else if (prim == StandardNames.XS_STRING) {
            switch (fp) {
                case StandardNames.XS_NORMALIZED_STRING:
                case StandardNames.XS_TOKEN:
                    break;
                case StandardNames.XS_LANGUAGE: {
                    PatternFacet pat = new PatternFacet(config);
                    pat.setValue("[a-zA-Z]{1,8}(-[a-zA-Z0-9]{1,8})*");
                    list.add(pat);
                    break;
                }
                case StandardNames.XS_NMTOKEN: {
                    PatternFacet pat = new PatternFacet(config);
                    pat.setValue("\\c+");
                    list.add(pat);
                    break;
                }
                case StandardNames.XS_NAME: {
                    PatternFacet pat = new PatternFacet(config);
                    pat.setValue("\\i\\c+");
                    list.add(pat);
                    break;
                }
                case StandardNames.XS_NCNAME:
                case StandardNames.XS_ID:
                case StandardNames.XS_IDREF:
                case StandardNames.XS_ENTITY: {
                    PatternFacet pat = new PatternFacet(config);
                    pat.setValue("\\i\\c+");
                    list.add(pat);
                    pat = new PatternFacet(config);
                    pat.setValue("[\\i-[:]][\\c-[:]]*");
                    list.add(pat);
                    break;
                }
                default:
                    break;
            }
        } else if (fp == StandardNames.XS_DAY_TIME_DURATION) {
            PatternFacet pat = new PatternFacet(config);
            pat.setValue("[^YM]*(T.*)?");
            list.add(pat);
        } else if (fp == StandardNames.XS_YEAR_MONTH_DURATION) {
            PatternFacet pat = new PatternFacet(config);
            pat.setValue("[^DT]*");
            list.add(pat);
        } else if (fp == StandardNames.XS_DATE_TIME_STAMP) {
            ExplicitTimezoneFacet etf = new ExplicitTimezoneFacet();
            try {
                etf.setValue("required");
            } catch (SchemaException e) {
                throw new AssertionError(e);
            }
            list.add(etf);
        }
        return list;
    }

}

// Copyright (c) 2004-2023 Saxonica Limited