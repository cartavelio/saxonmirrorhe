package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XQueryEvaluator;
import net.sf.saxon.s9api.XQueryExecutable;
import net.sf.saxon.s9api.XdmSequenceIterator;
import org.junit.jupiter.api.Test;
import org.xml.sax.*;

import javax.xml.transform.Source;
import javax.xml.transform.sax.SAXSource;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.io.Reader;
import java.util.Random;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Test of XQuery streaming from Gunther Rademacher (June 2015)
 */

public class XQueryStreamingTest {
    private final Random rand = new Random();

    @Test
    public void testMultiPassSaxonStreaming() throws Exception {
        Configuration configuration = new EnterpriseConfiguration();

        final Source[] source = new Source[]{null};

        String query = "saxon:stream(doc('uriresolver:resolve')/song/chorus)";
        Processor processor = new Processor(configuration);
        XQueryExecutable xqueryExecutable = processor.newXQueryCompiler().compile(query);

        int nPasses = 0;
        try {
            ReaderFactory readerFactory = new ReaderFactory();
            for (int repetition = 0; repetition < 4; ++repetition)
                for (int nResults = 8; nResults < 4096; nResults <<= 2) {
                    XQueryEvaluator xqueryEvaluator = xqueryExecutable.load();
                    xqueryEvaluator.setResourceResolver(request -> source[0]);
                    final Reader factoryReader = readerFactory.newReader();
                    final Reader delayingReader = new Reader() {

                        @Override
                        public int read(char[] cbuf, int off, int len) throws IOException {
                            randomDelay();
                            return factoryReader.read(cbuf, off, len);
                        }

                        @Override
                        public void close() throws IOException {
                        }
                    };
                    XMLReader xmlReader = configuration.getSourceParser();
                    xmlReader.setErrorHandler(new ErrorHandler() {
                        @Override
                        public void error(SAXParseException exception) throws SAXException {
                            System.err.println("E: " + exception.getMessage());
                        }

                        @Override
                        public void warning(SAXParseException exception) throws SAXException {
                            System.err.println("W: " + exception.getMessage());
                        }

                        @Override
                        public void fatalError(SAXParseException exception) throws SAXException {
                            System.err.println("F:" + exception.getMessage());
                        }
                    });
                    source[0] = new SAXSource(xmlReader, new InputSource(delayingReader));

                    XdmSequenceIterator iterator = xqueryEvaluator.iterator();
                    for (int i = 0; i < nResults; ++i) {
                        iterator.next();
                    }
                    ++nPasses;
                    randomDelay();
                    iterator.close();
                }
        } catch (Exception e) {
            throw new RuntimeException("caught exception after " + nPasses
                + " passes: " + e, e);
        } finally {
            System.err.println("done with " + nPasses + " passes");
        }
    }

    private void randomDelay() {
        try {
            Thread.sleep(rand.nextInt(10));
        } catch (InterruptedException e) {
        }
    }

    private class ReaderFactory {
        final AtomicInteger latest = new AtomicInteger(-1);

        public Reader newReader() {
            return new Reader() {
                final int generation = latest.incrementAndGet();
                final InputStreamReader inputStreamReader = new InputStreamReader(
                    strawberryFieldsForever());

                @Override
                public int read(char[] cbuf, int off, int len) throws IOException {
                    if (generation == latest.get())
                        return inputStreamReader.read(cbuf, off, len);
                    System.err.println("stale reader signals eof...");
                    return -1;
                }

                @Override
                public void close() throws IOException {
                }
            };
        }
    }

    InputStream strawberryFieldsForever() {
        return new InputStream() {
            private static final String intro = "<song>\n";
            private static final String chorus = "  <chorus>\n"
                + "    <line>Let me take you down</line>\n"
                + "    <line>Cause I'm going to Strawberry Fields</line>\n"
                + "    <line>Nothing is real</line>\n"
                + "    <line>And nothing to get hung about</line>\n"
                + "    <line>Strawberry Fields forever</line>\n"
                + "  </chorus>\n";
            private int i = -intro.length();

            @Override
            public int read() throws IOException {
                if (i++ < 0) {
                    return intro.charAt(i + intro.length() - 1);
                } else {
                    i %= chorus.length();
                    return chorus.charAt(i);
                }
            }
        };
    }
}
