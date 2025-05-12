package s9apitest;

import net.sf.saxon.s9api.*;
import net.sf.saxon.trans.SaxonErrorCode;
import org.junit.jupiter.api.Test;

import javax.xml.transform.stream.StreamSource;
import java.io.BufferedReader;
import java.io.StringReader;

import static org.junit.jupiter.api.Assertions.fail;


public class WrongNamePoolTest {



	public String execute(String xmlPayLoad,String xQuery)
	{
		return executeXQuery(this.getNode(xmlPayLoad),xQuery);
	}

	private String executeXQuery(XdmNode node,String xQueryString)
	{
		StringBuilder resultBuff = new StringBuilder();
        Processor proc = new Processor(false);

        XQueryCompiler comp = proc.newXQueryCompiler();

        //compile the query
        XQueryExecutable exprExec = null;
		try {
			exprExec = comp.compile(xQueryString);
		} catch (SaxonApiException e) {
			fail("Query compilation should succeed");
		}
        XQueryEvaluator queryEvaluator = exprExec.load();
        queryEvaluator.setExternalVariable(new QName("params"), node);
        //evaluate the query
        XdmValue result;
		try {
			result = queryEvaluator.evaluate();
			int len = result.size();
			for(int i=0;i<len;i++)
			{
				resultBuff.append(result.itemAt(i));
			}
            fail("Error not detected");

		} catch (SaxonApiException e) {
			if (!SaxonErrorCode.SXXP0004.equals(e.getErrorCode().getLocalName())) {
                fail("Wrong error code");
            }
		}

		return resultBuff.toString();

	}
	private  XdmNode getNode(String xmlPayLoad)
	{
		//create processor
	    Processor proc = new Processor(false);
	    //create buffered reader for the xml payload
	    BufferedReader reader = new BufferedReader(new StringReader(xmlPayLoad));
        //create stream source
        StreamSource streamSource = new StreamSource(reader);
        //create document builder
        DocumentBuilder docBuilder = proc.newDocumentBuilder();
        //create node
        XdmNode node = null;
		try {
			node = docBuilder.build(streamSource);
		} catch (SaxonApiException e) {
			e.printStackTrace();
		}
         return node;
	}

    @Test
	public void testWrongNamePool() throws Exception
	{

        WrongNamePoolTest test = new WrongNamePoolTest();


        String xmlPayLoad = "<offerings><offering><id>1</id>" +
        "<allowances><allowance><id>33</id>" +
        "<productName>AllowanceName</productName>" +
        "</allowance></allowances>"+
        "<segments><segment><id>11</id><name>segmentName</name></segment></segments>" +
        "</offering></offerings>";


        StringBuilder queryBuilder = new StringBuilder();
        queryBuilder.append("declare variable $params external; ");
        queryBuilder.append("for $param in ");
        queryBuilder.append("$params/");
        queryBuilder.append("offerings/offering");
        queryBuilder.append(" ");
        queryBuilder.append("return $param/segments/segment")
        ;
        System.out.println("Xquery generated is:"+queryBuilder.toString());
        System.out.println("response is:"+test.execute(xmlPayLoad,queryBuilder.toString()));

    }
}

