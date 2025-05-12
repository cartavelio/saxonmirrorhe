package jaxptest;

import javax.xml.transform.*;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayOutputStream;
import java.io.InputStream;
import java.io.StringReader;
import java.util.concurrent.CountDownLatch;
import java.util.concurrent.Executors;
import java.util.concurrent.ThreadFactory;

/**
 * // bug 3159
 */
public class UnsafeXsltBug3159 implements Runnable {

    private static final String xsl = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
            + "<xsl:stylesheet version=\"2.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:html=\"http://www.w3.org/1999/xhtml\">\n"
            + "    <xsl:strip-space elements=\"*\"/>\n"
            + "    <xsl:template match=\"*\">\n"
            + "        <xsl:message terminate=\"yes\">\n"
            + "            <xsl:text>Unknown element: </xsl:text>\n"
            + "            <xsl:element name=\"{name()}\" />\n"
            + "        </xsl:message>\n"
            + "    </xsl:template>\n"
            + "    <xsl:template match=\"html:html\">\n"
            + "        <root>\n"
            + "            <xsl:apply-templates />\n"
            + "        </root>\n"
            + "    </xsl:template>\n"
            + "    <xsl:template match=\"html:body\">\n"
            + "        <record>\n"
            + "            <xsl:apply-templates />\n"
            + "        </record>\n"
            + "    </xsl:template>\n"
            + "</xsl:stylesheet>";

    private static final String input = "<!DOCTYPE html>\n"
            + "<html xmlns=\"http://www.w3.org/1999/xhtml\"><body>content</body></html>";


    private static final Templates templates;

    static {
        final TransformerFactory factory = TransformerFactory.newInstance();
        factory.setURIResolver(new URIResolver() {
            @Override
            public Source resolve(String href, String base) throws TransformerException {
                final InputStream input = UnsafeXsltBug3159.class.getResourceAsStream(href);
                if (input == null) {
                    throw new TransformerException("Unable to find '" + href + "' in '" + base + "'.");
                }
                return new StreamSource(input, "ccm:" + href);
            }
        });
        try {
            templates = factory.newTemplates(new StreamSource(new StringReader(xsl)));
        } catch (TransformerConfigurationException e) {
            throw new RuntimeException(e);
        }
    }

//
//    static {
//        try {
//            input = String.join("\n", Files.readAllLines(Paths.get(UnsafeXslt.class.getResource("/unsafe.input.xml").toURI())));
//        } catch (IOException | URISyntaxException e) {
//            throw new RuntimeException(e);
//        }
//    }

    public static void main(String[] args) throws Throwable {
        final int threadSize = 30;
        final CountDownLatch latch = new CountDownLatch(threadSize);
        final ThreadFactory threadFactory = Executors.defaultThreadFactory();
        final Thread[] threads = new Thread[threadSize];
        for (int i = 0; i < threadSize; ++i) {
            threads[i] = threadFactory.newThread(new UnsafeXsltBug3159(latch));
        }
        for (int i = 0; i < threadSize; ++i) {
            threads[i].start();
        }
        for (int i = 0; i < threadSize; ++i) {
            threads[i].join();
        }
    }

    private final CountDownLatch latch;

    public UnsafeXsltBug3159(final CountDownLatch latch) {
        this.latch = latch;
    }

    @Override
    public void run() {
        try {
            latch.countDown();
            latch.await();
            final int size = 1000;
            final ByteArrayOutputStream output = new ByteArrayOutputStream();
            final StreamResult result = new StreamResult(output);
            for (int i = 0; i < size; ++i) {
                output.reset();
                final Transformer transformer;
                synchronized(templates) {
                    transformer = templates.newTransformer();
                }
                transformer.transform(new StreamSource(new StringReader(input), "ccm:input"), result);
            }
        } catch (Throwable t) {
            t.printStackTrace(System.out);
        }
    }

}

