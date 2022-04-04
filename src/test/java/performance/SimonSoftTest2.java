/**
 * Copyright (C) 2009-2013 Simonsoft Nordic AB
 * <p/>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p/>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p/>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
package performance;


import jaxptest.ConfigTest;
import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.Date;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

public class SimonSoftTest2 {
    private final ConfigTest configTest = new ConfigTest();

    // See bug 2929 - search for unused namespaces

    private static long numTransformed = 0;

    private final String xsl = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
            "<!--\n" +
            "\n" +
            "    Copyright (C) 2009-2013 Simonsoft Nordic AB\n" +
            "\n" +
            "    Licensed under the Apache License, Version 2.0 (the \"License\");\n" +
            "    you may not use this file except in compliance with the License.\n" +
            "    You may obtain a copy of the License at\n" +
            "\n" +
            "            http://www.apache.org/licenses/LICENSE-2.0\n" +
            "\n" +
            "    Unless required by applicable law or agreed to in writing, software\n" +
            "    distributed under the License is distributed on an \"AS IS\" BASIS,\n" +
            "    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.\n" +
            "    See the License for the specific language governing permissions and\n" +
            "    limitations under the License.\n" +
            "\n" +
            "-->\n" +
            "<xsl:stylesheet version=\"2.0\"\n" +
            "\txmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\"\n" +
            "\txmlns:cms=\"http://www.simonsoft.se/namespace/cms\"\n" +
            "\t>\n" +
            "\n" +
            "\n" +
            "\t<!-- Experimental support for determining which declared namespaces are not actually used. -->\n" +
            "\t<xsl:key name=\"ns_elem\" match=\"*[namespace-uri() != '']\" use=\"namespace-uri()\"/>\n" +
            "    <xsl:key name=\"ns_attr\" match=\"*/@*[namespace-uri() != '']\" use=\"namespace-uri()\"/>\n" +
            "\n" +
            "\t<xsl:template match=\"*\" mode=\"#default\">\n" +
            "        <xsl:element name=\"doc\">\n" +
            "        \t<xsl:apply-templates select=\".\" mode=\"i-ns-unused\"/>\n" +
            "        \n" +
            "        </xsl:element>\n" +
            "    </xsl:template>\n" +
            "    \n" +
            "    <!-- Experimental support for determining which declared namespaces are not actually used. -->\n" +
            "\t<!-- Investigates all namespaces declared on parent, which means they are inherited by this element. -->\n" +
            "\t<xsl:template match=\"*\" mode=\"i-ns-unused\">\n" +
            "\t\t<xsl:variable name=\"namespace-parent\" select=\"parent::node()/namespace::*[string() != 'http://www.w3.org/XML/1998/namespace']\"/>\n" +
            "\t\t\n" +
            "\t\t<xsl:for-each select=\"$namespace-parent\">\n" +
            "            <xsl:variable name=\"nsuri\" select=\".\"></xsl:variable>\n" +
            "            \n" +
            "            <xsl:if test=\"count(key('ns_attr', $nsuri)) = 0 and count(key('ns_elem', $nsuri)) = 0\">\n" +
            "                    <xsl:value-of select=\"$nsuri\"></xsl:value-of>\n" +
            "\t\t\t\t\t<xsl:value-of select=\"' '\"></xsl:value-of>\n" +
            "            </xsl:if>\n" +
            "        </xsl:for-each>\n" +
            "\t</xsl:template>\n" +
            "\t\n" +
            "</xsl:stylesheet>";


    @Test
    @Tag("performance")
    public void testPerformance() {
        for (int i=0; i<4; i++) {
            Date start = new Date();
            numTransformed = 0;
            try {
                StreamSource xslSource = new StreamSource(new StringReader(xsl));
                InputStream xml = new FileInputStream(configTest.getDataDir() + "xmark1withns.xml");
                transformElements(xslSource, xml);
            } catch (FileNotFoundException | SaxonApiException e) {
                fail();
            }

            Date end = new Date();
            if (i == 3) {
                long ms = end.getTime() - start.getTime();
                assertTrue(ms < 400, "speed");
                System.out.println("Transformations took: " + ms + " ms.");
            }
        }
    }

    private static void transformElements(StreamSource xsltSource, InputStream xml) throws SaxonApiException {

        Processor processor = new Processor(false);
        DocumentBuilder db = processor.newDocumentBuilder();

        XsltCompiler compiler = processor.newXsltCompiler();
        XsltExecutable xsltCompiled;
        xsltCompiled = compiler.compile(xsltSource);

        XsltTransformer transformer = xsltCompiled.load();

        XdmNode xdmDoc = db.build(new StreamSource(xml));

        StringWriter bulkWriter = new StringWriter();
        Destination destination = processor.newSerializer(bulkWriter);
        transformer.setDestination(destination);

        transformChildren(xdmDoc, transformer);

        System.out.println("Transformations: " + numTransformed);
        //assertTrue("number of transformations should be high",  numTransformed > 10000);
    }

    private static void transformChildren(XdmNode root, XsltTransformer transformer) throws SaxonApiException {

        XdmSequenceIterator it1 = root.axisIterator(Axis.CHILD);

        while (it1.hasNext()) {
            XdmItem xdmItem = it1.next();

            transformer.setInitialContextNode((XdmNode) xdmItem);
            transformer.transform();
            numTransformed++;

            if (((XdmNode) xdmItem).getNodeKind() == XdmNodeKind.ELEMENT) {
                XdmNode elem = (XdmNode) xdmItem;
                transformChildren(elem, transformer);
            }
        }
    }


}
