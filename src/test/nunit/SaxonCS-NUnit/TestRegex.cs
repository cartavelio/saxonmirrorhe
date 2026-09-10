using System;
using NUnit.Framework;
using Saxon.Api;

namespace SaxonCS_NUnit {
    
    /// <summary>
    /// Tests for XPath regular expressions using the ";n" extension, which uses the .NET regex engine
    /// (and syntax) in place of the XPath syntax.
    /// </summary>
    class TestRegex {
        


        [Test]
        public void TestMatches() {
            try {
                Processor p = new ();
                XPathCompiler c = p.NewXPathCompiler();
                XdmValue v = c.Evaluate("matches(' ', '^\\x20$', ';n')", null);
                Assert.That(((XdmAtomicValue)v).AsBoolean);
                XdmValue w = c.Evaluate("matches('Z ', '^z\\x20$', 'i;n')", null);
                Assert.That(((XdmAtomicValue)w).AsBoolean);
            }
            catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void TestTokenize() {  // bug 5655
            try {
                Processor p = new ();
                XPathCompiler c = p.NewXPathCompiler();
                XdmValue v = c.Evaluate(@"tokenize('dole0021', '\W+|(?<=\D)(?=\d)|(?<=\d)(?=\D)', ';n')", null);
                Assert.That(v.Count == 2);
                Assert.That((object)v[0].StringValue, Is.EqualTo("dole"));
                Assert.That((object)v[1].StringValue, Is.EqualTo("0021"));
            }
            catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                Assert.Fail(e.Message);
            }
        }
        
        
        
        [Test]
        public void TestReplace() {
            try {
                Processor p = new ();
                XPathCompiler c = p.NewXPathCompiler();
                XdmValue v = c.Evaluate("replace('3/20/1986', " +
                                        "'(?<mm>[0-9]+)/(?<dd>[0-9]+)/(?<yy>[0-9]+)', '${yy}/${mm}/${dd}$$', ';n')", null);
                object b = ((XdmAtomicValue)v).StringValue;
                Assert.That((object)"1986/3/20$", Is.EqualTo(b));
            }
            catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                Assert.Fail(e.Message);
            }
        }

    }
}