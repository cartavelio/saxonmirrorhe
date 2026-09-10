using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NUnit.Framework;
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
            Assert.AreEqual(@"<?xml version=""1.0"" encoding=""utf-16""?><in xmlns=""http://ns.com/"" foo=""bar""><baz /></in>", sw.ToString());

        }
        
        [Test]
        public void TestElementProperties() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc = builder.Build(new StringReader("<in xmlns='http://ns.com/' foo='bar'><baz/></in>"));
            XdmNode element = doc.Children().First();
            Assert.AreEqual("in", element.NodeName.LocalName);
            Assert.AreEqual("http://ns.com/", element.NodeName.Uri);
            Assert.AreEqual("", element.NodeName.Prefix);
            Assert.AreEqual(XdmNodeKind.Element, element.NodeKind);
            Assert.AreEqual(XmlNodeType.Element, element.NodeType);
            Assert.AreEqual("", element.StringValue);
            Assert.That(doc.Equals(element.Parent));
            Assert.That(doc.Equals(element.Root));
            Assert.False(element.IsAtomic());
        }
        
        [Test]
        public void TestChildren() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc = builder.Build(new StringReader("<in xmlns='http://ns.com/' foo='bar'><baz/></in>"));
            Assert.AreEqual(1, doc.Children().Count());
            Assert.AreEqual(1, doc.Children("in").Count());
            Assert.AreEqual(1, doc.Children("http://ns.com/","in").Count());
            Assert.AreEqual(0, doc.Children("out").Count());
            XdmNode element = doc.Children().First();
            Assert.AreEqual(1, element.Children().Count());
            Assert.AreEqual(1, element.Children("baz").Count());
            Assert.AreEqual(1, element.Children("http://ns.com/","baz").Count());
            Assert.AreEqual(0, element.Children("","baz").Count());
            Assert.AreEqual(0, element.Children(Predicates.IsText()).Count());
            Assert.AreEqual(1, element.Children(Predicates.IsElement()).Count());
            XdmNode baz = element.Children().First();
            Assert.AreEqual(0, baz.Children().Count());
            Assert.AreEqual(0, baz.Children("baz").Count());
        }
        
        [Test]
        public void TestOutermostElement() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc = builder.Build(new StringReader("<in xmlns='http://ns.com/' foo='bar'><baz/></in>"));
            Assert.AreEqual("in", doc.OutermostElement.NodeName.LocalName);
            XdmNode el1 = doc.OutermostElement;
            Assert.AreEqual("in", el1.OutermostElement.NodeName.LocalName);
            XdmNode baz = el1.Select(Steps.Child("baz")).First();
            Assert.AreEqual("in", baz.OutermostElement.NodeName.LocalName);
            XdmNode foo = el1.Select(Steps.Attribute("foo")).First();
            Assert.AreEqual("in", foo.OutermostElement.NodeName.LocalName);
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
            Assert.AreEqual(3, counter);
            foreach (XdmNode grandkid in grandkids) {
                counter++;
            }
            Assert.AreEqual(6, counter);
        }

        [Test]
        public void TestAttributes() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<in xmlns='http://ns.com/' foo='bar' xml:space='preserve'><baz/></in>"));
            XdmNode element = doc.Children().First();
            Assert.AreEqual("bar", element.GetAttributeValue("foo"));
            Assert.AreEqual("bar", element.GetAttributeValue(new QName("foo")));
            Assert.AreEqual("preserve", element.GetAttributeValue(new QName(NamespaceConstant.XML, "space")));
            Assert.Null(element.GetAttributeValue("pooh"));
        }

        [Test]
        public void TestAxisSteps() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>"));
            XdmNode one = doc.Select(Steps.Child("one")).First();
            Assert.NotNull(one);
            XdmNode doc2 = one.Select(Steps.Parent(Predicates.IsDocument())).First();
            Assert.NotNull(doc2);
            Assert.That(doc.Equals(doc2)); // For some reason, Assert.AreEqual fails with stack overflow
            XdmNode doc3 = one.Select(Steps.Ancestor()).First();
            Assert.NotNull(doc3);
            Assert.That(doc.Equals(doc3));
            XdmNode three = doc.Select(Steps.Descendant("three")).Last();
            Assert.NotNull(three);
            Assert.AreEqual("three", three.NodeName.LocalName);
            XdmNode openBra = three.Select(Steps.Preceding(Predicates.IsText())).First();
            Assert.NotNull(openBra);
            Assert.AreEqual("[", openBra.StringValue);
            XdmNode closeBra = three.Select(Steps.Following(Predicates.IsText())).First();
            Assert.NotNull(closeBra);
            Assert.AreEqual("]", closeBra.StringValue);
        }

        [Test]
        public void TestMultipleSteps() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>"));
            XdmNode grandKid = doc.Select(Steps.Child("one").Then(Steps.Child("two"))).First();
            Assert.AreEqual("two", grandKid.NodeName.LocalName);
            Assert.AreEqual("http://ns.com/", grandKid.NodeName.Uri);
            string foo = grandKid.Select(Steps.Parent().Then(Steps.Attribute("foo"))).Single().StringValue;
            Assert.AreEqual("bar", foo);
        }
        
        [Test]
        public void TestPath() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>"));
            XdmNode three = doc.Select(Steps.Path("one", "two", "three")).First();
            Assert.AreEqual("three", three.NodeName.LocalName);
            XdmNode three2 = doc.Select(Steps.Path("one", "//", "three")).First();
            Assert.AreEqual("three", three2.NodeName.LocalName);
            XdmNode foo = doc.Select(Steps.Path("one", "@foo")).First();
            Assert.AreEqual("bar", foo.StringValue);
        }
        
        [Test]
        public void TestLast() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>"));
            XdmNode three = doc.Select(Steps.Descendant("*")).Last();
            Assert.AreEqual("three", three.NodeName.LocalName);
        }
        
        

    }
}