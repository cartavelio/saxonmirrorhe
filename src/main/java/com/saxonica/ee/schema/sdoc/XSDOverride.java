package com.saxonica.ee.schema.sdoc;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.om.AttributeMap;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.StandardNames;

import net.sf.saxon.tree.util.Navigator;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.SimpleType;

import javax.xml.transform.Source;
import java.util.HashSet;

/**
 * An xs:override element in an XML Schema Document
 */
public class XSDOverride extends SchemaElement {

    private SingleNamespaceSchema originalSchema;      // the original schema before overriding of components
    private SchemaDocument originalSchemaDocument;     // the original schema document before overriding
    private SingleNamespaceSchema replacementSchema;     // the external schema after redefinition of components

    public XSDOverride() {
    }

    /**
     * Validate and process the attributes on this element in the schema document
     *
     * @throws net.sf.saxon.type.SchemaException if any errors are found in the attributes
     */

    @Override
    protected void prepareAttributes() throws SchemaException {

        if (getXSDSchema().getSchemaCompiler().getLanguageVersion() == Configuration.XSD10) {
            error("The xs:override element requires XSD 1.1 to be enabled");
        }

        AttributeMap atts = attributes();
        String[] allowed = {"id", "schemaLocation"};
        allowAttributes(atts, allowed);

        //-- @id
        processId();

        //-- @schemaLocation
        String schemaLocation = atts.getValue(NamespaceUri.NULL, "schemaLocation");
        if (schemaLocation == null) {
            missingAttribute("schemaLocation");
            return;
        }

        String documentBase = getBaseURI();
        SchemaCompiler compiler = getXSDSchema().getSchemaCompiler();

        Source source = SchemaReader.getSource(documentBase,
                                               schemaLocation,
                                               compiler,
                                               null);

        //-- Have we already imported this XML Schema file? We can't handle this at present.

        if (source == null || source.getSystemId() == null) {
            throw new SchemaException("The xsl:override/@schemaLocation resolves to a source with no known system ID");
        }

        StringBuilder fsb = new StringBuilder(64);
        fsb.append(source.getSystemId());
        fsb.append("?referrer=");
        fsb.append(documentBase);
        fsb.append(";index=");
        fsb.append(Navigator.getNumberSimple(this, null) + "");
        String key = fsb.toString();
        if (compiler.isBeingRead(key)) {
            replacementSchema = null;
            return;
        }
        //System.err.println("Processing override " + key);
        compiler.setBeingRead(key);
        if (compiler.getExistingSchemaDocument(schemaLocation, getXSDSchema().getTargetNamespace()) != null) {
            error("Saxon can't handle overriding of a schema document that has already been loaded by a different route");
            return;
        }

        try {
            PipelineConfiguration pipe = getSchemaNodeFactory().getPipelineConfiguration();
            SchemaDocument schemaDoc = SchemaReader.buildSchemaDocument(source, compiler, pipe, this);
            originalSchemaDocument = schemaDoc;
            originalSchema = schemaDoc.getXSDSchema().getSchema();
            NamespaceUri originalTargetNS = schemaDoc.getXSDSchema().getTargetNamespace();
            if (!getSchema().getTargetNamespace().equals(originalTargetNS)) {
                if (originalTargetNS.isEmpty()) {
                    // chameleon override
                    originalSchemaDocument = schemaDoc = compiler.doChameleonTransformation(
                            schemaDoc.getDocument().getRootNode(), this);
                    originalSchema = schemaDoc.getXSDSchema().getSchema();
                    try {
                        XSDSchema overridden = schemaDoc.getXSDSchema();
                        overridden.processAllAttributes();
                        overridden.validateSubtree(compiler);
                    } catch (SchemaException err) {
                        compiler.warning("The target of the xs:override is not a valid schema", this);
                    }
//                        try {
//                            System.err.println("After chameleon: ");
//                            System.err.println(QueryResult.serialize(schemaDoc.getXSDSchema()));
//                        } catch (XPathException e) {
//                            //
//                        }
                } else {
                    error("The targetNamespace of the overridden schema document differs from that of the overriding schema document");
                    originalSchema = null;
                }
            }
            SchemaDocument convertedSchemaDocument = compiler.doOverrideTransformation(
                    schemaDoc.getDocument().getRootNode(), this);
            XSDSchema top = convertedSchemaDocument.getXSDSchema();

            top.processAllAttributes();
            top.validateSubtree(compiler);
            replacementSchema = top.getSchema();

        } catch (SchemaException e) {
            Throwable cause = e.getException();
            if (cause instanceof java.io.IOException) {
                warning("Failed to locate overridden schema document " + schemaLocation);
                originalSchema = new SingleNamespaceSchema((EnterpriseConfiguration) getConfiguration(), NamespaceUri.NULL);
                // We will get a hard error if any components are actually redefined
            } else {
                // the error has already been reported
                throw e;
            }
        }
    }


    public SingleNamespaceSchema getReplacementSchema() {
        //return replacementSchema;
        return replacementSchema;
    }

    /*@Nullable*/
    public SingleNamespaceSchema getOriginalSchema() {
        return originalSchema;
    }

    public SchemaDocument getOriginalSchemaDocument() {
        return originalSchemaDocument;
    }


    /**
     * Validate this schema element, checking all document-level constraints
     *
     * @param compiler the schema compiler
     * @throws net.sf.saxon.type.SchemaException if any XML representation constraints are violated
     */

    @Override
    public void validate(SchemaCompiler compiler) throws SchemaException {
        if (compiler.getLanguageVersion() == Configuration.XSD10) {
            error("xs:override requires XML Schema 1.1 to be enabled");
            return;
        }

        if (originalSchema == null) {
            return;     // previous error
        }
        HashSet<String> redefinedTypes = new HashSet<String>();
        HashSet<String> redefinedAttributeGroups = new HashSet<String>();
        HashSet<String> redefinedModelGroups = new HashSet<String>();
        HashSet<String> redefinedElements = new HashSet<String>();
        HashSet<String> redefinedAttributes = new HashSet<String>();
        HashSet<String> redefinedNotations = new HashSet<String>();

        for (NodeInfo child : children()) {
            int fp = child.getFingerprint();
            switch (fp) {
                case StandardNames.XS_ANNOTATION: {
                    break;
                }
                case StandardNames.XS_ATTRIBUTE: {
                    String attributeName = child.getAttributeValue(NamespaceUri.NULL, "name");
                    if (attributeName != null && !redefinedAttributes.add(attributeName)) {
                        error("Attempting to override the same attribute declaration more than once");
                    }
                    break;
                }
                case StandardNames.XS_ELEMENT: {
                    String elementName = child.getAttributeValue(NamespaceUri.NULL, "name");
                    if (elementName != null && !redefinedElements.add(elementName)) {
                        error("Attempting to override the same element declaration more than once");
                    }
                    break;
                }
                case StandardNames.XS_ATTRIBUTE_GROUP: {
                    String attributeGroupName = child.getAttributeValue(NamespaceUri.NULL, "name");
                    if (attributeGroupName != null && !redefinedAttributeGroups.add(attributeGroupName)) {
                        error("Attempting to override the same attribute group more than once");
                    }
                    break;
                }
                case StandardNames.XS_COMPLEX_TYPE:
                case StandardNames.XS_SIMPLE_TYPE: {
                    String typeName = child.getAttributeValue(NamespaceUri.NULL, "name");
                    if (typeName != null && !redefinedTypes.add(typeName)) {
                        error("Attempting to override the same type more than once");
                    }
                    break;
                }
                case StandardNames.XS_GROUP: {
                    String modelGroupName = child.getAttributeValue(NamespaceUri.NULL, "name");
                    if (modelGroupName != null && !redefinedModelGroups.add(modelGroupName)) {
                        error("Attempting to override the same named model group more than once");
                    }
                    break;
                }
                case StandardNames.XS_notation: {
                    String notationName = child.getAttributeValue(NamespaceUri.NULL, "name");
                    if (notationName != null && !redefinedNotations.add(notationName)) {
                        error("Attempting to override the same notation declaration more than once");
                    }
                    break;
                }
                default: {
                    illegalElement(child);
                    break;
                }
            }
        }
//        replacementSchema = new SingleNamespaceSchema(
//                originalSchema.getConfiguration(), originalSchema.getTargetNamespace());
//        originalSchema.copyTo(replacementSchema, compiler);
    }

    /**
     * Process child elements after validation
     *
     * @param compiler used for reporting errors
     */

    @Override
    public void postValidate(SchemaCompiler compiler) throws SchemaException {
        if (originalSchema == null || replacementSchema == null) {
            // previous error
            return;
        }
        for (NodeInfo child : children()) {
            int childfp = child.getFingerprint();

            switch (childfp) {
                case StandardNames.XS_ATTRIBUTE: {
                    AttributeDecl attribute = ((XSDAttribute) child).getAttributeDeclaration();
                    if (attribute == null) {
                        break; // error already reported;
                    }
                    AttributeDecl existing = originalSchema.getAttributeDecl(attribute.getComponentName());
                    if (existing != null) {
                        attribute.setRedefinitionLevel(existing.getRedefinitionLevel() + 1);
                    }
                    break;
                }
                case StandardNames.XS_ATTRIBUTE_GROUP: {
                    AttributeGroupDecl ag = ((XSDAttributeGroup) child).getAttributeGroupDecl();
                    if (ag == null) {
                        // error already reported
                        break;
                    }
                    AttributeGroupDecl existing = originalSchema.getAttributeGroup(ag.getAttributeGroupName());
                    if (existing != null) {
                        ag.setRedefinitionLevel(existing.getRedefinitionLevel() + 1);
                    }
                    break;
                }
                case StandardNames.XS_COMPLEX_TYPE: {
                    UserComplexType typeDef = ((XSDComplexType) child).getComplexType();
                    if (typeDef == null) {
                        // error already reported
                        break;
                    }
                    SchemaType existing = originalSchema.getType(typeDef.getTypeName());
                    if (existing != null) {
                        typeDef.setRedefinitionLevel(existing.getRedefinitionLevel() + 1);
                    }
                    break;
                }

                case StandardNames.XS_SIMPLE_TYPE: {
                    SimpleTypeDefinition typeDef;
                    try {
                        typeDef = ((XSDSimpleType) child).getSimpleTypeDefinition();
                    } catch (SchemaException e) {
                        error(e.getMessage());
                        return;
                    }
                    if (typeDef == null) {
                        // error already reported
                        break;
                    }
                    SimpleType existing = originalSchema.getSimpleType(typeDef.getTypeName());
                    if (existing != null) {
                        typeDef.setRedefinitionLevel(existing.getRedefinitionLevel() + 1);
                    }
                    break;
                }
                case StandardNames.XS_ELEMENT: {
                    ElementDecl element = ((XSDElement) child).getElementDeclaration();
                    if (element == null) {
                        break; // error already reported;
                    }

                    ElementDecl existing = originalSchema.getElementDecl(element.getComponentName());
                    if (existing != null) {
                        element.setRedefinitionLevel(existing.getRedefinitionLevel() + 1);
                    }
                    break;
                }
                case StandardNames.XS_GROUP: {
                    ModelGroupDefinition group = ((XSDGroup) child).getGroupDeclaration();
                    if (group == null) {
                        break; // error already reported;
                    }
                    ModelGroupDefinition existing = originalSchema.getGroup(group.getModelGroupName());
                    if (existing != null) {
                        group.setRedefinitionLevel(existing.getRedefinitionLevel() + 1);
                    }
                    break;
                }
                case StandardNames.XS_notation: {
                    Notation n = ((XSDNotation) child).getNotation();
                    if (n == null) {
                        break; // error already reported
                    }
                    Notation existing = originalSchema.getNotation(n.getNotationName());
                    if (existing != null) {
                        n.setRedefinitionLevel(existing.getRedefinitionLevel() + 1);
                    }
                    break;
                }


            }
        }
        if (replacementSchema != null) {
            replacementSchema.copyTo(getSchema(), compiler);
        }

    }


}

// Copyright (c) 2009-2022 Saxonica Limited