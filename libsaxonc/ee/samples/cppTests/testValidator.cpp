#include "saxonc/SaxonProcessor.h"
#include "saxonc/XdmNode.h"
#include "saxonc/XdmValue.h"
#include "saxonc/XsdCompiler.h"
#include "saxonc/XsdSchema.h"
#include "CppTestUtils.h"

#include <string>

using namespace std;

void testValidator1(SaxonProcessor *processor,
                    sResultCount *sresult) {
  cout << endl
       << "testValidator1: Test Validate Schema from string - invalid doc"
       << endl;
  string invalid_xml =
      "<?xml version='1.0'?><request><a/><!--comment--></request>";
  const char *sch1 =
      "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' "
      "elementFormDefault='qualified' "
      "attributeFormDefault='unqualified'><xs:element "
      "name='request'><xs:complexType><xs:sequence><xs:element name='a' "
      "type='xs:string'/><xs:element name='b' "
      "type='xs:string'/></xs:sequence><xs:assert test='count(child::node()) = "
      "3'/></xs:complexType></xs:element></xs:schema>";

  SchemaValidator *val = nullptr;

  XdmNode *input = nullptr;
  try {
    val = processor->newSchemaValidator();
    val->registerSchemaFromString(sch1);

    input = processor->parseXmlFromString(invalid_xml.c_str());
    val->setSourceNode(input);
    val->validate();
    sresult->failure++;
    sresult->failureList.push_back("testValidator1");
  } catch (SaxonApiException &e) {
    const char *message = e.getMessage();
    if (message != nullptr) {
      cerr << "Exception thrown = " << message << endl;
    }
    sresult->success++;
  }
  delete input;
  delete val;
}

void testValidatorInParseXml(SaxonProcessor *processor, sResultCount *sresult) {
  cout << endl
       << "testValidatorInParseXml: Test Validate Schema from string - invalid "
          "doc"
       << endl;
  string invalid_xml =
      "<?xml version='1.0'?><request><a/><!--comment--></request>";
  const char *sch1 =
      "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' "
      "elementFormDefault='qualified' "
      "attributeFormDefault='unqualified'><xs:element "
      "name='request'><xs:complexType><xs:sequence><xs:element name='a' "
      "type='xs:string'/><xs:element name='b' "
      "type='xs:string'/></xs:sequence><xs:assert test='count(child::node()) = "
      "3'/></xs:complexType></xs:element></xs:schema>";
  XdmNode *input = nullptr;
  XsdCompiler * xsdCompiler = nullptr;
  SchemaValidator * val = nullptr;
  XsdSchema *schema = nullptr;

  try {
    xsdCompiler = processor->newXsdCompiler();
    schema = xsdCompiler->compileFromString(sch1);
    delete xsdCompiler;
  } catch (SaxonApiException &e) {
    const char *message = e.getMessage();
    if (message != nullptr) {
      cerr << "Exception thrown testValidatorInParseXml-a = " << message << endl;
    }
    sresult->failure++;
    sresult->failureList.push_back("testValidatorInParseXml-a");
    if (xsdCompiler != nullptr) {
      delete xsdCompiler;
    }
    return;
  }

  try {

    val = schema->newValidator();

    input = processor->parseXmlFromString(invalid_xml.c_str(), nullptr, val);

    sresult->failure++;
    sresult->failureList.push_back("testValidatorInParseXml");

  } catch (SaxonApiException &e) {
    const char *message = e.getMessage();
    if (message != nullptr) {
      cerr << "Exception thrown = " << message << endl;
    }
    sresult->success++;
  }
  delete schema;
  delete input;
  delete val;
}


//Test of the old Schema API for backward compatibility. This way will be removed in future release
void testValidator2(sResultCount *sresult) {
  SaxonProcessor *processor = new SaxonProcessor(true);
  processor->setConfigurationProperty("xsdversion", "1.1");

  cout << endl << "Test 2: Validate Schema from string" << endl;
  string invalid_xml =
      "<?xml version='1.0'?><request><a/><!--comment--></request>";
  const char *sch1 =
      "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' "
      "elementFormDefault='qualified' "
      "attributeFormDefault='unqualified'><xs:element "
      "name='request'><xs:complexType><xs:sequence><xs:element name='a' "
      "type='xs:string'/><xs:element name='b' "
      "type='xs:string'/></xs:sequence><xs:assert test='count(child::node()) = "
      "3'/></xs:complexType></xs:element></xs:schema>";

  string doc1 = "<Family "
                "xmlns='http://myexample/family'><Parent>John</"
                "Parent><Child>Alice</Child></Family>";

  XdmNode *input = nullptr;
  SchemaValidator * val = nullptr;

  try {
    val = processor->newSchemaValidator();
  } catch (SaxonApiException &e) {
    const char *message = e.getMessage();
    if (message != nullptr) {
      cerr << "Exception thrown = " << message << endl;
    }
    sresult->failure++;
    sresult->failureList.push_back("testValidator2-a");
    delete processor;
    return;
  }

  try {
    input = processor->parseXmlFromString(doc1.c_str());

  } catch (SaxonApiException &e) {
    const char *message = e.getMessage();
    if (message != nullptr) {
      cerr << "Exception thrown = " << message << endl;
    }
    sresult->success++;
  }

  val->setSourceNode(input);

  try {

    val->registerSchemaFromString(sch1);

    val->validate();

    cout << endl << "Doc is valid!" << endl;
    sresult->failure++;
    sresult->failureList.push_back("testValidator2");
  } catch (SaxonApiException &e) {
    cout << endl << "Doc is not valid!" << endl;
    const char *message = e.getMessage();
    const char *errorCode = e.getErrorCode();
    if (message != nullptr) {
      cerr << "Exception thrown = " << message << endl;
    }
    if (errorCode != nullptr) {
      cerr << "Error Code: " << errorCode << endl;
    }
    sresult->success++;
  }
  if (input != nullptr) {
    delete input;
    input = nullptr;
  }
  delete val;
  delete processor;
}

void testValidator3(SaxonProcessor *processor, const std::string *const dataDir,
                    sResultCount *sresult) {
  processor->exceptionClear();
  SchemaValidator *val = nullptr;
  XsdCompiler *compiler = nullptr;
  XsdSchema * schema = nullptr;

  cout << endl << "Test 3: Validate Schema from string" << endl;
  const char *sch1 =
      "<?xml version='1.0' encoding='UTF-8'?><schema "
      "targetNamespace='http://myexample/family' "
      "xmlns:fam='http://myexample/family' "
      "xmlns='http://www.w3.org/2001/XMLSchema'><element name='FamilyMember' "
      "type='string' /><element name='Parent' type='string' "
      "substitutionGroup='fam:FamilyMember'/><element name='Child' "
      "type='string' substitutionGroup='fam:FamilyMember'/><element "
      "name='Family'><complexType><sequence><element ref='fam:FamilyMember' "
      "maxOccurs='unbounded'/></sequence></complexType></element>  </schema>";

  try {
    compiler = processor->newXsdCompiler();
    schema = compiler->compileFromString(sch1);

  } catch (SaxonApiException &e) {
    const char *message = e.getMessage();
    if (message != nullptr) {
      cerr << "Exception thrown in compile = " << message << endl;
    }
    sresult->failure++;
    sresult->failureList.push_back("testValidator3-a");
    delete compiler;
    return;
  }

  delete compiler;

  try {
    val = schema->newValidator();
    val->setProperty("http://saxon.sf.net/feature/multipleSchemaImports", "on");
    val->validate(CppTestUtils::concat(2, dataDir->c_str(), "/family.xml").c_str());
    cout << endl << "Doc1 is OK" << endl;
    sresult->failure++;
    sresult->failureList.push_back("testValidator3");

  } catch (SaxonApiException &e) {
    cout << endl << "Error: Doc reported as invalid!" << endl;
    const char *message = e.getMessage();
    if (message != nullptr) {
      cerr << "Exception thrown = " << message << endl;
    }
    sresult->success++;
  }
  delete val;
  delete schema;
}

void testValidator4(const char *cwd,
                    const std::string *const dataDir,
                    sResultCount *sresult) {

  SaxonProcessor *processor = new SaxonProcessor(true);
  processor->setConfigurationProperty("xsdversion", "1.1");
  SchemaValidator *val = nullptr;


  try {
    val = processor->newSchemaValidator();
  } catch (SaxonApiException &e) {
    const char *message = e.getMessage();
    if (message != nullptr) {
      cerr << "Exception thrown = " << message << endl;
    }
    sresult->failure++;
    sresult->failureList.push_back("testValidator4-a");
    delete processor;
    return;
  }
  val->setcwd(cwd);
  val->clearParameters(true);
  val->clearProperties();
  cout << endl
       << "Test 4: Validate source file with schema file. i.e. family.xml and "
          "family.xsd"
       << endl;

  try {
    val->registerSchemaFromFile(CppTestUtils::concat(2, dataDir->c_str(), "/family-ext.xsd").c_str());

    val->registerSchemaFromFile(CppTestUtils::concat(2, dataDir->c_str(), "/family.xsd").c_str());
    val->validate(CppTestUtils::concat(2, dataDir->c_str(), "/family.xml").c_str());
    cout << endl << "Doc1 is OK" << endl;
    sresult->success++;
  } catch (SaxonApiException &e) {
    cout << endl << "Error: Doc reported as invalid! or schema load method called already" << endl;
    const char *message = e.getMessage();
    const char *errorCode = e.getErrorCode();
    if (errorCode != nullptr) {
      cerr << "ErrorCode = " << errorCode << endl;
    }
    if (message != nullptr) {

      cerr << "Exception thrown = " << message << endl;
    }
    sresult->failure++;
    sresult->failureList.push_back("testValidator4");
  }
  delete val;
  delete processor;
}

void testValidator4a(const std::string *const dataDir, sResultCount *sresult) {
  SaxonProcessor *processor = new SaxonProcessor(true);
  processor->setcwd(dataDir->c_str());
  processor->setConfigurationProperty("xsdversion", "1.1");
  SchemaValidator *val = nullptr;
  XsdCompiler *compiler = nullptr;
  XsdSchema *schema = nullptr;

  cout << endl
       << "Test testValidator4a: Validate source file with schema file. i.e. "
          "family.xml and family.xsd to XdmNode"
       << endl;
  try {
    compiler = processor->newXsdCompiler();
    const char ** filenames = new const char*[2];
    filenames[0] = CppTestUtils::concat(2, dataDir->c_str(), "/family-ext.xsd").c_str();
    filenames[1] = CppTestUtils::concat(2, dataDir->c_str(), "/family.xml").c_str();
    schema = compiler->compileFromFiles({CppTestUtils::concat(2, dataDir->c_str(), "/family-ext.xsd").c_str(),
                            CppTestUtils::concat(2, dataDir->c_str(), "/family.xml").c_str()});
  } catch (SaxonApiException &e) {
    const char *message = e.getMessage();
    if (message != nullptr) {
      cerr << "Exception thrown = " << message << endl;
    }
    sresult->failure++;
    sresult->failureList.push_back("testValidator4a-a");
    delete processor;
    delete compiler;
    return;
  }
  delete compiler;



  try {
    val = schema->newValidator();
    XdmNode *node = val->validateToNode(CppTestUtils::concat(2, dataDir->c_str(), "/family.xml").c_str());

    const char *valueStr = node->toString();
    if (valueStr != nullptr) {
      cout << endl << "Doc1 is OK:" << valueStr << endl;
      operator delete((char *)valueStr);
    } else {
      cout << endl << "Error: Doc reported as valid!" << endl;
    }
    sresult->success++;
    delete node;
  } catch (SaxonApiException &e) {
    cout << endl << "Error: Doc reported as invalid!" << endl;
    const char *message = e.getMessage();
    const char *errorCode = e.getErrorCode();
    if (errorCode != nullptr) {
      cerr << "ErrorCode = " << errorCode << endl;
    }
    if (message != nullptr) {

      cerr << "Exception thrown = " << message << endl;
    }
    sresult->failure++;
    sresult->failureList.push_back("testValidator4a");
  }
  delete schema;
  delete val;
  delete processor;
}

void testValidator5(sResultCount *sresult) {

  SaxonProcessor *processor = new SaxonProcessor(true);


  SchemaValidator *val = nullptr;
  XsdCompiler * compiler = nullptr;
  XsdSchema * schema = nullptr;
  XdmNode *input = nullptr;

  cout << endl << "Test 5: Validate Schema from string" << endl;
  string invalid_xml =
      "<?xml version='1.0'?><request><a/><!--comment--></request>";
  const char *sch1 =
      "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' "
      "elementFormDefault='qualified' "
      "attributeFormDefault='unqualified'><xs:element "
      "name='request'><xs:complexType><xs:sequence><xs:element name='a' "
      "type='xs:string'/><xs:element name='b' "
      "type='xs:string'/></xs:sequence><xs:assert test='count(child::node()) = "
      "3'/></xs:complexType></xs:element></xs:schema>";

  string doc1 = "<request "
                "xmlns='http://myexample/family'><Parent>John</"
                "Parent><Child1>Alice</Child1></request>";

  try {
    compiler = processor->newXsdCompiler();
    schema = compiler->compileFromString(sch1);

  } catch (SaxonApiException &e) {
    const char *message = e.getMessage();
    if (message != nullptr) {
      cerr << "Exception thrown = " << message << endl;
    }
    sresult->failure++;
    sresult->failureList.push_back("testValidator5-a");
    delete compiler;
    return;
  }

  delete compiler;


  try {
    val = schema->newValidator();
    input = processor->parseXmlFromString(doc1.c_str());
    val->setProperty("xsdversion", "1.1");
    val->setParameter("node", (XdmValue *)input, false);

    val->setProperty("report-node", "true");

    val->setProperty("verbose", "true");
    val->validate();
    sresult->failure++;
    sresult->failureList.push_back("testValidator5");



  } catch (SaxonApiException &e) {
    cout << endl << "Error: Doc reported as invalid!" << endl;

    const char *message = e.getMessage();
    const char *errorCode = e.getErrorCode();
    if (errorCode != nullptr) {
      cerr << "ErrorCode = " << errorCode << endl;
    }
    if (message != nullptr) {
      cerr << "Exception thrown = " << message << endl;
    }
    XdmNode *node = nullptr;
    node = val->getValidationReport();
    if (node != nullptr) {
      const char *value = node->toString();
      if (value != nullptr) {
        cout << endl << "Validation Report: " << value << endl;
        sresult->success++;
        operator delete((char *)value);
      } else {
        cout << endl << "Validation Report value is NULL" << endl;
        sresult->failure++;
        sresult->failureList.push_back("testValidator5");
      }
      delete node;
    } else {
      cout << endl
     << "Error: Validation Report is NULL - This should not be NULL. "
        "Probably no valid license file found."
     << endl;
    }
  }
  delete input;
  delete schema;
  delete val;
  delete processor;


}

void testValidator6( const std::string *const dataDir, sResultCount *sresult) {

  SaxonProcessor *processor = new SaxonProcessor(true);
  processor->setcwd(dataDir->c_str());
  SchemaValidator *val = nullptr;

  try {
    val = processor->newSchemaValidator();
  } catch (SaxonApiException &e) {
    const char *message = e.getMessage();
    if (message != nullptr) {
      cerr << "Exception thrown = " << message << endl;
    }
    sresult->failure++;
    sresult->failureList.push_back("testValidator6-a");
    return;
  }

  val->clearParameters(true);
  val->clearProperties();
  cout << endl << "Test 6: Validate Schema from string and export" << endl;
  string invalid_xml =
      "<?xml version='1.0'?><request><a/><!--comment--></request>";
  const char *sch1 =
      "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' "
      "elementFormDefault='qualified' "
      "attributeFormDefault='unqualified'><xs:element "
      "name='request'><xs:complexType><xs:sequence><xs:element name='a' "
      "type='xs:string'/><xs:element name='b' "
      "type='xs:string'/></xs:sequence><xs:assert test='count(child::node()) = "
      "3'/></xs:complexType></xs:element></xs:schema>";

  string doc1 = "<request "
                "xmlns='http://myexample/family'><Parent>John</"
                "Parent><Child1>Alice</Child1></request>";

  XdmNode *input = nullptr;
  try {
    input = processor->parseXmlFromString(doc1.c_str());

    val->setProperty("xsdversion", "1.1");

    val->registerSchemaFromString(sch1);
    val->exportSchema("exportedSchema.scm");
    cout << endl << "No errors found" << endl;
    sresult->success++;

  } catch (SaxonApiException &e) {
    cout << endl << "Error: Validation error found " << endl;
    const char *message = e.getMessage();
    const char *errorCode = e.getErrorCode();
    if (errorCode != nullptr) {
      cerr << "ErrorCode = " << errorCode << endl;
    }
    if (message != nullptr) {
      cerr << "Exception thrown = " << message << endl;
    }
    sresult->failure++;
    sresult->failureList.push_back("testValidator5");
  }
  delete input;
  delete val;
  delete processor;
}



void testValidatorToFile(const char *cwd,
                         const std::string *const dataDir,
                         sResultCount *sresult) {

  SaxonProcessor *processor = new SaxonProcessor(true);
  processor->setcwd(cwd);
  SchemaValidator *val = nullptr;
  XsdCompiler * xsdCompiler = nullptr;
  XsdSchema * xsdSchema1 = nullptr;
  XsdSchema * xsdSchema2 = nullptr;
  XsdSchema * xsdSchema3 = nullptr;

  try {
    xsdCompiler = processor->newXsdCompiler();

  } catch (SaxonApiException &e) {
    const char *message = e.getMessage();
    if (message != nullptr) {
      cerr << "Exception thrown = " << message << endl;
    }
    sresult->failure++;
    sresult->failureList.push_back("testValidatorToFile-a");
    return;
  }


  cout << endl
       << "Test 7: testValidatorToFile - Validate source file with schema file. i.e. family.xml and "
          "family.xsd and output result in file validatedDoc.xml" << endl;

  try {
    xsdSchema1 = xsdCompiler->compileFromFile(CppTestUtils::concat(2, dataDir->c_str(), "/family.xsd").c_str());
    xsdSchema2 = xsdCompiler->compileFromFile(CppTestUtils::concat(2, dataDir->c_str(), "/family-ext.xsd").c_str());
    xsdSchema3 = xsdSchema1->combine(xsdSchema2);
    val = xsdSchema3->newValidator();
    val->setProperty("!indent", "yes");
    val->setProperty("!omit-xml-declaration", "yes");
    val->setOutputFile(CppTestUtils::concat(2, dataDir->c_str(), "validatedDoc.xml").c_str());
    val->validate(CppTestUtils::concat(2, dataDir->c_str(), "/family.xml").c_str());
        if (CppTestUtils::exists("validatedDoc.xml")) {
          cout << "The file validatedDoc.xml exists" << endl;
          sresult->success++;
          /*if(CppTestUtils::CheckWord("validatedDoc.xml", "...")) {
            sresult->success++;
          } else {
            sresult->failure++;
            sresult->failureList.push_back("testTransformFromstring2Err2");

          }*/
          //remove("validatedDoc.xml");

        } else {
          cout << "The file validatedDoc.xml does not exist" << endl;

          sresult->failure++;
          sresult->failureList.push_back("testValidatorToFile");
        }


    cout << endl << "Doc1 is OK" << endl;
    sresult->success++;
  } catch (SaxonApiException &e) {
    cout << endl << "Error: Doc reported as invalid!" << endl;
    const char *message = e.getMessage();
    const char *errorCode = e.getErrorCode();
    if (errorCode != nullptr) {
      cerr << "ErrorCode = " << errorCode << endl;
    }
    if (message != nullptr) {

      cerr << "Exception thrown = " << message << endl;
    }
    sresult->failure++;
    sresult->failureList.push_back("testValidatorToFile");
  }
  delete xsdCompiler;
  delete xsdSchema1;
  delete xsdSchema2;
  delete xsdSchema3;
  delete val;
  delete processor;
}


void testXsdSchemaCompiler1(SaxonProcessor *processor, sResultCount *sresult) {
  cout << endl
       << "testXsdSchemaCompiler1: Test Validate Schema from string - invalid "
          "doc"
       << endl;
  string invalid_xml =       "<element><child1/><!-- missing child --><child3/></element>";
  const char *sch1 =
      "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n"
                       "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" elementFormDefault=\"qualified\">\n"
                       "\n"
                       "  <xs:element name=\"element\">\n"
                       "    <xs:complexType>\n"
                       "      <xs:sequence>\n"
                       "        <xs:element name=\"child1\"/>\n"
                       "        <xs:element name=\"child2\"/>\n"
                       "        <xs:element name=\"child3\"/>\n"
                       "      </xs:sequence>\n"
                       "      <xs:attribute name=\"requiredAttribute\" use=\"required\"/>\n"
                       "    </xs:complexType>\n"
                       "  </xs:element>\n"
                       "</xs:schema>\n";

  XsdCompiler * compiler = nullptr;
  SchemaValidator * validator = nullptr;
  XsdSchema * schema = nullptr;
  try {

    compiler = processor->newXsdCompiler();
    schema = compiler->compileFromString(sch1);

    delete compiler;

    if (schema!= nullptr) {
      validator = schema->newValidator();
    }
    //validator->setCaptureErrorHandler(true);


    validator->validateFromString(invalid_xml.c_str());



    sresult->failure++;
    sresult->failureList.push_back("testXsdSchemaCompiler1");
  } catch (SaxonApiException &e) {
    const char *message = e.getMessage();
    if (message != nullptr) {
      cerr << "Exception thrown in testXsdSchemaCompiler1a = " << message << endl;
    }
    string sresulti = string(message);
    if (sresulti.compare("The load() method has already been called") == 0) {
      sresult->failure++;
      sresult->failureList.push_back("testXsdSchemaCompiler1a");
    } else {
      sresult->success++;
    }
  }
  delete schema;
  delete validator;
}

int main(int argc, char *argv[]) {
  const std::string dataDir = argc > 1 ? string(argv[1]) : string();

  SaxonProcessor *processor = new SaxonProcessor(true);
  // setDebugMode(SaxonProcessor::sxn_environ->thread, 1);
  sResultCount *sresult = new sResultCount();
  cout << "Test: SchemaValidator with Saxon version=" << processor->version()
       << endl
       << endl;

  const char *cwd = nullptr;
  char buff[FILENAME_MAX]; // create string buffer to hold path
  GetCurrentDir(buff, FILENAME_MAX);
  processor->setcwd(buff);
  cwd = (const char *)buff;

  cout << "CWD = " << cwd << endl;

  // processor->setConfigurationProperty("xsdversion", "1.1");

  testValidator1(processor, sresult);
  testValidatorInParseXml(processor, sresult);
  testValidator2(sresult);

  testValidator3(processor, &dataDir, sresult);
  processor->setConfigurationProperty(
      "http://saxon.sf.net/feature/multipleSchemaImports", "on");
  testValidator4(cwd, &dataDir, sresult);
  testValidator4a(&dataDir, sresult);
  processor->setConfigurationProperty("xsdversion", "1.1");

  testValidator5(sresult);
  testValidator6(&dataDir, sresult);
  testValidatorToFile(cwd, &dataDir, sresult);
  testXsdSchemaCompiler1(processor, sresult);

  std::cout << "\nTest Results - Number of tests= "
            << (sresult->success + sresult->failure)
            << ", Successes = " << sresult->success
            << ",  Failures= " << sresult->failure << std::endl;

  std::list<std::string>::iterator it;
  std::cout << "Failed tests:" << std::endl;
  // Make iterate point to beginning and increment it one by one till it reaches
  // the end of list.
  for (it = sresult->failureList.begin(); it != sresult->failureList.end();
       it++) {
    // Print the contents
    std::cout << it->c_str() << std::endl;
  }

  delete processor;
  processor->release();
  delete sresult;
  return 0;
}
