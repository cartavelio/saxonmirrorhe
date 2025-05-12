package performance;

import com.saxonica.config.EnterpriseConfiguration;
import com.saxonica.ee.domino.DominoTreeModel;
import net.sf.saxon.Configuration;
import net.sf.saxon.dom.DOMObjectModel;
import net.sf.saxon.event.*;
import net.sf.saxon.expr.parser.Loc;
import net.sf.saxon.lib.ParseOptions;
import net.sf.saxon.om.*;
import net.sf.saxon.option.axiom.AxiomObjectModel;
import net.sf.saxon.option.dom4j.DOM4JObjectModel;
import net.sf.saxon.option.jdom2.JDOM2ObjectModel;
import net.sf.saxon.option.xom.XOMObjectModel;
import net.sf.saxon.pattern.NodeKindTest;
import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.XPathException;
import net.sf.saxon.type.Untyped;
import org.junit.jupiter.api.Tag;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.File;

import static org.junit.jupiter.api.Assertions.fail;

public class TreeModelPerformance extends PerformanceTestCase {

    private final String INPUT = configTest.getXMarkDir() + "xmark10.xml";

    private void build(final TreeModel model, final String fileName, final int expected) {
        final Configuration config = new EnterpriseConfiguration();
        final ParseOptions options = new ParseOptions().withModel(model);
        Measurable task = new Measurable() {
            @Override
            public void run() {
                StreamSource source = new StreamSource(new File(fileName));
                PipelineConfiguration pipe = config.makePipelineConfiguration();
                try {
                    config.buildDocumentTree(source, options);
                } catch (XPathException e) {
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
                return "Build " + model.getName();
            }
        };
        measure(task);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeBuild10Mb() {
        build(TreeModel.TINY_TREE, INPUT, 100);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeCondensedBuild10Mb() {
        build(TreeModel.TINY_TREE_CONDENSED, INPUT, 130);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeLinkedBuild10Mb() {
        build(TreeModel.LINKED_TREE, INPUT, 130);
    }

    @Test
    @Tag("performance")
    public void testDomBuild10Mb() {
        build(DOMObjectModel.getInstance(), INPUT, 140);
    }

    @Test
    @Tag("performance")
    public void testDominoBuild10Mb() {
        build(DominoTreeModel.getInstance(), INPUT, 180);
    }

    @Test
    @Tag("performance")
    public void testXOMBuild10Mb() {
        build(XOMObjectModel.getInstance(), INPUT, 180);
    }

    @Test
    @Tag("performance")
    public void testJDOM2Build10Mb() {
        build(JDOM2ObjectModel.getInstance(), INPUT, 130);
    }

    @Test
    @Tag("performance")
    public void testDOM4JBuild10Mb() {
        build(DOM4JObjectModel.getInstance(), INPUT, 130);
    }

    @Test
    @Tag("performance")
    public void testAxiomBuild10Mb() {
        build(AxiomObjectModel.getInstance(), INPUT, 130);
    }

    private void sink(final TreeModel model, final String fileName, final int expected) {
        final Configuration config = new EnterpriseConfiguration();
        final ParseOptions options = new ParseOptions().withModel(model);
        StreamSource source = new StreamSource(new File(fileName));
        PipelineConfiguration pipe = config.makePipelineConfiguration();
        TreeInfo tree = null;
        try {
            tree = config.buildDocumentTree(source, options);
        } catch (XPathException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
        final TreeInfo finalTree = tree;
        Measurable task = new Measurable() {
            @Override
            public void run() {
                PipelineConfiguration pipe = config.makePipelineConfiguration();
                try {
                    finalTree.getRootNode().copy(new Sink(pipe), CopyOptions.ALL_NAMESPACES, Loc.NONE);
                } catch (XPathException e) {
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
                return "Sink " + model.getName();
            }
        };
        measure(task);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeSink10Mb() {
        sink(TreeModel.TINY_TREE, INPUT, 5);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeCondensedSink10Mb() {
        sink(TreeModel.TINY_TREE_CONDENSED, INPUT, 5);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeLinkedSink10Mb() {
        sink(TreeModel.LINKED_TREE, INPUT, 18);
    }

    @Test
    @Tag("performance")
    public void testDOMSink10Mb() {
        sink(DOMObjectModel.getInstance(), INPUT, 120);
    }

    @Test
    @Tag("performance")
    public void testDominoSink10Mb() {
        sink(DominoTreeModel.getInstance(), INPUT, 42);
    }

    @Test
    @Tag("performance")
    public void testXOMSink10Mb() {
        sink(XOMObjectModel.getInstance(), INPUT, 42);
    }

    @Test
    @Tag("performance")
    public void testJDOM2Sink10Mb() {
        sink(JDOM2ObjectModel.getInstance(), INPUT, 42);
    }

    @Test
    @Tag("performance")
    public void testDOM4JSink10Mb() {
        sink(DOM4JObjectModel.getInstance(), INPUT, 42);
    }

    @Test
    @Tag("performance")
    public void testAxiomSink10Mb() {
        sink(AxiomObjectModel.getInstance(), INPUT, 42);
    }

    private void graft(final TreeModel model, final String fileName, final int expected) {
        final Configuration config = new EnterpriseConfiguration();
        final ParseOptions options = new ParseOptions().withModel(model);
        StreamSource source = new StreamSource(new File(fileName));
        PipelineConfiguration pipe = config.makePipelineConfiguration();
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
        Measurable task = new Measurable() {
            @Override
            public void run() {
                try {
                    PipelineConfiguration pipe = config.makePipelineConfiguration();
                    Builder builder = model.makeBuilder(pipe);
                    NamespaceReducer reducer = new NamespaceReducer(builder);
                    ComplexContentOutputter cco = new ComplexContentOutputter(reducer);
                    cco.open();
                    cco.startDocument(ReceiverOption.NONE);
                    cco.startElement(new NoNamespaceName("wrapper"), Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);

                    finalDocumentElement.copy(cco, CopyOptions.ALL_NAMESPACES, Loc.NONE);

                    cco.endElement();
                    cco.endDocument();
                    cco.close();

                } catch (XPathException e) {
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
                return "Graft " + model.getName();
            }
        };
        measure(task);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeGraft10Mb() {
        graft(TreeModel.TINY_TREE, INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeCondensedGraft10Mb() {
        graft(TreeModel.TINY_TREE_CONDENSED, INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testLinkedGraft10Mb() {
        graft(TreeModel.LINKED_TREE, INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testDOMGraft10Mb() {
        graft(DOMObjectModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testDominoGraft10Mb() {
        graft(DominoTreeModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testXOMGraft10Mb() {
        graft(XOMObjectModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testJDOM2Graft10Mb() {
        graft(JDOM2ObjectModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testDOM4JGraft10Mb() {
        graft(DOM4JObjectModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testAxiomGraft10Mb() {
        graft(AxiomObjectModel.getInstance(), INPUT, 500);
    }

    private void graftThenScan(final TreeModel model, final String fileName, final int expected) {
        final Configuration config = new EnterpriseConfiguration();
        final ParseOptions options = new ParseOptions().withModel(model);
        StreamSource source = new StreamSource(new File(fileName));
        PipelineConfiguration pipe = config.makePipelineConfiguration();
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

        NodeInfo afterGraft = null;
        try {
            Builder builder = model.makeBuilder(pipe);
            NamespaceReducer reducer = new NamespaceReducer(builder);
            ComplexContentOutputter cco = new ComplexContentOutputter(reducer);
            cco.open();
            cco.startDocument(ReceiverOption.NONE);
            cco.startElement(new NoNamespaceName("wrapper"), Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);

            finalDocumentElement.copy(cco, CopyOptions.ALL_NAMESPACES, Loc.NONE);

            cco.endElement();
            cco.endDocument();
            cco.close();
            afterGraft = builder.getCurrentRoot();

        } catch (XPathException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
        final NodeInfo finalAfterGraft = afterGraft;
        Measurable task = new Measurable() {
            @Override
            public void run() {
                PipelineConfiguration pipe = config.makePipelineConfiguration();
                try {
                    finalAfterGraft.copy(new Sink(pipe), CopyOptions.ALL_NAMESPACES, Loc.NONE);
                } catch (XPathException e) {
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
                return "Sink after Graft " + model.getName();
            }
        };
        measure(task);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeGraftScan10Mb() {
        graftThenScan(TreeModel.TINY_TREE, INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeCondensedGraftScan10Mb() {
        graftThenScan(TreeModel.TINY_TREE_CONDENSED, INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testLinkedGraftScan10Mb() {
        graftThenScan(TreeModel.LINKED_TREE, INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testDOMGraftScan10Mb() {
        graftThenScan(DOMObjectModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testDominoGraftScan10Mb() {
        graftThenScan(DominoTreeModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testXOMGraftScan10Mb() {
        graftThenScan(XOMObjectModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testJDOM2GraftScan10Mb() {
        graftThenScan(JDOM2ObjectModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testDOM4JGraftScan10Mb() {
        graftThenScan(DOM4JObjectModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testAxiomGraftScan10Mb() {
        graftThenScan(AxiomObjectModel.getInstance(), INPUT, 500);
    }

    private void graftThenSearch(final TreeModel model, final String fileName, final int expected) {
        Processor proc = new Processor(true);
        final Configuration config = proc.getUnderlyingConfiguration();
        final ParseOptions options = new ParseOptions().withModel(model);
        StreamSource source = new StreamSource(new File(fileName));
        PipelineConfiguration pipe = config.makePipelineConfiguration();
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

        NodeInfo afterGraft = null;
        try {
            Builder builder = model.makeBuilder(pipe);
            NamespaceReducer reducer = new NamespaceReducer(builder);
            ComplexContentOutputter cco = new ComplexContentOutputter(reducer);
            cco.open();
            cco.startDocument(ReceiverOption.NONE);
            cco.startElement(new NoNamespaceName("wrapper"), Untyped.getInstance(), Loc.NONE, ReceiverOption.NONE);

            finalDocumentElement.copy(cco, CopyOptions.ALL_NAMESPACES, Loc.NONE);

            cco.endElement();
            cco.endDocument();
            cco.close();
            afterGraft = builder.getCurrentRoot();

        } catch (XPathException e) {
            e.printStackTrace();
            fail(e.getMessage());
        }
        final NodeInfo finalAfterGraft = afterGraft;

        XPathCompiler xpc = proc.newXPathCompiler();
        XPathExecutable exec = null;
        try {
            exec = xpc.compile("count(//*[@id])");
        } catch (SaxonApiException e) {
            fail();
        }

        final XPathExecutable finalExec = exec;

        Measurable task = new Measurable() {
            @Override
            public void run() {
                PipelineConfiguration pipe = config.makePipelineConfiguration();
                try {
                    XPathSelector xps = finalExec.load();
                    xps.setContextItem((XdmNode)XdmNode.wrap(finalAfterGraft));
                    XdmAtomicValue result = (XdmAtomicValue)xps.evaluateSingle();
                    if (result.getLongValue() != 6025) {
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
                return "Search after Graft " + model.getName();
            }
        };
        measure(task);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeGraftSearch10Mb() {
        graftThenSearch(TreeModel.TINY_TREE, INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testTinyTreeCondensedGraftSearch10Mb() {
        graftThenSearch(TreeModel.TINY_TREE_CONDENSED, INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testLinkedGraftSearch10Mb() {
        graftThenSearch(TreeModel.LINKED_TREE, INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testDOMGraftSearch10Mb() {
        graftThenSearch(DOMObjectModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testDominoGraftSearch10Mb() {
        graftThenSearch(DominoTreeModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testXOMGraftSearch10Mb() {
        graftThenSearch(XOMObjectModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testJDOM2GraftSearch10Mb() {
        graftThenSearch(JDOM2ObjectModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testDOM4JGraftSearch10Mb() {
        graftThenSearch(DOM4JObjectModel.getInstance(), INPUT, 500);
    }

    @Test
    @Tag("performance")
    public void testAxiomGraftSearch10Mb() {
        graftThenSearch(AxiomObjectModel.getInstance(), INPUT, 500);
    }

}

