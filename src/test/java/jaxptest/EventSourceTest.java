package jaxptest;

import com.saxonica.config.EnterpriseConfiguration;
import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.EventSource;
import net.sf.saxon.event.Receiver;
import net.sf.saxon.expr.parser.Location;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.StandardURIResolver;
import net.sf.saxon.om.FingerprintedQName;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Untyped;

import javax.xml.transform.Source;

public class EventSourceTest extends TestCase {

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
                    public void send(Receiver receiver) throws XPathException {
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
