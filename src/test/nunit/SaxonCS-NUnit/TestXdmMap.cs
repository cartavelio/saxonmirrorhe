using System;
using System.Collections.Generic;
using Saxon.Api;
using NUnit.Framework;


namespace SaxonCS_NUnit
{
    class TestXdmMap
    {


        [Test]
        public void TestMapOperations() {
            XdmMap map = new XdmMap();
            map = map.Put(new XdmAtomicValue("a"), new XdmAtomicValue(1));
            map = map.Put(new XdmAtomicValue("b"), new XdmAtomicValue(2));
            map = map.Put(new XdmAtomicValue("c"), new XdmAtomicValue(3));
            Assert.That(3== map.Size, "Map has correct size");
            try
            {
                Assert.That(((XdmAtomicValue)map.Get(new XdmAtomicValue("b"))).AsLong()== 2);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            map = map.Remove(new XdmAtomicValue("c"));
            map = map.Remove(new XdmAtomicValue("d"));
            map = map.Put(new XdmAtomicValue("a"), new XdmAtomicValue(4));
            Assert.That(map.Size== 2);
            try
            {
                Assert.That(((XdmAtomicValue)map.Get(new XdmAtomicValue("a"))).AsLong()== 4);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }

        }

        [Test]
        public void TestConversionFromMap()
        {
            Dictionary<XdmAtomicValue, XdmAtomicValue> jam = new Dictionary<XdmAtomicValue, XdmAtomicValue>();
            jam.Add(new XdmAtomicValue("a"), new XdmAtomicValue(1));
            jam.Add(new XdmAtomicValue("b"), new XdmAtomicValue(2));
            jam.Add(new XdmAtomicValue("c"), new XdmAtomicValue(3));
            XdmMap map = XdmMap.MakeMap(jam);
            Assert.That((object)3, Is.EqualTo(map.Size));
            try {
                object b = ((XdmAtomicValue)map.Get(new XdmAtomicValue("b"))).AsLong();
                Assert.That((object)2, Is.EqualTo(b));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            map = map.Remove(new XdmAtomicValue("c"));
            map = map.Remove(new XdmAtomicValue("d"));
            map = map.Put(new XdmAtomicValue("a"), new XdmAtomicValue(4));
            Assert.That(map.Size== 2);
            try {
                object b = ((XdmAtomicValue)map.Get(new XdmAtomicValue("a"))).AsLong();
                Assert.That((object)4, Is.EqualTo(b));
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestMapAsFunction()
        {
            XdmMap map = new XdmMap();
            map = map.Put(new XdmAtomicValue("a"), new XdmAtomicValue(1));
            map = map.Put(new XdmAtomicValue("b"), new XdmAtomicValue(2));
            map = map.Put(new XdmAtomicValue("c"), new XdmAtomicValue(3));
            Assert.That(3 == map.Size);
            Processor proc = new Processor(true);
            try
            {
                XdmValue result = map.Get(new XdmAtomicValue("c"));
                Assert.That(result is XdmAtomicValue);
                Assert.That(((XdmAtomicValue)result).AsLong() == 3);
            }
            catch (Exception e)
            {
                Assert.Fail("Failure in TestMapAsFunction: " + e.Message);
            }
        }

        [Test]
        public void TestMapAsQueryParameter()
        {
            try
            {
                XdmMap map = new XdmMap();
                map = map.Put(new XdmAtomicValue("a"), new XdmAtomicValue(1));
                map = map.Put(new XdmAtomicValue("b"), new XdmAtomicValue(2));
                map = map.Put(new XdmAtomicValue("c"), new XdmAtomicValue(3));
                Processor proc = new Processor(true);
                String query =
                        "declare namespace m='http://www.w3.org/2005/xpath-functions/map';" +
                                "declare variable $a as map(*) external; " +
                                "m:size($a)";
                XQueryCompiler comp = proc.NewXQueryCompiler();
                XQueryEvaluator q = comp.Compile(query).Load();
                q.SetExternalVariable(new QName("a"), map);
                XdmAtomicValue result = (XdmAtomicValue)q.EvaluateSingle();
                Assert.That(3 == result.AsLong());
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestMapAsQueryResult()
        {
            try
            {
                Processor proc = new Processor(true);
                String query = "map{1:2, 2:3, 3:4}";
                XQueryCompiler comp = proc.NewXQueryCompiler();
                XQueryEvaluator q = comp.Compile(query).Load();
                XdmMap result = (XdmMap)q.EvaluateSingle();
                Assert.That(3== result.Size);
            }
            catch (Exception e)
            {
               
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void TestConversionToMap()
        {
            try
            {
                Processor proc = new Processor(true);
                String query = "map{1:2, 2:3, 3:4}";
                XQueryCompiler comp = proc.NewXQueryCompiler();
                XQueryEvaluator q = comp.Compile(query).Load();
                XdmMap result = (XdmMap)q.EvaluateSingle();
                Dictionary<XdmAtomicValue, XdmValue> jmap = result.AsDictionary();
                Assert.That(3 == jmap.Count);
                object output = jmap[new XdmAtomicValue(3)];
                Assert.That(4 == ((XdmAtomicValue)output).AsLong());
            }
            catch (Exception e)
            {
                
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void TestConversionFromMap2()
        {
            try
            {
                Dictionary <int, String > dictionary = new Dictionary<int, String>();
            dictionary.Add(1, "a");
            dictionary.Add(2, "b");
            XdmMap map = XdmMap.MakeMap(dictionary);
            Assert.That(2 == map.Size);
            XdmAtomicValue val1 = (XdmAtomicValue)XdmValue.MakeValue("b");
            XdmAtomicValue val2 = (XdmAtomicValue)map.Get(2);
            Assert.That(val1.Equals(val2));
                //Assert.That(ItemType.ANY_MAP.matches(map));
            }
            catch (Exception e)
            {

                Assert.Fail(e.Message);
            }

        }

        [Test]
        public void TestMapToString() {
            XdmMap map = new XdmMap();
            map = map.Put(new XdmAtomicValue("a"), new XdmAtomicValue(1));
            map = map.Put(new XdmAtomicValue("b"), new XdmAtomicValue(2));
            map = map.Put(new XdmAtomicValue("c"), new XdmAtomicValue(3));
            string result = map.ToString();
            //Console.WriteLine(result);
            Assert.That((object)result, Is.EqualTo("map{\"a\":1,\"b\":2,\"c\":3}")); // Could change
        }
    }
}
