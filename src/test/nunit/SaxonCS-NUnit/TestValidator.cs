using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using NUnit.Framework;
using Saxon.Api;
using Saxon.Callbacks;
using Saxon.Ejavax.xml.transform.stream;
using Saxon.Hej;
using Saxon.Hej.lib;
using Saxon.Hej.trace;
using Saxon.Hej.tree.tiny;
using SaxonCS_NUnit;
using DocumentBuilder = Saxon.Api.DocumentBuilder;
using Processor = Saxon.Api.Processor;
using SchemaManager = Saxon.Api.SchemaManager;
using SchemaValidator = Saxon.Api.SchemaValidator;
using Serializer = Saxon.Api.Serializer;
using TreeModel = Saxon.Api.TreeModel;
using XdmDestination = Saxon.Api.XdmDestination;
using XdmNodeKind = Saxon.Api.XdmNodeKind;
using XPathCompiler = Saxon.Api.XPathCompiler;
using XQueryCompiler = Saxon.Api.XQueryCompiler;
using XQueryEvaluator = Saxon.Api.XQueryEvaluator;
using Xslt30Transformer = Saxon.Api.Xslt30Transformer;
using XsltCompiler = Saxon.Api.XsltCompiler;

namespace SaxonCS_NUnit {
    internal class TestValidator {


        [Test]
        public void TestReportMultipleErrors() {

            // This validation run should report exactly two validation errors.

            string source_xml =
                "<?xml version='1.0'?><request><user_name>ed</user_name><password>sdsd</password><date1>a2009-01-01</date1><date2>b2009-01-01</date2></request>";
            string xsd =
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
                "</xs:schema>";

            List<ValidationFailure> errorList = new();
            using Stream output = File.Create(ConfigTest.DATA_DIR + "sandpit/validation.out");
            try {
                Processor proc = new Processor(true);
                SchemaManager sm = proc.SchemaManager;

                SchemaValidator sv = sm.NewSchemaValidator();
                sv.InvalidityListener = errorList.Add;
                sm.Compile(new StringReader(xsd));


                Serializer serializer = proc.NewSerializer();
                serializer.SetOutputProperty(Serializer.INDENT, "yes");
                serializer.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                serializer.OutputStream = output;
                sv.SetDestination(serializer);

                sv.Validate(new StringReader(source_xml));

                Assert.Fail("Validation errors not detected");

            }
            catch (SaxonApiException sapie) {
                Assert.AreEqual(2, errorList.Count);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
            finally {
                output.Close();
            }
        }

        /**
         * Test that multiple base URIs derived from a multiple-entity input document are
         * preserved through validation
         */

        [Ignore("Unresolved bug: 5107")]
        //[Test]
        public void TestValidationWithEntities() {
            try {
                Processor proc = new ();
                DocumentBuilder builder = proc.NewDocumentBuilder();
                XdmNode doc = builder.Build(new Uri(ConfigTest.DataDirUri, "book-wrapper.xml"));
                Assert.AreEqual(3, GetAllBaseUris(doc).Count, "3 distinct base URIs in input");
                Assert.That(doc.BaseUri.ToString().EndsWith("book-wrapper.xml"), "input doc base URI");

                SchemaManager mgr = proc.SchemaManager;
                mgr.Compile(new Uri(ConfigTest.DataDirUri, "books.xsd"));
                SchemaValidator v = mgr.NewSchemaValidator();
                XdmDestination dest = new();
                //dest.BaseUri = doc.BaseUri;
                v.SetDestination(dest);
                v.Validate(doc);

                XdmNode validDoc = dest.XdmNode;

                Assert.AreEqual(3, GetAllBaseUris(validDoc).Count, "3 distinct base URIs in output");
                Assert.That(validDoc.BaseUri.ToString().EndsWith("book-wrapper.xml"), "validated doc base URI");

            }
            catch (Exception e) {
                Debug.Print(e.StackTrace);
                Assert.Fail(e.Message);
            }
        }

        private static ISet<Uri> GetAllBaseUris(XdmNode root) {
            ISet<Uri> baseUris = new HashSet<Uri>();
            foreach (XdmNode node in root.Select(Steps.Descendant(Predicates.IsElement()))) {
                if (node.BaseUri != null) {
                    baseUris.Add(node.BaseUri);
                }
            }

            return baseUris;
        }


        [Test]
        public void TestMultipleSchemaImportsOn() {
            multipleSchemaImports(true);
        }

        [Test]
        public void TestMultipleSchemaImportsOff() {
            multipleSchemaImports(false);
        }

        private void multipleSchemaImports(bool on) {
            try {
                Processor p = new Processor(true);
                p.SetProperty(Saxon.Api.Feature.MULTIPLE_SCHEMA_IMPORTS, on);
                SchemaManager sm = p.SchemaManager;
                string xsd0 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\n" +
                              "<xs:import namespace='http://ns.example.com/' schemaLocation='file:///1'/>" +
                              "<xs:import namespace='http://ns.example.com/' schemaLocation='file:///2'/>" +
                              "</xs:schema>";
                string xsd1 =
                    "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" targetNamespace='http://ns.example.com/'>" +
                    "<xs:element name=\"a\"/>\n" +
                    "</xs:schema>";
                string xsd2 =
                    "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" targetNamespace='http://ns.example.com/'>" +
                    "<xs:element name=\"b\"/>\n" +
                    "</xs:schema>";

                sm.SchemaResolver = (ns, baseUri, hint) => {
                    if ("file:///1" == hint.ToString()) {
                        return new TextResource(xsd1, hint);
                    }

                    if ("file:///2" == hint.ToString()) {
                        return new TextResource(xsd2, hint);
                    }

                    return null;

                };

                sm.Compile(new StringReader(xsd0), new Uri("file:///0"));
                SchemaValidator sv = sm.NewSchemaValidator();
                string xml0 = "<a xmlns='http://ns.example.com/'/>";
                string xml1 = "<b xmlns='http://ns.example.com/'/>";
                sv.Validate(new StringReader(xml0));
                sv.Validate(new StringReader(xml1));
                if (!on) {
                    Assert.Fail("Validation should have failed");
                }
            }
            catch (SaxonApiException err) {
                if (on) {
                    Assert.Fail(err.Message);
                }
            }
        }

        // Test effect of multiple-schema-imports when A imports B, B imports C, and A and C have the same
        // target namespace

        [Test]
        public void TestCycleMultipleSchemaImportsOn() {
            cycleMultipleSchemaImports(true);
        }

        [Test]
        public void TestCycleMultipleSchemaImportsOff() {
            cycleMultipleSchemaImports(false);
        }

        private void cycleMultipleSchemaImports(bool on) {
            SchemaManager sm = null;
            try {
                Processor p = new Processor(true);
                p.SetProperty(Saxon.Api.Feature.MULTIPLE_SCHEMA_IMPORTS, on);
                sm = p.SchemaManager;
                string xsd0 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>\n" +
                              "<xs:import namespace='ns:b' schemaLocation='file:///1'/>" +
                              "<xs:element name='a'/>\n" +
                              "</xs:schema>";
                string xsd1 =
                    "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:b'>" +
                    "<xs:import namespace='ns:a' schemaLocation='file:///2'/>" +
                    "<xs:element name='b'/>\n" +
                    "</xs:schema>";
                string xsd2 =
                    "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='ns:a'>" +
                    "<xs:element name='c'/>\n" +
                    "</xs:schema>";
                
                sm.SchemaResolver = (ns, baseUri, hint) => {
                    if ("file:///1" == hint.ToString()) {
                        return new TextResource(xsd1, hint);
                    }

                    if ("file:///2" == hint.ToString()) {
                        return new TextResource(xsd2, hint);
                    }

                    return null;

                };

                sm.Compile(new StringReader(xsd0), new Uri("file:///0"));
            }
            catch (SaxonApiException err) {
                Assert.Fail(err.Message);
            }

            SchemaValidator sv = sm.NewSchemaValidator();
            string xml0 = "<a xmlns='ns:a'/>";
            string xml1 = "<b xmlns='ns:b'/>";
            string xml2 = "<c xmlns='ns:a'/>";
            try {
                sv.Validate(new StringReader(xml0));
            }
            catch (SaxonApiException err) {
                Assert.Fail(err.Message);
            }

            try {
                sv.Validate(new StringReader(xml1));
            }
            catch (SaxonApiException err) {
                Assert.Fail(err.Message);
            }

            try {
                sv.Validate(new StringReader(xml2));
                if (!on) {
                    Assert.Fail("Validation should have failed");
                }
            }
            catch (SaxonApiException err) {
                if (on) {
                    Assert.Fail("Validation should not have failed");
                }
            }
        }


        [Test]
        public void TestSchemaAwareQueryUsingSCM() {
            try {
                Processor proc = new Processor(true);
                XdmNode scm = proc.NewDocumentBuilder().Build(new Uri(ConfigTest.DataDirUri, "dita/concept.scm"));
                proc.SchemaManager.ImportComponents(scm);
                // define a ns prefix to be associated with a default attribute added to the instance during validation
                proc.Implementation.getNamePool()
                    .suggestPrefix("dita", "http://dita.oasis-open.org/architecture/2005/");
                XdmNode doc = proc.NewDocumentBuilder().Build(new Uri(ConfigTest.DataDirUri, "dita/concept.xml"));
                XQueryCompiler c = proc.NewXQueryCompiler();
                c.SchemaAware = (true);
                XQueryEvaluator e = c.Compile("validate{.}").Load();
                e.ContextItem = (doc);
                StringWriter sw = new StringWriter();
                Serializer s = proc.NewSerializer(sw);
                e.Run(s);
                Console.Error.WriteLine(sw);
                Assert.That(sw.ToString().Contains("xmlns:dita"));
                Assert.That(sw.ToString().Contains("dita:DITAArchVersion=\"1.2\""));
            }
            catch (SaxonApiException e) {
                //e.printStackTrace();
                Assert.Fail();
            }
        }

        [Test]
        public void TestImportUsingSchemaUriResolver() {
            try {
                Processor p = new Processor(true);
                SchemaManager sm = p.SchemaManager;
                sm.SchemaResolver = (targetNamespace, uri, hints) => {
                    if (targetNamespace == "http://two/") {
                        return new NodeResource(p.NewDocumentBuilder().Build(new StringReader(
                            "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" targetNamespace=\"http://two/\">\n" +
                            "    <xs:simpleType name='date'><xs:restriction base='xs:date'/></xs:simpleType>\n" +
                            "</xs:schema> "
                        )));
                    }
                    else {
                        return null;
                    }
                };

                string xsd0 = "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:b='http://two/'>\n" +
                              "<xs:import namespace='http://two/'/>" +
                              "<xs:element name='a' type='b:date'/>\n" +
                              "</xs:schema>";


                sm.Compile(new StringReader(xsd0), new Uri("file:///0"));
                SchemaValidator sv = sm.NewSchemaValidator();
                string xml0 = "<a>2010-11-02</a>";
                string xml1 = "<a>orange wednesday</a>";
                sv.Validate(new StringReader(xml0));
                try {
                    sv.Validate(new StringReader(xml1));
                    Assert.Fail();
                }
                catch (Exception ve) {
                    // OK
                }

            }
            catch (SaxonApiException err) {
                Assert.Fail();
                //err.printStackTrace();
            }
        }

        public class MyThing : StandardUnparsedTextResolver {
            public MyThing(Configuration config) : base(config) {
                
            }
        }

        [Test]
        public void TestAssertionsHidingComments() {

            // This validation run should report exactly two validation errors.

            string valid_xml = "<?xml version='1.0'?><request><a/>x<!--comment-->y<b/></request>";
            string invalid_xml = "<?xml version='1.0'?><request><a/>x<!--comment-->y</request>";
            string xsd =
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType mixed='true'>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t\t<xs:assert test=\"count(child::node()) = 3 and text()='xy'\"/>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";

            try {
                Processor proc = new Processor(true);
                proc.SetProperty(Saxon.Api.Feature.ASSERTIONS_CAN_SEE_COMMENTS, false);
                proc.SetProperty(Saxon.Api.Feature.XSD_VERSION, "1.1");
                SchemaManager sm = proc.SchemaManager;
                StreamSource source_xsd = new StreamSource(new StringReader(xsd));

                SchemaValidator sv = sm.NewSchemaValidator();
                sm.Compile(new StringReader(xsd));

                XdmDestination destination = new XdmDestination();
                sv.SetDestination(destination);

                sv.Validate(new StringReader(valid_xml));

                XPathCompiler xp = proc.NewXPathCompiler();
                xp.SchemaAware = true;
                XdmAtomicValue result = (XdmAtomicValue) xp.Evaluate("count(//comment())", destination.XdmNode);
                Assert.AreEqual(1, result.AsLong());

                try {
                    sv.Validate(new StringReader(invalid_xml));
                    Assert.Fail("Failed to spot errors");
                }
                catch (SaxonApiException er) {
                    Assert.That(er.Message.Contains("validation errors"));
                    // OK
                }
            }
            catch (SaxonApiException sapie) {
                //sapie.printStackTrace();
                Assert.Fail(sapie.Message);
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestAssertionsSeeingComments() {

            string valid_xml = "<?xml version='1.0'?><request><a/><!--comment--><b/></request>";
            string invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
            string xsd =
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t\t<xs:assert test='count(child::node()) = 3'/>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";

            try {
                Processor proc = new Processor(true);
                proc.SetProperty(Saxon.Api.Feature.ASSERTIONS_CAN_SEE_COMMENTS, true);
                proc.SetProperty(Saxon.Api.Feature.XSD_VERSION, "1.1");
                SchemaManager sm = proc.SchemaManager;

                sm.Compile(new StringReader(xsd));

                SchemaValidator sv = sm.NewSchemaValidator();
                XdmDestination destination = new XdmDestination();
                sv.SetDestination(destination);
                sv.Validate(new StringReader(valid_xml));

                XPathCompiler xp = proc.NewXPathCompiler();
                xp.SchemaAware = (true);
                XdmAtomicValue result = (XdmAtomicValue) xp.Evaluate("count(//comment())", destination.XdmNode);
                Assert.AreEqual(1, result.AsLong());

                try {
                    sv.Validate(new StringReader(invalid_xml));
                    Assert.Fail("Failed to spot errors");
                }
                catch (SaxonApiException er) {
                    // OK
                }
            }
            catch (SaxonApiException sapie) {
                Assert.Fail(sapie.Message);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        private void assertionUsingXPath3(bool allow) {

            string xsd =
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t\t<xs:assert test='child::node() => count() = 3'/>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";

            try {
                Processor proc = new Processor(true);
                proc.SetProperty(Saxon.Api.Feature.ASSERTIONS_CAN_SEE_COMMENTS, true);
                proc.SetProperty(Saxon.Api.Feature.XSD_VERSION, "1.1");
                proc.SetProperty(Saxon.Api.Feature.XPATH_VERSION_FOR_XSD, allow ? 31 : 20);
                SchemaManager sm = proc.SchemaManager;
                StreamSource source_xsd = new StreamSource(new StringReader(xsd));

                SchemaValidator sv = sm.NewSchemaValidator();
                try {
                    sm.Compile(new StringReader(xsd));
                    if (!allow) {
                        Assert.Fail("unexpected success");
                    }
                }
                catch (SaxonApiException e) {
                    if (allow) {
                        Assert.Fail(e.Message);
                        //e.printStackTrace();
                    }
                }
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        [Test]
        public void TestAssertionUsingXPath31allowed() {
            assertionUsingXPath3(true);
        }

        [Test]
        public void TestAssertionUsingXPath31notAllowed() {
            assertionUsingXPath3(false);
        }

        [Test]
        public void TestAssertionsOnBadData() {

            // This validation run should report exactly two validation errors. Bug 4172

            string invalid_xml = "<?xml version='1.0'?><request><a>x</a><b/></request>";
            string xsd =
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType mixed='true'>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"a\" type=\"xs:decimal\"/>\n" +
                "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t\t<xs:assert test=\"if (string-length(a) = 3) then exists(b) else empty(b)\"/>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";

            try {
                Processor proc = new Processor(true);
                SchemaManager sm = proc.SchemaManager;
                sm.Compile(new StringReader(xsd));

                SchemaValidator sv = sm.NewSchemaValidator();
                XdmDestination destination = new XdmDestination();
                sv.SetDestination(destination);

                try {
                    sv.Validate(new StringReader(invalid_xml));
                    Assert.Fail("Failed to spot errors");
                }
                catch (SaxonApiException er) {
                    Assert.That(er.Message.Contains("Two validation errors"));
                    // OK
                }
            }
            catch (SaxonApiException sapie) {
                //sapie.printStackTrace();
                Assert.Fail();
            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }


        /**
         * Test the effect of changing a schema to add members to a substitution group.
         * See bug 2174.
         */

        [Test]
        public void TestSchemaChange() {
            try {
                string sch1 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                              "<schema targetNamespace=\"http://myexample/family\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                              "  <element name=\"FamilyMember\" type=\"string\" />\n" +
                              "  <element name=\"Parent\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                              "  <element name=\"Child\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                              "  <element name=\"Family\">\n" +
                              "    <complexType>\n" +
                              "      <sequence>\n" +
                              "        <element ref=\"fam:FamilyMember\" maxOccurs=\"unbounded\"/>\n" +
                              "      </sequence>\n" +
                              "    </complexType>\n" +
                              "  </element>  \n" +
                              "</schema>";

                string doc1 = "<Family xmlns=\"http://myexample/family\">\n" +
                              "  <Parent>John</Parent>\n" +
                              "  <Child>Alice</Child>\n" +
                              "</Family>";

                string sch2 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                              "<schema targetNamespace=\"http://myexample/family_ext\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                              "  <import namespace=\"http://myexample/family\" schemaLocation=\"http://www.valid.nl/taxo3/family.xsd\"/>\n" +
                              "  <element name=\"Cousin\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                              "</schema>";

//            string doc2 = "<Family xmlns=\"http://myexample/family\" xmlns:fam2=\"http://myexample/family_ext\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://myexample/family_ext http://www.valid.nl/taxo3/family_ext.xsd\" >\n" +
//                "  <Parent>John</Parent>\n" +
//                "  <Child>Alice</Child>\n" +
//                "  <fam2:Cousin>Pete</fam2:Cousin>\n" +
//                "</Family>";

                Processor proc = new Processor(true);
                SchemaManager sm = proc.SchemaManager;
                sm.Compile(new StringReader(sch1));
                Console.Error.WriteLine("Schema 1 OK");

                SchemaValidator sv = sm.NewSchemaValidator();
                sv.Validate(new StringReader(doc1));
                Console.Error.WriteLine("Doc 1 OK");
                try {
                    sm.Compile(new StringReader(sch2));
                    Assert.Fail("Unexpected success");
                }
                catch (SaxonApiException e) {
                    Assert.That(e.Message.Contains("It is not possible to add to the substitution group"),
                        "expectedError");
                }
//            Console.Error.WriteLine("Schema 2 OK");
//            try {
//                sm.NewSchemaValidator().Validate(new StreamSource(new StringReader(doc2)));
//                Assert.Fail("Unexpected success");
//            } catch (SaxonApiException e) {
//                Assert.That("expectedError", e.getMessage().contains("It is not possible to add to the substitution group"));
//            }

            }
            catch (SaxonApiException sapie) {
                Console.Error.WriteLine(sapie.Message);
                Assert.Fail();
            }
        }

        [Test]
        public void TestImplicitSchemaImportsOption() {

            string source_xml = "<?xml version='1.0'?><e xmlns='http://y.com/ns'><f/></e>";
            string xsd1 =
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" targetNamespace='http://y.com/ns' elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:import namespace='http://x.com/ns'/>\n" +
                "\t<xs:element name='e' type='x:t' xmlns:x='http://x.com/ns'/>\n" +
                "\t<xs:element name='f' type='xs:anyType'/>\n" +
                "</xs:schema>";

            string xsd2 =
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" targetNamespace='http://x.com/ns' elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t\t<xs:complexType name='t'>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element ref='y:f' xmlns:y='http://y.com/ns'/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t</xs:complexType>\n" +
                "</xs:schema>";


            using Stream output = File.Create(ConfigTest.DATA_DIR + "sandpit/validation.out");
            try {
                Processor proc = new ();
                proc.SetProperty(Saxon.Api.Feature.IMPLICIT_SCHEMA_IMPORTS, true);

                SchemaManager sm = proc.SchemaManager;
                sm.SchemaResolver = (targetNamespace, uri, hints) => {
                    if (targetNamespace == "http://x.com/ns") {
                        return new NodeResource(proc.NewDocumentBuilder().Build(new StringReader(xsd2)));
                    }
                    else {
                        return null;
                    }
                };
                
                sm.Compile(new StringReader(xsd1));
                sm.Compile(new StringReader(xsd2));

                SchemaValidator sv = sm.NewSchemaValidator();


                Serializer serializer = proc.NewSerializer();
                serializer.SetOutputProperty(Serializer.INDENT, "yes");
                serializer.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                serializer.OutputStream = output;
                sv.SetDestination(serializer);


                sv.Validate(new StringReader(source_xml));

                output.Close();

            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void TestValidationReporting() {
            XdmDestination report = new();
            try {
                string valid_xml = "<?xml version='1.0'?><request><a/><!--comment--><b/></request>";
                string invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
                string sch1 =
                    "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                    "\t<xs:element name=\"request\">\n" +
                    "\t\t<xs:complexType>\n" +
                    "\t\t\t<xs:sequence>\n" +
                    "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                    "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                    "\t\t\t</xs:sequence>\n" +
                    "\t\t\t<xs:assert test='count(child::node()) = 3'/>\n" +
                    "\t\t</xs:complexType>\n" +
                    "\t</xs:element>\n" +
                    "</xs:schema>";

                string doc1 = "<Family xmlns=\"http://myexample/family\">\n" +
                              "  <Parent>John</Parent>\n" +
                              "  <Child>Alice</Child>\n" +
                              "</Family>";

                string sch2 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                              "<schema targetNamespace=\"http://myexample/family_ext\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                              "  <import namespace=\"http://myexample/family\" schemaLocation=\"http://www.valid.nl/taxo3/family.xsd\"/>\n" +
                              "  <element name=\"Cousin\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                              "</schema>";

                string doc2 =
                    "<Family xmlns=\"http://myexample/family\" xmlns:fam2=\"http://myexample/family_ext\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://myexample/family_ext http://www.valid.nl/taxo3/family_ext.xsd\" >\n" +
                    "  <Parent>John</Parent>\n" +
                    "  <Child>Alice</Child>\n" +
                    "  <fam2:Cousin>Pete</fam2:Cousin>\n" +
                    "</Family>";

                Processor proc = new Processor(true);
                SchemaManager sm = proc.SchemaManager;
                sm.XsdVersion = "1.1";
                sm.Compile(new StringReader(sch1));
                Console.Error.WriteLine("Schema 1 OK");
                SchemaValidator validator = sm.NewSchemaValidator();

                validator.SetValidityReporting(report);
                validator.Validate(new StringReader(invalid_xml), new Uri("http://doc1"));
                Assert.Fail("Invalidity not detected");


            }
            catch (SaxonApiException sapie) {
                Console.Error.WriteLine(sapie.Message);

            }
            finally {
                XdmNode reportDoc = report.XdmNode;
                Assert.AreEqual(1, reportDoc.Children("http://saxon.sf.net/ns/validation", "validation-report").Count());
                Assert.That(reportDoc.Select(Steps.Descendant("error")).Count() == 2);
                Assert.That(reportDoc.Select(Steps.Descendant("meta-data")).Count() == 1);
                Assert.That(reportDoc.Select(Steps.Descendant("schema")).First().GetAttributeValue("xsd-version") == "1.1");
            }


        }

        [Test]
        public void TestValidationReporting2() {
            SchemaValidator validator = null;
            Serializer dest = null;
            //   StringWriter writer = new StringWriter();
            try {
                string valid_xml = "<?xml version='1.0'?><request><a/><!--comment--><b/></request>";
                string invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
                string sch1 =
                    "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                    "\t<xs:element name=\"request\">\n" +
                    "\t\t<xs:complexType>\n" +
                    "\t\t\t<xs:sequence>\n" +
                    "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                    "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                    "\t\t\t</xs:sequence>\n" +
                    "\t\t\t<xs:assert test='count(child::node()) = 3'/>\n" +
                    "\t\t</xs:complexType>\n" +
                    "\t</xs:element>\n" +
                    "</xs:schema>";

                string doc1 = "<Family xmlns=\"http://myexample/family\">\n" +
                              "  <Parent>John</Parent>\n" +
                              "  <Child>Alice</Child>\n" +
                              "</Family>";

                string sch2 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                              "<schema targetNamespace=\"http://myexample/family_ext\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                              "  <import namespace=\"http://myexample/family\" schemaLocation=\"http://www.valid.nl/taxo3/family.xsd\"/>\n" +
                              "  <element name=\"Cousin\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                              "</schema>";

                string doc2 =
                    "<Family xmlns=\"http://myexample/family\" xmlns:fam2=\"http://myexample/family_ext\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://myexample/family_ext http://www.valid.nl/taxo3/family_ext.xsd\" >\n" +
                    "  <Parent>John</Parent>\n" +
                    "  <Child>Alice</Child>\n" +
                    "  <fam2:Cousin>Pete</fam2:Cousin>\n" +
                    "</Family>";

                Processor proc = new Processor(true);
                SchemaManager sm = proc.SchemaManager;
                sm.XsdVersion = "1.1";
                sm.Compile(new StringReader(sch1), new Uri("http://doc1"));
                //sm.Load(new StreamSource(new File("/Users/ond1/work/development/files/millicom/FafaSCript.xsd")));//new StringReader(sch1)));
                Console.Error.WriteLine("Schema 1 OK");
                validator = sm.NewSchemaValidator();

                dest = proc.NewSerializer();
                dest.SetOutputProperty(Serializer.INDENT, "yes");
                dest.OutputFile = ConfigTest.DATA_DIR + "sandpit/validationReport.xml";

                // dest.setOutputWriter(writer);
                validator.SetValidityReporting(dest);
                //validator.Validate(new StreamSource(new File("/Users/ond1/work/development/files/millicom/_XMLDestination.xml")));//new StringReader(invalid_xml)));
                validator.Validate(new StringReader(invalid_xml));
                Console.Error.WriteLine("Doc 2 OK");

                XdmNode resultdoc = proc.NewDocumentBuilder()
                    .Build(new Uri(ConfigTest.DataDirUri, "sandpit/validationReport.xml"));
                Console.Error.WriteLine(resultdoc.StringValue);
                Assert.That(false); // TBA

            }
            catch (SaxonApiException sapie) {
                Console.Error.WriteLine(sapie.Message);
                //sapie.printStackTrace();
                //  Console.Error.WriteLine(writer.toString());

            }
            catch (Exception ex) {
                Assert.Fail(ex.Message);
                //ex.printStackTrace();
            }


        }

        /**
         * Test validating multiple documents with reporting feature on
         */
        [Test]
        public void TestValidationReporting3() {
            SchemaValidator validator = null;
            XdmDestination report = new XdmDestination();
            try {
                string valid_xml = "<?xml version='1.0'?><request><a/><!--comment--><b/></request>";
                string invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
                string sch1 =
                    "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                    "\t<xs:element name=\"request\">\n" +
                    "\t\t<xs:complexType>\n" +
                    "\t\t\t<xs:sequence>\n" +
                    "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                    "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                    "\t\t\t</xs:sequence>\n" +
                    "\t\t\t<xs:assert test='count(child::node()) = 2'/>\n" +
                    "\t\t</xs:complexType>\n" +
                    "\t</xs:element>\n" +
                    "</xs:schema>";

                Processor proc = new Processor(true);
                SchemaManager sm = proc.SchemaManager;
                sm.XsdVersion = "1.1";
                
                sm.Compile(new StringReader(sch1));
                Console.Error.WriteLine("Schema 1 OK");
                validator = sm.NewSchemaValidator();
                validator.SetValidityReporting(report);
                validator.AddSource(new StringReader(invalid_xml), new Uri("http://invalid.xml/"));
                validator.AddSource(new StringReader(valid_xml), new Uri("http://valid.xml/"));
                validator.ValidateMultipleDocuments();
                Console.Error.WriteLine("Validation completed OK");
            }
            catch (SaxonApiException sapie) {
                Assert.Fail(sapie.Message);

            }
            finally {
                XdmNode reportDoc = report.XdmNode;
                Console.WriteLine(reportDoc.OuterXml);
                Assert.AreEqual(1, reportDoc.Children( "validation-reports").Count());
                Assert.AreEqual(2, reportDoc.Select(Steps.Descendant("http://saxon.sf.net/ns/validation", "validation-report")).Count());
                Assert.AreEqual(2, reportDoc.Select(
                    Steps.Descendant("validation-report")
                        .Where(Predicates.AttributeEq("system-id", "http://invalid.xml/"))
                        .Then(Steps.Child("error"))).Count());
                Assert.AreEqual(0, reportDoc.Select(
                    Steps.Descendant("validation-report")
                        .Where(Predicates.AttributeEq("system-id", "http://valid.xml/"))
                        .Then(Steps.Child("error"))).Count());
            }


        }


        /**
         * Test validating multiple documents with reporting feature off
         */
        [Test]
        public void TestValidationReporting4() {
            SchemaValidator validator = null;
            Serializer dest = null;
            StringWriter sw = new StringWriter();
            try {
                string valid_xml = "<?xml version='1.0'?><request><a/><!--comment--><b/></request>";
                string invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
                string sch1 =
                    "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                    "\t<xs:element name=\"request\">\n" +
                    "\t\t<xs:complexType>\n" +
                    "\t\t\t<xs:sequence>\n" +
                    "\t\t\t\t<xs:element name=\"a\" type=\"xs:string\"/>\n" +
                    "\t\t\t\t<xs:element name=\"b\" type=\"xs:string\"/>\n" +
                    "\t\t\t</xs:sequence>\n" +
                    "\t\t\t<xs:assert test='count(child::node()) = 3'/>\n" +
                    "\t\t</xs:complexType>\n" +
                    "\t</xs:element>\n" +
                    "</xs:schema>";

                string doc1 = "<Family xmlns=\"http://myexample/family\">\n" +
                              "  <Parent>John</Parent>\n" +
                              "  <Child>Alice</Child>\n" +
                              "</Family>";

                string sch2 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
                              "<schema targetNamespace=\"http://myexample/family_ext\" xmlns:fam=\"http://myexample/family\" xmlns=\"http://www.w3.org/2001/XMLSchema\">\n" +
                              "  <import namespace=\"http://myexample/family\" schemaLocation=\"http://www.valid.nl/taxo3/family.xsd\"/>\n" +
                              "  <element name=\"Cousin\" type=\"string\" substitutionGroup=\"fam:FamilyMember\"/>\n" +
                              "</schema>";

                string doc2 =
                    "<Family xmlns=\"http://myexample/family\" xmlns:fam2=\"http://myexample/family_ext\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://myexample/family_ext http://www.valid.nl/taxo3/family_ext.xsd\" >\n" +
                    "  <Parent>John</Parent>\n" +
                    "  <Child>Alice</Child>\n" +
                    "  <fam2:Cousin>Pete</fam2:Cousin>\n" +
                    "</Family>";

                Processor proc = new Processor(true);
                SchemaManager sm = proc.SchemaManager;
                sm.XsdVersion = "1.1";
                sm.Compile(new StringReader(sch1));
                Console.Error.WriteLine("Schema 1 OK");
                validator = sm.NewSchemaValidator();

                dest = proc.NewSerializer(sw);
                dest.SetOutputProperty(Serializer.INDENT, "yes");
                dest.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                validator.AddSource(new StringReader(invalid_xml));
                validator.AddSource(new StringReader(doc1));
                validator.ValidateMultipleDocuments();
                Console.Error.WriteLine("Validation completed OK");


            }
            catch (SaxonApiException sapie) {
                Console.Error.WriteLine(sapie.Message);

            }
            finally {
                if (dest != null) {
                    Console.Error.WriteLine(sw);
                    Assert.That(true, sw.ToString());

                }

            }


        }



        // [Test]
        // @Tag("missingfiles")
        // // Documents are not in the repository
        // public void TestMillicomErrorListener() {
        //     string[] args = {
        //             "-xsd:" + ConfigTest.DATA_DIR + "SubsidyTrackingFactXSD11.xsd",
        //             "-s:" + ConfigTest.DATA_DIR + "id2278_HN201506_SubsidyTrackingFact.xml",
        //             "-verbose",
        //             "-xsdversion:1.1"};
        //
        //
        //     try {
        //         MillicomTest.main(args);
        //     } catch (IOException | SaxonApiException e) {
        //         e.printStackTrace();
        //         Assert.Fail(e.Message);
        //     }
        //
        //
        // }

        [Test]
        public void TestLeak() {
            try {
                Processor processor = new Processor(true);
                processor.SetProperty(Saxon.Api.Feature.SCHEMA_VALIDATION_MODE, "lax");
                processor.SchemaManager.Compile(new Uri(ConfigTest.DataDirUri, "leak/schema.xsd"));

                DocumentBuilder builder = processor.NewDocumentBuilder();
                builder.TreeModel = TreeModel.TinyTree;

                XdmNode node = builder.Build(new Uri(ConfigTest.DataDirUri, "leak/instance.xml"));
                WeakReference<TinyTree> tinyTree = new(((TinyDocumentImpl) node.Implementation).getTree());

                XPathCompiler compiler = processor.NewXPathCompiler();
                compiler.ImportSchemaNamespace("http://example.org");
                compiler.Evaluate("namespace-uri-from-QName(//*/foo)", node);
                node = builder.Build(new Uri(ConfigTest.DataDirUri, "leak/instance.xml"));
                //Assert.False(tinyTree.Tar);
            }
            catch (SaxonApiException e) {
                Assert.Fail();
            }
        }

        [Test]
        public void TestMultipleSchemaImportsTT() {
            loadMultiple(true, true);
        }

        [Test]
        public void TestMultipleSchemaImportsTF() {
            loadMultiple(true, false);
        }

        [Test]
        public void TestMultipleSchemaImportsFT() {
            loadMultiple(false, true);
        }

        [Test]
        public void TestMultipleSchemaImportsFF() {
            loadMultiple(false, false);
        }

        private void loadMultiple(bool multiple, bool fromNode) {

            try {
                Processor proc = new();

                proc.SetProperty(Saxon.Api.Feature.MULTIPLE_SCHEMA_IMPORTS, multiple);

                Uri dir = new Uri(ConfigTest.DataDirUri, "petrk/AbstractType-2/");

                Console.Error.WriteLine("Loading from file:");

                if (fromNode) {
                    loadSchemaFromNode(proc, new Uri(dir, "src/schema.xsd"));
                    loadSchemaFromNode(proc, new Uri(dir, "src/schema1.xsd"));
                    loadSchemaFromNode(proc, new Uri(dir, "src/schema2.xsd"));
                }
                else {
                    loadSchemaOK(proc, new Uri(dir, "src/schema.xsd"));
                    loadSchemaOK(proc, new Uri(dir, "src/schema1.xsd"));
                    loadSchemaOK(proc, new Uri(dir, "src/schema2.xsd"));
                }

                Console.Error.WriteLine("Loading successful");

            }
            catch (Exception e) {
                //e.printStackTrace();
                Assert.Fail();
            }
        }

        private static void loadSchemaOK(Processor proc, Uri urlStr) {
            proc.SchemaManager.Compile(urlStr);

        }

        private static void loadSchemaFromNode(Processor proc, Uri urlStr) {
            DocumentBuilder builder = proc.NewDocumentBuilder();
            builder.LineNumbering = true;
            XdmNode ni = builder.Build(urlStr);
            //Console.Error.WriteLine( QueryResult.serialize(ni) );
            //Console.Error.WriteLine( ni.getSystemId() );
            proc.SchemaManager.Compile(ni);

        }


        [Test]
        public void TestValidateTransformationOutput() {

            // This validation run should report exactly two validation errors.

            string xml = "<?xml version='1.0'?>"
                         + "<doc><request>"
                         + "<user_name>ed</user_name>"
                         + "<password>sdsd</password>"
                         + "<d1>2009-01-01</d1>"
                         + "<d1>2010-01-01</d1>"
                         + "<d2>2011-01-01</d2>"
                         + "</request></doc>";
            string xsl = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>"
                         + "<xsl:mode on-no-match='shallow-copy'/>"
                         + "<xsl:template match='/'><xsl:result-document validation='strict'><xsl:apply-templates select='//request'/></xsl:result-document></xsl:template>"
                         + "<xsl:template match='d1'><date1><xsl:value-of select=\"'x' || .\"/></date1></xsl:template>"
                         + "<xsl:template match='d2'><date2><xsl:value-of select=\"'x' || .\"/></date2></xsl:template>"
                         + "</xsl:transform>";
            string xsd =
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"user_name\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"password\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"date1\" type=\"xs:date\" maxOccurs='unbounded'/>\n" +
                "\t\t\t\t<xs:element name=\"date2\" type=\"xs:date\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";

            List<Error> errors = new();
            try {
                Processor proc = new Processor(true);
                SchemaManager sm = proc.SchemaManager;
                sm.Compile(new StringReader(xsd));

                XsltCompiler xsltCompiler = proc.NewXsltCompiler();
                xsltCompiler.SchemaAware = true;
                Xslt30Transformer exec = xsltCompiler.Compile(new StringReader(xsl)).Load30();
                exec.ErrorReporter = err => {
                    Console.Error.WriteLine(err.Message);
                    errors.Add(err);
                };

                StringWriter sw = new StringWriter();
                exec.Transform(new MemoryStream(new UTF8Encoding().GetBytes(xml)),
                    proc.NewSerializer(sw));


                Assert.Fail("Validation errors not detected");

            }
            catch (SaxonApiException sapie) {
                Assert.AreEqual(4, errors.Count, "No of errors"); // 3 validation errors plus one summary fatal error
                //Assert.AreEqual("/doc[1]/request[1]/d1[2]", ((ValidationException)errors.get(1)).getValidationFailure().getContextPath().getPathUsingPrefixes(), "ContextPath");

            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }
        
        [Test]
        public void TestValidateTransformationOutputPassing() {
            // Use SchemaValidator as transformation destination (Saxon 11.1)
            // This validation run should report no validation errors.

            string xml = "<?xml version='1.0'?>"
                         + "<doc><request>"
                         + "<user_name>ed</user_name>"
                         + "<password>sdsd</password>"
                         + "<d1>2009-01-01</d1>"
                         + "<d1>2010-01-01</d1>"
                         + "<d2>2011-01-01</d2>"
                         + "</request></doc>";
            string xsl = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>"
                         + "<xsl:mode on-no-match='shallow-copy'/>"
                         + "<xsl:template match='/'><xsl:apply-templates select='//request'/></xsl:template>"
                         + "<xsl:template match='d1'><date1><xsl:value-of select=\"' ' || .\"/></date1></xsl:template>"
                         + "<xsl:template match='d2'><date2><xsl:value-of select=\"' ' || .\"/></date2></xsl:template>"
                         + "</xsl:transform>";
            string xsd =
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"user_name\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"password\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"date1\" type=\"xs:date\" maxOccurs='unbounded'/>\n" +
                "\t\t\t\t<xs:element name=\"date2\" type=\"xs:date\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";

            List<ValidationFailure> errors = new();
            try {
                Processor proc = new Processor(true);
                SchemaManager sm = proc.SchemaManager;
                sm.Compile(new StringReader(xsd));
                SchemaValidator sv = sm.NewSchemaValidator();
                sv.InvalidityListener = failure => errors.Add(failure);
                XsltCompiler xsltCompiler = proc.NewXsltCompiler();
                xsltCompiler.SchemaAware = true;
                Xslt30Transformer exec = xsltCompiler.Compile(new StringReader(xsl)).Load30();

                StringWriter sw = new StringWriter();
                sv.SetDestination(proc.NewSerializer(sw));
                exec.Transform(new MemoryStream(new UTF8Encoding().GetBytes(xml)), sv);


                Assert.That(sw.ToString().Contains("<date1> 2009-01-01</date1>"));

            }
            catch (SaxonApiException sapie) {
                Assert.Fail(sapie.Message); 
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

        
        [Test]
        public void TestValidateTransformationOutputFailing() {
            // Use SchemaValidator as transformation destination (Saxon 11.1)
            // This validation run should report exactly two validation errors.

            string xml = "<?xml version='1.0'?>"
                         + "<doc><request>"
                         + "<user_name>ed</user_name>"
                         + "<password>sdsd</password>"
                         + "<d1>2009-01-01</d1>"
                         + "<d1>2010-01-01</d1>"
                         + "<d2>2011-01-01</d2>"
                         + "</request></doc>";
            string xsl = "<xsl:transform xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='3.0'>"
                         + "<xsl:mode on-no-match='shallow-copy'/>"
                         + "<xsl:template match='/'><xsl:apply-templates select='//request'/></xsl:template>"
                         + "<xsl:template match='d1'><date1><xsl:value-of select=\"'x' || .\"/></date1></xsl:template>"
                         + "<xsl:template match='d2'><date2><xsl:value-of select=\"'x' || .\"/></date2></xsl:template>"
                         + "</xsl:transform>";
            string xsd =
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\" attributeFormDefault=\"unqualified\">\n" +
                "\t<xs:element name=\"request\">\n" +
                "\t\t<xs:complexType>\n" +
                "\t\t\t<xs:sequence>\n" +
                "\t\t\t\t<xs:element name=\"user_name\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"password\" type=\"xs:string\"/>\n" +
                "\t\t\t\t<xs:element name=\"date1\" type=\"xs:date\" maxOccurs='unbounded'/>\n" +
                "\t\t\t\t<xs:element name=\"date2\" type=\"xs:date\"/>\n" +
                "\t\t\t</xs:sequence>\n" +
                "\t\t</xs:complexType>\n" +
                "\t</xs:element>\n" +
                "</xs:schema>";

            List<ValidationFailure> errors = new();
            try {
                Processor proc = new Processor(true);
                SchemaManager sm = proc.SchemaManager;
                sm.Compile(new StringReader(xsd));
                SchemaValidator sv = sm.NewSchemaValidator();
                sv.InvalidityListener = failure => errors.Add(failure);
                XsltCompiler xsltCompiler = proc.NewXsltCompiler();
                xsltCompiler.SchemaAware = true;
                Xslt30Transformer exec = xsltCompiler.Compile(new StringReader(xsl)).Load30();

                StringWriter sw = new StringWriter();
                sv.SetDestination(proc.NewSerializer(sw));
                exec.Transform(new MemoryStream(new UTF8Encoding().GetBytes(xml)), sv);


                Assert.Fail("Validation errors not detected");

            }
            catch (SaxonApiException sapie) {
                Assert.AreEqual(3, errors.Count, "No of errors"); // 3 validation errors
                //Assert.AreEqual("/doc[1]/request[1]/d1[2]", ((ValidationException)errors.get(1)).getValidationFailure().getContextPath().getPathUsingPrefixes(), "ContextPath");

            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }


        [Test]
        public void TestErrorListenerBug4744() {

            // This validation run should report exactly two validation errors.

            using Stream output = File.Create(ConfigTest.DATA_DIR + "sandpit/validation.out");
            List<ValidationFailure> instanceErrors = new();
            try {
                Processor proc = new(true);
                SchemaManager sm = proc.SchemaManager;
                List<Error> schemaErrors = new();
                sm.ErrorReporter = schemaErrors.Add;
                sm.Compile(new Uri(ConfigTest.DataDirUri, "../bugs/4744/UniqueKeyBug.xsd"));
                Assert.That(schemaErrors.Count == 0);

                SchemaValidator sv = sm.NewSchemaValidator();
                sv.InvalidityListener = instanceErrors.Add;


                Serializer serializer = proc.NewSerializer();
                serializer.SetOutputProperty(Serializer.INDENT, "yes");
                serializer.SetOutputProperty(Serializer.OMIT_XML_DECLARATION, "yes");
                serializer.OutputStream = output;
                sv.SetDestination(serializer);
                sv.Validate(new Uri(ConfigTest.DataDirUri, "../bugs/4744/UniqueKeyBug.xml"));

                output.Close();
                Assert.Fail("Validation errors not detected");

            }
            catch (SaxonApiException sapie) {
                try {
                    output.Close();
                }
                catch (IOException ee) {
                    Assert.Fail();
                }

                Assert.AreEqual(1, instanceErrors.Count);
            }
            catch (Exception e) {
                Assert.Fail(e.Message);
            }
        }

 
    private void testSchemaLocationHints(bool useHints, bool withNS) {
        try {
            Processor processor = new ();
            DocumentBuilder builder = processor.NewDocumentBuilder();
            XdmNode source = builder.Build(new Uri(ConfigTest.DataDirUri, (withNS ? "xsdhints/documentNS.xml" : "xsdhints/document.xml")));

            SchemaManager manager = processor.SchemaManager;
            SchemaValidator validator = manager.NewSchemaValidator();

            validator.UseXsiSchemaLocation = useHints;

            validator.Validate(source);

            if (!useHints) {
                Assert.Fail("Document should not have validated");
            }
        } catch (Exception ex) {
            if (useHints) {
                Assert.Fail("Validation should not have failed");
            }
        }
    }

    [Test]
    public void testIgnoreSchemaLocationHints() {
        testSchemaLocationHints(false, false);
        testSchemaLocationHints(false, true);
        testSchemaLocationHints(true, false);
        testSchemaLocationHints(true, true);
    }

 


 

    }

}

