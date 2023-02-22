package nativetests;

import com.saxonica.config.ProfessionalConfiguration;
import com.saxonica.ptree.PTreeReader;
import com.saxonica.ptree.PTreeWriter;
import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.expr.sort.CodepointCollator;
import net.sf.saxon.expr.sort.GenericAtomicComparer;
import net.sf.saxon.functions.DeepEqual;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.query.QueryResult;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.SingleNodeIterator;
import net.sf.saxon.tree.tiny.TinyDocumentImpl;
import net.sf.saxon.tree.tiny.TinyTree;

import javax.xml.transform.stream.StreamSource;
import java.io.*;

/**
 * Test writing files to PTREE format and reading them back again
 */
public class PTreeTest extends TestCase {

    public static final String testDir = ConfigTest.DATA_DIR;

    public void testPTreeVersion1() {
        doPtreeTest(1);
    }


    private void doPtreeTest(int version) {
        ProfessionalConfiguration config = new ProfessionalConfiguration();
        GenericAtomicComparer comparer = new GenericAtomicComparer(CodepointCollator.getInstance(), null);
        File dir = new File(testDir);
        File[] files = dir.listFiles(
                new FilenameFilter() {
                    public boolean accept(File dir, String name) {
                        return name.endsWith(".xml") || name.endsWith(".xsl") || name.endsWith(".xsd");
                    }
                }
        );

        for (File file : files) {
            System.err.println(file.getPath());
            try {
                NodeInfo doc;
                try {
                    doc = config.buildDocumentTree(new StreamSource(file)).getRootNode();
                } catch (XPathException parseErr) {
                    // ignore bad XML documents
                    continue;
                }
                TinyTree tree = ((TinyDocumentImpl) doc).getTree();
                PTreeWriter writer = new PTreeWriter(config);
                writer.setPTreeVersion(version);
                ByteArrayOutputStream byteStream = new ByteArrayOutputStream(65536);
                DataOutputStream out = new DataOutputStream(byteStream);
                writer.writeTree(tree, out);
                out.close();
                byte[] byteArray = byteStream.toByteArray();

                // Read the file using the original configuration so we can compare using deep-equal
                boolean result1 = readAndCheck(config, config, comparer, doc, byteArray);
                assertTrue(file.getPath(), result1);

                // Read the file using a new configuration so we don't rely on a shared name pool
                boolean result2 = readAndCheck(config, new ProfessionalConfiguration(), comparer, doc, byteArray);
                assertTrue(file.getPath(), result2);

            } catch (XPathException e) {
                e.printStackTrace();
                fail();
            } catch (IOException e) {
                e.printStackTrace();
                fail();
            }
        }
    }

    private boolean readAndCheck(ProfessionalConfiguration oldConfig,
                                 ProfessionalConfiguration newConfig,
                                 GenericAtomicComparer comparer,
                                 NodeInfo doc,
                                 byte[] byteArray) throws IOException, XPathException {
        ByteArrayInputStream bytesIn = new ByteArrayInputStream(byteArray);
        PTreeReader reader = new PTreeReader();
        DataInputStream din = new DataInputStream(new BufferedInputStream(bytesIn));
        NodeInfo doc2 = reader.readTree(din, "http://dummy.uri/", newConfig).getRootNode();
        int flags = DeepEqual.WARNING_IF_FALSE | DeepEqual.COMPARE_ID_FLAGS;
        boolean compareSerializations;
        boolean ok = false;
        if (oldConfig == newConfig) {
            XPathContext context = oldConfig.getConversionContext();
            ok = DeepEqual.deepEqual(
                    SingleNodeIterator.makeIterator(doc),
                    SingleNodeIterator.makeIterator(doc2), comparer, context,
                    flags);
            compareSerializations = !ok;
        } else {
            compareSerializations = true;
        }
        if (compareSerializations) {
            String s1 = QueryResult.serialize(doc);
            String s2 = QueryResult.serialize(doc2);
            ok = s1.equals(s2);
            if (!ok) {
                System.err.println("================== ORIG =================");
                System.err.println(s1);
                System.err.println("=================== NEW =================");
                System.err.println(s2);
            }
        }
        return ok;
    }

    public void testLongText() {
        testLongText(10, "", "");
        testLongText(100, "", "");
        testLongText(1000, "", "");
        testLongText(10000, "", "");
        testLongText(100000, "", "");
    }

    public void testLongComment() {
        testLongText(0, "<!--", "-->");
        testLongText(10, "<!--", "-->");
        testLongText(100, "<!--", "-->");
        testLongText(1000, "<!--", "-->");
        testLongText(10000, "<!--", "-->");
        testLongText(100000, "<!--", "-->");
    }

    public void testLongPI() {
        testLongText(0, "<?pi", "?>");
        testLongText(10, "<?pi ", "?>");
        testLongText(100, "<?pi ", "?>");
        testLongText(1000, "<?pi ", "?>");
        testLongText(10000, "<?pi ", "?>");
        testLongText(100000, "<?pi ", "?>");
    }

    private void testLongText(int limit, String start, String end) {
        try {
            StringBuilder buff = new StringBuilder();
            buff.append("<doc>");
            buff.append(start);
            for (int i = 0; i < limit; i++) {
                buff.append("abcdefghijklmnopqrstuvwxyz");
            }
            buff.append(end);
            buff.append("</doc>");

            ProfessionalConfiguration config = new ProfessionalConfiguration();
            NodeInfo doc = config.buildDocumentTree(new StreamSource(new StringReader(buff.toString()))).getRootNode();
            TinyTree tree = ((TinyDocumentImpl) doc).getTree();
            PTreeWriter writer = new PTreeWriter(config);
            ByteArrayOutputStream byteStream = new ByteArrayOutputStream(65536);
            DataOutputStream out = new DataOutputStream(byteStream);
            writer.writeTree(tree, out);
            out.close();

            ProfessionalConfiguration config2 = new ProfessionalConfiguration();
            byte[] byteArray = byteStream.toByteArray();
            PTreeReader reader = new PTreeReader();
            DataInputStream din = new DataInputStream(new BufferedInputStream(new ByteArrayInputStream(byteArray)));
            NodeInfo doc2 = reader.readTree(din, "http://dummy.uri/", config2).getRootNode();
            String s = QueryResult.serialize(doc2).trim();

            assertEquals(buff.length(), s.length());
            assertEquals(buff.toString(), s);

        } catch (XPathException e) {
            e.printStackTrace();
            fail();
        } catch (IOException e) {
            e.printStackTrace();
            fail();
        }

    }


}




