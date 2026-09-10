using System;
using System.Collections.Generic;
using System.IO;
using Saxon.Api;
using NUnit.Framework;


namespace SaxonCS_NUnit {
    public class CatalogResolverTest {
        [Test]
        public void noResolverFail() {
            Processor processor = new ();
            DocumentBuilder builder = processor.NewDocumentBuilder();
            builder.DtdValidation = true;
            try {
                XdmNode doc = builder.Build(new FileStream(ConfigTest.DATA_DIR + "resolvers/input.xml", FileMode.Open));
                Assert.Fail();
            }
            catch (SaxonApiException ex) {
                if (!ex.Message.Contains("Cannot resolve external DTD subset")) {
                    Assert.Fail(ex.Message);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }

        [Test]
        public void noResolverPass() {
            Processor processor = new ();
            DocumentBuilder builder = processor.NewDocumentBuilder();
            builder.DtdValidation = (true);
            try {
                XdmNode doc = builder.Build(new Uri(ConfigTest.DataDirUri, "resolvers/ok.xml"));
                Assert.NotNull(doc);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }

        [Test]
        public void xmlResolverParse() {
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            try {
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
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            try {
                XsltCompiler compiler = processor.NewXsltCompiler();

                Stream xsl = new FileStream(ConfigTest.DATA_DIR + "resolvers/style.xsl", FileMode.Open);
                XsltExecutable exec = compiler.Compile(xsl);

                Xslt30Transformer transformer = exec.Load30();

                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.DtdValidation = true;
                XdmNode doc = builder.Build(new FileStream(ConfigTest.DATA_DIR + "resolvers/input.xml", FileMode.Open));

                transformer.GlobalContextItem = (doc);

                XdmDestination destination = new ();
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
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            try {
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
        public void xmlResolverStylesheetUriResolverOverride() {
            // Override catalog resolution with specific resolver
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            string included =
                "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
                "version='3.0'><xsl:template match='/'><boo/></xsl:template></xsl:transform>";
            try {
                XsltCompiler compiler = processor.NewXsltCompiler();
                compiler.BaseUri = ConfigTest.DataDirUri;
                compiler.StylesheetModuleResolver = request => new TextResource(included, request.Uri);
                Stream xsl = new FileStream(ConfigTest.DATA_DIR + "resolvers/stylesheet.xsl", FileMode.Open);
                XsltExecutable exec = compiler.Compile(xsl);

                Xslt30Transformer transformer = exec.Load30();

                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.DtdValidation = true;
                XdmNode doc = builder.Build(new FileStream(ConfigTest.DATA_DIR + "resolvers/input.xml", FileMode.Open));

                transformer.GlobalContextItem = (doc);

                StringWriter sw = new();
                Serializer destination = processor.NewSerializer(sw);
                transformer.ApplyTemplates(doc, destination);
                Assert.That(sw.ToString().Contains("<boo/>"));
                
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }
        
        [Test]
        public void xmlResolverStylesheetUriResolverFallback() {
            // Specific resolver returns null; fallback to catalog resolver
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            string included =
                "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' " +
                "version='3.0'><xsl:template match='/'><boo/></xsl:template></xsl:transform>";
            try {
                XsltCompiler compiler = processor.NewXsltCompiler();
                compiler.BaseUri = ConfigTest.DataDirUri;
                compiler.StylesheetModuleResolver = request => null;
                Stream xsl = new FileStream(ConfigTest.DATA_DIR + "resolvers/stylesheet.xsl", FileMode.Open);
                XsltExecutable exec = compiler.Compile(xsl);

                Xslt30Transformer transformer = exec.Load30();

                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.DtdValidation = true;
                XdmNode doc = builder.Build(new FileStream(ConfigTest.DATA_DIR + "resolvers/input.xml", FileMode.Open));

                transformer.GlobalContextItem = (doc);

                StringWriter sw = new();
                Serializer destination = processor.NewSerializer(sw);
                transformer.ApplyTemplates(doc, destination);
                Assert.That(sw.ToString().Contains("<p>Hello, world.</p>"));
                
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }

        [Test]
        public void XsltUnparsedTextUriResolver() {
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            try {
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
        public void XsltUnparsedTextUriResolverOverride() {
            // Override catalog resolver with specific resolver
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            try {
                XsltCompiler compiler = processor.NewXsltCompiler();

                Stream xsl = new FileStream(ConfigTest.DATA_DIR + "resolvers/stylesheet.xsl", FileMode.Open);
                XsltExecutable exec = compiler.Compile(xsl);

                Xslt30Transformer transformer = exec.Load30();

                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.DtdValidation = (false);
                XdmNode doc = builder.Build(new FileStream(ConfigTest.DATA_DIR + "resolvers/unparsed.xml", FileMode.Open));

                transformer.GlobalContextItem = (doc);
                transformer.TextResolver = (request) => (request.Uri.ToString() == "http://example.com/text") ? new TextResource("bananas", request.Uri) : null;

                StringWriter sw = new();
                Serializer destination = processor.NewSerializer(sw);
                transformer.ApplyTemplates(doc, destination);

                Assert.That(sw.ToString().Contains("bananas"));
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }
        
        [Test]
        public void XsltUnparsedTextUriResolverFallbackToCatalog() {
            // Text resolver returns null; fallback to catalog resolver
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            try {
                XsltCompiler compiler = processor.NewXsltCompiler();

                Stream xsl = new FileStream(ConfigTest.DATA_DIR + "resolvers/stylesheet.xsl", FileMode.Open);
                XsltExecutable exec = compiler.Compile(xsl);

                Xslt30Transformer transformer = exec.Load30();

                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.DtdValidation = (false);
                XdmNode doc = builder.Build(new FileStream(ConfigTest.DATA_DIR + "resolvers/unparsed.xml", FileMode.Open));

                transformer.GlobalContextItem = (doc);
                transformer.TextResolver = (request) => (request.Uri.ToString() == "http://example.com/there-are-no-bananas") ? new TextResource("bananas", request.Uri) : null;

                StringWriter sw = new();
                Serializer destination = processor.NewSerializer(sw);
                transformer.ApplyTemplates(doc, destination);

                Assert.That(sw.ToString().Contains("&lt;catalog"));
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }
        
        [Test]
        public void XsltUnparsedTextUriResolverFallbackToDirect() {
            // Text resolver returns null; fallback to direct file access
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            try {
                XsltCompiler compiler = processor.NewXsltCompiler();

                Stream xsl = new FileStream(ConfigTest.DATA_DIR + "resolvers/stylesheet.xsl", FileMode.Open);
                XsltExecutable exec = compiler.Compile(xsl);

                Xslt30Transformer transformer = exec.Load30();
                transformer.SetStylesheetParameters(new Dictionary<QName, XdmValue> {{
                    new QName("text-uri"), new XdmAtomicValue(new Uri(ConfigTest.DataDirUri, "sample.json"))}});
                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.DtdValidation = (false);
                XdmNode doc = builder.Build(new FileStream(ConfigTest.DATA_DIR + "resolvers/unparsed.xml", FileMode.Open));

                transformer.GlobalContextItem = (doc);
                transformer.TextResolver = (request) => (request.Uri.ToString() == "http://example.com/there-are-no-bananas") ? new TextResource("bananas", request.Uri) : null;

                StringWriter sw = new();
                Serializer destination = processor.NewSerializer(sw);
                transformer.ApplyTemplates(doc, destination);

                Assert.That(sw.ToString().Contains("\"A green door\""));
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }
        
        [Test]
        public void XQueryUnparsedTextUriResolver() {
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            try {
                XQueryCompiler compiler = processor.NewXQueryCompiler();
                XQueryEvaluator selector = compiler.Compile("string-length(unparsed-text('http://example.com/text'))").Load();
                XdmValue result = selector.Evaluate();
                Assert.That(result[0].StringValue == "559");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }
        
        [Test]
        public void XQueryUnparsedTextUriResolverOverride() {
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            try {
                XQueryCompiler compiler = processor.NewXQueryCompiler();
                XQueryEvaluator selector = compiler.Compile("string-length(unparsed-text('http://example.com/text'))").Load();
                selector.TextResolver = request =>
                    request.Uri.ToString() == "http://example.com/text" ? new TextResource("bananas", request.Uri) : null; 
                XdmValue result = selector.Evaluate();
                Assert.That(result[0].StringValue == "7");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }
        
        [Test]
        public void XQueryUnparsedTextUriResolverFallback() {
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            try {
                XQueryCompiler compiler = processor.NewXQueryCompiler();
                compiler.BaseUri = ConfigTest.DataDirUri;
                XQueryEvaluator selector = compiler.Compile("string-length(unparsed-text('sample.json'))").Load();
                selector.TextResolver = request =>
                    request.Uri.ToString() == "http://example.com/text" ? new TextResource("bananas", request.Uri) : null; 
                XdmValue result = selector.Evaluate();
                Assert.That(result[0].StringValue == "94");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }
        
        [Test]
        public void XPathUnparsedTextUriResolverFallback() {
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            try {
                XPathCompiler compiler = processor.NewXPathCompiler();
                compiler.BaseUri = ConfigTest.DataDirUri;
                XPathSelector selector = compiler.Compile("string-length(unparsed-text('sample.json'))").Load();
                selector.TextResolver = request =>
                    request.Uri.ToString() == "http://example.com/text" ? new TextResource("bananas", request.Uri) : null; 
                XdmValue result = selector.Evaluate();
                Assert.That(result[0].StringValue == "94");
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Assert.Fail();
            }
        }


        [Test]
        public void xmlResolverModuleUriResolver() {
            Processor processor = new();
            processor.SetCatalogFiles(ConfigTest.DATA_DIR + "resolvers/catalog.xml");
            try {
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
        
        [Test]
        public void testMagicUri() {
            try {
                Processor processor = new (true);
                XPathCompiler xpc = processor.NewXPathCompiler();
                // This is a special URI that can be used to check that the xmlresolver is present and correct
                XPathExecutable exec = xpc.Compile("doc-available('https://xmlresolver.org/data/resolver/succeeded/test/check.xml')");
                XPathSelector s = exec.Load();
                XdmValue val = s.Evaluate();
                Assert.That(val is XdmAtomicValue);
                Assert.That(((XdmAtomicValue)val).AsBoolean);
            } catch (Exception ex) {
                Assert.Fail();
            }
        }
    }
}