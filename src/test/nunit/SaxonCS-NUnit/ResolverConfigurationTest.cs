using System;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Org.XmlResolver.Features;
using Saxon.Api;
using Saxon.Callbacks;
using Saxon.Eej.config;
using Saxon.Ejava.lang;
using Saxon.Ejavax.xml.transform;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej;
using Exception = Saxon.Ejava.lang.Exception;

namespace SaxonCS_NUnit {
    public class ResolverConfigurationTest {
        [Test]
        public void defaultCatalogResourceResolver() {
            Configuration config = new EnterpriseConfiguration();
            Assert.NotNull(config.getCommonResourceResolver());
            Assert.That(config.getCommonResourceResolver() is CatalogResourceResolver);
        }

        [Test]
        public void explicitCatalogResourceResolverClass() {
            Processor proc = new();
            proc.SetProperty(Feature.COMMON_RESOURCE_RESOLVER, new CatalogResourceResolver());
            Configuration config = proc.Implementation;
            Assert.NotNull(config.getCommonResourceResolver());
            Assert.That(config.getCommonResourceResolver() is CatalogResourceResolver);
        }

        [Test]
        public void explicitCatalogResourceResolver() {
            Processor proc = new();
            Configuration config = proc.Implementation;

            CommonResourceResolver underlyingResolver = new CatalogResourceResolver();

            MyResolver resolver = new MyResolver(underlyingResolver);

            proc.SetProperty(Feature.COMMON_RESOURCE_RESOLVER, resolver);
            Assert.NotNull(config.getCommonResourceResolver());
            Assert.That(config.getCommonResourceResolver() is CommonResourceResolver);
            Assert.That(config.getCommonResourceResolver() is MyResolver);
        }

        [Test]
        public void explicitErroneousCatalogResourceResolver() {
            Processor proc = new();
            Configuration config = proc.Implementation;

            try {
                proc.SetProperty(Feature.COMMON_RESOURCE_RESOLVER, "Hello");
                Assert.Fail();
            }
            catch (IllegalArgumentException ex) {
                Assert.That(ex.getMessage().Contains("SAXON_RESOURCE_RESOLVER value must be an instance of"));
            }
            catch (Exception ex) {
                Assert.Fail();
            }
        }

        private class MyResolver : CommonResourceResolver {
            private readonly CommonResourceResolver resolver;

            public MyResolver(CommonResourceResolver resolver) {
                this.resolver = resolver;
            }


            public override CommonResourceResolver NewInstance() {
                return this;
            }

            public override IResource DereferenceLocation(ResourceRequest request) {
                return resolver.DereferenceLocation(request);
            }

            public override IResource DereferenceNamespace(string namespaceUri, ResourceRequest request) {
                return resolver.DereferenceNamespace(namespaceUri, request);
            }

            public override void SetFeature(ResolverFeature feature, object value) {
                resolver.SetFeature(feature, value);
            }

            public override object GetFeature(ResolverFeature feature) {
                return resolver.GetFeature(feature);
            }

            public override object GetEntity(Uri absoluteUri, string? role, Type? ofObjectToReturn) {
                throw new NotImplementedException();
            }

            public override Uri ResolveUri(Uri? baseUri, string? relativeUri) {
                return resolver.ResolveUri(baseUri, relativeUri);
            }
        }
    }
}