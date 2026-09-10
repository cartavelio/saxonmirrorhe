package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.StaticContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.FunctionLibraryList;
import net.sf.saxon.junit.Testdata;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.s9api.*;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trace.Traceable;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.Source;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.*;

import static org.junit.jupiter.api.Assertions.*;

/**
 * JUnit tests for the s9api XPathCompiler
 */
public class TestXPathCompilerEE {
    /**
     * Test case from malik@metamoji.com, Sept 2010
     */

    @Test
    public void testUseXQueryFunctions() {

        // NOTE: this test fetches schema documents from the internet.

        if (true) {
            return; // can't fetch docs from internet.
        }

        Processor processor = new Processor(true);
        SchemaManager schemaManager = processor.getSchemaManager();
        EnterpriseConfiguration conf = (EnterpriseConfiguration) processor.getUnderlyingConfiguration();

        conf.displayLicenseMessage();
        conf.setSchemaValidationMode(Validation.STRICT);

        XPathCompiler xpathCompiler;
        try {

            // 1. load schema into EnterpriseConfiguration
            XsdSchema schema = schemaManager.load(new StreamSource(Testdata.file("malix/schema.xsd")));

            XQueryCompiler xqueryCompiler = processor.newXQueryCompiler();
            xqueryCompiler.setSchemaAware(true);
            xqueryCompiler.useSchema(schema);

            xpathCompiler = processor.newXPathCompiler();
            xpathCompiler.setSchemaAware(true);
            xpathCompiler.useSchema(schema);

            String xq =
                    "declare namespace xfi='http://www.xbrl.org/2008/function/instance'; \n" +
                            "declare namespace xbrli='http://www.xbrl.org/2003/instance'; \n" +
                            " import schema 'http://www.xbrl.org/2003/instance';  \n" +
                            "declare function xfi:facts-in-instance( $xbrl as element(xbrli:xbrl) ) as element()* { \n" +
                            "for $e in $xbrl//*[. instance of schema-element(xbrli:item) or . instance of schema-element(xbrli:tuple)] \n" +
                            "return $e \n" +
                            "}; \n" +
                            "10";

            XQueryExecutable xqExec = xqueryCompiler.compile(xq);
            QueryModule qm = xqExec.getUnderlyingCompiledQuery().getMainModule();
            FunctionLibrary fl1 = qm.getFunctionLibrary();

            StaticContext sc1 = xpathCompiler.getUnderlyingStaticContext();
            FunctionLibrary fl2 = sc1.getFunctionLibrary();
            if (fl2 instanceof FunctionLibraryList) {
                ((FunctionLibraryList) fl2).addFunctionLibrary(fl1);
            }

            String xml =
                    "<?xml version='1.0' encoding='utf-8'?>\n" +
                            "<xbrl xmlns='http://www.xbrl.org/2003/instance' xmlns:link='http://www.xbrl.org/2003/linkbase'  xmlns:xlink='http://www.w3.org/1999/xlink'>\n" +
                            "  <link:schemaRef xlink:href='aem-20100331.xsd' xlink:type='simple' />\n" +
                            "</xbrl>";

            InputSource is = new InputSource(new StringReader(xml));
            Source ss = new SAXSource(is);
            ParseOptions options = new ParseOptions()
                    .withExpandAttributeDefaults(true)
                    .withSchemaValidationMode(Validation.STRICT);
            ss = new AugmentedSource(ss, options);
            NodeInfo doc = conf.buildDocumentTree(ss, options).getRootNode();

            xpathCompiler.setAllowUndeclaredVariables(true);

            IndependentContext sc2 = (IndependentContext) xpathCompiler.getUnderlyingStaticContext();
            FunctionLibrary fl3 = sc2.getFunctionLibrary();

            sc2.setNamespaceResolver(new NamespaceResolver() {

                @Override
                public Iterator<String> iteratePrefixes() {
                    return null;
                }

                @Override
                public NamespaceUri getURIForPrefix(String prefix, boolean useDefault) {
                    if ("xfi".equals(prefix)) {
                        return NamespaceUri.of("http://www.xbrl.org/2008/function/instance");
                    } else if ("xbrli".equals(prefix)) {
                        return NamespaceUri.of("http://www.xbrl.org/2003/instance");
                    }
                    return null;
                }
            });

            XPathExecutable exec = null;

            exec = xpathCompiler.compile("$n1 + xfi:facts-in-instance(/xbrli:xbrl)");

            Set<QName> dependencies = new HashSet<>();

            Iterator<QName> it = exec.iterateExternalVariables();
            while (it.hasNext()) {
                QName qn = it.next();
                dependencies.add(new QName(qn.getNamespace(), qn.getLocalName(), qn.getPrefix()));
            }

            if (dependencies.isEmpty()) {
                fail("Uh oh! NO VARIABLES!");
            }

        } catch (Throwable e) {
            e.printStackTrace();
            fail();
        }
    }

}

