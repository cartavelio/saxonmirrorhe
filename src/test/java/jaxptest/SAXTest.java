package jaxptest;

import net.sf.saxon.Configuration;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Test;
import org.xml.sax.*;

import javax.xml.parsers.SAXParserFactory;
import javax.xml.transform.sax.SAXSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


/**
 *
 */
public class SAXTest {

    /**
     * Test using a SAXSource with a user-supplied ErrorHandler. In 9.1 Saxon replaces this with
     * its own ErrorHandler
     */

    @Test
    public void testErrorHandler() {
        try {
            String in = "<?xml version='1.0'?> \n" +
                    "<Document><e></f></Document>";

            SAXParserFactory factory = SAXParserFactory.newInstance();
            factory.setNamespaceAware(true);
            factory.setValidating(false);
            XMLReader parser = factory.newSAXParser().getXMLReader();
            parser.setErrorHandler(new ErrorHandler() {
                public void warning(SAXParseException exception) throws SAXException {
                    System.err.println("Called warning");
                }

                public void error(SAXParseException exception) throws SAXException {
                    System.err.println("Called error");
                }

                public void fatalError(SAXParseException exception) throws SAXException {
                    System.err.println("Called fatalError");
                }
            });
            SAXSource source = new SAXSource(parser, new InputSource(new StringReader(in)));
            source.setSystemId("http://saxonica.com/");

            Configuration config = new Configuration();
            NodeInfo doc = config.buildDocumentTree(source).getRootNode();
            fail("parsing should have failed");
        }
        catch (Exception e) {
            assertTrue(e instanceof XPathException);
            assertTrue(e.getCause() instanceof SAXException);
            assertTrue(((XPathException)e).hasBeenReported());
        }
    }


}
