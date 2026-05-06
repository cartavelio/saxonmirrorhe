// Copyright (c) 2003, 2006, 2007, 2008 Oracle. All rights reserved.
package xqjtck.testcases;

import xqjtck.TestContentHandler;

import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamReader;
import javax.xml.transform.stream.StreamResult;
import javax.xml.xquery.*;
import java.io.ByteArrayOutputStream;
import java.io.StringWriter;
import java.io.UnsupportedEncodingException;
import java.util.NoSuchElementException;
import java.util.Properties;

import com.saxonica.xqj.SaxonXQDataSource;

import org.junit.jupiter.api.*;

import static org.junit.jupiter.api.Assertions.*;

/**
 * Additional Saxon test cases to cover Saxon bug 3805: effect of calling sequence-level methods after
 * an initial call of next().
 */
public class SaxonXQSequenceTest {
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
  public void testGetSequenceAsStream() throws XQException {
    XQExpression xqe;
    XQSequence xqs;
    XMLStreamReader xmlReader = null;

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    assertTrue(xqs.next(), "A-XQS-EXTRA-1: next() returned false");
    try {
      xmlReader = xqs.getSequenceAsStream();
    } catch (XQException e) {
      fail("A-XQS-22.1: getSequenceAsStream failed with message: " + e.getMessage());
    }
    try {
      assertNotNull(xmlReader, "A-XQS-22.1: getSequenceAsStream returned a null XMLStreamReader");
      assertEquals(XMLStreamReader.START_DOCUMENT, xmlReader.getEventType(), "A-XQS-22.1: unexpected first event returned by XMLStreamReader");
      assertEquals(XMLStreamReader.START_ELEMENT, xmlReader.next(), "A-XQS-22.1: unexpected second event returned by XMLStreamReader");
      assertEquals(XMLStreamReader.CHARACTERS, xmlReader.next(), "A-XQS-22.1: unexpected third event returned by XMLStreamReader");
      assertEquals(XMLStreamReader.END_ELEMENT, xmlReader.next(), "A-XQS-22.1: unexpected fourth event returned by XMLStreamReader");
      assertEquals(XMLStreamReader.END_DOCUMENT, xmlReader.next(), "A-XQS-22.1: unexpected fifth event returned by XMLStreamReader");
      try {
        int last = xmlReader.next();
        fail("A-XQS-22.1: XMLStreamReader.next() returned " + last + " at end");
      } catch (NoSuchElementException | IllegalStateException nsee) {
        // Javadoc is unclear which exception should be thrown. Raised as Saxon bug 3806, JDK bug ID:9055431
        // expected failure
      }
    } catch (XMLStreamException e) {
      fail("A-XQS-22.1: XMLStreamReader.next() failed with message: " + e.getMessage());
    }
    xqe.close();

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!<!--c--></e>");
    assertTrue(xqs.next(), "A-XQS-EXTRA-1: next() returned false");
    try {
      xmlReader = xqs.getSequenceAsStream();
    } catch (XQException e) {
      fail("A-XQS-22.1: getSequenceAsStream failed with message: " + e.getMessage());
    }
    try {
      assertNotNull(xmlReader, "A-XQS-22.1: getSequenceAsStream returned a null XMLStreamReader");
      assertEquals(XMLStreamReader.START_DOCUMENT, xmlReader.getEventType(),"A-XQS-22.1: unexpected first event returned by XMLStreamReader");
      assertEquals(XMLStreamReader.START_ELEMENT, xmlReader.next(), "A-XQS-22.1: unexpected second event returned by XMLStreamReader");
      assertEquals(XMLStreamReader.CHARACTERS, xmlReader.next(), "A-XQS-22.1: unexpected third event returned by XMLStreamReader");
      assertEquals(XMLStreamReader.COMMENT, xmlReader.next(), "A-XQS-22.1: unexpected next event returned by XMLStreamReader");
      assertEquals(XMLStreamReader.END_ELEMENT, xmlReader.next(), "A-XQS-22.1: unexpected fourth event returned by XMLStreamReader");
      assertEquals(XMLStreamReader.END_DOCUMENT, xmlReader.next(), "A-XQS-22.1: unexpected fifth event returned by XMLStreamReader");
      try {
        int last = xmlReader.next();
        fail("A-XQS-22.1: XMLStreamReader.next() returned " + last + " at end");
      } catch (NoSuchElementException | IllegalStateException nsee) {
        // Javadoc is unclear which exception should be thrown. Raised as Saxon bug 3806, JDK bug ID:9055431
        // expected failure
      }
    } catch (XMLStreamException e) {
      fail("A-XQS-22.1: XMLStreamReader.next() failed with message: " + e.getMessage());
    }
    xqe.close();

  }

  @Test
  void testGetSequenceAsString() throws XQException {
    XQExpression xqe;
    XQSequence xqs;

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    String result = null;
    assertTrue(xqs.next(), "A-XQS-EXTRA-1: next() returned false");
    try {
      result = xqs.getSequenceAsString(new Properties());
    } catch (XQException e) {
      fail("A-XQS-23.1: getSequenceAsString failed with message: " + e.getMessage());
    }
    assertTrue(result.indexOf("<e>Hello world!</e>") != -1, "A-XQS-23.1: Expects serialized result contains '<e>Hello world!</e>', but it is '" + result + "'");
    xqe.close();

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    assertTrue(xqs.next(), "A-XQS-EXTRA-1: next() returned false");
    try {
      assertEquals(result, xqs.getSequenceAsString(null), "A-XQS-23.2: null properties argument is equivalent to empty properties argument");
    } catch (XQException e) {
      fail("A-XQS-23.2: getSequenceAsString failed with message: " + e.getMessage());
    }
    xqe.close();

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("()");
    assertFalse(xqs.next(), "A-XQS-EXTRA-1: next() returned true");
    try {
      assertEquals("", xqs.getSequenceAsString(null), "A-XQS-23.2: null properties argument is equivalent to empty properties argument");
    } catch (XQException e) {
      fail("A-XQS-23.2: getSequenceAsString failed with message: " + e.getMessage());
    }
    xqe.close();

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>, <e>Goodbye Mars!</e>");
    assertTrue(xqs.next(), "A-XQS-EXTRA-1: next() returned false");
    assertTrue(xqs.next(), "A-XQS-EXTRA-2: next() returned false");
    try {
      assertTrue(xqs.getSequenceAsString(null).contains("<e>Goodbye Mars!</e>"), "A-XQS-23.2: wrong content");
    } catch (XQException e) {
      fail("A-XQS-23.2: getSequenceAsString failed with message: " + e.getMessage());
    }
    xqe.close();

  }


  @Test
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
    assertTrue(xqs.next(), "A-XQS-EXTRA-1: next() returned false");
    try {
      xqs.writeSequence(result, prop);
    } catch (XQException e) {
      fail("A-XQS-24.1: writeSequence failed with message: " + e.getMessage());
    }
    assertTrue(result.toString("UTF-8").indexOf("<e>Hello world!</e>") != -1, "A-XQS-24.1: Expects serialized result contains '<e>Hello world!</e>', but it is '" + result.toString("UTF-8") + "'");
    xqe.close();

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    ByteArrayOutputStream otherResult = new ByteArrayOutputStream();
    assertTrue(xqs.next(), "A-XQS-EXTRA-1: next() returned false");
    try {
      xqs.writeSequence(otherResult, prop);
    } catch (XQException e) {
      fail("A-XQS-24.2: writeSequence failed with message: " + e.getMessage());
    }
    assertEquals(result.toString("UTF-8"), otherResult.toString("UTF-8"), "A-XQS-24.2: null properties argument is equivalent to empty properties argument");
    xqe.close();

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("()");
    otherResult = new ByteArrayOutputStream();
    assertFalse(xqs.next(), "A-XQS-EXTRA-1: next() returned true");
    try {
      xqs.writeSequence(otherResult, prop);
    } catch (XQException e) {
      fail("A-XQS-24.2: writeSequence failed with message: " + e.getMessage());
    }
    assertEquals(new ByteArrayOutputStream().toString("UTF-8"), otherResult.toString("UTF-8"), "A-XQS-24.2: null properties argument is equivalent to empty properties argument");
    xqe.close();
  }

  @Test
  public void testWriteSequence_Writer() throws XQException {
    XQExpression xqe;
    XQSequence xqs;

    Properties prop = new Properties();
    prop.setProperty("encoding", "UTF-8");


    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    StringWriter result = new StringWriter();
    assertTrue(xqs.next(), "A-XQS-EXTRA-1: next() returned false");
    try {
      xqs.writeSequence(result, prop);
    } catch (XQException e) {
      fail("A-XQS-24.1: writeSequence failed with message: " + e.getMessage());
    }
    assertTrue(result.toString().indexOf("<e>Hello world!</e>") != -1, "A-XQS-24.1: Expects serialized result contains '<e>Hello world!</e>', but it is '" + result.toString() + "'");
    xqe.close();

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    StringWriter otherResult = new StringWriter();
    assertTrue(xqs.next(), "A-XQS-EXTRA-1: next() returned false");
    try {
      xqs.writeSequence(otherResult, prop);
    } catch (XQException e) {
      fail("A-XQS-24.2: writeSequence failed with message: " + e.getMessage());
    }
    assertEquals(result.toString(), otherResult.toString(), "A-XQS-24.2: null properties argument is equivalent to empty properties argument");
    xqe.close();

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("()");
    otherResult = new StringWriter();
    assertFalse(xqs.next(), "A-XQS-EXTRA-1: next() returned true");
    try {
      xqs.writeSequence(otherResult, prop);
    } catch (XQException e) {
      fail("A-XQS-24.2: writeSequence failed with message: " + e.getMessage());
    }
    assertEquals("", otherResult.toString(), "A-XQS-24.2: null properties argument is equivalent to empty properties argument");
    xqe.close();
  }

  @Test
  public void testWriteSequenceToSAX() throws XQException {
    XQExpression xqe;
    XQSequence xqs;

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    TestContentHandler result = new TestContentHandler();
    assertTrue(xqs.next(), "A-XQS-EXTRA-1: next() returned false");
    try {
      xqs.writeSequenceToSAX(result);
    } catch (XQException e) {
      fail("A-XQS-24.1: writeSequence failed with message: " + e.getMessage());
    }
    assertTrue(result.buffer.toString().indexOf("<e>Hello world!</e>") != -1, "A-XQS-24.1: Expects serialized result contains '<e>Hello world!</e>', but it is '" + result.buffer.toString() + "'");
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
    assertTrue(xqs.next(), "A-XQS-EXTRA-1: next() returned false");
    try {
      xqs.writeSequenceToResult(new StreamResult(new StringWriter()));
      fail("A-XQS-21.1: serialization process fails when sequence contains a top-level attribute");
    } catch (XQException xq) {
      // Expect an XQException
    }
    xqe.close();


    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("<e>Hello world!</e>");
    StringWriter result = new StringWriter();
    assertTrue(xqs.next(), "A-XQS-EXTRA-1: next() returned false");
    try {
      xqs.writeSequenceToResult(new StreamResult(result));
    } catch (XQException e) {
      fail("A-XQS-24.1: writeSequence failed with message: " + e.getMessage());
    }
    assertTrue(result.toString().indexOf("<e>Hello world!</e>") != -1, "A-XQS-24.1: Expects serialized result contains '<e>Hello world!</e>', but it is '" + result.toString() + "'");
    xqe.close();

    xqe = xqc.createExpression();
    xqs = xqe.executeQuery("()");
    result = new StringWriter();
    assertFalse(xqs.next(), "A-XQS-EXTRA-1: next() returned true");
    try {
      xqs.writeSequenceToResult(new StreamResult(result));
    } catch (XQException e) {
      fail("A-XQS-24.1: writeSequence failed with message: " + e.getMessage());
    }
    assertTrue(result.toString().equals(""), "A-XQS-24.1: Expects serialized result to be empty, but it is '" + result.toString() + "'");
    xqe.close();
  }
}
