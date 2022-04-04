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
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.lib.TraceListener;
import net.sf.saxon.om.Item;
import net.sf.saxon.option.jdom2.JDOM2ObjectModel;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trace.Traceable;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.*;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertSame;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the s9api Xslt30Transformer in streaming mode
 *
 */
public class TestXslt30Streaming {
    private final ConfigTest configTest = new ConfigTest();
    
    /**
     * Simple test
     */

    public void testSimple() {
        // Create a Processor instance.
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.TIMING, true);

            // Create a transformer for the stylesheet.
            Xslt30Transformer transformer = processor.newXsltCompiler().compile(
                    new StreamSource(new File(configTest.getDataDir() + "caffo/Transform2Streamable.xslt"))).load30();

            // Create a destination
            XdmDestination serializer = new XdmDestination();
            long start = new Date().getTime();

            // Transform the source XML to System.out.
            for (int i = 0; i < 10; i++) {
                // Load the source document
                Source input = new StreamSource(
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
     * Send output to a Serializer
     */

    @Test
    public void testToSerializer() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.TIMING, true);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                       "<xsl:mode streamable='true'/>" +
                       "<xsl:template match='/'><a count='{count(//*)}'/></xsl:template>" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            Source input = new StreamSource(
                    new File(configTest.getDataDir() + "caffo/Input.xml"));
            xsl.applyTemplates(input, out);
            assertEquals("<a count=\"1634\"/>", sw.toString(), "result");
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
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.TIMING, true);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:mode streamable='1'/>" +
                            "<xsl:template match='/'><xsl:comment>A</xsl:comment><out/><xsl:comment>Z</xsl:comment></xsl:template>" +
                            "<xsl:output method='xml' item-separator='§'/>" +
                                                       "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            Source input = new StreamSource(
                    new File(configTest.getDataDir() + "caffo/Input.xml"));
            XdmDestination out = new XdmDestination();
            xsl.applyTemplates(input, out);
            XdmNode root = out.getXdmNode();
            assertEquals("<!--A-->§<out/>§<!--Z-->", root.toString());
            assertSame(root.getNodeKind(), XdmNodeKind.DOCUMENT, "result is document node");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Test running two streaming transformations in parallel
     */

    @Test
    public void testForked() {
        try {
            Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.TIMING, true);

            // Create two transformers for the stylesheet.
            XsltExecutable exec = processor.newXsltCompiler().compile(
                    new StreamSource(new File(configTest.getDataDir() + "caffo/Transform2Streamable.xslt")));
            StringWriter sw1 = new StringWriter();
            StringWriter sw2 = new StringWriter();
            Destination d1 = exec.load30().asDocumentDestination(processor.newSerializer(sw1));
            Destination d2 = exec.load30().asDocumentDestination(processor.newSerializer(sw2));
            processor.newDocumentBuilder().parse(new File(configTest.getDataDir() + "caffo/Input.xml"), new TeeDestination(d1, d2));
            assertEquals(sw1.toString(), sw2.toString(), "equal");

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


    /**
     * Send output of streamed transformation to a JDOM2 tree
     */

    @Test
    public void testJDOM2Destination() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:mode streamable='yes'/>" +
                    "<xsl:template match='/'><a size='{count(//*)}'/></xsl:template>" +
                    "</xsl:stylesheet>");
            Source input = new StreamSource(
                    new File(configTest.getDataDir() + "caffo/Input.xml"));
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            XdmDestination out = new XdmDestination();
            out.setTreeModel(JDOM2ObjectModel.getInstance());
            xsl.applyTemplates(input, out);
            XdmNode root = out.getXdmNode();
            assertTrue(root.getUnderlyingNode() instanceof net.sf.saxon.option.jdom2.JDOM2NodeWrapper, "result is JDOM2 document node");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testStreamingWithSchemaAccess() {
        // Create a Processor instance.
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.TIMING, true);

            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(new File(configTest.getDataDir() + "books.xsd")));

            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                            "<xsl:mode streamable='yes' on-no-match='shallow-copy'/>" +
                            "<xsl:import-schema/>" +
                            "<xsl:template match='schema-element(ITEM)/AUTHOR'/> \n" +
                    "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();

            Source books = new StreamSource(new File(configTest.getDataDir() + "books.xml"));
            Source validBooks = sm.newSchemaValidator().asSource(books);

            // Create a serializer
            //String outfile = @"C:/MyJava/users/caffo/Output.xml";
            StringWriter sw = new StringWriter();
            Serializer serializer = proc.newSerializer(sw);
            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            transformer.applyTemplates(validBooks, serializer);
            assertTrue(sw.toString().contains("TITLE"));
            assertFalse(sw.toString().contains("AUTHOR"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * s9api streaming transformation pipeline
     */

    @Test
    public void testPipeline() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();

            String xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template>" +
                    "<xsl:mode streamable='yes'/>" +
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
            System.err.println(sw);
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
                    "<xsl:mode streamable='yes'/>" +
                    "</xsl:stylesheet>";
            String xml = "<z/>";
            Xslt30Transformer stage1 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            Xslt30Transformer stage2 = comp.compile(new StreamSource(new StringReader(xsl))).load30();
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);
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
     * Test that the base URI of the result document is the base output URI
     */

    @Test
    public void testOutputBaseUri() {
        try {
            Processor processor = new Processor(false);

            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setDTDValidation(false);
            builder.setLineNumbering(true);

            Source document = new StreamSource(new File(configTest.getDataDir() + "caffo/Input.xml"));
            XdmNode stylesheet = builder.build(new File(configTest.getDataDir() + "caffo/Transform2Streamable.xslt"));

            XsltCompiler compiler = processor.newXsltCompiler();
            compiler.setSchemaAware(processor.isSchemaAware());
            XsltExecutable exec = compiler.compile(stylesheet.asSource());
            Xslt30Transformer transformer = exec.load30();

            XdmDestination result = new XdmDestination();
            transformer.setBaseOutputURI("file://tmp/out.xml");

            transformer.setSchemaValidationMode(ValidationMode.DEFAULT);
            transformer.applyTemplates(document, result);

            XdmNode xformed = result.getXdmNode();
            assertEquals("file://tmp/out.xml", xformed.getBaseURI().toString(), "output base uri");
        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            fail();
        }
    }


    /**
     * Test calling a stylesheet with an initial template with parameters
     *
     * @throws SaxonApiException
     *          if the call fails
     */

    @Test
    public void testCallStreamableTemplateWithParams() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
        Processor processor = new Processor(configuration);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                version='3.0'>" +
                "            <xsl:mode streamable='yes'/>" +
                "                <xsl:template match='/*'>" +
                "                   <xsl:param name='a' as='xs:double'/>" +
                "                   <xsl:param name='b' as='xs:float'/>" +
                "                   <xsl:sequence select='string-length(.) + $a + $b'/>" +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        Map<QName, XdmValue> params = new HashMap<>();
        params.put(new QName("a"), new XdmAtomicValue(12));
        params.put(new QName("b"), new XdmAtomicValue(5));
        t.setInitialTemplateParameters(params, false);
        RawDestination se = new RawDestination();
        t.applyTemplates(new StreamSource(new StringReader("<x>xyz</x>")), se);
        assertEquals(20, ((XdmAtomicValue)se.getXdmValue().itemAt(0)).getLongValue());
    }

    /**
     * Test calling a stylesheet with an initial template with parameters
     */

    @Test
    public void testCallStreamableTemplateWithOmittedParams() {
        try {
            Configuration configuration = Configuration.newConfiguration();
            //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
            Processor processor = new Processor(configuration);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0'>" +
                    "            <xsl:mode streamable='yes'/>" +
                    "                <xsl:template match='/*'>" +
                    "                   <xsl:param name='a' as='xs:double'/>" +
                    "                   <xsl:param name='b' as='xs:float' required='yes'/>" +
                    "                   <xsl:sequence select='string-length(.) + $a + $b'/>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            Map<QName, XdmValue> params = new HashMap<>();
            params.put(new QName("a"), new XdmAtomicValue(12));
            t.setInitialTemplateParameters(params, false);
            RawDestination se = new RawDestination();
            t.applyTemplates(new StreamSource(new StringReader("<x>xyz</x>")), se);
            fail("unexpected success");
            assertEquals(20, ((XdmAtomicValue) se.getXdmValue().itemAt(0)).getLongValue());
        } catch (SaxonApiException e) {
            assertEquals("XTDE0700", e.getErrorCode().getLocalName());
        }
    }


    /**
     * Test calling a stylesheet with an initial template with tunnel parameters
     *
     * @throws SaxonApiException
     *          if the call fails
     */

    @Test
    public void testApplyStreamableTemplateWithTunnelParams() throws SaxonApiException {
        Configuration configuration = Configuration.newConfiguration();
        //configuration.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, false);
        Processor processor = new Processor(configuration);
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
                "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                "                version='3.0'>" +
                "            <xsl:mode streamable='yes'/>" +
                "                <xsl:template match='x'>" +
                "                   <xsl:apply-templates select='y'/>" +
                "                </xsl:template>" +
                "                <xsl:template match='y'>" +
                "                   <xsl:param name='a' as='xs:double' tunnel='yes'/>" +
                "                   <xsl:param name='b' as='xs:float' tunnel='yes'/>" +
                "                   <xsl:sequence select='$a + $b'/>" +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        Map<QName, XdmValue> params = new HashMap<>();
        params.put(new QName("a"), new XdmAtomicValue(12));
        params.put(new QName("b"), new XdmAtomicValue(5));
        t.setInitialTemplateParameters(params, true);
        StringWriter sw = new StringWriter();
        Serializer se = processor.newSerializer(sw);
        se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
        t.applyTemplates(new StreamSource(new StringReader("<x><y/></x>")), se);
        assertEquals(sw.toString(), "17");
    }

    /**
     * Test calling a streamable stylesheet with result validation
     *
     */

    @Test
    public void testApplyTemplatesWithResultValidation() {
        runApplyTemplatesWithResultValidation(true);
    }

    @Test
    public void testApplyTemplatesWithResultValidationFailing() {
        runApplyTemplatesWithResultValidation(false);
    }


    /**
     * Test calling a streamable stylesheet with result validation
     *
     */

    public void runApplyTemplatesWithResultValidation(boolean good) {
        try {
            Processor processor = new Processor(true);
            XsltCompiler xsltCompiler = processor.newXsltCompiler();
            xsltCompiler.setSchemaAware(true);

            String source = "<?xml version='1.0'?>" +
                    "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                    "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                    "                version='3.0' exclude-result-prefixes='#all' expand-text='yes'>" +
                    "                <xsl:import-schema><xs:schema><xs:element name='x' type='xs:int'/></xs:schema></xsl:import-schema>" +
                    "                <xsl:mode streamable='yes' on-no-match='shallow-skip'/>" +
                    "                <xsl:param name='prefix'/>" +
                    "                <xsl:template match='/a/b'>" +
                    "                   <xsl:result-document validation='strict'>" +
                    "                     <x>{$prefix}{3 + .}0</x>" +
                    "                   </xsl:result-document>" +
                    "                </xsl:template>" +
                    "                </xsl:stylesheet>";

            StreamSource ss = new StreamSource(new StringReader(source), "file:///dummy/");
            XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
            Xslt30Transformer t = xsltExecutable.load30();
            Map<QName, XdmValue> params = new HashMap<>();
            params.put(new QName("prefix"), new XdmAtomicValue(good ? "" : "$"));
            t.setStylesheetParameters(params);
            StringWriter sw = new StringWriter();
            Serializer se = processor.newSerializer(sw);
            se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            t.setResultDocumentHandler(uri -> se);
            t.applyTemplates(new StreamSource(new StringReader("<a><b>5</b></a>")), se);
            if (good) {
                assertEquals("<x>80</x>", sw.toString());
            }
        } catch (SaxonApiException e) {
            if (!good) {
                assertEquals("XTTE1510", e.getErrorCode().getLocalName());
            } else {
                e.printStackTrace();
                fail();
            }
        }
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
        XsltCompiler xsltCompiler = processor.newXsltCompiler();

        String source = "<?xml version='1.0'?>" +
            "                <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
            "                xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
            "                version='3.0'> <xsl:mode streamable='yes'/>" +
            "                <xsl:template match='*'>" +
            "                   <xsl:param name='a' as='xs:double'/>" +
            "                   <xsl:param name='b' as='xs:float'/>" +
            "                   <xsl:sequence select='copy-of(.), $a + $b'/>" +
            "                </xsl:template>" +
            "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        Map<QName, XdmValue> params = new HashMap<>();
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
     * @throws SaxonApiException if the call fails
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
                "                <xsl:output method='text' item-separator='~~'/> " +
                "                <xsl:mode streamable='yes'/>" +
                "                <xsl:template match='w'>" +
                "                   <xsl:param name='a' as='xs:double'/>" +
                "                   <xsl:param name='b' as='xs:float'/>" +
                "                   <xsl:sequence select='copy-of(.), $a + $b'/>" +
                "                </xsl:template>" +
                "                </xsl:stylesheet>";

        StreamSource ss = new StreamSource(new ByteArrayInputStream(source.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
        XsltExecutable xsltExecutable = xsltCompiler.compile(ss);
        Xslt30Transformer t = xsltExecutable.load30();
        Map<QName, XdmValue> params = new HashMap<>();
        params.put(new QName("a"), new XdmAtomicValue(12));
        params.put(new QName("b"), new XdmAtomicValue(5));
        t.setInitialTemplateParameters(params, false);
        StringWriter sw = new StringWriter();
        Serializer se = processor.newSerializer(sw);
        se.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
        t.applyTemplates(new StreamSource(new StringReader("<w>16</w>")), se);
        assertEquals("16~~17", sw.toString());
    }


    /**
     * Test Strip-type-annotations with streaming ApplyTemplates
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
                    "                input-type-annotations='strip'\n" +
                    "                version=\"2.0\">\n" +
                    "<!-- Purpose: Show that when @input-type-annotations=\"strip\", is-nilled property of element  nodes is set to false.-->\n" +
                    " " +
                    "   <xslt:template match='/my:userNode'>\n" +
                    "      <out>\n" +
                    "         <xslt:value-of select='nilled(.)'/>\n" +
                    "      </out>\n" +
                    "   </xslt:template>\n" +
                    "   <xslt:mode streamable='yes'/>" +
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
            StreamSource sDoc = new StreamSource(new ByteArrayInputStream(in.getBytes(StandardCharsets.UTF_8)), "file:///dummy/");
            Source vDoc = validator.asSource(sDoc);

            StringWriter writer = new StringWriter();
            Serializer out = proc.newSerializer(writer);
            xsl.applyTemplates(vDoc, out);
            assertTrue(writer.toString().contains("false"), "strip-type-annotations not working");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    /**
     * Test Strip-space with streaming ApplyTemplates
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
                                                       "   <xslt:mode on-no-match='deep-copy' streamable='yes'/>\n" +
                                                       "</xslt:transform>");

            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer xsl = exec.load30();
            String in = "<?xml version='1.0'?> \n" +
                    "<my:userNode xmlns:my='http://ns.example.com/strip-type'><foo> </foo><bar> </bar><my:foo> </my:foo><my:bar> </my:bar></my:userNode>";

            StringWriter writer = new StringWriter();
            Serializer out = proc.newSerializer(writer);
            xsl.applyTemplates(new StreamSource(new StringReader(in)), out);
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
                    "  <xsl:mode streamable='yes'/>" +
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
            Processor proc = new Processor(true);
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
     * Test that trace() output goes to TraceListener when streaming (bug 4028)
     */

    @Test
    public void testTraceOutput() {
        try {

            String xsl = "<xsl:stylesheet version=\"3.0\" \n" +
                    "  xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "  <xsl:mode streamable='yes' on-no-match='shallow-copy'/>" +
                    "  <xsl:template match='a'>\n" +
                    "    <c><xsl:value-of select=\"trace(copy-of(.), 'NAME:')\"/></c>\n" +
                    "  </xsl:template>\n" +
                    "</xsl:stylesheet>";
            Processor proc = new Processor(true);
            XsltCompiler compiler = proc.newXsltCompiler();
            compiler.setCompileWithTracing(true);
            Xslt30Transformer transformer = compiler.compile(new StreamSource(new StringReader(xsl))).load30();
            List<String> labels = new ArrayList<>();
            transformer.setTraceListener(new TraceListener() {

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
                    Object val = properties.get("label");
                    if (val != null) {
                        labels.add(val.toString());
                    }
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
            });
            StreamSource source = new StreamSource(new StringReader("<a>b</a>"));
            XdmValue xdmValue = transformer.applyTemplates(source);
            assertEquals("trace 0: NAME:: open", labels.get(0));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


}



