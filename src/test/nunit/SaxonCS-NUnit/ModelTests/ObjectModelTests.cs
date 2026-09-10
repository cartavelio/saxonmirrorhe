using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Saxon.Api;
using Saxon.Ejava.lang;
using Exception = System.Exception;

namespace SaxonCS_NUnit.ModelTests;

    /**
     * This is an abstract class containing tests designed to exercise common functionality against
     * different supported object models (that is, implementations of the NodeInfo interface).
     * Subclasses are defined for each specific object model.
     */
    
public abstract class ObjectModelTests {
        


    abstract protected string getTreeModel();

    abstract protected XdmNode buildExternalDocument(Processor processor, String fileName);

    IDestination makeDestination(Processor processor) {
        try {
            switch (getTreeModel()) {
                case "tiny":
                    return new XdmDestination() {
                        TreeModel = TreeModel.TinyTree
                    };
                case "linked":
                    return new XdmDestination() {
                        TreeModel = TreeModel.LinkedTree
                    };
                case "dom":
                    return new DomDestination();
                case "linq":
                    return new LinqDestination();
                default:
                    throw new IllegalArgumentException("unknown tree model");
                    
            }
        }
        catch (Exception e) {
            Assert.Fail(e.Message);
            return null;
        }
    }

    /**
     * Ask whether this tree model strips whitespace-only text nodes in element that are
     * defined by the DTD to have an element-only content model
     * @return true if such text nodes are stripped while parsing; false if not.
     */
    bool stripsIgnorableWhitespace() {
        return false;
    }

    /**
     * Ask whether Saxon's wrapper for this tree model merges adjacent sibling text nodes
     * when found on the external tree
     * @return if Saxon merges such text nodes, or if they are merged automatically during parsing
     * and tree construction.
     */
    protected virtual bool mergesAdjacentTextNodes() {
        return true;
    }

    private int count(IEnumerable<XdmNode> collection) {
        int c = 0;
        foreach (XdmNode node in collection) {
            c++;
        }

        return c;
    }

    [Test]
    public void testChildAxis1() {
        Processor proc = new Processor(true);
        XdmNode doc = buildExternalDocument(proc, "books.xml");
        object b = count(doc.Children());
        Assert.That((object)2, Is.EqualTo(b));
        object b1 = count(doc.Children(Predicates.IsProcessingInstruction()));
        Assert.That((object)1, Is.EqualTo(b1));
        object b2 = count(doc.Children("BOOKLIST"));
        Assert.That((object)1, Is.EqualTo(b2));
        XdmNode booklist = doc.Select(Steps.Child("BOOKLIST")).First();
        object b3 = count(booklist.Children("*"));
        Assert.That((object)2, Is.EqualTo(b3));
        XdmNode books = booklist.Select(Steps.Child("BOOKS")).First();
        object b4 = count(books.Children("*"));
        Assert.That((object)6, Is.EqualTo(b4));
        object b5 = count(books.Children());
        Assert.That((object)13, Is.EqualTo(b5));
    }

    [Test]
    public void testChildAxis2() {
        Processor proc = new Processor(true);
        XdmNode doc = buildExternalDocument(proc, "books-with-dtd.xml");
        object b = count(doc.Children());
        Assert.That((object)1, Is.EqualTo(b));
        object b1 = count(doc.Children("BOOKLIST"));
        Assert.That((object)1, Is.EqualTo(b1));
        XdmNode booklist = doc.Select(Steps.Child("BOOKLIST")).First();
        object b2 = count(booklist.Children("*"));
        Assert.That((object)2, Is.EqualTo(b2));
        XdmNode books = booklist.Select(Steps.Child("BOOKS")).First();
        object b3 = count(books.Children("*"));
        Assert.That((object)6, Is.EqualTo(b3));
        object b4 = count(books.Children());
        Assert.That((object)(stripsIgnorableWhitespace() ? 6 : 13), Is.EqualTo(b4));
    }

    [Test]
    public void testChildAxis3() {
        Processor proc = new Processor(true);
        XdmNode doc = buildExternalDocument(proc, "books-sql.xsl");
        object b = count(doc.Children());
        Assert.That((object)3, Is.EqualTo(b));
        object b1 = count(doc.Children(Predicates.IsElement()));
        Assert.That((object)1, Is.EqualTo(b1));
        object b2 = count(doc.Children(Predicates.IsText()));
        Assert.That((object)0, Is.EqualTo(b2));
        object b3 = count(doc.Children(Predicates.IsComment()));
        Assert.That((object)2, Is.EqualTo(b3));
        object b4 = count(doc.Children(NamespaceConstant.XSLT, "stylesheet"));
        Assert.That((object)1, Is.EqualTo(b4));
        object b5 = count(doc.Children("stylesheet"));
        Assert.That((object)1, Is.EqualTo(b5));
        XdmNode cDataElement = doc.Select(Steps.Child("stylesheet")
            .Then(Steps.Child("template")
                .Where(Predicates.AttributeEq("name", "UNUSED")))).First();
        Assert.That((object)"(This is CDATA)", Is.EqualTo(cDataElement.StringValue));
        if (mergesAdjacentTextNodes()) {
            object b6 = count(cDataElement.Children());
            Assert.That((object)1, Is.EqualTo(b6));
            object b7 = cDataElement.Select(Steps.Child()).First().StringValue;
            Assert.That((object)"(This is CDATA)", Is.EqualTo(b7));
        }
    }

    [Test]
    public void testAttributeAxis() {
        Processor proc = new Processor(true);
        XdmNode doc = buildExternalDocument(proc, "books.xml");
        XdmNode booklist = doc.Select(Steps.Child("BOOKLIST")).First();
        object b = booklist.Select(
            Steps.Attribute()).Count();
        Assert.That((object)1, Is.EqualTo(b));
        object b1 = booklist.Select(
            Steps.Attribute(Predicates.HasLocalName("noNamespaceSchemaLocation"))).Count();
        Assert.That((object)1, Is.EqualTo(b1));
        object b2 = booklist.Select(
                Steps.Attribute(Predicates.HasName(NamespaceConstant.SCHEMA_INSTANCE, "noNamespaceSchemaLocation")))
            .Count();
        Assert.That((object)1, Is.EqualTo(b2));
        object b3 = booklist.Select(
            Steps.Descendant("ITEM").Then(Steps.Attribute("CAT"))).Count();
        Assert.That((object)6, Is.EqualTo(b3));
        object b4 = booklist.Select(
            Steps.Descendant("ITEM").Then(Steps.Attribute("", "CAT"))).Count();
        Assert.That((object)6, Is.EqualTo(b4));
    }

    [Test]
    public void testGenerateId() {
        // Test that generate-id produces unique IDs and that they have the correct lexical form
        try {
            Processor proc = new(true);
            XdmNode doc = buildExternalDocument(proc, "books-sql.xsl");
            XdmFunctionItem genId =
                proc.GetSystemFunction(new QName(NamespaceConstant.FN, "generate-id"), 1);
            IDictionary<string, XdmNode> idMap = new Dictionary<string, XdmNode>();
            foreach (XdmNode node in doc.Select(Steps.Descendant())) {
                try {
                    string id = ((XdmAtomicValue)genId.Invoke(new XdmValue[] { node }, proc)).StringValue;
                    if (!new Regex("[A-Za-z][A-Za-z0-9]*").IsMatch(id)) {
                        Assert.Fail("Invalid id " + id);
                    }

                    if (idMap.ContainsKey(id)) {
                        Assert.Fail("Duplicate id " + id);
                    }

                    idMap[id] = node;
                    foreach (XdmNode att in node.Select(Steps.Attribute())) {
                        try {
                            string attId = ((XdmAtomicValue)genId.Invoke(new XdmValue[] { att }, proc)).StringValue;
                            if (!new Regex("[A-Za-z][A-Za-z0-9]*").IsMatch(attId)) {
                                Assert.Fail("Invalid id " + id);
                            }

                            if (idMap.ContainsKey(attId)) {
                                Assert.Fail("Duplicate attribute id " + attId);
                            }

                            idMap[attId] = att;
                        }
                        catch (SaxonApiException e) {
                            Assert.Fail(e.Message);
                        }
                    };
                }
                catch (SaxonApiException e) {
                    Assert.Fail(e.Message);
                }
            };
        }
        catch (SaxonApiException e) {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    public void testDocumentOrder() {
        try {
            Processor proc = new(true);
            XdmNode doc = buildExternalDocument(proc, "books.xml");
            XdmValue selection = XdmEmptySequence.Instance;
            Object[] items = doc.Select(Steps.Descendant("ITEM")).ToArray();
            selection = selection.Append((XdmNode)items[4]);
            selection = selection.Append((XdmNode)items[2]);
            selection = selection.Append((XdmNode)items[1]);
            selection = selection.Append((XdmNode)items[5]);
            selection = selection.Append((XdmNode)items[3]);
            selection = selection.Append((XdmNode)items[1]);
            foreach (XdmItem n in selection) {
                selection = selection.Append(
                    new XdmValue(n.Select(Steps.Child("DIMENSIONS").Then(Steps.Attribute("UNIT")))));
                selection = selection.Append(
                    new XdmValue(n.Select(Steps.Attribute("CAT"))));
                selection = selection.Append(
                    new XdmValue(n.Select(Steps.Child("AUTHOR").Then(Steps.Child(Predicates.IsText())))));
            }

            selection = selection.Append(doc);
            selection = selection.DocumentOrder();
            Assert.That((object)21, Is.EqualTo(selection.Count()));
            System.Text.StringBuilder builder = new();
            foreach (XdmItem v in selection) {
                builder.Append(((XdmNode)v).NodeKind.ToString()).Append(':');
                QName qn = ((XdmNode)v).NodeName;
                builder.Append(qn == null ? "~" : qn.LocalName).Append(':');
                if (!((XdmNode)v).Select(Steps.Child("*")).Any()) {
                    builder.Append(((XdmNode)v).StringValue);
                }

                builder.Append('|');
            }

            Assert.That((object)"document-node():~:|element():ITEM:|attribute():CAT:P|text():~:Charlotte Brontë|attribute():UNIT:in|element():ITEM:|attribute():CAT:P|text():~:Thomas Hardy|attribute():UNIT:in|element():ITEM:|attribute():CAT:P|text():~:Thomas Hardy|attribute():UNIT:in|element():ITEM:|attribute():CAT:H|text():~:Jasper Fforde|attribute():UNIT:cm|element():ITEM:|attribute():CAT:P|text():~:Jasper Fforde|attribute():UNIT:in|", Is.EqualTo(builder.ToString()));
        }
        catch (SaxonApiException e) {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    public void testNamespaces() {
        try {
            Processor proc = new(true);
            XdmNode doc = buildExternalDocument(proc, "books-sql.xsl");
            XdmNode root = doc.Children("stylesheet").First();
            ISet<string> bindingsFound = new HashSet<string>();
            IEnumerator<XdmNode> iter = root.EnumerateAxis(XdmAxis.Namespace);
            while (iter.MoveNext()) {
                XdmNode ns = iter.Current;
                string binding = ns.NodeName.LocalName + "=" + ns.StringValue;
                bindingsFound.Add(binding);
            }

            Assert.That((object)5, Is.EqualTo(bindingsFound.Count));
            Assert.That(bindingsFound.Contains("sql=http://saxon.sf.net/sql"));
            Assert.That(bindingsFound.Contains("xsl=http://www.w3.org/1999/XSL/Transform"));
            Assert.That(bindingsFound.Contains("saxon=http://saxon.sf.net/"));
            Assert.That(bindingsFound.Contains("java=http://saxon.sf.net/java-type"));
            Assert.That(bindingsFound.Contains("xml=" + NamespaceConstant.XML));
        }
        catch (Exception e) {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    public void testDeepCopy() {
        try {
            Processor proc = new Processor(true);
            XdmNode doc = buildExternalDocument(proc, "copytest.xml");
            XdmNode root = doc.OutermostElement;
            XdmNode p1 = root.Select(Steps.Child("http://namespace.p.com/", "section")).First();
            XdmNode p2 = root.Select(Steps.Child("http://namespace.p2.com/", "section2")).First();
            XsltCompiler comp = proc.NewXsltCompiler();

            String xsl = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>" +
                         "<xsl:template name='t1'><xsl:copy-of select='.' copy-namespaces='yes'/></xsl:template>" +
                         "<xsl:template name='t2'><xsl:copy-of select='.' copy-namespaces='no'/></xsl:template>" +
                         "</xsl:stylesheet>";
            XsltExecutable exec = comp.Compile(new StringReader(xsl));
            XsltTransformer trans = exec.Load();


            String result = doCopyTransform(proc, trans, p1, "t1");
            Assert.That(result.Contains("xmlns:p=\"http://namespace.p.com/\""));
            Assert.That(result.Contains("xmlns:q=\"http://namespace.q.com/\""));

            result = doCopyTransform(proc, trans, p1, "t2");
            Assert.That(result.Contains("xmlns:p=\"http://namespace.p.com/\""));
            Assert.That(result.Contains("xmlns:q=\"http://namespace.q.com/\""), Is.False);

            result = doCopyTransform(proc, trans, p2, "t1");
            Assert.That(result.Contains("xmlns=\"http://namespace.p2.com/\""));
            Assert.That(result.Contains("xmlns:my=\"http://namespace.my.com/\""));
            Assert.That(result.Contains("xmlns:q=\"http://namespace.q.com/\""));

            result = doCopyTransform(proc, trans, p2, "t2");
            Assert.That(result.Contains("xmlns=\"http://namespace.p2.com/\""));
            Assert.That(result.Contains("xmlns:my=\"http://namespace.my.com/\""));
            Assert.That(result.Contains("xmlns:q=\"http://namespace.q.com/\""), Is.False);
        }
        catch (Exception e) {
            //e.printStackTrace();
            Assert.Fail(e.Message);
        }
    }

    private static string doCopyTransform(Processor proc, XsltTransformer trans, XdmNode input,
        String initialTemplate) {
        try {
            StringWriter sw = new StringWriter();
            trans.InitialContextNode = input;
            trans.Destination = (proc.NewSerializer(sw));
            trans.InitialTemplate = (new QName(initialTemplate));
            trans.Run();
            return sw.ToString();
        }
        catch (Exception e) {
            //e.printStackTrace();
            Assert.Fail(e.Message);
            throw e;
        }
    }

    [Test]
    public void testDestination() {
        try {
            Processor proc = new Processor(true);
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode source = builder.Build(new Uri(ConfigTest.DataDirUri + "books-sql.xsl"));
            IDestination dest = makeDestination(proc);
            XQueryCompiler compiler = proc.NewXQueryCompiler();
            XQueryExecutable exec = compiler.Compile("copy-of(.)");
            XQueryEvaluator eval = exec.Load();
            eval.ContextItem = (source);
            eval.Run(dest);
            XdmFunctionItem deepEq =
                proc.GetSystemFunction(new QName(NamespaceConstant.FN, "deep-equal"), 2);
            //XdmValue comparison = deepEq.Invoke(new XdmValue[] { source, dest.XdmNode }, proc);
            //Assert.That((((XdmAtomicValue)comparison).AsBoolean()));
        }
        catch (Exception e) {
            Assert.Fail(e.Message);
        }
    }

    // [Test]
    // public void testId() {
    //     try {
    //         Processor proc = new Processor(true);
    //         XdmNode doc = buildExternalDocument(proc, "books-with-dtd.xml");
    //         XdmFunctionItem id = proc.GetSystemFunction(new QName(NamespaceConstant.FN, "id"), 2);
    //         XdmValue val = id.Invoke(new XdmValue[] { new XdmAtomicValue("MMP"), doc }, proc);
    //         Assert.That(val is XdmNode);
    //         assertAreEqual("Mass-market Paperback", ((XdmNode)val).GetAttributeValue("DESC"));
    //     }
    //     catch (SaxonApiException e) {
    //         //e.printStackTrace();
    //         Assert.Fail(e.Message);
    //     }
    // }

    [Test]
    public void testXPath() {
        try {
            Processor proc = new Processor(true);
            proc.SetProperty(Feature.TRACE_EXTERNAL_FUNCTIONS, true);
            XdmNode doc = buildExternalDocument(proc, "books.xml");
            XPathCompiler xpc = proc.NewXPathCompiler();
            XPathSelector xp = xpc.Compile(getExtensionFunctionCall()).Load();
            xp.ContextItem = (doc);
            XdmValue result = xp.Evaluate();
            object b = ((XdmAtomicValue)result).StringValue;
            Assert.That((object)"Jane Austen", Is.EqualTo(b));
        }
        catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail(e.Message);
        }
    }

    [Test]
    public void testReflexiveExtension() {
        try {
            Processor proc = new Processor(true);
            proc.SetProperty(Feature.TRACE_EXTERNAL_FUNCTIONS, true);
            XdmNode doc = buildExternalDocument(proc, "books.xml");
            XPathCompiler xpc = proc.NewXPathCompiler();
            XPathSelector xp = xpc.Compile("string((//AUTHOR)[1])").Load();
            xp.ContextItem = (doc);
            XdmValue result = xp.Evaluate();
            object b = ((XdmAtomicValue)result).StringValue;
            Assert.That((object)"Jane Austen", Is.EqualTo(b));
        }
        catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail(e.Message);
        }
    }

    /**
     * To be implemented in each subclass - supply an XPath extension function call that finds
     * the string value of the first author in the document
     * @return an XPath expression
     */
    public String getExtensionFunctionCall() {
        // default implementation
        return "string((//AUTHOR)[1])";
    }
}