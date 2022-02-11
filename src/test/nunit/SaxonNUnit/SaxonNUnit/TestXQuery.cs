using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saxon.Api;

namespace Saxon9.NUnit
{
    class TestXQuery
    {

        string sourcePath = Path.GetFullPath("C:\\work\\misc\\xmark\\xmark10.xml");
        string stylesheetPath = Path.GetFullPath("C:\\work\\misc\\xmark\\q12.xq");


        [Test]
        public void TestXXX() {
            try {
                Processor processor = new Processor();
                DocumentBuilder documentBuilder = processor.NewDocumentBuilder();
                documentBuilder.BaseUri = new Uri("http://example.com");
                documentBuilder.IsLineNumbering = true;
                documentBuilder.WhitespacePolicy = WhitespacePolicy.PreserveAll;
                XQueryCompiler compiler = processor.NewXQueryCompiler();
                string query = "//listitem";// BuildXqueryString();
                var source = File.OpenRead(sourcePath);

                if (!String.IsNullOrEmpty(query))
                {
                    XQueryExecutable executable = compiler.Compile(query);
                    XQueryEvaluator evaluator = executable.Load();
                    XdmNode xmlNode = documentBuilder.Build(source);
                    evaluator.ContextItem = xmlNode;
                    var evaluations = evaluator.Evaluate();
                    var evaluationsAsList = evaluations.GetList();
                    //List<XdmNode> evaluationsAsList = evaluations.GetList();//.Cast < XdmNode().ToList()>;
                    Boolean check = false;
                    foreach (var evaluation in evaluationsAsList)
                    {
                        if (evaluation is XdmNode)
                        {
                            check = true;
                        }
                        else {
                            check = false;
                            Assert.Fail("Item is not of Type XdmNode");
                        }
                        //runEvaluation((XdmNode)evaluation, document);
                    }
                    if (!check) {
                        Assert.Fail("Item is not of Type XdmNode");
                    }
                }
            }
            catch (Exception ex) {
                Assert.Fail(ex.Message);
            }
        }

        [Test]
        public void TestRegularExpression()
        {
            try
            {
                Processor processor = new Processor();
                DocumentBuilder documentBuilder = processor.NewDocumentBuilder();
                documentBuilder.BaseUri = new Uri("http://example.com");
                documentBuilder.IsLineNumbering = true;
                documentBuilder.WhitespacePolicy = WhitespacePolicy.PreserveAll;
                XQueryCompiler compiler = processor.NewXQueryCompiler();
                string query = "matches('qUery', 'q u', 'xi;n')";// BuildXqueryString();
                

                if (!String.IsNullOrEmpty(query))
                {
                    XQueryExecutable executable = compiler.Compile(query);
                    XQueryEvaluator evaluator = executable.Load();
                    
                   
                    var evaluations = evaluator.Evaluate();
                    var evaluation = evaluations.Simplify;
                    //List<XdmNode> evaluationsAsList = evaluations.GetList();//.Cast < XdmNode().ToList()>;
                    Boolean check = false;
                    
                        if (evaluation is XdmAtomicValue)
                        {
                            check = ((XdmAtomicValue)evaluation).GetBooleanValue();
                        }
                        else
                        {
                            check = false;
                            Assert.Fail("Item is not of Type XdmAtomicValue");
                        }
                        //runEvaluation((XdmNode)evaluation, document);
                   
                    if (!check)
                    {
                        Assert.Fail("failed regular expression");
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }


        [Test]
        public void TestMemoryRelease()
        {
            var tasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    Processor processor = new Processor();
                    DocumentBuilder documentBuilder = processor.NewDocumentBuilder();
                    documentBuilder.IsLineNumbering = true;
                    documentBuilder.WhitespacePolicy = WhitespacePolicy.PreserveAll;
                    XQueryCompiler compiler = processor.NewXQueryCompiler();
                    using (var source = File.OpenRead(sourcePath))
                    using (var queryFile = File.OpenRead(stylesheetPath))
                    {

                        XQueryExecutable executable = compiler.Compile(queryFile);
                        XQueryEvaluator evaluator = executable.Load();
                        documentBuilder.BaseUri = new Uri("http://example.com");
                        evaluator.ContextItem = documentBuilder.Build(source);
                        var evaluations = evaluator.Evaluate();

                    }
                }
        );

            }
        }
    }
}
