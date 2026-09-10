
using System;
using System.IO;
using System.Xml;
using System.Xml.Xsl;
using DDTek.XmlConverter;
using Saxon.Api;
using net.sf.saxon;


public class NodeContext3 {

	const String inputUri  = "file:///c:/bug/samples/trax/xml/foo.xml";
	const String xsltUri   = "file:///c:/bug/NodeContext2.xslt";
	//const String xsltUri = "file:///c:/bug/samples/trax/xsl/foo.xsl";

	public static void Main(String[] args) {

		try {

			// Create a Processor instance.
			Processor processor = new Processor();

			// Load the source document
			XdmNode input = processor.NewDocumentBuilder().Build(new Uri(inputUri));

			// Create a compiler
			XsltCompiler compiler = processor.NewXsltCompiler();

			// Compile all three stylesheets
			XsltTransformer transformer1 = compiler.Compile(new Uri(xsltUri)).Load();

			// Now run them in series
			transformer1.InitialContextNode = input;

			// This code crashes with a NPE
			XdmDestination results1 = new XdmDestination();
			transformer1.Run(results1);
		
			
			// This code works OK
			//Serializer serializer = new Serializer();
			//serializer.SetOutputWriter(Console.Out);
			//transformer1.Run(serializer);


		} catch (Exception e) {
			Console.Error.WriteLine(e.ToString());
			// Console.Error.WriteLine(e.StackTrace);
		}
	}
}
