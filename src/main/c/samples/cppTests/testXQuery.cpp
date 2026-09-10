
#include "../../Saxon.C.API/SaxonProcessor.h"
#include "../../Saxon.C.API/XdmValue.h"
#include "../../Saxon.C.API/XdmItem.h"
#include "../../Saxon.C.API/XdmNode.h"
#include "../../Saxon.C.API/XdmAtomicValue.h"
#include "../../Saxon.C.API/XdmFunctionItem.h"
#include "../../Saxon.C.API/XdmMap.h"
#include "../../Saxon.C.API/XdmArray.h"
#include "CppTestUtils.h"
#include <string>

using namespace std;

#ifdef MEM_DEBUG
#define new new(__FILE__, __LINE__)
#endif


/*
* Test1: 
*/
void testxQuery1(SaxonProcessor * processor, XQueryProcessor * queryProc, sResultCount *sresult){
    cout<<endl<<"Test testXQuery1:"<<endl;
    queryProc->clearParameters();
    queryProc->clearProperties();
   queryProc->setProperty("s", "../data/cat.xml");

    queryProc->setProperty("qs", "<out>{count(/out/person)}</out>");

    const char * result = queryProc->runQueryToString();
    if(result != nullptr){
    	cout<<"Result :"<<result<<endl;
    	sresult->success++;
    	delete result;
    } else {
        sresult->failure++;
        sresult->failureList.push_back("testXQuery1");
        if(queryProc->exceptionOccurred()) {
            SaxonApiException *exception = queryProc->getException();
            if(exception != nullptr) {
                cerr << "Error: " << exception->getMessage() << endl;
            }
            queryProc->exceptionClear();
        }

	
    }

    queryProc->executeQueryToFile(nullptr, "catOutput.xml", nullptr);

    if(queryProc->exceptionOccurred()) {
        SaxonApiException *exception = queryProc->getException();
        if(exception != nullptr) {
            cerr << "Error: " << exception->getMessage() << endl;
        }
        queryProc->exceptionClear();
    }

    if (CppTestUtils::exists("catOutput.xml")) {
        cout<<"The file $filename exists"<<endl;
        sresult->success++;
        remove("catOutput.xml");
    } else {
        sresult->failure++;
        sresult->failureList.push_back("testXQuery1");
        cout<<"The file $filename does not exist"<<endl;

    }
    queryProc->clearParameters();
    queryProc->clearProperties();

}




void testxQuery_ICU(SaxonProcessor * processor, XQueryProcessor * queryProc, sResultCount *sresult){
    cout<<endl<<"Test testXQuery_ICU:"<<endl;
    queryProc->clearParameters();
    queryProc->clearProperties();

    queryProc->setProperty("qs", "format-integer(33,'Ww','cs')");

    const char * result = queryProc->runQueryToString();
    if(result != nullptr){
        cout<<"Result :"<<result<<endl;
        sresult->success++;
        delete result;
    } else {
        sresult->failure++;
        sresult->failureList.push_back("testXQuery_ICU");
        if(queryProc->exceptionOccurred()) {
            SaxonApiException *exception = queryProc->getException();
            if(exception != nullptr) {
                cerr << "Error: " << exception->getMessage() << endl;
            }
            queryProc->exceptionClear();
        }


    }


    queryProc->clearParameters();
    queryProc->clearProperties();

}

void testxQueryError(XQueryProcessor * queryProc, sResultCount *sresult){
 cout<<endl<<"Test testXQueryError-Test:"<<endl;
    queryProc->clearParameters();
    queryProc->clearProperties();
    //queryProc->setProperty("s", "cat.xml");

    queryProc->setProperty("qs", "<out>{count(/out/person)}</out>");

     queryProc->executeQueryToFile(nullptr, "catOutput.xml", nullptr);

		if (queryProc->exceptionOccurred()) {
            cout << "Exception found. " << endl;
            SaxonApiException * exception = queryProc->getException();
            if(exception != nullptr) {
                sresult->success++;
                const char *message = exception->getMessage();
                cout << "Error Message = " << message << endl;
            }
            queryProc->exceptionClear();
		} else {

            sresult->failure++;
            sresult->failureList.push_back("testXQueryError");

		}
    queryProc->clearParameters();
    queryProc->clearProperties();
}

void testXQueryError2(SaxonProcessor * processor, XQueryProcessor * queryProc, sResultCount *sresult){
 cout<<endl<<"Test testXQueryError-test2:"<<endl;
	queryProc->clearProperties();
	queryProc->clearParameters();
   queryProc->setProperty("s", "../data/cat.xml");

    queryProc->setProperty("qs", "<out>{count(/out/person)}<out>");

    const char * result = queryProc->runQueryToString();
   if(result != nullptr){
    	cout<<"Result :"<<result<<endl;
    	delete result;
       sresult->failure++;
       sresult->failureList.push_back("testXQueryError2");
    } else {

       if (queryProc->exceptionOccurred()) {
           SaxonApiException * exception = queryProc->getException();
           if(exception != nullptr) {
               sresult->success++;
               cout << "Exception found. " << endl;
               const char *message = queryProc->getErrorMessage();
               cout << "Error Message = " << message << endl;

           } else {
               cout << "Exception obj is NULL !!!!. " << endl;

           }
           queryProc->exceptionClear();
       } else {

           sresult->failure++;
           sresult->failureList.push_back("testXQueryError2");
       }

    }
    queryProc->clearProperties();
    queryProc->clearParameters();

}


void testDefaultNamespace(SaxonProcessor * processor, XQueryProcessor * queryProc, sResultCount *sresult) {
 cout<<endl<<"Test testDefaultNamespace:"<<endl;
	queryProc->clearProperties();
	queryProc->clearParameters();
	queryProc->declareNamespace("", "http://one.uri/");
	 XdmNode * input = processor->parseXmlFromString("<foo xmlns='http://one.uri/'><bar/></foo>");

    if(input == nullptr) {
        cout << "Source document is null." << endl;
        if(processor->exceptionOccurred()) {
            cerr<<processor->getErrorMessage()<<endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("testDefaultNamespace");
        return;

    }

	queryProc->setContextItem((XdmItem *)input);
	queryProc->setQueryContent("/foo");

	XdmValue * value = queryProc->runQueryToValue();

	if(value != nullptr && value->size() == 1) {
	    sresult->success++;
		cout<<"Test1: Result is ok size is "<<value->size()<<endl;
        delete value;
	} else {
        if (queryProc->exceptionOccurred()) {
            sresult->failure++;
            sresult->failureList.push_back("testDefaultNamespace");
            SaxonApiException * exception = queryProc->getException();
            if(exception != nullptr) {
                cout << "Exception found. " << endl;
                const char *message = queryProc->getErrorMessage();
                cout << "Error Message = " << message << endl;
                queryProc->exceptionClear();
            }
        }
	}
    queryProc->clearProperties();
    queryProc->clearParameters();
	delete input;

}

// Test that the XQuery compiler can compile two queries without interference
void testReusability(SaxonProcessor * processor, sResultCount *sresult){
	cout<<endl<<"Test test XQuery reusability:"<<endl;
	XQueryProcessor * queryProc2 = processor->newXQueryProcessor();

	queryProc2->clearProperties();
	queryProc2->clearParameters();

 	XdmNode * input = processor->parseXmlFromString("<foo xmlns='http://one.uri/'><bar xmlns='http://two.uri'>12</bar></foo>");

 	if(input == nullptr) {
        cout << "Source document is null." << endl;
        if(processor->exceptionOccurred()) {
            cerr<<processor->getErrorMessage()<<endl;
        }
 	    sresult->failure++;
 	    sresult->failureList.push_back("testReusability");
 	    return;

 	}

	queryProc2->declareNamespace("", "http://one.uri/");
	queryProc2->setQueryContent("declare variable $p as xs:boolean external; exists(/foo) = $p");

	queryProc2->declareNamespace("", "http://two.uri");
	queryProc2->setQueryContent("declare variable $p as xs:integer external; /*/bar + $p");

	queryProc2->setContextItem((XdmItem *)input);

	XdmAtomicValue * value1 = processor->makeBooleanValue(true);
  	queryProc2->setParameter("p",(XdmValue *)value1);
	XdmValue * val = queryProc2->runQueryToValue();

	if(val != nullptr){
	    if( ((XdmItem*)val->itemAt(0))->isAtomic()) {
	        sresult->success++;
            cout << "Test1: Result is atomic" << endl;
            XdmAtomicValue *atomic = (XdmAtomicValue *) val->itemAt(0);
            bool result1 = atomic->getBooleanValue();
            cout << "Test2: Result value=" << (result1 == true ? "true" : "false") << endl;
            cout << "PrimitiveTypeName of  atomic=" << atomic->getPrimitiveTypeName() << endl;
        } else {
            sresult->failure++;
            sresult->failureList.push_back("testReusability");
	    }
	    delete val;
	} else {
        if (queryProc2->exceptionOccurred()) {
            sresult->failure++;
            sresult->failureList.push_back("testReusability");
            SaxonApiException * exception = queryProc2->getException();
            if(exception != nullptr) {
                cout << "Exception found. " << endl;
                const char *message = queryProc2->getErrorMessage();
                cout << "Error Message = " << message << endl;
                queryProc2->exceptionClear();
            }
        }

        return;
	}

	queryProc2->setContextItem((XdmItem *)input);

	XdmAtomicValue * value2 = processor->makeLongValue(6);
    cout<<"PrimitiveTypeName of  value2="<<value2->getPrimitiveTypeName()<<endl;
	queryProc2->setParameter("p",(XdmValue *)value2);

	XdmValue * val2 = queryProc2->runQueryToValue();


	if(val2 != nullptr){
        cout<<"XdmValue size="<<val2->size()<<", "<<(val2->itemAt(0))->getStringValue()<<endl;
	    if(((XdmItem*)val2->itemAt(0))->isAtomic()) {
            sresult->success++;
            cout << "Test3: Result is atomic" << endl;
            XdmAtomicValue *atomic2 = (XdmAtomicValue *) (val2->itemAt(0));
            long result2 = atomic2->getLongValue();
            cout << "Result value=" << result2 << endl;
            cout << "PrimitiveTypeName of  atomic2=" << atomic2->getPrimitiveTypeName() << endl;
        }
	    delete val2;
	} else {
        if (queryProc2->exceptionOccurred()) {
            sresult->failure++;
            sresult->failureList.push_back("testReusability-2");
            SaxonApiException * exception = queryProc2->getException();
            if(exception != nullptr) {
                cout << "Exception found. " << endl;
                const char *message = queryProc2->getErrorMessage();
                cout << "Error Message = " << message << endl;
                queryProc2->exceptionClear();
            }
        }
	    delete value2;
	    delete queryProc2;
	    return;
	}

    if (queryProc2->exceptionOccurred()) {
        sresult->failure++;
        sresult->failureList.push_back("testReusability-3");
        SaxonApiException * exception = queryProc2->getException();
        if(exception != nullptr) {
            cout << "Exception found. " << endl;
            const char *message = queryProc2->getErrorMessage();
            cout << "Error Message = " << message << endl;
            queryProc2->exceptionClear();
        }
    }
    delete value2;
    delete input;
    delete queryProc2;
	
}


//Test requirement of license file - Test should fail
void testXQueryLineNumberError(XQueryProcessor * queryProc, sResultCount *sresult){
 cout<<endl<<"Test testXQueryLineNumberError:"<<endl;
	queryProc->clearProperties();
	queryProc->clearParameters();
    queryProc->setcwd(".");
   queryProc->setProperty("s", "../data/cat.xml");

    queryProc->setProperty("qs", "saxon:line-number((//person)[1])");

    const char * result = queryProc->runQueryToString();
    if(result != nullptr){
        sresult->success++;
    	cout<<"Result :"<<result<<endl;
    	delete result;
    } else {
	if (queryProc->exceptionOccurred()) {
        sresult->failure++;
        sresult->failureList.push_back("testXQueryLineNumberError");
		    cout<<"Exception found. "<<endl;
        SaxonApiException * exception = queryProc->getException();
        if(exception != nullptr) {
            const char *message = queryProc->getErrorMessage();
            cout << "Error Message = " << message << endl;
            queryProc->exceptionClear();
        }
		
		}
	
    }
    queryProc->clearProperties();
    queryProc->clearParameters();

}

//Test requirement of license file - Test should succeed
void testXQueryLineNumber(SaxonProcessor * processor, sResultCount *sresult){
  //SaxonProcessor * processor = new SaxonProcessor(true);
  processor->setConfigurationProperty("l", "on");
  XQueryProcessor * queryProc = processor->newXQueryProcessor();
 cout<<endl<<"testXQueryLineNumber:"<<endl;


    //queryProc->setQueryBaseURI(processor->getcwd());

    queryProc->setProperty("s", "../data/cat.xml");
   queryProc->declareNamespace("saxon","http://saxon.sf.net/");

    queryProc->setProperty("qs", "saxon:line-number(doc('../data/cat.xml')/out/person[1])"); ///out/person[1]

    const char * result = queryProc->runQueryToString();
    if(result != nullptr){
    	cout<<"Result :"<<result<<endl;
    	sresult->success++;
    } else {
	if (queryProc->exceptionOccurred()) {
        sresult->failure++;
        sresult->failureList.push_back("testXQueryLineNumber");
		    cout<<"Exception found."<<endl;
				const char * message = queryProc->getErrorMessage();
				if(message != nullptr) {
					cout<<"Error Message = "<<message<<endl;		
				}

				queryProc->exceptionClear();

		
		}
	
    }
    delete queryProc;

}

int main(int argc, char* argv[])
{

    const char * cwd = nullptr;
    if(argc > 0) {
        cwd = argv[1];
    }

    SaxonProcessor * processor = new SaxonProcessor(true);
    cout<<"Test: XQueryProcessor with Saxon version="<<processor->version()<<endl<<endl;

    char buff[FILENAME_MAX]; //create string buffer to hold path
    GetCurrentDir( buff, FILENAME_MAX );
    cout<<"CWD = "<< buff<<endl;
    if(cwd != nullptr) {
        processor->setcwd(cwd); //set to the current working directory
    } else {
        processor->setcwd(buff);
    }


    XQueryProcessor * query = processor->newXQueryProcessor();

    sResultCount *sresult = new sResultCount();

    testxQuery1(processor, query, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testxQueryError(query,sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testXQueryError2(processor, query,sresult);

    cout<<endl<<"============================================================="<<endl<<endl;
    testDefaultNamespace(processor, query,sresult);

    cout<<endl<<"============================================================="<<endl<<endl;
    testReusability(processor, sresult);
    cout<<endl<<"============================================================="<<endl<<endl;
    testXQueryLineNumberError(query,sresult);
    cout<<endl<<"============================================================="<<endl<<endl;
    testXQueryLineNumber(processor, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;
    testxQuery_ICU(processor, query, sresult);

    delete query;

    delete processor;
    processor->release();


    cout<<endl<<"======================== Test Results ========================"<<endl<<endl;

    std::cout << "\nTest Results - Number of tests= " << (sresult->success + sresult->failure) << ", Successes = "
              << sresult->success << ",  Failures= " << sresult->failure << std::endl;

    std::list<std::string>::iterator it;
    std::cout << "Failed tests:" << std::endl;
    // Make iterate point to beginning and increment it one by one until it reaches the end of list.
    for (it = sresult->failureList.begin(); it != sresult->failureList.end(); it++) {
        //Print the contents
        std::cout << it->c_str() << std::endl;

    }

    delete sresult;



#ifdef MEM_DEBUG
    SaxonProcessor::getInfo();
#endif
    return 0;
}
