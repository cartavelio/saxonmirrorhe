package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.schema.Facet;
import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.ProxyReceiver;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.StartTagBuffer;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.om.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.BigDecimalValue;

import java.math.BigDecimal;
import java.util.HashSet;
import java.util.Set;
import java.util.StringTokenizer;

/**
 * This is a filter inserted into the input pipeline for processing schema documents, whose
 * task is to examine attributes such as vc:maxVersion and discard those parts of the schema document
 * that are to be ignored because of those attributes.
 * <p>Although this mechanism is defined only for use with XSD 1.1, we support it also with XSD 1.0.
 */

public class VersionControlFilter extends ProxyReceiver {

    private StartTagBuffer startTag;
    private SchemaCompiler compiler;
    private int depthOfHole = 0;
    private boolean emptySchemaElement = false;

    private BigDecimalValue thisVersion;
    private boolean isXSD11;
    private boolean foundErrors;


    /**
     * Create a UseWhenFilter
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
     * Set the start tag buffer
     *
     * @param stb a preceding filter on the pipeline that buffers the attributes of a start tag
     */

    public void setStartTagBuffer(StartTagBuffer stb) {
        this.startTag = stb;
    }

    /**
     * Start of document
     */

    public void open() throws XPathException {
        nextReceiver.open();
    }

    /**
     * Notify the start of an element.
     * @param elemName   the name of the element.
     * @param typeCode   integer code identifying the element's type within the name pool.
     * @param location   the location of the element in the source document
     * @param properties bit-significant properties of the element node
     */

    public void startElement(NodeName elemName, SchemaType typeCode, Location location, int properties) throws XPathException {
        if (emptySchemaElement) {
            depthOfHole++;
            return;
        }
        if (depthOfHole == 0) {
            boolean ignore = false;
            AttributeCollection atts = startTag.getAllAttributes();
            int numberOfAttributes = atts.getLength();
            attLoop:
            for (int a = 0; a < numberOfAttributes; a++) {
                if (atts.getNodeName(a).hasURI(NamespaceConstant.SCHEMA_VERSIONING)) {
                    String localName = atts.getNodeName(a).getLocalPart();
                    try {
                        switch (localName) {
                            case "minVersion": {
                                BigDecimalValue dv = parseDecimalValue(atts.getValue(a));
                                if (thisVersion.compareTo(dv) < 0) {
                                    ignore = true;
                                    break attLoop;
                                }
                                break;
                            }
                            case "maxVersion": {
                                BigDecimalValue dv = parseDecimalValue(atts.getValue(a));
                                if (thisVersion.compareTo(dv) >= 0) {
                                    ignore = true;
                                    break attLoop;
                                }
                                break;
                            }
                            case "typeAvailable":
                                Set<StructuredQName> typesAvailable = parseListOfQNames(atts.getValue(a), startTag);
                                for (StructuredQName name : typesAvailable) {
                                    if (!isAutomaticallyKnownTypeName(name)) {
                                        ignore = true;
                                        break attLoop;
                                    }
                                }
                                break;
                            case "typeUnavailable":
                                Set<StructuredQName> typesUnavailable = parseListOfQNames(atts.getValue(a), startTag);
                                ignore = true;
                                for (StructuredQName name : typesUnavailable) {
                                    if (!isAutomaticallyKnownTypeName(name)) {
                                        ignore = false;
                                        break attLoop;
                                    }
                                }
                                break;
                            case "facetAvailable":
                                Set<StructuredQName> facetsAvailable = parseListOfQNames(atts.getValue(a), startTag);
                                for (StructuredQName name : facetsAvailable) {
                                    if (!isKnownFacetName(name)) {
                                        ignore = true;
                                        break attLoop;
                                    }
                                }
                                break;
                            case "facetUnavailable":
                                Set<StructuredQName> facetsUnavailable = parseListOfQNames(atts.getValue(a), startTag);
                                ignore = true;
                                for (StructuredQName name : facetsUnavailable) {
                                    if (!isKnownFacetName(name)) {
                                        ignore = false;
                                        break attLoop;
                                    }
                                }
                                break;
                            default:
                                compiler.warning("Unrecognized attribute name " + atts.getQName(a), location);
                                break;
                        }
                    } catch (XPathException e) {
                        try {
                            compiler.error("Invalid value for attribute " + atts.getQName(a) +
                                                   ": " + e.getMessage(), location);
                            foundErrors = true;
                        } catch (SchemaException e1) {
                            //
                        }
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

            nextReceiver.startElement(elemName, typeCode, location, properties);
        } else {
            depthOfHole++;
        }
    }

    private static FingerprintedQName XS_SCHEMA =
            new FingerprintedQName("xs", NamespaceConstant.SCHEMA, "schema", StandardNames.XS_SCHEMA & NamePool.FP_MASK);

    /**
     * Notify a namespace. Namespaces are notified <b>after</b> the startElement event, and before
     * any children for the element. The namespaces that are reported are only required
     * to include those that are different from the parent element; however, duplicates may be reported.
     * A namespace must not conflict with any namespaces already used for element or attribute names.
     *
     * @param namespaceBindings the prefix/URI pair
     * @param properties       any additional properties of the namespace event
     * @throws IllegalStateException: attempt to output a namespace when there is no open element
     *                                start tag
     */

    public void namespace(NamespaceBindingSet namespaceBindings, int properties) throws XPathException {
        if (depthOfHole == 0) {
            nextReceiver.namespace(namespaceBindings, properties);
        }
    }

    /**
     * Notify an attribute. Attributes are notified after the startElement event, and before any
     * children. Namespaces and attributes may be intermingled.
     *
     * @param nameCode   The name of the attribute
     * @param typeCode   The type of the attribute
     * @param locationId The location of the node in the source file, or of the instruction that created it
     * @param properties Bit significant value. The following bits are defined:
     *                   <dl>
     *                   <dd>DISABLE_ESCAPING</dd>    <dt>Disable escaping for this attribute</dt>
     *                   <dd>NO_SPECIAL_CHARACTERS</dd>      <dt>Attribute value contains no special characters</dt>
     *                   </dl>
     * @throws IllegalStateException: attempt to output an attribute when there is no open element
     *                                start tag
     */

    public void attribute(NodeName nameCode, SimpleType typeCode, CharSequence value, Location locationId, int properties) throws XPathException {
        if (depthOfHole == 0) {
            nextReceiver.attribute(nameCode, typeCode, value, locationId, properties);
        }
    }

    /**
     * Notify the start of the content, that is, the completion of all attributes and namespaces.
     * Note that the initial receiver of output from XSLT instructions will not receive this event,
     * it has to detect it itself. Note that this event is reported for every element even if it has
     * no attributes, no namespaces, and no content.
     */


    public void startContent() throws XPathException {
        if (depthOfHole == 0) {
            nextReceiver.startContent();
        }
    }

    /**
     * End of element
     */

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

    public void characters(CharSequence chars, Location locationId, int properties) throws XPathException {
        if (depthOfHole == 0) {
            nextReceiver.characters(chars, locationId, properties);
        }
    }

    /**
     * Processing Instruction
     */

    public void processingInstruction(String target, CharSequence data, Location locationId, int properties) {
        // these are ignored in a schema document
    }

    /**
     * Output a comment
     */

    public void comment(CharSequence chars, Location locationId, int properties) throws XPathException {
        // these are ignored in a schema document
    }

    /**
     * Notify the end of a document node
     */

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
        StringTokenizer tokenizer = new StringTokenizer(value, " \t\n\r", false);
        while (tokenizer.hasMoreTokens()) {
            String tok = tokenizer.nextToken();
            StructuredQName val = StructuredQName.fromLexicalQName(tok, true, false, resolver);
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
        return (name.hasURI(NamespaceConstant.SCHEMA) &&
                Facet.isFacetName(localName, isXSD11 ? Configuration.XSD11 : Configuration.XSD10)) ||
                (name.hasURI(NamespaceConstant.SAXON) &&
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

// Copyright (c) 2008 Saxonica Limited