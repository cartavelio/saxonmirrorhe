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
            Assert.True(3== map.Size, "Map has correct size");
            try
            {
                Assert.True(((XdmAtomicValue)map.Get(new XdmAtomicValue("b"))).AsLong()== 2);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            map = map.Remove(new XdmAtomicValue("c"));
            map = map.Remove(new XdmAtomicValue("d"));
            map = map.Put(new XdmAtomicValue("a"), new XdmAtomicValue(4));
            Assert.True(map.Size== 2);
            try
            {
                Assert.True(((XdmAtomicValue)map.Get(new XdmAtomicValue("a"))).AsLong()== 4);
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
            Assert.AreEqual(3, map.Size);
            try
            {
                Assert.AreEqual(2, ((XdmAtomicValue)map.Get(new XdmAtomicValue("b"))).AsLong());
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
            map = map.Remove(new XdmAtomicValue("c"));
            map = map.Remove(new XdmAtomicValue("d"));
            map = map.Put(new XdmAtomicValue("a"), new XdmAtomicValue(4));
            Assert.True(map.Size== 2);
            try
            {
                Assert.AreEqual(4, ((XdmAtomicValue)map.Get(new XdmAtomicValue("a"))).AsLong());
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
            Assert.True(3 == map.Size);
            Processor proc = new Processor(true);
            try
            {
                XdmValue result = map.Get(new XdmAtomicValue("c"));
                Assert.True(result is XdmAtomicValue);
                Assert.True(((XdmAtomicValue)result).AsLong() == 3);
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
                Assert.True(3 == result.AsLong());
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
                Assert.True(3== result.Size);
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
                Assert.True(3 == jmap.Count);
                object output = jmap[new XdmAtomicValue(3)];
                Assert.True(4 == ((XdmAtomicValue)output).AsLong());
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
            Assert.True(2 == map.Size);
            XdmAtomicValue val1 = (XdmAtomicValue)XdmValue.MakeValue("b");
            XdmAtomicValue val2 = (XdmAtomicValue)map.Get(2);
            Assert.True(val1.Equals(val2));
                //Assert.True(ItemType.ANY_MAP.matches(map));
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
            Assert.AreEqual(result, "map{\"a\":1,\"b\":2,\"c\":3}"); // Could change
        }
    }
}
