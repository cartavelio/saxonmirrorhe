using Saxon.Api;

namespace SaxonCS_NUnit;

/// <summary>
/// Tests for environment variables in SaxonCS
/// </summary>
class TestEnvironmentVariables {
        
    [Test]
    public void TestAvailable() {
        try {
            Processor p = new ();
            XPathCompiler c = p.NewXPathCompiler();
            XdmValue v = c.Evaluate("available-environment-variables()", null);
            Console.WriteLine(v);
            Assert.That(v.Count > 10);
        }
        catch (Exception e) {
            Console.WriteLine(e.StackTrace);
            Assert.Fail(e.Message);
        }
    }

    [Test]
    public void TestHome() {
        try {
            Processor p = new ();
            XPathCompiler c = p.NewXPathCompiler();
            XdmAtomicValue v = (XdmAtomicValue)c.EvaluateSingle("environment-variable('HOME')", null);
            Assert.That(v.ToString().StartsWith("/Users"));
        }
        catch (Exception e) {
            Console.WriteLine(e.StackTrace);
            Assert.Fail(e.Message);
        }
    }
    
    [Test]
    public void TestResolver() {
        try {
            Processor p = new ();
            XPathCompiler c = p.NewXPathCompiler();
            XdmAtomicValue v = (XdmAtomicValue)c.EvaluateSingle("environment-variable('HOME')", null);
            Assert.That(v.ToString().StartsWith("/Users"));
        }
        catch (Exception e) {
            Console.WriteLine(e.StackTrace);
            Assert.Fail(e.Message);
        }
    }
        
 

}