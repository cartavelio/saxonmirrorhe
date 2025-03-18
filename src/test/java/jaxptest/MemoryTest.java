package jaxptest;

import net.sf.saxon.Configuration;
import net.sf.saxon.event.Builder;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.lib.StandardLogger;
import net.sf.saxon.om.AllElementsSpaceStrippingRule;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyDocumentImpl;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;

import static org.junit.jupiter.api.Assertions.fail;

/**
 *
 */
public class MemoryTest {
    private final ConfigTest configTest = new ConfigTest();

    @Test
    @Tag("performance")
    public void testTinyTree() {
        try {
            System.gc();
            long initFree = getUsedMemory();
            System.err.println("init: " + initFree);
            StreamSource source = new StreamSource(configTest.getXMarkDir() + "xmark1.xml"); //Use 100Mb for real work
            Configuration config = new Configuration();
            ParseOptions options = config.getParseOptions()
                    .withSpaceStrippingRule(AllElementsSpaceStrippingRule.getInstance());
            config.setTreeModel(Builder.TINY_TREE_CONDENSED);
            NodeInfo doc = config.buildDocumentTree(source, options).getRootNode();
            System.gc();
            long finalFree = getUsedMemory();
            System.err.println("final: " + finalFree);
            System.err.println("Tinytree memory used: " + (finalFree - initFree));
            ((TinyDocumentImpl) doc).getTree().showSize(new StandardLogger());
        } catch (XPathException e) {
            fail(e.getMessage());
        }
    }

    @Test
    @Tag("performance")
    public void testLinkedTree() {
        try {
            System.gc();
            long initFree = getUsedMemory();
            System.err.println("init: " + initFree);
            StreamSource source = new StreamSource(configTest.getXMarkDir() + "xmark1.xml");  //Use 100Mb for real work
            Configuration config = new Configuration();
            ParseOptions options = config.getParseOptions()
                    .withSpaceStrippingRule(AllElementsSpaceStrippingRule.getInstance());
            config.setTreeModel(Builder.LINKED_TREE);
            NodeInfo doc = config.buildDocumentTree(source, options).getRootNode();
            System.gc();
            long finalFree = getUsedMemory();
            System.err.println("final: " + finalFree);
            System.err.println("Linked tree memory used: " + (finalFree - initFree));
        } catch (XPathException e) {
            fail(e.getMessage());
        }
    }

    @Test
    @Tag("performance")
    public void testBothTrees() {
        testTinyTree();
        testLinkedTree();
    }

    private static long getUsedMemory() {
        gc();
        long totalMemory = Runtime.getRuntime().totalMemory();
        gc();
        long freeMemory = Runtime.getRuntime().freeMemory();
        long usedMemory = totalMemory - freeMemory;
        return usedMemory;
    }

    private static void gc() {
        try {
            System.gc();
            Thread.sleep(100);
            System.runFinalization();
            Thread.sleep(100);
            System.gc();
            Thread.sleep(100);
            System.runFinalization();
            Thread.sleep(100);
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

}

