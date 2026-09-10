package com.saxonica.ee.validate;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.schema.PreparedSchema;
import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.Location;
import net.sf.saxon.str.StringView;
import net.sf.saxon.trans.Err;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.*;
import net.sf.saxon.value.BooleanValue;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.*;

/**
 * This class processes attributes such as xsi:schemaLocation and xsi:type.
 * To achieve this, it needs to be preceded in the pipeline by a StartTagBuffer, which
 * buffers all attributes and namespaces, because the xsi:type and schemaLocation
 * need to be processed before the start element event. Apart from the
 * StartTagBuffer, this is the first class in the validation pipeline.
 * <p>The main work of this class can be done in the startElement event, because
 * the StartTagBuffer ensures that by this time, all the namespaces and attributes
 * for the element are known and accessible.</p>
 */

public class XSIAttributeHandler extends ValidatingFilter {

    private final HashSet<NamespaceUri> namespacesEncountered;
    private boolean useXsiSchemaLocation;
    private final List<ValidationFailure> xsiValidationErrors = new ArrayList<>(4);
    private final static String NULL_NAMESPACE_MARKER = ((char)31 + "");
            // An arbitrary string that can't appear as a namespace name

    /**
     * Create a handler for xsi: attributes
     *
     * @param out the next filter in the pipeline
     */

    public XSIAttributeHandler(ValidationStack out) {
        super(out);
        namespacesEncountered = new HashSet<>(10);
        useXsiSchemaLocation = getPipelineConfiguration().getParseOptions().isUseXsiSchemaLocation();
    }


    @Override
    public void setPipelineConfiguration(/*@NotNull*/ PipelineConfiguration pipe) {
        super.setPipelineConfiguration(pipe);
        useXsiSchemaLocation = pipe.getParseOptions().isUseXsiSchemaLocation();
    }

    /**
     * Notify the start of an element
     */

    @Override
    public void startElement(NodeName elemName, SchemaType type,
                             AttributeMap attributes, NamespaceMap namespaces,
                             Location location, int properties)
            throws XPathException {
        boolean nilled = false;
        xsiValidationErrors.clear();

        SchemaType xsiType = null;

        boolean hasXsiAttributes = false;
        for (AttributeInfo att : attributes) {
            if (att.getNodeName().hasURI(NamespaceUri.SCHEMA_INSTANCE)) {
                hasXsiAttributes = true;
                break;
            }
        }

        AttributeMap attMap2 = attributes;

        if (hasXsiAttributes) {
            String xsiTypeValue = null;
            List<AttributeInfo> list = new ArrayList<>(attributes.size());

            // Process schemaLocation attributes first: see bug 6148.
            StringBuilder fullSchemaLocation = new StringBuilder();
            for (AttributeInfo att : attributes) {
                NodeName attName = att.getNodeName();
                SimpleType attType = att.getType();
                // Check for special xsi attributes and annotate them with a type code
                if (attName.hasURI(NamespaceUri.SCHEMA_INSTANCE)) {
                    String value = att.getValue();
                    int fp = attName.obtainFingerprint(getNamePool());
                    if (fp == StandardNames.XSI_NO_NAMESPACE_SCHEMA_LOCATION) {
                        attType = BuiltInAtomicType.ANY_URI;
                        if (useXsiSchemaLocation) {
                            fullSchemaLocation.append(" " + NULL_NAMESPACE_MARKER + " ")
                                    .append(value)
                                    .append(" ");
                        }
                    } else if (fp == StandardNames.XSI_SCHEMA_LOCATION) {
                        attType = BuiltInListType.ANY_URIS;
                        if (useXsiSchemaLocation) {
                            fullSchemaLocation.append(value);
                        }
                    } else if (fp == StandardNames.XSI_TYPE) {
                        attType = BuiltInAtomicType.QNAME;
                        xsiTypeValue = value;            // Bug 6148
                    } else if (fp == StandardNames.XSI_NIL) {
                        attType = BuiltInAtomicType.BOOLEAN;
                    }
                    list.add(new AttributeInfo(att.getNodeName(), attType, att.getValue(),
                                               att.getLocation(), att.getProperties()));
                } else {
                    list.add(att);
                }
            }

            if (xsiTypeValue != null) {
                xsiType = processXsiType(xsiTypeValue, namespaces);
            }
            if (!fullSchemaLocation.isEmpty()) {
                processXsiSchemaLocation(fullSchemaLocation, location);
            }
            for (AttributeInfo att : attributes) {
                NodeName attName = att.getNodeName();
                if (attName.hasURI(NamespaceUri.SCHEMA_INSTANCE)) {
                    String value = att.getValue();
                    int fp = attName.obtainFingerprint(getNamePool());
                    if (fp == StandardNames.XSI_TYPE) {
                        xsiType = processXsiType(value, namespaces);
                    } else if (fp == StandardNames.XSI_NIL) {
                        nilled = processXsiNil(value);
                    }
                }
            }


            attMap2 = SequenceTool.attributeMapFromList(list);
        }

        // Tell the rest of the validation pipeline where to find a namespace resolver
        if (nextReceiver instanceof ValidationStack) {
            ((ValidationStack) nextReceiver).setValidationContext(getValidationContext());
            ((ValidationStack) nextReceiver).setIsNilled(nilled);
            ((ValidationStack) nextReceiver).setXsiValidationErrors(xsiValidationErrors);
        }

        // Keep track of all namespaces encountered
        namespacesEncountered.add(elemName.getNamespaceUri());
        for (AttributeInfo att : attributes) {
            NamespaceUri uri = att.getNodeName().getNamespaceUri();
            namespacesEncountered.add(uri);
        }

        getValidationContext().setXSIType(xsiType);

        nextReceiver.startElement(elemName, type, attMap2, namespaces, location, properties);

    }

    private void processXsiSchemaLocation(String value, Location locationId) throws XPathException {
        // value is a list of pairs of (target namespace, href)
        // In the constructed value, `~` represents the null namespace.
        StringTokenizer tokenizer = new StringTokenizer(value, " \t\n\r", false);
        NamespaceUri targetNS = null;
        List<Source> locationHints = new ArrayList<>();
        while (tokenizer.hasMoreTokens()) {
            String tok = tokenizer.nextToken();
            if (targetNS == null) {
                // odd-numbered tokens
                targetNS = tok.equals(NULL_NAMESPACE_MARKER) ? NamespaceUri.NULL : NamespaceUri.of(tok);
                // Note, there is no check that the schema that is loaded actually uses this
                // target namespace. This appears to be consistent with the spec.

            } else {
                // even-numbered tokens

                boolean ignore = false;
                URI absoluteLocation = null;
                try {
                    absoluteLocation = ResolveURI.makeAbsolute(tok, locationId.getSystemId());
                } catch (URISyntaxException err) {
                    // ignore for now
                    ignore = true;
                }

                if (namespacesEncountered.contains(targetNS)) {
                    List<String> locationsForNamespace =
                            getValidationContext().getSchema().getTargetNamespaceMap().get(targetNS);
                    boolean locationIsNew = absoluteLocation != null && !locationsForNamespace.contains(absoluteLocation.toString());
                    if (locationIsNew) {
                        // In XSD 1.1, ignore this schema document. In XSD 1.0, report an error.
                        // This is not a validity error, so it seems reasonable to report it even when
                        // doing skip validation. But see bug 9158 which suggests changing this.
                        // See also tests addB156 and addB158, and bug 4078 against those tests
                        if (getConfiguration().getXsdVersion() == Configuration.XSD11) {
                            ignore = true;
                        } else {
                            String message;
                            if (targetNS.equals(NamespaceUri.NULL)) {
                                message = "xsi:noNamespaceSchemaLocation cannot appear after the " +
                                        "first no-namespace element or attribute";
                            } else {
                                message = "xsi:schemaLocation namespace " +
                                        Err.wrap(targetNS.toUnicodeString()) + " has already appeared earlier in the instance document";
                            }
                            ValidationFailure ve = new ValidationFailure(message);
                            ve.setConstraintReference(1, "schema-loc", "4");
                            reportValidationError(ve, false, locationId);
                        }
                    } else {
                        ignore = true;
                    }
                }

                if (!ignore) {

                    // First see if we already have a loaded schema for this namespace. If so, assume it's the same one.

                    int status = getValidationContext().getSchema().getNamespaceStatus(targetNS);

                    // There are three possibilities. Either this schema is previously
                    // unknown, and we carry on to load it. Or it was a normal PreparedSchema, in which case we can
                    // make a quick return. Or it was a SurrogateSchema, which shouldn't really
                    // happen, unless (a) the schema is currently being loaded in another thread, or (b) someone
                    // started to load the schema, didn't finish, and didn't tidy up. In this case, we just
                    // pretend it wasn't there, and reload it from scratch.

                    if (status != PreparedSchema.NAMESPACE_UNKNOWN && status != PreparedSchema.NAMESPACE_UNDER_CONSTRUCTION) {
                        targetNS = null;
                        continue;
                    }
                    if (!getValidationContext().getSchema().includesSchemaLocation(absoluteLocation.toString())) {
                        // We've already checked the namespace, but it's possible that the schema location
                        // has been loaded without registering the same namespace (IBM test EDCWildcard/s3_8_6v01)
                        locationHints.add(new StreamSource(absoluteLocation.toString()));
                    }
                }
                targetNS = null;
            }

        }
        if (targetNS != null) {
            String message = "xsi:schemaLocation must contain an even number of URIs";
            // NOTE: this rule is implicit in the spec, but it doesn't way what to do on an error
            ValidationFailure ve = new ValidationFailure(message);
            reportValidationError(ve, false, locationId);
        }
        if (!locationHints.isEmpty()) {
            SchemaCompiler compiler = new SchemaCompiler((EnterpriseConfiguration) getConfiguration());
            PreparedSchema instanceSchema = compiler.loadSources(locationHints);
            getValidationContext().getSchema().copyTo(instanceSchema, compiler);
            compiler.recompile();
            getValidationContext().setSchema(instanceSchema);
        }
        // do a second pass to seal the namespaces
        tokenizer = new StringTokenizer(value, " \t\n\r", false);
        targetNS = null;
        while (tokenizer.hasMoreTokens()) {
            String tok = tokenizer.nextToken();
            if (targetNS == null) {
                targetNS = NamespaceUri.of(tok);
            } else {
                getConfiguration().sealNamespace(targetNS);
                targetNS = null;
            }
        }
    }

    /**
     * Process an xsi:nil attribute. Report a validation error if it is invalid
     *
     * @param value the value of the attribute
     * @return true or false depending on the value of the attribute
     */

    private boolean processXsiNil(String value) {
        // check that it is a valid boolean
        ConversionResult v = StringConverter.StringToBoolean.INSTANCE.convertString(StringView.of(value).tidy());
        if (v instanceof ValidationFailure) {
            String message = "The value of xsi:nil must be a boolean";
            ValidationFailure ve = new ValidationFailure(message);
            // No explicit constraint reference found in the spec
            xsiValidationErrors.add(ve);
        }
        return v == BooleanValue.TRUE;
    }

    /**
     * Process an xsi:type attribute. Report a validation failure if it is invalid; otherwise
     * set the type as a property of the ValidationStack.
     *
     * @param value the value of the attribute
     * @return the SchemaType corresponding to the xsi:type attribute, or null if it is invalid
     */

    private SchemaType processXsiType(String value, NamespaceResolver nsResolver) {
        try {
            StructuredQName qName = StructuredQName.fromLexicalQName((value), true, false, nsResolver);
            SchemaType type = getValidationContext().getSchema().getSchemaType(qName);

            if (type == null) {
                String message = "Unknown type " + Err.wrap(value) + " specified in xsi:type attribute";
                ValidationFailure ve = new ValidationFailure(message);
                ve.setConstraintReference(1, "cvc-elt", "4.2");
                //reportValidationError(ve, false, locationId);
                // errors are not reported immediately because we might be doing skip validation.
                // See test case Extra010, bug 1983849
                xsiValidationErrors.add(ve);
            } else if (type instanceof ComplexType && ((ComplexType) type).isAbstract()) {
                String message = "Type " + Err.wrap(value) + " specified in xsi:type attribute is abstract";
                ValidationFailure ve = new ValidationFailure(message);
                ve.setConstraintReference(1, "cvc-type", "2");
                //reportValidationError(ve, false, locationId);
                xsiValidationErrors.add(ve);
            } else {
                return type;
            }
        } catch (XPathException de) {
            String message = "Invalid QName " + Err.wrap(value) + " specified in xsi:type attribute: " +
                    de.getMessage();
            ValidationFailure ve = new ValidationFailure(message);
            ve.setConstraintReference(1, "cvc-elt", "4.1");
            xsiValidationErrors.add(ve);
        }
        return null;
    }

    @Override
    public void endDocument() throws XPathException {
        super.endDocument();
        reportIfInvalid();
    }

}

// Copyright (c) 2004-2023 Saxonica Limited
