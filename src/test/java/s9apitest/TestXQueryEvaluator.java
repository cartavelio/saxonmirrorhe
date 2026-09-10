package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.flwor.*;
import net.sf.saxon.expr.instruct.FixedAttribute;
import net.sf.saxon.expr.instruct.FixedElement;
import net.sf.saxon.expr.instruct.TraceExpression;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.lib.TraceListener;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trace.Traceable;
import net.sf.saxon.trans.SimpleMode;
import net.sf.saxon.type.Type;
import org.junit.jupiter.api.Test;
import org.xml.sax.helpers.XMLFilterImpl;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.IOException;
import java.io.StringReader;
import java.io.StringWriter;
import java.util.Map;

import static net.sf.saxon.s9api.streams.Steps.child;
import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertSame;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests for the s9api XQueryCompiler
 */
public class TestXQueryEvaluator {
    private final ConfigTest configTest = new ConfigTest();
    
    @Test
    public void testReturnDocumentNode() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo/>")));
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("document{.}").load();
            eval.setContextItem(foo);

            // First run in push mode
            XdmDestination dest = new XdmDestination();
            eval.run(dest);
            XdmNode doc = dest.getXdmNode();
            assertSame(doc.getNodeKind(), XdmNodeKind.DOCUMENT, "Push result is document");

            // Now run in pull mode
            XdmNode doc2 = (XdmNode) eval.evaluate();
            assertSame(doc2.getNodeKind(), XdmNodeKind.DOCUMENT, "Pull result is document");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testXQueryLineNumber() {
        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();

            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setLineNumbering(true);
            builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.ALL);
            XdmNode source = builder.build(new File(configTest.getDataDir() + "books.xml"));

            XQueryExecutable exp = comp.compile("saxon:line-number((//WEIGHT)[1])");
            XQueryEvaluator eval = exp.load();
            eval.setContextItem(source);
            Serializer out = proc.newSerializer();
            out.setOutputProperty(Serializer.Property.METHOD, "xml");
            out.setOutputProperty(Serializer.Property.INDENT, "yes");
            out.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            out.setOutputStream(System.out);
            StringWriter sw = new StringWriter();
            out.setOutputWriter(sw);
            eval.run(out);
            //System.out.println(sw.toString());
            assertEquals("15", sw.toString(), "linenumber");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testXdmDestinationMultipleNodes() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("document{<a/>}, document{<b/>}, document{<c/>}").load();

            XdmDestination dest = new XdmDestination();
            eval.run(dest);
            XdmNode doc = dest.getXdmNode();
            assertEquals(doc.getNodeKind(), XdmNodeKind.DOCUMENT, "kind");
            assertEquals(3, doc.axisIterator(Axis.CHILD).stream().count(), "child-count");

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    private void separatorTest(String separator, String expected) {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            String q = "declare namespace output = \"http://www.w3.org/2010/xslt-xquery-serialization\";" +
                    "declare option output:method   \"xml\";" +
                    "declare option output:omit-xml-declaration   \"yes\";" +
                    "declare option output:item-separator '" + separator + "';" +
                    "<!--start-->, 1 to 3, <!--end-->";
            XQueryEvaluator eval = c.compile(q).load();

            XdmDestination dest = new XdmDestination();
            eval.run(dest);
            XdmNode doc = dest.getXdmNode();
            assertEquals(XdmNodeKind.DOCUMENT, doc.getNodeKind(), "kind");
            assertEquals(3, doc.axisIterator(Axis.CHILD).stream().count(), "child-count");
            assertEquals(expected, doc.toString(), "content");

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testXdmDestinationItemSeparator() {
        separatorTest("~", "<!--start-->~1~2~3~<!--end-->");
    }

    @Test
    public void testXdmDestinationSpaceSeparator() {
        separatorTest("#absent", "<!--start-->1 2 3<!--end-->");
    }


    /**
     * Test writing a text node only to an XdmDestination
     */

    @Test
    public void testXdmDestinationTextNodeOnly() {
        try {
            Processor processor = new Processor(false);
            XQueryCompiler xpc = processor.newXQueryCompiler();
            XQueryEvaluator s = xpc.compile("text{'abcd'}").load();
            XdmDestination dest = new XdmDestination();
            s.run(dest);
            XdmNode doc = dest.getXdmNode();
            assertEquals(XdmNodeKind.DOCUMENT, doc.getNodeKind(), "kind");
            assertEquals(1, doc.axisIterator(Axis.CHILD).stream().count(), "child-count");
            assertEquals(XdmNodeKind.TEXT, doc.axisIterator(Axis.CHILD).next().getNodeKind(), "child-kind");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    @Test
    public void testRawDestinationMultipleDocumentNodes() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("document{<a/>}, document{<b/>}, document{<c/>}").load();

            RawDestination dest = new RawDestination();
            eval.run(dest);
            XdmValue result = dest.getXdmValue();
            assertEquals(XdmNodeKind.DOCUMENT, ((XdmNode) result.itemAt(0)).getNodeKind(), "kind");
            assertEquals(3, result.size(), "count");

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testRawDestinationMultipleElementNodes() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("<a x='1'><y/></a>, <b x='2'><y/></b>, <c x='3'><y/></c>").load();

            RawDestination dest = new RawDestination();
            eval.run(dest);
            XdmValue result = dest.getXdmValue();
            assertEquals(XdmNodeKind.ELEMENT, ((XdmNode) result.itemAt(0)).getNodeKind(), "kind");
            assertEquals("2", ((XdmNode) result.itemAt(1)).attribute("x"), "att");
            assertEquals("y", result.itemAt(2).select(child("y")).asNode().getNodeName().getLocalName(), "child");
            assertEquals(3, result.size(), "count");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testRawDestinationMultipleAttributeNodes() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("(1 to 3)!attribute{'a'}{string(.)}").load();

            RawDestination dest = new RawDestination();
            eval.run(dest);
            XdmValue result = dest.getXdmValue();
            assertEquals(XdmNodeKind.ATTRIBUTE, ((XdmNode) result.itemAt(0)).getNodeKind(), "kind");
            assertEquals("2", result.itemAt(1).getStringValue(), "att");
            assertEquals("a", ((XdmNode) result.itemAt(2)).getNodeName().getLocalName(), "child");
            assertEquals(3, result.size(), "count");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }


    @Test
    public void testRawDestinationAtomic() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("1 to 10").load();

            RawDestination dest = new RawDestination();
            eval.run(dest);
            XdmValue result = dest.getXdmValue();
            assertEquals(10, result.size());
            assertEquals(1, ((XdmAtomicValue) result.itemAt(0)).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSerializerWithItemSeparator() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile(
                    "declare option Q{http://www.w3.org/2010/xslt-xquery-serialization}item-separator '~'; 1 to 10").load();

            StringWriter writer = new StringWriter();
            Serializer sz = p.newSerializer(writer);
            sz.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            eval.setDestination(sz);
            eval.run();
            assertEquals("1~2~3~4~5~6~7~8~9~10", writer.toString());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSerializerWithItemSeparator2() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile(
                    "declare option Q{http://www.w3.org/2010/xslt-xquery-serialization}item-separator '~'; 1 to 10").load();

            StringWriter writer = new StringWriter();
            Serializer sz = p.newSerializer(writer);
            sz.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            eval.run(sz);
            assertEquals("1~2~3~4~5~6~7~8~9~10", writer.toString());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testSeparateCompilationFromFile() {
        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.compileLibrary(new File(configTest.getDataDir() + "module-one.xq"));
            String q1 = "import module namespace m='http://www.example.com/module-one'; m:add(2,2)";
            XQueryEvaluator ev = comp.compile(q1).load();
            assertEquals("4", ev.evaluate().toString(), "2+2");
            String q2 = "import module namespace m='http://www.example.com/module-one'; m:add($m:ten,5)";
            ev = comp.compile(q2).load();
            assertEquals("15", ev.evaluate().toString(), "10+5");
        } catch (SaxonApiException | IOException e) {
            fail(e.getMessage());
        }
    }


    /**
     * Show a direct call from the Java application to a function defined in the
     * Query. This is a very efficient way of invoking a query
     */

    @Test
    public void testDirectFunction() {
        try {
            final Processor processor = new Processor(true);
            final XQueryCompiler qc = processor.newXQueryCompiler();
            final XQueryExecutable exp1 = qc.compile("declare namespace f='f.ns';" +
                                                             "declare function f:t1($v1 as xs:integer, $v2 as xs:untypedAtomic*) { " +
                                                             "   $v1 div $v2" +
                                                             "};" +
                                                             "10");
            final XQueryEvaluator ev = exp1.load();
            XdmValue v1 = new XdmAtomicValue(10);
            XdmValue v2 = new XdmAtomicValue("5", ItemType.UNTYPED_ATOMIC);
            XdmValue result = ev.callFunction(new QName("f.ns", "f:t1"), v1, v2);
            assertTrue(result instanceof XdmAtomicValue, "isAtomic");
            assertEquals("2", result.toString(), "equals2");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    /**
     * Show a direct call from the Java application to a function defined in the
     * Query. This is a very efficient way of invoking a query
     */

    @Test
    public void testDirectFunctionWithGlobalVar() {
        try {
            final Processor processor = new Processor(true);
            final XQueryCompiler qc = processor.newXQueryCompiler();
            final XQueryExecutable exp1 = qc.compile("declare namespace f='f.ns';" +
                                                             "declare variable $f:value := 89;" +
                                                             "declare function f:t1($v1 as xs:integer, $v2 as xs:untypedAtomic*) { " +
                                                             "   $v1 div $v2 + $f:value" +
                                                             "};" +
                                                             "10");
            final XQueryEvaluator ev = exp1.load();
            XdmValue v1 = new XdmAtomicValue(10);
            XdmValue v2 = new XdmAtomicValue("5", ItemType.UNTYPED_ATOMIC);
            XdmValue result = ev.callFunction(new QName("f.ns", "f:t1"), v1, v2);
            assertTrue(result instanceof XdmAtomicValue, "isAtomic");
            assertEquals("91", result.toString(), "equals2");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    /**
     * Show a direct call from the Java application to a function defined in the
     * Query, with mismatched arguments
     */

    @Test
    public void testDirectFunctionArgConversion() {
        try {
            final Processor processor = new Processor(true);
            final XQueryCompiler qc = processor.newXQueryCompiler();
            final XQueryExecutable exp1 = qc.compile("declare namespace f='f.ns';" +
                                                             "declare function f:t1($v1 as xs:double, $v2 as xs:double) { " +
                                                             "   $v1 div $v2" +
                                                             "};" +
                                                             "10");
            final XQueryEvaluator ev = exp1.load();
            XdmValue v1 = new XdmAtomicValue(10);
            XdmValue v2 = new XdmAtomicValue("5e0", ItemType.UNTYPED_ATOMIC);
            XdmValue result = ev.callFunction(new QName("f.ns", "f:t1"), v1, v2);
            assertTrue(result instanceof XdmAtomicValue, "isAtomic");
            assertEquals("double", ((XdmAtomicValue) result).getPrimitiveTypeName().getLocalName());
            assertEquals("2", result.toString(), "equals2");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            assertTrue(e.getMessage().contains("does not match the required type"), "type-error");
        }
    }

    /**
     * Show a direct call from the Java application to a function defined in the
     * Query, with mismatched arguments
     */

    @Test
    public void testDirectFunctionTypeError() {
        try {
            final Processor processor = new Processor(true);
            final XQueryCompiler qc = processor.newXQueryCompiler();
            final XQueryExecutable exp1 = qc.compile("declare namespace f='f.ns';" +
                                                             "declare function f:t1($v1 as xs:integer, $v2 as xs:untypedAtomic*) { " +
                                                             "   $v1 div $v2" +
                                                             "};" +
                                                             "10");
            final XQueryEvaluator ev = exp1.load();
            XdmValue v1 = new XdmAtomicValue(10);
            XdmValue v2 = new XdmAtomicValue("banana");
            ev.callFunction(new QName("f.ns", "f:t1"), v1, v2);
            fail("Failed to throw error"); // expect failure
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            assertTrue(e.getMessage().contains("Failed to convert the second argument"), "type-error");
        }
    }

    /**
     * Show a direct call from the Java application to a function defined in the
     * Query. Use global variable and global parameter, do two calls that share the global variable computation
     */

    @Test
    public void testDirectFunctionRepeatedCalls() {
        try {
            final Processor processor = new Processor(true);
            final XQueryCompiler qc = processor.newXQueryCompiler();
            final XQueryExecutable exp1 = qc.compile("declare namespace f='f.ns';" +
                                                             "declare variable $z := 1 + xs:integer(doc-available('file:///" + configTest.getDataDir() + "books.xml'));" +
                                                             "declare variable $p as xs:integer external;" +
                                                             "declare function f:t1($v1 as xs:integer) { " +
                                                             "   $v1 div $z + $p" +
                                                             "};" +
                                                             "10");
            final XQueryEvaluator ev = exp1.load();
            ev.setExternalVariable(new QName("p"), new XdmAtomicValue(39));
            XdmValue v1 = new XdmAtomicValue(10);
            XdmValue result = ev.callFunction(new QName("f.ns", "f:t1"), v1);
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals("44", result.toString());
            v1 = new XdmAtomicValue(20);
            result = ev.callFunction(new QName("f.ns", "f:t1"), v1);
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals("49", result.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testDirectFunctionInLibraryModule() {
        try {
            final Processor processor = new Processor(true);
            processor.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
            processor.setConfigurationProperty(Feature.OPTIMIZATION_LEVEL, "0");
            final XQueryCompiler qc = processor.newXQueryCompiler();
            qc.setBaseURI(new File(".").toURI());
            qc.compileLibrary(
                    "module namespace lib = 'http://example.com/library';" +
                            "declare variable $lib:bang external := '!';" +
                            "declare function lib:foo($a) { lib:bar($a) };" +
                            "declare function lib:bar($b) { concat($b, $lib:bang) };"
            );

            XQueryExecutable exp = qc.compile(
                    "import module namespace lib = 'http://example.com/library';()");


            final XQueryEvaluator ev = exp.load();
            XdmValue v1 = new XdmAtomicValue("Hello");
            QName foo = new QName("http://example.com/library", "foo");
            XdmValue result = ev.callFunction(foo, v1);
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals("Hello!", result.toString());
            v1 = new XdmAtomicValue("Goodbye");
            ev.setExternalVariable(new QName("http://example.com/library", "bang"), new XdmAtomicValue("?"));
            result = ev.callFunction(foo, v1);
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals("Goodbye?", result.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testDirectFunctionInLibraryModule2() {
        try {
            final Processor processor = new Processor(true);
            final XQueryCompiler qc = processor.newXQueryCompiler();
            qc.setBaseURI(new File(".").toURI());
            qc.compileLibrary(
                    "module namespace lib = 'http://example.com/library';" +
                            "declare variable $lib:bang := '!';" +
                            "declare function lib:foo() { lib:bar() };" +
                            "declare function lib:bar() { " +
                            "   let $a := parse-xml('<xml><ABC>azaz</ABC></xml>')  " +
                            "   return upper-case($a//xml/ABC) };"
            );

            XQueryExecutable exp = qc.compile(
                    "import module namespace lib = 'http://example.com/library';()");


            final XQueryEvaluator ev = exp.load();

            QName foo = new QName("http://example.com/library", "foo");
            XdmValue result = ev.callFunction(foo);
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals("AZAZ", ((XdmAtomicValue)result).getStringValue());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testTraceListener() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.COMPILE_WITH_TRACING, true);
            XQueryCompiler comp = proc.newXQueryCompiler();
            String qq = "declare variable $TestVariable := 7.5;\n" +
                    "<demo_using_math>\n" +
                    "    {$TestVariable + 1}\n" +
                    "</demo_using_math>";
            XQueryEvaluator eval = comp.compile(qq).load();
            eval.setTraceListener(new TestXslt30TransformerDebug.MyTraceListener());
            XdmValue result = eval.evaluateSingle();
            assertTrue(result.toString().contains("8.5"));

        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    @Test
    public void testFlworTracing() {
        try {
            String doc = "<a xmlns='http://a/'><b>1</b><b>2</b><b>3</b><b>4</b></a>";
            String q = "declare namespace p='http://a/';\n" +
                    "for $i in //p:b \n" +
                    "where $i mod 2 = 0 \n" +
                    "order by -$i \n" +
                    "return $i";
            Processor p = new Processor(false);
            p.setConfigurationProperty(Feature.OPTIMIZATION_LEVEL, "");
            XQueryCompiler comp = p.newXQueryCompiler();
            comp.setCompileWithTracing(true);
            XQueryExecutable exec = comp.compile(q);
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(p.newDocumentBuilder().build(new StreamSource(new StringReader(doc))));
            eval.setTraceListener(new TraceListener() {
                int i = 0;

                @Override
                public void setOutputDestination(Logger stream) {

                }

                @Override
                public void open(Controller controller) {
                    // no action
                }

                @Override
                public void close() {
                    // no action
                }

                @Override
                public void enter(Traceable instruction, Map<String, Object> properties, XPathContext context) {
                    if (!(instruction instanceof TraceExpression)) {
                        System.err.println(i + ": @" + instruction.getLocation().getLineNumber() + " " +
                                                   (instruction instanceof ClauseInfo ? ((ClauseInfo) instruction).getClause().getClauseKey() : instruction.getClass()));
                        switch (i++) {
                            case 0:
                                assertTrue(instruction instanceof XQueryExpression, "isXQueryExpression");
                                break;
                            case 1:
                            case 2:
                            case 4:
                            case 5:
                                assertTrue(instruction instanceof ClauseInfo, "isClauseInfo" + i);
                                assertTrue(((ClauseInfo) instruction).getClause() instanceof ForClause, "isForClause" + i);
                                assertEquals("$i", properties.get("var"), "varName" + i);
                                assertEquals(NamespaceUri.of("http://a/"),
                                             ((ClauseInfo) instruction).getNamespaceResolver().getURIForPrefix("p", false), "rightPrefix0");
                                assertEquals(2, instruction.getLocation().getLineNumber(), "lineNumber" + i);
                                break;
                            case 3:
                            case 6:
                                assertTrue(instruction instanceof ClauseInfo, "isClauseInfo" + i);
                                assertTrue(((ClauseInfo) instruction).getClause() instanceof WhereClause, "isWhereClause" + i);
                                assertTrue(properties.containsKey("condition"), "condition" + i);
                                assertEquals(NamespaceUri.of("http://a/"),
                                             ((ClauseInfo) instruction).getNamespaceResolver().getURIForPrefix("p", false), "rightPrefix1");
                                assertEquals(3, instruction.getLocation().getLineNumber(), "lineNumber" + i);
                                break;
                            case 7:
                            case 8:
                                assertTrue(instruction instanceof ClauseInfo, "isClauseInfo" + i);
                                assertEquals(NamespaceUri.of("http://a/"),
                                             ((ClauseInfo) instruction).getNamespaceResolver().getURIForPrefix("p", false), "rightPrefix4");
                                assertTrue(((ClauseInfo) instruction).getClause() instanceof OrderByClause, "isOrderBy" + i);
                                assertEquals(4, instruction.getLocation().getLineNumber(), "lineNumber" + i);
                                break;
                            default:
                        }
                    }
                }
                @Override
                public void leave(Traceable instruction) {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

                @Override
                public void startCurrentItem(Item currentItem) {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

                @Override
                public void endCurrentItem(Item currentItem) {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

                /**
                 * Method called when a rule search has completed.
                 *
                 * @param rule the rule (or possible built-in ruleset) that has been selected
                 * @param mode the mode in operation
                 */
                @SuppressWarnings("EmptyMethod")
                public void endRuleSearch(Object rule, SimpleMode mode) {
                    //
                }

                /**
                 * Method called when a search for a template rule is about to start
                 */
                @Override
                public void startRuleSearch() {
                    //
                }
            });
            eval.setDestination(new SAXDestination(new XMLFilterImpl()));
            eval.run();

        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }

    @Test
    public void testFunctionTracing() {
        try {
            String doc = "<a><b>1</b><b>2</b><b>3</b><b>4</b></a>";
            String q = "declare function local:x($arg) {let $x := $arg/* return count($x) + count($arg)};\n" +
                    "for $el in //* \n" +
                    "return local:x($el)";
            Processor p = new Processor(false);
            p.setConfigurationProperty(Feature.OPTIMIZATION_LEVEL, "");
            XQueryCompiler comp = p.newXQueryCompiler();
            comp.setCompileWithTracing(true);
            XQueryExecutable exec = comp.compile(q);
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(p.newDocumentBuilder().build(new StreamSource(new StringReader(doc))));
            eval.setTraceListener(new TraceListener() {
                int i = 0;

                @Override
                public void setOutputDestination(Logger stream) {

                }

                @Override
                public void open(Controller controller) {
                    // no action
                }

                @Override
                public void close() {
                    // no action
                }

                @Override
                public void enter(Traceable instruction, Map<String, Object> properties, XPathContext context) {

                    // Note that this expected output assumes the function call will be evaluated in push mode.
                    // With pull mode, there is an extra "pull" on the LET clause - a second call on next() that always
                    // returns null, because the function always returns a singleton.

                    System.err.println(i + ": @" + instruction.getLocation().getLineNumber() + " " +
                                               (instruction instanceof ClauseInfo ? ((ClauseInfo) instruction).getClause().getClauseKey() + " " + properties.get("var") : instruction.getClass()));
                    switch (i++) {
                        case 0:
                            assertTrue(instruction instanceof XQueryExpression, "isXQueryExpression");
                            break;
                        case 1:
                        case 4:
                        case 7:
                            assertTrue(instruction instanceof ClauseInfo, "isClauseInfo" + i);
                            assertTrue(((ClauseInfo) instruction).getClause() instanceof ForClause, "isForClause" + i);
                            assertEquals("$el", properties.get("var"), "varName" + i);
                            assertEquals(2, instruction.getLocation().getLineNumber(), "lineNumber" + i);
                            break;
                        case 2:
                        case 5:
                        case 8:
                            assertTrue(instruction instanceof UserFunction);
                            assertEquals(1, instruction.getLocation().getLineNumber());
                            assertEquals("local:x", properties.get("name").toString(), "functionName" + i);
                            assertEquals(1, properties.get("arity"), "functionArity" + i);
                            break;
                        case 3:
                        case 6:
                        case 9:
                            assertTrue(instruction instanceof ClauseInfo, "isClauseInfo" + i);
                            assertTrue(((ClauseInfo) instruction).getClause() instanceof LetClause, "isLetClause" + i);
                            assertEquals("$x", properties.get("var"), "varName" + i);
                            assertEquals(1, instruction.getLocation().getLineNumber(), "lineNumber" + i);
                            break;

                        default:
                    }

                }

                @Override
                public void leave(Traceable instruction) {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

                @Override
                public void startCurrentItem(Item currentItem) {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

                @Override
                public void endCurrentItem(Item currentItem) {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

                /**
                 * Method called when a rule search has completed.
                 *
                 * @param rule the rule (or possible built-in ruleset) that has been selected
                 * @param mode the mode in operation
                 */
                @SuppressWarnings("EmptyMethod")
                public void endRuleSearch(Object rule, SimpleMode mode) {
                    //
                }

                /**
                 * Method called when a search for a template rule is about to start
                 */
                @Override
                public void startRuleSearch() {
                    //
                }
            });
            eval.setDestination(new SAXDestination(new XMLFilterImpl()));
            eval.run();

        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }

    @Test
    public void testConstructionTracing() {
        try {
            String q = "<out>{\nfor $i in 1 to 10 \nreturn \n<in att='{$i}'>{$i+2}\n</in>}\n</out>";
            Processor p = new Processor(false);
            p.setConfigurationProperty(Feature.OPTIMIZATION_LEVEL, "");
            XQueryCompiler comp = p.newXQueryCompiler();
            comp.setCompileWithTracing(true);
            XQueryExecutable exec = comp.compile(q);
            XQueryEvaluator eval = exec.load();
            eval.setTraceListener(new TraceListener() {
                int i = 0;

                @Override
                public void setOutputDestination(Logger stream) {

                }

                @Override
                public void open(Controller controller) {
                    // no action
                }

                @Override
                public void close() {
                    // no action
                }

                @Override
                public void enter(Traceable instruction, Map<String, Object> properties, XPathContext context) {

                    System.err.println(i + ": @" + instruction.getLocation().getLineNumber() + " " +
                                               (instruction instanceof ClauseInfo ? ((ClauseInfo) instruction).getClause().getClauseKey() : instruction.getClass()));
                    switch (i++) {
                        case 0:
                            assertTrue(instruction instanceof XQueryExpression, "isXQueryExpression");
                            break;
                        case 1:
                            assertTrue(instruction instanceof FixedElement);
                            assertEquals("out", properties.get("name").toString());
                            break;
                        case 2:
                        case 5:
                        case 8:
                            assertTrue(instruction instanceof ClauseInfo);
                            assertTrue(((ClauseInfo)instruction).getClause() instanceof ForClause);
                            assertEquals("$i", properties.get("var").toString());
                            break;
                        case 3:
                        case 6:
                        case 9:
                            assertTrue(instruction instanceof FixedElement);
                            assertEquals("in", properties.get("name").toString());
                            break;
                        case 4:
                        case 7:
                        case 10:
                            assertTrue(instruction instanceof FixedAttribute);
                            assertTrue(properties.get("name").toString().equals("att"));
//                            assertTrue(instruction instanceof UserFunction);
//                            assertEquals(1, instruction.getLocation().getLineNumber());
//                            assertEquals("local:x", properties.get("name").toString(), "functionName" + i);
//                            assertEquals(1, properties.get("arity"), "functionArity" + i);
                            break;


                        default:
                    }

                }

                @Override
                public void leave(Traceable instruction) {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

                @Override
                public void startCurrentItem(Item currentItem) {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

                @Override
                public void endCurrentItem(Item currentItem) {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

                /**
                 * Method called when a rule search has completed.
                 *
                 * @param rule the rule (or possible built-in ruleset) that has been selected
                 * @param mode the mode in operation
                 */
                @SuppressWarnings("EmptyMethod")
                public void endRuleSearch(Object rule, SimpleMode mode) {
                    //
                }

                /**
                 * Method called when a search for a template rule is about to start
                 */
                @Override
                public void startRuleSearch() {
                    //
                }
            });
            eval.setDestination(new SAXDestination(new XMLFilterImpl()));
            eval.run();

        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStreaming() {
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
            XQueryCompiler c = p.newXQueryCompiler();
            c.setStreaming(true);
            XQueryEvaluator eval = c.compile("round(avg(//QUANTITY/data()), 2)").load();
            StringWriter sw = new StringWriter();
            Serializer out = p.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            StreamSource in = new StreamSource(new File(configTest.getDataDir() + "books.xml"));
            eval.runStreamed(in, out);
            assertEquals("128.67", sw.toString());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testStreamingFailure() {
        try {
            Processor p = new Processor(true);
            XQueryCompiler c = p.newXQueryCompiler();
            c.setStreaming(true);
            XQueryEvaluator eval = c.compile("round(avg(descendant::ITEM/(QUANTITY*PRICE)), 2)").load();
            StringWriter sw = new StringWriter();
            Serializer out = p.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            StreamSource in = new StreamSource(new File(configTest.getDataDir() + "books.xml"));
            eval.runStreamed(in, out);
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            assertEquals("XTSE3430", e.getErrorCode().getLocalName());
        }
    }

    @Test
    public void testXmlParseTryCatch() {
        // Bug 2896: try to prevent reporting parse errors to the standard error output if the error
        // then gets caught.
        try {
            String query = "xquery version \"3.0\";\n" +
                    "declare namespace output = \"http://www.w3.org/2010/xslt-xquery-serialization\";\n" +
                    "declare option output:omit-xml-declaration   \"true\";\n" +
                    "\n" +
                    "try {\n" +
                    "   let $xml-string := '&lt;para&gt;Paragraph&lt;/p&gt;'\n" +
                    "   return parse-xml($xml-string)\n" +
                    "}\n" +
                    "catch * {\n" +
                    "   <error code='{$err:code}' desc='{$err:description}'>\n" +
                    "    {for $o in $err:value return <o>{$o}</o>}</error>\n" +
                    "}";
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable q = c.compile(query);
            XdmValue result = q.load().evaluateSingle();
            System.err.println(result.toString());
            assertTrue(result.toString().contains("code=\"err:FODC0006\""));
            assertTrue(result.toString().contains("The element type &#34;para&#34; must " +
                                                          "be terminated by the matching end-tag &#34;&lt;/para&gt;&#34;"));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }

    }

    @Test
    public void testContextItemExternalWithDefault() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo/>"))).select(child()).asNode();
            XQueryCompiler c = p.newXQueryCompiler();

            XQueryExecutable exec = c.compile("declare context item as node() external := <bar/>; .");

            // First run with supplied arg
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(foo);
            XdmItem result = eval.evaluateSingle();
            assertTrue(result instanceof XdmNode);
            assertEquals("foo", ((XdmNode) result).getNodeName().getLocalName());

            // Now with no supplied arg
            eval = exec.load();
            result = eval.evaluateSingle();
            assertTrue(result instanceof XdmNode);
            assertEquals("bar", ((XdmNode) result).getNodeName().getLocalName());

            // Now with arg of wrong type
            try {
                eval = exec.load();
                eval.setContextItem(new XdmAtomicValue(23));
                eval.evaluateSingle();
                fail();
            } catch (SaxonApiException e) {
                assertEquals("XPTY0004", e.getErrorCode().getLocalName());
            }

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testContextItemInternalWithDefault() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo/>"))).select(child()).asNode();
            XQueryCompiler c = p.newXQueryCompiler();

            XQueryExecutable exec = c.compile("declare context item as node() := <bar/>; .");

            // First run with supplied arg
            XQueryEvaluator eval;
            XdmItem result;
            try {
                eval = exec.load();
                eval.setContextItem(foo);
                eval.evaluateSingle();
                fail();
            } catch (SaxonApiException e) {
                // expected failure
                System.err.println(e.getMessage());
            }

            // Now with no supplied arg
            eval = exec.load();
            result = eval.evaluateSingle();
            assertTrue(result instanceof XdmNode);
            assertEquals("bar", ((XdmNode) result).getNodeName().getLocalName());

            // Now with arg of wrong type
            try {
                eval = exec.load();
                eval.setContextItem(new XdmAtomicValue(23));
                eval.evaluateSingle();
                fail();
            } catch (SaxonApiException e) {
                // expected failure
                System.err.println(e.getMessage());
            }

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testContextItemExternalWithoutDefault() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo/>"))).select(child()).asNode();
            XQueryCompiler c = p.newXQueryCompiler();

            XQueryExecutable exec = c.compile("declare context item as node() external; .");

            // First run with supplied arg
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(foo);
            XdmItem result = eval.evaluateSingle();
            assertTrue(result instanceof XdmNode);
            assertEquals("foo", ((XdmNode) result).getNodeName().getLocalName());

            // Now with no supplied arg
            try {
                eval = exec.load();
                eval.evaluateSingle();
                fail();
            } catch (SaxonApiException e) {
                // expected failure
                assertEquals("XPDY0002", e.getErrorCode().getLocalName());
                //System.err.println(e.getMessage());
            }

            // Now with arg of wrong type
            try {
                eval = exec.load();
                eval.setContextItem(new XdmAtomicValue(23));
                eval.evaluateSingle();
                fail();
            } catch (SaxonApiException e) {
                assertEquals("XPTY0004", e.getErrorCode().getLocalName());
            }

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testContextItemInternalWithoutDefault() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            c.compile("declare context item as node(); .");
            fail();
        } catch (SaxonApiException e) {
            assertEquals("XPST0003", e.getErrorCode().getLocalName());
        }

    }

    @Test
    public void testMapItemType() {
        // bug 4490
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();

            XQueryExecutable exec = c.compile("declare namespace map = \"http://www.w3.org/2005/xpath-functions/map\";\n"
                                                      + "map:merge((\n"
                                                      + "    map:entry(attribute one {\"one\"}, 1),\n"
                                                      + "    map:entry(\"two\", 2)\n"
                                                      + "))");
            XdmValue result = exec.load().evaluate();
            assertTrue(result instanceof XdmMap);
            net.sf.saxon.type.ItemType type = Type.getItemType(result.itemAt(0).getUnderlyingValue(), null);
            assertTrue(type instanceof MapType);
        } catch (SaxonApiException e) {
            fail();
        }

    }

    @Test
    public void testLocalURIResolver() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("doc('file:///a.xml')/z").load();
            eval.setResourceResolver(request -> new StreamSource(new StringReader("<z>17</z>")));

            XdmItem result = eval.iterator().next();
            assertEquals("17", result.getStringValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testGlobalURIResolver() {
        try {
            Processor p = new Processor(false);
            p.getUnderlyingConfiguration().setResourceResolver(request -> new StreamSource(new StringReader("<z>17</z>")));
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("doc('file:///a.xml')/z").load();
            XdmItem result = eval.iterator().next();
            assertEquals("17", result.getStringValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


}
