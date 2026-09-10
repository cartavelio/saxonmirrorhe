package jaxptest;


import net.sf.saxon.xpath.XPathFactoryImpl;
import com.saxonica.xqj.SaxonXQConnection;
import com.saxonica.xqj.SaxonXQDataSource;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Node;

import javax.xml.xpath.XPath;
import javax.xml.xpath.XPathConstants;
import javax.xml.xquery.XQConnection;
import javax.xml.xquery.XQDataSource;
import javax.xml.xquery.XQPreparedExpression;
import javax.xml.xquery.XQSequence;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

public class JaxpPlusXqjTest {

	/**
	 * A simple implementation of interface javax.xml.namespace.NamespaceContext.
	 * The associations of prefix and namespace URI are declared explicitly, using
	 * method <code>put</code>. Note that derived classes might offer additional
	 * possibilities for declaring associations, e.g. evaluating a given
	 * element information item.
	 *
	 * @author hjr
	 *
	 */
	private static class NamespaceMap implements javax.xml.namespace.NamespaceContext {

		Map<String, String> namespaces;

		public NamespaceMap() {
			super();
			namespaces = new HashMap<String, String>();
		}

		public void clear() {
			namespaces.clear();
		}

		public void put(String prefix, String namespaceURI) {
			namespaces.put(prefix, namespaceURI);
		}

		@Override
        public String getNamespaceURI(String prefix) {
			return namespaces.get(prefix);
		}

		@Override
        public String getPrefix(String namespaceURI) {
			return null;
		}

		@Override
        public Iterator getPrefixes(String arg0) {
			return null;
		}
	}




	/**
	 * Testcase: XPath applied to XQJ result node, using namespace bindings.
	 */
    @Test
	public void testNamespaceResolver() {
		String queryText = "<hero xmlns='http://whoiswho.com'><name>Michael</name></hero>";
		try {
			XQDataSource ds = new SaxonXQDataSource();
			XQConnection con = ds.getConnection();
			XQPreparedExpression ex = con.prepareExpression(queryText);
			XQSequence seq = ex.executeQuery();
			seq.next();
			Node n = seq.getNode();

			String path;
			XPath xp;
			String name;
			String uri;
			NamespaceMap nsmap = new NamespaceMap();
			nsmap.put("s", "http://whoiswho.comXXX");

			// Test #1: path without namespace
			//          expected: find nothing;
			//          found: <name>
			path = "descendant-or-self::name";
            XPathFactoryImpl factory = new XPathFactoryImpl();

            // sadly, we need some Saxon-specific code here
            factory.setConfiguration(((SaxonXQConnection)con).getConfiguration());
            xp = factory.newXPath();

			name = (String) xp.evaluate(path, n, XPathConstants.STRING);
            assertEquals("", name, "testNSR1");
//            System.out.println("test#1:\npath=" + path + "\nname=" + name);
//			System.out.println("============================================\n");

			// Test #2: path with wrong namespace
			//          expected: find nothing;
			//          found: <name>
			path = "descendant-or-self::s:name";
			xp = factory.newXPath();

			nsmap = new NamespaceMap();
			nsmap.put("s", "http://COMPLETENONSENSE");
			uri = nsmap.getNamespaceURI("s");
			xp.setNamespaceContext(nsmap);

			name = (String) xp.evaluate(path, n, XPathConstants.STRING);
            assertEquals("", name, "testNSR2");
//            System.out.println("test#2:\npath=" + path + "\nprefix s: " + uri + "\nname=" + name);
//			System.out.println("============================================\n");

			// Test #3: path with correct namespace
			//          expected: find <name>;
			//          found: <name>
			path = "descendant-or-self::s:name";
			xp = factory.newXPath();

			nsmap.clear();
			nsmap.put("s", "http://whoiswho.com");
			uri = nsmap.getNamespaceURI("s");
			xp.setNamespaceContext(nsmap);

			name = (String) xp.evaluate(path, n, XPathConstants.STRING);
//			System.out.println("test#3:\npath=" + path + "\nprefix s: " + uri + "\nname=" + name);
//			System.out.println("============================================\n");
            assertEquals("Michael", name, "testNSR3");

            // Test #4: path with unknown prefix
			//          expected: exception;
			//          found: exception;
			path = "descendant-or-self::s:name";
			xp = factory.newXPath();

			nsmap.clear();
			nsmap.put("sss", "http://whoiswho.com");
			uri = nsmap.getNamespaceURI("s");
			xp.setNamespaceContext(nsmap);
			try {
				name = "";
				name = (String) xp.evaluate("descendant-or-self::s:name", n,
						XPathConstants.STRING);
                fail("testNSR4");
            } catch (Exception e) {
				// System.out.println("Exception thrown, msg=" + e.getMessage());
                // OK
            }

//			System.out.println("test#4:\npath=" + path + "\nprefix s: " + uri + "\nname=" + name);
//			System.out.println("============================================");
		} catch (Exception e) {
			e.printStackTrace();
		}
	}


}



