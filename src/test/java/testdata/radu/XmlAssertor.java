package com.bofa.crme.xmlappliance.test;

import java.io.File;
import java.io.StringReader;
import java.util.HashMap;

import javax.xml.transform.stream.StreamSource;

import net.sf.saxon.s9api.Processor;
import net.sf.saxon.s9api.SaxonApiException;
import net.sf.saxon.s9api.XdmDestination;
import net.sf.saxon.s9api.XdmNode;
import net.sf.saxon.s9api.XsltCompiler;
import net.sf.saxon.s9api.XsltExecutable;
import net.sf.saxon.s9api.XsltTransformer;

public class XmlAssertor{
	public static final String SUCCESS = "SUCCESS";
	private Processor proc = new Processor(false);
	private XsltCompiler xsltCompiler = proc.newXsltCompiler();
	private HashMap<String, XsltTransformer> mapTransformers = new HashMap<String, XsltTransformer>();

	private XsltExecutable xslSchematronISOTextXSLTExec = null;
	private XsltTransformer xslSchematronISOTextTransformer = null;
	
	public XmlAssertor(String sISOSchematronFilePath){
		try {
			xslSchematronISOTextXSLTExec = xsltCompiler.compile(new StreamSource(new File(sISOSchematronFilePath)));
		} catch (SaxonApiException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		xslSchematronISOTextTransformer = xslSchematronISOTextXSLTExec.load();
	}

	private XsltTransformer getAssertionTransformer(String sSchematronFilePath) {
		XsltTransformer assertionTransformer = (XsltTransformer) mapTransformers.get(sSchematronFilePath);
		
		// lazily generate the assertion xsl transformer for corresponding schematron file
		if(assertionTransformer == null){
			try {
				XdmNode nodeSchematronRules = proc.newDocumentBuilder().build(new StreamSource(new File(sSchematronFilePath)));

				XdmDestination nodeStepOneResults = null;
				xslSchematronISOTextTransformer.setInitialContextNode(nodeSchematronRules);
				nodeStepOneResults = new XdmDestination();
				xslSchematronISOTextTransformer.setDestination(nodeStepOneResults);
				xslSchematronISOTextTransformer.transform();
				
//				System.out.println("nodeStepOneResults are: \n" + nodeStepOneResults.getXdmNode());

				XsltExecutable xslCompiledSchematronXSLTExec = xsltCompiler.compile(nodeStepOneResults.getXdmNode().asSource());
				assertionTransformer = xslCompiledSchematronXSLTExec.load();

				// store the transformer for later use
				mapTransformers.put(sSchematronFilePath, assertionTransformer);
			} catch (SaxonApiException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
		
		return assertionTransformer;
	}
	
	public String assertXMLDocument(String sXMLToBeAsserted, String sSchematronFilePath) {
		String sValidationResults = null;
		try {
			XsltTransformer assertionTransformer = getAssertionTransformer(sSchematronFilePath);

			XdmNode nodeXMLtoBeAsserted = proc.newDocumentBuilder().build(new StreamSource(new StringReader(sXMLToBeAsserted)));
			assertionTransformer.setInitialContextNode(nodeXMLtoBeAsserted);
			XdmDestination nodeStepTwoResults = new XdmDestination();
			assertionTransformer.setDestination(nodeStepTwoResults);
			assertionTransformer.transform();

		if (nodeStepTwoResults.getXdmNode() != null && nodeStepTwoResults.getXdmNode().size() > 0)
			// schematron rule checks triggered errors - pass them to the caller.
			sValidationResults = nodeStepTwoResults.getXdmNode().itemAt(0).getStringValue();
		else
			// all schematron rules passed. 
			sValidationResults = SUCCESS;

		} catch (SaxonApiException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	
		return sValidationResults;

	}

 }
