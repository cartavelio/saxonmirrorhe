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
	
	val->registerSchemaFromString(sch1);

}

void testValidator2(){
  SaxonProcessor * processor = new SaxonProcessor(true);
  processor->setConfigurationProperty("xsdversion", "1.1");
  SchemaValidator * val = processor->newSchemaValidator();

  cout<<endl<<"Test 2: Validate Schema from string"<<endl;	
  string invalid_xml = "<?xml version='1.0'?><request><a/><!--comment--></request>";
  const char* sch1 = "<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified' attributeFormDefault='unqualified'><xs:element name='request'><xs:complexType><xs:sequence><xs:element name='a' type='xs:string'/><xs:element name='b' type='xs:string'/></xs:sequence><xs:assert test='count(child::node()) = 3'/></xs:complexType></xs:element></xs:schema>";


        string doc1 = "<Family xmlns='http://myexample/family'><Parent>John</Parent><Child>Alice</Child></Family>";

	 XdmNode * input = processor->parseXmlFromString(doc1.c_str());

	 if(input == nullptr) {
	     if(val->exceptionOccurred()) {


	     }
	     val->exceptionClear();
	     return;

	 }

	val->setSourceNode(input);

	
	
	val->registerSchemaFromString(sch1);

	val->validate();
	if(val->exceptionOccurred()) {
		cout<<endl<<"Doc is not valid!"<<endl;
	}

	delete val;

}

void testValidator3(SaxonProcessor * processor, SchemaValidator * val){
  processor->exceptionClear();
  val->clearParameters(true);
  val->clearProperties();
  cout<<endl<<"Test 3: Validate Schema from string"<<endl;	
  const char* sch1 = "<?xml version='1.0' encoding='UTF-8'?><schema targetNamespace='http://myexample/family' xmlns:fam='http://myexample/family' xmlns='http://www.w3.org/2001/XMLSchema'><element name='FamilyMember' type='string' /><element name='Parent' type='string' substitutionGroup='fam:FamilyMember'/><element name='Child' type='string' substitutionGroup='fam:FamilyMember'/><element name='Family'><complexType><sequence><element ref='fam:FamilyMember' maxOccurs='unbounded'/></sequence></complexType></element>  </schema>";
	
	val->registerSchemaFromString(sch1);
    val->setProperty("http://saxon.sf.net/feature/multipleSchemaImports", "on");
	val->validate("family.xml");
	if(!val->exceptionOccurred()) {
		cout<<endl<<"Doc1 is OK"<<endl;
	} else {
		cout<<endl<<"Error: Doc reported as invalid!"<<endl;
	}
}

void testValidator4(SaxonProcessor * processor, SchemaValidator * val){
  processor->exceptionClear();
  val->clearParameters(true);
  val->clearProperties();
  cout<<endl<<"Test 4: Validate source file with schema file. i.e. family.xml and family.xsd"<<endl;	

	val->registerSchemaFromFile("../data/family-ext.xsd");
       //val->registerSchema("family.xsd");
	val->registerSchemaFromFile("../data/family.xsd");
	val->validate("../data/family.xml");
	if(!val->exceptionOccurred()) {
		cout<<endl<<"Doc1 is OK"<<endl;
	} else {
	cout<<endl<<"Error: Doc reported as valid!"<<endl;
	}

}

void testValidator4a(SaxonProcessor * processor, SchemaValidator * val){
  processor->exceptionClear();
  val->clearParameters(true);
  val->clearProperties();
  cout<<endl<<"Test 4: Validate source file with schema file. i.e. family.xml and family.xsd to XdmNode"<<endl;	

	val->registerSchemaFromFile("../data/family-ext.xsd");
       //val->registerSchema("family.xsd");
	val->registerSchemaFromFile("../data/family.xsd");
	XdmNode * node = val->validateToNode("../data/family.xml");
	if(node != NULL) {
	    const char * valueStr = node->toString();
		if(!val->exceptionOccurred() && valueStr != nullptr) {
			cout<<endl<<"Doc1 is OK:"<<valueStr<<endl;
			delete valueStr;
	 	} else {
			cout<<endl<<"Error: Doc reported as valid!"<<endl;
		}
	} else {
	cout<<endl<<"Error: node is NULL"<<endl;
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

	 XdmNode * input = processor->parseXmlFromString(doc1.c_str());
	//val->setSourceNode(input);

	//val->setProperty("string", doc1.c_str());
	val->setProperty("xsdversion", "1.1");
	val->setParameter("node", (XdmValue *)input);

	val->registerSchemaFromString(sch1);

	val->setProperty("report-node", "true");
	//val->setProperty("report-file", "validation-report2.xml");	
	val->setProperty("verbose", "true");
	val->validate();
	XdmNode * node = val->getValidationReport(); 
	if(node != NULL) {
        const char * value = node->toString();
        if(value != nullptr) {
            cout << endl << "Validation Report: " << value << endl;
        } else {
            cout << endl << "Validation Report value is NULL" << endl;
        }

	} else {
		cout<<endl<<"Error: Validation Report is NULL - This should not be NULL. Probably no valid license file found."<<endl;
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

	 XdmNode * input = processor->parseXmlFromString(doc1.c_str());
	//val->setSourceNode(input);

	//val->setProperty("string", doc1.c_str());
	val->setProperty("xsdversion", "1.1");

	val->registerSchemaFromString(sch1);
	val->exportSchema("exportedSchema.scm");
    const char * error = val->checkException();

    if(error != NULL) {
       cout<<endl<<"Error: Validation error: "<<error<<endl;
    } else {
        cout<<endl<<"No errors found"<<endl;

    }
}

int main(int argc, char* argv[])
{

	SaxonProcessor * processor = new SaxonProcessor(true);
    setDebugMode(SaxonProcessor::sxn_environ->thread, 1);
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
