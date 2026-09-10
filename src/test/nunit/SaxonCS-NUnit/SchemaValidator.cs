using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System.Text;
using Saxon.Api;
using NUnit.Framework;
using System.Xml;
using net.sf.saxon.tree.wrapper;

namespace SaxonCS_NUnit
{
    class TestSchemaValidator
    {

        [Test]
        public void TestSimpleValidation()
        {
            UriBuilder ub = new UriBuilder();
            ub.Scheme = "file";
            ub.Host = "";
            ub.Path = @"C:\work\tests\";
            Uri baseUri = ub.Uri;

            Processor saxon = new Processor(true);
            //saxon.SetProperty("http://saxon.sf.net/feature/streamability", "standard");

            SchemaManager manager = saxon.SchemaManager;
            manager.ErrorList = new List<StaticError>();
          
            manager.XsdVersion = "1.1";

            Uri schemaUri = new Uri(baseUri, "SAFTPT.xsd");

            try
            {
                manager.Compile(schemaUri);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Schema compilation failed with " + manager.ErrorList.Count + " errors");
                foreach (StaticError error in manager.ErrorList)
                {
                    Console.WriteLine("At line " + error.LineNumber + ": " + error.Message);
                }
                return;
            }

            Saxon.Api.SchemaValidator validator = manager.NewSchemaValidator();
            //Uri instanceUri = new Uri(baseUri, "SAFT-PT (500086826) (20160412 172334).xml");

            //10 gigas
            Stream fs = File.OpenRead(@"C:\work\tests\SAFT.xml");

            //300 megas
            //Stream fs = File.OpenRead("SAFT-PT (500086826) (20160412 172334).xml");

            validator.SetSource(fs, baseUri);
            validator.ErrorList = new List<ValidationFailure>();

            Console.WriteLine("\nFile Validating file.. ");

            try
            {
                validator.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);

                Console.WriteLine("\nInstance validation failed with " + validator.ErrorList.Count + " errors");
                using (StreamWriter writetext = new StreamWriter(@"C:\work\tests\validate.txt"))
                {
                    foreach (ValidationFailure error in validator.ErrorList)
                    {
                        //Console.WriteLine("At line " + error.LineNumber + ": " + error.Message);
                        writetext.WriteLine("At line " + error.GetLineNumber() + ": " + error.GetMessage());
                    }

                    Console.WriteLine("\n\nException: {0}", ex.Message);
                }

            }

            Console.WriteLine("\nFile validation terminated!");
            Console.ReadLine();
        }

        [Test]
        public void TestInvalidityHandler()
        {

            XmlReader xsd = XmlReader.Create(new StringReader("<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
               "\t<xs:element name=\"request\">\n" +
               "\t\t<xs:complexType>\n" +
               "\t\t\t<xs:sequence>\n" +
               "\t\t\t\t<xs:element name=\"user_name\" type=\"xs:string\"/>\n" +
               "\t\t\t\t<xs:element name=\"password\" type=\"xs:string\"/>\n" +
               "\t\t\t\t<xs:element name=\"date1\" type=\"xs:date\"/>\n" +
               "\t\t\t\t<xs:element name=\"date2\" type=\"xs:date\"/>\n" +
               "\t\t\t</xs:sequence>\n" +
               "\t\t</xs:complexType>\n" +
               "\t</xs:element>\n" +
               "</xs:schema>"));

            XmlReader source_xml = XmlReader.Create(new StringReader("<?xml version='1.0'?><request><user_name>ed</user_name><password>sdsd</password><date1>a2009-01-01</date1><date2>b2009-01-01</date2></request>"));

            UriBuilder ub = new UriBuilder();
            ub.Scheme = "file";
            ub.Host = "";
            ub.Path = @"C:\work\tests\";
            Uri baseUri = ub.Uri;

            Processor saxon = new Processor(true);


            SchemaManager manager = saxon.SchemaManager;
            manager.ErrorList = new List<StaticError>();
            manager.XsdVersion = "1.1";


            try
            {
                manager.Compile(xsd);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Schema compilation failed with " + manager.ErrorList.Count + " errors");
                foreach (StaticError error in manager.ErrorList)
                {
                    Console.WriteLine("At line " + error.LineNumber + ": " + error.Message);
                }
                return;
            }

            SchemaValidator validator = manager.NewSchemaValidator();
            validator.SetInvalidityHandler(new MyInvalidaityHandler());

            

            validator.SetSource(source_xml);
//            validator.ErrorList = new ArrayList();

            Console.WriteLine("\nFile Validating file.. ");

            try
            {
                validator.Run();
            }
            catch (StaticError ex)
            {
                Console.WriteLine(ex.StackTrace);
               
            }
        }

        public class MyInvalidaityHandler : IInvalidityHandler
        {
            public XdmValue endReporting()
            {
                return null;
            }

            public void reportInvalidity(ValidationFailure i)
            {
                Console.WriteLine(i.GetMessage());
            }

            public void startReporting(string systemId)
            {
                
            }
        }

        //throws out of memory exception
        [Test]
        public void SchemaValdiatorPerf() {
            // Load a schema

            Processor processor = new Processor(true);
        processor.SetProperty("http://saxon.sf.net/feature/timing", "true");
            processor.SetProperty("http://saxon.sf.net/feature/validation-warnings", "false"); //Set to true to suppress the exception
            SchemaManager manager = processor.SchemaManager;
        manager.XsdVersion = "1.1";
            manager.ErrorList = new List<StaticError>();

            UriBuilder ub = new UriBuilder();
            ub.Scheme = "file";
            ub.Host = "";
            ub.Path = @"C:\work\repositories\AHSAMedicalAgreements-v0030z\";
            Uri baseUri = ub.Uri;

            
            Uri schemaUri = new Uri(baseUri, "AHSAMedicalAgreements-v0030z.xsd");

            try
            {
                manager.Compile(schemaUri);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Schema compilation failed with " + manager.ErrorList.Count + " errors");
                foreach (StaticError error in manager.ErrorList)
                {
                    Console.WriteLine("At line " + error.LineNumber + ": " + error.Message);
                }
                Assert.Fail();
            }


            // Use this to validate an instance document

            SchemaValidator validator = manager.NewSchemaValidator();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
            String inputFileName = new Uri(baseUri, "Providers-H-20200403 MASKED.xml").ToString();
            XmlReader xmlReader = XmlReader.Create(inputFileName, settings);
            validator.SetSource(xmlReader);
            Console.WriteLine("Validating input file " + inputFileName);
            validator.ErrorList = new List<ValidationFailure>();
            XdmDestination psvi = new XdmDestination();
            validator.SetDestination(psvi);

            try
            {
                validator.Run();
                net.sf.saxon.trace.Instrumentation.report();
            }
            catch (Exception e)
            {
                net.sf.saxon.trace.Instrumentation.report();
                Console.WriteLine(e);
                Console.WriteLine("Instance validation failed with " + validator.ErrorList.Count + " errors");
                foreach (ValidationFailure error in validator.ErrorList)
                {
                    Console.WriteLine("At line " + error.GetLineNumber() + ": " + error.GetMessage());
                }
                Assert.Fail();
            }




        }
    }
}
