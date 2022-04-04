package internaltests;

import jaxptest.ConfigTest;
import net.sf.saxon.expr.sort.LFUCache;
import net.sf.saxon.s9api.*;
import net.sf.saxon.s9api.streams.Predicates;
import net.sf.saxon.s9api.streams.Steps;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.util.IdentityHashMap;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

import static org.junit.Assert.assertTrue;
import static org.junit.Assert.fail;


/**
 *  Tests of caching
 */
public class LFUCacheTest {
    private final ConfigTest configTest = new ConfigTest();

    // Test that compiled XPath expressions are cached

    @Test
    public void testXPathCache() {
        try {
            LFUCache<String, String> cache = new LFUCache<>(100);
            IdentityHashMap<XPathExecutable, XPathExecutable> executables = new IdentityHashMap<>();
            Processor processor = new Processor(false);
//            StreamSource source = new StreamSource(new File(configTest.getDataDir() + "bible/nt.xml"));
//            DocumentBuilder builder = processor.newDocumentBuilder();
//            XdmNode d = builder.build(source);
            XPathCompiler xpath = processor.newXPathCompiler();
            xpath.setCaching(true);
            for (int j = 0; j < 10000; j++) {
                final String source = "child::x" + (Double.toString(Math.random()).substring(0, 4));
                XPathExecutable exec = xpath.compile(source);
                //System.err.println("Expression " + source + " is " + (executables.containsKey(exec) ? "old" : "new"));
                executables.put(exec, exec);
            }
            //System.err.println(executables.size());
            assertTrue(executables.size() <= 150);
        } catch (SaxonApiException e) {
            fail();
        }
    }

    @Test
    public void testLFUCache() {
        try {
            LFUCache<String, String> cache = new LFUCache<>(1000);
            Processor processor = new Processor(false);
            StreamSource source = new StreamSource(new File(configTest.getDataDir() + "bible/nt/nt.xml"));
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode doc = builder.build(source);
            int hits = 0;
            int misses = 0;
            for (XdmNode text : doc.select(Steps.descendant(Predicates.isText())).asListOfNodes()) {
                 for (String word : text.getStringValue().split(" ")) {
                     if (cache.get(word) != null) {
                         hits++;
                     } else {
                         misses++;
                         cache.put(word, word + "-found");
                     }
                 }
            }
            //System.err.println("Cache size " + cache.size() + " hits=" + hits + " misses=" + misses);
            Assertions.assertTrue(cache.size() <= 3000);
            Assertions.assertTrue(hits > 150000);
            Assertions.assertTrue(misses < 30000);
        } catch (SaxonApiException e) {
            Assertions.fail(e.getMessage());
        }
    }

    @Test
    public void testLFUCacheThreaded() {
        try {
            LFUCache<String, String> cache = new LFUCache<>(1000, true);
            Processor processor = new Processor(false);
            StreamSource source = new StreamSource(new File(configTest.getDataDir() + "bible/nt/nt.xml"));
            DocumentBuilder builder = processor.newDocumentBuilder();
            XdmNode doc = builder.build(source);

            ExecutorService executor = Executors.newFixedThreadPool(10);
            for (int i = 0; i < 10; i++) {
                executor.execute(new Runnable() {

                    @Override
                    public void run() {
                        int hits = 0;
                        int misses = 0;
                        try {
                            for (XdmNode text : doc.select(Steps.descendant(n -> n.getNodeKind() == XdmNodeKind.TEXT)).asListOfNodes()) {
                                for (String word : text.getStringValue().split(" ")) {
                                    if (cache.get(word) != null) {
                                        hits++;
                                    } else {
                                        misses++;
                                        cache.put(word, word + "-found");
                                    }
                                }
                            }
                            System.err.println("Cache size " + cache.size() + " hits=" + hits + " misses=" + misses);
                            Assertions.assertTrue(cache.size() <= 3000);
                            Assertions.assertTrue(hits > 150000);
                            Assertions.assertTrue(misses < 30000);
                            //System.out.println("thread-#" + Thread.currentThread().getId() + " OK");
                        } catch (Throwable ex) {
                            ex.printStackTrace();
                            Assertions.fail("thread-# " + Thread.currentThread().getId() + " ERROR");
                        }
                    }
                });
            }
            executor.shutdown();
            executor.awaitTermination(300, TimeUnit.SECONDS);

        } catch (InterruptedException e) {
            e.printStackTrace();
            Assertions.fail(e.getMessage());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }
}
