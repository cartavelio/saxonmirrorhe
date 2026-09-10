package jaxptest;

import com.saxonica.config.ProfessionalTransformerFactory;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.jaxp.TransformerImpl;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.OutputURIResolver;
import org.junit.jupiter.api.Disabled;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.*;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Test for s9api serialization
 */
public class SerializerTests {
    private final ConfigTest configTest = new ConfigTest();

    @Test
    public void testTemplatesOutputProperties() {
        try {
            // Create stylesheet with call to extension function

            String stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                            "<xsl:output method='xml' indent='no' encoding='utf-8'/>" +
                            "<xsl:template match='/'>\n" +
                            "<xsl:value-of select='17'/>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Properties props = templates.getOutputProperties();
            assertEquals("xml", props.getProperty("method"), "method");
            assertEquals("no", props.getProperty("indent"), "indent");
            assertNull(props.getProperty("doctype-system"), "doctype-system");
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testUserOutputMethod() {
        try {
            // Create stylesheet with user-defined serialization method

            String stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:my='http://my.com/'>\n" +
                            "<xsl:output method='my:jaxptest.MyEmitter' indent='no' encoding='utf-8'/>" +
                            "<xsl:template match='/'>\n" +
                            "<xsl:value-of select='17'/>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer trans = templates.newTransformer();
            StreamSource input = new StreamSource(new StringReader(stylesheet));
            StringWriter output = new StringWriter();
            StreamResult result = new StreamResult(output);
            trans.transform(input, result);
            assertTrue(output.toString().contains("17"), "output1");
            assertTrue(output.toString().contains("<!--custom-emitter-->"), "output2");
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testProperties() throws TransformerException {
        TransformerFactory tfactory = TransformerFactory.newInstance();
        String xsl = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
                "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                " version=\"2.0\">\n" +
                " <xsl:output method=\"text\" encoding=\"UTF-8\" />\n" +
                "</xsl:stylesheet>";

        Templates t = tfactory.newTemplates(new StreamSource(new StringReader(xsl)));
        Transformer transformer = t.newTransformer();

        String xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                "<test></test>";

        StreamSource source = new StreamSource(new StringReader(xml));

        // output
        StreamResult result = new StreamResult(new ByteArrayOutputStream());

        // transform
        transformer.transform(source, result);

        assertEquals("text", transformer.getOutputProperties().getProperty("method"));
    }


    @Test
    public void testUserOutputMethodSecondary() {
        try {
            // Create stylesheet with user-defined serialization method used for xsl:result-document

            String stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:my='http://my.com/'>\n" +
                            "<xsl:output name='x' method='my:jaxptest.MyEmitter' indent='no' encoding='utf-8'/>" +
                            "<xsl:template match='/'>\n" +
                            "<xsl:result-document format='x'>\n" +
                            "<xsl:value-of select='17'/>\n" +
                            "</xsl:result-document>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer trans = templates.newTransformer();
            StreamSource input = new StreamSource(new StringReader(stylesheet));
            StringWriter output = new StringWriter();
            final StreamResult result = new StreamResult(output);
            result.setSystemId("http://dummy.com/");
            trans.transform(input, result);
            assertTrue(output.toString().contains("17"), "output1");
            assertTrue(output.toString().contains("<!--custom-emitter-->"), "output2");
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testOutputUriResolverClose() {
        try {
            // Test that calls on OutputURIResolver are correctly made

            String stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:my='http://my.com/'>\n" +
                            "<xsl:output name='x' method='xml' indent='no' encoding='utf-8'/>" +
                            "<xsl:template match='/'>\n" +
                            "<xsl:for-each select='1 to 5'>" +
                            "<xsl:result-document format='x' href='out{.}'>\n" +
                            "<xsl:value-of select='17 + .'/>\n" +
                            "</xsl:result-document>\n" +
                            "</xsl:for-each>" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            tFactory.setAttribute(FeatureKeys.ALLOW_MULTITHREADING, false);
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer trans = templates.newTransformer();
            StreamSource input = new StreamSource(new StringReader(stylesheet));
            StringWriter output = new StringWriter();
            final StreamResult result = new StreamResult(output);
            result.setSystemId("http://dummy.com/");
            final StringBuilder openCalls = new StringBuilder();
            final StringBuilder closeCalls = new StringBuilder();
            ((TransformerImpl) trans).getUnderlyingController().setOutputURIResolver(new OutputURIResolver() {
                @Override
                public OutputURIResolver newInstance() {
                    return this;
                }

                @Override
                public Result resolve(String href, String base)  {
                    openCalls.append(href).append(' ');
                    StreamResult result = new StreamResult(new StringWriter());
                    result.setSystemId(href);
                    return result;
                }

                @Override
                public void close(Result result)  {
                    closeCalls.append(result.getSystemId()).append(' ');
                }
            });
            trans.transform(input, result);
            assertTrue(openCalls.toString().equals("out1 out2 out3 out4 out5 "));
            assertTrue(closeCalls.toString().equals("out1 out2 out3 out4 out5 "));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testIllegalUri() {
        // It's unclear to ndw how platform-specific this test might be.
        try {
            String stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:my='http://my.com/'>\n" +
                            "<xsl:output name='x' method='my:jaxptest.MyEmitter' indent='no' encoding='utf-8'/>" +
                            "<xsl:template match='/'>\n" +
                            "<xsl:result-document format='x'>\n" +
                            "<xsl:value-of select='17'/>\n" +
                            "</xsl:result-document>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer trans = templates.newTransformer();
            StreamSource input = new StreamSource(new StringReader(stylesheet));
            String s = configTest.getTempDir() + ".\\config\\testIllegalUriOutput.xml";
            StreamResult result = new StreamResult(new File(s));
            trans.transform(input, result);
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testExtensionAttribute() {
        // Bug 2364
        try {
            String stylesheet =
                "<?xml version=\"1.0\"?> \n" +
                    "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:my='http://my.com/'>\n" +
                    "<xsl:output indent='no' encoding='utf-8' xml:space='preserve'/>" +
                    "<xsl:template match='/'>\n" +
                    "<foo><bar/></foo>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer trans = templates.newTransformer();
            String encoding = trans.getOutputProperty("encoding");
            System.out.println("Encoding " + encoding);
            StreamSource input = new StreamSource(new StringReader(stylesheet));
            StreamResult result = new StreamResult(System.out);
            trans.transform(input, result);

        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }




    @Test
    public void testRequireWellFormedBad1() {
        try {
            // Create stylesheet with user-defined emitter. The require-well-formed serialization property
            // has no effect in this situation.

            String stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:my='http://my.com/'>\n" +
                            "<xsl:output method='my:jaxptest.MyEmitter' indent='no' encoding='utf-8' saxon:require-well-formed='yes' xmlns:saxon='http://saxon.sf.net/'/>" +
                            "<xsl:template match='/'>\n" +
                            "<xsl:value-of select='17'/>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer trans = templates.newTransformer();
            StreamSource input = new StreamSource(new StringReader(stylesheet));
            StringWriter output = new StringWriter();
            StreamResult result = new StreamResult(output);
            trans.transform(input, result);
            assertTrue(output.toString().contains("17"), "output1");
            assertTrue(output.toString().contains("<!--custom-emitter-->"), "output2");
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSAXRequireWellFormedOK() {
        try {
            // Create stylesheet with user-defined SAX content handler. Output must be well-formed (and is)

            String stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:my='http://my.com/'>\n" +
                            "<xsl:output method='my:jaxptest.MyContentHandler' indent='no' encoding='utf-8' saxon:require-well-formed='yes' xmlns:saxon='http://saxon.sf.net/'/>" +
                            "<xsl:template match='/'>\n" +
                            "<e><xsl:value-of select='17'/></e>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer trans = templates.newTransformer();
            StreamSource input = new StreamSource(new StringReader(stylesheet));
            StringWriter output = new StringWriter();
            StreamResult result = new StreamResult(output);
            trans.transform(input, result);
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSAXRequireWellFormedBad1() {
        try {
            // Create stylesheet with user-defined emitter. The require-well-formed serialization property
            // has no effect in this situation.

            String stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:my='http://my.com/'>\n" +
                            "<xsl:output method='my:jaxptest.MyContentHandler' indent='no' encoding='utf-8' saxon:require-well-formed='yes' xmlns:saxon='http://saxon.sf.net/'/>" +
                            "<xsl:template match='/'>\n" +
                            "<xsl:value-of select='17'/>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer trans = templates.newTransformer();
            StreamSource input = new StreamSource(new StringReader(stylesheet));
            StringWriter output = new StringWriter();
            StreamResult result = new StreamResult(output);
            trans.transform(input, result);
            fail("Error not detected");
        } catch (Exception e) {
            // OK
        }
    }


    @Test
    public void testSAXRequireWellFormedBad2() {
        try {
            // Create stylesheet with user-defined serialization method. Output must be well-formed (and is not)

            String stylesheet =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:my='http://my.com/'>\n" +
                            "<xsl:output method='my:jaxptest.MyContentHandler' indent='no' encoding='utf-8' saxon:require-well-formed='yes' xmlns:saxon='http://saxon.sf.net/'/>" +
                            "<xsl:template match='/'>\n" +
                            "<a/><b/>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer trans = templates.newTransformer();
            StreamSource input = new StreamSource(new StringReader(stylesheet));
            StringWriter output = new StringWriter();
            StreamResult result = new StreamResult(output);
            trans.transform(input, result);
            fail("Error not detected");
        } catch (Exception e) {
            // all is well
        }
    }

    @Test
    public void testNonBMP() throws Exception {

        TransformerFactory tf = new net.sf.saxon.TransformerFactoryImpl();

        Source templateSource = new StreamSource(new FileInputStream(configTest.getDataDir() + "davis/formatter.xsl"));

        Transformer t = tf.newTransformer(templateSource);

        Source inputSource = new StreamSource(new FileInputStream(configTest.getDataDir() + "davis/inputData.xml"));
        ByteArrayOutputStream os = new ByteArrayOutputStream();
        Result result = new StreamResult(os);
        t.transform(inputSource, result);
        os.close();
        String out = os.toString("utf-8");
        System.out.println(out);
        assertFalse(out.contains("&#"));
    }

    @Test
    public void testNextInChain() {
        try {
            String stylesheet1 =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                            "<xsl:output method='xml' saxon:next-in-chain='next.xsl' xmlns:saxon='http://saxon.sf.net/'/>" +
                            "<xsl:template match='/'>\n" +
                            "<a/>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            final String stylesheet2 =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                            "<xsl:template match='/'>\n" +
                            "<b><xsl:copy-of select='.'/></b>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            tFactory.setURIResolver((href, base) -> {
                if (href.endsWith("next.xsl")) {
                    return new StreamSource(new StringReader(stylesheet2));
                } else {
                    return null;
                }
            });
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet1)));
            Transformer trans = templates.newTransformer();
            StreamSource input = new StreamSource(new StringReader(stylesheet1));
            StringWriter output = new StringWriter();
            StreamResult result = new StreamResult(output);
            trans.transform(input, result);
            assertTrue(output.toString().contains("<b><a/></b>"), "output");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    // Error case - target stylesheet does not exist (base URI unknown)

    @Test
    public void testNextInChainError() {
        //noinspection CatchMayIgnoreException
        try {
            String stylesheet1 =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                            "<xsl:output method='xml' saxon:next-in-chain='next.xsl' xmlns:saxon='http://saxon.sf.net/'/>" +
                            "<xsl:template match='/'>\n" +
                            "<a/>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            final String stylesheet2 =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                            "<xsl:template match='/'>\n" +
                            "<b><xsl:copy-of select='.'/></b>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet1)));
            Transformer trans = templates.newTransformer();
            StreamSource input = new StreamSource(new StringReader(stylesheet1), "file://unknown-dir/base/");
            StringWriter output = new StringWriter();
            StreamResult result = new StreamResult(output);
            trans.transform(input, result);
            fail("failed to fail");
        } catch (TransformerException e) {
            assertTrue(e.getCause() instanceof IOException);
        }
    }

    @Test
    public void testNextInChainResultDoc() {
        try {
            String stylesheet1 =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                            "<xsl:output name='n' method='xml' saxon:next-in-chain='next.xsl' xmlns:saxon='http://saxon.sf.net/'/>" +
                            "<xsl:template match='/'>\n" +
                            "<xsl:result-document format='n'><a/></xsl:result-document>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            final String stylesheet2 =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                            "<xsl:template match='/'>\n" +
                            "<b><xsl:copy-of select='.'/></b>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            tFactory.setURIResolver((href, base) -> {
                if (href.endsWith("next.xsl")) {
                    return new StreamSource(new StringReader(stylesheet2));
                } else {
                    return null;
                }
            });
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet1)));
            Transformer trans = templates.newTransformer();
            StreamSource input = new StreamSource(new StringReader(stylesheet1));
            StringWriter output = new StringWriter();
            StreamResult result = new StreamResult(output);
            result.setSystemId("http://dummy/");
            ((TransformerImpl)trans).getUnderlyingController().setOutputURIResolver(new OutputURIResolver() {
                @Override
                public OutputURIResolver newInstance() {
                    return this;
                }

                @Override
                public Result resolve(String href, String base) throws TransformerException {
                    return result;
                }

                @Override
                public void close(Result result) throws TransformerException {

                }
            });
            trans.transform(input, result);
            assertTrue(output.toString().contains("<b><a/></b>"), "output");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testNextInChainNamedResultDoc() {
        try {
            String stylesheet1 =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                            "<xsl:output name='n' method='xml' saxon:next-in-chain='next.xsl' xmlns:saxon='http://saxon.sf.net/'/>" +
                            "<xsl:template match='/'>\n" +
                            "<xsl:result-document href='out.xml' format='n'><a/></xsl:result-document>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            final String stylesheet2 =
                    "<?xml version=\"1.0\"?> \n" +
                            "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                            "<xsl:template match='/'>\n" +
                            "<b><xsl:copy-of select='.'/></b>\n" +
                            "</xsl:template>\n" +
                            "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            tFactory.setURIResolver((href, base) -> {
                if (href.endsWith("next.xsl")) {
                    return new StreamSource(new StringReader(stylesheet2));
                } else {
                    return null;
                }
            });
            final StringWriter sw = new StringWriter();
            tFactory.setAttribute(FeatureKeys.OUTPUT_URI_RESOLVER, new OutputURIResolver() {
                @Override
                public OutputURIResolver newInstance() {
                    return this;
                }

                @Override
                public Result resolve(String href, String base) {
                    if (href.endsWith("out.xml")) {
                        return new StreamResult(sw);
                    } else {
                        return null;
                    }
                }

                @Override
                public void close(Result result) {
                    //
                }
            });
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet1)));
            Transformer trans = templates.newTransformer();
            StreamSource input = new StreamSource(new StringReader(stylesheet1));
            StringWriter output = new StringWriter();
            StreamResult result = new StreamResult(output);
            trans.transform(input, result);
            assertTrue(sw.toString().contains("<b><a/></b>"), "output");
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    // We need to run this junit test on different platforms to make sure we get the desired outcome on all of them.
    @Test
    public void testOutFileClosed() throws Exception {
        String stylesheet =
                "<?xml version=\"1.0\"?> \n" +
                        "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:my='http://my.com/'>\n" +
                        "<xsl:output method='my:jaxptest.MyContentHandler' indent='no' encoding='utf-8' saxon:require-well-formed='yes' xmlns:saxon='http://saxon.sf.net/'/>" +
                        "<xsl:template match='/'>\n" +
                        "<a/>\n" +
                        "</xsl:template>\n" +
                        "</xsl:stylesheet>";
        String xslPath = "test.xsl";
        String xmlPath = "test.xml";
        String outPath = configTest.getDataDir() + "output.xml";
        // Create transformer
        File xslFile = new File(xslPath);
        InputSource inputSource = new InputSource(xslFile.toURI().toString());

        TransformerFactory transformerFactory = new ProfessionalTransformerFactory();
        Transformer transformer = transformerFactory.newTransformer(new StreamSource(new StringReader(stylesheet)));

        // Create XML SAX source
        StreamSource input = new StreamSource(new StringReader(stylesheet));
        // Output stream result
        File outFile = new File(outPath);
        Result outputTarget = new StreamResult(outFile);

        // Run transformation
        System.out.println("Run transformation...");
        transformer.transform(input, outputTarget);
        assertTrue(outFile.exists(), "Output file exists: ");
        assertTrue(outFile.delete(), "Can the output file be deleted? ");
    }


    @Test
    public void testRequireWellFormedOK() {
        try {
            // Create stylesheet with user-defined serialization method. Output must be well-formed (and is)

            String stylesheet =
                "<?xml version=\"1.0\"?> \n" +
                    "<xsl:stylesheet version=\"1.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:my='http://my.com/'>\n" +
                    "<xsl:output method='my:jaxptest.MyEmitter' indent='no' encoding='utf-8' saxon:require-well-formed='yes' xmlns:saxon='http://saxon.sf.net/'/>" +
                    "<xsl:template match='/'>\n" +
                    "<e><xsl:value-of select='17'/></e>\n" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>";

            TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
            Templates templates = tFactory.newTemplates(new StreamSource(new StringReader(stylesheet)));
            Transformer trans = templates.newTransformer();
            StreamSource input = new StreamSource(new StringReader(stylesheet));
            StringWriter output = new StringWriter();
            StreamResult result = new StreamResult(output);
            trans.transform(input, result);
            assertTrue(output.toString().contains("17"), "output1");
            assertTrue(output.toString().contains("<!--custom-emitter-->"), "output2");
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }



    @Test
    public void testNextInChainPisoiTest() {
        try {
            String xslPath = configTest.getDataDir() + "next-in-chain/main.xsl";
            String xmlPath = configTest.getDataDir() + "next-in-chain/test.xml";

            // Create transformer
            File xslFile = new File(xslPath);
            InputSource inputSource = new InputSource(xslFile.toURI().toString());
            Source source = new SAXSource(inputSource);

            TransformerFactory transformerFactory = new ProfessionalTransformerFactory();
            Transformer transformer = transformerFactory.newTransformer(source);

            // Create XML SAX source
            Source xmlSource = new SAXSource(new InputSource(new File(xmlPath).toURI().toString()));

            // Output stream result
            StringWriter sw = new StringWriter();
            Result outputTarget = new StreamResult(sw);
            outputTarget.setSystemId(new File(configTest.getDataDir() + "next-in-chain/dummy-output.xml").toURI().toASCIIString());

            // Run transformation
            System.out.println("Run transformation...");
            transformer.transform(xmlSource, outputTarget);
            assertEquals( "<second>principal</second>", sw.toString(), "result");
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }

    }

    @Test
    public void testCdataSectionProperty() {
        try {
            Source xslSource = new StreamSource(new StringReader(
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
                            "   version=\"1.0\">\n" +
                            "   \n" +
                            "   <xsl:output cdata-section-elements=\"MASTER_COMMENTS\"/>\n" +
                            "   \n" +
                            "   <xsl:template match=\"@* | node()\">\n" +
                            "      <xsl:copy>\n" +
                            "         <xsl:apply-templates select=\"@* | node()\"/>\n" +
                            "      </xsl:copy>\n" +
                            "   </xsl:template>\n" +
                            "</xsl:stylesheet>"));

            Transformer newTransformer = TransformerFactoryImpl.newInstance().newTransformer(xslSource);
            String outputProperty = newTransformer.getOutputProperty("cdata-section-elements");
            assertTrue(outputProperty.trim().equals("MASTER_COMMENTS") || outputProperty.trim().equals("Q{}MASTER_COMMENTS"));

            Source xmlSource = new StreamSource(new StringReader(
                    "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                            "<MASTER_COMMENTS>\n" +
                            "        <![CDATA[<pre> Nice Work done </pre>]]>;\n" +
                            "</MASTER_COMMENTS>"));
            StringWriter writer = new StringWriter();
            Result outputTarget = new StreamResult(writer);
            newTransformer.transform(xmlSource, outputTarget);

            assertTrue(writer.toString().contains("<MASTER_COMMENTS><![CDATA["));
            assertTrue(writer.toString().contains("]]></MASTER_COMMENTS>"));
            //System.out.println(writer.toString());
        } catch (TransformerException e) {
            fail();
            e.printStackTrace();
        }
    }


}

