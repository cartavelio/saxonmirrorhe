package com.saxonica.ee.schema.sdoc;

import com.saxonica.ee.config.StandardSchemaResolver;
import com.saxonica.ee.jaxp.SchemaFactoryImpl;
import com.saxonica.ee.schema.SchemaCompiler;
import net.sf.saxon.Configuration;
import net.sf.saxon.Version;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.SchemaURIResolver;
import net.sf.saxon.lib.Validation;
import net.sf.saxon.om.*;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.trans.XmlProcessingException;
import net.sf.saxon.transpile.CSharp;
import net.sf.saxon.transpile.CSharpReplaceBody;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.linked.LinkedTreeBuilder;
import net.sf.saxon.tree.tiny.TinyBuilder;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.Type;
import org.xml.sax.SAXParseException;
import org.xml.sax.XMLReader;

import javax.xml.transform.Source;
import javax.xml.transform.dom.DOMSource;

/**
 * A class for reading XML Schema documents.
 * The class is not instantiated, its methods are static.
 */

public abstract class SchemaReader {

    private SchemaReader() {
    }

    /**
     * Resolve and dereference a URI to get a Source object representing a schema document
     *
     * @param baseURI         the base URI
     * @param relativeURI     the URI to be resolved relative to the base URI
     * @param compiler            the pipeline configuration (which may define a URIResolver to be used)
     * @param targetNamespaceUri the targetNamespace of the schema document required (this is set to
     *                        null when doing an include or redefine, because in those cases the targetNamespace cannot
     *                        be used to locate the schema document)
     * @return the JAXP Source object representing the deferenced URI, or null if no source could be located
     * @throws SchemaException if any errors occur
     */

    /*@Nullable*/
    public static Source getSource(String baseURI,
                                   String relativeURI,
                                   SchemaCompiler compiler,
                                   NamespaceUri targetNamespaceUri) throws SchemaException {

        try {
            String[] locations = {relativeURI};
            Source[] sources = null;
            SchemaURIResolver r = compiler.getSchemaURIResolver();
            String targetNamespace = targetNamespaceUri == null ? null : targetNamespaceUri.toString();
            if (r != null) {
                sources = r.resolve(targetNamespace, baseURI, locations);
            }
            if (sources == null || (sources.length == 1 && sources[0] == null)) {
                r = new StandardSchemaResolver(compiler.getConfiguration());
                sources = r.resolve(targetNamespace, baseURI, locations);
            }
            if (sources.length == 0 || sources[0] == null) {
                // No action required
                return null;
            } else if (sources.length == 1) {
                return sources[0];
            } else {
                throw new UnsupportedOperationException(
                        "SchemaURIResolver is currently restricted to return one source for each schemaLocation");
            }
        } catch (XPathException e) {
            throw new SchemaException(e);
        }
    }

    /**
     * Resolve and dereference a set of URIs to get a set of Source objects representing schema documents
     *
     * @param baseURI         the base URI
     * @param relativeURIs    the URIs (location hints) to be resolved relative to the base URI
     * @param compiler            the pipeline configuration (which may define a URIResolver to be used)
     * @param targetNamespace the targetNamespace of the schema document required (this is set to
     *                        null when doing an include or redefine, because in those cases the targetNamespace cannot
     *                        be used to locate the schema document)
     * @return the JAXP Source object representing the deferenced URI
     * @throws SchemaException if any errors occur
     */

    public static Source[] getSources(String baseURI,
                                      String[] relativeURIs,
                                      SchemaCompiler compiler,
                                      NamespaceUri targetNamespace) throws SchemaException {

        try {
            Source[] sources = compiler.getSchemaURIResolver().resolve(targetNamespace.toString(), baseURI, relativeURIs);
            if (sources == null) {
                SchemaURIResolver r = new StandardSchemaResolver(compiler.getConfiguration());
                sources = r.resolve(targetNamespace.toString(), baseURI, relativeURIs);
            }
            return sources;
        } catch (XPathException e) {
            throw new SchemaException(e);
        }
    }

    /**
     * Read the Schema from a source XML document and return the Schema
     * document. The schema document is validated against the XML Representation Constraints,
     * but the resulting schema components are not validated against schema component constraints:
     * this must be done by the caller after all schema components are assembled, by calling
     * {@link SchemaCompiler#validate}
     *
     * @param source   the JAXP Source containing the schema document. This may be either a source
     *                 XSD schema document, or a compiled SCM schema document.
     * @param compiler used for tracking source documents read, and for error reporting
     * @param pipe     the pipeline configuration
     * @param referrer if the schema document is referenced from an xs:import, xs:include, or xs:redefined, this
     *                 parameter identifies that element; otherwise, null.
     * @return the new SchemaDocument created from the source of this SchemaReader. This may be either a source
     *                 XSD schema document, or a compiled SCM schema document.
     * @throws SchemaException if the schema module can't be read or parsed,
     *                         or if the schema module is an invalid schema
     */
    public static SchemaDocument read(Source source,
                                      SchemaCompiler compiler,
                                      PipelineConfiguration pipe,
                                      /*@Nullable*/ SchemaElement referrer)
            throws SchemaException {
        boolean tracing = pipe.getConfiguration().isTiming();
        String docId = source.getSystemId();
        if (docId == null) {
            docId = "(unknown URI)";
        }
        if (tracing) {
            pipe.getConfiguration().getLogger().info("Loading schema document " + docId);
        }
        SchemaDocument schemaDocument = buildSchemaDocument(source, compiler, pipe, referrer);
        AxisIterator children = schemaDocument.getDocument().getRootNode().iterateAxis(
                AxisInfo.CHILD, NodeKindTest.makeNodeKindTest(Type.ELEMENT));
        NodeInfo top = children.next();
        if (top.getNamespaceUri().equals(NamespaceUri.SCM)) {
            if (tracing) {
                pipe.getConfiguration().getLogger().info("Loaded compiled schema document " + docId);
            }
            return schemaDocument;
        }
        if (!(top instanceof XSDSchema)) {
            SchemaException err = new SchemaException("Outermost element of schema document must be xs:schema");
            Loc loc = new Loc(source.getSystemId(), top == null ? 1 : top.getLineNumber(), -1);
            err.setLocator(loc);
            pipe.getErrorReporter().report(new XmlProcessingException(err));
            throw err;
        }

        compiler.setPipelineConfiguration(pipe);
        compiler.setBeingRead(source.getSystemId());
        ((XSDSchema) top).processAllAttributes();
        ((XSDSchema) top).validateSubtree(compiler);
        compiler.markAsRead(source.getSystemId(), (XSDSchema) top);

        int errors = ((XSDSchema) top).getErrorCount();
        if (errors > 0) {
            String msg = errors == 1 ? "One error was" : errors + " errors were";
            throw new SchemaException(msg + " found while processing the schema");
        }

        if (tracing) {
            pipe.getConfiguration().getLogger().info("Finished loading schema document " + source.getSystemId());
        }

        return schemaDocument;

    }


    /**
     * Build the tree representation of a schema document, without doing any processing or XSD-specific
     * validation of the content
     *
     * @param source   the source of the module
     * @param compiler the SchemaCompiler (holds configuration information and keeps track of errors)
     * @param pipe     the Pipeline Configuration
     * @param referrer the xs:import or xs:include element, etc, that requested processing of this module
     * @return the root Document node of the tree containing the schema document
     * @throws SchemaException if XML parsing or tree
     *                         construction fails
     */
    public static SchemaDocument buildSchemaDocument(Source source,
                                                     SchemaCompiler compiler,
                                                     PipelineConfiguration pipe,
                                                     SchemaElement referrer)
            throws SchemaException {

        SchemaNodeFactory nodeFactory = new SchemaNodeFactory(compiler, pipe, referrer);
//        System.err.println("SchemaReader#processModule source= " + source);
//        System.err.println("pipe= " + pipe);
//        System.err.println("manager= " + manager);
//        System.err.println("referrer= " + referrer);
        LinkedTreeBuilder schemaBuilder = new LinkedTreeBuilder(pipe, Durability.LASTING);
        schemaBuilder.setSystemId(source.getSystemId());
        schemaBuilder.setNodeFactory(nodeFactory);
        schemaBuilder.setLineNumbering(true);

        Stripper stripper = new Stripper(AllElementsSpaceStrippingRule.getInstance(), schemaBuilder);
        VersionControlFilter vcFilter = new VersionControlFilter(compiler, stripper);
        CommentStripper commentStripper = new CommentStripper(vcFilter);

        if (source instanceof NodeInfo) {
            String base = ((NodeInfo) source).getBaseURI();
            if (!base.equals(source.getSystemId())) {
                schemaBuilder.setSystemId(base);
            }
            LocationCopier copier = new LocationCopier(((NodeInfo) source).getNodeKind() == Type.DOCUMENT, base);
            pipe.setCopyInformee(CSharp.methodRef(copier::notifyElementNode));
        }

        // Alternative pipeline for compiled schema documents

        TinyBuilder scmBuilder = new TinyBuilder(pipe);
        scmBuilder.setSystemId(source.getSystemId());
        Valve valve = new Valve(NamespaceUri.SCM, commentStripper, scmBuilder);


        // build the schema document

        TreeInfo tree;
        Configuration config = pipe.getConfiguration();
        //AugmentedSource aug = AugmentedSource.makeAugmentedSource(source);
        ParseOptions options = new ParseOptions(config.getParseOptions());
        options.setSchemaValidationMode(Validation.PRESERVE);
        options.setErrorReporter(compiler.getErrorReporter());
        try {
            commentStripper.setPipelineConfiguration(pipe);
            sendSchemaSource(source, compiler, pipe, valve, config, options);
            if (valve.wasDiverted()) {
                tree = scmBuilder.getTree();
            } else {
                tree = schemaBuilder.getCurrentRoot().getTreeInfo();
            }
            schemaBuilder.reset();
        } catch (XPathException err) {
            Throwable cause = err.getException();
            if (cause != null) {
                if (isSAXException(cause)) {
                    //#if CSHARP==false
                    // This normally means there was an XML parsing error, in which
                    // case it has already been reported. But in the case of Crimson,
                    // the SAXParseException might wrap an exception that happened
                    // within callback code in Saxon.
                    SAXParseException spe = (SAXParseException) cause;
                    cause = spe.getException();
                    if (cause != null) {
                        if (cause instanceof SchemaException) {
                            throw (SchemaException) cause;
                        } else {
                            if (cause instanceof RuntimeException) {
                                // A RunTimeException during schema processing is bad news...
                                cause.printStackTrace();
                            }
                            throw new SchemaException(cause);
                        }
                    }
                    // details already reported, don't repeat them
                    //#endif
                    throw new SchemaException("Failed to parse schema document");
                } else if (cause instanceof SchemaException) {
                    throw (SchemaException) cause;
                } else {
                    throw new SchemaException(cause);
                }
            }
            throw new SchemaException(err);
        } finally {
            if (options.isPleaseCloseAfterUse()) {
                ParseOptions.close(source);
            }
        }

        NodeInfo doc = tree.getRootNode();
        if (doc.iterateAxis(AxisInfo.CHILD, NodeKindTest.ELEMENT).next() == null) {
            throw new SchemaException("Schema document is empty or absent");
        }

        return new SchemaDocument(tree);

    }

    @CSharpReplaceBody(code="return false;")
    private static boolean isSAXException(Throwable e) {
        return e instanceof SAXParseException;
    }

    @CSharpReplaceBody(code="Saxon.Hej.@event.Sender.send(source, valve, options);")
    private static void sendSchemaSource(Source source, SchemaCompiler compiler, PipelineConfiguration pipe, Valve valve, Configuration config, ParseOptions options) throws XPathException {
        if (Version.platform.isJava() && options.getXMLReader() == null && options.getXMLReaderMaker() == null
                && !(source instanceof NodeInfo || source instanceof DOMSource)) {
            setupSourceWithReader(source, compiler, valve, config, options);
        } else if ((source instanceof NodeInfo && ((NodeInfo) source).getNodeKind() == Type.ELEMENT) ||
                (source instanceof DOMSource && ((DOMSource) source).getNode().getNodeType() == Type.ELEMENT)) {
            // bug 2217
            TinyBuilder temp = new TinyBuilder(pipe);
            temp.setSystemId(source.getSystemId());
            temp.open();
            temp.startDocument(ReceiverOption.NONE);
            Sender.send(source, temp, options);
            temp.endDocument();
            temp.close();
            Sender.send(temp.getCurrentRoot().asActiveSource(), valve, options);
        } else {
            Sender.send(source, valve, options);
        }
    }

//#if CSHARP==false
    public static void setupSourceWithReader(Source source, SchemaCompiler compiler, Valve valve, Configuration config, ParseOptions options) throws XPathException {
        XMLReader parser = config.getStyleParser();
        options.setXMLReader(parser);
        if (options.getEntityResolver() != null && parser.getEntityResolver() == null) {
            parser.setEntityResolver(options.getEntityResolver());
        }
        if (compiler.getErrorReporter() instanceof SchemaFactoryImpl.ErrorReporterWrappingErrorHandler) {
            parser.setErrorHandler(
                    ((SchemaFactoryImpl.ErrorReporterWrappingErrorHandler)compiler.getErrorReporter()).getErrorHandler());
        }
        options.setDTDValidationMode(Validation.SKIP);
        Sender.send(source, valve, options);
        config.reuseStyleParser(parser);
    }
//#endif


}

// Copyright (c) 2004-2022 Saxonica Limited