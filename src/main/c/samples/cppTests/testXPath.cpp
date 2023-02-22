#include "../../Saxon.C.API/SaxonProcessor.h"
#include "../../Saxon.C.API/XdmValue.h"
#include "../../Saxon.C.API/XdmItem.h"
#include "../../Saxon.C.API/XdmNode.h"
#include "CppTestUtils.h"
#include <string>

using namespace std;



// Test case on the evaluate method in XPathProcessor. Here we test that we have morethan one XdmItem.
void testXPathSingle(SaxonProcessor * processor, XPathProcessor * xpath, sResultCount *sresult){

    cout<<endl<<"Test testXPathSingle:"<<endl;
    XdmNode * input = processor->parseXmlFromString("<out><person>text1</person><person>text2</person><person>text3</person></out>");

	xpath->setContextItem((XdmItem *)input);
	XdmItem * result = xpath->evaluateSingle("//person[1]");
	
	if(result == nullptr) {
		 printf("result is null \n");
        SaxonApiException * exception = xpath->getException();
        if(exception != nullptr) {
            const char *message = xpath->getErrorMessage();
            cout << "Error Message = " << message << endl;
            xpath->exceptionClear();
        }
        sresult->failure++;
        sresult->failureList.push_back("testXPathSingle");

	} else {
	    const char * messagei = result->getStringValue();
		cout<<"Number of items="<<result->size()<<endl;
		cout<<"String Value of result="<<messagei<<endl;
		sresult->success++;
		//delete messagei;
		//messagei = nullptr;
	}
	delete result;

	xpath->clearParameters();
	xpath->clearProperties();
	delete input;

	

}

// Test case on the evaluate method in XPathProcessor. Here we test that we have more than one XdmItem.
void testXPathValues(SaxonProcessor * processor, XPathProcessor * xpath, sResultCount *sresult){
     cout<<endl<<"Test testXPathValues:"<<endl; 
  	xpath->clearParameters();
  	xpath->clearProperties();
	 XdmNode * input = processor->parseXmlFromString("<out><person>text1</person><person>text2</person><person1>text3</person1></out>");
	if(input == nullptr) {
        SaxonApiException * exception = xpath->getException();
        if(exception != nullptr) {
            const char *message = xpath->getErrorMessage();
            cout << "Error Message = " << message << endl;
            xpath->exceptionClear();
        }
        sresult->failure++;
        sresult->failureList.push_back("testXPathValues");

	    return;

	}

	XdmNode ** children = input->getChildren();
	XdmNode ** children0 = children[0]->getChildren();

	int num = children[0]->getChildCount();

    for(int i=0; i<num;i++) {
        if(children0[i] == nullptr) {
            sresult->failure++;
            sresult->failureList.push_back("testXPathValues-1");
            return;
        }
        if(children0[i]->getNodeName() != nullptr) {
            cout << "node name:" << children0[i]->getNodeName() << endl;
        }
    }



    for(int i=0; i< children[0]->getChildCount(); i++) {
        delete children0[i];
    }
    delete [] children0;

    for(int i=0; i<input->getChildCount(); i++) {
        delete children[i];
    }

    delete [] children;



    xpath->setContextItem((XdmItem *)input);
    XdmValue * resultValues = xpath->evaluate("//person");
    if(resultValues == nullptr) {
         printf("result is null \n");
        SaxonApiException * exception = xpath->getException();
        if(exception != nullptr) {
            const char *message = xpath->getErrorMessage();
            cout << "Error Message = " << message << endl;
            xpath->exceptionClear();
        }
        sresult->failure++;
        sresult->failureList.push_back("testXPathValues");

    } else {
        cout<<"Number of items="<<resultValues->size()<<endl;
        bool nullFound = false;
        for(int i =0; i< resultValues->size();i++){
            XdmItem * itemi = resultValues->itemAt(i);
            if(itemi == nullptr) {
                cout<<"Item at position "<<i<<" should not be null"<<endl;
                nullFound = true;
                break;
            }
            cout<<"Item at "<<i<<" ="<<itemi->getStringValue()<<endl;
        }
        delete resultValues;
        if(!nullFound) {
            sresult->success++;
        } else {
            sresult->failure++;
            sresult->failureList.push_back("testXPathValues");
        }
    }
    delete input;
    xpath->clearParameters();
    xpath->clearProperties();

}

// Test case on the evaluate method in XPathProcessor. Here we test that we have morethan one XdmItem.
void testXPathAttrValues(SaxonProcessor * processor, XPathProcessor * xpath, sResultCount *sresult){
     cout<<endl<<"Test testXPathValues1:"<<endl; 
  	xpath->clearParameters();
  	xpath->clearProperties();
	 XdmNode * input = processor->parseXmlFromString("<out attr='valuex'><person attr1='value1' attr2='value2'>text1</person><person>text2</person><person1>text3</person1></out>");
	if(input == nullptr) {
        SaxonApiException * exception = xpath->getException();
        if(exception != nullptr) {
            const char *message = xpath->getErrorMessage();
            cout << "Error Message = " << message << endl;
            xpath->exceptionClear();
        }
        sresult->failure++;
        sresult->failureList.push_back("testXPathAttrValues");

	    return;

	}

	//cout<<"Name of attr1= "<<input->getAttributeValue("attr")<<endl;


	xpath->setContextItem((XdmItem *)input);
	XdmItem * result = xpath->evaluateSingle("(//person)[1]");
	
	if(result == nullptr) {
		 printf("result is null \n");
	} else {
		XdmNode *nodeValue = (XdmNode*)result;
		cout<<"Attribute Count= "<<nodeValue->getAttributeCount()<<endl;
		const char *attrVal = nodeValue->getAttributeValue("attr1");
		if(attrVal != nullptr) {
			cout<<"Attribute value= "<<attrVal<<endl;
		}
		
		XdmNode ** attrNodes = nodeValue->getAttributeNodes();
		if(attrNodes == nullptr) { return;}
	
		const char *name1 = attrNodes[0]->getNodeName();
		if(name1 != nullptr) {
			cout<<"Name of attr1= "<<name1<<endl;
		}
		XdmNode * parent = attrNodes[0]->getParent();
		if(parent != nullptr) {
			cout<<"Name of parent= "<<parent->getNodeName()<<endl;
		}
		XdmNode * parent1 = parent->getParent();
		if(parent1 != nullptr) {
			cout<<"Name of parent= "<<parent1->getNodeName()<<endl;
		}
		sresult->success++;
	//TODO test if attr value is not there
	}
	
	

}


void testXPathValues2(SaxonProcessor * processor, XPathProcessor * xpath, sResultCount *sresult){


    cout<<endl<<"Test testXPathValues2:"<<endl;
    XdmValue * resultValues = xpath->evaluate("1, 'string', true(), parse-xml-fragment(string-join((1 to 3) ! ('<item>' || . || '</item>')))/node(), array { 1 to 5 }, map { 'key1' : 1, 'key2' : 'foo' }");


    if(resultValues == nullptr) {
        printf("result is null \n");
        SaxonApiException * exception = xpath->getException();
        if(exception != nullptr) {
            const char *message = xpath->getErrorMessage();
            cout << "Error Message = " << message << endl;
            xpath->exceptionClear();
        }
        sresult->failure++;
        sresult->failureList.push_back("testXPathValues2");

    } else {
        cout << "Number of items=" << resultValues->size() << endl;
        const char * resultStr = resultValues->toString();
        if(resultStr != nullptr) {
            cout << "toString()=" << resultValues->toString() << endl;
            delete resultStr;
        }

    }
}

// Test case on the evaluate method in XPathProcessor. Here we test that we have morethan one XdmItem.
void testXPathOnFile(SaxonProcessor * processor, XPathProcessor * xpath, sResultCount *sresult){
    	 cout<<endl<<"Test testXPath with file source:"<<endl;
  	xpath->clearParameters(true);
  	xpath->clearProperties(); 
	xpath->setContextFile("../data/cat.xml");

	XdmValue * resultValues = xpath->evaluate("//person");
	
	if(resultValues == nullptr) {
		 printf("result is null \n");
        SaxonApiException * exception = xpath->getException();
        if(exception != nullptr) {
            const char *message = xpath->getErrorMessage();
            cout << "Error Message = " << message << endl;
            xpath->exceptionClear();
        }
        sresult->failure++;
        sresult->failureList.push_back("testXPathOnFile");
	} else {
		cout<<"Number of items="<<resultValues->size()<<endl;
		for(int i =0; i< resultValues->size();i++){
			XdmItem * itemi = resultValues->itemAt(i);
			if(itemi == nullptr) {
				cout<<"Item at position "<<i<<" should not be null"<<endl;
				break;
			}
			cout<<"Item at "<<i<<" ="<<itemi->getStringValue()<<endl;		
		}
	}
	delete resultValues;
	

}

// Test case on the evaluate method in XPathProcessor. Here we test that we have morethan one XdmItem.
void testXPathOnFile2(XPathProcessor * xpath, sResultCount *sresult){
    	 cout<<endl<<"Test testXPath with file source-2:"<<endl;
  	xpath->clearParameters();
  	xpath->clearProperties(); 
	xpath->setContextFile("../data/cat.xml");

	XdmItem * result = xpath->evaluateSingle("//person[1]");
	
	if(result == nullptr) {
		 printf("result is null \n");
        SaxonApiException * exception = xpath->getException();
        if(exception != nullptr) {
            const char *message = xpath->getErrorMessage();
            cout << "Error Message = " << message << endl;
            xpath->exceptionClear();
        }
        sresult->failure++;
        sresult->failureList.push_back("testXPathOnFile2");
        return;
	} else {
		cout<<"Number of items="<<result->size()<<endl;
		if(result->isAtomic()) {
			cout<<"XdmItem is atomic - Error"<<endl;
		} else {
			cout<<"XdmItem is not atomic"<<endl;
		}
		XdmNode *node = (XdmNode*)result;
		if(node->getNodeKind() == ELEMENT){
		   cout<<"Result is a ELEMENT"<<endl;
		   cout<<"Node name: "<<node->getNodeName()<<endl;
		} else {
			cout<<"Node is of kind:"<<node->getNodeKind()<<endl;
		}
		const char * baseURI = node->getBaseUri();
		if(baseURI != nullptr) {
			cout<<"baseURI of node: "<<baseURI<<endl;
		}
		sresult->success++;
	}
	delete result;
}

int main()
{

    SaxonProcessor * processor = new SaxonProcessor(false);
    XPathProcessor * xpathProc = processor->newXPathProcessor();
    sResultCount *sresult = new sResultCount();
    cout<<endl<<"Test: XPathProcessor with Saxon version="<<processor->version()<<endl; 
    testXPathSingle(processor, xpathProc, sresult);
    testXPathValues(processor, xpathProc, sresult);
    testXPathAttrValues(processor, xpathProc, sresult);
    testXPathOnFile(processor, xpathProc, sresult);
    testXPathOnFile2(xpathProc, sresult);
    testXPathValues2(processor, xpathProc, sresult);

    std::cout << "\nTest Results - Number of tests= " << (sresult->success + sresult->failure) << ", Successes = "
              << sresult->success << ",  Failures= " << sresult->failure << std::endl;

    std::list<std::string>::iterator it;
    std::cout << "Failed tests:" << std::endl;
// Make iterate point to begining and incerement it one by one till it reaches the end of list.
    for (it = sresult->failureList.begin(); it != sresult->failureList.end(); it++) {
        //Print the contents
        std::cout << it->c_str() << std::endl;

    }

    delete xpathProc;
    delete processor;

     processor->release();
     delete sresult;


#ifdef MEM_DEBUG
    SaxonProcessor::getInfo();
#endif
    return 0;
}
