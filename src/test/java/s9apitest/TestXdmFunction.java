package s9apitest;

import net.sf.saxon.lib.ExtensionFunctionDefinition;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.NamespaceConstant;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.math.BigDecimal;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * Test XdmFunctionItem class, representing an XPath function item
 */
public class TestXdmFunction {

    @Test
    public void testCallSystemFunction() {
        try {
            Processor proc = new Processor(false);
            XdmFunctionItem fi = XdmFunctionItem.getSystemFunction(proc, new QName(NamespaceConstant.FN, "parse-json"), 1);
            assertNotNull(fi);
            XdmArray result = (XdmArray) fi.call(proc, new XdmAtomicValue("[1,2,3]"));
            assertEquals(3, result.arrayLength());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testCallSystemFunctionMath() {
        try {
            Processor proc = new Processor(false);
            XdmFunctionItem fi = XdmFunctionItem.getSystemFunction(proc, new QName(NamespaceConstant.MATH, "sqrt"), 1);
            assertNotNull(fi);
            XdmAtomicValue result = (XdmAtomicValue) fi.call(proc, new XdmAtomicValue(4));
            assertEquals(2, result.getLongValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testCallSystemFunctionArray() {
        try {
            Processor proc = new Processor(false);
            XdmFunctionItem fi = XdmFunctionItem.getSystemFunction(proc, new QName(NamespaceConstant.ARRAY_FUNCTIONS, "sort"), 1);
            assertNotNull(fi);
            XdmArray result = (XdmArray) fi.call(
                    proc, new XdmArray(new XdmValue[]{new XdmAtomicValue(3), new XdmAtomicValue(1)}));
            assertEquals(1, ((XdmAtomicValue) result.get(0)).getLongValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testCallSystemFunctionSaxon() {
        try {
            Processor proc = new Processor(true);
            XdmFunctionItem fi = XdmFunctionItem.getSystemFunction(proc, new QName(NamespaceConstant.SAXON, "in-summer-time"), 2);
            assertNotNull(fi);
            XdmAtomicValue arg1 = new XdmAtomicValue("2016-08-20T12:00:00", ItemType.DATE_TIME);
            XdmAtomicValue arg2 = new XdmAtomicValue("Europe/London");
            XdmAtomicValue result = (XdmAtomicValue) fi.call(proc, arg1, arg2);
            assertTrue(result.getBooleanValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testCallSystemFunctionBadArg() {
        try {
            Processor proc = new Processor(true);
            XdmFunctionItem fi = XdmFunctionItem.getSystemFunction(proc, new QName(NamespaceConstant.MATH, "sqrt"), 1);
            XdmAtomicValue result = (XdmAtomicValue) fi.call(proc, new XdmAtomicValue("minus one"));
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            assertEquals("XPTY0004", e.getErrorCode().getLocalName());
        }
    }

    @Test
    public void testCallSystemFunctionFailIfUnlicensed() {
        try {
            Processor proc = new Processor(false);
            XdmFunctionItem fi = XdmFunctionItem.getSystemFunction(proc, new QName(NamespaceConstant.SAXON, "in-summer-time"), 2);
            assertNull(fi);
//            XdmAtomicValue arg1 = new XdmAtomicValue("2016-08-20T12:00:00", ItemType.DATE_TIME);
//            XdmAtomicValue arg2 = new XdmAtomicValue("Europe/London");
//            XdmAtomicValue result = (XdmAtomicValue) fi.call(proc, arg1, arg2);
//            assertTrue(result.getBooleanValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testExtensionFunctionDefinitionAsFunction() {
        try {
            ExtensionFunctionDefinition sqrt = new TestIntegratedFunctions.SqrtFunction();
            XdmFunctionItem fi = new XdmFunctionItem(sqrt.asFunction(1));
            Processor proc = new Processor(true);
            XQueryCompiler compiler = proc.newXQueryCompiler();
            XQueryExecutable exec = compiler.compile("declare variable $f as function(xs:double) as xs:double external; $f(4)");
            XQueryEvaluator eval = exec.load();
            eval.setExternalVariable(new QName("f"), fi);
            XdmAtomicValue result = (XdmAtomicValue) eval.evaluateSingle();
            assertEquals(new XdmAtomicValue(2), result);
        } catch (SaxonApiException e) {
            fail();
        }


    }

    @Test
    public void testTransformReturningFunction() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xsl:template name='main'><xsl:sequence select='f:f#1'/></xsl:template> \n" +
                            "<xsl:function name='f:f' as='xs:integer'><xsl:param name='p'/><xsl:sequence select='$p+42'/></xsl:function> \n" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();
            XdmValue value = transformer.callTemplate(new QName("main"));
            XdmValue result = ((XdmFunctionItem) value).call(proc, new XdmAtomicValue(17));
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals(59, ((XdmAtomicValue) result).getLongValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testTransformReturningFunctionReferencingGlobalVariables() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xsl:template name='main'><xsl:sequence select='f:f#1'/></xsl:template> \n" +
                            "<xsl:param name='offset' select='42'/> \n" +
                            "<xsl:function name='f:f' as='xs:integer'><xsl:param name='p'/><xsl:sequence select='$p+$offset'/></xsl:function> \n" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();
            XdmValue value = transformer.callTemplate(new QName("main"));
            XdmValue result = ((XdmFunctionItem) value).call(proc, new XdmAtomicValue(17));
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals(59, ((XdmAtomicValue) result).getLongValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testTransformReturningFunctionCallingTemplate() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xsl:template name='main'><xsl:sequence select='f:f#1'/></xsl:template> \n" +
                            "<xsl:template name='sub'><xsl:sequence select='$offset'/></xsl:template> \n" +
                            "<xsl:param name='offset' select='42'/> \n" +
                            "<xsl:function name='f:f' as='xs:integer'><xsl:param name='p'/>" +
                            "  <xsl:variable name='t' as='xs:integer'><xsl:call-template name='sub'/></xsl:variable>" +
                            "  <xsl:sequence select='$p+$t'/></xsl:function> \n" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();
            XdmValue value = transformer.callTemplate(new QName("main"));
            XdmValue result = ((XdmFunctionItem) value).call(proc, new XdmAtomicValue(17));
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals(59, ((XdmAtomicValue) result).getLongValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testTransformReturningInlineFunctionReferencingGlobalVariables() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xsl:template name='main'><xsl:sequence select='let $q:=23 return function($p){$p+$q+$offset}'/></xsl:template> \n" +
                            "<xsl:param name='offset' select='42'/> \n" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();
            XdmValue value = transformer.callTemplate(new QName("main"));
            XdmValue result = ((XdmFunctionItem) value).call(proc, new XdmAtomicValue(17));
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals(82, ((XdmAtomicValue) result).getLongValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testFunctionReturnedByFunctionLookup() {
        try {
            Processor proc = new Processor(true);
            XPathCompiler comp = proc.newXPathCompiler();
            XdmItem value = comp.evaluateSingle("function-lookup(QName('http://www.w3.org/2005/xpath-functions', 'round'), 2)", null);
            XdmValue result = ((XdmFunctionItem) value).call(proc,
                                                             new XdmAtomicValue(BigDecimal.valueOf(3.14159)),
                                                             new XdmAtomicValue(3));
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals(BigDecimal.valueOf(3.142), ((XdmAtomicValue) result).getDecimalValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testUserFunctionReturnedByFunctionLookup() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setSchemaAware(true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xsl:template name='main'><xsl:sequence select=\"function-lookup(QName('f', 'f'), 1)\"/></xsl:template> \n" +
                            "<xsl:param name='offset' select='42'/> \n" +
                            "<xsl:function name='f:f' as='xs:integer'><xsl:param name='p'/><xsl:sequence select='$p+$offset'/></xsl:function> \n" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.compile(new StreamSource(sr));
            Xslt30Transformer transformer = exec.load30();
            XdmValue value = transformer.callTemplate(new QName("main"));
            XdmValue result = ((XdmFunctionItem) value).call(proc, new XdmAtomicValue(17));
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals(59, ((XdmAtomicValue) result).getLongValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }


    }

    @Test
    public void testContextDependentFunctionReturnedByFunctionLookup() {
        try {
            Processor proc = new Processor(true);
            XPathCompiler comp = proc.newXPathCompiler();
            XdmItem value = comp.evaluateSingle("function-lookup(QName('http://www.w3.org/2005/xpath-functions', 'string'), 0)",
                                                new XdmAtomicValue(3.14159));
            XdmValue result = ((XdmFunctionItem) value).call(proc);
            assertTrue(result instanceof XdmAtomicValue);
            assertTrue(((XdmAtomicValue) result).getStringValue().startsWith("3.14"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testLangFunction() {
        try {
            Processor proc = new Processor(true);
            XPathCompiler comp = proc.newXPathCompiler();
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode doc = builder.build(new StreamSource(new StringReader("<a xml:lang='de'>Verboten</a>")));
            XdmItem value = comp.evaluateSingle("lang#1", doc.axisIterator(Axis.CHILD).next());
            XdmValue result1 = ((XdmFunctionItem) value).call(proc, new XdmAtomicValue("de"));
            assertTrue(result1 instanceof XdmAtomicValue);
            assertTrue(((XdmAtomicValue) result1).getBooleanValue());
            XdmValue result2 = ((XdmFunctionItem) value).call(proc, new XdmAtomicValue("en"));
            assertTrue(result2 instanceof XdmAtomicValue);
            assertFalse(((XdmAtomicValue) result2).getBooleanValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }

    @Test
    public void testQueryReturningFunctionReferencingGlobalVariables() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.setSchemaAware(true);
            String query = "declare variable $offset as xs:integer external;"
                    + "declare function local:f($p) as xs:integer {$p + $offset};"
                    + "local:f#1";
            XQueryExecutable exec = comp.compile(query);
            XQueryEvaluator eval = exec.load();
            eval.setExternalVariable(new QName("offset"), new XdmAtomicValue(42));
            XdmValue value = eval.evaluate();
            XdmValue result = ((XdmFunctionItem) value).call(proc, new XdmAtomicValue(17));
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals(59, ((XdmAtomicValue) result).getLongValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

    }


}




