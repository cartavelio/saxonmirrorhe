package xqjtests;

import com.saxonica.xqj.SaxonXQDataSource;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import javax.xml.xquery.*;

import javax.xml.datatype.DatatypeFactory;
import javax.xml.datatype.XMLGregorianCalendar;
import javax.xml.namespace.QName;
import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMResult;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xpath.XPathExpressionException;
import javax.xml.xpath.XPathFactory;
import java.io.StringWriter;
import java.math.BigInteger;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


public class RennauBugs {

    /**
     * Testcase: XPath applied to XQJ result node produces runtime
     * error.
     */
    @Test
    public void testXPath() {
        String queryText = "<place><name>abc</name></place>";
        try {
            XQDataSource ds = new SaxonXQDataSource();
            XQConnection con = ds.getConnection();
            XQPreparedExpression ex = con.prepareExpression(queryText);
            XQSequence seq = ex.executeQuery();
            seq.next();
            Node n = seq.getNode();

            XPath xp = XPathFactory.newInstance().newXPath();
            // NOW IT HAPPENS!
            Element name = (Element) xp.evaluate("descendant-or-self::name", n,
                    XPathConstants.NODE);

            System.out.println("name=" + name.getTextContent());
            return;

        } catch (XQException e) {
            e.printStackTrace();
            System.out.println("XQ ERROR!");
        } catch (XPathExpressionException e) {
            e.printStackTrace();
            System.out.println("XPATH ERROR!");
        } catch (Exception e) {
            e.printStackTrace();
            System.out.println("RUNTIME ERROR!");
        }
    }

    /**
     * Testcase: omit-xml-declaration="no"
     */
    @Test
    public void testOmitXmlDeclaration() {
        String queryText = "<place><name>abc</name></place>";
        try {
            XQDataSource ds = new SaxonXQDataSource();
            XQConnection con = ds.getConnection();
            XQPreparedExpression ex = con.prepareExpression(queryText);
            XQSequence seq = ex.executeQuery();
            Properties props = new Properties();
            props.setProperty("omit-xml-declaration", "no");
            String s = seq.getSequenceAsString(props);
            assertTrue(s.startsWith("<?xml "), "no XML declaration");
            props.setProperty("omit-xml-declaration", "yes");
            seq = ex.executeQuery();
            s = seq.getSequenceAsString(props);
            assertFalse(s.startsWith("<?xml "), "has XML declaration");
            try {
                seq = ex.executeQuery();
                props.setProperty("omit-xml-declaration", "maybe");
                s = seq.getSequenceAsString(props);
                fail("Invalid serialization property not detected");
            } catch (XQException e) {
                assertTrue(e.getMessage().contains("omit-xml-declaration"));
                // System.err.println(e);
                // OK
            }

        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }


    @Test
    public void testWriteToResult1() {
        writeToResult(1);
    }

    @Test
    public void testWriteToResult2() {
        writeToResult(2);
    }

    /**
     * Testcase: writing XQJ to result when result is obtained fron XQJ => error
     */
    private void writeToResult(int subMode) {
        try {
            XQDataSource ds = new SaxonXQDataSource(); // XQJ implementation
            XQConnection con = ds.getConnection(); // get a connection

            String query = "<hydroReport><placeSearch/></hydroReport>";

            XQExpression ex0 = con.createExpression();
            XQSequence seq0 = ex0.executeQuery(query);
            seq0.next();

            // resultXQ = result obtained from XQJ
            Element reportXQ = (Element) seq0.getNode();
            Element placeSearchXQ = (Element) reportXQ.getElementsByTagName(
                    "placeSearch").item(0);

            // resultDB = result obtained from DocumentBuilder
            Element reportDB = null;
            Element placeSearchDB = null;
            try {

                DocumentBuilder docBuilder = DocumentBuilderFactory
                        .newInstance().newDocumentBuilder();
                Document doc = docBuilder.newDocument();
                reportDB = doc.createElement("hydroReport");
                placeSearchDB = doc.createElement("placeSearch");
                reportDB.appendChild(placeSearchDB);
            } catch (Exception e) {
                e.printStackTrace();
            }
            System.out.println("Document obtained from XQ:");
            System.out.println(node2String(placeSearchXQ, false, null));

            System.out.println("Document obtained from DocumentBuilder:");
            System.out.println(node2String(placeSearchDB, false, null));

            // *** query #2
            String query2 = "<place><name>abc</name></place>";
            XQPreparedExpression ex = con.prepareExpression(query2);
            XQSequence seq = ex.executeQuery();
            seq.next();

            if (subMode == 1) {
                seq.writeItemToResult(new DOMResult(placeSearchDB));
            } else if (subMode == 2) {
                seq.writeItemToResult(new DOMResult(placeSearchXQ));
                fail("Expected failure");
            }

            // error handling
        } catch (XQException e) {
            if (subMode == 1) {
                e.printStackTrace();
                fail(e.getMessage());
            }
        }
    }


    /**
     * Implements the method defined in <code>XQuery</code>.
     */
    public String node2String(Node node, boolean omitXmlDeclaration,
            String encoding) {
        if (node == null)
            return "";

        String result = null;
        try {
            // create transformer
            Transformer trans = TransformerFactory.newInstance()
                    .newTransformer();

            // configure transformer
            Properties props = new Properties();
            props.put("method", "xml");
            props.put("indent", "yes");
            if (omitXmlDeclaration)
                props.put("omit-xml-declaration", "yes");
            if (encoding != null && encoding.length() > 0)
                props.put("encoding", encoding);
            trans.setOutputProperties(props);

            // transform
            StringWriter sWriter = new StringWriter();
            trans.transform(new DOMSource(node), new StreamResult(sWriter));
            result = sWriter.toString();

            // error exit
        } catch (Exception e) {
            e.printStackTrace();
            throw new RuntimeException("failed to serialize DOM", e);
        }
        return result;
    }

    /**
     * Bug with binding GregorianCalendar object
     */

    @Test
    public void testGregorianCalendar() {
        try {
            XQDataSource ds = new SaxonXQDataSource(); // XQJ implementation
            XQConnection con = ds.getConnection(); // get a connection
            XQExpression ex = con.createExpression();
            DatatypeFactory dtfact = DatatypeFactory.newInstance();
            XMLGregorianCalendar date = dtfact.newXMLGregorianCalendar("2010-04-01");
            assertEquals("2010-04-01", date.toString(), "dateOK");
            System.out.println("dateObject=" + date);
            ex.bindObject(new QName("date"), date, con.createAtomicType(XQItemType.XQBASETYPE_DATE));
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }

    }

    @Test
    public void testGetInt() {
        try {
           StringBuffer buffer = new StringBuffer();
           XQDataSource ds = new SaxonXQDataSource();
           XQConnection co = ds.getConnection();
           String query = "for $i in 120 to 130 return xs:int($i)";
           XQPreparedExpression ex = co.prepareExpression(query);
           XQSequence result = ex.executeQuery();
           while (result.next()) {
               buffer.append(result.getInt());
               buffer.append(',');
           }
           assertEquals(buffer.toString(), "120,121,122,123,124,125,126,127,128,129,130,", "eq");
        } catch (Exception e) {
              e.printStackTrace();
        }
    }

    @Test
    public void testGetShort() {
        try {
           StringBuffer buffer = new StringBuffer();
           XQDataSource ds = new SaxonXQDataSource();
           XQConnection co = ds.getConnection();
           String query = "for $i in 120 to 130 return xs:int($i)";
           XQPreparedExpression ex = co.prepareExpression(query);
           XQSequence result = ex.executeQuery();
           while (result.next()) {
               buffer.append(result.getShort());
               buffer.append(',');
           }
           assertEquals(buffer.toString(), "120,121,122,123,124,125,126,127,128,129,130,", "eq");
        } catch (Exception e) {
              e.printStackTrace();
        }
    }

    /**
     * Tests binding values to a sub type of xs:decimal, binding method
     * <code>bindInt</code>. The XQuery-declared type is <code>xs:short</code>.
     * For the Java-specified type, all sub types of xs:decimal are tried which
     * are compatible with a value of <code>1</code>. (That is: all subtypes of
     * xs:decimal except xs:nonPositiveInteger and xs:negativeInteger.)
     * <p/>
     * The ONLY attempt that should be successful is when using XQBASETYPE_BYTE
     * or XQBASETYPE_SHORT. However, presently (Saxon 9.2) all attempts are
     * successful.
     *
     * @author Hans-Jurgen
     *
     */
    @Test
    public void testBindInt() {
        try {
            XQDataSource ds = new SaxonXQDataSource();
            XQConnection co = ds.getConnection();
            String query = "declare variable $i as xs:short external; $i";
            XQPreparedExpression ex = co.prepareExpression(query);

            QName nameI = new QName("i");
            List<XQItemType> types = new ArrayList<XQItemType>();
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_DECIMAL));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_INTEGER));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_LONG));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_INT));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_SHORT));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_BYTE));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_NONNEGATIVE_INTEGER));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_UNSIGNED_LONG));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_UNSIGNED_INT));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_UNSIGNED_SHORT));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_UNSIGNED_BYTE));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_POSITIVE_INTEGER));

            // trying the Java-specified types one by one => all successful!

            StringBuffer sb = new StringBuffer();
            for (XQItemType type : types) {
                try {
                    ex.bindInt(nameI, 1, type);
                    XQSequence result = ex.executeQuery();
                    result.writeSequence(System.out, null);
                    sb.append("Y");
                } catch (XQException e) {
                    sb.append("N");
                }
            }
            try {
                ex.bindInt(nameI, 1, null);
                XQSequence result = ex.executeQuery();
                result.writeSequence(System.out, null);
                sb.append("Y");
            } catch (XQException e) {
                sb.append("N");
            }
            assertEquals("NNNNYYNNNNNNN", sb.toString(), "results");
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    /**
     * Tests binding values to a sub type of xs:decimal, binding method
     * <code>bindAtomicValue</code>. The XQuery-declared type is
     * <code>xs:byte</code>. For the Java-specified type, all numeric types are
     * tried which are compatible with a value of <code>1</code>. (That is:
     * xs:float, xs:double and all sub types of xs:decimal except
     * xs:nonPositiveInteger and xs:negativeInteger.)
     * <p/>
     * The ONLY attempt that should be successful is when using XQBASETYPE_BYTE
     * or XQBASETYPE_UNSIGNED_BYTE. However, presently all attempts are
     * successful.
     *
     * @author Hans-Jurgen
     *
     */
    @Test
    public void testBindAtomicValue() {
        try {
            XQDataSource ds = new SaxonXQDataSource();
            XQConnection co = ds.getConnection();
            String query = "declare variable $i as xs:byte external; $i";
            XQPreparedExpression ex = co.prepareExpression(query);

            QName nameI = new QName("i");
            List<XQItemType> types = new ArrayList<XQItemType>();
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_DECIMAL));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_INTEGER));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_LONG));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_INT));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_SHORT));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_BYTE));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_NONNEGATIVE_INTEGER));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_UNSIGNED_LONG));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_UNSIGNED_INT));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_UNSIGNED_SHORT));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_UNSIGNED_BYTE));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_POSITIVE_INTEGER));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_FLOAT));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_DOUBLE));

            StringBuffer sb = new StringBuffer();
            for (XQItemType type : types) {
                try {
                    ex.bindAtomicValue(nameI,"1", type);
                    XQSequence result = ex.executeQuery();
                    result.writeSequence(System.out, null);
                    sb.append("Y");
                } catch (XQException e) {
                    sb.append("N");
                }
            }
            try {
                ex.bindAtomicValue(nameI, "1", null);
                XQSequence result = ex.executeQuery();
                result.writeSequence(System.out, null);
                sb.append("Y");
            } catch (XQException e) {
                sb.append("N");
            }
            assertEquals("NNNNNYNNNNNNNNN", sb.toString(), "results");
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * Tests binding values to a sub type of xs:decimal, binding method
     * <code>bindObject</code>.
     * <p/>
     * The XQuery-declared type is <code>xs:byte</code> . For the Java-specified
     * type, all sub types of xs:decimal are tried which are compatible with a
     * value of <code>1</code>. (That is: all sub types of xs:decimal except
     * xs:nonPositiveInteger and xs:negativeInteger.)
     * <p/>
     * The ONLY attempt that should be successful is when using XQBASETYPE_BYTE
     * or XQBASETYPE_UNSIGNED_BYTE. However, presently all attempts are
     * successful.
     *
     * @author Hans-Jurgen
     *
     */
    @Test
    public void testBindObject() {
        try {
            XQDataSource ds = new SaxonXQDataSource();
            XQConnection co = ds.getConnection();
            String query = "declare variable $i as xs:byte external; $i";
            XQPreparedExpression ex = co.prepareExpression(query);

            QName nameI = new QName("i");
            List<XQItemType> types = new ArrayList<XQItemType>();
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_DECIMAL));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_INTEGER));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_LONG));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_INT));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_SHORT));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_BYTE));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_NONNEGATIVE_INTEGER));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_UNSIGNED_LONG));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_UNSIGNED_INT));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_UNSIGNED_SHORT));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_UNSIGNED_BYTE));
            types.add(co.createAtomicType(XQItemType.XQBASETYPE_POSITIVE_INTEGER));

            StringBuffer sb = new StringBuffer();
            for (XQItemType type : types) {
                try {
                    ex.bindObject(nameI, new BigInteger("1"), type);
                    XQSequence result = ex.executeQuery();
                    result.writeSequence(System.out, null);
                    sb.append("Y");
                } catch (XQException e) {
                    sb.append("N");
                }
            }
            try {
                ex.bindObject(nameI, new BigInteger("1"), null);
                XQSequence result = ex.executeQuery();
                result.writeSequence(System.out, null);
                sb.append("Y");
            } catch (XQException e) {
                sb.append("N");
            }
            assertEquals("NNNNNYNNNNNNN", sb.toString(), "results");
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * Tests binding values to a sub type of xs:double, binding method
     * <code>bindInt</code>. The XQuery-declared type is <code>xs:short</code>.
     * For the Java-specified type, all sub types of xs:decimal are tried which
     * are compatible with a value of <code>1</code>. (That is: all subtypes of
     * xs:decimal except xs:nonPositiveInteger and xs:negativeInteger.)
     * <p/>
     * The ONLY attempt that should be successful is when using XQBASETYPE_DOUBLE.
     * However, presently (Saxon 9.2) all attempts are successful, because Saxon applies
     * the function conversion rules to supplied parameters.
     *
     */
    @Test
    public void testExpectDouble() {
        try {
            XQDataSource ds = new SaxonXQDataSource();
            XQConnection co = ds.getConnection();
            String query = "declare variable $i as xs:double external; $i";
            XQPreparedExpression ex = co.prepareExpression(query);

            QName nameI = new QName("i");

            StringBuffer sb = new StringBuffer();

            try {
                ex.bindInt(nameI, 1, co.createAtomicType(XQItemType.XQBASETYPE_INT));
                XQSequence result = ex.executeQuery();
                result.writeSequence(System.out, null);
                sb.append("Y");
            } catch (XQException e) {
                sb.append("N");
            }
            try {
                ex.bindFloat(nameI, 1, co.createAtomicType(XQItemType.XQBASETYPE_FLOAT));
                XQSequence result = ex.executeQuery();
                result.writeSequence(System.out, null);
                sb.append("Y");
            } catch (XQException e) {
                sb.append("N");
            }
            try {
                ex.bindDouble(nameI, 1, co.createAtomicType(XQItemType.XQBASETYPE_DOUBLE));
                XQSequence result = ex.executeQuery();
                result.writeSequence(System.out, null);
                sb.append("Y");
            } catch (XQException e) {
                sb.append("N");
            }
            try {
                ex.bindDouble(nameI, 1.0e0, null);
                XQSequence result = ex.executeQuery();
                result.writeSequence(System.out, null);
                sb.append("Y");
            } catch (XQException e) {
                sb.append("N");
            }
            assertEquals("NNYY", sb.toString(), "results");
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    @Test
    public void testStaticContext() {
        try {
            XQDataSource ds = new SaxonXQDataSource();
            XQConnection co = ds.getConnection();
            XQStaticContext scon = co.getStaticContext();
            scon.declareNamespace("c", "http://www.bits-ac.com/ns/serviceframework");
            scon.setScrollability(XQConstants.SCROLLTYPE_SCROLLABLE);
            co.setStaticContext(scon);
            String queryText = "<e/>[not(c:serviceContainer/c:container/c:variables)]";
            XQPreparedExpression ex = co.prepareExpression(queryText);
            XQSequence seq = ex.executeQuery();
            assertTrue(seq.count() == 1);
        } catch (XQException e) {
            e.printStackTrace();
            fail();
        }



    }


}
