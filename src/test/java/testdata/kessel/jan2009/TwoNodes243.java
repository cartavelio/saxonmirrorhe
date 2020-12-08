

import com.ddtek.xmlconverter.ConverterFactory;
import com.ddtek.xmlconverter.ConverterResolver;
import java.io.*;
import java.net.URI;
import java.net.URISyntaxException;
import java.util.Properties;
import javax.xml.namespace.QName;
import javax.xml.transform.stream.StreamSource;
import javax.xml.xquery.*;
import net.sf.saxon.Configuration;
import net.sf.saxon.value.Whitespace;
import net.sf.saxon.xqj.SaxonXQDataSource;

// This file was automatically generated from:
// file:///C:/sty/testing/Pipeline/AutoGenerate/autogen/TwoNodes243.pipeline
//
// The following directories and jars must be in your classpath:
// C:/sty/testing/Pipeline/AutoGenerate/
// C:/sty/struzzo/Components/XML Converters for Java/lib/XMLConverters.jar
// C:/sty/Struzzo/Debug/saxonsa.jar
// C:/sty/struzzo/Components/XML Converters for Java/lib/codehaus/wstx-asl.jar
// C:/sty/struzzo/Components/XML Converters for Java/lib/codehaus/jsr173_1.0_api.jar

public class TwoNodes243 {

	public static void main(String[] args) throws Exception {
		TwoNodes243 thePipeline = new TwoNodes243();
		thePipeline.run();
	}

	public void run() throws Exception {



		ConverterFactory  cFactory           = new ConverterFactory();
		ConverterResolver resolver           = cFactory.newResolver();

		// Declarations for Saxon XQJ interface.
		Reader            xqueryReader       = null;
		XQConnection      xqconnection       = null;

		// Declarations for node XQueryOne_2
		final String      XQueryOne_2_uri    = "file:///c:/dev/PipelineAutogen/OneXMLParm.xquery";
		XQPreparedExpression XQueryOne_2_xqExpr = null;

		// Declarations for node XQueryOne
		final String      XQueryOne_uri      = "file:///c:/dev/PipelineAutogen/OneXMLParm.xquery";
		XQPreparedExpression XQueryOne_xqExpr   = null;
		String            XQueryOne_XMLparm_path= "file:///c:/dev/PipelineAutogen/DefaultInput.fo.xml";
		String            output_uri         = "file:///c:/dev/PipelineAutogen/Output244.fo.xml";
		OutputStream      output_strm        = null;

		try {

			// setup common to all XQuery nodes.
			Configuration config = new Configuration();
			config.getSystemURIResolver().setRecognizeQueryParameters(true);
			config.setValidation(false);
			config.setStripsWhiteSpace(Whitespace.IGNORABLE);
			config.setURIResolver(resolver);
			XQDataSource  dataSource   = new SaxonXQDataSource(config);
			xqconnection = dataSource.getConnection();
			XQStaticContext context = xqconnection.getStaticContext();
			context.setBindingMode(XQConstants.BINDING_MODE_DEFERRED);
			xqconnection.setStaticContext(context);

			System.out.println("Running node XQueryOne");
			xqueryReader = new FileReader(new File(new URI(XQueryOne_uri)));
			XQueryOne_xqExpr = xqconnection.prepareExpression(xqueryReader);
			XQueryOne_xqExpr.bindDocument(new QName("XMLparm"), new StreamSource(XQueryOne_XMLparm_path), null);
			XQSequence XQueryOne_result = XQueryOne_xqExpr.executeQuery();
			Properties XQueryOne_2_p = new Properties();
			XQueryOne_2_p.setProperty("omit-xml-declaration", "yes");

			// Create a stream to write the output from node XQueryOne_2 to a file.
			output_strm = new FileOutputStream(getPath(output_uri));




			System.out.println("Running node XQueryOne_2");
			xqueryReader = new FileReader(new File(new URI(XQueryOne_2_uri)));
			XQueryOne_2_xqExpr = xqconnection.prepareExpression(xqueryReader);
			XQueryOne_2_xqExpr.bindDocument(new QName("XMLparm"), XQueryOne_result.getSequenceAsStream(), null);

			XQueryOne_2_xqExpr.executeQuery().writeSequence(output_strm, XQueryOne_2_p);
			output_strm.close();

		} finally {
			if (xqueryReader != null) xqueryReader.close();
			if (xqconnection != null) xqconnection.close();
			if (XQueryOne_2_xqExpr != null)     XQueryOne_2_xqExpr.close();
			if (XQueryOne_xqExpr != null)     XQueryOne_xqExpr.close();
			if (output_strm != null) output_strm.close();
		}
	}

	// Utility routine to extract a pathname from a file: uri
	private static String getPath(String uri) throws URISyntaxException {
		return new URI(uri).getSchemeSpecificPart();
	}

}
