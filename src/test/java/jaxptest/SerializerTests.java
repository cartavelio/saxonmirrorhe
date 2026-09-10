package jaxptest;

import com.saxonica.config.ProfessionalTransformerFactory;
import junit.framework.TestCase;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.jaxp.TransformerImpl;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.OutputURIResolver;
import org.xml.sax.InputSource;

import javax.xml.transform.*;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.Properties;

/**
 * Test for s9api serialization
 */
public class SerializerTests extends TestCase {

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
            assertEquals("method", "xml", props.getProperty("method"));
            assertEquals("indent", "no", props.getProperty("indent"));
            assertNull("doctype-system", props.getProperty("doctype-system"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

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
            assertTrue("output1", output.toString().contains("17"));
            assertTrue("output2", output.toString().contains("<!--custom-emitter-->"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

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
            assertTrue("output1", output.toString().contains("17"));
            assertTrue("output2", output.toString().contains("<!--custom-emitter-->"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

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
            tFactory.setAttribute(Feature.ALLOW_MULTITHREADING.name, false);
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

    public void testIllegalUri() {
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
            String s = ".\\config\\testIllegalUriOutput.xml";
            StreamResult result = new StreamResult(new File(s));
            System.err.println(result.getSystemId());
            trans.transform(input, result);

        } catch (Exception e) {
            fail(e.getMessage());
        }

    }

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
            assertTrue("output1", output.toString().contains("17"));
            assertTrue("output2", output.toString().contains("<!--custom-emitter-->"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

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

    public void testNonBMP() throws Exception {

        TransformerFactory tf = new net.sf.saxon.TransformerFactoryImpl();

        Source templateSource = new StreamSource(new FileInputStream(ConfigTest.DATA_DIR + "davis/formatter.xsl"));

        Transformer t = tf.newTransformer(templateSource);

        Source inputSource = new StreamSource(new FileInputStream(ConfigTest.DATA_DIR + "davis/inputData.xml"));
        ByteArrayOutputStream os = new ByteArrayOutputStream();
        Result result = new StreamResult(os);
        t.transform(inputSource, result);
        os.close();
        String out = os.toString("utf-8");
        System.out.println(out);
        assertFalse(out.contains("&#"));
    }

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
            assertTrue("output", output.toString().contains("<b><a/></b>"));
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    // Error case - target stylesheet does not exist (base URI unknown)

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
                public OutputURIResolver newInstance() {
                    return this;
                }

                public Result resolve(String href, String base) throws TransformerException {
                    return result;
                }

                public void close(Result result) throws TransformerException {

                }
            });
            trans.transform(input, result);
            assertTrue("output", output.toString().contains("<b><a/></b>"));
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

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
                public OutputURIResolver newInstance() {
                    return this;
                }

                public Result resolve(String href, String base) {
                    if (href.endsWith("out.xml")) {
                        return new StreamResult(sw);
                    } else {
                        return null;
                    }
                }

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
            assertTrue("output", sw.toString().contains("<b><a/></b>"));
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    // We need to run this junit test on different platforms to make sure we get the desired outcome on all of them.
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
        String outPath = ConfigTest.DATA_DIR + "output.xml";
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
        assertTrue("Output file exists: ", outFile.exists());
        assertTrue("Can the output file be deleted? ", outFile.delete());
    }


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
            assertTrue("output1", output.toString().contains("17"));
            assertTrue("output2", output.toString().contains("<!--custom-emitter-->"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }



    public void testNextInChainPisoiTest() {
        try {
            String xslPath = ConfigTest.DATA_DIR + "next-in-chain/main.xsl";
            String xmlPath = ConfigTest.DATA_DIR + "next-in-chain/test.xml";

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
            outputTarget.setSystemId(new File(ConfigTest.DATA_DIR + "next-in-chain/dummy-output.xml").toURI().toASCIIString());

            // Run transformation
            System.out.println("Run transformation...");
            transformer.transform(xmlSource, outputTarget);
            assertEquals("result", "<second>principal</second>", sw.toString());
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }

    }

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
            assertTrue(outputProperty.trim().equals("MASTER_COMMENTS") || outputProperty.trim().equals("{}MASTER_COMMENTS"));

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

//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//



