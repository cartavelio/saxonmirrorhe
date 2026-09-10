
import java.io.OutputStreamWriter;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;
import net.sf.saxon.lib.FeatureKeys;

// Run an XSLT
// The following directories and jars must be in your classpath:
// C:/base64/java
// C:/Program Files (x86)/Stylus Studio X14 XML Enterprise Suite/bin/saxonsa.jar
//

public class TextTobase64 {

	public static void main(String[] args) throws Exception {

		String    inputUri                = "file:///c:/base64/books.xml";
		String       xsltUrl    = "file:///c:/base64/TextTobase64.xsl";

		TransformerFactory tFactory = new net.sf.saxon.TransformerFactoryImpl();
		tFactory.setAttribute(FeatureKeys.RECOGNIZE_URI_QUERY_PARAMETERS, Boolean.TRUE);
		tFactory.setAttribute(FeatureKeys.STRIP_WHITESPACE, "ignorable");
		tFactory.setAttribute(FeatureKeys.VERSION_WARNING, Boolean.FALSE);

		Transformer transformer = tFactory.newTransformer(new StreamSource(xsltUrl));


		System.out.println();
		System.out.println("XSLT starting.");
		transformer.transform(new StreamSource(inputUri),
		                      new StreamResult(new OutputStreamWriter(System.out)));
		System.out.println();
		System.out.println("XSLT finished.");

	}
}
