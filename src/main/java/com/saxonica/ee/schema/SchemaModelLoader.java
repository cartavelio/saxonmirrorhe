package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.fsa.DeterminizedState;
import com.saxonica.ee.schema.fsa.Edge;
import com.saxonica.ee.schema.fsa.FiniteStateMachine;
import com.saxonica.ee.schema.sdoc.SelectionParser;
import com.saxonica.ee.schema.sdoc.XSDAssert;
import com.saxonica.ee.stream.Sweep;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.*;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.Pattern;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.sxpath.XPathEvaluator;
import net.sf.saxon.sxpath.XPathExpression;
import net.sf.saxon.sxpath.XPathVariable;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.PackageLoaderHE;
import net.sf.saxon.trans.XPathException;
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
    private final HashSet<NamespaceUri> targetNamespaces = new HashSet<>(10);
    private final HashSet<SchemaComponent> anonComponents = new HashSet<>(100);
    private final IdentityHashMap<UserComplexType, FiniteStateMachine> pendingCompilations = new IdentityHashMap<>();
    private final List<Fixup> fixups = new ArrayList<>();
    private final Stack<SCMNodeHandler> handlerStack = new Stack<>();

    //private NamespaceResolver namespaceResolver;

    //private NodeName elementName;

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
        ParseOptions options = config.getParseOptions()
                        .withSchemaValidationMode(Validation.SKIP);
        getPipelineConfiguration().setParseOptions(options);
        this.config = (EnterpriseConfiguration) config;
        schema = new PreparedSchema((EnterpriseConfiguration) config);
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
        pipe.setParseOptions(config.getParseOptions().withSchemaValidationMode(Validation.SKIP));
        pipe.setHostLanguage(HostLanguage.XML_SCHEMA);
        compiler = config.newSchemaCompiler();
        CheckSumFilter filter = new CheckSumFilter(this);
        filter.setCheckExistingChecksum(true);
        Sender.send(source, filter, null);
        if (!filter.isChecksumCorrect()) {
            // Don't worry about this if there's no checksum and no DMK
            if (filter.isChecksumFound() || dmk != null) {
                throw new XPathException("Invalid checksum in compiled schema file");
            }
        }

        for (NamespaceUri ns : targetNamespaces) {
            schema.setNamespaceStatus(ns, PreparedSchema.NAMESPACE_KNOWN);
        }
        for (SchemaType type : schema.getAllTypes()) {
            if (type instanceof UserComplexType) {
                FiniteStateMachine fsm = pendingCompilations.get(type);
                schema.rebuildComplexType((UserComplexType)type, fsm);
            }
        }

        try {
            for (Fixup fixup : fixups) {
                fixup.apply(this);
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

            for (CompiledComplexType cct : schema.getCompiledComplexTypes()) {
                cct.makeContextDeterminedTypeMapForAttributes(schema);
                cct.makeContextDeterminedTypeMapForElements(schema);
            }
            
        } catch (SchemaException err) {
            throw new XPathException(err);
        }
        return schema;
    }

    public PreparedSchema getSchema() {
        return schema;
    }

    /**
     * Notify the start of an element
     */

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
//        System.err.println("start " + config.getNamePool().getDisplayName(nameCode));
        SCMNodeHandler handler;
        if (handlerStack.isEmpty()) {
            handler = new SchemaHandler();
        } else {
            handler = handlerStack.peek().handleChildElement(elemName.getLocalPart());
            if (handler == null) {
                throw new XPathException("No handler defined for element " + elemName.getLocalPart() +
                        " in " + handlerStack.peek().getClass().getName());
            }
        }
        handler.setSchemaModelLoader(this);
        handlerStack.push(handler);
        try {

            handler.processElementStart(elemName.getLocalPart(), attributes, namespaces);
        } catch (SchemaException e) {
            throw new XPathException(e);
        }

    }

    /**
     * Character data
     */

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        if (!Whitespace.isAllWhite(chars)) {
            throw new XPathException("Character content not allowed in SCM document");
        }
    }

    /**
     * End of element
     */

    @Override
    public void endElement() throws XPathException {
        SCMNodeHandler handler = handlerStack.pop();
        handler.processElementEnd();
    }

    /**
     * Interface for an SCM element handler. This handles the attributes and children of a specific element
     * in the SCM serialization.
     */

    private abstract static class SCMNodeHandler {
        
        // The design here is distorted by the constraints of inner classes in C#
        protected SchemaModelLoader loader;

        public void setSchemaModelLoader(SchemaModelLoader loader) {
            this.loader = loader;
        }

        /**
         * Process an element start tag together with its attributes
         *
         * @param localName  the name of the element to be processed
         * @param attributes the attributes of the element being processed
         * @param namespaces the in scope namespaces of the element being processed
         * @throws SchemaException if the data is invalid
         */

        public abstract void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) throws SchemaException;

        /**
         * Handle a child element of the element currently being processed.
         *
         * @param localName the local name of the child element
         * @return an SCMNodeHandler which will be used to process the child element
         */

        public abstract SCMNodeHandler handleChildElement(String localName);

        /**
         * Process the element end tag
         */

        public abstract void processElementEnd();

    }

    /**
     * Handle the schema element (the outermost element of the SCM document)
     */

    private static class SchemaHandler extends SCMNodeHandler {

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) throws SchemaException {
            String xsdVersion = attributes.getValue(NamespaceUri.NULL, "xsdVersion");
            if (xsdVersion != null &&
                    xsdVersion.compareTo(loader.config.getConfigurationProperty(Feature.XSD_VERSION)) > 0) {
                throw new SchemaException("Imported schema component model requires XSD version " + xsdVersion);
            }
            loader.dmk = attributes.getValue(NamespaceUri.NULL, "dmk");
            if (loader.dmk != null) {
                loader.schema.setLocalLicenseId(loader.config.registerLocalLicense(loader.dmk));
            }
        }

        @Override
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

        @Override
        public void processElementEnd() {
        }
    }


    /**
     * Handle an element declaration in the SCM schema document
     */

    private static class ElementHandler extends SCMNodeHandler {

        private ElementDecl elementDecl;

        @Override
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

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) throws SchemaException {
            EnterpriseConfiguration config = loader.config;
            String id = attributes.getValue(NamespaceUri.NULL, "id");
            String name = attributes.getValue(NamespaceUri.NULL, "name");
            String namespace = attributes.getValue(NamespaceUri.NULL, "targetNamespace");
            NamespaceUri targetNS = NamespaceUri.of(namespace);
            loader.targetNamespaces.add(targetNS);
            boolean isGlobal = toBoolean(attributes.getValue(NamespaceUri.NULL, "global"));
            boolean isNillable = toBoolean(attributes.getValue(NamespaceUri.NULL, "nillable"));
            boolean isAbstract = toBoolean(attributes.getValue(NamespaceUri.NULL, "abstract"));
            String type = attributes.getValue(NamespaceUri.NULL, "type");
            String finalAtt = attributes.getValue(NamespaceUri.NULL, "final");
            String defaultValue = attributes.getValue(NamespaceUri.NULL, "default");
            int finalSet = parseFinalOrBlock("final",
                    finalAtt,
                    Derivation.DERIVATION_EXTENSION |
                    Derivation.DERIVATION_RESTRICTION |
                    Derivation.DERIVE_BY_SUBSTITUTION);
            String blockAtt = attributes.getValue(NamespaceUri.NULL, "block");
            int blockSet = parseFinalOrBlock("block",
                    blockAtt,
                    Derivation.DERIVATION_EXTENSION |
                    Derivation.DERIVATION_RESTRICTION |
                    Derivation.DERIVE_BY_SUBSTITUTION);
            elementDecl = new ElementDecl(config, isGlobal, null);
            elementDecl.setGeneratedId(id);
            loader.components.put(id, elementDecl);
            elementDecl.setElementName(new StructuredQName("", targetNS, name));
            elementDecl.setAbstract(isAbstract);
            elementDecl.setNillable(isNillable);
            elementDecl.setBlock(blockSet);
            elementDecl.setFinalProhibitions(finalSet);
            if (defaultValue != null) {
                ValueConstraint vc = new ValueConstraint(ValueConstraint.Variety.DEFAULT, StringView.tidy(defaultValue));
                elementDecl.setValueConstraint(vc);
            }
            if (type != null) {
                if (type.startsWith("#")) {
                    int fp = config.getNamePool().getFingerprint(NamespaceUri.SCHEMA, type.substring(1));
                    elementDecl.setTypeReference(new TypeReference(fp, config, null));
                } else {
                    loader.fixups.add(new Fixup(elementDecl, type, FIXUP_ELEMENT_TYPE));
                }
            }
            if (isGlobal) {
                loader.schema.addElementDecl(elementDecl);
            }
            loader.schema.addSubstitutionGroupMember(elementDecl, elementDecl);

        }

        @Override
        public void processElementEnd() {
        }
    }

    /**
     * Handle a substitution group affiliation
     */

    private static class SubstitutionGroupAffiliationHandler extends SCMNodeHandler {

        private final ElementDecl elementDecl;

        /**
         * Create a substitution group affiliation for a given element declaration
         *
         * @param decl the element declaration
         */

        public SubstitutionGroupAffiliationHandler(ElementDecl decl) {
            elementDecl = decl;
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {
            String head = attributes.getValue(NamespaceUri.NULL, "ref");
            if (head != null) {
                loader.fixups.add(new Fixup(elementDecl, head, FIXUP_ELEMENT_SUBSTITUTION_GROUP));
            }
        }

        @Override
        public void processElementEnd() {
        }
    }

    /**
     * Handle an alternative type for an element declaration
     */

    private static class AlternativeTypeHandler extends SCMNodeHandler {

        private final ElementDecl elementDecl;

        /**
         * Create an AlternativeTypeHandler for a given element declaration
         *
         * @param decl the element declaration
         */

        public AlternativeTypeHandler(ElementDecl decl) {
            elementDecl = decl;
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) throws SchemaException {
            EnterpriseConfiguration config = loader.config;
            String xpathText = attributes.getValue(NamespaceUri.NULL, "test");
            String xmlBase = attributes.getValue(NamespaceUri.XML, "base");
            String defaultNamespace = attributes.getValue(NamespaceUri.NULL, "defaultNamespace");
            String type = attributes.getValue(NamespaceUri.NULL, "type");
            XPathEvaluator evaluator = new XPathEvaluator(config);
            IndependentContext env = (IndependentContext) evaluator.getStaticContext();
            String nsContext = attributes.getValue(NamespaceUri.NULL, "nsContext");
            NamespaceMap nsMap = PackageLoaderHE.fromExportedNamespaces(nsContext);
            env.setNamespaceResolver(nsMap);
            env.setDefaultElementNamespace(NamespaceUri.of(defaultNamespace));
            env.setBaseURI(xmlBase);
            XPathExpression condition;
            try {
                condition = evaluator.createExpression(xpathText);
            } catch (XPathException err) {
                throw new SchemaException(err);
            }
            TypeAlternative alt = new TypeAlternative(loader.config, condition, xpathText);
            alt.setBaseURI(xmlBase);
            alt.setDefaultElementNamespace(NamespaceUri.of(defaultNamespace));
            alt.setNamespaceContext(nsMap);
            alt.setParameters(new HashMap<>());
            if (type.startsWith("#")) {
                int fp = config.getNamePool().getFingerprint(NamespaceUri.SCHEMA, type.substring(1));
                alt.setTypeReference(new TypeReference(fp, loader.config, null));
            } else {
                loader.fixups.add(new Fixup(alt, type, FIXUP_ALTERNATIVE_TYPE));
            }
            elementDecl.addTypeAlternative(alt);
        }

        @Override
        public void processElementEnd() {
        }
    }

    /**
     * Handle an identity constraint reference in an element declaratoin
     */

    private static class IdentityConstraintReferenceHandler extends SCMNodeHandler {

        private final ElementDecl container;

        public IdentityConstraintReferenceHandler(ElementDecl container) {
            this.container = container;
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {

            String ref = attributes.getValue(NamespaceUri.NULL, "ref");

            IdentityConstraintReference constraintRef = new IdentityConstraintReference();
            container.addIdentityConstraint(constraintRef);
            loader.fixups.add(new Fixup(constraintRef, ref, FIXUP_IDENTITY_CONSTRAINT_REF));

        }

        @Override
        public void processElementEnd() {
        }
    }


    /**
     * Handle an identity constraint (key, keyref, unique)
     */

    private static class IdentityConstraintHandler extends SCMNodeHandler {

        private IdentityConstraint identityConstraint;

        /**
         * Create an IdentityConstraintHandler
         */

        public IdentityConstraintHandler() {
        }

        @Override
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

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {
            String id = attributes.getValue(NamespaceUri.NULL, "id");
            String name = attributes.getValue(NamespaceUri.NULL, "name");
            String namespace = attributes.getValue(NamespaceUri.NULL, "targetNamespace");
            NamespaceUri targetNS = NamespaceUri.of(namespace);
            loader.targetNamespaces.add(targetNS);
            String key = attributes.getValue(NamespaceUri.NULL, "key");
            StructuredQName nameCode = new StructuredQName("", targetNS, name);
            IdentityConstraint ic;
            //int kind;
            switch (localName) {
                case "unique":
                    ic = new Unique();
                    loader.components.put(id, ic);
                    //kind = StandardNames.XS_UNIQUE;
                    break;
                case "key":
                    ic = new Key();
                    loader.components.put(id, ic);
                    //kind = StandardNames.XS_KEY;
                    break;
                case "keyref":
                    ic = new KeyRef();
                    loader.components.put(id, ic);
                    loader.fixups.add(new Fixup(ic, key, FIXUP_KEYREF_KEY));
                    //kind = StandardNames.XS_KEYREF;
                    break;
                default:
                    throw new AssertionError("unknown IdentityConstraint " + localName);
            }
            ic.setConfiguration(loader.config);
            ic.setConstraintName(nameCode);
            identityConstraint = ic;
        }

        @Override
        public void processElementEnd() {
        }
    }

    /**
     * Handle a selector or field within an identity constraint (key, keyref, unique)
     */

    private static class SelectorHandler extends SCMNodeHandler {

        private final IdentityConstraint identityConstraint;

        /**
         * Create a SelectorHandler for a given IdentityConstraint
         *
         * @param identityConstraint the constraint
         */

        public SelectorHandler(IdentityConstraint identityConstraint) {
            this.identityConstraint = identityConstraint;
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) throws SchemaException {
            String xpath = attributes.getValue(NamespaceUri.NULL, "xpath");
            boolean isField = localName.equals("field");
            String defaultNamespace = attributes.getValue(NamespaceUri.NULL, "defaultNamespace");
            NamespaceUri defaultNamespaceUri = NamespaceUri.of(defaultNamespace);
            IndependentContext env = new IndependentContext(loader.config);
            String nsContext = attributes.getValue(NamespaceUri.NULL, "nsContext");
            NamespaceMap nsMap = PackageLoaderHE.fromExportedNamespaces(nsContext);
            env.setNamespaceResolver(nsMap);
            env.setDefaultElementNamespace(defaultNamespaceUri);
            env.setXPathLanguageLevel(20);
            Pattern selector;
            try {
                selector = new SelectionParser(env).parseSelector(xpath, env, isField);
            } catch (XPathException err) {
                throw new SchemaException(err);
            }
            if (isField) {
                IdentityField idf = new IdentityField(xpath);
                idf.setSelection(selector);
                idf.setNamespaceContext(namespaces);
                idf.setXPathDefaultNamespace(defaultNamespaceUri);
                identityConstraint.addField(idf);
            } else {
                IdentitySelector ids = new IdentitySelector(xpath);
                ids.setSelection(selector);
                ids.setNamespaceContext(namespaces);
                ids.setXPathDefaultNamespace(defaultNamespaceUri);
                identityConstraint.setSelector(ids);
            }
        }

        @Override
        public void processElementEnd() {
        }
    }


    /**
     * Handle an attribute declaration in the SCM schema document
     */

    private static class AttributeHandler extends SCMNodeHandler {

        private AttributeDecl attributeDecl;

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("fixed") || localName.equals("default")) {
                return new ValueConstraintHandler(attributeDecl, localName);
            } else {
                return null;
            }
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {
            String id = attributes.getValue(NamespaceUri.NULL, "id");
            String name = attributes.getValue(NamespaceUri.NULL, "name");
            NamespaceUri namespace = NamespaceUri.of(attributes.getValue(NamespaceUri.NULL, "targetNamespace"));
            loader.targetNamespaces.add(namespace);
            StructuredQName attributeName = null;
            if (name != null) {
                attributeName = new StructuredQName("", namespace, name);
            }
            boolean isGlobal = toBoolean(attributes.getValue(NamespaceUri.NULL, "global"));
            boolean isInheritable = toBoolean(attributes.getValue(NamespaceUri.NULL, "inheritable"));
            String type = attributes.getValue(NamespaceUri.NULL, "type");
            String defaultValue = attributes.getValue(NamespaceUri.NULL, "default");

            attributeDecl = new AttributeDecl(loader.config, isGlobal);
            loader.components.put(id, attributeDecl);
            attributeDecl.setAttributeName(attributeName, loader.config.getNamePool().allocateFingerprint(namespace, name));
            attributeDecl.setInheritable(isInheritable);

            if (defaultValue != null) {
                ValueConstraint vc = new ValueConstraint(ValueConstraint.Variety.DEFAULT, StringView.tidy(defaultValue));
                attributeDecl.setValueConstraint(vc);
            }

            if (type != null) {
                if (type.startsWith("#")) {
                    int fp = loader.config.getNamePool().getFingerprint(NamespaceUri.SCHEMA, type.substring(1));
                    attributeDecl.setTypeReference(new TypeReference(fp, loader.config, null));
                } else {
                    loader.fixups.add(new Fixup(attributeDecl, type, FIXUP_ATTRIBUTE_TYPE));
                }
            }

            if (isGlobal) {
                loader.schema.addAttributeDecl(attributeDecl);
            }

        }

        @Override
        public void processElementEnd() {
        }

    }

    /**
     * Handle a complex type definition in the SCM schema document. This may be a named complex type
     * or an anonymous type.
     */

    private static class ComplexTypeHandler extends SCMNodeHandler {

        UserComplexType complexType;

        @Override
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
                    return new FiniteStateMachineHandler(complexType, loader);
                case "assertion":
                    return new AssertionHandler(complexType);
                default:
                    return null;
            }
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) throws SchemaException {
            EnterpriseConfiguration config = loader.config;
            String id = attributes.getValue(NamespaceUri.NULL, "id");
            String name = attributes.getValue(NamespaceUri.NULL, "name");
            String namespace = attributes.getValue(NamespaceUri.NULL, "targetNamespace");
            NamespaceUri namespaceUri = NamespaceUri.of(namespace);
            loader.targetNamespaces.add(namespaceUri);
            int fp;
            if (name == null) {
                fp = loader.config.getNamePool().allocateFingerprint(NamespaceUri.ANONYMOUS, id);
                name = id;
                namespaceUri = NamespaceUri.ANONYMOUS;
            } else {
                fp = loader.config.getNamePool().allocateFingerprint(namespaceUri, name);
            }
            String base = attributes.getValue(NamespaceUri.NULL, "base");
            String variety = attributes.getValue(NamespaceUri.NULL, "variety");
            boolean isAbstract = toBoolean(attributes.getValue(NamespaceUri.NULL, "abstract"));
            String derivation = attributes.getValue(NamespaceUri.NULL, "derivationMethod");
            String simpleType = attributes.getValue(NamespaceUri.NULL, "simpleType");
            complexType = new UserComplexType(config, null);
            complexType.setEmptyCombinedAttributeGroup();
            //complexType.setSaxonDebug(true);
            loader.components.put(id, complexType);
            complexType.setTypeName(new StructuredQName("", namespaceUri, name), fp);
            if (base != null) {
                if (base.startsWith("#")) {
                    int basefp = config.getNamePool().getFingerprint(NamespaceUri.SCHEMA, base.substring(1));
                    complexType.setBaseTypeReference(new TypeReference(basefp, config, null));
                } else {
                    loader.fixups.add(new Fixup(complexType, base, FIXUP_COMPLEX_TYPE_BASE_TYPE));
                }
            }
            ComplexVariety varietyCode;
            switch (variety) {
                case "mixed":
                    varietyCode = ComplexVariety.MIXED;
                    break;
                case "empty":
                    varietyCode = ComplexVariety.EMPTY;
                    break;
                case "simple":
                    varietyCode = ComplexVariety.SIMPLE;
                    break;
                case "element-only":
                default:
                    varietyCode = ComplexVariety.ELEMENT_ONLY;
                    break;
            }
            complexType.setVariety(varietyCode);
            complexType.setAbstract(isAbstract);
            complexType.setDerivationMethodName(derivation);
            if (simpleType != null) {
                if (simpleType.startsWith("#")) {
                    int typefp = config.getNamePool().getFingerprint(NamespaceUri.SCHEMA, simpleType.substring(1));
                    complexType.setSimpleContentTypeReference(new TypeReference(typefp, config, null));
                } else {
                    loader.fixups.add(new Fixup(complexType, simpleType, FIXUP_COMPLEX_TYPE_SIMPLE_TYPE));
                }
            }
            if (name == null) {
                loader.schema.addAnonymousType(complexType);
            } else {
                loader.schema.addType(complexType);
            }
            String blockAtt = attributes.getValue(NamespaceUri.NULL, "block");
            int blockSet = parseFinalOrBlock("block",
                    blockAtt,
                    Derivation.DERIVATION_EXTENSION |
                                                     Derivation.DERIVATION_RESTRICTION);
            complexType.setBlock(blockSet);
            String finalAtt = attributes.getValue(NamespaceUri.NULL, "final");
            int finalSet = parseFinalOrBlock("final",
                    finalAtt,
                    Derivation.DERIVATION_EXTENSION |
                                                     Derivation.DERIVATION_RESTRICTION);
            complexType.setFinalProhibitions(finalSet);
        }

        @Override
        public void processElementEnd() {
        }

    }

    /**
     * Handle a simple type definition in the SCM schema document. This may be a named simple type or an
     * anonymous simple type
     */

    private static class SimpleTypeHandler extends SCMNodeHandler {

        private UserSimpleType simpleType;

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            if (Facet.isFacetName(localName, loader.config.getXsdVersion()) || localName.equals("assert")) {
                return new FacetHandler(simpleType);
            } else if (localName.equals("preprocess")) {
                return new PreprocessHandler(simpleType);
            } else {
                return null;
            }
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) throws SchemaException {
            EnterpriseConfiguration config = loader.config;
            String id = attributes.getValue(NamespaceUri.NULL, "id");
            String name = attributes.getValue(NamespaceUri.NULL, "name");
            String namespace = attributes.getValue(NamespaceUri.NULL, "targetNamespace");
            NamespaceUri namespaceUri = NamespaceUri.of(namespace);
            loader.targetNamespaces.add(namespaceUri);
            int fingerprint;
            if (name == null) {
                fingerprint = config.getNamePool().allocateFingerprint(NamespaceUri.ANONYMOUS, id);
                name = id;
                namespaceUri = NamespaceUri.ANONYMOUS;
                // TODO: this won't be unique if loading several SCM schemas
            } else {
                fingerprint = loader.config.getNamePool().allocateFingerprint(namespaceUri, name);
            }
            String base = attributes.getValue(NamespaceUri.NULL, "base");
            String variety = attributes.getValue(NamespaceUri.NULL, "variety");
            String itemType = attributes.getValue(NamespaceUri.NULL, "itemType");
            String memberTypes = attributes.getValue(NamespaceUri.NULL, "memberTypes");
            //String primitiveType = attributes.getValue(NamespaceUri.NULL, "primitiveType");
            if ("atomic".equals(variety)) {
                simpleType = new UserAtomicType(config);
            } else if ("list".equals(variety)) {
                simpleType = new UserListType(config);
            } else if ("union".equals(variety)) {
                simpleType = new UserUnionType(config);
            } else {
                throw new SchemaException("Unknown simple type variety: " + variety);
            }
            loader.components.put(id, simpleType);
            simpleType.setTypeName(new StructuredQName("", namespaceUri, name), fingerprint);
            if (base != null) {
                if (base.startsWith("#")) {
                    int fp = loader.config.getNamePool().getFingerprint(NamespaceUri.SCHEMA, base.substring(1));
                    simpleType.setBaseTypeReference(new TypeReference(fp, loader.config, null));
                } else {
                    loader.fixups.add(new Fixup(simpleType, base, FIXUP_SIMPLE_TYPE_BASE_TYPE));
                }
            }

            if (itemType != null) {
                if (itemType.startsWith("#")) {
                    int fp = loader.config.getNamePool().getFingerprint(NamespaceUri.SCHEMA, itemType.substring(1));
                    ((UserListType) simpleType).setItemTypeReference(new TypeReference(fp, loader.config, null));
                } else {
                    loader.fixups.add(new Fixup(simpleType, itemType, FIXUP_SIMPLE_TYPE_ITEM_TYPE));
                }
            }
            if (memberTypes != null) {
                StringTokenizer st = new StringTokenizer(memberTypes, " \t\n\r");
                while (st.hasMoreTokens()) {
                    String memberType = st.nextToken();
                    if (memberType.startsWith("#")) {
                        int fp = loader.config.getNamePool().getFingerprint(NamespaceUri.SCHEMA, memberType.substring(1));
                        ((UserUnionType) simpleType).addMemberTypeReference(new TypeReference(fp, loader.config, null));
                    } else {
                        loader.fixups.add(new Fixup(simpleType, memberType, FIXUP_SIMPLE_TYPE_MEMBER_TYPE));
                    }

                }

            }
            if (name == null) {
                loader.schema.addAnonymousType(simpleType);
            } else {
                loader.schema.addType(simpleType);
            }
        }

        @Override
        public void processElementEnd() {
        }

    }

    /**
     * Handle a named attribute group definition in the SCM Schema Document
     */

    private static class AttributeGroupHandler extends SCMNodeHandler {

        private AttributeGroupDecl group;

        public AttributeGroupHandler() {

        }

        @Override
        public void setSchemaModelLoader(SchemaModelLoader loader) {
            super.setSchemaModelLoader(loader);
            group = new AttributeGroupDecl(loader.config);
        }

        @Override
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

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {
            String id = attributes.getValue(NamespaceUri.NULL, "id");
            String name = attributes.getValue(NamespaceUri.NULL, "name");
            String namespace = attributes.getValue(NamespaceUri.NULL, "targetNamespace");
            NamespaceUri targetNS = NamespaceUri.of(namespace);
            loader.targetNamespaces.add(targetNS);
            StructuredQName nameCode = null;
            if (name != null) {
                nameCode = new StructuredQName("", targetNS, name);
            }

            loader.components.put(id, group);
            group.setAttributeGroupName(nameCode);
            loader.schema.addAttributeGroup(group);
        }

        @Override
        public void processElementEnd() {
        }

    }

    /**
     * Handle a named attribute group definition in the SCM Schema Document
     */

    private static class NotationHandler extends SCMNodeHandler {

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {
            Notation notation = new Notation(loader.config, null);
            String id = attributes.getValue(NamespaceUri.NULL, "id");
            String name = attributes.getValue(NamespaceUri.NULL, "name");
            String namespace = attributes.getValue(NamespaceUri.NULL, "targetNamespace");
            NamespaceUri targetNS = NamespaceUri.of(namespace);
            loader.targetNamespaces.add(targetNS);
            StructuredQName nameCode;
            if (name == null) {
                nameCode = new StructuredQName("anon", NamespaceUri.ANONYMOUS, id);
                // TODO: this won't be unique if loading several SCM schemas
            } else {
                nameCode = new StructuredQName("", targetNS, name);
            }
            notation.setNotationName(nameCode);
            String systemId = attributes.getValue(NamespaceUri.NULL, "systemId");
            notation.setNotationSystemId(systemId);
            String publicId = attributes.getValue(NamespaceUri.NULL, "publicId");
            notation.setNotationPublicId(publicId);
            loader.components.put(id, notation);
            loader.schema.addNotation(notation);
        }

        @Override
        public void processElementEnd() {
        }

    }

    /**
     * Handle a named model group definition in the SCM Schema document
     */

    private static class ModelGroupDefinitionHandler extends ModelGroupHandler {

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {
            String id = attributes.getValue(NamespaceUri.NULL, "id");
            String name = attributes.getValue(NamespaceUri.NULL, "name");
            String namespace = attributes.getValue(NamespaceUri.NULL, "targetNamespace");
            NamespaceUri namespaceUri = NamespaceUri.of(namespace);
            loader.targetNamespaces.add(namespaceUri);
            StructuredQName modelGroupName = new StructuredQName("", namespaceUri, name);
            ModelGroupDefinition mgd = new ModelGroupDefinition(loader.config, null);
            mgd.setModelGroupName(modelGroupName);
            loader.components.put(id, mgd);
            loader.schema.addGroup(mgd);
            group = mgd;
        }

        @Override
        public void processElementEnd() {
        }

    }

    /**
     * Handle an attribute use component in the SCM schema document
     */

    private static class AttributeUseHandler extends SCMNodeHandler {

        private final SchemaComponent container;
        private AttributeUse attributeUse;

        public AttributeUseHandler(SchemaComponent container) {
            this.container = container;
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("fixed") || localName.equals("default")) {
                return new ValueConstraintHandler(attributeUse, localName);
            } else {
                return null;
            }
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {

            boolean required = toBoolean(attributes.getValue(NamespaceUri.NULL, "required"));
            boolean isInheritable = toBoolean(attributes.getValue(NamespaceUri.NULL, "inheritable"));
            String ref = attributes.getValue(NamespaceUri.NULL, "ref");
            String defaultValue = attributes.getValue(NamespaceUri.NULL, "default");

            attributeUse = new AttributeUse(loader.config);
            attributeUse.setInheritable(isInheritable ? 1 : 0);
            if (container instanceof UserComplexType) {
                ((UserComplexType) container).addNonInheritedAttributeUse(attributeUse);
            } else {
                ((AttributeGroupDecl) container).addAttributeUse(attributeUse);
            }

            attributeUse.setUse(required ? Optionality.REQUIRED : Optionality.OPTIONAL);

            if (defaultValue != null) {
                ValueConstraint vc = new ValueConstraint(ValueConstraint.Variety.DEFAULT, StringView.tidy(defaultValue));
                attributeUse.setValueConstraint(vc);
            }
            loader.fixups.add(new Fixup(attributeUse, ref, FIXUP_ATTRIBUTE_USE_DECLARATION));

        }

        @Override
        public void processElementEnd() {
        }
    }

    /**
     * Handle a facet definition in the SCM Schema document
     */

    private static class FacetHandler extends SCMNodeHandler {

        private final UserSimpleType simpleType;

        public FacetHandler(UserSimpleType simpleType) {
            this.simpleType = simpleType;
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("assertion")) {
                return new AssertionHandler(simpleType);
            }
            return null;
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) throws SchemaException {
            if (!localName.equals("assert")) {
                String value = attributes.getValue(NamespaceUri.NULL, "value");
                Facet facet = Facet.makeFacet(localName, value, loader.config, namespaces);
                String fixed = attributes.getValue(NamespaceUri.NULL, "fixed");
                if (fixed != null) {
                    facet.setFixed(toBoolean(fixed));
                }
                if (facet instanceof PatternFacet) {
                    ((PatternFacet) facet).setDoNotMerge(true);
                    String flags = attributes.getValue(NamespaceUri.NULL, "flags");
                    if (flags != null) {
                        ((PatternFacet) facet).setFlags(flags);
                    }
                    ((PatternFacet) facet).compile(null);
                }
                simpleType.addFacet(facet);
            }
        }

        @Override
        public void processElementEnd() {
        }
    }

    /**
     * Handle a saxon:preprocess facet definition in the SCM Schema document
     */

    private static class PreprocessHandler extends SCMNodeHandler {

        private final PreprocessFacet facet;

        public PreprocessHandler(UserSimpleType simpleType) {
            facet = new PreprocessFacet();
            simpleType.addFacet(facet);
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("assertion")) {
                return new AssertionHandler(facet);
            }
            return null;
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {

            String fixed = attributes.getValue(NamespaceUri.NULL, "fixed");
            if (fixed != null) {
                facet.setFixed(toBoolean(fixed));
            }
        }

        @Override
        public void processElementEnd() {
        }
    }


    /**
     * Handle a particle in the SCM schema document. This may be an element particle, and
     * element wildcard particle, or a model group particle. If it is a model group particle
     * it may either be a reference to a named model group definition, or may contain a
     * model group
     */

    private static class ParticleHandler extends SCMNodeHandler {

        private final SchemaComponent container;
        private Particle particle;

        public ParticleHandler(SchemaComponent container) {
            this.container = container;
        }

        @Override
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

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) throws SchemaException {
            EnterpriseConfiguration config = loader.config;
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
            String minOccursAtt = attributes.getValue(NamespaceUri.NULL, "minOccurs");
            int minOccurs = 1;
            if (minOccursAtt != null) {
                minOccurs = Integer.parseInt(minOccursAtt.trim());
            }
            particle.setMinOccurs(minOccurs);
            String maxOccursAtt = attributes.getValue(NamespaceUri.NULL, "maxOccurs");
            int maxOccurs = -1; // meaning "unbounded"
            if (maxOccursAtt != null) {
                maxOccursAtt = maxOccursAtt.trim();
                if (!maxOccursAtt.equals("unbounded")) {
                    maxOccurs = Integer.parseInt(maxOccursAtt);
                }
            }
            particle.setMaxOccurs(maxOccurs);

            if (particle instanceof ElementParticle) {
                String ref = attributes.getValue(NamespaceUri.NULL, "ref");
                loader.fixups.add(new Fixup(particle, ref, FIXUP_ELEMENT_PARTICLE_REF));
            } else if (particle instanceof ModelGroupParticle) {
                String ref = attributes.getValue(NamespaceUri.NULL, "ref");
                if (ref != null) {
                    loader.fixups.add(new Fixup(particle, ref, FIXUP_MODEL_GROUP_PARTICLE_REF));
                }
            } else if (particle instanceof ElementWildcard) {
                String ref = attributes.getValue(NamespaceUri.NULL, "ref");
                loader.fixups.add(new Fixup(particle, ref, FIXUP_ELEMENT_WILDCARD_REF));
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

        @Override
        public void processElementEnd() {
        }
    }

    /**
     * Handle an unnamed model group, that is, a sequence, choice, or all.
     */

    private static class ModelGroupHandler extends SCMNodeHandler {

        private final ModelGroupParticle referrer;
        protected ModelGroup group;

        public ModelGroupHandler() {
            referrer = null;
        }

        public ModelGroupHandler(ModelGroupParticle referrer) {
            this.referrer = referrer;
        }

        @Override
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

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) throws SchemaException {
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
            loader.anonComponents.add(group);
        }

        @Override
        public void processElementEnd() {
        }
    }

    /**
     * Handle an assertion in the SCM schema document.
     */

    private static class AssertionHandler extends SCMNodeHandler {

        private final Object container;

        public AssertionHandler(Object container) {
            this.container = container;
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) throws SchemaException {
            String xpathText = attributes.getValue(NamespaceUri.NULL, "test");
            String xmlBase = attributes.getValue(NamespaceUri.XML, "base");
            String defaultNamespace = attributes.getValue(NamespaceUri.NULL, "defaultNamespace");
            NamespaceUri defaultNamespaceUri = NamespaceUri.of(defaultNamespace);
            String sweep = attributes.getValue(NamespaceUri.NULL, "sweep");

            XPathEvaluator evaluator;
            if (container instanceof UserComplexType) {
                evaluator = new XSDAssert.CustomizedXPath(loader.config);
            } else {
                evaluator = new XPathEvaluator(loader.config);
            }
            IndependentContext env = (IndependentContext) evaluator.getStaticContext();
            String nsContext = attributes.getValue(NamespaceUri.NULL, "nsContext");
            NamespaceMap nsMap = PackageLoaderHE.fromExportedNamespaces(nsContext);
            env.setNamespaceResolver(nsMap);
            env.setBaseURI(xmlBase);
            env.setDefaultElementNamespace(defaultNamespaceUri);
            env.getPackageData().setLocalLicenseId(loader.schema.getLocalLicenseId());
            env.setSchemaAware(true);
            boolean isSimple = (container instanceof UserSimpleType) ||
                    (container instanceof UserComplexType && ((UserComplexType) container).isSimpleContent());
            XPathVariable valueVariable = null;
            if (isSimple || container instanceof PreprocessFacet || xpathText.contains("$value")) {
                valueVariable = env.declareVariable(NamespaceUri.NULL, "value");
            }
            XPathExpression condition;
            try {
                condition = evaluator.createExpression(xpathText);
            } catch (XPathException err) {
                throw new SchemaException(err);
            }
            Assertion assertion = new Assertion(loader.config, condition, valueVariable, new HashMap<>(), xpathText);
            assertion.setStaticContext(env);
            if (sweep != null) {
                assertion.setSweep(Sweep.valueOf(sweep));
            }
            assertion.setReturnsFailingNodes(container instanceof UserComplexType && sweep == null);
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

        @Override
        public void processElementEnd() {
        }
    }


    /**
     * Handle an open content element in the SCM schema document.
     */

    private static class OpenContentHandler extends SCMNodeHandler {

        private final UserComplexType container;

        public OpenContentHandler(UserComplexType container) {
            this.container = container;
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            //if (localName.equals("wildcard")) {
            //    return new WildcardHandler(container, mode);
            //} else {
            return null;
            //}
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {
            String mode = attributes.getValue(NamespaceUri.NULL, "mode");
            String wildcardRef = attributes.getValue(NamespaceUri.NULL, "wildcard");
            if (mode.equals("suffix")) {
                loader.fixups.add(new Fixup(container, wildcardRef, FIXUP_COMPLEX_TYPE_OPEN_CONTENT_SUFFIX));
            } else {
                loader.fixups.add(new Fixup(container, wildcardRef, FIXUP_COMPLEX_TYPE_OPEN_CONTENT_INTERLEAVE));
            }
        }

        @Override
        public void processElementEnd() {
        }
    }

    /**
     * Handle a attribute wildcard in the SCM schema document.
     */

    private static class AttributeWildcardHandler extends SCMNodeHandler {

        private final SchemaComponent container;

        public AttributeWildcardHandler(SchemaComponent container) {
            this.container = container;
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {
            AttributeWildcard awc = new AttributeWildcard((EnterpriseConfiguration) loader.getConfiguration());
            if (container instanceof AttributeGroupDecl) {
                ((AttributeGroupDecl) container).setAnyAttribute(awc);
            } else if (container instanceof UserComplexType) {
                ((UserComplexType) container).setNonInheritedAnyAttribute(awc);
            } else {
                throw new IllegalArgumentException(container.getClass().getName());
            }
            String ref = attributes.getValue(NamespaceUri.NULL, "ref");
            loader.fixups.add(new Fixup(awc, ref, FIXUP_ATTRIBUTE_WILDCARD_REF));
        }

        @Override
        public void processElementEnd() {
        }
    }

    /**
     * Handle a fixed value element in the SCM schema document.
     */

    private static class ValueConstraintHandler extends SCMNodeHandler {

        private final SchemaComponent container;
        private final ArrayList<AtomicValue> atomicValues = new ArrayList<>(4);
        private String lexicalForm;
        private final String variety;

        public ValueConstraintHandler(SchemaComponent container, String variety) {
            this.container = container;
            this.variety = variety;
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("item")) {
                return new ItemHandler(this);
            } else {
                return null;
            }
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {
            lexicalForm = attributes.getValue(NamespaceUri.NULL, "lexicalForm");
        }

        @Override
        public void processElementEnd() {
            AtomicSequence v;
            if (atomicValues.size() == 1) {
                v = atomicValues.get(0);
            } else {
                v = new AtomicArray(atomicValues);
            }
            if (container instanceof ComponentWithValueConstraint) {
                ValueConstraint.Variety var = variety.equals("fixed") ? ValueConstraint.Variety.FIXED : ValueConstraint.Variety.DEFAULT;
                ValueConstraint vc = new ValueConstraint(var, lexicalForm==null ? null : StringView.tidy(lexicalForm));
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

    private static class ItemHandler extends SCMNodeHandler {

        private final ValueConstraintHandler container;

        public ItemHandler(ValueConstraintHandler container) {
            this.container = container;
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) throws SchemaException {
            String type = attributes.getValue(NamespaceUri.NULL, "type");
            BuiltInAtomicType t = (BuiltInAtomicType) loader.getSchema().getSchemaType(new StructuredQName("", NamespaceUri.SCHEMA, type.substring(1)));
            String value = attributes.getValue(NamespaceUri.NULL, "value");
            String namespaceAtt = attributes.getValue(NamespaceUri.NULL, "namespace");
            AtomicValue typedValue;
            try {
                if (namespaceAtt == null) {
                    typedValue = t.getTypedValue(StringView.tidy(value), null, loader.config.getConversionRules()).head();
                } else {
                    NamespaceUri namespace = NamespaceUri.of(namespaceAtt);
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

        @Override
        public void processElementEnd() {
        }
    }


    /**
     * Handle a wildcard within an element or attribute wildcard component
     */

    private static class WildcardHandler extends SCMNodeHandler {

        public WildcardHandler() {
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) throws SchemaException {

            Wildcard wildcard = new Wildcard((EnterpriseConfiguration) loader.getConfiguration());
            String id = attributes.getValue(NamespaceUri.NULL, "id");
            if (id != null) {
                loader.components.put(id, wildcard);
            }
            String processContents = attributes.getValue(NamespaceUri.NULL, "processContents");
            wildcard.setProcessContents(processContents);
            String constraint = attributes.getValue(NamespaceUri.NULL, "constraint");
            switch (constraint) {
                case "any":
                    // no action
                    break;
                case "not": {
                    String namespaceList = attributes.getValue(NamespaceUri.NULL, "namespaces");
                    StringTokenizer st = new StringTokenizer(namespaceList, " \t\n\r");
                    while (st.hasMoreTokens()) {
                        String ns = st.nextToken();
                        wildcard.addDisallowedNamespace(ns.equals("##local") ? NamespaceUri.NULL : NamespaceUri.of(ns));
                    }
                    break;
                }
                case "enumeration": {
                    wildcard.setNoNamespacesAllowed();
                    String namespaceList = attributes.getValue(NamespaceUri.NULL, "namespaces");
                    StringTokenizer st = new StringTokenizer(namespaceList, " \t\n\r");
                    while (st.hasMoreTokens()) {
                        String ns = st.nextToken();
                        wildcard.addAllowedNamespace(ns.equals("##local") ? NamespaceUri.NULL : NamespaceUri.of(ns));
                    }
                    break;
                }
                default:
                    throw new SchemaException("Unknown wildcard constraint " + constraint);
            }
            String disallowedQNames = attributes.getValue(NamespaceUri.NULL, "notQName");
            if (disallowedQNames != null) {
                NamePool pool = loader.getNamePool();
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

        @Override
        public void processElementEnd() {
        }
    }


    /**
     * Handle a finite state machine within a complex type
     */

    private static class FiniteStateMachineHandler extends SCMNodeHandler {

        private final FiniteStateMachine fsm;

        public FiniteStateMachineHandler(UserComplexType complexType, SchemaModelLoader loader) {
            fsm = new FiniteStateMachine(complexType, true);
            loader.pendingCompilations.put(complexType, fsm);
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("state")) {
                return new FiniteStateMachineStateHandler(this);
            } else {
                return null;
            }
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {
            //
        }

        public FiniteStateMachine getFiniteStateMachine() {
            return fsm;
        }

        @Override
        public void processElementEnd() {
        }

    }

    /**
     * Handle a state within a finite state machine
     */

    private static class FiniteStateMachineStateHandler extends SCMNodeHandler {

        private final FiniteStateMachineHandler fsmHandler;
        private DeterminizedState state;

        public FiniteStateMachineStateHandler(FiniteStateMachineHandler fsmHandler) {
            this.fsmHandler = fsmHandler;
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            if (localName.equals("edge")) {
                return new FiniteStateMachineEdgeHandler(fsmHandler, state);
            } else {
                return null;
            }
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {
            String stateNumber = attributes.getValue(NamespaceUri.NULL, "nr");
            FiniteStateMachine fsm = fsmHandler.getFiniteStateMachine();
            if (stateNumber.equals("0")) {
                state = (DeterminizedState) fsm.getInitialState();
            } else {
                state = new DeterminizedState(fsm);
            }
            String isFinal = attributes.getValue(NamespaceUri.NULL, "final");
            state.setFinalState(toBoolean(isFinal)) ;
            String min = attributes.getValue(NamespaceUri.NULL, "minOccurs");
            String max = attributes.getValue(NamespaceUri.NULL, "maxOccurs");
            if (min != null && max != null) {
                state.setLimits(Integer.parseInt(min), max.equals("unbounded") ? -1 : Integer.parseInt(max));
            }
            String afterMax = attributes.getValue(NamespaceUri.NULL, "afterMax");
            if (afterMax != null) {
                PendingEdge leavingEdge = new PendingEdge();
                leavingEdge.machine = fsm;
                leavingEdge.termId = "##afterMax";
                leavingEdge.fromState = Integer.parseInt(stateNumber);
                leavingEdge.toState = Integer.parseInt(afterMax);
                loader.pendingEdges.add(leavingEdge);
            }
        }

        @Override
        public void processElementEnd() {
        }
    }

    /**
     * Handle an Edge within a finite state machine
     */

    private static class FiniteStateMachineEdgeHandler extends SCMNodeHandler {

        private final FiniteStateMachineHandler fsm;
        private final DeterminizedState state;

        public FiniteStateMachineEdgeHandler(FiniteStateMachineHandler fsm, DeterminizedState state) {
            this.fsm = fsm;
            this.state = state;
        }

        @Override
        public SCMNodeHandler handleChildElement(String localName) {
            return null;
        }

        @Override
        public void processElementStart(String localName, AttributeMap attributes, NamespaceMap namespaces) {
            String term = attributes.getValue(NamespaceUri.NULL, "term");
            int targetState = Integer.parseInt(attributes.getValue(NamespaceUri.NULL, "to"));
            PendingEdge pe = new PendingEdge();
            pe.machine = fsm.getFiniteStateMachine();
            pe.fromState = state.getStateNumber();
            pe.toState = targetState;
            pe.termId = term;
            loader.pendingEdges.add(pe);
        }

        @Override
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

    public static int parseFinalOrBlock(String attrName, String finalValue, int allowed) throws SchemaException {
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
                        v = Derivation.DERIVATION_UNION;
                        break;
                    case "list":
                        v = Derivation.DERIVATION_LIST;
                        break;
                    case "restriction":
                        v = Derivation.DERIVATION_RESTRICTION;
                        break;
                    case "extension":
                        v = Derivation.DERIVATION_EXTENSION;
                        break;
                    case "substitution":
                        v = Derivation.DERIVE_BY_SUBSTITUTION;
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

    private static class Fixup {

        private final Object source;
        private final String targetId;
        private final int property;

        public Fixup(Object source, String targetId, int property) {
            this.source = source;
            this.targetId = targetId;
            this.property = property;
        }

        public void apply(SchemaModelLoader loader) {
            switch (property) {
                case FIXUP_ELEMENT_TYPE: {
                    ((ElementDecl) source).setTypeReference(loader.makeTypeReference(targetId));
                    break;
                }
                case FIXUP_ELEMENT_SUBSTITUTION_GROUP: {
                    ElementDecl head = (ElementDecl) loader.components.get(targetId);
                    ((ElementDecl) source).addSubstitutionGroupHead(head.getComponentName());
                    break;
                }
                case FIXUP_ALTERNATIVE_TYPE: {
                    ((TypeAlternative) source).setTypeReference(loader.makeTypeReference(targetId));
                    break;
                }
                case FIXUP_ATTRIBUTE_TYPE: {
                    ((AttributeDecl) source).setTypeReference(loader.makeTypeReference(targetId));
                    break;
                }
                case FIXUP_COMPLEX_TYPE_BASE_TYPE: {
                    ((UserComplexType) source).setBaseTypeReference(loader.makeTypeReference(targetId));
                    break;
                }
                case FIXUP_COMPLEX_TYPE_SIMPLE_TYPE: {
                    ((UserComplexType) source).setSimpleContentTypeReference(loader.makeTypeReference(targetId));
                    break;
                }
                case FIXUP_COMPLEX_TYPE_OPEN_CONTENT_SUFFIX: {
                    Wildcard wildcard = (Wildcard) loader.components.get(targetId);
                    UserComplexType type = ((UserComplexType) source);
                    type.setOpenContentWildcard(wildcard, "suffix", true);
                    CompiledComplexType cct = loader.getSchema().getCompiledComplexType(type);
                    FiniteStateMachine fsm = cct.getFiniteStateMachine();
                    if (fsm != null) {
                        fsm.setOpenContentWildcard(wildcard, false);
                    }
                    break;
                }
                case FIXUP_COMPLEX_TYPE_OPEN_CONTENT_INTERLEAVE: {
                    Wildcard wildcard = (Wildcard) loader.components.get(targetId);
                    UserComplexType type = ((UserComplexType) source);
                    type.setOpenContentWildcard(wildcard, "interleave", true);
                    CompiledComplexType cct = loader.getSchema().getCompiledComplexType(type);
                    FiniteStateMachine fsm = cct.getFiniteStateMachine();
                    if (fsm != null) {
                        fsm.setOpenContentWildcard(wildcard, true);
                    }
                    break;
                }
                case FIXUP_SIMPLE_TYPE_ITEM_TYPE: {
                    ((UserListType) source).setItemTypeReference(loader.makeTypeReference(targetId));
                    break;
                }
                case FIXUP_SIMPLE_TYPE_MEMBER_TYPE: {
                    ((UserUnionType) source).addMemberTypeReference(loader.makeTypeReference(targetId));
                    break;
                }
                case FIXUP_SIMPLE_TYPE_BASE_TYPE: {
                    ((UserSimpleType) source).setBaseTypeReference(loader.makeTypeReference(targetId));
                    break;
                }
                case FIXUP_ATTRIBUTE_USE_DECLARATION: {
                    final SchemaComponent target = loader.components.get(targetId);
                    ((AttributeUse) source).setTarget(target);
                    ((AttributeUse) source).setTargetComponentName(((AttributeDecl) target).getComponentName());
                    break;
                }
                case FIXUP_ATTRIBUTE_WILDCARD_REF: {
                    final SchemaComponent target = loader.components.get(targetId);
                    ((AttributeWildcard) source).setWildcard((Wildcard) target);
                    break;
                }
                case FIXUP_ELEMENT_PARTICLE_REF: {
                    SchemaComponent target = loader.components.get(targetId);
                    ((ElementParticle) source).setTarget(target);
                    ((ElementParticle) source).setTargetComponentName(((ElementDecl) target).getComponentName());
                    break;
                }
                case FIXUP_ELEMENT_WILDCARD_REF: {
                    final SchemaComponent target = loader.components.get(targetId);
                    ((ElementWildcard) source).setWildcard((Wildcard) target);
                    break;
                }
                case FIXUP_MODEL_GROUP_PARTICLE_REF: {
                    SchemaComponent target = loader.components.get(targetId);
                    ((ModelGroupParticle) source).setTarget(target);
                    ((ModelGroupParticle) source).setTargetComponentName(((ModelGroupDefinition) target).getModelGroupName());
                    break;
                }
                case FIXUP_KEYREF_KEY: {
                    IdentityConstraint target = (IdentityConstraint) loader.components.get(targetId);
                    ((KeyRef) source).setTargetKey(target);
                    ((KeyRef) source).setTargetComponentName(target.getConstraintName());
                    break;
                }
                case FIXUP_IDENTITY_CONSTRAINT_REF: {
                    IdentityConstraint target = (IdentityConstraint) loader.components.get(targetId);
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

// Copyright (c) 2007-2023 Saxonica Limited