package resolvers;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.ResourceRequest;
import net.sf.saxon.lib.ResourceResolver;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.Source;
import javax.xml.transform.sax.SAXSource;
import java.io.File;
import java.io.FileInputStream;
import java.nio.file.Files;
import java.util.Iterator;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

public class CatalogResolverTest {
    private final ConfigTest configTest = new ConfigTest();

    @Test
    public void noResolverFail() {
        Configuration config = new Configuration();
        Processor processor = new Processor(config);
        DocumentBuilder builder = processor.newDocumentBuilder();
        builder.setDTDValidation(true);
        try {
            XdmNode doc = builder.build(new File("src/test/testdata/resolvers/input.xml"));
            fail();
        } catch (SaxonApiException ex) {
            assert(ex.getMessage().contains("I/O error"));
        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }

    @Test
    public void noResolverPass() {
        Configuration config = new Configuration();
        Processor processor = new Processor(config);
        DocumentBuilder builder = processor.newDocumentBuilder();
        builder.setDTDValidation(true);
        try {
            XdmNode doc = builder.build(new File("src/test/testdata/resolvers/ok.xml"));
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void xmlResolverParse() {
        Processor processor = new Processor(false);
        processor.setCatalogFiles("src/test/testdata/resolvers/catalog.xml");
        try {
            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setDTDValidation(true);
            XdmNode doc = builder.build(new File("src/test/testdata/resolvers/input.xml"));
            Assertions.assertNotNull(doc);
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void xmlResolverUriResolver() {
        Processor processor = new Processor(false);
        processor.setCatalogFiles("src/test/testdata/resolvers/catalog.xml");
        try {
            XsltCompiler compiler = processor.newXsltCompiler();

            Source xsl = new SAXSource(new InputSource(new FileInputStream(new File("src/test/testdata/resolvers/style.xsl"))));
            XsltExecutable exec = compiler.compile(xsl);

            Xslt30Transformer transformer = exec.load30();

            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setDTDValidation(true);
            XdmNode doc = builder.build(new File("src/test/testdata/resolvers/input.xml"));

            transformer.setGlobalContextItem(doc);

            XdmDestination destination = new XdmDestination();
            transformer.applyTemplates(doc, destination);

            System.out.println(destination.getXdmNode());

            } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void xmlResolverStylesheetUriResolver() {
        Processor processor = new Processor(false);
        processor.setCatalogFiles("src/test/testdata/resolvers/catalog.xml");
        try {
            XsltCompiler compiler = processor.newXsltCompiler();

            Source xsl = new SAXSource(new InputSource(new FileInputStream(new File("src/test/testdata/resolvers/stylesheet.xsl"))));
            XsltExecutable exec = compiler.compile(xsl);

            Xslt30Transformer transformer = exec.load30();

            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setDTDValidation(true);
            XdmNode doc = builder.build(new File("src/test/testdata/resolvers/input.xml"));

            transformer.setGlobalContextItem(doc);

            XdmDestination destination = new XdmDestination();
            transformer.applyTemplates(doc, destination);

            System.out.println(destination.getXdmNode());

        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void xmlResolverUnparsedTextUriResolver() {
        Processor processor = new Processor(false);
        processor.setCatalogFiles("src/test/testdata/resolvers/catalog.xml");
        try {
            XsltCompiler compiler = processor.newXsltCompiler();

            Source xsl = new SAXSource(new InputSource(new FileInputStream(new File("src/test/testdata/resolvers/stylesheet.xsl"))));
            XsltExecutable exec = compiler.compile(xsl);

            Xslt30Transformer transformer = exec.load30();

            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setDTDValidation(false);
            XdmNode doc = builder.build(new File("src/test/testdata/resolvers/unparsed.xml"));

            transformer.setGlobalContextItem(doc);

            XdmDestination destination = new XdmDestination();
            transformer.applyTemplates(doc, destination);

            System.out.println(destination.getXdmNode());

        } catch (Exception ex) {
            ex.printStackTrace();
            fail();
        }
    }

    @Test
    public void xmlResolverModuleUriResolver() {
        Processor processor = new Processor(false);
        processor.setCatalogFiles("src/test/testdata/resolvers/catalog.xml");
        try {
            XQueryCompiler compiler = processor.newXQueryCompiler();

            File xqy = new File("src/test/testdata/resolvers/module.xqy");
            XQueryExecutable exec = compiler.compile(xqy);

            XQueryEvaluator eval = exec.load();

            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setDTDValidation(false);
            XdmNode doc = builder.build(new File("src/test/testdata/resolvers/unparsed.xml"));

            eval.setContextItem(doc);

            Iterator<XdmItem> iter = eval.iterator();
            while (iter.hasNext()) {
                System.out.println(iter.next().toString());
            }

        } catch (Exception ex) {
            fail();
        }
    }


    @Test
    public void testMagicUri() {
        try {
            Processor processor = new Processor(false);
            XPathCompiler xpc = processor.newXPathCompiler();
            // This is a special URI that can be used to check that the xmlresolver is present and correct
            XPathExecutable exec = xpc.compile("doc-available('https://xmlresolver.org/data/resolver/succeeded/test/check.xml')");
            XPathSelector s = exec.load();
            XdmValue val = s.evaluate();
            assertTrue(val instanceof XdmAtomicValue);
            assertTrue(((XdmAtomicValue)val).getBooleanValue());
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void resolveWithFileURI() {
        // This stylesheet contains a doc() call that loads a file directly. That means it
        // goes through the DirectResourceResolver, gets a different kind of source,
        // gets different options, and succeeds.
        resolveWith("style-file.xsl");
    }

    @Test
    public void resolveWithHttpURI() {
        // This stylesheet contains a doc() call that loads a URI. The URI is resolved
        // by the catalog to the same document as the resolveWithFileURI test, but it
        // gets a different kind of source, different options, and fails.
        //
        // Basically, in this case, the parser options include the configured DTD
        // validation setting which won't work if you're loading a stylesheet.
        resolveWith("style-http.xsl");
    }

    private void resolveWith(String xslFile) {
        Processor processor = new Processor(false);
        processor.setCatalogFiles("src/test/testdata/resolvers/catalog.xml");
        try {
            XsltCompiler compiler = processor.newXsltCompiler();

            String fn = "src/test/testdata/resolvers/" + xslFile;
            Source xsl = new SAXSource(new InputSource(Files.newInputStream(new File(fn).toPath())));
            XsltExecutable exec = compiler.compile(xsl);

            Xslt30Transformer transformer = exec.load30();

            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setDTDValidation(true);
            XdmNode doc = builder.build(new File("src/test/testdata/resolvers/input.xml"));

            transformer.setGlobalContextItem(doc);

            XdmDestination destination = new XdmDestination();
            transformer.applyTemplates(doc, destination);

            System.out.println(destination.getXdmNode());

        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void resolveWithJAXPEntityResolver() {
        // This is a test for #5950
        
        Processor processor = new Processor(false);
        processor.getUnderlyingConfiguration().setResourceResolver(new Bug5950Resolver());

        try {
            XsltCompiler compiler = processor.newXsltCompiler();

            String fn = configTest.getDataDir() + "bug5950/identity.xsl";
            Source xsl = new SAXSource(new InputSource(Files.newInputStream(new File(fn).toPath())));
            XsltExecutable exec = compiler.compile(xsl);

            Xslt30Transformer transformer = exec.load30();

            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setDTDValidation(true);
            XdmNode doc = builder.build(new File(configTest.getDataDir() + "bug5950/doc.xml"));

            transformer.setGlobalContextItem(doc);

            XdmDestination destination = new XdmDestination();
            transformer.applyTemplates(doc, destination);

            System.out.println(destination.getXdmNode());

        } catch (Exception ex) {
            fail();
        }
    }

    private class Bug5950Resolver implements ResourceResolver {

        @Override
        public Source resolve(ResourceRequest request) throws XPathException {
            if ("chapter.xml".equals(request.uri)) {
                return new SAXSource(new InputSource(configTest.getDataDir() + "bug5950/chapter.xml"));
                /*
                // This shows what happens if you return a StAXSource...it goes bang.
                try {
                    XMLInputFactory factory = XMLInputFactory.newInstance();
                    XMLEventReader reader = factory.createXMLEventReader(new FileInputStream(configTest.getDataDir() + "bug5950/chapter.xml"));
                    return new StAXSource(reader);
                } catch (Exception ex) {
                    throw new RuntimeException(ex);
                }
                 */
                /*
                // This shows what happens if you return a DOMSource...also bang.
                try {
                    DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
                    javax.xml.parsers.DocumentBuilder builder = factory.newDocumentBuilder();
                    Document doc = builder.parse(configTest.getDataDir() + "bug5950/chapter.xml");
                    return new DOMSource(doc);
                } catch (Exception ex) {
                    throw new RuntimeException(ex);
                }
                 */
            }
            return null;
        }
    }

}
