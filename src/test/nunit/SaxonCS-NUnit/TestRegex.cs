using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
                Assert.AreEqual(v[0].StringValue, "dole");
                Assert.AreEqual(v[1].StringValue, "0021");
            }
            catch (Exception e) {
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
                Assert.AreEqual("1986/3/20$", ((XdmAtomicValue)v).StringValue);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

    }
}