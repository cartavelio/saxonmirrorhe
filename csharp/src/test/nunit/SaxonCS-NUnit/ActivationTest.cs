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
            "zab00b9030b0b00bj09bk0b0fbkbq0v00030k090k00b3b0b9bkbb0f0kbkb" +
            "3b3bfb909b9b0030f0a090b0f0k0a0j000b00b90v0f0a03bfbkbk0fbkb30" +
            "j03000fba090a0f030bbv0kb90bb90bb0bj0k030a0a0a09090b0f0k0a00b" +
            "k0a000r093kj9jbjf3w3j3q309zaf0a0kb90q000a03bf0b0a09bq0f0v090" +
            "f03b9b30j0j0k0bbfb000bbbb0f09bj0v000a0j0f0f000kb9b9bq0fbfbfb" +
            "30fbkbk0f0900b9ba03b30v0v0k030j00bv0003ba0q03bb0f0q0q0q0a0q0" +
            "bba0a0q0j03bbbv0a00000bbf0v0kb90v0kbbbk00b00bbk0kbf0a0j0kb90" +
            "f0k0bba0v0q0bb300bfb00k0b0f030b0kbk0bbbba00br0b0k0a0k093kj9j" +
            "bjf3w3j3q309za903000r00jqjf3bb9303w3f3w393bjw3q3f3rbza903000" +
            "r00jqjf3bb93b3f3kjj3aj99zakj933j93w3r0w3d3q3bjf3kjq3ajvj9bza" +
            "d3w3r0w3d3q3bjf39j$3f33j9bzaf0a0a0a0a9r0kj930j99zak0903000f0" +
            "a039r0$3f3q3kj9309za39r00j93q3kj9309zaq0a0rkq0a0rk90k0a0k0r0" +
            "b3939j0j0jqbza0j93qjr039fb09za0j93qjr0f9fb09za0j93qjr0b9fb09" +
            "za9b9br0w3d3q3bjq3b39bzaw3d3q3bj9jk3q3kjbj0jq3b393kjr093ajqj" +
            "b9zak9rk9b9br093b3d30bbj039jb3d3kja9zar3d303wkf303q3w3d3vjf3" +
            "0jab93;3q3r3r0$3q3f3r39bzaf303q3w3d3vjf309akrb9bdbr0qjw3f3aj" +
            "r3d30bzaqjf3;bak$393f3v303q3rbr093930jw39303q3$bzaf303q3w3d3" +
            "vjf309r0kjd30jw39303q3$b";


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