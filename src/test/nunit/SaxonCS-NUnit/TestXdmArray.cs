using System;
using System.Collections.Generic;
using Saxon.Api;
using NUnit.Framework;

namespace SaxonCS_NUnit {
    class TestXdmArray {
        [Test]
        public void testArrayOperations() {
            XdmArray a = new XdmArray();
            Assert.True(0 == a.ArrayLength());
            Assert.True(1 == a.Count);

            a = new XdmArray(new XdmValue[] {new XdmAtomicValue(1), new XdmAtomicValue(2), new XdmAtomicValue(3)});
            Assert.True(3 == a.ArrayLength());
            Assert.True(1 == a.Count);
            try {
                Assert.True(((XdmAtomicValue) a.Get(1)).AsLong() == 2);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }

            a = a.Put(1, new XdmAtomicValue(17));
            try {
                Assert.True(((XdmAtomicValue) a.Get(1)).AsLong() == 17);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }

            List<XdmValue> values = a.AsList();
            try {
                Assert.True(((XdmAtomicValue) values[1]).AsLong() == 17);
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
                Assert.True(result is XdmAtomicValue);
                Assert.True(((XdmAtomicValue) result).AsLong() == 1);
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
                Assert.True(3 == result.AsLong());
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
                Assert.True(3 == result.ArrayLength());
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestArrayConvertIntegers() {
            int[] inn = new int[] {5, 6, 7, 8};
            XdmArray result = XdmArray.MakeArray(inn);
            Assert.True(4 == result.ArrayLength());
            Assert.True(new XdmAtomicValue(5).Equals(result.Get(0)));
            //Assert.True(XdmItemType.INT.matches((XdmAtomicValue)result.get(3)));
        }

        [Test]
        public void TestArrayConvertLongs() {
            long[] inn = new long[] {5, 6, 7, 8};
            XdmArray result = XdmArray.MakeArray(inn);
            Assert.True(4 == result.ArrayLength());
            Assert.True(new XdmAtomicValue(5).Equals(result.Get(0)));
            Console.WriteLine(((XdmAtomicValue) result.Get(3)).TypeName.ToString());
            // Assert.True(ItemType.LONG.matches((XdmAtomicValue)result.Get(3)));
        }

        [Test]
        public void TestArrayConvertStrings() {
            String[] inn = new String[] {"a", "b", "c", "d"};
            XdmArray result = XdmArray.MakeArray(inn);
            Assert.True(4 == result.ArrayLength());
            Assert.True(new XdmAtomicValue("a").Equals(result.Get(0)));
            //Assert.True(ItemType.ANY_ARRAY.matches(result));
            //Assert.True(ItemType.STRING.matches((XdmAtomicValue)result.Get(3)));
        }

        [Test]
        public void TestArrayConvertBooleans() {
            bool[] inn = new bool[] {true, true, false};
            XdmArray result = XdmArray.MakeArray(inn);
            Assert.True(3 == result.ArrayLength());
            Assert.True(XdmValue.MakeValue(true).Equals(result.Get(0)));
            //Assert.True(ItemType.BOOLEAN.matches((XdmAtomicValue)result.Get(2)));
        }

        [Test]
        public void TestArrayConvertArrays() {
            String[] in0 = new String[] {"a", "b", "c", "d"};
            String[] in1 = new String[] {"w", "x", "y", "z"};
            XdmArray result = XdmArray.MakeArray(new String[][] {in0, in1});
            Assert.True(2 == result.ArrayLength());
            Assert.True(new XdmAtomicValue("a").Equals(((XdmArray) result.Get(0)).Get(0)));
            // Assert.True(ItemType.ANY_ARRAY.matches((XdmItem)result.get(1)));
            // Assert.True(ItemType.STRING.matches((XdmItem)((XdmArray)result.get(1)).get(2)));
            // Processor proc = new Processor(true);
            // ItemTypeFactory factory = new ItemTypeFactory(proc);
            // ItemType arrayOfString = factory.getArrayType(SequenceType.makeSequenceType(ItemType.STRING, OccurrenceIndicator.ONE));
            // ItemType arrayOfArray = factory.getArrayType(SequenceType.makeSequenceType(arrayOfString, OccurrenceIndicator.ONE));
            //  Assert.True(arrayOfArray.matches(result));
        }
        
        [Test]
        public void TestArrayToString() {
            XdmArray a = new XdmArray();
            a = a.AppendMember(new XdmAtomicValue("a"));
            a = a.AppendMember(new XdmAtomicValue("b"));
            a = a.AppendMember(new XdmAtomicValue(3));
            string result = a.ToString();
            //Console.WriteLine(result);
            Assert.AreEqual(result, "[\"a\",\"b\",3]"); 
        }
    }
}