package net.sf.saxon.sapling;

import net.sf.saxon.s9api.*;
import org.junit.jupiter.api.Nested;
import org.junit.jupiter.api.Test;

import java.io.StringWriter;

import static net.sf.saxon.sapling.Saplings.doc;
import static net.sf.saxon.sapling.Saplings.elem;
import static org.junit.jupiter.api.Assertions.*;

class SaplingsTest implements BaseSaplingTest {
    @Nested
    class Documents {
        @Test
        void testEmptyDocument() {
            assertInstanceOf(SaplingDocument.class, Saplings.doc(), "should create a SaplingDocument");
        }

        @Test
        void testEmptyDocumentWithBaseURI() {
            SaplingDocument doc = Saplings.doc("http://example.org/");
            assertEquals("http://example.org/", doc.getSystemId(), "baseURI should set system ID");
        }

        @Test
        void testAllowsDsl() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc(d -> {
                d.elem("root");
            });

            assertEquals("<root/>", serializeSaplingToString(doc), "result");
        }

        @Test
        void testAllowsBaseUriAndDsl() throws SaxonApiException {
            SaplingDocument doc = Saplings.doc("http://example.org", d -> {
                d.elem("root");
            });

            assertEquals("<root/>", serializeSaplingToString(doc), "result");
        }
    }

    @Nested
    class Elements {
        @Test
        void allowsCreatingElementWithStringNameAndDsl() throws SaxonApiException {
            SaplingElement element = Saplings.elem("root", el -> {
                el.text("Hello, World!");
            });

            assertEquals("<root>Hello, World!</root>", serializeSaplingToString(element), "result should contain empty child element");
        }

        @Test
        void allowsCreatingElementWithQNameAndDsl() throws SaxonApiException {
            SaplingElement element = Saplings.elem(new QName("", "http://example.org/", "root"), el -> {
                el.text("Hello, World!");
            });

            assertEquals("<root xmlns=\"http://example.org/\">Hello, World!</root>", serializeSaplingToString(element), "result should contain empty child element");
        }
    }
}