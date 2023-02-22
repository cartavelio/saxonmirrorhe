// Copyright (c) 2003, 2006, 2007, 2008 Oracle. All rights reserved.
package com.oracle.xqj.tck.testcases;

import com.oracle.xqj.tck.TestContentHandler;

import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import javax.xml.transform.stream.StreamResult;
import javax.xml.xquery.XQException;
import javax.xml.xquery.XQExpression;
import javax.xml.xquery.XQSequence;
import java.io.ByteArrayOutputStream;
import java.io.StringWriter;
import java.io.UnsupportedEncodingException;
import java.util.NoSuchElementException;
import java.util.Properties;

/**
 * Additional Saxon test cases to cover Saxon bug 3805: effect of calling sequence-level methods after
 * an initial call of next().
 */

public class SaxonXQSequenceTest extends XQJTestCase {
  
  

  public void testGetSequenceAsStream() throws XQException {
    XQExpression xqe;
    XQSequence xqs;
    XMLStreamReader xmlReader = null;

      xqe = xqc.createExpression();
      xqs = xqe.executeQuery("<e>Hello world!</e>");
      junit.framework.Assert.assertTrue("A-XQS-EXTRA-1: next() returned false", xqs.next());
      try {
          xmlReader = xqs.getSequenceAsStream();
      } catch (XQException e) {
          junit.framework.Assert.fail("A-XQS-22.1: getSequenceAsStream failed with message: " + e.getMessage());
      }
      try {
          junit.framework.Assert.assertNotNull("A-XQS-22.1: getSequenceAsStream returned a null XMLStreamReader", xmlReader);
          junit.framework.Assert.assertEquals("A-XQS-22.1: unexpected first event returned by XMLStreamReader", XMLStreamReader.START_DOCUMENT, xmlReader.getEventType());
          junit.framework.Assert.assertEquals("A-XQS-22.1: unexpected second event returned by XMLStreamReader", XMLStreamReader.START_ELEMENT, xmlReader.next());
          junit.framework.Assert.assertEquals("A-XQS-22.1: unexpected third event returned by XMLStreamReader", XMLStreamReader.CHARACTERS, xmlReader.next());
          junit.framework.Assert.assertEquals("A-XQS-22.1: unexpected fourth event returned by XMLStreamReader", XMLStreamReader.END_ELEMENT, xmlReader.next());
          junit.framework.Assert.assertEquals("A-XQS-22.1: unexpected fifth event returned by XMLStreamReader", XMLStreamReader.END_DOCUMENT, xmlReader.next());
          try {
              int last = xmlReader.next();
              junit.framework.Assert.fail("A-XQS-22.1: XMLStreamReader.next() returned " + last + " at end");
          } catch (NoSuchElementException | IllegalStateException nsee) {
              // Javadoc is unclear which exception should be thrown. Raised as Saxon bug 3806, JDK bug ID:9055431
              // expected failure
          }
      } catch (XMLStreamException e) {
          junit.framework.Assert.fail("A-XQS-22.1: XMLStreamReader.next() failed with message: " + e.getMessage());
      }
      xqe.close();
    
    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!<!--c--></e>");
    junit.framework.Assert.assertTrue("A-XQS-EXTRA-1: next() returned false", xqs.next());
    try {
      xmlReader = xqs.getSequenceAsStream();
    } catch (XQException e) {
      junit.framework.Assert.fail("A-XQS-22.1: getSequenceAsStream failed with message: " + e.getMessage());
    }
    try {
      junit.framework.Assert.assertNotNull("A-XQS-22.1: getSequenceAsStream returned a null XMLStreamReader", xmlReader);
      junit.framework.Assert.assertEquals("A-XQS-22.1: unexpected first event returned by XMLStreamReader", XMLStreamReader.START_DOCUMENT, xmlReader.getEventType());
      junit.framework.Assert.assertEquals("A-XQS-22.1: unexpected second event returned by XMLStreamReader", XMLStreamReader.START_ELEMENT, xmlReader.next());
      junit.framework.Assert.assertEquals("A-XQS-22.1: unexpected third event returned by XMLStreamReader", XMLStreamReader.CHARACTERS, xmlReader.next());
        junit.framework.Assert.assertEquals("A-XQS-22.1: unexpected next event returned by XMLStreamReader", XMLStreamReader.COMMENT, xmlReader.next());
      junit.framework.Assert.assertEquals("A-XQS-22.1: unexpected fourth event returned by XMLStreamReader", XMLStreamReader.END_ELEMENT, xmlReader.next());
      junit.framework.Assert.assertEquals("A-XQS-22.1: unexpected fifth event returned by XMLStreamReader", XMLStreamReader.END_DOCUMENT, xmlReader.next());
      try {
          int last = xmlReader.next();
          junit.framework.Assert.fail("A-XQS-22.1: XMLStreamReader.next() returned " + last + " at end");
      } catch (NoSuchElementException | IllegalStateException nsee) {
          // Javadoc is unclear which exception should be thrown. Raised as Saxon bug 3806, JDK bug ID:9055431
          // expected failure
      }
    } catch (XMLStreamException e) {
      junit.framework.Assert.fail("A-XQS-22.1: XMLStreamReader.next() failed with message: " + e.getMessage());
    }
    xqe.close();

  }

  public void testGetSequenceAsString() throws XQException {
    XQExpression xqe;
    XQSequence xqs;

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    String result = null;
    junit.framework.Assert.assertTrue("A-XQS-EXTRA-1: next() returned false", xqs.next());
    try {
      result = xqs.getSequenceAsString(new Properties());
    } catch (XQException e) {
      junit.framework.Assert.fail("A-XQS-23.1: getSequenceAsString failed with message: " + e.getMessage());
    }
    junit.framework.Assert.assertTrue("A-XQS-23.1: Expects serialized result contains '<e>Hello world!</e>', but it is '" + result + "'", result.indexOf("<e>Hello world!</e>") != -1);
    xqe.close();

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    junit.framework.Assert.assertTrue("A-XQS-EXTRA-1: next() returned false", xqs.next());
    try {
      junit.framework.Assert.assertEquals("A-XQS-23.2: null properties argument is equivalent to empty properties argument", result, xqs.getSequenceAsString(null));
    } catch (XQException e) {
      junit.framework.Assert.fail("A-XQS-23.2: getSequenceAsString failed with message: " + e.getMessage());
    }
    xqe.close();

      xqe = xqc.createExpression();
      xqs = xqe.executeQuery("()");
      junit.framework.Assert.assertFalse("A-XQS-EXTRA-1: next() returned true", xqs.next());
      try {
          junit.framework.Assert.assertEquals("A-XQS-23.2: null properties argument is equivalent to empty properties argument", "", xqs.getSequenceAsString(null));
      } catch (XQException e) {
          junit.framework.Assert.fail("A-XQS-23.2: getSequenceAsString failed with message: " + e.getMessage());
      }
      xqe.close();

      xqe = xqc.createExpression();
      xqs = xqe.executeQuery("<e>Hello world!</e>, <e>Goodbye Mars!</e>");
      junit.framework.Assert.assertTrue("A-XQS-EXTRA-1: next() returned false", xqs.next());
      junit.framework.Assert.assertTrue("A-XQS-EXTRA-2: next() returned false", xqs.next());
      try {
          junit.framework.Assert.assertTrue("A-XQS-23.2: wrong content", xqs.getSequenceAsString(null).contains("<e>Goodbye Mars!</e>"));
      } catch (XQException e) {
          junit.framework.Assert.fail("A-XQS-23.2: getSequenceAsString failed with message: " + e.getMessage());
      }
      xqe.close();

  }


  public void testWriteSequence_OutputStream() throws XQException, UnsupportedEncodingException {

    // We don't expect this method ever to throw UnsupportedEncodingException, as we only request the "UTF-8" encoding.
    // However, in order to make the compiler happy, and to play it safe, add UnsupportedEncodingException to the throws clause.

    XQExpression xqe;
    XQSequence xqs;

    Properties prop = new Properties();
    prop.setProperty("encoding", "UTF-8");

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    ByteArrayOutputStream result = new ByteArrayOutputStream();
    junit.framework.Assert.assertTrue("A-XQS-EXTRA-1: next() returned false", xqs.next());
    try {
      xqs.writeSequence(result, prop);
    } catch (XQException e) {
      junit.framework.Assert.fail("A-XQS-24.1: writeSequence failed with message: " + e.getMessage());
    }
    junit.framework.Assert.assertTrue("A-XQS-24.1: Expects serialized result contains '<e>Hello world!</e>', but it is '" + result.toString("UTF-8") + "'", result.toString("UTF-8").indexOf("<e>Hello world!</e>") != -1);
    xqe.close();

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    ByteArrayOutputStream otherResult = new ByteArrayOutputStream();
    junit.framework.Assert.assertTrue("A-XQS-EXTRA-1: next() returned false", xqs.next());
    try {
      xqs.writeSequence(otherResult, prop);
    } catch (XQException e) {
      junit.framework.Assert.fail("A-XQS-24.2: writeSequence failed with message: " + e.getMessage());
    }
    junit.framework.Assert.assertEquals("A-XQS-24.2: null properties argument is equivalent to empty properties argument", result.toString("UTF-8"), otherResult.toString("UTF-8"));
    xqe.close();

      xqe = xqc.createExpression();
      xqs = xqe.executeQuery("()");
      otherResult = new ByteArrayOutputStream();
      junit.framework.Assert.assertFalse("A-XQS-EXTRA-1: next() returned true", xqs.next());
      try {
          xqs.writeSequence(otherResult, prop);
      } catch (XQException e) {
          junit.framework.Assert.fail("A-XQS-24.2: writeSequence failed with message: " + e.getMessage());
      }
      junit.framework.Assert.assertEquals("A-XQS-24.2: null properties argument is equivalent to empty properties argument", new ByteArrayOutputStream().toString("UTF-8"), otherResult.toString("UTF-8"));
      xqe.close();
  }

  public void testWriteSequence_Writer() throws XQException {
    XQExpression xqe;
    XQSequence xqs;

    Properties prop = new Properties();
    prop.setProperty("encoding", "UTF-8");


    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    StringWriter result = new StringWriter();
    junit.framework.Assert.assertTrue("A-XQS-EXTRA-1: next() returned false", xqs.next());
    try {
      xqs.writeSequence(result, prop);
    } catch (XQException e) {
      junit.framework.Assert.fail("A-XQS-24.1: writeSequence failed with message: " + e.getMessage());
    }
    junit.framework.Assert.assertTrue("A-XQS-24.1: Expects serialized result contains '<e>Hello world!</e>', but it is '" + result.toString() + "'", result.toString().indexOf("<e>Hello world!</e>") != -1);
    xqe.close();

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    StringWriter otherResult = new StringWriter();
    junit.framework.Assert.assertTrue("A-XQS-EXTRA-1: next() returned false", xqs.next());
    try {
      xqs.writeSequence(otherResult, prop);
    } catch (XQException e) {
      junit.framework.Assert.fail("A-XQS-24.2: writeSequence failed with message: " + e.getMessage());
    }
    junit.framework.Assert.assertEquals("A-XQS-24.2: null properties argument is equivalent to empty properties argument", result.toString(), otherResult.toString());
    xqe.close();

      xqe = xqc.createExpression();
      xqs = xqe.executeQuery("()");
      otherResult = new StringWriter();
      junit.framework.Assert.assertFalse("A-XQS-EXTRA-1: next() returned true", xqs.next());
      try {
          xqs.writeSequence(otherResult, prop);
      } catch (XQException e) {
          junit.framework.Assert.fail("A-XQS-24.2: writeSequence failed with message: " + e.getMessage());
      }
      junit.framework.Assert.assertEquals("A-XQS-24.2: null properties argument is equivalent to empty properties argument", "", otherResult.toString());
      xqe.close();
  }

  public void testWriteSequenceToSAX() throws XQException {
    XQExpression xqe;
    XQSequence xqs;

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    TestContentHandler result = new TestContentHandler();
    junit.framework.Assert.assertTrue("A-XQS-EXTRA-1: next() returned false", xqs.next());
    try {
      xqs.writeSequenceToSAX(result);
    } catch (XQException e) {
      junit.framework.Assert.fail("A-XQS-24.1: writeSequence failed with message: " + e.getMessage());
    }
    junit.framework.Assert.assertTrue("A-XQS-24.1: Expects serialized result contains '<e>Hello world!</e>', but it is '" + result.buffer.toString() + "'", result.buffer.toString().indexOf("<e>Hello world!</e>") != -1);
    xqe.close();
  }

  public void testWriteSequenceToResult() throws XQException {
    XQExpression xqe;
    XQSequence xqs;


//  The spec doesn't say serialization is performed according to XSLT 2.0 and XQuery 1.0 serialization.
//  As such we don't test here if the serialization process can fail.
      // Test reinstated as a Saxon-specific test

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e a='Hello world!'/>/@*");
    junit.framework.Assert.assertTrue("A-XQS-EXTRA-1: next() returned false", xqs.next());
    try {
      xqs.writeSequenceToResult(new StreamResult(new StringWriter()));
      junit.framework.Assert.fail("A-XQS-21.1: serialization process fails when sequence contains a top-level attribute");
    } catch (XQException xq) {
      // Expect an XQException
    }
    xqe.close();


    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    StringWriter result = new StringWriter();
    junit.framework.Assert.assertTrue("A-XQS-EXTRA-1: next() returned false", xqs.next());
    try {
      xqs.writeSequenceToResult(new StreamResult(result));
    } catch (XQException e) {
      junit.framework.Assert.fail("A-XQS-24.1: writeSequence failed with message: " + e.getMessage());
    }
    junit.framework.Assert.assertTrue("A-XQS-24.1: Expects serialized result contains '<e>Hello world!</e>', but it is '" + result.toString() + "'", result.toString().indexOf("<e>Hello world!</e>") != -1);
    xqe.close();

      xqe = xqc.createExpression();
      xqs = xqe.executeQuery("()");
      result = new StringWriter();
      junit.framework.Assert.assertFalse("A-XQS-EXTRA-1: next() returned true", xqs.next());
      try {
          xqs.writeSequenceToResult(new StreamResult(result));
      } catch (XQException e) {
          junit.framework.Assert.fail("A-XQS-24.1: writeSequence failed with message: " + e.getMessage());
      }
      junit.framework.Assert.assertTrue("A-XQS-24.1: Expects serialized result to be empty, but it is '" + result.toString() + "'", result.toString().equals(""));
      xqe.close();
  }
}
