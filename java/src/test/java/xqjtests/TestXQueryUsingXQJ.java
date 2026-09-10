package xqjtests;

import com.saxonica.xqj.SaxonXQDataSource;
import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.IgnorableSpaceStrippingRule;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.namespace.QName;
import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import javax.xml.xquery.*;
import java.io.*;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Some XQJ tests not covered by the XQJ compatibility tests
 */
public class TestXQueryUsingXQJ {
    private final ConfigTest configTest = new ConfigTest();

    @Test
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

    @Test
    public void testBindSimpleDocument() {
        // Investigating bug #4469
        try {
            // setup common to all XQuery nodes.
            Configuration config = new Configuration();
            XQDataSource dataSource = new SaxonXQDataSource(config);
            XQConnection xqconnection = dataSource.getConnection();
            XQStaticContext context = xqconnection.getStaticContext();
            context.setBindingMode(XQConstants.BINDING_MODE_DEFERRED);
            xqconnection.setStaticContext(context);

            XQPreparedExpression q1 = xqconnection.prepareExpression(".");
            q1.bindDocument(XQConstants.CONTEXT_ITEM,
                                          new StreamSource(new StringReader("<a>x<b>text</b></a>")), null);
            XQSequence q1_result = q1.executeQuery();

            XQPreparedExpression q2= xqconnection.prepareExpression(".");
            q2.bindDocument(XQConstants.CONTEXT_ITEM, q1_result.getSequenceAsStream(), null);
            StringWriter sw = new StringWriter();
            Properties props = new Properties();
            props.setProperty("method", "xml");
            props.setProperty("indent", "no");
            props.setProperty("omit-xml-declaration", "yes");
            q2.executeQuery().writeSequence(sw, props);

            assertEquals("<a>x<b>text</b></a>", sw.toString().trim(), "result");

        } catch (XQException err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }


    @Test
    public void testBindDocument() {
        String dir = configTest.getDataDir() + "kessel/jan2009/";
        try {
			// setup common to all XQuery nodes.
			Configuration config = new Configuration();
			config.setBooleanProperty(Feature.RECOGNIZE_URI_QUERY_PARAMETERS, true);
			config.setValidation(false);
			config.setParseOptions(config.getParseOptions().withSpaceStrippingRule(IgnorableSpaceStrippingRule.getInstance()));
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

    @Test
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
			StreamSource source = new StreamSource(new FileInputStream(configTest.getDataDir() + "before.xml"));
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

    @Test
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
    @Test
    public void testAbstractValidation() {
        try {
            XQDataSource ds = new SaxonXQDataSource();
            XQConnection conn = ds.getConnection();
            XQExpression exp = conn.createExpression();
            exp.getStaticContext().setBaseURI(new File(configTest.getDataDir() + "AbstractType/src/").toURI().toString());

            //OK - document validation, abstract types
            exp.bindDocument(XQConstants.CONTEXT_ITEM, new FileReader(configTest.getDataDir() + "AbstractType/src/objDoc.xml"), null, conn.createDocumentType());
            exp.executeQuery(new FileReader(configTest.getDataDir() + "AbstractType/src/validateDoc.xq"));

            //OK - element validation, no abstract types
            exp.bindDocument(XQConstants.CONTEXT_ITEM, new FileReader(configTest.getDataDir() + "AbstractType/src/concreteObj.xml"), null, conn.createDocumentType());
            exp.executeQuery(new FileReader(configTest.getDataDir() + "AbstractType/src/validateElem.xq"));

            //BAD - element validation, abstract types
            try {
                exp.bindDocument(XQConstants.CONTEXT_ITEM, new FileReader(configTest.getDataDir() + "AbstractType/src/obj.xml"), null, conn.createDocumentType());
                exp.executeQuery(new FileReader(configTest.getDataDir() + "AbstractType/src/validateElem.xq"));
            } catch (Exception e) {
                e.printStackTrace();
                fail(e.getMessage());
            }

            //BAD - inside declaration, abstract types
            try {
                exp.executeQuery(new FileReader(configTest.getDataDir() + "AbstractType/src/validateVar.xq"));
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
            fail("A-XQIA-9.1: getItemAsStream failed with message: " + e.getMessage());
        }
        try {
            assertNotNull(xmlReader, "A-XQIA-9.1: getItemAsStream returned a null XMLStreamReader");
            assertEquals(XMLStreamReader.START_DOCUMENT, xmlReader.getEventType(), "A-XQIA-9.1: unexpected first event returned by XMLStreamReader");
            assertEquals(XMLStreamReader.START_ELEMENT, xmlReader.next(), "A-XQIA-9.1: unexpected second event returned by XMLStreamReader");
            assertEquals(XMLStreamReader.COMMENT, xmlReader.next(), "A-XQIA-9.1: unexpected third event returned by XMLStreamReader");
            assertEquals(XMLStreamReader.END_ELEMENT, xmlReader.next(), "A-XQIA-9.1: unexpected fourth event returned by XMLStreamReader");
            assertEquals(XMLStreamReader.END_DOCUMENT, xmlReader.next(), "A-XQIA-9.1: unexpected fifth event returned by XMLStreamReader");
        } catch (XMLStreamException e) {
            fail("A-XQIA-9.1: XMLStreamReader.next() failed with message: " + e.getMessage());
        }

        xqs = xqe.executeQuery("2+2");
        xqs.next();
        try {
            xmlReader = xqs.getItemAsStream();
        } catch (XQException e) {
            fail("A-XQIA-9.1: getItemAsStream failed with message: " + e.getMessage());
        }
        try {
            assertNotNull(xmlReader, "A-XQIA-9.1: getItemAsStream returned a null XMLStreamReader");
            assertEquals(XMLStreamReader.START_DOCUMENT, xmlReader.getEventType(),"A-XQIA-9.1: unexpected first event returned by XMLStreamReader");
            assertEquals(XMLStreamReader.CHARACTERS, xmlReader.next(), "A-XQIA-9.1: unexpected second event returned by XMLStreamReader");
            assertEquals(XMLStreamReader.END_DOCUMENT, xmlReader.next(), "A-XQIA-9.1: unexpected third event returned by XMLStreamReader");
        } catch (XMLStreamException e) {
            fail("A-XQIA-9.1: XMLStreamReader.next() failed with message: " + e.getMessage());
        }
        xqe.close();
    }

    @Test
    public void testBug4433() {
        SaxonXQDataSource dataSource = new SaxonXQDataSource();
        dataSource.getConfiguration().setConfigurationProperty(Feature.TIMING, true);
        XQConnection connection = dataSource.getConnection();
        try (InputStream query = new FileInputStream(configTest.getBugsDir() + "4433/query.xq");
             InputStream input = new FileInputStream(configTest.getBugsDir() + "4433/input.atom")) {
            XQPreparedExpression expression = connection.prepareExpression(query);
            expression.bindDocument(XQConstants.CONTEXT_ITEM, input, null, null);
            XQResultSequence results = expression.executeQuery();
            results.writeSequence(System.out, null);
            results.close();
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }


}

