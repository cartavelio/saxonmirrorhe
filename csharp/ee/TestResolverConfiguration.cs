using Saxon.Api;
using Saxon.Callbacks;
using Saxon.Hej;
using XmlResolver.Features;

namespace SaxonCS_NUnit;

public class TestResolverConfiguration
{
    [Test]
    public void ConfigureResolver()
    {
        Configuration config = new();
        CatalogResourceResolver resolver = ((CatalogResourceResolver)config.getResourceResolver());
        resolver.setFeature(ResolverFeature.URI_FOR_SYSTEM, true);

        try
        {
            config.setConfigurationProperty(ResolverFeature.URI_FOR_SYSTEM.GetFeatureName(), false);
            config.setConfigurationProperty(ResolverFeature.CATALOG_ADDITIONS.GetFeatureName(),
                new List<string> { "a.xml", "b.xml" });
            
            Assert.That(resolver.getFeature(ResolverFeature.URI_FOR_SYSTEM).Equals(false));

            List<string> additions = (List<string>) resolver.getFeature(ResolverFeature.CATALOG_ADDITIONS);
            Assert.That(additions.Count == 2);
            Assert.That(additions[0] == "a.xml");
            Assert.That(additions[1] == "b.xml");
        }
        catch (Exception ex)
        {
            Assert.Fail(ex.Message);
        }
    }

    [Test]
    public void ReadConfigurationFile()
    {
        var configfn = ConfigTest.DATA_DIR + "resolver-config/resolver-config.xml";
        var configFile = new FileStream(configfn, FileMode.Open);
        var processor = new Processor(configFile, new Uri(configfn));

        CatalogResourceResolver resolver = (CatalogResourceResolver)processor.Implementation.getResourceResolver();

        Assert.That(resolver.getFeature(ResolverFeature.URI_FOR_SYSTEM).Equals(false));

        List<string> additions = (List<string>)resolver.getFeature(ResolverFeature.CATALOG_ADDITIONS);
        Assert.That(additions.Count == 2);
        Assert.That(additions[0] == "a.xml");
        Assert.That(additions[1] == "b.xml");
    }
    
    [Test]
    public void ReadConfigurationFileBadType()
    {
        var configfn = ConfigTest.DATA_DIR + "resolver-config/resolver-config-bad-type.xml";
        var configFile = new FileStream(configfn, FileMode.Open);

        try
        {
            var processor = new Processor(configFile, new Uri(configfn));
            Assert.Fail("Should not have loaded configuration");
        }
        catch (Exception ex)
        {
            Assert.That(ex.Message.Contains("Invalid XML resolver feature value"));
        }
    }

    [Test]
    public void ReadConfigurationFileBadName()
    {
        var configfn = ConfigTest.DATA_DIR + "resolver-config/resolver-config-bad-name.xml";
        var configFile = new FileStream(configfn, FileMode.Open);

        try
        {
            var processor = new Processor(configFile, new Uri(configfn));
            Assert.Fail("Should not have loaded configuration");
        }
        catch (Exception ex)
        {
            Assert.That(ex.Message.Contains("Unknown XML resolver feature"));
        }
    }

    [Test]
    public void ReadConfigurationNotConfigurable()
    {
        var configfn = ConfigTest.DATA_DIR + "resolver-config/resolver-config-not-config.xml";
        var configFile = new FileStream(configfn, FileMode.Open);

        try
        {
            var processor = new Processor(configFile, new Uri(configfn));
            Assert.Fail("Should not have loaded configuration");
        }
        catch (Exception ex)
        {
            Assert.That(ex.Message.Contains("XML resolver feature cannot be configured"));
        }
    }
}