package jaxptest;

import com.saxonica.config.EnterpriseTransformerFactory;
import com.saxonica.config.ProfessionalTransformerFactory;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.jaxp.TransformerImpl;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.OutputURIResolver;
import net.sf.saxon.serialize.MessageEmitter;
import net.sf.saxon.serialize.MessageWarner;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.*;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.sax.SAXTransformerFactory;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
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
        tFactory.setFeature(Feature.ALLOW_MULTITHREADING.name, false);

        Source src = new StreamSource(new StringReader("<test/>"));

        Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(IDENTITY_STYLESHEET)));
        Transformer transformer = templates.newTransformer();

        final List<String> warnings = new ArrayList<String>();

        transformer.setErrorListener(new ErrorListener() {
            @Override
            public void error(TransformerException exception) throws TransformerException
            {
                System.err.println("error: " + exception.getMessage());
            }

            @Override
            public void fatalError(TransformerException exception) throws TransformerException
            {
                System.err.println("fatal error: " + exception.getMessage());
            }

            @Override
            public void warning(TransformerException exception) throws TransformerException
            {
                System.err.println("warning: " + exception.getMessage());
                warnings.add(exception.getMessage());
            }
        });

        MessageWarner mw = new MessageWarner();
        mw.setWriter(new StringWriter());
        ((TransformerImpl) transformer).getUnderlyingXsltTransformer().getUnderlyingController().setMessageEmitter(mw);

        transformer.transform(src, new StreamResult(System.out));

        assertTrue(warnings.size() == 1);
        assertTrue(warnings.get(0).equals("DONE"));

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

            for(int i = 0; i < iterations; i++) {

                Map<String, String> map = new HashMap<>();
                map.put("key", "value " + i);

                Transformer transformer = templates.newTransformer();
                transformer.setParameter("map", map);

                StringWriter resultWriter = new StringWriter();
                transformer.transform(new StreamSource(new StringReader("<input/>")),
                    new StreamResult(resultWriter));

                assertTrue(resultWriter.toString().contains("</results>"), "Results OK");

                if(i >= iterations - 2) {
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
                public void warning(TransformerException exception) throws TransformerException {
                    fail("warning");
                }

                @Override
                public void error(TransformerException exception) throws TransformerException {
                    fail("error");
                }

                @Override
                public void fatalError(TransformerException exception) throws TransformerException {
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
            final List<TransformerException> warnings = new ArrayList<TransformerException>();

            Transformer transformer = templates.newTransformer();
            transformer.setErrorListener(new ErrorListener() {

                @Override
                public void warning(TransformerException exception) throws TransformerException {
                    warnings.add(exception);
                }

                @Override
                public void error(TransformerException exception) throws TransformerException {
                    System.err.println("Error: " + exception.getMessage());
                    fail("unexpected error");
                }

                @Override
                public void fatalError(TransformerException exception) throws TransformerException {
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
        final List<TransformerException> errors = new ArrayList<TransformerException>();
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
                public void warning(TransformerException exception) throws TransformerException {
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
        final List<TransformerException> errors = new ArrayList<TransformerException>();
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
                public void warning(TransformerException exception) throws TransformerException {
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
            ((TransformerImpl)transformer).getUnderlyingController().setOutputURIResolver(
                    new OutputURIResolver() {
                        @Override
                        public OutputURIResolver newInstance() {
                            return this;
                        }

                        @Override
                        public Result resolve(String href, String base) throws TransformerException {
                            assertEquals("http://some.base.uri/", base);
                            return new StreamResult(secondary);
                        }

                        @Override
                        public void close(Result result) throws TransformerException {
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
            transFactory.setAttribute(Feature.ALLOW_MULTITHREADING.name, "false");
            //transFactory.setAttribute(FeatureKeys.MESSAGE_EMITTER_CLASS, "jaxptest.TransformMessageTest$MyMessageEmitter");

            Templates templates = transFactory.newTemplates(
                new SAXSource(new InputSource(new StringReader(stylesheet))));


            StreamResult result = new StreamResult(new StringWriter());
            final StringWriter messageOut = new StringWriter();
            Transformer transformer = templates.newTransformer();
            ((net.sf.saxon.jaxp.TransformerImpl)transformer).getUnderlyingController().setMessageEmitter(
                new MessageEmitter() {
                    @Override
                    public void open() throws XPathException {
                        setWriter(messageOut);
                        super.open();
                    }
                }
            );
            transformer.transform(new StreamSource(new StringReader("<in/>")), result);
            assertEquals("Hi!", messageOut.toString().trim());
        } catch (TransformerException e) {
            e.printStackTrace();
            fail();
        }


    }

    public static class MyMessageEmitter extends MessageEmitter {

        public MyMessageEmitter() throws XPathException {
        }

        public void setStringWriter(StringWriter sw) throws XPathException {
            setWriter(sw);
        }

    }






}
