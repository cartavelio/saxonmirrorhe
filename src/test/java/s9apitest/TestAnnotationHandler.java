package s9apitest;

import net.sf.saxon.lib.FunctionAnnotationHandler;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.query.Annotation;
import net.sf.saxon.query.AnnotationList;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Affinity;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * test that annotations and annotation assertions can be handled using a FunctionAnnotationHandler
 */
public class TestAnnotationHandler {

    private static class ColourHandler implements FunctionAnnotationHandler {

        @Override
        public NamespaceUri getAssertionNamespace () {
            return "http://s.com";
        }

        /**
         * Test whether a given set of annotations in this namespace is valid.
         *
         * @param annotations the annotation list (filtered by namespace: all the annotations
         *                    will have names in the namespace for this annotation handler)
         * @param construct   the construct on which this list of annotations appears. One of "DF" (declare function),
         *                    "DV" (declare variable), "IF" (inline function), "FT" (function test)
         * @throws XPathException if the annotation is invalid, or if it is inconsistent with the annotations
         *                        in the existing list.
         */
        @Override
        public void check(AnnotationList annotations, String construct) throws XPathException {
            //
        }

        @Override
        public boolean satisfiesAssertion(Annotation assertion, AnnotationList annotationList) {
            if (assertion.getAnnotationQName().getLocalPart().equals("coloured")) {
                return annotationList.includes("colour");
            } else {
                return false;
            }
        }

        @Override
        public Affinity relationship(AnnotationList firstList, AnnotationList secondList) {
            return Affinity.OVERLAPS;
        }
    }

    @Test
    public void testSimple() {
        try {
            String q = "" +
                    "declare namespace s = 'http://s.com';" +
                    "declare %s:colour('blue') function local:f(){'blue'};" +
                    "local:f#0 instance of %s:coloured function(*)";

            Processor p = new Processor(true);

            p.getUnderlyingConfiguration().registerFunctionAnnotationHandler(new ColourHandler());
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable e = c.compile(q);
            XQueryEvaluator t = e.load();
            XdmValue result = t.evaluateSingle();
            assertTrue(((XdmAtomicValue)result).getBooleanValue(), "returned true");
        } catch (SaxonApiException e1) {
            e1.printStackTrace();
            fail();
        }
    }

    @Test
    public void testNegative() {
        try {
            String q = "" +
                    "declare namespace s = 'http://s.com';" +
                    "declare %s:shape('circular') function local:f(){'blue'};" +
                    "local:f#0 instance of %s:coloured function(*)";

            Processor p = new Processor(true);

            p.getUnderlyingConfiguration().registerFunctionAnnotationHandler(new ColourHandler());
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable e = c.compile(q);
            XQueryEvaluator t = e.load();
            XdmValue result = t.evaluateSingle();
            assertFalse(((XdmAtomicValue) result).getBooleanValue(), "returned false");
        } catch (SaxonApiException e1) {
            e1.printStackTrace();
            fail();
        }
    }

    @Test
    public void testInference() {
        try {
            String q = "" +
                    "declare namespace s = 'http://s.com';" +
                    "declare function local:g($f as %s:coloured function(xs:integer) as xs:string) {'Paris'};" +
                    "declare %s:colour('blue') function local:f($v as xs:integer) as xs:string{'Paris'[$v]};" +
                    "local:g(local:f#1)";

            Processor p = new Processor(true);

            p.getUnderlyingConfiguration().registerFunctionAnnotationHandler(new ColourHandler());
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable e = c.compile(q);
            XQueryEvaluator t = e.load();
            XdmValue result = t.evaluateSingle();
            assertEquals("Paris", ((XdmAtomicValue) result).getStringValue(), "returned Paris");
        } catch (SaxonApiException e1) {
            e1.printStackTrace();
            fail();
        }
    }

    @Test
    public void testTypeError() {
        try {
            String q = "" +
                    "declare namespace s = 'http://s.com';" +
                    "declare function local:g($f as %s:coloured function(xs:integer) as xs:string) {$f(1)};" +
                    "declare %s:shape('circular') function local:f($v as xs:integer) as xs:string{'Paris'[$v]};" +
                    "local:g(local:f#1)";

            Processor p = new Processor(true);

            p.getUnderlyingConfiguration().registerFunctionAnnotationHandler(new ColourHandler());
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable e = c.compile(q);
            XQueryEvaluator t = e.load();
            XdmValue result = t.evaluateSingle();
            fail("Unexpected success");
        } catch (SaxonApiException e1) {
            assertTrue(e1.getErrorCode().getLocalName().equals("XPTY0004"), "errorCode");
        }
    }

}

