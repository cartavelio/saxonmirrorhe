package nativetests;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Builder;
import net.sf.saxon.event.ReceiverOption;
import net.sf.saxon.om.*;
import net.sf.saxon.query.DynamicQueryContext;
import net.sf.saxon.query.QueryResult;
import net.sf.saxon.query.StaticQueryContext;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.BuiltInAtomicType;
import org.junit.jupiter.api.Test;

import javax.xml.transform.Source;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;


public class TestXQueryUpdate {
    private static final String xml =
            "<?xml version='1.0' encoding='UTF-8'?><record><datafield ind1=' '></datafield></record>";

    // Bug report against 9.1: compiling a query against one Configuration and
    // running using another appears to give no error
    @Test
    public void testUpdateConfig() {

        try {
            final EnterpriseConfiguration config = new EnterpriseConfiguration();
            config.setResourceResolver(request -> {
                     Source s = new StreamSource(new StringReader(xml));
                     s.setSystemId(request.uri);
                     return s;
            });
            config.setTreeModel(Builder.LINKED_TREE);
            final StaticQueryContext sqc = config.newStaticQueryContext();
            sqc.setUpdatingEnabled(true);
            sqc.setBaseURI("http://www.w3.org/");
            final XQueryExpression exp = sqc.compileQuery(
                    "insert node <n/> after " +
                    "doc('file:///D:/My%20Documents/MyProjects/curve_cleaning/trunk/scripts/xml/XXXXs.xml')/record/datafield[1]");

            final DynamicQueryContext dynamicContext = new DynamicQueryContext(config);

            java.util.Set<MutableNodeInfo> s = exp.runUpdate(dynamicContext);

            assertEquals(1, s.size(), "one updated node");
        } catch (XPathException e) {
            fail(e.getMessage());
        }

    }

    // bug report against 9.1 - adding a namespace node to a MutableNodeInfo

    @Test
    public void testAddNamespace() {
        try {
            Configuration config = new EnterpriseConfiguration();
            config.setTreeModel(Builder.LINKED_TREE);
            String source = "<foo/>";
            NodeInfo doc = config.buildDocumentTree(new StreamSource(new StringReader(source))).getRootNode();
            NamespaceUri fooBar = NamespaceUri.of("http://foo.bar/");
            NodeName name = new FingerprintedQName("a", fooBar, "x");
            NamespaceBinding nsCode = new NamespaceBinding("a", fooBar);
            MutableNodeInfo elem = (MutableNodeInfo)doc.iterateAxis(AxisInfo.CHILD).next();
            elem.addAttribute(name, BuiltInAtomicType.UNTYPED_ATOMIC, "xxxx", ReceiverOption.NONE, true);
            elem.addNamespace(nsCode, true);
            String s = QueryResult.serialize(elem).trim();
            assertEquals("<foo xmlns:a=\"http://foo.bar/\" a:x=\"xxxx\"/>", s, "result");
            //System.err.println(s);

        } catch (XPathException e) {
            fail(e.getMessage());
        }
    }
}
