using System;
using System.Linq;
using NUnit.Framework;
using Saxon.Api;

namespace SaxonCS_NUnit {
    class TestXdmItemType {
        [Test]
        public void testParseSimpleItemType() {
            Processor proc = new();
            XdmItemType xs_string = proc.ParseItemType("xs:string");
            Assert.AreEqual(xs_string, XdmAtomicType.String);
            Assert.That(xs_string.Matches(new XdmAtomicValue("banana")));
            XdmSequenceType xs_string_plus = xs_string.Occurs('+');
            Assert.That(xs_string_plus.Matches(new XdmAtomicValue("banana")));
            Assert.That(!xs_string_plus.Matches(XdmEmptySequence.Instance));
            Assert.That(!xs_string_plus.Matches(new XdmAtomicValue(42)));
        }

        [Test]
        public void testParseMapType() {
            Processor proc = new();
            XdmItemType mapType = proc.ParseItemType("map(xs:string, xs:integer*)");
            XdmMap map = new XdmMap();
            map = map.Put(new XdmAtomicValue("banana"), new XdmAtomicValue(42));
            map = map.Put(new XdmAtomicValue("rhubarb"), new XdmAtomicValue(-5));
            Assert.That(mapType.Matches(map));
            XdmMap hetero = map.Put(new XdmAtomicValue("spinach"), new XdmAtomicValue(3.14159));
            Assert.That(!mapType.Matches(hetero));
            XdmSequenceType manyMaps = mapType.OneOrMore;
            XdmValue twoMaps = map.Append(map);
            Assert.That(manyMaps.Matches(twoMaps));
            XdmSequenceType oneMap = mapType.One;
            Assert.That(oneMap.Matches(map));
            Assert.That(!oneMap.Matches(twoMaps));
            Assert.That(mapType.Occurs('*').Matches(XdmEmptySequence.Instance));
        }

        [Test]
        public void testParseRecordType() {
            Processor proc = new();
            proc.SetProperty(Feature.XQUERY_VERSION, "4.0");
            XdmItemType mapType = proc.ParseItemType("record(banana as xs:string, rhubarb as xs:integer*)");
            XdmMap map = new XdmMap();
            map = map.Put(new XdmAtomicValue("banana"), new XdmAtomicValue("custard"));
            map = map.Put(new XdmAtomicValue("rhubarb"), new XdmAtomicValue(-5));
            Assert.That(mapType.Matches(map));
            XdmMap hetero = map.Put(new XdmAtomicValue("spinach"), new XdmAtomicValue(3.14159));
            Assert.That(!mapType.Matches(hetero));
            XdmSequenceType manyMaps = mapType.ZeroOrMore;
            XdmValue twoMaps = map.Append(map);
            Assert.That(manyMaps.Matches(twoMaps));
            XdmSequenceType oneMap = mapType.ZeroOrOne;
            Assert.That(oneMap.Matches(map));
            Assert.That(!oneMap.Matches(twoMaps));
            Assert.That(mapType.Occurs('*').Matches(XdmEmptySequence.Instance));
        }

        [Test]
        public void testPrimitiveConstructors() {
            try {
                XdmAtomicValue v = new XdmAtomicValue("2009-10-08", XdmAtomicType.Date);
                Assert.That(XdmAtomicType.Date.Matches(v), "date");
                v = new XdmAtomicValue("2009-10-08T12:00:00", XdmAtomicType.DateTime);
                Assert.That(XdmAtomicType.DateTime.Matches(v), "dateTime");
                v = new XdmAtomicValue("12:00:00", XdmAtomicType.Time);
                Assert.That(XdmAtomicType.Time.Matches(v), "time");
                v = new XdmAtomicValue("P12Y5M6DT12M", XdmAtomicType.Duration);
                Assert.That(XdmAtomicType.Duration.Matches(v), "duration");
                v = new XdmAtomicValue("2009-10", XdmAtomicType.GYearMonth);
                Assert.That(XdmAtomicType.GYearMonth.Matches(v), "gYM");
                v = new XdmAtomicValue("2009", XdmAtomicType.GYear);
                Assert.That(XdmAtomicType.GYear.Matches(v), "gy");
                v = new XdmAtomicValue("--10-08", XdmAtomicType.GMonthDay);
                Assert.That(XdmAtomicType.GMonthDay.Matches(v), "gmd");
                v = new XdmAtomicValue("--10", XdmAtomicType.GMonth);
                Assert.That(XdmAtomicType.GMonth.Matches(v), "gm");
                v = new XdmAtomicValue("---08", XdmAtomicType.GDay);
                Assert.That(XdmAtomicType.GDay.Matches(v), "gd");
                v = new XdmAtomicValue("1C1C00", XdmAtomicType.HexBinary);
                Assert.That(XdmAtomicType.HexBinary.Matches(v), "hex");
                v = new XdmAtomicValue("-31000", XdmAtomicType.Short);
                Assert.That(XdmAtomicType.Short.Matches(v), "short");
                Assert.That(XdmAtomicType.Integer.Matches(v), "int");
                Assert.False(XdmAtomicType.Byte.Matches(v), "byte");
                v = new XdmAtomicValue("banana", XdmAtomicType.UntypedAtomic);
                Assert.That(XdmAtomicType.UntypedAtomic.Matches(v), "untyped");
            }
            catch (SaxonApiException err) {
                Assert.Fail("Valid value rejected");
            }
        }

        [Test]
        public void testItemTypeAsPredicate() {
            XdmValue v = new XdmAtomicValue(11);
            v = v.Append(new XdmAtomicValue(12.1));
            v = v.Append(new XdmAtomicValue(13));
            v = v.Append(new XdmAtomicValue(14.1));
            v = v.Append(new XdmAtomicValue(15));
            XdmValue result = XdmEmptySequence.Instance;
            foreach (XdmItem it in v.Where(XdmAtomicType.Integer.Matches)) {
                result = result.Append(it);
            }

            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void testEquals() {
            Processor proc = new ();
            XdmItemType mapType1 = proc.ParseItemType("map(xs:string, element()*)");
            XdmItemType mapType2 = proc.ParseItemType("map ( xs:string , element(*) *)");
            XdmItemType mapType3 = proc.ParseItemType("map ( xs:string , element(*)+)");
            Assert.AreEqual(mapType1, mapType2);
            Assert.AreNotEqual(mapType1, mapType3);
            Assert.AreEqual(mapType1.OneOrMore, mapType2.OneOrMore);
            Assert.AreNotEqual(mapType1.OneOrMore, mapType1.ZeroOrMore);
        }

        [Test]
        public void testSubsumption() {
            Processor proc = new ();
            Assert.That(XdmAtomicType.String.Subsumes(XdmAtomicType.NCName));
            Assert.That(XdmAtomicType.Decimal.Subsumes(XdmAtomicType.Integer));
            Assert.That(XdmAtomicType.Integer.Subsumes(XdmAtomicType.NonNegativeInteger));
            XdmItemType mapType1 = proc.ParseItemType("map(xs:string, node()*)");
            XdmItemType mapType2 = proc.ParseItemType("map(xs:string, element()*)");
            Assert.That(mapType1.Subsumes(mapType2));
            Assert.That(XdmAnyMapType.Instance.Subsumes(mapType1));
            Assert.That(XdmAnyNodeType.Instance.Subsumes(XdmNodeKind.Element));

            Assert.That(!XdmAtomicType.NCName.Subsumes(XdmAtomicType.String));
            Assert.That(!XdmAtomicType.Integer.Subsumes(XdmAtomicType.Decimal));
            Assert.That(!XdmAtomicType.Integer.Subsumes(XdmAtomicType.String));
            Assert.That(!mapType2.Subsumes(mapType1));
            Assert.That(!mapType1.Subsumes(XdmAnyMapType.Instance));
            Assert.That(!XdmNodeKind.Comment.Subsumes(XdmAnyNodeType.Instance));
        }





        [Test]
        public void testItemTypeToString() {
            try {
                Processor p = new Processor(true);
                Assert.AreEqual("xs:boolean", XdmAtomicType.Boolean.ToString());
                Assert.AreEqual("xs:QName", XdmAtomicType.QName.ToString());
                Assert.AreEqual("item()", XdmAnyItemType.Instance.ToString());
                Assert.AreEqual("xs:anyAtomicType", XdmAtomicType.Instance.ToString());
                Assert.AreEqual("node()", XdmAnyNodeType.Instance.ToString());
                Assert.AreEqual("element()", XdmNodeKind.Element.ToString());
                Assert.AreEqual("comment()", XdmNodeKind.Comment.ToString());
                Assert.AreEqual("map(*)", XdmAnyMapType.Instance.ToString());
                Assert.AreEqual("array(*)", XdmAnyArrayType.Instance.ToString());
                Assert.AreEqual("function(*)", XdmAnyFunctionType.Instance.ToString());
                XdmFunctionItem nameFn = p.GetSystemFunction(new QName(NamespaceConstant.FN, "name"), 1);
                Assert.NotNull(nameFn);
                XdmItemType nameFnType = XdmItemType.ForItem(nameFn, p);
                Assert.AreEqual("(function(node()?) as xs:string)", nameFnType.ToString());
            }
            catch (Exception e) {
                Assert.Fail();
            }
        }

        [Test]
        public void testSequenceTypeToString() {
            try {
                Processor p = new Processor(true);
                Assert.AreEqual("xs:boolean", XdmAtomicType.Boolean.One.ToString());
                Assert.AreEqual("xs:QName*", XdmAtomicType.QName.ZeroOrMore.ToString());
                Assert.AreEqual("item()+", XdmAnyItemType.Instance.OneOrMore.ToString());
                Assert.AreEqual("element()?", XdmNodeKind.Element.ZeroOrOne.ToString());
                XdmFunctionItem nameFn = p.GetSystemFunction(new QName(NamespaceConstant.FN, "name"), 1);
                Assert.NotNull(nameFn);
                XdmItemType nameFnType = XdmItemType.ForItem(nameFn, p);
                Assert.AreEqual("(function(node()?) as xs:string)*", nameFnType.ZeroOrMore.ToString());
            }
            catch (Exception e) {
                Assert.Fail();
            }
        }
    }
}
