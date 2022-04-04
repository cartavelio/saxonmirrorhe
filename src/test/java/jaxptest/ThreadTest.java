package jaxptest;

import com.saxonica.config.EnterpriseTransformerFactory;
import net.sf.saxon.lib.FeatureKeys;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Templates;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test that DOM access is threadsafe. Test deleted - because DOM access is not thread-safe!
 */
public class ThreadTest {
    private final ConfigTest configTest = new ConfigTest();

    @Test
    public void testDummy() {
        assertTrue(true);
    }

//    private static final String copyXsl =
//            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\">\n" +
//            "    <xsl:output method=\"xml\"/>\n" +
//            "\n" +
//            "    <xsl:template match=\"/\">\n" +
//            "        <xsl:apply-templates/>\n" +
//            "    </xsl:template>\n" +
//            "\n" +
//            "    <xsl:template match=\"@* | node()\">\n" +
//            "        <xsl:copy>\n" +
//            "            <xsl:apply-templates select=\"@* | node()\"/>\n" +
//            "        </xsl:copy>\n" +
//            "    </xsl:template>\n" +
//            "</xsl:stylesheet>";
//
//    private final static String testXml =
//            "<alpha a=\"b\" c=\"d\">\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "</alpha>";
//
//    public void testDomTransformationThreaded() {
//        //TransformerFactory factory = TransformerFactory.newInstance();
//        try {
//            TransformerFactory factory = new net.sf.saxon.TransformerFactoryImpl();
//            StreamSource styleSource = new StreamSource(new StringReader(copyXsl));
//            final Templates templates = factory.newTemplates(styleSource);
//            DocumentBuilderFactory docBuilderFactory =
//                DocumentBuilderFactory.newInstance();
//            DocumentBuilder docBuilder = docBuilderFactory.newDocumentBuilder();
//            final Document domDoc = docBuilder.parse(new InputSource(new StringReader(testXml)));
//            final NodeList sourceTags = domDoc.getElementsByTagName("*");
//
//            ExecutorService executor = Executors.newFixedThreadPool(10);
//            for (int i = 0; i < 100; i++) {
//                executor.execute(new Runnable() {
//
//                    public void run() {
//                        try {
//                            Transformer transformer = templates.newTransformer();
//                            DOMSource source = new DOMSource(domDoc);
//                            DOMResult result = new DOMResult();
//                            transformer.transform(source, result);
//                            NodeList resultTags = ((Document)result.getNode()).getElementsByTagName("*");
//                            assert sourceTags.getLength() == resultTags.getLength();
//                            System.out.println("thread-#" + Thread.currentThread().getId() + " OK");
//                        } catch (Throwable ex) {
//                            System.err.println("thread-# " + Thread.currentThread().getId() + " ERROR");
//                            ex.printStackTrace();
//                        }
//                    }
//                });
//            }
//            executor.shutdown();
//            executor.awaitTermination(300, TimeUnit.SECONDS);
//        } catch (TransformerConfigurationException e) {
//            fail(e.getMessage());
//        } catch (ParserConfigurationException e) {
//            e.printStackTrace();
//            fail(e.getMessage());
//        } catch (SAXException e) {
//            e.printStackTrace();
//            fail(e.getMessage());
//        } catch (IOException e) {
//            e.printStackTrace();
//            fail(e.getMessage());
//        } catch (InterruptedException e) {
//            e.printStackTrace();
//            fail(e.getMessage());
//        }
//    }

//    private static final String copyXsl =
//            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\">\n" +
//            "    <xsl:output method=\"xml\"/>\n" +
//            "\n" +
//            "    <xsl:template match=\"/\">\n" +
//            "        <xsl:apply-templates/>\n" +
//            "    </xsl:template>\n" +
//            "\n" +
//            "    <xsl:template match=\"@* | node()\">\n" +
//            "        <xsl:copy>\n" +
//            "            <xsl:apply-templates select=\"@* | node()\"/>\n" +
//            "        </xsl:copy>\n" +
//            "    </xsl:template>\n" +
//            "</xsl:stylesheet>";
//
//    private final static String testXml =
//            "<alpha a=\"b\" c=\"d\">\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "</alpha>";

    @Test
    public void testParallelByteCodeGeneration() {
        //TransformerFactory factory = TransformerFactory.newInstance();
        try {

            ExecutorService executor = Executors.newFixedThreadPool(5);
            for (int i = 0; i < 100; i++) {
                executor.execute(() -> {
                    try {
                        TransformerFactory factory = new net.sf.saxon.TransformerFactoryImpl();
                        factory.setAttribute(FeatureKeys.OPTIMIZATION_LEVEL, "-j");
                        factory.setAttribute(FeatureKeys.GENERATE_BYTE_CODE, true);
                        factory.setAttribute(FeatureKeys.THRESHOLD_FOR_HOTSPOT_BYTE_CODE, 1);
                        StreamSource styleSource = new StreamSource(new File(configTest.getDataDir() + "bills.xsl"));
                        Templates templates = factory.newTemplates(styleSource);
                        Transformer trans = templates.newTransformer();
                        StringWriter sw = new StringWriter();
                        trans.transform(new StreamSource(new File(configTest.getDataDir() + "bills.xsl")),
                                        new StreamResult(sw));
                        System.out.println("thread-#" + Thread.currentThread().getId() + " OK");
                    } catch (Throwable ex) {
                        System.err.println("thread-# " + Thread.currentThread().getId() + " ERROR");
                        ex.printStackTrace();
                    }
                });
            }
            executor.shutdown();
            executor.awaitTermination(300, TimeUnit.SECONDS);

        } catch (InterruptedException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    private static final String copyXslSa =
            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\">\n" +
                    "    <xsl:output method=\"xml\"/>\n" +
                    "    <xsl:import-schema namespace='http://www.millicom.com' schema-location='SubsidyTrackingFactXSD11.xsd'/>" +
                    "\n" +
                    "    <xsl:template match=\"/\">\n" +
                    "        <xsl:apply-templates/>\n" +
                    "    </xsl:template>\n" +
                    "\n" +
                    "    <xsl:template match=\"@* | node()\">\n" +
                    "        <xsl:copy>\n" +
                    "            <xsl:apply-templates select=\"@* | node()\"/>\n" +
                    "        </xsl:copy>\n" +
                    "    </xsl:template>\n" +
                    "</xsl:stylesheet>";

    @Test
    public void testParallelSchemaImport() {
        final TransformerFactory factory = new EnterpriseTransformerFactory();
        try {
            List<Throwable> exceptions = new ArrayList<>();
            ExecutorService executor = Executors.newFixedThreadPool(5);
            for (int i = 0; i < 100; i++) {
                executor.execute(new Runnable() {

                    @Override
                    public void run() {
                        try {
                            StreamSource styleSource = new StreamSource(new StringReader(copyXslSa), configTest.getDataDir() + "/");
                            final Templates templates = factory.newTemplates(styleSource);
                            System.out.println("thread-#" + Thread.currentThread().getId() + " OK");
                            assertNotNull(templates);
                        } catch (Throwable ex) {
                            System.err.println("thread-# " + Thread.currentThread().getId() + " ERROR " + ex.getMessage());
                            exceptions.add(ex);
                            ex.printStackTrace();
                            fail("Concurrent schema import failed");
                        }
                    }
                });
            }
            executor.shutdown();
            executor.awaitTermination(300, TimeUnit.SECONDS);
            assertTrue(exceptions.isEmpty());
        } catch (InterruptedException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

}



