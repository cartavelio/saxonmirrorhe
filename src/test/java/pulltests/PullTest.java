package pulltests;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.xqj.pull.PullToStax;
import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.event.EventSource;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.StandardURIResolver;
import net.sf.saxon.om.FingerprintedQName;
import net.sf.saxon.query.QueryResult;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trace.Instrumentation;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyDocumentImpl;
import net.sf.saxon.type.Untyped;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamConstants;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import javax.xml.transform.Result;
import javax.xml.transform.Source;
import javax.xml.transform.Transformer;
import javax.xml.transform.stax.StAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.HashMap;
import java.util.Map;
import java.util.function.Predicate;

/**
 *
 */
public class PullTest extends TestCase {

    public void testPullSource() {
        try {
            // Note: this test fails with the Sun XMLStreamReader because the initial state
            // of the parser is 3 (XML Processing Instruction) rather than 7 (start document)
            // as required by the new StaxSource() constructor.
            String strStylSheetPath = ConfigTest.DATA_DIR + "books.xsl";
            String omElemPath = ConfigTest.DATA_DIR + "books.xml";
            InputStream imp = new FileInputStream(new File(omElemPath));
            InputStreamReader impsr = new InputStreamReader(imp);
            System.setProperty("javax.xml.stream.XMLInputFactory", "com.ctc.wstx.stax.WstxInputFactory");
            XMLStreamReader parser = XMLInputFactory.newInstance().createXMLStreamReader(impsr);
            System.err.println(parser.getEventType());
            FileInputStream fileInputStream = new FileInputStream(strStylSheetPath);
            Source srcStylSheet = new StreamSource(fileInputStream);
            System.setProperty("javax.xml.transform.TransformerFactory", TransformerFactoryImpl.class.getName());
            Transformer xDSFieldsValidationStylesheet = TransformerFactoryImpl.newInstance().newTransformer(srcStylSheet);
            Source src = new StAXSource(parser);
            ByteArrayOutputStream bos = new ByteArrayOutputStream();
            Result rs = new StreamResult(bos);

            xDSFieldsValidationStylesheet.transform(src, rs);

        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    public void testSaxonStreamOnEventSource() throws Exception {
        testSaxonOnEventSource(true);
    }

    public void testDocOnEventSource() throws Exception {
        testSaxonOnEventSource(false);
    }

    public void testSaxonOnEventSource(boolean streaming) throws Exception {
        Configuration configuration = new EnterpriseConfiguration();
        configuration.setBooleanProperty(Feature.STREAMING_FALLBACK, true);

        Processor processor = new Processor(configuration);
        XQueryCompiler xqueryCompiler = processor.newXQueryCompiler();
        String query = streaming ? "saxon:stream(doc('uriresolver:resolve')/(a/b))/c"
                : "(doc('uriresolver:resolve')/(a/b))/c";
        XQueryExecutable xqueryExecutable = xqueryCompiler.compile(query);
        XQueryEvaluator xqueryEvaluator = xqueryExecutable.load();

        xqueryEvaluator.setURIResolver(new StandardURIResolver() {
            @Override
            public Source resolve(String href, String base) {
                return new EventSource() {
                    @Override
                    public void send(Receiver receiver) throws XPathException {
                        receiver.open();
                        receiver.startDocument(0);
                        receiver.startElement(new FingerprintedQName("", "", "a"), Untyped.getInstance(), LOCATION, 0);
                        receiver.startContent();
                        receiver.startElement(new FingerprintedQName("", "", "b"), Untyped.getInstance(), LOCATION, 0);
                        receiver.startContent();
                        receiver.startElement(new FingerprintedQName("", "", "c"), Untyped.getInstance(), LOCATION, 0);
                        receiver.startContent();
                        receiver.characters("X", LOCATION, 0);
                        receiver.endElement();
                        receiver.endElement();
                        receiver.endElement();
                        receiver.endDocument();
                        receiver.close();
                    }
                };
            }
        });

        int count = 0;
        for (XdmItem item : xqueryEvaluator) {
            assertEquals("<c>X</c>", item.toString().trim());
            ++count;
        }
        assertEquals(count, 1);
        xqueryEvaluator.close();
    }

//	private static final int LOCATION = 0; // for 9.6

    private static final Location LOCATION = new Location() {
        public int getColumnNumber() {
            return 0;
        }

        public int getLineNumber() {
            return 0;
        }

        public String getPublicId() {
            return null;
        }

        public String getSystemId() {
            return null;
        }

        public Location saveLocation() {
            return this;
        }
    };


    public void testPullFromGraftedTinyTree() {

        try {
            Processor proc = new Processor(true);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode root = builder.build(new StreamSource(new StringReader("<a><b/></a>")));
            XdmNode branch = builder.build(new StreamSource(new StringReader("<p><q>quire</q></p>")));
            XsltCompiler xc = proc.newXsltCompiler();


            // Walk through the initial (ungrafted) tinytree

//            PullFromIterator pull = new PullFromIterator(
//                    SingleNodeIterator.makeIterator(root.getUnderlyingNode()));
//            pull.setPipelineConfiguration(proc.getUnderlyingConfiguration().makePipelineConfiguration());
            XMLStreamReader reader = PullToStax.getXMLStreamReader(root.getUnderlyingNode());
            try {
                assertEquals("a01", XMLStreamConstants.START_DOCUMENT, reader.getEventType());
                assertEquals("a02", XMLStreamConstants.START_ELEMENT, reader.next());
                assertEquals("a03", new javax.xml.namespace.QName("a"), reader.getName());
                assertEquals("a04", XMLStreamConstants.START_ELEMENT, reader.next());
                assertEquals("a05", new javax.xml.namespace.QName("b"), reader.getName());
                assertEquals("a06", XMLStreamConstants.END_ELEMENT, reader.next());
                assertEquals("a07", new javax.xml.namespace.QName("b"), reader.getName());
                assertEquals("a08", XMLStreamConstants.END_ELEMENT, reader.next());
                assertEquals("a09", new javax.xml.namespace.QName("a"), reader.getName());
                assertEquals("a10", XMLStreamConstants.END_DOCUMENT, reader.next());
                try {
                    reader.next();
                    fail("Unexpected success");
                } catch (IllegalStateException e) {
                    // OK - expected failure;
                }

            } catch (XMLStreamException e) {
                e.printStackTrace();
            }

            // Walk through the tinytree after grafting extra content

            String xsl = ""
                    + "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>\n"
                    + "  <xsl:mode on-no-match='shallow-copy'/>"
                    + "  <xsl:param name='p'/>"
                    + "  <xsl:variable name='a2'><xsl:for-each select='/*'><xsl:copy><xsl:copy-of select='$p'/></xsl:copy></xsl:for-each></xsl:variable>"
                    + "  <xsl:template match='/'><xsl:sequence select='$a2'/></xsl:template>"
                    + "</xsl:stylesheet>";
            Xslt30Transformer xt = xc.compile(new StreamSource(new StringReader(xsl))).load30();
            xt.setGlobalContextItem(root);
            RawDestination out = new RawDestination();
            Map<QName, XdmNode> params = new HashMap<>();
            params.put(new QName("p"), branch);
            xt.setStylesheetParameters(params);
            xt.applyTemplates(root, out);
            XdmNode graftedTree = (XdmNode) out.getXdmValue();
            assertEquals(XdmNodeKind.DOCUMENT, graftedTree.getNodeKind());

            assertTrue(((TinyDocumentImpl)graftedTree.getUnderlyingNode()).containsGraftedNodes());
            try {
                System.out.println(QueryResult.serialize(graftedTree.getUnderlyingNode()));
            } catch (XPathException e) {
                e.printStackTrace();
            }

//            pull = new PullFromIterator(
//                    SingleNodeIterator.makeIterator(graftedTree.getUnderlyingNode()));
//            pull.setPipelineConfiguration(proc.getUnderlyingConfiguration().makePipelineConfiguration());
//            reader = new PullToStax(pull);
            reader = PullToStax.getXMLStreamReader(graftedTree.getUnderlyingNode());
            try {
                assertEquals("b01", XMLStreamConstants.START_DOCUMENT, reader.getEventType());
                assertEquals("b02", XMLStreamConstants.START_ELEMENT, reader.next());
                assertEquals("b03", new javax.xml.namespace.QName("a"), reader.getName());
                assertEquals("b04", XMLStreamConstants.START_ELEMENT, reader.next());
                assertEquals("b05", new javax.xml.namespace.QName("p"), reader.getName());
                assertEquals("b06", XMLStreamConstants.START_ELEMENT, reader.next());
                assertEquals("b07", new javax.xml.namespace.QName("q"), reader.getName());
                assertEquals("b08", XMLStreamConstants.CHARACTERS, reader.next());
                assertEquals("b09", reader.getText(), "quire");
                assertEquals("b10", XMLStreamConstants.END_ELEMENT, reader.next());
                assertEquals("b11", new javax.xml.namespace.QName("q"), reader.getName());
                assertEquals("b12", XMLStreamConstants.END_ELEMENT, reader.next());
                assertEquals("b13", new javax.xml.namespace.QName("p"), reader.getName());
                assertEquals("b14", XMLStreamConstants.END_ELEMENT, reader.next());
                assertEquals("b15", new javax.xml.namespace.QName("a"), reader.getName());
                assertEquals("b16", XMLStreamConstants.END_DOCUMENT, reader.next());
                try {
                    reader.next();
                    fail("Unexpected success");
                } catch (IllegalStateException e) {
                    // OK - expected failure;
                }

            } catch (XMLStreamException e) {
                e.printStackTrace();
            }


        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testDescendantIteratorOverGraftedTinyTree() {

        try {
            Processor proc = new Processor(true);
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode root = builder.build(new StreamSource(new StringReader("<a><b/><c>choir</c></a>")));
            XdmNode branch = builder.build(new StreamSource(new StringReader("<p><q>quire</q></p>")));
            XsltCompiler xc = proc.newXsltCompiler();

            // Walk through the tinytree after grafting extra content

            String xsl = ""
                    + "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>\n"
                    + "  <xsl:mode on-no-match='shallow-copy'/>"
                    + "  <xsl:param name='p'/>"
                    + "  <xsl:variable name='a2'><xsl:for-each select='/*'><xsl:copy><xsl:copy-of select='$p'/><xsl:copy-of select='*'/></xsl:copy></xsl:for-each></xsl:variable>"
                    + "  <xsl:template match='/'><xsl:sequence select='$a2'/></xsl:template>"
                    + "</xsl:stylesheet>";
            Xslt30Transformer xt = xc.compile(new StreamSource(new StringReader(xsl))).load30();
            xt.setGlobalContextItem(root);
            RawDestination out = new RawDestination();
            Map<QName, XdmNode> params = new HashMap<>();
            params.put(new QName("p"), branch);
            xt.setStylesheetParameters(params);
            xt.applyTemplates(root, out);
            XdmNode graftedTree = (XdmNode) out.getXdmValue();
            assertEquals(XdmNodeKind.DOCUMENT, graftedTree.getNodeKind());

            assertTrue(((TinyDocumentImpl) graftedTree.getUnderlyingNode()).containsGraftedNodes());
            try {
                System.out.println(QueryResult.serialize(graftedTree.getUnderlyingNode()));
            } catch (XPathException e) {
                e.printStackTrace();
            }

            XdmSequenceIterator<XdmNode> iter = graftedTree.axisIterator(Axis.DESCENDANT_OR_SELF);
            expect(iter, "00", XdmNodeKind.DOCUMENT, node->true);
            expect(iter, "01", XdmNodeKind.ELEMENT, hasName("a"));
            expect(iter, "02", XdmNodeKind.ELEMENT, hasName("p"));
            expect(iter, "03", XdmNodeKind.ELEMENT, hasName("q"));
            expect(iter, "04", XdmNodeKind.TEXT, hasStringValue("quire"));
            expect(iter, "05", XdmNodeKind.ELEMENT, hasName("b"));
            expect(iter, "06", XdmNodeKind.ELEMENT, hasName("c"));
            expect(iter, "07", XdmNodeKind.TEXT, hasStringValue("choir"));
            assertFalse("08", iter.hasNext());



        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }


    }

    private void expect(XdmSequenceIterator<XdmNode> iter, String label, XdmNodeKind kind, Predicate<XdmNode> predicate) {
        assertTrue("exists" + label, iter.hasNext());
        XdmNode next = iter.next();
        assertEquals("kind" + label, next.getNodeKind(), kind);
        assertTrue("test" + label, predicate.test(next));
    }

    private Predicate<XdmNode> hasName(String local) {
        return node -> node.getNodeName().getLocalName().equals(local);
    }

    private Predicate<XdmNode> hasStringValue(String value) {
        return node -> node.getStringValue().equals(value);
    }


    public void testBug4507Pull() {
        try {
            Processor proc = new Processor(true);
            SchemaManager sm = proc.getSchemaManager();
            sm.load(new StreamSource(
                    new File("/Users/mike/bugs/2020/4507-Carter/2000-04-03/data/schema.xsd")));
            SchemaValidator val = sm.newSchemaValidator();
            InputStream imp = new FileInputStream(new File("/Users/mike/bugs/2020/4507-Carter/2000-04-03/data/data.xml"));
            InputStreamReader impsr = new InputStreamReader(imp);
            System.setProperty("javax.xml.stream.XMLInputFactory", "com.ctc.wstx.stax.WstxInputFactory");
            XMLStreamReader parser = XMLInputFactory.newInstance().createXMLStreamReader(impsr);
            val.validate(new StAXSource(parser));
            Instrumentation.report();
        } catch (SaxonApiException | FileNotFoundException | XMLStreamException e) {
            fail();
        }
    }

}

