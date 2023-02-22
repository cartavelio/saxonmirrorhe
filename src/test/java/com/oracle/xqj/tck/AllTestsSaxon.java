// Copyright (c) 2003, 2006, 2007, Oracle. All rights reserved.
package com.oracle.xqj.tck;

import com.oracle.xqj.tck.testcases.*;
import junit.framework.Test;
import junit.framework.TestSuite;
import junit.textui.TestRunner;


public class AllTestsSaxon {

  public static void main(String[] args) {
   System.setProperty("com.oracle.xqj.tck.datasource", "/Users/mike/repo2/lib/xqj-final/tck/examples/saxon.properties");
   TestRunner.run(suite());
 /*  TestResult result =  new TestRunner().doRun(suite());
   System.out.print(result.toString());
   result = null;
  */}

  public static Test suite() {
    TestSuite suite = (TestSuite)AllTests.suite();
    suite.addTestSuite(SaxonXQConnectionTest.class);
    suite.addTestSuite(SaxonXQExternalFunctionTest.class);
    return suite;
  }
}