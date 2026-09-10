using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using NUnit.Framework;
using Saxon.Api;
using Singulink.Numerics;

namespace SaxonCS_NUnit;

class TestXdmFunction {
    

    [Test]
    public void testArrayOperations() {
        XdmArray a = new XdmArray();
        Assert.That(0 == a.ArrayLength());
        Assert.That(1 == a.Count);

        a = new XdmArray(new XdmValue[] {new XdmAtomicValue(1), new XdmAtomicValue(2), new XdmAtomicValue(3)});
        Assert.That(3 == a.ArrayLength());
        Assert.That(1 == a.Count);
        try {
            Assert.That(((XdmAtomicValue) a.Get(1)).AsLong() == 2);
        }
        catch (Exception e) {
            Assert.Fail(e.Message);
        }

        a = a.Put(1, new XdmAtomicValue(17));
        try {
            Assert.That(((XdmAtomicValue) a.Get(1)).AsLong() == 17);
        }
        catch (Exception e) {
            Assert.Fail(e.Message);
        }

        List<XdmValue> values = a.AsList();
        try {
            Assert.That(((XdmAtomicValue) values[1]).AsLong() == 17);
        }
        catch (Exception e) {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    public void TestArrayAsFunction() {
        XdmArray a = new XdmArray(new XdmValue[]
            {new XdmAtomicValue(1), new XdmAtomicValue(2), new XdmAtomicValue(3)});
        Processor proc = new Processor(true);
        try {
            XdmValue result = a.Invoke(new XdmValue[] {new XdmAtomicValue(1)}, proc); // 1-based indexing here
            Assert.That(result is XdmAtomicValue);
            Assert.That(((XdmAtomicValue) result).AsLong() == 1);
        }
        catch (Exception e) {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    public void TestArrayAsQueryParameter() {
        try {
            XdmArray a = new XdmArray(new XdmValue[]
                {new XdmAtomicValue(1), new XdmAtomicValue(2), new XdmAtomicValue(3)});
            Processor proc = new Processor(true);
            String query =
                "declare namespace a='http://www.w3.org/2005/xpath-functions/array';" +
                "declare variable $a as array(*) external; " +
                "a:size($a)";
            XQueryCompiler comp = proc.NewXQueryCompiler();
            XQueryEvaluator q = comp.Compile(query).Load();
            q.SetExternalVariable(new QName("a"), a);
            XdmAtomicValue result = (XdmAtomicValue) q.EvaluateSingle();
            Assert.That(3 == result.AsLong());
        }
        catch (Exception e) {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    public void TestArrayAsQueryResult() {
        try {
            Processor proc = new Processor(true);
            String query = "[1, 2, 3]";
            XQueryCompiler comp = proc.NewXQueryCompiler();
            XQueryEvaluator q = comp.Compile(query).Load();
            XdmArray result = (XdmArray) q.EvaluateSingle();
            Assert.That(3 == result.ArrayLength());
        }
        catch (Exception e) {
            Assert.Fail(e.Message);
        }
    }

    [Test]
    public void TestArrayConvertIntegers() {
        int[] inn = new int[] {5, 6, 7, 8};
        XdmArray result = XdmArray.MakeArray(inn);
        Assert.That(4 == result.ArrayLength());
        Assert.That(new XdmAtomicValue(5).Equals(result.Get(0)));
        //Assert.That(XdmItemType.INT.matches((XdmAtomicValue)result.get(3)));
    }

    [Test]
    public void TestArrayConvertLongs() {
        long[] inn = new long[] {5, 6, 7, 8};
        XdmArray result = XdmArray.MakeArray(inn);
        Assert.That(4 == result.ArrayLength());
        Assert.That(new XdmAtomicValue(5).Equals(result.Get(0)));
        Console.WriteLine(((XdmAtomicValue) result.Get(3)).TypeName.ToString());
        // Assert.That(ItemType.LONG.matches((XdmAtomicValue)result.Get(3)));
    }

    [Test]
    public void TestArrayConvertStrings() {
        String[] inn = new String[] {"a", "b", "c", "d"};
        XdmArray result = XdmArray.MakeArray(inn);
        Assert.That(4 == result.ArrayLength());
        Assert.That(new XdmAtomicValue("a").Equals(result.Get(0)));
        //Assert.That(ItemType.ANY_ARRAY.matches(result));
        //Assert.That(ItemType.STRING.matches((XdmAtomicValue)result.Get(3)));
    }

    [Test]
    public void TestArrayConvertBooleans() {
        bool[] inn = new bool[] {true, true, false};
        XdmArray result = XdmArray.MakeArray(inn);
        Assert.That(3 == result.ArrayLength());
        Assert.That(XdmValue.MakeValue(true).Equals(result.Get(0)));
        //Assert.That(ItemType.BOOLEAN.matches((XdmAtomicValue)result.Get(2)));
    }

    [Test]
    public void TestArrayConvertArrays() {
        String[] in0 = new String[] {"a", "b", "c", "d"};
        String[] in1 = new String[] {"w", "x", "y", "z"};
        XdmArray result = XdmArray.MakeArray(new String[][] {in0, in1});
        Assert.That(2 == result.ArrayLength());
        Assert.That(new XdmAtomicValue("a").Equals(((XdmArray) result.Get(0)).Get(0)));
        // Assert.That(ItemType.ANY_ARRAY.matches((XdmItem)result.get(1)));
        // Assert.That(ItemType.STRING.matches((XdmItem)((XdmArray)result.get(1)).get(2)));
        // Processor proc = new Processor(true);
        // ItemTypeFactory factory = new ItemTypeFactory(proc);
        // ItemType arrayOfString = factory.getArrayType(SequenceType.makeSequenceType(ItemType.STRING, OccurrenceIndicator.ONE));
        // ItemType arrayOfArray = factory.getArrayType(SequenceType.makeSequenceType(arrayOfString, OccurrenceIndicator.ONE));
        //  Assert.That(arrayOfArray.matches(result));
    }
        
    [Test]
    public void TestArrayToString() {
        XdmArray a = new XdmArray();
        a = a.AppendMember(new XdmAtomicValue("a"));
        a = a.AppendMember(new XdmAtomicValue("b"));
        a = a.AppendMember(new XdmAtomicValue(3));
        string result = a.ToString();
        //Console.WriteLine(result);
        Assert.That((object)result, Is.EqualTo("[\"a\",\"b\",3]")); 
    }
    
      [Test]
    public void testCallSystemFunction() {
        try {
            Processor proc = new Processor(true);
            XdmFunctionItem fi = proc.GetSystemFunction(new QName(NamespaceConstant.FN, "parse-json"), 1);
            Assert.That((object)fi, Is.Not.Null);
            XdmArray result = (XdmArray) fi.Invoke( new XdmValue[]{new XdmAtomicValue("[1,2,3]")}, proc);
            object b = result.ArrayLength();
            Assert.That((object)3, Is.EqualTo(b));
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail();
        }
    }

    [Test]
    public void testCallSystemFunctionMath() {
        try {
            Processor proc = new Processor();
            XdmFunctionItem fi = proc.GetSystemFunction( new QName(NamespaceConstant.MATH, "sqrt"), 1);
            Assert.That((object)fi, Is.Not.Null);
            XdmAtomicValue result = (XdmAtomicValue) fi.Invoke(new XdmValue[]{ new XdmAtomicValue(4.0e0)}, proc);
            object b = result.AsLong();
            Assert.That((object)2, Is.EqualTo(b));
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail();
        }
    }

    [Test]
    public void testCallSystemFunctionArray() {
        try {
            Processor proc = new (true);
            XdmFunctionItem fi = proc.GetSystemFunction(new QName(NamespaceConstant.ARRAY, "sort"), 1);
            Assert.That((object)fi, Is.Not.Null);
            XdmArray result = (XdmArray) fi.Invoke(
                new XdmValue[]{ new XdmArray(new XdmValue[]{new XdmAtomicValue(3), new XdmAtomicValue(1)})}, proc);
            XdmValue first = result.Get(0);
            object b = ((XdmAtomicValue) first).AsLong();
            Assert.That((object)1, Is.EqualTo(b));
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail();
        }
    }

    [Test]
    public void testCallSystemFunctionSaxon() {
        try {
            Processor proc = new Processor(true);
            XdmFunctionItem fi = proc.GetSystemFunction(new QName(NamespaceConstant.SAXON, "in-summer-time"), 2);
            Assert.That((object)fi, Is.Not.Null);
            XdmAtomicValue arg1 = new ("2016-08-20T12:00:00", XdmAtomicType.DateTime);
            XdmAtomicValue arg2 = new ("Europe/London");
            XdmAtomicValue result = (XdmAtomicValue) fi.Invoke(new XdmValue[]{arg1, arg2}, proc);
            Assert.That(result.AsBoolean);
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail();
        }
    }

    [Test]
    public void testCallSystemFunctionBadArg() {
        try {
            Processor proc = new Processor(true);
            XdmFunctionItem fi = proc.GetSystemFunction(new QName(NamespaceConstant.MATH, "sqrt"), 1);
            XdmAtomicValue result = (XdmAtomicValue) fi.Invoke(new XdmValue[]{ new XdmAtomicValue("minus one")}, proc);
            Assert.Fail("Unexpected success");
        } catch (SaxonApiException e) {
            Assert.That((object)"XPTY0004", Is.EqualTo(e.ErrorCode.LocalName));
        }
    }

    [Test]
    public void testCallSystemFunctionWithContextDependency() {
        try {
            Processor proc = new ();
            XdmFunctionItem fi = proc.GetSystemFunction( new QName(NamespaceConstant.FN, "local-name"), 0);
            Assert.That((object)fi, Is.Not.Null);
            XdmValue result = fi.Invoke(new XdmValue[]{}, proc);
            Assert.Fail("Unexpected success");
        } catch (SaxonApiException e) {
            Assert.That((object)"XPDY0002", Is.EqualTo(e.ErrorCode.LocalName));
        }
    }

    [Test]
    public void testCallPositionFunction() {
        try {
            Processor proc = new Processor(true);
            XdmFunctionItem fi = proc.GetSystemFunction(new QName(NamespaceConstant.FN, "position"), 0);
            Assert.That((object)fi, Is.Not.Null);
            XdmValue result = fi.Invoke(new XdmValue[]{}, proc);
            Assert.Fail("Unexpected success");
        } catch (SaxonApiException e) {
            Assert.That((object)"XPDY0002", Is.EqualTo(e.ErrorCode.LocalName));
        }
    }

    [Test]
    public void testCallSystemFunctionWithLinqNode() {
        try {
            Processor proc = new (true);
            XDocument doc = new (new XElement("root"));
            XdmNode top = proc.NewDocumentBuilder().Wrap(doc).OutermostElement;
            XdmFunctionItem fi = proc.GetSystemFunction(new QName(NamespaceConstant.FN, "local-name"), 1);
            Assert.That((object)fi, Is.Not.Null);
            XdmValue result = fi.Invoke(new XdmValue[]{top}, proc);
            object b = result[0].StringValue;
            Assert.That((object)"root", Is.EqualTo(b));
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail();
        }
    }

//     [Test]
//     public void testCallSystemFunctionFailIfUnlicensed() {
//         try {
//             Processor proc = new Processor(false);
//             XdmFunctionItem fi = proc.GetSystemFunction(new QName(NamespaceConstant.SAXON, "in-summer-time"), 2);
//             Assert.Null(fi);
// //            XdmAtomicValue arg1 = new XdmAtomicValue("2016-08-20T12:00:00", ItemType.DATE_TIME);
// //            XdmAtomicValue arg2 = new XdmAtomicValue("Europe/London");
// //            XdmAtomicValue result = (XdmAtomicValue) fi.Invoke(proc, arg1, arg2);
// //            Assert.That(result.AsBoolean);
//         } catch (SaxonApiException e) {
//             //e.printStackTrace();
//             Assert.Fail();
//         }
//     }

    // [Test]
    // public void testExtensionFunctionDefinitionAsFunction() {
    //     try {
    //         ExtensionFunctionDefinition sqrt = new TestIntegratedFunctions.SqrtFunction();
    //         XdmFunctionItem fi = new XdmFunctionItem(sqrt.asFunction(1));
    //         Processor proc = new Processor(true);
    //         XQueryCompiler compiler = proc.NewXQueryCompiler();
    //         XQueryExecutable exec = compiler.Compile("declare variable $f as function(xs:double) as xs:double external; $f(4)");
    //         XQueryEvaluator eval = exec.Load();
    //         eval.SetExternalVariable(new QName("f"), fi);
    //         XdmAtomicValue result = (XdmAtomicValue) eval.EvaluateSingle();
    //         assertAreEqual(new XdmAtomicValue(2), result);
    //     } catch (SaxonApiException e) {
    //         Assert.Fail();
    //     }
    //
    //
    // }

    [Test]
    public void testTransformReturningFunction() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.NewXsltCompiler();
            comp.SchemaAware =(true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xsl:template name='main'><xsl:sequence select='f:f#1'/></xsl:template> \n" +
                            "<xsl:function name='f:f' as='xs:integer'><xsl:param name='p'/><xsl:sequence select='$p+42'/></xsl:function> \n" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.Compile(sr);
            Xslt30Transformer transformer = exec.Load30();
            XdmValue value = transformer.CallTemplate(new QName("main"));
            XdmValue result = ((XdmFunctionItem) value).Invoke( new XdmValue[]{new XdmAtomicValue(17)}, proc);
            Assert.That(result is XdmAtomicValue);
            object b = ((XdmAtomicValue) result).AsLong();
            Assert.That((object)59, Is.EqualTo(b));
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail(e.Message);
        }

    }

    [Test]
    public void testTransformReturningFunctionReferencingGlobalVariables() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.NewXsltCompiler();
            comp.SchemaAware = (true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xsl:template name='main'><xsl:sequence select='f:f#1'/></xsl:template> \n" +
                            "<xsl:param name='offset' select='42'/> \n" +
                            "<xsl:function name='f:f' as='xs:integer'><xsl:param name='p'/><xsl:sequence select='$p+$offset'/></xsl:function> \n" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.Compile(sr);
            Xslt30Transformer transformer = exec.Load30();
            XdmValue value = transformer.CallTemplate(new QName("main"));
            XdmValue result = ((XdmFunctionItem) value).Invoke(new XdmValue[]{ new XdmAtomicValue(17)}, proc);
            Assert.That(result is XdmAtomicValue);
            object b = ((XdmAtomicValue) result).AsLong();
            Assert.That((object)59, Is.EqualTo(b));
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail(e.Message);
        }

    }

    [Test]
    public void testTransformReturningFunctionCallingTemplate() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.NewXsltCompiler();
            comp.SchemaAware = (true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xsl:template name='main'><xsl:sequence select='f:f#1'/></xsl:template> \n" +
                            "<xsl:template name='sub'><xsl:sequence select='$offset'/></xsl:template> \n" +
                            "<xsl:param name='offset' select='42'/> \n" +
                            "<xsl:function name='f:f' as='xs:integer'><xsl:param name='p'/>" +
                            "  <xsl:variable name='t' as='xs:integer'><xsl:call-template name='sub'/></xsl:variable>" +
                            "  <xsl:sequence select='$p+$t'/></xsl:function> \n" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.Compile(sr);
            Xslt30Transformer transformer = exec.Load30();
            XdmValue value = transformer.CallTemplate(new QName("main"));
            XdmValue result = ((XdmFunctionItem) value).Invoke(new XdmValue[]{new XdmAtomicValue(17)}, proc);
            Assert.That(result is XdmAtomicValue);
            object b = ((XdmAtomicValue) result).AsLong();
            Assert.That((object)59, Is.EqualTo(b));
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail(e.Message);
        }

    }

    [Test]
    public void testTransformReturningInlineFunctionReferencingGlobalVariables() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.NewXsltCompiler();
            comp.SchemaAware = (true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xsl:template name='main'><xsl:sequence select='let $q:=23 return function($p){$p+$q+$offset}'/></xsl:template> \n" +
                            "<xsl:param name='offset' select='42'/> \n" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.Compile(sr);
            Xslt30Transformer transformer = exec.Load30();
            XdmValue value = transformer.CallTemplate(new QName("main"));
            XdmValue result = ((XdmFunctionItem) value).Invoke(new XdmValue[]{new XdmAtomicValue(17)}, proc);
            Assert.That(result is XdmAtomicValue);
            object b = ((XdmAtomicValue) result).AsLong();
            Assert.That((object)82, Is.EqualTo(b));
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail(e.Message);
        }

    }

    [Test]
    public void testFunctionReturnedByFunctionLookup() {
        try {
            Processor proc = new Processor(true);
            XPathCompiler comp = proc.NewXPathCompiler();
            XdmItem value =
                comp.EvaluateSingle("function-lookup(QName('http://www.w3.org/2005/xpath-functions', 'round'), 2)",
                    null);
            XdmValue result = ((XdmFunctionItem)value).Invoke(new XdmValue[] { 
                new XdmAtomicValue(3.14159),
                new XdmAtomicValue(3)
            }, proc);
            Assert.That(result is XdmAtomicValue);
            object b = ((XdmAtomicValue) result).AsDecimal();
            Assert.That((object)3.142, Is.EqualTo(b));
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail(e.Message);
        }

    }

    [Test]
    public void testUserFunctionReturnedByFunctionLookup() {
        try {
            Processor proc = new Processor(true);
            XsltCompiler comp = proc.NewXsltCompiler();
            comp.SchemaAware = (true);
            StringReader sr = new StringReader(
                    "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:f='f' xmlns:xs='http://www.w3.org/2001/XMLSchema'>" +
                            "<xsl:template name='main'><xsl:sequence select=\"function-lookup(QName('f', 'f'), 1)\"/></xsl:template> \n" +
                            "<xsl:param name='offset' select='42'/> \n" +
                            "<xsl:function name='f:f' as='xs:integer'><xsl:param name='p'/><xsl:sequence select='$p+$offset'/></xsl:function> \n" +
                            "</xsl:stylesheet>");
            XsltExecutable exec = comp.Compile(sr);
            Xslt30Transformer transformer = exec.Load30();
            XdmValue value = transformer.CallTemplate(new QName("main"));
            XdmValue result = ((XdmFunctionItem) value).Invoke(new XdmValue[]{ new XdmAtomicValue(17)}, proc);
            Assert.That(result is XdmAtomicValue);
            object b = ((XdmAtomicValue) result).AsLong();
            Assert.That((object)59, Is.EqualTo(b));
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail(e.Message);
        }


    }

    [Test]
    public void testContextDependentFunctionReturnedByFunctionLookup() {
        try {
            Processor proc = new Processor(true);
            XPathCompiler comp = proc.NewXPathCompiler();
            XdmItem value = comp.EvaluateSingle("function-lookup(QName('http://www.w3.org/2005/xpath-functions', 'string'), 0)",
                                                new XdmAtomicValue(3.14159));
            XdmValue result = ((XdmFunctionItem) value).Invoke(new XdmValue[]{}, proc);
            Assert.That(result is XdmAtomicValue);
            Assert.That(((XdmAtomicValue) result).StringValue.StartsWith("3.14"));
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail(e.Message);
        }

    }

    [Test]
    public void testLangFunction() {
        try {
            Processor proc = new (true);
            XPathCompiler comp = proc.NewXPathCompiler();
            DocumentBuilder builder = proc.NewDocumentBuilder();
            XdmNode doc = builder.Build(new StringReader("<a xml:lang='de'>Verboten</a>"));
            XdmItem value = comp.EvaluateSingle("lang#1", doc.OutermostElement);
            XdmValue result1 = ((XdmFunctionItem) value).Invoke(new XdmValue[]{ new XdmAtomicValue("de")}, proc);
            Assert.That(result1 is XdmAtomicValue);
            Assert.That(((XdmAtomicValue) result1).AsBoolean());
            XdmValue result2 = ((XdmFunctionItem) value).Invoke(new XdmValue[]{ new XdmAtomicValue("en")}, proc);
            Assert.That(result2 is XdmAtomicValue);
            Assert.That((object)((XdmAtomicValue)result2).AsBoolean(), Is.EqualTo(false));
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail(e.Message);
        }

    }

    [Test]
    public void testQueryReturningFunctionReferencingGlobalVariables() {
        try {
            Processor proc = new Processor(true);
            XQueryCompiler comp = proc.NewXQueryCompiler();
            comp.SchemaAware = (true);
            String query = "declare variable $offset as xs:integer external;"
                    + "declare function local:f($p) as xs:integer {$p + $offset};"
                    + "local:f#1";
            XQueryExecutable exec = comp.Compile(query);
            XQueryEvaluator eval = exec.Load();
            eval.SetExternalVariable(new QName("offset"), new XdmAtomicValue(42));
            XdmValue value = eval.Evaluate();
            XdmValue result = ((XdmFunctionItem) value).Invoke(new XdmValue[]{new XdmAtomicValue(17)}, proc);
            Assert.That(result is XdmAtomicValue);
            object b = ((XdmAtomicValue) result).AsLong();
            Assert.That((object)59, Is.EqualTo(b));
        } catch (SaxonApiException e) {
            //e.printStackTrace();
            Assert.Fail(e.Message);
        }

    }

}