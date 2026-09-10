package millicom;

import net.sf.saxon.Version;
import net.sf.saxon.s9api.*;
import net.sf.saxon.value.DateTimeValue;
import org.xml.sax.InputSource;

import javax.xml.stream.XMLStreamException;
import javax.xml.stream.XMLStreamWriter;
import javax.xml.transform.ErrorListener;
import javax.xml.transform.sax.SAXSource;
import javax.xml.transform.stream.StreamSource;
import java.io.*;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.List;
import java.text.SimpleDateFormat;
import java.util.Date;

/**
 * Created by O'Neil Delpratt, Saxonica
 * Date: 08/06/15
 */
public class MillicomTest {

    private Processor proc;

    private int errors = 0;
    private int warnings = 0;
    private String xsdVersion = "1.0";
    private DateTimeValue start;
    private DateTimeValue end;
    String xsd = null;
    boolean verbose = false;

    MillicomTest(String[] args) throws IOException, SaxonApiException {
        if (args.length < 2) {
            System.err.println("Error - Command: MillicomTest -xsd:xsd-file -s:xml-file [options]\n Options: -xsdversion:[1.0|1.1], -verbose");
            return;
        }

        List <String>xmlFiles = new ArrayList<String>();


        for (int i = 0; i < args.length; i++)
		{
            if(args[i].startsWith("-xsd:"))
			{
                xsd = args[i].substring(5);
				xsd = xsd.replace('\\', '/');
            }
            else if (args[i].startsWith("-xsdversion:"))
			{
                xsdVersion = args[i].substring(12);
            }
			else if (args[i].startsWith("-s:"))
			{
				String value = args[i].substring(3);
				value = value.replace('\\', '/');
				if(value.contains(";"))
				{
				   xmlFiles = Arrays.asList(value.split(";"));
				}
				else
				{
					xmlFiles.add(args[i].substring(3));
				}

            }
			else if (args[i].startsWith("-verbose"))
			{
                verbose = true;
            }
			else
			{
                System.out.println("command line option not recognized:" + args[i]);
            }


        }
        if (xmlFiles.size() == 0) {
            System.out.println("Error: XML file must be specified");
            System.exit(0);
        }
        start = getDateTime();
        if (verbose) {
            System.out.println("Running with " + Version.getProductName() + ", " + Version.getProductVersion() + ", " + Version.getProductVendor());
            System.out.println("Start time: " + start.getUnicodeStringValue());
        }
        FileOutputStream out = null;


        proc = new Processor(true);
        proc.setConfigurationProperty("http://saxon.sf.net/feature/validation-warnings", "true");
        proc.setConfigurationProperty("http://saxon.sf.net/feature/xsd-version", xsdVersion);
        SchemaManager sm = proc.getSchemaManager();
        StreamSource source_xsd = new StreamSource(new FileReader(new File(xsd)));
        try {

            sm.load(source_xsd);
        } catch (SaxonApiException e) {
            e.printStackTrace();  //To change body of catch statement use File | Settings | File Templates.
        }


        class MyThread {
            SchemaManager sm = null;
            String xmlFile = null;

            public MyThread(SchemaManager sm, String xmlFile) {
                this.sm = sm;
                this.xmlFile = xmlFile;
            }

            public void run() {
				SimpleDateFormat formatter = new SimpleDateFormat("yyyy-MM-dd'T'HH-mm-ss");
				Date now = new Date();
                String outFile = xmlFile.substring(0, xmlFile.indexOf('.')) + "_RECEIPT_" + formatter.format(now) + ".xml";
				outFile = outFile.replace('\\', '/');
                FileOutputStream out = null;
                XMLStreamWriter writer = null;
                try {
                    out= new FileOutputStream(outFile + "_temp");
                } catch (FileNotFoundException e) {
                    e.printStackTrace();
                }

                SchemaValidator sv = sm.newSchemaValidator();
                ErrorListener listener = null;


                Serializer serializer = proc.newSerializer();
                serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
                serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
//				serializer.setOutputProperty(Serializer.Property.SAXON_INDENT_SPACES, "1");
                serializer.setOutputStream(out);
                try {
                    writer = serializer.getXMLStreamWriter();
                } catch (SaxonApiException e) {
                    e.printStackTrace();
                }
                try {
                    writer.writeStartDocument();
                    writer.writeStartElement("messages");
                } catch (XMLStreamException e) {
                    e.printStackTrace();  //To change body of catch statement use File | Settings | File Templates.
                }
                listener = new MyErrorListener(writer);
               // sv.setErrorReporter(listener);


                StreamSource source = new StreamSource((new File(xmlFile)).toURI().toString());


                try {
                    sv.validate(source);
                } catch (SaxonApiException e) {
                    e.printStackTrace();  //To change body of catch statement use File | Settings | File Templates.
                }


                if (outFile != null) {
                    try {
                        writer.writeEndElement();//</message>
                        writer.writeEndDocument();
                        writer.flush();
                        writer.close();
                    } catch (XMLStreamException e) {
                    } finally {
                        try {
                            out.flush();
                            out.close();
                        } catch (IOException e) {
                            e.printStackTrace();  //To change body of catch statement use File | Settings | File Templates.
                        }


                    }

                    end = getDateTime();
                    errors = ((MyErrorListener) listener).getErrorCount();
                    warnings = ((MyErrorListener) listener).getWarningCount();
                    addExtraInfo2ValidationOutput(xmlFile, outFile, (errors <= 0 && warnings <= 0));

					// delete temporary file
                    File file = new File(outFile + "_temp");
                    try
					{
						boolean success = file.delete();
						if (verbose)
						{
						   if (success)
							  System.out.println(file.getName() + " has been deleted!");
						   else
							  System.out.println("File " + file.getName() + " failed to delete.");
						}
					}
					catch (SecurityException e)
					{
						if (verbose)
						{
						   System.err.println(e);
						}
					}
                }

				if (verbose)
				{
					 System.out.println("End time: "+ getDateTime().getUnicodeStringValue());
					 System.out.println("Validation process completed");
				}
            }

        }

        for(String xmlFile : xmlFiles){
            MyThread thread = new MyThread(sm, xmlFile);
            thread.run();
        }





    }

    public static DateTimeValue getDateTime() {
        return DateTimeValue.getCurrentDateTime(null);
    }


    public boolean addExtraInfo2ValidationOutput(String xmlFile, String outputFilename, boolean result) {

        try {
            XsltCompiler compiler = proc.newXsltCompiler();

            StringReader sr = new StringReader("<xsl:stylesheet version=\"3.0\" xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\">\n" +
                    "<xsl:output omit-xml-declaration=\"yes\"/>\n" +
                    "<xsl:template name=\"main\">" +
                    "<root>" +
                    "<metadata>" +
                    "<result>" + (result ? "true" : "false") + "</result>\n" +
                    "<errors>" + errors + "</errors>\n" +
                    "<warnings>" + warnings + "</warnings>" +
                    "<startTime>" + start.getUnicodeStringValue() + "</startTime>\n" +
                    "<endTime>" + end.getUnicodeStringValue() + "</endTime>\n" +
                    "<xmlFile>" + xmlFile.replace('/', '\\') + "</xmlFile>\n" +
                    "<xsdFile>" + xsd.replace('/', '\\') + "</xsdFile>\n" +
                    "<xsdProcessor>" + Version.getProductName() + " " + Version.getProductVersion() + ", " + proc.getSaxonEdition() + "-Java, " + Version.getProductVendor() + "</xsdProcessor>\n" +
                    "<xsdVersion>" + xsdVersion + "</xsdVersion>" +
                    "</metadata>" +
                    "<xsl:stream href='" + outputFilename + "_temp'>" +
                    "<xsl:copy-of select='.'/>" +
                    "</xsl:stream>" +
                    "</root>" +
                    "</xsl:template>\n" +
                    "</xsl:stylesheet>");
            SAXSource style = new SAXSource(new InputSource(sr));
            XsltTransformer trans = compiler.compile(style).load();

            FileOutputStream out = new FileOutputStream(outputFilename);

            Serializer serializer = proc.newSerializer();
            serializer.setOutputProperty(Serializer.Property.INDENT, "yes");
            serializer.setOutputProperty(Serializer.Property.OMIT_XML_DECLARATION, "yes");
			serializer.setOutputProperty(Serializer.Property.SAXON_INDENT_SPACES, "2");
            serializer.setOutputStream(out);
            XMLStreamWriter writer = serializer.getXMLStreamWriter();


            trans.setDestination(serializer);
            trans.setInitialTemplate(new QName("main"));
            trans.transform();

			try{
				out.flush();
				out.close();
			}
			catch (IOException e) {
                 e.printStackTrace();
			}

            return true;

        } catch (SaxonApiException ex) {
            System.out.println("Error :" + ex.getMessage());
            return false;
        } catch (FileNotFoundException ex) {
            System.out.println("Error :" + ex.getMessage());
            return false;
        }

    }


    public static void main(String[] args) throws IOException, SaxonApiException {

        MillicomTest test = new MillicomTest(args);

    }
}
