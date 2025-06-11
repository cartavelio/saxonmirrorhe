using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;
using NUnit.Framework;
using Saxon.Api;
using Saxon.Hej;

namespace SaxonCS_NUnit {
    class ActivationTest {

        private static String oldKey = 
            "0b3bj0009b900bb0v030f0b0fbbb30f0f00b9090q0v0a000bbb030bbk0j0" +
            "903bf03b9bbbb030k0f0b0f0k0a0f03bfbq0a0q0f0v0b0a09bk0j0q090j0" +
            "00300bkbb0b00bk0300b00f030bb0bv0b0309bk0bb3030q0a0a090f0k0a0" +
            "bbk0a000r093kj9jbjf3w3j3q309zaza303000r00jqjf3bb9303w3f3w393" +
            "bjw3q3f3rbza303000r00jqjf3bb93b3f3kjj3aj99zakj933j93w3r0w3d3" +
            "q3bjf3kjq3ajvj9bzad3w3r0w3d3q3bjf39j$3f33j9bzaf0a0a0a0a9r0kj" +
            "930j99za00v0b0f0f0a0;br0$3f3q3kj9309za;br00j93q3kj9309zak0k0" +
            "rk30a0rk00k0a0k0r0b3939j0j0jqbza0j93qjr039fb09za0j93qjr0f9fb" +
            "09za0j93qjr0b9fb09za9b9br0w3d3q3bjq3b39bzar3d303wkf303q3w3d3" +
            "vjf30jab93;3q3r3r0$3q3f3r39bza930jw39303q3$bakbj0j93b9akrb9b" +
            "dbr0qjw3f3ajr3d30bzaqjf3;bak$393f3v303q3rbr093930jw39303q3$b" +
            "zaf303q3w3d3vjf309r0kjd30jw39303q3$b";
        
        private static String key2025 = 
            "zaj030v0b0q0v0q0q0q03030kbb00ba0kbv0b03ba0000b9b0bf0q03bfbq0" +
            "9bfbb00ba0bbq030b0k0b0b0f0k0a0f0f0bbfbj0a0q0b0bb0bfb3ba0j00b" +
            "v000v00b00bbj0a030f0f030k00000f09b9bv0kbk00b0b00k0b0f0k0a00b" +
            "k0a000r093kj9jbjf3w3j3q309zaj0a0fbfbj0kbbbj03b9b00fb3bv090a0" +
            "k000a0a0j0v0a030fb3b3b0bv03000v0a000f030bb90j0b090q00bq0j0f0" +
            "q090kb903bfb309b0bbb00j0v0j09bj000kbfbbbb0b0q00000f09bj0k030" +
            "v0j0kb909bfb00q0v030b0b09bf0bb9bkb9bkbkbbb00q0009b309bj0309b" +
            "30k00bf0b090fbk0kb90fbq00b3ba09bq09ba00090k0r0b0k0a0k093kj9j" +
            "bjf3w3j3q309za903000r00jqjf3bb9303w3f3w393bjw3q3f3rbza903000" +
            "r00jqjf3bb93b3f3kjj3aj99zakj933j93w3r0w3d3q3bjf3kjq3ajvj9bza" +
            "d3w3r0w3d3q3bjf39j$3f33j9bzaf0a0a0a0a9r0kj930j99zab0b0k000f0" +
            "a039r0$3f3q3kj9309za39r00j93q3kj9309zaq0a0rkb0a0rk90k0a0k0r0" +
            "b3939j0j0jqbza0j93qjr039fb09za0j93qjr0f9fb09za0j93qjr0b9fb09" +
            "za9b9br0w3d3q3bjq3b39bzaw3d3q3bj9jk3q3kjbj0jq3b393kjr093ajqj" +
            "b9zak9rk9b9br093b3d30bbj039jb3d3kja9zar3d303wkf303q3w3d3vjf3" +
            "0jab93;3q3r3r0$3q3f3r39bzaf303q3w3d3vjf309akrb9bdbr0qjw3f3aj" +
            "r3d30bzabj0j93b9rkbjq3w399akf303q3w3d3vjf309r093930jw39303q3" +
            "$bzaf303q3w3d3vjf309r0kjd30jw39303q3$b";


        private static String Deobfuscate(String x) {
            String digits = "afk0b93jvqz;$rwd";
            StringBuilder buff = new StringBuilder(x.Length);
            for (int i = x.Length - 1; i >= 0; i -= 2) {
                int c1 = digits.IndexOf(x[i]);
                int c2 = digits.IndexOf(x[i - 1]);
                buff.Append((char) (c1 << 4 | c2));
            }
            return buff.ToString();
        }

        // Activate Saxon at the level of a Saxon.Api Processor
        public static void activate(Processor processor, String key) {
            processor.Implementation.setConfigurationProperty("http://saxonica.com/oem-data", Deobfuscate(key));
        }

        // Test that it works
        [Test]
        public void TestActivation() {
            Processor processor = new Processor(true);
            activate(processor, key2025);
            XPathCompiler xpc = processor.NewXPathCompiler();
            xpc.DeclareNamespace("saxon", "http://saxon.sf.net/");
            XPathExecutable xpe = xpc.Compile("saxon:decimal-divide(10.0, 3.0, 5)");
            XdmValue result = xpe.Load().Evaluate();
            Console.Out.WriteLine(result.ToString());
        }
        
        [Test]
        public void TestExpiredActivation() {
            try {
                Processor processor = new Processor(true);
                activate(processor, oldKey);
                XPathCompiler xpc = processor.NewXPathCompiler();
                xpc.DeclareNamespace("saxon", "http://saxon.sf.net/");
                XPathExecutable xpe = xpc.Compile("saxon:decimal-divide(10.0, 3.0, 5)");
                XdmValue result = xpe.Load().Evaluate();
                Console.Out.WriteLine(result.ToString());
                Assert.Fail("Expired license not detected");
            }
            catch (Exception e) {
                Assert.That(e is Saxon.Hej.trans.LicenseException);
            }
        }



    }
}