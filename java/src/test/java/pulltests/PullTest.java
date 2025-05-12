package pulltests;

import com.saxonica.config.EnterpriseConfiguration;
import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.event.EventSource;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.StandardURIResolver;
import net.sf.saxon.om.*;
import net.sf.saxon.s9api.*;
import net.sf.saxon.str.BMPString;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Untyped;
import org.junit.jupiter.api.Test;

import javax.xml.stream.XMLInputFactory;
import javax.xml.stream.XMLStreamReader;
import javax.xml.transform.Result;
import javax.xml.transform.Source;
import javax.xml.transform.Transformer;
import javax.xml.transform.stax.StAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.*;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *
 */
public class PullTest {
    private final ConfigTest configTest = new ConfigTest();
    
    @Test
    public void testPullSource() {
        try {
            // Note: this test fails with the Sun XMLStreamReader because the initial state
            // of the parser is 3 (XML Processing Instruction) rather than 7 (start document)
            // as required by the new StaxSource() constructor.
            String strStylSheetPath = configTest.getDataDir() + "books.xsl";
            String omElemPath = configTest.getDataDir() + "books.xml";
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

    @Test
    public void testSaxonStreamOnEventSource() throws Exception {
        testSaxonOnEventSource(true);
    }

    @Test
    public void testDocOnEventSource() throws Exception {
        testSaxonOnEventSource(false);
    }

    private void testSaxonOnEventSource(boolean streaming) throws Exception {
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
                    public void deliver(Receiver receiver, ParseOptions options) throws XPathException {
                        AttributeMap attributes = EmptyAttributeMap.getInstance();
                        NamespaceMap namespaces = NamespaceMap.emptyMap();
                        receiver.open();
                        receiver.startDocument(ReceiverOption.NONE);
                        receiver.startElement(new NoNamespaceName("a"), Untyped.getInstance(), attributes, namespaces, LOCATION, ReceiverOption.NONE);
                        receiver.startElement(new NoNamespaceName("b"), Untyped.getInstance(), attributes, namespaces, LOCATION, ReceiverOption.NONE);
                        receiver.startElement(new NoNamespaceName("c"), Untyped.getInstance(), attributes, namespaces, LOCATION, ReceiverOption.NONE);
                        receiver.characters(BMPString.of("X"), LOCATION, ReceiverOption.NONE);
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
        assertEquals(1, count);
        xqueryEvaluator.close();
    }

//	private static final int LOCATION = 0; // for 9.6

    private static final Location LOCATION = new Location() {
        @Override
        public int getColumnNumber() {
            return 0;
        }

        @Override
        public int getLineNumber() {
            return 0;
        }

        @Override
        public String getPublicId() {
            return null;
        }

        @Override
        public String getSystemId() {
            return null;
        }

        @Override
        public Location saveLocation() {
            return this;
        }
    };


}

