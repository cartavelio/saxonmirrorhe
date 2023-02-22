using System;
using System.Runtime.CompilerServices;
using System.Xml;
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
        // [Test]
        // public void defaultCatalogResourceResolver() {
        //     Configuration config = new EnterpriseConfiguration();
        //     Assert.NotNull(config.getResourceResolver());
        //     Assert.That(config.getResourceResolver() is Saxon.Callbacks.CatalogResourceResolver);
        // }
        //
        // [Test]
        // public void explicitCatalogResourceResolverClass() {
        //     Processor proc = new();
        //     proc.SetProperty(Feature.RESOURCE_RESOLVER, new CatalogResourceResolver());
        //     Configuration config = proc.Implementation;
        //     Assert.NotNull(config.getResourceResolver());
        //     Assert.That(config.getResourceResolver() is CatalogResourceResolver);
        // }
        //
        // [Test]
        // public void explicitCatalogResourceResolver() {
        //     Processor proc = new();
        //     Configuration config = proc.Implementation;
        //
        //     CatalogResourceResolver underlyingResolver = new CatalogResourceResolver();
        //
        //     MyResolver resolver = new MyResolver(underlyingResolver);
        //
        //     proc.SetResourceResolver(resolver);
        //     Assert.NotNull(config.getResourceResolver());
        //     Assert.That(config.getResourceResolver() is CatalogResourceResolver);
        //     Assert.That(config.getResourceResolver() is MyResolver);
        // }
        //
        // [Test]
        // public void explicitErroneousCatalogResourceResolver() {
        //     Processor proc = new();
        //     Configuration config = proc.Implementation;
        //
        //     try {
        //         proc.SetProperty(Feature.RESOURCE_RESOLVER, "Hello");
        //         Assert.Fail();
        //     }
        //     catch (IllegalArgumentException ex) {
        //         Assert.That(ex.getMessage().Contains("SAXON_RESOURCE_RESOLVER value must be an instance of"));
        //     }
        //     catch (Exception ex) {
        //         Assert.Fail();
        //     }
        // }
        //
        // private class MyResolver : CatalogResourceResolver {
        //     private readonly CatalogResourceResolver resolver;
        //
        //     public MyResolver(CatalogResourceResolver resolver) {
        //         this.resolver = resolver;
        //     }
        //     
        //
        //     public Source Resolve(ResourceRequest request) {
        //         return resolver.resolve(request);
        //     }
        //
        //     public override void SetFeature(ResolverFeature feature, object value) {
        //         resolver.SetFeature(feature, value);
        //     }
        //
        //     public override object GetFeature(ResolverFeature feature) {
        //         return resolver.GetFeature(feature);
        //     }
        //
        //     public override XmlResolver GetEntityResolver() {
        //         throw new NotImplementedException();
        //     }
        //     
        // }
    }
}