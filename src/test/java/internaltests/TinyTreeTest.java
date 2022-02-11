package internaltests;

import jaxptest.ConfigTest;
import junit.framework.TestCase;
import net.sf.saxon.Configuration;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmNode;
import net.sf.saxon.tree.tiny.TinyDocumentImpl;
import net.sf.saxon.tree.tiny.TinyNodeImpl;
import org.xml.sax.InputSource;

import javax.xml.transform.Source;
import javax.xml.transform.Templates;
import javax.xml.transform.Transformer;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;

/**
 *  Tests of the NodeInfo interface as implemented by the tinytree
 */
public class TinyTreeTest extends TestCase {

    public void testIsAncestor() {
        try {
            StringReader sr = new StringReader("<doc><chap><p n='1'/><p n='2'/></chap><chap><p n='3'/><p n='4'/></chap></doc>");
            SAXSource source = new SAXSource(new InputSource(sr));
            DocumentBuilder builder = new Processor(false).newDocumentBuilder();
            XdmNode d = builder.build(source);
            TinyDocumentImpl doc = (TinyDocumentImpl)d.getUnderlyingNode();
            SequenceIterator iter = doc.iterateAxis(AxisInfo.DESCENDANT_OR_SELF);
            while (true) {
                TinyNodeImpl desc = (TinyNodeImpl)iter.next();
                if (desc == null) break;
                assertTrue("isAncestor", doc.isAncestorOrSelf(desc));
                if (desc != doc) {
                    assertFalse("reverse isAncestor", desc.isAncestorOrSelf(doc));
                }
            }
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    public void testCondensed() {
       try
          {
            TransformerFactoryImpl transformerFactory = new TransformerFactoryImpl();
            Configuration configuration = transformerFactory.getConfiguration();

            configuration.setTreeModel(
              net.sf.saxon.om.TreeModel.TINY_TREE_CONDENSED.getSymbolicValue());

            Source source = new StreamSource(new File(ConfigTest.DATA_DIR + "tinytreetest.xsl"));

            Templates templates = transformerFactory.newTemplates(source);
            Transformer transformer = templates.newTransformer();

            transformer.transform(
              new StreamSource(new StringReader("<e/>")),
              new StreamResult(new File(ConfigTest.DATA_DIR + "temp/out.xml")));
          }
       catch (Exception err) {
           err.printStackTrace();
           fail(err.getMessage());
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



