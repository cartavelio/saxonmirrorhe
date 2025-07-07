using System;
using System.IO;
using System.Net;
using NUnit.Framework;
using Saxon.Api;
using Saxon.Hej;

namespace SaxonCS_NUnit;

public class TestBug6813
{
    [Test]
    public void TestFileAccessOk()
    {
        Processor processor = new Processor(true);
        
        Assert.That(processor, Is.Not.Null);
        Assert.That(processor.Implementation.isLicensedFeature(Configuration.LicenseFeature.SCHEMA_VALIDATION));

        XPathCompiler compiler = processor.NewXPathCompiler();
        compiler.BaseUri =  new Uri("file:///tmp/");
        compiler.DeclareNamespace("saxon", "http://saxon.sf.net/");
        string uri = "file://" + System.IO.Directory.GetCurrentDirectory() + "/../../../../../../../../BUILD.md";
        XPathExecutable executable = compiler.Compile("saxon:read-binary-resource('" + uri + "')");
        XPathSelector selector = executable.Load();
        XdmValue value = selector.Evaluate();
        Assert.Pass();
    }
    
    [Test]
    public void TestFileAccessFail()
    {
        Processor processor = new Processor(true);
        
        Assert.That(processor, Is.Not.Null);
        Assert.That(processor.Implementation.isLicensedFeature(Configuration.LicenseFeature.SCHEMA_VALIDATION));

        processor.SetProperty(Feature.ALLOWED_PROTOCOLS, "http,https");

        try
        {
            XPathCompiler compiler = processor.NewXPathCompiler();
            compiler.BaseUri = new Uri("file:///tmp/");
            compiler.DeclareNamespace("saxon", "http://saxon.sf.net/");
            string uri = "file://" + System.IO.Directory.GetCurrentDirectory() + "/../../../../../../../../BUILD.md";
            XPathExecutable executable = compiler.Compile("saxon:read-binary-resource('" + uri + "')");
            XPathSelector selector = executable.Load();
            XdmValue value = selector.Evaluate();
            Assert.Fail();
        }
        catch (SaxonApiException ex)
        {
            Assert.That(ex.Message, Does.Contain("has been prohibited"));
        }
    }

}