package jaxptest;

import com.saxonica.config.EnterpriseTransformerFactory;
import junit.framework.TestCase;
import net.sf.saxon.lib.FeatureKeys;

import javax.xml.transform.Templates;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;

/**
 * Test that DOM access is threadsafe. Test deleted - because DOM access is not thread-safe!
 */
public class ThreadTest extends TestCase {

    public void testDummy() {
        assertTrue(true);
    }

//    private static final String copyXsl =
//            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\">\n" +
//            "    <xsl:output method=\"xml\"/>\n" +
//            "\n" +
//            "    <xsl:template match=\"/\">\n" +
//            "        <xsl:apply-templates/>\n" +
//            "    </xsl:template>\n" +
//            "\n" +
//            "    <xsl:template match=\"@* | node()\">\n" +
//            "        <xsl:copy>\n" +
//            "            <xsl:apply-templates select=\"@* | node()\"/>\n" +
//            "        </xsl:copy>\n" +
//            "    </xsl:template>\n" +
//            "</xsl:stylesheet>";
//
//    private final static String testXml =
//            "<alpha a=\"b\" c=\"d\">\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "</alpha>";
//
//    public void testDomTransformationThreaded() {
//        //TransformerFactory factory = TransformerFactory.newInstance();
//        try {
//            TransformerFactory factory = new net.sf.saxon.TransformerFactoryImpl();
//            StreamSource styleSource = new StreamSource(new StringReader(copyXsl));
//            final Templates templates = factory.newTemplates(styleSource);
//            DocumentBuilderFactory docBuilderFactory =
//                DocumentBuilderFactory.newInstance();
//            DocumentBuilder docBuilder = docBuilderFactory.newDocumentBuilder();
//            final Document domDoc = docBuilder.parse(new InputSource(new StringReader(testXml)));
//            final NodeList sourceTags = domDoc.getElementsByTagName("*");
//
//            ExecutorService executor = Executors.newFixedThreadPool(10);
//            for (int i = 0; i < 100; i++) {
//                executor.execute(new Runnable() {
//
//                    public void run() {
//                        try {
//                            Transformer transformer = templates.newTransformer();
//                            DOMSource source = new DOMSource(domDoc);
//                            DOMResult result = new DOMResult();
//                            transformer.transform(source, result);
//                            NodeList resultTags = ((Document)result.getNode()).getElementsByTagName("*");
//                            assert sourceTags.getLength() == resultTags.getLength();
//                            System.out.println("thread-#" + Thread.currentThread().getId() + " OK");
//                        } catch (Throwable ex) {
//                            System.err.println("thread-# " + Thread.currentThread().getId() + " ERROR");
//                            ex.printStackTrace();
//                        }
//                    }
//                });
//            }
//            executor.shutdown();
//            executor.awaitTermination(300, TimeUnit.SECONDS);
//        } catch (TransformerConfigurationException e) {
//            fail(e.getMessage());
//        } catch (ParserConfigurationException e) {
//            e.printStackTrace();
//            fail(e.getMessage());
//        } catch (SAXException e) {
//            e.printStackTrace();
//            fail(e.getMessage());
//        } catch (IOException e) {
//            e.printStackTrace();
//            fail(e.getMessage());
//        } catch (InterruptedException e) {
//            e.printStackTrace();
//            fail(e.getMessage());
//        }
//    }

//    private static final String copyXsl =
//            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\">\n" +
//            "    <xsl:output method=\"xml\"/>\n" +
//            "\n" +
//            "    <xsl:template match=\"/\">\n" +
//            "        <xsl:apply-templates/>\n" +
//            "    </xsl:template>\n" +
//            "\n" +
//            "    <xsl:template match=\"@* | node()\">\n" +
//            "        <xsl:copy>\n" +
//            "            <xsl:apply-templates select=\"@* | node()\"/>\n" +
//            "        </xsl:copy>\n" +
//            "    </xsl:template>\n" +
//            "</xsl:stylesheet>";
//
//    private final static String testXml =
//            "<alpha a=\"b\" c=\"d\">\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "    <beta d=\"e\">\n" +
//                    "        <gamma f=\"g\"/>\n" +
//                    "        <delta h=\"i\">epsilon</delta>\n" +
//                    "    </beta>\n" +
//                    "</alpha>";

    public void testParallelByteCodeGeneration() {
        //TransformerFactory factory = TransformerFactory.newInstance();
        try {

            ExecutorService executor = Executors.newFixedThreadPool(5);
            for (int i = 0; i < 100; i++) {
                executor.execute(new Runnable() {

                    public void run() {
                        try {
                            TransformerFactory factory = new net.sf.saxon.TransformerFactoryImpl();
                            factory.setAttribute(FeatureKeys.GENERATE_BYTE_CODE, true);
                            //factory.setAttribute(FeatureKeys.DEBUG_BYTE_CODE, true);
                            StreamSource styleSource = new StreamSource(new File("/Users/mike/bugs/2014/nanz/bug-2133/pdf/main.xsl"));
                            final Templates templates = factory.newTemplates(styleSource);
                            System.out.println("thread-#" + Thread.currentThread().getId() + " OK");
                        } catch (Throwable ex) {
                            System.err.println("thread-# " + Thread.currentThread().getId() + " ERROR");
                            ex.printStackTrace();
                        }
                    }
                });
            }
            executor.shutdown();
            executor.awaitTermination(300, TimeUnit.SECONDS);

        } catch (InterruptedException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

    public void testNonParallelByteCodeGeneration() {
        // Same stylesheet as above but not multithreaded, for comparison
        try {
            TransformerFactory factory = new net.sf.saxon.TransformerFactoryImpl();
            factory.setAttribute(FeatureKeys.GENERATE_BYTE_CODE, true);
            factory.setAttribute(FeatureKeys.DEBUG_BYTE_CODE, true);
            StreamSource styleSource = new StreamSource(new File("/Users/mike/bugs/2014/nanz/bug-2133/pdf/main.xsl"));
            final Templates templates = factory.newTemplates(styleSource);
        } catch (Throwable ex) {
            ex.printStackTrace();
        }
    }


    private static final String copyXslSa =
            "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\">\n" +
                    "    <xsl:output method=\"xml\"/>\n" +
                    "    <xsl:import-schema namespace='http://schemas.openxmlformats.org/wordprocessingml/2006/main' schema-location='file:///Users/mike/Desktop/ooxml/wml.xsd'/>" +
                    "\n" +
                    "    <xsl:template match=\"/\">\n" +
                    "        <xsl:apply-templates/>\n" +
                    "    </xsl:template>\n" +
                    "\n" +
                    "    <xsl:template match=\"@* | node()\">\n" +
                    "        <xsl:copy>\n" +
                    "            <xsl:apply-templates select=\"@* | node()\"/>\n" +
                    "        </xsl:copy>\n" +
                    "    </xsl:template>\n" +
                    "</xsl:stylesheet>";

    public void testParallelSchemaImport() {
        final TransformerFactory factory = new EnterpriseTransformerFactory();
        try {

            ExecutorService executor = Executors.newFixedThreadPool(5);
            for (int i = 0; i < 100; i++) {
                executor.execute(new Runnable() {

                    public void run() {
                        try {
                            //StreamSource styleSource = new StreamSource(new File("/Users/mike/repo/samples/styles/books-sa.xsl"));
                            StreamSource styleSource = new StreamSource(new StringReader(copyXslSa), "file:///Users/mike/Desktop/ooxml/wml.xsd");
                            final Templates templates = factory.newTemplates(styleSource);
                            System.out.println("thread-#" + Thread.currentThread().getId() + " OK");
                        } catch (Throwable ex) {
                            System.err.println("thread-# " + Thread.currentThread().getId() + " ERROR");
                            ex.printStackTrace();
                        }
                    }
                });
            }
            executor.shutdown();
            executor.awaitTermination(300, TimeUnit.SECONDS);

        } catch (InterruptedException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
    }

}


//
// The contents of this file are subject to the Mozilla Public License Version
// 1.0 (the "License");
// you may not use this file except in compliance with the License. You may
// obtain a copy of the
// License at http://www.mozilla.org/MPL/
//
// Software distributed under the License is distributed on an "AS IS" basis,
// WITHOUT WARRANTY OF ANY KIND, either express or implied.
// See the License for the specific language governing rights and limitations
// under the License.
//
// The Original Code is: all this file.
//
// The Initial Developer of the Original Code is Michael Kay,
//
// Portions created by (your name) are Copyright (C) (your legal entity). All
// Rights Reserved.
//
// Contributor(s): none.
//



