package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.sdoc.SimpleTypeDefinition;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.*;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.trace.ExpressionPresenter;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.DateTimeValue;
import net.sf.saxon.value.QualifiedNameValue;

import java.util.IdentityHashMap;
import java.util.LinkedList;


/**
 * This class provides services enabling a compiled schema to be serialized to disk
 */

public class SchemaModelSerializer {

    private final EnterpriseConfiguration config;
    private final Receiver receiver;
    private final ComplexContentOutputter cco;
    private final LinkedList<SchemaComponent> pendingComponents = new LinkedList<>();
    private final IdentityHashMap<SchemaComponent, String> componentToIdMap = new IdentityHashMap<>(200);
    private final IdentityHashMap<SerializableSchemaComponent, String> serializedComponents = new IdentityHashMap<>(200);
    private int nextComponent = 0;
    private NamespaceMap namespaceMap = NamespaceMap.emptyMap();

    private boolean inStartTag = false;

    /**
     * Create a SchemaModelSerializer
     *
     * @param config the Saxon Configuration. This must be a schema-aware configuration
     * @param out    the receiver to which the compiled schema document will be written
     * @throws IllegalArgumentException if the configuration is not schema-aware
     */

    public SchemaModelSerializer(Configuration config, Receiver out) {
        config.checkLicensedFeature(Configuration.LicenseFeature.SCHEMA_VALIDATION, "schema processing", -1);
        this.config = (EnterpriseConfiguration) config;
        NamespaceReducer reducer = new NamespaceReducer(out);
        this.receiver = new CheckSumFilter(reducer);
        this.cco = new ComplexContentOutputter(this.receiver);
    }

    /**
     * Export the pre-compiled schema components in this configuration to the specified Receiver
     *
     * @throws XPathException if any failure occurs
     */

    public void serialize() throws XPathException {
        cco.open();
        cco.startDocument(ReceiverOption.NONE);
        startElement("schema");
        emitAttribute("generatedAt", DateTimeValue.getCurrentDateTime(null).getStringValue());
        emitAttribute("xsdVersion", config.getConfigurationProperty(Feature.XSD_VERSION));
        String dmk = config.getLicenseFeature("DMK");
        if (dmk != null) {
            emitAttribute("dmk", dmk);
        }
        PreparedSchema schema = config.getSuperSchema();
        // System.err.println("Serializing schema " + schema);
        for (SchemaType type : schema.getAllTypes()) {
            SchemaType sType = type;
            if (sType instanceof SimpleTypeDefinition) {
                sType = ((SimpleTypeDefinition)type).getWorkingType();
            }
            if (sType instanceof SerializableSchemaComponent) {
                serializeComponent((SerializableSchemaComponent) sType);
            }
        }

        for (ElementDecl decl : schema.getElementDeclarations()) {
            serializeComponent(decl);
        }

        for (AttributeDecl decl : schema.getAttributeDeclarations()) {
            serializeComponent(decl);
        }

        for (ModelGroupDefinition decl : schema.getAllModelGroups()) {
            serializeComponent(decl);
        }

        for (AttributeGroupDecl group : schema.getAllAttributeGroups()) {
            serializeComponent(group);
        }

        for (Notation notation : schema.getAllNotations()) {
            serializeComponent(notation);
        }

        while (!pendingComponents.isEmpty()) {
            SerializableSchemaComponent component = (SerializableSchemaComponent) pendingComponents.getFirst();
            serializeComponent(component);
            pendingComponents.removeFirst();
        }

        endElement();
        if (receiver instanceof CheckSumFilter) {
            int c = ((CheckSumFilter) receiver).getChecksum();
            cco.processingInstruction(CheckSumFilter.SIGMA, BMPString.of(Integer.toHexString(c)),
                                      Loc.NONE, ReceiverOption.NONE);
        }
        cco.endDocument();
        cco.close();

    }

    public void startElement(String elementName) throws XPathException {
        if (inStartTag) {
            cco.namespace("", NamespaceUri.NULL, ReceiverOption.NONE);
            cco.startContent();
        }
        inStartTag = true;
        NodeName nc = new FingerprintedQName("scm", NamespaceUri.SCM, elementName);
        cco.startElement(nc, Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);
    }

    public void endElement() throws XPathException {
        if (inStartTag) {
            cco.startContent();
            inStartTag = false;
        }
        receiver.endElement();
    }

    public void emitNamespaceContext(NamespaceResolver resolver) throws XPathException {
        NamespaceMap namespaces = NamespaceMap.fromNamespaceResolver(resolver);
        String nsDisplay = ExpressionPresenter.getNamespacesAsString(namespaces, false);
        emitAttribute("nsContext", nsDisplay);
    }

    public void declareNamespace(String prefix, NamespaceUri uri) throws XPathException {
        cco.namespace(prefix, uri, 0);
    }

    public void emitAttribute(String attributeName, String value) throws XPathException {
        if (!inStartTag) {
            throw new IllegalStateException("Attribute emitted when not in start tag");
        }
        NodeName nc;
        if (attributeName.equals("xml:base")) {
            nc = XML_BASE;
        } else {
            nc = new NoNamespaceName(attributeName);
        }
        cco.attribute(nc, BuiltInAtomicType.UNTYPED_ATOMIC, value, Loc.NONE, ReceiverOption.NONE);
    }

    private static final FingerprintedQName XML_BASE =
            new FingerprintedQName("xml", NamespaceUri.XML, "base", StandardNames.XML_BASE);

    public void setIsSerialized(SerializableSchemaComponent component) {
        serializedComponents.put(component, "");
    }

    public String getId(SchemaComponent component, boolean forSelf) {
        //System.err.println("getID " + nextComponent + "= " + component + (forSelf ? " for Self" : " for Reference"));
        if (component instanceof SimpleTypeDefinition) {
            throw new IllegalStateException("Unresolved SimpleTypeDefinition found");
        }
        if (component instanceof AttributeUse) {
            throw new IllegalStateException("AttributeUse has no ID");
        }

        if (component instanceof SerializableSchemaComponent) {
            // If the referenced component has already been output, then return the integer serial
            // number allocated to it. Otherwise, put the referenced component on a pending list,
            String existing = componentToIdMap.get(component);
            if (existing != null) {
                return existing;
            } else {
                String id = "C" + nextComponent++;
                componentToIdMap.put(component, id);
                if (!forSelf) {
                    pendingComponents.add(component);
                }
                return id;
            }
        } else {
            throw new UnsupportedOperationException(component.getClass().getName());
        }
    }

    private void serializeComponent(SerializableSchemaComponent component) throws XPathException {
        if (serializedComponents.get(component) == null) {
            component.serialize(this);
            serializedComponents.put(component, "");
        }
    }

    public void emitTypedValue(AtomicSequence value) throws XPathException {
        for (AtomicValue atom : value) {
            startElement("item");
            AtomicType type = atom.getPrimitiveType();
            emitAttribute("type", "#" + type.getName());
            emitAttribute("value", atom.getUnicodeStringValue().toString());
            if (atom instanceof QualifiedNameValue) {
                emitAttribute("namespace", ((QualifiedNameValue) atom).getNamespaceURI().toString());
            }
            endElement();
        }
    }

    public String getTypeLink(SchemaType type) {
        if (type instanceof UserSimpleType || type instanceof UserComplexType) {
            return getId(type, false);
        } else {
            return "#" + type.getName();
        }
    }

}

// Copyright (c) 2007-2023 Saxonica Limited