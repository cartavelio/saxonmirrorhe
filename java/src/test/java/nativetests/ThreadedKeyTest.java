package nativetests;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.w3c.dom.NodeList;

import javax.xml.transform.Templates;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerConfigurationException;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMResult;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * Test that use of keys is threadsafe.
 */
public class ThreadedKeyTest {
    private final ConfigTest configTest = new ConfigTest();

    private static final String keyXsl =
            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n" +
            "    <xsl:output method=\"xml\"/>\n" +
            "    <xsl:key name='k' match='LINE' use='string-length(.)'/>\n" +
            "    <xsl:template match=\"/\">\n" +
            "        <out><xsl:value-of select=\"count(key('k', 17))\"/></out>\n" +
            "    </xsl:template>\n" +
            "</xsl:stylesheet>";


    @Test
    public void testKeyTransformationThreaded() {
        try {
            Configuration config = new Configuration();
            final NodeInfo doc = config.buildDocumentTree(new StreamSource(configTest.getDataDir() + "othello.xml")).getRootNode();
            TransformerFactory factory = new net.sf.saxon.TransformerFactoryImpl(config);
            StreamSource styleSource = new StreamSource(new StringReader(keyXsl));
            final Templates templates = factory.newTemplates(styleSource);

            ExecutorService executor = Executors.newFixedThreadPool(10);
            for (int i = 0; i < 100; i++) {
                executor.execute(new Runnable() {

                    @Override
                    public void run() {
                        try {
                            Transformer transformer = templates.newTransformer();
                            DOMResult result = new DOMResult();
                            transformer.transform(doc, result);
                            NodeList resultTags = ((Document)result.getNode()).getElementsByTagName("*");
                            assertEquals(1, resultTags.getLength());
                            System.out.println("thread-#" + Thread.currentThread().getId() + " OK");
                        } catch (Throwable ex) {
                            System.err.println("thread-# " + Thread.currentThread().getId() + " ERROR");
                            ex.printStackTrace();
                        }
                    }
                });
            }
            executor.shutdown();
            executor.awaitTermination(300, TimeUnit.SECONDS);
        } catch (TransformerConfigurationException e) {
            fail(e.getMessage());
        } catch (XPathException | InterruptedException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }
}

