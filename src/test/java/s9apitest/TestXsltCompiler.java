////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013-2022 Saxonica Limited
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.config.ProfessionalConfiguration;
import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.ChainedResourceResolver;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xml.sax.XMLFilter;
import org.xml.sax.XMLReader;
import org.xml.sax.helpers.XMLFilterImpl;
import org.xml.sax.helpers.XMLReaderFactory;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.URIResolver;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the s9api XsltCompiler
 */
public class TestXsltCompiler {

    private void xpathVersionTest(String label, String expression, int version, boolean expectSuccess) {
        try {
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.XPATH_VERSION_FOR_XSLT, version);
            XsltCompiler comp = proc.newXsltCompiler();
            String xsl = "<xsl:stylesheet version='3.0' xmlns:xs='http://www.w3.org/2001/XMLSchema' " +
                    "xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:template match='/'>\n" +
                    "<xsl:sequence select=\"" + expression + "\"/>" +
                    "</xsl:template>" +
                    "</xsl:stylesheet>";
            comp.compile(new StreamSource(new StringReader(xsl)));
            if (!expectSuccess) {
                fail(label + ": expected failure");
            }
        } catch (SaxonApiException e) {
            if (expectSuccess) {
                fail(label + ": " + e.getMessage());
            }
        }
    }

    @Test
    public void testXPathVersion20() {
        xpathVersionTest("A", "'b'||'c'", 20, false);
        xpathVersionTest("B", "(1 to 3)!(.*.)", 20, false);
        xpathVersionTest("C", "exists(map{})", 20, false);
        xpathVersionTest("D", "[1,2]", 20, false);
        xpathVersionTest("E", "(1 to 5)=>count()", 20, false);
        xpathVersionTest("F", "for $x in 1 to 3 return $x+1", 20, true);
        xpathVersionTest("G", "if (current-date() lt xs:date('2010-01-01')) then 0 else 1", 20, true);
        xpathVersionTest("H", "let $x := 3 return $x+1", 20, false);
    }

    @Test
    public void testXPathVersion30() {
        xpathVersionTest("A", "'b'||'c'" , 30, true);
        xpathVersionTest("B", "(1 to 3)!(.*.)", 30, true);
        xpathVersionTest("C", "exists(map{})", 30, false);
        xpathVersionTest("D", "[1,2]", 30, false);
        xpathVersionTest("E", "(1 to 5)=>count()", 30, false);
        xpathVersionTest("F", "for $x in 1 to 3 return $x+1", 30, true);
        xpathVersionTest("G", "if (current-date() lt xs:date('2010-01-01')) then 0 else 1", 30, true);
        xpathVersionTest("H", "let $x := 3 return $x+1", 30, true);
    }

    @Test
    public void testXPathVersion305() {
        xpathVersionTest("A", "'b'||'c'", 305, true);
        xpathVersionTest("B", "(1 to 3)!(.*.)", 305, true);
        xpathVersionTest("C", "exists(map{})", 305, true);
        xpathVersionTest("D", "[1,2]", 305, false);
        xpathVersionTest("E", "(1 to 5)=>count()", 305, false);
        xpathVersionTest("F", "for $x in 1 to 3 return $x+1", 305, true);
        xpathVersionTest("G", "if (current-date() lt xs:date('2010-01-01')) then 0 else 1", 305, true);
        xpathVersionTest("H", "let $x := 3 return $x+1", 305, true);
    }

    @Test
    public void testXPathVersion31() {
        xpathVersionTest("A", "'b'||'c'", 31, true);
        xpathVersionTest("B", "(1 to 3)!(.*.)", 31, true);
        xpathVersionTest("C", "exists(map{})", 31, true);
        xpathVersionTest("D", "[1,2]", 31, true);
        xpathVersionTest("E", "(1 to 5)=>count()", 31, true);
        xpathVersionTest("F", "for $x in 1 to 3 return $x+1", 31, true);
        xpathVersionTest("G", "if (current-date() lt xs:date('2010-01-01')) then 0 else 1", 31, true);
        xpathVersionTest("H", "let $x := 3 return $x+1", 31, true);
    }

    @Test
    public void testSampleError() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.XPATH_VERSION_FOR_XSLT, 31);
            XsltCompiler comp = proc.newXsltCompiler();
            String xsl = "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:saxon='http://saxon.sf.net/'>\n" +
                    " <xsl:template name='main'> " +
                    " <xsl:source-document href='/Users/mike/repo/samples/data/othello.xml'>"
                    + "<xsl:value-of select=\"count(//LINE[count(tokenize(.,'\\s')) &gt; 0] )\" />"
                    + "</xsl:source-document>" +
                    " </xsl:template></xsl:transform>";
            comp.compile(new StreamSource(new StringReader(xsl)));
        } catch (SaxonApiException e) {
            fail();
        }
    }


    /* String s = "<xsl:transform version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:saxon='http://saxon.sf.net/'>\n" +
            " <xsl:template name='main'> " +
            " <xsl:value-of select=\"count(saxon:stream(doc('" + new Uri(samplesDir, "data/othello.xml").ToString() + "')//LINE[count(tokenize(.,'/s')) &gt; 0] ))\" />" +
            " </xsl:template></xsl:transform>";


        // Create a transformer for the stylesheet.
        XsltTransformer transformer = processor.NewXsltCompiler().Compile(new StringReader(s)).Load();*/

    @Test
    public void testXPath30Failure() {
        List<XmlProcessingError> errors = new ArrayList<>();
        try {
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.XPATH_VERSION_FOR_XSLT, 30);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setErrorList(errors);
            String xsl = "<xsl:stylesheet version='3.0' " +
                    "xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:template match=\"*[map{1:'a'}?1 = 'a']\"/>\n" +
                    "<xsl:variable name='x' select='map{2:4}' as='map(*)*'/>" +
                    "<xsl:variable name='y' select='[1,2,3]' as='array(*)?'/>" +
                    "<xsl:variable name='z' select='array{1,21,3}' as='array(*)?'/>" +
                    "</xsl:stylesheet>";
            comp.compile(new StreamSource(new StringReader(xsl)));
            fail("unexpected success");
        } catch (SaxonApiException e) {
            assertTrue(errors.size() > 1);
            Set<String> codes = new HashSet<>();
            for (XmlProcessingError se : errors) {
                codes.add(se.getErrorCode().getLocalName());
            }
            assertTrue(codes.contains("XPST0003"));
        }
    }

    @Test
    public void testXPath30FailureWithErrorReporter() {
        List<XmlProcessingError> errors = new ArrayList<>();
        try {
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.XPATH_VERSION_FOR_XSLT, 30);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setErrorReporter(errors::add);
            String xsl = "<xsl:stylesheet version='3.0' " +
                    "xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:template match=\"*[map{1:'a'}?1 = 'a']\"/>\n" +
                    "<xsl:variable name='x' select='map{2:4}' as='map(*)*'/>" +
                    "<xsl:variable name='y' select='[1,2,3]' as='array(*)?'/>" +
                    "<xsl:variable name='z' select='array{1,21,3}' as='array(*)?'/>" +
                    "</xsl:stylesheet>";
            comp.compile(new StreamSource(new StringReader(xsl)));
            fail("unexpected success");
        } catch (SaxonApiException e) {
            assertTrue(errors.size() > 1);
            Set<String> codes = new HashSet<>();
            for (XmlProcessingError se : errors) {
                codes.add(se.getErrorCode().getLocalName());
            }
            assertTrue(codes.contains("XPST0003"));
        }
    }

    @Test
    public void testXPath305Success() {
        try {
            Processor proc = new Processor(false);
            proc.setConfigurationProperty(Feature.XPATH_VERSION_FOR_XSLT, 305);
            XsltCompiler comp = proc.newXsltCompiler();
            String xsl = "<xsl:stylesheet version='3.0' " +
                    "xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:template match=\"*[map{1:'a'}(1) = 'a']\"/>\n" +
                    "<xsl:variable name='x' select='map{2:4}' as='map(*)*'/>" +
                    "</xsl:stylesheet>";
            comp.compile(new StreamSource(new StringReader(xsl)));
        } catch (SaxonApiException e) {
            fail();
            e.printStackTrace();
        }
    }


    /**
     * Test location information for run-time errors when stylesheet built as an XdmNode
     */

    @Test
    public void testErrorNotificationFromParseError() {
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

            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' " +
                    "xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:saxon='http://saxon.sf.net/'>\n" +
                    "<xsl:template name='main'>\n" +
                    "<xsl:copy-of select=\"saxon:parse('&lt;doc>&lt;e>text&lt;/f>&lt;/doc>')\"/>\n" +
                    "</xsl:templateXX>\n" +
                    "</xsl:stylesheet>");


            xqcomp.setErrorListener(listener);
            xqcomp.compile(new StreamSource(sr));

            fail("Compilation should fail");

        } catch (SaxonApiException e) {
            assertTrue(warnings.isEmpty(), "warnings");
            assertTrue(errors.isEmpty(), "errors");
            assertEquals(1, fatals.size(), "fatals");
            //e.printStackTrace();
        }
    }

    @Test
    public void testMultipleSyntaxErrors() {
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

            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' " +
                                                       "xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:saxon='http://saxon.sf.net/'>\n" +
                                                       "<xsl:template name='main'>\n" +
                                                       "<xsl:call-template name='t'/>\n" +
                                                       "<xsl:call-template name='u'/>\n" +
                                                       "<xsl:call-template name='v'/>\n" +
                                                       "<xsl:call-template name='w'/>\n" +
                                                       "<xsl:call-template name='x'/>\n" +
                                                       "</xsl:template>\n" +
                                                       "</xsl:stylesheet>");


            xqcomp.setErrorListener(listener);
            xqcomp.compile(new StreamSource(sr));

            fail("Compilation should fail");

        } catch (SaxonApiException e) {
            assertTrue(warnings.isEmpty(), "warnings");
            assertTrue(errors.isEmpty(), "errors");
            assertEquals(5, fatals.size(), "fatals");
            //e.printStackTrace();
        }
    }

    @Test
    public void testJITStaticErrors() {
        // Show how static errors can be detected at run-time using JIT
        final List<TransformerException> warnings = new ArrayList<>(4);
        final List<TransformerException> errors = new ArrayList<>(4);
        final List<TransformerException> fatals = new ArrayList<>(4);
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

            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' " +
                                                       "xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
                                                       "xmlns:saxon='http://saxon.sf.net/'>\n" +
                                                       "<xsl:template match='/'>\n" +
                                                       "   <xsl:apply-templates/>\n" +
                                                       "</xsl:template>\n" +
                                                       "<xsl:template match='/*'>\n" +
                                                       "   <xsl:apply-templates select='$bad'/>\n" +
                                                       "</xsl:template>\n" +
                                                       "</xsl:stylesheet>");


            xqcomp.setErrorListener(listener);
            XsltExecutable exec = null;
            try {
                exec = xqcomp.compile(new StreamSource(sr));
                assertTrue(warnings.isEmpty(), "warnings");
                assertTrue(errors.isEmpty(), "errors");
                assertTrue(fatals.isEmpty(), "fatals");
            } catch (SaxonApiException ce) {
                fail("Compilation should not fail yet");
            }
            XdmNode in = proc.newDocumentBuilder().build(
                    new StreamSource(new StringReader("<a><b/></a>")));
            try {
                XdmValue out = exec.load30().applyTemplates(in);
            } catch (SaxonApiException ce) {
                // OK - expected
            }

        } catch (SaxonApiException e) {
            assertTrue(warnings.isEmpty(), "warnings");
            assertTrue(errors.isEmpty(), "errors");
            assertEquals(1, fatals.size(), "fatals");
            //e.printStackTrace();
        }
    }


    /**
     * Test location information for import errors
     */

    @Test
    public void testErrorNotificationFromImportError() {
        final List<TransformerException> warnings = new ArrayList<TransformerException>(4);
        final List<TransformerException> errors = new ArrayList<TransformerException>(4);
        final List<TransformerException> fatals = new ArrayList<TransformerException>(4);
        ErrorListener listener = new ErrorListener() {

            @Override
            public void warning(TransformerException exception) throws TransformerException {
                System.out.println("Warning: " + exception + "\nlocation: " + exception.getLocator());
                warnings.add(exception);
            }

            @Override
            public void error(TransformerException exception) throws TransformerException {
                System.out.println("Error: " + exception + "\nlocation: " + exception.getLocator());
                errors.add(exception);
            }

            @Override
            public void fatalError(TransformerException exception) throws TransformerException {
                if (!exception.getMessage().contains("No such file or directory")) {
                    System.out.println("Fatal: " + exception + "\nlocation: " + exception.getLocator());
                }
                assertEquals(3, exception.getLocator().getLineNumber());
                fatals.add(exception);
            }

        };
        try {

            Processor proc = new Processor(true);

            XsltCompiler xqcomp = proc.newXsltCompiler();

            StringReader sr = new StringReader("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n" +
                "   <xsl:import href=\"invalidRef.xsl\"/>\n" +
                "</xsl:stylesheet>");


            xqcomp.setErrorListener(listener);
            xqcomp.compile(new StreamSource(sr));

            fail("Compilation should fail");

        } catch (SaxonApiException e) {
            assertTrue(warnings.isEmpty(), "warnings");
            assertTrue(errors.isEmpty(), "errors");
            assertEquals(1, fatals.size(), "fatals");
            //e.printStackTrace();
        }
    }

    /**
     * Test location information for import errors
     */

    @Test
    public void testErrorReportingFromImportError() {
        final List<XmlProcessingError> warnings = new ArrayList<>(4);
        final List<XmlProcessingError> errors = new ArrayList<>(4);

        try {

            Processor proc = new Processor(true);

            XsltCompiler xqcomp = proc.newXsltCompiler();

            StringReader sr = new StringReader("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                                                       "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n" +
                                                       "   <xsl:import href=\"invalidRef.xsl\"/>\n" +
                                                       "</xsl:stylesheet>");

            xqcomp.setErrorReporter(error -> {
                if (error.isWarning())
                    warnings.add(error);
                else
                    errors.add(error);
                });
            xqcomp.compile(new StreamSource(sr));

            fail("Compilation should fail");

        } catch (SaxonApiException e) {
            assertTrue(warnings.isEmpty(), "warnings");
            assertEquals(1, errors.size(), "errors");
        }
    }


    @Test
    public void testErrorLocation() {

            // Create a Processor instance.
            Processor processor = new Processor(false);

            // Create the XSLT Compiler
            XsltCompiler compiler = processor.newXsltCompiler();


            // Define a stylesheet containing errors
            String stylesheet =
                    "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0'>\n" +
                            "<xsl:template name='eee:template'>\n" +
                            "  <xsl:value-of select='32'/>\n" +
                            "</xsl:template>\n" +
                            "<xsl:template name='main'>\n" +
                            "  <xsl:value-of select='$var'/>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";


            // Attempt to compile the stylesheet and display the errors
            try {
                compiler.setErrorReporter(error -> {
                    if (error.getErrorCode().getLocalName().equals("XTSE0280")) {
                        assertEquals(2, error.getLocation().getLineNumber());
                        assertEquals(35, error.getLocation().getColumnNumber());
                    } else if (error.getErrorCode().getLocalName().equals("XPST0008")) {
                        assertEquals(6, error.getLocation().getLineNumber());
                        //assertEquals(32, error.getLocation().getColumnNumber());
                    } else {
                        fail(error.getErrorCode().getLocalName());
                    }
                });
                compiler.compile(new StreamSource(new StringReader(stylesheet), "http://localhost/stylesheet"));
                fail("Stylesheet compilation succeeded unexpectedly");
            } catch (SaxonApiException e) {
                System.err.println("Stylesheet compilation failed as expected");
            }


    }

    @Test
    public void testCustomXmlReader() {
        try {
            // Create a Processor instance.
            Processor processor = new Processor(false);

            // Create the XSLT Compiler
            XsltCompiler compiler = processor.newXsltCompiler();


            // Define a stylesheet
            String stylesheet =
                    "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='1.0'>\n" +
                            "<xsl:template name='main'>22</xsl:template>\n" +
                            "</xsl:stylesheet>";

            //
            XMLReader reader = XMLReaderFactory.createXMLReader("org.apache.xerces.parsers.SAXParser");
            List<String> progress = new ArrayList<String>();
            XMLFilter filter = new XMLFilterImpl(reader) {
                /**
                 * Filter a start document event.
                 *
                 * @throws SAXException The client may throw
                 *                      an exception during processing.
                 */
                @Override
                public void startDocument() throws SAXException {
                    progress.add("started");
                    super.startDocument();
                }
            };
            SAXSource source = new SAXSource(filter, new InputSource(new StringReader(stylesheet)));
            compiler.compile(source);
            System.err.println("Stylesheet compilation succeeded");
            assertEquals(1, progress.size());
            assertEquals("started", progress.get(0));
        } catch (SaxonApiException e) {
            fail("Stylesheet compilation failed");
        } catch (SAXException e) {
            fail();
        }
    }

    @Test
    public void testPreEvaluateDocumentFunction() {
        // Bug 2963
        String stylesheet =
                "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>\n" +
                        "<xsl:param name='call' static='yes'/>\n" +
                        "<xsl:variable name='d' _select='{$call}'/>\n" +
                        "<xsl:template name='xsl:initial-template'>\n" +
                        "  <xsl:copy-of select='$d'/>\n" +
                        "</xsl:template>\n" +
                        "</xsl:stylesheet>";
        String badQuery = "document('http://example.com/bad')";
        String goodQuery = "document('http://example.com/good')";
        Processor p = new Processor(false);
        XsltCompiler c = p.newXsltCompiler();
        final List<String> counter = new ArrayList<>();
        p.getUnderlyingConfiguration().setResourceResolver(
                new ChainedResourceResolver(request -> {
                    switch (request.relativeUri == null ? "" : request.relativeUri) {
                        case "http://example.com/bad":
                            String badXml = "<bad>";
                            counter.add(request.relativeUri);
                            return new StreamSource(new StringReader(badXml));
                        case "http://example.com/good":
                            String goodXml = "<good/>";
                            counter.add(request.relativeUri);
                            return new StreamSource(new StringReader(goodXml));
                        default:
                            return null;
                    }
                },
                p.getUnderlyingConfiguration().getResourceResolver()));
        p.setConfigurationProperty(Feature.PRE_EVALUATE_DOC_FUNCTION, false);
        XsltExecutable q = null;
        try {
            c.setParameter(new QName("call"), new XdmAtomicValue(badQuery));
            q = c.compile(new StreamSource(new StringReader(stylesheet)));
        } catch (Exception err) {
            err.printStackTrace();
            fail("Compilation 1 should not fail");
        }
        assertEquals(0, counter.size(), "URI not accessed");
        p.setConfigurationProperty(Feature.PRE_EVALUATE_DOC_FUNCTION, true);
        try {
            c.setParameter(new QName("call"), new XdmAtomicValue(goodQuery));
            q = c.compile(new StreamSource(new StringReader(stylesheet)));
            assertTrue(counter.size() > 0, "URI accessed");
            //assertTrue("Compiled to Literal", q.getUnderlyingCompiledQuery().getExpression() instanceof Literal);
            //assertTrue("Compiled to NodeInfo", ((Literal) q.getUnderlyingCompiledQuery().getExpression()).getValue().head() instanceof NodeInfo);
        } catch (Exception err) {
            err.printStackTrace();
            fail("Compilation 2 should not fail");
        }
        try {
            XdmValue result = q.load30().callTemplate(new QName(NamespaceConstant.XSLT, "initial-template"));
            assertTrue(result instanceof XdmNode);
        } catch (Exception err) {
            fail("Execution should not fail");
            err.printStackTrace();
        }

    }


    @Test
    public void testJitEEOn() {
        testJit(new EnterpriseConfiguration(), "10", false);
    }

    @Test
    public void testJitEEOnJ() {
        testJit(new EnterpriseConfiguration(), "j", false);
    }

    @Test
    public void testJitEEOff() {
        testJit(new EnterpriseConfiguration(), "0", true);
    }

    @Test
    public void testJitEEOffJ() {
        testJit(new EnterpriseConfiguration(), "-j", true);
    }

    @Test
    public void testJitPEOn() {
        testJit(new ProfessionalConfiguration(), "10", true);
    }

    @Test
    public void testJitPEOff() {
        testJit(new ProfessionalConfiguration(), "0", true);
    }

    @Test
    public void testJitHEOn() {
        testJit(new Configuration(), "10", true);
    }

    @Test
    public void testJitHEOff() {
        testJit(new Configuration(), "0", true);
    }

    private void testJit(Configuration config, Object opt, boolean failEarly) {
        String xsl =
                "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>\n" +
                        "<xsl:template match='/'>\n" +
                        "  <xsl:apply-templates/>\n" +
                        "</xsl:template>\n" +
                        "<xsl:template match='doc'>\n" +
                        "  <xsl:apply-templates select='$x'/>\n" +    // deliberate syntax error
                        "</xsl:template>\n" +
                        "</xsl:stylesheet>";
        String xml = "<doc/>";
        Processor p = new Processor(config);
        p.setConfigurationProperty(FeatureKeys.OPTIMIZATION_LEVEL, opt);
        XsltCompiler c = p.newXsltCompiler();
        XsltExecutable q = null;
        try {
            q = c.compile(new StreamSource(new StringReader(xsl)));
            if (failEarly) {
                fail("Compilation should fail");
            }
        } catch (Exception err) {
            if (!failEarly) {
                fail("Compilation should not fail");
                err.printStackTrace();
            }
            return;
        }
        try {
            Xslt30Transformer t = q.load30();
            t.applyTemplates(new StreamSource(new StringReader(xml)));
            fail("Execution should not succeed");
        } catch (SaxonApiException err) {
            // OK
        }


    }

    @Test
    public void testAnyNamespaceOption() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setUnprefixedElementMatchingPolicy(UnprefixedElementMatchingPolicy.ANY_NAMESPACE);
            String xml="<a xmlns='urn:one'><a xmlns='urn:two'/></a>";
            String xsl = "<xsl:stylesheet version='3.0' " +
                    "xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:template match='/'>\n" +
                    "<out count='{count(//a)}'><xsl:apply-templates select='//a'/></out>" +
                    "</xsl:template>" +
                    "<xsl:template match='a'>A</xsl:template>" +
                    "</xsl:stylesheet>";
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(xsl)));
            Xslt30Transformer t = exec.load30();
            StringWriter sw = new StringWriter();
            t.applyTemplates(new StreamSource(new StringReader(xml)), proc.newSerializer(sw));
            System.err.println(sw);
            assertTrue(sw.toString().contains("<out count=\"2\">AA</out>"), "result");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    @Test
    public void testDefaultOrNullOption() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setUnprefixedElementMatchingPolicy(UnprefixedElementMatchingPolicy.DEFAULT_NAMESPACE_OR_NONE);
            String xml = "<a xmlns='urn:one'><a xmlns=''/></a>";
            String xsl = "<xsl:stylesheet version='3.0' " +
                    "xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xpath-default-namespace='urn:one'>\n" +
                    "<xsl:template match='/'>\n" +
                    "<out count='{count(//a)}'><xsl:apply-templates select='//a'/></out>" +
                    "</xsl:template>" +
                    "<xsl:template match='a'>A</xsl:template>" +
                    "</xsl:stylesheet>";
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(xsl)));
            Xslt30Transformer t = exec.load30();
            StringWriter sw = new StringWriter();
            t.applyTemplates(new StreamSource(new StringReader(xml)), proc.newSerializer(sw));
            System.err.println(sw);
            assertTrue(sw.toString().contains("<out count=\"2\">AA</out>"), "result");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    @Test
    public void testSetDefaultNamespaceProgrammatically() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setDefaultElementNamespace("urn:one");
            comp.setUnprefixedElementMatchingPolicy(UnprefixedElementMatchingPolicy.DEFAULT_NAMESPACE_OR_NONE);
            String xml = "<a xmlns='urn:one'><a xmlns=''/></a>";
            String xsl = "<xsl:stylesheet version='3.0' " +
                    "xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:template match='/'>\n" +
                    "<out count='{count(//a)}'><xsl:apply-templates select='//a'/></out>" +
                    "</xsl:template>" +
                    "<xsl:template match='a'>A</xsl:template>" +
                    "</xsl:stylesheet>";
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(xsl)));
            Xslt30Transformer t = exec.load30();
            StringWriter sw = new StringWriter();
            t.applyTemplates(new StreamSource(new StringReader(xml)), proc.newSerializer(sw));
            System.err.println(sw);
            assertTrue(sw.toString().contains("<out count=\"2\">AA</out>"), "result");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testBug4326() {
        // The essence of this bug is that a user-written URI resolver returns stylesheet
        // modules with a base URI that is different from the one Saxon computes, which causes
        // confusion in locating the module subsequently
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            String xsl1 = "<xsl:stylesheet version='3.0' " +
                    "xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:include href='xsl2.xsl' xml:base='custard'/>\n" +
                    "<xsl:template name='main'>\n" +
                    "<out a='{Q{f}go()}'/>" +
                    "</xsl:template>" +
                    "</xsl:stylesheet>";
            String xsl2 = "<xsl:stylesheet version='3.0' " +
                    "xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>\n" +
                    "<xsl:function name='Q{f}go'>\n" +
                    "<xsl:sequence select='42'/>" +
                    "</xsl:function>" +
                    "</xsl:stylesheet>";
            comp.setURIResolver(new URIResolver() {
                @Override
                public Source resolve(String href, String base) throws TransformerException {
                    if (href.equals("xsl2.xsl")) {
                        return new StreamSource(new StringReader(xsl2), "http://toasted-spaghetti/");
                    } else {
                        return null;
                    }
                }
            });
            InputSource is1 = new InputSource(new StringReader(xsl1));
            is1.setSystemId("http://veggie-burger.com/");
            SAXSource ss1 = new SAXSource(is1);
            ss1.setSystemId("http://fried-chicken/suprême/");
            XsltExecutable exec = comp.compile(ss1);
            Xslt30Transformer trans = exec.load30();
            StringWriter writer = new StringWriter();
            trans.callTemplate(new QName("main"), proc.newSerializer(writer));
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?><out a=\"42\"/>", writer.toString(), "result");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testErrorNotificationFromImportSchemaError() {
        final List<XmlProcessingError> errors = new ArrayList<>(4);

        try {

            Processor proc = new Processor(true);

            XsltCompiler xqcomp = proc.newXsltCompiler();
            xqcomp.setErrorList(errors);

            StringReader xsd = new StringReader("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:element name='123'/></xs:schema>");

            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' " +
                                                       "xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:saxon='http://saxon.sf.net/'>\n" +
                                                       "<xsl:import-schema schema-location='xsd1.xsd'/>\n" +
                                                       "<xsl:template name='main'>\n" +
                                                       "<out xsl:validation='strict'/>\n" +
                                                       "</xsl:template>\n" +
                                                       "</xsl:stylesheet>");

            xqcomp.setURIResolver((href, base) -> {
                if (href.endsWith("xsd1.xsd")) {
                    return new StreamSource(xsd);
                } else {
                    return null;
                }
            });
            xqcomp.compile(new StreamSource(sr));

            fail("Compilation should fail");

        } catch (SaxonApiException e) {
            assertFalse(errors.isEmpty(), "errors");
        }
    }

    @Test
    public void testErrorNotificationFromInlineSchemaError() {
        final List<XmlProcessingError> errors = new ArrayList<>(4);

        try {

            Processor proc = new Processor(true);

            XsltCompiler xqcomp = proc.newXsltCompiler();
            xqcomp.setErrorList(errors);

            StringReader xsd = new StringReader("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:element name='123'/></xs:schema>");

            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' " +
                                                       "xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:saxon='http://saxon.sf.net/'>\n" +
                                                       "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:element nomine='pater'/></xs:schema></xsl:import-schema>\n" +
                                                       "<xsl:template name='main'>\n" +
                                                       "<out xsl:validation='strict'/>\n" +
                                                       "</xsl:template>\n" +
                                                       "</xsl:stylesheet>");

            xqcomp.setURIResolver((href, base) -> {
                if (href.endsWith("xsd1.xsd")) {
                    return new StreamSource(xsd);
                } else {
                    return null;
                }
            });
            xqcomp.compile(new StreamSource(sr));

            fail("Compilation should fail");

        } catch (SaxonApiException e) {
            assertFalse(errors.isEmpty(), "errors");
            //e.printStackTrace();
        }
    }

    @Test
    public void testWarningNotificationFromInlineSchema() {
        final List<XmlProcessingError> errors = new ArrayList<>(4);

        try {

            Processor proc = new Processor(true);

            XsltCompiler xqcomp = proc.newXsltCompiler();
            xqcomp.setErrorList(errors);

            StringReader xsd = new StringReader("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:element name='123'/></xs:schema>");

            StringReader sr = new StringReader("<xsl:stylesheet version='2.0' " +
                                                       "xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:saxon='http://saxon.sf.net/'>\n" +
                                                       "<xsl:import-schema><xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>\n" +
                                                       "<xs:simpleType name='foo'><xs:restriction base='xs:integer'><xs:minInclusive value='0'/><xs:enumeration value='-3'/></xs:restriction></xs:simpleType>\n" +
                                                       "</xs:schema></xsl:import-schema>\n" +
                                                       "<xsl:template name='main'>\n" +
                                                       "<out xsl:validation='strict'/>\n" +
                                                       "</xsl:template>\n" +
                                                       "</xsl:stylesheet>");

            xqcomp.setURIResolver((href, base) -> {
                if (href.endsWith("xsd1.xsd")) {
                    return new StreamSource(xsd);
                } else {
                    return null;
                }
            });
            xqcomp.compile(new StreamSource(sr));

            fail("Compilation should fail");

        } catch (SaxonApiException e) {
            assertFalse(errors.isEmpty(), "errors");
            //e.printStackTrace();
        }
    }

    @Test
    public void testBug4969() {
        // The essence of this bug is that a SEF file generated for a schema-aware stylesheet
        // contained a reference to an anonymous type, which could not be found when the stylesheet
        // was reloaded from the SEF file
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            String xsl1 = "<xsl:stylesheet exclude-result-prefixes=\"#all\" version=\"3.0\"\n"
                    + "  xmlns:test=\"http://www.example.com/test\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n"
                    + "  xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n"
                    + "<xsl:template name=\"xsl:initial-template\">\n"
                    + "  <xsl:variable as=\"element(test:e)\" name=\"e\">\n"
                    + "    <test:e />\n"
                    + "  </xsl:variable>\n"
                    + "  <xsl:variable as=\"element(test:e)\" name=\"ve\">\n"
                    + "    <xsl:copy-of select=\"$e\" validation=\"strict\" />\n"
                    + "  </xsl:variable>\n"
                    + "  <xsl:sequence select=\"$ve, $ve\" />\n"
                    + "</xsl:template>"
                    + "</xsl:stylesheet>";

            String xsd = "<xs:schema elementFormDefault=\"qualified\" targetNamespace=\"http://www.example.com/test\"\n"
                    + "  xmlns=\"http://www.example.com/test\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n"
                    + "  <xs:element name=\"e\">\n"
                    + "    <xs:complexType />\n"
                    + "  </xs:element>\n"
                    + "</xs:schema>";
            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(new StringReader(xsd)));
            StringWriter scm = new StringWriter();
            Serializer scmOut = proc.newSerializer(scm);
            sm.exportComponents(scmOut);

            XsltPackage pack = comp.compilePackage(new StreamSource(new StringReader(xsl1)));
            File sefFile = new File(ConfigTest.TEMP_DIR + "6969test.sef");
            pack.save(sefFile);

            Processor proc2 = new Processor(true);
            SchemaManager sm2 = proc2.getSchemaManager();
            sm2.importComponents(new StreamSource(new StringReader(scm.toString())));
            XsltCompiler comp2 = proc2.newXsltCompiler();
            comp2.setSchemaAware(true);
            XsltExecutable exec = comp2.loadExecutablePackage(sefFile.toURI());
            Xslt30Transformer trans = exec.load30();
            StringWriter out = new StringWriter();
            trans.callTemplate(null, proc2.newSerializer(out));
            assertTrue(out.toString().contains("<test:e xmlns:test=\"http://www.example.com/test\"/>"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    @Test
    public void testReloadFromSource() {
        // Variant of testBug4969 that reloads the SEF file from a string
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            String xsl1 = "<xsl:stylesheet exclude-result-prefixes=\"#all\" version=\"3.0\"\n"
                    + "  xmlns:test=\"http://www.example.com/test\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n"
                    + "  xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n"
                    + "<xsl:template name=\"xsl:initial-template\">\n"
                    + "  <xsl:variable as=\"element(test:e)\" name=\"e\">\n"
                    + "    <test:e />\n"
                    + "  </xsl:variable>\n"
                    + "  <xsl:variable as=\"element(test:e)\" name=\"ve\">\n"
                    + "    <xsl:copy-of select=\"$e\" validation=\"strict\" />\n"
                    + "  </xsl:variable>\n"
                    + "  <xsl:sequence select=\"$ve, $ve\" />\n"
                    + "</xsl:template>"
                    + "</xsl:stylesheet>";

            String xsd = "<xs:schema elementFormDefault=\"qualified\" targetNamespace=\"http://www.example.com/test\"\n"
                    + "  xmlns=\"http://www.example.com/test\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n"
                    + "  <xs:element name=\"e\">\n"
                    + "    <xs:complexType />\n"
                    + "  </xs:element>\n"
                    + "</xs:schema>";
            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(new StringReader(xsd)));
            StringWriter scm = new StringWriter();
            Serializer scmOut = proc.newSerializer(scm);
            sm.exportComponents(scmOut);

            XsltExecutable pack = comp.compile(new StreamSource(new StringReader(xsl1)));
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            pack.export(baos);

            Processor proc2 = new Processor(true);
            SchemaManager sm2 = proc2.getSchemaManager();
            sm2.importComponents(new StreamSource(new StringReader(scm.toString())));
            XsltCompiler comp2 = proc2.newXsltCompiler();
            comp2.setSchemaAware(true);
            XsltExecutable exec = comp2.loadExecutablePackage(
                    new StreamSource(new ByteArrayInputStream(baos.toByteArray())));
            Xslt30Transformer trans = exec.load30();
            StringWriter out = new StringWriter();
            trans.callTemplate(null, proc2.newSerializer(out));
            assertTrue(out.toString().contains("<test:e xmlns:test=\"http://www.example.com/test\"/>"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


}



