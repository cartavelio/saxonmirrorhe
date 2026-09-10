package com.saxonica.ee.schema;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.sdoc.SimpleTypeDefinition;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.CheckSumFilter;
import net.sf.saxon.event.NamespaceReducer;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.parser.ExplicitLocation;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.DateTimeValue;
import net.sf.saxon.value.QualifiedNameValue;

import java.util.*;


/**
 * This class provides services enabling a compiled schema to be serialized to disk
 */

public class SchemaModelSerializer {

    private EnterpriseConfiguration config;
    private Receiver receiver;
    private LinkedList<SchemaComponent> pendingComponents = new LinkedList<>();
    private IdentityHashMap<SchemaComponent, String> componentToIdMap = new IdentityHashMap<>(200);
    private IdentityHashMap<SerializableSchemaComponent, String> serializedComponents = new IdentityHashMap<>(200);
    private int nextComponent = 0;

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
    }

    /**
     * Export the pre-compiled schema components in this configuration to the specified Receiver
     *
     * @throws XPathException if any failure occurs
     */

    public void serialize() throws XPathException {
        receiver.open();
        receiver.startDocument(0);
        startElement("schema");
        emitAttribute("generatedAt", DateTimeValue.getCurrentDateTime(null).getStringValue());
        emitAttribute("xsdVersion", config.getConfigurationProperty(Feature.XSD_VERSION));
        String dmk = config.getLicenseFeature("DMK");
        if (dmk != null) {
            emitAttribute("dmk", dmk);
        }
        PreparedSchema schema = config.getSuperSchema();
        // System.err.println("Serializing schema " + schema);
        Iterator iter = schema.iterateTypes();
        while (iter.hasNext()) {
            SchemaType type = (SchemaType) iter.next();
            if (type instanceof SimpleTypeDefinition) {
                type = ((SimpleTypeDefinition)type).getSimpleType();
            }
            if (type instanceof SerializableSchemaComponent) {
                serializeComponent((SerializableSchemaComponent) type);
            }
        }

        iter = schema.iterateElementDeclarations();
        while (iter.hasNext()) {
            ElementDecl decl = (ElementDecl) iter.next();
            serializeComponent(decl);
        }

        iter = schema.iterateAttributeDeclarations();
        while (iter.hasNext()) {
            AttributeDecl decl = (AttributeDecl) iter.next();
            serializeComponent(decl);
        }

        iter = schema.iterateModelGroups();
        while (iter.hasNext()) {
            ModelGroupDefinition decl = (ModelGroupDefinition) iter.next();
            serializeComponent(decl);
        }

        iter = schema.iterateAttributeGroups();
        while (iter.hasNext()) {
            AttributeGroupDecl decl = (AttributeGroupDecl) iter.next();
            serializeComponent(decl);
        }

        iter = schema.iterateNotations();
        while (iter.hasNext()) {
            Notation decl = (Notation) iter.next();
            serializeComponent(decl);
        }

        while (!pendingComponents.isEmpty()) {
            SerializableSchemaComponent component = (SerializableSchemaComponent) pendingComponents.getFirst();
            serializeComponent(component);
            pendingComponents.removeFirst();
        }

        endElement();
        if (receiver instanceof CheckSumFilter) {
            int c = ((CheckSumFilter) receiver).getChecksum();
            receiver.processingInstruction(CheckSumFilter.SIGMA, Integer.toHexString(c), ExplicitLocation.UNKNOWN_LOCATION, 0);
        }
        receiver.endDocument();
        receiver.close();

    }

    public void startElement(String elementName) throws XPathException {
        if (inStartTag) {
            receiver.startContent();
        }
        inStartTag = true;
        NodeName nc = new FingerprintedQName("scm", NamespaceConstant.SCM, elementName);
        receiver.startElement(nc, Untyped.getInstance(), ExplicitLocation.UNKNOWN_LOCATION, 0);
    }

    public void endElement() throws XPathException {
        if (inStartTag) {
            receiver.startContent();
            inStartTag = false;
        }
        receiver.endElement();
    }

    public void emitNamespaceContext(NamespaceResolver resolver) throws XPathException {
        for (Iterator iter = resolver.iteratePrefixes(); iter.hasNext(); ) {
            String prefix = (String) iter.next();
            String uri = resolver.getURIForPrefix(prefix, false);
            declareNamespace(prefix, uri);
        }
    }

    public void declareNamespace(String prefix, String uri) throws XPathException {
        receiver.namespace(new NamespaceBinding(prefix, uri), 0);
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
        receiver.attribute(nc, BuiltInAtomicType.UNTYPED_ATOMIC, value, ExplicitLocation.UNKNOWN_LOCATION, 0);
    }

    private static FingerprintedQName XML_BASE =
            new FingerprintedQName("xml", NamespaceConstant.XML, "base", StandardNames.XML_BASE);

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
        value.iterate().forEachOrFail(atom -> {
            startElement("item");
            AtomicType type = atom.getPrimitiveType();
            emitAttribute("type", "#" + type.getName());
            emitAttribute("value", atom.getStringValue());
            if (atom instanceof QualifiedNameValue) {
                emitAttribute("namespace", ((QualifiedNameValue) atom).getNamespaceURI());
            }
            endElement();
        });
    }

    public String getTypeLink(SchemaType type) {
        if (type instanceof UserSimpleType || type instanceof UserComplexType) {
            return getId(type, false);
        } else {
            return "#" + type.getName();
        }
    }

}

// Copyright (c) 2007-2018 Saxonica Limited.