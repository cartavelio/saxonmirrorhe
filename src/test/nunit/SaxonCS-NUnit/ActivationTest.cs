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

        private static String key = 
            "bbv0fbj0v0b0a0bba0j00bj0fbk0bbk03b0bk0909b3bv0k030j0bb30f0j0" +
            "0b0bb0v0q0q0a0bb9bb0b0f0k0a0303b30q0a0k0a000k0f0v03b00bb3030" +
            "f0q0bbb00bbbfbk090fbbb3bq030fbkba0j0j0b0a09090b0b0f0k0a00bk0" +
            "a000r093kj9jbjf3w3j3q309zaza303000r00jqjf3bb9303w3f3w393bjw3" +
            "q3f3rbza303000r00jqjf3bb93b3f3kjj3aj99zakj933j93w3r0w3d3q3bj" +
            "f3kjq3ajvj9bzad3w3r0w3d3q3bjf39j$3f33j9bzaf0a0a0a0a9r0kj930j" +
            "99zaj030k0a0f0a039r0$3f3q3kj9309za39r00j93q3kj9309zab0k0rkk0" +
            "a0rkk0k0a0k0r0b3939j0j0jqbza0j93qjr039fb09za0j93qjr0f9fb09za" +
            "0j93qjr0b9fb09za9b9br0w3d3q3bjq3b39bzar3d303wkf303q3w3d3vjf3" +
            "0jab93;3q3r3r0$3q3f3r39bzaqkbj0j93b9vkakf303q3w3d3vjf309akrb" +
            "9bdbr0qjw3f3ajr3d30bzaqjf3;bak$393f3v303q3rbr093930jw39303q3" +
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
        public static void activate(Processor processor) {
            processor.Implementation.setConfigurationProperty("http://saxonica.com/oem-data", Deobfuscate(key));
        }

        // Test that it works
        [Test]
        public void TestActivation() {
            Processor processor = new Processor(true);
            activate(processor);
            XPathCompiler xpc = processor.NewXPathCompiler();
            xpc.DeclareNamespace("saxon", "http://saxon.sf.net/");
            XPathExecutable xpe = xpc.Compile("saxon:decimal-divide(10.0, 3.0, 5)");
            XdmValue result = xpe.Load().Evaluate();
            Console.Out.WriteLine(result.ToString());
        }



    }
}