package s9apitest;

import net.sf.saxon.Configuration;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.URIQueryParameters;
import net.sf.saxon.junit.TestHttpServer;
import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.CollectionFinder;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.Resource;
import net.sf.saxon.lib.ResourceCollection;
import net.sf.saxon.om.Item;
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
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.ByteArrayInputStream;
import java.io.File;
import java.io.StringReader;
import java.net.URI;
import java.nio.charset.StandardCharsets;
import java.nio.file.Path;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

import static org.junit.jupiter.api.Assertions.*;

/**
 * JUnit tests for handling of Saxon collection URIs and collection URI resolver APIs
 */
public class TestCollectionsEE {
    private final Path testDataDirPath = Testdata.dir().resolve("collectionTests");
    private TestHttpServer testHttp = new TestHttpServer(testDataDirPath);

    private XdmAtomicValue composeURI(String additional, String query) {
        return new XdmAtomicValue(Testdata.file(additional).toURI() + query);
    }

    @Test
    public void testStrictValidation() {
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(Feature.USE_XSI_SCHEMA_LOCATION, true);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable exec = c.compile("collection(.)/* instance of element(*, xs:untyped)");
            XPathSelector eval = exec.load();
            eval.setContextItem(composeURI("", "?select=books.xml;recurse=no;validation=strict"));
            XdmValue val = eval.evaluate();
            assertFalse(((XdmAtomicValue) val).getBooleanValue());
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
            eval.setContextItem(composeURI("", "?select=books.xml;recurse=no;validation=skip"));
            XdmValue val = eval.evaluate();
            assertEquals(true, ((XdmAtomicValue) val).getBooleanValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }
}
