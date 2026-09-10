package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.dom.DOMNodeWrapper;
import net.sf.saxon.dom.DocumentWrapper;
import net.sf.saxon.expr.Literal;
import net.sf.saxon.expr.instruct.GlobalVariable;
import net.sf.saxon.lib.Feature;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.query.DynamicQueryContext;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Test;
import org.w3c.dom.Document;
import org.w3c.dom.Element;

import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.parsers.ParserConfigurationException;
import javax.xml.transform.ErrorListener;
import javax.xml.transform.TransformerException;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.io.StringWriter;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests for the s9api XQueryCompiler
 */
public class TestXQueryCompiler {
    private final ConfigTest configTest = new ConfigTest();
    
    @Test
    public void testModuleUriResolverNoBaseUri() {
        try {
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            c.setModuleURIResolver((moduleURI, baseURI, locations) -> {
                String module = "";
                if (moduleURI.equals("b.uri")) {
                    module = "module namespace b='b.uri'; import module namespace c = 'c.uri' at 'c.xq'; " +
                            "declare variable $b:b := $c:c;";
                } else if (moduleURI.equals("c.uri")) {
                    module = "module namespace c='c.uri';" +
                            "declare variable $c:c := 42;";
                }
                StringReader sr = new StringReader(module);
                StreamSource ss = new StreamSource();
                ss.setReader(sr);   // don't set the base URI
                return new StreamSource[]{ss};
            });
            XQueryExecutable exec = c.compile("import module namespace b = 'b.uri' at 'b.xq'; $b:b");
            XdmValue val = exec.load().evaluate();
            assertEquals(42L, ((XdmAtomicValue) val).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testDefaultNamespace() {
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo xmlns='http://one.uri/'><bar/></foo>")));
            XQueryCompiler c = p.newXQueryCompiler();
            c.declareNamespace("", "http://one.uri/");
            XQueryEvaluator eval = c.compile("/foo").load();
            eval.setContextItem(foo);
            XdmValue val = eval.evaluate();
            assertEquals(1, val.size());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }

    }

    @Test
    public void testReusability() {
        // Test that the XQuery compiler can compile two queries without interference
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader(
                    "<foo xmlns='http://one.uri/'><bar xmlns='http://two.uri'>12</bar></foo>")));
            XQueryCompiler c = p.newXQueryCompiler();
            c.declareNamespace("", "http://one.uri/");
            XQueryExecutable q1 = c.compile("declare variable $p as xs:boolean external; exists(/foo) = $p");
            c.declareNamespace("", "http://two.uri");
            XQueryExecutable q2 = c.compile("declare variable $p as xs:integer external; /*/bar + $p");
            XQueryEvaluator e1 = q1.load();
            XQueryEvaluator e2 = q2.load();
            e1.setContextItem(foo);
            e1.setExternalVariable(new QName("p"), new XdmAtomicValue(true));
            XdmValue val = e1.evaluate();
            assertTrue(((XdmAtomicValue) val).getBooleanValue());
            e2.setContextItem(foo);
            e2.setExternalVariable(new QName("p"), new XdmAtomicValue(6));
            val = e2.evaluate();
            assertEquals(18, ((XdmAtomicValue) val).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }

    }

    @Test
    public void testMultithreading() {
        // Test that the XQuery compiler can compile many queries concurrently without interference
        try {
            Processor p = new Processor(false);
            DocumentBuilder b = p.newDocumentBuilder();
            final XdmNode foo = b.build(new StreamSource(new StringReader(
                    "<foo xmlns='http://one.uri/'><bar xmlns='http://two.uri/'>12</bar></foo>")));
            final XQueryCompiler c = p.newXQueryCompiler();
            c.declareNamespace("a", "http://one.uri/");
            c.declareNamespace("b", "http://two.uri/");
            Runnable task = () -> {
                try {
                    XQueryEvaluator q = c.compile("a:foo/b:bar + a:foo/b:bar + a:foo/b:bar + a:foo/b:bar").load();
                    q.setContextItem(foo);
                    XdmAtomicValue v = (XdmAtomicValue)q.evaluateSingle();
                    assertEquals("48", v.toString(), "ok");
                } catch (Exception e) {
                    e.printStackTrace();
                    fail();
                }
            };
            for (int i = 0; i < 100; i++) {
                task.run();
            }
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }

    }



    @Test
    public void testSeparateCompilation() {
        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.compileLibrary("module namespace m='http://www.example.com/module-one'; " +
                    "declare function m:add($a as xs:integer, $b as xs:integer) as xs:integer { $a + $b };" +
                    "declare variable $m:ten as xs:integer := 10;");
            String q1 = "import module namespace m='http://www.example.com/module-one'; m:add(2,2)";
            XQueryEvaluator ev = comp.compile(q1).load();
            assertEquals("4", ev.evaluate().toString(), "2+2");
            String q2 = "import module namespace m='http://www.example.com/module-one'; m:add($m:ten,5)";
            ev = comp.compile(q2).load();
            assertEquals("15", ev.evaluate().toString(), "10+5");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSeparateCompilationNested() {
        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.compileLibrary(new StringReader("module namespace m1='http://www.example.com/module-one'; " +
                    "declare function m1:add($a as xs:integer, $b as xs:integer) as xs:integer { $a + $b };" +
                    "declare variable $m1:ten as xs:integer := 10;"));
            comp.compileLibrary("module namespace m2='http://www.example.com/module-two'; " +
                    "import module namespace m1='http://www.example.com/module-one';" +
                    "declare function m2:add($a as xs:integer, $b as xs:integer) as xs:integer { m1:add($a, $b) };" +
                    "declare variable $m2:ten as xs:integer := $m1:ten;");
            String q1 = "import module namespace m2='http://www.example.com/module-two'; m2:add($m2:ten,2)";
            XQueryEvaluator ev = comp.compile(q1).load();
            assertEquals("12", ev.evaluate().toString(), "10+2");
            String q2 = "import module namespace m1='http://www.example.com/module-one'; m1:add($m1:ten,5)";
            ev = comp.compile(q2).load();
            assertEquals("15", ev.evaluate().toString(), "10+5");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSeparateCompilationImportFromNonSeparatelyCompiledLibraryModule() {
        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();
//            try {
//                comp.setBaseURI(new URI("http://www.example.com/module-one"));
//            } catch (URISyntaxException e) {
//                //
//            }
            comp.compileLibrary(new StringReader("module namespace m1='http://www.example.com/module-one'; " +
                    "declare function m1:add($a as xs:integer, $b as xs:integer) as xs:integer { $a + $b };" +
                    "declare variable $m1:ten as xs:integer := 10;"));
            final String m2 = "module namespace m2='http://www.example.com/module-two'; " +
                    "import module namespace m1='http://www.example.com/module-one';" +
                    "declare function m2:add($a as xs:integer, $b as xs:integer) as xs:integer { m1:add($a, $b) };" +
                    "declare variable $m2:ten as xs:integer := $m1:ten;";
            String q1 = "import module namespace m2='http://www.example.com/module-two'; m2:add($m2:ten,2)";
            comp.setModuleURIResolver((moduleURI, baseURI, locations) -> {
                if (moduleURI.equals("http://www.example.com/module-two")) {
                    return new StreamSource[]{new StreamSource(new StringReader(m2), "http://www.example.com/module-two")};
                } else {
                    return null;
                }
            });
            XQueryEvaluator ev = comp.compile(q1).load();
            assertEquals("12", ev.evaluate().toString(), "10+2");
            String q2 = "import module namespace m1='http://www.example.com/module-one'; m1:add($m1:ten,5)";
            ev = comp.compile(q2).load();
            assertEquals("15", ev.evaluate().toString(), "10+5");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSeparateCompilationDiamond() {
        try {
            Processor proc = new Processor(true);
            proc.setConfigurationProperty(Feature.GENERATE_BYTE_CODE, false);
            XQueryCompiler comp = proc.newXQueryCompiler();

            try {
                comp.setBaseURI(new URI("http://www.example.com/module-one"));
            } catch (URISyntaxException e) {
                //
            }
            comp.compileLibrary(new StringReader("module namespace m1='http://www.example.com/module-one'; " +
                    "declare function m1:add($a as xs:integer, $b as xs:integer) as xs:integer { $a + $b };"));
            try {
                comp.setBaseURI(new URI("http://www.example.com/module-two"));
            } catch (URISyntaxException e) {
                //
            }
            comp.compileLibrary("module namespace m2='http://www.example.com/module-two'; " +
                    "import module namespace m1='http://www.example.com/module-one';" +
                    "declare function m2:subtract($a as xs:integer, $b as xs:integer) as xs:integer { m1:add($a, -$b) };");
            try {
                comp.setBaseURI(new URI("http://www.example.com/module-three"));
            } catch (URISyntaxException e) {
                //
            }
            comp.compileLibrary("module namespace m3='http://www.example.com/module-three'; " +
                    "import module namespace m1='http://www.example.com/module-one';" +
                    "declare function m3:times($a as xs:integer, $b as xs:integer) as xs:integer { " +
                    "   if ($b=1) then $a else m1:add($a, m3:times($a, $b - 1)) };");
            String q1 = "import module namespace m2='http://www.example.com/module-two'; " +
                    "import module namespace m3='http://www.example.com/module-three';" +
                    "m2:subtract(m3:times(4,2),1)";
            XQueryEvaluator ev = comp.compile(q1).load();
            assertEquals("7", "7", ev.evaluate().toString());
            String q2 = "import module namespace m2='http://www.example.com/module-two'; " +
                    "import module namespace m3='http://www.example.com/module-three';" +
                    "m2:subtract(m3:times(5,2),1)";
            XQueryEvaluator ev2 = comp.compile(q2).load();
            assertEquals("9", "9", ev2.evaluate().toString());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }


    @Test
    public void testSeparateCompilationDiamondWithVariables() {
        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();

            comp.compileLibrary(new StringReader("module namespace m1='http://www.example.com/module-one'; " +
                    "declare function m1:add($a as xs:integer, $b as xs:integer) as xs:integer { $a + $b };" +
                    "declare variable $m1:ten := 10;"
            ));

            comp.compileLibrary("module namespace m2='http://www.example.com/module-two'; " +
                    "import module namespace m1='http://www.example.com/module-one';" +
                    "declare function m2:subtract($a as xs:integer, $b as xs:integer) as xs:integer { m1:add($a, -$b) };" +
                    "declare variable $m2:eleven := $m1:ten + 1;");

            comp.compileLibrary("module namespace m3='http://www.example.com/module-three'; " +
                    "import module namespace m1='http://www.example.com/module-one';" +
                    "declare function m3:times($a as xs:integer, $b as xs:integer) as xs:integer { " +
                    "   if ($b=1) then $a else m1:add($a, m3:times($a, $b - 1)) };" +
                    "declare variable $m3:twelve := $m1:ten + 2;");

            String q1 = "import module namespace m2='http://www.example.com/module-two'; " +
                    "import module namespace m3='http://www.example.com/module-three';" +
                    "m2:subtract(m3:times(4,2),1) + $m3:twelve";
            XQueryEvaluator ev = comp.compile(q1).load();
            assertEquals("19", "19", ev.evaluate().toString());
            String q2 = "import module namespace m2='http://www.example.com/module-two'; " +
                    "import module namespace m3='http://www.example.com/module-three';" +
                    "m2:subtract(m3:times(5,2),1) + $m2:eleven";
            XQueryEvaluator ev2 = comp.compile(q2).load();
            assertEquals("20", "20", ev2.evaluate().toString());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testSeparateCompilationForwardsRef() {
        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.newXQueryCompiler();
            comp.compileLibrary(
                    "module namespace lib = 'http://example.com/library';" +
                            "declare function lib:foo($a) { lib:bar($a) };" +
                            "declare function lib:bar($b) { concat($b, '!') };");

            XQueryExecutable exp = comp.compile(
                    "import module namespace lib = 'http://example.com/library';"
                            + "\nlib:foo('a')"
            );

            XdmItem result = exp.load().evaluateSingle();
            assertEquals("a!", result.getStringValue());

        } catch (SaxonApiException err) {
            fail(err.getMessage());
        }
    }


    /**
     * s9api query/transformation pipeline
     */

    @Test
    public void testPipeline() {
        try {
            Processor proc = new Processor(false);
            XsltCompiler tComp = proc.newXsltCompiler();
            XQueryCompiler qComp = proc.newXQueryCompiler();
            StringWriter sw = new StringWriter();
            Serializer out = proc.newSerializer(sw);
            String xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
                    "<xsl:template match='/'><t><xsl:copy-of select='.'/></t></xsl:template>" +
                    "</xsl:stylesheet>";
            String q = "document { <q>{.}</q> }";
            String xml = "<z/>";
            XdmNode in = proc.newDocumentBuilder().build(new StreamSource(new StringReader(xml)));
            XsltTransformer stage1 = tComp.compile(new StreamSource(new StringReader(xsl))).load();
            XQueryEvaluator stage2 = qComp.compile(q).load();
            XsltTransformer stage3 = tComp.compile(new StreamSource(new StringReader(xsl))).load();
            XQueryEvaluator stage4 = qComp.compile(q).load();
            XsltTransformer stage5 = tComp.compile(new StreamSource(new StringReader(xsl))).load();
            stage1.setDestination(stage2);
            stage2.setDestination(stage3);
            stage3.setDestination(stage4);
            stage4.setDestination(stage5);
            stage5.setDestination(out);
            stage1.setInitialContextNode(in);
            stage1.transform();
            assertTrue(sw.toString().contains("<t><q><t><q><t><z/></t></q></t></q></t>"), "output");

        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }


    @Test
    public void testErrorListener() {
        final List<TransformerException> errors = new ArrayList<>();
        final List<TransformerException> fatalErrors = new ArrayList<>();
        final List<TransformerException> warnings = new ArrayList<>();
        try {
            Processor p = new Processor(true);
            XQueryCompiler c = p.newXQueryCompiler();

            c.setErrorListener(new ErrorListener() {
                @Override
                public void error(TransformerException exception) {
                    errors.add(exception);
                }

                @Override
                public void warning(TransformerException exception) {
                    warnings.add(exception);
                }

                @Override
                public void fatalError(TransformerException exception) {
                    fatalErrors.add(exception);
                }
            });
            XQueryEvaluator eval = c.compile("$x + /undec:y = ").load();
            StringWriter sw = new StringWriter();
            Serializer out = p.newSerializer(sw);
            out.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            StreamSource in = new StreamSource(new File(configTest.getDataDir() + "books.xml"));
            eval.runStreamed(in, out);
            fail("unexpected success");
        } catch (SaxonApiException e) {
            assertEquals(0, errors.size(), "errors");
            assertEquals(1, fatalErrors.size(), "fatalErrors");
            assertEquals(0, warnings.size(), "warnings");
        }
    }


    @Test
    public void testIssue2111() {
        try {
            final Configuration configuration = new Configuration();
            configuration.setConfigurationProperty(Feature.XQUERY_VERSION, "1.0");
            configuration.setModuleURIResolver((moduleURI, baseURI, locations) -> {
                StringReader reader = new StringReader("module namespace x='x'; declare variable $x:x := 'x';");
                StreamSource module = new StreamSource(reader);
                module.setSystemId(moduleURI);
                return new StreamSource[]{module};
            });
            Processor processor = new Processor(configuration);
            XQueryCompiler xqueryCompiler = processor.newXQueryCompiler();
            xqueryCompiler.setBaseURI(new File(".").toURI());
            XQueryExecutable executable = xqueryCompiler.compile("import module namespace x='x'; $x:x\n");
            executable.load().run(processor.newSerializer(System.out));

        } catch (SaxonApiException err) {
            fail();
        }

    }

    @Test
    public void testAbleToCompileWithCompileLibraryMethod() throws SaxonApiException {
        // See bug 2429
        XQueryCompiler compiler = new Processor(true).newXQueryCompiler();
        final String main = "" +
                "module namespace main = \"main_mod.xq\";\n" +
                "import module namespace vi_imp = \"imported_mod.xq\";\n" +
                "declare %private variable $main:variableThatIsInstantiatedTwice as xs:integer := 123;";

        compiler.setModuleURIResolver((moduleURI, baseURI, locations) -> {
            String module = "";
            if (moduleURI.equals("imported_mod.xq")) {
                module =
                        "module namespace vi_imp = \"imported_mod.xq\";\n" +
                                "import module namespace vi_main = \"main_mod.xq\" at \"file:///nowhere/variable_issue_main.xq\";";
            } else if (moduleURI.equals("main_mod.xq")) {
                module = main;
            }
            return new StreamSource[]{new StreamSource(new StringReader(module), moduleURI)};
        });
//        try {
//            compiler.setBaseURI(new URI("file:///nowhere/variable_issue_main.xq"));
//        } catch (URISyntaxException e) {
//            fail(e.getMessage());
//        }
        compiler.compileLibrary(main);
    }

    @Test
    public void testGlobalVariableLineNumber() {
        try {
            String query = "declare namespace c = 'c.uri';\n\n\ndeclare variable $foo := 3;\n$foo";
            Processor p = new Processor(false);
            XQueryCompiler c = p.newXQueryCompiler();
            XQueryExecutable q = c.compile(query);
            Iterator<GlobalVariable> vars = q.getUnderlyingCompiledQuery().getMainModule().getModuleVariables();
            int count = 0;
            while (vars.hasNext()) {
                GlobalVariable v = vars.next();
                assertEquals(4, v.getLineNumber());
                count++;
            }
            assertEquals(1, count);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }

    }


    @Test
    public void testPreEvaluateDocFunction() {
        // Bug 2963
        String badQuery = "doc('http://example.com/bad')";
        String goodQuery = "doc('http://example.com/good')";
        Processor p = new Processor(false);
        XQueryCompiler c = p.newXQueryCompiler();
        final List<String> counter = new ArrayList<>();
        p.getUnderlyingConfiguration().setResourceResolver(request -> {
                switch (request.relativeUri) {
                    case "http://example.com/bad":
                        String badXml = "<bad>";
                        counter.add(request.relativeUri);
                        return new StreamSource(new StringReader(badXml));
                    case "http://example.com/good":
                        String goodXml = "<good/>";
                        counter.add(request.relativeUri);
                        return new StreamSource(new StringReader(goodXml));
                    default:
                        return null;
                }
            });
        p.setConfigurationProperty(Feature.PRE_EVALUATE_DOC_FUNCTION, false);
        XQueryExecutable q = null;
        try {
            q = c.compile(badQuery);
        } catch (Exception err) {
            fail("Compilation should not fail");
            err.printStackTrace();
        }
        assertEquals(0, counter.size(), "URI not accessed");
        p.setConfigurationProperty(Feature.PRE_EVALUATE_DOC_FUNCTION, true);
        try {
            q = c.compile(goodQuery);
            assertTrue(counter.size() > 0, "URI accessed");
            assertTrue(q.getUnderlyingCompiledQuery().getExpression() instanceof Literal, "Compiled to Literal");
            assertTrue(((Literal) q.getUnderlyingCompiledQuery().getExpression()).getGroundedValue().head() instanceof NodeInfo, "Compiled to NodeInfo");
        } catch (Exception err) {
            fail("Compilation should not fail");
        }
        try {
            XdmValue result = q.load().evaluateSingle();
            assertTrue(result instanceof XdmNode);
        } catch (Exception err) {
            fail("Execution should not fail");
            err.printStackTrace();
        }

    }

    private static class MyErrorListener implements ErrorListener {
        public final List<TransformerException> warnings = new ArrayList<>();
        public final List<TransformerException> errors = new ArrayList<>();
        public final List<TransformerException> fatalErrors = new ArrayList<>();

        @Override
        public void warning(TransformerException exception) {
            warnings.add(exception);
        }

        @Override
        public void error(TransformerException exception) {
            errors.add(exception);
        }

        @Override
        public void fatalError(TransformerException exception) {
            fatalErrors.add(exception);
        }
    }

    @Test
    public void testXQueryValidationErrorReporting() {
        // Bug 3123
        MyErrorListener listener = new MyErrorListener();
        try {
            Processor p = new Processor(true);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo xmlns='http://one.uri/'><bar/></foo>")));
            XQueryCompiler c = p.newXQueryCompiler();
            c.declareNamespace("", "http://one.uri/");
            XQueryEvaluator eval = c.compile("validate strict{.}").load();

            eval.setErrorListener(listener);
            eval.setContextItem(foo);
            XdmValue val = eval.evaluate();
            fail();
        } catch (SaxonApiException e) {
            assertEquals(0, listener.errors.size());
            assertTrue(listener.fatalErrors.size() > 0);
        }
    }

    @Test
    public void testDiagnosticsOnAtomizationFailure() {
        // Bug 4086
        try {
            Processor p = new Processor(true);
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo xmlns='http://one.uri/'><bar/></foo>")));
            XQueryCompiler c = p.newXQueryCompiler();
            c.declareNamespace("", "http://one.uri/");
            XQueryEvaluator eval = c.compile("declare function local:x($n as node(), $a as xs:integer){$a+string-length(name($n))}; local:x(<z/>, map{'x':2})").load();
            XdmValue val = eval.evaluate();
            fail();
        } catch (SaxonApiException e) {
            System.err.println(e);
        }
    }

    @Test
    public void testXQueryOptimizationWithTracing() {
        // Bug 3497

        String query = "declare variable $root := <root><text>AAA</text></root>;" +
                "declare function local:foo($root) {$root/text};" +
                "local:foo($root)";

        try {
            Processor p = new Processor(true);
            XQueryCompiler c = p.newXQueryCompiler();
            c.setCompileWithTracing(true);
            XQueryEvaluator eval = c.compile(query).load();
            XdmValue val = eval.evaluate();
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testBug5680() {
        try {
            Configuration c = new EnterpriseConfiguration();

            Processor processor = new Processor(c);
            XQueryCompiler qc = processor.newXQueryCompiler();
            XQueryExecutable exp = qc.compile(
                    "declare function local:f($v as element()?) { " +
                            "   $v/node()" +
                            "};" +
                            "local:f(//inner)");

            XQueryEvaluator ev = exp.load();
            DocumentBuilderFactory docFactory = DocumentBuilderFactory.newInstance();
            docFactory.setNamespaceAware(true);
            javax.xml.parsers.DocumentBuilder docBuilder = docFactory.newDocumentBuilder();
            Document doc = docBuilder.newDocument();
            Element rootElement = doc.createElement("test");
            Element inner = doc.createElement("inner");
            //rootElement.appendChild(inner); //uncomment to pass the test
            doc.appendChild(rootElement);

            DynamicQueryContext dynamicContext = ev.getUnderlyingQueryContext();
            DocumentWrapper docWrapper = new DocumentWrapper(doc, doc.getBaseURI(), dynamicContext.getConfiguration());
            DOMNodeWrapper context = docWrapper.wrap(doc.getDocumentElement());
            dynamicContext.setContextItem(context);

            for (int i = 1; i < 200; i++) //fails on 100 iteration
            {
                System.out.println(i);
                XdmSequenceIterator<XdmItem> result = ev.iterator();
            }
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        } catch (SaxonApiUncheckedException e) {
            e.printStackTrace();
            fail();
        } catch (ParserConfigurationException e) {
            e.printStackTrace();
            fail();
        }
    }


}
