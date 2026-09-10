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
            Assert.That((object)@"<?xml version=""1.0"" encoding=""utf-16""?><in xmlns=""http://ns.com/"" foo=""bar""><baz /></in>", Is.EqualTo(sw.ToString()));

        }
        
        [Test]
        public void TestElementProperties() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc = builder.Build(new StringReader("<in xmlns='http://ns.com/' foo='bar'><baz/></in>"));
            XdmNode element = doc.Children().First();
            Assert.That((object)"in", Is.EqualTo(element.NodeName.LocalName));
            Assert.That((object)"http://ns.com/", Is.EqualTo(element.NodeName.Uri));
            Assert.That((object)"", Is.EqualTo(element.NodeName.Prefix));
            Assert.That((object)XdmNodeKind.Element, Is.EqualTo(element.NodeKind));
            Assert.That((object)XmlNodeType.Element, Is.EqualTo(element.NodeType));
            Assert.That((object)"", Is.EqualTo(element.StringValue));
            Assert.That(doc.Equals(element.Parent));
            Assert.That(doc.Equals(element.Root));
            Assert.That(element.IsAtomic(), Is.False);
        }
        
        [Test]
        public void TestChildren() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc = builder.Build(new StringReader("<in xmlns='http://ns.com/' foo='bar'><baz/></in>"));
            object b = doc.Children().Count();
            Assert.That((object)1, Is.EqualTo(b));
            object b1 = doc.Children("in").Count();
            Assert.That((object)1, Is.EqualTo(b1));
            object b2 = doc.Children("http://ns.com/","in").Count();
            Assert.That((object)1, Is.EqualTo(b2));
            object b3 = doc.Children("out").Count();
            Assert.That((object)0, Is.EqualTo(b3));
            XdmNode element = doc.Children().First();
            object b4 = element.Children().Count();
            Assert.That((object)1, Is.EqualTo(b4));
            object b5 = element.Children("baz").Count();
            Assert.That((object)1, Is.EqualTo(b5));
            object b6 = element.Children("http://ns.com/","baz").Count();
            Assert.That((object)1, Is.EqualTo(b6));
            object b7 = element.Children("","baz").Count();
            Assert.That((object)0, Is.EqualTo(b7));
            object b8 = element.Children(Predicates.IsText()).Count();
            Assert.That((object)0, Is.EqualTo(b8));
            object b9 = element.Children(Predicates.IsElement()).Count();
            Assert.That((object)1, Is.EqualTo(b9));
            XdmNode baz = element.Children().First();
            object b10 = baz.Children().Count();
            Assert.That((object)0, Is.EqualTo(b10));
            object b11 = baz.Children("baz").Count();
            Assert.That((object)0, Is.EqualTo(b11));
        }
        
        [Test]
        public void TestOutermostElement() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc = builder.Build(new StringReader("<in xmlns='http://ns.com/' foo='bar'><baz/></in>"));
            Assert.That((object)"in", Is.EqualTo(doc.OutermostElement.NodeName.LocalName));
            XdmNode el1 = doc.OutermostElement;
            Assert.That((object)"in", Is.EqualTo(el1.OutermostElement.NodeName.LocalName));
            XdmNode baz = el1.Select(Steps.Child("baz")).First();
            Assert.That((object)"in", Is.EqualTo(baz.OutermostElement.NodeName.LocalName));
            XdmNode foo = el1.Select(Steps.Attribute("foo")).First();
            Assert.That((object)"in", Is.EqualTo(foo.OutermostElement.NodeName.LocalName));
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
            Assert.That((object)3, Is.EqualTo(counter));
            foreach (XdmNode grandkid in grandkids) {
                counter++;
            }
            Assert.That((object)6, Is.EqualTo(counter));
        }

        [Test]
        public void TestAttributes() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<in xmlns='http://ns.com/' foo='bar' xml:space='preserve'><baz/></in>"));
            XdmNode element = doc.Children().First();
            object b = element.GetAttributeValue("foo");
            Assert.That((object)"bar", Is.EqualTo(b));
            object b1 = element.GetAttributeValue(new QName("foo"));
            Assert.That((object)"bar", Is.EqualTo(b1));
            object b2 = element.GetAttributeValue(new QName(NamespaceConstant.XML, "space"));
            Assert.That((object)"preserve", Is.EqualTo(b2));
            Assert.That(element.GetAttributeValue("pooh"), Is.Null);
        }

        [Test]
        public void TestAxisSteps() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>"));
            XdmNode one = doc.Select(Steps.Child("one")).First();
            Assert.That((object)one, Is.Not.Null);
            XdmNode doc2 = one.Select(Steps.Parent(Predicates.IsDocument())).First();
            Assert.That((object)doc2, Is.Not.Null);
            Assert.That(doc.Equals(doc2)); // For some reason, assertAreEqual fails with stack overflow
            XdmNode doc3 = one.Select(Steps.Ancestor()).First();
            Assert.That((object)doc3, Is.Not.Null);
            Assert.That(doc.Equals(doc3));
            XdmNode three = doc.Select(Steps.Descendant("three")).Last();
            Assert.That((object)three, Is.Not.Null);
            Assert.That((object)"three", Is.EqualTo(three.NodeName.LocalName));
            XdmNode openBra = three.Select(Steps.Preceding(Predicates.IsText())).First();
            Assert.That((object)openBra, Is.Not.Null);
            Assert.That((object)"[", Is.EqualTo(openBra.StringValue));
            XdmNode closeBra = three.Select(Steps.Following(Predicates.IsText())).First();
            Assert.That((object)closeBra, Is.Not.Null);
            Assert.That((object)"]", Is.EqualTo(closeBra.StringValue));
        }

        [Test]
        public void TestMultipleSteps() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>"));
            XdmNode grandKid = doc.Select(Steps.Child("one").Then(Steps.Child("two"))).First();
            Assert.That((object)"two", Is.EqualTo(grandKid.NodeName.LocalName));
            Assert.That((object)"http://ns.com/", Is.EqualTo(grandKid.NodeName.Uri));
            string foo = grandKid.Select(Steps.Parent().Then(Steps.Attribute("foo"))).Single().StringValue;
            Assert.That((object)"bar", Is.EqualTo(foo));
        }
        
        [Test]
        public void TestPath() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>"));
            XdmNode three = doc.Select(Steps.Path("one", "two", "three")).First();
            Assert.That((object)"three", Is.EqualTo(three.NodeName.LocalName));
            XdmNode three2 = doc.Select(Steps.Path("one", "//", "three")).First();
            Assert.That((object)"three", Is.EqualTo(three2.NodeName.LocalName));
            XdmNode foo = doc.Select(Steps.Path("one", "@foo")).First();
            Assert.That((object)"bar", Is.EqualTo(foo.StringValue));
        }
        
        [Test]
        public void TestLast() {
            Processor proc = new();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc =
                builder.Build(
                    new StringReader("<one foo='bar'>[<two xmlns='http://ns.com/'><three/></two>]</one>"));
            XdmNode three = doc.Select(Steps.Descendant("*")).Last();
            Assert.That((object)"three", Is.EqualTo(three.NodeName.LocalName));
        }
        
        

    }
}