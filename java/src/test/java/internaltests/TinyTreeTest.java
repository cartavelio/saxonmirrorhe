package internaltests;

import jaxptest.ConfigTest;
import net.sf.saxon.Configuration;
import net.sf.saxon.TransformerFactoryImpl;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.s9api.DocumentBuilder;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.XdmNode;
import net.sf.saxon.tree.tiny.TinyDocumentImpl;
import net.sf.saxon.tree.tiny.TinyNodeImpl;
import org.junit.jupiter.api.Test;
import org.xml.sax.InputSource;

import javax.xml.transform.Source;
import javax.xml.transform.Templates;
import javax.xml.transform.Transformer;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import java.io.File;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.assertFalse;
import static org.junit.jupiter.api.Assertions.assertTrue;
import static org.junit.jupiter.api.Assertions.fail;

/**
 *  Tests of the NodeInfo interface as implemented by the tinytree
 */
public class TinyTreeTest {
    private final ConfigTest configTest = new ConfigTest();

    @Test
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
                assertTrue(doc.isAncestorOrSelf(desc), "isAncestor");
                if (desc != doc) {
                    assertFalse(desc.isAncestorOrSelf(doc), "reverse isAncestor");
                }
            }
        } catch (Exception err) {
            err.printStackTrace();
            fail(err.getMessage());
        }
    }

    @Test
    public void testCondensed() {
       try
          {
            TransformerFactoryImpl transformerFactory = new TransformerFactoryImpl();
            Configuration configuration = transformerFactory.getConfiguration();

            configuration.setTreeModel(
              net.sf.saxon.om.TreeModel.TINY_TREE_CONDENSED.getSymbolicValue());

            Source source = new StreamSource(new File(configTest.getDataDir() + "tinytreetest.xsl"));

            Templates templates = transformerFactory.newTemplates(source);
            Transformer transformer = templates.newTransformer();

            transformer.transform(
              new StreamSource(new StringReader("<e/>")),
              new StreamResult(new File(configTest.getDataDir() + "temp/out.xml")));
          }
       catch (Exception err) {
           err.printStackTrace();
           fail(err.getMessage());
       }

    }


}



