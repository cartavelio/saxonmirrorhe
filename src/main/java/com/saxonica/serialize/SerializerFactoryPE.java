package com.saxonica.serialize;

import com.saxonica.config.ProfessionalConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.PreparedStylesheet;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.instruct.GlobalParameterSet;
import net.sf.saxon.functions.ResolveURI;
import net.sf.saxon.lib.DirectResourceResolver;
import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.lib.SaxonOutputKeys;
import net.sf.saxon.lib.SerializerFactory;
import net.sf.saxon.s9api.HostLanguage;
import net.sf.saxon.serialize.*;
import net.sf.saxon.style.Compilation;
import net.sf.saxon.trans.*;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Whitespace;

import javax.xml.transform.OutputKeys;
import javax.xml.transform.Result;
import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamResult;
import java.net.URISyntaxException;
import java.util.*;

/**
 * Saxon-PE extensions to the serializer factory
 */
public class SerializerFactoryPE extends SerializerFactory {

    public SerializerFactoryPE(ProfessionalConfiguration config) {
        super(config);
    }

    @Override
    protected SequenceReceiver createSaxonSerializationMethod(
            String method, SerializationProperties serProps,
            PipelineConfiguration pipe,
            CharacterMapExpander characterMapExpander,
            ProxyReceiver normalizer,
            ExpandedStreamResult expandedResult,
            Result result) throws XPathException {

        Properties props = serProps.getProperties();
        pipe.getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION, "custom serialization", -1);
        if (SaxonOutputKeys.SAXON_XQUERY_METHOD.equals(method)) {
            XMLEmitter emitter = new XQueryEmitter();
            emitter.setPipelineConfiguration(pipe);
            if (result instanceof UnicodeWriterResult) {
                emitter.setUnicodeWriter(((UnicodeWriterResult)result).getUnicodeWriter());
            } else {
                emitter.setUnicodeWriter(expandedResult.obtainUnicodeWriter());
            }
            props.setProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
            emitter.setOutputProperties(props);
            return createXMLSerializer(emitter, serProps);

        } else if (SaxonOutputKeys.SAXON_BASE64_BINARY_METHOD.equals(method)) {
            BinaryEmitter emitter = new Base64BinaryEmitter((StreamResult) result, pipe);
            props.setProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
            emitter.setOutputProperties(props);
            return emitter;

        } else if (SaxonOutputKeys.SAXON_HEX_BINARY_METHOD.equals(method)) {
            Emitter emitter = new HexBinaryEmitter((StreamResult) result, pipe);
            emitter.setPipelineConfiguration(pipe);
            props.setProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
            emitter.setOutputProperties(props);
            return emitter;

        } else if (SaxonOutputKeys.SAXON_XML_TO_JSON_METHOD.equals(method)) {
            Emitter emitter = new TEXTEmitter();
            emitter.setPipelineConfiguration(pipe);
            if (result instanceof UnicodeWriterResult) {
                emitter.setUnicodeWriter(((UnicodeWriterResult)result).getUnicodeWriter());
            } else {
                emitter.setUnicodeWriter(expandedResult.obtainUnicodeWriter());
            }
            props.setProperty(OutputKeys.OMIT_XML_DECLARATION, "yes");
            emitter.setOutputProperties(props);
            return emitter;

        } else {
            XPathException err = new XPathException("Unknown serialization method " + method);
            err.setErrorCode(SaxonErrorCode.SXCH0004);
            throw err;
        }
    }

    @Override
    protected ProxyReceiver newHTMLIndenter(Receiver next, Properties outputProperties) {
        HTMLIndenterPE r = new HTMLIndenterPE(next, "html");
        r.setOutputProperties(outputProperties);
        return r;
    }

    @Override
    protected ProxyReceiver newXHTMLIndenter(Receiver next, Properties outputProperties) {
        String method = "xhtml";
        String htmlVersion = outputProperties.getProperty("html-version");
        if (htmlVersion != null && htmlVersion.startsWith("5")) {
            method = "xhtml5";
        }
        HTMLIndenterPE r = new HTMLIndenterPE(next, method);
        r.setOutputProperties(outputProperties);
        return r;
    }

    @Override
    protected ProxyReceiver newXMLIndenter(XMLEmitter next, Properties outputProperties) {
        if ("yes".equals(outputProperties.getProperty(SaxonOutputKeys.CANONICAL))) {
            return new OuterNewlineInserter(next);
        } else {
            XMLIndenterPE r = new XMLIndenterPE(next);
            r.setOutputProperties(outputProperties);
            return r;
        }
    }

    @Override
    protected Emitter newXMLEmitter(Properties properties) {
        XMLEmitter emitter = new XMLEmitter();
        String rep = properties.getProperty(SaxonOutputKeys.CHARACTER_REPRESENTATION);
        rep = Whitespace.trim(rep);
        if ("decimal".equals(rep)) {
            emitter.setCharacterReferenceGenerator(DecimalCharacterReferenceGenerator.THE_INSTANCE);
        }
        return emitter;
    }

    @Override
    protected Emitter newHTMLEmitter(Properties properties) {
        HTMLEmitter emitter;
        if (SaxonOutputKeys.isHtmlVersion5(properties)) {
            emitter = new HTML50Emitter();
        } else {
            emitter = new HTML40Emitter();
        }
        setCharacterRepresention(emitter, properties);
        return emitter;
    }

    /**
     * Create a new AttributeSorter, responsible for sorting of attributes into a specified order.
     * This method exists so that it can be overridden in a subclass.
     *
     * @param next             the next receiver in the pipeline
     * @param outputProperties the serialization parameters
     * @return the newly created attribute sorter.
     */

    @Override
    protected ProxyReceiver newAttributeSorter(Receiver next, Properties outputProperties) {
        AttributeSorter r = new AttributeSorter(next);
        r.setOutputProperties(outputProperties);
        return r;
    }

    /**
     * Create a new NamespaceSorter, responsible for sorting of namespaces into a specified order.
     *
     * @param next             the next receiver in the pipeline
     * @param outputProperties the serialization parameters
     * @return the newly created filter.
     */

    @Override
    protected Receiver newNamespaceSorter(Receiver next, Properties outputProperties) {
        return next; // Namespaces are automatically sorted by the NamespaceMap.iterator()
        //return new NamespaceSorter(next);
    }

    private void setCharacterRepresention(XMLEmitter emitter, Properties properties) {
        String rep = properties.getProperty(SaxonOutputKeys.CHARACTER_REPRESENTATION);
        if (rep != null) {
            String nonASCIIrep;
            String excludedRep;
            int semi = rep.indexOf(';');
            if (semi < 0) {
                nonASCIIrep = Whitespace.trim(rep);
                excludedRep = nonASCIIrep;
            } else {
                nonASCIIrep = Whitespace.trim(rep.substring(0, semi));
                excludedRep = Whitespace.trim(rep.substring(semi + 1));
            }
            if ("native".equals(excludedRep)) {
                excludedRep = "entity";
            }
            if (!"native".equals(nonASCIIrep)) {
                emitter.setEscapeNonAscii(true);
                if ("entity".equals(nonASCIIrep)) {
                    if ("decimal".equals(excludedRep)) {
                        emitter.setCharacterReferenceGenerator(HTMLEntityReferenceGenerator.DECIMAL);
                    } else {
                        emitter.setCharacterReferenceGenerator(HTMLEntityReferenceGenerator.HEX);
                    }
                } else if ("decimal".equals(nonASCIIrep)) {
                    emitter.setCharacterReferenceGenerator(DecimalCharacterReferenceGenerator.THE_INSTANCE);
                } else {
                    emitter.setCharacterReferenceGenerator(HexCharacterReferenceGenerator.THE_INSTANCE);
                }
            }
        }
    }

    /**
     * Create a new XHTML Emitter. This method exists so that it can be overridden in a subclass.
     *
     * @param properties the output properties
     * @return the newly created XHTML emitter.
     */

    @Override
    protected Emitter newXHTMLEmitter(Properties properties) {
        boolean is5 = SaxonOutputKeys.isXhtmlHtmlVersion5(properties);
        XMLEmitter emitter = is5 ? new XHTML5Emitter() : new XHTML1Emitter();
        setCharacterRepresention(emitter, properties);
        return emitter;
    }

    /**
     * Add a filter to the text output method pipeline. This does nothing unless overridden
     * in a superclass
     *
     * @param next       the next receiver (typically the TextEmitter)
     * @param properties the output properties
     * @return the receiver to be used in place of the "next" receiver
     */

    @Override
    public Receiver addTextOutputFilter(Receiver next, Properties properties) throws XPathException {
        boolean recognize = "yes".equals(properties.getProperty(SaxonOutputKeys.RECOGNIZE_BINARY));
        if (recognize) {
            return new BinaryTextDecoder(next, properties);
        } else {
            return next;
        }
    }

    /**
     * Prepare another stylesheet to handle the output of this one.
     * <p>This method is intended for internal use, to support the
     * <code>saxon:next-in-chain</code> extension.</p>
     *
     * @param pipe       the pipeline for the current transformation
     * @param href       URI of the next stylesheet to be applied
     * @param baseURI    base URI for resolving href if it's a relative
     *                   URI
     * @param result     the output destination of the current stylesheet
     * @return a replacement destination for the current stylesheet
     * @throws XPathException if any dynamic error occurs
     */

    @Override
    public SequenceReceiver prepareNextStylesheet(PipelineConfiguration pipe, String href, String baseURI, Result result)
            throws XPathException {

        Configuration config = pipe.getConfiguration();
        config.checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION, "saxon:next-in-chain", -1);
        final PreparedStylesheet thisStylesheet = (PreparedStylesheet) pipe.getController().getExecutable();
        PreparedStylesheet next = thisStylesheet.getCachedStylesheet(href, baseURI);

        if (next == null) {
            Source source = null;
            try {
                ResourceRequest request = new ResourceRequest();
                request.relativeUri = href;
                try {
                    if (baseURI == null) {
                        request.uri = href;
                    } else {
                        request.baseUri = baseURI;
                        request.uri = ResolveURI.makeAbsolute(href, baseURI).toString();
                    }
                } catch (URISyntaxException e) {
                    throw new XPathException("Cannot resolve URI supplied for next-in-chain", e);
                }
                source = request.resolve(config.getResourceResolver(), new DirectResourceResolver(config));
            } catch (TransformerException e) {
                throw XPathException.makeXPathException(e);
            }
            CompilerInfo compilerInfo = new CompilerInfo(config.getDefaultXsltCompilerInfo());
            compilerInfo.setGenerateByteCode(config.isGenerateByteCode(HostLanguage.XSLT));
            compilerInfo.setTargetEdition(config.getEditionCode());
            compilerInfo.setJustInTimeCompilation(config.isJITEnabled());
            next = Compilation.compileSingletonPackage(config, compilerInfo, source);
            thisStylesheet.putCachedStylesheet(href, baseURI, next);
        }

        Receiver destination = next.getConfiguration().getSerializerFactory().getReceiver(
                result, next.getDeclaredSerializationProperties(), pipe);

        XsltController nextController = next.newController();
        TransformerReceiver nextTransformer =
                new TransformerReceiver(nextController);
        nextController.initializeController(new GlobalParameterSet());

        nextTransformer.setSystemId(pipe.getController().getBaseOutputURI());
        nextTransformer.setPipelineConfiguration(new PipelineConfiguration(pipe));
        nextTransformer.setDestination(destination);
        //nextTransformer.open();

        return new SequenceNormalizerWithSpaceSeparator(nextTransformer);
    }

    /**
     * Check that a supplied output property is valid, and normalize the value (specifically in the case of boolean
     * values where yes|true|1 are normalized to "yes", and no|false|0 are normalized to "no").
     *
     * @param key     the name of the property, in Clark format
     * @param value   the value of the property. This may be set to null, in which case no validation takes place.
     *                The value must be in JAXP format, that is, with lexical QNames expanded to Clark names
     * @return normalized value of the property, or null if the supplied value is null
     * @throws XPathException if the property name or value is invalid
     */

    @Override
    public String checkOutputProperty(String key, String value) throws XPathException {
        if (key.startsWith("{http://saxon.sf.net/}")) {
            switch (key) {
                case SaxonOutputKeys.STYLESHEET_VERSION:
                    // allowed in HE
                    break;
                case SaxonOutputKeys.PARAMETER_DOCUMENT_BASE_URI:
                    // allowed in HE
                    break;
                case SaxonOutputKeys.SUPPLY_SOURCE_LOCATOR:
                case SaxonOutputKeys.UNFAILING:
                    // allowed in HE
                    if (value != null) {
                        value = checkYesOrNo(key, value);
                    }
                    break;
                case SaxonOutputKeys.NEWLINE:
                case SaxonOutputKeys.NEXT_IN_CHAIN:
                case SaxonOutputKeys.CHARACTER_REPRESENTATION:
                case SaxonOutputKeys.NEXT_IN_CHAIN_BASE_URI:
                case SaxonOutputKeys.PROPERTY_ORDER:
                case SaxonOutputKeys.INTERNAL_DTD_SUBSET:
                    // no constraints
                    if (value != null) {
                        checkExtensions(key);
                    }
                    break;
                case SaxonOutputKeys.ATTRIBUTE_ORDER:
                    if (value != null) {
                        checkExtensions(key);
                        value= checkListOfEQNamesAllowingStar(key, value);
                    }
                    break;
                case SaxonOutputKeys.CANONICAL:
                case SaxonOutputKeys.REQUIRE_WELL_FORMED:
                case SaxonOutputKeys.RECOGNIZE_BINARY:
                case SaxonOutputKeys.WRAP:
                case SaxonOutputKeys.SINGLE_QUOTES:
                    if (value != null) {
                        checkExtensions(key);
                        value = checkYesOrNo(key, value);
                    }
                    break;

                case SaxonOutputKeys.INDENT_SPACES:
                case SaxonOutputKeys.LINE_LENGTH:
                    if (value != null) {
                        checkExtensions(key);
                        checkNonNegativeInteger(key, value);
                    }
                    break;

                case SaxonOutputKeys.DOUBLE_SPACE:
                    if (value != null) {
                        checkExtensions(key);
                        checkListOfEQNames(key, value);
                    }
                    break;

                default:
                    throw new XPathException("Unknown serialization parameter " + Err.wrap(key), "XQST0109");
            }
        } else {
            value = super.checkOutputProperty(key, value);
        }
        return value;
    }

    @Override
    protected void checkExtensions(String key)  {
        getConfiguration().checkLicensedFeature(Configuration.LicenseFeature.PROFESSIONAL_EDITION, "custom serialization " + key, -1);
    }

    /**
     * Get a Comparator that sorts key values in a map into the order implied by the
     * serialization property saxon:property-order
     * @param propertyOrder the value of the saxon:property-order serialization property
     * @return a Comparator that implements the requested property order
     */

    @Override
    protected Comparator<AtomicValue> getPropertySorter(String propertyOrder) {
        final int MIDDLE_GROUND = 10000000;
        final Map<String, Integer> knownKeys = new HashMap<>();
        int pos = 0;
        StringTokenizer st2 = new StringTokenizer(propertyOrder, " \t\n\r", false);
        while (st2.hasMoreTokens()) {
            String key = st2.nextToken();
            if (key.equals("*")) {
                pos += MIDDLE_GROUND;
            } else {
                knownKeys.put(key, pos++);
            }
        }

        return new PropertyComparator(knownKeys);
    }

    private static class PropertyComparator implements Comparator<AtomicValue> {

        public final static int MIDDLE_GROUND = 10000000;
        private final Map<String, Integer> knownKeys;

        public PropertyComparator(Map<String, Integer> knownKeys) {
            this.knownKeys = knownKeys;
        }
        @Override
        public int compare(AtomicValue n0, AtomicValue n1) {
            int r0 = knownKeys.getOrDefault(n0.getStringValue(), -1);
            int r1 = knownKeys.getOrDefault(n1.getStringValue(), -1);
            if (r0 != -1) {
                if (r1 == -1) {
                    return Integer.compare(r0, MIDDLE_GROUND);
                } else {
                    return Integer.compare(r0, r1);
                }
            } else if (r1 != -1) {
                return Integer.compare(MIDDLE_GROUND, r1);
            } else {
                // both null: sort on name
                return n0.getStringValue().compareTo(n1.getStringValue());
            }
        }
    }


}

// Copyright (c) 2010-2022 Saxonica Limited



