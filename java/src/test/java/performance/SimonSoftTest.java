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

public class SimonSoftTest {
    private final ConfigTest configTest = new ConfigTest();

    // See bug 2929 - cost of whitespace stripping

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
            "\t<xsl:strip-space elements=\"*\"/>\n" +
            "\n" +
            "\t<xsl:template match=\"@*|node()\" mode=\"#all\">\n" +
            "        <xsl:copy>\n" +
            "            <xsl:apply-templates select=\"@*|node()\" mode=\"#current\"/>\n" +
            "        </xsl:copy>\n" +
            "    </xsl:template>\n" +
            "\t\n" +
            "</xsl:stylesheet>";


    @Test
    @Tag("performance")
    public void testPerformance() {
        for (int i=0; i<4; i++) {
            numTransformed = 0;
            Date start = new Date();

            try {
                StreamSource xslSource = new StreamSource(new StringReader(xsl));


                InputStream xml = new FileInputStream(configTest.getDataDir() + "xmark1.xml");

                transformElements(xslSource, xml);
            } catch (FileNotFoundException | SaxonApiException e) {
                fail();
            }

            Date end = new Date();
            if (i == 3) {
                long ms = end.getTime() - start.getTime();
                System.out.println("Transformations took: " + (end.getTime() - start.getTime()) + " ms.");
                // Performance requirement relaxed in v11. The test is significantly slower than v10
                // because of the overhead of converting input strings to UnicodeString objects.
                assertTrue(ms < 450, "speed");
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

        //StringWriter bulkWriter = new StringWriter();
        ByteArrayOutputStream bulkWriter = new ByteArrayOutputStream();
        Destination destination = processor.newSerializer(bulkWriter);
        transformer.setDestination(destination);

        transformChildren(xdmDoc, transformer);

        System.out.println("Transformations: " + numTransformed);
        //assertTrue("number of transformations should be high",  numTransformed > 10000);
    }

    private static void transformChildren(XdmNode root, XsltTransformer transformer) throws SaxonApiException {

        XdmSequenceIterator it1 = root.axisIterator(Axis.CHILD);

        while (it1.hasNext()) {
            XdmNode xdmItem = (XdmNode)it1.next();
            if (!(xdmItem.getNodeKind() == XdmNodeKind.TEXT && "".equals(xdmItem.getStringValue().trim()))) {
                transformer.setInitialContextNode(xdmItem);
                transformer.transform();
                numTransformed++;

                if (xdmItem.getNodeKind() == XdmNodeKind.ELEMENT) {
                    transformChildren(xdmItem, transformer);
                }
            }
        }
    }


}
