package jaxptest;

import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.event.Builder;
import net.sf.saxon.om.AllElementsSpaceStrippingRule;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.tiny.TinyDocumentImpl;

import javax.xml.transform.stream.StreamSource;

/**
 *
 */
public class MemoryTest extends TestCase {

    public void testTinyTree() {
        try {
            System.gc();
            long initFree = getUsedMemory();
            System.err.println("init: " + initFree);
            StreamSource source = new StreamSource(ConfigTest.XMARK_DIR + "xmark1.xml"); //Use 100Mb for real work
            Configuration config = new Configuration();
            config.getParseOptions().setSpaceStrippingRule(AllElementsSpaceStrippingRule.getInstance());
            config.setTreeModel(Builder.TINY_TREE_CONDENSED);
            NodeInfo doc = config.buildDocumentTree(source).getRootNode();
            System.gc();
            long finalFree = getUsedMemory();
            System.err.println("final: " + finalFree);
            System.err.println("Tinytree memory used: " + (finalFree - initFree));
            ((TinyDocumentImpl) doc).getTree().showSize();
        } catch (XPathException e) {
            fail(e.getMessage());
        }
    }

    public void testLinkedTree() {
        try {
            System.gc();
            long initFree = getUsedMemory();
            System.err.println("init: " + initFree);
            StreamSource source = new StreamSource(ConfigTest.XMARK_DIR + "xmark1.xml");  //Use 100Mb for real work
            Configuration config = new Configuration();
            config.getParseOptions().setSpaceStrippingRule(AllElementsSpaceStrippingRule.getInstance());
            config.setTreeModel(Builder.LINKED_TREE);
            NodeInfo doc = config.buildDocumentTree(source).getRootNode();
            System.gc();
            long finalFree = getUsedMemory();
            System.err.println("final: " + finalFree);
            System.err.println("Linked tree memory used: " + (finalFree - initFree));
        } catch (XPathException e) {
            fail(e.getMessage());
        }
    }

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
            Thread.currentThread().sleep(100);
            System.runFinalization();
            Thread.currentThread().sleep(100);
            System.gc();
            Thread.currentThread().sleep(100);
            System.runFinalization();
            Thread.currentThread().sleep(100);
        } catch (Exception e) {
            e.printStackTrace();
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



