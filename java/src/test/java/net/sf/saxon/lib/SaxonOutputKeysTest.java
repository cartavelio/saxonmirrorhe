package net.sf.saxon.lib;

import org.junit.jupiter.api.Test;

import java.util.Properties;

import static org.junit.jupiter.api.Assertions.*;

class SaxonOutputKeysTest {

    @Test
    void isXhtmlHtmlVersion5_unset() {
        Properties properties = new Properties();
        properties.setProperty("method", "xhtml");
        assertTrue(SaxonOutputKeys.isXhtmlHtmlVersion5(properties));
    }

    @Test
    void isXhtmlHtmlVersion5_unset_ignoreXMLVersion() {
        Properties properties = new Properties();
        properties.setProperty("method", "xhtml");
        properties.setProperty("version", "1.0");
        assertTrue(SaxonOutputKeys.isXhtmlHtmlVersion5(properties));
    }

    @Test
    void isXhtmlHtmlVersion5_v1() {
        Properties properties = new Properties();
        properties.setProperty("method", "xhtml");
        properties.setProperty("html-version", "1");
        assertFalse(SaxonOutputKeys.isXhtmlHtmlVersion5(properties));
    }

    @Test
    void isXhtmlHtmlVersion5_v1_1() {
        Properties properties = new Properties();
        properties.setProperty("method", "xhtml");
        properties.setProperty("html-version", "1.1");
        assertFalse(SaxonOutputKeys.isXhtmlHtmlVersion5(properties));
    }

    @Test
    void isXhtmlHtmlVersion5_v5() {
        Properties properties = new Properties();
        properties.setProperty("method", "xhtml");
        properties.setProperty("html-version", "5");
        assertTrue(SaxonOutputKeys.isXhtmlHtmlVersion5(properties));
    }

    @Test
    void isXhtmlHtmlVersion5_v5_0() {
        Properties properties = new Properties();
        properties.setProperty("method", "xhtml");
        properties.setProperty("html-version", "5.0");
        assertTrue(SaxonOutputKeys.isXhtmlHtmlVersion5(properties));
    }

    @Test
    void isXhtmlHtmlVersion5_unexpected_v5_1() {
        Properties properties = new Properties();
        properties.setProperty("method", "xhtml");
        properties.setProperty("html-version", "5.1");
        assertFalse(SaxonOutputKeys.isXhtmlHtmlVersion5(properties));
    }

    @Test
    void isXhtmlHtmlVersion5_unexpected_v4() {
        Properties properties = new Properties();
        properties.setProperty("method", "xhtml");
        properties.setProperty("html-version", "4");
        assertFalse(SaxonOutputKeys.isXhtmlHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_unset() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        assertTrue(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_unset_fallbackToVersion() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("version", "1.0");
        assertFalse(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_ignoreXMLVersion() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("html-version", "5");
        properties.setProperty("version", "1.0");
        assertTrue(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_v1() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("html-version", "1");
        assertFalse(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_v1_fallbackToVersion() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("version", "1.0");
        assertFalse(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_v2() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("html-version", "2");
        assertFalse(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_v2_fallbackToVersion() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("version", "2");
        assertFalse(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_v3() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("html-version", "3.2");
        assertFalse(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_v3_fallbackToVersion() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("version", "3.2");
        assertFalse(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_v4() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("html-version", "4");
        assertFalse(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_v4_01() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("html-version", "4.01");
        assertFalse(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_v4_fallbackToVersion() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("version", "4.01");
        assertFalse(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_v5() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("html-version", "5");
        assertTrue(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_v5_0() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("html-version", "5.0");
        assertTrue(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_unexpected_v5_1() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("html-version", "5.1");
        assertFalse(SaxonOutputKeys.isHtmlVersion5(properties));
    }

    @Test
    void isHtmlVersion5_unexpected_v4() {
        Properties properties = new Properties();
        properties.setProperty("method", "html");
        properties.setProperty("html-version", "0");
        assertFalse(SaxonOutputKeys.isHtmlVersion5(properties));
    }
}
