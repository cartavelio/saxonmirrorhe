package internaltests;

import com.saxonica.config.EnterpriseConfiguration;
import net.sf.saxon.Configuration;
import net.sf.saxon.expr.UnionIterator;
import net.sf.saxon.expr.sort.LocalOrderComparer;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.SequenceIterator;
import net.sf.saxon.s9api.Processor;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.tree.iter.AxisIterator;
import net.sf.saxon.tree.iter.ListIterator;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.util.ArrayList;
import java.util.List;

import static org.junit.jupiter.api.Assertions.*;


/**
 * Tests of the NodeInfo interface as implemented by the tinytree
 */
public class UnionIteratorTest {

    @Test
    public void testUnionIterator() {
        String xml = "<z><a/><b/><c/><d/><e/><f/><g/><h/><i/><j/><k/><l/><m/></z>";
        try {
            Configuration config = new EnterpriseConfiguration();
            Processor p = new Processor(config);
            NodeInfo root = config.buildDocumentTree(new StreamSource(new StringReader(xml))).getRootNode();
            List<List<NodeInfo>> listOfLists = new ArrayList<>();
            for (int i = 0; i < 10; i++) {
                listOfLists.add(new ArrayList<>());
            }
            AxisIterator top = root.iterateAxis(AxisInfo.CHILD);
            AxisIterator children = top.next().iterateAxis(AxisInfo.CHILD);
            NodeInfo node;
            while ((node = children.next()) != null) {
                switch (node.getLocalPart()) {
                    case "a":
                        listOfLists.get(2).add(node);
                        listOfLists.get(4).add(node);
                        listOfLists.get(8).add(node);
                        listOfLists.get(8).add(node);
                        break;
                    case "b":
                        listOfLists.get(0).add(node);
                        listOfLists.get(0).add(node);
                        listOfLists.get(3).add(node);
                        listOfLists.get(7).add(node);
                        break;
                    case "c":
                        listOfLists.get(1).add(node);
                        listOfLists.get(5).add(node);
                        listOfLists.get(9).add(node);
                        break;
                    case "f":
                        listOfLists.get(3).add(node);
                        listOfLists.get(6).add(node);
                        listOfLists.get(9).add(node);
                        break;
                    default:
                        break;
                }
            }
            List<SequenceIterator> iters = new ArrayList<>();
            for (List<NodeInfo> list : listOfLists) {
                iters.add(new ListIterator.Of<>(list));
            }
            UnionIterator union = new UnionIterator(iters, LocalOrderComparer.getInstance());
            assertEquals("a", union.next().getLocalPart());
            assertEquals("b", union.next().getLocalPart());
            assertEquals("c", union.next().getLocalPart());
            assertEquals("f", union.next().getLocalPart());
            assertNull(union.next());
        } catch (XPathException e) {
            fail();
        }

    }
}
