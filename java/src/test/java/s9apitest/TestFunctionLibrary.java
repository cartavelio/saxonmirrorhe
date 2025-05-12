package s9apitest;

import com.saxonica.config.ProfessionalConfiguration;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.query.StaticQueryContext;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import java.io.File;
import java.io.FileInputStream;
import java.io.IOException;

import static org.junit.jupiter.api.Assertions.fail;

public class TestFunctionLibrary {
    @Test
    public void testXsltFunctionLibrary() {
        Processor processor = new Processor(true);
        if (!"EE".equals(processor.getUnderlyingConfiguration().getEditionCode())) {
            System.err.println("The testXsltFunctionLibrary test requires EE");
            return;
        }

        try {
            File library = new File("src/test/testdata/functionLibrary.xsl");
            SAXSource stylesheet = new SAXSource(new InputSource(library.toURI().toString()));
            XsltCompiler compiler = processor.newXsltCompiler();
            XsltPackage exec = compiler.compilePackage(stylesheet);


            XPathCompiler xcompiler = processor.newXPathCompiler();
            xcompiler.declareNamespace("test", "http://example.com/ns/test");
            xcompiler.declareNamespace("test1", "http://example.com/ns/test1");
            xcompiler.addXsltFunctionLibrary(exec);
            XPathExecutable xexec = xcompiler.compile("test:function1()");
            XPathSelector selector = xexec.load();
            XdmValue value = selector.evaluate();
            Assertions.assertEquals("test:function1", value.getUnderlyingValue().getStringValue());

            xexec = xcompiler.compile("test:function2('called')");
            selector = xexec.load();
            value = selector.evaluate();
            Assertions.assertEquals("test:function1-called", value.getUnderlyingValue().getStringValue());

            try {
                xexec = xcompiler.compile("test1:function1()");
                selector = xexec.load();
                value = selector.evaluate();
                fail("unexpected success");
            } catch (SaxonApiException e) {
                Assertions.assertTrue(e.getMessage().contains("Cannot find a 0-argument function"));
            }

            try {
                xexec = xcompiler.compile("test:private-function()");
                selector = xexec.load();
                value = selector.evaluate();
                fail("unexpected success");
            } catch (SaxonApiException ex) {
                Assertions.assertTrue(ex.getMessage().contains("Cannot find a 0-argument function"));
            }
        } catch (SaxonApiException | XPathException ex) {
            ex.printStackTrace();
            fail();
        }
    }


    @Test
    public void testXQueryFunctionLibrary() {
        Processor processor = new Processor(true);
        if (!"EE".equals(processor.getUnderlyingConfiguration().getEditionCode())) {
            System.err.println("The testXQueryFunctionLibrary test requires EE");
            return;
        }

        try {
            File library = new File("src/test/testdata/functionLibrary.xq");
            FileInputStream stream = new FileInputStream(library);

            XQueryCompiler compiler = processor.newXQueryCompiler();
            StaticQueryContext context = compiler.getUnderlyingStaticContext();
            context.compileLibrary(stream, "utf-8");
            XQueryExpression expr = context.compileQuery("import module namespace f='http://example.com/ns/test';.");
            QueryModule module = expr.getMainModule();
            FunctionLibrary flib = module.getFunctionLibrary();

            ProfessionalConfiguration pc = (ProfessionalConfiguration) processor.getUnderlyingConfiguration();
            pc.setExtensionBinder("https://saxonica.com/unit-test", flib);

            XPathCompiler xcompiler = processor.newXPathCompiler();
            xcompiler.declareNamespace("test", "http://example.com/ns/test");
            XPathExecutable xexec = xcompiler.compile("test:function1()");
            XPathSelector selector = xexec.load();
            XdmValue value = selector.evaluate();
            Assertions.assertEquals("test:function1", value.getUnderlyingValue().getStringValue());

            xexec = xcompiler.compile("test:function2('called')");
            selector = xexec.load();
            value = selector.evaluate();
            Assertions.assertEquals("test:function2-called", value.getUnderlyingValue().getStringValue());

            xexec = xcompiler.compile("test:function3()");
            selector = xexec.load();
            value = selector.evaluate();
            Assertions.assertEquals("test:private-function-called", value.getUnderlyingValue().getStringValue());

            try {
                xexec = xcompiler.compile("test:private-function()");
                selector = xexec.load();
                value = selector.evaluate();
                fail();
            } catch (SaxonApiException ex) {
                Assertions.assertTrue(ex.getMessage().contains("Cannot find a 0-argument function"));
            }
        } catch (SaxonApiException | IOException | XPathException ex) {
            ex.printStackTrace();
            fail();
        }
    }
}
