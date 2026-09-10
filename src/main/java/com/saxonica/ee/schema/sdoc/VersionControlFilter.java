package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.Facet;
import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringView;
import net.sf.saxon.str.UnicodeString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInType;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.BigDecimalValue;
import net.sf.saxon.value.StringValue;
import net.sf.saxon.value.Whitespace;

import java.math.BigDecimal;
import java.util.HashSet;
import java.util.Set;

/**
 * This is a filter inserted into the input pipeline for processing schema documents, whose
 * task is to examine attributes such as vc:maxVersion and discard those parts of the schema document
 * that are to be ignored because of those attributes.
 * <p>Although this mechanism is defined only for use with XSD 1.1, we support it also with XSD 1.0.
 */

public class VersionControlFilter extends ProxyReceiver {

    private final SchemaCompiler compiler;
    private int depthOfHole = 0;
    private boolean emptySchemaElement = false;

    private final BigDecimalValue thisVersion;
    private final boolean isXSD11;
    private boolean foundErrors;


    /**
     * Create a VersionControlFilter
     *
     * @param compiler the schema compiler
     * @param next     the next receiver in the pipeline
     */

    public VersionControlFilter(SchemaCompiler compiler, Receiver next) {
        super(next);
        this.compiler = compiler;
        int xsdVersion = compiler.getLanguageVersion();
        thisVersion = new BigDecimalValue(new BigDecimal(xsdVersion == Configuration.XSD10 ? "1.0" : "1.1"));
        isXSD11 = xsdVersion == Configuration.XSD11;
    }

    /**
     * Start of document
     */

    @Override
    public void open() throws XPathException {
        nextReceiver.open();
    }

    /**
     * Notify the start of an element.
     * @param elemName   the name of the element.
     * @param type   integer code identifying the element's type within the name pool.
     * @param attributes the attributes of the element
     * @param namespaces the in-scope namespaces of the element
     * @param location   the location of the element in the source document
     * @param properties bit-significant properties of the element node
     */

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties) throws XPathException {
        if (emptySchemaElement) {
            depthOfHole++;
            return;
        }
        if (depthOfHole == 0) {
            boolean ignore = false;
            boolean doneAttributes = false;
            for (AttributeInfo att : attributes) {
                final NodeName attName = att.getNodeName();
                if (attName.hasURI(NamespaceUri.SCHEMA_VERSIONING)) {
                    String localName = attName.getLocalPart();
                    try {
                        switch (localName) {
                            case "minVersion": {
                                BigDecimalValue dv = parseDecimalValue(att.getValue());
                                if (thisVersion.compareTo(dv) < 0) {
                                    ignore = true;
                                    doneAttributes = true;
                                }
                                break;
                            }
                            case "maxVersion": {
                                BigDecimalValue dv = parseDecimalValue(att.getValue());
                                if (thisVersion.compareTo(dv) >= 0) {
                                    ignore = true;
                                    doneAttributes = true;
                                }
                                break;
                            }
                            case "typeAvailable":
                                Set<StructuredQName> typesAvailable = parseListOfQNames(att.getValue(), namespaces);
                                for (StructuredQName name : typesAvailable) {
                                    if (!isAutomaticallyKnownTypeName(name)) {
                                        ignore = true;
                                        doneAttributes = true;
                                        break;
                                    }
                                }
                                break;
                            case "typeUnavailable":
                                Set<StructuredQName> typesUnavailable = parseListOfQNames(att.getValue(), namespaces);
                                ignore = true;
                                for (StructuredQName name : typesUnavailable) {
                                    if (!isAutomaticallyKnownTypeName(name)) {
                                        ignore = false;
                                        doneAttributes = true;
                                        break;
                                    }
                                }
                                break;
                            case "facetAvailable":
                                Set<StructuredQName> facetsAvailable = parseListOfQNames(att.getValue(), namespaces);
                                for (StructuredQName name : facetsAvailable) {
                                    if (!isKnownFacetName(name)) {
                                        ignore = true;
                                        doneAttributes = true;
                                        break;
                                    }
                                }
                                break;
                            case "facetUnavailable":
                                Set<StructuredQName> facetsUnavailable = parseListOfQNames(att.getValue(), namespaces);
                                ignore = true;
                                for (StructuredQName name : facetsUnavailable) {
                                    if (!isKnownFacetName(name)) {
                                        ignore = false;
                                        doneAttributes = true;
                                        break;
                                    }
                                }
                                break;
                            default:
                                compiler.warning("Unrecognized attribute name " + attName, location);
                                break;
                        }
                        if (doneAttributes) {
                            break;
                        }
                    } catch (XPathException e) {
                        compiler.error("Invalid value for attribute " + attName +
                                               ": " + e.getMessage(), location);
                        foundErrors = true;
                    }
                }
            }

            if (ignore) {
                if (elemName.equals(XS_SCHEMA)) {
                    emptySchemaElement = true;
                } else {
                    depthOfHole = 1;
                    return;
                }
            }

            nextReceiver.startElement(elemName, type, attributes, namespaces, location, properties);
        } else {
            depthOfHole++;
        }
    }

    private static final FingerprintedQName XS_SCHEMA =
            new FingerprintedQName("xs", NamespaceUri.SCHEMA, "schema", StandardNames.XS_SCHEMA & NamePool.FP_MASK);


    /**
     * End of element
     */

    @Override
    public void endElement() throws XPathException {
        if (depthOfHole > 0) {
            depthOfHole--;
        } else {
            nextReceiver.endElement();
        }
    }

    /**
     * Character data
     */

    @Override
    public void characters(UnicodeString chars, Location locationId, int properties) throws XPathException {
        if (depthOfHole == 0) {
            nextReceiver.characters(chars, locationId, properties);
        }
    }

    /**
     * Processing Instruction
     */

    @Override
    public void processingInstruction(String target, UnicodeString data, Location locationId, int properties) {
        // these are ignored in a schema document
    }

    /**
     * Output a comment
     */

    @Override
    public void comment(UnicodeString chars, Location locationId, int properties) throws XPathException {
        // these are ignored in a schema document
    }

    /**
     * Notify the end of a document node
     */

    @Override
    public void endDocument() throws XPathException {
        super.endDocument();
        if (foundErrors) {
            throw new XPathException("One or more errors were found in the content of schema versioning attributes");
        }
    }

    /**
     * Parse an attribute whose type is xs:decimal
     *
     * @param value the value to be parsed
     * @return the parsed value of the attribute
     * @throws ValidationException if the value is not a valid decimal
     */

    private BigDecimalValue parseDecimalValue(String value) throws ValidationException {
        return (BigDecimalValue)BigDecimalValue.makeDecimalValue(value, true).asAtomic();
    }

    /**
     * Parse an attribute whose type is a list of QNames
     *
     * @param value    the value of the attribute
     * @param resolver the namespace resolver for resolving namespace prefixes
     * @return the value of the attribute, as a set of QNames
     * @throws XPathException if a token cannot be resolved as a valid QName
     */

    private Set<StructuredQName> parseListOfQNames(String value, NamespaceResolver resolver)
            throws XPathException {
        Set<StructuredQName> result = new HashSet<>(4);
        Whitespace.Tokenizer tokenizer = new Whitespace.Tokenizer(StringView.tidy(value));
        StringValue tok;
        while ((tok = tokenizer.next()) != null) {
            StructuredQName val = StructuredQName.fromLexicalQName(tok.getStringValue(), true, false, resolver);
            result.add(val);
        }
        return result;
    }

    /**
     * Determine whether a given QName is the name of a known facet
     *
     * @param name the name to be tested
     * @return true if the name is a known facet name
     */

    private boolean isKnownFacetName(StructuredQName name) {
        String localName = name.getLocalPart();
        return (name.hasURI(NamespaceUri.SCHEMA) &&
                Facet.isFacetName(localName, isXSD11 ? Configuration.XSD11 : Configuration.XSD10)) ||
                (name.hasURI(NamespaceUri.SAXON) &&
                        localName.equals("preprocess"));
    }

    /**
     * Determine whether a given QName is the name of an automatically known type
     *
     * @param name the name of a type
     * @return true if the name is a built-in or predefined type name
     */

    private boolean isAutomaticallyKnownTypeName(StructuredQName name) {
        int fp = StandardNames.getFingerprint(name.getURI(), name.getLocalPart());
        return fp != -1 && BuiltInType.getSchemaType(fp) != null &&
                (isXSD11 || fp != StandardNames.XS_ERROR);
    }


}

// Copyright (c) 2008-2022 Saxonica Limited