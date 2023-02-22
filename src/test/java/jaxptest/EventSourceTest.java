package jaxptest;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
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

import javax.xml.transform.Source;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class EventSourceTest {

    @Test
    public void testSaxonStreamOnEventSource() throws Exception {
        Configuration configuration = new EnterpriseConfiguration();
        configuration.setBooleanProperty(Feature.STREAMING_FALLBACK, true);

        Processor processor = new Processor(configuration);
        XQueryCompiler xqueryCompiler = processor.newXQueryCompiler();
        String query = "saxon:stream(doc('uriresolver:resolve')/(a/b))/c";
        XQueryExecutable xqueryExecutable = xqueryCompiler.compile(query);
        XQueryEvaluator xqueryEvaluator = xqueryExecutable.load();

        xqueryEvaluator.setURIResolver(new StandardURIResolver() {
            @Override
            public Source resolve(String href, String base) throws XPathException {
                return new EventSource() {
                    @Override
                    public void deliver(Receiver receiver, ParseOptions options) throws XPathException {
                        AttributeMap attributes = EmptyAttributeMap.getInstance();
                        NamespaceMap namespaces = NamespaceMap.emptyMap();
                        receiver.startDocument(ReceiverOption.NONE);
                        receiver.startElement(new NoNamespaceName("a"), Untyped.getInstance(), attributes, namespaces, LOCATION, ReceiverOption.NONE);
                        receiver.startElement(new NoNamespaceName("b"), Untyped.getInstance(), attributes, namespaces, LOCATION, ReceiverOption.NONE);
                        receiver.startElement(new NoNamespaceName("c"), Untyped.getInstance(), attributes, namespaces, LOCATION, ReceiverOption.NONE);
                        receiver.characters(BMPString.of("X"), LOCATION, ReceiverOption.NONE);
                        receiver.endElement();
                        receiver.endElement();
                        receiver.endElement();
                        receiver.endDocument();
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
