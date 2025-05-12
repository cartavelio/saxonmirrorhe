package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.StandardURIResolver;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.InputStream;
import java.io.Reader;
import java.io.StringReader;
import java.io.StringWriter;
import java.net.URI;

import static org.junit.jupiter.api.Assertions.*;

/**
 *
 */
public class TestXQueryStreaming {
    private final ConfigTest configTest = new ConfigTest();

    public final static int limit = 100000;

    @Test
    public void test1() throws Exception {
        streaming("for $x in saxon:stream(doc('uri')/*/*) return string($x)");
    }


    @Test
    public void test2() throws Exception {
        streaming("for $x in saxon:stream(doc('uri')/*/*) return $x");
    }


    @Test
    public void test3() throws Exception {
        streaming("for $x in saxon:stream(doc('uri')/*/*) return $x/*");
    }


    @Test
    public void test4() throws Exception {
        streaming("saxon:stream(doc('uri')/*/*)/string()");
    }


    @Test
    public void test5() throws Exception {
        streaming("saxon:stream(doc('uri')/*/*)");
    }

 // Following test commented out prior to 9.7 release, it is failing because there is a sort involved: the output of
 // xsl:stream/snapshot(.) is not recognized as being in document order.
    @Test
    public void test6() throws Exception {
        // MHK 2015-11-10: currently failing (never terminates) because it's attempting a sort into document order.
        // See also bugs 2013 and 2546
        streaming("saxon:stream(doc('uri')/*/*)/*");
    }

    @Test
    public void test6a() throws Exception {
        streaming("for $x in saxon:stream(doc('uri')/*/*)/* return $x");
    }

    @Test
    public void test7() throws Exception {
        // Bug 2550
        streaming("for $x in saxon:stream(doc('uri')/*/*)/element {node-name()} {node()} return $x");
    }

    @Test
    public void test8() throws Exception {
        // Bug 2550
        streaming("saxon:stream(doc('uri')/*/*/element {node-name()} {node()})");
    }

    @Test
    public void test9() throws Exception {
        // let $x := saxon:stream(doc('doc.xml')/*/*)[1] return $x
        streaming("let $x := saxon:stream(doc('doc.xml')/*/*)[1] return $x");
    }



    /**
     * This test provides the input to a streamed query as an infinite input stream. It therefore relies on the query
     * itself to stop reading the input when it has had enough. It also displays the processing type used for processing
     * N bytes of input.
     */

    private void streaming(String query) throws Exception {
        Processor proc = new Processor(true);
        proc.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
        XQueryCompiler xQueryCompiler = proc.newXQueryCompiler();
        xQueryCompiler.setBaseURI(new URI("http://dummy/"));
        XQueryExecutable xqueryExecutable = xQueryCompiler.compile(query);
        XQueryEvaluator xqueryEvaluator = xqueryExecutable.load();
        xqueryEvaluator.setURIResolver(new StandardURIResolver() {

            @Override
            public Source resolve(String href, String base) {
                return new StreamSource(new InputStream() {
                    private static final String intro =
                            "<song>\n";
                    private static final String chorus =
                            "  <chorus>\n" +
                                    "    <line>Let me take you down</line>\n" +
                                    "    <line>Cause I'm going to Strawberry Fields</line>\n" +
                                    "    <line>Nothing is real</line>\n" +
                                    "    <line>And nothing to get hung about</line>\n" +
                                    "    <line>Strawberry Fields forever</line>\n" +
                                    "  </chorus>\n";
                    private int i = -intro.length();

                    @Override
                    public int read() {
                        if (i++ < 0) {
                            return intro.charAt(i + intro.length() - 1);
                        } else {
                            i %= chorus.length();
                            return chorus.charAt(i);
                        }
                    }
                });
            }
        });

        long start = System.currentTimeMillis();
        long count = 0;
        long nextStop = 1;
        for (XdmSequenceIterator<?> xdmItemIterator = xqueryEvaluator.iterator(); xdmItemIterator.hasNext(); ) {
            xdmItemIterator.next();
            if (++count == nextStop) {
                long elapsed = System.currentTimeMillis() - start;
                System.out.println("t(" + count + "): " + elapsed + " msec");
                if (count >= limit) {
                    xdmItemIterator.close();
                    break;
                }
                nextStop <<= 1;
            }
        }
    }

    @Test
    public void testClose() {
        try {
            String query = "saxon:stream(doc('uriresolver:resolve')/x/y/z)";
            Processor proc = new Processor(true);
            XQueryCompiler xQueryCompiler = proc.newXQueryCompiler();
            XQueryExecutable xqueryExecutable = xQueryCompiler.compile(query);
            XQueryEvaluator xqueryEvaluator = xqueryExecutable.load();
            Reader reader = new StringReader("<x><y><z>GOLD</z></y></x>");
            final StreamSource source = new StreamSource(reader);
            xqueryEvaluator.setURIResolver((href, base) -> source);
            XdmSequenceIterator<?> iterator = xqueryEvaluator.iterator();
            while (iterator.hasNext()) {
                XdmItem item = iterator.next();
                System.out.println(item + "|");
            }
            iterator.close();
            xqueryEvaluator.close();
        } catch (Exception err) {
            fail();
        }
    }

    private boolean runFallback(boolean fallback, String query) {
        try {
            Processor proc = new Processor(true);
            if (fallback) {
                proc.setConfigurationProperty(Feature.STREAMING_FALLBACK, true);
            }
            proc.setConfigurationProperty(Feature.TIMING, true);
            XQueryCompiler xQueryCompiler = proc.newXQueryCompiler();
            XQueryExecutable xqueryExecutable = xQueryCompiler.compile(query);
            XQueryEvaluator xqueryEvaluator = xqueryExecutable.load();
            XdmSequenceIterator<?> iterator = xqueryEvaluator.iterator();
            while (iterator.hasNext()) {
                XdmItem item = iterator.next();
                System.out.println(item + "|");
            }
            iterator.close();
            xqueryEvaluator.close();
            return true;
        } catch (Exception err) {
            //err.printStackTrace();
            return false;
        }
    }

    @Test
    public void testConversionFallbackOn() {
        String uri = "file:///" + configTest.getDataDir() + "books.xml";
        assertTrue(runFallback(true, "saxon:stream(doc('" + uri + "')/BOOKLIST/BOOKS/ITEM[last()]/AUTHOR)"));
    }

    @Test
    public void testConversionFallbackOff() {
        String uri = "file:///" + configTest.getDataDir() + "books.xml";
        assertFalse(runFallback(false, "saxon:stream(doc('" + uri + "')/BOOKLIST/BOOKS/ITEM[last()]/AUTHOR)"));
    }

    @Test
    public void testStreamabilityFallbackOn() {
        String uri = "file:///" + configTest.getDataDir() + "books.xml";
        String query = "declare function local:f($element)\n" +
                "{\n" +
                "  element x\n" +
                "  {\n" +
                "     for $child in $element/*\n" +
                "     return element y {string($child)}\n" +
                "  }\n" +
                "};\n" +
                "for $x in saxon:stream(doc('" + uri + "')/*/*/local:f(.))\n" +
                "return $x";
        assertTrue(runFallback(true, query));
    }

    @Test
    public void testStreamabilityFallbackOff() {
        String uri = "file:///" + configTest.getDataDir() + "books.xml";

        String query = "declare function local:f($element)\n" +
                "{\n" +
                "  element x\n" +
                "  {\n" +
                "     for $child in $element/*\n" +
                "     return element y {string($child)}\n" +
                "  }\n" +
                "};\n" +
                "for $x in saxon:stream(doc('" + uri + "')/*/*/local:f(.))\n" +
                "return $x";
        assertFalse(runFallback(false, query));
    }

    /**
     * Bug 4128
     */
    @Test
    public void testMotionlessQuery() {
        try {
            String uri = "file:///" + configTest.getDataDir() + "othello.xml";
            String query = "declare variable $n external; count(saxon:stream(doc('" + uri +
                    "')/((1 to $n)!element e {.})))";
            Processor proc = new Processor(true);
            XQueryCompiler xQueryCompiler = proc.newXQueryCompiler();
            XQueryExecutable xqueryExecutable = xQueryCompiler.compile(query);
            XQueryEvaluator xqueryEvaluator = xqueryExecutable.load();
            xqueryEvaluator.setExternalVariable(new QName("n"), new XdmAtomicValue(5));
            XdmValue result = xqueryEvaluator.evaluate();
            assertEquals("5", result.toString());
        } catch (Exception err) {
            fail();
        }
    }


    @Test
    public void testEarlyExit() {
        try {
            String uri = "file:///" + configTest.getDataDir() + "othello.xml";
            String query = "declare variable $name as xs:string external;\n" +
                    "    exists(//PERSONA/text()[starts-with(., $name)])";
            Processor processor = new Processor(true);
            XQueryCompiler compiler = processor.newXQueryCompiler();
            compiler.setStreaming(true);
            XQueryExecutable exec = compiler.compile(query);
            XQueryEvaluator eval = exec.load();
            StreamSource source = new StreamSource(uri);
            eval.setSource(source);
            eval.setExternalVariable(new QName("name"), new XdmAtomicValue("IAGO"));
            StringWriter sw = new StringWriter();
            Serializer serializer = processor.newSerializer(sw);
            eval.runStreamed(source, serializer);
            assertTrue(sw.toString().endsWith("true"));
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * Bug 5569
     */
    @Test
    public void testStreamedSerialization() {
        try {
            String uri = "file:///" + configTest.getDataDir() + "books.xml";
            String query = "/BOOKLIST/BOOKS/ITEM/string-join(*, '|')";
            Processor proc = new Processor(true);
            XQueryCompiler xQueryCompiler = proc.newXQueryCompiler();
            xQueryCompiler.setStreaming(true);
            XQueryExecutable xqueryExecutable = xQueryCompiler.compile(query);
            XQueryEvaluator xqueryEvaluator = xqueryExecutable.load();
            StringWriter writer = new StringWriter();
            Serializer destination = proc.newSerializer(writer);
            destination.setOutputProperty(Serializer.Property.METHOD, "text");
            destination.setOutputProperty(Serializer.Property.ITEM_SEPARATOR, "+");
            xqueryEvaluator.runStreamed(new StreamSource(uri), destination);
            assertTrue(writer.toString().contains("352|8.3 5.7 1.1|6.1+Wuthering Heights|Charlotte Brontë|Penguin Classics"));
        } catch (Exception err) {
            fail();
        }
    }


}



