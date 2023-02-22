using System;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;
using Saxon.Api;
using Saxon.Ejava.net;
using Saxon.Hej;
using Exception = System.Exception;

namespace SaxonCS_NUnit {
    class TestXQueryCompiler {


        [Test]
        public void TestModuleUriResolverNoBaseUri() {
            try {
                Processor p = new();
                XQueryCompiler c = p.NewXQueryCompiler();
                c.XQueryResolver = ((moduleURI, baseURI, locations) => {
                    string module = moduleURI switch {
                        "b.uri" => "module namespace b='b.uri'; import module namespace c = 'c.uri' at 'c.xq'; " +
                                   "declare variable $b:b := $c:c;",
                        "c.uri" => "module namespace c='c.uri';" + "declare variable $c:c := 42;",
                        _ => ""
                    };

                    return new[] {
                        new TextResource(module,  new Uri("http://dummy.com/"))
                    };
                });
                XQueryExecutable exec = c.Compile("import module namespace b = 'b.uri' at 'b.xq'; $b:b");
                XdmValue val = exec.Load().Evaluate();
                Assert.AreEqual(42L, ((XdmAtomicValue) val).AsLong());
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestDefaultNamespace() {
            try {
                Processor p = new ();
                DocumentBuilder b = p.NewDocumentBuilder();
                XdmNode foo = b.Build(new StringReader("<foo xmlns='http://one.uri/'><bar/></foo>"));
                XQueryCompiler c = p.NewXQueryCompiler();
                c.DeclareNamespace("", "http://one.uri/");
                XQueryEvaluator eval = c.Compile("/foo").Load();
                eval.ContextItem = (foo);
                XdmValue val = eval.Evaluate();
                Assert.AreEqual(1, val.Count);
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }

        }

        [Test]
        public void TestReusability() {
            // Test that the XQuery compiler can compile two queries without interference
            try {
                Processor p = new ();
                DocumentBuilder b = p.NewDocumentBuilder();
                XdmNode foo = b.Build(new StringReader(
                    "<foo xmlns='http://one.uri/'><bar xmlns='http://two.uri'>12</bar></foo>"));
                XQueryCompiler c = p.NewXQueryCompiler();
                c.DeclareNamespace("", "http://one.uri/");
                XQueryExecutable q1 = c.Compile("declare variable $p as xs:boolean external; exists(/foo) = $p");
                c.DeclareNamespace("", "http://two.uri");
                XQueryExecutable q2 = c.Compile("declare variable $p as xs:integer external; /*/bar + $p");
                XQueryEvaluator e1 = q1.Load();
                XQueryEvaluator e2 = q2.Load();
                e1.ContextItem = (foo);
                e1.SetExternalVariable(new QName("p"), new XdmAtomicValue(true));
                XdmValue val = e1.Evaluate();
                Assert.That(((XdmAtomicValue) val).AsBoolean());
                e2.ContextItem = (foo);
                e2.SetExternalVariable(new QName("p"), new XdmAtomicValue(6));
                val = e2.Evaluate();
                Assert.AreEqual(18, ((XdmAtomicValue) val).AsLong());
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }

        }

        [Test]
        public void TestMultithreading() {
            // Test that the XQuery compiler can compile many queries concurrently without interference
            try {
                Processor p = new();
                DocumentBuilder b = p.NewDocumentBuilder();
                XdmNode foo = b.Build(new StringReader(
                    "<foo xmlns='http://one.uri/'><bar xmlns='http://two.uri/'>12</bar></foo>"));
                XQueryCompiler c = p.NewXQueryCompiler();
                c.DeclareNamespace("a", "http://one.uri/");
                c.DeclareNamespace("b", "http://two.uri/");
                Parallel.For(0, 100, (i) => {
                    try {
                        XQueryEvaluator q = c.Compile("a:foo/b:bar + a:foo/b:bar + a:foo/b:bar + a:foo/b:bar").Load();
                        q.ContextItem = (foo);
                        XdmAtomicValue v = (XdmAtomicValue) q.EvaluateSingle();
                        Assert.AreEqual("48", v.ToString(), "ok");
                    }
                    catch (Exception e) {
                        //e.printStackTrace();
                        Assert.Fail();
                    }
                });
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }

        }



        [Test]
        public void TestSeparateCompilation() {
            try {
                Processor proc = new Processor(true);
                XQueryCompiler comp = proc.NewXQueryCompiler();
                comp.CompileLibrary("module namespace m='http://www.example.com/module-one'; " +
                                    "declare function m:add($a as xs:integer, $b as xs:integer) as xs:integer { $a + $b };" +
                                    "declare variable $m:ten as xs:integer := 10;");
                string q1 = "import module namespace m='http://www.example.com/module-one'; m:add(2,2)";
                XQueryEvaluator ev = comp.Compile(q1).Load();
                Assert.AreEqual("4", ev.Evaluate().ToString(), "2+2");
                string q2 = "import module namespace m='http://www.example.com/module-one'; m:add($m:ten,5)";
                ev = comp.Compile(q2).Load();
                Assert.AreEqual("15", ev.Evaluate().ToString(), "10+5");
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestSeparateCompilationNested() {
            try {
                Processor proc = new();
                XQueryCompiler comp = proc.NewXQueryCompiler();
                comp.CompileLibrary("module namespace m1='http://www.example.com/module-one'; " +
                                    "declare function m1:add($a as xs:integer, $b as xs:integer) as xs:integer { $a + $b };" +
                                    "declare variable $m1:ten as xs:integer := 10;");
                comp.CompileLibrary("module namespace m2='http://www.example.com/module-two'; " +
                                    "import module namespace m1='http://www.example.com/module-one';" +
                                    "declare function m2:add($a as xs:integer, $b as xs:integer) as xs:integer { m1:add($a, $b) };" +
                                    "declare variable $m2:ten as xs:integer := $m1:ten;");
                const string q1 = "import module namespace m2='http://www.example.com/module-two'; m2:add($m2:ten,2)";
                XQueryEvaluator ev = comp.Compile(q1).Load();
                Assert.AreEqual("12", ev.Evaluate().ToString(), "10+2");
                const string q2 = "import module namespace m1='http://www.example.com/module-one'; m1:add($m1:ten,5)";
                ev = comp.Compile(q2).Load();
                Assert.AreEqual("15", ev.Evaluate().ToString(), "10+5");
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestSeparateCompilationImportFromNonSeparatelyCompiledLibraryModule() {
            try {
                Processor proc = new();
                XQueryCompiler comp = proc.NewXQueryCompiler();
                comp.BaseUri = new Uri("http://www.example.com/module-one");
                comp.CompileLibrary("module namespace m1='http://www.example.com/module-one'; " +
                                    "declare function m1:add($a as xs:integer, $b as xs:integer) as xs:integer { $a + $b };" +
                                    "declare variable $m1:ten as xs:integer := 10;");
                const string m2 = "module namespace m2='http://www.example.com/module-two'; " +
                                  "import module namespace m1='http://www.example.com/module-one';" +
                                  "declare function m2:add($a as xs:integer, $b as xs:integer) as xs:integer { m1:add($a, $b) };" +
                                  "declare variable $m2:ten as xs:integer := $m1:ten;";
                const string q1 = "import module namespace m2='http://www.example.com/module-two'; m2:add($m2:ten,2)";
                comp.XQueryResolver = (moduleUri, baseURI, locations) => {
                    return moduleUri == "http://www.example.com/module-two" 
                        ? new[] {new TextResource(m2, new Uri("http://www.example.com/module-two"))} 
                        : null;
                };
                XQueryEvaluator ev = comp.Compile(q1).Load();
                Assert.AreEqual("12", ev.Evaluate().ToString(), "10+2");
                const string q2 = "import module namespace m1='http://www.example.com/module-one'; m1:add($m1:ten,5)";
                ev = comp.Compile(q2).Load();
                Assert.AreEqual("15", ev.Evaluate().ToString(), "10+5");
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestSeparateCompilationDiamond() {
            try {
                Processor proc = new Processor(true);
                XQueryCompiler comp = proc.NewXQueryCompiler();

                comp.BaseUri = (new Uri("http://www.example.com/module-one"));
                comp.CompileLibrary("module namespace m1='http://www.example.com/module-one'; " +
                                    "declare function m1:add($a as xs:integer, $b as xs:integer) as xs:integer { $a + $b };");

                comp.BaseUri = (new Uri("http://www.example.com/module-two"));
                comp.CompileLibrary("module namespace m2='http://www.example.com/module-two'; " +
                                    "import module namespace m1='http://www.example.com/module-one';" +
                                    "declare function m2:subtract($a as xs:integer, $b as xs:integer) as xs:integer { m1:add($a, -$b) };");
                try {
                    comp.BaseUri = new Uri("http://www.example.com/module-three");
                }
                catch (URISyntaxException e) {
                    //
                }

                comp.CompileLibrary("module namespace m3='http://www.example.com/module-three'; " +
                                    "import module namespace m1='http://www.example.com/module-one';" +
                                    "declare function m3:times($a as xs:integer, $b as xs:integer) as xs:integer { " +
                                    "   if ($b=1) then $a else m1:add($a, m3:times($a, $b - 1)) };");
                const string q1 = "import module namespace m2='http://www.example.com/module-two'; " +
                                  "import module namespace m3='http://www.example.com/module-three';" +
                                  "m2:subtract(m3:times(4,2),1)";
                XQueryEvaluator ev = comp.Compile(q1).Load();
                Assert.AreEqual("7", "7", ev.Evaluate().ToString());
                const string q2 = "import module namespace m2='http://www.example.com/module-two'; " +
                                  "import module namespace m3='http://www.example.com/module-three';" +
                                  "m2:subtract(m3:times(5,2),1)";
                XQueryEvaluator ev2 = comp.Compile(q2).Load();
                Assert.AreEqual("9", ev2.Evaluate().ToString());
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestSeparateCompilationDiamondByteCode() {
            try {
                Processor proc = new Processor(true);
                XQueryCompiler comp = proc.NewXQueryCompiler();

                comp.BaseUri = new Uri("http://www.example.com/module-one");
                comp.CompileLibrary("module namespace m1='http://www.example.com/module-one'; " +
                                    "declare function m1:add($a as xs:integer, $b as xs:integer) as xs:integer { $a + $b };");
                comp.BaseUri = new Uri("http://www.example.com/module-two");
                comp.CompileLibrary("module namespace m2='http://www.example.com/module-two'; " +
                                    "import module namespace m1='http://www.example.com/module-one';" +
                                    "declare function m2:subtract($a as xs:integer, $b as xs:integer) as xs:integer { m1:add($a, -$b) };");
                comp.BaseUri = new Uri("http://www.example.com/module-three");
                comp.CompileLibrary("module namespace m3='http://www.example.com/module-three'; " +
                                    "import module namespace m1='http://www.example.com/module-one';" +
                                    "declare function m3:times($a as xs:integer, $b as xs:integer) as xs:integer { " +
                                    "   if ($b=1) then $a else m1:add($a, m3:times($a, $b - 1)) };");
                const string q1 = "import module namespace m2='http://www.example.com/module-two'; " +
                                  "import module namespace m3='http://www.example.com/module-three';" +
                                  "m2:subtract(m3:times(4,2),1)";
                XQueryEvaluator ev = comp.Compile(q1).Load();
                Assert.AreEqual("7", ev.Evaluate().ToString());
                const string q2 = "import module namespace m2='http://www.example.com/module-two'; " +
                                  "import module namespace m3='http://www.example.com/module-three';" +
                                  "m2:subtract(m3:times(5,2),1)";
                XQueryEvaluator ev2 = comp.Compile(q2).Load();
                Assert.AreEqual("9", ev2.Evaluate().ToString());
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestSeparateCompilationDiamondWithVariables() {
            try {
                Processor proc = new();
                XQueryCompiler comp = proc.NewXQueryCompiler();

                comp.CompileLibrary("module namespace m1='http://www.example.com/module-one'; " +
                                    "declare function m1:add($a as xs:integer, $b as xs:integer) as xs:integer { $a + $b };" +
                                    "declare variable $m1:ten := 10;"
                );

                comp.CompileLibrary("module namespace m2='http://www.example.com/module-two'; " +
                                    "import module namespace m1='http://www.example.com/module-one';" +
                                    "declare function m2:subtract($a as xs:integer, $b as xs:integer) as xs:integer { m1:add($a, -$b) };" +
                                    "declare variable $m2:eleven := $m1:ten + 1;");

                comp.CompileLibrary("module namespace m3='http://www.example.com/module-three'; " +
                                    "import module namespace m1='http://www.example.com/module-one';" +
                                    "declare function m3:times($a as xs:integer, $b as xs:integer) as xs:integer { " +
                                    "   if ($b=1) then $a else m1:add($a, m3:times($a, $b - 1)) };" +
                                    "declare variable $m3:twelve := $m1:ten + 2;");

                const string q1 = "import module namespace m2='http://www.example.com/module-two'; " +
                                  "import module namespace m3='http://www.example.com/module-three';" +
                                  "m2:subtract(m3:times(4,2),1) + $m3:twelve";
                XQueryEvaluator ev = comp.Compile(q1).Load();
                Assert.AreEqual("19", ev.Evaluate().ToString());
                const string q2 = "import module namespace m2='http://www.example.com/module-two'; " +
                                  "import module namespace m3='http://www.example.com/module-three';" +
                                  "m2:subtract(m3:times(5,2),1) + $m2:eleven";
                XQueryEvaluator ev2 = comp.Compile(q2).Load();
                Assert.AreEqual("20", ev2.Evaluate().ToString());
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestSeparateCompilationForwardsRef() {
            try {
                Processor proc = new();
                XQueryCompiler comp = proc.NewXQueryCompiler();
                comp.CompileLibrary(
                    "module namespace lib = 'http://example.com/library';" +
                    "declare function lib:foo($a) { lib:bar($a) };" +
                    "declare function lib:bar($b) { concat($b, '!') };");

                XQueryExecutable exp = comp.Compile(
                    "import module namespace lib = 'http://example.com/library';"
                    + "\nlib:foo('a')"
                );

                XdmItem result = exp.Load().EvaluateSingle();
                Assert.AreEqual("a!", result.StringValue);

            }
            catch (SaxonApiException err) {
                Assert.Fail(err.Message);
            }
        }


        // /**
        //  * s9api query/transformation pipeline
        //  */
        //
        // [Test]
        // public void TestPipeline() {
        //     try {
        //         Processor proc = new ();
        //         XsltCompiler tComp = proc.NewXsltCompiler();
        //         XQueryCompiler qComp = proc.NewXQueryCompiler();
        //         StringWriter sw = new StringWriter();
        //         Serializer @out = proc.NewSerializer(sw);
        //         const string xsl = "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'>" +
        //                            "<xsl:template match='/'><t><xsl:copy-of select='.'/></t></xsl:template>" +
        //                            "</xsl:stylesheet>";
        //         const string q = "document { <q>{.}</q> }";
        //         const string xml = "<z/>";
        //         XdmNode @in = proc.NewDocumentBuilder().Build(new StringReader(xml));
        //         XsltTransformer stage1 = tComp.Compile(new StringReader(xsl)).Load();
        //         XQueryEvaluator stage2 = qComp.Compile(q).Load();
        //         XsltTransformer stage3 = tComp.Compile(new StringReader(xsl)).Load();
        //         XQueryEvaluator stage4 = qComp.Compile(q).Load();
        //         XsltTransformer stage5 = tComp.Compile(new StringReader(xsl)).Load();
        //         stage1.Destination = (stage2);
        //         stage2.Destination = (stage3);
        //         stage3.Destination = (stage4);
        //         stage4.Destination = (stage5);
        //         stage5.Destination = (@out);
        //         stage1.InitialContextNode = (@in);
        //         stage1.Run();
        //         Assert.That(sw.ToString().Contains("<t><q><t><q><t><z/></t></q></t></q></t>"), "output");
        //
        //     } catch (SaxonApiException err) {
        //         //err.printStackTrace();
        //         Assert.Fail(err.Message);
        //     }
        // }


        [Test]
        public void TestIssue2111() {
            try {
                Processor processor = new ();
                XQueryCompiler xqueryCompiler = processor.NewXQueryCompiler();
                xqueryCompiler.XQueryResolver = (moduleURI, baseURI, locations) => {
                    const string q = "module namespace x='x'; declare variable $x:x := 'x';";
                    return new [] {new TextResource(q, new Uri("http://dummy/"))};
                };

                xqueryCompiler.BaseUri = new Uri(Environment.CurrentDirectory);
                XQueryExecutable executable = xqueryCompiler.Compile("import module namespace x='x'; $x:x\n");
                executable.Load().Run(processor.NewSerializer(Console.Out));

            }
            catch (SaxonApiException err) {
                Assert.Fail();
            }

        }

        [Test]
        public void TestAbleToCompileWithCompileLibraryMethod() {
            // See bug 2429
            XQueryCompiler compiler = new Processor(true).NewXQueryCompiler();
            const string main = "" +
                                "module namespace main = 'https://example.com/main_mod.xq';\n" +
                                "import module namespace vi_imp = 'https://example.com/imported_mod.xq';\n" +
                                "declare %private variable $main:variableThatIsInstantiatedTwice as xs:integer := 123;";

            compiler.XQueryResolver = (moduleURI, baseURI, locations) => {
                string module = moduleURI switch {
                    "https://example.com/imported_mod.xq" => 
                        "module namespace vi_imp = 'https://example.com/imported_mod.xq';\n" +
                        "import module namespace vi_main = 'https://example.com/main_mod.xq' at 'file:///nowhere/variable_issue_main.xq';",
                    "https://example.com/main_mod.xq" => main,
                    _ => ""
                };

                return new[] {new TextResource(module, new Uri(moduleURI))};
            };
            compiler.CompileLibrary(main);
        }

        [Test]
        public void TestGlobalVariableLineNumber() {
            try {
                string query = "declare namespace a = 'b';\n\n declare variable $foo as xs:integer external := 3; $foo";
                Processor p = new ();
                XQueryCompiler c = p.NewXQueryCompiler();
                XQueryExecutable q = c.Compile(query);
                var globals = q.GetExternalVariables();
                ParameterDetails details = globals[new QName("foo")];
                Assert.AreEqual(3, details.Location.LineNumber);
                Assert.False(details.Required);
                Assert.AreEqual(1, globals.Count);
            }
            catch (SaxonApiException e) {
                Assert.Fail(e.Message);
            }

        }



        [Test]
        public void TestDiagnosticsOnAtomizationFailure() {
            // Bug 4086
            try {
                Processor p = new();
                DocumentBuilder b = p.NewDocumentBuilder();
                XdmNode foo = b.Build(new StringReader("<foo xmlns='http://one.uri/'><bar/></foo>"));
                XQueryCompiler c = p.NewXQueryCompiler();
                c.DeclareNamespace("", "http://one.uri/");
                XQueryEvaluator eval =
                    c.Compile(
                            "declare function local:x($n as node(), $a as xs:integer){$a+string-length(name($n))}; local:x(<z/>, map{'x':2})")
                        .Load();
                XdmValue val = eval.Evaluate();
                Assert.Fail();
            }
            catch (SaxonApiException e) {
                Console.Error.WriteLine(e);
            }
        }

        [Test]
        public void TestXQueryOptimizationWithTracing() {
            // Bug 3497

            const string query = "declare variable $root := <root><text>AAA</text></root>;" +
                                 "declare function local:foo($root) {$root/text};" +
                                 "local:foo($root)";

            try {
                Processor p = new();
                XQueryCompiler c = p.NewXQueryCompiler();
                c.CompileWithTracing = (true);
                XQueryEvaluator eval = c.Compile(query).Load();
                XdmValue val = eval.Evaluate();
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail();
            }
        }

    }
}

