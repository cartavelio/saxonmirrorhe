package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.dom.DOMNodeWrapper;
import net.sf.saxon.dom.DocumentWrapper;
import net.sf.saxon.query.DynamicQueryContext;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Disabled;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;

import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests for the s9api XQueryCompiler with EE
 */
public class TestXQueryCompilerEE {
    // We don't do byte code generation anymore, so the test is irrelevant?
    @Disabled
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
