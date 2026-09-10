package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.dom.DOMNodeWrapper;
import net.sf.saxon.dom.DocumentWrapper;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.query.DynamicQueryContext;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.ErrorListener;
import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.io.StringWriter;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

import static org.junit.jupiter.api.Assertions.*;

/**
 * JUnit tests for the s9api XQueryCompiler with EE
 */
public class TestXQueryCompilerEE {
    @Test
    public void testBug5680() {
        try {
            Configuration c = new EnterpriseConfiguration();

            Processor processor = new Processor(c);
            XQueryCompiler qc = processor.newXQueryCompiler();
            XQueryExecutable exp = qc.compile(
                    "declare function local:f($v as element()?) { " +
                            "   $v/node()" +
                            "};" +
                            "local:f(//inner)");

            XQueryEvaluator ev = exp.load();
            DocumentBuilderFactory docFactory = DocumentBuilderFactory.newInstance();
            docFactory.setNamespaceAware(true);
            javax.xml.parsers.DocumentBuilder docBuilder = docFactory.newDocumentBuilder();
            Document doc = docBuilder.newDocument();
            Element rootElement = doc.createElement("test");
            Element inner = doc.createElement("inner");
            //rootElement.appendChild(inner); //uncomment to pass the test
            doc.appendChild(rootElement);

            DynamicQueryContext dynamicContext = ev.getUnderlyingQueryContext();
            DocumentWrapper docWrapper = new DocumentWrapper(doc, doc.getBaseURI(), dynamicContext.getConfiguration());
            DOMNodeWrapper context = docWrapper.wrap(doc.getDocumentElement());
            dynamicContext.setContextItem(context);

            for (int i = 1; i < 200; i++) //fails on 100 iteration
            {
                System.out.println(i);
                XdmSequenceIterator<XdmItem> result = ev.iterator();
            }
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        } catch (SaxonApiUncheckedException e) {
            e.printStackTrace();
            fail();
        } catch (ParserConfigurationException e) {
            e.printStackTrace();
            fail();
        }
    }
}
