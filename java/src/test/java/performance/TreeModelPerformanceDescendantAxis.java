package performance;

import com.saxonica.ee.domino.DominoTreeModel;
import net.sf.saxon.Configuration;
import net.sf.saxon.dom.DOMObjectModel;
import net.sf.saxon.event.PipelineConfiguration;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.om.AxisInfo;
import net.sf.saxon.om.NodeInfo;
import net.sf.saxon.om.TreeInfo;
import net.sf.saxon.om.TreeModel;
import net.sf.saxon.option.axiom.AxiomObjectModel;
import net.sf.saxon.option.dom4j.DOM4JObjectModel;
import net.sf.saxon.option.jdom2.JDOM2ObjectModel;
import net.sf.saxon.option.xom.XOMObjectModel;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;

import static org.junit.jupiter.api.Assertions.fail;

public class TreeModelPerformanceDescendantAxis extends PerformanceTestCase {

    private final String INPUT = configTest.getXMarkDir() + "xmark10.xml";

    private void searchXPathDescendants(final TreeModel model, final String fileName, final int expected) {
        Processor proc = new Processor(true);
        final Configuration config = proc.getUnderlyingConfiguration();
        final ParseOptions options = new ParseOptions().withModel(model);
        StreamSource source = new StreamSource(new File(fileName));
        TreeInfo tree = null;
        NodeInfo documentElement = null;
        try {
            tree = config.buildDocumentTree(source, options);
            documentElement = tree.getRootNode().iterateAxis(AxisInfo.CHILD, NodeKindTest.ELEMENT).next();
        } catch (XPathException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
        final TreeInfo finalTree = tree;
        final NodeInfo finalDocumentElement = documentElement;

        XPathExecutable exec = null;
        try {
            XPathCompiler compiler = proc.newXPathCompiler();
            exec = compiler.compile("count(descendant::date)");
        } catch (SaxonApiException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }

        final XPathExecutable finalExec = exec;

        Measurable task = new Measurable() {
            @Override
            public void run() {
                PipelineConfiguration pipe = config.makePipelineConfiguration();
                try {
                    XPathSelector xps = finalExec.load();
                    xps.setContextItem((XdmNode) XdmNode.wrap(finalDocumentElement));
                    XdmAtomicValue result = (XdmAtomicValue) xps.evaluateSingle();
                    if (result.getLongValue() != 9296) {
                        fail("Got " + result.getLongValue());
                    }

                } catch (SaxonApiException e) {
                    e.printStackTrace();
                    fail(e.getMessage());
                }
            }

            @Override
            public int expectedTimeMicroseconds() {
                return expected*1000;
            }

            @Override
            public String name() {
                return "Search named descendants " + model.getName();
            }
        };
        measure(task);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeDescendants10Mb() {
        searchXPathDescendants(TreeModel.TINY_TREE, INPUT, 1);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeCondensedDescendants10Mb() {
        searchXPathDescendants(TreeModel.TINY_TREE_CONDENSED, INPUT, 2);
    }

    @Test
    @Tag("performance")
    public void testLinkedDescendants10Mb() {
        searchXPathDescendants(TreeModel.LINKED_TREE, INPUT, 25);
    }

    @Test
    @Tag("performance")
    public void testDOMDescendants10Mb() {
        searchXPathDescendants(DOMObjectModel.getInstance(), INPUT, 15);
    }

    @Test
    @Tag("performance")
    public void testDominoDescendants10Mb() {
        searchXPathDescendants(DominoTreeModel.getInstance(), INPUT, 2);
    }

    @Test
    @Tag("performance")
    public void testXOMDescendants10Mb() {
        searchXPathDescendants(XOMObjectModel.getInstance(), INPUT, 50);
    }

    @Test
    @Tag("performance")
    public void testJDOM2Descendants10Mb() {
        searchXPathDescendants(JDOM2ObjectModel.getInstance(), INPUT, 25);
    }

    @Test
    @Tag("performance")
    public void testDOM4JDescendants10Mb() {
        searchXPathDescendants(DOM4JObjectModel.getInstance(), INPUT, 150);
    }

    @Test
    @Tag("performance")
    public void testAxiomDescendants10Mb() {
        searchXPathDescendants(AxiomObjectModel.getInstance(), INPUT, 110);
    }
}

