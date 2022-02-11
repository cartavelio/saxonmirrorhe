package s9apitest;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.flwor.ClauseInfo;
import net.sf.saxon.expr.flwor.ForClause;
import net.sf.saxon.expr.flwor.OrderByClause;
import net.sf.saxon.expr.flwor.WhereClause;
import net.sf.saxon.expr.instruct.TraceExpression;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.lib.Logger;
import net.sf.saxon.lib.TraceListener;
import net.sf.saxon.ma.map.MapType;
import net.sf.saxon.om.Item;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trace.InstructionInfo;
import net.sf.saxon.trans.SimpleMode;
import net.sf.saxon.type.Type;
import org.xml.sax.helpers.XMLFilterImpl;

import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.IOException;
import java.io.StringReader;
import java.io.StringWriter;

import static net.sf.saxon.s9api.streams.Steps.child;

/**
 * JUnit tests for the s9api XQueryCompiler
 */
public class TestXQueryEvaluator extends TestCase {

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
            assertSame("Push result is document", doc.getNodeKind(), XdmNodeKind.DOCUMENT);

            // Now run in pull mode
            XdmNode doc2 = (XdmNode) eval.evaluate();
            assertSame("Pull result is document", doc2.getNodeKind(), XdmNodeKind.DOCUMENT);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testXQueryLineNumber() {
        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();

            DocumentBuilder builder = proc.newDocumentBuilder();
            builder.setLineNumbering(true);
            builder.setWhitespaceStrippingPolicy(WhitespaceStrippingPolicy.ALL);
            XdmNode source = builder.build(new File(ConfigTest.DATA_DIR + "books.xml"));

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
            assertEquals("linenumber", "15", sw.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testXdmDestinationMultipleNodes() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("document{<a/>}, document{<b/>}, document{<c/>}").load();

            XdmDestination dest = new XdmDestination();
            eval.run(dest);
            XdmNode doc = dest.getXdmNode();
            assertEquals("kind", doc.getNodeKind(), XdmNodeKind.DOCUMENT);
            assertEquals("child-count", doc.axisIterator(Axis.CHILD).stream().count(), 3);

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testXdmDestinationItemSeparator() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            String q = "declare namespace output = \"http://www.w3.org/2010/xslt-xquery-serialization\";" +
                    "declare option output:method   \"xml\";" +
                    "declare option output:omit-xml-declaration   \"yes\";" +
                    "declare option output:item-separator '~';" +
                    "<!--start-->, 1 to 3, <!--end-->";
            XQueryEvaluator eval = c.compile(q).load();

            XdmDestination dest = new XdmDestination();
            eval.run(dest);
            XdmNode doc = dest.getXdmNode();
            assertEquals("kind", XdmNodeKind.DOCUMENT, doc.getNodeKind());
            assertEquals("child-count", 3, doc.axisIterator(Axis.CHILD).stream().count());
            assertEquals("content", "<!--start-->~1~2~3~<!--end-->", doc.toString());

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testXdmDestinationSpaceSeparator() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            String q = "declare namespace output = \"http://www.w3.org/2010/xslt-xquery-serialization\";" +
                    "declare option output:method   \"xml\";" +
                    "declare option output:omit-xml-declaration   \"yes\";" +
                    "declare option output:item-separator '#absent';" +
                    "<!--start-->, 1 to 3, <!--end-->";
            XQueryEvaluator eval = c.compile(q).load();

            XdmDestination dest = new XdmDestination();
            eval.run(dest);
            XdmNode doc = dest.getXdmNode();
            assertEquals("kind", XdmNodeKind.DOCUMENT, doc.getNodeKind());
            assertEquals("child-count", 3, doc.axisIterator(Axis.CHILD).stream().count());
            assertEquals("content", "<!--start-->1 2 3<!--end-->", doc.toString());

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }


    /**
     * Test writing a text node only to an XdmDestination
     */

    public void testXdmDestinationTextNodeOnly() {
        try {
            Processor processor = new Processor(false);
            XQueryCompiler xpc = processor.newXQueryCompiler();
            XQueryEvaluator s = xpc.compile("text{'abcd'}").load();
            XdmDestination dest = new XdmDestination();
            s.run(dest);
            XdmNode doc = dest.getXdmNode();
            assertEquals("kind", doc.getNodeKind(), XdmNodeKind.DOCUMENT);
            assertEquals("child-count", doc.axisIterator(Axis.CHILD).stream().count(), 1);
            assertEquals("child-kind", doc.axisIterator(Axis.CHILD).next().getNodeKind(), XdmNodeKind.TEXT);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    public void testRawDestinationMultipleDocumentNodes() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("document{<a/>}, document{<b/>}, document{<c/>}").load();

            RawDestination dest = new RawDestination();
            eval.run(dest);
            XdmValue result = dest.getXdmValue();
            assertEquals("kind", ((XdmNode) result.itemAt(0)).getNodeKind(), XdmNodeKind.DOCUMENT);
            assertEquals("count", result.size(), 3);

        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testRawDestinationMultipleElementNodes() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("<a x='1'><y/></a>, <b x='2'><y/></b>, <c x='3'><y/></c>").load();

            RawDestination dest = new RawDestination();
            eval.run(dest);
            XdmValue result = dest.getXdmValue();
            assertEquals("kind", ((XdmNode) result.itemAt(0)).getNodeKind(), XdmNodeKind.ELEMENT);
            assertEquals("att", "2", ((XdmNode) result.itemAt(1)).attribute("x"));
            assertEquals("child", "y", result.itemAt(2).select(child("y")).asNode().getNodeName().getLocalName());
            assertEquals("count", result.size(), 3);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testRawDestinationMultipleAttributeNodes() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryEvaluator eval = c.compile("(1 to 3)!attribute{'a'}{string(.)}").load();

            RawDestination dest = new RawDestination();
            eval.run(dest);
            XdmValue result = dest.getXdmValue();
            assertEquals("kind", ((XdmNode) result.itemAt(0)).getNodeKind(), XdmNodeKind.ATTRIBUTE);
            assertEquals("att", "2", result.itemAt(1).getStringValue());
            assertEquals("child", "a", ((XdmNode) result.itemAt(2)).getNodeName().getLocalName());
            assertEquals("count", result.size(), 3);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }


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


    public void testSeparateCompilationFromFile() {
        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.compileLibrary(new File(ConfigTest.DATA_DIR + "module-one.xq"));
            String q1 = "import module namespace m='http://www.example.com/module-one'; m:add(2,2)";
            XQueryEvaluator ev = comp.compile(q1).load();
            assertEquals("2+2", "4", ev.evaluate().toString());
            String q2 = "import module namespace m='http://www.example.com/module-one'; m:add($m:ten,5)";
            ev = comp.compile(q2).load();
            assertEquals("10+5", "15", ev.evaluate().toString());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        } catch (IOException e) {
            fail(e.getMessage());
        }
    }


    /**
     * Show a direct call from the Java application to a function defined in the
     * Query. This is a very efficient way of invoking a query
     */

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
            XdmValue result = ev.callFunction(new QName("f.ns", "f:t1"), new XdmValue[]{v1, v2});
            assertTrue("isAtomic", result instanceof XdmAtomicValue);
            assertEquals("equals2", "2", result.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    /**
     * Show a direct call from the Java application to a function defined in the
     * Query. This is a very efficient way of invoking a query
     */

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
            XdmValue result = ev.callFunction(new QName("f.ns", "f:t1"), new XdmValue[]{v1, v2});
            assertTrue("isAtomic", result instanceof XdmAtomicValue);
            assertEquals("equals2", "91", result.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }


    /**
     * Show a direct call from the Java application to a function defined in the
     * Query, with mismatched arguments
     */

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
            XdmValue result = ev.callFunction(new QName("f.ns", "f:t1"), new XdmValue[]{v1, v2});
            assertTrue("isAtomic", result instanceof XdmAtomicValue);
            assertEquals("double", ((XdmAtomicValue) result).getPrimitiveTypeName().getLocalName());
            assertEquals("equals2", "2", result.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            assertTrue("type-error", e.getMessage().contains("does not match the required type"));
        }
    }

    /**
     * Show a direct call from the Java application to a function defined in the
     * Query, with mismatched arguments
     */

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
            XdmValue result = ev.callFunction(new QName("f.ns", "f:t1"), new XdmValue[]{v1, v2});
            fail("Failed to throw error"); // expect failure
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            assertTrue("type-error", e.getMessage().contains("required item type of the second argument"));
        }
    }

    /**
     * Show a direct call from the Java application to a function defined in the
     * Query. Use global variable and global parameter, do two calls that share the global variable computation
     */

    public static void testDirectFunctionRepeatedCalls() {
        try {
            final Processor processor = new Processor(true);
            final XQueryCompiler qc = processor.newXQueryCompiler();
            final XQueryExecutable exp1 = qc.compile("declare namespace f='f.ns';" +
                                                             "declare variable $z := 1 + xs:integer(doc-available('file:///" + ConfigTest.DATA_DIR + "books.xml'));" +
                                                             "declare variable $p as xs:integer external;" +
                                                             "declare function f:t1($v1 as xs:integer) { " +
                                                             "   $v1 div $z + $p" +
                                                             "};" +
                                                             "10");
            final XQueryEvaluator ev = exp1.load();
            ev.setExternalVariable(new QName("p"), new XdmAtomicValue(39));
            XdmValue v1 = new XdmAtomicValue(10);
            XdmValue result = ev.callFunction(new QName("f.ns", "f:t1"), new XdmValue[]{v1});
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals("44", result.toString());
            v1 = new XdmAtomicValue(20);
            result = ev.callFunction(new QName("f.ns", "f:t1"), new XdmValue[]{v1});
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals("49", result.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    public static void testDirectFunctionInLibraryModule() {
        // 2013-01-25 - test currently failing.
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
            XdmValue result = ev.callFunction(foo, new XdmValue[]{v1});
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals("Hello!", result.toString());
            v1 = new XdmAtomicValue("Goodbye");
            ev.setExternalVariable(new QName("http://example.com/library", "bang"), new XdmAtomicValue("?"));
            result = ev.callFunction(foo, new XdmValue[]{v1});
            assertTrue(result instanceof XdmAtomicValue);
            assertEquals("Goodbye?", result.toString());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

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
                            "   let $a := <xml><ABC>A</ABC></xml>  " +
                            "   return upper-case($a//xml/ABC) };"
            );

            XQueryExecutable exp = qc.compile(
                    "import module namespace lib = 'http://example.com/library';()");


            final XQueryEvaluator ev = exp.load();

            QName foo = new QName("http://example.com/library", "foo");
            XdmValue result = ev.callFunction(foo, new XdmValue[]{});
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

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

    public void testFlworTracing() {
        try {
            String doc = "<a xmlns='http://a/'><b>1</b><b>2</b><b>3</b><b>4</b></a>";
            String q = "declare namespace p='http://a/';\n" +
                    "for $i in //p:b \n where $i mod 2 = 0 \n order by -$i \n return $i";
            Processor p = new Processor(false);
            XQueryCompiler comp = p.newXQueryCompiler();
            comp.setCompileWithTracing(true);
            XQueryExecutable exec = comp.compile(q);
            XQueryEvaluator eval = exec.load();
            eval.setContextItem(p.newDocumentBuilder().build(new StreamSource(new StringReader(doc))));
            eval.setTraceListener(new TraceListener() {
                int i = 0;

                public void setOutputDestination(Logger stream) {

                }

                public void open(Controller controller) {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

                public void close() {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

                public void enter(InstructionInfo instruction, XPathContext context) {
                    if (!(instruction instanceof TraceExpression)) {
                        switch (i++) {
                            case 0:
                                assertTrue("isClauseInfo0", instruction instanceof ClauseInfo);
                                assertEquals("rightPrefix0", "http://a/", ((ClauseInfo) instruction).getNamespaceResolver().getURIForPrefix("p", false));
                                assertTrue("isForClause0", ((ClauseInfo) instruction).getClause() instanceof ForClause);
                                assertEquals("lineNumber0", 2, instruction.getLineNumber());
                                break;
                            case 1:
                                assertTrue("isClauseInfo1", instruction instanceof ClauseInfo);
                                assertEquals("rightPrefix1", "http://a/", ((ClauseInfo) instruction).getNamespaceResolver().getURIForPrefix("p", false));
                                assertTrue("isWhereClause1", ((ClauseInfo) instruction).getClause() instanceof WhereClause);
                                assertEquals("lineNumber1", 3, instruction.getLineNumber());
                                break;
                            case 4:
                                assertTrue("isClauseInfo4", instruction instanceof ClauseInfo);
                                assertEquals("rightPrefix4", "http://a/", ((ClauseInfo) instruction).getNamespaceResolver().getURIForPrefix("p", false));
                                assertTrue("isOrderBy4", ((ClauseInfo) instruction).getClause() instanceof OrderByClause);
                                assertEquals("lineNumber4", 4, instruction.getLineNumber());
                                break;
                            default:
                        }
                    }
                }

                public void leave(InstructionInfo instruction) {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

                public void startCurrentItem(Item currentItem) {
                    //To change body of implemented methods use File | Settings | File Templates.
                }

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
            StreamSource in = new StreamSource(new File(ConfigTest.DATA_DIR + "books.xml"));
            eval.runStreamed(in, out);
            assertEquals("128.67", sw.toString());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * This was supposed to test that streaming works with bytecode enabled. Actually it only
     * works because byte code generation is suppressed for streamable code.
     */

    public void testStreamingWithByteCode() {
        try {
            Processor p = new Processor(true);
            p.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, true);
            XQueryCompiler c = p.newXQueryCompiler();
            c.setStreaming(true);
            XQueryEvaluator eval = c.compile("round(avg(//QUANTITY/data()), 2)").load();
            StringWriter sw = new StringWriter();
            Serializer out = p.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            StreamSource in = new StreamSource(new File(ConfigTest.DATA_DIR + "books.xml"));
            eval.runStreamed(in, out);
            assertEquals("128.67", sw.toString());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testStreamingFailure() {
        try {
            Processor p = new Processor(true);
            XQueryCompiler c = p.newXQueryCompiler();
            c.setStreaming(true);
            XQueryEvaluator eval = c.compile("round(avg(descendant::ITEM/(QUANTITY*PRICE)), 2)").load();
            StringWriter sw = new StringWriter();
            Serializer out = p.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            StreamSource in = new StreamSource(new File(ConfigTest.DATA_DIR + "books.xml"));
            eval.runStreamed(in, out);
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            assertEquals("XTSE3430", e.getErrorCode().getLocalName());
        }
    }

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
            assertTrue(result.toString().contains("<error code=\"err:FODC0006\""));
            assertTrue(result.toString().contains("The element type &#34;para&#34; must " +
                                                          "be terminated by the matching end-tag &#34;&lt;/para&gt;&#34;"));
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }

    }

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
                result = eval.evaluateSingle();
                fail();
            } catch (SaxonApiException e) {
                assertEquals("XPTY0004", e.getErrorCode().getLocalName());
            }

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testContextItemInternalWithDefault() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo/>"))).select(child()).asNode();
            XQueryCompiler c = p.newXQueryCompiler();

            XQueryExecutable exec = c.compile("declare context item as node() := <bar/>; .");

            // First run with supplied arg
            XQueryEvaluator eval = null;
            XdmItem result = null;
            try {
                eval = exec.load();
                eval.setContextItem(foo);
                result = eval.evaluateSingle();
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
                result = eval.evaluateSingle();
                fail();
            } catch (SaxonApiException e) {
                // expected failure
                System.err.println(e.getMessage());
            }

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

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
                result = eval.evaluateSingle();
                fail();
            } catch (SaxonApiException e) {
                // expected failure
                assertEquals("XPDY0002", e.getErrorCode().getLocalName());
                System.err.println(e.getMessage());
            }

            // Now with arg of wrong type
            try {
                eval = exec.load();
                eval.setContextItem(new XdmAtomicValue(23));
                result = eval.evaluateSingle();
                fail();
            } catch (SaxonApiException e) {
                assertEquals("XPTY0004", e.getErrorCode().getLocalName());
            }

        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    public void testContextItemInternalWithoutDefault() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo/>"))).select(child()).asNode();
            XQueryCompiler c = p.newXQueryCompiler();

            XQueryExecutable exec = c.compile("declare context item as node(); .");
            fail();
        } catch (SaxonApiException e) {
            assertEquals("XPST0003", e.getErrorCode().getLocalName());
        }

    }

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
            net.sf.saxon.type.ItemType type = Type.getItemType(result.itemAt(0).getUnderlyingValue(), null);
            assertTrue(type instanceof MapType);
        } catch (SaxonApiException e) {
            fail();
        }

    }


}