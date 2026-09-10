package jaxptest;

import com.saxonica.config.EnterpriseTransformerFactory;
import com.saxonica.config.ProfessionalTransformerFactory;
import net.sf.saxon.Configuration;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.jaxp.TransformerImpl;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.OutputURIResolver;
import net.sf.saxon.lib.StandardMessageHandler;
import net.sf.saxon.s9api.Message;
import net.sf.saxon.serialize.UTF8Writer;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.Result;
import javax.xml.transform.Source;
import javax.xml.transform.Templates;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.sax.SAXTransformerFactory;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayOutputStream;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


public class TransformMessageTest {

    private static final String IDENTITY_STYLESHEET
            = "<?xml version=\"1.0\"?> \n" +
            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
            "<xsl:template match='test'><xsl:apply-templates/><xsl:message>DONE</xsl:message></xsl:template>\n" +
            "<xsl:template match=\"@*|node()\">\n" +
            "<xsl:copy>\n" +
            "  <xsl:apply-templates select=\"@*|node()\" /> \n" +
            "</xsl:copy>\n" +
            "</xsl:template>\n" +
            "</xsl:stylesheet>";


    @Test
    public void testMessageWarner() throws Exception {

        SAXTransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
        tFactory.setFeature(FeatureKeys.ALLOW_MULTITHREADING, false);

        Source src = new StreamSource(new StringReader("<test/>"));

        Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(IDENTITY_STYLESHEET)));
        Transformer transformer = templates.newTransformer();

        MyMessageHandler mmh = new MyMessageHandler(((TransformerImpl) transformer).getConfiguration());
        ((TransformerImpl) transformer).getUnderlyingXsltTransformer().getUnderlyingController().setMessageHandler(mmh);
        transformer.transform(src, new StreamResult(System.out));

        final List<String> warnings = mmh.getWarnings();

        assertEquals(warnings.size(), 1);
        assertEquals(warnings.get(0), "DONE");

    }

    @Test
    public void testTypeHierarchyLeak() {
        try {
            String stylesheet =
                    "<?xml version='1.0'?>" +
                            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                            "    xmlns:xs='http://www.w3.org/2001/XMLSchema'" +
                            "    xmlns:map='java:java.util.Map' xmlns:jt='http://saxon.sf.net/java-type'" +
                            "    version='2.0'>" +
                            "" +
                            "  <xsl:param name='map' as='jt:java.util.Map'/>" +
                            "" +
                            "  <xsl:output method='xml'/>" +
                            "" +
                            "  <xsl:template match='/'>" +
                            "    <results>" +
                            "      <result>" +
                            "        <xsl:value-of select=\"map:get($map, 'key')\"/>" +
                            "      </result>" +
                            "    </results>" +
                            "  </xsl:template>" +
                            "</xsl:stylesheet>";

            TransformerFactory transFactory = new ProfessionalTransformerFactory();
            Templates templates = transFactory.newTemplates(
                    new SAXSource(new InputSource(new StringReader(stylesheet))));

            final int iterations = 1000;

            for (int i = 0; i < iterations; i++) {

                Map<String, String> map = new HashMap<>();
                map.put("key", "value " + i);

                Transformer transformer = templates.newTransformer();
                transformer.setParameter("map", map);

                StringWriter resultWriter = new StringWriter();
                transformer.transform(new StreamSource(new StringReader("<input/>")),
                        new StreamResult(resultWriter));

                assertTrue(resultWriter.toString().contains("</results>"), "Results OK");

                if (i >= iterations - 2) {
                    System.out.println("Breakpoint here to check memory " +
                            "consumption of template on last two runs.");
                }

            }
        } catch (TransformerException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testSilentRecovery() {
        try {
            String stylesheet =
                    "<?xml version='1.0'?>" +
                            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                            "    version='2.0'><xsl:mode warning-on-multiple-match='false'/>" +
                            "  <xsl:template match='/'>first</xsl:template>" +
                            "  <xsl:template match='/'>second</xsl:template>" +
                            "</xsl:stylesheet>";

            TransformerFactory transFactory = new TransformerFactoryImpl();
            Templates templates = transFactory.newTemplates(
                    new SAXSource(new InputSource(new StringReader(stylesheet))));

            Transformer transformer = templates.newTransformer();
            transformer.setErrorListener(new ErrorListener() {

                @Override
                public void warning(TransformerException exception) {
                    fail("warning");
                }

                @Override
                public void error(TransformerException exception) {
                    fail("error");
                }

                @Override
                public void fatalError(TransformerException exception) {
                    fail("fatalError");
                }
            });

            StringWriter resultWriter = new StringWriter();
            transformer.transform(new StreamSource(new StringReader("<input/>")),
                    new StreamResult(resultWriter));

            assertTrue(resultWriter.toString().contains("second"), "Results OK");


        } catch (TransformerException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testWarningRecovery() {
        try {
            String stylesheet =
                    "<?xml version='1.0'?>" +
                            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                            "    version='2.0'><xsl:mode warning-on-multiple-match='true'/>" +
                            "  <xsl:template match='/'>first</xsl:template>" +
                            "  <xsl:template match='/'>second</xsl:template>" +
                            "</xsl:stylesheet>";

            TransformerFactory transFactory = new TransformerFactoryImpl();
            Templates templates = transFactory.newTemplates(
                    new SAXSource(new InputSource(new StringReader(stylesheet))));
            final List<TransformerException> warnings = new ArrayList<>();

            Transformer transformer = templates.newTransformer();
            transformer.setErrorListener(new ErrorListener() {

                @Override
                public void warning(TransformerException exception) {
                    warnings.add(exception);
                }

                @Override
                public void error(TransformerException exception) {
                    System.err.println("Error: " + exception.getMessage());
                    fail("unexpected error");
                }

                @Override
                public void fatalError(TransformerException exception) {
                    fail("fatalError");
                }
            });

            StringWriter resultWriter = new StringWriter();
            transformer.transform(new StreamSource(new StringReader("<input/>")),
                    new StreamResult(resultWriter));

            assertTrue(resultWriter.toString().contains("second"), "Results OK");
            assertEquals(1, warnings.size(), "Warnings");


        } catch (TransformerException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testNonRecovery() {
        final List<TransformerException> errors = new ArrayList<>();
        try {
            String stylesheet =
                    "<?xml version='1.0'?>" +
                            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                            "    version='2.0'><xsl:mode on-multiple-match='fail'/>" +
                            "  <xsl:template match='/'>first</xsl:template>" +
                            "  <xsl:template match='/'>second</xsl:template>" +
                            "</xsl:stylesheet>";

            TransformerFactory transFactory = new TransformerFactoryImpl();
            transFactory.setAttribute("http://saxon.sf.net/feature/recoveryPolicy", 2);
            Templates templates = transFactory.newTemplates(
                    new SAXSource(new InputSource(new StringReader(stylesheet))));


            Transformer transformer = templates.newTransformer();
            transformer.setErrorListener(new ErrorListener() {

                @Override
                public void warning(TransformerException exception) {
                    fail("warning");
                }

                @Override
                public void error(TransformerException exception) throws TransformerException {
                    errors.add(exception);
                    throw exception;
                }

                @Override
                public void fatalError(TransformerException exception) throws TransformerException {
                    errors.add(exception);
                    throw exception;
                }
            });

            StringWriter resultWriter = new StringWriter();
            transformer.transform(new StreamSource(new StringReader("<input/>")),
                    new StreamResult(resultWriter));

            fail("should not succeed");


        } catch (TransformerException e) {
            assertEquals(1, errors.size(), "Warnings");
        }
    }

    @Test
    public void testErrorNotificationSchemaAware() {
        final List<TransformerException> errors = new ArrayList<>();
        try {

            String stylesheet =
                    "<?xml version='1.0'?>" +
                            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                            "    version='2.0'>" +
                            "  <xsl:import-schema namespace='http://alien.com/'/>" +
                            "  <xsl:template match='/'>first</xsl:template>" +
                            "  <xsl:template match='schema-element(abc)'>second</xsl:template>" +
                            "</xsl:stylesheet>";
            InputSource inputSource = new InputSource(new StringReader(stylesheet));
            Source source = new SAXSource(inputSource);

            // Enterprise transformer factory with schema aware processing activated
            TransformerFactory transformerFactory = new EnterpriseTransformerFactory();
            transformerFactory.setAttribute(net.sf.saxon.lib.FeatureKeys.XSLT_SCHEMA_AWARE, true);
            transformerFactory.setErrorListener(new ErrorListener() {

                @Override
                public void warning(TransformerException exception) {
                    assertTrue(exception.getMessage().contains("No schema for this namespace"));
                }

                @Override
                public void error(TransformerException exception) throws TransformerException {
                    errors.add(exception);
                    throw exception;
                }

                @Override
                public void fatalError(TransformerException exception) throws TransformerException {
                    if (errors.isEmpty()) {
                        assertTrue(exception.getMessage().contains("No schema has been imported"));
                    }
                    errors.add(exception);
                    throw exception;
                }
            });

// Create transformer
            Transformer transformer = transformerFactory.newTransformer(source);
            assertTrue(errors.get(0).getMessage().contains("No schema has been imported"));
            fail("Compilation should fail");
        } catch (TransformerException e) {
            //assertTrue("warnings", warnings.isEmpty());
            assertFalse(errors.isEmpty(), "errors");

        }
    }

    @Test
    public void testBaseOutputUri() {
        try {
            String stylesheet =
                    "<?xml version='1.0'?>" +
                            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                            "    version='2.0'>" +
                            "  <xsl:template match='/'>" +
                            "<first/>" +
                            "<xsl:result-document href='second.xml'><second/></xsl:result-document>" +
                            "  </xsl:template>" +
                            "</xsl:stylesheet>";

            TransformerFactory transFactory = new TransformerFactoryImpl();
            Templates templates = transFactory.newTemplates(
                    new SAXSource(new InputSource(new StringReader(stylesheet))));


            Transformer transformer = templates.newTransformer();
            final StringWriter secondary = new StringWriter();
            ((TransformerImpl) transformer).getUnderlyingController().setOutputURIResolver(
                    new OutputURIResolver() {
                        @Override
                        public OutputURIResolver newInstance() {
                            return this;
                        }

                        @Override
                        public Result resolve(String href, String base) {
                            assertEquals("http://some.base.uri/", base);
                            return new StreamResult(secondary);
                        }

                        @Override
                        public void close(Result result) {
                            assertTrue(secondary.toString().contains("<second/>"));
                        }
                    }
            );

            StringWriter resultWriter = new StringWriter();
            Result result = new StreamResult(resultWriter);
            result.setSystemId("http://some.base.uri/");
            transformer.transform(new StreamSource(new StringReader("<input/>")), result);
            assertTrue(secondary.toString().contains("<second/>"));


        } catch (TransformerException e) {
            fail();
            e.printStackTrace();
        }
    }

    @Test
    public void testMessageCapture() {
        try {
            String stylesheet =
                    "<?xml version='1.0'?>" +
                            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'" +
                            "    version='2.0'>" +
                            "  <xsl:template match='/'>" +
                            "<first/>" +
                            "<xsl:message>Hi!</xsl:message>" +
                            "  </xsl:template>" +
                            "</xsl:stylesheet>";

            TransformerFactory transFactory = new TransformerFactoryImpl();
            transFactory.setAttribute(FeatureKeys.ALLOW_MULTITHREADING, "false");

            Templates templates = transFactory.newTemplates(
                    new SAXSource(new InputSource(new StringReader(stylesheet))));

            StreamResult result = new StreamResult(new StringWriter());
            Transformer transformer = templates.newTransformer();

            StandardMessageHandler handler = new StandardMessageHandler(
                    ((net.sf.saxon.jaxp.TransformerImpl) transformer).getConfiguration());
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            UTF8Writer messageWriter = new UTF8Writer(baos);
            handler.setUnicodeWriter(messageWriter);

            ((net.sf.saxon.jaxp.TransformerImpl) transformer).getUnderlyingController().setMessageHandler(handler);

            transformer.transform(new StreamSource(new StringReader("<in/>")), result);

            assertEquals("Hi!", baos.toString().trim());
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }
    }

    static class MyMessageHandler extends StandardMessageHandler {
        ArrayList<String> warnings = new ArrayList<String> ();
        public MyMessageHandler(Configuration config) {
            super(config);
        }

        public List<String> getWarnings() {
            return warnings;
        }

        @Override
        public synchronized void accept(Message message) {
            super.accept(message);
            if (!message.isTerminate()) {
                warnings.add(message.getContent().toString());
            }
        }
    }
}
