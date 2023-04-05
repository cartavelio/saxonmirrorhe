#include "../../Saxon.C.API/SaxonProcessor.h"
#include "../../Saxon.C.API/XdmValue.h"
#include "../../Saxon.C.API/XdmNode.h"
#include "CppTestUtils.h"

#include <string>

using namespace std;

void testValidator1(SchemaValidator * val){
	cout<<endl<<"Test Validate Schema from string"<<endl;	
 string invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
 const char* sch1 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified' attributeFormDefault='unqualified'><xs:element name='request'><xs:complexType><xs:sequence><xs:element name='a' type='xs:string'/><xs:element name='b' type='xs:string'/></xs:sequence><xs:assert test='count(child::node()) = 3'/></xs:complexType></xs:element></xs:schema>";


        string doc1 = "<Family xmlns='http://myexample/family'><Parent>John</Parent><Child>Alice</Child></Family>";
	try {
	val->registerSchemaFromString(sch1);
	} catch(SaxonApiException &e){
	    const char * message = e.getMessage();
	    if(message != nullptr) {
	        cerr<<"Exception thrown = "<<message<<endl;
	    }
	}

}

void testValidator2(){
  SaxonProcessor * processor = new SaxonProcessor(true);
  processor->setConfigurationProperty("xsdversion", "1.1");
  SchemaValidator * val = processor->newSchemaValidator();

  cout<<endl<<"Test 2: Validate Schema from string"<<endl;	
  string invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
  const char* sch1 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified' attributeFormDefault='unqualified'><xs:element name='request'><xs:complexType><xs:sequence><xs:element name='a' type='xs:string'/><xs:element name='b' type='xs:string'/></xs:sequence><xs:assert test='count(child::node()) = 3'/></xs:complexType></xs:element></xs:schema>";

  string doc1 = "<Family xmlns='http://myexample/family'><Parent>John</Parent><Child>Alice</Child></Family>";

  XdmNode * input = nullptr;
   try {
        input = processor->parseXmlFromString(doc1.c_str());

   } catch(SaxonApiException &e){
           	    const char * message = e.getMessage();
           	    if(message != nullptr) {
           	        cerr<<"Exception thrown = "<<message<<endl;
           	    }
   }


	val->setSourceNode(input);

	try{
	
	    val->registerSchemaFromString(sch1);

	    val->validate();
	    delete val;
	    cout<<endl<<"Doc is valid!"<<endl;
	}catch(SaxonApiException &e){
	    cout<<endl<<"Doc is not valid!"<<endl;
        const char * message = e.getMessage();
        if(message != nullptr) {
              cerr<<"Exception thrown = "<<message<<endl;
        }
    }




}

void testValidator3(SaxonProcessor * processor, SchemaValidator * val){
  processor->exceptionClear();
  val->clearParameters(true);
  val->clearProperties();
  cout<<endl<<"Test 3: Validate Schema from string"<<endl;	
  const char* sch1 = "<?xml version='1.0' encoding='UTF-8'?><schema targetNamespace='http://myexample/family' xmlns:fam='http://myexample/family' xmlns='http://www.w3.org/2001/XMLSchema'><element name='FamilyMember' type='string' /><element name='Parent' type='string' substitutionGroup='fam:FamilyMember'/><element name='Child' type='string' substitutionGroup='fam:FamilyMember'/><element name='Family'><complexType><sequence><element ref='fam:FamilyMember' maxOccurs='unbounded'/></sequence></complexType></element>  </schema>";
  try {
    val->registerSchemaFromString(sch1);
    val->setProperty("http://saxon.sf.net/feature/multipleSchemaImports", "on");
    val->validate("family.xml");
    cout<<endl<<"Doc1 is OK"<<endl;
  } catch(SaxonApiException &e){
    cout<<endl<<"Error: Doc reported as invalid!"<<endl;
    const char * message = e.getMessage();
    if(message != nullptr) {
              cerr<<"Exception thrown = "<<message<<endl;
    }

  }
}

void testValidator4(SaxonProcessor * processor, SchemaValidator * val){
  processor->exceptionClear();
  val->clearParameters(true);
  val->clearProperties();
  cout<<endl<<"Test 4: Validate source file with schema file. i.e. family.xml and family.xsd"<<endl;	

  try {
	val->registerSchemaFromFile("../data/family-ext.xsd");

	val->registerSchemaFromFile("../data/family.xsd");
	val->validate("../data/family.xml");
    cout<<endl<<"Doc1 is OK"<<endl;
  } catch(SaxonApiException &e){
    cout<<endl<<"Error: Doc reported as invalid!"<<endl;
    const char * message = e.getMessage();
    const char * errorCode = e.getErrorCode();
    if(errorCode != nullptr) {
        cerr<<"ErrorCode = "<<errorCode<<endl;
    }
    if(message != nullptr) {

        cerr<<"Exception thrown = "<<message<<endl;
    }

  }

}

void testValidator4a(SaxonProcessor * processor, SchemaValidator * val){
  processor->exceptionClear();
  val->clearParameters(true);
  val->clearProperties();
  cout<<endl<<"Test 4: Validate source file with schema file. i.e. family.xml and family.xsd to XdmNode"<<endl;	

  try{
	val->registerSchemaFromFile("../data/family-ext.xsd");
	val->registerSchemaFromFile("../data/family.xsd");
	XdmNode * node = val->validateToNode("../data/family.xml");

    const char * valueStr = node->toString();
	if(!val->exceptionOccurred() && valueStr != nullptr) {
			cout<<endl<<"Doc1 is OK:"<<valueStr<<endl;
			delete valueStr;
	 	} else {
			cout<<endl<<"Error: Doc reported as valid!"<<endl;
	}

  } catch(SaxonApiException &e) {
    cout<<endl<<"Error: Doc reported as invalid!"<<endl;
    const char * message = e.getMessage();
    const char * errorCode = e.getErrorCode();
    if(errorCode != nullptr) {
        cerr<<"ErrorCode = "<<errorCode<<endl;
    }
    if(message != nullptr) {

        cerr<<"Exception thrown = "<<message<<endl;
    }
    }



}

void testValidator5(SaxonProcessor * processor, SchemaValidator * val){
  processor->exceptionClear();
  val->clearParameters(true);
  val->clearProperties();
  cout<<endl<<"Test 5: Validate Schema from string"<<endl;	
  string invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
  const char* sch1 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified' attributeFormDefault='unqualified'><xs:element name='request'><xs:complexType><xs:sequence><xs:element name='a' type='xs:string'/><xs:element name='b' type='xs:string'/></xs:sequence><xs:assert test='count(child::node()) = 3'/></xs:complexType></xs:element></xs:schema>";


  string doc1 = "<request xmlns='http://myexample/family'><Parent>John</Parent><Child1>Alice</Child1></request>";
   try {
    XdmNode * input = processor->parseXmlFromString(doc1.c_str());

	val->setProperty("xsdversion", "1.1");
	val->setParameter("node", (XdmValue *)input);

	val->registerSchemaFromString(sch1);

	val->setProperty("report-node", "true");

	val->setProperty("verbose", "true");
	val->validate();
	XdmNode * node = val->getValidationReport();
    const char * value = node->toString();
    if(value != nullptr) {
            cout << endl << "Validation Report: " << value << endl;
        } else {
            cout << endl << "Validation Report value is NULL" << endl;
    }

   } catch (SaxonApiException &e){
        cout<<endl<<"Error: Doc reported as invalid!"<<endl;
        cout<<endl<<"Error: Validation Report is NULL - This should not be NULL. Probably no valid license file found."<<endl;
        const char * message = e.getMessage();
        const char * errorCode = e.getErrorCode();
        if(errorCode != nullptr) {
          cerr<<"ErrorCode = "<<errorCode<<endl;
        }
        if(message != nullptr) {
            cerr<<"Exception thrown = "<<message<<endl;
        }
	}

}


void testValidator6(SaxonProcessor * processor, SchemaValidator * val){
  processor->exceptionClear();
  val->clearParameters(true);
  val->clearProperties();
  cout<<endl<<"Test 6: Validate Schema from string and export"<<endl;
  string invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
  const char* sch1 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified' attributeFormDefault='unqualified'><xs:element name='request'><xs:complexType><xs:sequence><xs:element name='a' type='xs:string'/><xs:element name='b' type='xs:string'/></xs:sequence><xs:assert test='count(child::node()) = 3'/></xs:complexType></xs:element></xs:schema>";


        string doc1 = "<request xmlns='http://myexample/family'><Parent>John</Parent><Child1>Alice</Child1></request>";
    try {
	 XdmNode * input = processor->parseXmlFromString(doc1.c_str());

	val->setProperty("xsdversion", "1.1");

	val->registerSchemaFromString(sch1);
	val->exportSchema("exportedSchema.scm");
	 cout<<endl<<"No errors found"<<endl;
	} catch (SaxonApiException &e) {
        cout<<endl<<"Error: Validation error found "<<endl;
      const char * message = e.getMessage();
        const char * errorCode = e.getErrorCode();
        if(errorCode != nullptr) {
          cerr<<"ErrorCode = "<<errorCode<<endl;
        }
        if(message != nullptr) {
            cerr<<"Exception thrown = "<<message<<endl;
        }
	}

}

int main(int argc, char* argv[])
{

	SaxonProcessor * processor = new SaxonProcessor(true);
    //setDebugMode(SaxonProcessor::sxn_environ->thread, 1);
    cout<<"Test: SchemaValidator with Saxon version="<<processor->version()<<endl<<endl;

    char * cwd = nullptr;
    if(argc > 0) {
        cwd = argv[1];
    }
    char buff[FILENAME_MAX]; //create string buffer to hold path
    GetCurrentDir( buff, FILENAME_MAX );
    cout<<"CWD = "<< buff<<endl;
    if(cwd == nullptr) {
        cwd = (char *)malloc(strlen(buff)+1);
        strcpy(cwd, buff);
    }

    if(cwd != nullptr) {
        processor->setcwd(cwd); //set to the current working directory
    } else {
        processor->setcwd(buff);
    }

	//processor->setConfigurationProperty("xsdversion", "1.1");
	
	 SchemaValidator * validator = processor->newSchemaValidator();
	testValidator1(validator);
	testValidator2();

	testValidator3(processor, validator);
	processor->setConfigurationProperty("http://saxon.sf.net/feature/multipleSchemaImports", "on");
	SchemaValidator * validator2 = processor->newSchemaValidator();
	testValidator4(processor, validator2);
	testValidator4a(processor, validator2);
	processor->setConfigurationProperty("xsdversion", "1.1");
	SchemaValidator * validator3 = processor->newSchemaValidator();
	testValidator5(processor, validator3);
	testValidator6(processor, validator3);
	delete validator;
	delete validator2;
	delete validator3;
        processor->release();
	return 0;
}
