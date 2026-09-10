////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2026 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package net.sf.saxon.functions;

import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.Version;
import net.sf.saxon.event.Builder;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.Sender;
import net.sf.saxon.expr.Callable;
import net.sf.saxon.expr.PackageData;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.style.StylesheetPackage;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.tree.linked.DocumentImpl;
import net.sf.saxon.tree.tiny.TinyDocumentImpl;
import net.sf.saxon.type.Schema;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.type.ValidationException;
import net.sf.saxon.value.*;
import org.xml.sax.*;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayInputStream;
import java.io.InputStream;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.List;

public class ParseXml extends SystemFunction implements Callable {

    public static OptionsParameter makeOptionsParameter(int version) {
        OptionsParameter options = new OptionsParameter(version);
        options.addAllowedOption("base-uri", SequenceType.SINGLE_ANY_URI);
        options.addAllowedOption("dtd-validation", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        options.addAllowedOption("strip-space", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        options.addAllowedOption("trusted", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        options.addAllowedOption("use-xsi-schema-location", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        options.addAllowedOption("xsd-validation", SequenceType.SINGLE_STRING, StringValue.bmp("skip"));
        options.addAllowedOption("xinclude", SequenceType.SINGLE_BOOLEAN, BooleanValue.FALSE);
        return options;
    }


    /**
     * Evaluate the expression
     *
     * @param context   the dynamic evaluation context
     * @param arguments the values of the arguments, supplied as SequenceIterators
     * @return the result of the evaluation, in the form of a SequenceIterator
     * @throws net.sf.saxon.trans.XPathException
     *          if a dynamic error occurs during the evaluation of the expression
     */
    @Override
    public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
        boolean is40 = getRetainedStaticContext().getPackageData().getHostLanguageVersion() >= 40;
        AtomicValue input = (AtomicValue) arguments[0].head();
        if (input == null) {
            return EmptySequence.INSTANCE;
        }

        ProcessedOptions checkedOptions =
                is40 ? getDetails().optionDetails.processSuppliedOptions(arguments, 1, context, 40) : ProcessedOptions.EMPTY_OPTIONS;
        
        ParseOptions parseOptions = context.getConfiguration().getParseOptions();

        PackageData pd = getRetainedStaticContext().getPackageData();
        if (pd instanceof StylesheetPackage) {
            parseOptions = parseOptions.withSpaceStrippingRule(((StylesheetPackage) pd).getSpaceStrippingRule());
        } else {
            parseOptions = parseOptions.withSpaceStrippingRule(IgnorableSpaceStrippingRule.INSTANCE);
        }

        if (checkedOptions.asBoolean("dtd-validation", false)) {
            parseOptions = parseOptions.withDTDValidationMode(Validation.STRICT);
        }

        boolean validating = false;
        if (!checkedOptions.containsKey("xsd-validation")) {
            // In 3.1 we defaulted from the configuration values, in 4.0 it has to be explicit
            parseOptions = parseOptions.withSchemaValidationMode(Validation.SKIP);
        } else {
            String validationValue = Whitespace.normalize(checkedOptions.asString("xsd-validation"));
            validating = !"skip".equals(validationValue);
            if (validationValue.equals("strict")) {
                parseOptions = parseOptions.withSchemaValidationMode(Validation.STRICT);
                parseOptions = parseOptions.withSchema(getRetainedStaticContext().getImportedSchema());
            } else if (validationValue.equals("lax")) {
                parseOptions = parseOptions.withSchemaValidationMode(Validation.LAX);
                parseOptions = parseOptions.withSchema(getRetainedStaticContext().getImportedSchema());
            } else if (validationValue.equals("skip")) {
                parseOptions = parseOptions.withSchemaValidationMode(Validation.SKIP);
            } else if (validationValue.startsWith("type ")) {
                String eqName = validationValue.substring(5).trim();
                if (!eqName.startsWith("Q{")) {
                    throw new XPathException("Type name in xsd-validation option of fn:parse-xml must start with 'Q{'", "FODC0008");
                }
                StructuredQName typeName;
                try {
                    typeName = StructuredQName.fromLexicalQName(eqName, false, StructuredQName.QUPL, null);
                } catch (XPathException e) {
                    throw e.withErrorCode("FODC0008");
                }
                Schema schema = getRetainedStaticContext().getImportedSchema();
                SchemaType type = schema.getSchemaType(typeName);
                if (type == null) {
                    throw new XPathException("Unknown type " + eqName, "FODC0008");
                }
                parseOptions = parseOptions
                        .withSchema(schema)
                        .withSchemaValidationMode(Validation.BY_TYPE)
                        .withTopLevelType(type);
            } else {
                throw new XPathException("Unknown xsd-validation option of fn:parse-xml: " + validationValue, "FODC0008");
            }

            parseOptions = parseOptions.withUseXsiSchemaLocation(checkedOptions.asBoolean("use-xsi-schema-location", false));

            if (is40 && checkedOptions.containsKey("trusted")) {
                boolean trusted = checkedOptions.asBoolean("trusted", false);
                parseOptions = parseOptions.withTrusted(trusted);
                //#if CSHARP==false
                if (!trusted) {
                    parseOptions = parseOptions.withEntityResolver(new EntityResolver() {

                        @Override
                        public InputSource resolveEntity(String publicId, String systemId) throws SAXException {
                            throw new SAXException(
                                    new XPathException("The parse-xml input references '" + systemId +
                                                               "' but external entity expansion is disallowed", "FODC0016"));
                        }
                    });
                }
                //#endif
            }

//            val = checkedOptions.get("entity-expansion-limit").head();
//            if (val != null) {
//                //#if CSHARP==false
//                parseOptions = parseOptions
//                        .withParserFeature(XMLConstants.ACCESS_EXTERNAL_DTD, false)
//                        .withParserFeature("http://xml.org/sax/features/external-general-entities", false)
//                        .withParserFeature("http://xml.org/sax/features/external-parameter-entities", false)
//                        .withParserFeature(XMLConstants.FEATURE_SECURE_PROCESSING, true);
//                //#endif
//            }

            if (checkedOptions.containsKey("strip-space")) {
                parseOptions = parseOptions.withSpaceStrippingRule(
                        checkedOptions.asBoolean("strip-space", false)
                                ? validating
                                    ? IgnorableSpaceStrippingRule.INSTANCE
                                    : AllElementsSpaceStrippingRule.INSTANCE
                                : NoElementsSpaceStrippingRule.INSTANCE);
            }

            if (checkedOptions.containsKey("xinclude")) {
                try {
                    parseOptions = parseOptions.withXIncludeAware(checkedOptions.asBoolean("xinclude", false));
                } catch (Exception e) {
                    // SaxonCS - XInclude not supported
                    throw new XPathException(e);
                }
            }
        }

        String baseUri;
        if (checkedOptions.containsKey("base-uri")) {
            baseUri = checkedOptions.asString("base-uri");
        } else {
            baseUri = getStaticBaseUriString();
        }


        if (usePushParser()) {
            return parseXmlPush(input, parseOptions, baseUri, context);
        } else {
            return parseXmlPull(input, parseOptions, baseUri, context);
        }
    }

    @CSharpReplaceBody(code="return false;")
    private static boolean usePushParser() {
        return true;
    }

    /**
     * Evaluate the function using a push parser (SAX parser, obtained using JAXP)
     * @param inputArg the string content of the XML to be parsed
     * @param context the XPath evaluation context
     * @return the parsed document, as a tree
     * @throws XPathException if parsing fails
     */
    @CSharpReplaceBody(code="return null;")
    private NodeInfo parseXmlPush(
            AtomicValue inputArg,
            ParseOptions options,
            String baseUri,
            XPathContext context) throws XPathException {


        RetentiveErrorHandler errorHandler = new RetentiveErrorHandler();
        try {
            Controller controller = context.getController();
            if (controller == null) {
                throw new XPathException("parse-xml() function is not available in this environment");
            }

            Source source;
            if (inputArg instanceof StringValue) {
                String inputXml = inputArg.getStringValue();
                if (!inputXml.isEmpty() && inputXml.charAt(0) == 0xFEFF) {  // Strip a leading BOM
                    inputXml = inputXml.substring(1);
                }
                StringReader sr = new StringReader(inputXml);
                source = new StreamSource(sr);
                source.setSystemId(baseUri);
            } else if (inputArg instanceof BinaryValue) {
                byte[] binaryXml = ((BinaryValue)inputArg).getBinaryValue();
                InputStream stream = new ByteArrayInputStream(binaryXml);
                source = new StreamSource(stream);
                source.setSystemId(baseUri);
            } else {
                throw new XPathException("Input to parse-xml() must be either a string or a binary value", "XPTY0004");
            }

            Builder b = TreeModel.TINY_TREE.makeBuilder(controller.makePipelineConfiguration());
            Receiver s = b;

            options = options.withErrorHandler(errorHandler);

            s.setPipelineConfiguration(b.getPipelineConfiguration());

            Sender.send(source, s, options);

            if (errorHandler.failed || !errorHandler.errors.isEmpty()) {
                // Typically, a DTD validation failure
                StringBuilder message = new StringBuilder();
                for (SAXParseException err : errorHandler.errors) {
                    message.append(err.getMessage()).append(" ");
                }
                throw new XPathException(message.toString(), "FODC0007");
            }
            TinyDocumentImpl node = (TinyDocumentImpl) b.getCurrentRoot();
            node.setBaseURI(baseUri);
            b.reset();
            return node;
        } catch (XPathException err) {
            String msg = makeParsingErrorMessage(err);
            String code = null;
            if (!err.hasStandardErrorCode() || err.hasErrorCode("SXXP0003")) {
                code = "FODC0006";
            }
            if (err instanceof ValidationException) {
                code = "FODC0007";
            }
            XPathException xe = new XPathException(msg, code);
            errorHandler.captureRetainedErrors(xe);
            xe.maybeSetContext(context);
            throw xe;
        }
    }

    //#if CSHARP==false
    public static class RetentiveErrorHandler implements ErrorHandler {

        public List<SAXParseException> errors = new ArrayList<>();
        public boolean failed = false;

        @Override
        public void error(SAXParseException exception) {
            errors.add(exception);
        }

        @Override
        public void warning(SAXParseException exception) {
            // no action
        }

        @Override
        public void fatalError(SAXParseException exception) {
            errors.add(exception);
            failed = true;
        }

        public void captureRetainedErrors(XPathException xe) {
            List<SAXParseException> retainedErrors = errors;
            if (!retainedErrors.isEmpty()) {
                List<Item> wrappedErrors = new ArrayList<>();
                for (SAXParseException e : retainedErrors) {
                    wrappedErrors.add(new ObjectValue<SAXParseException>(e, SAXParseException.class));
                }
                xe.setErrorObject(SequenceExtent.makeSequenceExtent(wrappedErrors));
            }
        }
    }
    //#endif

    /**
     * Evaluate the function using a pull parser (typically the .NET XmlReader)
     *
     * @param inputArg the string content of the XML to be parsed
     * @param context  the XPath evaluation context
     * @return the parsed document, as a tree
     * @throws XPathException if parsing fails
     */
    private NodeInfo parseXmlPull(
            AtomicValue inputArg,
            ParseOptions options,
            String baseUri,
            XPathContext context) throws XPathException {
        try {
            Controller controller = context.getController();
            if (controller == null) {
                throw new XPathException("parse-xml() function is not available in this environment");
            }
            Configuration config = context.getConfiguration();

            StreamSource ss;
            if (inputArg instanceof StringValue) {
                String inputXml = inputArg.getStringValue();
                if (!inputXml.isEmpty() && inputXml.charAt(0) == 0xFEFF) { // Strip a leading BOM
                    inputXml = inputXml.substring(1);
                }
                StringReader sr = new StringReader(inputXml);
                ss = new StreamSource(sr, baseUri);
            } else if (inputArg instanceof BinaryValue) {
                byte[] binaryXml = ((BinaryValue) inputArg).getBinaryValue();
                InputStream stream = new ByteArrayInputStream(binaryXml);
                ss = new StreamSource(stream, baseUri);
            } else {
                throw new XPathException("Input to parse-xml() must be either a string or a binary value", "XPTY0004");
            }
            Source pullSource = Version.platform.resolveSource(ss, options, config);

            Builder b = TreeModel.TINY_TREE.makeBuilder(controller.makePipelineConfiguration());
            Receiver s = b;
            PackageData pd = getRetainedStaticContext().getPackageData();
            if (pd instanceof StylesheetPackage) {
                if (((StylesheetPackage) pd).isStripsTypeAnnotations()) {
                    s = config.getAnnotationStripper(s);
                }
            }


            s.setPipelineConfiguration(b.getPipelineConfiguration());
            Sender.send(pullSource, s, options);

            NodeInfo root = b.getCurrentRoot();
            if (root instanceof TinyDocumentImpl) {
                TinyDocumentImpl node = (TinyDocumentImpl) root;
                node.setBaseURI(baseUri);
                node.getTreeInfo().setUserData("saxon:document-uri", "");
            } else if (root instanceof DocumentImpl) {
                DocumentImpl node = (DocumentImpl) root;
                node.setBaseURI(baseUri);
                node.getTreeInfo().setUserData("saxon:document-uri", "");
            }
            b.reset();
            return root;
        } catch (XPathException err) {
            String msg = makeParsingErrorMessage(err);
            XPathException xe = new XPathException(msg, "FODC0006");
            xe.maybeSetContext(context);
            throw xe;
        }
    }

    private String makeParsingErrorMessage(XPathException err) {
        String msg = "First argument to parse-xml() is not a well-formed and valid XML document. ";
        msg += err.getMessage();
        Throwable cause = err.getCause();
        if (cause != null) {
            msg += cause.getMessage();
        }
        return msg;
    }

}

// Copyright (c) 2010-2026 Saxonica Limited
