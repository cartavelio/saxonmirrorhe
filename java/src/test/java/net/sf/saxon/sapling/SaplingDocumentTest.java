package net.sf.saxon.sapling;

import net.sf.saxon.s9api.QName;
import net.sf.saxon.s9api.SaxonApiException;
import org.junit.jupiter.api.DisplayName;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.*;

class SaplingDocumentTest implements BaseSaplingTest {

    @Nested
    class DSL {
        @Test
        void dslAllowsEmptyElementCreation() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc(d -> {
                d.elem("root");
            });

            assertEquals("<root/>", serializeSaplingToString(doc), "result should contain empty root element");
        }

        @Test
        void dslAllowsEmptyElementCreationUsingQName() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc(d -> {
                d.elem(new QName("eg","http://example.org/#ns", "root"));
            });

            assertEquals("<eg:root xmlns:eg=\"http://example.org/#ns\"/>", serializeSaplingToString(doc), "result should contain empty root element in a namespace");
        }

        @Test
        void dslAllowsElementCreationWithTextContent() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc(d -> {
                d.elem("root", "content");
            });

            assertEquals("<root>content</root>", serializeSaplingToString(doc), "result should contain root element with simple text content");
        }

        @Test
        void dslAllowsEmptyElementCreationWithTextContentUsingQName() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc(d -> {
                d.elem(new QName("eg","http://example.org/#ns", "root"), "content");
            });

            assertEquals("<eg:root xmlns:eg=\"http://example.org/#ns\">content</eg:root>", serializeSaplingToString(doc), "result should contain empty root element in a namespace");
        }

        @Test
        void testDslAllowsCommentCreation() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc(d -> {
                d.comment("Hello, World!");
            });

            assertEquals("<!--Hello, World!-->", serializeSaplingToString(doc), "result");
        }

        @Test
        void testDslAllowsTextCreation() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc(d -> {
                d.text("Hello, World!");
            });

            assertEquals("Hello, World!", serializeSaplingToString(doc), "result");
        }

        @Test
        void testDslAllowsPICreation() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc(d -> {
                d.pi("Hello", "World");
            });

            assertEquals("<?Hello World?>", serializeSaplingToString(doc), "result");
        }

        @Test
        @DisplayName("Allows multiple children of the document to be created in the DSL")
        void testDslAllowsMultipleThings() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc(d -> {
                d.pi("Hello", "World");
                d.elem("root");
            });

            assertEquals("<?Hello World?><root/>", serializeSaplingToString(doc), "result");
        }

        @Test
        @DisplayName("DSL methods can be chained, following the Builder pattern")
        void testDslUsesBuilderPattern() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc(d -> {
                d.elem("root").pi("Hello", "World");
            });

            assertEquals("<root/><?Hello World?>", serializeSaplingToString(doc), "result");
        }

        @Test
        @DisplayName("Elements named with a String and created with the DSL can pass their own DSL lambda")
        void dslAllowsElementCreationWithDsl() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc(d -> {
                d.elem("root", e -> {
                    e.elem("child");
                });
            });

            assertEquals("<root><child/></root>", serializeSaplingToString(doc), "result should contain root element with child");
        }

        @Test
        @DisplayName("Elements named with a QName and created with the DSL can pass their own DSL lambda")
        void dslAllowsElementCreationWithQNameAndDsl() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc(d -> {
                d.elem(new QName("eg","http://example.org/#ns", "root"), e -> {
                    e.elem("child");
                });
            });

            assertEquals("<eg:root xmlns:eg=\"http://example.org/#ns\"><child/></eg:root>", serializeSaplingToString(doc), "result should contain root element with child");
        }

        @Test
        void dslAllowsSettingDefaultNsForChildren() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc(root -> {
                root.defaultNs("http://example.org/#ns");
                root.elem("root",  el -> {
                    el.elem("child");
                });
            });

            assertEquals("<root xmlns=\"http://example.org/#ns\"><child/></root>", serializeSaplingToString(doc), "result should contain correctly serialized elements");
        }
    }
}
