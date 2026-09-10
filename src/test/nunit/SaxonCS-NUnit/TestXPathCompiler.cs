using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using Saxon.Api;
using NUnit.Framework;
using System.Xml;
using System.Text;
using DocumentBuilder = Saxon.Api.DocumentBuilder;
using Processor = Saxon.Api.Processor;
using SchemaManager = Saxon.Api.SchemaManager;
using SchemaValidator = Saxon.Api.SchemaValidator;
using XdmDestination = Saxon.Api.XdmDestination;
using XPathCompiler = Saxon.Api.XPathCompiler;
using XPathExecutable = Saxon.Api.XPathExecutable;
using XPathSelector = Saxon.Api.XPathSelector;


namespace SaxonCS_NUnit {
    class TestXPathCompiler {

        private void assertAreEqual(Object a, Object b) {
            Assert.That(a, Is.EqualTo(b));
        }

        private void assertNotNull(Object a) {
            Assert.That(a, Is.Not.Null);
        }
        [Test]
        public void TestSimple() {
            try {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                XPathExecutable e = c.Compile("count(//*)");
                DocumentBuilder b = p.NewDocumentBuilder();
                b.BaseUri = new Uri("http://www.example.com");
                XdmNode foo = b.Build(new StringReader("<foo><bar/></foo>"));
                XPathSelector s = e.Load();
                s.ContextItem = foo;
                object b1 = ((XdmAtomicValue) s.EvaluateSingle()).AsLong();
                Assert.That((object)2, Is.EqualTo(b1));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestSimpleDelegates() {
            //TODO (MHK 2021-07-01) Reinstate this
            // try {
            //     Processor p = new Processor(false);
            //     XPathCompiler c = p.NewXPathCompiler();
            //     XPathExecutable e = c.Compile("count(//*)");
            //     DocumentBuilder b = p.NewDocumentBuilder();
            //     b.BaseUri = new Uri("http://www.example.com");
            //     XdmValue value = new XdmValue(XdmAtomicValue.MakeAtomicValue("test1"));
            //
            //     value = value.Append(XdmAtomicValue.MakeAtomicValue("test2"));
            //     //IEnumerable<XdmItem> iterx = value.Select(Root);
            //     IEnumerator<XdmNode> iter =
            //         b.Build(new StringReader("<foo attr='value1'><bar attr2='value2' />text</foo>"))
            //             .EnumerateAxis(XdmAxis.Child);
            //     bool moved = iter.MoveNext();
            //     XdmNode foo = iter.Current;
            //     value = value.Append(foo);
            //
            //     IEnumerator<XdmNode> iter2 =
            //         b.Build(new StringReader("<doc attr='value1'><name attr2='value2' />Joe</doc>"))
            //             .EnumerateAxis(XdmAxis.Child);
            //     bool moved2 = iter2.MoveNext();
            //     XdmNode foo2 = iter2.Current;
            //     value = value.Append(foo2);
            //
            //
            //     // test 1
            //     // 
            //     // XdmValue value2 = new XdmValue(value.Where(
            //     //     Predicates.AnyOf<XdmItem>(
            //     //         Predicates.Eq<XdmItem>(XdmAtomicValue.MakeAtomicValue("test1")),
            //     //         Predicates.Eq(XdmAtomicValue.MakeAtomicValue("test2")))));
            //     // string text1 = ((XdmAtomicValue) value2.ItemAt(0)).GetStringValue();
            //     // assertAreEqual("test1", text1);
            //     // assertAreEqual("test2", ((XdmAtomicValue) value2.ItemAt(1)).GetStringValue());
            //
            //     //test 1a
            //     IEnumerable<XdmNode> valueNodes = value.Where<XdmNode>(Predicates.HasLocalName("foo"));
            //
            //     // test 2
            //     XdmValue value3 = new XdmValue(value.Where(i => i is XdmAtomicValue));
            //     assertAreEqual(2, value3.Count);
            //
            //
            //     // test 3
            //     IEnumerable<XdmItem> value4 = value.Where(i => i is XdmAtomicValue);
            //     assertAreEqual(2, value4.Count());
            //
            //     // test 3a
            //     Assert.That(value.AnyMatch(Predicates.IsNode()), "XdmValue contains a Node");
            //
            //     // test 3b
            //     Assert.That(!value.AllMatch(Predicates.IsAtomic()), "XdmValue All Atomic");
            //
            //     Assert.That(value.Where(Predicates.IsAtomic()).All(Predicates.IsAtomic()),
            //         "Should only contain Atomic values");
            //     //test3
            //     var iterx = foo.Select(Steps.Root());
            //
            //
            //     int count = iterx.Count();
            //
            //     var myenum = iterx.GetEnumerator();
            //     while (myenum.MoveNext()) {
            //         var root = myenum.Current;
            //     }
            //
            //     moved = myenum.MoveNext();
            //
            //
            //     //test 4a
            //     IEnumerator<XdmNode> textNodes = foo.Children(Predicates.IsText()).GetEnumerator();
            //
            //
            //     //test 4b
            //     Assert.That(Predicates.IsElement(foo), "Check that node is an element node");
            //     moved = textNodes.MoveNext();
            //
            //     //test 4c
            //     Assert.That(Predicates.IsText().Func(textNodes.Current), "Check that node is a text node");
            //     string text = textNodes.Current.StringValue;
            //     assertAreEqual("text", text);
            // }
            // catch (Exception e) {
            //     Assert.Fail(e.Message);
            // }
        }

        /**
         * Test using declared variables
         */
        [Test]
        public void TestWithDeclaredVariables() {
            try {
                Processor p = new ();
                XPathCompiler c = p.NewXPathCompiler();
                c.DeclareVariable(new QName("a"));
                c.DeclareVariable(new QName("b"));
                XPathExecutable e = c.Compile("$a + $b");
                XPathSelector s = e.Load();
                s.SetVariable(new QName("a"), new XdmAtomicValue(2));
                s.SetVariable(new QName("b"), new XdmAtomicValue(2));
                object b = ((XdmAtomicValue) s.EvaluateSingle()).AsLong();
                Assert.That((object)4, Is.EqualTo(b));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        /**
         * Test using undeclared variables
         */
        [Test]
        public void TestWithUndeclaredVariables() {
            try {
                Processor p = new ();
                XPathCompiler c = p.NewXPathCompiler();
                c.AllowUndeclaredVariables = (true);
                XPathExecutable e = c.Compile("$a + $b");
                XPathSelector s = e.Load();
                for (IEnumerator iq = e.EnumerateExternalVariables(); iq.MoveNext();) {
                    QName q = (QName) iq.Current;
                    s.SetVariable(q, new XdmAtomicValue(2));
                }

                object b = ((XdmAtomicValue) s.EvaluateSingle()).AsLong();
                Assert.That((object)4, Is.EqualTo(b));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        /**
         * Test using undeclared variables when disallowed
         */
        [Test]
        public void TestWithDisallowedUndeclaredVariables() {
            try {
                Processor p = new Processor(false);
                XPathCompiler c = p.NewXPathCompiler();
                c.AllowUndeclaredVariables = (false);
                XPathExecutable e = c.Compile("$a + $b");
                XPathSelector s = e.Load();
                for (IEnumerator iq = e.EnumerateExternalVariables(); iq.MoveNext();) {
                    QName q = (QName) iq.Current;
                    s.SetVariable(q, new XdmAtomicValue(2));
                }

                Assert.Fail("No error reported for undeclared variables");
            }
            catch (Exception e) {
                Assert.That(true);
            }
        }
        
        /**
         * Test with statically-known collation
         */
        [Test]
        public void TestStaticallyKnownSaxonCollation() {
            try {
                Processor p = new ();
                XPathCompiler c = p.NewXPathCompiler();
                XPathExecutable e = c.Compile("compare('A', 'a', 'http://saxon.sf.net/collation?ignore-case=yes')");
                object b = ((XdmAtomicValue) e.Load().EvaluateSingle()).AsLong();
                Assert.That((object)0, Is.EqualTo(b));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        /**
         * Test with dynamically-computed collation URI
         */
        [Test]
        public void TestDynamicallyKnownSaxonCollation() {
            try {
                Processor p = new();
                XPathCompiler c = p.NewXPathCompiler();
                XPathExecutable e = c.Compile("compare('A', 'a',  " +
                                              "if (string(current-date()) eq '1900-01-01') then 'rubbish' else 'http://saxon.sf.net/collation?ignore-case=yes')");
                XdmAtomicValue result = ((XdmAtomicValue) e.Load().EvaluateSingle());
                object b = result.AsLong();
                Assert.That((object)0, Is.EqualTo(b));
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.Message + '\n' + e.StackTrace);
                Assert.Fail(e.Message);
            }
        }

        /**
         * Test with function-lookup
         */
        [Test]
        public void TestFunctionLookup() {
            try {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                XPathExecutable e =
                    c.Compile(
                        "year-from-date(function-lookup(QName('http://www.w3.org/2001/XMLSchema', 'date'), 1)('2001-12-31'))");
                object b = ((XdmAtomicValue) e.Load().EvaluateSingle()).AsLong();
                Assert.That((object)2001, Is.EqualTo(b));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        /**
         * Test for XPath 2.1 EQName syntax
         */
        [Test]
        public void TestEQNameAllowedIn30FunctionName() {
            try {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = ("3.0");
                XPathExecutable e = c.Compile("Q{http://www.w3.org/2001/XMLSchema}integer('1234')");
                XPathSelector s = e.Load();
                object b = ((XdmAtomicValue) s.EvaluateSingle()).AsLong();
                Assert.That((object)1234, Is.EqualTo(b));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestEQNameAllowedIn30NameTest() {
            try {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = ("3.0");
                DocumentBuilder builder = p.NewDocumentBuilder();
                builder.BaseUri = new Uri("http://example.com");
                XdmNode root = builder.Build(new StringReader("<a:doc xmlns:a='urn:a'>93</a:doc>"));
                XPathExecutable e = c.Compile("data(/Q{urn:a}doc)");
                XPathSelector s = e.Load();
                s.ContextItem = (root);
                Assert.That(s.GetEnumerator().MoveNext(), "element not found");
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        /**
         * Test for XPath 3.0 EQName syntax
         */
        [Test]
        public void TestEQNameDisallowedIn20FunctionName() {
            try {
                Processor p = new Processor(false);
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = ("3.0");
                c.Compile("'http://www.w3.org/2001/XMLSchema':integer('1234')");
                Assert.Fail("EQName syntax not allowed in 2.0, error not detected");
            }
            catch (Exception e) {
                // OK
            }
        }

        [Test]
        public void TestEQNameDisallowedIn20NameTest() {
            try {
                Processor p = new Processor(false);
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = ("3.0");
                XdmNode root = p.NewDocumentBuilder().Build(new StringReader("<a:doc xmlns:a='urn:a'>93</a:doc>"));
                XPathExecutable e = c.Compile("data(/'urn:a':doc)");
                XPathSelector s = e.Load();
                s.ContextItem = root;
                Assert.Fail("EQName syntax not allowed in 2.0, error not detected");
            }
            catch (Exception e) {
                //OK
            }
        }


        /**
         * Test "let" expression allowed in XPath 3.0
         */
        [Test]
        public void TestLetAllowedIn21() {
            try {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = ("3.0");
                XPathExecutable e = c.Compile("let $n := 5 return 1229 + $n");
                XPathSelector s = e.Load();
                object b = ((XdmAtomicValue) s.EvaluateSingle()).AsLong();
                Assert.That((object)1234, Is.EqualTo(b));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        /**
         * Test "let" expression disallowed in XPath 2.0
         */
        [Test]
        public void TestLetDisallowedIn20() {
            try {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = "2.0";
                XPathExecutable e = c.Compile("let $n := 5 return 1229 + $n");
                XPathSelector s = e.Load();
                Assert.Fail("No error reported for let in 2.0");
            }
            catch (Exception e) {
                // success
            }
        }

        /**
         * Test that it's OK (since 9.2) not to bind a value for a declared variable if the variable
         * isn't actually used
         */
        [Test]
        public void TestOkNotToBindUnusedButDeclaredVariable() {
            try {
                Processor p = new ();
                XPathCompiler c = p.NewXPathCompiler();
                c.DeclareVariable(new QName("vvv"));
                XPathExecutable e = c.Compile("2+2");
                XPathSelector s = e.Load();
                object b = ((XdmAtomicValue) s.EvaluateSingle()).AsLong();
                Assert.That((object)4, Is.EqualTo(b));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        /**
         * Test that it's not OK not to bind a value for a declared variable if the variable
         * is actually used
         */
        [Test]
        public void TestFailureToBindVariable() {
            try {
                Processor p = new Processor(false);
                XPathCompiler c = p.NewXPathCompiler();
                c.DeclareVariable(new QName("vvv"));
                XPathExecutable e = c.Compile("2+$vvv");
                XPathSelector s = e.Load();
                XdmAtomicValue result = (XdmAtomicValue) s.Evaluate();
                Assert.Fail("undetected Assert.Failure to bind variable");
            }
            catch (Exception e) {
                // Assert.Failure expected
            }
        }

        /**
         * Test use of required context item type
         */
        [Test]
        public void TestContextItemTypeOk() {
            try {
                Processor p = new ();
                XPathCompiler c = p.NewXPathCompiler();
                c.ContextItemType = XdmAtomicType.Integer;
                XPathExecutable e = c.Compile(".+3");
                XPathSelector s = e.Load();
                s.ContextItem = new XdmAtomicValue(2);
                XdmAtomicValue result = (XdmAtomicValue) s.Evaluate();
                Assert.That((object)result.AsLong(), Is.EqualTo(5));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        public void TestContextItemTypeNotOk() {
            try {
                Processor p = new Processor(false);
                XPathCompiler c = p.NewXPathCompiler();
                c.ContextItemType = XdmAtomicType.String;
                XPathExecutable e = c.Compile("number(.)+3");
                XPathSelector s = e.Load();
                s.ContextItem = (new XdmAtomicValue(2));
                XdmAtomicValue result = (XdmAtomicValue) s.Evaluate();
                Assert.Fail("Context item is wrong type - not detected");
            }
            catch (Exception e) {
                // Assert.Failure expected
            }
        }

        [Test]
        public void TestContextItemTypeNotConverted() {
            try {
                Processor p = new Processor(false);
                XPathCompiler c = p.NewXPathCompiler();
                c.ContextItemType = XdmAtomicType.Double;
                XPathExecutable e = c.Compile("number(.)+3");
                XPathSelector s = e.Load();
                s.ContextItem = new XdmAtomicValue(2);
                XdmAtomicValue result = (XdmAtomicValue) s.Evaluate();
                Assert.Fail("Context item is wrong type - not detected");
            }
            catch (Exception e) {
                // Assert.Failure expected
            }
        }

        [Test]
        public void TestContextItemTypeStaticError() {
            try {
                Processor p = new Processor(false);
                XPathCompiler c = p.NewXPathCompiler();
                c.ContextItemType = XdmAtomicType.Integer;
                XPathExecutable e = c.Compile("substring(., 2, 5)");
                Assert.Fail("Context item is wrong type - not detected");
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                // Assert.Failure expected
            }
        }


        [Test]
        public void TestStringLength() {
            // bug report from Norm Walsh, 2 Dec 2011
            try {
                string xpath = "boolean(string-length($href) > 0)";

                Processor processor = new Processor(true);
                Dictionary<string, string> nsBindings = new ();
                Dictionary<QName, string> globals = new ();

                globals.Add(new QName("", "href"), "test");

                XdmNode doc = null;
                IList<XdmItem> results = new List<XdmItem>();

                XPathCompiler xcomp = processor.NewXPathCompiler();

                foreach (QName varname in globals.Keys) {
                    xcomp.DeclareVariable(varname);
                }

                foreach (String prefix in nsBindings.Keys) {
                    xcomp.DeclareNamespace(prefix, nsBindings[prefix]);
                }

                XPathExecutable xexec = xcomp.Compile(xpath);
                XPathSelector selector = xexec.Load();

                foreach (QName varname in globals.Keys) {
                    XdmAtomicValue value = new ("irrelevant");
                    selector.SetVariable(varname, value);
                }

                try {
                    foreach (XdmItem result in selector) {
                        results.Add(result);
                    }
                }
                catch (Exception saue) {
                    throw saue;
                }

                Assert.That((object)1, Is.EqualTo(results.Count));
                object b = results[0].ToString();
                Assert.That((object)"true", Is.EqualTo(b));
            }
            catch (Exception e) {
                Console.Error.WriteLine(e.StackTrace);
                Assert.Fail();
            }
        }
        

        [Test]
        public void TestSchemaAwareEqualityGood() {
            try {
                bool result = trySchemaAwareEquality(true);
                Assert.That(result, "result");
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestSchemaAwareEqualityBad() {
            try {
                trySchemaAwareEquality(false);
                Assert.Fail();
            }
            catch (Exception e) {
                // success
            }
        }

        private bool trySchemaAwareEquality(bool isSchemaAware) {
            Processor processor = new Processor(true);
            SchemaManager sm = processor.SchemaManager;
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
            sm.Compile(new MemoryStream(Encoding.ASCII.GetBytes(xsd)), new Uri("http://www.example.com"));
            SchemaValidator sv = sm.NewSchemaValidator();
            XdmDestination xdmDest = new XdmDestination();
            sv.SetDestination(xdmDest);
            string xml = "<comparables xmlns=\"http://www.xpathtest.com/test\">\n" +
                         "  <comparable>\n" +
                         "    <string>hello</string>\n" +
                         "    <int>1</int>\n" +
                         "  </comparable>\n" +
                         "  <comparable>\n" +
                         "    <string>hello</string>\n" +
                         "    <int>1</int>\n" +
                         "  </comparable>\n" +
                         "</comparables>";
            sv.Validate(new StringReader(xml), new Uri("http://www.example.com"));
            string xpathExpr = "//test:comparable[1]/test:int eq //test:comparable[2]/test:int";
            XPathCompiler comp = processor.NewXPathCompiler();
            if (isSchemaAware) {
                comp.SchemaAware = true;
            }

            comp.DeclareNamespace("test", "http://www.xpathtest.com/test");
            XPathExecutable xpathExec = comp.Compile(xpathExpr);
            XPathSelector selector = xpathExec.Load();
            selector.ContextItem = xdmDest.XdmNode;
            return selector.EffectiveBooleanValue();
        }


        [Test, Explicit]  // Expect failure when the Crayfish server isn't available!
        public void TestExPathUNCPaths() {
            try {
                Processor p = new Processor(true);
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = "3.1";
                c.DeclareNamespace("file", "http://expath.org/ns/file");
                XPathExecutable e = c.Compile("file:exists('//crayfish/team/xmark/results8-8.xml')");
                XPathSelector s = e.Load();
                bool result = s.EffectiveBooleanValue();
                Assert.That(result);
            }
            catch (Exception e) {
                //Console.WriteLine(e + "\n" + e.StackTrace);
                Assert.Fail(e.Message);
            }
 
        }

        [Test]
        public void TestDefaultDecimalFormat() {
            try {
                Processor p = new ();
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = ("3.1");
                c.SetDecimalFormatProperty( "decimal-separator", "~");
                c.SetDecimalFormatProperty( "grouping-separator", "|");
                c.SetDecimalFormatProperty( "exponent-separator", "E");
                XPathExecutable e = c.Compile("format-number(123.4, '0|000~0E0')");
                XPathSelector s = e.Load();
                String result = s.EvaluateSingle().StringValue;
                Assert.That(result.Equals("1|234~0E-1"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void TestDecimalFormat() {
            try {
                Processor p = new ();
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = ("3.1");
                c.SetDecimalFormatProperty(new QName("deci"), "decimal-separator", "~");
                c.SetDecimalFormatProperty(new QName("deci"), "grouping-separator", "|");
                c.SetDecimalFormatProperty(new QName("deci"), "exponent-separator", "E");
                XPathExecutable e = c.Compile("format-number(123.4, '0|000~0E0', 'deci')");
                XPathSelector s = e.Load();
                String result = s.EvaluateSingle().StringValue;
                Assert.That(result.Equals("1|234~0E-1"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestBug3188() {
            String xpathString =
                "/xbrli:xbrl/xbrli:context[xbrli:scenario/xbrldi:typedMember[fn:resolve-QName(string(@dimension), .) =\n"
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

                DocumentBuilder docBuilder = processor.NewDocumentBuilder();
                // FileStream inputFile = new FileStream(ConfigTest.DATA_DIR + "xbrlInstance.xml", FileMode.Open);
                // docBuilder.BaseUri = new Uri(ConfigTest.DATA_DIR + "xbrlInstance.xml");
                XdmItem contextItem = docBuilder.Build(new Uri(ConfigTest.DATA_DIR + "xbrlInstance.xml"));

                XPathCompiler xpathCompiler = processor.NewXPathCompiler();
                xpathCompiler.DeclareNamespace("fn", "http://www.w3.org/2005/xpath-functions");
                xpathCompiler.DeclareNamespace("xbrli", "http://www.xbrl.org/2003/instance");
                xpathCompiler.DeclareNamespace("xbrldi", "http://xbrl.org/2006/xbrldi");
                xpathCompiler.DeclareNamespace("ocw-dim",
                    "http://www.nltaxonomie.nl/nt11/ocw/20170301.b/dictionary/ocw-axes");

                XdmValue result = xpathCompiler.Evaluate(xpathString, contextItem);
                Assert.That(result is XdmNode);
                Assert.That((object)((XdmNode) result).NodeKind, Is.EqualTo(XdmNodeKind.Element));
                Assert.That("context".Equals(((XdmNode) result).NodeName.LocalName));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        /**
         * round#2 is rejected under version 2.0
         */
        [Test]
        public void TestAbsent30Function() {
            try {
                Processor p = new ();
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = ("2.0");
                XPathExecutable e = c.Compile("round(1.234, 2)");
                XdmItem result = e.Load().EvaluateSingle();
                Assert.Fail("unexpected success");
            }
            catch (Saxon.Api.SaxonApiException e) {
                Assert.That((object)"XPST0017", Is.EqualTo(e.ErrorCode.LocalName));
            }
        }
        
        [Test]
        public void TestUnprefixedElementPolicy() {
            try {
                Processor p = new Processor();
                DocumentBuilder db = p.NewDocumentBuilder();
                XdmNode doc =
                    db.Build(new StringReader(
                        "<doc><p>one</p><p xmlns='n2'>two</p><n3:p xmlns:n3='n3'>three</n3:p></doc>"));
                XPathCompiler c = p.NewXPathCompiler();
                c.XPathLanguageVersion = ("3.1");
                XdmItem result = c.EvaluateSingle("string-join(//p)", doc);
                Assert.That((object)"one", Is.EqualTo(result.StringValue));
                c.UnprefixedElementMatchingPolicy = UnprefixedElementMatchingPolicy.AnyNamespace;
                result = c.EvaluateSingle("string-join(//p)", doc);
                Assert.That((object)"onetwothree", Is.EqualTo(result.StringValue));
                c.UnprefixedElementMatchingPolicy = UnprefixedElementMatchingPolicy.DefaultNamespace;
                c.DeclareNamespace("", "n2");
                result = c.EvaluateSingle("string-join(//p)", doc);
                Assert.That((object)"two", Is.EqualTo(result.StringValue));
                c.UnprefixedElementMatchingPolicy = UnprefixedElementMatchingPolicy.DefaultNamespaceOrNone;
                result = c.EvaluateSingle("string-join(//p)", doc);
                Assert.That((object)"onetwo", Is.EqualTo(result.StringValue));
            }
            catch (Exception e) {
                Assert.Fail();
            }
        }
    }
}