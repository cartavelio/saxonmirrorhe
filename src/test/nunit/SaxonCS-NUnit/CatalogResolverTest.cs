using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Saxon.Api;
using NUnit.Framework;
using NUnit.Framework.Internal;
using Org.XmlResolver.Features;
using Saxon.Callbacks;
using Saxon.Eej.config;
using Saxon.Ejava.util;
using Saxon.Ejavax.xml.transform.sax;
using Saxon.Hej;

namespace SaxonCS_NUnit {
    public class CatalogResolverTest {
        [Test]
        public void noResolverFail() {
            Processor processor = new Processor();
            DocumentBuilder builder = processor.NewDocumentBuilder();
            builder.DtdValidation = true;
            try {
                XdmNode doc = builder.Build(new FileStream(ConfigTest.DATA_DIR + "resolvers/input.xml", FileMode.Open));
                Assert.Fail();
            }
            catch (SaxonApiException ex) {
                Assert.That(ex.Message.Contains("An error has occurred while opening external DTD"));
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }

        [Test]
        public void noResolverPass() {
            Processor processor = new Processor();
            DocumentBuilder builder = processor.NewDocumentBuilder();
            builder.DtdValidation = (true);
            try {
                XdmNode doc = builder.Build(new Uri(ConfigTest.DataDirUri, "resolvers/ok.xml"));
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }

        [Test]
        public void xmlResolverParse() {
            Configuration config = new EnterpriseConfiguration();
            CommonResourceResolver resolver = config.getCommonResourceResolver();
            try {
                resolver.SetFeature(ResolverFeature.CATALOG_FILES,
                    Collections.singletonList(ConfigTest.DATA_DIR + "resolvers/catalog.xml"));
                Processor processor = new Processor(config);
                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.DtdValidation = (true);
                XdmNode doc = builder.Build(new Uri(ConfigTest.DataDirUri,"resolvers/input.xml"));
                Assert.NotNull(doc);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }

        [Test]
        public void xmlResolverUriResolver() {
            Configuration config = new EnterpriseConfiguration();
            CommonResourceResolver resolver = config.getCommonResourceResolver();
            try {
                resolver.SetFeature(ResolverFeature.CATALOG_ADDITIONS,
                    Collections.singletonList(ConfigTest.DATA_DIR + "resolvers/catalog.xml"));
                Processor processor = new Processor(config);
                XsltCompiler compiler = processor.NewXsltCompiler();

                Stream xsl = new FileStream(ConfigTest.DATA_DIR + "resolvers/style.xsl", FileMode.Open);
                XsltExecutable exec = compiler.Compile(xsl);

                Xslt30Transformer transformer = exec.Load30();

                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.DtdValidation = true;
                XdmNode doc = builder.Build(new FileStream(ConfigTest.DATA_DIR + "resolvers/input.xml", FileMode.Open));

                transformer.GlobalContextItem = (doc);

                XdmDestination destination = new XdmDestination();
                transformer.ApplyTemplates(doc, destination);

                Console.WriteLine(destination.XdmNode);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }

        [Test]
        public void xmlResolverStylesheetUriResolver() {
            Configuration config = new EnterpriseConfiguration();
            CommonResourceResolver resolver = config.getCommonResourceResolver();
            try {
                resolver.SetFeature(ResolverFeature.CATALOG_FILES,
                    Collections.singletonList(ConfigTest.DATA_DIR + "resolvers/catalog.xml"));
                Processor processor = new Processor(config);
                XsltCompiler compiler = processor.NewXsltCompiler();

                Stream xsl = new FileStream(ConfigTest.DATA_DIR + "resolvers/stylesheet.xsl", FileMode.Open);
                XsltExecutable exec = compiler.Compile(xsl);

                Xslt30Transformer transformer = exec.Load30();

                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.DtdValidation = true;
                XdmNode doc = builder.Build(new FileStream(ConfigTest.DATA_DIR + "resolvers/input.xml", FileMode.Open));

                transformer.GlobalContextItem = (doc);

                XdmDestination destination = new XdmDestination();
                transformer.ApplyTemplates(doc, destination);

                Console.WriteLine(destination.XdmNode);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }

        [Test]
        public void xmlResolverUnparsedTextUriResolver() {
            Configuration config = new EnterpriseConfiguration();
            CommonResourceResolver resolver = config.getCommonResourceResolver();
            try {
                resolver.SetFeature(ResolverFeature.CATALOG_ADDITIONS,
                    Collections.singletonList(ConfigTest.DATA_DIR + "resolvers/catalog.xml"));
                Processor processor = new Processor(config);
                XsltCompiler compiler = processor.NewXsltCompiler();

                Stream xsl = new FileStream(ConfigTest.DATA_DIR + "resolvers/stylesheet.xsl", FileMode.Open);
                XsltExecutable exec = compiler.Compile(xsl);

                Xslt30Transformer transformer = exec.Load30();

                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.DtdValidation = (false);
                XdmNode doc = builder.Build(new FileStream(ConfigTest.DATA_DIR + "resolvers/unparsed.xml", FileMode.Open));

                transformer.GlobalContextItem = (doc);

                XdmDestination destination = new XdmDestination();
                transformer.ApplyTemplates(doc, destination);

                Console.WriteLine(destination.XdmNode);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }

        [Test]
        public void xmlResolverModuleUriResolver() {
            Configuration config = new EnterpriseConfiguration();
            CommonResourceResolver resolver = config.getCommonResourceResolver();
            try {
                resolver.SetFeature(ResolverFeature.CATALOG_FILES,
                    Collections.singletonList(ConfigTest.DATA_DIR + "resolvers/catalog.xml"));
                Processor processor = new Processor(config);
                XQueryCompiler compiler = processor.NewXQueryCompiler();

                Stream xqy = new FileStream(ConfigTest.DATA_DIR + "resolvers/module.xqy", FileMode.Open);
                XQueryExecutable exec = compiler.Compile(xqy);

                XQueryEvaluator eval = exec.Load();

                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.DtdValidation = (false);
                XdmNode doc = builder.Build(new FileStream(ConfigTest.DATA_DIR + "resolvers/unparsed.xml", FileMode.Open));

                eval.ContextItem = (doc);

                foreach(XdmItem item in eval) {
                    Console.WriteLine(item.ToString());
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }
    }
}