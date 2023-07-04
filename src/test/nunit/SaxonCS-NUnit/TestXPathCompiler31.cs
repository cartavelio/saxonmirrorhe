using System;
using System.Globalization;
using System.IO;
using Saxon.Api;
using NUnit.Framework;


namespace SaxonCS_NUnit {
    class TestXPathCompiler31 {
        Processor proc = new Processor(true);
        XPathCompiler compiler;
        XdmNode document;

        private XPathCompiler GetCompiler() {
            return compiler ??= proc.NewXPathCompiler();
        }


        public TestXPathCompiler31() {
            try {
                GetCompiler().XPathLanguageVersion = "3.1";
                DocumentBuilder b = proc.NewDocumentBuilder();
                b.BaseUri = new Uri("http://www.example.com");
                document = b.Build(new StringReader("<foo x='1'><bar/><baz/></foo>"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        private string good(string expr) {
            try {
                XPathExecutable exec = GetCompiler().Compile(expr);
                XPathSelector selector = exec.Load();
                selector.ContextItem = document;
                return selector.Evaluate().ToString();
            }
            catch (Exception e1) {
                Assert.Fail(e1.Message);
                return null;
            }
        }

        private void bad(string expr) {
            try {
                XPathExecutable exec = GetCompiler().Compile(expr);
                XPathSelector selector = exec.Load();
                selector.ContextItem = (document);
                Assert.Fail("unexpected success");
            }
            catch (Exception e1) {
                // Failure was expected
            }
        }

        [Test]
        public void TestArith() {
            string result = good("2+2");
            Assert.AreEqual("4", result);
        }

        [Test]
        public void TestNumberFormat() {
            //CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("de-DE");
            //CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("de-DE");
            decimal d = 2.5m;
            //String value2 = d.ToString(System.Globalization.CultureInfo.InvariantCulture);
            XdmAtomicValue value = new XdmAtomicValue(d);
            Assert.AreEqual("2.5", value.ToString());
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("en-GB");
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("en-GB");
        }

        [Test]
        public void TestIf() {
            string result = good("if (//*) then 3 else 4");
            Assert.AreEqual("3", result);
        }

        [Test]
        public void TestFor() {
            String result = good("string-join(for $i in 1 to 4 return $i, '-')");
            Assert.AreEqual("1-2-3-4", result);
        }

        [Test]
        public void TestExists() {
            String result = good("exists(//*)");
            Assert.AreEqual("true", result);
        }

        [Test]
        public void TestBang() {
            String result = good("string-join((1 to 3)!(. + 1), '-')");
            Assert.AreEqual("2-3-4", result);
        }

        [Test]
        public void TestConcat() {
            String result = good("'abc' || 'def'");
            Assert.AreEqual("abcdef", result);
        }

        [Test]
        public void TestLet() {
            String result = good("let $x := 3 return $x + 1");
            Assert.AreEqual("4", result);
        }

        [Test]
        public void TestInlineFunctions() {
            String result = good("function($x){$x + 1}(3)");
            Assert.AreEqual("4", result);
        }

        [Test]
        public void TestNoAnnotations() {
            bad("%private function($x){$x + 1}(3)");
        }

        [Test]
        public void TestDynamicFunctionCall() {
            String result = good("exists#1(3)");
            Assert.AreEqual("true", result);
        }

        [Test]
        public void TestEQNames() {
            String result = good("Q{http://www.w3.org/2005/xpath-functions}exists(3)");
            Assert.AreEqual("true", result);
        }

        [Test]
        public void TestNoConstructors() {
            bad("<a/>");
        }

        [Test]
        public void TestMaps() {
            String result = good("map{1:'a'}?1");
            Assert.AreEqual("a", result);
        }

        [Test]
        public void TestMapType() {
            String result = good("map{1:'a'} instance of map(*)");
            Assert.AreEqual("true", result);
        }

        [Test]
        public void TestArrays() {
            String result = good("['1a','2a','3a','4a']?2");
            Assert.AreEqual("2a", result);
        }

        [Test]
        public void TestArrayType() {
            String result = good("['1a','2a','3a','4a'] instance of array(*)");
            Assert.AreEqual("true", result);
        }

        [Test]
        public void TestNoWhere() {
            bad("for $i in 1 to 10 where $i gt 3 return $i");
        }

        [Test]
        public void TestNoOrderBy() {
            bad("for $i in 1 to 10 order by -$i return $i");
        }

        [Test]
        public void TestNoCurrent() {
            bad("current()");
        }

        [Test]
        public void TestRound2() {
            String result = good("round(12.341, 2)");
            Assert.AreEqual("12.34", result);
        }

        [Test]
        public void TestGenerateId() {
            String result = good("generate-id(/)");
            Assert.True(result.Length > 0);
        }

        [Test]
        public void TestQFlag() {
            String result = good("matches('quid', 'quod', 'q')");
            Assert.AreEqual("false", result);
        }

        [Test]
        public void TestNoNonCapturingGroups() {
            String result = good("replace('abcdef', '(?:abc)(def)', '$1')");
            Assert.AreEqual("def", result);
        }

        [Test]
        public void TestMapFunctions() {
            String result =
                good(
                    "Q{http://www.w3.org/2005/xpath-functions/map}get(Q{http://www.w3.org/2005/xpath-functions/map}entry('a', 'b'), 'a')");
            Assert.AreEqual("b", result);
        }

        [Test]
        public void TestArrayFunctions() {
            String result =
                good(
                    "Q{http://www.w3.org/2005/xpath-functions/array}flatten(Q{http://www.w3.org/2005/xpath-functions/array}append([], '12'))");
            Assert.AreEqual("12", result);
        }

        [Test]
        public void TestDefaultLanguage() {
            String result = good("default-language()");
            Assert.True(result.StartsWith("en"));
        }

        [Test]
        public void TestBug4081() {
            String xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                         + "<Employees>\n"
                         + "    <Employee id=\"1\">" + Environment.NewLine
                         + "        <age:ag xmlns:age=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">29</age:ag>" +
                         Environment.NewLine
                         + "        <name>Pankaj</name>" + Environment.NewLine
                         + "        <gender>Male</gender>" + Environment.NewLine
                         + "        <role>Java Developer</role>" + Environment.NewLine
                         + "    </Employee>" + Environment.NewLine
                         + "    <Employee id=\"2\">" + Environment.NewLine
                         + "        <age:ag xmlns:age=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">35</age:ag>" +
                         Environment.NewLine
                         + "        <name>Lisa</name>" + Environment.NewLine
                         + "        <gender>Female</gender>" + Environment.NewLine
                         + "        <role>CEO</role>" + Environment.NewLine
                         + "    </Employee>" + Environment.NewLine
                         + "    <Employee id=\"3\">" + Environment.NewLine
                         + "        <age:ag xmlns:age=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">45</age:ag>" +
                         Environment.NewLine
                         + "        <name>Tom</name>" + Environment.NewLine
                         + "        <gender>Male</gender>" + Environment.NewLine
                         + "        <role>Manager</role>" + Environment.NewLine
                         + "    </Employee>@\n"
                         + "    <Employee id=\"4\">" + Environment.NewLine
                         + "        <age:ag xmlns:age=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">55</age:ag>" +
                         Environment.NewLine
                         + "        <name>Meghan</name>" + Environment.NewLine
                         + "        <gender>Female</gender>" + Environment.NewLine
                         + "        <role>Manager</role>" + Environment.NewLine
                         + "    </Employee>" + Environment.NewLine
                         + "</Employees>";


            String xpath = "//Employees/Employee[1]/age:ag";
            try {
                Processor proc = new ();
                XPathCompiler c = proc.NewXPathCompiler();
                c.DeclareNamespace("age", "http://www.w3.org/2003/01/geo/wgs84_pos#");
                XPathExecutable e = c.Compile(xpath);
                XPathSelector s = e.Load();
                DocumentBuilder builder = proc.NewDocumentBuilder();
                builder.BaseUri = new Uri("http://www.example.com");
                s.ContextItem = (builder.Build(new StringReader(xml)));
                XdmItem val = s.EvaluateSingle();
                Assert.AreEqual("29", val.StringValue);
                Assert.AreEqual("<age:ag xmlns:age=\"http://www.w3.org/2003/01/geo/wgs84_pos#\">29</age:ag>",
                    val.ToString());
            }
            catch (Exception) {
                Assert.Fail();
            }
        }
    }
}