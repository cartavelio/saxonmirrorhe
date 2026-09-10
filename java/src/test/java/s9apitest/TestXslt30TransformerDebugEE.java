////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2018-2023 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package s9apitest;

import com.saxonica.config.EnterpriseTransformerFactory;
import com.saxonica.config.ProfessionalTransformerFactory;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.lib.TraceListener;
import net.sf.saxon.om.Item;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trace.Traceable;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.*;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the s9api Xslt30Transformer in "debugging" mode, that is,
 * with a TraceListener enabled.
 */
public class TestXslt30TransformerDebugEE {
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

    /**
     * Same as non-EE tests, but using JAXP invocation
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
            Source xmlSource = new StreamSource(Testdata.file("caffo/Input.xml"));

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
            transformer.setParameter("p", Testdata.file("books.xml").getAbsoluteFile());

            // Create XML  SAX source
            Source xmlSource = new StreamSource(Testdata.file("caffo/Input.xml"));

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
            transformer.setTraceListener(new TestXslt30TransformerDebug.MyTraceListener());

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
}



