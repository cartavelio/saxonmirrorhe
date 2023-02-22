package nativetests;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.optim.XQueryExpressionEE;
import com.saxonica.ee.stream.Projector;
import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.RootExpression;
import net.sf.saxon.expr.parser.PathMap;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.query.DynamicQueryContext;
import net.sf.saxon.query.QueryResult;
import net.sf.saxon.query.StaticQueryContext;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.serialize.SerializationProperties;
import net.sf.saxon.sxpath.XPathEvaluator;
import net.sf.saxon.sxpath.XPathExpression;

import javax.xml.transform.OutputKeys;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringWriter;
import java.util.Properties;

/**
 *  Tests for queries using document projection
 */
public class ProjectionTests extends TestCase {

    public void testProjection001() {
        try {

            final String fileName = ConfigTest.DATA_DIR + "books.xml";

            Configuration config = new EnterpriseConfiguration();
            StaticQueryContext sqc = config.newStaticQueryContext();
            sqc.setBaseURI(fileName);
            String query2 = "<out>{doc('" + fileName + "')//CATEGORY}</out>";
            XQueryExpression exp = sqc.compileQuery(query2);

            final PathMap map = ((XQueryExpressionEE)exp).getPathMap();
            FilterFactory ff = new FilterFactory() {
                public ProxyReceiver makeFilter(Receiver next) {
                    return new Projector(map.getRootForDocument(fileName), next);
                }
            };
            StreamSource ss = new StreamSource(fileName);

            ParseOptions options = new ParseOptions();
            options.addFilter(ff);
            NodeInfo doc = config.buildDocumentTree(ss, options).getRootNode();

            // Test that the projected doc contains the CATEGORY elements

            Properties props = new Properties();
            props.setProperty(OutputKeys.METHOD, "xml");
            props.setProperty(OutputKeys.INDENT, "yes");
            StringWriter sw = new StringWriter();
            QueryResult.serialize(doc, new StreamResult(sw), props);
            //System.err.println(sw.toString());
            assertTrue("contains CODE", sw.toString().contains("CODE=\"P\""));
            assertTrue("contains CAT", sw.toString().contains("DESC=\"Paperback\""));

            // Test that the projected doc omits the ITEM elements

            exp = sqc.compileQuery("count(//ITEM)");
            DynamicQueryContext dqc = new DynamicQueryContext(config);
            dqc.setContextItem(doc);
            Object result = exp.evaluateSingle(dqc);
            assertTrue("omits ITEM", result instanceof Long && (Long)result == 0);

        } catch(Exception ex) {
            ex.printStackTrace();
            fail(ex.getMessage());
        }
    }

    public void testProjection002() {
        projection("xpath", "//CATEGORY");
        projection("xpath", "count(//ITEM)");
        projection("xpath", "//CATEGORY[@CODE]");
        projection("xpath", "sum(//ITEM[TITLE]/PRICE)");
        projection("xpath", "//ITEM[not(TITLE = AUTHOR)]/PAGES/number()");
        projection("xpath", "/BOOKLIST/BOOKS/ITEM[not(TITLE = AUTHOR)]/DIMENSIONS/string(@UNIT)");
        projection("xquery", "<item>{//ITEM/TITLE}</item>");
    }

    private void projection(String lang, String expr) {
        try {

            String fileName = ConfigTest.DATA_DIR + "books.xml";;

            Configuration config = new Configuration();
            StreamSource source = new StreamSource(new File(fileName));
            Expression exp = null;
            if (lang.equals("xpath")) {
                XPathEvaluator xpath = new XPathEvaluator(config);
                XPathExpression xpexp = xpath.createExpression(expr);
                exp = xpexp.getInternalExpression();
            } else if (lang.equals("xquery")) {
                StaticQueryContext sqc = config.newStaticQueryContext();
                sqc.setBaseURI(new File(fileName).toURI().toString());
                XQueryExpression xqe = sqc.compileQuery(expr);
                exp = xqe.getExpression();
            } else {
                throw new IllegalArgumentException("second argument must be xpath or xquery");
            }
            PathMap initialPath = new PathMap(exp);
            //initialPath.diagnosticDump(System.err);

            PathMap.PathMapRoot contextRoot = null;
            PathMap.PathMapRoot[] roots = initialPath.getPathMapRoots();
            for (PathMap.PathMapRoot root : roots) {
                PathMap.PathMapRoot newRoot = initialPath.reduceToDownwardsAxes(root);
                if (newRoot.getRootExpression() instanceof RootExpression) {
                    if (contextRoot != null) {
                        System.err.println("*** More than one root matches!!!");
                    } else {
                        contextRoot = newRoot;
                    }
                }
            }
            //System.err.println("AFTER REDUCTION:");
            //initialPath.diagnosticDump(System.err);
            if (contextRoot == null) {
                System.err.println("*** No path for context document!!!");
                return;
            }
            StreamResult result = new StreamResult(System.out);
            SerializationProperties props = new SerializationProperties();
            props.setProperty("method", "xml");
            props.setProperty("indent", "yes");
            Receiver serializer = config.getSerializerFactory().getReceiver(result, props);
            Projector projector = new Projector(contextRoot, serializer);
            Sender.send(source, projector, null);
        } catch (Exception err) {
            err.printStackTrace();
            fail();
        }
    }
}


