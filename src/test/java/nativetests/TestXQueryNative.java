package nativetests;

import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.ee.optim.StaticQueryContextEE;
import com.saxonica.expr.StaticQueryContextPE;
import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.Expression;
import net.sf.saxon.expr.StaticContext;
import net.sf.saxon.expr.StringLiteral;
import net.sf.saxon.expr.instruct.UserFunction;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.lib.FeatureKeys;
import net.sf.saxon.lib.StaticQueryContextFactory;
import net.sf.saxon.om.*;
import net.sf.saxon.query.DynamicQueryContext;
import net.sf.saxon.query.StaticQueryContext;
import net.sf.saxon.query.XQueryExpression;
import net.sf.saxon.trans.SymbolicName;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.value.AtomicValue;
import net.sf.saxon.value.Int64Value;
import net.sf.saxon.value.IntegerValue;
import net.sf.saxon.value.SequenceType;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.io.StringWriter;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.List;
import java.util.Properties;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotNull;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

// Some tests of the "native" XQuery API

public class TestXQueryNative {
    private static final SequenceType showTheBug = SequenceType.ATOMIC_SEQUENCE;


    private static final String xml =
            "<?xml version='1.0' encoding='UTF-8'?><record><datafield ind1=' '></datafield></record>";

    // Bug report against 9.1: compiling a query against one Configuration and running using another appears to give no error

    @Test
    public void testConfigConsistent() {
        try {
            String query = "/*/*";
            StaticQueryContext sqc = new Configuration().newStaticQueryContext();
            XQueryExpression exp = sqc.compileQuery(query);
            Configuration dc = new Configuration();
            DynamicQueryContext dqc = new DynamicQueryContext(dc);
            dqc.setContextItem(dc.buildDocumentTree(new StreamSource(new StringReader(xml))).getRootNode());
            exp.run(dqc, new StreamResult(System.out), new Properties());
            assertTrue(false, "Configuration mismatch not detected");
        } catch (XPathException e) {
            // All is well, error detected
        }
    }

    // Bug report on 9.1.0.2 NPE with invalid query because executable not found

    @Test
    public void testExecutableNPE() {
        try {
            String query = "let $t := text{<e a='abc'>0</e>/data((+., @a))} return max($t)";
            Configuration c = new Configuration();
            StaticQueryContext s = c.newStaticQueryContext();
            XQueryExpression e = s.compileQuery(query);
            DynamicQueryContext d = new DynamicQueryContext(c);
            StreamResult r = new StreamResult(System.out);
            e.run(d, r, null);

            assertTrue(false, "Error in query not detected");
        } catch (XPathException e) {
            // All is well, error detected
        }
    }

    @Test
    public void testSeparateCompilation() {
        try {
            Configuration c = new Configuration();
            StaticQueryContextEE s = new StaticQueryContextEE(c);
            s.compileLibrary("module namespace m='http://www.example.com/module-one'; " +
                    "declare function m:add($a as xs:integer, $b as xs:integer) as xs:integer { $a + $b };" +
                    "declare variable $m:ten as xs:integer := 10;");
            String q1 = "import module namespace m='http://www.example.com/module-one'; m:add(2,2)";
            XQueryExpression e = s.compileQuery(q1);
            DynamicQueryContext d = new DynamicQueryContext(c);
            assertTrue("4".equals(e.evaluateSingle(d).toString()), "2+2");
            String q2 = "import module namespace m='http://www.example.com/module-one'; m:add($m:ten,5)";
            e = s.compileQuery(q2);
            d = new DynamicQueryContext(c);
            assertTrue("15".equals(e.evaluateSingle(d).toString()), "10+5");
        } catch (XPathException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testIterator() {
        try {
            Configuration c = new Configuration();
            String xml = "<a><b>3</b><b>5</b></a>";
            NodeInfo doc = c.buildDocumentTree(new StreamSource(new StringReader(xml))).getRootNode();
            StaticQueryContext s = c.newStaticQueryContext();
            XQueryExpression exp = s.compileQuery("//b");
            DynamicQueryContext dqc = new DynamicQueryContext(c);
            dqc.setContextItem(doc);
            SequenceIterator iter = exp.iterator(dqc);
            int count=0;
            while (true) {
                Item item = iter.next();
                if (item == null) {
                    break;
                }
                count++;
//                assertTrue("value", (iter.position()==1 && item.getStringValue().equals("3")) ||
//                            (iter.position()==2 && item.getStringValue().equals("5")));
            }
            assertEquals(2, count, "count " + count);
        } catch (XPathException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testDirectFunctionCall() {
        try {
            Configuration config = new Configuration();
            StaticQueryContext sqc = config.newStaticQueryContext();

            XQueryExpression exp1 = sqc.compileQuery(
                    "declare namespace f='f.ns';" +
                    "declare function f:t1($p as xs:integer) { if ($p eq 20000) then $p else () };" +
                    "declare function f:t2($p as xs:integer) { for $i in 1 to $p return " +
                                                              "if ($i eq 20000) then $i else () };" +
                    "1"
            );

            UserFunction fn1 = exp1.getMainModule().getUserDefinedFunction("f.ns", "t1", 1);
            UserFunction fn2 = exp1.getMainModule().getUserDefinedFunction("f.ns", "t2", 1);
            DynamicQueryContext dqc = new DynamicQueryContext(config);
            Controller controller = exp1.newController(dqc);

            long start = System.currentTimeMillis();
            IntegerValue[] arglist = new IntegerValue[1];
            ArrayList result = new ArrayList();
            for (int x=1; x<1000000; x++) {
                arglist[0] = new Int64Value(x);
                Sequence v = fn1.call(arglist, controller);
                if (v instanceof AtomicValue) {
                    result.add(v);
                    System.err.println("Returned: " + v);
                }
            }

            long tim = System.currentTimeMillis();
            System.err.println("Elapsed time for t1: " + (tim - start) + "ms");

            IntegerValue[] arglist2 = new IntegerValue[1];
            ArrayList result2 = new ArrayList();
            arglist2[0] = new Int64Value(1000000);
            Sequence v2 = fn2.call(arglist2, controller);
            if (v2 instanceof AtomicValue) {
                result2.add(v2);
                System.err.println("Returned: " + v2);
            }
            System.err.println("Elapsed time for t1: " + (System.currentTimeMillis() - tim) + "ms");
        } catch (XPathException err) {
            fail(err.getMessage());
            err.printStackTrace();
        }
    }

    /**
     * Test addition of an extension function library at the per-query level
     */

    @Test
    public void testAddExtensionLibrary() {
        try {
            StaticQueryContextPE sqc = (StaticQueryContextPE)new ProfessionalConfiguration().newStaticQueryContext();

            sqc.setExtensionFunctionLibrary(new FunctionLibrary() {

                final SymbolicName.F fn = new SymbolicName.F(
                        new StructuredQName("hello", "http://my.hello.com/", "world"), 0);

                @Override
                public Function getFunctionItem(SymbolicName.F functionName, StaticContext env) {
                    if (functionName.equals(fn)) {
                        return null;
                    } else {
                        return null;
                    }
                }

                @Override
                public boolean isAvailable(SymbolicName.F functionName, int languageLevel) {
                    return functionName.equals(fn);
                }

                @Override
                public Expression bind(SymbolicName.F functionName, Expression[] staticArgs, StaticContext env, List<String> reasons) {
                    if (functionName.equals(fn)) {
                        return new StringLiteral("hello world");
                    } else {
                        return null;
                    }
                }

                @Override
                public FunctionLibrary copy() {
                    return null;
                }
            });

            XQueryExpression exp1 = sqc.compileQuery(
                    "declare namespace hello='http://my.hello.com/';" +
                    "hello:world()"
            );
            String result = (String)exp1.evaluateSingle(new DynamicQueryContext(sqc.getConfiguration()));
            assertEquals("hello world", result);
        } catch (Exception err) {
            err.printStackTrace();
            fail();
        }
    }

    /**
     * Test use of a custom StaticQueryContext
     */

    private static class MyStaticQueryContext extends StaticQueryContextPE {
        public MyStaticQueryContext(Configuration config) {
            super((ProfessionalConfiguration)config, false);
            setDefaultElementNamespace("http://suprise.com/");
        }
    }

    @Test
    public void testCustomSQC() {
        StaticQueryContextFactory factory = new StaticQueryContextFactory() {
            @Override
            public StaticQueryContext newStaticQueryContext(Configuration config, boolean copyFromDefault) {
                return new MyStaticQueryContext(config);
            }
        };
        try {
            Configuration config = new ProfessionalConfiguration();
            config.setStaticQueryContextFactory(factory);
            StaticQueryContext sqc = config.newStaticQueryContext();
            XQueryExpression exp = sqc.compileQuery("<e/>");
            Item result = exp.iterator(new DynamicQueryContext(config)).next();
            assertEquals("http://suprise.com/", ((NodeInfo) result).getURI(), "wrong URI");
        } catch (XPathException e) {
            fail();
        }
    }

    // Declare variables via SQC
    // Bug 2248

    @Test
    public void testDeclareVariables() {
        try {
            String query = "$x + $y";
            StaticQueryContext sqc = new Configuration().newStaticQueryContext();
            sqc.declareGlobalVariable(new StructuredQName("", "", "x"), SequenceType.SINGLE_INTEGER, Int64Value.MINUS_ONE, false);
            sqc.declareGlobalVariable(new StructuredQName("", "", "y"), SequenceType.SINGLE_INTEGER, null, true);
            XQueryExpression exp = sqc.compileQuery(query);
            DynamicQueryContext dqc = new DynamicQueryContext(sqc.getConfiguration());
            dqc.setParameter(new StructuredQName("", "", "y"), Int64Value.PLUS_ONE);
            StringWriter sw = new StringWriter();
            exp.run(dqc, new StreamResult(sw), new Properties());
            assertEquals("<?xml version=\"1.0\" encoding=\"UTF-8\"?>0", sw.toString());
        } catch (XPathException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    @Test
    public void testCollationQuery() throws XPathException {
        Configuration config = Configuration.newConfiguration();
        config.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, true);
        StaticQueryContext sqc = config.newStaticQueryContext();
        String testDir = ConfigTest.DATA_DIR;
        sqc.setBaseURI(testDir);
        //sqc.declareDefaultCollation("");
        DynamicQueryContext dqc = new DynamicQueryContext(config);
        dqc.setApplyFunctionConversionRulesToExternalVariables(false);

        String query = "declare base-uri \"../xmark/\";\n" +
                "let $auction := fn:doc(\"xmark1.xml\") return\n" +
                "for $i in $auction/site//item\n" +
                "where contains(string(exactly-one($i/description)), \"gold\")\n" +
                "return $i/name/text()";

        XQueryExpression xqExp = sqc.compileQuery(query);
        SequenceIterator itr = xqExp.iterator(dqc);
        Item item = itr.next();
        assertNotNull(item);
        assertNotNull(item.getUnicodeStringValue());
    }

    @Test
    public void testDynamicCollationQuery() throws XPathException {
        Configuration config = Configuration.newConfiguration();
        config.setConfigurationProperty(FeatureKeys.GENERATE_BYTE_CODE, true);
        StaticQueryContext sqc = config.newStaticQueryContext();
        String testDir = ConfigTest.DATA_DIR;
        sqc.setBaseURI(testDir);
        //sqc.declareDefaultCollation("");
        DynamicQueryContext dqc = new DynamicQueryContext(config);
        dqc.setApplyFunctionConversionRulesToExternalVariables(false);

        String query = "declare base-uri \"../xmark/\";\n" +
                "let $auction := fn:doc(\"xmark1.xml\") return\n" +
                "for $i in $auction/site//item\n" +
                "where contains(string(exactly-one($i/description)), \"gold\", "
                + "if ($i/nonexistent) then 'iiii' else 'http://www.w3.org/2005/xpath-functions/collation/codepoint')\n" +
                "return $i/name/text()";

        XQueryExpression xqExp = sqc.compileQuery(query);
        SequenceIterator itr = xqExp.iterator(dqc);
        Item item = itr.next();
        assertNotNull(item);
        assertNotNull(item.getUnicodeStringValue());
    }



    @Test
    public void testLocalW3CDTDaccess () {  // bug 3678
        Configuration config = new Configuration();

        String uri = "https://www.dbis.informatik.uni-goettingen.de/mini.html";
        String src = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\n"
                + "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\n"
                + "<html xmlns=\"http://www.w3.org/1999/xhtml\">\n"
                + "<head>\n"
                + "<title>test</title>\n"
                + "</head>\n"
                + "<body><p>bla</p></body>\n"
                + "</html>";
        try {
            DynamicQueryContext dynamicContext = new DynamicQueryContext(config);
            SimpleDateFormat sdf = new SimpleDateFormat("HH:mm:ss");
            Long start = System.currentTimeMillis();
            TreeInfo tree = config.buildDocumentTree(new StreamSource(new StringReader(src), uri));
            Long end = System.currentTimeMillis();
            // Test there is no network fetch
            assertTrue(end - start < 1000);
            dynamicContext.setContextItem(tree.getRootNode());
            StaticQueryContext staticContext = config.newStaticQueryContext();
            XQueryExpression exp = staticContext.compileQuery("/");
            List<Object> sxresults = exp.evaluate(dynamicContext);
            //System.out.println(getXMLResult((NodeInfo) sxresults.get(0)));

        } catch (Exception e) {
            System.out.println("<h1>ERROR</h1><p>" + e.toString() + "</p>");
            System.out.println("Error: " + e.toString());
            e.printStackTrace();
        }
    }


}
