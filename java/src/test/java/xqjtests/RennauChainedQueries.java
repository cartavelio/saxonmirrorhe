package xqjtests;

import com.saxonica.xqj.SaxonXQDataSource;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Node;

import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMResult;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.xquery.*;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.fail;


/**
 * Class containing demo testcase.
 *
 * @author Hans-Jurgen
 *
 */
public class RennauChainedQueries {

	/**
	 * Test demonstrating problem with chained queries.
     *
     * This test failed (2010-09-01). It appears to be a problem with the Xalan IdentityTransformer,
     * which is testing to see when it has reached the end of a tree-walk of an element node by
     * comparing the node-identity of the current node with that of the initial node by using the
     * equals() method, rather than the DOM isSameNode() method. There's not much we can do about
     * this at the Java level.
     *
     * The test now works (2010-09-03). It has been made to work by implementing the equals() method
     * on class NodeOverNodeInfo.
	 *
	 */
    @Test
	public void testChainedQueries() {
		try {
			String inputDoc =
				      "<execQueryRequest>" + "\n"
					+ "   <param name=\"address\">" + "\n" +
					"      <address>" + "\n"
					+ "	        <name>Mueller</name>" + "\n"
					+ "	        <city>Berlin</city>" + "\n"
					+ "       </address>" + "\n" + "	   </param>" + "\n"
					+ "</execQueryRequest>" + "\n";

			String queryText = " " +
					  "<params>{" + "\n"
					+ "   for $param in //param return" + "\n"
					+ "      element {node-name($param)} {" + "\n"
					+ "         $param/@*, $param/node()" + "\n"
					+ "      }"	+ "\n"
					+ "}</params>";

			StringReader input = new StringReader(inputDoc);
			StringReader query = new StringReader(queryText);

			// install a connection
			XQDataSource ds = new SaxonXQDataSource();
			XQConnection con = ds.getConnection();

			// execute query #1 and obtain XQItem
			XQPreparedExpression ex = con.prepareExpression(query);
			ex.bindDocument(XQConstants.CONTEXT_ITEM, input, null, null);
			XQSequence res1 = ex.executeQuery();
			res1.next();
			XQItem item1 = res1.getItem();

			// execute query #2 and obtain node
			XQPreparedExpression ex2 = con.prepareExpression(".//param");
			ex2.bindItem(XQConstants.CONTEXT_ITEM, item1);
			XQSequence res2 = ex2.executeQuery();
			res2.next();
			XQItem item2 = res2.getItem();
			Node node2 = item2.getNode();

			// a) attempt identity transformation to StreamResult
			//    -> faulty result (trailing "</params>")
			System.out.println("serialization:\n" + serialize(node2));

			// b) attempt identity transformation to DOMResult
			//    -> EXCEPTION
			Node d = identityTransform(node2);
		} catch (Throwable e) {
			e.printStackTrace();
            fail(e.getMessage());
        }
	}

	static public Node identityTransform(Node node) throws Throwable {
		TransformerFactory transFactory = TransformerFactory.newInstance();
		Transformer trans = transFactory.newTransformer();
		DOMResult domRes = new DOMResult();
		trans.transform(new DOMSource(node), domRes);
		return domRes.getNode();
	}

	static public String serialize(Node node) throws Throwable {
		TransformerFactory transFactory = /* new net.sf.saxon.TransformerFactoryImpl(); // */ TransformerFactory.newInstance();
		Transformer trans = transFactory.newTransformer();

		Properties props = new Properties();
		props.put("method", "xml");
		props.put("indent", "yes");
		props.put("omit-xml-declaration", "yes");
		trans.setOutputProperties(props);

		StringWriter sWriter = new StringWriter();
		trans.transform(new DOMSource(node), new StreamResult(sWriter));
		return sWriter.toString();
	}



}

