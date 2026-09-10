package s9apitest;

import com.saxonica.config.EnterpriseConfiguration;
import jaxptest.ConfigTest;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.StaticContext;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.functions.FunctionLibrary;
import net.sf.saxon.functions.FunctionLibraryList;
import net.sf.saxon.lib.*;
import net.sf.saxon.om.Item;
import net.sf.saxon.om.NamespaceResolver;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.query.QueryModule;
import net.sf.saxon.s9api.*;
import net.sf.saxon.sxpath.IndependentContext;
import net.sf.saxon.trace.Traceable;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.Source;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.*;

import static org.junit.jupiter.api.Assertions.assertEquals;
import static org.junit.jupiter.api.Assertions.assertNotSame;
import static org.junit.jupiter.api.Assertions.assertSame;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests for the s9api XPathCompiler
 */
public class TestXPathCompiler {
    private final ConfigTest configTest = new ConfigTest();
    
    @Test
    public void testSimple() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable e = c.compile("count(//*)");
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo><bar/></foo>")));
            XPathSelector s = e.load();
            s.setContextItem(foo);
            assertEquals(2, ((XdmAtomicValue) s.evaluateSingle()).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test using declared variables
     */

    @Test
    public void testWithDeclaredVariables() {
        try {
            Processor p = new Processor(false);
            ItemTypeFactory itf = new ItemTypeFactory(p);
            ItemType xsInteger = itf.getAtomicType(new QName(NamespaceConstant.SCHEMA, "integer"));
            XPathCompiler c = p.newXPathCompiler();
            c.declareVariable(new QName("a"), xsInteger, OccurrenceIndicator.ONE);
            c.declareVariable(new QName("b"), xsInteger, OccurrenceIndicator.ONE);
            XPathExecutable e = c.compile("$a + $b");
            XPathSelector s = e.load();
            s.setVariable(new QName("a"), new XdmAtomicValue(2));
            s.setVariable(new QName("b"), new XdmAtomicValue(2));
            assertEquals(4, ((XdmAtomicValue) s.evaluateSingle()).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test using undeclared variables
     */

    @Test
    public void testWithUndeclaredVariables() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setAllowUndeclaredVariables(true);
            XPathExecutable e = c.compile("$a + $b");
            XPathSelector s = e.load();
            for (Iterator<QName> iq = e.iterateExternalVariables(); iq.hasNext(); ) {
                QName q = iq.next();
                s.setVariable(q, new XdmAtomicValue(2));
            }
            assertEquals(4, ((XdmAtomicValue) s.evaluateSingle()).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test using undeclared variables when disallowed
     */

    @Test
    public void testWithDisallowedUndeclaredVariables() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setAllowUndeclaredVariables(false);
            XPathExecutable e = c.compile("$a + $b");
            XPathSelector s = e.load();
            for (Iterator<QName> iq = e.iterateExternalVariables(); iq.hasNext(); ) {
                QName q = iq.next();
                s.setVariable(q, new XdmAtomicValue(2));
            }
            fail("No error reported for undeclared variables");
        } catch (SaxonApiException e) {
            assertTrue(true);
        }
    }


    /**
     * Test with statically-known collation
     */

    @Test
    public void testStaticallyKnownSaxonCollation() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable e = c.compile("compare('A', 'a', 'http://saxon.sf.net/collation?ignore-case=yes')");
            assertEquals(0, ((XdmAtomicValue) e.load().evaluateSingle()).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test with dynamicall-computed collation
     */

    @Test
    public void testDynamicallyKnownSaxonCollation() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable e = c.compile("compare('A', 'a',  " +
                                                  "if (string(current-date()) eq '1900-01-01') then 'rubbish' else 'http://saxon.sf.net/collation?ignore-case=yes')");
            assertEquals(0, ((XdmAtomicValue) e.load().evaluateSingle()).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test with function-lookup
     */

    @Test
    public void testFunctionLookup() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable e = c.compile("year-from-date(function-lookup(QName('http://www.w3.org/2001/XMLSchema', 'date'), 1)('2001-12-31'))");
            assertEquals(2001, ((XdmAtomicValue) e.load().evaluateSingle()).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test for XPath 2.1 EQName syntax
     */

    @Test
    public void testEQNameAllowedIn30FunctionName() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.0");
            XPathExecutable e = c.compile("Q{http://www.w3.org/2001/XMLSchema}integer('1234')");
            XPathSelector s = e.load();
            assertEquals(1234, ((XdmAtomicValue) s.evaluateSingle()).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testEQNameAllowedIn30NameTest() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.0");
            XdmNode root = p.newDocumentBuilder().build(
                    new StreamSource(new StringReader("<a:doc xmlns:a='urn:a'>93</a:doc>")));
            XPathExecutable e = c.compile("data(/Q{urn:a}doc)");
            XPathSelector s = e.load();
            s.setContextItem(root);
            assertTrue(s.iterator().hasNext(), "element not found");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test for XPath 3.0 EQName syntax
     */

    @Test
    public void testEQNameDisallowedIn20FunctionName() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.0");
            c.compile("'http://www.w3.org/2001/XMLSchema':integer('1234')");
            fail("EQName syntax not allowed in 2.0, error not detected");
        } catch (SaxonApiException | UnsupportedOperationException e) {
            // OK
        }
    }

    @Test
    public void testEQNameDisallowedIn20NameTest() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.0");
            XdmNode root = p.newDocumentBuilder().build(
                    new StreamSource(new StringReader("<a:doc xmlns:a='urn:a'>93</a:doc>")));
            XPathExecutable e = c.compile("data(/'urn:a':doc)");
            XPathSelector s = e.load();
            s.setContextItem(root);
            fail("EQName syntax not allowed in 2.0, error not detected");
        } catch (SaxonApiException | UnsupportedOperationException e) {
            //OK
        }
    }

    @Test
    public void testCaching() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setCaching(true);
            XPathExecutable e1 = c.compile("2+2");
            XPathExecutable e2 = c.compile("2+2");
            assertSame(e1, e2, "Found in cache");
            c.setCaching(false);
            XPathExecutable e3 = c.compile("2+2");
            assertNotSame(e3, e2, "Not found in cache");
            assertEquals(e3.getUnderlyingExpression().getInternalExpression(), e2.getUnderlyingExpression().getInternalExpression(), "Not identical, but equal");
            c.setCaching(true);
            XPathExecutable e4 = c.compile("2+2");
            try {
                c.setBaseURI(new URI("http://www.w3.org/"));
            } catch (URISyntaxException err) {
                fail(err.getMessage());
            }
            XPathExecutable e5 = c.compile("2+2");
            assertNotSame(e5, e4, "Not found in cache");
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test "let" expression allowed in XPath 3.0
     */

    @Test
    public void testLetAllowedIn21() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.0");
            XPathExecutable e = c.compile("let $n := 5 return 1229 + $n");
            XPathSelector s = e.load();
            assertEquals(1234, ((XdmAtomicValue) s.evaluateSingle()).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test "let" expression disallowed in XPath 2.0
     */

    @Test
    public void testLetDisallowedIn20() {
        try {
            Processor p = new Processor(true);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("2.0");
            XPathExecutable e = c.compile("let $n := 5 return 1229 + $n");
            XPathSelector s = e.load();
            fail("No error reported for let in 2.0");
        } catch (SaxonApiException e) {
            // success
        }
    }

    /**
     * Test that it's OK (since 9.2) not to bind a value for a declared variable if the variable
     * isn't actually used
     */

    @Test
    public void testOkNotToBindUnusedButDeclaredVariable() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.declareVariable(new QName("vvv"));
            XPathExecutable e = c.compile("2+2");
            XPathSelector s = e.load();
            assertEquals(4, ((XdmAtomicValue) s.evaluateSingle()).getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    /**
     * Test that it's not OK not to bind a value for a declared variable if the variable
     * is actually used
     */

    @Test
    public void testFailureToBindVariable() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.declareVariable(new QName("vvv"));
            XPathExecutable e = c.compile("2+$vvv");
            XPathSelector s = e.load();
            XdmAtomicValue result = (XdmAtomicValue) s.evaluate();
            fail("undetected failure to bind variable");
        } catch (SaxonApiException e) {
            // failure expected
        }
    }

    /**
     * Test use of required context item type
     */

    @Test
    public void testContextItemTypeOk() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setRequiredContextItemType(ItemType.INTEGER);
            XPathExecutable e = c.compile(".+3");
            XPathSelector s = e.load();
            s.setContextItem(new XdmAtomicValue(2));
            XdmAtomicValue result = (XdmAtomicValue) s.evaluate();
            assertEquals(5, result.getLongValue());
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testContextItemTypeNotOk() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setRequiredContextItemType(ItemType.STRING);
            XPathExecutable e = c.compile("number(.)+3");
            XPathSelector s = e.load();
            s.setContextItem(new XdmAtomicValue(2));
            XdmAtomicValue result = (XdmAtomicValue) s.evaluate();
            fail("Context item is wrong type - not detected");
        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            // failure expected
        }
    }

    @Test
    public void testContextItemTypeNotConverted() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setRequiredContextItemType(ItemType.DOUBLE);
            XPathExecutable e = c.compile("number(.)+3");
            XPathSelector s = e.load();
            s.setContextItem(new XdmAtomicValue(2));
            XdmAtomicValue result = (XdmAtomicValue) s.evaluate();
            fail("Context item is wrong type - not detected");
        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            // failure expected
        }
    }

    @Test
    public void testContextItemTypeStaticError() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setRequiredContextItemType(ItemType.INTEGER);
            XPathExecutable e = c.compile("substring(., 2, 5)");
            fail("Context item is wrong type - not detected");
        } catch (SaxonApiException e) {
            System.err.println(e.getMessage());
            // failure expected
        }
    }

//    public void testSchemaAware() {
//        // Rewrite of XPathExampleSA to use s9api
//        try {
//            Processor p = new Processor(true);
//            p.getSchemaManager().load(
//                    new StreamSource(new File(configTest.getDataDir() + "books.xsd").toURI().toString()));
//            p.registerExtensionFunction(
//                    new ExtensionFunctionDefinition() {
//
//                        public StructuredQName getFunctionQName() {
//                            return new StructuredQName("f", "http://local/functions", "toCentimetres");
//                        }
//
//                        public int getMinimumNumberOfArguments() {
//                            return 2;
//                        }
//
//                        public SequenceType[] getArgumentTypes() {
//                            return new SequenceType[]{
//                                    BuiltInAtomicType.DECIMAL.oneOrMore(),
//                                    SequenceType.SINGLE_STRING};
//                        }
//
//                        public SequenceType getResultType(SequenceType[] suppliedArgumentTypes) {
//                            return BuiltInAtomicType.DECIMAL.oneOrMore();
//                        }
//
//                        public ExtensionFunctionCall makeCallExpression() {
//                            final BigDecimal INCHES_TO_CM = new BigDecimal(2.54);
//                            return new ExtensionFunctionCall() {
//                                public Sequence call(XPathContext context, Sequence[] arguments) throws XPathException {
//                                    String units = arguments[1].head().getStringValue();
//                                    if (units.equals("cm")) {
//                                        return arguments[0];
//                                    } else if (units.equals("in")) {
//                                        return SequenceTool.toLazySequence(
//                                                new ItemMappingIterator(
//                                                        arguments[0].iterate(),
//                                                        new ItemMappingFunction() {
//                                                            public Item mapItem(Item item) throws XPathException {
//                                                                BigDecimal dec = ((BigDecimalValue) item).getDecimalValue();
//                                                                return new BigDecimalValue(dec.multiply(INCHES_TO_CM));
//                                                            }
//                                                        }
//                                                )
//                                        );
//                                    } else {
//                                        throw new XPathException("Unknown units value: " + units);
//                                    }
//                                }
//                            };
//                        }
//                    }
//            );
//            DocumentBuilder builder = p.newDocumentBuilder();
//            builder.setSchemaValidator(p.getSchemaManager().newSchemaValidator());
//            XdmNode doc = builder.build(new File(configTest.getDataDir() + "books.xml"));
//            XPathCompiler c = p.newXPathCompiler();
//            c.importSchemaNamespace("");
//            c.setSchemaAware(true);
//            c.declareVariable(new QName("age"));
//            c.declareVariable(new QName("thickness"));
//            c.declareNamespace("f", "http://local/functions");
//            String exp = "//schema-element(ITEM)" +
//                    "[PUB-DATE gt (current-date() - xs:yearMonthDuration($age))]" +
//                    "[min((f:toCentimetres(data(DIMENSIONS) treat as dimensionType*, " +
//                    "data(DIMENSIONS/@UNIT)))) gt $thickness]" +
//                    "/TITLE/string(.)";
//            XPathExecutable e = c.compile(exp);
//            XPathSelector s = e.load();
//            s.setVariable(new QName("age"), new XdmAtomicValue("P10Y"));
//            s.setVariable(new QName("thickness"), new XdmAtomicValue(new BigDecimal("2.0")));
//            s.setContextItem(doc);
//            for (XdmItem r : s) {
//                System.err.println(r);
//                assertTrue("resultsAsExpected",
//                           r.toString().equals("The Big Over Easy") ||
//                                   r.toString().equals("Pride and Prejudice") ||
//                                   r.toString().equals("Wuthering Heights") ||
//                                   r.toString().equals("The Eyre Affair"));
//            }
//        } catch (SaxonApiException e) {
//            fail(e.getMessage());
//        }
//    }

    /**
     * Test case from malik@metamoji.com, Sept 2010
     */

    @Test
    public void testUseXQueryFunctions() {

        // NOTE: this test fetches schema documents from the internet.

        if (true) {
            return; // can't fetch docs from internet.
        }

        Processor processor = new Processor(true);
        SchemaManager schemaManager = processor.getSchemaManager();
        EnterpriseConfiguration conf = (EnterpriseConfiguration) processor.getUnderlyingConfiguration();

        conf.displayLicenseMessage();
        conf.setSchemaValidationMode(Validation.STRICT);

        XPathCompiler xpathCompiler;
        try {

            // 1. load schema into EnterpriseConfiguration
            XsdSchema schema = schemaManager.load(new StreamSource(configTest.getDataDir() + "malix/schema.xsd"));

            XQueryCompiler xqueryCompiler = processor.newXQueryCompiler();
            xqueryCompiler.setSchemaAware(true);
            xqueryCompiler.useSchema(schema);

            xpathCompiler = processor.newXPathCompiler();
            xpathCompiler.setSchemaAware(true);
            xpathCompiler.useSchema(schema);

            String xq =
                    "declare namespace xfi='http://www.xbrl.org/2008/function/instance'; \n" +
                            "declare namespace xbrli='http://www.xbrl.org/2003/instance'; \n" +
                            " import schema 'http://www.xbrl.org/2003/instance';  \n" +
                            "declare function xfi:facts-in-instance( $xbrl as element(xbrli:xbrl) ) as element()* { \n" +
                            "for $e in $xbrl//*[. instance of schema-element(xbrli:item) or . instance of schema-element(xbrli:tuple)] \n" +
                            "return $e \n" +
                            "}; \n" +
                            "10";

            XQueryExecutable xqExec = xqueryCompiler.compile(xq);
            QueryModule qm = xqExec.getUnderlyingCompiledQuery().getMainModule();
            FunctionLibrary fl1 = qm.getFunctionLibrary();

            StaticContext sc1 = xpathCompiler.getUnderlyingStaticContext();
            FunctionLibrary fl2 = sc1.getFunctionLibrary();
            if (fl2 instanceof FunctionLibraryList) {
                ((FunctionLibraryList) fl2).addFunctionLibrary(fl1);
            }

            String xml =
                    "<?xml version='1.0' encoding='utf-8'?>\n" +
                            "<xbrl xmlns='http://www.xbrl.org/2003/instance' xmlns:link='http://www.xbrl.org/2003/linkbase'  xmlns:xlink='http://www.w3.org/1999/xlink'>\n" +
                            "  <link:schemaRef xlink:href='aem-20100331.xsd' xlink:type='simple' />\n" +
                            "</xbrl>";

            InputSource is = new InputSource(new StringReader(xml));
            Source ss = new SAXSource(is);
            ParseOptions options = new ParseOptions()
                    .withExpandAttributeDefaults(true)
                    .withSchemaValidationMode(Validation.STRICT);
            ss = new AugmentedSource(ss, options);
            NodeInfo doc = conf.buildDocumentTree(ss, options).getRootNode();

            xpathCompiler.setAllowUndeclaredVariables(true);

            IndependentContext sc2 = (IndependentContext) xpathCompiler.getUnderlyingStaticContext();
            FunctionLibrary fl3 = sc2.getFunctionLibrary();

            sc2.setNamespaceResolver(new NamespaceResolver() {

                @Override
                public Iterator<String> iteratePrefixes() {
                    return null;
                }

                @Override
                public NamespaceUri getURIForPrefix(String prefix, boolean useDefault) {
                    if ("xfi".equals(prefix)) {
                        return NamespaceUri.of("http://www.xbrl.org/2008/function/instance");
                    } else if ("xbrli".equals(prefix)) {
                        return NamespaceUri.of("http://www.xbrl.org/2003/instance");
                    }
                    return null;
                }
            });

            XPathExecutable exec = null;

            exec = xpathCompiler.compile("$n1 + xfi:facts-in-instance(/xbrli:xbrl)");

            Set<QName> dependencies = new HashSet<>();

            Iterator<net.sf.saxon.s9api.QName> it = exec.iterateExternalVariables();
            while (it.hasNext()) {
                net.sf.saxon.s9api.QName qn = it.next();
                dependencies.add(new QName(qn.getNamespace(), qn.getLocalName(), qn.getPrefix()));
            }

            if (dependencies.isEmpty()) {
                fail("Uh oh! NO VARIABLES!");
            }

        } catch (Throwable e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testStringLength() {
        // bug report from Norm Walsh, 2 Dec 2011
        try {
            String xpath = "boolean(string-length($href) > 0)";

            Processor processor = new Processor(false);
            Hashtable<String, String> nsBindings = new Hashtable<>();
            Hashtable<QName, String> globals = new Hashtable<>();

            globals.put(new QName("", "href"), "test");

            XdmNode doc = null;
            Vector<XdmItem> results = new Vector<>();

            XPathCompiler xcomp = processor.newXPathCompiler();

            for (QName varname : globals.keySet()) {
                xcomp.declareVariable(varname);
            }

            for (String prefix : nsBindings.keySet()) {
                xcomp.declareNamespace(prefix, nsBindings.get(prefix));
            }
            XPathExecutable xexec = xcomp.compile(xpath);
            XPathSelector selector = xexec.load();

            for (QName varname : globals.keySet()) {
                XdmAtomicValue value = new XdmAtomicValue("irrelevant");
                selector.setVariable(varname, value);
            }

            if (doc != null) {
                selector.setContextItem(doc);
            }

            try {
                for (XdmItem result : selector) {
                    results.add(result);
                }
            } catch (SaxonApiUncheckedException saue) {
                throw saue;
            }

            assertEquals(1, results.size());
            assertEquals("true", results.get(0).toString());
        } catch (SaxonApiException e) {
            fail();
            e.printStackTrace();
        }
    }

    @Test
    public void testReflexiveExtension() {
        try {
            Processor proc = new Processor(true);
            XPathCompiler comp = proc.newXPathCompiler();
            comp.declareNamespace("eg", "java:java.lang.Double");
            comp.declareVariable(new QName("arg"));
            XPathExecutable exp = comp.compile("eg:toString($arg)");
            XPathSelector ev = exp.load();
            ev.setVariable(new QName("arg"), new XdmAtomicValue(1.0));
            XdmValue val = ev.evaluate();
            String result = val.toString();
            System.out.println(result);
        } catch (SaxonApiException err) {
            err.printStackTrace();
            fail();
        }
    }

    @Test
    public void testErrorListener() {
        // From 9.7, static errors are no longer notified to the Configuration's ErrorListener
        // From 10.0, the Configuration no longer owns an ErrorListener, so the test becomes rather pointless
        final List<String> errors = new ArrayList<>();
        try {
            Processor p = new Processor(false);
//            p.getUnderlyingConfiguration().setErrorListener(
//                    new StandardErrorListener() {
//                        public void error(TransformerException exception) {
//                            errors.add(exception.getMessageAndLocation());
//                        }
//                    }
//            );
            XPathCompiler c = p.newXPathCompiler();
            XPathExecutable e = c.compile("count(//z:*)");
            DocumentBuilder b = p.newDocumentBuilder();
            XdmNode foo = b.build(new StreamSource(new StringReader("<foo><bar/></foo>")));
            XPathSelector s = e.load();
            s.setContextItem(foo);
            fail("Unexpected success");
        } catch (SaxonApiException e) {
            assertTrue(errors.isEmpty()); // Changed in 9.7
        }
    }

    @Test
    public void testSchemaAwareEqualityGood() {
        try {
            boolean result = trySchemaAwareEquality(true);
            assertTrue(result, "result");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testSchemaAwareEqualityBad() {
        try {
            trySchemaAwareEquality(false);
            fail();
        } catch (SaxonApiException e) {
            // success
        }
    }

    private boolean trySchemaAwareEquality(boolean isSchemaAware) throws SaxonApiException {

        Processor processor = new Processor(true);
        XsdCompiler sm = processor.newXsdCompiler();
        String xsd = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" \n" +
                "           targetNamespace=\"http://www.xpathtest.com/test\" \n" +
                "           xmlns:test=\"http://www.xpathtest.com/test\" \n" +
                "           elementFormDefault=\"qualified\">\n" +
                "  <xs:element name=\"comparables\">\n" +
                "    <xs:complexType>\n" +
                "      <xs:sequence>\n" +
                "        <xs:element ref=\"test:comparable\" maxOccurs=\"unbounded\"/>\n" +
                "      </xs:sequence>\n" +
                "    </xs:complexType>\n" +
                "  </xs:element>\n" +
                "  <xs:element name=\"comparable\">\n" +
                "    <xs:complexType>\n" +
                "      <xs:sequence>\n" +
                "        <xs:element name=\"string\" type=\"xs:string\"/>\n" +
                "        <xs:element name=\"int\" type=\"xs:integer\"/>\n" +
                "      </xs:sequence>\n" +
                "    </xs:complexType>\n" +
                "  </xs:element>\n" +
                "</xs:schema>";
        XsdSchema schema = sm.compile(new StreamSource(new StringReader(xsd)));
        SchemaValidator sv = schema.newValidator();
        XdmDestination xdmDest = new XdmDestination();
        sv.setDestination(xdmDest);
        String xml = "<comparables xmlns=\"http://www.xpathtest.com/test\">\n" +
                "  <comparable>\n" +
                "    <string>hello</string>\n" +
                "    <int>1</int>\n" +
                "  </comparable>\n" +
                "  <comparable>\n" +
                "    <string>hello</string>\n" +
                "    <int>1</int>\n" +
                "  </comparable>\n" +
                "</comparables>";
        sv.validate(new StreamSource(new StringReader(xml)));
        String xpathExpr = "//test:comparable[1]/test:int eq //test:comparable[2]/test:int";
        XPathCompiler comp = processor.newXPathCompiler();
        if (isSchemaAware) {
            comp.setSchemaAware(true);
        }
        comp.declareNamespace("test", "http://www.xpathtest.com/test");
        XPathExecutable xpathExec = comp.compile(xpathExpr);
        XPathSelector selector = xpathExec.load();
        selector.setContextItem(xdmDest.getXdmNode());
        return selector.effectiveBooleanValue();

    }

    @Test
    public void testXPathTraceListener() throws SaxonApiException {
        net.sf.saxon.s9api.Processor processor = new net.sf.saxon.s9api.Processor(true);
        XPathCompiler newXPathCompiler = processor.newXPathCompiler();
        processor.getUnderlyingConfiguration().setTraceListener(new TraceListener() {

            @Override
            public void startCurrentItem(Item currentItem) {
            }

            @Override
            public void setOutputDestination(Logger stream) {
            }

            @Override
            public void open(Controller controller) {
            }

            @Override
            public void leave(Traceable instruction) {
            }

            public void enter(Traceable instruction, XPathContext context) {
                assertTrue(instruction.getLocation().getLineNumber() > 0);
            }

            @Override
            public void endCurrentItem(Item currentItem) {
            }

            @Override
            public void close() {
            }
        });
        XPathExecutable executable = newXPathCompiler.compile("\n\n\ntrace(true(),'label')");
        XPathSelector selector = executable.load();

        XdmNode node = processor.newDocumentBuilder().build(
                new SAXSource(new InputSource(new StringReader("<root/>"))));

        selector.setContextItem(node);
        selector.evaluate();

    }

    @Test
    public void testDecimalFormat() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            c.setDecimalFormatProperty(new QName("deci"), "decimal-separator", "~");
            c.setDecimalFormatProperty(new QName("deci"), "grouping-separator", "|");
            c.setDecimalFormatProperty(new QName("deci"), "exponent-separator", "E");
            XPathExecutable e = c.compile("format-number(123.4, '0|000~0E0', 'deci')");
            XPathSelector s = e.load();
            String result = s.evaluateSingle().getStringValue();
            assertEquals("1|234~0E-1", result);
        } catch (SaxonApiException e) {
            fail(e.getMessage());
        }
    }

    @Test
    public void testBug3188() {

        String xpathString = "/xbrli:xbrl/xbrli:context[xbrli:scenario/xbrldi:typedMember[fn:resolve-QName(string(@dimension), .) =\n"
                + "  (xs:QName('ocw-dim:EquityIndividualSpecificationAxis'),\n"
                + "   xs:QName('ocw-dim:PropertyPlantEquipmentDepreciationSpecificationAxis'),\n"
                + "   xs:QName('ocw-dim:RealEstateEconomicValueSpecificationAxis'),\n"
                + "   xs:QName('ocw-dim:ContingentLiabilitiesSpecificationAxis'),\n"
                + "   xs:QName('ocw-dim:ContingentAssetsSpecificationAxis'),\n"
                + "   xs:QName('ocw-dim:ProvisionsIndividualSpecificationAxis'),\n"
                + "   xs:QName('ocw-dim:LiabilitiesNoncurrentIndividualSpecificationAxis'),\n"
                + "   xs:QName('ocw-dim:RelatedPartySpecificationAxis'),\n"
                + "   xs:QName('ocw-dim:GrantSpecificationAxis'),\n"
                + "   xs:QName('ocw-dim:ProjectSubsidyFinancialSpecificationAxis'))]]";
        try {
            Processor processor = new Processor(true);

            DocumentBuilder docBuilder = processor.newDocumentBuilder();
            File inputFile = new File(configTest.getDataDir() + "xbrlInstance.xml");
            XdmItem contextItem = docBuilder.build(inputFile);

            XPathCompiler xpathCompiler = processor.newXPathCompiler();
            xpathCompiler.declareNamespace("fn", "http://www.w3.org/2005/xpath-functions");
            xpathCompiler.declareNamespace("xbrli", "http://www.xbrl.org/2003/instance");
            xpathCompiler.declareNamespace("xbrldi", "http://xbrl.org/2006/xbrldi");
            xpathCompiler.declareNamespace("ocw-dim", "http://www.nltaxonomie.nl/nt11/ocw/20170301.b/dictionary/ocw-axes");

            XdmValue result = xpathCompiler.evaluate(xpathString, contextItem);
            assertTrue(result instanceof XdmNode);
            assertSame(((XdmNode) result).getNodeKind(), XdmNodeKind.ELEMENT);
            assertEquals("context", ((XdmNode) result).getNodeName().getLocalName());
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    /**
     * round#2 is rejected under version 2.0
     */

    @Test
    public void testAbsent30Function() {
        try {
            Processor p = new Processor(false);
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("2.0");
            XPathExecutable e = c.compile("round(1.234, 2)");
            XdmItem result = e.load().evaluateSingle();
            fail("unexpected success");
        } catch (SaxonApiException e) {
            assertEquals("XPST0017", e.getErrorCode().getLocalName());
        }
    }

    /**
     * Invoke functions in an XSLT library package
     */

    @Test
    public void testInvokeXsltFunction() {
        try {
            Processor p = new Processor(false);

            XsltCompiler compiler = p.newXsltCompiler();
            String xsl = "<xsl:package\n" +
                    "    name = \"package-002.xsl\"\n" +
                    "    package-version = \"2.1.0.5\"\n" +
                    "    version = \"3.0\" xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>\n" +
                    "        <xsl:function name='Q{http://square.com/}square' visibility='public'>\n" +
                    "          <xsl:param name='x' as='xs:double'/>\n" +
                    "          <xsl:sequence select='$x*$x'/>\n" +
                    "        </xsl:function>\n" +
                    "</xsl:package>";
            XsltPackage pack = compiler.compilePackage(new StreamSource(new StringReader(xsl)));
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            c.addXsltFunctionLibrary(pack);
            c.setAllowUndeclaredVariables(true);
            XPathExecutable e = c.compile("Q{http://square.com/}square($x)");
            XPathSelector exp = e.load();
            exp.setVariable(new QName("x"), new XdmAtomicValue(5e0));
            XdmItem result = exp.evaluateSingle();
            assertEquals(((XdmAtomicValue)result).getDoubleValue(), 25e0);
        } catch (SaxonApiException e) {
            fail();
        }
    }

    /**
     * Invoke functions in an XSLT library package
     */

    @Test
    public void testInvokeXsltFunctionExposedAsPublic() {
        try {
            Processor p = new Processor(false);

            XsltCompiler compiler = p.newXsltCompiler();
            String xsl = "<xsl:package\n" +
                    "    name = \"package-002.xsl\"\n" +
                    "    package-version = \"2.1.0.5\"\n" +
                    "    version = \"3.0\" xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>\n" +
                    "        <xsl:function name='Q{http://square.com/}square'>\n" +
                    "          <xsl:param name='x' as='xs:double'/>\n" +
                    "          <xsl:sequence select='$x*$x'/>\n" +
                    "        </xsl:function>\n" +
                    "        <xsl:expose component='function' names='Q{http://square.com/}square#1' visibility='public'/>\n" +
                    "</xsl:package>";
            XsltPackage pack = compiler.compilePackage(new StreamSource(new StringReader(xsl)));
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            c.addXsltFunctionLibrary(pack);
            c.setAllowUndeclaredVariables(true);
            XPathExecutable e = c.compile("Q{http://square.com/}square($x)");
            XPathSelector exp = e.load();
            exp.setVariable(new QName("x"), new XdmAtomicValue(5e0));
            XdmItem result = exp.evaluateSingle();
            assertEquals(((XdmAtomicValue) result).getDoubleValue(), 25e0);
        } catch (SaxonApiException e) {
            fail();
        }
    }

    /**
     * Fail to invoke private functions in an XSLT library package
     */

    @Test
    public void testInvokePrivateXsltFunction() {
        try {
            Processor p = new Processor(false);

            XsltCompiler compiler = p.newXsltCompiler();
            String xsl = "<xsl:package\n" +
                    "    name = \"package-002.xsl\"\n" +
                    "    package-version = \"2.1.0.5\"\n" +
                    "    version = \"3.0\" xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema'>\n" +
                    "        <xsl:function name='Q{http://square.com/}square'>\n" +
                    "          <xsl:param name='x' as='xs:double'/>\n" +
                    "          <xsl:sequence select='$x*$x'/>\n" +
                    "        </xsl:function>\n" +
                    "</xsl:package>";
            XsltPackage pack = compiler.compilePackage(new StreamSource(new StringReader(xsl)));
            XPathCompiler c = p.newXPathCompiler();
            c.setLanguageVersion("3.1");
            c.addXsltFunctionLibrary(pack);
            c.setAllowUndeclaredVariables(true);
            XPathExecutable e = c.compile("Q{http://square.com/}square($x)");
            XPathSelector exp = e.load();
            exp.setVariable(new QName("x"), new XdmAtomicValue(5e0));
            XdmItem result = exp.evaluateSingle();
            fail("unexpected success");
        } catch (SaxonApiException e) {
            System.err.println("Failed as expected: " + e.getMessage());
        }
    }

}

