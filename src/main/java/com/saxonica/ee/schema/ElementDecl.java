package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.sdoc.SimpleTypeDefinition;
import com.saxonica.ee.validate.SchemaElementTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.CallableDelegate;
import net.sf.saxon.functions.CallableFunction;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeTest;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.sxpath.XPathExpression;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.*;

import javax.xml.transform.SourceLocator;
import java.util.*;

import static net.sf.saxon.type.SchemaValidationStatus.INVALID;
import static net.sf.saxon.type.SchemaValidationStatus.UNVALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATED;
import static net.sf.saxon.type.SchemaValidationStatus.VALIDATING;

/**
 * The object represents an element declaration in the schema. It may be a global element
 * declaration, or a local element declaration.
 */
public class ElementDecl extends SchemaStructure
        implements UserSchemaComponent, SerializableSchemaComponent, ComponentWithValueConstraint, SchemaDeclaration, Term {

    /**
     * The block attribute for this element definition.
     */
    private int block = 0;

    /**
     * In the case of a locally defined element declaration that is part of a named model group,
     * the containing model group
     */
    private ModelGroupDefinition containingModelGroup = null;

    /**
     * In the case of a locally defined element declaration that is not part of a named model group,
     * the containing complex type
     */
    private UserComplexType containingComplexType = null;

    /**
     * Collection of Identity Constraints
     */
    private List<IdentityConstraintReference> constraintRefs = null;


    /**
     * The 'final' value for this element definition: that is, the set of derivations that
     * are not permitted, as a bit-significant integer.
     */

    private int finalProhibitions = 0;

    /**
     * The value constraint (fixed or default value), if any
     */

    private ValueConstraint valueConstraint;

    /**
     * Flag indicating whether this element declaration is global
     */
    private final boolean global;

    /**
     * Flag indicating whether or not this element declaration is
     * abstract
     */
    private boolean isAbstractDecl = false;

    /**
     * The element name
     */
    private StructuredQName elementName;

    /**
     * The NamePool fingerprint of the element name
     */

    private int fingerprint = -1;

    /**
     * Namespace resolved used for resolving prefixes in fixed/default value
     */
    private NamespaceResolver namespaceResolver;

    /**
     * Flag indicating whether or not the element value may specify xsi:nil='true'.
     */
    private boolean nillable = false;

    /**
     * The name of the head(s) of the substitutionGroup(s) for this element definition.
     */

    private List<SimpleComponentReference> substitutionGroupOwners = null;

    /**
     * The elements that are known to be members of the substitution group
     * of this element. This is the transitive closure of the tree. The set
     * holds the actual element declarations
     */

    private Collection<ElementDecl> substitutionGroupMembers = new ArrayList<>();

    /**
     * The target namespace of the element declaration. Set to "" for no-namespace names.
     */

    /*@NotNull*/
    private NamespaceUri targetNamespace;

    /**
     * The SchemaType for this element declaration. This is null if the element
     * declaration specifies no constraints, e.g. &lt;xs:element name="a"/>.
     */

    private TypeReference typeReference = null;

    /**
     * The list of complex types in which this element declaration appears as a particle.
     * These complex types must all be recompiled if additional members are added to the
     * substitution group of this element declaration.
     */

    private Collection<UserComplexType> complexTypesUsingThisElement = new ArrayList<>();

    /**
     * The table of type alternatives for this element, if any. The items in this list are instances of
     * class {@link TypeAlternative} The last type alternative will always have the condition "true()",
     * representing the default type definition. An empty table is indicated by a null value.
     */

    private List<TypeAlternative> typeAlternatives = null;

    /**
     * Internal generated ID used for comparing element declarations for identity
     */

    private String generatedId = null;

    /**
     * Creates a new element declaration
     *
     * @param config  the Configuration to which this element declaration
     *                belongs
     * @param global  true if the element declaration is top-level
     * @param locator identifies the location of the element declaration in the
     */

    public ElementDecl(EnterpriseConfiguration config, boolean global, SourceLocator locator) {
        setLocator(locator);
        this.global = global;
        setConfiguration(config);
        constraintRefs = new ArrayList<>(3);
    }

    /**
     * Set the generated Id for this element declaration
     *
     * @param id the generated ID
     */

    @Override
    public void setGeneratedId(String id) {
        this.generatedId = id;
    }

    /**
     * Set the namespace resolver to be used
     *
     * @param resolver the namespace resolver
     */

    public void setNamespaceResolver(NamespaceResolver resolver) {
        namespaceResolver = resolver;
    }

    @Override
    public NamespaceResolver getNamespaceResolver() {
        return namespaceResolver;
    }

    /**
     * Adds the given IdentityConstraint to this element definition.
     * For internal use only.
     *
     * @param constraint the IdentityConstraint to add.
     */

    public void addIdentityConstraint(IdentityConstraintReference constraint) {
        if (constraint == null) {
            return;
        }
        constraintRefs.add(constraint);
    }

    /**
     * Sets the containing complex type definition.
     * For internal use only.
     *
     * @param type the complex type containing this local element declaration
     */

    public void setContainingComplexType(UserComplexType type) {
        containingComplexType = type;
    }

    /**
     * Sets the containing model group.
     * For internal use only.
     *
     * @param type the named model group containing this local element declaration
     */

    public void setContainingModelGroup(ModelGroupDefinition type) {
        containingModelGroup = type;
    }

    /**
     * Returns the value of the 'block' attribute for this element. This corresponds to the
     * value of the {disallowed substitutions} property in the schema component model.
     *
     * @return the value of the block attribute, as a bit-significant integer
     *         using constants such as {@link Derivation#DERIVE_BY_SUBSTITUTION}, indicating
     *         which kinds of derivation from this element declaration are blocked.
     */

    public int getBlock() {
        return block;
    }

    /**
     * Get the value constraint (fixed or default value) that applies to this element declaration,
     * if any
     * @return the applicable value constraint if there is one, or null otherwise
     */

    @Override
    public ValueConstraint getValueConstraint() {
        return valueConstraint;
    }

    /**
     * Get the substitution group exclusions of this element, that is, the value of
     * the applicable <code>final</code> or <code>finalDefault</code> attributes.
     * Corresponds to the property {substitution group exclusions} in the schema component
     * model.
     *
     * @return the substitution group exclusions, represented as a bit-significant integer,
     *         the relevant bits being {@link Derivation#DERIVATION_EXTENSION} and
     *         {@link Derivation#DERIVATION_RESTRICTION}
     */

    public int getSubstitutionGroupExclusions() {
        return finalProhibitions;
    }

    /**
     * Determines whether derivation (of a particular kind)
     * from this type is allowed, based on the "final" property (that is,
     * the {substitution group exclusions} in the component model
     *
     * @param derivation the kind of derivation, e.g. {@link Derivation#DERIVATION_EXTENSION}
     * @return true if this kind of derivation is allowed
     */
    public boolean allowsDerivation(int derivation) {
        return (finalProhibitions & derivation) == 0;
    }

    /**
     * Get the containing complex type. This represents one aspect of the {scope} property
     * defined in the schema component model. If the element is a local element declaration
     * and is defined as part of a complex type (and not as part of a named model group),
     * returns the complex type definition. Otherwise, returns null.
     *
     * @return the containing complex type definition if there is one, otherwise null
     */

    public UserComplexType getContainingComplexType() {
        return containingComplexType;
    }

    /**
     * Get the containing named model group. This represents one aspect of the {scope} property
     * defined in the schema component model. If the element is a local element declaration
     * and is defined as part of a named model group (not as part of a complex type),
     * returns the model group definition. Otherwise, returns null.
     *
     * @return the containing model group definition if there is one, otherwise null
     */

    public ModelGroupDefinition getContainingModelGroup() {
        return containingModelGroup;
    }

    /**
     * Returns the 'fixed' value defined for this element declaration.
     *
     * @return the fixed value for this element declaration,
     *         or null if no fixed value was specified.
     */

    @Override
    public AtomicSequence getFixedValue() {
        ValueConstraint vc = getFixedValueConstraint();
        return vc == null ? null : vc.getValue();
    }

    /**
     * Return the lexical form of the "fixed" value defined for this element declaration. This is
     * the form as written in the source schema, after whitespace normalization
     *
     * @return the lexical form of the "fixed" value, or null if there is no fixed value
     */

    @Override
    public UnicodeString getFixedValueLexicalForm() {
        ValueConstraint vc = getFixedValueConstraint();
        return vc == null ? null : vc.getLexicalForm();
    }

    /**
     * Returns a list of the identity constraints defined within
     * this element definition. This corresponds to the {identity-constraint definitions}
     * property in the schema component model.
     *
     * @return a list of of {@link IdentityConstraintReference} objects contained within
     *         this element declaration.
     */

    public List<IdentityConstraintReference> getIdentityConstraints() {
        return constraintRefs;
    }

    /**
     * Returns the name of this Element declaration in a form suitable for error messages.
     * This will be the Clark name
     *
     * @return the name of this element declaration in Clark format
     */

    public String getDisplayName() {
        return elementName.getDisplayName();
    }

    /**
     * Returns the local name of this Element declaration. The name
     * returned, if not null, will be an NCName. This corresponds to the {name}
     * property defined in the schema component model.
     *
     * @return the local name of this element declaration, if it is global, or null if it is anonymous
     */
    public String getName() {
        return elementName.getLocalPart();
    }

    /**
     * Get the namespace URI of this element declaration. This corresponds to the
     * {targetNamespace} property defined in the schema component model.
     *
     * @return the target namespace if the element declaration is global and the target
     *         namespace is not "absent". Return "" for a local element declaration or for a
     *         global element in a no-namespace schema.
     */

    /*@NotNull*/
    public NamespaceUri getTargetNamespace() {
        return targetNamespace;
    }

    /**
     * Get the namepool fingerprint of the name of the element's type.
     *
     * @return the fingerprint of the type, or -1 if none is available.
     */

    public int getTypeFingerprint() {
        if (typeReference != null) {
            return typeReference.getTargetFingerprint();
        } else {
            return -1;
        }
    }

    /**
     * Returns the Schema Type (ComplexType or SimpleType) of this Element Declaration.
     * This corresponds to the {typeDefinition} property in the schema component model.
     *
     * @return the Schema Type of this Element Declaration.
     * @throws MissingComponentException if the reference from the element declaration
     *                                      to its type cannot be resolved.
     */

    @Override
    public SchemaType getType() throws MissingComponentException {
        SchemaType schemaType;
        if (typeReference == null) {
            return AnyType.getInstance();
        }
        schemaType = (SchemaType) typeReference.getTarget();
        if (schemaType == null || schemaType.getValidationStatus() == INVALID) {
            // error already reported
            return AnyType.getInstance();
        }
        if (schemaType instanceof SimpleTypeDefinition) {
            schemaType = ((SimpleTypeDefinition) schemaType).getWorkingType();
            typeReference.setTarget(schemaType);
        }
        return schemaType;
    }

    /**
     * Returns the head(s) of the substitutionGroup for this element declaration, or
     * null if there are none. This corresponds to the {substitution group affiliation} property
     * in the schema component model.
     *
     * @return the element declarations representing the heads of the substitution groups in which this element
     *         declaration is a member, or an empty set if none is defined. These are the immediate parents in the substitution
     *         group lattice.
     * @throws MissingComponentException if the element is a member of a substitution group but
     *                                      no declaration of the referenced element is (yet) available
     */

    public Set<ElementDecl> getSubstitutionGroupOwners() throws MissingComponentException {
        if (substitutionGroupOwners != null) {
            Set<ElementDecl> heads = new HashSet<>(substitutionGroupOwners.size());
            for (ComponentReference headRef : substitutionGroupOwners) {
                heads.add((ElementDecl) headRef.getTarget());
            }
            return heads;
        } else {
            return Collections.emptySet();
        }
    }

    /**
     * Get the known members of the substitution group of this element, as a Set
     * containing the ElementDecl objects. This includes all applicable element declarations
     * in schemas compiled within the same Configuration as this schema.
     *
     * @return a set containing all the substitution group members, expanded transtively
     */

    public Collection<ElementDecl> getSubstitutionGroupMembers() {
        return substitutionGroupMembers;
    }

    /**
     * Add a member of the substitution group that has this element as its head.
     * For internal use only.
     * <p>This method may be called at any time, for example when a new schema is loaded as a result
     * of validating an instance document. The method is therefore synchronized, since the
     * schema may be used in several threads concurrently. Adding a new member to a substitution group
     * means that all complex type definitions that include this element in their content model
     * must be recompiled.</p>
     * <p>It is not possible to add to a substitution group once the element declaration has been used,
     * either for validating a source document or for compiling stylesheets or schemas.</p>
     *
     * @param member   The element declaration to be added to the substitution group of which this
     *                 element declaration is the head. Elements are added to each ancestor in the substitution
     *                 group hierarchy.
     * @param compiler used for error reporting
     * @throws net.sf.saxon.type.SchemaException
     *          if a fatal error occurs
     */

    public synchronized void addSubstitutionGroupMember(ElementDecl member, SchemaCompiler compiler)
            throws SchemaException {
        if (!substitutionGroupMembers.contains(member)) {
            EnterpriseConfiguration config = getConfiguration();
            if (member != this) {
                member.fixup(compiler);
                String reason = member.isValidlySubstitutable(this);
                // If the member is not validly substitutable, this is not in general an error,
                // but we report it as a warning.
                if (reason != null) {
                    boolean isError = false;
                    // In XSD 1.1 this becomes an error if the reason it's not validly substitutable
                    // is to do with the element's type.
                    if (compiler.getConfiguration().getXsdVersion() == Configuration.XSD11 &&
                            reason.startsWith("required type derivation method")) {
                        isError = true;
                    }
                    String memberName = member.getDisplayName();
                    String msg = "Element " + Err.wrap(memberName, Err.ELEMENT) +
                            " cannot participate in the substitution group of element " +
                            Err.wrap(getName(), Err.ELEMENT) + ": " + reason;
                    try {
                        Set<ElementDecl> grandFathers = getSubstitutionGroupOwners();
                        for (ElementDecl grandFather : grandFathers) {
                            if (member.isValidlySubstitutable(grandFather) == null) {
                                msg += ". However, element " + Err.wrap(memberName, Err.ELEMENT) +
                                        " can substitute for element " +
                                        Err.wrap(grandFather.getDisplayName(), Err.ELEMENT);
                                break;
                            }
                        }
                    } catch (MissingComponentException err) {
                        // ignore
                    }

                    if (isError) {
                        compiler.error(msg, member);
                        throw new SchemaException("Disallowed substitution group membership");
                    } else {
                        compiler.warning(msg, member);
                    }
                    return;
                }
            }

            if (member != this && config.isSealedNamespace(getTargetNamespace())) {
                String msg = "It is not possible to add to the substitution group of element " + getName() +
                        " in namespace '" + getTargetNamespace() +
                        "', because the schema for that namespace has already been used for validating instance documents," +
                        " or for compiling queries or stylesheets";
                SchemaException err = new SchemaException(msg);
                err.setLocator(member);
                throw err;
            }
            substitutionGroupMembers.add(member);
            if (substitutionGroupMembers.size() > 10) {
                substitutionGroupMembers = new HashSet<>(substitutionGroupMembers);
            }
            // This invalidates the compiled form of all complex types using this element
            if (member != this) {
                for (UserComplexType type : complexTypesUsingThisElement) {
                    type.recompile(compiler);
                }
            }
        }
    }

    /**
     * Register a complex type that contains this element as a particle. If new elements are added
     * to the substitution group headed by this element, these complex types will need to be
     * recompiled to permit the new substititution group members. For internal use only.
     *
     * @param type A complex type that contains this element as a particle.
     */

    public synchronized void registerComplexTypeUsingThisElement(UserComplexType type) {
        if (!complexTypesUsingThisElement.contains(type)) {
            complexTypesUsingThisElement.add(type);
            if (complexTypesUsingThisElement.size() > 10) {
                complexTypesUsingThisElement = new HashSet<>(complexTypesUsingThisElement);
            }
        }
    }

    /**
     * Returns true if this element definition is abstract. This
     * corresponds to the {abstract} property in the schema component model.
     *
     * @return true if this element definition is abstract
     */

    @Override
    public boolean isAbstract() {
        return isAbstractDecl;
    }

    /**
     * Determine whether or not instances of this element definition
     * permit xsi:nil to be set to "yes". This corresponds to the
     * {nillable} property in the schema component model.
     *
     * @return true if instances conforming to this element definition
     *         may be nilled, and false otherwise.
     */

    @Override
    public boolean isNillable() {
        return nillable;
    }

    /**
     * Determine whether this element declaration is global. This represents
     * one aspect of the {scope} property defined in the schema data model.
     *
     * @return true if the element declaration is global, false if it is local
     */

    public boolean isGlobal() {
        return global;
    }

    /**
     * Sets whether or not this element definition is abstract.
     * For internal use only.
     *
     * @param isAbstract a boolean: when true indicates that this
     *                   element definition is abstract
     */

    public void setAbstract(boolean isAbstract) {
        this.isAbstractDecl = isAbstract;
    }

    /**
     * Sets the value of the 'block' attribute for this element
     * For internal use only.
     *
     * @param block the value of the block attribute for this
     *              element definition, indicating the types of derivation that are not allowed,
     *              as a bit-significant integer, using constants such as {@link Derivation#DERIVATION_EXTENSION}
     */

    public void setBlock(int block) {
        this.block = block;
    }

    /**
     * Sets the value of the 'final' property, indicating which
     * types of derivation are not allowed.
     * For internal use only.
     *
     * @param finalValue the bit-significant code representing the final property,
     *                   made up of values such as {@link Derivation#DERIVE_BY_SUBSTITUTION}
     */

    public void setFinalProhibitions(int finalValue) {
        finalProhibitions = finalValue;
    }

    /**
     * Set the value constraint (fixed or default value) that applies to this element
     * declaration
     * @param vc the value constraint
     */

    @Override
    public void setValueConstraint(ValueConstraint vc) {
        this.valueConstraint = vc;
    }

    /**
     * Sets whether or not instances of this element definition may
     * set xsi:nil='true'. For internal use only.
     *
     * @param nillable the flag when true indicates that instances
     *                 of this element definition may be nilled
     */

    public void setNillable(boolean nillable) {
        this.nillable = nillable;
    }

    /**
     * Set the name of this element declaration
     *
     * @param elementName the element name
     */

    public void setElementName(StructuredQName elementName, int fingerprint) {
        this.elementName = elementName;
        this.fingerprint = fingerprint;
        this.targetNamespace = elementName.getURI();
        substitutionGroupMembers.add(this);
    }

    public void setElementName(StructuredQName elementName) {
        this.elementName = elementName;
        this.fingerprint = getConfiguration().getNamePool().allocateFingerprint(elementName.getURI(), elementName.getLocalPart());
        this.targetNamespace = elementName.getURI();
        substitutionGroupMembers.add(this);
    }

    @Override
    public StructuredQName getComponentName() {
        return elementName;
    }

    /**
     * Get the fingerprint of the name of this element declaration in the namePool
     *
     * @return the fingerprint of the element name
     */

    @Override
    public int getFingerprint() {
        return fingerprint;
    }


    /**
     * Adds the name of the head of a
     * substitutionGroup for this element definition.
     * For internal use only.
     *
     * @param qName the name of the head of the
     *                 substitutionGroup of this element definition.
     */

    public void addSubstitutionGroupHead(StructuredQName qName) {
        if (substitutionGroupOwners == null) {
            substitutionGroupOwners = new ArrayList<>(2);
        }
        SimpleComponentReference headRef = new SimpleComponentReference(SymbolSpace.ELEMENT, qName, getConfiguration());
        headRef.setLocator(this);
        substitutionGroupOwners.add(headRef);
    }

    /**
     * Sets the schema type for this element declaration.
     * For internal use only.
     *
     * @param type the schema type for this element declaration.
     */

    public void setTypeReference(TypeReference type) {
        typeReference = type;
    }

    /**
     * Add a type alternative to the type alternative table. Note that the order in which alternatives
     * are added is significant.
     *
     * @param alternative the type alternative to be added
     */

    public void addTypeAlternative(TypeAlternative alternative) {
        if (typeAlternatives == null) {
            typeAlternatives = new ArrayList<>(5);
        }
        typeAlternatives.add(alternative);
    }

    /**
     * Ask whether any type alternatives are defined for this element
     *
     * @return true if type alternatives have been defined
     */

    @Override
    public boolean hasTypeAlternatives() {
        return typeAlternatives != null;
    }

    /**
     * Get the type alternatives defined for this element. This should be called
     * only after callling hasTypeAlternatives() to check that there are any.
     *
     * @return an ordered list of the type alternatives defined for this element declaration.
     */

    public List<TypeAlternative> getTypeAlternatives() {
        return typeAlternatives;
    }

    /**
     * This method is called to look for cycles. The object implementing this
     * method is required (a) to raise an exception if the object itself appears
     * in the list of references, (b) to add itself to the list of references, and
     * (c) to call the lookForCycles method on all the objects that it references.
     * <p>In this particular case the cycles we are looking for are cycles in substitution
     * group membership.</p>
     *
     * @param references A list of objects that contain direct or indirect references
     *                   to this object, and that must therefore not be referred to from this object.
     * @param compiler   used for error reporting
     * @throws SchemaException if cycles are found
     */

    @Override
    public void lookForCycles(Stack<SchemaComponent> references, SchemaCompiler compiler) throws SchemaException {
        for (Object reference : references) {
            ElementDecl member = (ElementDecl) reference;
            if (member == this) {
                compiler.error("Element declaration " + getDisplayName() +
                        " participates in a substitution group whose definition is circular", this);
                throw new SchemaException("Circular definition found");
            }
            addSubstitutionGroupMember(member, compiler);
        }
        if (substitutionGroupOwners != null) {
            references.push(this);
            for (ComponentReference headRef : substitutionGroupOwners) {
                headRef.tryToResolve(compiler, false);
                if (headRef.isResolved()) {
                    ElementDecl head = (ElementDecl) headRef.getTarget();
                    head.lookForCycles(references, compiler);
                }
            }
            references.pop();
        }
    }


    /**
     * Check references from this component to other components
     *
     * @param compiler used for error reporting
     * @return true if all is well, false if errors found
     */

    @Override
    public boolean fixup(SchemaCompiler compiler) throws SchemaException {
        boolean result = true;
        if (getFixupStatus() == UNVALIDATED) {
            setFixupStatus(VALIDATING);
            if (substitutionGroupOwners != null) {
                for (ComponentReference headRef : substitutionGroupOwners) {
                    ElementDecl decl = (ElementDecl) PreparedSchema.validateReference(headRef, compiler, false);
                    if (decl == null) {
                        setFixupStatus(INVALID);
                        substitutionGroupOwners = null; // for recovery, remove the element from the substitution group
                        return false;
                    }
                    result = decl.fixup(compiler);
                }
            }
            if (typeReference != null && !typeReference.isDangling()) {
                SchemaType type;
                try {
                    type = (SchemaType) PreparedSchema.validateReference(typeReference, compiler, false);
                } catch (MissingComponentException e) {
                    type = null;
                }
                if (type == null) {
                    setFixupStatus(INVALID);
                    typeReference = new TypeReference(StandardNames.XS_ERROR, getConfiguration(),this);
                    return false;
                }
                if (type instanceof UserSchemaComponent) {
                    result &= ((UserSchemaComponent) type).fixup(compiler);
                }
            }
            if (hasTypeAlternatives()) {
                for (TypeAlternative ta : getTypeAlternatives()) {
                    result &= ta.fixup(compiler);
                }
            }
            for (IdentityConstraintReference icr : constraintRefs) {
                icr.tryToResolve(compiler, false);
            }
        }
        setFixupStatus(result ? VALIDATED : INVALID);
        return result;
    }

    /**
     * Create a NodeTest that implements the semantics of schema-element(name) or
     * schema-attribute(name) applied to this element or attribute declaration.
     * @throws MissingComponentException if the type of the declaration is not present in the schema.
     */

    @Override
    public NodeTest makeSchemaNodeTest() throws MissingComponentException {
        return new SchemaElementTest(this);
    }

    /**
     * Check the validity (consistency) of this element definition.
     *
     * @param compiler used for error reporting
     * @return true if validation succeeded, false if errors were found.
     * @throws SchemaException if an error is found and the
     *                         error listener decides that the error is fatal
     */

    @Override
    public boolean validate(SchemaCompiler compiler) throws SchemaException {
        // System.err.println("Validate " + getName());
        // check for circularities and for previous validation

        // check for previous validation
        if (!isValidationNeeded()) {
            return true;
        }
        setValidationStatus(VALIDATING);

        boolean result = true;
        boolean checkFirstSubstitutionGroup = true;

        if (typeReference != null) {
            typeReference.tryToResolve(compiler, false);
        } else {
            if (substitutionGroupOwners != null) {
                // if the element has no type definition but is a member of a substitution group,
                // default the type information from the head of the first substitution group.
                ElementDecl head = (ElementDecl) PreparedSchema.validateReference(substitutionGroupOwners.get(0), compiler, false);
                if (head != null) {
                    SchemaType headType = head.getType();
                    typeReference = new TypeReference(headType.getFingerprint(), getConfiguration(), this);
                    typeReference.setTarget(headType);
                    checkFirstSubstitutionGroup = false;
                } else {
                    typeReference = new TypeReference(AnyType.getInstance().getFingerprint(), getConfiguration(), this);
                    typeReference.setTarget(AnyType.getInstance());
                    checkFirstSubstitutionGroup = false;
                }
            }
        }
        if (typeReference != null) {
            PreparedSchema.validateReference(typeReference, compiler, false);
            if (!typeReference.isResolved()) {
                setValidationStatus(INVALID);
                return true;
            }
        }
        SchemaType type = getType();

        //-- If type is anonymous, make sure the type is valid.

        if (type != null) {
            if (type.isComplexType()) {
                if (type instanceof UserComplexType) {
                    result = ((UserComplexType) type).validate(compiler);
                } else {
                    // it must be AnyType, which is OK
                }
            } else if (type.isSimpleType()) {
                SimpleType simpleType = (SimpleType) type;
                if (simpleType instanceof UserSimpleType) {
                    result = ((UserSimpleType) simpleType).validate(compiler);
                }
                if (result &&
                        compiler.getLanguageVersion() == Configuration.XSD10 &&
                        getValueConstraint() != null &&
                        simpleType.getBuiltInBaseType().getFingerprint() == StandardNames.XS_ID) {
                    compiler.error("An element of type xs:ID must have no fixed or default value", this);
                    result = false;
                }
            }

            if (getFixedValue() != null) {
                result &= validateSimpleValue(type, "fixed", compiler);
            }
            if (getDefaultValueLexicalForm() != null) {
                // Schema Component Constraint: Element Default Valid (Immediate)
                result &= validateSimpleValue(type, "default", compiler);
            }

            // If the element is in any substitution groups, check that its type is
            // consistent with the head of each such substitution group

            boolean first = true;
            if (substitutionGroupOwners != null && checkFirstSubstitutionGroup) {
                for (ComponentReference headRef : substitutionGroupOwners) {
                    if (checkFirstSubstitutionGroup || !first) {
                        ElementDecl head = (ElementDecl) PreparedSchema.validateReference(headRef, compiler, false);
                        if (head != null) {
                            SchemaType headType = head.getType();
                            try {
                                compiler.getConfiguration().checkTypeDerivationIsOK(type, headType, head.finalProhibitions);
                            } catch (SchemaException err) {
                                compiler.error("Element " + getDisplayName() + " cannot be in the substitution group of " +
                                    head.getDisplayName() + ". " + err.getMessage(), this);
                                result = false;
                            }
                        }
                    }
                    if (compiler.getLanguageVersion() == Configuration.XSD10 && !first) {
                        compiler.error("In XML Schema 1.0, an element can participate in one substitution group only", this);
                    }
                    first = false;
                }
            }

            for (IdentityConstraintReference constraintRef : constraintRefs) {
                compiler.addPendingTypeCheck(constraintRef, type);
            }

        }

        // If there is a table of type alternatives, ensure that they are all consistent with the base type

        if (hasTypeAlternatives()) {
            for (TypeAlternative ta : getTypeAlternatives()) {
                SchemaType altType = ta.getSchemaType();
                if (altType instanceof UserDefinedType) {
                    result &= ((UserSchemaComponent) altType).validate(compiler);
                }
                if (altType != ErrorType.getInstance()) {
                    try {
                        compiler.getConfiguration().checkTypeDerivationIsOK(altType, type, finalProhibitions);
                    } catch (SchemaException err) {
                        compiler.error("Invalid alternative type. " + err.getMessage(), ta);
                        result = false;
                    }
                }
            }
        }

        for (IdentityConstraintReference icr : constraintRefs) {
            icr.tryToResolve(compiler, false);
            if (!icr.isResolved()) {
                compiler.error("Cannot resolve integrity constraint reference " +
                        icr.getTargetComponentName().getDisplayName(), this);
                return false;
            }
            IdentityConstraint ic = icr.getTarget();
            switch (icr.getExpectedKindOfConstraint()) {
                case StandardNames.XS_UNIQUE:
                    if (!(ic instanceof Unique)) {
                        compiler.error("Target of xs:unique reference must be an xs:unique element", icr);
                        result = false;
                    }
                    break;
                case StandardNames.XS_KEY:
                    if (!(ic instanceof Key)) {
                        compiler.error("Target of xs:key reference must be an xs:key element", icr);
                        result = false;
                    }
                    break;
                case StandardNames.XS_KEYREF:
                    if (!(ic instanceof KeyRef)) {
                        compiler.error("Target of xs:keyref reference must be an xs:keyref element", icr);
                        result = false;
                    }
                    break;
            }
        }

        setValidationStatus(result ? VALIDATED : INVALID);
        return result;

    }

    /**
     * Test whether this element declaration is validly substitutable for another element declaration,
     * given a blocking constraint. In XSD 1.1 this is referred to as "Substitution Group OK (Transitive)"
     * (see section 3.3.6.3)
     *
     * @param other the other element declaration
     * @return null if substitution is possible, or a string explaining why not
     */

    private String isValidlySubstitutable(ElementDecl other) throws MissingComponentException {
        int block = other.getBlock();
        // rule 1
        if (this == other) {
            return null;
        }
        // rule 2.1
        if ((block & Derivation.DERIVE_BY_SUBSTITUTION) != 0) {
            return "substitution has been blocked";
        }
        // rule 2.2
        boolean found = false;
        LinkedList<ElementDecl> unsearched = new LinkedList<>();
        unsearched.add(this);
        while (!unsearched.isEmpty()) {
            // NOTE: relies on the fact that we have already detected any cycles
            if (unsearched.contains(other)) {
                found = true;
                break;
            }
            ElementDecl first = unsearched.pollFirst();
            //unsearched.remove(first);
            unsearched.addAll(first.getSubstitutionGroupOwners());
        }
        if (!found) {
            return "element declaration is not a member of the substitution group";
        }

        // rule 2.3
        int actualDerivations = 0;
        int prohibitedDerivations = block;
        SchemaType t = getType();
        SchemaType ot = other.getType();
        if (ot == null) {
            ot = AnyType.getInstance();
        }
        while (t != null && t != AnyType.getInstance() && !t.isSameType(ot)) {
            actualDerivations |= t.getDerivationMethod();
            t = t.getBaseType();
            prohibitedDerivations |= t.getBlock();
        }
        if ((actualDerivations & prohibitedDerivations) != 0) {
            return "required type derivation method (extension or restriction) has been blocked";
        }

        return null;
    }

    /**
     * Check that the supplied default or fixed value for the element is OK for its type
     *
     * @param type     the simple type of the content, which must have been validated
     * @param property "fixed" or "default", defines the value being checked
     * @param compiler used for error reporting
     * @return true if successful
     * @throws SchemaException if the errorListener throws an exception
     */

    private boolean validateSimpleValue(SchemaType type, String property, SchemaCompiler compiler)
            throws SchemaException {
        SimpleType simpleType;
        if (type.isSimpleType()) {
            simpleType = (SimpleType) type;
        } else if (((ComplexType) type).isSimpleContent()) {
            simpleType = ((ComplexType) type).getSimpleContentType();
        } else if (((ComplexType) type).isMixedContent()) {
            if (!((ComplexType) type).isEmptiable()) {
                compiler.error("An element with mixed content may have a " + property +
                        " value only if its type is emptiable", this);
                return false;
            }
            simpleType = BuiltInAtomicType.STRING;
        } else {
            compiler.error("An element with a " + property + " value must have simple or mixed content", this);
            return false;
        }
        assert simpleType != null;

        if (simpleType instanceof SimpleTypeDefinition) {
            boolean result = ((SimpleTypeDefinition) simpleType).validate(compiler);
            if (!result) {
                return false;
            }
            simpleType = ((SimpleTypeDefinition) simpleType).getWorkingType();
        }

        if (valueConstraint != null) {
            return valueConstraint.makeTypedValue(compiler, simpleType, getNamespaceResolver(), this);
        }

        return true;
    }

    /**
     * Fix up the reference(s) to the head(s) of the substitition group(s)
     *
     * @param compiler used for error reporting
     * @throws SchemaException if any error occurs, for example a name that is referenced
     *                         but not declared
     */
    protected void fixupSubstitutionGroup(SchemaCompiler compiler) throws SchemaException {
        // Check that an element declaration is available for the head(s) of the
        // substitution group(s). We don't actually add to the substitution group membership
        // at this stage

        if (substitutionGroupOwners != null) {
            for (ComponentReference headRef : substitutionGroupOwners) {
                ElementDecl head = (ElementDecl) PreparedSchema.validateReference(headRef, compiler, false);
                if (head == null) {
                    String err = "Element " +
                            Err.wrap(headRef.getTargetComponentName().getDisplayName(), Err.ELEMENT) +
                            " is referenced as head of a substitution group for " +
                            Err.wrap(getDisplayName()) +
                            ", but is not declared in the schema";
                    compiler.warning(err, this);
                }
            }
            lookForCycles(new Stack<SchemaComponent>(), compiler);
        }
    }

    /**
     * Test whether this is the same declaration as another declaration.
     * They are considered to be the same declaration
     * if they are derived from the same definition in the original XML representation (which
     * can happen when there are multiple includes of the same file)
     *
     * @param other the declaration with which to compare this one
     * @return true if the two declarations are the same
     */

    public boolean isSameDeclaration(ElementDecl other) {
        // The logic is complicated by the fact that both element declarations might be generated by a
        // transformation (chameleon or override) which makes the system Id useless. So we test for the path
        // within the schema document being the same, and all properties being the same
        if (this == other) {
            return true;
        }
        if (getFingerprint() != other.getFingerprint()) {
            return false;
        }
        if (!generatedId.equals(other.generatedId)) {
            return false;
        }
        if (getBlock() != other.getBlock()) {
            return false;
        }
        if (getSubstitutionGroupExclusions() != other.getSubstitutionGroupExclusions()) {
            return false;
        }
        if (typeReference != null && other.typeReference != null) {
            if (typeReference.isAnonymousType()) {
                return other.typeReference.isAnonymousType();
            } else {
                return typeReference.getTargetFingerprint() == other.typeReference.getTargetFingerprint();
            }
        }
        return true;
    }

    /**
     * Test whether this element declaration has "the same" type table as another element
     * declaration
     *
     * @param other the other element declaration
     * @return true if the type tables are the same, false if they are different
     */

    public boolean hasSameTypeTable(ElementDecl other) throws MissingComponentException {
        if (this.isSameDeclaration(other)) {
            return true;
        }
        if (typeAlternatives == null && other.typeAlternatives == null) {
            return true;
        }
        if (typeAlternatives == null || other.typeAlternatives == null) {
            return false;
        }
        if (typeAlternatives.size() != other.typeAlternatives.size()) {
            return false;
        }
        if (!this.getType().isSameType(other.getType())) {
            return false;
        }
        for (int i = 0; i < typeAlternatives.size(); i++) {
            TypeAlternative a0 = this.typeAlternatives.get(i);
            TypeAlternative a1 = other.typeAlternatives.get(i);
            XPathExpression c0 = a0.getCondition();
            XPathExpression c1 = a1.getCondition();
            if ((c0 == null) != (c1 == null)) {
                return false;
            }
            if (c0 != null && !c0.getInternalExpression().isEqual(c1.getInternalExpression())) {
                return false;
            }
            if (!a0.getDefaultElementNamespace().equals(a1.getDefaultElementNamespace())) {
                return false;
            }
            if (!a0.getBaseURI().equals(a1.getBaseURI())) {
                return false;
            }
            if (!a0.getSchemaType().isSameType(a1.getSchemaType())) {
                return false;
            }
            Set<String> prefixes0 = new HashSet<>();
            for (Iterator<String> it = a0.getNamespaceContext().iteratePrefixes(); it.hasNext(); ) {
                prefixes0.add(it.next());
            }
            for (Iterator<String> it = a1.getNamespaceContext().iteratePrefixes(); it.hasNext(); ) {
                String prefix = it.next();
                if (!prefixes0.contains(prefix)) {
                    return false;
                }
            }
            for (String p : prefixes0) {
                if (!a0.getNamespaceContext().getURIForPrefix(p, false).equals(a1.getNamespaceContext().getURIForPrefix(p, false))) {
                    return false;
                }
            }
        }
        return true;
    }

    /*@Nullable*/
    public String toString() {
        return "ElementDeclaration " + getName();
    }

    /**
     * Serialize the schema component
     */

    @Override
    public void serialize(SchemaModelSerializer serializer) throws XPathException {
        String id = serializer.getId(this, true);
        serializer.startElement("element");
        serializer.emitAttribute("id", id);
        if (getName() != null) {
            serializer.emitAttribute("name", getName());
            if (!getTargetNamespace().isEmpty()) {
                serializer.emitAttribute("targetNamespace", getTargetNamespace().toString());
            }
        }

        serializer.emitAttribute("type", serializer.getTypeLink(getType()));

        serializer.emitAttribute("global", isGlobal() ? "true" : "false");
        if (!isGlobal() && getContainingComplexType() != null) {
            serializer.emitAttribute("containingComplexType", serializer.getId(getContainingComplexType(), false));
        }
        if (getDefaultValueLexicalForm() != null) { // for backwards compatibility
            serializer.emitAttribute("default", getDefaultValueLexicalForm().toString());
        }
        serializer.emitAttribute("nillable", isNillable() ? "true" : "false");

        if (finalProhibitions != 0) {
            String ex = "";
            if ((finalProhibitions & Derivation.DERIVATION_EXTENSION) != 0) {
                ex = "extension";
            }
            if ((finalProhibitions & Derivation.DERIVATION_RESTRICTION) != 0) {
                ex += (ex.isEmpty() ? "" : " ") + "restriction";
            }
            serializer.emitAttribute("final", ex);
        }
        if (block != 0) {
            String dis = "";
            if ((block & Derivation.DERIVATION_EXTENSION) != 0) {
                dis = "extension";
            }
            if ((block & Derivation.DERIVATION_RESTRICTION) != 0) {
                dis += (dis.isEmpty() ? "" : " ") + "restriction";
            }
            if ((block & Derivation.DERIVE_BY_SUBSTITUTION) != 0) {
                dis += (dis.isEmpty() ? "" : " ") + "substitution";
            }
            serializer.emitAttribute("block", dis);
        }
        serializer.emitAttribute("abstract", isAbstractDecl ? "true" : "false");

        if (substitutionGroupOwners != null) {
            for (ComponentReference headRef : substitutionGroupOwners) {
                serializer.startElement("substitutionGroupAffiliation");
                serializer.emitAttribute("ref", serializer.getId(headRef.getTarget(), false));
                serializer.endElement();
            }
        }

        if (hasTypeAlternatives()) {
            for (TypeAlternative alt : getTypeAlternatives()) {
                alt.serialize(serializer);
            }
        }

        for (IdentityConstraintReference icr : getIdentityConstraints()) {
            serializer.startElement("identityConstraint");
            serializer.emitAttribute("ref", serializer.getId(icr.getTarget(), false));
            serializer.endElement();
        }

        if (getValueConstraint() != null) {
            getValueConstraint().serialize(serializer);
        }
        serializer.endElement();
    }

    @Override
    public Function getComponentAsFunction() {
        ElementDecl thisDecl = this;
        CallableDelegate callable = new CallableDelegate((context, arguments) -> {
            String key = arguments[0].head().getStringValue();
            switch (key) {
                case "class":
                    return StringValue.bmp("Element Declaration");
                case "implementation":
                    return new ObjectValue<>(thisDecl);
                case "name":
                    return new StringValue(getName(), BuiltInAtomicType.NCNAME);
                case "target namespace":
                    return new AnyURIValue(getTargetNamespace().toUnicodeString());
                case "type definition":
                    return getType().getComponentAsFunction();
                case "type table":
                    if (hasTypeAlternatives()) {
                        CallableDelegate typeTable = new CallableDelegate((context1, arguments1) -> {
                            String key1 = arguments1[0].head().getStringValue();
                            switch (key1) {
                                case "class":
                                    return StringValue.bmp("Type Table");
                                case "alternatives":
                                    List<Item> list = new ArrayList<>();
                                    for (TypeAlternative ta : getTypeAlternatives()) {
                                        list.add(ta.getComponentAsFunction());
                                    }
                                    return SequenceExtent.makeSequenceExtent(list);
                                case "default type definition":
                                    return typeAlternatives.get(typeAlternatives.size() - 1).getComponentAsFunction();
                                default:
                                    return EmptySequence.getInstance();
                            }
                        });
                        return new CallableFunction(1, typeTable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
                    } else {
                        return EmptySequence.getInstance();
                    }
                case "scope":
                    CallableDelegate scopeFunction = new CallableDelegate((context12, arguments12) -> {
                        String key12 = ((StringValue) arguments12[0].head()).getStringValue();
                        switch (key12) {
                            case "class":
                                return StringValue.bmp("Scope");
                            case "variety":
                                return StringValue.bmp(isGlobal() ? "global" : "local");
                            case "parent":
                                if (isGlobal()) {
                                    return EmptySequence.getInstance();
                                } else if (getContainingComplexType() != null) {
                                    return getContainingComplexType().getComponentAsFunction();
                                } else if (getContainingModelGroup() != null) {
                                    return getContainingModelGroup().getComponentAsFunction();
                                } else {
                                    return EmptySequence.getInstance();
                                }
                            default:
                                return EmptySequence.getInstance();
                        }
                    });
                    return new CallableFunction(1, scopeFunction, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
                case "value constraint":
                    return getValueConstraintAsFunction(ElementDecl.this);
                case "nillable":
                    return BooleanValue.get(isNillable());
                case "identity-constraint definitions":
                    List<Item> constraints = new ArrayList<>();
                    for (IdentityConstraintReference ic : getIdentityConstraints()) {
                        constraints.add(ic.getTarget().getComponentAsFunction());
                    }
                    return SequenceExtent.makeSequenceExtent(constraints);
                case "substitution group affiliations":
                    List<Item> groups = new ArrayList<>();
                    for (ElementDecl decl : getSubstitutionGroupOwners()) {
                        groups.add(decl.getComponentAsFunction());
                    }
                    return SequenceExtent.makeSequenceExtent(groups);
                case "substitution group exclusions": {
                    List<AtomicValue> exclusions = new ArrayList<>();
                    if ((getSubstitutionGroupExclusions() & Derivation.DERIVATION_EXTENSION) != 0) {
                        exclusions.add(StringValue.bmp("extension"));
                    }
                    if ((getSubstitutionGroupExclusions() & Derivation.DERIVATION_RESTRICTION) != 0) {
                        exclusions.add(StringValue.bmp("restriction"));
                    }
                    return new AtomicArray(exclusions);
                }
                case "disallowed substitutions": {
                    List<AtomicValue> exclusions = new ArrayList<>();
                    if ((getBlock() & Derivation.DERIVATION_EXTENSION) != 0) {
                        exclusions.add(StringValue.bmp("extension"));
                    }
                    if ((getBlock() & Derivation.DERIVATION_RESTRICTION) != 0) {
                        exclusions.add(StringValue.bmp("restriction"));
                    }
                    if ((getBlock() & Derivation.DERIVE_BY_SUBSTITUTION) != 0) {
                        exclusions.add(StringValue.bmp("substitution"));
                    }
                    return new AtomicArray(exclusions);
                }
                case "abstract":
                    return BooleanValue.get(isAbstract());
                default:
                    return EmptySequence.getInstance();
            }
        });
        return new CallableFunction(1, callable, SpecificFunctionType.COMPONENT_FUNCTION_TYPE);
    }

    public boolean equals(Object other) {
        if (!(other instanceof ElementDecl)) {
            return false;
        }
        ElementDecl e2 = (ElementDecl) other;
        return getFingerprint() == e2.getFingerprint() &&
                getTypeFingerprint() == e2.getTypeFingerprint() &&
                getBlock() == e2.getBlock() &&
                getSubstitutionGroupExclusions() == e2.getSubstitutionGroupExclusions() &&
                (getFixedValueLexicalForm() == null || getFixedValueLexicalForm().equals(e2.getFixedValueLexicalForm()));
        // that will do for now - it's probabilistic, but adequate

    }

    public int hashCode() {
        return getFingerprint() ^
                (getTypeFingerprint() << 10);
    }

}

// Copyright (c) 2004-2022 Saxonica Limited
