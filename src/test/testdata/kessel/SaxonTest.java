package test;

import java.io.FileInputStream;
import java.io.FileOutputStream;

import net.sf.saxon.TransformerFactoryImpl;

import org.xml.sax.ErrorHandler;
import org.xml.sax.SAXException;
import org.xml.sax.SAXParseException;

import javax.xml.transform.Transformer;
import javax.xml.transform.sax.SAXResult;
import javax.xml.transform.sax.SAXTransformerFactory;
import javax.xml.transform.sax.TransformerHandler;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import javax.xml.validation.Schema;
import javax.xml.validation.SchemaFactory;
import javax.xml.validation.ValidatorHandler;

/*
 * Created on Jun 28, 2006
 * @author Clyde Kessel
 * Copyright (c) 2006 Progress Software Corporation and Data Direct Corportation.
 */

public class SaxonTest implements ErrorHandler {

	/**
	 * @param args
	 */
	public static void main(String[] args) throws Exception {
		SaxonTest test = new SaxonTest();
		test.run();
		
	}
	
	public void run() throws Exception {
		
		SAXTransformerFactory tFactory = new TransformerFactoryImpl();
		TransformerHandler resultTransformer = tFactory.newTransformerHandler();
		StreamResult finalResult = new StreamResult(new FileOutputStream("validated.xml"));
		resultTransformer.setResult(finalResult);
		
		SchemaFactory sFactory = new com.saxonica.ee.jaxp.SchemaFactoryImpl();
		StreamSource schemaSource = new StreamSource(new FileInputStream("html.xsd"));
		Schema schema = sFactory.newSchema(schemaSource);
		ValidatorHandler handler = schema.newValidatorHandler();	
		handler.setContentHandler(resultTransformer);
		
		handler.setErrorHandler(this);
		SAXResult result = new SAXResult(handler);
		StreamSource dataSource = new StreamSource(new FileInputStream("fo.xml"));
		Transformer transformer = tFactory.newTransformer();
		transformer.transform(dataSource, result);
	}

	public void warning(SAXParseException exception) throws SAXException {
		System.out.println("ErrorHandler.warning: " + exception);
		throw exception;
	}

	public void error(SAXParseException exception) throws SAXException {
		System.out.println("ErrorHandler.warning: " + exception);
		throw exception;
	}

	public void fatalError(SAXParseException exception) throws SAXException {
		System.out.println("ErrorHandler.warning: " + exception);
		throw exception;
	}

}
