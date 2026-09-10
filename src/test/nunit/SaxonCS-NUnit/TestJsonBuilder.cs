using System;
using System.IO;
using NUnit.Framework;
using Saxon.Api;

namespace SaxonCS_NUnit {
    /**
    * NUnit tests for the s9api JsonBuilder
    */
    class TestJsonBuilder {
        

        [Test]
        public void TestStringSource() {
            try {
                String sr = "{\"one\": 1, \"two\": 2}";
                JsonBuilder builder =  new Processor().NewJsonBuilder();
                XdmValue d = builder.ParseJson(sr);
                Assert.That(d is XdmMap);
                XdmMap m = (XdmMap) d;
                Assert.That((object)((XdmAtomicValue) m.Get("one")).AsLong(), Is.EqualTo(1));
                Assert.That((object)((XdmAtomicValue) m.Get("two")).AsLong(), Is.EqualTo(2));
                Assert.That((object)m.Size, Is.EqualTo(2));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestStringReaderSource() {
            try {
                StringReader sr = new("{\"one\": 1, \"two\": 2}");
                JsonBuilder builder =  new Processor().NewJsonBuilder();
                XdmValue d = builder.ParseJson(sr);
                Assert.That(d is XdmMap);
                XdmMap m = (XdmMap) d;
                Assert.That((object)((XdmAtomicValue) m.Get("one")).AsLong(), Is.EqualTo(1));
                Assert.That((object)((XdmAtomicValue) m.Get("two")).AsLong(), Is.EqualTo(2));
                Assert.That((object)m.Size, Is.EqualTo(2));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void TestError() {
            try {
                string sr = "{\"one\": 1, \"two\": 2,}";
                JsonBuilder builder =  new Processor().NewJsonBuilder();
                XdmValue d = builder.ParseJson(sr);
                Assert.Fail("Error not detected");
            }
            catch (SaxonApiException e) {
                Assert.That(e.ErrorCode.LocalName == "FOJS0001");
                Assert.That(e.Message.StartsWith("Invalid JSON"));
                Assert.That(e.Message.Contains("Trailing comma"));
            }
        }
        
        [Test]
        public void TestLiberal() {
            try {
                String sr = "{\"one\": 1, \"two\": 2, }";
                JsonBuilder builder =  new Processor().NewJsonBuilder();
                builder.Liberal = true;
                XdmValue d = builder.ParseJson(sr);
                Assert.That(d is XdmMap);
                XdmMap m = (XdmMap) d;
                Assert.That((object)((XdmAtomicValue) m.Get("one")).AsLong(), Is.EqualTo(1));
                Assert.That((object)((XdmAtomicValue) m.Get("two")).AsLong(), Is.EqualTo(2));
                Assert.That((object)m.Size, Is.EqualTo(2));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


    }
}