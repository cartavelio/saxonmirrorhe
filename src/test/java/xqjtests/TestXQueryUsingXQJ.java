package xqjtests;

import com.saxonica.xqj.SaxonXQDataSource;
import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.om.IgnorableSpaceStrippingRule;
import org.xml.sax.InputSource;

import javax.xml.namespace.QName;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import javax.xml.xquery.*;
import java.io.*;
import java.util.Properties;

/**
 * Some XQJ tests not covered by the XQJ compatibility tests
 */
public class TestXQueryUsingXQJ extends TestCase {

    public void testBoundarySpace() {
        // This test case demonstrated a problem with the default setting of boundary-space policy under XQJ
        try {
            String contentString = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                    "<X>\n" +
                    "<a1 att=\"123\">stuff</a1>\n" +
                    "<a1 att=\"234\">stuff2</a1>\n" +
                    "</X>";
            String scriptString = "declare variable $docContent as document-node() external; " +
                    "let $doc := $docContent " +
                    "for $item in $doc/X/a1/@att " +
                    "let $val := fn:data($item) where $item > 123\n" +
                    "   return\n" +
                    "    <resultMessage>\n" +
                    "        {$item} {fn:name($item)} == {$val}\n" +
                    "    </resultMessage>";

            // Read content into String and create Reader from that String
//            InputStream contentStream =
//                    StringContentTest.class.getResourceAsStream(content);
//            while ((len = contentStream.read(buf)) > -1) {
//                contentString += new String(buf, 0, len);
//            }
            Reader contentReader = new StringReader(contentString);

            // Read script into String
//            InputStream scriptStream =
//                    StringContentTest.class.getResourceAsStream(script);
//            while ((len = scriptStream.read(buf)) > -1) {
//                scriptString += new String(buf, 0, len);
//            }
            System.err.println("------ script -------\n" + scriptString);

            // Execute query on content
            Configuration configuration = new Configuration();
            XQDataSource xqDataSource = new
                    SaxonXQDataSource(configuration);
            XQConnection xqConn = xqDataSource.getConnection();
            //xqConn.getStaticContext().setBoundarySpacePolicy(XQConstants.BOUNDARY_SPACE_STRIP);
            XQPreparedExpression xqExpr =
                    xqConn.prepareExpression(scriptString);

            xqExpr.bindItem(new QName("docContent"), xqConn.createItemFromDocument(
                    new SAXSource(new
                            InputSource(contentReader)), null));
            XQResultSequence xqResult = xqExpr.executeQuery();
            while (xqResult.next())

            {
                System.err.println(xqResult.getItemAsString(null));
            }
        } catch (XQException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    public static void testBindDocument() {
        String dir = ConfigTest.DATA_DIR + "kessel/jan2009/";
        try {
			// setup common to all XQuery nodes.
			Configuration config = new Configuration();
			config.getSystemURIResolver().setRecognizeQueryParameters(true);
			config.setValidation(false);
			config.getParseOptions().setSpaceStrippingRule(IgnorableSpaceStrippingRule.getInstance());
			XQDataSource dataSource   = new SaxonXQDataSource(config);
			XQConnection xqconnection = dataSource.getConnection();
			XQStaticContext context = xqconnection.getStaticContext();
			context.setBindingMode(XQConstants.BINDING_MODE_DEFERRED);
			xqconnection.setStaticContext(context);

			System.out.println("Running node XQueryOne");
			Reader xqueryReader = new FileReader(new File(dir+"OneXMLParm.xquery"));
			XQPreparedExpression XQueryOne_xqExpr = xqconnection.prepareExpression(xqueryReader);
			XQueryOne_xqExpr.bindDocument(new QName("XMLparm"),
                    new StreamSource(new File(dir+"DefaultInput.fo.xml").toURI().toString()), null);
			XQSequence XQueryOne_result = XQueryOne_xqExpr.executeQuery();
			Properties XQueryOne_2_p = new Properties();
			XQueryOne_2_p.setProperty("omit-xml-declaration", "yes");

			// Create a stream to write the output from node XQueryOne_2 to a file.
			FileOutputStream output_strm = new FileOutputStream(new File(dir+"out.xml"));

			System.out.println("Running node XQueryOne_2");
			xqueryReader = new FileReader(new File(dir+"OneXMLParm.xquery"));
			XQPreparedExpression XQueryOne_2_xqExpr = xqconnection.prepareExpression(xqueryReader);
			XQueryOne_2_xqExpr.bindDocument(new QName("XMLparm"), XQueryOne_result.getSequenceAsStream(), null);

			XQueryOne_2_xqExpr.executeQuery().writeSequence(output_strm, XQueryOne_2_p);
			output_strm.close();

		} catch (XQException err) {
            err.printStackTrace();
            fail(err.getMessage());
        } catch (IOException err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    /**
     * This test demonstrated a problem with PARENT_POINTER pseudo-nodes in pull mode
     */

    public void testParentPointerPull() {

        // Declarations for Saxon XQJ interface.
		Reader            xqueryReader       = null;
		XQConnection      xqconnection       = null;

		XQPreparedExpression xq1= null;
		XQPreparedExpression xq2= null;

		try {

			XQDataSource  dataSource   = new SaxonXQDataSource();
			xqconnection = dataSource.getConnection();


			System.out.println("xq1");
			xq1 = xqconnection.prepareExpression(".");
			StreamSource source = new StreamSource(new FileInputStream(ConfigTest.DATA_DIR + "before.xml"));
			xq1.bindDocument(XQConstants.CONTEXT_ITEM, source, null);
			XQSequence xq1_result = xq1.executeQuery();

			System.out.println("xq2");
			xq2 = xqconnection.prepareExpression(".");
			xq2.bindDocument(XQConstants.CONTEXT_ITEM, xq1_result.getSequenceAsStream(), null);
			xq2.executeQuery().writeSequence(System.out, new Properties());

        } catch (XQException err) {
            fail(err.getMessage());

        } catch (IOException err) {
            fail(err.getMessage());

        }
	}

    /**
     * This test demonstrated a problem with setting serialization properties to null in writeSequence()
     */

    public void testNullOutputProperties() {
        try {
            XQDataSource xqjd = new SaxonXQDataSource();
            XQConnection xqjc = xqjd.getConnection();
            String xquery="<a>test</a>";
            XQPreparedExpression xqje = xqjc.prepareExpression(xquery);
            XQSequence xqjs = xqje.executeQuery();
            xqjs.writeSequence(System.out, null);
        } catch (XQException e) {
            fail(e.getMessage());
        }


    }

    // Bug 3062 from Petr K
    public void testAbstractValidation() {
        try {
            XQDataSource ds = new SaxonXQDataSource();
            XQConnection conn = ds.getConnection();
            XQExpression exp = conn.createExpression();
            exp.getStaticContext().setBaseURI(new File(ConfigTest.DATA_DIR + "AbstractType/src/").toURI().toString());

            //OK - document validation, abstract types
            exp.bindDocument(XQConstants.CONTEXT_ITEM, new FileReader(ConfigTest.DATA_DIR + "AbstractType/src/objDoc.xml"), null, conn.createDocumentType());
            exp.executeQuery(new FileReader(ConfigTest.DATA_DIR + "AbstractType/src/validateDoc.xq"));

            //OK - element validation, no abstract types
            exp.bindDocument(XQConstants.CONTEXT_ITEM, new FileReader(ConfigTest.DATA_DIR + "AbstractType/src/concreteObj.xml"), null, conn.createDocumentType());
            exp.executeQuery(new FileReader(ConfigTest.DATA_DIR + "AbstractType/src/validateElem.xq"));

            //BAD - element validation, abstract types
            try {
                exp.bindDocument(XQConstants.CONTEXT_ITEM, new FileReader(ConfigTest.DATA_DIR + "AbstractType/src/obj.xml"), null, conn.createDocumentType());
                exp.executeQuery(new FileReader(ConfigTest.DATA_DIR + "AbstractType/src/validateElem.xq"));
            } catch (Exception e) {
                e.printStackTrace();
                fail(e.getMessage());
            }

            //BAD - inside declaration, abstract types
            try {
                exp.executeQuery(new FileReader(ConfigTest.DATA_DIR + "AbstractType/src/validateVar.xq"));
            } catch (Exception e) {
                e.printStackTrace();
                fail(e.getMessage());
            }
        } catch (XQException e) {
            e.printStackTrace();
            fail(e.getMessage());
        } catch (FileNotFoundException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    // Some additional tests of getItemAsStream()

    public void testGetItemAsStream() throws XQException {
        XQDataSource ds = new SaxonXQDataSource();
        XQConnection xqc = ds.getConnection();
        XQExpression xqe;
        XQSequence xqs;
        XMLStreamReader xmlReader = null;

        xqe = xqc.createExpression();
        xqs = xqe.executeQuery("<e><!--comment--></e>");
        xqs.next();
        try {
            xmlReader = xqs.getItemAsStream();
        } catch (XQException e) {
            junit.framework.Assert.fail("A-XQIA-9.1: getItemAsStream failed with message: " + e.getMessage());
        }
        try {
            junit.framework.Assert.assertNotNull("A-XQIA-9.1: getItemAsStream returned a null XMLStreamReader", xmlReader);
            junit.framework.Assert.assertEquals("A-XQIA-9.1: unexpected first event returned by XMLStreamReader", XMLStreamReader.START_DOCUMENT, xmlReader.getEventType());
            junit.framework.Assert.assertEquals("A-XQIA-9.1: unexpected second event returned by XMLStreamReader", XMLStreamReader.START_ELEMENT, xmlReader.next());
            junit.framework.Assert.assertEquals("A-XQIA-9.1: unexpected third event returned by XMLStreamReader", XMLStreamReader.COMMENT, xmlReader.next());
            junit.framework.Assert.assertEquals("A-XQIA-9.1: unexpected fourth event returned by XMLStreamReader", XMLStreamReader.END_ELEMENT, xmlReader.next());
            junit.framework.Assert.assertEquals("A-XQIA-9.1: unexpected fifth event returned by XMLStreamReader", XMLStreamReader.END_DOCUMENT, xmlReader.next());
        } catch (XMLStreamException e) {
            junit.framework.Assert.fail("A-XQIA-9.1: XMLStreamReader.next() failed with message: " + e.getMessage());
        }

        xqs = xqe.executeQuery("2+2");
        xqs.next();
        try {
            xmlReader = xqs.getItemAsStream();
        } catch (XQException e) {
            junit.framework.Assert.fail("A-XQIA-9.1: getItemAsStream failed with message: " + e.getMessage());
        }
        try {
            junit.framework.Assert.assertNotNull("A-XQIA-9.1: getItemAsStream returned a null XMLStreamReader", xmlReader);
            junit.framework.Assert.assertEquals("A-XQIA-9.1: unexpected first event returned by XMLStreamReader", XMLStreamReader.START_DOCUMENT, xmlReader.getEventType());
            junit.framework.Assert.assertEquals("A-XQIA-9.1: unexpected second event returned by XMLStreamReader", XMLStreamReader.CHARACTERS, xmlReader.next());
            junit.framework.Assert.assertEquals("A-XQIA-9.1: unexpected third event returned by XMLStreamReader", XMLStreamReader.END_DOCUMENT, xmlReader.next());
        } catch (XMLStreamException e) {
            junit.framework.Assert.fail("A-XQIA-9.1: XMLStreamReader.next() failed with message: " + e.getMessage());
        }
        xqe.close();
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



