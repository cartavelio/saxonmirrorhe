using System;
using System.Xml;
using NUnit.Framework;
using XmlResolver.Features;
using Saxon.Api;
using Saxon.Callbacks;
using Saxon.Eej.config;
using Saxon.Ejava.lang;
using Saxon.Ejavax.xml.transform;
using Saxon.Hej;
using Saxon.Hej.lib;
using Exception = Saxon.Ejava.lang.Exception;

namespace SaxonCS_NUnit {
    public class ResolverConfigurationTest {
        [Test]
        public void defaultCatalogResourceResolver() {
            Configuration config = new EnterpriseConfiguration();
            Assert.That((object)config.getResourceResolver(), Is.Not.Null);
            Assert.That(config.getResourceResolver() is ConfigurableResourceResolver);
        }

        [Test]
        public void explicitCatalogResourceResolverClass() {
            Processor proc = new();
            proc.SetProperty(Feature.RESOURCE_RESOLVER, new Saxon.Callbacks.CatalogResourceResolver());
            Configuration config = proc.Implementation;
            Assert.That((object)config.getResourceResolver(), Is.Not.Null);
            Assert.That(config.getResourceResolver() is CatalogResourceResolver);
        }

        [Test]
        public void explicitCatalogResourceResolver() {
            Processor proc = new();
            Configuration config = proc.Implementation;

            CatalogResourceResolver underlyingResolver = new CatalogResourceResolver();

            MyResolver resolver = new MyResolver(underlyingResolver);

            config.setResourceResolver(resolver);
            Assert.That((object)config.getResourceResolver(), Is.Not.Null);
            Assert.That(config.getResourceResolver() is CatalogResourceResolver);
            Assert.That(config.getResourceResolver() is MyResolver);
        }

        [Test]
        public void explicitErroneousCatalogResourceResolver() {
            Processor proc = new();
            Configuration config = proc.Implementation;

            try {
                proc.SetProperty(Feature.RESOURCE_RESOLVER, "Hello");
                Assert.Fail();
            }
            catch (IllegalArgumentException ex) {
                Assert.That(ex.getMessage().Contains("RESOURCE_RESOLVER value must be an instance of"));
            }
            catch (Exception ex) {
                Assert.Fail();
            }
        }

        private class MyResolver : CatalogResourceResolver {
            private readonly CatalogResourceResolver resolver;

            public MyResolver(CatalogResourceResolver resolver) {
                this.resolver = resolver;
            }


            public override Source resolve(JJ_ResourceRequest request) {
                return resolver.resolve(request);
            }

            public override void setFeature(ResolverFeature feature, object value) {
                resolver.setFeature(feature, value);
            }

            public override object getFeature(ResolverFeature resolverFeature) {
                return resolver.getFeature(resolverFeature);
            }

            public override XmlResolver.XmlResolver GetEntityResolver() {
                throw new NotImplementedException();
            }

        }


        [Test]
        public void parseXmlTest() {

            String query = "parse-xml('<!DOCTYPE root [\n" +
                           "  <!ENTITY xxe SYSTEM \"TEST_02.xml\">\n" +
                           "]>\n" +
                           "<root>&amp;xxe;</root>')";
            // Test 1 - no resrtictions
            try {
                Processor processor = new Processor();

                XQueryCompiler xqueryCompiler = processor.NewXQueryCompiler();
                xqueryCompiler.BaseUri = ConfigTest.DataDirUri;
                XQueryEvaluator xqueryEvaluator = xqueryCompiler.Compile(query).Load();
                MemoryStream os1 = new MemoryStream();
                xqueryEvaluator.Run(processor.NewSerializer(os1));
                Console.WriteLine("First run successful");
            }
            catch (SaxonApiException err) {
                Assert.Fail(err.Message);
            }

            // Test 2 - restricted using a custom resource resolver
            try {
                Processor processor = new Processor();
                processor.SetResourceResolver(request => {
                    if (request.Uri.IsFile) {
                        throw new SaxonApiException("File URI not allowed");
                    }
                    else {
                        return null;
                    }
                }, true);
                XQueryCompiler xqueryCompiler = processor.NewXQueryCompiler();
                xqueryCompiler.BaseUri = ConfigTest.DataDirUri;
                XQueryEvaluator xqueryEvaluator = xqueryCompiler.Compile(query).Load();
                MemoryStream os = new MemoryStream();
                xqueryEvaluator.Run(processor.NewSerializer(os));
                Console.WriteLine("Second run successful");
                Assert.Fail("Error not detected");
            }
            catch (SaxonApiException err) {
                // error is expected
            }

            // Test 3 - restricted set of allowed protocols (bug 6711)
            try {
                Processor processor = new Processor();
                processor.SetProperty(Feature.ALLOWED_PROTOCOLS, "https,http");
                XQueryCompiler xqueryCompiler = processor.NewXQueryCompiler();
                xqueryCompiler.BaseUri = ConfigTest.DataDirUri;
                XQueryEvaluator xqueryEvaluator = xqueryCompiler.Compile(query).Load();
                MemoryStream os = new MemoryStream();
                xqueryEvaluator.Run(processor.NewSerializer(os));
                Console.WriteLine("Second run successful");
                Assert.Fail("Error not detected");
            }
            catch (SaxonApiException err) {
                // error is expected
            }
        }
    }

}