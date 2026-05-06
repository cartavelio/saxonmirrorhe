// Copyright (c) 2003, 2006, 2007, Oracle. All rights reserved.
package xqjtck.testcases;

import javax.xml.namespace.QName;
import java.util.Locale;
import javax.xml.xquery.*;
import com.saxonica.xqj.SaxonXQDataSource;

import org.junit.jupiter.api.*;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class SaxonXQExternalFunctionTest {
    XQDataSource xqds;
    XQConnection xqc;

    @BeforeEach
    void setUp() throws Exception {
        xqds = new SaxonXQDataSource();
        xqc = xqds.getConnection();
    }

    @AfterEach
    void tearDown() throws Exception {
        xqc.close();
    }

    @Test
    @DisplayName("Test the ability to invoke instance-level methods on an external object supplied as a query parameter")
    void testInstanceLevelMethodsOnExternalObject() throws XQException {

        XQConnection conn = xqds.getConnection();

        XQPreparedExpression exp = conn.prepareExpression(
                "declare namespace jt = 'http://saxon.sf.net/java-type';\n" +
                "declare namespace Locale = 'java:java.util.Locale';\n" +
                "declare variable $locale as jt:java.util.Locale external;\n" +
                "Locale:getDisplayLanguage($locale)");

        XQItemType localeType = conn.createAtomicType(
                XQItemType.XQBASETYPE_ANYATOMICTYPE,
                new QName("http://saxon.sf.net/java-type", "java.util.Locale"),
                null);
        XQItem italianLocale = conn.createItemFromObject(Locale.ITALIAN, localeType);
        exp.bindItem(new QName("locale"), italianLocale);
        XQResultSequence result = exp.executeQuery();
        result.next();

        assertEquals(result.getAtomicValue(), "Italian", "A-SXQEF-1.1: wrong result");
    }
}