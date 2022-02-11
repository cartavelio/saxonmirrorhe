package s9apitest;

import junit.framework.TestCase;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.s9api.*;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;


public class TestReflexionNoByteCode extends TestCase {

    // This is a copy of TestReflexion with a different value for BYTECODE: do not fork!!!

    private final static boolean BYTECODE = false;

    private XQueryCompiler makeCompiler() {
        Processor p = new Processor(true);
        p.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, BYTECODE);
        XQueryCompiler c = p.newXQueryCompiler();
        c.declareNamespace("this", "java:s9apitest.TestReflexionHelper");
        return c;
    }


    public void testConstructor0() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                    "let $x := this:new() return $x => this:getIntValue()");
            XdmValue val = exec.load().evaluate();
            long result = ((XdmAtomicValue)val).getLongValue();
            assertEquals(0, result);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testConstructor1() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                    "let $x := this:new(42) return $x => this:getIntValue()");
            XdmValue val = exec.load().evaluate();
            long result = ((XdmAtomicValue) val).getLongValue();
            assertEquals(42, result);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testVoidSetter1() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                    "let $x := this:new() return ($x => this:setIntValue(13), $x => this:getIntValue())");
            XdmValue val = exec.load().evaluate();
            long result = ((XdmAtomicValue) val).getLongValue();
            assertEquals(13, result);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testThisSetter1() {
        try {
            XQueryCompiler c = makeCompiler();
            c.declareNamespace("this", "java:s9apitest.TestReflexionHelper?void=this");
            XQueryExecutable exec = c.compile(
                    "let $x := this:new() return $x => this:setIntValue(13) => this:setStringValue('banana') => this:getIntValue()");
            XdmValue val = exec.load().evaluate();
            long result = ((XdmAtomicValue) val).getLongValue();
            assertEquals(13, result);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    public void testStringVsByteXQuery() {
        // See http://stackoverflow.com/questions/11448305/
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                "let $x := (if (current-date() lt xs:date('2099-01-01')) then 'abc' else 21) return this:process($x)");
            XdmValue val = exec.load().evaluate();
            assertTrue(((XdmAtomicValue) val).getStringValue().startsWith("String"));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testStringVsByteXSLT() {
        // See http://stackoverflow.com/questions/11448305/
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, BYTECODE);
            XsltCompiler c = p.newXsltCompiler();
            StringReader sr = new StringReader(
                "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
                    "xmlns:this='java:s9apitest.TestReflexionHelper' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                    "<xsl:param name='x' as='xs:string'>abc</xsl:param>" +
                    "<xsl:template name='main'>[<xsl:value-of select='this:process($x)'/>]</xsl:template>" +
                    "</xsl:stylesheet>");
            SAXSource style = new SAXSource(new InputSource(sr));
            XsltTransformer trans = c.compile(style).load();
            trans.setInitialTemplate(new QName("main"));
            StringWriter sw = new StringWriter();
            Serializer serializer = p.newSerializer(sw);
            trans.setDestination(serializer);
            trans.transform();
            assertTrue(sw.toString().endsWith("[String abc]"));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testEmptySequenceToNull() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                "let $x as xs:string? := 'x'[minutes-from-duration(implicit-timezone())=23] return this:process($x)");
            XdmValue val = exec.load().evaluate();
            assertEquals("String null", ((XdmAtomicValue) val).getStringValue().trim());
            exec = c.compile(
                "let $x as xs:double? := () return this:processDouble($x)");
            val = exec.load().evaluate();
            assertEquals("Double null", ((XdmAtomicValue) val).getStringValue().trim());
            exec = c.compile(
                "let $x as xs:double? := () return this:processDoubleValue($x)");
            val = exec.load().evaluate();
            assertEquals("DoubleValue null", ((XdmAtomicValue) val).getStringValue().trim());
            try {
                exec = c.compile(
                    "let $x as xs:int? := () return this:processInt($x)");
                val = exec.load().evaluate();
                fail();
            } catch (SaxonApiException e) {
                assertTrue(e.getMessage().contains("An empty sequence is not allowed"));
            }
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testEmptyList() {
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, BYTECODE);
            XsltCompiler c = p.newXsltCompiler();
            String s = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                "    xmlns:jf=\"java:s9apitest.TestReflexionHelper\">\n" +
                "    <xsl:output method=\"text\" />\n" +
                "    <xsl:template name='main'>\n" +
                "        <xsl:variable name=\"theList\" select=\"jf:getEmptyList()\" />\n" +
                "        <xsl:value-of select=\"Q{java:java.util.List}size($theList)\" />\n" +
                "    </xsl:template>\n" +
                "</xsl:stylesheet>";
            XsltTransformer t = c.compile(new StreamSource(new StringReader(s))).load();
            StringWriter out = new StringWriter();
            Serializer ser = p.newSerializer(out);
            t.setDestination(ser);
            t.setInitialTemplate(new QName("main"));
            t.transform();
            assertEquals("0", out.toString());
        } catch (SaxonApiException e1) {
            fail(e1.getMessage());
        }
    }

    public void testNonEmptyList() {
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, BYTECODE);
            XsltCompiler c = p.newXsltCompiler();
            String s = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"\n" +
                "    xmlns:jf=\"java:s9apitest.TestReflexionHelper\">\n" +
                "    <xsl:output method=\"text\" />\n" +
                "    <xsl:template name='main'>\n" +
                "        <xsl:variable name=\"theList\" select=\"jf:getList()\" />\n" +
                "        <xsl:value-of select=\"Q{java:java.util.List}size($theList)\" />\n" +
                "    </xsl:template>\n" +
                "</xsl:stylesheet>";
            XsltTransformer t = c.compile(new StreamSource(new StringReader(s))).load();
            StringWriter out = new StringWriter();
            Serializer ser = p.newSerializer(out);
            t.setDestination(ser);
            t.setInitialTemplate(new QName("main"));
            t.transform();
            assertEquals("2", out.toString());
        } catch (SaxonApiException e1) {
            fail(e1.getMessage());
        }
    }

    public void testStringSplit() {
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, BYTECODE);
            XsltCompiler c = p.newXsltCompiler();
            String s = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                "    <xsl:output method=\"text\" />\n" +
                "    <xsl:template name='main'>\n" +
                "        <xsl:value-of select=\"Q{java:java.lang.String}split('a,b,c,d', ',')\" separator='|'/>\n" +
                "    </xsl:template>\n" +
                "</xsl:stylesheet>";
            XsltTransformer t = c.compile(new StreamSource(new StringReader(s))).load();
            StringWriter out = new StringWriter();
            Serializer ser = p.newSerializer(out);
            t.setDestination(ser);
            t.setInitialTemplate(new QName("main"));
            t.transform();
            assertTrue(out.toString().equals("a|b|c|d"));
        } catch (SaxonApiException e1) {
            fail(e1.getMessage());
        }
    }

    public void testStaticInvalidRequiredExternalObjectType() {
        // Error should be detected at compile time
        Processor p = new Processor(true);
        p.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, BYTECODE);
        XsltCompiler c = p.newXsltCompiler();
        String s = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
            "    <xsl:output method='text' />\n" +
            "    <xsl:variable name='v' as='Q{http://saxon.sf.net/java-type}java.util.Map' " +
            "           select='Q{java:java.util.Collections}emptyList()'/>\n" +
            "    <xsl:template name='main'>\n" +
            "        <xsl:value-of select='$v' separator='|'/>\n" +
            "    </xsl:template>\n" +
            "</xsl:stylesheet>";
        XsltTransformer t;
        try {
            t = c.compile(new StreamSource(new StringReader(s))).load();
        } catch (SaxonApiException err) {
            // OK
            return;
        }
        fail("Type error not detected");
//            StringWriter out = new StringWriter();
//            Serializer ser = p.newSerializer(out);
//            t.setDestination(ser);
//            t.setInitialTemplate(new QName("main"));
//            t.transform();
//            assertTrue(out.toString().equals("a|b|c|d"));

    }

    public void testDynamicInvalidRequiredExternalObjectType() {
        XsltTransformer t;
        try {
            // Error should be detected at run time
            Processor p = new Processor(true);
            p.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, BYTECODE);
            XsltCompiler c = p.newXsltCompiler();
            String s = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                "<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                "    <xsl:output method='text' />\n" +
                "    <xsl:variable name='v' as='Q{http://saxon.sf.net/java-type}java.util.Vector' " +
                "           select='Q{java:java.util.Collections}emptyList()'/>\n" +
                "    <xsl:template name='main'>\n" +
                "        <xsl:value-of select='$v' separator='|'/>\n" +
                "    </xsl:template>\n" +
                "</xsl:stylesheet>";
            t = c.compile(new StreamSource(new StringReader(s))).load();

            StringWriter out = new StringWriter();
            Serializer ser = p.newSerializer(out);
            t.setDestination(ser);
            t.setInitialTemplate(new QName("main"));
            try {
                t.transform();
            } catch (SaxonApiException e) {
                // OK: failure was expected
                //return;
            }
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }




    public void testConvertUntypedAtomic() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec;
            XdmValue val;
            exec = c.compile(
                "let $x as xs:untypedAtomic := data(<a>1.234</a>) return this:processDouble($x)");
            val = exec.load().evaluate();
            assertTrue(((XdmAtomicValue) val).getStringValue().trim().equals("Double 1.234"));
            exec = c.compile(
                "let $x as xs:untypedAtomic := data(<a>2</a>) return this:processInt($x)");
            val = exec.load().evaluate();
            assertTrue(((XdmAtomicValue) val).getStringValue().trim().equals("int 2"));
            exec = c.compile(
                "let $x as xs:untypedAtomic := data(<a>1.234</a>) return this:processDoubleValue($x)");
            val = exec.load().evaluate();
            assertTrue(((XdmAtomicValue) val).getStringValue().trim().equals("DoubleValue 1.234"));
            exec = c.compile(
                "let $x as xs:untypedAtomic := data(<a>2</a>) return this:processIntegerValue($x)");
            val = exec.load().evaluate();
            assertTrue(((XdmAtomicValue) val).getStringValue().trim().equals("IntegerValue 2"));

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testAtomization() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec;
            XdmValue val;
            exec = c.compile(
                "let $x := <a>1.234</a> return this:processDouble($x)");
            val = exec.load().evaluate();
            assertTrue(((XdmAtomicValue) val).getStringValue().trim().equals("Double 1.234"));
            exec = c.compile(
                "let $x := <a>2</a> return this:processInt($x)");
            val = exec.load().evaluate();
            assertTrue(((XdmAtomicValue) val).getStringValue().trim().equals("int 2"));
            exec = c.compile(
                "let $x := <a>1.234</a> return this:processDoubleValue($x)");
            val = exec.load().evaluate();
            assertTrue(((XdmAtomicValue) val).getStringValue().trim().equals("DoubleValue 1.234"));
            exec = c.compile(
                "let $x := <a>2</a> return this:processIntegerValue($x)");
            val = exec.load().evaluate();
            assertTrue(((XdmAtomicValue) val).getStringValue().trim().equals("IntegerValue 2"));

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testReturnNodes() {
        // Test that we detect when returned nodes have the wrong Configuration. See non-bug 1856.
        try {
            Processor p = new Processor(true);
            boolean[] options = {true, false};
            for (boolean option : options) {
                p.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, option);
                XQueryCompiler c = p.newXQueryCompiler();
                c.declareNamespace("this", "java:s9apitest.TestReflexionHelper");
                XQueryExecutable exec;
                XdmValue val;
                exec = c.compile(
                    "name(this:makeDocGood()[last()]/*)");
                val = exec.load().evaluate();
                assertTrue(((XdmAtomicValue) val).getStringValue().trim().equals("doc"));
                try {
                    exec = c.compile(
                        "name(this:makeDocBad()[last()]/*)");
                    val = exec.load().evaluate();
                    fail("Failed to report error 1");
                } catch (SaxonApiException e) {
                    assertTrue(e.getMessage().contains("incompatible Configuration"));
                }
                exec = c.compile(
                    "name(this:makeDocsGood()[last()]/*)");
                val = exec.load().evaluate();
                assertTrue(((XdmAtomicValue) val).getStringValue().trim().equals("doc2"));
                try {
                    exec = c.compile(
                        "name(this:makeDocsBad()[last()]/*)");
                    val = exec.load().evaluate();
                    fail("Failed to report error 2");
                } catch (SaxonApiException e) {
                    assertTrue(e.getMessage().contains("incompatible Configuration"));
                }
            }

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }
}

