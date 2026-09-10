package internaltests;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.Controller;
import net.sf.saxon.expr.XPathContext;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.UnparsedTextIterator;
import org.junit.jupiter.api.Test;

import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.IOException;
import java.io.LineNumberReader;
import java.net.URI;
import java.net.URISyntaxException;

import static com.ibm.icu.impl.Assert.fail;
import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;

/**
 * A number of tests to ensure that certain constructs require a valid license,
 * especially after export/import
 */

public class ResourceTests {

    public static int javaMajorVersion() {
        String vn = System.getProperty("java.version");
        if (vn.startsWith("1.")) {
            vn = vn.substring(2);
        }
        int dot = vn.indexOf('.');
        if (dot >= 0) {
            vn = vn.substring(0, dot);
        }
        return Integer.parseInt(vn);
    }

    /**
     * Test that the file opened by an UnparsedTextIterator gets closed
     * on garbage collection of the iterator in the case where the iterator
     * is not read to completion and is not explicitly closed.
     */

    @Test
    public void testReleasingUnparsedTextIterator() {
        try {
            Configuration config = new Configuration();
            Controller controller = new Controller(config);
            XPathContext context = controller.newXPathContext();
            LineNumberReader reader = new LineNumberReader(new FileReader(new ConfigTest().getDataDir() + "books.xml"));
            readFilePartly(reader, context);
            try {
                Thread.sleep(1000);
                System.gc();
                Thread.sleep(1000);
            } catch (InterruptedException e) {
                // no action
            }
            // Test whether reader is closed
            if (javaMajorVersion() >= 9) {
                assertTrue(isReaderClosed(reader));
            } else {
                // It doesn't get closed under Java 8 because the Cleaner isn't available
                assertFalse(isReaderClosed(reader));
            }
        } catch (FileNotFoundException e) {
            fail(e);
        }
    }


    private void readFilePartly(LineNumberReader reader, XPathContext context) {
        try {
            URI uri = new URI(new ConfigTest().getDataDir() + "books.xml");
            UnparsedTextIterator iter = new UnparsedTextIterator(reader, uri, context, "UTF-8");
            iter.next();
            iter.next();
        } catch (URISyntaxException | XPathException e) {
            throw new RuntimeException(e);
        }
    }

    private boolean isReaderClosed(LineNumberReader reader) {
        try {
            reader.readLine();
            return false;
        } catch (IOException e) {
            return e.getMessage().contains("Stream closed");
        }
    }

//    @Test
//    public void testReleasingJarCollection() {
//        try {
//            Configuration config = new Configuration();
//            Controller controller = new Controller(config);
//            XPathContext context = controller.newXPathContext();
//
//            ZipInputStream stream = new ZipInputStream(new FileInputStream(new ConfigTest().getDataDir() + "collection3/saxon9he.jar"));
//
//            JarCollection jar = new JarCollection(context, "http://dummy/dummy.jar", new URIQueryParameters("", config));
//
//            readJarPartly(stream, context);
//            try {
//                Thread.sleep(1000);
//                System.gc();
//                Thread.sleep(1000);
//            } catch (InterruptedException e) {
//                // no action
//            }
//            // Test whether reader is closed
//            assertTrue(isJarClosed(jar));
//        } catch (FileNotFoundException | XPathException e) {
//            fail(e);
//        }
//    }
//
//    private void readJarPartly(ZipInputStream stream, XPathContext context) throws XPathException{
//        JarCollection.JarIterator iter = new JarCollection.JarIterator(context, stream, (dir, name)->true);
//        Iterator<? extends Resource> jarIter = new J;
//        jarIter.next();
//        jarIter.next();
//    }
//
//    private boolean isJarClosed(JarCollection jar) {
//
//    }



}

