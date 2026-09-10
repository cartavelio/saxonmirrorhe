package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.URIQueryParameters;
import net.sf.saxon.lib.CollectionFinder;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.Resource;
import net.sf.saxon.lib.ResourceCollection;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.resource.AbstractResourceCollection;
import net.sf.saxon.resource.DirectoryCollection;
import net.sf.saxon.resource.XmlResource;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.UncheckedXPathException;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.DateTimeValue;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.net.URI;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertInstanceOf;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests for handling of Saxon collection URIs and collection URI resolver APIs
 */
public class TestCollections {
    private final ConfigTest configTest = new ConfigTest();
    
    private XdmAtomicValue composeURI(String basedir, String additional) {
        if (basedir.startsWith("/")) {
            return new XdmAtomicValue("file://" + basedir + additional);
        } else {
            return new XdmAtomicValue("file:///" + basedir + additional);
        }
    }

    @Test
    public void testDirectoryNoRecurse() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable exec = c.compile("collection(.)!base-uri(.)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "trax/xml/?select=*.xml"));
            XdmValue val = eval.evaluate();
            assertEquals(5, val.size());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testDirectoryMatch() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            c.declareVariable(new QName("a"));
            c.declareVariable(new QName("b"));
            XPathExecutable exec = c.compile("collection($a||'trax/xml/?match='||encode-for-uri($b))!base-uri(.)");
            XPathSelector eval = exec.load();
            eval.setVariable(new QName("a"), composeURI(configTest.getDataDir(), ""));
            eval.setVariable(new QName("b"), new XdmAtomicValue("[A-Za-z]+\\.xml"));
            XdmValue val = eval.evaluate();
            assertEquals(5, val.size());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testDirectoryNoRecurse30() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.0");
            XPathExecutable exec = c.compile("collection(.)!base-uri(.)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "trax/xml/?select=*.xml"));
            XdmValue val = eval.evaluate();
            assertEquals(5, val.size());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testDirectoryRecurse() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable exec = c.compile("collection(.)!base-uri(.)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "trax/xml/?select=*.xml;recurse=yes"));
            XdmValue val = eval.evaluate();
            assertEquals(7, val.size(), "found7");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testDirectoryForbiddenProtocol() {
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(Feature.ALLOWED_PROTOCOLS, "http https");
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable exec = c.compile("collection(.)!base-uri(.)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "trax/xml/?select=*.xml;recurse=yes"));
            XdmValue val = eval.evaluate();
            fail("File scheme should have been disallowed");
        } catch (SaxonApiException e) {
            Assertions.assertTrue(e.getMessage().contains("has been disallowed"));
        }
    }

    @Test
    public void testDirectoryWithContentType() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable exec = c.compile("collection(.)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "trax/xml/?select=*.xml;recurse=yes;content-type=text/plain"));
            XdmValue val = eval.evaluate();
            assertEquals(7, val.size(), "found7");
            assertTrue(val.itemAt(0) instanceof XdmAtomicValue, "isString");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testDirectoryUriNoRecurse() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable exec = c.compile("uri-collection(.)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(),"trax/xml/?select=*.xml"));
            XdmValue val = eval.evaluate();
            assertEquals(5, val.size());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testDirectoryUriForbiddenProtocol() {
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(Feature.ALLOWED_PROTOCOLS, "http https");
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable exec = c.compile("uri-collection(.)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(),"trax/xml/?select=*.foo"));
            XdmValue val = eval.evaluate();
            fail("File scheme should have been disallowed");
        } catch (SaxonApiException e) {
            Assertions.assertTrue(e.getMessage().contains("has been disallowed"));
        }
    }

    @Test
    public void testDirectoryUriRecurse() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable exec = c.compile("uri-collection(.)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "trax/xml/?select=*.xml;recurse=yes"));
            XdmValue val = eval.evaluate();
            assertEquals(7, val.size());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testParallelSpeed() {
        try {
            for (int i = 0; i < 5; i++) {
                speedTest(true);
                speedTest(false);
            }
            long serial = speedTest(false);
            System.err.println("Serial processing (HE): " + serial + "ms");
            long parallel = speedTest(true);
            System.err.println("Parallel processing (EE): " + parallel + "ms");
            assertTrue(parallel < serial, "not faster!");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    private long speedTest(boolean parallel) throws SaxonApiException {
        Processor p = new Processor(parallel);
        long startTime = System.currentTimeMillis();
        XPathCompiler c = p.newXPathCompiler();
        XPathExecutable exec = c.compile("sum(collection(.)!count(//*))");
        XPathSelector eval = exec.load();
        eval.setContextItem(composeURI(configTest.getDataDir(), "collection/?select=*.xml;recurse=yes"));
        XdmValue val = eval.evaluate();
        long result = System.currentTimeMillis() - startTime;
        System.err.println("Found " + val + " nodes");
        return result;
    }

    @Test
    public void testStable() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable exec = c.compile("count(collection(.)//* | collection(.)//*)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection/?select=*.xml;recurse=yes;stable=yes"));
            XdmValue val = eval.evaluate();
            assertEquals(10680, ((XdmAtomicValue) val).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testUnstable() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable exec = c.compile("count(collection(.)//* | collection(.)//*)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection/?select=*.xml;recurse=yes;stable=no"));
            XdmValue val = eval.evaluate();
            assertEquals(21360, ((XdmAtomicValue) val).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testStrictValidation() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable exec = c.compile("collection(.)/* instance of element(*, xs:untyped)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "?select=books.xml;recurse=no;validation=strict"));
            XdmValue val = eval.evaluate();
            assertEquals(false, ((XdmAtomicValue) val).getBooleanValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testNoValidation() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable exec = c.compile("collection(.)/* instance of element(*, xs:untyped)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "?select=books.xml;recurse=no;validation=skip"));
            XdmValue val = eval.evaluate();
            assertEquals(true, ((XdmAtomicValue) val).getBooleanValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testJSONCollection() {
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(Feature.ALLOW_MULTITHREADING, false);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            XPathExecutable exec = c.compile("for $X in (collection(.))" +
                "return (if ($X instance of node()) then name($X/*) else $X('id'))");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection.xml"));
            XdmValue val = eval.evaluate();
            assertEquals( "1.0e0\n\"BOOKLIST\"", val.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testCustomMediaType() {
        try {
            Processor p = new Processor(true);
            Configuration config = p.getUnderlyingConfiguration();
            //???
            //config.registerFileExtension("json", "personal/plain");
            //config.registerMediaType("personal/plain", (config2, details) -> new JSONResource(context, details));
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            XPathExecutable exec = c.compile("for $X in (collection(.))" +
                                                     "return (if ($X instance of node()) then name($X/*) else $X('id'))");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection.xml"));
            XdmValue val = eval.evaluate();
            assertEquals("1.0e0\n\"BOOKLIST\"", val.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    @Test
    public void testMixedCollection() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            XPathExecutable exec = c.compile("count(collection(.))");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection2/?select=*"));
            XdmValue val = eval.evaluate();
            assertEquals(2, Integer.parseInt(val.toString()));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testMixedCollectionWithMetadata() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            XPathExecutable exec = c.compile("collection(.)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection2/?select=*.xml;metadata=yes"));
            XdmValue val = eval.evaluate();
            for (XdmItem item : val) {
                assertTrue(item instanceof XdmFunctionItem);
                XdmFunctionItem map = (XdmFunctionItem)item;
                XdmAtomicValue name = (XdmAtomicValue)map.call(p, new XdmAtomicValue("canonical-path"));
                XdmAtomicValue length = (XdmAtomicValue) map.call(p, new XdmAtomicValue("length"));
                XdmAtomicValue lastModified = (XdmAtomicValue) map.call(p, new XdmAtomicValue("last-modified"));
                XdmAtomicValue contentType = (XdmAtomicValue) map.call(p, new XdmAtomicValue("content-type"));
                assertEquals("application/xml", contentType.toString());
                System.err.println(name + " " + lastModified + " " + length + " " + contentType);
            }
            assertEquals(1, val.size());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testMixedCollectionMetadataFetch() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            String query = "for $m in collection(.||'?metadata=yes') " +
                           "return if ($m?content-type='application/xml') then $m?fetch() else ()";
            XPathExecutable exec = c.compile(query);
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection2"));
            XdmValue val = eval.evaluate();
            for (XdmItem item : val) {
                assertTrue(item instanceof XdmNode);
            }
            assertEquals(1, val.size());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testCollectionWithHttp() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            XPathExecutable exec = c.compile("count(collection(.))");
            XPathSelector eval = exec.load();
            eval.setContextItem(new XdmAtomicValue("https://dev.saxonica.com/tests/collection/catalog.xml"));
            XdmValue val = eval.evaluate();
            assertEquals(2, Integer.parseInt(val.toString()));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testMixedWithHttpCollection() {
        try {
            Processor p = new Processor(true);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("for $X in (collection(.))" +
                "return if ($X instance of node()) then name($X/*) else $X('id')");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection2.xml"));
            XdmValue val = eval.evaluate();
            assertEquals("1.0e0\n\"html\"", val.toString());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testMixedWithHttpCollection2() {
        try {
            Processor p = new Processor(true);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("count(collection(.))");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection3.xml"));
            XdmValue val = eval.evaluate();
            assertEquals(3, Integer.parseInt(val.toString()));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testJarCollection() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            XPathExecutable exec = c.compile("count(collection(.))");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection3/saxon9-test.jar"));
            XdmValue val = eval.evaluate();
            assertEquals(34, Integer.parseInt(val.toString()));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testJarCollection2() {
        try {
            Processor p = new Processor(true);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("for $c at $p in collection(.) return ($p || ' ' || local-name-from-QName(saxon:type-annotation($c)))");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection3/saxon9-test.jar"));
            XdmValue val = eval.evaluate();
            assertTrue(val.toString().contains("34 base64Binary"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testJarCollectionUri() {
        try {
            Processor p = new Processor(true);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("for $x at $p in uri-collection(.) return ($p || ' ' || xs:string($x))");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection3/saxon9-test.jar"));
            XdmValue val = eval.evaluate();
            System.out.println(val.toString());
            assertTrue(true, "Uri retrieved successfully: ");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testJarCollectionUriWithOptions() {
        try {
            Processor p = new Processor(true);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("for $x in uri-collection(.) return switch(substring-after(xs:string($x), '!/META-INF/'))" +
                "case 'MANIFEST.MF' return true()" +
                "case 'SAXONICA.SF' return true()" +
                "case 'SAXONICA.RSA' return true()" +
                "default return false()");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection3/saxon9-test.jar?select=META-INF/*"));
            XdmValue val = eval.evaluate();
            assertEquals(true, ((XdmAtomicValue) val.itemAt(1)).getBooleanValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testJarCollectionUriWithXmlOptions() {
        try {
            Processor p = new Processor(true);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("for $x in collection(.) return " +
                "if($x instance of node()) then true() else false()");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection3/saxon9he.jar?select=*.xsl;validation=skip"));
            XdmValue val = eval.evaluate();
            assertEquals(true, ((XdmAtomicValue) val.itemAt(1)).getBooleanValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testJarCollectionUriWithXmlOptions2() {
        try {
            Processor p = new Processor(true);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("for $x in collection(.) return " +
                                                      "if($x instance of node()) then true() else false()");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection3/saxon9he.jar?select=*.xsl;validation=skip;content-type=text/plain"));
            XdmValue val = eval.evaluate();
            assertEquals(false, ((XdmAtomicValue) val.itemAt(1)).getBooleanValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testJarMetadataCollection() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            XPathExecutable exec = c.compile("collection(.)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collection3/saxon9-test.jar?metadata=yes"));
            XdmValue val = eval.evaluate();
            for (XdmItem item : val) {
                XdmFunctionItem map = (XdmFunctionItem)item;
                assertTrue(map.call(p, new XdmAtomicValue("entry-name")) != null);
                assertTrue(map.call(p, new XdmAtomicValue("last-modified")).getUnderlyingValue() instanceof DateTimeValue);
            }

            assertEquals(34, val.size());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testCollectionFinder() {
        try {
            Processor p = new Processor(true);
            final Configuration config = p.getUnderlyingConfiguration();
            config.setCollectionFinder(new CollectionFinder() {
                @Override
                public ResourceCollection findCollection(XPathContext context, String collectionURI) throws XPathException {
                    return new DirectoryCollection(config, "http://example.com/collection", new File(configTest.getDataDir() + "trax/"),
                        new URIQueryParameters("select=*.xml;recurse=yes", context.getConfiguration()));
                }
            });
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("for $x in collection(.) return " +
                "if($x instance of node()) then true() else false()");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(new XdmAtomicValue("http://example.com/collection"));
            XdmValue val = eval.evaluate();
            assertEquals(7, val.size());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testCollectionFinderUri() {
        try {
            Processor p = new Processor(true);
            final Configuration config = p.getUnderlyingConfiguration();
            config.setCollectionFinder((context, collectionUri) ->
                                               new DirectoryCollection(config, "http://example.com/collection", new File(configTest.getDataDir() + "trax/"),
                                                                       new URIQueryParameters("select=*.xml;recurse=yes", context.getConfiguration())));

            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("distinct-values(uri-collection(.))");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(new XdmAtomicValue("http://example.com/collection"));
            XdmValue val = eval.evaluate();
            assertEquals(7, val.size());
            assertTrue(val.stream().filter(a -> a.getStringValue().endsWith("cities.xml")).exists());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testCollectionFinderLocally() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable exec = c.compile("distinct-values(uri-collection(.))");
            XPathSelector eval = exec.load();
            eval.getUnderlyingXPathContext().setCollectionFinder(
                    (context, collectionUri) ->
                            new DirectoryCollection(p.getUnderlyingConfiguration(),
                                                    "http://example.com/collection",
                                                    new File(configTest.getDataDir() + "trax/"),
                                                    new URIQueryParameters("select=*.xml;recurse=yes",
                                                                           context.getConfiguration())));

            eval.setContextItem(new XdmAtomicValue("http://example.com/collection"));
            XdmValue val = eval.evaluate();
            assertEquals(7, val.size());
            assertTrue(val.stream().filter(a -> a.getStringValue().endsWith("cities.xml")).exists());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testRegisterMediaType() {
        try {
            Processor p = new Processor(true);
            final Configuration config = p.getUnderlyingConfiguration();
            config.registerFileExtension("java", "text/plain");
            config.registerFileExtension("xq", "text/plain");
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("for $x in collection(.) return " +
                                                      "if($x instance of xs:string) then true() else false()");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "ganta"));
            XdmValue val = eval.evaluate();
            assertEquals(6, val.size());
            for (XdmItem item : val) {
                assertTrue(((XdmAtomicValue)item).getBooleanValue(), "isText");
            }
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testMediaTypeConfigFile() {
        try {
            String cf = "<configuration xmlns='http://saxon.sf.net/ns/configuration'\n" +
                    "               edition='EE'\n" +
                    "               label='Some label'>\n" +
                    "          <resources>\n" +
                    "            <fileExtension extension='xq' mediaType='text/plain'/>" +
                    "            <fileExtension extension='java' mediaType='text/plain'/>" +
                    "          </resources></configuration>";
            Processor p = new Processor(new StreamSource(new StringReader(cf)));
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("for $x in collection(.) return " +
                                                      "if($x instance of xs:string) then true() else false()");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "ganta"));
            XdmValue val = eval.evaluate();
            assertEquals(6, val.size());
            for (XdmItem item : val) {
                assertTrue(((XdmAtomicValue) item).getBooleanValue(), "isText");
            }
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testRegisteredCollection() {
        try {
            Processor p = new Processor(true);
            Configuration config = p.getUnderlyingConfiguration();
            List<File> contents = new ArrayList<>();
            contents.add(new File(configTest.getDataDir() + "trax/xml/cities.xml"));
            contents.add(new File(configTest.getDataDir() + "trax/xml/baz.xml"));
            config.registerCollection("ftp://myCollection", new AbstractResourceCollection(config) {
                @Override
                public Iterator<String> getResourceURIs(XPathContext context) throws XPathException {
                    return contents.stream().map(File::toURI).map(URI::toString).iterator();
                }

                @Override
                public Iterator<? extends Resource> getResources(XPathContext context) throws XPathException {
                    return contents.stream().map(file -> {
                        try {
                            StreamSource ss = new StreamSource(file);
                            NodeInfo doc = config.buildDocumentTree(ss).getRootNode();
                            return new XmlResource(doc);
                        } catch (XPathException e) {
                            throw new UncheckedXPathException(e);
                        }
                    }).iterator();
                }
            });
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            XPathExecutable exec = c.compile("count(uri-collection(.))");
            XPathSelector eval = exec.load();
            eval.setContextItem(new XdmAtomicValue("ftp://myCollection"));
            XdmValue val = eval.evaluate();
            assertEquals(2, Integer.parseInt(val.toString()));
            exec = c.compile("collection(.)/* ! name()");
            eval = exec.load();
            eval.setContextItem(new XdmAtomicValue("ftp://myCollection"));
            val = eval.evaluate();
            assertEquals("cities", val.itemAt(0).getStringValue(), "one");
            assertEquals("foo:document", val.itemAt(1).getStringValue(), "two");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    public static class Finder implements CollectionFinder {
        @Override
        public ResourceCollection findCollection(XPathContext context, String collectionURI) throws XPathException {
            ConfigTest ctest = new ConfigTest();
            if (collectionURI.equals("http://example.com/collection")) {
                File dir = new File(ctest.getDataDir() + "collection");
                URIQueryParameters params = new URIQueryParameters("select=*.xml", context.getConfiguration());
                return new DirectoryCollection(context.getConfiguration(), collectionURI, dir, params);
            } else {
                throw new XPathException("Collection " + collectionURI + " not found");
            }
        }
    }

    @Test
    public void testCollectionFinderConfigFile() {
        try {
            String cf = "<configuration xmlns='http://saxon.sf.net/ns/configuration'\n" +
                    "               edition='EE'\n" +
                    "               label='Some label'>\n" +
                    "          <global collectionFinder='s9apitest.TestCollections$Finder'/>\n" +
                    "    </configuration>";
            Processor p = new Processor(new StreamSource(new StringReader(cf)));
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("distinct-values(collection(\"http://example.com/collection\")!node-name(*))");
            XQueryEvaluator eval = exec.load();
            XdmValue val = eval.evaluate();
            assertEquals(39, val.size());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testCollectionOnDocxFile() {
        try {
            Processor p = new Processor(true);
            Configuration config = p.getUnderlyingConfiguration();

            config.registerFileExtension("docx", "application/java-archive");

            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable exec = c.compile("for $x in collection(.) return " +
                                                      "if($x instance of node()) then true() else false()");
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(composeURI(configTest.getDataDir(), "collecton_test_doc.docx?select=*.xml;validation=skip;content-type=text/plain"));
            XdmValue val = eval.evaluate();
            assertEquals(false, ((XdmAtomicValue) val.itemAt(1)).getBooleanValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }


    }


}
