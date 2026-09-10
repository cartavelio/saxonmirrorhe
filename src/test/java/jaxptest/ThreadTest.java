package jaxptest;

import com.saxonica.config.EnterpriseTransformerFactory;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Templates;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
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



