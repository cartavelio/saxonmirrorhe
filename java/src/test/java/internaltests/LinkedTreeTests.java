package internaltests;

import net.sf.saxon.om.FingerprintedQName;
import net.sf.saxon.om.NamePool;
import net.sf.saxon.om.StructuredQName;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.s9api.*;
import net.sf.saxon.tree.linked.DocumentImpl;
import net.sf.saxon.tree.linked.ElementImpl;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.StringReader;
import java.util.List;
import java.util.stream.Collectors;
import java.util.stream.IntStream;

import static org.junit.jupiter.api.Assertions.fail;

public class LinkedTreeTests {

    // JUnit tests for the Linked Tree object model, in particular its updating interfaces

    private static void buildLinkedTree(boolean well) throws SaxonApiException {
        String skeleton = "<wrapper></wrapper>";
        Processor p = new Processor(true);
        DocumentBuilder b = p.newDocumentBuilder();
        b.setTreeModel(TreeModel.LINKED_TREE);
        XdmNode src1 = b.build(new StreamSource(new StringReader(skeleton)));
        if (well) {
            addChildrenWell((DocumentImpl) src1.getUnderlyingNode(), 2);
        } else {
            addChildrenBadly((DocumentImpl) src1.getUnderlyingNode(), 2);
        }
        Serializer ser = p.newSerializer();
        ser.setOutputProperty(Serializer.Property.INDENT, "yes");
        ser.setOutputStream(System.out);
        ser.serializeNode(src1);
    }

    private static void addChildrenBadly(DocumentImpl d, int width) {
        addChildrenBadly(d.getNamePool(), d.getDocumentElement(), width);

    }

    private static void addChildrenWell(DocumentImpl d, int width) {
        addChildrenWell(d.getNamePool(), d.getDocumentElement(), width);
    }

    private static final String ChildName = "Child";

    private static void addChildrenWell(NamePool context, ElementImpl e, int width) {
        List<ElementImpl> children = IntStream.range(1, width + 1).mapToObj(i -> {
            ElementImpl c = createLinkedTreeElement(context, ChildName + "-" + i);
            return c;
        }).collect(Collectors.toList());
        if (!children.isEmpty()) {
            ElementImpl head = children.remove(0);
            ElementImpl[] h = {head};
            e.insertChildren(h, true, true); //?Bug in saxon, cleanUpChildren(NodeImpl[]) must be called to set the siblings up, but adding them all in on go does not do this :-)
            e.insertChildren(children.toArray(new ElementImpl[children.size()]), false, true);
        }
    }


    private static void addChildrenBadly(NamePool context, ElementImpl e, int width) {
        List<ElementImpl> children = IntStream.range(1, width + 1).mapToObj(i -> {
            ElementImpl c = createLinkedTreeElement(context, ChildName + "-" + i);
            return c;
        }).collect(Collectors.toList());
        e.insertChildren(children.toArray(new ElementImpl[children.size()]), false, true);
    }

    private static ElementImpl createLinkedTreeElement(NamePool context, String name) {
        ElementImpl e = new ElementImpl();
        e.setNodeName(new FingerprintedQName(new StructuredQName("", "", name), context));
        return e;
    }

    @Test
    public void testWell() {
        try {
            buildLinkedTree(true);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

    @Test
    public void testBadly() {
        try {
            buildLinkedTree(false);
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail();
        }
    }

}

