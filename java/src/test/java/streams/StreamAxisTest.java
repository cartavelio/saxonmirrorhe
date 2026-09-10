package streams;

import net.sf.saxon.s9api.*;
import net.sf.saxon.om.NamespaceUri;
import net.sf.saxon.s9api.streams.Predicates;import org.junit.jupiter.api.Assertions;
import org.junit.jupiter.api.BeforeAll;
import org.junit.jupiter.api.Disabled;
import org.junit.jupiter.api.Test;

import java.io.File;
import java.math.BigInteger;import java.util.ArrayList;import java.util.List;

import static net.sf.saxon.s9api.streams.Predicates.*;
import static net.sf.saxon.s9api.streams.Steps.*;

public class StreamAxisTest {
    private static final NamespaceUri ns = NamespaceUri.of("http://example.org/ns");
    private static final NamespaceUri exns = NamespaceUri.of("http://example.org/ns/ex");
    private static final QName ns_author = new QName(ns, "author");
    private static final QName ns_appendix = new QName(ns, "appendix");
    private static final QName ns_back = new QName(ns, "back");
    private static final QName ns_body = new QName(ns, "body");
    private static final QName ns_book = new QName(ns, "book");
    private static final QName ns_chapter = new QName(ns, "chapter");
    private static final QName ns_section = new QName(ns, "section");
    private static final QName ns_front = new QName(ns, "front");
    private static final QName ns_para = new QName(ns, "para");
    private static final QName ns_preface = new QName(ns, "preface");
    private static final QName ns_title = new QName(ns, "title");
    private static final QName ex_appendix = new QName(exns, "appendix");
    private static final QName ex_chapter = new QName(exns, "chapter");
    private static final QName ex_author = new QName(exns, "author");
    private static final QName ex_para = new QName(exns, "para");
    private static final QName ex_title = new QName(exns, "title");
    private static final QName xml_id = new QName(NamespaceUri.XML, "id");

    private static Processor processor;
    private static XdmNode document;
    private static XdmNode book;
    private static XdmNode body;
    private static XdmNode alternate;
    private static XdmNode doc;

    // FIXME: atomize()
    // FIXME: castAs()

    @BeforeAll
    public static void setup() {
        processor = new Processor(false);
        DocumentBuilder builder = processor.newDocumentBuilder();
        try {
            document = builder.build(new File("src/test/resources/document.xml"));
            alternate = builder.build(new File("src/test/resources/altdocument.xml"));
        } catch (Exception ex) {
            throw new RuntimeException("BANG", ex);
        }

        // Let's cheat and use the things we're going to be testing
        book = document.select(child(ns_book)).asNode();
        body = book.select(child(ns_body)).asNode();

        Assertions.assertEquals(XdmNodeKind.ELEMENT, book.getNodeKind());
        Assertions.assertEquals(ns_book, book.getNodeName());
        Assertions.assertEquals(XdmNodeKind.ELEMENT, body.getNodeKind());
        Assertions.assertEquals(ns_body, body.getNodeName());

        doc = alternate.select(child("doc")).asNode();
    }

    @Test
    public void nothingTest() {
        Assertions.assertFalse(book.select(nothing()).exists());
    }

    @Test
    public void rootAxis() {
        XdmNode root = book.select(root()).asNode();
        Assertions.assertEquals(XdmNodeKind.DOCUMENT, root.getNodeKind());
    }

    @Test
    public void selfAxis() {
        XdmNode self = book.select(self()).asNode();
        Assertions.assertEquals(XdmNodeKind.ELEMENT, self.getNodeKind());
        Assertions.assertEquals(ns_book, self.getNodeName());

        self = book.select(self("book")).asNode();
        Assertions.assertEquals(XdmNodeKind.ELEMENT, self.getNodeKind());
        Assertions.assertEquals(ns_book, self.getNodeName());

        self = book.select(self("*")).asNode();
        Assertions.assertEquals(XdmNodeKind.ELEMENT, self.getNodeKind());
        Assertions.assertEquals(ns_book, self.getNodeName());

        self = book.select(self(ns.toString(), "book")).asNode();
        Assertions.assertEquals(XdmNodeKind.ELEMENT, self.getNodeKind());
        Assertions.assertEquals(ns_book, self.getNodeName());

        self = book.select(self(ns_book)).asNode();
        Assertions.assertEquals(XdmNodeKind.ELEMENT, self.getNodeKind());
        Assertions.assertEquals(ns_book, self.getNodeName());

        Assertions.assertFalse(book.select(self("not-a-book")).exists());
    }

    private void checkAncestors(List<XdmNode> ancestors) {
        Assertions.assertEquals(ns_section, ancestors.get(0).getNodeName());
        Assertions.assertEquals(ns_section, ancestors.get(1).getNodeName());
        Assertions.assertEquals(ns_chapter, ancestors.get(2).getNodeName());
        Assertions.assertEquals(ns_body, ancestors.get(3).getNodeName());
        Assertions.assertEquals(ns_book, ancestors.get(4).getNodeName());
        if (ancestors.size() == 5) {
            return;
        }
        Assertions.assertEquals(XdmNodeKind.DOCUMENT, ancestors.get(5).getNodeKind());
    }

    @Test
    public void ancestorAxis() {
        XdmNode section = body.select(descendant(ns_section)).lastItem();
        List<XdmNode> ancestors = section.select(ancestor()).asList();
        Assertions.assertEquals(6, ancestors.size());
        checkAncestors(ancestors);
        ancestors = section.select(ancestor("*")).asList();
        Assertions.assertEquals(5, ancestors.size());
        checkAncestors(ancestors);
        ancestors = section.select(ancestor("section")).asList();
        Assertions.assertEquals(2, ancestors.size());
        ancestors = section.select(ancestor("chapter")).asList();
        Assertions.assertEquals(1, ancestors.size());
        ancestors = section.select(ancestor(ns.toString(), "section")).asList();
        Assertions.assertEquals(2, ancestors.size());
        ancestors = section.select(ancestor(ns_section)).asList();
        Assertions.assertEquals(2, ancestors.size());
    }

    @Test
    public void ancestorAxisWithPredicate() {
        XdmNode section = body.select(descendant(ns_section)).lastItem();
        List<XdmNode> ancestors = section.select(ancestor(hasAttribute("id"))).asList();
        Assertions.assertEquals(1, ancestors.size());
        Assertions.assertEquals(ns_chapter, ancestors.get(0).getNodeName());
    }

    @Test
    public void ancestorOrSelfAxis() {
        XdmNode section = body.select(descendant(ns_section)).lastItem();
        List<XdmNode> ancestors = section.select(ancestorOrSelf()).asList();
        Assertions.assertEquals(7, ancestors.size());
        Assertions.assertEquals(ns_section, ancestors.get(0).getNodeName());
        checkAncestors(ancestors.subList(1, ancestors.size()));

        ancestors = section.select(ancestorOrSelf("*")).asList();
        Assertions.assertEquals(6, ancestors.size());
        Assertions.assertEquals(ns_section, ancestors.get(0).getNodeName());
        checkAncestors(ancestors.subList(1, ancestors.size()));

        ancestors = section.select(ancestorOrSelf("section")).asList();
        Assertions.assertEquals(3, ancestors.size());
        ancestors = section.select(ancestorOrSelf("chapter")).asList();
        Assertions.assertEquals(1, ancestors.size());
        ancestors = section.select(ancestorOrSelf(ns.toString(), "section")).asList();
        Assertions.assertEquals(3, ancestors.size());
        ancestors = section.select(ancestorOrSelf(ns_section)).asList();
        Assertions.assertEquals(3, ancestors.size());
    }

    @Test
    public void ancestorOrSelfAxisWithPredicate() {
        XdmNode section = body.select(descendant(ns_section)).lastItem();
        List<XdmNode> ancestors = section.select(ancestorOrSelf(hasAttribute("id"))).asList();
        Assertions.assertEquals(2, ancestors.size());
        Assertions.assertEquals(ns_section, ancestors.get(0).getNodeName());
        Assertions.assertEquals(ns_chapter, ancestors.get(1).getNodeName());
    }

    @Test
    public void attributeAxis() {
        XdmNode appendix = book.select(descendant("appendix")).firstItem();
        List<XdmNode> attributes = appendix.select(attribute()).asList();
        Assertions.assertEquals(2, attributes.size());
        Assertions.assertEquals("draft", appendix.select(attribute("status")).asString());

        appendix = book.select(descendant("appendix").where(attributeEq("label", "B"))).firstItem();
        String value = appendix.select(attribute("label")).asString();
        Assertions.assertEquals("B", value);
        value = appendix.select(attribute("", "label")).asString();
        Assertions.assertEquals("B", value);
        value = appendix.select(attribute(new QName("label"))).asString();
        Assertions.assertEquals("B", value);

        value = appendix.select(attribute("id")).asString();
        Assertions.assertEquals("spoon", value);
        value = appendix.select(attribute(exns.toString(), "id")).asString();
        Assertions.assertEquals("spoon", value);
        value = appendix.select(attribute(new QName(exns, "id"))).asString();
        Assertions.assertEquals("spoon", value);
    }

    @Test
    public void attributeAxisWithPredicate() {
        XdmNode appendix = book.select(descendant("appendix")).firstItem();
        XdmNode value = appendix.select(attribute(hasLocalName("status"))).firstItem();
        Assertions.assertEquals("draft", value.getStringValue());
    }

    @Test
    public void childAxis() {
        XdmNode chapter = body.select(child("chapter")).firstItem();
        List<XdmNode> children = chapter.select(child()).asList();
        Assertions.assertEquals(7, children.size());
        children = chapter.select(child("*")).asList();
        Assertions.assertEquals(3, children.size());
        Assertions.assertEquals(ns_author, children.get(0).getNodeName());
        Assertions.assertEquals(ns_title, children.get(1).getNodeName());
        Assertions.assertEquals(ns_para, children.get(2).getNodeName());

        children = chapter.select(child("title")).asList();
        Assertions.assertEquals(1, children.size());
        Assertions.assertEquals(ns_title, children.get(0).getNodeName());

        children = chapter.select(child(ns.toString(), "title")).asList();
        Assertions.assertEquals(1, children.size());
        Assertions.assertEquals(ns_title, children.get(0).getNodeName());

        children = chapter.select(child(ns_title)).asList();
        Assertions.assertEquals(1, children.size());
        Assertions.assertEquals(ns_title, children.get(0).getNodeName());
    }

    @Test
    public void childAxisWithPredicate() {
        XdmNode chapter = body.select(child("chapter")).firstItem();
        List<XdmNode> children = chapter.select(child(isText())).asList();
        Assertions.assertEquals(4, children.size());
    }

    private void checkDescendants(List<XdmNode> descendants) {
        Assertions.assertEquals(ns_author, descendants.get(0).getNodeName());
        Assertions.assertEquals(ns_title, descendants.get(1).getNodeName());
        Assertions.assertEquals(ns_section, descendants.get(2).getNodeName());
        Assertions.assertEquals(ns_title, descendants.get(3).getNodeName());
        Assertions.assertEquals(ns_section, descendants.get(4).getNodeName());
        Assertions.assertEquals(ns_title, descendants.get(5).getNodeName());
        Assertions.assertEquals(ns_section, descendants.get(6).getNodeName());
        Assertions.assertEquals(ns_title, descendants.get(7).getNodeName());
        Assertions.assertEquals(ns_para, descendants.get(8).getNodeName());
    }

    @Test
    public void descendantAxis() {
        XdmNode chapter = body.select(descendant("chapter").where(hasAttribute("", "id"))).firstItem();
        List<XdmNode> descendants = chapter.select(descendant()).asList();
        Assertions.assertEquals(28, descendants.size());

        descendants = chapter.select(descendant("*")).asList();
        Assertions.assertEquals(9, descendants.size());
        checkDescendants(descendants);

        descendants = chapter.select(descendant("title")).asList();
        Assertions.assertEquals(4, descendants.size());

        descendants = chapter.select(descendant(ns.toString(), "title")).asList();
        Assertions.assertEquals(4, descendants.size());

        descendants = chapter.select(descendant(ns_title)).asList();
        Assertions.assertEquals(4, descendants.size());

        descendants = chapter.select(descendant(ns_chapter)).asList();
        Assertions.assertEquals(0, descendants.size());
    }

    @Test
    public void descendantAxisWithPredicate() {
        XdmNode chapter = body.select(descendant("chapter").where(hasAttribute("", "id"))).firstItem();
        List<XdmNode> descendants = chapter.select(descendant(hasAttribute("", "id"))).asList();
        Assertions.assertEquals(1, descendants.size());
        Assertions.assertEquals(ns_section, descendants.get(0).getNodeName());
    }

    @Test
    public void descendantOrSelfAxis() {
        XdmNode chapter = body.select(descendant("chapter").where(hasAttribute("", "id"))).firstItem();
        List<XdmNode> descendants = chapter.select(descendantOrSelf()).asList();
        Assertions.assertEquals(29, descendants.size());

        descendants = chapter.select(descendantOrSelf("*")).asList();
        Assertions.assertEquals(10, descendants.size());
        Assertions.assertEquals(ns_chapter, descendants.get(0).getNodeName());
        checkDescendants(descendants.subList(1, descendants.size()));

        descendants = chapter.select(descendantOrSelf("title")).asList();
        Assertions.assertEquals(4, descendants.size());

        descendants = chapter.select(descendantOrSelf(ns.toString(), "title")).asList();
        Assertions.assertEquals(4, descendants.size());

        descendants = chapter.select(descendantOrSelf(ns_title)).asList();
        Assertions.assertEquals(4, descendants.size());

        descendants = chapter.select(descendantOrSelf(ns_chapter)).asList();
        Assertions.assertEquals(1, descendants.size());
    }

    @Test
    public void descendantOrSelfAxisWithPredicate() {
        XdmNode chapter = body.select(descendant("chapter").where(hasAttribute("", "id"))).firstItem();
        List<XdmNode> descendants = chapter.select(descendantOrSelf(hasAttribute("", "id"))).asList();
        Assertions.assertEquals(2, descendants.size());
        Assertions.assertEquals(ns_chapter, descendants.get(0).getNodeName());
        Assertions.assertEquals(ns_section, descendants.get(1).getNodeName());
    }

    @Test
    public void followingAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(ns_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode title = third.select(child("title")).asNode();

        List<XdmNode> following = title.select(following()).asList();
        Assertions.assertEquals(69, following.size());

        following = title.select(following("*")).asList();
        Assertions.assertEquals(22, following.size());

        following = title.select(following("para")).asList();
        Assertions.assertEquals(6, following.size());

        following = title.select(following(ns.toString(), "para")).asList();
        Assertions.assertEquals(5, following.size());

        following = title.select(following(ns_para)).asList();
        Assertions.assertEquals(5, following.size());

        following = title.select(following(exns.toString(), "para")).asList();
        Assertions.assertEquals(1, following.size());

        following = title.select(following(ns_book)).asList();
        Assertions.assertEquals(0, following.size());

        following = title.select(following(ns_title)).asList();
        Assertions.assertEquals(6, following.size());
    }

    @Test
    public void followingAxisWithPredicate() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(ns_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode title = third.select(child("title")).asNode();

        List<XdmNode> following = title.select(following(hasAttribute(xml_id))).asList();
        Assertions.assertEquals(1, following.size());
        Assertions.assertEquals(ns_chapter, following.get(0).getNodeName());
    }

    @Test
    public void followingOrSelfAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(ns_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode title = third.select(child("title")).asNode();

        List<XdmNode> following = title.select(followingOrSelf()).asList();
        Assertions.assertEquals(70, following.size());

        following = title.select(followingOrSelf("*")).asList();
        Assertions.assertEquals(23, following.size());

        following = title.select(followingOrSelf("para")).asList();
        Assertions.assertEquals(6, following.size());

        following = title.select(followingOrSelf(ns.toString(), "para")).asList();
        Assertions.assertEquals(5, following.size());

        following = title.select(followingOrSelf(ns_para)).asList();
        Assertions.assertEquals(5, following.size());

        following = title.select(followingOrSelf(exns.toString(), "para")).asList();
        Assertions.assertEquals(1, following.size());

        following = title.select(followingOrSelf(ns_book)).asList();
        Assertions.assertEquals(0, following.size());

        following = title.select(followingOrSelf(ns_title)).asList();
        Assertions.assertEquals(7, following.size());
    }

    @Test
    public void followingOrSelfAxisWithPredicate() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(ns_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode title = third.select(child("title")).asNode();

        List<XdmNode> following = title.select(followingOrSelf(hasAttribute(xml_id))).asList();
        Assertions.assertEquals(1, following.size());
        Assertions.assertEquals(ns_chapter, following.get(0).getNodeName());

        following = title.select(followingOrSelf(hasName(ns_title))).asList();
        Assertions.assertEquals(7, following.size());
        Assertions.assertEquals(ns_title, following.get(0).getNodeName());
    }

    @Test
    public void followingSiblingAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq(xml_id, "fourth"))).asNode();
        Assertions.assertEquals(ns_chapter, third.getNodeName());
        Assertions.assertEquals("fourth", third.getAttributeValue(xml_id));

        XdmNode author = third.select(child("author")).asNode();

        List<XdmNode> following = author.select(followingSibling()).asList();
        Assertions.assertEquals(7, following.size());

        following = author.select(followingSibling("*")).asList();
        Assertions.assertEquals(3, following.size());

        following = author.select(followingSibling("para")).asList();
        Assertions.assertEquals(2, following.size());

        following = author.select(followingSibling(ns.toString(), "para")).asList();
        Assertions.assertEquals(2, following.size());

        following = author.select(followingSibling(ns_para)).asList();
        Assertions.assertEquals(2, following.size());

        following = author.select(followingSibling(ns_title)).asList();
        Assertions.assertEquals(1, following.size());

        following = author.select(followingSibling(ns_chapter)).asList();
        Assertions.assertEquals(0, following.size());
    }

    @Test
    public void followingSiblingAxisWithPredicate() {
        XdmNode third = body.select(child("chapter").where(attributeEq(xml_id, "fourth"))).asNode();
        XdmNode author = third.select(child("author")).asNode();
        List<XdmNode> following = author.select(followingSibling(hasName(ns_title))).asList();
        Assertions.assertEquals(1, following.size());
    }

    @Test
    public void followingSiblingOrSelfAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq(xml_id, "fourth"))).asNode();
        Assertions.assertEquals(ns_chapter, third.getNodeName());
        Assertions.assertEquals("fourth", third.getAttributeValue(xml_id));

        XdmNode author = third.select(child("author")).asNode();

        List<XdmNode> following = author.select(followingSiblingOrSelf()).asList();
        Assertions.assertEquals(8, following.size());

        following = author.select(followingSiblingOrSelf("*")).asList();
        Assertions.assertEquals(4, following.size());

        following = author.select(followingSiblingOrSelf("para")).asList();
        Assertions.assertEquals(2, following.size());

        following = author.select(followingSiblingOrSelf(ns.toString(), "para")).asList();
        Assertions.assertEquals(2, following.size());

        following = author.select(followingSiblingOrSelf(ns_para)).asList();
        Assertions.assertEquals(2, following.size());

        following = author.select(followingSiblingOrSelf(ns_author)).asList();
        Assertions.assertEquals(1, following.size());

        following = author.select(followingSibling(ns_chapter)).asList();
        Assertions.assertEquals(0, following.size());
    }

    @Test
    public void followingSiblingOrSelfAxisWithPredicate() {
        XdmNode third = body.select(child("chapter").where(attributeEq(xml_id, "fourth"))).asNode();
        XdmNode author = third.select(child("author")).asNode();
        List<XdmNode> following = author.select(followingSiblingOrSelf(hasName(ns_title))).asList();
        Assertions.assertEquals(1, following.size());
        following = author.select(followingSiblingOrSelf(hasName(ns_author))).asList();
        Assertions.assertEquals(1, following.size());
    }

    @Test
    public void namespaceAxis() {
        List<XdmNode> namespaces = body.select(namespace()).toList();
        Assertions.assertEquals(3, namespaces.size());
        Assertions.assertEquals(exns.toString(), body.select(namespace("ex")).asString());

        namespaces = body.select(namespace("*")).toList();
        Assertions.assertEquals(3, namespaces.size());

        namespaces = body.select(namespace("ex")).toList();
        Assertions.assertEquals(1, namespaces.size());

        namespaces = body.select(namespace("xml")).toList();
        Assertions.assertEquals(1, namespaces.size());

        namespaces = body.select(namespace("notns")).toList();
        Assertions.assertEquals(0, namespaces.size());
    }

    @Test
    public void namespaceAxisWithPredicate() {
        XdmNode prefix = body.select(namespace(eq(exns.toString()))).asNode();
        Assertions.assertEquals("ex", prefix.getNodeName().getLocalName());
    }

    @Test
    public void parentAxis() {
        XdmNode parent = body.select(parent()).asNode();
        Assertions.assertEquals(ns_book, parent.getNodeName());

        parent = body.select(parent("*")).asNode();
        Assertions.assertEquals(ns_book, parent.getNodeName());

        parent = body.select(parent("book")).asNode();
        Assertions.assertEquals(ns_book, parent.getNodeName());

        parent = body.select(parent(ns.toString(), "book")).asNode();
        Assertions.assertEquals(ns_book, parent.getNodeName());

        parent = body.select(parent(ns_book)).asNode();
        Assertions.assertEquals(ns_book, parent.getNodeName());

        Assertions.assertFalse(body.select(parent("chapter")).exists());
        Assertions.assertFalse(body.select(parent(ns.toString(), "chapter")).exists());
        Assertions.assertFalse(body.select(parent(ns_chapter)).exists());
    }

    @Test
    public void parentAxisWithPredicate() {
        XdmNode parent = body.select(parent(hasLocalName("book"))).asNode();
        Assertions.assertEquals(ns_book, parent.getNodeName());

        Assertions.assertFalse(body.select(parent(hasAttribute("nosuchattr"))).exists());
    }

    private void checkPreceding(List<XdmNode> preceding) {
        Assertions.assertEquals(ns_title, preceding.get(0).getNodeName());
        Assertions.assertEquals(ns_title, preceding.get(1).getNodeName());
        Assertions.assertEquals(ns_title, preceding.get(2).getNodeName());
        Assertions.assertEquals(ns_title, preceding.get(3).getNodeName());
        Assertions.assertEquals(ns_author, preceding.get(4).getNodeName());
        Assertions.assertEquals(ex_para, preceding.get(5).getNodeName());
        Assertions.assertEquals(ex_title, preceding.get(6).getNodeName());
        Assertions.assertEquals(ex_chapter, preceding.get(7).getNodeName());
        Assertions.assertEquals(ns_para, preceding.get(8).getNodeName());
        Assertions.assertEquals(ns_title, preceding.get(9).getNodeName());
        Assertions.assertEquals(ns_author, preceding.get(10).getNodeName());
        Assertions.assertEquals(ns_chapter, preceding.get(11).getNodeName());
        Assertions.assertEquals(ns_para, preceding.get(12).getNodeName());
        Assertions.assertEquals(ns_title, preceding.get(13).getNodeName());
        Assertions.assertEquals(ns_preface, preceding.get(14).getNodeName());
        Assertions.assertEquals(ns_front, preceding.get(15).getNodeName());
    }

    @Test
    public void precedingAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(ns_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode lastPara = third.select(descendant("para")).lastItem();

        List<XdmNode> preceding = lastPara.select(preceding()).asList();
        Assertions.assertEquals(54, preceding.size());

        preceding = lastPara.select(preceding("*")).asList();
        Assertions.assertEquals(16, preceding.size());
        checkPreceding(preceding);

        preceding = lastPara.select(preceding("title")).asList();
        Assertions.assertEquals(7, preceding.size());

        preceding = lastPara.select(preceding(ns.toString(), "title")).asList();
        Assertions.assertEquals(6, preceding.size());

        preceding = lastPara.select(preceding(ns_title)).asList();
        Assertions.assertEquals(6, preceding.size());

        preceding = lastPara.select(preceding(ex_title)).asList();
        Assertions.assertEquals(1, preceding.size());

        preceding = lastPara.select(preceding(ns_back)).asList();
        Assertions.assertEquals(0, preceding.size());
    }

    @Test
    public void precedingAxisWithPredicate() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        XdmNode lastPara = third.select(descendant("para")).lastItem();
        List<XdmNode> preceding = lastPara.select(preceding(hasAttribute("status"))).asList();
        Assertions.assertEquals(1, preceding.size());
    }

    @Test
    public void precedingOrSelfAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        Assertions.assertEquals(ns_chapter, third.getNodeName());
        Assertions.assertEquals("third", third.attribute("id"));

        XdmNode lastPara = third.select(descendant("para")).lastItem();

        List<XdmNode> preceding = lastPara.select(precedingOrSelf()).asList();
        Assertions.assertEquals(55, preceding.size());

        preceding = lastPara.select(precedingOrSelf("*")).asList();
        Assertions.assertEquals(17, preceding.size());
        checkPreceding(preceding.subList(1, preceding.size()));

        preceding = lastPara.select(precedingOrSelf("title")).asList();
        Assertions.assertEquals(7, preceding.size());

        preceding = lastPara.select(precedingOrSelf(ns.toString(), "title")).asList();
        Assertions.assertEquals(6, preceding.size());

        preceding = lastPara.select(precedingOrSelf(ns_title)).asList();
        Assertions.assertEquals(6, preceding.size());

        preceding = lastPara.select(precedingOrSelf(ex_title)).asList();
        Assertions.assertEquals(1, preceding.size());

        preceding = lastPara.select(precedingOrSelf(ns_para)).asList();
        Assertions.assertEquals(3, preceding.size());

        preceding = lastPara.select(precedingOrSelf(ns_back)).asList();
        Assertions.assertEquals(0, preceding.size());
    }

    @Test
    public void precedingOrSelfAxisWithPredicate() {
        XdmNode third = body.select(child("chapter").where(attributeEq("id", "third"))).asNode();
        XdmNode lastPara = third.select(descendant("para")).lastItem();

        List<XdmNode> preceding = lastPara.select(precedingOrSelf(hasName(ns_title))).asList();
        Assertions.assertEquals(6, preceding.size());
    }

    @Test
    public void precedingSiblingAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq(xml_id, "fourth"))).asNode();
        XdmNode lastPara = third.select(descendant("para")).lastItem();

        List<XdmNode> preceding = lastPara.select(precedingSibling()).asList();
        Assertions.assertEquals(7, preceding.size());

        preceding = lastPara.select(precedingSibling("*")).asList();
        Assertions.assertEquals(3, preceding.size());

        preceding = lastPara.select(precedingSibling("para")).asList();
        Assertions.assertEquals(1, preceding.size());

        preceding = lastPara.select(precedingSibling(ns.toString(), "para")).asList();
        Assertions.assertEquals(1, preceding.size());

        preceding = lastPara.select(precedingSibling(ns_para)).asList();
        Assertions.assertEquals(1, preceding.size());

        preceding = lastPara.select(precedingSibling(ns_chapter)).asList();
        Assertions.assertEquals(0, preceding.size());
    }

    @Test
    public void precedingSiblingAxisWithPredicate() {
        XdmNode third = body.select(child("chapter").where(attributeEq(xml_id, "fourth"))).asNode();
        XdmNode lastPara = third.select(descendant("para")).lastItem();

        List<XdmNode> preceding = lastPara.select(precedingSibling(hasName(ns_para))).asList();
        Assertions.assertEquals(1, preceding.size());

        preceding = lastPara.select(precedingSibling(attributeEq(xml_id, "spoon"))).asList();
        Assertions.assertEquals(0, preceding.size());
    }

    @Test
    public void precedingSiblingOrSelfAxis() {
        XdmNode third = body.select(child("chapter").where(attributeEq(xml_id, "fourth"))).asNode();
        XdmNode lastPara = third.select(descendant("para")).lastItem();

        List<XdmNode> preceding = lastPara.select(precedingSiblingOrSelf()).asList();
        Assertions.assertEquals(8, preceding.size());

        preceding = lastPara.select(precedingSiblingOrSelf("*")).asList();
        Assertions.assertEquals(4, preceding.size());

        preceding = lastPara.select(precedingSiblingOrSelf("para")).asList();
        Assertions.assertEquals(2, preceding.size());

        preceding = lastPara.select(precedingSiblingOrSelf(ns.toString(), "para")).asList();
        Assertions.assertEquals(2, preceding.size());

        preceding = lastPara.select(precedingSiblingOrSelf(ns_para)).asList();
        Assertions.assertEquals(2, preceding.size());

        preceding = lastPara.select(precedingSiblingOrSelf(ns_chapter)).asList();
        Assertions.assertEquals(0, preceding.size());
    }

    @Test
    public void precedingSiblingOrSelfAxisWithPredicate() {
        XdmNode third = body.select(child("chapter").where(attributeEq(xml_id, "fourth"))).asNode();
        XdmNode lastPara = third.select(descendant("para")).lastItem();

        List<XdmNode> preceding = lastPara.select(precedingSiblingOrSelf(hasName(ns_para))).asList();
        Assertions.assertEquals(2, preceding.size());

        preceding = lastPara.select(precedingSiblingOrSelf(attributeEq(xml_id, "spoon"))).asList();
        Assertions.assertEquals(0, preceding.size());
    }

    @Test
    public void atomizeTest() {
        XdmNode author = book.select(descendant("author")).firstItem();
        List<XdmAtomicValue> value = author.select(atomize()).asList();
        Assertions.assertEquals(1, value.size());
    }

    @Test
    public void tokenizeTest() {
        XdmNode author = book.select(descendant("author")).firstItem();
        List<XdmAtomicValue> value = author.select(tokenize()).asList();
        Assertions.assertEquals(2, value.size());
    }

    @Test
    public void castAsTest() {
        XdmNode chapter = book.select(descendant("chapter").where(attributeEq("id", "third"))).firstItem();
        XdmNode attr = chapter.select(attribute("number")).firstItem();
        XdmAtomicValue number = attr.select(castAs(ItemType.INTEGER)).firstItem();
        Assertions.assertInstanceOf(BigInteger.class, number.getValue());
        Assertions.assertEquals(BigInteger.valueOf(3), (BigInteger) number.getValue());
    }

    @Test
    public void pathNodeTest() {
        XdmNode chapter = book.select(path(child(), followingSibling("body"), child(ns_chapter))).at(2).get();
        XdmNode value = chapter.select(attribute("status")).firstItem();
        Assertions.assertEquals("draft", value.getUnderlyingValue().getStringValue());
    }

    @Test
    public void pathStringNodeTest() {
        XdmNode value = book.select(path("body", "chapter", "@status")).firstItem();
        Assertions.assertEquals("draft", value.getUnderlyingValue().getStringValue());
    }

    @Test
    public void idTest() {
        XdmNode idAttr = book.select(descendant("*").where(hasAttribute(xml_id)).then(attribute(xml_id))).firstItem();
        XdmNode chapter = idAttr.select(id(document)).firstItem();
        Assertions.assertEquals("fourth", chapter.getAttributeValue(xml_id));
    }

    @Test
    public void catTest() {
        List<XdmNode> titlesAndAuthors = book.select(descendant(ns_title).cat(descendant("author"))).asList();
        Assertions.assertEquals(12, titlesAndAuthors.size());
        for (int i = 0; i < titlesAndAuthors.size(); i++) {
            if (i >= 9) {
                Assertions.assertEquals(ns_author, titlesAndAuthors.get(i).getNodeName());
            } else {
                Assertions.assertEquals(ns_title, titlesAndAuthors.get(i).getNodeName());
            }
        }
    }

    @Test
    public void firstTest() {
        List<XdmNode> titles = book.select(descendant(ns_title).first()).asList();
        Assertions.assertEquals(1, titles.size());
        Assertions.assertEquals("Preface", titles.get(0).getStringValue());
    }

    @Test
    public void lastTest() {
        List<XdmNode> titles = book.select(descendant(ns_title).last()).asList();
        Assertions.assertEquals(1, titles.size());
        Assertions.assertEquals("Third Appendix", titles.get(0).getStringValue());
    }

    private XdmValue evaluateXPath(String xpath) {
        try {
            XPathCompiler compiler = processor.newXPathCompiler();
            XPathExecutable exec = compiler.compile(xpath);
            XPathSelector selector = exec.load();
            selector.setContextItem(document);
            return selector.evaluate();
        } catch (SaxonApiException ex) {
            throw new RuntimeException(ex);
        }
    }

    @Test
    public void nodeTest() {
        List<XdmNode> children = doc.select(child(isNode())).asList();
        Assertions.assertEquals(15, children.size());

        children = doc.select(child(isElement())).asList();
        Assertions.assertEquals(5, children.size());

        XdmNode element = doc.select(child(isElement()).first()).asNode();

        children = element.select(attribute(isAttribute())).asList();
        Assertions.assertEquals(2, children.size());

        children = doc.select(child(isText())).asList();
        Assertions.assertEquals(8, children.size());

        children = doc.select(child(isComment())).asList();
        Assertions.assertEquals(1, children.size());

        children = doc.select(child(isProcessingInstruction())).asList();
        Assertions.assertEquals(1, children.size());

        children = alternate.select(self(isDocument())).asList();
        Assertions.assertEquals(1, children.size());

        children = element.select(namespace(isNamespace())).asList();
        Assertions.assertEquals(2, children.size());
    }

    @Test
    public void predicateTest1() {
        XdmNode element = doc.select(child(isElement()).first()).asNode();

        XdmValue value = evaluateXPath("17");
        Assertions.assertTrue(value.stream().allMatch(Predicates.isAtomic()));

        value = evaluateXPath("map { 'x': 'spoon' }");
        Assertions.assertTrue(value.stream().allMatch(Predicates.isFunction()));
        Assertions.assertTrue(value.stream().allMatch(Predicates.isMap()));
        Assertions.assertFalse(value.stream().allMatch(Predicates.isArray()));

        value = evaluateXPath("array { (1,2,3) }");
        Assertions.assertTrue(value.stream().allMatch(Predicates.isFunction()));
        Assertions.assertFalse(value.stream().allMatch(Predicates.isMap()));
        Assertions.assertTrue(value.stream().allMatch(Predicates.isArray()));

        List<XdmNode> children = doc.select(child(empty(attribute(xml_id)))).asList();
        Assertions.assertEquals(14, children.size());

        children = doc.select(child(not(empty(attribute(xml_id))))).asList();
        Assertions.assertEquals(1, children.size());

        children = doc.select(child(exists(attribute(xml_id)))).asList();
        Assertions.assertEquals(1, children.size());
    }

    @Test
    public void predicateTest2() {
        List<XdmNode> children = doc.select(child(some(child("field"), hasAttribute("name")))).asList();
        Assertions.assertEquals(1, children.size());

        children = doc.select(child(some(child("property"), hasAttribute("name")))).asList();
        Assertions.assertEquals(1, children.size());

        // Note that every() returns true if the step doesn't select anything, so we have to work a little harder...
        children = doc.select(child(some(child("field"), hasAttribute("name")).and(every(child("field"), hasAttribute("name"))))).toList();
        Assertions.assertEquals(0, children.size());

        children = doc.select(child(some(child("property"), hasAttribute("name")).and(every(child("property"), hasAttribute("name"))))).asList();
        Assertions.assertEquals(1, children.size());
    }

    @Test
    public void predicateTest3() {
        List<XdmNode> children = doc.select(descendant(eq("Now is the time"))).toList();
        Assertions.assertEquals(4, children.size());
        Assertions.assertEquals("text", children.get(0).getNodeName().getLocalName());
        Assertions.assertEquals(XdmNodeKind.TEXT, children.get(1).getNodeKind());
        Assertions.assertEquals("text", children.get(2).getNodeName().getLocalName());
        Assertions.assertEquals(XdmNodeKind.TEXT, children.get(3).getNodeKind());

        children = doc.select(descendant(isText().and(eq("Now is the time")))).toList();
        Assertions.assertEquals(2, children.size());

        children = doc.select(descendant(eq(child(),"Now is the time"))).toList();
        Assertions.assertEquals(4, children.size());
        Assertions.assertEquals("wrapper3", children.get(0).getNodeName().getLocalName());
        Assertions.assertEquals("text", children.get(1).getNodeName().getLocalName());
        Assertions.assertEquals("wrapper4", children.get(2).getNodeName().getLocalName());
        Assertions.assertEquals("text", children.get(3).getNodeName().getLocalName());
    }

    @Test
    public void predicateTest4() {
        List<XdmNode> children = doc.select(descendant(isElement().and(matchesRegex("^Now is")))).toList();
        Assertions.assertEquals(3, children.size());

        children = doc.select(descendant(isElement().and(matchesRegex("^Now is the")))).toList();
        Assertions.assertEquals(2, children.size());
    }

    @Test
    public void mapTest() {
        List<String> children = doc.select(child(isElement())).map(node -> node.getNodeName().getLocalName()).toList();
        Assertions.assertEquals(5, children.size());
        Assertions.assertEquals("element", children.get(0));
        Assertions.assertEquals("wrapper1", children.get(1));
        Assertions.assertEquals("wrapper2", children.get(2));
        Assertions.assertEquals("wrapper3", children.get(3));
        Assertions.assertEquals("wrapper4", children.get(4));

        children = doc.select(descendant(isElement())).map(node -> node.getNodeName().getLocalName()).distinct().toList();
        Assertions.assertEquals(8, children.size());
        Assertions.assertEquals("element", children.get(0));
        Assertions.assertEquals("wrapper1", children.get(1));
        Assertions.assertEquals("field", children.get(2));
        Assertions.assertEquals("wrapper2", children.get(3));
        Assertions.assertEquals("property", children.get(4));
        Assertions.assertEquals("wrapper3", children.get(5));
        Assertions.assertEquals("text", children.get(6));
        Assertions.assertEquals("wrapper4", children.get(7));

    }

    @Test
    public void theHardWay() {
        // document = ... some process that loaded the book

        ArrayList<String> authors = new ArrayList<>();
        try {
            XPathCompiler compiler = processor.newXPathCompiler();
            compiler.declareNamespace("ex", "http://example.org/ns");
            XPathExecutable exec = compiler.compile("//ex:chapter[@status='draft']/ex:author/text()");
            XPathSelector selector = exec.load();
            selector.setContextItem(document);
            XdmValue value = selector.evaluate();
            for (XdmValue author : value) {
                authors.add(author.toString());
            }
        } catch (SaxonApiException ex) {
            // do something
        }
        Assertions.assertEquals(3, authors.size());
    }

    @Test
    public void theEasyWay() {
        // document = ... some process that loaded the book

        ArrayList<String> authors = new ArrayList<>();
        document.select(descendant("chapter")
                        .where(attributeEq("status", "draft"))
                        .then(child("author"))
                        .then(text()))
                        .asList().forEach(item -> authors.add(item.toString()));

        Assertions.assertEquals(3, authors.size());
    }

}
