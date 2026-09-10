using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System.Text;
using Saxon.Api;
using NUnit.Framework;
using System.Xml;

namespace SaxonCS_NUnit {
    class TestPackage {
        [Test]
        public void TestSimple() {
            try {
                Processor proc = new Processor(true);
                XsltCompiler xc = proc.NewXsltCompiler();
                FileStream file1 = new FileStream(TestHelper.XsltTestSuitePath("decl/override/override-base-f-001.xsl"),
                    FileMode.Open);
                XsltPackage pack1 = xc.CompilePackage(file1);
                FileStream file2 = new FileStream(TestHelper.DataDirPath("test-package-001.pack"), FileMode.Create);
                pack1.Save(file2);

                Processor proc2 = new Processor(true);
                XsltCompiler xc2 = proc2.NewXsltCompiler();
                XsltPackage xp = xc2.LoadLibraryPackage(TestHelper.DataDirUri("test-package-001.pack"));
                xc2.ImportPackage(xp);
                FileStream file3 = new FileStream(TestHelper.XsltTestSuitePath("decl/override/override-f-001.xsl"),
                    FileMode.Open);
                XsltExecutable xe = xc2.Compile(file3);
                Xslt30Transformer xt = xe.Load30();
                XdmValue result = xt.CallTemplate(new QName("main"));


                file1.Close();
                file2.Close();
                Assert.True(true);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestPackage1() {
            try {
                Processor processor = new Processor(true);

                XsltCompiler xsltCompiler = processor.NewXsltCompiler();
                xsltCompiler.XsltLanguageVersion = "3.0";
                XsltPackage packageA;
                using (Stream stream = File.OpenRead(TestHelper.DataDirPath("packageA.xsl")))
                    packageA = xsltCompiler.CompilePackage(stream);

                xsltCompiler = processor.NewXsltCompiler();
                xsltCompiler.XsltLanguageVersion = "3.0";
                xsltCompiler.ImportPackage(packageA);
                XsltPackage packageB;
                using (Stream stream = File.OpenRead(TestHelper.DataDirPath("packageB.xsl")))
                    packageB = xsltCompiler.CompilePackage(stream);

                xsltCompiler = processor.NewXsltCompiler();
                xsltCompiler.XsltLanguageVersion = "3.0";
                xsltCompiler.ImportPackage(packageA);
                xsltCompiler.ImportPackage(packageB);
                XsltExecutable exec =
                    xsltCompiler.Compile(TestHelper.DataDirUri("testPackage.xsl"));
                Xslt30Transformer transf = exec.Load30();
                Serializer serializer = processor.NewSerializer();
                StringWriter sw = new StringWriter();
                serializer.OutputWriter = sw;
                serializer.SetOutputProperty(Serializer.METHOD, "text");

                transf.CallTemplate(null, serializer);
                string res = sw.ToString();
                Assert.NotNull(res);
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Assert.Fail();
            }
        }
    }
}