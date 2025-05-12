package jaxptest;

import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.lib.ErrorReporter;
import net.sf.saxon.lib.StandardErrorListener;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.s9api.*;
import net.sf.saxon.tree.tiny.TinyDocumentImpl;
import net.sf.saxon.tree.tiny.TinyNodeImpl;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.ErrorListener;
import javax.xml.transform.Source;
import javax.xml.transform.Templates;
import javax.xml.transform.TransformerException;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.sax.TransformerHandler;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.FileInputStream;
import java.io.StringReader;
import java.io.StringWriter;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test that line numbers are reported correctly for dynamic errors
 */
public class StylesheetCopyLineNumbersTest {
    private final ConfigTest configTest = new ConfigTest();


    @Test
    public static void testStylesheetCopyInterpreted() throws Exception {
        testStylesheetCopyTree(false);
        testStylesheetCopySax(false);
    }

    public static void testStylesheetCopyTree(boolean compile) throws Exception {
        ConfigTest ctest = new ConfigTest();
        TransformerFactoryImpl factory = new TransformerFactoryImpl();

        // Run the stylesheets with an empty document to show the difference in error line number
        ErrorListener el = new StandardErrorListener() {
            @Override
            public void fatalError(TransformerException exception) {
                //System.err.println("error here: " + exception.getLocator().getLineNumber());
                assertEquals(14, exception.getLocator().getLineNumber(), "line number is ");
                //super.error(exception);
            }
        };

        // Make an in memory tiny tree for caching in memory.
        Source sourceToCache2 = new SAXSource(new InputSource(new FileInputStream(ctest.getDataDir() + "runtime-error.xsl")));
        DocumentBuilder docBuilder = new Processor(false).newDocumentBuilder();
        docBuilder.setLineNumbering(true);
        Source cachedTree = docBuilder.build(sourceToCache2).asSource();

        // Show that tree has line numbers.
        TinyNodeImpl rootElement = (TinyNodeImpl)((TinyDocumentImpl)cachedTree).iterateAxis(AxisInfo.CHILD).next();
        assertEquals("xsl:stylesheet",  rootElement.getDisplayName(), "name ok?");
        assertEquals(5, rootElement.getLineNumber(), "line ok?");
        //System.err.println("Line num: " + rootElement.getLineNumber());


        Templates stylesheetTree = factory.newTemplates(cachedTree);
        TransformerHandler treeStylesheetHandler = factory.newTransformerHandler(stylesheetTree);
        treeStylesheetHandler.getTransformer().setErrorListener(el);
        treeStylesheetHandler.setResult(new StreamResult(new StringWriter()));

        try {
            treeStylesheetHandler.startDocument();
            treeStylesheetHandler.endDocument();
            fail("Runtime error expected");
        }
        catch (Exception e) {
            //e.printStackTrace();
        }
    }

    public static void testStylesheetCopySax(boolean compile) throws Exception {
        ConfigTest ctest = new ConfigTest();
        TransformerFactoryImpl factory = new TransformerFactoryImpl();

        Source sourceToCache = new SAXSource(new InputSource(new FileInputStream(ctest.getDataDir() + "runtime-error.xsl")));

        // Compile stylesheets from different sources.
        Templates stylesheetSax = factory.newTemplates(sourceToCache);

        // Run the stylesheets with an empty document to show the difference in error line number
        ErrorListener el = new StandardErrorListener() {
            @Override
            public void fatalError(TransformerException exception) {
                //System.err.println("error here: " + exception.getLocator().getLineNumber());
                assertEquals(14, exception.getLocator().getLineNumber(), "line number is ");
                //super.error(exception);
            }
        };

        TransformerHandler saxStylesheetHandler = factory.newTransformerHandler(stylesheetSax);
        saxStylesheetHandler.getTransformer().setErrorListener(el);
        saxStylesheetHandler.setResult(new StreamResult(new StringWriter()));

        try {
            saxStylesheetHandler.startDocument();
            saxStylesheetHandler.endDocument();
            fail("Runtime error expected");
        }
        catch (Exception e) {
            //e.printStackTrace();
        }

    }

    /**
     * This test is designed to exercise the global location map held in the configuration to hold location information
     * when there are multiple separately compiled packages; it deliberately triggers an error that's detected on the output
     * pipeline rather than during expression interpretation, when the originating expression isn't directly available.
     */

    @Test
    public static void testMultiPackageLocationInfo() {
        try {
            Processor p = new Processor(true);
            XsltCompiler c = p.newXsltCompiler();
            String packageA = "" +
                    "<xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform' name='http://A/' version='3.0' package-version='1.0'>\n" +
                    "  <xsl:use-package name='http://B/' package-version='1.0'/>\n" +
                    "  <xsl:template name='main' visibility='public'>\n" +
                    "    <out><in/><xsl:call-template name='t'/></out>\n" +
                    "  </xsl:template>\n" +
                    "</xsl:package>";
            String packageB = "" +
                    "<xsl:package xmlns:xsl='http://www.w3.org/1999/XSL/Transform' name='http://B/' version='3.0' package-version='1.0'>\n" +
                    "  <xsl:template name='t' visibility='public'>\n" +
                    "    <xsl:attribute name='a' select='2'/>\n" +
                    "  </xsl:template>\n" +
                    "</xsl:package>";
            XsltPackage b = c.compilePackage(new StreamSource(new StringReader(packageB), "http://B/"));
            c.importPackage(b);
            XsltExecutable e = c.compile(new StreamSource(new StringReader(packageA), "http://A/"));
            Xslt30Transformer t = e.load30();

            ErrorReporter el = err -> {
                System.err.println("error here, as expected, on line: " + err.getLocation().getLineNumber());
                assertEquals(3, err.getLocation().getLineNumber(), "line number is ");
                assertEquals("module is ", "http://B/", err.getModuleUri());
            };
            t.setErrorReporter(el);
            XdmValue result = t.callTemplate(new QName("main"));
            fail("Unexpected success");
        } catch (SaxonApiException err) {
            assertEquals("error code", "XTDE0410", err.getErrorCode().getLocalName());
            assertEquals(3, err.getLineNumber(), "line number");
            assertEquals("uri", "http://B/", err.getSystemId());
        }
    }


}



