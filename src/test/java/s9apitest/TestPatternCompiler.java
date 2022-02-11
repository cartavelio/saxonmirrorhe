package s9apitest;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests for the s9api XPathCompiler used to compile and evaluate XSLT patterns
 */
public class TestPatternCompiler {

    @Test
    public void testSimple() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable e = c.compilePattern("document-node(element(foo))");
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo/>")));
            XdmNode bar = b.build(new StreamSource(new StringReader("<bar/>")));
            XPathSelector s = e.load();
            s.setContextItem(foo);
            assertTrue(s.effectiveBooleanValue());
            s.setContextItem(bar);
            assertFalse(s.effectiveBooleanValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testNamespaces() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.declareNamespace("f", "http://foo.com/");
            XPathExecutable e = c.compilePattern("document-node(element(f:foo))");
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo xmlns='http://foo.com/'/>")));
            XdmNode bar = b.build(new StreamSource(new StringReader("<foo xmlns='http://bar.com/'/>")));
            XPathSelector s = e.load();
            s.setContextItem(foo);
            assertTrue(s.effectiveBooleanValue());
            s.setContextItem(bar);
            assertFalse(s.effectiveBooleanValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testExternalVariables() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            QName var = new QName("var");
            c.declareVariable(var);
            XPathExecutable e = c.compilePattern("document-node(element(foo))[. = $var]");
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo>3</foo>")));
            XdmNode bar = b.build(new StreamSource(new StringReader("<foo>4</foo>")));
            XPathSelector s = e.load();
            s.setContextItem(foo);
            s.setVariable(var, new XdmAtomicValue(3));
            assertTrue(s.effectiveBooleanValue());
            s.setContextItem(bar);
            assertFalse(s.effectiveBooleanValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testInternalVariables() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable e = c.compilePattern("document-node(element(foo))[some $x in //* satisfies $x = 3]");
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo><bar>2</bar><bar>3</bar></foo>")));
            XdmNode bar = b.build(new StreamSource(new StringReader("<foo><bar>2</bar><bar>4</bar></foo>")));
            XPathSelector s = e.load();
            s.setContextItem(foo);
            assertTrue(s.effectiveBooleanValue());
            s.setContextItem(bar);
            assertFalse(s.effectiveBooleanValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testStaticSyntaxError() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable e = c.compilePattern("foo,bar");
            fail("failed to detect static error");
        } catch (SaxonApiException e) {
            // OK;
        }
    }

    @Test
    public void testStaticSemanticError() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable e = c.compilePattern("f:foo");
            fail("failed to detect static error");
        } catch (SaxonApiException e) {
            // OK;
        }
    }

    @Test
    public void testDynamicError() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            QName var = new QName("var");
            c.declareVariable(var);
            XPathExecutable e = c.compilePattern("document-node(element(foo))[. idiv $var = 12]");
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo>3</foo>")));
            XPathSelector s = e.load();
            s.setContextItem(foo);
            s.setVariable(var, new XdmAtomicValue(3));
            assertFalse(s.effectiveBooleanValue());
        } catch (SaxonApiException e) {
            fail("failed to suppress dynamic error");
        }
    }


}
