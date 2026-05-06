// Copyright (c) 2003, 2006, 2007, Oracle. All rights reserved.
package xqjtck.testcases;


import javax.xml.xquery.XQConnection;
import javax.xml.xquery.XQDataSource;
import javax.xml.xquery.XQException;
import com.saxonica.xqj.SaxonXQDataSource;

import org.junit.jupiter.api.*;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class SaxonXQConnectionTest {
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

    /**
     * Test the Saxon method to copy a preparedExpression from one connection to another
     * @throws XQException
     */
//    @Test
//    void testCopy() throws XQException {
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
//        Assert.assertEquals(xqs1.getItem().getInt(), xqs2.getItem().getInt(),
//                "A-SXQC-1.1: copied expression produces same result");
//        Assert.assertEquals(xqe1.getStaticContext().getBaseURI(), xqe2.getStaticContext().getBaseURI(),
//                "A-SXQC-1.2: copied expression has same base URI");
//    }
}