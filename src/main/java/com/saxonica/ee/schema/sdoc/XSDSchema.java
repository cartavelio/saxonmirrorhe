package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.instruct.Executable;
import net.sf.saxon.expr.instruct.GlobalParam;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeSelector;
import net.sf.saxon.type.Derivation;
import net.sf.saxon.type.SchemaException;

import java.util.*;

/**
 * An xs:schema element in an XML Schema document
 */

public class XSDSchema extends SchemaElement {

    private SingleNamespaceSchema schema = null;
    private SchemaCompiler compiler;
    private int errorCount = 0;
    private Executable executable;

    /**
     * Map of global parameters declared using the saxon:param extension, indexed by the parameter name
     */
    private final HashMap<StructuredQName, GlobalParam> params = new HashMap<>();

    /**
     * If this schema is loaded via include/import/redefine, the referring element
     */

    private SchemaElement referrer;

    /**
     * The attributeFormDefault property
     */
    private String attributeFormDefault = null;

    /**
     * The value of the blockDefault attribute.
     */
    private int blockDefault = 0;

    /**
     * The elementFormDefault attribute for this Schema, as a string ("qualified" or
     * "unqualified")
     */
    private String elementFormDefault = null;

    /**
     * The value of the finalDefault attribute.
     */
    private int finalDefault = 0;

    /**
     * The target namespace of this schema document
     */

    private NamespaceUri targetNamespace;

    /**
     * chameleon is set to true when this schema document is included or redefined by one that specifies
     * its effective target namespace. When this is set, names that would normally have been resolved using
     * the default namespace are instead resolved using the target namespace.
     */
    private boolean chameleon = false;

    /**
     * A set containing all the namespaces imported into this schema document. Note that schema import is
     * not transitive.
     */

    private Set<NamespaceUri> importedNamespaces = null;

    /**
     * The default open content wildcard, if any
     */

    private Wildcard openContentWildcard;

    /**
     * The default open content mode, if any
     */

    private String openContentMode;

    /**
     * Boolean: true if the open content model applies to types that are (otherwise) empty
     */

    private boolean openContentAppliesToEmpty = false;

    /**
     * The default namespace for elements and types in XPath expressions defined in this schema document
     */

    private String xpathDefaultNamespace = "##local";

    /**
     * The QName of the attribute-group corresponding to the defaultAttributes attribute
     */

    private StructuredQName defaultAttributeGroup = null;

    /**
     * Attribute group reference to the default attribute group
     */

    private AttributeGroupReference defaultAttributeGroupReference = null;

    /**
     * Saxon extensions enabled
     */

    private final Set<String> saxonExtensionEnabled = new HashSet<>();

    /**
     * Create an XSDSchema object, representing the xs:schema element in a schema document
     */

    public XSDSchema() {
    }

    /**
     * Get the PreparedSchema to which schema components are being added
     *
     * @return the PreparedSchema containing the compiled schema components
     */

    @Override
    public SingleNamespaceSchema getSchema() {
        return schema;
    }

    /**
     * Get the Executable corresponding to this schema document. This is a container
     * for the XPath expressions it contains.
     *
     * @return the Executable for XPath expressions
     */

    public Executable getExecutable() {
        return executable;
    }

    /**
     * Get the set of extensions allowed in this schema document
     *
     * @return the set of allowed extensions (from the saxon:extensions attribute)
     */

    public Set<String> getAllowedExtensions() {
        return saxonExtensionEnabled;
    }

    /**
     * Set the SingleNamespaceSchema to which schema components are being added
     *
     * @param schema the SingleNamespaceSchema containing the compiled schema components
     */

    public void setSchema(SingleNamespaceSchema schema) {
        this.schema = schema;
    }

    /**
     * Set the SchemaCompiler in use
     *
     * @param manager the schema compiler
     */

    public void setSchemaCompiler(SchemaCompiler manager) {
        this.compiler = manager;
    }

    /**
     * Get the SchemaCompiler in use
     *
     * @return the schema compiler
     */

    public SchemaCompiler getSchemaCompiler() {
        return compiler;
    }

    /**
     * Set the xs:include, xs:import, or xs:redefine element that caused this schema document to be loaded
     *
     * @param referrer the referring xs:include, xs:import, or xs:redefine element
     */

    public void setReferrer(SchemaElement referrer) {
        this.referrer = referrer;
    }

    /**
     * Determine if this is a chameleon schema document, that is, one with no targetNamespace
     * of its own, whose effective target namespace (and for many purposes, default namespace)
     * are set to the targetNamespace of the referring schema document.
     *
     * @return true if this document is a chameleon
     */

    public boolean isChameleon() {
        return chameleon;
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        Configuration config = getConfiguration();
        this.executable = new Executable(config);

        AttributeMap atts = attributes();
        String[] allowed = {"attributeFormDefault", "blockDefault", "defaultAttributes", "elementFormDefault",
                "finalDefault", "id", "targetNamespace", "version", "xpathDefaultNamespace"};
        allowAttributes(atts, allowed);

        String nsURIatt = atts.getValue(NamespaceUri.NULL, "targetNamespace");
        if (nsURIatt != null && nsURIatt.isEmpty()) {
            error("The zero-length string is not a legal namespace URI");
            nsURIatt = null;   // for error recovery
        }
        if (nsURIatt != null) {
            NamespaceUri nsURI = NamespaceUri.of(nsURIatt);
            // if we are including a schema we must take care that the namespaces are the same
            if (referrer instanceof XSDInclude || referrer instanceof XSDRedefine) {
                final NamespaceUri includingNamespace = referrer.getXSDSchema().getTargetNamespace();
                if (!includingNamespace.equals(nsURI)) {
                    error("The target namespace of the included schema must be the same as the target namespace of the including schema");
                    return;
                }
            }
            schema.setTargetNamespace(nsURI);
            schema.setNamespaceStatus(nsURI, PreparedSchema.NAMESPACE_UNDER_CONSTRUCTION);
            //((EnterpriseConfiguration) config).addSurrogateSchema(nsURI);
            targetNamespace = nsURI;
        } else {
            if (referrer instanceof XSDInclude || referrer instanceof XSDRedefine || referrer instanceof XSDOverride) {
                // if no target namespace specified for an included schema document, adopt the target
                // namespace of the including schema
                targetNamespace = referrer.getXSDSchema().getTargetNamespace();
                chameleon = true;
            } else {
                targetNamespace = NamespaceUri.NULL;
                ((EnterpriseConfiguration) config).addSurrogateSchema(NamespaceUri.NULL);
                // targetNamespace absent: no action needed.
                // In the case of a top-level or imported schema,
                // the targetNamespace is null; in the case of an included schema,
                // it is the same as that of the including schema document.
            }
        }

        String extensions = atts.getValue(NamespaceUri.SAXON, "extensions");
        if (extensions != null) {
            StringTokenizer st = new StringTokenizer(extensions, " \t\n\r");
            while (st.hasMoreTokens()) {
                saxonExtensionEnabled.add(st.nextToken());
            }
        }

        // @id
        processId();

        // @version
        //schema.setVersion(atts.getValue(NamespaceUri.NULL, "version"));
        atts.getValue(NamespaceUri.NULL, "version");
        // @attributeFormDefault

        String attValue = atts.getValue(NamespaceUri.NULL, "attributeFormDefault");
        if (attValue != null) {
            if (attValue.equals("qualified") || attValue.equals("unqualified")) {
                setAttributeFormDefault(attValue);
            } else {
                invalidAttributeValue("attributeFormDefault",
                        attValue,
                        "Must be 'qualified' or 'unqualified'");
            }
        }

        // @defaultAttributes
        attValue = atts.getValue(NamespaceUri.NULL, "defaultAttributes");
        if (attValue != null) {
            try {
                // We can't resolve this the usual way using getNameCode() because we haven't yet processed
                // the xs:import elements.
                defaultAttributeGroup = getComponentName(attValue, USE_DEFAULT_NS_WITHOUT_IMPORT);
            } catch (SchemaException err) {
                error(err.getMessage());
            }
        }

        // @elementFormDefault
        attValue = atts.getValue(NamespaceUri.NULL, "elementFormDefault");
        if (attValue != null) {
            if (attValue.equals("qualified") || attValue.equals("unqualified")) {
                setElementFormDefault(attValue);
            } else {
                invalidAttributeValue("elementFormDefault",
                        attValue,
                        "Must be 'qualified' or 'unqualified'");
            }
        }

        // @blockDefault
        attValue = atts.getValue(NamespaceUri.NULL, "blockDefault");
        if (attValue != null) {
            int v = parseFinalOrBlock("blockDefault",
                    attValue,
                    Derivation.DERIVATION_EXTENSION |
                                              Derivation.DERIVATION_RESTRICTION |
                                              Derivation.DERIVE_BY_SUBSTITUTION);
            setBlockDefault(v);
        }

        // @finalDefault
        attValue = atts.getValue(NamespaceUri.NULL, "finalDefault");
        if (attValue != null) {
            int v = parseFinalOrBlock("finalDefault",
                    attValue,
                    Derivation.DERIVATION_EXTENSION |
                                              Derivation.DERIVATION_RESTRICTION |
                                              Derivation.DERIVATION_LIST |
                                              Derivation.DERIVATION_UNION);
            // the last two permitted by erratum E1-53
            setFinalDefault(v);
        }

        // @version
        //attValue = atts.getValue(NamespaceUri.NULL, "version");
//        if (attValue != null) {
//            schema.setVersion(attValue);
//        }

        // @xpathDefaultNamespace
        attValue = atts.getValue(NamespaceUri.NULL, "xpathDefaultNamespace");
        if (attValue != null) {
            xpathDefaultNamespace = attValue;
        }

    }


    /**
     * Determine whether a given URI is that of a namespace that has been imported into this
     * schema document.
     *
     * @param uri the namespace URI ("" for the null namespace)
     * @return true if the namespace has been imported, false otherwise
     */

    public boolean isImportedNamespace(NamespaceUri uri) {
        if (importedNamespaces == null) {
            // Saxon-specific option to license references to components in other namespaces without
            // an explicit xs:import in the containing schema document. Needed for XBRL schemas?
            if (getConfiguration().getBooleanProperty(Feature.IMPLICIT_SCHEMA_IMPORTS)) {
                return true;
            }
            importedNamespaces = new HashSet<>(10);
            for (NodeInfo child : children()) {
                if (child instanceof XSDImport) {
                    NamespaceUri targetNamespace = ((XSDImport) child).getTargetNamespace();
                    importedNamespaces.add(targetNamespace);
                }
            }
        }
        return importedNamespaces.contains(uri);
    }

    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler used for reporting errors
     * @throws SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {

//        if (getParent() == null || !(getParent().getNodeKind() == Type.DOCUMENT)) {
//            error(Err.wrap(getDisplayName(), Err.ELEMENT) + " must be the outermost element in a schema document");
//            return;
//        }

        if (compiler.getLanguageVersion() == Configuration.XSD10) {
            if (!xpathDefaultNamespace.equals("##local")) {
                error("The xpathDefaultNamespace attribute requires XSD 1.1 to be enabled");
            }
            if (defaultAttributeGroup != null) {
                error("The defaultAttributes attribute requires XSD 1.1 to be enabled");
            }
        }

        checkContentModel(stateTerms, stateTargets, false);

        if (defaultAttributeGroup != null) {
            // process it again to check that the namespace has been imported
            String attValue = getAttributeValue(NamespaceUri.NULL, "defaultAttributes");
            try {
                defaultAttributeGroup = getComponentName(attValue, USE_DEFAULT_NS);
            } catch (SchemaException err) {
                error(err.getMessage());
            }
        }

    }

    /**
     * Validate the children of this node, recursively.
     *
     * @param compiler used for error reporting
     * @throws net.sf.saxon.type.SchemaException
     *          if the subtree is not valid
     */
    @Override
    protected void validateChildren(SchemaCompiler compiler) throws SchemaException {
        // The usual order of processing is changed to handle defaultOpenContent first (before any xs:redefine)
        // - see test open042
        for (NodeInfo child : children(NodeSelector.of(c -> c instanceof XSDDefaultOpenContent))) {
            ((SchemaElement) child).validateSubtree(compiler);
        }
        for (NodeInfo child : children()) {
            if (child instanceof SchemaElement && !(child instanceof XSDDefaultOpenContent)) {
                ((SchemaElement) child).validateSubtree(compiler);
            }
        }
    }

    // Following FSM generated using
    // Transform s4s11-2008-06-13.scm generate-s4s-code.xsl type=C230

    // NB: this stylesheet incorrectly generates references to XS_NOTATION rather than XS_notation. These
    // need to be changed by hand.

    private final static int[] state0terms = {StandardNames.XS_OVERRIDE, StandardNames.XS_IMPORT,
            StandardNames.XS_DEFAULT_OPEN_CONTENT, StandardNames.XS_SIMPLE_TYPE, StandardNames.XS_ANNOTATION,
            StandardNames.XS_ATTRIBUTE_GROUP, StandardNames.XS_COMPLEX_TYPE, StandardNames.XS_INCLUDE,
            StandardNames.XS_notation, StandardNames.XS_ELEMENT, StandardNames.XS_ATTRIBUTE,
            StandardNames.XS_GROUP, StandardNames.XS_REDEFINE};
    private final static int[] state0targets = {2, 2, 3, 1, 2, 1, 1, 2, 1, 1, 1, 1, 2};

    private final static int[] state1terms = {StandardNames.XS_ELEMENT, StandardNames.XS_GROUP,
            StandardNames.XS_SIMPLE_TYPE, StandardNames.XS_COMPLEX_TYPE, StandardNames.XS_notation,
            StandardNames.XS_ATTRIBUTE_GROUP, StandardNames.XS_ATTRIBUTE, StandardNames.XS_ANNOTATION};
    private final static int[] state1targets = {1, 1, 1, 1, 1, 1, 1, 4};

    private final static int[] state2terms = {StandardNames.XS_OVERRIDE, StandardNames.XS_IMPORT,
            StandardNames.XS_DEFAULT_OPEN_CONTENT, StandardNames.XS_SIMPLE_TYPE, StandardNames.XS_ANNOTATION,
            StandardNames.XS_ATTRIBUTE_GROUP, StandardNames.XS_COMPLEX_TYPE, StandardNames.XS_INCLUDE,
            StandardNames.XS_notation, StandardNames.XS_ELEMENT, StandardNames.XS_ATTRIBUTE,
            StandardNames.XS_GROUP, StandardNames.XS_REDEFINE};
    private final static int[] state2targets = {2, 2, 3, 1, 2, 1, 1, 2, 1, 1, 1, 1, 2};

    private final static int[] state3terms = {StandardNames.XS_ELEMENT, StandardNames.XS_GROUP,
            StandardNames.XS_SIMPLE_TYPE, StandardNames.XS_COMPLEX_TYPE, StandardNames.XS_notation,
            StandardNames.XS_ATTRIBUTE_GROUP, StandardNames.XS_ATTRIBUTE, StandardNames.XS_ANNOTATION};
    private final static int[] state3targets = {1, 1, 1, 1, 1, 1, 1, 5};

    private final static int[] state4terms = {StandardNames.XS_ELEMENT, StandardNames.XS_GROUP,
            StandardNames.XS_SIMPLE_TYPE, StandardNames.XS_COMPLEX_TYPE, StandardNames.XS_notation,
            StandardNames.XS_ATTRIBUTE_GROUP, StandardNames.XS_ATTRIBUTE, StandardNames.XS_ANNOTATION};
    private final static int[] state4targets = {1, 1, 1, 1, 1, 1, 1, 4};

    private final static int[] state5terms = {StandardNames.XS_ELEMENT, StandardNames.XS_GROUP,
            StandardNames.XS_SIMPLE_TYPE, StandardNames.XS_COMPLEX_TYPE, StandardNames.XS_notation,
            StandardNames.XS_ATTRIBUTE_GROUP, StandardNames.XS_ATTRIBUTE, StandardNames.XS_ANNOTATION};
    private final static int[] state5targets = {1, 1, 1, 1, 1, 1, 1, 5};

    private final static int[][] stateTerms = {
            state0terms, state1terms, state2terms, state3terms, state4terms, state5terms};
    private final static int[][] stateTargets = {
            state0targets, state1targets, state2targets, state3targets, state4targets, state5targets};

    @Override
    protected boolean isFinalState(int state) {
        return state == 0 || state == 1 || state == 2 || state == 3 || state == 4 || state == 5;
    }

    /**
     * Process child elements after validation
     *
     * @param compiler used for reporting errors
     */

    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {
        for (NodeInfo child : children()) {
            int childfp = child.getFingerprint();

            switch (childfp) {
                case StandardNames.XS_ATTRIBUTE:
                    AttributeDecl att = ((XSDAttribute) child).getAttributeDeclaration();
                    if (att == null) {
                        // error already reported
                        break;
                    }
                    if (schema.getAttributeDecl(att.getComponentName()) != null) {
                        duplicate("attribute declaration", att.getDisplayName());
                    }
                    schema.addAttributeDecl(att);
                    break;

                case StandardNames.XS_ATTRIBUTE_GROUP:
                    AttributeGroupDecl ag = ((XSDAttributeGroup) child).getAttributeGroupDecl();
                    if (ag == null) {
                        // error already reported
                        break;
                    }
                    if (schema.getAttributeGroup(ag.getAttributeGroupName()) != null) {
                        duplicate("attribute group", ag.getDisplayName());
                    } else {
                        schema.addAttributeGroup(ag);
                    }
                    break;

                case StandardNames.XS_COMPLEX_TYPE:
                    UserComplexType complexType = ((XSDComplexType) child).getComplexType();
                    int fpt = complexType.getFingerprint();
                    if (schema.getSchemaType(complexType.getTypeName()) != null && fpt > 1023) {
                        duplicate("complex type", complexType.getName());
                    } else {
                        schema.addType(complexType);
                    }
                    break;

                case StandardNames.XS_DEFAULT_OPEN_CONTENT:
                    break;

                case StandardNames.XS_SIMPLE_TYPE:
                    SimpleTypeDefinition typeDef;
                    try {
                        typeDef = ((XSDSimpleType) child).getSimpleTypeDefinition();
                    } catch (SchemaException e) {
                        error(e.getMessage());
                        return;
                    }
                    int fingerprint = typeDef.getFingerprint();
                    if (schema.getSchemaType(typeDef.getTypeName()) != null && fingerprint > 1023) {
                        duplicate("simple type", typeDef.getDisplayName());
                    } else {
                        schema.addType(typeDef);
                    }
                    break;

                case StandardNames.XS_ELEMENT:
                    ElementDecl element = ((XSDElement) child).getElementDeclaration();
                    String elname = element.getName();
                    if (schema.getElementDecl(element.getComponentName()) != null) {
                        duplicate("element declaration", elname);
                    }
                    schema.addElementDecl(element);
                    break;

                case StandardNames.XS_GROUP:
                    ModelGroupDefinition group = ((XSDGroup) child).getGroupDeclaration();
                    //if (!group.isReference()) {
                    String groupName = group.getName();
                    if (groupName == null) {
                        // error already reported
                    } else {
                        if (schema.getGroup(group.getModelGroupName()) != null) {
                            duplicate("group", groupName);
                        } else {
                            schema.addGroup(group);
                        }
                    }
                    //}
                    break;

                case StandardNames.XS_notation:
                    Notation n = ((XSDNotation) child).getNotation();
                    if (schema.getNotation(n.getNotationName()) != null) {
                        duplicate("notation", n.getNotationName().getDisplayName());
                    } else {
                        schema.addNotation(n);
                    }
                    break;


                case StandardNames.XS_INCLUDE:
                    PreparedSchema includedSchema = ((XSDInclude) child).getIncludedSchema();
                    if (includedSchema == null) {
                        // do nothing: the include was redundant
                    } else if (compiler.getErrorCount() > 0) {
                        ((XSDInclude) child).error(
                                "Errors were found in the included schema document");
                    } else {
                        // the include was successful
                        includedSchema.copyTo(schema, compiler);
                        if (compiler.getErrorCount() > 0) {
                            ((XSDInclude) child).error(
                                    "The included schema document resulted in conflicting components");
                        }
                    }
                    break;

                case StandardNames.XS_IMPORT:
                    PreparedSchema importedSchema = ((XSDImport) child).getImportedSchema();
                    if (importedSchema != null) {
                        // the import was successful
                        importedSchema.copyTo(schema, compiler);
                        if (compiler.getErrorCount() > 0) {
                            ((XSDImport) child).error(
                                    "The imported schema document resulted in conflicting components");
                        }
                    }
                    break;

                case StandardNames.XS_REDEFINE:
                    PreparedSchema redefinedSchema = ((XSDRedefine) child).getRedefinedSchema();
                    if (redefinedSchema != null) {
                        redefinedSchema.copyTo(schema, compiler);
                        if (compiler.getErrorCount() > 0) {
                            ((XSDRedefine) child).error(
                                    "The redefined schema document resulted in conflicting components");
                        }
                    }
                    break;
            }
        }
    }


    /**
     * Get the target namespace of this schema document.
     *
     * @return {@link NamespaceUri#NULL} for the no-namespace case
     */

    /*@Nullable*/
    public NamespaceUri getTargetNamespace() {
        if (targetNamespace == null) {
            // can happen on error paths
            String tn = getAttributeValue(NamespaceUri.NULL, "targetNamespace");
            targetNamespace = tn == null ? NamespaceUri.NULL : NamespaceUri.of(tn);
        }
        return targetNamespace;
    }

    /**
     * Returns the attributeFormDefault property of this Schema.
     *
     * @return the attributeFormDefault property of this Schema, or null
     *         if no default Form was set. If no default Form has been set, the
     *         user should assume Form.Unqualified. The value is returned as a string,
     *         "qualified" or "unqualified".
     */

    public String getAttributeFormDefault() {
        return attributeFormDefault;
    }

    /**
     * Returns the default BlockList for this Schema.
     *
     * @return the default BlockList for this Schema, as an integer with bit-significant
     *         fields
     */
    public int getBlockDefault() {
        return blockDefault;
    }


    /**
     * Returns the elementFormDefault property of this Schema.
     *
     * @return the elementFormDefault property of this Schema. The value is
     *         returned as a string ("qualified" or "unqualified"), or null
     *         if no default Form was set. If no default Form has been set, the
     *         user should assume "unqualified".
     */

    public String getElementFormDefault() {
        return elementFormDefault;
    }

    /**
     * Returns the default value of "final" for this Schema.
     *
     * @return final the default value of "final" for this Schema.
     */

    public int getFinalDefault() {
        return finalDefault;
    }

    /**
     * Get the default open content wildcard, if any
     *
     * @return the default open content wildcard if there is one, otherwise null
     */

    public Wildcard getDefaultOpenContentWildcard() {
        return openContentWildcard;
    }

    /**
     * Get the default open content mode, as a string (interleave or suffix)
     *
     * @return the default open content mode, or null if none is defined
     */

    public String getDefaultOpenContentMode() {
        return openContentMode;
    }

    /**
     * Ask if the default open content wildcard applies to types that would otherwise
     * have an EMPTY content model
     *
     * @return true if open content applies by default to empty types
     */

    public boolean defaultOpenContentAppliesToEmpty() {
        return openContentAppliesToEmpty;
    }

    /**
     * Sets the attributeFormDefault property of this Schema.
     *
     * @param attributeFormDefault the Form value of the attributeFormDefault
     *                             property for this Schema, as a string, "qualified" or "unqualified".
     *                             The caller is responsible for validating the value.
     */

    public void setAttributeFormDefault(String attributeFormDefault) {
        this.attributeFormDefault = attributeFormDefault;
    }


    /**
     * Sets the default Block values for this Schema.
     *
     * @param value the default Block values to set for this Schema, as
     *              a bit-significant integer.
     */

    public void setBlockDefault(int value) {
        blockDefault = value;
    }

    /**
     * Sets the elementFormDefault property of this Schema.
     *
     * @param elementFormDefault the Form value of the elementFormDefault
     *                           property for this Schema.
     */

    public void setElementFormDefault(String elementFormDefault) {
        this.elementFormDefault = elementFormDefault;
    }

    /**
     * Set the default "final" list for this Schema.
     *
     * @param value the default final values to set for this Schema, as a bit-significant
     *              integer
     */

    public void setFinalDefault(int value) {
        finalDefault = value;
    }

    /**
     * Set the open content wildcard and mode
     *
     * @param wildcard       the default open content wildcard
     * @param mode           the default open content mode, as a string
     * @param appliesToEmpty true if the default open content wildcard applies to types that would otherwise
     *                       have variety empty
     */

    public void setDefaultOpenContent(Wildcard wildcard, String mode, boolean appliesToEmpty) {
        openContentWildcard = wildcard;
        openContentMode = mode;
        openContentAppliesToEmpty = appliesToEmpty;
    }

    /**
     * Get the value of the defaultXPathDefaultNamespace attribute defined on this element
     *
     * @return the value of the defaultXPathDefaultNamespace attribute, or "##local" if no value
     *         has been defined for the attribute
     */

    public String getXPathDefaultNamespace() {
        return xpathDefaultNamespace;
    }

    /**
     * Get a reference to the default attribute group
     *
     * @return a reference to the default attribute group if there is one, or null otherwise
     */

    public AttributeGroupReference getDefaultAttributeGroupReference() {
        if (defaultAttributeGroupReference != null) {
            return defaultAttributeGroupReference;
        }
        if (defaultAttributeGroup == null) {
            return null;
        }
        defaultAttributeGroupReference = new AttributeGroupReference(
                (EnterpriseConfiguration) getConfiguration(), defaultAttributeGroup);
        return defaultAttributeGroupReference;
    }

    /**
     * Get the number of errors detected in this schema document
     *
     * @return the number of errors detected
     */

    public int getErrorCount() {
        return errorCount;
    }

    /**
     * Increment the number of errors detected in this schema document
     */

    public void incrementErrorCount() {
        errorCount++;
    }

    /**
     * Declare a global parameter for XPath expressions in this schema document (Saxon extension)
     *
     * @param param the details of the parameter
     * @throws SchemaException if the variable name is a duplicate
     */

    public void addGlobalParam(GlobalParam param) throws SchemaException {
        GlobalParam prev = params.put(param.getVariableQName(), param);
        if (prev != null) {
            error("Duplicate declaration of parameter " + param.getVariableQName().getEQName());
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
}

// Copyright (c) 2004-2023 Saxonica Limited