
import java.io.StringWriter;

import javax.xml.transform.Result;
import javax.xml.transform.Source;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.stream.StreamResult;
import javax.xml.transform.stream.StreamSource;

public class Saxon
{
	private final String factory;

	public Saxon()
	{
		this("net.sf.saxon.TransformerFactoryImpl");
	}

	public Saxon(String factory)
	{
		this.factory = factory;
	}

	public String transformToString(String xslt, String source, String... params) throws Exception
	{
		StringWriter sw = new StringWriter();
		transform(xslt, source, new StreamResult(sw), params);
		return sw.toString();	
	}

	public void transformToStdout(String xslt, String source, String... params) throws Exception
	{
		transform(xslt, source, new StreamResult(System.out), params);
	}

	public void transform(String xslt, String source, Result result, String... params) throws Exception
	{
		TransformerFactory transformerFactory = TransformerFactory.newInstance(factory, Saxon.class.getClassLoader());
		//		transformerFactory.setFeature(FeatureKeys.TIMING, true);

		Transformer transformer = transformerFactory.newTransformer(getSource(xslt));
		for (int i = 0; i < params.length;)
		{
			String name = params[i++];
			String value = params[i++];
			transformer.setParameter(name, value);
		}

		long t0 = System.currentTimeMillis();

		transformer.transform(getSource(source), result);

		System.out.println();
		System.out.println("transform done in " + ((System.currentTimeMillis() - t0) / 1000) + "s");
	}

	private Source getSource(String source)
	{
		if (!source.startsWith("/"))
		{
			source = "/" + source;
		}
		return new StreamSource(Saxon.class.getResource(source).toExternalForm());
	}
	
	public static void main(String[] args) throws Exception {
		new Saxon("com.saxonica.config.ProfessionalTransformerFactory").transform("bug.xslt", "bug.xslt", new StreamResult(System.out), "myparam", "london");
	}
}
