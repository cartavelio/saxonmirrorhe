package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.sdoc.SchemaDocument;
import com.saxonica.ee.schema.sdoc.SchemaReader;
import com.saxonica.ee.schema.sdoc.SimpleTypeDefinition;
import com.saxonica.ee.validate.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.instruct.DummyNamespaceResolver;
import net.sf.saxon.expr.instruct.GlobalParam;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.*;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.type.*;
import net.sf.saxon.z.IntHashMap;
import net.sf.saxon.z.IntIterator;

import javax.xml.transform.Source;
import javax.xml.transform.SourceLocator;
import javax.xml.transform.sax.SAXSource;
import java.util.*;

import static net.sf.saxon.Configuration.XSD11;


/**
 * A schema is a collection of schema components. It may be derived from a single schema
 * document (an xs:schema element), in which case it contains those components defined in that
 * schema document, the components defined in the included documents (expanded
 * transitively), and the components defined in imported schema documents (not
 * expanded transitively). Alternatively, it may be a collection of schema components
 * derived from multiple independent schema documents.
 *
 * <p>All components in the schema are indexed by name. In addition, elements, attributes,
 * and types are indexed by their integer NamePool fingerprint.</p>
 *
 * <p>Construction of the PreparedSchema is not thread-safe; all creation of new components
 * must happen in a single thread.</p>
 */

public class PreparedSchema implements Schema, NotationSet {

    public static final int NAMESPACE_UNKNOWN = 1;
    public static final int NAMESPACE_UNDER_CONSTRUCTION = 2;
    public static final int NAMESPACE_KNOWN = 3;
    public static final int NAMESPACE_SEALED = 4;

    /**
     * The system configuration
     */

    private EnterpriseConfiguration config;

    /**
     * The set of all target namespaces of components in this schema
     */

    private final Set<NamespaceUri> targetNamespaces = new HashSet<>();

    private final HashMap<NamespaceUri, Integer> namespaceStatus = new HashMap<>();

    /**
     * The global attribute declarations for this Schema, indexed by their name
     */

    private Map<StructuredQName, AttributeDecl> attributesByName = null;

    /**
     * The global attribute declarations for this Schema, indexed by their fingerprint
     */

    private IntHashMap<AttributeDecl> attributesByFingerprint = null;


    /**
     * The global attribute group declarations for this Schema, indexed by their fingerprint
     */

    private Map<StructuredQName, AttributeGroupDecl> attributeGroups = null;

    /**
     * The global group declarations for this Schema, indexed by their fingerprint
     */

    private Map<StructuredQName, ModelGroupDefinition> groups = null;

    /**
     * A table of defined and named simple and complex types, indexed by name
     */

    private Map<StructuredQName, SchemaType> typesByName = null;

    /**
     * A table of anonymous (unnamed) types defined within the schema, indexed by name.
     * These cannot be referenced from any schema components, but they can appear as type annotations
     * in a document instance
     */

    private Map<StructuredQName, SchemaType> anonymousTypesByName = null;

    /**
     * A table of global element declarations, indexed by name
     */
    private Map<StructuredQName, ElementDecl> elementsByName = null;

    /**
     * A table of global element declarations, indexed by fingerprint
     */
    private IntHashMap<ElementDecl> elementsByFingerprint = null;

    /**
     * Index of identity constraints (indexed by their name)
     */

    private Map<StructuredQName, IdentityConstraint> identityConstraints;

    /**
     * Index of xs:notation declarations
     */

    /*@Nullable*/
    private Map<StructuredQName, Notation> notations;

    /**
     * Saxon NamePool used to hold the names in this schema
     */

    private NamePool namePool;

    /**
     * List of components awaiting fixup and validation
     */

    private transient List<UserSchemaComponent> components;

    /**
     * Index of complex types in this schema that extend built-in types, including types that
     * extend user-defined simple types that restrict built-in types
     * Key is the fingerprint of a built-in type; value is a Set of UserComplexType objects
     */

    private IntHashMap<Set<UserComplexType>> extensionsOfBuiltInTypes;

    /**
     * Map of global parameters declared using the saxon:param extension, indexed by the parameter name
     */

    private HashMap<StructuredQName, GlobalParam> params;

    /**
     * Identifier of the local license key, if this schema was loaded from a compiled schema
     * that contained its own license
     */

    private int localLicenseId = -1;

    /**
     * Create a dummy schema definition. This constructor is defined only
     * for use by subclasses
     */

    protected PreparedSchema() {
    }

    /**
     * Create a new Schema definition
     *
     * @param config The configuration object holding system configuration information
     */

    public PreparedSchema(EnterpriseConfiguration config) {
        namePool = config.getNamePool();
        this.config = config;

        attributesByName = new HashMap<>(10);
        attributesByFingerprint = new IntHashMap<>(10);
        attributeGroups = new HashMap<>(20);
        groups = new HashMap<>(20);
        typesByName = new HashMap<>(100);
        elementsByName = new HashMap<>(100);
        elementsByFingerprint = new IntHashMap<>(100);
        anonymousTypesByName = new HashMap<>(100);
        params = new HashMap<>(10);
    }

    /**
     * Set the configuration
     */

    public void setConfiguration(EnterpriseConfiguration config) {
        this.config = config;
    }

    /**
     * Get the configuration
     */

    public EnterpriseConfiguration getConfiguration() {
        return config;
    }

    /**
     * Set the local license id, identifying any local license in the case
     * where this package was loaded from a compiled package that contained its own
     * embedded license
     *
     * @param id identifier used to distinguish this local license
     */
    public void setLocalLicenseId(int id) {
        localLicenseId = id;
    }

    /**
     * Get the local license id, identifying any local license in the case
     * where this package was loaded from a compiled package that contained its own
     * embedded license
     *
     * @return integer identifying the local license, or -1 if there is none
     */

    public int getLocalLicenseId() {
        return localLicenseId;
    }

    /**
     * Add an object that might contain a forwards reference to be fixed up later
     *
     * @param obj the component to be added. The caller should ensure that the same component
     *            is not added twice.
     */

    public void addComponent(UserSchemaComponent obj) {
        if (components == null) {
            components = new ArrayList<>(20);
        }
        components.add(obj);
    }

    /**
     * Fix up references and check for circularities
     *
     * @param compiler used for error reporting
     * @throws net.sf.saxon.type.SchemaException
     *          if a fatal error is detected
     */

    private void fixupComponentReferences(SchemaCompiler compiler)
            throws SchemaException {
        int errors = 0;
        if (components != null) {
            for (UserSchemaComponent component : components) {
                try {
                    //System.err.println("Fixing up " + component.toString());
                    boolean b = component.fixup(compiler);
                    if (!b && !compiler.isAllowDanglingReferences()) {
                        errors++;
                    }
                } catch (SchemaException err) {
                    errors++;
                    compiler.getErrorReporter().report(new XmlProcessingException(err));
                }
            }
            if (errors > 0) {
                throw new SchemaException("Errors were found in the schema");
            }
        }
    }

    private boolean isBuiltInNamespace(NamespaceUri namespace) {
        return namespace.equals(NamespaceUri.XML) ||
                namespace.equals(NamespaceUri.FN) ||
                namespace.equals(NamespaceUri.SCHEMA_INSTANCE);
    }

    /**
     * Get the attribute declaration with a given name if there is one.
     *
     * @param name the name of the required attribute declaration
     * @return the attribute declaration if found, or null otherwise
     */

    public AttributeDecl getAttributeDecl(StructuredQName name) {
        AttributeDecl decl = attributesByName.get(name);
        if (decl == null && isBuiltInNamespace(name.getNamespaceUri())) {
            config.addSchemaForBuiltInNamespace(name.getNamespaceUri());
            decl = attributesByName.get(name);
        }
        return decl;
    }

    /**
     * Get the attribute declaration with a given name if there is one.
     *
     * @param fingerprint the NamePool fingerprint of the name of the required attribute declaration
     * @return the attribute declaration if found, or null otherwise
     */

    public AttributeDecl getAttributeDecl(int fingerprint) {
        return attributesByFingerprint.get(fingerprint);
    }


    /**
     * Get all the global attribute declarations defined in this schema
     *
     * @return an iterable collection of attribute declarations. Each item returned by
     *         the iterator will be an instance of {@link AttributeDecl}
     */

    public Iterable<AttributeDecl> getAttributeDeclarations() {
        return attributesByName.values();
    }

    /**
     * Adds the given global attribute declaration to this Schema. It will overwrite
     * any existing attribute declaration with the same name.
     *
     * @param attribute the AttributeDecl to add
     */

    public void addAttributeDecl(AttributeDecl attribute) {
        AttributeDecl oldDecl = attributesByName.put(attribute.getComponentName(), attribute);
        // index it within this schema by its name
        if (oldDecl != attribute) {
            // register it for fixup and validation
            attributesByFingerprint.put(attribute.getFingerprint(), attribute);
            addComponent(attribute);
            targetNamespaces.add(attribute.getTargetNamespace());
        }
    }

    /**
     * Get the attribute declaration with a given name if there is one.
     *
     * @param name the name of the required attribute group
     * @return the attribute declaration if found, or null otherwise
     */

    public AttributeGroupDecl getAttributeGroup(StructuredQName name) {
        return attributeGroups.get(name);
    }

    /**
     * Get an iterator over all the named attribute groups defined in this schema
     *
     * @return an iterator over the attribute groups. Each item returned by
     *         the iterator will be an instance of {@link AttributeGroupDecl}
     */

    public Iterable<AttributeGroupDecl> getAllAttributeGroups() {
        return attributeGroups.values();
    }

    /**
     * Adds the given attribute group declaration to this Schema. It will overwrite
     * any existing attribute group declaration with the same name, unless the existing
     * attribute has a higher redefinition level.
     *
     * @param attributeGroup the AttributeDecl to add
     */

    public void addAttributeGroup(AttributeGroupDecl attributeGroup) {

        AttributeGroupDecl existing = attributeGroups.get(attributeGroup.getAttributeGroupName());
        if (existing == null || existing.getRedefinitionLevel() <= attributeGroup.getRedefinitionLevel()) {
            attributeGroups.put(attributeGroup.getAttributeGroupName(), attributeGroup);
            if (existing != attributeGroup) {
                // register it for fixup and validation
                addComponent(attributeGroup);
                targetNamespaces.add(attributeGroup.getTargetNamespace());
            }
        }
    }

    /**
     * Get the model group with a given name if there is one.
     *
     * @param name the name of the required named model group
     * @return the group if found, or null otherwise
     */

    public ModelGroupDefinition getGroup(StructuredQName name) {
        return groups.get(name);
    }

    /**
     * Get an iterator over all the named model groups defined in this schema
     *
     * @return an iterator over the model groups. Each item returned by
     *         the iterator will be an instance of {@link ModelGroupDefinition}
     */

    public Iterable<ModelGroupDefinition> getAllModelGroups() {
        return groups.values();
    }

    /**
     * Adds the given group (xs:group) to this Schema. It will overwrite
     * any existing group with the same name.
     *
     * @param group the group to add
     */

    public void addGroup(ModelGroupDefinition group) {

        // index it within this schema by its fingerprint
        ModelGroupDefinition existing = groups.get(group.getModelGroupName());
        if (existing == null || existing.getRedefinitionLevel() <= group.getRedefinitionLevel()) {
            groups.put(group.getModelGroupName(), group);
            if (existing != group) {
                // register it for fixup and validation
                addComponent(group);
                targetNamespaces.add(group.getTargetNamespace());
            }
        }
    }

    /**
     * Get the type with a given name if there is one. This method will also
     * find built-in types.
     *
     * @param name the name of the required schema type
     * @return the named simple or complex type if found, or null otherwise
     */

    public SchemaType getSchemaType(StructuredQName name) {
        if (name.hasURI(NamespaceUri.SCHEMA)) {
            SchemaType type = BuiltInType.getSchemaTypeByLocalName(name.getLocalPart());
            if (type != null) {
                return type;
            }
            // fall-through: we might be processing the schema for schemas
        }
        return typesByName.get(name);
    }

    /**
     * Get the simple type with a given name if there is one. This method
     * will also find built-in types.
     *
     * @param name the name of the required simple type
     * @return the simple type if found, or null otherwise
     */

    public SimpleType getSimpleType(StructuredQName name) {
        SchemaType t = getSchemaType(name);
        if (t instanceof SimpleType) {
            return (SimpleType) t;
        }
        return null;
    }

    /**
     * Get all the named simple and complex types defined in this schema.
     * This iterable will not include built-in types or anonymous types.
     *
     * @return an iterable of the types.
     */

    public Iterable<SchemaType> getAllTypes() {
        return typesByName.values();
    }


    /**
     * Adds the given type definition to this Schema.
     * If another type with this name already exists, it is overwritten, unless it has a higher redefinition level
     *
     * @param type the type to add to this Schema
     */

    public void addType(SchemaType type) {
        SchemaType existing = typesByName.get(type.getStructuredQName());
        if (existing == null || existing.getRedefinitionLevel() < type.getRedefinitionLevel()) {
            typesByName.put(type.getStructuredQName(), type);
            if (existing != type && type instanceof UserSchemaComponent) {
                addComponent((UserSchemaComponent) type);
                targetNamespaces.add(type.getTargetNamespace());
            }
        }
    }

    /**
     * Register an anonymous type defined in this schema
     *
     * @param type the anonymous type to be added
     */

    public void addAnonymousType(SchemaType type) {
        //System.err.println("add anonymous type " + type + " to schema " + this);
        anonymousTypesByName.put(type.getStructuredQName(), type);
    }

    /**
     * Get type defined in this schema, searching both named and anonymous types
     *
     * @param name the name of the required schema type
     * @return the schema type if found, else null
     */

    public SchemaType getType(StructuredQName name) {
        SchemaType type = getSchemaType(name);
        if (type == null) {
            type = anonymousTypesByName.get(name);
        }
//        if (type instanceof SimpleTypeDefinition) {
//            SimpleType workingType = ((SimpleTypeDefinition) type).getWorkingType();
//            if (workingType != null) {
//                type = workingType;
//                typesByName.put(name, type);
//            }
//        }
        return type;
    }

    /**
     * Register a complex type that extends a built-in simple type
     *
     * @param fp        the fingerprint of the built-in type
     * @param extension the complex type that extends the built-in (directly or indirectly)
     */

    public void registerExtensionOfBuiltInType(int fp, UserComplexType extension) {
        if (extensionsOfBuiltInTypes == null) {
            extensionsOfBuiltInTypes = new IntHashMap<>(20);
        }
        Set<UserComplexType> set = extensionsOfBuiltInTypes.get(fp);
        if (set == null) {
            set = new HashSet<>(4);
            extensionsOfBuiltInTypes.put(fp, set);
        }
        set.add(extension);
    }

    /**
     * Get the set of user-defined complex types that extend a given built-in type
     *
     * @param fp the fingerprint of the built-in type
     * @return the set of user-defined complex types that have been registered as extensions of this
     *         built-in type
     */

    public Set<UserComplexType> getExtensionsOfBuiltInType(int fp) {
        if (extensionsOfBuiltInTypes == null) {
            return Collections.emptySet();
        }
        Set<UserComplexType> set = extensionsOfBuiltInTypes.get(fp);
        if (set == null) {
            return Collections.emptySet();
        } else {
            return set;
        }
    }

    /**
     * Adds the given global element declaration to this Schema definition
     *
     * @param elementDecl the ElementDecl to add to this Schema
     */
    public void addElementDecl(ElementDecl elementDecl) {
        ElementDecl existing = elementsByName.get(elementDecl.getComponentName());
        if (existing == null || existing.getRedefinitionLevel() < elementDecl.getRedefinitionLevel()) {
            elementsByName.put(elementDecl.getComponentName(), elementDecl);
            elementsByFingerprint.put(elementDecl.getFingerprint(), elementDecl);
            if (existing != elementDecl) {
                // register it for fixup and validation
                addComponent(elementDecl);
                targetNamespaces.add(elementDecl.getTargetNamespace());
            }
        }
    }

    /**
     * Get the global element declaration with a given  name
     *
     * @param name the name of the element
     * @return the element declaration, or null if not found.
     */

    public ElementDecl getElementDecl(StructuredQName name) {
        ElementDecl decl = elementsByName.get(name);
        if (decl == null && isBuiltInNamespace(name.getNamespaceUri()) ) {
            config.addSchemaForBuiltInNamespace(name.getNamespaceUri());
            decl = elementsByName.get(name);
        }
        return decl;
    }

    /**
     * Get the global element declaration with a given fingerprint
     *
     * @param fingerprint the NamePool fingerprint of the name of the element
     * @return the element declaration, or null if not found.
     */

    public ElementDecl getElementDecl(int fingerprint) {
        return elementsByFingerprint.get(fingerprint);
    }

    /**
     * Get an iterator over all the global element declarations defined in this schema.
     *
     * @return an iterator over the global element declarations. Each item returned by
     *         the iterator will be an instance of {@link ElementDecl}
     */

    public Iterable<ElementDecl> getElementDeclarations() {
        return elementsByName.values();
    }

    /**
     * Adds the given Notation declaration to this Schema defintion
     *
     * @param notation the Notation to add to this Schema
     */
    public void addNotation(Notation notation) {
        if (notations == null) {
            notations = new HashMap<>(20);
        }
        Map<StructuredQName, Notation> nn = notations;
        Notation oldNotation = nn.put(notation.getNotationName(), notation);
        if (oldNotation != notation) {
            addComponent(notation);
            targetNamespaces.add(notation.getTargetNamespace());
        }
    }

    /**
     * Get the notation with a given  name
     *
     * @param notationName identifies the name of the notation
     * @return the element notation, or null if not found.
     */

    public Notation getNotation(StructuredQName notationName) {
        if (notations == null) {
            return null;
        }
        return notations.get(notationName);
    }

    /**
     * Get an iterator over all the notations defined in this schema.
     *
     * @return an iterator over the notations. Each item returned by
     *         the iterator will be an instance of {@link Notation}
     */

    public Iterable<Notation> getAllNotations() {
        if (notations == null) {
            return Collections.emptyList();
        }
        return notations.values();
    }

    /**
     * Add an identity constraint
     *
     * @param constraint the identity constraint to be added
     */

    public void addIdentityConstraint(IdentityConstraint constraint) {
        if (identityConstraints == null) {
            identityConstraints = new HashMap<>(20);
        }
        IdentityConstraint oldConstraint = identityConstraints.put(constraint.getConstraintName(), constraint);
        if (oldConstraint != constraint) {
            addComponent(constraint);
            targetNamespaces.add(constraint.getTargetNamespace());
        }
    }

    /**
     * Get the identity constraint whose name has a given fingerprint
     *
     * @param name the name of the required constraint
     * @return the identity constraint, or null if there is none with that name
     */

    public IdentityConstraint getIdentityConstraint(StructuredQName name) {
        if (identityConstraints == null) {
            return null;
        }
        return identityConstraints.get(name);
    }

    /**
     * Get an iterator over all the identity constraints defined in this schema
     * (that is, unique, key, and keyref components).
     *
     * @return an iterator over the identity constraints. Each item returned by
     *         the iterator will be an instance of {@link IdentityConstraint}
     */

    public Iterable<IdentityConstraint> getIdentityConstraints() {
        if (identityConstraints == null) {
            return Collections.emptyList();
        }
        return identityConstraints.values();
    }


    /**
     * Get the set of all target namespaces of components present in this schema
     *
     * @return the set of all target namespaces for these components
     */

    public Set<NamespaceUri> getTargetNamespaces() {
        return targetNamespaces;
    }

    /**
     * Get the NamePool used by this schema
     *
     * @return the namepool
     */

    public NamePool getNamePool() {
        return namePool;
    }

    /**
     * Declare a global parameter for XPath expressions in this schema document (Saxon extension)
     *
     * @param param the details of the parameter
     */

    public void addGlobalParam(GlobalParam param) throws SchemaException {
        GlobalParam prev = params.put(param.getVariableQName(), param);
        if (prev != null && prev != param) {
            // TODO: allow duplicates if they are compatible
            throw new SchemaException("Duplicate declaration of parameter " + param.getVariableQName().getEQName());
        }
    }

    /**
     * Get the GlobalParam object representing a named parameter (Saxon extension)
     *
     * @param name the parameter name
     * @return the GlobalParam object if a parameter with this name exists, else null
     */

    public GlobalParam getGlobalParam(StructuredQName name) {
        return params.get(name);
    }

    /**
     * Return the set of global parameters defined in this schema document
     *
     * @return the set of global parameters
     */

    public Collection<GlobalParam> getGlobalParams() {
        return params.values();
    }

    /**
     * Copy the components of this schema to another schema
     *
     * @param destination the destination of the copy
     * @param compiler    used for error reporting
     * @throws SchemaException if there are name clashes between this
     *                         schema and the destination schema (that is, the same name used to refer to different components.
     *                         Components are compared by checking their name and origin, where the origin is the SystemId and
     *                         line number of the schema document from which they were initially derived)
     */

    public void copyTo(PreparedSchema destination, SchemaCompiler compiler) throws SchemaException {
        List<Action> fixupActions = new ArrayList<>();
        checkConsistency(destination, compiler, fixupActions);
        copyWithoutChecking(destination);
        for (Action action : fixupActions) {
            try {
                action.doAction();
            } catch (XPathException e) {
                throw new SchemaException(e);
            }
        }
    }

    /**
     * Copy the components of this schema to another schema, without any consistency checking
     *
     * @param destination the destination of the copy
     */

    private void copyWithoutChecking(PreparedSchema destination) {

        for (StructuredQName fp : typesByName.keySet()) {
            // We do it this way to allow for redefinition: the same type may have two different fingerprints
            SchemaType type = typesByName.get(fp);
            destination.addType(type);
        }

        for (SchemaType schemaType : anonymousTypesByName.values()) {
            UserDefinedType type = (UserDefinedType) schemaType;
            destination.addAnonymousType(type);
        }

        for (ElementDecl el : elementsByName.values()) {
            destination.addElementDecl(el);
        }

        for (AttributeDecl at : attributesByName.values()) {
            destination.addAttributeDecl(at);
        }

        for (ModelGroupDefinition mg : groups.values()) {
            destination.addGroup(mg);
        }

        for (AttributeGroupDecl ag : attributeGroups.values()) {
            destination.addAttributeGroup(ag);
        }

        if (notations != null) {
            for (Notation n : notations.values()) {
                destination.addNotation(n);
            }
        }

        if (identityConstraints != null) {
            for (IdentityConstraint ic : identityConstraints.values()) {
                destination.addIdentityConstraint(ic);
            }
        }

        HashSet<UserSchemaComponent> destinationComponents = new HashSet<>();
        if (destination.components != null) {
            destinationComponents.addAll(destination.components);
        }

        if (components != null) {
            for (UserSchemaComponent comp : components) {
                if (!destinationComponents.contains(comp)) {
                    destination.addComponent(comp);
                }
            }
        }

        if (extensionsOfBuiltInTypes != null) {
            IntIterator ii = extensionsOfBuiltInTypes.keyIterator();
            while (ii.hasNext()) {
                int fp = ii.next();
                Set<UserComplexType> types = getExtensionsOfBuiltInType(fp);
                for (UserComplexType type : types) {
                    destination.registerExtensionOfBuiltInType(fp, type);
                }
            }
        }

        for (GlobalParam param : params.values()) {
            try {
                destination.addGlobalParam(param);
            } catch (SchemaException err) {
                throw new IllegalStateException(err); // should have been checked earlier
            }
        }
        
    }

    /**
     * Check that the components in this PreparedSchema have no conflicts with those in a given
     * destination schema. This is called prior to merging the two schemas.
     *
     * @param destination the destination schema
     * @param compiler    the schema compiler. Any conflicts will be reported as errors to the
     *                    schema compiler.
     * @throws SchemaException thrown if there are any conflicts.
     */

    private void checkConsistency(PreparedSchema destination, SchemaCompiler compiler, List<Action> fixupActions)
            throws SchemaException {
        boolean conflicts = false;
        for (StructuredQName fp : typesByName.keySet()) {
            // We do it this way to allow for redefinition: the same type may have two different fingerprints
            SchemaType type = typesByName.get(fp);

            final SchemaType existing = destination.getSchemaType(fp);
            if (existing != null &&
                type.getRedefinitionLevel() == existing.getRedefinitionLevel() &&
                !type.isSameType(existing)) {
                String loc = "";
                if (existing instanceof SourceLocator) {
                    loc = existingLocation((SourceLocator) existing);
                }
                compiler.error("Duplicate type " + Err.wrap(type.getDisplayName()) + loc,
                    type instanceof Location ? (Location) type : null);
                conflicts = true;
            }
        }

        for (SchemaType schemaType : anonymousTypesByName.values()) {
            UserDefinedType type = (UserDefinedType) schemaType;
            final SchemaType existing = destination.getSchemaType(type.getTypeName());
            if (existing != null && !type.isSameType(existing)) {
                String loc = "";
                if (existing instanceof SourceLocator) {
                    loc = existingLocation((SourceLocator) existing);
                }
                compiler.error("Duplicate type " + Err.wrap(type.getName()) + loc, type);
                conflicts = true;
            }
        }

        for (ElementDecl decl : elementsByName.values()) {
            final ElementDecl existing = destination.getElementDecl(decl.getComponentName());
            if (existing != null && !existing.isSameDeclaration(decl)) {
                String loc = existingLocation(existing);
                compiler.error(
                    "Duplicate element declaration " + Err.wrap(decl.getDisplayName(), Err.ELEMENT) + loc, decl);
                conflicts = true;
            }
        }

        for (AttributeDecl decl : attributesByName.values()) {
            final AttributeDecl existing = destination.getAttributeDecl(decl.getComponentName());
            if (existing != null && !existing.isSameDeclaration(decl)) {
                String loc = existingLocation(existing);
                compiler.error(
                    "Duplicate attribute declaration " + Err.wrap(decl.getDisplayName()) + loc, decl);
                conflicts = true;
            }
        }

        for (ModelGroupDefinition decl : groups.values()) {
            final ModelGroupDefinition existing = destination.getGroup(decl.getModelGroupName());
            if (existing != null &&
                existing.getRedefinitionLevel() == decl.getRedefinitionLevel() &&
                !existing.isSameGroup(decl)) {
                String loc = existingLocation(existing);
                compiler.error("Duplicate group declaration " + Err.wrap(decl.getName()) + loc, decl);
                conflicts = true;
            }
        }

        for (AttributeGroupDecl decl : attributeGroups.values()) {
            final AttributeGroupDecl existing = destination.getAttributeGroup(decl.getAttributeGroupName());
            if (existing != null &&
                    existing.getRedefinitionLevel() == decl.getRedefinitionLevel() &&
                    !existing.isSameDeclaration(decl)) {
                String loc = existingLocation(existing);
                compiler.error(
                    "Duplicate attribute group declaration " + Err.wrap(decl.getDisplayName()) + loc, decl);
                conflicts = true;
            }
        }

        if (notations != null) {
            for (Notation decl : notations.values()) {
                final Notation existing = destination.getNotation(decl.getNotationName());
                if (existing != null && existing != decl && !decl.isSameDeclaration(existing)) {
                    String loc = existingLocation(existing);
                    compiler.error("Duplicate notation declaration " + Err.wrap(decl.getName()) + loc, decl);
                    conflicts = true;
                }
            }
        }

        if (identityConstraints != null) {
            for (IdentityConstraint decl : identityConstraints.values()) {
                final IdentityConstraint existing = destination.getIdentityConstraint(decl.getConstraintName());
                if (existing != null && !existing.isSameDeclaration(decl)) {
                    String loc = existingLocation(existing);
                    compiler.error(
                        "Duplicate identity constraint name " + Err.wrap(decl.getName()) + loc, decl);
                    conflicts = true;
                }
            }
        }

        for (GlobalParam param : params.values()) {
            if (destination.getGlobalParam(param.getVariableQName()) != null) {
                compiler.error("Duplicate saxon:param declaration " + param.getVariableQName().getEQName(), param);
                conflicts = true;
            }
        }
        if (conflicts) {
            throw new SchemaException("Duplicated components found in schema");
        }
    }


    private String existingLocation(SourceLocator existing) {
        if (existing.getLineNumber() < 1) {
            return " - previously defined in imported component";
        } else {
            return " - previously defined on line " + existing.getLineNumber() +
                    " of " + existing.getSystemId();
        }
    }

    /**
     * Checks the validity of this Schema definition, and if valid, adds the schema to the
     * cache held by the EnterpriseConfiguration
     *
     * @param compiler a SchemaCompiler to which all errors should be reported
     * @return true if validation succeeds, false if it fails
     * @throws SchemaException if the SchemaCompiler decides that an error is fatal
     */

    public boolean validate(SchemaCompiler compiler)
            throws SchemaException {

        //System.err.println("*** Validating " + targetNamespace);

        boolean result = true;

        // Fix up all component references
        fixupComponentReferences(compiler);

        // Now fix up substitution groups
        for (ElementDecl elementDecl : elementsByName.values()) {
            elementDecl.fixupSubstitutionGroup(compiler);
        }

        //System.err.println("Validating top-level components");

        //-- validate top-level components
        if (components != null) {
            for (UserSchemaComponent s : components) {
                //System.err.println("Validating " + s.toString());
                result &= s.validate(compiler);
                if (result && (s instanceof SimpleTypeDefinition)) {
                    UserSimpleType st = ((SimpleTypeDefinition) s).getWorkingType();
                    typesByName.put(st.getTypeName(), st);
                    PreparedSchema superSchema = compiler.getConfiguration().getUnnamedSchema();
                    if (superSchema.getSchemaType(st.getTypeName()) == s) {
                        superSchema.typesByName.put(st.getTypeName(), st);
                    }
                }
            }
        }

        // type-check the XPath expressions in identity constraints

        if (result) {
            compiler.typeCheckIdentityConstraints();
        }

        // let the garbage collector have its day
        components = null;

        // if valid, add the components of this schema to the cache maintained by the Configuration
//        if (result) {
//            config.addSchema(this);
//        }
        return result;

    }

    /**
     * Helper method to validate that a reference to a schema component can be resolved
     *
     * @param ref      The Component reference that needs to be resolved
     * @param compiler used for error reporting
     * @param fatal    True if failure to resolve the reference is an error rather than a warning
     * @return the target of the reference
     * @throws SchemaException if the reference cannot be resolved
     */

    public static SchemaComponent validateReference(ComponentReference ref, SchemaCompiler compiler, boolean fatal)
            throws SchemaException {
        if (ref.isDangling()) {
            // we've been here before and failed; don't repeat the error message
            return ref.getTarget();
        }
        ref.tryToResolve(compiler, fatal);
        if (ref.isResolved()) {
            return ref.getTarget();
        } else {
            String componentType = SymbolSpace.getSymbolSpaceName(ref.getSymbolSpace());
            String reason = "has not been declared";
            if (ref.getTargetComponentName().hasURI(NamespaceUri.SCHEMA)) {
                reason = "is not defined in the XML Schema namespace";
            }
            String message = "The " + componentType + ' ' +
                Err.wrap(ref.getTargetComponentName().getDisplayName()) +
                " is referenced, but " + reason;
            if (compiler.reportMissingComponent(componentType, ref.getTargetComponentName())) {
                if (fatal || !compiler.isAllowDanglingReferences()) {
                    compiler.error(message, ref);
                } else {
                    compiler.warning(message, SaxonErrorCode.SXSD1007, ref);
                }
            }
            return null;
        }
    }

    /**
     * Merge two PreparedSchemas into one, rejecting any components that are defined
     * in both.
     *
     * @param ps1 the first PreparedSchema
     * @param ps2 the second PreparedSchema
     * @return the merged PreparedSchema
     * @throws net.sf.saxon.type.SchemaException
     *          if there are inconsistencies between the two schemas,
     *          for example different components with the same name
     */

    public static PreparedSchema merge(PreparedSchema ps1, PreparedSchema ps2) throws SchemaException {
        if (ps1.config != ps2.config) {
            throw new SchemaException("The two schemas to be merged must use the same EnterpriseConfiguration");
        }
//        if (!ps1.targetNamespace.equals(ps2.targetNamespace)) {
//            throw new SchemaException("The two schemas to be merged must have the same target namespace");
//        }
        NamePool pool = ps1.config.getNamePool();
        PreparedSchema result = new PreparedSchema(ps1.config);
        result.attributesByName = combine(ps1.attributesByName, ps2.attributesByName, "Attribute declaration");
        result.attributesByFingerprint = combine(ps1.attributesByFingerprint, ps2.attributesByFingerprint, pool, "Attribute declaration");
        result.attributeGroups = combine(ps1.attributeGroups, ps2.attributeGroups, "Attribute group");
        result.groups = combine(ps1.groups, ps2.groups, "Model group");
        result.typesByName = combine(ps1.typesByName, ps2.typesByName, "Type");
        result.anonymousTypesByName = combine(ps1.anonymousTypesByName, ps2.anonymousTypesByName, "Anonymous type");
        result.elementsByName = combine(ps1.elementsByName, ps2.elementsByName, "Element declaration");
        result.elementsByFingerprint = combine(ps1.elementsByFingerprint, ps2.elementsByFingerprint, pool, "Element declaration");
        result.notations = combine(ps1.notations, ps2.notations, "Notation");
        result.identityConstraints = combine(ps1.identityConstraints, ps2.identityConstraints, "Identity constraint");
        if (ps1.components != null || ps2.components != null) {
            result.components = new ArrayList<>(
                    (ps1.components == null ? 0 : ps1.components.size()) +
                            (ps2.components == null ? 0 : ps2.components.size()));
            if (ps1.components != null) {
                result.components.addAll(ps1.components);
            }
            if (ps2.components != null) {
                result.components.addAll(ps2.components);
            }
        }
        result.targetNamespaces.addAll(ps1.targetNamespaces);
        result.targetNamespaces.addAll(ps2.targetNamespaces);
        return result;
    }

    /**
     * Combine two hash maps containing schema components
     *
     * @param m1   first hash map, of components indexed by name
     * @param m2   second hash map, of components indexed by name
     * @param kind the name of the kind of component
     * @return the combined integer hash map
     * @throws SchemaException if the two hash maps contain conflicting definitions for the same component
     */

    //@CSharpTypeBounds(bounds = {"T:class"})
    private static <T extends SchemaComponent> Map<StructuredQName, T> combine(
                Map<StructuredQName, T> m1,
                Map<StructuredQName, T> m2,
                String kind) throws SchemaException {
        if (m1 == null) {
            return m2;
        }
        if (m2 == null) {
            return m1;
        }
        HashMap<StructuredQName, T> result = new HashMap<>(m1.size() + m2.size());
        for (StructuredQName k1 : m1.keySet()) {
            T v1 = m1.get(k1);
            T v2 = m2.get(k1);
            //noinspection RedundantCast  (needed for C#)
            if (v2 != null && (net.sf.saxon.type.SchemaComponent)v2 != (net.sf.saxon.type.SchemaComponent)v1) {
                String loc1 = identify(v1);
                String loc2 = identify(v2);
                if (!loc1.equals(loc2)) {
                    throw new SchemaException(kind + " " +
                        k1.getDisplayName() +
                        " is defined in more than one schema document (" +
                        loc1 + ", " + loc2 + ")");
                }
            }
            result.put(k1, v1);
        }
        for (StructuredQName k2 : m2.keySet()) {
            result.put(k2, m2.get(k2));
        }
        return result;
    }

    private static String identify(SchemaComponent component) {
        if (component instanceof SchemaStructure) {
            SchemaStructure ss = (SchemaStructure) component;
            return "line " + ss.getLineNumber() + " of " + ss.getSystemId();
        } else if (component instanceof SchemaType) {
            return ((SchemaType)component).getEQName();
        } else {
            return component.getClass().getName();
        }
    }

    /**
     * Combine two hash maps containing schema components indexed by fingerprint
     *
     * @param m1   first hash map, of components indexed by fingerprint
     * @param m2   second hash map, of components indexed by fingerprint
     * @param pool the name pool
     * @param kind the name of the kind of component
     * @return the combined integer hash map
     * @throws SchemaException if the two hash maps contain conflicting definitions for the same component
     */

    //@CSharpTypeBounds(bounds = {"T:com.saxonica.ee.schema.SchemaStructure"})
    private static <T extends SchemaStructure> IntHashMap<T>  combine
            (IntHashMap<T> m1, IntHashMap<T> m2, NamePool pool, String kind)
            throws SchemaException {
        if (m1 == null) {
            return m2;
        }
        if (m2 == null) {
            return m1;
        }
        IntHashMap<T> result = new IntHashMap<>(m1.size() + m2.size());
        IntIterator i1 = m1.keyIterator();
        while (i1.hasNext()) {
            int k1 = i1.next();
            T v1 = m1.get(k1);
            T v2 = m2.get(k1);
            if (v2 != null && v2 != v1) {
                String loc1 = identify(v1);
                String loc2 = identify(v2);
                if (!loc1.equals(loc2)) {
                    throw new SchemaException(kind + " " +
                        pool.getEQName(k1) +
                        " is defined in more than one schema document (" +
                        loc1 + ", " + loc2 + ")");
                }
            }
            result.put(k1, v1);
        }
        IntIterator i2 = m2.keyIterator();
        while (i2.hasNext()) {
            int k2 = i2.next();
            result.put(k2, m2.get(k2));
        }
        return result;
    }


    @Override
    public boolean isDeclaredNotation(NamespaceUri uri, String local) {
        return notations != null && notations.get(new StructuredQName("", uri, local)) != null;
    }


    /**
     * Load a schema, which will be available for use by all subsequent operations using
     * this EnterpriseConfiguration.
     *
     * @param schemaSource  the JAXP Source object identifying the schema document to be loaded. This may be either
     *                      a source schema document in XSD format, or a compiled schema in Saxon's SCM format
     * @param errorReporter the ErrorListener to be notified of any errors in the schema. If null,
     *                      the configuration's errorListener is used
     * @param compiler      the schema compiler to be used. This allows a cache of schema documents (retained by the
     *                      schema compiler) to be reused when compiling multiple schema documents. See bug #4982.
     * @throws SchemaException if the schema cannot be read or parsed or if it is invalid
     */

    public synchronized void addSchemaSource(Source schemaSource, /*@Nullable*/ ErrorReporter errorReporter, SchemaCompiler compiler) throws SchemaException {
        try {
            //#if CSHARP==false
            if (schemaSource instanceof SAXSource && config.getBooleanProperty(Feature.IGNORE_SAX_SOURCE_PARSER)) {
                // This option is provided to allow the parser set by applications such as Ant to be overridden by
                // the parser requested using FeatureKeys.SOURCE_PARSER or FeatureKeys.STYLE_PARSER
                ((SAXSource) schemaSource).setXMLReader(null);
            }
            //#endif
            if (errorReporter != null) {
                compiler.setErrorReporter(errorReporter);
            }
            final PipelineConfiguration pipe = config.makePipelineConfiguration();
            pipe.setErrorReporter(errorReporter == null ? config.makeErrorReporter() : errorReporter);
            pipe.setHostLanguage(HostLanguage.XML_SCHEMA);
            SchemaDocument schemadoc = SchemaReader.read(schemaSource, compiler, pipe, null);
            if (schemadoc.isCompiledSchemaDocument()) {
                try {
                    importComponents(schemadoc.getDocument().getRootNode().asActiveSource());
                } catch (XPathException e) {
                    throw new SchemaException(e);
                }
            } else {
                PreparedSchema ps = compiler.validate(schemadoc);
                addSchema(ps);
                setNamespaceStatus(schemadoc.getTargetNamespace(), PreparedSchema.NAMESPACE_KNOWN);
            }

        } catch (UncheckedXPathException e) {
            // indicates that the error reporter asked for termination
            XPathException xe = e.getXPathException();
            if (xe instanceof SchemaException) {
                throw (SchemaException) xe;
            } else {
                throw new SchemaException(xe);
            }
        }
    }

    /**
     * Import a precompiled Schema Component Model from a given Source. The schema components derived from this schema
     * document are added to the cache of schema components maintained by this SchemaManager
     *
     * @param source the XML file containing the schema component model, as generated by a previous call on
     *               {@link Configuration#exportComponents}
     */


    public void importComponents(Source source) throws XPathException {
        try {
            SchemaModelLoader loader = new SchemaModelLoader(config);
            PreparedSchema schema = loader.load(source);
            addSchema(schema);
            //schema.copyTo(this, config.newSchemaCompiler());
            for (NamespaceUri ns : schema.getTargetNamespaces()) {
                setNamespaceStatus(ns, PreparedSchema.NAMESPACE_KNOWN);
            }
        } catch (SchemaException e) {
            throw new XPathException(e);
        }
    }

    /**
     * Add a schema (a collection of schema components) to the cache
     *
     * @param pschema The schema to be added
     * @throws net.sf.saxon.type.SchemaException if the schema cannot be added because components in the schema
     *                                           are incompatible with components already in the cache
     */

    public synchronized void addSchema(PreparedSchema pschema) throws SchemaException {
        config.checkLicensedFeature(Configuration.LicenseFeature.SCHEMA_VALIDATION, "schema processing", pschema.getLocalLicenseId());
        if (pschema == this) {
            return;
        }
        if (pschema instanceof SingleNamespaceSchema) {
            NamespaceUri targetNamespace = ((SingleNamespaceSchema) pschema).getTargetNamespace();
            setNamespaceStatus(targetNamespace, PreparedSchema.NAMESPACE_KNOWN);
        } else {
            for (NamespaceUri ns : pschema.getTargetNamespaces()) {
                setNamespaceStatus(ns, PreparedSchema.NAMESPACE_KNOWN);
            }
        }
        pschema.copyTo(this, config.newSchemaCompiler());
        setLocalLicenseId(pschema.getLocalLicenseId());

    }


    /**
     * Get the status of a particular schema namespace.
     *
     * @param namespace the Namespace URI whose status is required (supply "" for the non-namespace)
     * @return one of {@link PreparedSchema#NAMESPACE_KNOWN}, {@link PreparedSchema#NAMESPACE_UNKNOWN},
     * {@link PreparedSchema#NAMESPACE_UNDER_CONSTRUCTION}, {@link PreparedSchema#NAMESPACE_SEALED}
     */

    public synchronized int getNamespaceStatus(NamespaceUri namespace) {
        return namespaceStatus.getOrDefault(namespace, PreparedSchema.NAMESPACE_UNKNOWN);
    }

    /**
     * Set the status of a particular schema namespace.
     *
     * @param namespace the Namespace URI whose status is required (supply "" for the non-namespace)
     * @param status    one of {@link PreparedSchema#NAMESPACE_KNOWN}, {@link PreparedSchema#NAMESPACE_UNKNOWN},
     *                  {@link PreparedSchema#NAMESPACE_UNDER_CONSTRUCTION}, {@link PreparedSchema#NAMESPACE_SEALED}
     */

    public synchronized void setNamespaceStatus(NamespaceUri namespace, int status) {
        if (status == PreparedSchema.NAMESPACE_UNKNOWN) {
            namespaceStatus.remove(namespace);
        } else {
            namespaceStatus.put(namespace, status);
        }
    }

    /**
     * Get a document-level validator to add to a Receiver pipeline
     *
     * @param receiver           The receiver to which events should be sent after validation
     * @param systemId           the base URI of the document being validated
     * @param validationOptions  Supplies options relevant to XSD validation
     * @param initiatingLocation (optional) Location in query or stylesheet of instruction
     *                           that requested validation
     * @return A Receiver to which events can be sent for validation
     */

    public Receiver getDocumentValidator(Receiver receiver,
                                         String systemId,
                                         ParseOptions validationOptions,
                                         Location initiatingLocation) {

        int validationMode = validationOptions.getSchemaValidationMode();
        SpaceStrippingRule stripRules = validationOptions.getSpaceStrippingRule();
        SchemaType schemaType = validationOptions.getTopLevelType();
        StructuredQName topLevelElementName = validationOptions.getTopLevelElement();
        ValidationParams params = validationOptions.getValidationParams();

        PipelineConfiguration pipe = receiver.getPipelineConfiguration();
        if (validationMode == Validation.STRICT || validationMode == Validation.LAX || schemaType != null) {
            Receiver destination = receiver;
            if (stripRules == null) {
                stripRules = config.getParseOptions().getSpaceStrippingRule();
            }
            if (stripRules == IgnorableSpaceStrippingRule.getInstance()) {
                destination = stripRules.makeStripper(destination);
            }

            ValidationContext vc = new ValidationContext(config.getConversionRules());
            vc.setInitiatingLocation(initiatingLocation);
            vc.setErrorLimit(validationOptions.getValidationErrorLimit());
            vc.setSchema(this);
            if (pipe.getController() == null) {
                pipe.setController(new Controller(config));
                pipe.getController().getExecutable().setSchemaAware(true);
            }
            vc.setController(pipe.getController());
            vc.setValidationParams(params);

            pipe.setComponent("com.saxonica.ee.validate.ValidationContext", vc);

            if (validationOptions.isCheckEntityReferences()) {
                EntityValidator ev = new EntityValidator(destination);
                ev.setValidationContext(vc);
                destination = ev;
            }

            allocateInvalidityHandler(validationOptions, vc);

            IdValidator idval = new IdValidator(destination);
            idval.setValidationContext(vc);
            ConstraintChecker checker = new ConstraintChecker(pipe /*idval*/);
            checker.setValidationContext(vc);

            destination = new TeeOutputter(idval, checker);

            // If running XSD 1.1, add an assertion tree builder before the constraint checker
            destination = allocateAssertionTreeBuilder(destination, pipe, vc);


            ValidationStack val = new ValidationStack(vc, destination, validationMode, schemaType);
            val.setPipelineConfiguration(pipe);

            vc.setConstraintChecker(checker);
            if (topLevelElementName != null) {
                val.setTopLevelElement(new FingerprintedQName(
                        topLevelElementName.getPrefix(),
                        topLevelElementName.getNamespaceUri(),
                        topLevelElementName.getLocalPart()));
            }
            HostLanguage hostLanguage = pipe.getHostLanguage();
            if (hostLanguage == HostLanguage.XSLT) {
                if (schemaType != null) {
                    vc.setErrorCode("XTTE1540");
                } else if (validationMode == Validation.STRICT) {
                    vc.setErrorCode("XTTE1510");
                } else {
                    vc.setErrorCode("XTTE1515");
                }
            } else if (hostLanguage == HostLanguage.XQUERY) {
                vc.setErrorCode("XQDY0027");
            } else if (hostLanguage == HostLanguage.XML_SCHEMA) {
                vc.setErrorCode(null);
            }

            XSIAttributeHandler xsi = new XSIAttributeHandler(val);
            xsi.setSystemId(systemId);

            AttributeInheritor stb = new AttributeInheritor(xsi);
            vc.setAttributeInheritor(stb);
            xsi.setValidationContext(vc);

            PathMaintainer pathMaintainer = new PathMaintainer(stb);
            pipe.setComponent("net.sf.saxon.event.PathMaintainer", pathMaintainer);

            DocumentValidator dv = new DocumentValidator(pathMaintainer, hostLanguage == HostLanguage.XSLT ? "XTTE1550" : "XQDY0061");

            LocationCopier copier = new LocationCopier(true, systemId);
            pipe.setCopyInformee(CSharp.methodRef(copier::notifyElementNode));

            vc.startValidation(systemId);

            return dv;

        } else if (validationMode == Validation.STRIP) {
            SkipValidator a = new SkipValidator(receiver);
            ValidationContext vc = new ValidationContext(config.getConversionRules());
            a.setValidationContext(vc);
            a.setSystemId(systemId);
            return a;
        } else {
            return receiver;
        }
    }

    private void allocateInvalidityHandler(ParseOptions validationOptions, ValidationContext vc) {
        if (validationOptions.getInvalidityHandler() == null) {
            ErrorReporter reporter = validationOptions.getErrorReporter();
            if (reporter == null) {
                Controller controller = vc.getController();
                if (controller != null) {
                    reporter = controller.getErrorReporter();
                } else {
                    reporter = config.makeErrorReporter();
                }
            }
            vc.setInvalidityHandler(new InvalidityHandlerWrappingErrorReporter(reporter));
        } else {
            vc.setInvalidityHandler(validationOptions.getInvalidityHandler());
        }
    }

    private Receiver allocateAssertionTreeBuilder(Receiver destination, PipelineConfiguration pipe, ValidationContext vc) {
        if (config.getXsdVersion() == XSD11) {
            // TODO: allow this to vary per schema
            boolean retainComments = config.getBooleanProperty(Feature.ASSERTIONS_CAN_SEE_COMMENTS);
            AssertionTreeBuilder atb = new AssertionTreeBuilder(destination, retainComments);
            atb.setPipelineConfiguration(pipe);
            vc.setAssertionTreeBuilder(atb);
            pipe.setComponent("com.saxonica.ee.validate.AssertionTreeBuilder", atb);
            destination = atb;
        }
        return destination;
    }

    /**
     * Validate an attribute value (XSLT only)
     *
     * @param nodeName   the name of the attribute
     * @param value      the value of the attribute as a string
     * @param validation STRICT or LAX
     * @return an int, containing the type annotation to apply to the attribute node
     * @throws ValidationException if the value is invalid
     */


    public SimpleType validateAttribute(StructuredQName nodeName, UnicodeString value, int validation)
            throws ValidationException, MissingComponentException {

        AttributeDecl decl = getAttributeDecl(nodeName);
        if (decl == null) {
            if (validation == Validation.STRICT) {
                ValidationFailure ve = new ValidationFailure(
                        "No global attribute declaration found for attribute " +
                                nodeName.getDisplayName());
                ve.setErrorCode("XTTE1512");
                throw ve.makeException();
            } else {
                // lax validation
                return BuiltInAtomicType.UNTYPED_ATOMIC;
            }
        }
        SimpleType type = decl.getSimpleType();
        if (type.isNamespaceSensitive()) {
            ValidationFailure nsErr = new ValidationFailure("Cannot create a parentless attribute whose " +
                                                                    "type is namespace-sensitive (such as xs:QName)");
            nsErr.setSchemaType(type);
            nsErr.setErrorCode("XTTE1545");
            throw nsErr.makeException();
        }
        ValidationFailure contentErr = type.validateContent(value, DummyNamespaceResolver.getInstance(), config.getConversionRules());
        if (contentErr != null) {
            contentErr.setSchemaType(type);
            contentErr.setErrorCode(validation == Validation.STRICT ? "XTTE1510" : "XTTE1515");
            throw contentErr.makeException();
        }
        return type;
    }


}

// Copyright (c) 2005-2023 Saxonica Limited
