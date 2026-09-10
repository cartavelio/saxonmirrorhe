////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package s9apitest;

import com.saxonica.config.EnterpriseTransformerFactory;
import com.saxonica.config.ProfessionalTransformerFactory;
import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.AccessorFn;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.option.jdom2.JDOM2ObjectModel;
import net.sf.saxon.option.xom.XOMDocumentWrapper;
import net.sf.saxon.option.xom.XOMObjectModel;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trace.Traceable;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.ComplexType;
import net.sf.saxon.type.SchemaException;
import net.sf.saxon.type.SchemaType;
import net.sf.saxon.value.Cardinality;
import net.sf.saxon.value.QNameValue;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.*;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.*;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the s9api Xslt30Transformer in "debugging" mode, that is,
 * with a TraceListener enabled.
 */
public class TestXslt30TransformerDebug {
    private final ConfigTest configTest = new ConfigTest();
    public static class MyTraceListener implements TraceListener {

        @Override
        public void setOutputDestination(Logger stream) {

        }

        @Override
        public void open(Controller controller) {

        }

        @Override
        public void close() {

        }

        @Override
        public void enter(Traceable instruction, Map<String, Object> properties, XPathContext context) {

        }

        @Override
        public void leave(Traceable instruction) {

        }

        @Override
        public void startCurrentItem(Item currentItem) {

        }

        @Override
        public void endCurrentItem(Item currentItem) {

        }
    }

    /**
     * Simple test
     */

    @Test
    public void testSimple() {
        // Create a Processor instance.
        try {
            Processor processor = new Processor(false);
            processor.setConfigurationProperty(Feature.TIMING, true);
            processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);

            // Create a transformer for the stylesheet.
            Xslt30Transformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new File(configTest.getDataDir() + "caffo/Transform2.xslt"))).load30();
            transformer.setTraceListener(new MyTraceListener());
            // Create a serializer
            XdmDestination serializer = new XdmDestination();
            long start = new Date().getTime();

            // Transform the source XML to System.out.
            for (int i = 0; i < 10; i++) {
                // Load the source document
                XdmNode input = processor.newDocumentBuilder().build(
                        new File(configTest.getDataDir() + "caffo/Input.xml"));
                // Set the root node of the source document to be the initial context node
                transformer.applyTemplates(input, serializer);
            }

            long end = new Date().getTime();
            System.err.println("Av Time: " + (end - start) / 10 + "ms");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testGlobalVar() {
        // Test with a global variable that depends on context - with tracing enabled, this is preevaluated
        try {

            String xx = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n" +
                    "   <xsl:variable name=\"v1\" select=\"//*\" />\n" +
                    "   <xsl:template match=\"/\">\n" +
                    "   \tNumber of elements: <xsl:value-of select=\"count($v1)\"/>\n" +
                    "   </xsl:template>\n" +
                    "</xsl:stylesheet>";
            Processor processor = new Processor(false);
            processor.setConfigurationProperty(Feature.TIMING, true);
            processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);

            // Create a transformer for the stylesheet.
            Xslt30Transformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new StringReader(xx))).load30();
            transformer.setTraceListener(new MyTraceListener());

            XdmNode input = processor.newDocumentBuilder().build(
                    new File(configTest.getDataDir() + "caffo/Input.xml"));

            transformer.setGlobalContextItem(input);

            // Create a serializer
            XdmDestination serializer = new XdmDestination();
            long start = new Date().getTime();

            // Transform the source XML to System.out.
            for (int i = 0; i < 10; i++) {
                // Set the root node of the source document to be the initial context node
                transformer.applyTemplates(input, serializer);
            }

            long end = new Date().getTime();
            System.err.println("Av Time: " + (end - start) / 10 + "ms");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Same as above, but using JAXP invocation
     */

    @Test
    public void testGlobalVarWithJaxp() {
        try {
            String xx = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\">\n" +
                    "   <xsl:variable name=\"v1\" select=\"//*\" />\n" +
                    "   <xsl:template match=\"/\">\n" +
                    "   \tNumber of elements: <xsl:value-of select=\"count($v1)\"/>\n" +
                    "   </xsl:template>\n" +
                    "</xsl:stylesheet>";
            TransformerFactory transformerFactory = new ProfessionalTransformerFactory();
            transformerFactory.setAttribute(
                    FeatureKeys.TRACE_LISTENER, new MyTraceListener());
            transformerFactory.setAttribute(
                    FeatureKeys.COMPILE_WITH_TRACING, Boolean.TRUE);
            transformerFactory.setAttribute(
                    FeatureKeys.LINE_NUMBERING, true);

            // Create transformer
            InputSource inputSource = new InputSource(new StringReader(xx));
            Source source = new SAXSource(inputSource);
            Transformer transformer = transformerFactory.newTransformer(source);

            // Create XML  SAX source
            Source xmlSource = new StreamSource(new File(configTest.getDataDir() + "caffo/Input.xml"));

            // Output stream result
//		Result outputTarget = new StreamResult(new File(outPath));
            Result outputTarget = new StreamResult(System.out);

            // Run transformation
            System.out.println("Start transformation...");
            transformer.transform(xmlSource, outputTarget);
            System.out.println("Transformation done.");
        } catch (TransformerException e) {
            fail();
        }
    }

    @Test
    public void testDocCallWithJaxp() {
        try {
            String xx = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:output method='xml' indent='yes'/>\n" +
                    "<xsl:param name='p'/>" +
                    "<xsl:template match='/'>\n" +
                    "<xsl:copy-of select='doc($p)'/>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>";
            TransformerFactory transformerFactory = new EnterpriseTransformerFactory();
            transformerFactory.setAttribute(
                    FeatureKeys.TRACE_LISTENER, new MyTraceListener());
            transformerFactory.setAttribute(
                    FeatureKeys.COMPILE_WITH_TRACING, Boolean.TRUE);
            transformerFactory.setAttribute(
                    FeatureKeys.LINE_NUMBERING, true);

            // Create transformer
            InputSource inputSource = new InputSource(new StringReader(xx));
            Source source = new SAXSource(inputSource);
            Transformer transformer = transformerFactory.newTransformer(source);
            transformer.setParameter("p", configTest.getDataDir() + "books.xml");

            // Create XML  SAX source
            Source xmlSource = new StreamSource(new File(configTest.getDataDir() + "caffo/Input.xml"));

            // Output stream result
//		Result outputTarget = new StreamResult(new File(outPath));
            Result outputTarget = new StreamResult(System.out);

            // Run transformation
            System.out.println("Start transformation...");
            transformer.transform(xmlSource, outputTarget);
            System.out.println("Transformation done.");
        } catch (TransformerException e) {
            fail();
        }
    }


    /**
     * Test for the case where the context item is not the root
     */

    @Test
    public void testContextNotRoot() {
        try {
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);

            StringReader sr = new StringReader("<doc><e>text</e></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode node = builder.build(source);
            XsltCompiler xqcomp = proc.newXsltCompiler();
            sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:variable name='x' select='.'/>" +
                    "<xsl:template match='/'>errorA</xsl:template>" +
                    "<xsl:template match='e'>[<xsl:value-of select='name($x)'/>]</xsl:template>" +
                    "</xsl:stylesheet>");
            SAXSource style = new SAXSource(new InputSource(sr));
            Xslt30Transformer trans = xqcomp.compile(style).load30();
            trans.setTraceListener(new MyTraceListener());
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            Serializer serializer = proc.newSerializer(os);

            XdmSequenceIterator iter = node.axisIterator(Axis.DESCENDANT, new QName("e"));
            XdmNode e = (XdmNode) iter.next();
            trans.setGlobalContextItem(node);
            trans.applyTemplates(e, serializer);

            String result = os.toString();
            assertFalse(result.contains("errorA"), "matched root");
            assertTrue(result.contains("[]"), "matched e");

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    /**
     * Test location information for run-time errors when stylesheet built as an XdmNode
     */

    @Test
    public void testErrorLocationFromXdm() {
        try {
            // Norm Walsh reported getting line numbers > 2^20. Can't reproduce.
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
            StringReader sr = new StringReader("<doc><e>text</e></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setLineNumbering(true);
            XdmNode node = builder.build(source);
            XsltCompiler xqcomp = proc.newXsltCompiler();

            sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:import href='file:///" + configTest.getDataDir() + "identity.xsl'/>\n" +
                    "<xsl:import href='http://imported'/>\n" +
                    "<xsl:param name='x' select='0'/>\n" +
                    "<xsl:template match='/'>\n" +
                    "<xsl:apply-templates select='*'>\n" +
                    "<xsl:with-param name='p' select='()'/>\n" +
                    "</xsl:apply-templates>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>");
            XdmNode style2 = builder.build(new StreamSource(sr, "http://importer/"));

            sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:template match='e' as='node()'>\n" +
                    "<xsl:param name='x' select='0'/>\n" +
                    "<xsl:param name='p' as='node()*' select='()'/>\n" +
                    //"<e1/>" +
                    //"<e2/>" +
                    "<xsl:sequence select='$p'/>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>");
            final XdmNode style1 = builder.build(new StreamSource(sr, "http://imported/"));

            xqcomp.setURIResolver(new URIResolver() {
                @Override
                public Source resolve(String href, String base) throws TransformerException {
                    if (href.startsWith("http:")) {
                        return style1.asSource();
                    } else {
                        return null;
                    }
                }
            });

            Xslt30Transformer trans = xqcomp.compile(style2.asSource()).load30();
            trans.setTraceListener(new MyTraceListener());
            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.applyTemplates(node, serializer);

            fail("Transformation should fail");

        } catch (SaxonApiException e) {
            // Different from the non-debug case, but equally valid - see bug 2606
            assertEquals(3, ((XPathException) e.getCause()).getLocator().getLineNumber(), "line=3");
        }
    }

    /**
     * Test created ErrorListener picks up errors in imported stylesheet
     */

    @Test
    public void testErrorInImportedStylesheet() {
        final List<TransformerException> warnings = new ArrayList<TransformerException>(4);
        final List<TransformerException> errors = new ArrayList<TransformerException>(4);
        final List<TransformerException> fatals = new ArrayList<TransformerException>(4);
        ErrorListener listener = new ErrorListener() {

            @Override
            public void warning(TransformerException exception) throws TransformerException {
                warnings.add(exception);
            }

            @Override
            public void error(TransformerException exception) throws TransformerException {
                errors.add(exception);
            }

            @Override
            public void fatalError(TransformerException exception) throws TransformerException {
                fatals.add(exception);
            }

        };
        try {

            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);


            XsltCompiler xqcomp = proc.newXsltCompiler();
            xqcomp.setErrorListener(listener);

            Xslt30Transformer trans = xqcomp.compile(new StreamSource((new File(configTest.getDataDir() + "main.xsl")))).load30();
            trans.setTraceListener(new MyTraceListener());
            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.callTemplate(new QName("main"), serializer);

            fail("Transformation should fail");

        } catch (SaxonApiException e) {
            assertTrue(warnings.isEmpty(), "warnings");
            assertTrue(errors.isEmpty(), "errors");
            assertTrue(fatals.size() > 0, "fatals");
            //e.printStackTrace();
        }
    }


    /**
     * Test for the case where the context item is not the root, when starting at a named template
     */

    @Test
    public void testContextNotRootNamedTemplate() {
        try {
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);

            StringReader sr = new StringReader("<doc><e>text</e></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode node = builder.build(source);
            XsltCompiler xqcomp = proc.newXsltCompiler();
            sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:variable name='x' select='.'/>" +
                    "<xsl:template match='/'>errorA</xsl:template>" +
                    "<xsl:template name='main'>[<xsl:value-of select='name($x)'/>]</xsl:template>" +
                    "</xsl:stylesheet>");
            SAXSource style = new SAXSource(new InputSource(sr));
            Xslt30Transformer trans = xqcomp.compile(style).load30();
            trans.setTraceListener(new MyTraceListener());

            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.setGlobalContextItem(node);
            trans.callTemplate(new QName("main"), serializer);

            String result = os.toString();
            assertFalse(result.contains("errorA"), "matched root");
            assertTrue(result.contains("[]"), "matched e");

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testUseAssociated() {

        try {

            Processor processor = new Processor(false);
            processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);

            XsltCompiler compiler = processor.newXsltCompiler();

            String d = configTest.getDataDir() + "trax/";

            String foo_xml = d + "xml/foo.xml";
            Source sources =
                    compiler.getAssociatedStylesheet(new StreamSource(new File(foo_xml).toURI().toASCIIString()), null,
                            null, null);


            Xslt30Transformer transformer = compiler.compile(sources).load30();
            transformer.setTraceListener(new MyTraceListener());

            transformer.applyTemplates(new StreamSource(foo_xml),
                    processor.newSerializer(System.out));


        } catch (SaxonApiException ex) {
            ex.printStackTrace();
            fail();
        }
    }

    /**
     * Build a document from a non-root element of a DOMSource
     */

    @Test
    public void testNonRootDomSource() {
        try {
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'>///<xsl:apply-templates/></xsl:template>" +
                    "<xsl:template match='*'>***<xsl:apply-templates/></xsl:template>" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            xsl.setTraceListener(new MyTraceListener());
            String in = "<?xml version='1.0'?> \n" +
                    "<doc><a><b/></a></doc>";

            // Read an XML file to a DOM
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            javax.xml.parsers.DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(in));
            org.w3c.dom.Document doc = db.parse(is);
            org.w3c.dom.Element ele = doc.getDocumentElement();
            StringWriter writer = new StringWriter();
            Serializer out = proc.newSerializer(writer);
            xsl.applyTemplates(new DOMSource(ele), out);
            assertFalse(writer.toString().contains("///"), "started at element node");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Send output to an XdmDestination
     */

    @Test
    public void testXdmDestination() {
        try {
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template name='go'><a/></xsl:template>" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            xsl.setTraceListener(new MyTraceListener());
            XdmDestination out = new XdmDestination();
            xsl.callTemplate(new QName("go"), out);
            XdmNode root = out.getXdmNode();
            assertTrue(root.getNodeKind() == XdmNodeKind.DOCUMENT, "result is document node");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Send output to a XOM tree
     */

    @Test
    public void testXOMDestination() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template name='go'><a/></xsl:template>" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            xsl.setTraceListener(new MyTraceListener());
            XdmDestination out = new XdmDestination();
            out.setTreeModel(XOMObjectModel.getInstance());
            xsl.callTemplate(new QName("go"), out);
            XdmNode root = out.getXdmNode();
            assertTrue(root.getUnderlyingNode() instanceof XOMDocumentWrapper, "result is XOM document node");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Send output to a JDOM2 tree
     */

    @Test
    public void testJDOM2Destination() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template name='go'><a/></xsl:template>" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            xsl.setTraceListener(new MyTraceListener());
            XdmDestination out = new XdmDestination();
            out.setTreeModel(JDOM2ObjectModel.getInstance());
            xsl.callTemplate(new QName("go"), out);
            XdmNode root = out.getXdmNode();
            assertTrue(root.getUnderlyingNode() instanceof net.sf.saxon.option.jdom2.JDOM2NodeWrapper, "result is JDOM2 document node");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testExtensionWithSchemaAccess() {
        // Create a Processor instance.
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
            proc.setConfigurationProperty(Feature.TIMING, true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:my='java:s9apitest.TestXslt30Transformer' exclude-result-prefixes='my'>" +
                    "<xsl:template match='*'><xsl:copy><xsl:copy-of select='@*'/><xsl:apply-templates/></xsl:copy></xsl:template>\n" +
                    "<xsl:template match='*/*/*/*[my:isOptional(.., node-name(.))]'/> \n" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();
            transformer.setTraceListener(new MyTraceListener());

            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(new File(configTest.getDataDir() + "books.xsd")));
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setSchemaValidator(sm.newSchemaValidator());
            XdmNode books = builder.build(new File(configTest.getDataDir() + "books.xml"));
            // Create a serializer
            //String outfile = @"C:/MyJava/users/caffo/Output.xml";
            StringWriter sw = new StringWriter();
            Serializer serializer = proc.newSerializer(sw);
            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            transformer.applyTemplates(books, serializer);
            assertTrue(sw.toString().contains("TITLE"));
            assertFalse(sw.toString().contains("AUTHOR"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Sample extension function - explores schema information
     *
     * @return true if the element defined by childName is optional in the context of the governing
     *         type of parent.
     */

    public static boolean isOptional(NodeInfo parent, QNameValue childName) throws Exception {
        Configuration config = parent.getConfiguration();
        SchemaType parentType = parent.getSchemaType();
        int fp = config.getNamePool().allocateFingerprint(
                NamespaceUri.of(childName.getComponent(AccessorFn.Component.NAMESPACE).getStringValue()),
                childName.getComponent(AccessorFn.Component.LOCALNAME).getStringValue());
        int cardinality = ((ComplexType) parentType).getElementParticleCardinality(fp, null, true);
        return Cardinality.allowsZero(cardinality);
    }

    /**
     * s9api transformation pipeline
     */

    @Test
    public void testPipeline() {
        try {
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
            XsltCompiler comp = proc.newXsltCompiler();

            String xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                    "</xsl:stylesheet>";
            String xml = "<z/>";
            XdmNode in = proc.newDocumentBuilder().build(new StreamSource(new StringReader(xml)));

            Xslt30Transformer stage1 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage2 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage3 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage4 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage5 = comp.compile(new StreamSource(new StringReader(xsl))).load30();

            stage1.setTraceListener(new MyTraceListener());
            stage2.setTraceListener(new MyTraceListener());
            stage3.setTraceListener(new MyTraceListener());
            stage4.setTraceListener(new MyTraceListener());
            stage5.setTraceListener(new MyTraceListener());

            XdmDestination d1 = new XdmDestination();
            stage1.applyTemplates(in, d1);
            XdmDestination d2 = new XdmDestination();
            stage2.applyTemplates(d1.getXdmNode(), d2);
            XdmDestination d3 = new XdmDestination();
            stage3.applyTemplates(d2.getXdmNode(), d3);
            XdmDestination d4 = new XdmDestination();
            stage4.applyTemplates(d3.getXdmNode(), d4);
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);
            stage5.applyTemplates(d4.getXdmNode(), out);
            assertTrue(sw.toString().contains("<a><a><a><a><a><z/></a></a></a></a></a>"), "output");

        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    /**
     * Test notification of errors from a call on saxon:parse()
     */

    @Test
    public void testErrorNotificationFromSaxonParse() {
        final List<TransformerException> warnings = new ArrayList<>(4);
        final List<TransformerException> errors = new ArrayList<>(4);
        final List<TransformerException> fatals = new ArrayList<>(4);
        ErrorListener listener = new ErrorListener() {

            @Override
            public void warning(TransformerException exception)  {
                warnings.add(exception);
            }

            @Override
            public void error(TransformerException exception)  {
                errors.add(exception);
            }

            @Override
            public void fatalError(TransformerException exception)  {
                fatals.add(exception);
            }

        };
        try {
            // Norm Walsh reported saxon:parse errors not notified to the ErrorListener. Can't reproduce.

            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);

            XsltCompiler xqcomp = proc.newXsltCompiler();


            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' " +
                    "xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:saxon='http://saxon.sf.net/'>\n" +
                    "<xsl:template name='main'>\n" +
                    "<xsl:copy-of select=\"saxon:parse('&lt;doc>&lt;e>text&lt;/f>&lt;/doc>')\"/>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>");


            Xslt30Transformer trans = xqcomp.compile(new StreamSource(sr)).load30();
            trans.setTraceListener(new MyTraceListener());
            trans.setErrorListener(listener);

            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.callTemplate(new QName("main"), serializer);

            fail("Transformation should fail");

        } catch (SaxonApiException e) {
            assertTrue(warnings.isEmpty(), "warnings");
            assertTrue(errors.isEmpty(), "errors");
            assertEquals(1, fatals.size(), "fatals");
            //e.printStackTrace();
        }
    }

    @Test
    public void testSchemaAware11() {
        // Create a Processor instance.
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.TIMING, true);
            proc.setConfigurationProperty(Feature.XSD_VERSION, "1.1");
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xs:element name='e'><xs:complexType><xs:sequence><xs:element name='p'/></xs:sequence><xs:assert test='exists(p)'/></xs:complexType></xs:element>" +
                            "</xs:schema></xsl:import-schema>" +
                            "<xsl:variable name='v'><e><p/></e></xsl:variable>" +
                            "<xsl:template name='main'><xsl:copy-of select='$v' validation='strict'/></xsl:template>" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();
            transformer.setTraceListener(new MyTraceListener());

            StringWriter sw = new StringWriter();
            Serializer serializer = proc.newSerializer(sw);
            serializer.setOutputProperty(Serializer.Property.INDENT, "no");
            transformer.callTemplate(new QName("main"), serializer);
            assertTrue(sw.toString().contains("<e>"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testValidationWarnings() {
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.VALIDATION_WARNINGS, Boolean.TRUE);
            processor.getUnderlyingConfiguration().addSchemaSource(
                    new StreamSource(configTest.getDataDir() + "books.xsd"));
            processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);

            Serializer serializer = processor.newSerializer();
            serializer.setOutputFile(new File(configTest.getTempDir() + "validationWarnings.xml"));

            XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setSchemaAware(true);

            String xsl = "<xsl:stylesheet version=\"2.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "\n" +
                    "    <xsl:output indent=\"yes\" encoding=\"UTF-8\" />\n" +
                    "\n" +
                    "    <xsl:template match=\"/\">\n" +
                    "        <xsl:document validation='strict'>\n" +
                    "            <xsl:next-match />\n" +
                    "        </xsl:document>\n" +
                    "    </xsl:template>\n" +
                    "\n" +
                    "    <xsl:template match=\"*\">\n" +
                    "        <xsl:element name=\"{name()}\" namespace=\"{namespace-uri()}\">\n" +
                    "            <invalid-element/>\n" +
                    "        </xsl:element>\n" +
                    "    </xsl:template>\n" +
                    "</xsl:stylesheet> ";
            Xslt30Transformer transformer = compiler.compile(new StreamSource(new StringReader(xsl))).load30();
            transformer.setTraceListener(new MyTraceListener());

            transformer.applyTemplates(new StreamSource(new StringReader(xsl)), serializer);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        } catch (SchemaException se) {
            se.printStackTrace();
            fail();
        }
    }

    /**
     * Test that the base URI of the result document is the base output URI
     */

    @Test
    public void testOutputBaseUri() throws SaxonApiException {
        try {
            Processor processor = new Processor(false);
            processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);

            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setDTDValidation(false);
            builder.setLineNumbering(true);

            XdmNode document = builder.build(new File(configTest.getDataDir() + "books.xml"));
            XdmNode stylesheet = builder.build(new File(configTest.getDataDir() + "books.xsl"));

            XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setSchemaAware(processor.isSchemaAware());
            XsltExecutable exec = compiler.compile(stylesheet.asSource());
            Xslt30Transformer transformer = exec.load30();
            transformer.setTraceListener(new MyTraceListener());

            XdmDestination result = new XdmDestination();
            transformer.setBaseOutputURI("file://tmp/out.xml");

            transformer.setSchemaValidationMode(ValidationMode.DEFAULT);
            transformer.setGlobalContextItem(document);
            transformer.applyTemplates(document, result);

            XdmNode xformed = result.getXdmNode();
            assertEquals("file://tmp/out.xml", xformed.getBaseURI().toString(), "output base uri");
        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            fail();
        }
    }

    /**
     * test a call on XsltExecutable.getGlobalParameters
     *
     * @throws SaxonApiException
     *
     */

    @Test
    public void testGlobalParameters() throws SaxonApiException, UnsupportedEncodingException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
        processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                version='2.0'>" +

                "                <xsl:param name='surname' as='xs:string' />" +
                "                <xsl:param name='age' as='xs:integer' />" +

                "                <xsl:template match='/'><xsl:element name='root' /></xsl:template>" +
                "                </xsl:stylesheet>";

        XsltExecutable xsltExecutable = xsltCompiler.compile(new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8))));
        Map<QName, XsltExecutable.ParameterDetails> params = xsltExecutable.getGlobalParameters();

        System.out.println();
        int i = 0;
        for (QName key : params.keySet()) {
            System.out.println("Parameter n." + (++i) + ": " + key);
        }
        System.out.println();
        assertEquals(2, 2);
    }

    /**
     * Get declared types of global parameters
     * @throws SaxonApiException
     * @throws UnsupportedEncodingException
     */

    @Test
    public void testGlobalParameterTypes() throws SaxonApiException, UnsupportedEncodingException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
        processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                version='2.0'>" +

                "                <xsl:param name='surname' as='xs:string' />" +
                "                <xsl:param name='age' as='xs:integer?' />" +
                "                <xsl:param name='doc' as='document-node()' />" +
                "                <xsl:param name='ele' as='element(pre:a, xs:date)+' xmlns:pre='http://pre/'/>" +

                "                <xsl:template match='/'><xsl:element name='root' /></xsl:template>" +
                "                </xsl:stylesheet>";

        XsltExecutable xsltExecutable = xsltCompiler.compile(new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8))));
        Map<QName, XsltExecutable.ParameterDetails> params = xsltExecutable.getGlobalParameters();
        ItemType t1 = params.get(new QName("surname")).getDeclaredItemType();
        assertTrue(t1.toString().equals("Q{" + NamespaceConstant.SCHEMA + "}string"), "t1");
        OccurrenceIndicator c1 = params.get(new QName("surname")).getDeclaredCardinality();
        assertTrue(c1.toString().equals(""), "c1");
        ItemType t2 = params.get(new QName("age")).getDeclaredItemType();
        assertTrue(t2.toString().equals("Q{" + NamespaceConstant.SCHEMA + "}integer"), "t2");
        OccurrenceIndicator c2 = params.get(new QName("age")).getDeclaredCardinality();
        assertTrue(c2.toString().equals("?"), "c2");
        ItemType t3 = params.get(new QName("doc")).getDeclaredItemType();
        assertTrue(t3.toString().equals("document-node()"), "t3");
        OccurrenceIndicator c3 = params.get(new QName("doc")).getDeclaredCardinality();
        assertTrue(c3.toString().equals(""), "c3");
        ItemType t4 = params.get(new QName("ele")).getDeclaredItemType();
        assertEquals("element(Q{http://pre/}a, Q{" + NamespaceConstant.SCHEMA + "}date)", t4.toString(), "t4");
        OccurrenceIndicator c4 = params.get(new QName("ele")).getDeclaredCardinality();
        assertTrue(c4.toString().equals("+"), "c4");
    }

    /**
     * Test an external  call on a stylesheet function
     *
     * @throws SaxonApiException
     *          if the call fails
     */

    @Test
    public void testCallFunction() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
        processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                xmlns:f='http://localhost/'" +
                "                version='2.0'>" +
                "                <xsl:function name='f:add' visibility='public'><xsl:param name='a'/><xsl:param name='b'/>" +
                "                  <xsl:sequence select='$a + $b'/></xsl:function>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        t.setTraceListener(new MyTraceListener());

        XdmValue v = t.callFunction(new QName("http://localhost/", "f:add"), new XdmValue[]{new XdmAtomicValue(2), new XdmAtomicValue(3)});
        assertEquals(((XdmAtomicValue) v).getLongValue(), 5);
    }

    /**
     * Test an external  call on a stylesheet function with argument conversion
     *
     * @throws SaxonApiException
     *          if the call fails
     */

    @Test
    public void testCallFunctionArgConversion() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
        processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                xmlns:f='http://localhost/'" +
                "                version='2.0'>" +
                "                <xsl:function name='f:add' visibility='public'>" +
                "                   <xsl:param name='a' as='xs:double'/>" +
                "                   <xsl:param name='b' as='xs:double'/>" +
                "                   <xsl:sequence select='$a + $b'/>" +
                "                </xsl:function>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        t.setTraceListener(new MyTraceListener());

        XdmValue v = t.callFunction(new QName("http://localhost/", "f:add"), new XdmValue[]{new XdmAtomicValue(2), new XdmAtomicValue(3)});
        assertEquals(((XdmAtomicValue) v).getDoubleValue(), 5.0e0);
        assertEquals((((XdmAtomicValue) v)).getPrimitiveTypeName().getLocalName(), "double");
    }

    /**
     * Test an external  call on a stylesheet function serializing the results
     * Also makes access to a stylesheet parameter which in turn accesses the global context item
     *
     * @throws SaxonApiException
     *          if the call fails
     */

    @Test
    public void testCallFunctionWrapResults() throws SaxonApiException {
        Processor processor = new Processor(true);
        processor.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
        processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                xmlns:f='http://localhost/'" +
                "                version='2.0'>" +
                "                <xsl:param name='x' as='xs:integer'/>" +
                "                <xsl:param name='y' select='.+2'/>" +
                "                <xsl:function name='f:add' visibility='final'>" +
                "                   <xsl:param name='a' as='xs:double'/>" +
                "                   <xsl:param name='b' as='xs:double'/>" +
                "                   <xsl:sequence select='$a + $b + $x + $y'/>" +
                "                </xsl:function>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        t.setTraceListener(new MyTraceListener());
        t.setGlobalContextItem(new XdmAtomicValue(20));
        Map<QName, XdmValue> globals = new HashMap<QName, XdmValue>();
        globals.put(new QName("x"), new XdmAtomicValue(30));
        t.setStylesheetParameters(globals);
        StringWriter sw = new StringWriter();
        Serializer s = processor.newSerializer();
        s.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
        s.setOutputWriter(sw);
        t.callFunction(new QName("http://localhost/", "f:add"), new XdmValue[]{new XdmAtomicValue(2), new XdmAtomicValue(3)}, s);
        assertEquals(sw.toString(), "57");
    }


    /**
     * Test an external  call on a stylesheet function with invalid argument conversion
     *
     * @throws SaxonApiException
     *          if the call fails
     */

    @Test
    public void testCallFunctionArgInvalid() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            Processor processor = new Processor(configuration);
            processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                xmlns:f='http://localhost/'" +
                    "                version='2.0'>" +
                    "                <xsl:function name='f:add'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:double'/>" +
                    "                   <xsl:sequence select='$a + $b'/>" +
                    "                </xsl:function>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            t.setTraceListener(new MyTraceListener());
            XdmValue v = t.callFunction(new QName("http://localhost/", "f:add"), new XdmValue[]{new XdmAtomicValue(2), new XdmAtomicValue("3")});
            fail("Failed to throw error");
        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            // OK
        }
    }

    /**
     * Test calling a stylesheet with an initial template with parameters
     *
     * @throws SaxonApiException
     *          if the call fails
     */

    @Test
    public void testCallNamedTemplateWithParams() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        //configuration.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
        Processor processor = new Processor(configuration);
        processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                version='3.0'>" +
                "                <xsl:template name='t'>" +
                "                   <xsl:param name='a' as='xs:double'/>" +
                "                   <xsl:param name='b' as='xs:float'/>" +
                "                   <xsl:sequence select='$a + $b'/>" +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        t.setTraceListener(new MyTraceListener());
        Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
        params.put(new QName("a"), new XdmAtomicValue(12));
        params.put(new QName("b"), new XdmAtomicValue(5));
        t.setInitialTemplateParameters(params, false);
        StringWriter sw = new StringWriter();
        Serializer se = processor.newSerializer(sw);
        se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
        t.callTemplate(new QName("t"), se);
        assertEquals(sw.toString(), "17");
    }

    /**
     * Test calling a stylesheet with an initial template omitting a required parameters
     *
     * @throws SaxonApiException
     *          if the call fails
     */

    @Test
    public void testCallNamedTemplateWithParamsRequired() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            //configuration.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
            Processor processor = new Processor(configuration);
            processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0'>" +
                    "                <xsl:template name='t'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:float' required='yes'/>" +
                    "                   <xsl:sequence select='$a + $b'/>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            t.setTraceListener(new MyTraceListener());
            Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
            params.put(new QName("a"), new XdmAtomicValue(12));
            t.setInitialTemplateParameters(params, false);
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.callTemplate(new QName("t"), se);
            fail("failed to detect error");
        } catch (SaxonApiException e) {
            assertTrue(e.getMessage().contains("required parameter $b"), "message");
        }
    }

    /**
     * Test calling a stylesheet with an initial template with tunnel parameters
     *
     * @throws SaxonApiException
     *          if the call fails
     */

    @Test
    public void testCallNamedTemplateWithTunnelParams() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        //configuration.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
        Processor processor = new Processor(configuration);
        processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                version='3.0'>" +
                "                <xsl:template name='t'>" +
                "                   <xsl:call-template name='u'/>" +
                "                </xsl:template>" +
                "                <xsl:template name='u'>" +
                "                   <xsl:param name='a' as='xs:double' tunnel='yes'/>" +
                "                   <xsl:param name='b' as='xs:float' tunnel='yes'/>" +
                "                   <xsl:sequence select='$a + $b'/>" +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        t.setTraceListener(new MyTraceListener());
        Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
        params.put(new QName("a"), new XdmAtomicValue(12));
        params.put(new QName("b"), new XdmAtomicValue(5));
        t.setInitialTemplateParameters(params, true);
        StringWriter sw = new StringWriter();
        Serializer se = processor.newSerializer(sw);
        se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
        t.callTemplate(new QName("t"), se);
        assertEquals(sw.toString(), "17");
    }

    /**
     * Test calling a stylesheet with an initial template rule with parameters
     *
     * @throws SaxonApiException
     *          if the call fails
     */

    @Test
    public void testCallTemplateRuleWithParams() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        //configuration.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
        Processor processor = new Processor(configuration);
        processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                version='3.0'>" +
                "                <xsl:template match='*'>" +
                "                   <xsl:param name='a' as='xs:double'/>" +
                "                   <xsl:param name='b' as='xs:float'/>" +
                "                   <xsl:sequence select='name(.), $a + $b'/>" +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        t.setTraceListener(new MyTraceListener());
        Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
        params.put(new QName("a"), new XdmAtomicValue(12));
        params.put(new QName("b"), new XdmAtomicValue(5));
        t.setInitialTemplateParameters(params, false);
        StringWriter sw = new StringWriter();
        Serializer se = processor.newSerializer(sw);
        se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
        t.applyTemplates(new StreamSource(new StringReader("<e/>")), se);
        assertEquals(sw.toString(), "e 17");
    }

    /**
     * Test calling a stylesheet named template getting raw results back
     *
     * @throws SaxonApiException if the call fails
     */

    @Test
    public void testCallNamedTemplateWithParamsRaw() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        //configuration.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
        Processor processor = new Processor(configuration);
        processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
            "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
            "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
            "                version='3.0'>" +
            "                <xsl:template name='t'>" +
            "                   <xsl:param name='a' as='xs:double'/>" +
            "                   <xsl:param name='b' as='xs:float'/>" +
            "                   <xsl:sequence select='$a+1, $b+1'/>" +
            "                </xsl:template>" +
            "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        t.setTraceListener(new MyTraceListener());
        Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
        params.put(new QName("a"), new XdmAtomicValue(12));
        params.put(new QName("b"), new XdmAtomicValue(5));
        t.setInitialTemplateParameters(params, false);
        XdmValue val = t.callTemplate(new QName("t"));
        assertEquals(2, val.size());
        assertEquals(13, ((XdmAtomicValue) val.itemAt(0)).getLongValue());
        assertEquals(6, ((XdmAtomicValue) val.itemAt(1)).getLongValue());
    }


    /**
     * Test calling a stylesheet with an initial template rule getting the raw result
     *
     * @throws SaxonApiException if the call fails
     */

    @Test
    public void testApplyTemplatesRaw() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
        processor.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
            "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
            "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
            "                version='3.0'>" +
            "                <xsl:template match='*'>" +
            "                   <xsl:param name='a' as='xs:double'/>" +
            "                   <xsl:param name='b' as='xs:float'/>" +
            "                   <xsl:sequence select='., $a + $b'/>" +
            "                </xsl:template>" +
            "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        t.setTraceListener(new MyTraceListener());
        Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
        params.put(new QName("a"), new XdmAtomicValue(12));
        params.put(new QName("b"), new XdmAtomicValue(5));
        t.setInitialTemplateParameters(params, false);
        StringWriter sw = new StringWriter();
        Serializer se = processor.newSerializer(sw);
        se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
        XdmValue result = t.applyTemplates(new StreamSource(new StringReader("<e/>")));
        assertEquals(2, result.size(), "size");
        XdmItem first = result.itemAt(0);
        assertTrue(first instanceof XdmNode, "t1");
        assertEquals("e", ((XdmNode) first).getNodeName().getLocalName(), "v1");
        XdmItem second = result.itemAt(1);
        assertTrue(second instanceof XdmAtomicValue, "t2");
        assertEquals(17e0, ((XdmAtomicValue) second).getDoubleValue(), "v2");
    }


    /**
     * Test Strip-type-annotations with ApplyTemplates
     */

    @Test
    public void testStripTypeAnnotations() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xslt:transform xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                    "                xmlns:my=\"http://ns.example.com/strip-type\"\n" +
                    "                xmlns:xslt=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                    "                exclude-result-prefixes=\"xs my\"\n" +
                    "                input-type-annotations=\"strip\"\n" +
                    "                version=\"2.0\">\n" +
                    "<!-- Purpose: Show that when @input-type-annotations=\"strip\", is-nilled property of element  nodes is set to false.-->\n" +
                    "\n" +
                    "   <xslt:template match=\"/my:userNode\">\n" +
                    "      <out>\n" +
                    "         <xslt:value-of select=\"nilled(.)\"/>\n" +
                    "      </out>\n" +
                    "   </xslt:template>\n" +
                    "</xslt:transform>");

            SchemaManager manager = proc.getSchemaManager();
            comp.setSchemaAware(true);

            manager.load(new StreamSource(new File(configTest.getDataDir() + "varietyOfTypes.xsd")));

            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            xsl.setTraceListener(new MyTraceListener());
            String in = "<?xml version='1.0'?> \n" +
                    "<my:userNode xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"\n" +
                    "\t\t  xsi:schemaLocation=\"http://ns.example.com/strip-type varietyOfTypes.xsd\"\n" +
                    "\t\t  xmlns:my=\"http://ns.example.com/strip-type\"\n" +
                    "\t \t  specialPart=\"123-AB\" listParts=\"000-SS 000-RR\" xsi:nil=\"true\"/>";

            SchemaValidator validator = manager.newSchemaValidator();
            validator.setLax(false);
            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setSchemaValidator(validator);
            StreamSource sDoc = new StreamSource(new ByteArrayInputStream(in.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
            XdmNode doc = builder.build(sDoc);

            StringWriter writer = new StringWriter();
            Serializer out = proc.newSerializer(writer);
            xsl.applyTemplates(doc, out);
            assertTrue(writer.toString().contains("false"), "strip-type-annotations not working");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


}



