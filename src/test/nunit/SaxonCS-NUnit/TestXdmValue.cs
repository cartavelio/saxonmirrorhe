using System;
using System.Linq;
using NUnit.Framework;
using Saxon.Api;

namespace SaxonCS_NUnit {
    public class TestXdmValue {

        [Test]
        public void TestGetRange() {
            XdmValue v = XdmEmptySequence.Instance;
            v = v.Append(new XdmAtomicValue(10));
            v = v.Append(new XdmAtomicValue(11));
            v = v.Append(new XdmAtomicValue(12));
            v = v.Append(new XdmAtomicValue(13));
            v = v.Append(new XdmAtomicValue(14));
            XdmValue v1 = v.GetRange(1, 4);
            Assert.AreEqual(v1.Count(), 4);
            Assert.AreEqual(v1.ItemAt(0).StringValue, "11");
            XdmValue v2 = v.GetRange(0, 4);
            Assert.AreEqual(v2.Count(), 4);
            Assert.AreEqual(v2.ItemAt(0).StringValue, "10");
            try {
                XdmValue v3 = v.GetRange(-1, 3);
                Assert.Fail("Unexpected success");
            }
            catch (IndexOutOfRangeException e) {
                // ok, expected error
            }
        }

        [Test]
        public void TestFilterByPredicate() {
            XdmValue v = XdmEmptySequence.Instance;
            v = v.Append(new XdmAtomicValue(10));
            v = v.Append(new XdmAtomicValue(11));
            v = v.Append(new XdmAtomicValue(12));
            v = v.Append(new XdmAtomicValue(13));
            v = v.Append(new XdmAtomicValue(14));
            XdmValue w = new XdmValue(v.Where(it => {
                try {
                    return ((XdmAtomicValue)it).AsLong() > 12 ;
                } catch (SaxonApiException e) {
                    return false;
                }
            }).GetEnumerator());
            Assert.AreEqual(w.Count, 2);
            Assert.AreEqual(w.ItemAt(0).StringValue, "13");
        }

        [Test]
        public void testFilterByType() {
            XdmValue v = XdmEmptySequence.Instance;
            v = v.Append(new XdmAtomicValue(10));
            v = v.Append(new XdmAtomicValue(11.1));
            v = v.Append(new XdmAtomicValue(12));
            v = v.Append(new XdmAtomicValue(13.1));
            v = v.Append(new XdmAtomicValue(14));
            XdmValue w = new (v.Where(it => XdmAtomicType.Integer.Matches(it)).GetEnumerator());
            Assert.AreEqual(3, w.Count());
            Assert.AreEqual("10\n12\n14", w.ToString());
            Assert.AreEqual("10", w.ItemAt(0).StringValue, "10");
        }

        [Test]
        public void testDecimalDivide() {
            Processor p = new();
            XdmValue result = p.NewXPathCompiler().Compile("1 div 7").Load().Evaluate();
            Assert.AreEqual(((XdmAtomicValue)result).StringValue, "0.142857142857142857");
        }


    }

}