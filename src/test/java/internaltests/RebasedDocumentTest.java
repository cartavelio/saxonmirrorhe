package internaltests;

import junit.framework.TestCase;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.TreeInfo;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmNode;
import net.sf.saxon.tree.tiny.TinyDocumentImpl;
import net.sf.saxon.tree.wrapper.RebasedDocument;
import org.xml.sax.InputSource;

import javax.xml.transform.sax.SAXSource;
import java.io.StringReader;
import java.net.URI;
import java.util.function.Function;

import static net.sf.saxon.s9api.streams.Steps.descendant;

/**
 *  Tests of the virtual rebased document
 */
public class RebasedDocumentTest extends TestCase {

    public void testSimpleRebase() {
        try {
            StringReader sr = new StringReader("<doc><chap><p n='1'/><p n='2'/></chap><chap><p n='3'/><p n='4'/></chap></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = new Processor(false).newDocumentBuilder();
            builder.setBaseURI(new URI("http://saxonica.com/"));
            XdmNode d = builder.build(source);
            TinyDocumentImpl doc = (TinyDocumentImpl) d.getUnderlyingNode();
            TreeInfo tree = doc.getTree();
            RebasedDocument tree2 = new RebasedDocument(tree, s -> "http://saxon.sf.net/" , s -> "http://dev.saxonica.com/");
            XdmNode d2 = new XdmNode(tree2.getRootNode());
            assertEquals(d2.getBaseURI().toASCIIString(), "http://saxon.sf.net/");
            assertEquals(d2.getUnderlyingNode().getSystemId(), "http://dev.saxonica.com/");
            XdmNode p2 = d2.select(descendant("p")).findFirst().get();
            assertEquals(p2.getBaseURI().toASCIIString(), "http://saxon.sf.net/");
            assertEquals(p2.getUnderlyingNode().getSystemId(), "http://dev.saxonica.com/");
        } catch (Exception e) {
            e.printStackTrace();
            fail();
        }
    }

    public void testRebaseRelative() {
        try {
            StringReader sr = new StringReader("<doc><chap><p n='1'/><p n='2'/></chap><chap><p n='3'/><q xml:base='sub/' n='4'/></chap></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = new Processor(false).newDocumentBuilder();
            builder.setBaseURI(new URI("http://saxonica.com/"));
            XdmNode d = builder.build(source);
            TinyDocumentImpl doc = (TinyDocumentImpl) d.getUnderlyingNode();
            TreeInfo tree = doc.getTree();
            Function<NodeInfo, String> rebase = n -> {
                String s = n.getBaseURI();
                if (s.startsWith("http://saxonica.com/")) {
                    String suffix = s.substring("http://saxonica.com/".length());
                    return "http://saxon.sf.net/" + suffix;
                } else {
                    return s;
                }
            };
            RebasedDocument tree2 = new RebasedDocument(tree, rebase, rebase);
            XdmNode d2 = new XdmNode(tree2.getRootNode());
            assertEquals(d2.getBaseURI().toASCIIString(), "http://saxon.sf.net/");
            XdmNode p2 = d2.select(descendant("q")).findFirst().get();
            assertEquals(p2.getBaseURI().toASCIIString(), "http://saxon.sf.net/sub/");
        } catch (Exception e) {
            e.printStackTrace();
            fail();
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



