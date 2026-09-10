// Copyright (c) 2003, 2006, 2007, Oracle. All rights reserved.
package com.oracle.xqj.tck.testcases;


import javax.xml.xquery.XQException;

public class SaxonXQConnectionTest extends XQJTestCase {

    /**
     * Test the Saxon method to copy a preparedExpression from one connection to another
     * @throws XQException
     */

//    public void testCopy() throws XQException {
//
//        XQConnection xqc1, xqc2;
//        XQPreparedExpression xqe1, xqe2;
//        XQSequence xqs1, xqs2;
//
//        xqc1 = xqds.getConnection();
//        xqc2 = xqds.getConnection();
//
//        XQStaticContext sc1 = xqc1.getStaticContext();
//        sc1.setBaseURI("http://www.w3.org/somewhere");
//
//        xqc1.setStaticContext(sc1);
//        xqe1 = xqc1.prepareExpression("2+2");
//        xqs1 = xqe1.executeQuery();
//
//        xqe2 = ((SaxonXQConnection)xqc2).copyPreparedExpression(xqe1);
//        xqs2 = xqe2.executeQuery();
//
//        xqs1.next();
//        xqs2.next();
//
//        Assert.assertEquals("A-SXQC-1.1: copied expression produces same result",
//                xqs1.getItem().getInt(), xqs2.getItem().getInt());
//        Assert.assertEquals("A-SXQC-1.2: copied expression has same base URI",
//                xqe1.getStaticContext().getBaseURI(), xqe2.getStaticContext().getBaseURI());
//    }

}