////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2013 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is “Incompatible With Secondary Licenses”, as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.config.ProfessionalConfiguration;
import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.lib.StandardURIResolver;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import org.xml.sax.InputSource;
import org.xml.sax.SAXException;
import org.xml.sax.XMLFilter;
import org.xml.sax.XMLReader;
import org.xml.sax.helpers.XMLFilterImpl;
import org.xml.sax.helpers.XMLReaderFactory;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.Source;
import javax.xml.transform.TransformerException;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;
import java.util.Set;


/**
 * JUnit tests for the s9api XsltCompiler
 */
public class TestXsltCompiler extends TestCase {

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

    public void testXPath30Failure() {
        List<StaticError> errors = new ArrayList<>();
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
            for (StaticError se : errors) {
                codes.add(se.getErrorCode().getLocalName());
            }
            assertTrue(codes.contains("XPST0003"));
        }
    }

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

    public void testErrorNotificationFromParseError() {
        final List<TransformerException> warnings = new ArrayList<TransformerException>(4);
        final List<TransformerException> errors = new ArrayList<TransformerException>(4);
        final List<TransformerException> fatals = new ArrayList<TransformerException>(4);
        ErrorListener listener = new ErrorListener() {

            public void warning(TransformerException exception) throws TransformerException {
                warnings.add(exception);
            }

            public void error(TransformerException exception) throws TransformerException {
                errors.add(exception);
            }

            public void fatalError(TransformerException exception) throws TransformerException {
                fatals.add(exception);
            }

        };
        try {

            Processor proc = new Processor(true);
            proc.getUnderlyingConfiguration().setErrorListener(listener);

            XsltCompiler xqcomp = proc.newXsltCompiler();

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
            assertTrue("warnings", warnings.isEmpty());
            assertTrue("errors", errors.isEmpty());
            assertEquals("fatals", 1, fatals.size());
            //e.printStackTrace();
        }
    }

    public void testMultipleSyntaxErrors() {
        final List<TransformerException> warnings = new ArrayList<TransformerException>(4);
        final List<TransformerException> errors = new ArrayList<TransformerException>(4);
        final List<TransformerException> fatals = new ArrayList<TransformerException>(4);
        ErrorListener listener = new ErrorListener() {

            public void warning(TransformerException exception) throws TransformerException {
                warnings.add(exception);
            }

            public void error(TransformerException exception) throws TransformerException {
                errors.add(exception);
            }

            public void fatalError(TransformerException exception) throws TransformerException {
                fatals.add(exception);
            }

        };
        try {

            Processor proc = new Processor(true);
            proc.getUnderlyingConfiguration().setErrorListener(listener);

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
            assertTrue("warnings", warnings.isEmpty());
            assertTrue("errors", errors.isEmpty());
            assertTrue("fatals", fatals.size() == 5);
            //e.printStackTrace();
        }
    }

    public void testJITStaticErrors() {
        // Show how static errors can be detected at run-time using JIT
        final List<TransformerException> warnings = new ArrayList<>(4);
        final List<TransformerException> errors = new ArrayList<>(4);
        final List<TransformerException> fatals = new ArrayList<>(4);
        ErrorListener listener = new ErrorListener() {

            public void warning(TransformerException exception) throws TransformerException {
                warnings.add(exception);
            }

            public void error(TransformerException exception) throws TransformerException {
                errors.add(exception);
            }

            public void fatalError(TransformerException exception) throws TransformerException {
                fatals.add(exception);
            }

        };
        try {

            Processor proc = new Processor(true);
            proc.getUnderlyingConfiguration().setErrorListener(listener);

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
                assertTrue("warnings", warnings.isEmpty());
                assertTrue("errors", errors.isEmpty());
                assertTrue("fatals", fatals.isEmpty());
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
            assertTrue("warnings", warnings.isEmpty());
            assertTrue("errors", errors.isEmpty());
            assertEquals("fatals", 1, fatals.size());
            //e.printStackTrace();
        }
    }


    /**
     * Test location information for import errors
     */

    public void testErrorNotificationFromImportError() {
        final List<TransformerException> warnings = new ArrayList<TransformerException>(4);
        final List<TransformerException> errors = new ArrayList<TransformerException>(4);
        final List<TransformerException> fatals = new ArrayList<TransformerException>(4);
        ErrorListener listener = new ErrorListener() {

            public void warning(TransformerException exception) throws TransformerException {
                System.out.println("Warning: " + exception + "\nlocation: " + exception.getLocator());
                warnings.add(exception);
            }

            public void error(TransformerException exception) throws TransformerException {
                System.out.println("Error: " + exception + "\nlocation: " + exception.getLocator());
                errors.add(exception);
            }

            public void fatalError(TransformerException exception) throws TransformerException {
                if (!exception.getMessage().contains("No such file or directory")) {
                    System.out.println("Fatal: " + exception + "\nlocation: " + exception.getLocator());
                }
                assertEquals(exception.getLocator().getLineNumber(), 3);
                fatals.add(exception);
            }

        };
        try {

            Processor proc = new Processor(true);
            proc.getUnderlyingConfiguration().setErrorListener(listener);

            XsltCompiler xqcomp = proc.newXsltCompiler();

            StringReader sr = new StringReader("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n" +
                "   <xsl:import href=\"invalidRef.xsl\"/>\n" +
                "</xsl:stylesheet>");


            xqcomp.setErrorListener(listener);
            xqcomp.compile(new StreamSource(sr));

            fail("Compilation should fail");

        } catch (SaxonApiException e) {
            assertTrue("warnings", warnings.isEmpty());
            assertTrue("errors", errors.isEmpty());
            assertTrue("fatals", fatals.size() == 1);
            //e.printStackTrace();
        }
    }

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
                compiler.compile(new StreamSource(new StringReader(stylesheet), "http://localhost/stylesheet"));
                System.err.println("Stylesheet compilation succeeded");
            } catch (SaxonApiException e) {
                System.err.println("Stylesheet compilation failed");
            }


    }

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
            assertEquals(progress.size(), 1);
            assertEquals(progress.get(0), "started");
        } catch (SaxonApiException e) {
            fail("Stylesheet compilation failed");
        } catch (SAXException e) {
            fail();
        }
    }

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
        final List<String> counter = new ArrayList<String>();
        p.getUnderlyingConfiguration().setURIResolver(new StandardURIResolver() {
            @Override
            public Source resolve(String href, String base) throws XPathException {
                switch (href) {
                    case "http://example.com/bad":
                        String badXml = "<bad>";
                        counter.add(href);
                        return new StreamSource(new StringReader(badXml));
                    case "http://example.com/good":
                        String goodXml = "<good/>";
                        counter.add(href);
                        return new StreamSource(new StringReader(goodXml));
                    default:
                        return super.resolve(href, base);
                }
            }

            ;
        });
        p.setConfigurationProperty(Feature.PRE_EVALUATE_DOC_FUNCTION, false);
        XsltExecutable q = null;
        try {
            c.setParameter(new QName("call"), new XdmAtomicValue(badQuery));
            q = c.compile(new StreamSource(new StringReader(stylesheet)));
        } catch (Exception err) {
            err.printStackTrace();
            fail("Compilation 1 should not fail");
        }
        assertEquals("URI not accessed", 0, counter.size());
        p.setConfigurationProperty(Feature.PRE_EVALUATE_DOC_FUNCTION, true);
        try {
            c.setParameter(new QName("call"), new XdmAtomicValue(goodQuery));
            q = c.compile(new StreamSource(new StringReader(stylesheet)));
            assertTrue("URI accessed", counter.size() > 0);
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


    public void testJitEEOn() {
        testJit(new EnterpriseConfiguration(), "10", false);
    }

    public void testJitEEOnJ() {
        testJit(new EnterpriseConfiguration(), "j", false);
    }

    public void testJitEEOff() {
        testJit(new EnterpriseConfiguration(), "0", true);
    }

    public void testJitEEOffJ() {
        testJit(new EnterpriseConfiguration(), "-j", true);
    }

    public void testJitPEOn() {
        testJit(new ProfessionalConfiguration(), "10", true);
    }

    public void testJitPEOff() {
        testJit(new ProfessionalConfiguration(), "0", true);
    }

    public void testJitHEOn() {
        testJit(new Configuration(), "10", true);
    }

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

    public void testErrorNotificationFromImportSchemaError() {
        final List<TransformerException> warnings = new ArrayList<>(4);
        final List<TransformerException> errors = new ArrayList<>(4);
        final List<TransformerException> fatals = new ArrayList<>(4);
        ErrorListener listener = new ErrorListener() {

            public void warning(TransformerException exception) throws TransformerException {
                warnings.add(exception);
            }

            public void error(TransformerException exception) throws TransformerException {
                errors.add(exception);
            }

            public void fatalError(TransformerException exception) throws TransformerException {
                fatals.add(exception);
            }

        };
        try {

            Processor proc = new Processor(true);
            
            XsltCompiler xqcomp = proc.newXsltCompiler();

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
            xqcomp.setErrorListener(listener);
            xqcomp.compile(new StreamSource(sr));

            fail("Compilation should fail");

        } catch (SaxonApiException e) {
            assertTrue("warnings", warnings.isEmpty());
            assertFalse("errors", errors.isEmpty());
            assertEquals("fatals", 1, fatals.size());
            //e.printStackTrace();
        }
    }

    public void testErrorNotificationFromInlineSchemaError() {
        final List<TransformerException> warnings = new ArrayList<>(4);
        final List<TransformerException> errors = new ArrayList<>(4);
        final List<TransformerException> fatals = new ArrayList<>(4);
        ErrorListener listener = new ErrorListener() {

            public void warning(TransformerException exception)  {
                warnings.add(exception);
            }

            public void error(TransformerException exception)  {
                errors.add(exception);
            }

            public void fatalError(TransformerException exception)  {
                fatals.add(exception);
            }

        };
        try {

            Processor proc = new Processor(true);

            XsltCompiler xqcomp = proc.newXsltCompiler();

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
            xqcomp.setErrorListener(listener);
            xqcomp.compile(new StreamSource(sr));

            fail("Compilation should fail");

        } catch (SaxonApiException e) {
            assertTrue("warnings", warnings.isEmpty());
            assertEquals("errors", 3, errors.size());
            assertEquals("fatals", 1, fatals.size());
            //e.printStackTrace();
        }
    }

    public void testWarningNotificationFromInlineSchema() {
        final List<TransformerException> warnings = new ArrayList<>(4);
        final List<TransformerException> errors = new ArrayList<>(4);
        final List<TransformerException> fatals = new ArrayList<>(4);
        ErrorListener listener = new ErrorListener() {

            public void warning(TransformerException exception) {
                warnings.add(exception);
            }

            public void error(TransformerException exception) {
                errors.add(exception);
            }

            public void fatalError(TransformerException exception) {
                fatals.add(exception);
            }

        };
        try {

            Processor proc = new Processor(true);

            XsltCompiler xqcomp = proc.newXsltCompiler();

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
            xqcomp.setErrorListener(listener);
            xqcomp.compile(new StreamSource(sr));

            fail("Compilation should fail");

        } catch (SaxonApiException e) {
            assertEquals("warnings", 1, warnings.size());
            assertTrue("errors", errors.isEmpty());
            assertEquals("fatals", 1, fatals.size());
            //e.printStackTrace();
        }
    }


}



