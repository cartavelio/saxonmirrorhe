package streams;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.util.List;

import static net.sf.saxon.s9api.streams.Predicates.*;
import static net.sf.saxon.s9api.streams.Steps.*;

public class StreamAxisTest {
    private static final String exns = "http://example.org/ns/ex";
    private static final QName _appendix = new QName("appendix");
    private static final QName _back = new QName("back");
    private static final QName _body = new QName("body");
    private static final QName _book = new QName("book");
    private static final QName _chapter = new QName("chapter");
    private static final QName _front = new QName("front");
    private static final QName _para = new QName("para");
    private static final QName _preface = new QName("preface");
    private static final QName _title = new QName("title");
    private static final QName ex_appendix = new QName(exns, "appendix");
    private static final QName ex_chapter = new QName(exns, "chapter");
    private static final QName ex_para = new QName(exns, "para");
    private static final QName ex_title = new QName(exns, "title");

    private static XdmNode document;
    private static XdmNode book;
    private static XdmNode body;

    @BeforeAll
    public static void setup() {
        Processor processor = new Processor(false);
        DocumentBuilder builder = processor.newDocumentBuilder();
        try {
            document = builder.build(new File("src/test/resources/document.xml"));
        } catch (Exception ex) {
            throw new RuntimeException("BANG", ex);
        }

        // Let's cheat and use the things we're going to be testing
        book = document.select(child("book")).asNode();
        body = book.select(child("body")).asNode();

        Assertions.assertEquals(XdmNodeKind.ELEMENT, book.getNodeKind());
        Assertions.assertEquals(_book, book.getNodeName());
        Assertions.assertEquals(XdmNodeKind.ELEMENT, body.getNodeKind());
        Assertions.assertEquals(_body, body.getNodeName());
    }

    @Test
    public void selfAxis() {
        XdmNode self = book.select(self()).asNode();
        Assertions.assertEquals(XdmNodeKind.ELEMENT, self.getNodeKind());
        Assertions.assertEquals(_book, self.getNodeName());
    }

    @Test
    public void ancestorAxis() {
        XdmNode chapter = body.select(child("chapter")).firstItem();
        List<XdmNode> ancestors = chapter.select(ancestor()).asList();
        Assertions.assertEquals(3, ancestors.size());
        Assertions.assertEquals(_body, ancestors.get(0).getNodeName());
        Assertions.assertEquals(_book, ancestors.get(1).getNodeName());
        Assertions.assertEquals(XdmNodeKind.DOCUMENT, ancestors.get(2).getNodeKind());
    }

    @Test
    public void ancestorOrSelfAxis() {
        XdmNode chapter = body.select(child("chapter")).firstItem();
        List<XdmNode> ancestors = chapter.select(ancestorOrSelf()).asList();
        Assertions.assertEquals(4, ancestors.size());
        Assertions.assertEquals(_chapter, ancestors.get(0).getNodeName());
        Assertions.assertEquals(_body, ancestors.get(1).getNodeName());
        Assertions.assertEquals(_book, ancestors.get(2).getNodeName());
        Assertions.assertEquals(XdmNodeKind.DOCUMENT, ancestors.get(3).getNodeKind());
    }

    @Test
    public void attributeAxis() {
        XdmNode appendix = book.select(descendant("appendix")).firstItem();
        List<XdmNode> attributes = appendix.select(attribute()).asList();
        Assertions.assertEquals(2, attributes.size());
        Assertions.assertEquals("draft", appendix.select(attribute("status")).asString());

        appendix = book.select(descendant("appendix").where(attributeEq("label", "B"))).firstItem();
        attributes = appendix.select(attribute()).asList();
        Assertions.assertEquals(2, attributes.size());
        XdmNode value = appendix.select(self().where(hasAttribute(exns, "id"))).asNode();
        Assertions.assertEquals("spoon", value.getAttributeValue(new QName(exns, "id")));

        appendix = book.select(descendant("appendix").where(attributeEq(exns, "id", "spoon"))).firstItem();
        Assertions.assertEquals("B", appendix.getAttributeValue(new QName("label")));
    }

    @Test
    public void childAxis() {
        XdmNode chapter = body.select(child("chapter")).firstItem();
        List<XdmNode> children = chapter.select(child()).asList();
        Assertions.assertEquals(5, children.size());
        children = chapter.select(child("*")).asList();
        Assertions.assertEquals(2, children.size());
        Assertions.assertEquals(_title,  children.get(0).getNodeName());
        Assertions.assertEquals(_para,  children.get(1).getNodeName());
    }

    @Test
    public void descendantAxis() {
        List<XdmNode> chapters = body.select(descendant("chapter")).asList();
        Assertions.assertEquals(3, chapters.size());
        chapters = body.select(descendant(exns, "chapter")).asList();
        Assertions.assertEquals(1, chapters.size());
    }

    @Test
    public void descendantOrSelfAxis() {
        List<XdmNode> nodes = body.select(descendant("*")).asList();
        Assertions.assertEquals(10, nodes.size());
        nodes = body.select(descendantOrSelf("*")).asList();
        Assertions.assertEquals(11, nodes.size());
        Assertions.assertEquals(_body, nodes.get(0).getNodeName());
    }

    @Test
    public void followingAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode title = third.select(child("title")).asNode();
        List<XdmNode> following = title.select(following("para")).asList();
        Assertions.assertEquals(5, following.size());

        following = title.select(following(exns, "para")).asList();
        Assertions.assertEquals(1, following.size());
    }

    @Test
    public void followingSiblingAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode title = third.select(child("title")).asNode();
        List<XdmNode> following = title.select(followingSibling("para")).asList();
        Assertions.assertEquals(2, following.size());

        following = title.select(followingSibling(exns, "para")).asList();
        Assertions.assertEquals(0, following.size());
    }

    @Test
    public void namespaceAxis() {
        List<XdmNode> namespaces = body.select(namespace()).toList();

        Assertions.assertEquals(2, namespaces.size());
        Assertions.assertEquals(exns, body.select(namespace("ex")).asString());
    }

    @Test
    public void parentAxis() {
        XdmNode parent = body.select(parent()).asNode();
        Assertions.assertEquals(_book, parent.getNodeName());
    }

    @Test
    public void precedingAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode lastPara = third.select(child("para")).lastItem();
        List<XdmNode> preceding = lastPara.select(preceding("para")).asList();
        Assertions.assertEquals(4, preceding.size());

        preceding = lastPara.select(preceding(exns, "para")).asList();
        Assertions.assertEquals(1, preceding.size());
    }

    @Test
    public void precedingSiblingAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode lastPara = third.select(child("para")).lastItem();
        List<XdmNode> preceding = lastPara.select(precedingSibling("para")).asList();
        Assertions.assertEquals(1, preceding.size());

        preceding = lastPara.select(precedingSibling(exns, "para")).asList();
        Assertions.assertEquals(0, preceding.size());
    }

    @Test
    public void precedingOrSelfAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode lastPara = third.select(child("para")).lastItem();
        List<XdmNode> preceding = lastPara.select(preceding()).asList();
        Assertions.assertEquals(39, preceding.size());

        preceding = lastPara.select(precedingOrSelf()).asList();
        Assertions.assertEquals(40, preceding.size());

        preceding = lastPara.select(precedingOrSelf("para")).asList();
        Assertions.assertEquals(5, preceding.size());

        preceding = lastPara.select(precedingOrSelf(exns, "para")).asList();
        Assertions.assertEquals(1, preceding.size());
    }

    @Test
    public void precedingSiblingOrSelfAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode lastPara = third.select(child("para")).lastItem();
        List<XdmNode> preceding = lastPara.select(precedingSiblingOrSelf()).asList();
        Assertions.assertEquals(6, preceding.size());

        preceding = lastPara.select(precedingSiblingOrSelf("*")).asList();
        Assertions.assertEquals(3, preceding.size());

        preceding = lastPara.select(precedingSiblingOrSelf("para")).asList();
        Assertions.assertEquals(2, preceding.size());

        preceding = lastPara.select(precedingSiblingOrSelf(exns, "para")).asList();
        Assertions.assertEquals(0, preceding.size());
    }

    @Test
    public void followingOrSelfAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode title = third.select(child("title")).asNode();
        List<XdmNode> following = title.select(following()).asList();
        Assertions.assertEquals(39, following.size());

        following = title.select(followingOrSelf()).asList();
        Assertions.assertEquals(40, following.size());

        following = title.select(followingOrSelf("para")).asList();
        Assertions.assertEquals(5, following.size());

        following = title.select(followingOrSelf(exns, "para")).asList();
        Assertions.assertEquals(1, following.size());
    }

    @Test
    public void followingSiblingOrSelfAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode title = third.select(child("title")).asNode();
        List<XdmNode> following = title.select(followingSiblingOrSelf()).asList();
        Assertions.assertEquals(6, following.size());

        following = title.select(followingSiblingOrSelf("*")).asList();
        Assertions.assertEquals(3, following.size());

        following = title.select(followingSiblingOrSelf("para")).asList();
        Assertions.assertEquals(2, following.size());

        following = title.select(followingSiblingOrSelf(exns, "para")).asList();
        Assertions.assertEquals(0, following.size());
    }


}
