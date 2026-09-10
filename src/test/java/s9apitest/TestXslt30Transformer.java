////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.instruct.TemplateRule;
import net.sf.saxon.functions.AccessorFn;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.option.jdom2.JDOM2ObjectModel;
import net.sf.saxon.option.xom.XOMDocumentWrapper;
import net.sf.saxon.option.xom.XOMObjectModel;
import net.sf.saxon.resource.BinaryResource;
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
import java.nio.file.Files;
import java.nio.file.Path;
import java.nio.file.Paths;
import java.util.*;
import java.util.zip.ZipEntry;
import java.util.zip.ZipOutputStream;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the s9api Xslt30Transformer
 *
 */
public class TestXslt30Transformer {
    private final ConfigTest configTest = new ConfigTest();
    
    /**
     * Simple test
     */

    @Test
    public void testSimple() {
        // Create a Processor instance.
        try {
            Processor processor = new Processor(false);
            processor.setConfigurationProperty(Feature.TIMING, true);

            // Create a transformer for the stylesheet.
            Xslt30Transformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new File(configTest.getDataDir() + "caffo/Transform2.xslt"))).load30();

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

    /**
     * Simple test using the transform() method
     */

    @Test
    public void testTransformMethod() {
        // Create a Processor instance.
        try {
            Processor processor = new Processor(false);
            processor.setConfigurationProperty(Feature.TIMING, true);

            // Create a transformer for the stylesheet.
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            xsltCompiler.setParameter(new QName("USE_GLOBAL_CONTEXT"), XdmAtomicValue.makeAtomicValue(true));
            Xslt30Transformer transformer = xsltCompiler.compile(
                    new StreamSource(new File(configTest.getDataDir() + "caffo/Transform2.xslt"))).load30();

            // Create a serializer
            XdmDestination serializer = new XdmDestination();
            long start = new Date().getTime();

            // Transform the source XML to System.out.

            StreamSource source = new StreamSource(
                    new File(configTest.getDataDir() + "caffo/Input.xml"));
            transformer.transform(source, serializer);

            long end = new Date().getTime();
            System.err.println("Av Time: " + (end - start) / 10 + "ms");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Test for the case where the context item is not the root
     */

    @Test
    public void testContextNotRoot() {
        try {
            Processor proc = new Processor(false);
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
            proc.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
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

            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.applyTemplates(node, serializer);

            fail("Transformation should fail");

        } catch (SaxonApiException e) {
            int line = ((XPathException) e.getCause()).getLocator().getLineNumber();
            assertTrue(line >= 2 && line <= 6, "lineNumber");
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


            XsltCompiler xqcomp = proc.newXsltCompiler();
            xqcomp.setErrorListener(listener);

            Xslt30Transformer trans = xqcomp.compile(new StreamSource((new File(configTest.getDataDir() + "main.xsl")))).load30();
            trans.setErrorListener(listener);
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

            XsltCompiler compiler = processor.newXsltCompiler();

            String d = configTest.getDataDir() + "trax/";

            String foo_xml = d + "xml/foo.xml";
            Source sources =
                    compiler.getAssociatedStylesheet(new StreamSource(new File(foo_xml).toURI().toASCIIString()), null,
                                                     null, null);


            Xslt30Transformer transformer = compiler.compile(sources).load30();

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
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                                       "<xsl:template match='/'>///<xsl:apply-templates/></xsl:template>" +
                                                       "<xsl:template match='*'>***<xsl:apply-templates/></xsl:template>" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
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
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                                       "<xsl:template name='go'><a/></xsl:template>" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
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
     * Send output to an XdmDestination with item separators
     */

    @Test
    public void testXdmDestinationWithItemSeparator() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:template name='go'><xsl:comment>A</xsl:comment><out/><xsl:comment>Z</xsl:comment></xsl:template>" +
                            "<xsl:output method='xml' item-separator='§'/>" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            XdmDestination out = new XdmDestination();
            xsl.callTemplate(new QName("go"), out);
            XdmNode root = out.getXdmNode();
            assertEquals("<!--A-->§<out/>§<!--Z-->", root.toString());
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
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                                       "<xsl:template name='go'><a/></xsl:template>" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
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
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                                       "<xsl:template name='go'><a/></xsl:template>" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
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
            proc.setConfigurationProperty(Feature.TIMING, true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:my='java:s9apitest.TestXslt30Transformer' exclude-result-prefixes='my'>" +
                                                       "<xsl:template match='*'><xsl:copy><xsl:copy-of select='@*'/><xsl:apply-templates/></xsl:copy></xsl:template>\n" +
                                                       "<xsl:template match='*/*/*/*[my:isOptional(.., node-name(.))]'/> \n" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();

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
     * type of parent.
     */

    public static boolean isOptional(NodeInfo parent, QNameValue childName) throws Exception {
        Configuration config = parent.getConfiguration();
        SchemaType parentType = parent.getSchemaType();
        int fp = config.getNamePool().allocateFingerprint(
                NamespaceUri.of(childName.getComponent(AccessorFn.Component.NAMESPACE).getStringValue()),
                                childName.getComponent(AccessorFn.Component.LOCALNAME).getStringValue());
        int cardinality = ((ComplexType) parentType).getElementParticleCardinality(fp, true);
        return Cardinality.allowsZero(cardinality);
    }

    /**
     * s9api transformation pipeline
     */

    @Test
    public void testPipeline() {
        try {
            Processor proc = new Processor(false);
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
     * s9api streaming transformation pipeline
     */

    @Test
    public void testPipeline2() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();

            String xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                    "</xsl:stylesheet>";
            String xml = "<z/>";
            Xslt30Transformer stage1 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage2 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage3 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage4 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage5 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);
            stage1.applyTemplates(new StreamSource(new StringReader(xml)),
                                  stage2.asDocumentDestination(
                                          stage3.asDocumentDestination(
                                                  stage4.asDocumentDestination(
                                                          stage5.asDocumentDestination(out)))));
            System.err.println(sw.toString());
            assertTrue(sw.toString().contains("<a><a><a><a><a><z/></a></a></a></a></a>"), "output");

        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    @Test
    public void testPipelineShort() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();

            String xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                    "</xsl:stylesheet>";
            String xml = "<z/>";
            Xslt30Transformer stage1 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage2 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            stage1.applyTemplates(new StreamSource(new StringReader(xml)),
                                  stage2.asDocumentDestination(out));
            System.err.println(sw);
            assertTrue(sw.toString().contains("<a><a><z/></a></a>"), "output");

        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    /**
     * Test location information for run-time errors when stylesheet built as an XdmNode
     */

    @Test
    public void testErrorNotificationFromSaxonParse() {
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
            // Norm Walsh reported saxon:parse errors not notified to the ErrorListener. Can't reproduce.

            Processor proc = new Processor(true);

            XsltCompiler xqcomp = proc.newXsltCompiler();
            xqcomp.setErrorListener(listener);

            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' " +
                                                       "xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:saxon='http://saxon.sf.net/'>\n" +
                                                       "<xsl:template name='main'>\n" +
                                                       "<xsl:copy-of select=\"saxon:parse('&lt;doc>&lt;e>text&lt;/f>&lt;/doc>')\"/>\n" +
                                                       "</xsl:template>\n" +
                                                       "</xsl:stylesheet>");


            Xslt30Transformer trans = xqcomp.compile(new StreamSource(sr)).load30();
            trans.setErrorListener(listener);

            Serializer serializer = proc.newSerializer();
            ByteArrayOutputStream os = new ByteArrayOutputStream();
            serializer.setOutputStream(os);

            trans.callTemplate(new QName("main"), serializer);

            fail("Transformation should fail");

        } catch (SaxonApiException e) {
            assertTrue(warnings.isEmpty(), "warnings");
            assertTrue(errors.isEmpty(), "errors");
            assertTrue(fatals.size() == 1, "fatals");
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

    /**
     * Test option to treat validation failures as non-fatal
     */

    @Test
    public void testValidationWarnings() {
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.VALIDATION_WARNINGS, true);
            processor.getUnderlyingConfiguration().addSchemaSource(
                    new StreamSource(configTest.getDataDir() + "books.xsd"));

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

            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setDTDValidation(false);
            builder.setLineNumbering(true);

            XdmNode document = builder.build(new File(configTest.getDataDir() + "books.xml"));
            XdmNode stylesheet = builder.build(new File(configTest.getDataDir() + "books.xsl"));

            XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setSchemaAware(processor.isSchemaAware());
            XsltExecutable exec = compiler.compile(stylesheet.asSource());
            Xslt30Transformer transformer = exec.load30();

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
     * @throws net.sf.saxon.s9api.SaxonApiException
     */

    @Test
    public void testGlobalParameters() throws SaxonApiException, UnsupportedEncodingException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
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
     *
     * @throws SaxonApiException
     * @throws UnsupportedEncodingException
     */

    @Test
    public void testGlobalParameterTypes() throws SaxonApiException, UnsupportedEncodingException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
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
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testCallFunction() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                xmlns:f='http://localhost/'" +
                "                version='3.0'>" +
                "                <xsl:function name='f:add' visibility='public'>" +
                "                  <xsl:param name='a'/><xsl:param name='b'/>" +
                "                  <xsl:sequence select='$a + $b'/></xsl:function>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        XdmValue v = t.callFunction(new QName("http://localhost/", "f:add"), new XdmValue[]{new XdmAtomicValue(2), new XdmAtomicValue(3)});
        assertEquals(((XdmAtomicValue) v).getLongValue(), 5);
    }

    /**
     * Test an external  call on a stylesheet function with argument conversion
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testCallFunctionArgConversion() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                xmlns:f='http://localhost/'" +
                "                version='3.0'>" +
                "                <xsl:function name='f:add' visibility='public'>" +
                "                   <xsl:param name='a' as='xs:double'/>" +
                "                   <xsl:param name='b' as='xs:double'/>" +
                "                   <xsl:sequence select='$a + $b'/>" +
                "                </xsl:function>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        XdmValue v = t.callFunction(new QName("http://localhost/", "f:add"), new XdmValue[]{new XdmAtomicValue(2), new XdmAtomicValue(3)});
        assertEquals(((XdmAtomicValue) v).getDoubleValue(), 5.0e0);
        assertEquals(((XdmAtomicValue) v).getPrimitiveTypeName().getLocalName(), "double");
    }

    /**
     * Test an external  call on a stylesheet function serializing the results
     * Also makes access to a stylesheet parameter which in turn accesses the global context item
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testCallFunctionWrapResults() throws SaxonApiException {
        Processor processor = new Processor(true);
        processor.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                xmlns:f='http://localhost/'" +
                "                version='3.0'>" +
                "                <xsl:param name='x' as='xs:integer'/>" +
                "                <xsl:param name='y' select='.+2'/>" +
                "                <xsl:function name='f:add' visibility='public'>" +
                "                   <xsl:param name='a' as='xs:double'/>" +
                "                   <xsl:param name='b' as='xs:double'/>" +
                "                   <xsl:sequence select='$a + $b + $x + $y'/>" +
                "                </xsl:function>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        t.setGlobalContextItem(new XdmAtomicValue(20));
        Map<QName, XdmValue> globals = new HashMap<QName, XdmValue>();
        globals.put(new QName("x"), new XdmAtomicValue(30));
        t.setStylesheetParameters(globals);
        StringWriter sw = new StringWriter();
        Serializer s = processor.newSerializer();
        s.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
        s.setOutputWriter(sw);
        t.callFunction(new QName("http://localhost/", "f:add"), new XdmValue[]{new XdmAtomicValue(2), new XdmAtomicValue(3)}, s);
        assertEquals("57", sw.toString());
    }


    /**
     * Test an external  call on a stylesheet function with invalid argument conversion
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testCallFunctionArgInvalid() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            Processor processor = new Processor(configuration);
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
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testCallNamedTemplateWithParams() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
        Processor processor = new Processor(configuration);
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
        Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
        params.put(new QName("a"), new XdmAtomicValue(12));
        params.put(new QName("b"), new XdmAtomicValue(5));
        t.setInitialTemplateParameters(params, false);
        StringWriter sw = new StringWriter();
        Serializer se = processor.newSerializer(sw);
        se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
        t.callTemplate(new QName("t"), se);
        assertEquals("17", sw.toString(), "output");
    }

    /**
     * Test calling a stylesheet with an initial template omitting a required parameters
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testCallNamedTemplateWithParamsRequired() throws SaxonApiException {
        try {
            Configuration configuration = Configuration.newConfiguration();
            //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
            Processor processor = new Processor(configuration);
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
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testCallNamedTemplateWithTunnelParams() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
        Processor processor = new Processor(configuration);
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
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testCallTemplateRuleWithParams() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
        Processor processor = new Processor(configuration);
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
     * Test calling a stylesheet with an initial template rule with parameters
     */

    @Test
    public void testCallTemplateWithResultValidation() {
        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            xsltCompiler.setSchemaAware(true);

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0' exclude-result-prefixes='#all'>" +
                    "                <xsl:import-schema><xs:schema><xs:element name='x' type='xs:int'/></xs:schema></xsl:import-schema>" +
                    "                <xsl:template name='main'>" +
                    "                   <xsl:result-document validation='strict'>" +
                    "                     <x>3</x>" +
                    "                   </xsl:result-document>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.callTemplate(new QName("main"), se);
            assertEquals("<x>3</x>", sw.toString(), "output");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testCallTemplateWithResultValidationFailure() {
        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            xsltCompiler.setSchemaAware(true);

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0' expand-text='yes' exclude-result-prefixes='#all'>" +
                    "                <xsl:import-schema><xs:schema><xs:element name='x' type='xs:int'/></xs:schema></xsl:import-schema>" +
                    "                <xsl:param name='p'>zzz</xsl:param>" +
                    "                <xsl:template name='main'>" +
                    "                   <xsl:result-document validation='strict'>" +
                    "                     <x>{$p}</x>" +
                    "                   </xsl:result-document>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.callTemplate(new QName("main"), se);
            fail("unexpected success");
        } catch (SaxonApiException e) {
            System.err.println("Failed as expected: " + e.getMessage());
        }
    }

    @Test
    public void testCallTemplateWithResultValidationAndItemSeparator() throws SaxonApiException {
        // The presence of the item separator in the output makes the document invalid
        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            xsltCompiler.setSchemaAware(true);

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0' expand-text='yes' exclude-result-prefixes='#all'>" +
                    "                <xsl:import-schema><xs:schema><xs:element name='x' type='xs:int'/></xs:schema></xsl:import-schema>" +
                    "                <xsl:param name='p'>3</xsl:param>" +
                    "                <xsl:template name='main'>" +
                    "                   <xsl:result-document validation='strict' item-separator='|'>" +
                    "                     <xsl:comment>start</xsl:comment><x>{$p}</x>" +
                    "                   </xsl:result-document>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.setResultDocumentHandler(uri -> se);
            t.callTemplate(new QName("main"), se);
            fail("unexpected success");
        } catch (SaxonApiException e) {
            assertEquals("XTTE1550", e.getErrorCode().getLocalName());
        }
    }

    @Test
    public void testCallTemplateWithSecondaryResultValidationAndItemSeparator() throws SaxonApiException {
        // The presence of the item separator in the output makes the document invalid
        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            xsltCompiler.setSchemaAware(true);

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0' expand-text='yes' exclude-result-prefixes='#all'>" +
                    "                <xsl:import-schema><xs:schema><xs:element name='x' type='xs:int'/></xs:schema></xsl:import-schema>" +
                    "                <xsl:param name='p'>3</xsl:param>" +
                    "                <xsl:template name='main'>" +
                    "                   <xsl:result-document href='test-out.xml' validation='strict' item-separator='|'>" +
                    "                     <xsl:comment>start</xsl:comment><x>{$p}</x>" +
                    "                   </xsl:result-document>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            t.setBaseOutputURI(new File(configTest.getDataDir() + "sandpit/dummy.xml").toURI().toASCIIString());
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.callTemplate(new QName("main"), se);
            fail("unexpected success");
        } catch (SaxonApiException e) {
            assertEquals("XTTE1550", e.getErrorCode().getLocalName());
        }
    }

    /**
     * Raw output from named template
     */

    @Test
    public void testCallTemplateNoParamsRaw() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                                       "<xsl:template name='xsl:initial-template'><xsl:sequence select='42'/></xsl:template>" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            XdmValue result = xsl.callTemplate(null);
            assertTrue(result instanceof XdmAtomicValue, "result is atomic value");
            assertEquals(42, ((XdmAtomicValue) result).getLongValue(), "result is 42");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


    /**
     * Test calling a stylesheet named template getting raw results back
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testCallNamedTemplateWithParamsRaw() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
        Processor processor = new Processor(configuration);
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
     * Test calling a stylesheet named template getting raw results back as a Destination
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testCallNamedTemplateWithParamsRawDestination() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
        Processor processor = new Processor(configuration);
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
        Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
        params.put(new QName("a"), new XdmAtomicValue(12));
        params.put(new QName("b"), new XdmAtomicValue(5));
        t.setInitialTemplateParameters(params, false);
        RawDestination out = new RawDestination();
        t.callTemplate(new QName("t"), out);
        XdmValue val = out.getXdmValue();
        assertEquals(2, val.size());
        assertEquals(13, ((XdmAtomicValue) val.itemAt(0)).getLongValue());
        assertEquals(6, ((XdmAtomicValue) val.itemAt(1)).getLongValue());
    }


    /**
     * Test calling a stylesheet with an initial template rule getting the raw result
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testApplyTemplatesRaw() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
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
        Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
        params.put(new QName("a"), new XdmAtomicValue(12));
        params.put(new QName("b"), new XdmAtomicValue(5));
        t.setInitialTemplateParameters(params, false);
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
     * Test calling a stylesheet with an initial template rule getting the serialized
     * result with item separators
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testApplyTemplatesToSerializer() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                version='3.0'>" +
                "                <xsl:output method='text' item-separator='~~'/>" +
                "                <xsl:template match='.'>" +
                "                   <xsl:param name='a' as='xs:double'/>" +
                "                   <xsl:param name='b' as='xs:float'/>" +
                "                   <xsl:sequence select='., $a + $b'/>" +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        Map<QName, XdmValue> params = new HashMap<QName, XdmValue>();
        params.put(new QName("a"), new XdmAtomicValue(12));
        params.put(new QName("b"), new XdmAtomicValue(5));
        t.setInitialTemplateParameters(params, false);
        StringWriter sw = new StringWriter();
        Serializer se = processor.newSerializer(sw);
        se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
        t.applyTemplates(new XdmAtomicValue(16), se);
        assertEquals("16~~17", sw.toString());
    }


    /**
     * Test calling a stylesheet with an initial template rule getting the raw result
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testApplyTemplatesToXdm() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
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

    /**
     * Test Strip-space with ApplyTemplates
     */

    @Test
    public void testStripSpace() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader("<xslt:transform xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                                                       "                xmlns:my=\"http://ns.example.com/strip-type\"\n" +
                                                       "                xmlns:xslt=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                                                       "                exclude-result-prefixes=\"xs my\"\n" +
                                                       "                version=\"3.0\">\n" +
                                                       "   <xslt:strip-space elements='foo my:foo'/>\n" +
                                                       "   <xslt:mode on-no-match='deep-copy'/>\n" +
                                                       "</xslt:transform>");

            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            String in = "<?xml version='1.0'?> \n" +
                    "<my:userNode xmlns:my='http://ns.example.com/strip-type'><foo> </foo><bar> </bar><my:foo> </my:foo><my:bar> </my:bar></my:userNode>";

            DocumentBuilder builder = proc.newDocumentBuilder();
            StreamSource sDoc = new StreamSource(new ByteArrayInputStream(in.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
            XdmNode doc = builder.build(sDoc);

            StringWriter writer = new StringWriter();
            Serializer out = proc.newSerializer(writer);
            xsl.applyTemplates(doc, out);
            assertTrue(writer.toString().contains("<foo/>"), "strip-space not working");
            assertTrue(writer.toString().contains("<my:foo/>"), "strip-space not working");
            assertFalse(writer.toString().contains("<bar/>"), "strip-space not working");
            assertFalse(writer.toString().contains("<my:bar/>"), "strip-space not working");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testResultDocument() {
        // bug 2771
        try {
            String xsl = "<xsl:stylesheet version=\"3.0\" \n" +
                    "  xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "\n" +
                    "  <xsl:template match='a'>\n" +
                    "    <c>d</c>\n" +
                    "  </xsl:template>\n" +
                    "\n" +
                    "  <xsl:template match='whatever'>\n" +
                    "    <xsl:result-document href='out.xml'>\n" +
                    "      <e>f</e>\n" +
                    "    </xsl:result-document>\n" +
                    "  </xsl:template>\n" +
                    "\n" +
                    "</xsl:stylesheet>";
            Processor proc = new Processor(false);
            XsltCompiler compiler = proc.newXsltCompiler();
            Xslt30Transformer transformer = compiler.compile(new StreamSource(new StringReader(xsl))).load30();
            StreamSource source = new StreamSource(new StringReader("<a>b</a>"));
            XdmValue xdmValue = transformer.applyTemplates(source);
            assertEquals(1, xdmValue.size(), "size");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Test saxon:next-in-chain
     */

    @Test
    public void testNextInChain() {
        // Create a Processor instance.
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.TIMING, true);

            processor.getUnderlyingConfiguration().setOutputURIResolver(new StandardOutputResolver() {
                StringWriter writer;

                @Override
                public Result resolve(String href, String base) throws XPathException {
                    writer = new StringWriter();
                    return new StreamResult(writer);
                }

                @Override
                public void close(Result result) throws XPathException {
                    System.err.println("Checking secondary result");
                    assertTrue(writer.toString().equals("<second>secondary</second>"));
                }
            });

            // Create a transformer for the stylesheet.
            Xslt30Transformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new File(configTest.getDataDir() + "next-in-chain/main.xsl"))).load30();

            // Create a destination
            Serializer out = processor.newSerializer();
            StringWriter sw = new StringWriter();
            out.setOutputWriter(sw);

            // Load the source document
            XdmNode input = processor.newDocumentBuilder().build(
                    new File(configTest.getDataDir() + "next-in-chain/test.xml"));
            // Set the root node of the source document to be the initial context node
            transformer.applyTemplates(input, out);
            assertTrue(sw.toString().equals("<second>principal</second>"));

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Test relative URI resolver - bug 4643
     */

    @Test
    public void testRelativeURIResolver() {
        // Create a Processor instance.
        try {
            String xsl = "<xsl:package name='myPack' package-version='1.0.0' version=\"3.0\" \n" +
                    "  xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "  <xsl:mode/>\n" +
                    "  <xsl:template match='/'>\n" +
                    "    <c><xsl:if test=\"doc-available('test.xml')\"><xsl:copy-of select=\"doc('test.xml')\"/></xsl:if></c>\n" +
                    "  </xsl:template>\n" +
                    "</xsl:package>";

            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.TIMING, true);

            // Create a transformer for the stylesheet.
            Xslt30Transformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new StringReader(xsl))).load30();

            // Add a relative URI resolver
            transformer.setURIResolver(new RelativeURIResolver() {
                @Override
                public String makeAbsolute(String href, String base) throws TransformerException {
                    return href;
                }

                @Override
                public Source dereference(String uri) throws TransformerException {
                    return uri.equals("test.xml") ? new StreamSource(new StringReader("<widget/>"), "http://dummy/") : null;
                }

                @Override
                public Source resolve(String href, String base) throws TransformerException {
                    return dereference(makeAbsolute(href, base));
                }
            });
            // Create a destination
            Serializer out = processor.newSerializer();
            StringWriter sw = new StringWriter();
            out.setOutputWriter(sw);

            // Load the source document
            XdmNode input = processor.newDocumentBuilder().build(
                    new File(configTest.getDataDir() + "next-in-chain/test.xml"));
            // Set the root node of the source document to be the initial context node
            transformer.applyTemplates(input, out);
            assertTrue(sw.toString().contains("<c><widget/></c>"));

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


    @Test
    public void testTraceListenerTemplates() {
        // Bug 3472
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setCompileWithTracing(true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:my='java:s9apitest.TestXslt30Transformer' exclude-result-prefixes='my'>" +
                            "<xsl:template match='a'><a><xsl:apply-templates/></a></xsl:template>\n" +
                            "<xsl:template match='b'/></xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();
            List<String> log = new ArrayList<>();
            transformer.setTraceListener(new TraceListener() {
                @Override
                public void setOutputDestination(Logger stream) {

                }

                @Override
                public void open(Controller controller) {
                    log.add("open");
                }

                @Override
                public void close() {
                    log.add("close");
                }

                @Override
                public void enter(Traceable instruction, Map<String, Object> properties, XPathContext context) {
                    log.add("enter " + instruction.getClass());
                    if (instruction instanceof TemplateRule) {
                        log.add("matching " + properties.get("match"));
                    }
                }

                @Override
                public void leave(Traceable instruction) {
                    log.add("leave " + instruction.getClass());
                }

                @Override
                public void startCurrentItem(Item currentItem) {
                    log.add("start " + (currentItem instanceof NodeInfo ? ((NodeInfo) currentItem).getLocalPart() : currentItem.getUnicodeStringValue()));
                }

                @Override
                public void endCurrentItem(Item currentItem) {
                    log.add("end " + (currentItem instanceof NodeInfo ? ((NodeInfo) currentItem).getLocalPart() : currentItem.getUnicodeStringValue()));
                }
            });
            String xml = "<a><b>stuff</b></a>";
            StringWriter sw = new StringWriter();
            Serializer serializer = proc.newSerializer(sw);
            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            transformer.applyTemplates(new StreamSource(new StringReader(xml)), serializer);
            assertTrue(log.size() > 10);
            assertTrue(log.contains("matching a"));
            assertTrue(log.contains("matching b"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


    /**
     * Test result document handler
     */

    @Test
    public void testResultDocHandler() throws SaxonApiException {
        Processor processor = new Processor(false);

        String xsl = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                version='3.0'>" +
                "                <xsl:template name='main'>" +
                "                   <xsl:for-each select='5 to 10'> " +
                "                      <xsl:result-document href='out{position()}.xml'>" +
                "                          <out nr='{.}'/>" +
                "                      </xsl:result-document>" +
                "                   </xsl:for-each>" +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";


        XsltCompiler compiler = processor.newXsltCompiler();
        compiler.setSchemaAware(false);
        XsltExecutable exec = compiler.compile(new StreamSource(new StringReader(xsl)));
        Xslt30Transformer transformer = exec.load30();
        transformer.setBaseOutputURI("http://base.output.com/");
        final Map<String, XdmDestination> results = new HashMap<>();
        transformer.setResultDocumentHandler(uri -> {
            XdmDestination dest = new XdmDestination();
            dest.setDestinationBaseURI(uri);
            assertTrue(uri.toASCIIString().startsWith("http://base.output.com/"));
            String href = uri.toASCIIString().replace("http://base.output.com/", "");
            results.put(href, dest);
            return dest;
        });
        XdmDestination result = new XdmDestination();
        transformer.callTemplate(new QName("main"), result);

        assertEquals(6, results.size());
        assertNotNull(results.get("out1.xml"));
        System.err.println(results.get("out1.xml").getXdmNode().toString());
        assertTrue(results.get("out1.xml").getXdmNode().toString().contains("<out nr=\"5\"/>"));
        assertEquals("http://base.output.com/out1.xml", results.get("out1.xml").getXdmNode().getBaseURI().toString(), "base-uri");
    }
    

    /**
     * Test result document handler with raw destination
     */

    public void testResultDocHandlerRaw() throws SaxonApiException {
        Processor processor = new Processor(false);

        String xsl = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                version='3.0'>" +
                "                <xsl:template name='main'>" +
                "                   <xsl:for-each select='5 to 10'> " +
                "                      <xsl:result-document href='out{position()}.xml' build-tree='no'>" +
                "                          <xsl:sequence select='. to .+10'/>" +
                "                      </xsl:result-document>" +
                "                   </xsl:for-each>" +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";


        XsltCompiler compiler = processor.newXsltCompiler();
        compiler.setSchemaAware(false);
        XsltExecutable exec = compiler.compile(new StreamSource(new StringReader(xsl)));
        Xslt30Transformer transformer = exec.load30();
        transformer.setBaseOutputURI("http://base.output.com/");
        final Map<String, RawDestination> results = new HashMap<>();
        transformer.setResultDocumentHandler(uri -> {
            RawDestination dest = new RawDestination();
            assertTrue(uri.toASCIIString().startsWith("http://base.output.com/"));
            String href = uri.toASCIIString().replace("http://base.output.com/", "");
            results.put(href, dest);
            return dest;
        });
        XdmDestination result = new XdmDestination();
        transformer.callTemplate(new QName("main"), result);

        assertEquals(6, results.size());
        assertNotNull(results.get("out1.xml"));
        System.err.println(results.get("out1.xml").getXdmValue().toString());
        assertEquals(11, results.get("out1.xml").getXdmValue().size(), "size");
        assertEquals(5, ((XdmAtomicValue) results.get("out1.xml").getXdmValue().itemAt(0)).getLongValue(), "value");
    }

    /**
     * Send output to an XdmDestination
     */

    @Test
    public void testItemSeparatorToSerializer() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "  <xsl:template name='go'>"
                            + "<xsl:comment>start</xsl:comment><a><b/><c/></a><xsl:comment>end</xsl:comment>"
                            + "</xsl:template>" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.METHOD, "xml");
            out.setOutputProperty(Serializer.Property.INDENT, "no");
            out.setOutputProperty(Serializer.Property.ITEM_SEPARATOR, "+++");
            xsl.callTemplate(new QName("go"), out);
            System.err.println(sw.toString());
            assertTrue(sw.toString().contains("<!--start-->+++"));
            assertTrue(sw.toString().contains("+++<!--end-->"));
            assertTrue(sw.toString().contains("<a><b/><c/></a>"));
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testTransformReturningFunction() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xsl:template name='main'><xsl:sequence select='f:f#1'/></xsl:template> \n" +
                            "<xsl:function name='f:f' as='xs:integer'><xsl:param name='p'/><xsl:sequence select='$p+42'/></xsl:function> \n" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();
            XdmValue value = transformer.callTemplate(new QName("main"));
            XdmValue result = ((XdmFunctionItem) value).call(proc, new XdmAtomicValue(17));
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals(59, ((XdmAtomicValue) result).getLongValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testJarFileInput() {
        String xsl = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>"
                + "   <xsl:template match=\"/\">\n"
                + "    <html>\n"
                + "      <xsl:comment><xsl:copy-of select=\"doc('jar:file:" + configTest.getDataDir() + "temp/booksCopy.zip!/books.xml')\"/></xsl:comment>\n"
                + "\n"
                + "      <head>\n"
                + "        <xsl:attribute name=\"title\" select=\"current-dateTime()\"/>\n"
                + "      </head>\n"
                + "\n"
                + "      <body leftmargin=\"100\" >\n"
                + "        <xsl:apply-templates/>\n"
                + "      </body>\n"
                + "    </html>\n"
                + "  </xsl:template>"
                + "</xsl:transform>";
        try {
            File input = new File(configTest.getDataDir() + "books.xml");
            File inputCopy = new File(configTest.getDataDir() + "temp/booksCopy.zip");
            ZipOutputStream out = new ZipOutputStream(new FileOutputStream(inputCopy));
            ZipEntry e = new ZipEntry("books.xml");
            out.putNextEntry(e);

            byte[] data = BinaryResource.readBinaryFromStream(new FileInputStream(input), "books.xml");
            out.write(data, 0, data.length);
            out.closeEntry();

            out.close();

            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            XsltExecutable exp = comp.compile(new StreamSource(new StringReader(xsl), configTest.getDataDir()));
            XdmNode source = proc.newDocumentBuilder().build(new StreamSource(new File(configTest.getDataDir() + "books.xml")));
            StringWriter sw = new StringWriter();
            Serializer ser = proc.newSerializer(sw);
            ser.setOutputProperty(Serializer.Property.METHOD, "html");
            ser.setOutputProperty(Serializer.Property.INDENT, "yes");
            XsltTransformer trans = exp.load();
            trans.setInitialContextNode(source);
            trans.setDestination(ser);
            trans.transform();

            System.out.println("Output written to books.html");
            System.out.println("File delete (booksCopy.zip): " + inputCopy.delete());

            System.out.println("complete");
            //assertTrue(result instanceof XdmAtomicValue);
            //assertEquals(59, ((XdmAtomicValue) result).getLongValue());
        } catch (SaxonApiException | IOException | XPathException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    /**
     * Test an external  call on a stylesheet function with argument conversion
     *
     * @throws net.sf.saxon.s9api.SaxonApiException if the call fails
     */

    @Test
    public void testSequenceResult() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        Processor processor = new Processor(configuration);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                version='3.0'>" +
                "                <xsl:template name='xsl:initial-template' as='xs:integer+'>" +
                "                     <xsl:sequence select='(1 to 5)' />        " +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        XdmValue res = t.callTemplate(null);
        int count = res.size();
        XdmAtomicValue value = (XdmAtomicValue) res.itemAt(0);
    }

    /**
     * Load and run a SEF file that specifies relocatable="yes" and calls the doc() function
     */

    @Test
    public void testRelocatableExport() {
        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            XsltExecutable executable = xsltCompiler.compile(new StreamSource(new File(configTest.getDataDir() + "booksWithMain10.sef")));

            Xslt30Transformer transformer = executable.load30();

            Serializer dest = processor.newSerializer();
            dest.setOutputFile(new File(configTest.getDataDir() + "/sandpit/OutputStep2.xml"));

            transformer.callTemplate(new QName("main"), dest);


        } catch (Exception e) {
            e.printStackTrace();
            fail();

        }

    }

    @Test
    public void testDiagnosticsOnAtomizationFailureSansExport() {
        testDiagnosticsOnAtomizationFailure(false);
    }

    @Test
    public void testDiagnosticsOnAtomizationFailureWithExport() {
        testDiagnosticsOnAtomizationFailure(true);
    }

    private void testDiagnosticsOnAtomizationFailure(boolean export) {
        // Bug 4086

        String xsl = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                xmlns:f = 'http://functions/' " +
                "                version='3.0'>" +
                "                <xsl:param name='p' select='map{2:5}'/>" +
                "                <xsl:template name='main' as='xs:integer+'>" +
                "                     <xsl:sequence select=\"f:do($p)\" />        " +
                "                </xsl:template>" +
                "                <xsl:function name='f:do' as='xs:integer+'>" +
                "                     <xsl:param name='x' as='xs:integer'/>" +
                "                     <xsl:sequence select='$x+3' />        " +
                "                </xsl:function>" +
                "                </xsl:stylesheet>";

        try {
            Processor p = new Processor(true);
            XsltCompiler c = p.newXsltCompiler();
            XsltExecutable exec = c.compile(new StreamSource(new StringReader(xsl)));
            if (export) {
                ByteArrayOutputStream baos = new ByteArrayOutputStream();
                exec.export(baos);
                try {
                    baos.close();
                } catch (IOException e) {
                    fail();
                }
                ByteArrayInputStream bais = new ByteArrayInputStream(baos.toByteArray());
                Processor p2 = new Processor(true);
                XsltCompiler c2 = p2.newXsltCompiler();
                exec = c2.loadLibraryPackage(new StreamSource(bais)).link();
            }
            XdmValue val = exec.load30().callTemplate(new QName("main"));
            fail();
        } catch (SaxonApiException e) {
            assertTrue(e.getMessage().contains("Found while atomizing the first argument of f:do()"));
            System.err.println(e);
        }
    }


    @Test
    public void test4301() {
        try {
            String s = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
                    + "       <xsl:template name='xsl:initial-template' as='xs:string'> "
                    + "           <xsl:param name='code' as='xs:string'/> "
                    + "           <xsl:sequence select=\"if ($code = ('test')) then $code else error(QName('http://www.ctk.cz/error-code/', 'NotExists'))\"/>"
                    + "        </xsl:template>"
                    + " </xsl:transform>";
            Processor processor = new Processor(true);
            XsltCompiler comp = processor.newXsltCompiler();
            Xslt30Transformer transformer = comp.compile(new StreamSource(new StringReader(s))).load30();
            Map<QName, XdmValue> p = new HashMap<>();
            p.put(new QName("code"), new XdmAtomicValue("abc"));
            transformer.setInitialTemplateParameters(p, false);
            XdmDestination dest = new XdmDestination();
            transformer.callTemplate(null, dest);
        } catch (SaxonApiException err) {
            if (err.getErrorCode().getNamespaceUri().toString().equals("http://www.ctk.cz/error-code/")) {
                //return;
            } else {
                err.printStackTrace();
                fail();
            }
        }
    }

    @Test
    public void test3929inlineSchema() {
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS, true);
            SchemaManager sm = processor.getSchemaManager();
            String xsd0 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>\n" +
                    "<xs:element name='a'/>\n" +
                    "</xs:schema>";
            sm.load(new StreamSource(new StringReader(xsd0)));
            String s = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
                    + "       <xsl:import-schema>"
                    + "        <xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>\n"
                    + "         <xs:element name='b' type='xs:integer'/>\n"
                    + "        </xs:schema>"
                    + "       </xsl:import-schema>"
                    + "       <xsl:template name='xsl:initial-template' as='xs:string'> "
                    + "           <b xmlns='ns:a'>12</b>"
                    + "        </xsl:template>"
                    + "        </xsl:transform>";
            XsltCompiler comp = processor.newXsltCompiler();
            Xslt30Transformer transformer = comp.compile(new StreamSource(new StringReader(s))).load30();
            XdmDestination dest = new XdmDestination();
            transformer.callTemplate(null, dest);
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }


    @Test
    public void test3929externalSchema() {
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.MULTIPLE_SCHEMA_IMPORTS, false);
            SchemaManager sm = processor.getSchemaManager();
            String xsd0 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>\n" +
                    "<xs:element name='a'/>\n" +
                    "</xs:schema>";
            String xsd1 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>\n" +
                    "<xs:element name='b'/>\n" +
                    "</xs:schema>";
            sm.load(new StreamSource(new StringReader(xsd0)));
            String s = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>"
                    + "       <xsl:import-schema namespace='ns:a' schema-location='ns:a'/>"
                    + "       <xsl:template name='xsl:initial-template' as='xs:string'> "
                    + "           <b xmlns='ns:a'>12</b>"
                    + "        </xsl:template>"
                    + "        </xsl:transform>";
            XsltCompiler comp = processor.newXsltCompiler();
            comp.setURIResolver(new URIResolver() {
                @Override
                public Source resolve(String href, String base) throws TransformerException {
                    if (href.equals("ns:a")) {
                        return new StreamSource(new StringReader(xsd1));
                    } else {
                        return null;
                    }
                }
            });
            Xslt30Transformer transformer = comp.compile(new StreamSource(new StringReader(s))).load30();
            XdmDestination dest = new XdmDestination();
            transformer.callTemplate(null, dest);
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }

    public void testBug4563() {

        try {
            XsltCompiler s_comp = null;
            XsltExecutable s_exp;
            Processor s_proc = new Processor(true);

            String dir = "/Users/mike/bugs/2020/4563-Fritz/repro/";
            String xslFilePath = dir + "resources/01632275/RemoveNSAtChildNodes.xsl";
            String xmlFilePath = dir + "resources/01632275/OK.xml";
            String xmlFilePath2 = dir + "resources/01632275/NOK.xml";

            if (s_comp == null) {// only saves 3ms on my test xsl
                s_comp = s_proc.newXsltCompiler();
            }

            String xml = "";

            Path path = Paths.get(xmlFilePath);
            String xml1 = new String(Files.readAllBytes(path));

            path = Paths.get(xmlFilePath2);
            String xml2 = new String(Files.readAllBytes(path));

            s_exp = s_comp.compile(new StreamSource(new File(xslFilePath)));

            // reuse the transformer which seems to be allowed according to javadoc
            // https://www.saxonica.com/html/documentation/javadoc/net/sf/saxon/s9api/Xslt30Transformer.html
            Xslt30Transformer trans = s_exp.load30();

            for (int i = 0; i < 3; i++) {

                // will cause all output be without any namespace --> WRONG
                // if(i==0) {

                // will cause all output 1 and 3 to be with namespace on root node --> CORRECT
                // if(i==1) {

                if (i == 0) {
                    xml = xml2;
                } else {
                    xml = xml1;
                }

                final StringWriter result = new StringWriter();
                Serializer out = s_proc.newSerializer(result);
                DocumentBuilder builder = s_proc.newDocumentBuilder();
                XdmNode node = builder.build(new StreamSource(new StringReader(xml)));
                trans.setGlobalContextItem(node); // should succeed the first time, fail the second
                if (i > 0) {
                    fail("Resetting the global context item isn't allowed; error not detected");
                }
                trans.applyTemplates(node, out);
                String xmlOut = result.toString();

                System.out.println("\n" + xmlOut);
            }
        } catch (IllegalStateException e) {
            assertEquals("Stylesheet has already been evaluated", e.getMessage());
        } catch (IOException | SaxonApiException e) {
            fail();
        }
    }

   @Test
    public void TestBug5341() {
       try {
           String xslt = "<xsl:stylesheet \n"
                   + "         xmlns:xsl='http://www.w3.org/1999/XSL/Transform' \n"
                   + "         version='3.0'>\n"
                   + "\n"
                   + "            <xsl:template match = '/'>\n"
                   + "                <xsl:sequence select = \"substring-after(/eventItem/contentMeta/scheduled/start, 'T')\"/>\n"
                   + "            </xsl:template>\n"
                   + "    </xsl:stylesheet>";


           Processor processor = new Processor(true);
           XsltCompiler xsltCompiler = processor.newXsltCompiler();
           xsltCompiler.setXsltLanguageVersion ("3.0");
           StringReader rdr = new StringReader(xslt);
           XsltExecutable exec = xsltCompiler.compile(new StreamSource(rdr));
           Xslt30Transformer transf = exec.load30();
           StringWriter sw = new StringWriter();
           Serializer ser = processor.newSerializer(sw);
           StreamSource stream = new StreamSource(new File(configTest.getDataDir() + "cirkev.xml"));

           transf.applyTemplates(stream, ser);
           assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?>10:00", sw.toString());
       } catch (SaxonApiException e) {
           fail();
       }
   }


}



