package internaltests;

import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.om.CopyOptions;
import net.sf.saxon.om.Durability;
import net.sf.saxon.s9api.*;
import net.sf.saxon.s9api.streams.Steps;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyBuilder;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.util.HashMap;
import java.util.Map;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

public class MemoFunctionTests {

    /**
     * General-purpose framework for testing arity-1 memo function. We do this by having a function that actually
     * produces different results if invoked twice with the same arguments (it calls saxon:time-stamp) so we can
     * distinguish whether the function value has been retrieved from cache or re-calculated.
     * @return an XsltExecutable
     */

    private XsltExecutable prepareArity1() {
        try {
            String xsl = "<x:transform xmlns:x='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' version='3.0'>" +
                    "<x:function name='f:memo' cache='yes'><x:param name='p'/>"
                    + "<x:sequence select='substring(string(sum(count($p) to 1000000)), 1, 1)||Q{http://saxon.sf.net/}timestamp()'/>"
                    + "</x:function>" +
                    "<x:param name='p'/><x:param name='q'/>" +
                    "<x:template name='x:initial-template'><x:sequence select='deep-equal(f:memo($p), f:memo($q))'/></x:template>" +
                    "</x:transform>";
            Processor proc = new Processor(true);
            return proc.newXsltCompiler().compile(new StreamSource(new StringReader(xsl)));
        } catch (SaxonApiException e) {
            fail();
            throw new RuntimeException(e);
        }
    }

    /**
     * General-purpose framework for testing arity-2 memo function
     * @return an XsltExecutable
     */

    private XsltExecutable prepareArity2() {
        try {
            String xsl = "<x:transform xmlns:x='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' version='3.0'>" +
                    "<x:function name='f:memo' cache='yes'><x:param name='x'/><x:param name='y'/>"
                    + "<x:sequence select='substring(string(sum(count($x) to 1000000)), 1, 1)||count($y)||Q{http://saxon.sf.net/}timestamp()'/>"
                    + "</x:function>" +
                    "<x:param name='p'/><x:param name='q'/>" +
                    "<x:param name='r'/><x:param name='s'/>" +
                    "<x:template name='x:initial-template'><x:sequence select='deep-equal(f:memo($p, $q), f:memo($r, $s))'/></x:template>" +
                    "</x:transform>";
            Processor proc = new Processor(true);
            return proc.newXsltCompiler().compile(new StreamSource(new StringReader(xsl)));
        } catch (SaxonApiException e) {
            fail();
            throw new RuntimeException(e);
        }
    }

    private void testArity1(XsltExecutable exec, XdmValue p, XdmValue q, boolean expected) {
        // Test whether two argument values are considered "the same value"; if they are,
        // the cached value will be returned for the second call. To determine whether the second
        // call gets the answer from cache, we call saxon:timestamp(), which (hopefully) produces different
        // answers each time it is called.
        try {
            Xslt30Transformer trans = exec.load30();
            Map<QName, XdmValue> params = new HashMap<>();
            params.put(new QName("p"), p);
            params.put(new QName("q"), q);
            trans.setStylesheetParameters(params);
            XdmValue result = trans.callTemplate(null);
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals(expected, ((XdmAtomicValue) result).getBooleanValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    private void testArity2(XsltExecutable exec, XdmValue p, XdmValue q, XdmValue r, XdmValue s, boolean expected) {
        // Test whether two argument values are considered "the same value"; if they are,
        // the cached value will be returned for the second call. To determine whether the second
        // call gets the answer from cache, we call saxon:timestamp(), which (hopefully) produces different
        // answers each time it is called.
        try {
            Xslt30Transformer trans = exec.load30();
            Map<QName, XdmValue> params = new HashMap<>();
            params.put(new QName("p"), p);
            params.put(new QName("q"), q);
            params.put(new QName("r"), r);
            params.put(new QName("s"), s);
            trans.setStylesheetParameters(params);
            XdmValue result = trans.callTemplate(null);
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals(expected, ((XdmAtomicValue) result).getBooleanValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }


    @Test
    public void testStrings() {
        try {
            XsltExecutable exec = prepareArity1();
            testArity1(exec, new XdmAtomicValue("fred"), new XdmAtomicValue("fred"), true);
            testArity1(exec, new XdmAtomicValue(""), new XdmAtomicValue(""), true);
            testArity1(exec, new XdmAtomicValue("fred"), new XdmAtomicValue("bill"), false);
            testArity1(exec, new XdmAtomicValue("fred", ItemType.STRING), new XdmAtomicValue("fred", ItemType.UNTYPED_ATOMIC), false);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testNumbers() {
        try {
            XsltExecutable exec = prepareArity1();
            testArity1(exec, new XdmAtomicValue(3), new XdmAtomicValue(3), true);
            testArity1(exec, new XdmAtomicValue(Double.NaN), new XdmAtomicValue(Double.NaN), true);
            testArity1(exec, new XdmAtomicValue("0e0", ItemType.DOUBLE), new XdmAtomicValue("-0e0", ItemType.DOUBLE), false);
            testArity1(exec, new XdmAtomicValue(3), new XdmAtomicValue(3.0e0), false);
            testArity1(exec, new XdmAtomicValue(3), new XdmAtomicValue("3", ItemType.DECIMAL), false);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testQNames() {
        // prefix is significant
        XsltExecutable exec = prepareArity1();
        testArity1(exec, new XdmAtomicValue(new QName("a", "b", "c")), new XdmAtomicValue(new QName("a", "b", "c")), true);
        testArity1(exec, new XdmAtomicValue(new QName("a", "b", "c")), new XdmAtomicValue(new QName("x", "b", "c")), false);
    }

    @Test
    public void testDateTimes() {
        try {
            XsltExecutable exec = prepareArity1();
            testArity1(exec,
                       new XdmAtomicValue("2022-05-29T12:00:00Z", ItemType.DATE_TIME),
                       new XdmAtomicValue("2022-05-29T12:00:00+00:00", ItemType.DATE_TIME), true);
            testArity1(exec,
                       new XdmAtomicValue("2022-05-29T12:00:00Z", ItemType.DATE_TIME),
                       new XdmAtomicValue("2022-05-29T12:01:00+00:00", ItemType.DATE_TIME), false);
            testArity1(exec,
                       new XdmAtomicValue("2022-05-29T24:00:00Z", ItemType.DATE_TIME),
                       new XdmAtomicValue("2022-05-30T00:00:00Z", ItemType.DATE_TIME), true);
            testArity1(exec,
                       new XdmAtomicValue("2022-05-29T12:00:00Z", ItemType.DATE_TIME),
                       new XdmAtomicValue("2022-05-29T13:00:00+01:00", ItemType.DATE_TIME), false);
            testArity1(exec,
                       new XdmAtomicValue("2022-05-29T12:00:00Z", ItemType.DATE_TIME),
                       new XdmAtomicValue("2022-05-29T13:00:00", ItemType.DATE_TIME), false);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testLastingNodes() {
        try {
            XsltExecutable exec = prepareArity1();
            Processor proc = exec.getProcessor();
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode n0 = builder.build(new StreamSource(new StringReader("<doc/>")));
            XdmNode e0 = n0.select(Steps.child("doc")).asNode();
            XdmNode n1 = builder.build(new StreamSource(new StringReader("<doc/>")));
            XdmNode e1 = n1.select(Steps.child("doc")).asNode();
            testArity1(exec, n0, n0, true);
            testArity1(exec, n0, n1, false);
            testArity1(exec, n0, e0, false);
            testArity1(exec, e0, e1, false);
            testArity1(exec, e1, e1, true);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testTemporaryNodes() {
        try {
            XsltExecutable exec = prepareArity1();
            Processor proc = exec.getProcessor();
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode n0 = builder.build(new StreamSource(new StringReader("<doc/>")));
            XdmNode e0 = n0.select(Steps.child("doc")).asNode();
            XdmNode n1 = builder.build(new StreamSource(new StringReader("<doc/>")));
            XdmNode e1 = n1.select(Steps.child("doc")).asNode();

            TinyBuilder b = new TinyBuilder(proc.getUnderlyingConfiguration().makePipelineConfiguration());
            b.setDurability(Durability.TEMPORARY);

            b.open();
            n0.getUnderlyingNode().copy(b, CopyOptions.ALL_NAMESPACES, Loc.NONE);
            b.close();
            XdmNode n0c = new XdmNode(b.getCurrentRoot());

            b.reset();
            b.open();
            e0.getUnderlyingNode().copy(b, CopyOptions.ALL_NAMESPACES, Loc.NONE);
            b.close();
            XdmNode e0c = new XdmNode(b.getCurrentRoot());

            b.reset();
            b.open();
            n1.getUnderlyingNode().copy(b, CopyOptions.ALL_NAMESPACES, Loc.NONE);
            b.close();
            XdmNode n1c = new XdmNode(b.getCurrentRoot());

            b.reset();
            b.open();
            e1.getUnderlyingNode().copy(b, CopyOptions.ALL_NAMESPACES, Loc.NONE);
            b.close();
            XdmNode e1c = new XdmNode(b.getCurrentRoot());

            testArity1(exec, n0c, n0c, true);
            testArity1(exec, n0c, n1c, false);
            testArity1(exec, n0c, e0c, false);
            testArity1(exec, e0c, e1c, false);
            testArity1(exec, e1c, e1c, true);
        } catch (SaxonApiException | XPathException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testMixed() {
        try {
            XsltExecutable exec = prepareArity1();
            Processor proc = exec.getProcessor();
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode n0 = builder.build(new StreamSource(new StringReader("<doc/>")));
            XdmNode n1 = builder.build(new StreamSource(new StringReader("<doc/>")));
            testArity1(exec, n0.append(n1), n0.append(n1), true);
            testArity1(exec, n0.append(n1), n1.append(n0), false);
            testArity1(exec, XdmEmptySequence.getInstance(), XdmEmptySequence.getInstance(), true);
            testArity1(exec, XdmEmptySequence.getInstance(), n0, false);
            testArity1(exec, new XdmAtomicValue("a"), new XdmAtomicValue("a").append(new XdmAtomicValue("b")), false);
            testArity1(exec, new XdmAtomicValue("a").append(new XdmAtomicValue("b")), new XdmAtomicValue("a").append(new XdmAtomicValue("b")), true);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testArity2Atomic() {
        XsltExecutable exec = prepareArity2();
        XdmAtomicValue a = new XdmAtomicValue("aaa");
        XdmAtomicValue b = new XdmAtomicValue("bbb");
        testArity2(exec, a, b, a, b, true);
        testArity2(exec, a, b, b, a, false);
        testArity2(exec, a, b, a, XdmEmptySequence.getInstance(), false);
        testArity2(exec, a, XdmEmptySequence.getInstance(), a, XdmEmptySequence.getInstance(), true);
    }

    @Test
    public void testArity2Nodes() {
        try {
            XsltExecutable exec = prepareArity2();
            Processor proc = exec.getProcessor();
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode n0 = builder.build(new StreamSource(new StringReader("<doc/>")));
            XdmNode e0 = n0.select(Steps.child("doc")).asNode();
            XdmNode n1 = builder.build(new StreamSource(new StringReader("<doc/>")));
            XdmNode e1 = n1.select(Steps.child("doc")).asNode();
            testArity2(exec, n0, n1, n0, n1, true);
            testArity2(exec, n0, n1, n0, n0, false);
            testArity2(exec, n0, e0, n0, e0, true);
            testArity2(exec, n0, e0, n0, e1, false);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

}


