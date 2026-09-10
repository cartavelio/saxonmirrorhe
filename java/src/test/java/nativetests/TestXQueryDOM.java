package nativetests;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.dom.DOMNodeWrapper;
import net.sf.saxon.dom.DOMObjectModel;
import net.sf.saxon.dom.DocumentWrapper;
import net.sf.saxon.dom.NodeOverNodeInfo;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.TreeInfo;
import net.sf.saxon.query.DynamicQueryContext;
import net.sf.saxon.query.StaticQueryContext;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyTree;
import org.junit.jupiter.api.Test;
import org.w3c.dom.*;
import org.xml.sax.InputSource;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.Transformer;
import javax.xml.transform.dom.DOMResult;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.List;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


public class TestXQueryDOM {

    private static final String query1 = ".//form";
    private static final String query2 = ".//input";

    private static final String xml =
            "<?xml version='1.0' encoding='UTF-8'?><form><input/><input/></form>";

    // Bug report against 9.1: native XQuery interface with DOMSource
    @Test
    public void testXQueryDOM() {
        try {
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder db = factory.newDocumentBuilder();
            InputSource is = new InputSource(new StringReader(xml));
            org.w3c.dom.Document pageDom = db.parse(is);

            Configuration config = new Configuration();
            StaticQueryContext staticContext = config.newStaticQueryContext();
            XQueryExpression findFormQuery = staticContext.compileQuery(query1);
            XQueryExpression findInputQuery = staticContext.compileQuery(query2);
            DOMNodeWrapper docWrapper = (DOMNodeWrapper) DOMObjectModel.getInstance().unravel(new DOMSource(pageDom), config);
            DocumentWrapper info = docWrapper.getTreeInfo();
            DynamicQueryContext dynamicContext = new DynamicQueryContext(config);
            dynamicContext.setContextItem(info.getRootNode());
            List<Object> formNodes = findFormQuery.evaluate(dynamicContext);
            for (Object formNode : formNodes) {
                dynamicContext.clearParameters();
                dynamicContext.setContextItem(info.wrap((Node) formNode));
                List<Object> inputNodes = findInputQuery.evaluate(dynamicContext);
                for (Object inputNode : inputNodes) {
                    System.err.println("found: " + ((Node) inputNode).getLocalName());
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    //

    @Test
    public void testNamespaceUndeclaration10() {
        runNamespaceUndeclarationTest(Configuration.XML10);
    }

    @Test
    public void testNamespaceUndeclaration11() {
        runNamespaceUndeclarationTest(Configuration.XML11);
    }



    private void runNamespaceUndeclarationTest(int version) {

        try {
            EnterpriseConfiguration config = new EnterpriseConfiguration();
            config.setXMLVersion(version);

            String query =
                    "declare namespace person=\"http://www.example.org/person\"; \n" +
                    "declare namespace people=\"http://www.example.org/people\"; \n" +
                    "\n" +
                    "<people:people>\n" +
                    "\t<person:person>\n" +
                    "\t\t<person:surname>surname</person:surname>\n" +
                    "\t\t<person:firstname>firstname</person:firstname>\n" +
                    "\t\t<person:birthday>birthday</person:birthday>\n" +
                    "\t</person:person>\n" +
                    "</people:people>";

            StaticQueryContext ctx = config.newStaticQueryContext();

            XQueryExpression ex = ctx.compileQuery(query);
            DynamicQueryContext dynamicContext = new DynamicQueryContext(config);
            Object result = ex.evaluateSingle(dynamicContext);

            final TreeInfo treeInfo = config.buildDocumentTree((NodeInfo) result);
            ((TinyTree)treeInfo).diagnosticDump();
            Node doc = NodeOverNodeInfo.wrap(treeInfo.getRootNode());

            boolean foundUndeclaration = printChildren(((Document) doc).getDocumentElement(), 0, version);
            assertEquals(version != Configuration.XML10, foundUndeclaration, "foundUndeclaration");


        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    private static boolean printChildren(Element e, int p, int version) {

        NamedNodeMap nodes = e.getAttributes();

        System.out.println("--------------" + e.getNodeName());
        boolean foundUndeclaration = false;
        if (nodes != null) {
            int len = nodes.getLength();
            for (int i = 0; i < len; i++) {
                Node node = nodes.item(i);
                String value = node.getNodeValue();
                String name = node.getNodeName();
                if (e.getNodeName().equals("person:person") && name.equals("xmlns:people") && value.isEmpty()) {
                    foundUndeclaration = true;
                }

                System.out.println(name + ":" + value);
            }
        }

        NodeList nl = e.getChildNodes();
        for (int i = 0; i < nl.getLength(); i++) {
            if (nl.item(i) instanceof Element) {
                foundUndeclaration |= printChildren((Element) nl.item(i), p++, version);
            }
        }
        return foundUndeclaration;
    }

    @Test
    public void testBug5951() {
        try {
            DOMResult result = new DOMResult();
            Configuration config = new Configuration();
            XQueryExpression expression = config.newStaticQueryContext().compileQuery(
                    "declare namespace b = 'test';<b:book><title/></b:book>");
            expression.run(new DynamicQueryContext(config), result, new Properties());

            Document doc = (Document) result.getNode();
            StringWriter sw = new StringWriter();
            Transformer transformer = new TransformerFactoryImpl().newTransformer();
            transformer.transform(new DOMSource(doc), new StreamResult(sw));
            assertTrue(sw.toString().contains("<b:book xmlns:b=\"test\"><title/></b:book>"));
            System.out.println(sw);
        } catch (Exception e) {
            fail(e);
        }
    }

}
