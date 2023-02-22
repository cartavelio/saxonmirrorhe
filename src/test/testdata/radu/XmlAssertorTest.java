package com.bofa.crme.xmlappliance.test;

import junit.framework.TestCase;

public class XmlAssertorTest extends TestCase {

	protected void setUp() throws Exception {
		super.setUp();
	}

	protected void tearDown() throws Exception {
		super.tearDown();
	}
	
	public void testAssertXMLDocument() {
		XmlAssertor xmlAssertor = new XmlAssertor("iso_schematron_text.xsl");
		String sAssertionResult = xmlAssertor.assertXMLDocument(TestUtil.readFile("to_be_asserted.xml"), "legacygcib.sch");
		
		System.out.println("sAssertionResult = " + sAssertionResult);
		
		assertEquals(XmlAssertor.SUCCESS, sAssertionResult);
	}

}
