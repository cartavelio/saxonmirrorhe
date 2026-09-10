package jaxptest;

import net.sf.saxon.Configuration;
import net.sf.saxon.jaxp.SaxonTransformerFactory;
import net.sf.saxon.lib.EmptySource;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.StandardURIResolver;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.Xslt30Transformer;
import net.sf.saxon.s9api.XsltCompiler;
import net.sf.saxon.s9api.XsltExecutable;
import net.sf.saxon.trans.XPathException;
import org.jdom2.Document;
import org.jdom2.JDOMException;
import org.jdom2.input.SAXBuilder;
import org.jdom2.transform.JDOMSource;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.*;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.nio.charset.StandardCharsets;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.assertSame;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 * JUnit tests for the JAXP Transformation API (trax).
 * Adapted by converting the TraxExamples file
 * originally created by Scott Boag, but much evolved since.
 */
public class URIResolverTest {
    private final ConfigTest configTest = new ConfigTest();

    private final String d = configTest.getDataDir() + "trax/";

    private final String foo_xml = d + "xml/foo.xml";
    private final String foo_xsl = d + "xsl/foo.xsl";

    @Test
    public void testGetURIResolver() {

        try {

            // Create a transform factory instance.
            //TransformerFactory tfactory = TransformerFactory.newInstance();
            TransformerFactory tfactory = new net.sf.saxon.TransformerFactoryImpl();
            tfactory.setAttribute(FeatureKeys.OPTIMIZATION_LEVEL, "-j"); // Disable JIT compilation
            Resolver1 phoo = new Resolver1("phoo");
            tfactory.setURIResolver(phoo);
            assertSame(tfactory.getURIResolver(), phoo);

            Templates foo1Templates = tfactory.newTemplates(new StreamSource(foo_xsl));
            assertTrue(phoo.history.contains("inc1/inc1.xsl"));
            assertTrue(phoo.history.contains("inc2/inc2.xsl"));

            Transformer foo1Transformer = foo1Templates.newTransformer();
            assertSame(foo1Transformer.getURIResolver(), phoo);

            Resolver1 phut = new Resolver1("phut");
            phut.map("inc1/inc1.xsl", "identity.xsl");
            tfactory.setURIResolver(phut);
            assertSame(tfactory.getURIResolver(), phut);

            Resolver1 phum = new Resolver1("phum");
            foo1Transformer.setURIResolver(phum);
            assertSame(foo1Transformer.getURIResolver(), phum);
            assertSame(tfactory.getURIResolver(), phut);

            foo1Transformer.setURIResolver(null);
            assertNull(foo1Transformer.getURIResolver());
            assertSame(tfactory.getURIResolver(), phut);

            Templates foo2Templates;
            try {
                foo2Templates = tfactory.newTemplates(new StreamSource(foo_xsl));
                fail("Should have failed");
            } catch (TransformerException err) {
                // expected failure
            }
            assertTrue(phut.history.contains("inc1/inc1.xsl"));
            assertFalse(phut.history.contains("inc2/inc2.xsl"));

            String whimsy = "<xsl:transform version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><xsl:copy-of select=\"doc('mimsy.xml')\"/></xsl:template>" +
                    "</xsl:transform>";
            Templates foo3Templates = tfactory.newTemplates(
                    new StreamSource(new StringReader(whimsy), new StreamSource(foo_xsl).getSystemId()));
            Resolver1 whimsical = new Resolver1("whim");
            whimsical.map("mimsy.xml", "../xml/baz.xml");
            tfactory.setURIResolver(whimsical);
            Transformer transformer3 = foo3Templates.newTransformer();
            StringWriter sw3 = new StringWriter();
            transformer3.transform(new StreamSource(foo_xml), new StreamResult(sw3));
            assertTrue(sw3.toString().contains("<foo:document"));

            tfactory.setURIResolver(new StandardURIResolver());
            Templates foo4Templates = tfactory.newTemplates(
                    new StreamSource(new StringReader(whimsy), new StreamSource(foo_xsl).getSystemId()));
            whimsical.map("mimsy.xml", "../xml/cities.xml");
            Transformer transformer4 = foo4Templates.newTransformer();
            transformer4.setURIResolver(whimsical);
            StringWriter sw4 = new StringWriter();
            transformer4.transform(new StreamSource(foo_xml), new StreamResult(sw4));
            assertTrue(sw4.toString().contains("<cities"));
            assertTrue(whimsical.history.contains("mimsy.xml"));


        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }


    private static class Resolver1 extends StandardURIResolver {

        public String name;
        public Set<String> history = new HashSet<String>();
        public Map<String, String> mappings = new HashMap<String, String>();

        public Resolver1(String nameOfFoo) {
            this.name = nameOfFoo;
        }

        public void map(String from, String to) {
            mappings.put(from, to);
        }

        @Override
        public Source resolve(String href, String base) throws XPathException {
            history.add(href);
            for (Map.Entry<String, String> entry : mappings.entrySet()) {
                href = href.replace(entry.getKey(), entry.getValue());
            }
            return super.resolve(href, base);
        }
    }

    @Test
    public void testEmptySource() {
        // Bug 4752 introduces the ability for the URIResolver to return an EmptySource object,
        // interpreted as "no document": the effect is that the doc() and document() functions
        // return an empty sequence.
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            XsltExecutable exec = comp.compile(
                    new StreamSource(new StringReader(
                    "<out xsl:version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:copy-of select='document(\"unknown.xml\")'/></out>")));
            Xslt30Transformer trans = exec.load30();
            StringWriter sw = new StringWriter();
            trans.setURIResolver((href, base) -> EmptySource.getInstance());
            trans.applyTemplates(new StreamSource(new StringReader("<in/>")),
                                 proc.newSerializer(sw));
            assertTrue(sw.toString().endsWith("<out/>"));
        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }

    @Test
    public void testEmptyStylesheet() {
        // Bug 4752 introduces the ability for the URIResolver to return an EmptySource object,
        // interpreted as "no document": the effect is that the doc() and document() functions
        // return an empty sequence.
        try {
            Processor proc = new Processor(false);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setURIResolver((href, base) -> EmptySource.getInstance());
            XsltExecutable exec = comp.compile(
                    new StreamSource(new StringReader(
                            "<xsl:transform version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                                    "<xsl:include href='unknown.xml'/></xsl:transform>")));
            Xslt30Transformer trans = exec.load30();
            StringWriter sw = new StringWriter();
            trans.applyTemplates(new StreamSource(new StringReader("<in>xyz</in>")),
                                 proc.newSerializer(sw));
            assertTrue(sw.toString().endsWith("xyz"));
        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }

    @Test
    public void testSaxSourceForUnparsedText_ReturningReader() {
        // Bug 5582: for legacy reasons (and Ant), we should allow an unparsed-text URI to return
        // a SAXSource
        try {
            TransformerFactory f = new SaxonTransformerFactory();
            f.setURIResolver(new URIResolver() {
                @Override
                public Source resolve(String href, String base) throws TransformerException {
                    if (href.endsWith("foo.txt")) {
                        return new SAXSource(new InputSource(new StringReader("this be foo")));
                    } else {
                        return null;
                    }
                }
            });
            Templates exec = f.newTemplates(
                    new StreamSource(new StringReader(
                            "<e xsl:version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
                                    "content='{unparsed-text(\"foo.txt\")}'/>")));

            Transformer trans = exec.newTransformer();

            StringWriter sw = new StringWriter();
            trans.transform(new StreamSource(new StringReader("<in>xyz</in>")),
                                  new StreamResult(sw));
            assertTrue(sw.toString().contains("<e content=\"this be foo\"/>"));
        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }

    @Test
    public void testSaxSourceForUnparsedText_ReturningStream() {
        // Bug 5582: for legacy reasons (and Ant), we should allow an unparsed-text URI to return
        // a SAXSource
        try {
            TransformerFactory f = new SaxonTransformerFactory();
            f.setURIResolver(new URIResolver() {
                @Override
                public Source resolve(String href, String base) throws TransformerException {
                    if (href.endsWith("foo.txt")) {
                        ByteArrayInputStream stream = new ByteArrayInputStream("this be foo".getBytes(StandardCharsets.UTF_8));
                        return new SAXSource(new InputSource(stream));
                    } else {
                        return null;
                    }
                }
            });
            Templates exec = f.newTemplates(
                    new StreamSource(new StringReader(
                            "<e xsl:version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
                                    "content='{unparsed-text(\"foo.txt\")}'/>")));

            Transformer trans = exec.newTransformer();

            StringWriter sw = new StringWriter();
            trans.transform(new StreamSource(new StringReader("<in>xyz</in>")),
                            new StreamResult(sw));
            assertTrue(sw.toString().contains("<e content=\"this be foo\"/>"));
        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }


    @Test
    public void testBug5800() {
        // URIResolver for stylesheet modules returns a JDOMSource
        try {
            net.sf.saxon.TransformerFactoryImpl tfactory = new net.sf.saxon.TransformerFactoryImpl();
            Resolver2 phoo = new Resolver2(tfactory.getConfiguration());
            tfactory.setURIResolver(phoo);
            assertSame(tfactory.getURIResolver(), phoo);

            Templates foo1Templates = tfactory.newTemplates(new StreamSource(foo_xsl));
            assertTrue(phoo.history.contains("inc1/inc1.xsl"));
            assertTrue(phoo.history.contains("inc2/inc2.xsl"));

            Transformer foo1Transformer = foo1Templates.newTransformer();
            assertSame(foo1Transformer.getURIResolver(), phoo);

        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }

    private static class Resolver2 extends StandardURIResolver {

        public Configuration config;
        public Set<String> history = new HashSet<String>();
        public Map<String, String> mappings = new HashMap<String, String>();

        public Resolver2(Configuration config) {
            this.config = config;
        }

        public void map(String from, String to) {
            mappings.put(from, to);
        }

        @Override
        public Source resolve(String href, String base) throws XPathException {
            try {
                history.add(href);
                base = base.replace("/dummy/", "");
                for (Map.Entry<String, String> entry : mappings.entrySet()) {
                    href = href.replace(entry.getKey(), entry.getValue());
                }
                SAXBuilder builder = new SAXBuilder();
                File file;
                if (href.endsWith("inc1/inc1.xsl")) {
                    file = new File("/Users/mike/GitHub/saxon2020/src/test/testdata/trax/xsl/inc1/inc1.xsl");
                } else if (href.endsWith("inc2/inc2.xsl")) {
                    file = new File("/Users/mike/GitHub/saxon2020/src/test/testdata/trax/xsl/inc1/inc2/inc2.xsl");
                } else {
                    throw new XPathException("Unknown file " + href);
                }
                Document doc = builder.build(file);
                JDOMSource js = new JDOMSource(doc);
                js.setSystemId(base + "/dummy/" + href);
                return js;
            } catch (JDOMException | IOException e) {
                throw new XPathException(e);
            }
        }
    }


}
