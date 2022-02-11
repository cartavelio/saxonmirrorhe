package jaxptest;

import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.trans.XPathException;
import org.xml.sax.*;

import javax.xml.parsers.SAXParserFactory;
import javax.xml.transform.sax.SAXSource;
import java.io.StringReader;


/**
 *
 */
public class SAXTest extends TestCase {

    /**
     * Test using a SAXSource with a user-supplied ErrorHandler. In 9.1 Saxon replaces this with
     * its own ErrorHandler
     */

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

//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//