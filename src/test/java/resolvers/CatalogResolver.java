package resolvers;

import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.SaxonResourceResolver;
import net.sf.saxon.s9api.*;
import org.junit.Assert;
import org.junit.Test;
import org.xml.sax.InputSource;
import org.xmlresolver.ResolverFeature;

import javax.xml.transform.Source;
import javax.xml.transform.sax.SAXSource;
import java.io.File;
import java.io.FileInputStream;
import java.util.Collections;
import java.util.Iterator;
import java.util.List;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertNotNull;
import static org.junit.Assert.fail;

public class CatalogResolver {
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
        Configuration config = new Configuration();
        SaxonResourceResolver resolver = config.getSaxonResourceResolver();
        try {
            resolver.setFeature(ResolverFeature.CATALOG_ADDITIONS, Collections.singletonList("src/test/testdata/resolvers/catalog.xml"));
            Processor processor = new Processor(config);
            DocumentBuilder builder = processor.newDocumentBuilder();
            builder.setDTDValidation(true);
            XdmNode doc = builder.build(new File("src/test/testdata/resolvers/input.xml"));
            Assert.assertNotNull(doc);
        } catch (Exception ex) {
            fail();
        }
    }

    @Test
    public void xmlResolverUriResolver() {
        Configuration config = new Configuration();
        SaxonResourceResolver resolver = config.getSaxonResourceResolver();
        try {
            resolver.setFeature(ResolverFeature.CATALOG_ADDITIONS, Collections.singletonList("src/test/testdata/resolvers/catalog.xml"));
            Processor processor = new Processor(config);
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
        Configuration config = new Configuration();
        SaxonResourceResolver resolver = config.getSaxonResourceResolver();
        try {
            resolver.setFeature(ResolverFeature.CATALOG_ADDITIONS, Collections.singletonList("src/test/testdata/resolvers/catalog.xml"));
            Processor processor = new Processor(config);
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
        Configuration config = new Configuration();
        SaxonResourceResolver resolver = config.getSaxonResourceResolver();
        try {
            resolver.setFeature(ResolverFeature.CATALOG_ADDITIONS, Collections.singletonList("src/test/testdata/resolvers/catalog.xml"));
            Processor processor = new Processor(config);
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
            fail();
        }
    }

    @Test
    public void xmlResolverModuleUriResolver() {
        Configuration config = new Configuration();
        SaxonResourceResolver resolver = config.getSaxonResourceResolver();
        try {
            resolver.setFeature(ResolverFeature.CATALOG_ADDITIONS, Collections.singletonList("src/test/testdata/resolvers/catalog.xml"));
            Processor processor = new Processor(config);
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

}
