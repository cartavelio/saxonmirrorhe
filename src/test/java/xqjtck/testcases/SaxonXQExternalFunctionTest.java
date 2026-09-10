// Copyright (c) 2003, 2006, 2007, Oracle. All rights reserved.
package xqjtck.testcases;

import org.junit.Assert;

import javax.xml.namespace.QName;
import java.util.Locale;
import javax.xml.xquery.*;

public class SaxonXQExternalFunctionTest extends XQJTestCase {

    /**
     * Test the ability to invoke instance-level methods on an external object supplied as a query parameter
     * @throws javax.xml.xquery.XQException
     */

    public void testInstanceLevelMethodsOnExternalObject() throws XQException {

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

        Assert.assertEquals("A-SXQEF-1.1: wrong result", result.getAtomicValue(), "Italian");

    }

}