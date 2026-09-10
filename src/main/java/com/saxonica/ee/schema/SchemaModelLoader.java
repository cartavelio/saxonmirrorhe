package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.fsa.DeterminizedState;
import com.saxonica.ee.schema.fsa.Edge;
import com.saxonica.ee.schema.fsa.FiniteStateMachine;
import com.saxonica.ee.schema.sdoc.SelectionParser;
import com.saxonica.ee.schema.sdoc.XSDAssert;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.instruct.SavedNamespaceContext;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.sxpath.XPathEvaluator;
import net.sf.saxon.sxpath.XPathExpression;
import net.sf.saxon.sxpath.XPathVariable;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.util.AttributeCollectionImpl;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.NotationValue;
import net.sf.saxon.value.QNameValue;
import net.sf.saxon.value.Whitespace;

import javax.xml.transform.Source;
import java.util.*;

/**
 * This class allows a compiled schema model in SCM format
 * to be loaded into memory. At present the data is accepted without validation.
 */

public class SchemaModelLoader extends ProxyReceiver {

    private final EnterpriseConfiguration config;
    private final PreparedSchema schema;
    private SchemaCompiler compiler;
    private final HashMap<String, SchemaComponent> components = new HashMap<>(100);
    private final HashSet<SchemaComponent> anonComponents = new HashSet<>(100);
    private final List<Fixup> fixups = new ArrayList<>();
    private final Stack<SCMNodeHandler> handlerStack = new Stack<>();
    private NamespaceResolver namespaceResolver;
    private NodeName elementName;
    private final AttributeCollectionImpl attributes;
    private final List<PendingEdge> pendingEdges = new ArrayList<>();
    private String dmk = null;

    /**
     * Create a schema model loader
     *
     * @param config the Saxon configuration. This must be a schema-aware configuration.
     *               This SchemaModelLoader creates a PreparedSchema but does not directly load it into the
     *               Configuration cache; that is the job of the caller.
     * @throws IllegalArgumentException if the configuration is not schema-aware
     */

    public SchemaModelLoader(Configuration config) {
        super(new Sink(config.makePipelineConfiguration()));
        getPipelineConfiguration().getParseOptions().setSchemaValidationMode(Validation.SKIP);
        //config.checkLicensedFeature(Configuration.LicenseFeature.SCHEMA_VALIDATION, "schema processing", -1);
        this.config = (EnterpriseConfiguration) config;
        schema = new PreparedSchema((EnterpriseConfiguration) config);
        attributes = new AttributeCollectionImpl(config);
    }

    /**
     * Load a schema from a serialized component model
     *
     * @param source the XML source of the serialized component model
     * @return the corresponding PreparedSchema containing the deserialized schema components
     * @throws XPathException if an error occurs
     */

    public PreparedSchema load(Source source) throws XPathException {
        PipelineConfiguration pipe = config.makePipelineConfiguration();
        pipe.getParseOptions().setSchemaValidationMode(Validation.SKIP);
        pipe.setHostLanguage(Configuration.XML_SCHEMA);
        NamespaceReducer reducer = new NamespaceReducer(this);
        namespaceResolver = reducer;
        compiler = config.newSchemaCompiler();
        CheckSumFilter filter = new CheckSumFilter(reducer);
        filter.setCheckExistingChecksum(true);
        Sender.send(source, filter, null);
        if (!filter.isChecksumCorrect()) {
            // Don't worry about this if there's no checksum and no DMK
            if (filter.isChecksumFound() || dmk != null) {
                throw new XPathException("Invalid checksum in compiled schema file");
            }
        }
        try {
            for (Fixup fixup : fixups) {
                fixup.apply();
            }

            // Fix up the edges in all finite state machines
            for (PendingEdge pe : pendingEdges) {
                DeterminizedState start = (DeterminizedState) pe.machine.getState(pe.fromState);
                DeterminizedState end = (DeterminizedState) pe.machine.getState(pe.toState);
                if (pe.termId.equals("##afterMax")) {
                    Term term = Edge.MaxOccursTerm.getInstance();
                    Edge edge = new Edge();
                    edge.setTerm(term);
                    edge.setTargetState(end);
                    start.setMaxTransition(edge);
                } else {
                    Term term = (Term) components.get(pe.termId);
                    if (term == null) {
                        throw new SchemaException("Unresolved reference to term " + pe.termId);
                    }
                    Edge edge = new Edge();
                    edge.setTargetState(end);
                    edge.setTerm(term);
                    //edge.setCounting(pe.countFrom);
                    if (term instanceof ElementDecl) {
                        start.addSpecificTransition(edge, compiler);
                    } else {
                        start.addWildcardTransition(compiler, edge);
                    }
                }
            }
            // Fix up substitution groups (must be done before all-groups are elaborated)
            compiler.setPreparedSchema(schema);
            for (SchemaComponent o : components.values()) {
                if (o instanceof ElementDecl && ((ElementDecl)o).isGlobal()) {
                    ((ElementDecl) o).fixupSubstitutionGroup(compiler);
                }
            }

            // Perform "elaboration" of all components, computing derived information as necessary
            for (SchemaComponent o : components.values()) {
                if (o instanceof UserSchemaComponent) {
                    ((UserSchemaComponent) o).elaborate(compiler);
                }
            }

            for (SchemaComponent o : anonComponents) {
                if (o instanceof UserSchemaComponent) {
                    ((UserSchemaComponent) o).elaborate(compiler);
                }
            }
        } catch (SchemaException err) {
            throw new XPathException(err);
        }
        return schema;
    }

    /**
     * Notify the start of an element
     * @param nameCode   integer code identifying the name of the element within the name pool.
     * @param typeCode   integer code identifying the element's type within the name pool.
     * @param location   originating location of the event
     * @param properties for future use. Should be set to zero.
     */

    public void startElement(NodeName nameCode, SchemaType typeCode, Location location, int properties) throws XPathException {
//        System.err.println("start " + config.getNamePool().getDisplayName(nameCode));
        elementName = nameCode;
        attributes.clear();
        if (handlerStack.isEmpty()) {
            handlerStack.push(new SchemaHandler());
        } else {
            SCMNodeHandler next = handlerStack.peek().handleChildElement(elementName.getLocalPart());
            if (next == null) {
                throw new XPathException("No handler defined for element " + elementName.getLocalPart() +
                        " in " + handlerStack.peek().getClass().getName());
            }
            handlerStack.push(next);
        }
    }


    /**
     * Notify the start of the content, that is, the completion of all attributes and namespaces.
     * Note that the initial receiver of output from XSLT instructions will not receive this event,
     * it has to detect it itself. Note that this event is reported for every element even if it has
     * no attributes, no namespaces, and no content.
     */


    public void startContent() throws XPathException {
        //System.err.println("startContent ");
        try {
            handlerStack.peek().processElementStart(
                    elementName.getLocalPart(), attributes);
        } catch (SchemaException e) {
            throw new XPathException(e);
        }
    }

    /**
     * Notify an attribute. Attributes are notified after the startElement event, and before any
     * children. Namespaces and attributes may be intermingled.
     *
     * @param nameCode   The name of the attribute
     * @param typeCode   The type of the attribute
     * @param locationId the location of the node in the source, or of the instruction that created it
     * @param properties Bit significant value. The following bits are defined:
     *                   <dl>
     *                   <dd>DISABLE_ESCAPING</dd>    <dt>Disable escaping for this attribute</dt>
     *                   <dd>NO_SPECIAL_CHARACTERS</dd>      <dt>Attribute value contains no special characters</dt>
     *                   </dl>
     * @throws IllegalStateException: attempt to output an attribute when there is no open element
     *                                start tag
     */

    public void attribute(NodeName nameCode, SimpleType typeCode, CharSequence value, Location locationId, int properties) {
//        if (config.getNamePool().getLocalName(nameCode).equals("id")) {
//            System.err.println("id = " + value);
//        }
        attributes.addAttribute(nameCode, typeCode, value.toString(), locationId, properties);
    }

    /**
     * Character data
     */

    public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
        if (!Whitespace.isWhite(chars)) {
            throw new XPathException("Character content not allowed in SCM document");
        }
    }

    /**
     * End of element
     */

    public void endElement() throws XPathException {
        SCMNodeHandler handler = handlerStack.pop();
        handler.processElementEnd();
    }

    /**
     * Interface for an SCM element handler. This handles the attributes and children of a specific element
     * in the SCM serialization.
     */

    private interface SCMNodeHandler {

        /**
         * Process an element start tag together with its attributes
         *
         * @param localName  the name of the element to be processed
         * @param attributes the attributes of the element being processed
         * @throws SchemaException if the data is invalid
         */

        void processElementStart(String localName, AttributeCollection attributes) throws SchemaException;

        /**
         * Handle a child element of the element currently being processed.
         *
         * @param localName the local name of the child element
         * @return an SCMNodeHandler which will be used to process the child element
         */

        SCMNodeHandler handleChildElement(String localName);

        /**
         * Process the element end tag
         */

        void processElementEnd();

    }

    /**
     * Handle the schema element (the outermost element of the SCM document)
     */

    private class SchemaHandler implements SCMNodeHandler {

        public void processElementStart(String localName, AttributeCollection attributes) throws SchemaException {
            String xsdVersion = attributes.getValue("", "xsdVersion");
            if (xsdVersion != null &&
                    xsdVersion.compareTo(config.getConfigurationProperty(Feature.XSD_VERSION)) > 0) {
                throw new SchemaException("Imported schema component model requires XSD version " + xsdVersion);
            }
            dmk = attributes.getValue("", "dmk");
            if (dmk != null) {
                schema.setLocalLicenseId(config.registerLocalLicense(dmk));
            }
        }

        public SCMNodeHandler handleChildElement(String localName) {
            switch (localName) {
                case "element":
                    return new ElementHandler();
                case "attribute":
                    return new AttributeHandler();
                case "complexType":
                    return new ComplexTypeHandler();
                case "simpleType":
                    return new SimpleTypeHandler();
                case "attributeGroup":
                    return new AttributeGroupHandler();
                case "modelGroupDefinition":
                    return new ModelGroupDefinitionHandler();
                case "notation":
                    return new NotationHandler();
                case "wildcard":
                    return new WildcardHandler();
                case "unique":
                    return new IdentityConstraintHandler();
                case "key":
                    return new IdentityConstraintHandler();
                case "keyref":
                    return new IdentityConstraintHandler();
            }
            return null;
        }

        public void processElementEnd() {
        }
    }


    /**
     * Handle an element declaration in the SCM schema document
     */

    private class ElementHandler implements SCMNodeHandler {

        private ElementDecl elementDecl;

        public SCMNodeHandler handleChildElement(String localName) {
            switch (localName) {
                case "identityConstraint":
                    return new IdentityConstraintReferenceHandler(elementDecl);
                case "substitutionGroupAffiliation":
                    return new SubstitutionGroupAffiliationHandler(elementDecl);
                case "alternativeType":
                    return new AlternativeTypeHandler(elementDecl);
                case "fixed":
                case "default":
                    return new ValueConstraintHandler(elementDecl, localName);
                default:
                    return null;
            }
        }

        public void processElementStart(String localName, AttributeCollection attributes) throws SchemaException {
            String id = attributes.getValue("", "id");
            String name = attributes.getValue("", "name");
            String namespace = attributes.getValue("", "targetNamespace");
            boolean isGlobal = toBoolean(attributes.getValue("", "global"));
            boolean isNillable = toBoolean(attributes.getValue("", "nillable"));
            boolean isAbstract = toBoolean(attributes.getValue("", "abstract"));
            String type = attributes.getValue("", "type");
            String finalAtt = attributes.getValue("", "final");
            String defaultValue = attributes.getValue("", "default");
            int finalSet = parseFinalOrBlock("final",
                    finalAtt,
                    SchemaType.DERIVATION_EXTENSION |
                            SchemaType.DERIVATION_RESTRICTION |
                            SchemaType.DERIVE_BY_SUBSTITUTION);
            String blockAtt = attributes.getValue("", "block");
            int blockSet = parseFinalOrBlock("block",
                    blockAtt,
                    SchemaType.DERIVATION_EXTENSION |
                            SchemaType.DERIVATION_RESTRICTION |
                            SchemaType.DERIVE_BY_SUBSTITUTION);
            elementDecl = new ElementDecl(config, isGlobal, null);
            elementDecl.setGeneratedId(id);
            components.put(id, elementDecl);
            elementDecl.setElementName(new StructuredQName("", namespace, name));
            elementDecl.setAbstract(isAbstract);
            elementDecl.setNillable(isNillable);
            elementDecl.setBlock(blockSet);
            elementDecl.setFinalProhibitions(finalSet);
            if (defaultValue != null) {
                ValueConstraint vc = new ValueConstraint(ValueConstraint.Variety.DEFAULT, defaultValue);
                elementDecl.setValueConstraint(vc);
            }
            if (type != null) {
                if (type.startsWith("#")) {
                    int fp = config.getNamePool().getFingerprint(NamespaceConstant.SCHEMA, type.substring(1));
                    elementDecl.setTypeReference(new TypeReference(fp, config, null));
                } else {
                    fixups.add(new Fixup(elementDecl, type, FIXUP_ELEMENT_TYPE));
                }
            }
            if (isGlobal) {
                schema.addElementDecl(elementDecl);
            }

        }

        public void processElementEnd() {
        }
    }

    /**
     * Handle a substitution group affiliation
     */

    public class SubstitutionGroupAffiliationHandler implements SCMNodeHandler {

        private final ElementDecl elementDecl;

        /**
         * Create a substitution group affiliation for a given element declaration
         *
         * @param decl the element declaration
         */

        public SubstitutionGroupAffiliationHandler(ElementDecl decl) {
            elementDecl = decl;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        public void processElementStart(String localName, AttributeCollection attributes) {
            String head = attributes.getValue("", "ref");
            if (head != null) {
                fixups.add(new Fixup(elementDecl, head, FIXUP_ELEMENT_SUBSTITUTION_GROUP));
            }
        }

        public void processElementEnd() {
        }
    }

    /**
     * Handle an alternative type for an element declaration
     */

    public class AlternativeTypeHandler implements SCMNodeHandler {

        private final ElementDecl elementDecl;

        /**
         * Create an AlternativeTypeHandler for a given element declaration
         *
         * @param decl the element declaration
         */

        public AlternativeTypeHandler(ElementDecl decl) {
            elementDecl = decl;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        public void processElementStart(String localName, AttributeCollection attributes) throws SchemaException {
            String xpathText = attributes.getValue("", "test");
            String xmlBase = attributes.getValue(NamespaceConstant.XML, "base");
            String defaultNamespace = attributes.getValue("", "defaultNamespace");
            String type = attributes.getValue("", "type");
            XPathEvaluator evaluator = new XPathEvaluator(config);
            IndependentContext env = (IndependentContext) evaluator.getStaticContext();
            env.setNamespaceResolver(namespaceResolver);
            env.setDefaultElementNamespace(defaultNamespace);
            env.setBaseURI(xmlBase);
            XPathExpression condition;
            try {
                condition = evaluator.createExpression(xpathText);
            } catch (XPathException err) {
                throw new SchemaException(err);
            }
            TypeAlternative alt = new TypeAlternative(config, condition, xpathText);
            alt.setBaseURI(xmlBase);
            alt.setDefaultElementNamespace(defaultNamespace);
            alt.setNamespaceContext(new SavedNamespaceContext(namespaceResolver));
            alt.setParameters(new HashMap<>());
            if (type.startsWith("#")) {
                int fp = config.getNamePool().getFingerprint(NamespaceConstant.SCHEMA, type.substring(1));
                alt.setTypeReference(new TypeReference(fp, config, null));
            } else {
                fixups.add(new Fixup(alt, type, FIXUP_ALTERNATIVE_TYPE));
            }
            elementDecl.addTypeAlternative(alt);
        }

        public void processElementEnd() {
        }
    }

    /**
     * Handle an identity constraint reference in an element declaratoin
     */

    private class IdentityConstraintReferenceHandler implements SCMNodeHandler {

        private final ElementDecl container;
        private IdentityConstraintReference constraintRef;

        public IdentityConstraintReferenceHandler(ElementDecl container) {
            this.container = container;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        public void processElementStart(String localName, AttributeCollection attributes) {

            String ref = attributes.getValue("", "ref");

            constraintRef = new IdentityConstraintReference();
            container.addIdentityConstraint(constraintRef);
            fixups.add(new Fixup(constraintRef, ref, FIXUP_IDENTITY_CONSTRAINT_REF));

        }

        public void processElementEnd() {
        }
    }


    /**
     * Handle an identity constraint (key, keyref, unique)
     */

    public class IdentityConstraintHandler implements SCMNodeHandler {

        private IdentityConstraint identityConstraint;

        /**
         * Create an IdentityConstraintHandler
         */

        public IdentityConstraintHandler() {
        }

        public SCMNodeHandler handleChildElement(String localName) {
            switch (localName) {
                case "selector":
                    return new SelectorHandler(identityConstraint);
                case "field":
                    return new SelectorHandler(identityConstraint);
                default:
                    return null;
            }
        }

        public void processElementStart(String localName, AttributeCollection attributes) {
            String id = attributes.getValue("", "id");
            String name = attributes.getValue("", "name");
            String namespace = attributes.getValue("", "targetNamespace");
            String key = attributes.getValue("", "key");
            StructuredQName nameCode = new StructuredQName("", namespace, name);
            IdentityConstraint ic;
            //int kind;
            switch (localName) {
                case "unique":
                    ic = new Unique();
                    components.put(id, ic);
                    //kind = StandardNames.XS_UNIQUE;
                    break;
                case "key":
                    ic = new Key();
                    components.put(id, ic);
                    //kind = StandardNames.XS_KEY;
                    break;
                case "keyref":
                    ic = new KeyRef();
                    components.put(id, ic);
                    fixups.add(new Fixup(ic, key, FIXUP_KEYREF_KEY));
                    //kind = StandardNames.XS_KEYREF;
                    break;
                default:
                    throw new AssertionError("unknown IdentityConstraint " + localName);
            }
            ic.setConfiguration(config);
            ic.setConstraintName(nameCode);
            identityConstraint = ic;
        }

        public void processElementEnd() {
        }
    }

    /**
     * Handle a selector or field within an identity constraint (key, keyref, unique)
     */

    public class SelectorHandler implements SCMNodeHandler {

        private final IdentityConstraint identityConstraint;

        /**
         * Create a SelectorHandler for a given IdentityConstraint
         *
         * @param identityConstraint the constraint
         */

        public SelectorHandler(IdentityConstraint identityConstraint) {
            this.identityConstraint = identityConstraint;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        public void processElementStart(String localName, AttributeCollection attributes) throws SchemaException {
            String xpath = attributes.getValue("", "xpath");
            boolean isField = localName.equals("field");
            String defaultNamespace = attributes.getValue("", "defaultNamespace");
            IndependentContext env = new IndependentContext(config);
            env.setNamespaceResolver(new SavedNamespaceContext(namespaceResolver));
            env.setDefaultElementNamespace(defaultNamespace);
            //env.setBaseURI(xmlBase);
            Pattern selector;
            try {
                selector = new SelectionParser().parseSelector(xpath, env, isField);
            } catch (XPathException err) {
                throw new SchemaException(err);
            }
            if (isField) {
                IdentityField idf = new IdentityField(xpath);
                idf.setSelection(selector);
                idf.setNamespaceContext(new SavedNamespaceContext(namespaceResolver));
                idf.setXPathDefaultNamespace(defaultNamespace);
                identityConstraint.addField(idf);
            } else {
                IdentitySelector ids = new IdentitySelector(xpath);
                ids.setSelection(selector);
                ids.setNamespaceContext(new SavedNamespaceContext(namespaceResolver));
                ids.setXPathDefaultNamespace(defaultNamespace);
                identityConstraint.setSelector(ids);
            }
        }

        public void processElementEnd() {
        }
    }


    /**
     * Handle an attribute declaration in the SCM schema document
     */

    private class AttributeHandler implements SCMNodeHandler {

        private AttributeDecl attributeDecl;

        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("fixed") || localName.equals("default")) {
                return new ValueConstraintHandler(attributeDecl, localName);
            } else {
                return null;
            }
        }

        public void processElementStart(String localName, AttributeCollection attributes) {
            String id = attributes.getValue("", "id");
            String name = attributes.getValue("", "name");
            String namespace = attributes.getValue("", "targetNamespace");
            StructuredQName attributeName = null;
            if (name != null) {
                attributeName = new StructuredQName("", namespace, name);
            }
            boolean isGlobal = toBoolean(attributes.getValue("", "global"));
            boolean isInheritable = toBoolean(attributes.getValue("", "inheritable"));
            String type = attributes.getValue("", "type");
            String defaultValue = attributes.getValue("", "default");

            attributeDecl = new AttributeDecl(config, isGlobal);
            components.put(id, attributeDecl);
            attributeDecl.setAttributeName(attributeName, config.getNamePool().allocateFingerprint(namespace, name));
            attributeDecl.setInheritable(isInheritable);

            if (defaultValue != null) {
                ValueConstraint vc = new ValueConstraint(ValueConstraint.Variety.DEFAULT, defaultValue);
                attributeDecl.setValueConstraint(vc);
            }

            if (type != null) {
                if (type.startsWith("#")) {
                    int fp = config.getNamePool().getFingerprint(NamespaceConstant.SCHEMA, type.substring(1));
                    attributeDecl.setTypeReference(new TypeReference(fp, config, null));
                } else {
                    fixups.add(new Fixup(attributeDecl, type, FIXUP_ATTRIBUTE_TYPE));
                }
            }

            if (isGlobal) {
                schema.addAttributeDecl(attributeDecl);
            }

        }

        public void processElementEnd() {
        }

    }

    /**
     * Handle a complex type definition in the SCM schema document. This may be a named complex type
     * or an anonymous type.
     */

    private class ComplexTypeHandler implements SCMNodeHandler {

        UserComplexType complexType;

        public SCMNodeHandler handleChildElement(String localName) {
            switch (localName) {
                case "openContent":
                    return new OpenContentHandler(complexType);
                case "attributeUse":
                    return new AttributeUseHandler(complexType);
                case "attributeWildcard":
                    return new AttributeWildcardHandler(complexType);
                case "modelGroup":
                    return new ParticleHandler(complexType);
                case "elementParticle":
                    return new ParticleHandler(complexType);
                case "elementWildcard":
                    return new ParticleHandler(complexType);
                case "modelGroupParticle":
                    return new ParticleHandler(complexType);
                case "finiteStateMachine":
                    return new FiniteStateMachineHandler(complexType);
                case "assertion":
                    return new AssertionHandler(complexType);
                default:
                    return null;
            }
        }

        public void processElementStart(String localName, AttributeCollection attributes) throws SchemaException {
            String id = attributes.getValue("", "id");
            String name = attributes.getValue("", "name");
            String namespace = attributes.getValue("", "targetNamespace");
            int fp;
            if (name == null) {
                fp = config.getNamePool().allocateFingerprint(NamespaceConstant.ANONYMOUS, id);
                name = id;
                namespace = NamespaceConstant.ANONYMOUS;
            } else {
                fp = config.getNamePool().allocateFingerprint(namespace, name);
            }
            String base = attributes.getValue("", "base");
            String variety = attributes.getValue("", "variety");
            boolean isAbstract = toBoolean(attributes.getValue("", "abstract"));
            String derivation = attributes.getValue("", "derivationMethod");
            String simpleType = attributes.getValue("", "simpleType");
            complexType = new UserComplexType(config, null);
            complexType.setEmptyCombinedAttributeGroup();
            //complexType.setSaxonDebug(true);
            components.put(id, complexType);
            complexType.setTypeName(new StructuredQName("", namespace, name), fp);
            if (base != null) {
                if (base.startsWith("#")) {
                    int basefp = config.getNamePool().getFingerprint(NamespaceConstant.SCHEMA, base.substring(1));
                    complexType.setBaseTypeReference(new TypeReference(basefp, config, null));
                } else {
                    fixups.add(new Fixup(complexType, base, FIXUP_COMPLEX_TYPE_BASE_TYPE));
                }
            }
            int varietyCode = ComplexType.VARIETY_ELEMENT_ONLY;
            switch (variety) {
                case "mixed":
                    varietyCode = ComplexType.VARIETY_MIXED;
                    break;
                case "empty":
                    varietyCode = ComplexType.VARIETY_EMPTY;
                    break;
                case "element-only":
                    varietyCode = ComplexType.VARIETY_ELEMENT_ONLY;
                    break;
                case "simple":
                    varietyCode = ComplexType.VARIETY_SIMPLE;
                    break;
            }
            complexType.setVariety(varietyCode);
            complexType.setAbstract(isAbstract);
            complexType.setDerivationMethodName(derivation);
            if (simpleType != null) {
                if (simpleType.startsWith("#")) {
                    int typefp = config.getNamePool().getFingerprint(NamespaceConstant.SCHEMA, simpleType.substring(1));
                    complexType.setSimpleContentTypeReference(new TypeReference(typefp, config, null));
                } else {
                    fixups.add(new Fixup(complexType, simpleType, FIXUP_COMPLEX_TYPE_SIMPLE_TYPE));
                }
            }
            if (name == null) {
                schema.addAnonymousType(complexType);
            } else {
                schema.addType(complexType);
            }
            String blockAtt = attributes.getValue("", "block");
            int blockSet = parseFinalOrBlock("block",
                    blockAtt,
                    SchemaType.DERIVATION_EXTENSION |
                            SchemaType.DERIVATION_RESTRICTION);
            complexType.setBlock(blockSet);
            String finalAtt = attributes.getValue("", "final");
            int finalSet = parseFinalOrBlock("final",
                    finalAtt,
                    SchemaType.DERIVATION_EXTENSION |
                            SchemaType.DERIVATION_RESTRICTION);
            complexType.setFinalProhibitions(finalSet);
        }

        public void processElementEnd() {
        }

    }

    /**
     * Handle a simple type definition in the SCM schema document. This may be a named simple type or an
     * anonymous simple type
     */

    private class SimpleTypeHandler implements SCMNodeHandler {

        private UserSimpleType simpleType;

        public SCMNodeHandler handleChildElement(String localName) {
            if (Facet.isFacetName(localName, getConfiguration().getXsdVersion()) || localName.equals("assert")) {
                return new FacetHandler(simpleType);
            } else if (localName.equals("preprocess")) {
                return new PreprocessHandler(simpleType);
            } else {
                return null;
            }
        }

        public void processElementStart(String localName, AttributeCollection attributes) throws SchemaException {
            String id = attributes.getValue("", "id");
            String name = attributes.getValue("", "name");
            String namespace = attributes.getValue("", "targetNamespace");
            int fingerprint;
            if (name == null) {
                fingerprint = config.getNamePool().allocateFingerprint(NamespaceConstant.ANONYMOUS, id);
                name = id;
                namespace = NamespaceConstant.ANONYMOUS;
                // TODO: this won't be unique if loading several SCM schemas
            } else {
                fingerprint = config.getNamePool().allocateFingerprint(namespace, name);
            }
            String base = attributes.getValue("", "base");
            String variety = attributes.getValue("", "variety");
            String itemType = attributes.getValue("", "itemType");
            String memberTypes = attributes.getValue("", "memberTypes");
            //String primitiveType = attributes.getValue("", "primitiveType");
            if ("atomic".equals(variety)) {
                simpleType = new UserAtomicType(config);
            } else if ("list".equals(variety)) {
                simpleType = new UserListType(config);
            } else if ("union".equals(variety)) {
                simpleType = new UserUnionType(config);
            } else {
                throw new SchemaException("Unknown simple type variety: " + variety);
            }
            components.put(id, simpleType);
            simpleType.setTypeName(new StructuredQName("", namespace, name), fingerprint);
            if (base != null) {
                if (base.startsWith("#")) {
                    int fp = config.getNamePool().getFingerprint(NamespaceConstant.SCHEMA, base.substring(1));
                    simpleType.setBaseTypeReference(new TypeReference(fp, config, null));
                } else {
                    fixups.add(new Fixup(simpleType, base, FIXUP_SIMPLE_TYPE_BASE_TYPE));
                }
            }

            if (itemType != null) {
                if (itemType.startsWith("#")) {
                    int fp = config.getNamePool().getFingerprint(NamespaceConstant.SCHEMA, itemType.substring(1));
                    ((UserListType) simpleType).setItemTypeReference(new TypeReference(fp, config, null));
                } else {
                    fixups.add(new Fixup(simpleType, itemType, FIXUP_SIMPLE_TYPE_ITEM_TYPE));
                }
            }
            if (memberTypes != null) {
                StringTokenizer st = new StringTokenizer(memberTypes, " \t\n\r");
                while (st.hasMoreTokens()) {
                    String memberType = st.nextToken();
                    if (memberType.startsWith("#")) {
                        int fp = config.getNamePool().getFingerprint(NamespaceConstant.SCHEMA, memberType.substring(1));
                        ((UserUnionType) simpleType).addMemberTypeReference(new TypeReference(fp, config, null));
                    } else {
                        fixups.add(new Fixup(simpleType, memberType, FIXUP_SIMPLE_TYPE_MEMBER_TYPE));
                    }

                }

            }
            if (name == null) {
                schema.addAnonymousType(simpleType);
            } else {
                schema.addType(simpleType);
            }
        }

        public void processElementEnd() {
        }

    }

    /**
     * Handle a named attribute group definition in the SCM Schema Document
     */

    private class AttributeGroupHandler implements SCMNodeHandler {

        final AttributeGroupDecl group;

        public AttributeGroupHandler() {
            group = new AttributeGroupDecl(config);
        }

        public SCMNodeHandler handleChildElement(String localName) {
            switch (localName) {
                case "attributeUse":
                    return new AttributeUseHandler(group);
                case "attributeWildcard":
                    return new AttributeWildcardHandler(group);
                default:
                    return null;
            }
        }

        public void processElementStart(String localName, AttributeCollection attributes) {
            String id = attributes.getValue("", "id");
            String name = attributes.getValue("", "name");
            String namespace = attributes.getValue("", "targetNamespace");
            StructuredQName nameCode = null;
            if (name != null) {
                nameCode = new StructuredQName("", namespace, name);
            }

            components.put(id, group);
            group.setAttributeGroupName(nameCode);
            schema.addAttributeGroup(group);
        }

        public void processElementEnd() {
        }

    }

    /**
     * Handle a named attribute group definition in the SCM Schema Document
     */

    private class NotationHandler implements SCMNodeHandler {

        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        public void processElementStart(String localName, AttributeCollection attributes) {
            Notation notation = new Notation(config, null);
            String id = attributes.getValue("", "id");
            String name = attributes.getValue("", "name");
            String namespace = attributes.getValue("", "targetNamespace");
            StructuredQName nameCode;
            if (name == null) {
                nameCode = new StructuredQName("anon", NamespaceConstant.ANONYMOUS, id);
                // TODO: this won't be unique if loading several SCM schemas
            } else {
                nameCode = new StructuredQName("", namespace, name);
            }
            notation.setNotationName(nameCode);
            String systemId = attributes.getValue("", "systemId");
            notation.setNotationSystemId(systemId);
            String publicId = attributes.getValue("", "publicId");
            notation.setNotationPublicId(publicId);
            components.put(id, notation);
            schema.addNotation(notation);
        }

        public void processElementEnd() {
        }

    }

    /**
     * Handle a named model group definition in the SCM Schema document
     */

    private class ModelGroupDefinitionHandler extends ModelGroupHandler {

        public void processElementStart(String localName, AttributeCollection attributes) {
            String id = attributes.getValue("", "id");
            String name = attributes.getValue("", "name");
            String namespace = attributes.getValue("", "targetNamespace");
            StructuredQName modelGroupName = new StructuredQName("", namespace, name);
            ModelGroupDefinition mgd = new ModelGroupDefinition(config, null);
            mgd.setModelGroupName(modelGroupName);
            components.put(id, mgd);
            schema.addGroup(mgd);
            group = mgd;
        }

        public void processElementEnd() {
        }

    }

    /**
     * Handle an attribute use component in the SCM schema document
     */

    private class AttributeUseHandler implements SCMNodeHandler {

        private final SchemaComponent container;
        private AttributeUse attributeUse;

        public AttributeUseHandler(SchemaComponent container) {
            this.container = container;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("fixed") || localName.equals("default")) {
                return new ValueConstraintHandler(attributeUse, localName);
            } else {
                return null;
            }
        }

        public void processElementStart(String localName, AttributeCollection attributes) {

            boolean required = toBoolean(attributes.getValue("", "required"));
            boolean isInheritable = toBoolean(attributes.getValue("", "inheritable"));
            String ref = attributes.getValue("", "ref");
            String defaultValue = attributes.getValue("", "default");

            attributeUse = new AttributeUse(config);
            attributeUse.setInheritable(isInheritable ? 1 : 0);
            if (container instanceof UserComplexType) {
                ((UserComplexType) container).addNonInheritedAttributeUse(attributeUse);
            } else {
                ((AttributeGroupDecl) container).addAttributeUse(attributeUse);
            }

            attributeUse.setUse(required ? AttributeUse.REQUIRED : AttributeUse.OPTIONAL);

            if (defaultValue != null) {
                ValueConstraint vc = new ValueConstraint(ValueConstraint.Variety.DEFAULT, defaultValue);
                attributeUse.setValueConstraint(vc);
            }
            fixups.add(new Fixup(attributeUse, ref, FIXUP_ATTRIBUTE_USE_DECLARATION));

        }

        public void processElementEnd() {
        }
    }

    /**
     * Handle a facet definition in the SCM Schema document
     */

    private class FacetHandler implements SCMNodeHandler {

        private final UserSimpleType simpleType;

        public FacetHandler(UserSimpleType simpleType) {
            this.simpleType = simpleType;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("assertion")) {
                return new AssertionHandler(simpleType);
            }
            return null;
        }

        public void processElementStart(String localName, AttributeCollection attributes) throws SchemaException {
            if (!localName.equals("assert")) {
                String value = attributes.getValue("", "value");
                boolean namespaceSensitive = toBoolean(attributes.getValue("", "namespaceSensitive"));
                NamespaceResolver resolver = null;
                if (namespaceSensitive) {
                    resolver = new SavedNamespaceContext(namespaceResolver);
                }
                Facet facet = Facet.makeFacet(localName, value, config, resolver, compiler);
                String fixed = attributes.getValue("", "fixed");
                if (fixed != null) {
                    facet.setFixed(toBoolean(fixed));
                }
                if (facet instanceof PatternFacet) {
                    ((PatternFacet) facet).setDoNotMerge(true);
                    String flags = attributes.getValue("", "flags");
                    if (flags != null) {
                        ((PatternFacet) facet).setFlags(flags);
                    }
                    ((PatternFacet) facet).compile(null);
                }
                simpleType.addFacet(facet);
            }
        }

        public void processElementEnd() {
        }
    }

    /**
     * Handle a saxon:preprocess facet definition in the SCM Schema document
     */

    private class PreprocessHandler implements SCMNodeHandler {

        private final UserSimpleType simpleType;
        private final PreprocessFacet facet;

        public PreprocessHandler(UserSimpleType simpleType) {
            this.simpleType = simpleType;
            facet = new PreprocessFacet();
            simpleType.addFacet(facet);
        }

        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("assertion")) {
                return new AssertionHandler(facet);
            }
            return null;
        }

        public void processElementStart(String localName, AttributeCollection attributes) {

            String fixed = attributes.getValue("", "fixed");
            if (fixed != null) {
                facet.setFixed(toBoolean(fixed));
            }
        }

        public void processElementEnd() {
        }
    }


    /**
     * Handle a particle in the SCM schema document. This may be an element particle, and
     * element wildcard particle, or a model group particle. If it is a model group particle
     * it may either be a reference to a named model group definition, or may contain a
     * model group
     */

    private class ParticleHandler implements SCMNodeHandler {

        private final SchemaComponent container;
        private Particle particle;

        public ParticleHandler(SchemaComponent container) {
            this.container = container;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            switch (localName) {
                case "sequence":
                    return new ModelGroupHandler((ModelGroupParticle) particle);
                case "choice":
                    return new ModelGroupHandler((ModelGroupParticle) particle);
                case "all":
                    return new ModelGroupHandler((ModelGroupParticle) particle);
                default:
                    return null;
            }
        }

        public void processElementStart(String localName, AttributeCollection attributes) throws SchemaException {
            switch (localName) {
                case "elementParticle":
                    particle = new ElementParticle(config, null);
                    break;
                case "elementWildcard":
                    particle = new ElementWildcard(config);
                    break;
                case "modelGroupParticle":
                    particle = new ModelGroupParticle(config, null);
                    break;
                default:
                    throw new SchemaException("Unknown kind of particle " + localName);
            }
            String minOccursAtt = attributes.getValue("", "minOccurs");
            int minOccurs = 1;
            if (minOccursAtt != null) {
                minOccurs = Integer.parseInt(minOccursAtt.trim());
            }
            particle.setMinOccurs(minOccurs);
            String maxOccursAtt = attributes.getValue("", "maxOccurs");
            int maxOccurs = -1;
            if (maxOccursAtt != null) {
                maxOccursAtt = maxOccursAtt.trim();
                if (maxOccursAtt.equals("unbounded")) {
                    maxOccurs = -1;
                } else {
                    maxOccurs = Integer.parseInt(maxOccursAtt);
                }
            }
            particle.setMaxOccurs(maxOccurs);

            if (particle instanceof ElementParticle) {
                String ref = attributes.getValue("", "ref");
                fixups.add(new Fixup(particle, ref, FIXUP_ELEMENT_PARTICLE_REF));
            } else if (particle instanceof ModelGroupParticle) {
                String ref = attributes.getValue("", "ref");
                if (ref != null) {
                    fixups.add(new Fixup(particle, ref, FIXUP_MODEL_GROUP_PARTICLE_REF));
                }
            } else if (particle instanceof ElementWildcard) {
                String ref = attributes.getValue("", "ref");
                fixups.add(new Fixup(particle, ref, FIXUP_ELEMENT_WILDCARD_REF));
            } else {
                throw new IllegalStateException("Unknown particle " + particle.getClass().getName());
            }

            if (container instanceof ModelGroup) {
                ((ModelGroup) container).addParticle(particle);
            } else if (container instanceof UserComplexType) {
                ((UserComplexType) container).setParticle(particle);
            } else {
                throw new IllegalArgumentException(container.getClass().getName());
            }

        }

        public void processElementEnd() {
        }
    }

    /**
     * Handle an unnamed model group, that is, a sequence, choice, or all.
     */

    private class ModelGroupHandler implements SCMNodeHandler {

        private final ModelGroupParticle referrer;
        protected ModelGroup group;

        public ModelGroupHandler() {
            referrer = null;
        }

        public ModelGroupHandler(ModelGroupParticle referrer) {
            this.referrer = referrer;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            switch (localName) {
                case "elementParticle":
                    return new ParticleHandler(group);
                case "elementWildcard":
                    return new ParticleHandler(group);
                case "modelGroupParticle":
                    return new ParticleHandler(group);
                default:
                    return null;
            }
        }

        public void processElementStart(String localName, AttributeCollection attributes) throws SchemaException {
            switch (localName) {
                case "sequence":
                    group = new SequenceModelGroup();
                    break;
                case "choice":
                    group = new ChoiceModelGroup();
                    break;
                case "all":
                    group = new AllModelGroup();
                    break;
                default:
                    throw new SchemaException("Unknown kind of model group " + localName);
            }

            if (referrer != null) {
                referrer.setTarget(group);
            }
            anonComponents.add(group);
        }

        public void processElementEnd() {
        }
    }

    /**
     * Handle an assertion in the SCM schema document.
     */

    private class AssertionHandler implements SCMNodeHandler {

        private final Object container;

        public AssertionHandler(Object container) {
            this.container = container;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        public void processElementStart(String localName, AttributeCollection attributes) throws SchemaException {
            String xpathText = attributes.getValue("", "test");
            String xmlBase = attributes.getValue(NamespaceConstant.XML, "base");
            String defaultNamespace = attributes.getValue("", "defaultNamespace");

            XPathEvaluator evaluator;
            if (container instanceof UserComplexType) {
                evaluator = new XSDAssert.CustomizedXPath(config);
            } else {
                evaluator = new XPathEvaluator(config);
            }
            IndependentContext env = (IndependentContext) evaluator.getStaticContext();
            env.setNamespaceResolver(new SavedNamespaceContext(namespaceResolver));
            env.setBaseURI(xmlBase);
            env.setDefaultElementNamespace(defaultNamespace);
            env.getPackageData().setLocalLicenseId(schema.getLocalLicenseId());
            env.setSchemaAware(true);
            boolean isSimple = (container instanceof UserSimpleType) ||
                    (container instanceof UserComplexType && ((UserComplexType) container).isSimpleContent());
            XPathVariable valueVariable = null;
            if (isSimple || container instanceof PreprocessFacet || xpathText.contains("$value")) {
                valueVariable = env.declareVariable("", "value");
            }
            XPathExpression condition;
            try {
                condition = evaluator.createExpression(xpathText);
            } catch (XPathException err) {
                throw new SchemaException(err);
            }
            Assertion assertion = new Assertion(config, condition, valueVariable, new HashMap<>(), xpathText);
            assertion.setStaticContext(env);
            if (container instanceof UserComplexType) {
                ((UserComplexType) container).addAssertion(assertion);
            } else if (container instanceof UserSimpleType) {
                UserSimpleType ust = (UserSimpleType) container;
                AssertionFacet facet = new AssertionFacet(assertion);
                ust.addFacet(facet);
            } else if (container instanceof PreprocessFacet) {
                PreprocessFacet ppf = (PreprocessFacet) container;
                if (ppf.getAction() == null) {
                    ppf.setAction(assertion);
                } else {
                    ppf.setReverseAction(assertion);
                }
            }
        }

        public void processElementEnd() {
        }
    }


    /**
     * Handle an open content element in the SCM schema document.
     */

    private class OpenContentHandler implements SCMNodeHandler {

        private final UserComplexType container;

        public OpenContentHandler(UserComplexType container) {
            this.container = container;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            //if (localName.equals("wildcard")) {
            //    return new WildcardHandler(container, mode);
            //} else {
            return null;
            //}
        }

        public void processElementStart(String localName, AttributeCollection attributes) {
            String mode = attributes.getValue("", "mode");
            String wildcardRef = attributes.getValue("", "wildcard");
            if (mode.equals("suffix")) {
                fixups.add(new Fixup(container, wildcardRef, FIXUP_COMPLEX_TYPE_OPEN_CONTENT_SUFFIX));
            } else {
                fixups.add(new Fixup(container, wildcardRef, FIXUP_COMPLEX_TYPE_OPEN_CONTENT_INTERLEAVE));
            }
        }

        public void processElementEnd() {
        }
    }

    /**
     * Handle a attribute wildcard in the SCM schema document.
     */

    private class AttributeWildcardHandler implements SCMNodeHandler {

        private final SchemaComponent container;

        public AttributeWildcardHandler(SchemaComponent container) {
            this.container = container;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        public void processElementStart(String localName, AttributeCollection attributes) {
            AttributeWildcard awc = new AttributeWildcard((EnterpriseConfiguration) getConfiguration());
            if (container instanceof AttributeGroupDecl) {
                ((AttributeGroupDecl) container).setAnyAttribute(awc);
            } else if (container instanceof UserComplexType) {
                ((UserComplexType) container).setNonInheritedAnyAttribute(awc);
            } else {
                throw new IllegalArgumentException(container.getClass().getName());
            }
            String ref = attributes.getValue("", "ref");
            fixups.add(new Fixup(awc, ref, FIXUP_ATTRIBUTE_WILDCARD_REF));
        }

        public void processElementEnd() {
        }
    }

    /**
     * Handle a fixed value element in the SCM schema document.
     */

    private class ValueConstraintHandler implements SCMNodeHandler {

        private final SchemaComponent container;
        private final ArrayList<AtomicValue> atomicValues = new ArrayList<>(4);
        private String lexicalForm;
        private String variety;

        public ValueConstraintHandler(SchemaComponent container, String variety) {
            this.container = container;
            this.variety = variety;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("item")) {
                return new ItemHandler(this);
            } else {
                return null;
            }
        }

        public void processElementStart(String localName, AttributeCollection attributes) {
            lexicalForm = attributes.getValue("", "lexicalForm");
        }

        public void processElementEnd() {
            AtomicSequence v;
            if (atomicValues.size() == 1) {
                v = atomicValues.get(0);
            } else {
                v = new AtomicArray(atomicValues);
            }
            if (container instanceof ComponentWithValueConstraint) {
                ValueConstraint.Variety var = variety.equals("fixed") ? ValueConstraint.Variety.FIXED : ValueConstraint.Variety.DEFAULT;
                ValueConstraint vc = new ValueConstraint(var, lexicalForm);
                vc.setValue(v);
                ((ComponentWithValueConstraint) container).setValueConstraint(vc);
            }
        }

        public void appendAtomicValue(AtomicValue item) {
            atomicValues.add(item);
        }
    }

    /**
     * Handle an Item element within a Fixed value
     */

    private class ItemHandler implements SCMNodeHandler {

        private final ValueConstraintHandler container;

        public ItemHandler(ValueConstraintHandler container) {
            this.container = container;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        public void processElementStart(String localName, AttributeCollection attributes) throws SchemaException {
            String type = attributes.getValue("", "type");
            BuiltInAtomicType t = (BuiltInAtomicType) config.getSchemaType(
                new StructuredQName("", NamespaceConstant.SCHEMA, type.substring(1)));
            String value = attributes.getValue("", "value");
            String namespace = attributes.getValue("", "namespace");
            AtomicValue typedValue;
            try {
                if (namespace == null) {
                    typedValue = t.getTypedValue(value, null, config.getConversionRules()).head();
                } else {
                    String[] parts = NameChecker.getQNameParts(value);
                    if (t.equals(BuiltInAtomicType.QNAME)) {
                        typedValue = new QNameValue(parts[0], namespace, parts[1]);
                    } else if (t.equals(BuiltInAtomicType.NOTATION)) {
                        typedValue = new NotationValue(parts[0], namespace, parts[1]);
                    } else {
                        throw new AssertionError("Unknown namespace-sensitive type " + type);
                    }
                }
            } catch (QNameException | XPathException e) {
                throw new SchemaException(e);
            }
            container.appendAtomicValue(typedValue);
        }

        public void processElementEnd() {
        }
    }


    /**
     * Handle a wildcard within an element or attribute wildcard component
     */

    private class WildcardHandler implements SCMNodeHandler {

        public WildcardHandler() {
        }

        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        public void processElementStart(String localName, AttributeCollection attributes) throws SchemaException {

            Wildcard wildcard = new Wildcard();
            wildcard.setConfiguration((EnterpriseConfiguration) getConfiguration());
            String id = attributes.getValue("", "id");
            if (id != null) {
                components.put(id, wildcard);
            }
            String processContents = attributes.getValue("", "processContents");
            wildcard.setProcessContents(processContents);
            String constraint = attributes.getValue("", "constraint");
            switch (constraint) {
                case "any":
                    // no action
                    break;
                case "not": {
                    String namespaces = attributes.getValue("", "namespaces");
                    StringTokenizer st = new StringTokenizer(namespaces, " \t\n\r");
                    while (st.hasMoreTokens()) {
                        String ns = st.nextToken();
                        wildcard.addDisallowedNamespace(ns.equals("##local") ? "" : ns);
                    }
                    break;
                }
                case "enumeration": {
                    wildcard.setNoNamespacesAllowed();
                    String namespaces = attributes.getValue("", "namespaces");
                    StringTokenizer st = new StringTokenizer(namespaces, " \t\n\r");
                    while (st.hasMoreTokens()) {
                        String ns = st.nextToken();
                        wildcard.addAllowedNamespace(ns.equals("##local") ? "" : ns);
                    }
                    break;
                }
                default:
                    throw new SchemaException("Unknown wildcard constraint " + constraint);
            }
            String disallowedQNames = attributes.getValue("", "notQName");
            if (disallowedQNames != null) {
                NamePool pool = getNamePool();
                StringTokenizer st = new StringTokenizer(disallowedQNames, " \r\t\n");
                while (st.hasMoreTokens()) {
                    String token = st.nextToken();
                    if ("##defined".equals(token)) {
                        wildcard.setDisallowDefinedNames(true);
                    } else if ("##definedSibling".equals(token)) {
                        wildcard.setDisallowDefinedSiblings(true);
                    } else {
                        int nc = pool.allocateClarkName(token);
                        wildcard.addDisallowedQName(nc & NamePool.FP_MASK);
                    }
                }
            }
        }

        public void processElementEnd() {
        }
    }


    /**
     * Handle a finite state machine within a complex type
     */

    private class FiniteStateMachineHandler implements SCMNodeHandler {

        private final FiniteStateMachine fsm;
        //private IntHashMap forwardReferences = new IntHashMap();

        public FiniteStateMachineHandler(UserComplexType complexType) {
            fsm = new FiniteStateMachine(complexType, true);
            complexType.setFiniteStateMachine(fsm);
//            if (complexType.getOpenContentWildcard() != null) {
//                fsm.setOpenContentWildcard(complexType.getOpenContentWildcard(),
//                        complexType.getOpenContentMode() == UserComplexType.OPEN_CONTENT_INTERLEAVE);
//            }
        }

        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("state")) {
                return new FiniteStateMachineStateHandler(this);
            } else {
                return null;
            }
        }

        public void processElementStart(String localName, AttributeCollection attributes) {
            //
        }

        public FiniteStateMachine getFiniteStateMachine() {
            return fsm;
        }

        public void processElementEnd() {
        }

    }

    /**
     * Handle a state within a finite state machine
     */

    private class FiniteStateMachineStateHandler implements SCMNodeHandler {

        private final FiniteStateMachineHandler fsmHandler;
        private DeterminizedState state;

        public FiniteStateMachineStateHandler(FiniteStateMachineHandler fsmHandler) {
            this.fsmHandler = fsmHandler;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("edge")) {
                return new FiniteStateMachineEdgeHandler(fsmHandler, state);
            } else {
                return null;
            }
        }

        public void processElementStart(String localName, AttributeCollection attributes) {
            String stateNumber = attributes.getValue("", "nr");
            FiniteStateMachine fsm = fsmHandler.getFiniteStateMachine();
            if (stateNumber.equals("0")) {
                state = (DeterminizedState) fsm.getInitialState();
            } else {
                state = new DeterminizedState(fsm);
            }
            String isFinal = attributes.getValue("", "final");
            state.setFinalState(toBoolean(isFinal));
            String min = attributes.getValue("", "minOccurs");
            String max = attributes.getValue("", "maxOccurs");
            if (min != null && max != null) {
                state.setLimits(Integer.parseInt(min), max.equals("unbounded") ? -1 : Integer.parseInt(max));
            }
            String afterMax = attributes.getValue("", "afterMax");
            if (afterMax != null) {
                PendingEdge leavingEdge = new PendingEdge();
                leavingEdge.machine = fsm;
                leavingEdge.termId = "##afterMax";
                leavingEdge.fromState = Integer.parseInt(stateNumber);
                leavingEdge.toState = Integer.parseInt(afterMax);
                pendingEdges.add(leavingEdge);
            }
        }

        public void processElementEnd() {
        }
    }

    /**
     * Handle an Edge within a finite state machine
     */

    private class FiniteStateMachineEdgeHandler implements SCMNodeHandler {

        private final FiniteStateMachineHandler fsm;
        private final DeterminizedState state;

        public FiniteStateMachineEdgeHandler(FiniteStateMachineHandler fsm, DeterminizedState state) {
            this.fsm = fsm;
            this.state = state;
        }

        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        public void processElementStart(String localName, AttributeCollection attributes) {
            String term = attributes.getValue("", "term");
            int targetState = Integer.parseInt(attributes.getValue("", "to"));
            PendingEdge pe = new PendingEdge();
            pe.machine = fsm.getFiniteStateMachine();
            pe.fromState = state.getStateNumber();
            pe.toState = targetState;
            pe.termId = term;
            pendingEdges.add(pe);
        }

        public void processElementEnd() {
        }
    }


    /**
     * Simple structure class to represent a pending Edge object, which cannot be resolved to a real
     * Edge until all sources and targets are known
     */

    private static class PendingEdge {
        public FiniteStateMachine machine;
        public int fromState;
        public int toState;
        public String termId;
    }


    /**
     * Convert a boolean attribute value
     *
     * @param s the attribute value
     * @return true if the value is "true" or "1", false if it is "false" or 0 or null.
     */

    private static boolean toBoolean(String s) {
        if (s == null) {
            return false;
        }
        s = s.trim();
        return s.equals("1") || s.equals("true");
    }

    /**
     * Parse a list of blocked or final derivations. Used to process the block,
     * final, blockDefault, and finalDefault attributes.
     *
     * @param attrName   the attribute name (block or final)
     * @param finalValue A list of values to be parsed, as a string,
     *                   for example "extension restriction"
     * @param allowed    The permitted values, as a bit-significant integer. For example
     *                   the value (DERIVE_BY_EXTENSION | DERIVE_BY_RESTRICTION) allows the values
     *                   "extension" and "restriction" to appear.
     * @return The final/blocked values, as a bit-significant integer
     * @throws SchemaException if a failure occurs
     */

    public int parseFinalOrBlock(String attrName, String finalValue, int allowed) throws SchemaException {
        if (finalValue == null) {
            return 0;
        }
        if (Whitespace.trim(finalValue).equals("#all")) {
            return allowed;
        } else {
            int result = 0;
            StringTokenizer tokenizer = new StringTokenizer(finalValue, " \t\n\r", false);
            while (tokenizer.hasMoreTokens()) {
                String token = tokenizer.nextToken();
                int v;
                switch (token) {
                    case "union":
                        v = SchemaType.DERIVATION_UNION;
                        break;
                    case "list":
                        v = SchemaType.DERIVATION_LIST;
                        break;
                    case "restriction":
                        v = SchemaType.DERIVATION_RESTRICTION;
                        break;
                    case "extension":
                        v = SchemaType.DERIVATION_EXTENSION;
                        break;
                    case "substitution":
                        v = SchemaType.DERIVE_BY_SUBSTITUTION;
                        break;
                    default:
                        String err = "Invalid " + Err.wrap(attrName, Err.ATTRIBUTE) + " value " +
                                Err.wrap(token, Err.VALUE);
                        throw new SchemaException(err);
                }
                if ((v & ~allowed) != 0) {
                    String err = "The " + Err.wrap(attrName, Err.ATTRIBUTE) + " value " +
                            Err.wrap(token, Err.VALUE) + " is not allowed here";
                    throw new SchemaException(err);
                }
                result |= v;
            }
            return result;
        }
    }


    private final static int FIXUP_ELEMENT_TYPE = 1;
    private final static int FIXUP_ELEMENT_SUBSTITUTION_GROUP = 2;

    private final static int FIXUP_ATTRIBUTE_TYPE = 11;
    private final static int FIXUP_ALTERNATIVE_TYPE = 12;

    private final static int FIXUP_COMPLEX_TYPE_BASE_TYPE = 21;
    private final static int FIXUP_COMPLEX_TYPE_SIMPLE_TYPE = 22;
    private final static int FIXUP_COMPLEX_TYPE_OPEN_CONTENT_SUFFIX = 23;
    private final static int FIXUP_COMPLEX_TYPE_OPEN_CONTENT_INTERLEAVE = 24;

    private final static int FIXUP_SIMPLE_TYPE_ITEM_TYPE = 31;
    private final static int FIXUP_SIMPLE_TYPE_MEMBER_TYPE = 32;
    private final static int FIXUP_SIMPLE_TYPE_BASE_TYPE = 33;

    private final static int FIXUP_ATTRIBUTE_USE_DECLARATION = 41;
    private final static int FIXUP_ATTRIBUTE_WILDCARD_REF = 42;

    private final static int FIXUP_ELEMENT_PARTICLE_REF = 51;
    private final static int FIXUP_ELEMENT_WILDCARD_REF = 52;

    private final static int FIXUP_MODEL_GROUP_PARTICLE_REF = 61;

    private final static int FIXUP_KEYREF_KEY = 62;
    private final static int FIXUP_IDENTITY_CONSTRAINT_REF = 63;

    /**
     * Object defining a fixup action, something to be done when all the data has been read
     */

    private class Fixup {

        private final Object source;
        private final String targetId;
        private final int property;

        public Fixup(Object source, String targetId, int property) {
            this.source = source;
            this.targetId = targetId;
            this.property = property;
        }

        public void apply() {
            switch (property) {
                case FIXUP_ELEMENT_TYPE: {
                    ((ElementDecl) source).setTypeReference(makeTypeReference(targetId));
                    break;
                }
                case FIXUP_ELEMENT_SUBSTITUTION_GROUP: {
                    ElementDecl head = (ElementDecl) components.get(targetId);
                    ((ElementDecl) source).addSubstitutionGroupHead(head.getComponentName());
                    break;
                }
                case FIXUP_ALTERNATIVE_TYPE: {
                    ((TypeAlternative) source).setTypeReference(makeTypeReference(targetId));
                    break;
                }
                case FIXUP_ATTRIBUTE_TYPE: {
                    ((AttributeDecl) source).setTypeReference(makeTypeReference(targetId));
                    break;
                }
                case FIXUP_COMPLEX_TYPE_BASE_TYPE: {
                    ((UserComplexType) source).setBaseTypeReference(makeTypeReference(targetId));
                    break;
                }
                case FIXUP_COMPLEX_TYPE_SIMPLE_TYPE: {
                    ((UserComplexType) source).setSimpleContentTypeReference(makeTypeReference(targetId));
                    break;
                }
                case FIXUP_COMPLEX_TYPE_OPEN_CONTENT_SUFFIX: {
                    Wildcard wildcard = (Wildcard) components.get(targetId);
                    ((UserComplexType) source).setOpenContentWildcard(wildcard, "suffix", true);
                    FiniteStateMachine fsm = ((UserComplexType) source).getFiniteStateMachine();
                    if (fsm != null) {
                        fsm.setOpenContentWildcard(wildcard, false);
                    }
                    break;
                }
                case FIXUP_COMPLEX_TYPE_OPEN_CONTENT_INTERLEAVE: {
                    Wildcard wildcard = (Wildcard) components.get(targetId);
                    ((UserComplexType) source).setOpenContentWildcard(wildcard, "interleave", true);
                    FiniteStateMachine fsm = ((UserComplexType) source).getFiniteStateMachine();
                    if (fsm != null) {
                        fsm.setOpenContentWildcard(wildcard, true);
                    }
                    break;
                }
                case FIXUP_SIMPLE_TYPE_ITEM_TYPE: {
                    ((UserListType) source).setItemTypeReference(makeTypeReference(targetId));
                    break;
                }
                case FIXUP_SIMPLE_TYPE_MEMBER_TYPE: {
                    ((UserUnionType) source).addMemberTypeReference(makeTypeReference(targetId));
                    break;
                }
                case FIXUP_SIMPLE_TYPE_BASE_TYPE: {
                    ((UserSimpleType) source).setBaseTypeReference(makeTypeReference(targetId));
                    break;
                }
                case FIXUP_ATTRIBUTE_USE_DECLARATION: {
                    final SchemaComponent target = components.get(targetId);
                    ((AttributeUse) source).setTarget(target);
                    ((AttributeUse) source).setTargetComponentName(((AttributeDecl) target).getComponentName());
                    break;
                }
                case FIXUP_ATTRIBUTE_WILDCARD_REF: {
                    final SchemaComponent target = components.get(targetId);
                    ((AttributeWildcard) source).setWildcard((Wildcard) target);
                    break;
                }
                case FIXUP_ELEMENT_PARTICLE_REF: {
                    SchemaComponent target = components.get(targetId);
                    ((ElementParticle) source).setTarget(target);
                    ((ElementParticle) source).setTargetComponentName(((ElementDecl) target).getComponentName());
                    break;
                }
                case FIXUP_ELEMENT_WILDCARD_REF: {
                    final SchemaComponent target = components.get(targetId);
                    ((ElementWildcard) source).setWildcard((Wildcard) target);
                    break;
                }
                case FIXUP_MODEL_GROUP_PARTICLE_REF: {
                    SchemaComponent target = components.get(targetId);
                    ((ModelGroupParticle) source).setTarget(target);
                    ((ModelGroupParticle) source).setTargetComponentName(((ModelGroupDefinition) target).getModelGroupName());
                    break;
                }
                case FIXUP_KEYREF_KEY: {
                    IdentityConstraint target = (IdentityConstraint) components.get(targetId);
                    ((KeyRef) source).setTargetKey(target);
                    ((KeyRef) source).setTargetComponentName(target.getConstraintName());
                    break;
                }
                case FIXUP_IDENTITY_CONSTRAINT_REF: {
                    IdentityConstraint target = (IdentityConstraint) components.get(targetId);
                    ((IdentityConstraintReference) source).setTarget(target);
                    break;
                }
            }

        }
    }

    /*@Nullable*/
    private TypeReference makeTypeReference(String targetId) {
        if (targetId.startsWith("#")) {
            SchemaType builtInType = BuiltInType.getSchemaTypeByLocalName(targetId.substring(1));
            TypeReference ref = new TypeReference(builtInType.getFingerprint(), config, null);
            ref.setTarget(builtInType);
            return ref;
        } else {
            SchemaType userType = (SchemaType) components.get(targetId);
            int fp = userType.getFingerprint();
            TypeReference ref = new TypeReference(fp, config, null);
            ref.setTarget(userType);
            return ref;
        }
    }

}

// Copyright (c) 2007 Saxonica Limited. All rights reserved