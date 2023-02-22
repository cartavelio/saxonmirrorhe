// Copyright (c) 2003, 2006, 2007, 2008 Oracle. All rights reserved.
package com.oracle.xqj.tck.testcases;

import org.junit.Assert;

import javax.xml.namespace.QName;
import javax.xml.xquery.XQCancelledException;
import javax.xml.xquery.XQStackTraceElement;

import static org.junit.jupiter.api.Assertions.fail;

public class XQCancelledExceptionTest extends XQJTestCase {

  public void testConstructor() {
    try {
      XQCancelledException ex = new XQCancelledException("Hello world!", "VendorCode", new QName("foo"), 7, 8, 56,
                                                         "moduleuri", null, (XQStackTraceElement[])null);
      Assert.assertEquals("A-XQCEX-1.1: XQCancelledException successfully created", "Hello world!", ex.getMessage());
      Assert.assertEquals("A-XQCEX-1.1: XQCancelledException successfully created", "VendorCode", ex.getVendorCode());
      Assert.assertEquals("A-XQCEX-1.1: XQCancelledException successfully created", new QName("foo"), ex.getErrorCode());
      Assert.assertEquals("A-XQCEX-1.1: XQCancelledException successfully created", 7, ex.getLineNumber());
      Assert.assertEquals("A-XQCEX-1.1: XQCancelledException successfully created", 8, ex.getColumnNumber());
      Assert.assertEquals("A-XQCEX-1.1: XQCancelledException successfully created", 56, ex.getPosition());
      Assert.assertEquals("A-XQCEX-1.1: XQCancelledException successfully created", "moduleuri", ex.getModuleURI());
      Assert.assertTrue("A-XQCEX-1.1: XQCancelledException successfully created", null == ex.getQueryStackTrace());
      
    } catch (Exception e) {
      fail("A-XQCEX-1.1: Creating XQCancelledException faild with message: " + e.getMessage());
    }
    
  }

}
