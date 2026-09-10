using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System.Text;
using Saxon.Api;
using NUnit.Framework;
using System.Xml;
using Saxon.Hej.trace;
using Processor = Saxon.Api.Processor;
using SchemaManager = Saxon.Api.SchemaManager;
using SchemaValidator = Saxon.Api.SchemaValidator;
using XdmDestination = Saxon.Api.XdmDestination;

namespace SaxonCS_NUnit {
    internal class TestSchemaValidator {

        [Ignore("Uses files on OND machine?")]
        public void TestSimpleValidation() {
            UriBuilder ub = new UriBuilder();
            ub.Scheme = "file";
            ub.Host = "";
            ub.Path = @"C:\work\tests\";
            Uri baseUri = ub.Uri;

            Processor saxon = new Processor(true);
            //saxon.SetProperty("http://saxon.sf.net/feature/streamability", "standard");

            SchemaManager manager = saxon.SchemaManager;
            List<Error> errors = new();
            manager.ErrorReporter = errors.Add;

            manager.XsdVersion = "1.1";

            Uri schemaUri = new Uri(baseUri, "SAFTPT.xsd");

            try {
                manager.Compile(schemaUri);
            }
            catch (Exception ex) {
                Console.WriteLine("Schema compilation failed with " + errors.Count + " errors");
                foreach (Error error in errors) {
                    Console.WriteLine("At line " + error.Location.LineNumber + ": " + error.Message);
                }

                return;
            }

            Saxon.Api.SchemaValidator validator = manager.NewSchemaValidator();
            //Uri instanceUri = new Uri(baseUri, "SAFT-PT (500086826) (20160412 172334).xml");

            //10 gigas
            Stream fs = File.OpenRead(@"C:\work\tests\SAFT.xml");

            //300 megas
            //Stream fs = File.OpenRead("SAFT-PT (500086826) (20160412 172334).xml");

            List<ValidationFailure> ErrorList = new();
            validator.InvalidityListener = ErrorList.Add;

            Console.WriteLine("\nFile Validating file.. ");

            try {
                validator.Validate(fs, baseUri);
            }
            catch (Exception ex) {
                Console.WriteLine(ex.StackTrace);

                Console.WriteLine("\nInstance validation failed with " + ErrorList.Count + " errors");
                using (StreamWriter writetext = new StreamWriter(@"C:\work\tests\validate.txt")) {
                    foreach (ValidationFailure error in ErrorList) {
                        writetext.WriteLine("At line " + error.LineNumber + ": " + error.Message);
                    }

                    Console.WriteLine("\n\nException: {0}", ex.Message);
                }

            }

            Console.WriteLine("\nFile validation terminated!");
            Console.ReadLine();
        }

        [Ignore("Taking ages to fetch the schema?")]
        public void testSchemaForXslt30() {
            // This should use the resolver mechanism to load the schema locally
            try {
                Processor proc = new();
                SchemaManager schemaManager = proc.SchemaManager;
                //schemaManager.Compile(new Uri("https://www.w3.org/TR/xslt-30/schema-for-xslt30.xsd"));

                // DocumentBuilder builder = proc.NewDocumentBuilder();
                // XdmNode schema = builder.Build(new Uri("https://www.w3.org/TR/xslt-30/schema-for-xslt30.xsd"));
                // schemaManager.Compile(schema);

                XQueryCompiler compiler = proc.NewXQueryCompiler();
                XQueryEvaluator eval = compiler.Compile("doc('https://www.w3.org/TR/xslt-30/schema-for-xslt30.xsd')")
                    .Load();
                XdmNode schema = (XdmNode)eval.Evaluate();

                schemaManager.Compile(schema);
                SchemaValidator validator = schemaManager.NewSchemaValidator();
                validator.InvalidityListener = failure => Assert.Fail();
                validator.Validate(new Uri(ConfigTest.DataDirUri, "caffo/Transform2.xslt"));
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Assert.Fail();
            }
        }

        [Test]
        public void TestInvalidityHandler() {

            XmlReader xsd = XmlReader.Create(new StringReader(
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
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

            XmlReader source_xml = XmlReader.Create(new StringReader(
                "<?xml version='1.0'?><request><user_name>ed</user_name><password>sdsd</password><date1>a2009-01-01</date1><date2>b2009-01-01</date2></request>"));

            UriBuilder ub = new UriBuilder();
            ub.Scheme = "file";
            ub.Host = "";
            ub.Path = @"C:\work\tests\";
            Uri baseUri = ub.Uri;

            Processor saxon = new Processor(true);


            SchemaManager manager = saxon.SchemaManager;
            List<Error> errors = new();
            manager.ErrorReporter = errors.Add;
            manager.XsdVersion = "1.1";


            try {
                manager.Compile(xsd);
            }
            catch (Exception ex) {
                Console.WriteLine("Schema compilation failed with " + errors.Count + " errors");
                foreach (Error error in errors) {
                    Console.WriteLine("At line " + error.Location.LineNumber + ": " + error.Message);
                }

                return;
            }

            SchemaValidator validator = manager.NewSchemaValidator();
            validator.InvalidityHandler = new MyInvalidityHandler();

            Console.WriteLine("Validating file.. ");

            try {
                validator.Validate(source_xml);
            }
            catch (SaxonApiException ex) {
                Console.WriteLine(ex.StackTrace);

            }
        }

        public class MyInvalidityHandler : IInvalidityHandler {
            public XdmValue EndReporting() {
                return null;
            }

            public void ReportInvalidity(ValidationFailure i) {
                Console.WriteLine(i.Message);
            }

            public void StartReporting(string systemId) {

            }
        }







        [Test]
        public void testMakeWarningFatal() {

            // Attempt to promote the warning on an unresolved xsl:include to a fatal error. Bug 5478.

            string xsd = "<xsd:schema\n"
                         + "elementFormDefault='qualified'\n"
                         + "xmlns:xsd='http://www.w3.org/2001/XMLSchema'>\n"
                         + "<xsd:include schemaLocation='nonexistentA.xsd'/>\n"
                         + "<xsd:include schemaLocation='nonexistentB.xsd'/>\n"
                         + "</xsd:schema>";
            List<Error> errorList = new();
            try {
                Processor proc = new Processor(true);
                SchemaManager sm = proc.SchemaManager;
                sm.ErrorReporter = err => {
                    err.TerminationMessage = "Far too many errors";
                    errorList.Add(err);
                };

                sm.Compile(new StringReader(xsd));
                Assert.Fail("Validation errors not detected");

            }
            catch (SaxonApiException sapie) {
                Assert.AreEqual(1, errorList.Count());
                Assert.True(sapie.Message.Contains("Far too many errors"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void testMakeDuplicateErrorFatal() {

            // Attempt to promote the error on a duplicate declaration to a fatal error. Bug 5478.

            string xsd = "<xsd:schema\n"
                         + "elementFormDefault='qualified'\n"
                         + "xmlns:xsd='http://www.w3.org/2001/XMLSchema'>\n"
                         + "<xsd:element name='AAA'/>\n"
                         + "<xsd:element name='AAA'/>\n"
                         + "<xsd:element name='BBB'/>\n"
                         + "<xsd:element name='BBB'/>\n"
                         + "</xsd:schema>";
            List<Error> errorList = new();
            try {
                Processor proc = new Processor(true);
                SchemaManager sm = proc.SchemaManager;
                sm.ErrorReporter = err => {
                    err.TerminationMessage = "Far too many errors";
                    errorList.Add(err);
                };

                sm.Compile(new StringReader(xsd));
                Assert.Fail("Validation errors not detected");

            }
            catch (SaxonApiException sapie) {
                Assert.AreEqual(1, errorList.Count);
                Assert.True(sapie.Message.Contains("Far too many errors"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void TestSchemaForXslt30viaSchemaCompiler() {
            // This should use the resolver mechanism to load the schema locally
            try {
                Processor proc = new Processor(true);
                SchemaManager schemaManager = proc.SchemaManager;
                schemaManager.Compile(new Uri("https://www.w3.org/TR/xslt-30/schema-for-xslt30.xsd"));
                SchemaValidator validator = schemaManager.NewSchemaValidator();
                validator.Validate(new Uri(ConfigTest.DataDirUri + "books.xsl"));
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }
    }
    
    
    
    


}
