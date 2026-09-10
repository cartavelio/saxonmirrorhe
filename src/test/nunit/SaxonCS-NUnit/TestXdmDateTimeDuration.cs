using System;
using NUnit.Framework;
using Saxon.Api;

namespace SaxonCS_NUnit {
    /// <summary>
    /// Tests for APIs specific to XDM dates, times, and duration
    /// </summary>
    class TestXdmDateTimeDuration {

        [Test]
        public void TestNewAtomicValueFromDateTime() {
            try {
                DateTime now1 = DateTime.Now;
                XdmAtomicValue now2 = new (now1);
                Assert.That(now1.ToString("yyyy-MM-ddTHH:mm:ss"), Is.EqualTo(now2.StringValue[..19]));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void TestNewAtomicValueFromDateTimeOffset() {
            try {
                DateTimeOffset now1 = new(new DateTime(2022, 5, 7, 19, 39, 23), TimeSpan.FromHours(-5));
                XdmAtomicValue now2 = new (now1);
                Assert.That("2022-05-07T19:39:23-05:00", Is.EqualTo(now2.StringValue));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }
    }



}