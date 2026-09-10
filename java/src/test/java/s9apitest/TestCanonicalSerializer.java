package s9apitest;

import jaxptest.ConfigTest;
import net.sf.saxon.option.xom.XOMObjectModel;
import net.sf.saxon.s9api.*;
import nu.xom.Document;
import org.junit.jupiter.api.Test;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.IOException;
import java.util.Arrays;
import java.util.Objects;

import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 * JUnit tests for the serialization option (new in 9.9) saxon:canonical="yes"
 * <p>
 * The effect is tested by comparing the results with the XOM Canonicalizer.
 */
public class TestCanonicalSerializer {
    private final ConfigTest configTest = new ConfigTest();

    private void compareCanonical(File input) {
        try {
            // Canonicalize using Saxon
            Processor proc = new Processor(true);
            ByteArrayOutputStream os1 = new ByteArrayOutputStream();
            Serializer serializer = proc.newSerializer(os1);
            serializer.setOutputProperty(Serializer.Property.METHOD, "xml");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
            serializer.setOutputProperty(Serializer.Property.SAXON_CANONICAL, "yes");
            DocumentBuilder builder = proc.newDocumentBuilder();
            XdmNode doc = builder.build(input);
            proc.writeXdmValue(doc, serializer);

            // Canonicalize using XOM
            ByteArrayOutputStream os2 = new ByteArrayOutputStream();
            nu.xom.canonical.Canonicalizer canon = new nu.xom.canonical.Canonicalizer(os2);
            builder.setTreeModel(XOMObjectModel.getInstance());
            XdmNode xomWrapper = builder.build(input);
            Document xomDoc = (Document) xomWrapper.getExternalNode();
            canon.write(xomDoc);

            // Compare the results
            byte[] b1 = os1.toByteArray();
            byte[] b2 = os2.toByteArray();
            if (!Arrays.equals(b1, b2)) {
                for (int i=0; i<Math.min(b1.length, b2.length); i++) {
                    if (b1[i] != b2[i]) {
                        System.err.println("Results differ at offset " + i);
                        StringBuilder sb1 = new StringBuilder();
                        for (int j=i; j<Math.min(i+50, b1.length); j++) {
                            sb1.append((char) b1[j]);
                        }
                        System.err.println(sb1);
                        sb1.setLength(0);
                        for (int j = i; j < Math.min(i + 50, b2.length); j++) {
                            sb1.append((char) b2[j]);
                        }
                        System.err.println(sb1);
                        break;
                    }
                }
            }
            assertTrue(Arrays.equals(os1.toByteArray(), os2.toByteArray()));
            //assertTrue(Arrays.equals(os1.toByteArray(), os3));
        } catch (SaxonApiException | IOException e) {
            fail("Processing " + input + ": " + e.getMessage());
        }
    }

    @Test
    public void testCanonicalization1() {
        compareCanonical(new File(configTest.getDataDir() + "bills.xml"));
        compareCanonical(new File(configTest.getDataDir() + "categories.xml"));
        compareCanonical(new File(configTest.getDataDir() + "collection.xml"));
        compareCanonical(new File(configTest.getDataDir() + "goToWork.xml"));
        compareCanonical(new File(configTest.getDataDir() + "schema-with-param.xsd"));
    }

    @Test
    public void testCanonicalization2() {
        File dir = new File (configTest.getDataDir() + "collection");
        for (File file : Objects.requireNonNull(dir.listFiles())) {
            if (!file.isDirectory() && !file.getName().endsWith("atrs15.xml") && !file.getName().endsWith(".DS_Store")) {
                System.err.println("Processing " + file);
                compareCanonical(file);
            }
        }
    }

    @Test
    public void testCanonicalization3() {
        compareCanonical(new File(configTest.getDataDir() + "collection/axes268.xml"));
    }

    @Test
    public void testCanonicalization4() {
        compareCanonical(new File(configTest.getDataDir() + "collection/expr80.xml"));
    }


}
