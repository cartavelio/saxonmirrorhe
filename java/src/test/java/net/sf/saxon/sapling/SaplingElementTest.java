package net.sf.saxon.sapling;

import net.sf.saxon.s9api.QName;
import net.sf.saxon.s9api.SaxonApiException;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

class SaplingElementTest implements BaseSaplingTest {
    @Test
    void creatingElementInDefaultNSSerializesCorrectly() throws SaxonApiException {
        SaplingElement element = Saplings.elem(new QName("", "http://example.org/", "root")).withChild(Saplings.elem(new QName("", "http://example.org/", "child")));

        assertEquals("<root xmlns=\"http://example.org/\"><child/></root>", serializeSaplingToString(element), "result should contain empty child element");
    }

    @Nested
    class DSL {
        @Test
        void testDslAllowsEmptyElementCreation() throws SaxonApiException {
            SaplingElement element = Saplings.elem("root", el -> {
                el.elem("child");
            });

            assertEquals("<root><child/></root>", serializeSaplingToString(element), "result should contain empty child element");
        }

        @Test
        void testDslAllowsEmptyElementCreationUsingQName() throws SaxonApiException {
            SaplingElement element = Saplings.elem("root", el -> {
                el.elem(new QName("eg", "http://example.org/#ns", "child"));
            });

            assertEquals("<root><eg:child xmlns:eg=\"http://example.org/#ns\"/></root>", serializeSaplingToString(element), "result should contain empty child element in a namespace");
        }

        @Test
        void testDslAllowsNamespaceCreation() throws SaxonApiException {
            SaplingElement element = Saplings.elem("el", el -> {
                el.ns("eg", "http://example.org/#ns");
            });

            assertEquals("<el xmlns:eg=\"http://example.org/#ns\"/>", serializeSaplingToString(element), "result element should have a namespace declaration");
        }

        @Test
        void testDslAllowsAttributeCreation() throws SaxonApiException {
            SaplingElement element = Saplings.elem("el", el -> {
                el.attr("eg", "definitive");
            });

            assertEquals("<el eg=\"definitive\"/>", serializeSaplingToString(element), "result element should have a namespace declaration");
        }

        @Test
        void testDslAllowsAttributeCreationWithQName() throws SaxonApiException {
            SaplingElement element = Saplings.elem("el", el -> {
                el.attr(new QName("eg", "http://example.org/#ns", "attr"), "namespaced");
            });

            assertEquals("<el xmlns:eg=\"http://example.org/#ns\" eg:attr=\"namespaced\"/>", serializeSaplingToString(element), "result element should have a namespace declaration");
        }

        @Test
        void testDslAllowsCommentCreation() throws SaxonApiException {
            SaplingElement element = Saplings.elem("eg", el -> {
                el.comment("Hello, World!");
            });

            assertEquals("<eg><!--Hello, World!--></eg>", serializeSaplingToString(element), "result element should contain a comment");
        }

        @Test
        void testDslAllowsTextCreation() throws SaxonApiException {
            SaplingElement element = Saplings.elem("eg", el -> {
                el.text("Hello, World!");
            });

            assertEquals("<eg>Hello, World!</eg>", serializeSaplingToString(element), "result element should have specified text content");
        }

        @Test
        void testDslAllowsPICreation() throws SaxonApiException {
            SaplingElement element = Saplings.elem("eg", el -> {
                el.pi("Hello", "World");
            });

            assertEquals("<eg><?Hello World?></eg>", serializeSaplingToString(element), "result element should contain a PI");
        }

        @Test
        void dslEnablesMixedContent() throws SaxonApiException {
            SaplingElement element = Saplings.elem("eg", el -> {
                el.text("before").elem("empty").elem("child", "simple content").text("after");
            });

            assertEquals("<eg>before<empty/><child>simple content</child>after</eg>", serializeSaplingToString(element), "result element should contain mixed content");
        }

        @Test
        void dslAllowsSettingDefaultNsForChildren() throws SaxonApiException {
            SaplingElement element = Saplings.elem("root", el -> {
                el.defaultNs("http://example.org/");
                el.elem("child", child -> {
                    child.elem("grandchild");
                });
            });

            assertEquals("<root><child xmlns=\"http://example.org/\"><grandchild/></child></root>", serializeSaplingToString(element), "result should contain correctly serialized elements");
        }
    }
}
