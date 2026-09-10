package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.Sequence;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.math.BigDecimal;
import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;


public class TestReflexion {
    private final ConfigTest configTest = new ConfigTest();

    private XQueryCompiler makeCompiler() {
        Processor p = new Processor(true);
        XQueryCompiler c = p.newXQueryCompiler();
        c.declareNamespace("this", "java:s9apitest.TestReflexionHelper");
        return c;
    }


    @Test
    public void testConstructor0() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                    "let $x := this:new() return $x => this:getIntValue()");
            XdmValue val = exec.load().evaluate();
            long result = ((XdmAtomicValue) val).getLongValue();
            assertTrue(result == 0);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testConstructor1() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                    "let $x := this:new(42) return $x => this:getIntValue()");
            XdmValue val = exec.load().evaluate();
            long result = ((XdmAtomicValue) val).getLongValue();
            assertTrue(result == 42);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testVoidSetter1() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                    "let $x := this:new() return ($x => this:setIntValue(13), $x => this:getIntValue())");
            XdmValue val = exec.load().evaluate();
            long result = ((XdmAtomicValue) val).getLongValue();
            assertTrue(result == 13);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testThisSetter1() {
        try {
            XQueryCompiler c = makeCompiler();
            c.declareNamespace("this", "java:s9apitest.TestReflexionHelper?void=this");
            XQueryExecutable exec = c.compile(
                    "let $x := this:new() return $x => this:setIntValue(13) => this:setStringValue('banana') => this:getIntValue()");
            XdmValue val = exec.load().evaluate();
            long result = ((XdmAtomicValue) val).getLongValue();
            assertTrue(result == 13);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    private final String xslVoidThis = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
            + "<xsl:stylesheet exclude-result-prefixes=\"#all\" version=\"2.0\" \n"
            + "    xmlns:date=\"java:java.util.Date?void=this\" xmlns:jt=\"http://saxon.sf.net/java-type\" \n"
            + "    xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n"
            + "\n"
            + "    <xsl:output indent=\"yes\" />\n"
            + "\n"
            + "    <xsl:template name=\"main\">\n"
            + "        <xsl:variable as=\"jt:java.util.Date\" name=\"date\" select=\"date:new()\" />\n"
            + "        <new>\n"
            + "            <xsl:value-of select=\"date:toGMTString($date)\" />\n"
            + "        </new>\n"
            + "\n"
            + "        <xsl:variable as=\"jt:java.util.Date\" name=\"date\" select=\"date:setTime($date, 0)\" />\n"
            + "        <updated>\n"
            + "            <xsl:value-of select=\"date:toGMTString($date)\" />\n"
            + "        </updated>\n"
            + "    </xsl:template>\n"
            + "</xsl:stylesheet>";

    @Test
    public void testThisSetter2() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(xslVoidThis)));
            Xslt30Transformer trans = exec.load30();
            XdmValue result = trans.callTemplate(new QName("main"));
            assertTrue(result.toString().contains("<updated>1 Jan 1970 00:00:00 GMT</updated>"));
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    @Test
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

    @Test
    public void testStringVsByteXSLT() {
        // See http://stackoverflow.com/questions/11448305/
        try {
            Processor p = new Processor(true);
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

    @Test
    public void testEmptySequenceToNull() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                    "let $x as xs:string? := 'xxx'[current-date() gt xs:date('2100-01-01')] return this:process($x)");
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

    @Test
    public void testByteArrayInAndOut() {
        // See bugs 3525, 3818
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                    "declare function local:ub($x as xs:integer) as xs:unsignedByte { xs:unsignedByte(abs($x) mod 256)};" +
                    "let $x as xs:unsignedByte* := (10,11,12,13,14,15,16,17,18,19,20) ! local:ub(.) " +
                    "let $y := this:toEvenByteArray($x) " +
                    "return (($y instance of xs:unsignedByte*), (count($y) eq 11), ($y[1] eq 10), ($y[2] eq 10), ($y[3] eq 12), ($y[4] eq 12))");
            XdmValue val = exec.load().evaluate();
            assertEquals("true", val.itemAt(0).getStringValue(), "type");
            assertEquals("true", val.itemAt(1).getStringValue(), "size");
            assertEquals("true", val.itemAt(2).getStringValue(), "item1");
            assertEquals("true", val.itemAt(3).getStringValue(), "item2");
            assertEquals("true", val.itemAt(4).getStringValue(), "item3");
            assertEquals("true", val.itemAt(5).getStringValue(), "item4");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSupplyDate() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                    "this:is2008d(xs:dateTime('2008-01-01T12:30:00Z'))");
            XdmValue val = exec.load().evaluate();
            assertEquals("true", val.itemAt(0).getStringValue(), "val");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSupplyInstant() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                    "this:is2008i(xs:dateTime('2008-01-01T12:30:00Z'))");
            XdmValue val = exec.load().evaluate();
            assertEquals("true", val.itemAt(0).getStringValue(), "val");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSupplyZonedDateTime() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                    "this:is2008z(xs:dateTime('2008-01-01T12:30:00Z'))");
            XdmValue val = exec.load().evaluate();
            assertEquals("true", val.itemAt(0).getStringValue(), "val");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSupplyLocalDateTime() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                    "this:is2008localDateTime(xs:dateTime('2008-01-01T12:30:00Z'))");
            XdmValue val = exec.load().evaluate();
            assertEquals("true", val.itemAt(0).getStringValue(), "val");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSupplyLocalDate() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec = c.compile(
                    "this:is2008localDate(xs:date('2008-01-01'))");
            XdmValue val = exec.load().evaluate();
            assertEquals("true", val.itemAt(0).getStringValue(), "val");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testEmptyList() {
        try {
            Processor p = new Processor(true);
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
            assertTrue(out.toString().equals("0"));
        } catch (SaxonApiException e1) {
            fail(e1.getMessage());
        }
    }

    @Test
    public void testNonEmptyList() {
        try {
            Processor p = new Processor(true);
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
            assertTrue(out.toString().equals("2"));
        } catch (SaxonApiException e1) {
            fail(e1.getMessage());
        }
    }

    @Test
    public void testStringSplit() {
        try {
            Processor p = new Processor(true);
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

    @Test
    public void testStaticInvalidRequiredExternalObjectType() {
        // Error should be detected at compile time
        Processor p = new Processor(true);
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

    @Test
    public void testDynamicInvalidRequiredExternalObjectType() {
        XsltTransformer t;
        try {
            // Error should be detected at run time
            Processor p = new Processor(true);
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

    @Test
    public void testDecimals() {
        // Bug 2765
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec;
            XdmValue val;

            try {
                exec = c.compile(
                        "this:addDecimals(())");
                val = exec.load().evaluate();
                fail("unexpected success");
            } catch (SaxonApiException e) {
                assertEquals("XPTY0004", e.getErrorCode().getLocalName());
            }


            exec = c.compile(
                    "this:addDecimals((1, 2, 3))");
            val = exec.load().evaluate();
            assertEquals(((XdmAtomicValue) val).getDecimalValue(), new BigDecimal(6));

            exec = c.compile(
                    "this:addDecimals((1.1, 2.2, 3.3))");
            val = exec.load().evaluate();
            assertEquals(((XdmAtomicValue) val).getDecimalValue(), new BigDecimal("6.6"));


            try {
                exec = c.compile(
                        "this:addDecimals(('NaN'))");
                val = exec.load().evaluate();
                fail("unexpected success");
            } catch (SaxonApiException e) {
                assertEquals("XPTY0004", e.getErrorCode().getLocalName());
            }

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testConvertUntypedAtomic() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec;
            XdmValue val;
            exec = c.compile(
                    "let $x as xs:untypedAtomic := data(<a>1.234</a>) return this:processDouble($x)");
            val = exec.load().evaluate();
            assertEquals("Double 1.234", ((XdmAtomicValue) val).getStringValue().trim());
            exec = c.compile(
                    "let $x as xs:untypedAtomic := data(<a>2</a>) return this:processInt($x)");
            val = exec.load().evaluate();
            assertEquals("int 2", ((XdmAtomicValue) val).getStringValue().trim());
            exec = c.compile(
                    "let $x as xs:untypedAtomic := data(<a>1.234</a>) return this:processDoubleValue($x)");
            val = exec.load().evaluate();
            assertEquals("DoubleValue 1.234", ((XdmAtomicValue) val).getStringValue().trim());
            exec = c.compile(
                    "let $x as xs:untypedAtomic := data(<a>2</a>) return this:processIntegerValue($x)");
            val = exec.load().evaluate();
            assertEquals("IntegerValue 2", ((XdmAtomicValue) val).getStringValue().trim());

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testAtomization() {
        try {
            XQueryCompiler c = makeCompiler();
            XQueryExecutable exec;
            XdmValue val;
            exec = c.compile(
                    "let $x := <a>1.234</a> return this:processDouble($x)");
            val = exec.load().evaluate();
            assertEquals("Double 1.234", ((XdmAtomicValue) val).getStringValue().trim());
            exec = c.compile(
                    "let $x := <a>2</a> return this:processInt($x)");
            val = exec.load().evaluate();
            assertEquals("int 2", ((XdmAtomicValue) val).getStringValue().trim());
            exec = c.compile(
                    "let $x := <a>1.234</a> return this:processDoubleValue($x)");
            val = exec.load().evaluate();
            assertEquals("DoubleValue 1.234", ((XdmAtomicValue) val).getStringValue().trim());
            exec = c.compile(
                    "let $x := <a>2</a> return this:processIntegerValue($x)");
            val = exec.load().evaluate();
            assertEquals("IntegerValue 2", ((XdmAtomicValue) val).getStringValue().trim());

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testReturnNodes() {
        // Test that we detect when returned nodes have the wrong Configuration. See non-bug 1856.
        try {
            Processor p = new Processor(true);
            boolean[] options = {true, false};
            for (boolean option : options) {
                p.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, option);
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

    @Test
    public void testBridge0() {
        try {
            XQueryCompiler c = makeCompiler();
            c.getProcessor().setConfigurationProperty(Feature.TRACE_EXTERNAL_FUNCTIONS, true);
            XQueryExecutable exec = c.compile(
                    "let $n :=  Q{java:s9apitest.MyFoo}new() return Q{java:s9apitest.MyFoo}foo($n)");
            XdmValue val = exec.load().evaluate();
            assertTrue("foo".equals(val.toString()));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testPrajapati() {
        String xsl = "" +
                "<?xml version='1.0'?>\n" +
                "\n" +
                "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.1\" " +
                "   xmlns:saxonJavaMappingTransformation='java:s9apitest.TestReflexion' " +
                "xmlns:saxonJavaRecordFetch='java:s9apitest.TestReflexion' " +
                "xmlns:saxonJavaAggregateFunctionHandler=\"java:com.adeptia.indigo.services.mapping.AggregateFunctionHandler\"" +
                " xmlns:str=\"http://exslt.org/strings\" exclude-result-prefixes=\"saxonJavaMappingTransformation  saxonJavaAggregateFunctionHandler str\" xmlns:saxon=\"http://saxon.sf.net/\" extension-element-prefixes=\"saxon\">\n" +
                "\n     <xsl:output method=\"xml\" version=\"1.0\" encoding=\"utf-8\" indent=\"yes\"/>\n" +
                "\n     <xsl:param name=\"_userName\"/>\n" +
                "\n     <xsl:param name=\"_password\"/>\n" +
                "\n     <xsl:param name=\"_class\"/>\n" +
                "\n     <xsl:param name=\"_identifier\"/>\n" +
                "\n     <xsl:param name=\"_subject\"/>\n" +
                "\n     <xsl:param name=\"_repositoryPath\"/>\n" +
                "\n     <xsl:param name=\"_Input1\"/>\n" +
                "\n     <xsl:variable name=\"varConn\" select=\"saxonJavaRecordFetch:fetchRecordXml()\"/>\n" +
                "\n     <xsl:variable name=\"apos\">'</xsl:variable>\n" +
                "\n     <xsl:template name='t1'>\n" +
                "\n          <Root>\n" +
                "\n               <Record>\n" +
                "\n                    <xsl:attribute name=\"recordNumber\"></xsl:attribute>\n" +
                "\n                    <Source><xsl:value-of select='$varConn//Record//PERS_NBR'/></Source>\n" +
                "\n               </Record>\n" +
                "\n          </Root>\n" +
                "\n     </xsl:template>\n" +
                "\n     <xsl:template name='t2'>\n" +
                "\n          <Root>\n" +
                "\n               <Record>\n" +
                "\n                    <xsl:attribute name=\"recordNumber\"></xsl:attribute>\n" +
                "\n                    <Source><xsl:value-of select='$varConn//Record//PERS_NBR'/></Source>\n" +
                "\n                    <Resp_Work_Phone></Resp_Work_Phone>\n" +
                "\n               </Record>\n" +
                "\n          </Root>\n" +
                "\n     </xsl:template>\n" +
                "\n</xsl:stylesheet>\n";
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(xsl)));
            Xslt30Transformer trans = exec.load30();
            //trans.callTemplate(new QName("t1"), trans.newSerializer(System.out));
            //trans.callTemplate(new QName("t2"), trans.newSerializer(System.out));
        } catch (SaxonApiException e) {
            e.printStackTrace();
        }
    }

    @Test
    public void testAmbiguousConstructorExport() {
        // Bug 3019
        String xsl = "<xsl:stylesheet exclude-result-prefixes=\"#all\" version=\"2.0\" xmlns:URL=\"java:java.net.URL\"\n" +
                "\txmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                "\t<xsl:template name=\"main\">\n" +
                "\t\t<xsl:sequence select=\"'URL:', URL:new('http', 'www.example.com', '/file')\" />\n" +
                "\t</xsl:template>\n" +
                "</xsl:stylesheet>";
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setJustInTimeCompilation(false);
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(xsl)));
            ByteArrayOutputStream out = new ByteArrayOutputStream();
            exec.export(out);

            Processor proc2 = new Processor(true);
            XsltCompiler comp2 = proc2.newXsltCompiler();
            comp2.setJustInTimeCompilation(false);
            XsltExecutable exec2 = comp2.compile(new StreamSource(new ByteArrayInputStream(out.toByteArray())));
            Xslt30Transformer trans = exec2.load30();
            StringWriter sw = new StringWriter();
            trans.callTemplate(new QName("main"), trans.newSerializer(sw));
            assertTrue(sw.toString().endsWith("URL: http://www.example.com/file"));
        } catch (SaxonApiException e) {
            fail();
            e.printStackTrace();
        }

    }

    public static Object fetchRecordXml() {
        String xml = "<Root><Record><PERS_NBR>123</PERS_NBR><Name>SAN</Name></Record></Root>";
        InputSource ins = new InputSource(new StringReader(xml));
        SAXSource sax = new SAXSource(ins);
        Object obj = null;
        Processor processor = new Processor(true);
        DocumentBuilder builder = processor.newDocumentBuilder();
        try {
            XdmNode xdmnode = builder.build(sax);
            Sequence sq = xdmnode.getUnderlyingNode();
            obj = sq.iterate();
        } catch (Exception e) {
            e.printStackTrace();
        }
        return obj;
    }

    @Test
    public void testObjectValueAsMap() {
        String xsl = "<xsl:stylesheet exclude-result-prefixes=\"#all\" "
                + "version=\"3.0\" "
                + "xmlns:URL=\"java:java.net.URL\"\n" +
                "\txmlns:xs=\"http://www.w3.org/2001/XMLSchema\" "
                + "xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" "
                + "xmlns:map='http://www.w3.org/2005/xpath-functions/map'>\n" +
                "<xsl:variable name='eo' select=\"URL:new('http', 'www.example.com', '/file')\"/>" +
                "<xsl:variable name='eo-map' select='Q{http://saxon.sf.net/}object-map($eo)'/>" +
                "\t<xsl:template name=\"main\">\n" +
                //"\t\t<xsl:message select='map:keys($eo)'/>" +
                "\t\t<out protocol='{$eo?getProtocol()}' string='{$eo?toString()}' ok1=\"{map:contains($eo-map, 'openStream')}\""
                + " ok2=\"{map:put($eo-map, 'extra', 17) => map:contains('extra')}\"/>\n" +
                "\t</xsl:template>\n" +
                "</xsl:stylesheet>";
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            XsltExecutable exec = comp.compile(new StreamSource(new StringReader(xsl)));
            Xslt30Transformer trans = exec.load30();
            StringWriter sw = new StringWriter();
            trans.callTemplate(new QName("main"), trans.newSerializer(sw));
            //System.err.println(sw.toString());
            String r = sw.toString();
            assertTrue(r.contains("protocol=\"http\""));
            assertTrue(r.contains("string=\"http://www.example.com/file\""));
            assertTrue(r.contains("ok1=\"true\""));
            assertTrue(r.contains("ok2=\"true\""));
        } catch (SaxonApiException e) {
            fail();
            e.printStackTrace();
        }

    }


    /**
     * Test overriding of reflexive extension function using stylesheet functions in XSLT
     */

    @Test
    public void testOverrideFunctionXsltOverriding() {
        testOverrideFunctionXslt(xslWithOverridingFunction1, true);
        testOverrideFunctionXslt(xslWithOverridingFunction2, true);
    }

    @Test
    public void testOverrideFunctionXsltUnderriding() {
        testOverrideFunctionXslt(xslWithOverridingFunction1, false);
        testOverrideFunctionXslt(xslWithOverridingFunction2, false);
    }

    private static final String xslWithOverridingFunction1 =
            "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:f='java:java.lang.Math'>" +
                    "<xsl:template name='go'><a sqrt2='{Q{java:java.lang.Math}sqrt(2)}' />" +
                    "</xsl:template>" +
                    "<xsl:param name='O' as='xs:boolean' static='true'/>" +
                    "<xsl:function name='f:sqrt' as='xs:string' _override-extension-function='{$O}'>" +
                    "<xsl:param name='num'/>" +
                    "<xsl:sequence select=\"'local'\" />" +
                    "</xsl:function>" +
                    "</xsl:stylesheet>";

    private static final String xslWithOverridingFunction2 =
            "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:f='java:java.lang.Math'>" +
                    "<xsl:template name='go'><a sqrt2='{Q{java:java.lang.Math}sqrt(2)}' />" +
                    "</xsl:template>" +
                    "<xsl:param name='O' as='xs:boolean' static='true'/>" +
                    "<xsl:function name='f:sqrt' as='xs:string' _override='{$O}'>" +
                    "<xsl:param name='num'/>" +
                    "<xsl:sequence select=\"'local'\" />" +
                    "</xsl:function>" +
                    "</xsl:stylesheet>";

    private static void testOverrideFunctionXslt(String xsl, boolean override) {
        // Bug #4111
        try {
            ConfigTest ctest = new ConfigTest();
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.newXsltCompiler();
            comp.setJustInTimeCompilation(false);
            StringReader sr = new StringReader(xsl);
            comp.setParameter(new QName("O"), new XdmAtomicValue(override));
            XsltExecutable exec1 = comp.compile(new StreamSource(sr));
            List<XsltExecutable> execs = new ArrayList<>();
            execs.add(exec1);
            exec1.export(new FileOutputStream(ctest.getDataDir() + "sandpit/export.sef"));
            XsltExecutable exec2 = comp.loadExecutablePackage(new File(ctest.getDataDir() + "sandpit/export.sef").toURI());
            execs.add(exec2);
            for (XsltExecutable exec : execs) {
                XsltTransformer trans = exec.load();
                trans.setInitialTemplate(new QName("go"));
                StringWriter sw = new StringWriter();
                Serializer out = proc.newSerializer();
                out.setOutputWriter(sw);
                trans.setDestination(out);
                trans.transform();
                assertTrue(override == sw.toString().contains("local"), "sqrt ");
            }

        } catch (Exception e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    @Test
    public void testLookupNotationWithContext() {    // bug 4056
        try {
            Processor proc = new Processor(true);
            XPathCompiler compiler = proc.newXPathCompiler();
            XPathExecutable exp = compiler.compile(
                    "let $e := Q{java:net.sf.saxon.expr.Literal}makeEmptySequence() return $e?effectiveBooleanValue()");
            XdmValue result = exp.load().evaluateSingle();
            assertFalse(((XdmAtomicValue) result).getBooleanValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testPublicStaticField() {
        try {
            Processor proc = new Processor(true);
            XPathCompiler compiler = proc.newXPathCompiler();
            XPathExecutable exp = compiler.compile(
                    "let $e := Q{java:net.sf.saxon.om.NamespaceUri}SAXON() return $e?toString()");
            XdmValue result = exp.load().evaluateSingle();
            assertEquals(((XdmAtomicValue) result).getStringValue(), NamespaceUri.SAXON.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testPublicInstanceField() {
        try {
            Processor proc = new Processor(true);
            XPathCompiler compiler = proc.newXPathCompiler();
            XPathExecutable exp = compiler.compile(
                    "Q{java:net.sf.saxon.style.PackageVersion}new('1.2.3-alpha') "
                            + "=> Q{java:net.sf.saxon.style.PackageVersion}suffix()");
            XdmValue result = exp.load().evaluateSingle();
            assertEquals(((XdmAtomicValue) result).getStringValue(), "alpha");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

}

