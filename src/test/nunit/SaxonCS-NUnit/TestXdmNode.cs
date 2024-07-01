using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Saxon.Api;

namespace SaxonCS_NUnit {
    class TestXdmNode
    {

        [Test]
        public void testNodeToXmlWriter() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode node = builder.Build(new StringReader("<in xmlns='http://ns.com/' foo='bar'><baz/></in>"));
            StringWriter sw = new();
            XmlWriter writer = XmlWriter.Create(sw);
            node.WriteTo(writer);
            Console.Error.WriteLine(sw);
            ClassicAssert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?><in xmlns=""http://ns.com/"" foo=""bar""><baz /></in>", sw.ToString());

        }
        
        [Test]
        public void TestElementProperties() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc = builder.Build(new StringReader("<in xmlns='http://ns.com/' foo='bar'><baz/></in>"));
            XdmNode element = doc.Children().First();
            ClassicAssert.AreEqual("in", element.NodeName.LocalName);
            ClassicAssert.AreEqual("http://ns.com/", element.NodeName.Uri);
            ClassicAssert.AreEqual("", element.NodeName.Prefix);
            ClassicAssert.AreEqual(XdmNodeKind.Element, element.NodeKind);
            ClassicAssert.AreEqual(XmlNodeType.Element, element.NodeType);
            ClassicAssert.AreEqual("", element.StringValue);
            Assert.That(doc.Equals(element.Parent));
            Assert.That(doc.Equals(element.Root));
            Assert.That(!element.IsAtomic());
        }
        
        [Test]
        public void TestChildren() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc = builder.Build(new StringReader("<in xmlns='http://ns.com/' foo='bar'><baz/></in>"));
            ClassicAssert.AreEqual(1, doc.Children().Count());
            ClassicAssert.AreEqual(1, doc.Children("in").Count());
            ClassicAssert.AreEqual(1, doc.Children("http://ns.com/","in").Count());
            ClassicAssert.AreEqual(0, doc.Children("out").Count());
            XdmNode element = doc.Children().First();
            ClassicAssert.AreEqual(1, element.Children().Count());
            ClassicAssert.AreEqual(1, element.Children("baz").Count());
            ClassicAssert.AreEqual(1, element.Children("http://ns.com/","baz").Count());
            ClassicAssert.AreEqual(0, element.Children("","baz").Count());
            ClassicAssert.AreEqual(0, element.Children(Predicates.IsText()).Count());
            ClassicAssert.AreEqual(1, element.Children(Predicates.IsElement()).Count());
            XdmNode baz = element.Children().First();
            ClassicAssert.AreEqual(0, baz.Children().Count());
            ClassicAssert.AreEqual(0, baz.Children("baz").Count());
        }
        
        [Test]
        public void TestOutermostElement() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc = builder.Build(new StringReader("<in xmlns='http://ns.com/' foo='bar'><baz/></in>"));
            ClassicAssert.AreEqual("in", doc.OutermostElement.NodeName.LocalName);
            XdmNode el1 = doc.OutermostElement;
            ClassicAssert.AreEqual("in", el1.OutermostElement.NodeName.LocalName);
            XdmNode baz = el1.Select(Steps.Child("baz")).First();
            ClassicAssert.AreEqual("in", baz.OutermostElement.NodeName.LocalName);
            XdmNode foo = el1.Select(Steps.Attribute("foo")).First();
            ClassicAssert.AreEqual("in", foo.OutermostElement.NodeName.LocalName);
        }
        
        [Test]
        public void TestChildrenRepeatable() {
            // Children() returns an Enumerable so we need to be able to process it more than once...
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc = builder.Build(new StringReader("<in><foo/><foo/><foo/></in>"));
            XdmNode element = doc.Children().First();
            IEnumerable<XdmNode> grandkids = element.Children();
            int counter = 0;
            foreach (XdmNode grandkid in grandkids) {
                counter++;
            }
            ClassicAssert.AreEqual(3, counter);
            foreach (XdmNode grandkid in grandkids) {
                counter++;
            }
            ClassicAssert.AreEqual(6, counter);
        }

        [Test]
        public void TestAttributes() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<in xmlns='http://ns.com/' foo='bar' xml:space='preserve'><baz/></in>"));
            XdmNode element = doc.Children().First();
            ClassicAssert.AreEqual("bar", element.GetAttributeValue("foo"));
            ClassicAssert.AreEqual("bar", element.GetAttributeValue(new QName("foo")));
            ClassicAssert.AreEqual("preserve", element.GetAttributeValue(new QName(NamespaceConstant.XML, "space")));
            ClassicAssert.Null(element.GetAttributeValue("pooh"));
        }

        [Test]
        public void TestAxisSteps() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>"));
            XdmNode one = doc.Select(Steps.Child("one")).First();
            ClassicAssert.NotNull(one);
            XdmNode doc2 = one.Select(Steps.Parent(Predicates.IsDocument())).First();
            ClassicAssert.NotNull(doc2);
            Assert.That(doc.Equals(doc2)); // For some reason, ClassicAssert.AreEqual fails with stack overflow
            XdmNode doc3 = one.Select(Steps.Ancestor()).First();
            ClassicAssert.NotNull(doc3);
            Assert.That(doc.Equals(doc3));
            XdmNode three = doc.Select(Steps.Descendant("three")).Last();
            ClassicAssert.NotNull(three);
            ClassicAssert.AreEqual("three", three.NodeName.LocalName);
            XdmNode openBra = three.Select(Steps.Preceding(Predicates.IsText())).First();
            ClassicAssert.NotNull(openBra);
            ClassicAssert.AreEqual("[", openBra.StringValue);
            XdmNode closeBra = three.Select(Steps.Following(Predicates.IsText())).First();
            ClassicAssert.NotNull(closeBra);
            ClassicAssert.AreEqual("]", closeBra.StringValue);
        }

        [Test]
        public void TestMultipleSteps() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>"));
            XdmNode grandKid = doc.Select(Steps.Child("one").Then(Steps.Child("two"))).First();
            ClassicAssert.AreEqual("two", grandKid.NodeName.LocalName);
            ClassicAssert.AreEqual("http://ns.com/", grandKid.NodeName.Uri);
            string foo = grandKid.Select(Steps.Parent().Then(Steps.Attribute("foo"))).Single().StringValue;
            ClassicAssert.AreEqual("bar", foo);
        }
        
        [Test]
        public void TestPath() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>"));
            XdmNode three = doc.Select(Steps.Path("one", "two", "three")).First();
            ClassicAssert.AreEqual("three", three.NodeName.LocalName);
            XdmNode three2 = doc.Select(Steps.Path("one", "//", "three")).First();
            ClassicAssert.AreEqual("three", three2.NodeName.LocalName);
            XdmNode foo = doc.Select(Steps.Path("one", "@foo")).First();
            ClassicAssert.AreEqual("bar", foo.StringValue);
        }
        
        [Test]
        public void TestLast() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>"));
            XdmNode three = doc.Select(Steps.Descendant("*")).Last();
            ClassicAssert.AreEqual("three", three.NodeName.LocalName);
        }
        
        

    }
}