package commands;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.Transform;

import java.io.*;

/**
 * JUnit tests on Transform CommandLine options
 */
public class TransformTests extends TestCase {

    public void testTransformation() {

        String args[] = {"-t", "-xsl:" + ConfigTest.DATA_DIR + "play.xsl", "-s:" + ConfigTest.DATA_DIR + "othello.xml", "dir=" + ConfigTest.DATA_DIR + "samples/output"};
        try {
            Transform.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
        assertEquals(16, new File(ConfigTest.DATA_DIR+"samples/output").listFiles().length);
    }

    public void testThreadTransformation() {

        String args[] = {"-xsl:" + ConfigTest.DATA_DIR + "threads/test.xsl", "-s:" + ConfigTest.DATA_DIR + "threads/input", "-o:" + ConfigTest.DATA_DIR + "threads/output", "-threads:2"};
        try {
            Transform.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
        assertEquals(5, new File(ConfigTest.DATA_DIR + "threads/output").listFiles().length);

    }

    public void testDefaultSerialization() {
        String[] args = {
                "-ext:off",
                "-quit:off",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "books.xsl"
        };
        try {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            System.setOut(new PrintStream(baos));   // redirect System.out
            Transform.main(args);
            System.setOut(new PrintStream(new FileOutputStream(FileDescriptor.out))); // revert change to System.out
            String strResult = baos.toString();
            assertTrue("html", strResult.startsWith("<html>"));
            assertTrue("head-meta", strResult.contains("<head><meta"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    public void testOverriddenSerialization() {
        String[] args = {
                "-ext:off",
                "-quit:off",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "books.xsl",
                "!indent=yes"
        };
        try {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            System.setOut(new PrintStream(baos));   // redirect System.out
            Transform.main(args);
            System.setOut(new PrintStream(new FileOutputStream(FileDescriptor.out))); // revert change to System.out
            String result = baos.toString();
            assertTrue("html", result.startsWith("<html>"));
            assertFalse("head-meta", result.contains("<head><meta"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }

    public void testSerializationOptions() {
        String[] args = {
                "-ext:off",
                "-quit:off",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "books.xsl",
                "!method=xml"
        };
        try {
            ByteArrayOutputStream baos = new ByteArrayOutputStream();
            System.setOut(new PrintStream(baos));   // redirect System.out
            Transform.main(args);
            System.setOut(new PrintStream(new FileOutputStream(FileDescriptor.out))); // revert change to System.out
            String result = baos.toString();
            assertTrue(result.startsWith("<?xml"));
        } catch (Exception e) {
            fail(e.getMessage());
        }
    }


    /**
     * Test that values for static stylesheet parameters can be supplied on the command line
     */

    public void testStaticParams() {

        String args[] = {
                "-xsl:" + ConfigTest.DATA_DIR + "use-static.xsl",
                "-s:" + ConfigTest.DATA_DIR + "bills.xml",
                "date=2013-01-01",
                "?now=current-time()",
                "+ext=" + ConfigTest.DATA_DIR + "bills.xml"};
        try {
            Transform.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
        // Expected output (but with @time being the current time):
//        <?xml version="1.0" encoding="UTF-8"?><Bills date="2013-01-01" time="22:02:52.954+01:00">
//        <Bill No="10030" Name="John Doe" date="2013-01-01" time="22:02:52.954+01:00">
//        </Bill>
//        <Bill No="10009" Name="Jane Jones" date="2013-01-01" time="22:02:52.954+01:00">
//        </Bill>
//        <Bill No="10100" Name="Bob Smith" date="2013-01-01" time="22:02:52.954+01:00">
//        </Bill>
//        <Bill No="10030" Name="John Doe" date="2013-01-01" time="22:02:52.954+01:00">
//        </Bill>
//        </Bills>
    }

    /**
     * Test that the xml:stylesheet PI works
     */

    public void testAssociated() {
        String args[] = {
                "-a:on",
                "--version-warning:off",
                "-s:" + ConfigTest.DATA_DIR + "bills.xml"};
        try {
            Transform.main(args);
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
        // Expected output:
//            <?xml version="1.0" encoding="UTF-8"?><Bills><Bill No="10009" Name="Jane Jones" NewBill="1">
//            </Bill><Bill No="10030" Name="John Doe" NewBill="1">
//            </Bill><Bill No="10030" Name="John Doe" NewBill="0">
//            </Bill><Bill No="10100" Name="Bob Smith" NewBill="1">
//            </Bill></Bills>
    }

    /**
     * Test suppression of extension functions
     */

    public void testDisallowExtensions() {
        String args[] = {
                "-ext:off",
                "-quit:off",
                "-s:" + ConfigTest.DATA_DIR + "samples/data/books.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "samples/styles/books-pe.xsl"
        };
        try {
            Transform.main(args);
            fail();
        } catch (Exception e) {
            // OK
        }
    }

    public void testTimingProfile() {
        String args[] = {
                "-ext:off",
                "-TP:" + ConfigTest.DATA_DIR + "temp/profile.html",
                "-o:" + ConfigTest.DATA_DIR + "temp/books.html",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "books.xsl"
        };
        try {
            Transform.main(args);
        } catch (Exception e) {
            fail();
        }
    }

    public void testJsonMethod() {
        String args[] = {
                "-ext:off",
                "-o:" + ConfigTest.DATA_DIR + "temp/books.json",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "books-to-json.xsl"
        };
        try {
            Transform.main(args);
        } catch (Exception e) {
            fail();
        }
    }

    /**
     * Command line, two packages, library package first
     */
    public void testPackages001() {
        String sep = System.getProperty("path.separator");
        String args[] = {
                "-t",
                "-o:" + ConfigTest.DATA_DIR + "temp/packs-out.xml",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-lib:" + ConfigTest.DATA_DIR + "packages/pack-002.xsl",
                "-xsl:" + ConfigTest.DATA_DIR + "packages/pack-001.xsl"
        };
        try {
            Transform.main(args);
        } catch (Exception e) {
            fail();
        }
    }

    /**
     * Command line, two packages, library package second
     */
    public void testPackages002() {
        String sep = System.getProperty("path.separator");
        String args[] = {
                "-t",
                "-o:" + ConfigTest.DATA_DIR + "temp/packs-out.xml",
                "-s:" + ConfigTest.DATA_DIR + "books.xml",
                "-lib:" + ConfigTest.DATA_DIR + "packages/pack-001.xsl",
                "-xsl:" + ConfigTest.DATA_DIR + "packages/pack-002.xsl"
        };
        try {
            Transform.main(args);
        } catch (Exception e) {
            fail();
        }
    }

    /**
     * Command line, first stylesheet uses saxon:next-in-chain
     */
    public void testNextInChain() {
        String args[] = {
                "-t",
                "-o:" + ConfigTest.DATA_DIR + "temp/chain-out.xml",
                "-s:" + ConfigTest.DATA_DIR + "next-in-chain/test.xml",
                "-xsl:" + ConfigTest.DATA_DIR + "next-in-chain/main.xsl"
        };
        try {
            Transform.main(args);
        } catch (Exception e) {
            fail();
        }
    }


}

