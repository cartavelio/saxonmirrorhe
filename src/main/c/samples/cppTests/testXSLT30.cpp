
#include <sstream>
#include <stdio.h>

#include "../../Saxon.C.API/SaxonProcessor.h"
#include "../../Saxon.C.API/XdmValue.h"
#include "../../Saxon.C.API/XdmItem.h"
#include "../../Saxon.C.API/XdmNode.h"
#include "../../Saxon.C.API/XdmFunctionItem.h"
#include "../../Saxon.C.API/DocumentBuilder.h"
#include "../../Saxon.C.API/XdmMap.h"
#include "../../Saxon.C.API/XdmArray.h"
//#include "cppExtensionFunction.h"
#include "CppTestUtils.h"
#include <string>
#include <thread>




using namespace std;


#ifdef MEM_DEBUG
#define new new(__FILE__, __LINE__)
#endif





/*char fname[] = "_nativeCall";
char funcParameters[] = "(Ljava/lang/String;[Ljava/lang/Object;[Ljava/lang/String;)Ljava/lang/Object;";

JNINativeMethod cppMethods[] =
        {
                {
                        fname,
                        funcParameters,
                        (void *) &cppExtensionFunction::cppNativeCall
                }
        }; */


/*
* Test transform to String. Source and stylesheet supplied as arguments
*/
void testApplyTemplatesString1(Xslt30Processor *trans, sResultCount *sresult) {

    cout << "Test: testApplyTemplatesString1:" << endl;
//    trans->setupXslMessage(false);

    XsltExecutable *executable = trans->compileFromFile("../data/test.xsl");

    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testApplyTemplatesString1");
        cerr << "testApplyTemplatesString1 NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testApplyTemplatesString1 error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }
    executable->setInitialMatchSelectionAsFile("../data/cat.xml");

    const char *output = executable->applyTemplatesReturningString();
    if (output == nullptr) {
        printf("result is null ====== FAIL ====== \n");
        sresult->failure++;
        fflush(stdout);
        sresult->failureList.push_back("testApplyTemplatesString1-0");
    } else if (string(output).find(string("<out>text2</out>")) != std::string::npos) {
        printf("%s", output);
        printf("result is OK \n");
        sresult->success++;
        delete output;
    } else {
        printf("result is null ====== FAIL ====== \n");
        sresult->failure++;
        sresult->failureList.push_back("testApplyTemplatesString1-1");
    	std::cout<<"output="<<output<<std::endl;
        delete output;
    }
    fflush(stdout);
    delete executable;

}

/*
* Test transform to String. Source and stylesheet supplied as arguments
*/
void testTransformToStringExtensionFunc(const char * cwd,SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {

    /*cout << endl << "Test: TransformToStringExtensionFunc:" << endl;

    bool nativeFound = processor->registerNativeMethods(SaxonProcessor::sxn_environ->env,
                                                        "com/saxonica/functions/extfn/cpp/NativeCall",
                                                        cppMethods, sizeof(cppMethods) / sizeof(cppMethods[0]));

    XsltExecutable * executable = trans->compileFromFile("testExtension.xsl");

    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("TransformToStringExtensionFunc");
        cerr << "TransformToStringExtensionFunc NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"TransformToStringExtensionFunc error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }
    executable->setcwd(cwd);
    std::string libStr= cwd +  "cppExtensionFunction";
    executable->setProperty("extc",  libStr);

    if (nativeFound) {
        const char *output = executable->transformFileToString("../data/cat.xml");

        if (output == nullptr) {

            printf("result is null ====== FAIL ======  \n");
            sresult->failure++;
            sresult->failureList.push_back("testTransformToStringExtensionFunc");
        } else {
            sresult->success++;
            printf("result is OK \n");
        }
        fflush(stdout);
        delete output;
    } else {
        printf("native Class not found ====== FAIL ====== ");
        sresult->failure++;
        sresult->failureList.push_back("testTransformToStringExtensionFunc");
    }
    delete executable;*/
}


/*
* Test transform to String. stylesheet supplied as argument. Source supplied as XdmNode
*/
void testApplyTemplatesString2(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {

    cout << "Test: testApplyTemplatesString2:" << endl;


    XdmNode *input = processor->parseXmlFromFile("../data/cat.xml");

    if (input== nullptr) {
        cout << "Source document is null." << endl;
        if(processor->exceptionOccurred()) {
            cerr<<processor->getErrorMessage()<<endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("testApplyTemplatesString2");
        return;

    }
    XsltExecutable * executable = trans->compileFromFile("../data/test.xsl");
    if(executable == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testApplyTemplatesString2");
        if (trans->exceptionOccurred()) {
            cerr <<"testApplyTemplatesString2 error: "<< trans->getErrorMessage() << endl;
        }
        delete input;
        return;
    }

    executable->setInitialMatchSelection((XdmValue *) input);

    const char *output = executable->applyTemplatesReturningString();
    if (output== nullptr) {
        printf("result is null ====== FAIL ======  \n");
        sresult->failureList.push_back("testApplyTemplatesString2");
        sresult->failure++;
    } else {
        sresult->success++;
        printf("%s", output);
        printf("result is OK \n");
        delete output;
    }
    fflush(stdout);
    delete input;
    delete executable;
}

/*
* Test transform to String. stylesheet supplied as argument. Source supplied as XdmNode
Should be error. Stylesheet file does not exist
*/
void testApplyTemplates2a_Error(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {

    cout << "Test: TransformToString2a_Error:" << endl;

    XdmNode *input = processor->parseXmlFromFile("../data/cat.xml");

    if (input == nullptr) {
        cout << "Source document is null. ====== FAIL ======" << endl;
        if(processor->exceptionOccurred()) {
            cerr<<processor->getErrorMessage()<<endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("testApplyTemplates2a_Error");
        trans->exceptionClear();
        return;
    }
    XsltExecutable * executable = trans->compileFromFile("test-error.xsl");

    if(executable== nullptr) {
        printf("Expected result is null \n");
        sresult->success++;

        if (trans->exceptionOccurred()) {
            cerr <<"testApplyTemplates2a_Error error: "<< trans->getErrorMessage() << endl;
        }
        delete input;
        trans->exceptionClear();
        return;
    }

    sresult->failure++;
    sresult->failureList.push_back("testApplyTemplates2a_Error");
    fflush(stdout);

    delete executable;

}

/*
* Test transform to String. stylesheet supplied as argument. Source supplied as XdmNode
Should be error. Source file does not exist
*/
void testTransformToString2b(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: TransformToString2b:" << endl;

    const char * result  = trans->transformFileToString("cat-error.xml", "test-error.xsl");

    if(result== nullptr) {
        if(trans->exceptionOccurred()) {
            cerr<<"Expected failure = "<<trans->getErrorMessage()<<endl;
        }
        sresult->success++;
        trans->exceptionClear();
        return;
    }else {
        delete result;
        sresult->failure++;
        sresult->failureList.push_back("testTransformToString2b");
        cerr << "testTransformToString2b NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"Error: "<< trans->getErrorMessage() << endl;
        }
        trans->exceptionClear();
        return;
    }


}


/*
* Test transform to String. stylesheet supplied as argument. Source supplied as xml string
and integer parmater created and supplied
*/
void testTransformToString3(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: testTransformToString3" << endl;

    XdmNode *inputi = processor->parseXmlFromString(
            "<out><person>text1</person><person>text2</person><person>text3</person></out>");

    if (inputi== nullptr) {
        cout << "Source document inputi is null." << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testTransformToString3 error: "<< trans->getErrorMessage() << endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("testTransformToString3");

        return;
    }

    XdmAtomicValue *value1 = processor->makeIntegerValue(10);

    if (value1== nullptr) {
        cout << "value1 is null." << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testTransformToString3 error: "<< trans->getErrorMessage() << endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("testTransformToString3");
        delete inputi;
        return;
    }

    XsltExecutable * executable = trans->compileFromFile("../data/test.xsl");

    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testTransformToString3");
        cerr << "testTransformToString3 NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testTransformToString3 error: "<< trans->getErrorMessage() << endl;
        }
        delete value1;
        return;
    }


    executable->setParameter("numParam", (XdmValue *) value1);

    executable->setInitialMatchSelection((XdmNode *) inputi);
    const char *output = executable->applyTemplatesReturningString();
    if (output == nullptr) {
        printf("result is null ====== FAIL ====== \n");
        sresult->failureList.push_back("testTransformToString3");
    } else {
        printf("%s", output);
        printf("result is OK \n");
        delete output;
    }
    fflush(stdout);

    delete value1;
    delete inputi;
    inputi = nullptr;
    delete executable;

}

/*
* Test transform to String. stylesheet supplied as argument. Source supplied as xml string
and integer parmater created and supplied
*/
void testTransformToString4(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {

    cout <<  "Test: testTransformToString4:" << endl;

    XdmNode *input = processor->parseXmlFromString(
            "<out><person>text1</person><person>text2</person><person>text3</person></out>");

    if (input== nullptr) {
        cout << "Source document is null. ====== FAIL ====== " << endl;
        sresult->failure++;
        sresult->failureList.push_back("testTransformToString4");
        return;
    }


    XdmValue *values = new XdmValue();
    XdmAtomicValue * a1 = processor->makeIntegerValue(10);
    XdmAtomicValue * a2 = processor->makeIntegerValue(5);
    XdmAtomicValue * a3 = processor->makeIntegerValue(6);
    XdmAtomicValue * a4 = processor->makeIntegerValue(7);
    values->addXdmItem(a1);
    values->addXdmItem(a2);
    values->addXdmItem(a3);
    values->addXdmItem(a4);


    XdmNode *sheet = processor->parseXmlFromFile("../data/test2.xsl");
    if(sheet == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testTransformToString4");
        if (processor->exceptionOccurred()) {
            cerr <<"testTransformToString4 error in compiling test2.xsl: "<< processor->getErrorMessage() << endl;
        }
        delete input;
        delete values;

    }
    XsltExecutable *executable = trans->compileFromXdmNode(sheet);

    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testTransformToString4");
        cerr << "testTransformToString4 NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testTransformToString4 error: "<< trans->getErrorMessage() << endl;
        }
        delete values;
        return;
    }

    executable->setParameter("values", (XdmValue *) values);
    executable->setInitialMatchSelection((XdmNode *) input);
    const char *output = executable->applyTemplatesReturningString();
    if (output== nullptr) {
        printf("result is null \n");
        sresult->failure++;
        sresult->failureList.push_back("testTransformToString4");
    } else {
        printf("%s", output);
        printf("result is OK \n");
        delete output;
    }
    fflush(stdout);
    delete sheet;
    delete executable;
    delete input;
    delete values;

}

void testTransformFromstring(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {
    cout << endl << "Test: testTransfromFromstring: "<< endl;

    if(processor == nullptr) {
        cout<<" processor is null"<<endl;
        return;

    }
    XdmNode *input = processor->parseXmlFromString(
            "<out><person>text1</person><person>text2</person><person>text3</person></out>");

    if(input == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testTransformFromstring");
        cerr << "testTransformFromstring NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testTransformFromstring error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }

    XsltExecutable * executable = trans->compileFromString(
            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='2.0'>       <xsl:param name='values' select='(2,3,4)' /><xsl:output method='xml' indent='yes' /><xsl:template match='*'><output><xsl:for-each select='$values' ><out><xsl:value-of select='. * 3'/></out></xsl:for-each></output></xsl:template></xsl:stylesheet>");

    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testTransformFromstring");
        cerr << "testTransformFromstring NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testTransformFromstring error: "<< trans->getErrorMessage() << endl;
        }
        delete input;
        return;
    }

    const char *output = executable->transformToString((XdmNode *) input);
    if (output== nullptr) {
        printf("result is null ====== FAIL ====== \n");
        sresult->failure++;
        sresult->failureList.push_back("testTransformFromString");
        if (trans->exceptionOccurred()) {
            cerr <<"testTransformFromstring error: "<< trans->getErrorMessage() << endl;
        }
    } else {
        printf("%s", output);
        printf("result is OK \n");
        sresult->success++;
        delete output;
    }
    fflush(stdout);
    delete input;
    delete executable;


}

//Test case has error in the stylesheet
void testTransformFromstring2Err(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {
    cout << endl << "Test: testTransfromFromstring2-Error:" << endl;

    XsltExecutable * executable = trans->compileFromString(
            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='2.0'>       <xsl:param name='values' select='(2,3,4)' /><xsl:output method='xml' indent='yes' fail='no' /><xsl:template match='*'><output><xsl:for-each select1='$values' ><out><xsl:value-of select1='. * 3'/></out></xsl:for-each></output></xsl:template></xsl:stylesheet>");

    if(executable== nullptr) {
        sresult->success++;

        if (trans->exceptionOccurred()) {
            SaxonApiException * exception = trans->getException();
            cerr <<"Error expected: "<< exception->getMessage() << endl;

            const char * smessages = exception->getCombinedStaticErrorMessages();
            cerr <<"Static Error count: "<< exception->staticErrorCount() << endl;
            if(smessages != nullptr) {
                cerr <<"Static Error expected: "<< smessages << endl;
            } else {
                cerr <<"Static Errors is null: "<< endl;
            }
        }
        trans->exceptionClear();
        return;
    }


    sresult->failure++;
    sresult->failureList.push_back("testTransfromFromstring2-Error");
    delete executable;

}

void testTrackingOfValueReference(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: TrackingOfValueReference:" << endl;
    ostringstream test;
    ostringstream valStr;
    ostringstream name;
    for (int i = 0; i < 10; i++) {
        test << "v" << i;
        valStr << "<out><person>text1</person><person>text2</person><person>text3</person><value>" << test.str()
               << "</value></out>";
        name << "value" << i;

        XdmValue *values = (XdmValue *) processor->parseXmlFromString(valStr.str().c_str());
        //cout<<"Name:"<<name.str()<<", Value:"<<values->getHead()->getStringValue()<<endl;
        trans->setParameter(name.str().c_str(), values);
        test.str("");
        valStr.str("");
        name.str("");

        if(values == nullptr) {
            cerr<<"TrackingOfValueReference failed to create XdmNode object" <<endl;
            sresult->failure++;
            sresult->failureList.push_back("TrackingOfValueReference");
            return;
        }
    }

    std::map<std::string, XdmValue *> parMap = trans->getParameters();
    if (parMap.size() > 0) {
        //cout << "Parameter size: " << parMap.size() << endl;
        //cout << "Parameter size: " << parMap.size()<< endl;//", Value:"<<trans->getParameters()["value0"]->getHead()->getStringValue()<<endl;
        ostringstream name1;
        for (int i = 0; i < 10; i++) {
            name1 << "sparam:value" << i;
            cout << " i:" << i << " Map size:" << parMap.size() << ", ";
            XdmValue *valuei = parMap[name1.str()];
            if (valuei != nullptr) {
                cout << name1.str();
                if (valuei->itemAt(0) != nullptr)
                    cout << "= " << valuei->itemAt(0)->getStringValue();
                cout << endl;

            } else {
                sresult->failure++;
                std::cerr << "trackingValueReference ====== FAIL ======" << std::endl;
                sresult->failureList.push_back("testTrackingOfValueReference");
                return;
            }
            name1.str("");
        }
    }
    if(parMap.size() > 0 ) {
        cerr<<"Deleting map"<<endl;
        trans->clearParameters(true);
    }
    sresult->success++;
}

/*Test case should be error.*/
void testTrackingOfValueReferenceError(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {
    trans->clearParameters();

    cout << endl << "Test: TrackingOfValueReference-Error:" << endl;
    cout << "Parameter Map size: " << (trans->getParameters().size()) << endl;
    ostringstream test;
    ostringstream valStr;
    ostringstream name;
    for (int i = 0; i < 2; i++) {
        test << "v" << i;
        valStr << "<out><person>text1</person><person>text2</person><person>text3</person><value>" << test.str()
               << "<value></out>";
        name << "value" << i;

        XdmValue *values = (XdmValue *) processor->parseXmlFromString(valStr.str().c_str());
        if(values == nullptr) {
            cerr<<"values NULL ====== FAIL ======="<<endl;
            processor->exceptionClear();
        } else {
            trans->setParameter(name.str().c_str(), values);
        }
        test.str("");
        valStr.str("");
        name.str("");
    }
    std::map<std::string, XdmValue *> parMap = trans->getParameters();
    cout << "Parameter Map size: " << parMap.size() << endl;

    ostringstream name1;
    bool errorFound = false;
    for (int i = 0; i < 2; i++) {
        name1 << "sparam:value" << i;
        cout << " i:" << i << " Map size:" << parMap.size() << ", ";
        try {

            XdmValue *valuei = parMap.at(name1.str());
            if (valuei != nullptr) {
                cout << name1.str();
                if (valuei->itemAt(0) != nullptr)
                    cout << "= " << valuei->itemAt(0)->getStringValue();
                cout << endl;
            }
        } catch (const std::out_of_range &oor) {
            cout << "Out of range exception occurred. Exception " << endl;
            if (!errorFound) {
                sresult->success++;
                errorFound = true;

                return;
            }
        }
        name1.str("");
    }
    sresult->failure++;
    sresult->failureList.push_back("testTrackingOfValueReferenceError");

}

void testValidation(Xslt30Processor *trans, sResultCount *sresult) {
    trans->clearParameters();



    XsltExecutable * executable = trans->compileFromString(
            "<?xml version='1.0'?><xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'                 xmlns:xs='http://www.w3.org/2001/XMLSchema' version='3.0' exclude-result-prefixes='#all'>     <xsl:import-schema><xs:schema><xs:element name='x' type='xs:int'/></xs:schema></xsl:import-schema> <xsl:template name='main'>          <xsl:result-document validation='strict'> <x>3</x>   </xsl:result-document>    </xsl:template>    </xsl:stylesheet>");


    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testValidation");
        if (trans->exceptionOccurred()) {
            cerr <<"Error message: "<< trans->getErrorMessage() << endl;
        }
        trans->exceptionClear();
        return;
    }



    const char *rootValue = executable->callTemplateReturningString("main");


    if (rootValue== nullptr) {
        std::cout << "NULL found" << std::endl;
        sresult->failure++;
        sresult->failureList.push_back("testValidation");
        return;

    } else {
        std::cout << "Result=" << rootValue << endl;
        sresult->success++;
    }
    delete executable;
}



void testContext2NotRootNamedTemplate(SaxonProcessor * saxonproc, Xslt30Processor *trans, sResultCount *sresult){
     trans->clearParameters();

    cout << endl << "Test: testContext2NotRootNamedTemplate " << endl;
    XdmNode * input_ = saxonproc->parseXmlFromString("<doc><e>text</e></doc>");
    XsltExecutable * executable = trans->compileFromString("<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:variable name='x' select='.'/><xsl:template match='/'>errorA</xsl:template><xsl:template name='main'>[<xsl:value-of select='$x'/>]</xsl:template></xsl:stylesheet>");
    executable->setGlobalContextItem(input_);
    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testContext2NotRootNamedTemplate");
        if (trans->exceptionOccurred()) {
            cerr <<"Error message: "<< trans->getErrorMessage() << endl;
        }
        trans->exceptionClear();
        return;
    }


    executable->setGlobalContextItem(input_);
    XdmValue * result = executable->callTemplateReturningValue("main");
    if (result== nullptr) {
            if (executable->exceptionOccurred()) {
                SaxonApiException *exception = executable->getException();
                cerr << "Error: " << exception->getMessage() << endl;
                delete exception;
            }
            sresult->failure++;
            sresult->failureList.push_back("testContext2NotRootNamedTemplate");
            return;
    }
    XdmItem * item1 = result->getHead();
    const char * resultStr = item1->getStringValue();

    cerr<<"result = "<<resultStr<<std::endl;

    const char * result2Str = executable->callTemplateReturningString("main");
    if (result2Str == nullptr) {
            if (executable->exceptionOccurred()) {
                SaxonApiException *exception = executable->getException();
                cerr << "Error: " << exception->getMessage() << endl;
                delete exception;
            }
            sresult->failure++;
            sresult->failureList.push_back("testContext2NotRootNamedTemplate");
            return;
    }


    cerr<<"result2 = "<<result2Str<<std::endl;
    delete result2Str;
    delete item1;

    delete result;
    delete input_;
    delete executable;
  }

void testXdmNodeOutput(Xslt30Processor *trans, sResultCount *sresult) {

    std::cout << "testXdmNodeOutput" << std::endl;
    XsltExecutable * executable = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template name='go'><a/></xsl:template></xsl:stylesheet>");


    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testXdmNodeOutput");
        if (trans->exceptionOccurred()) {
            cerr <<"Error message: "<< trans->getErrorMessage() << endl;
        }
        trans->exceptionClear();
        return;
    }

    XdmValue *rootValue = executable->callTemplateReturningValue("go");
    if (rootValue== nullptr) {
        if (executable->exceptionOccurred()) {
            SaxonApiException *exception = executable->getException();
            cerr << "Error: " << exception->getMessage() << endl;
            delete exception;
        }
        sresult->failure++;
        sresult->failureList.push_back("testXdmNodeOutput-0.0");
        return;
    }
    XdmItem *rootItem = rootValue->getHead();
    if (rootItem== nullptr) {

        cout << "Result is null ====== FAIL ====== " << endl;
        sresult->failure++;
        sresult->failureList.push_back("testXdmNodeOutput-0");
        delete rootValue;
        delete executable;
        return;
    }
    XdmNode *root = (XdmNode *) rootItem;
    if (root->getNodeKind() == DOCUMENT) {
        cout << "Result is a Document" << endl;
    } else {
        cout << "Node is of kind:" << root->getNodeKind() << endl;
    }
    const char *result = executable->callTemplateReturningString("go");
    if (string(result).find(string("<a/>")) != std::string::npos) {
        sresult->success++;
        delete result;
    } else {
        //TODO - this test case prints the XML declaration. Check if this correct
        sresult->failure++;
        cout << "testXdmNodeOutputAndString ======= FAIL========" << endl;
        sresult->failureList.push_back("testXdmNodeOutput");
    }
    delete rootValue;
    delete executable;

}

void exampleSimple1(Xslt30Processor *proc, sResultCount *sresult) {
    cout << "ExampleSimple1 taken from PHP:" << endl;

    XsltExecutable * executable = proc->compileFromFile("../php/xsl/foo.xsl");

    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("exampleSimple1");
        cerr << "exampleSimple1 NULL found" << endl;
        if (proc->exceptionOccurred()) {
            cerr <<"exampleSimple1 error: "<< proc->getErrorMessage() << endl;
        }
        return;
    }
    executable->setInitialMatchSelectionAsFile("../php/xml/foo.xml");
    const char *result = executable->applyTemplatesReturningString();
    if (result != NULL) {
        cout << result << endl;
        sresult->success++;
        delete result;
    } else {
        cout << "Result is null ====== FAIL ====== " << endl;
        sresult->failureList.push_back("exampleSimple1");
        sresult->failure++;
    }
    proc->clearParameters();
    delete executable;
}

void exampleSimple1Err(Xslt30Processor *proc, sResultCount *sresult) {
    cout << "ExampleSimple1Err taken from PHP:" << endl;

    XsltExecutable * executable = proc->compileFromFile("err.xsl");
    if(executable == nullptr || proc->exceptionOccurred()) {
        if( proc->exceptionOccurred()) {
            cout << "Error Message= "<< proc->getErrorMessage() << endl;
        }
        cout << "Result expected as null " << endl;
        proc->exceptionClear();
        sresult->success++;
        return;
    } else {
        sresult->failure++;
        sresult->failureList.push_back("exampleSimple1Err");

        delete executable;
    }

}


void exampleSimple2(Xslt30Processor *proc, sResultCount *sresult) {
    cout << "<b>exampleSimple2:</b><br/>" << endl;

    XsltExecutable * executable = proc->compileFromFile("../php/xsl/foo.xsl");
    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("exampleSimple2");
        cerr << "exampleSimple2 NULL found" << endl;
        if (proc->exceptionOccurred()) {
            cerr <<"exampleSimple2 error: "<< proc->getErrorMessage() << endl;
        }
        return;
    }

    executable->setInitialMatchSelectionAsFile("../php/xml/foo.xml");
    const char *filename = "output1.xml";
    executable->setOutputFile(filename);
    executable->applyTemplatesReturningFile("output1.xml");

    if (CppTestUtils::exists("output1.xml")) {
        cout << "The file $filename exists" << endl;
        remove("output1.xml");
        sresult->success++;
    } else {
        cout << "The file " << filename << " does not exist" << endl;
        if (executable->exceptionOccurred()) {
            cout << proc->getErrorMessage() << endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("exampleSimple2");
    }

    delete executable;

}

void exampleTransformToFile(SaxonProcessor * sproc, Xslt30Processor *proc, sResultCount *sresult) {
    cout << "<b>exampleTransformToFile:</b><br/>" << endl;
    sproc->exceptionClear();
    XdmNode* xmlfile = sproc->parseXmlFromFile("../php/xml/foo.xml");
    if(xmlfile == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("exampleTransformToFile");
        cerr << "exampleTransformToFile NULL found" << endl;
        if (sproc->exceptionOccurred()) {
            const char * errorMessage = sproc->getErrorMessage();
            if(errorMessage!= nullptr) {
                cerr << "exampleTransformToFile error: " << errorMessage << endl;
            }
        }

        return;
    }
    XsltExecutable * executable = proc->compileFromFile("../php/xsl/foo.xsl");
    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("exampleTransformToFile");
        cerr << "exampleTransformToFile NULL found" << endl;
        if (proc->exceptionOccurred()) {
            cerr <<"exampleTransformToFile error: "<< proc->getErrorMessage() << endl;
        }
        return;
    }

    const char *filename = "output2.xml";
    executable->setInitialMatchSelectionAsFile("../php/xml/foo.xml");
    executable->setOutputFile(filename);
    executable->transformToFile(xmlfile);

    if (CppTestUtils::exists("output2.xml")) {
        cout << "The file output2.xml exists" << endl;
        remove("output2.xml");
        sresult->success++;
    } else {
        cout << "The file " << filename << " does not exist" << endl;
        if (executable->exceptionOccurred()) {
            cout << proc->getErrorMessage() << endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("exampleTransformToFile");
    }

    delete executable;

}

void exampleTransformToString(SaxonProcessor * sproc, Xslt30Processor *proc, sResultCount *sresult) {
    cout << "<b>exampleTransformToString:</b><br/>" << endl;
    sproc->exceptionClear();
    proc->exceptionClear();
    XdmNode* xmlfile = sproc->parseXmlFromString("<xml>\n"
                                                 " <value>5</value>\n"
                                                 "</xml>");
    if(xmlfile == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("exampleTransformToString");
        cerr << "exampleTransformToFile NULL found" << endl;
        if (sproc->exceptionOccurred() && sproc->getErrorMessage() != nullptr) {
            cerr <<"exampleTransformToFile error: "<< sproc->getErrorMessage() << endl;
        }

        return;
    }
    XsltExecutable * executable = proc->compileFromString("<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"2.0\">\n"
                                                          "  <xsl:output method=\"xml\" encoding=\"utf-8\" standalone=\"yes\" indent=\"yes\"/>\n"
                                                          "\n"
                                                          "  <xsl:template match=\"/\">\n"
                                                          "    <xsl:message>producing sheet</xsl:message>\n"
                                                          "    <xsl:result-document href=\"output.xml\">\n"
                                                          "      <result>\n"
                                                          "        <value>You put a <xsl:value-of select=\"/xml/value\"/></value>\n"
                                                          "      </result>\n"
                                                          "    </xsl:result-document>\n"
                                                          "  </xsl:template>\n"
                                                          "</xsl:stylesheet>");

    string baseURI = string("file://")+sproc->getcwd();
    executable->setBaseOutputURI(baseURI.c_str());
    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("exampleTransformToString");
        cerr << "exampleTransformToString NULL found" << endl;
        if (proc->exceptionOccurred()) {
            cerr <<"exampleTransformToString error: "<< proc->getErrorMessage() << endl;
        }
        return;
    }

    executable->setInitialMatchSelection(xmlfile);
    executable->setGlobalContextItem(xmlfile);
    const char * result = executable->transformToString();

    if (result == nullptr) {

        cerr << "exampleTransformToString: result is NULL" << endl;
        if (executable->exceptionOccurred()) {
            cout << executable->getErrorMessage() << endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("exampleTransformToString");
    } else {
        cout<< "exampleTransformToString = "<<result<<endl;
        sresult->success++;
    }
    delete result;
    delete executable;

}



void exampleSimple3(SaxonProcessor *saxonProc, Xslt30Processor *proc, sResultCount *sresult) {
    cout << "<b>exampleSimple3:</b><br/>" << endl;
    proc->clearParameters();

    XdmNode *xdmNode = saxonProc->parseXmlFromString("<doc><b>text value of out</b></doc>");
    if (xdmNode== nullptr) {
        cout << "Error: xdmNode is null'" << endl;
        if(saxonProc->exceptionOccurred()) {
            cout<<"Error message="<<saxonProc->getErrorMessage()<<endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("exampleSimple3");
        return;
    }

    XsltExecutable * executable = proc->compileFromFile("../php/xsl/foo.xsl");

   if(executable == nullptr) {
	cout << "executable is NULL" <<endl;
	if(proc->exceptionOccurred()) {
		cout << proc->getErrorMessage() <<endl;

	}
	sresult->failure++;
	sresult->failureList.push_back("exampleSimple3");
	return;
   }

    executable->setInitialMatchSelection((XdmNode *) xdmNode);
    sresult->success++;
    delete xdmNode;
    delete executable;
}

void exampleSimple3aError(SaxonProcessor *saxonProc, Xslt30Processor *proc, sResultCount *sresult) {
    cout << "<b>exampleSimple3aError:</b><br/>" << endl;

    XsltExecutable * executable = proc->compileFromFile(nullptr);


    if (executable == nullptr) {


        cout << "Expected failure of test exampleSimple3aError:" << endl;
        if(proc->exceptionOccurred()) {
            const char * message = proc->getErrorMessage();
            if(message!= nullptr) {
                cout << proc->getErrorMessage() << endl;

            }
        }

        sresult->success++;
        proc->exceptionClear();
        return;
    }

    sresult->failure++;
    sresult->failureList.push_back("exampleSimple3aError");
    delete executable;
    return;


}

void exampleParam(SaxonProcessor *saxonProc, Xslt30Processor *proc, sResultCount *sresult) {
    cout << "Test: ExampleParam" << endl;

    XsltExecutable * executable = proc->compileFromFile("../php/xsl/foo.xsl");


    if(executable == nullptr) {
        cout << "executable is NULL" <<endl;
        if(proc->exceptionOccurred()) {
            cout << "Error : " << proc->getErrorMessage() <<endl;

        }
        sresult->failure++;
        sresult->failureList.push_back("exampleParam");
	proc->exceptionClear();
        return;

    }

    executable->setInitialMatchSelectionAsFile("../php/xml/foo.xml");

    XdmAtomicValue *xdmvalue = saxonProc->makeStringValue("Hello to you");
    if (xdmvalue != nullptr) {

        executable->setParameter("a-param", (XdmValue *) xdmvalue);

    } else {
        cout << "Xdmvalue is NULL - ====== FAIL =====" << endl;
        sresult->failure++;
        sresult->failureList.push_back("exampleParam");
        if(executable->exceptionOccurred()) {
            SaxonApiException * exception = executable->getException();
            cerr<<"Error: " << exception->getMessage() <<endl;
            delete exception;
        }
        delete executable;
        return;
    }
    const char *result = executable->applyTemplatesReturningString();
    if (result != nullptr) {
        cout << "Output:" << result << endl;
        sresult->success++;
        delete result;
    } else {
        cout << "Result is NULL<br/>  ======= fail =====" << endl;
        sresult->failure++;
        sresult->failureList.push_back("exampleParam");
        if(executable->exceptionOccurred()) {
            SaxonApiException * exception = executable->getException();
            cerr<<"Error: " << exception->getMessage() <<endl;
            delete exception;
        }
        delete executable;
        return;
    }

    //proc->clearParameters();
    //unset($result);
    //echo 'again with a no parameter value<br/>';

    executable->setProperty("!indent", "yes");
    const char *result2 = executable->applyTemplatesReturningString();


    if (result2 != nullptr) {
        cout << "Result2 output= " << result2 << endl;
        sresult->success++;
        delete result2;
    } else {
        cout << "Result2 is NULL<br/>  ======= fail =====" << endl;
        sresult->failure++;
        sresult->failureList.push_back("exampleParam");
        if(executable->exceptionOccurred()) {
            SaxonApiException * exception = executable->getException();
            cerr<<"Error: " << exception->getMessage() <<endl;
            delete exception;
        }
        delete xdmvalue;
        delete executable;
        return;
    }

    //  unset($result);
    // echo 'again with no parameter and no properties value set. This should fail as no contextItem set<br/>';
    //delete xdmvalue;
    //executable->clearParameters();

    XdmAtomicValue *xdmValue2 = saxonProc->makeStringValue("goodbye to you");

    if (xdmValue2 == nullptr) {
        cout << "Xdmvalue is NULL - ====== FAIL =====" << endl;
        sresult->failure++;
        sresult->failureList.push_back("exampleParam");
        if(executable->exceptionOccurred()) {
            SaxonApiException * exception = executable->getException();
            cerr<<"Error: " << exception->getMessage() <<endl;
            delete exception;
        }
        delete executable;
        delete xdmvalue;
        return;
    }

    executable->setParameter("a-param", (XdmValue *) xdmValue2);
    delete xdmvalue;


    const char *result3 = executable->applyTemplatesReturningString();
    if (result3 != nullptr) {
        cout << "Output =" << result3 << endl;

        sresult->success++;
        delete result3;

    } else {
        cout << "Error in result ===== FAIL =======" << endl;
        sresult->failure++;
        sresult->failureList.push_back("exampleParam");
        if(executable->exceptionOccurred()) {
            SaxonApiException * exception = executable->getException();
            cerr<<"Error: " << exception->getMessage() <<endl;
            delete exception;
        }

    }
    delete xdmValue2;
    delete executable;



}

// test parameter and properties maps where we update key, value pair.
void exampleParam2(SaxonProcessor *saxonProc, Xslt30Processor *proc, sResultCount *sresult) {
    cout << "\nExampleParam:</b><br/>" << endl;
    XsltExecutable * executable = proc->compileFromFile("../php/xsl/foo.xsl");
    executable->setInitialMatchSelectionAsFile("../php/xml/foo.xml");


    XdmAtomicValue *xdmvalue = saxonProc->makeStringValue("Hello to you");
    XdmAtomicValue *xdmvalue2i = saxonProc->makeStringValue("Hello from me");
    if (xdmvalue != NULL) {

        executable->setParameter("a-param", (XdmValue *) xdmvalue);
        executable->setParameter("a-param", (XdmValue *) xdmvalue2i);

    } else {
        cout << "Xdmvalue is null - ====== FAIL =====" << endl;
        sresult->failure++;
        sresult->failureList.push_back("exampleParam-1");
    }
    const char *result = executable->applyTemplatesReturningString();
    if (result != NULL) {
        string sresulti = string(result);
        if (sresulti.compare("Hello from me") == 0) {
            cout << "Output:" << result << endl;
            sresult->success++;
        } else {
            cout << "Result is " << result << " <br/> ======= fail ===== " << endl;
            sresult->failure++;
            sresult->failureList.push_back("exampleParam-2");
        }
    } else {
        cout<<"Result is NULL<br/>  ======= fail ====="<<endl;
        sresult->failure++;
        sresult->failureList.push_back("exampleParam-2");
    }

//proc->clearParameters();
//unset($result);
//echo 'again with a no parameter value<br/>';

    executable->setProperty("!indent", "no");
    executable->setProperty("!indent", "yes");
    const char *result2 = executable->applyTemplatesReturningString();

    executable->clearProperties();

    if(result2 != nullptr) {
        cout<<result2<<endl;
        sresult->success++;

    }

    //  unset($result);
    // echo 'again with no parameter and no properties value set. This should fail as no contextItem set<br/>';
    XdmAtomicValue *xdmValue2 = saxonProc->makeStringValue("goodbye to you");
    executable->setParameter("a-param", (XdmValue*)xdmValue2);

    const char *result3 = executable->applyTemplatesReturningString();
    if(result3 != nullptr) {
        cout<<"Output ="<<result3<<endl;

        sresult->success++;
    } else {
        cout<<"Error in result ===== FAIL ======="<<endl;
        sresult->failure++;
        sresult->failureList.push_back("exampleParam");
    }

}

/* XMarkbench mark test q12.xsl with just-in-time=true*/
void xmarkTest1(Xslt30Processor *proc, sResultCount *sresult) {
    cout << "Test: xmarkTest1 - XMarkbench mark test q12.xsl (JIT=true):" << endl;

    proc->setJustInTimeCompilation(true);

    XdmValue *result = proc->transformFileToValue("../data/xmark100k.xml", "../data/q12.xsl");
    if (result != nullptr && !proc->exceptionOccurred()) {
        cout << "XdmNode returned" << endl;
        sresult->success++;
        delete result;
    } else {
        printf("result is null \nCheck For errors:");
        sresult->failure++;
        sresult->failureList.push_back("xmarkTest1");
        if (proc->exceptionOccurred()) {
            const char * message = proc->getErrorMessage();
            if(message != nullptr) {
                cerr << proc->getErrorMessage() << endl;
		proc->exceptionClear();
            } else {
                cerr << "Message is nullptr" << endl;
               // SaxonProcessor::sxn_environ->env->ExceptionDescribe();
		//SaxonProcessor::sxn_environ->env->ExceptionClear();
            }
        }
    }
    proc->setJustInTimeCompilation(false);

}


/* XMarkbench mark test q12.xsl with just-in-time=true*/
void xmarkTest2(Xslt30Processor *proc, sResultCount *sresult) {
    cout << "Test: xmarkTest2 - XMarkbench mark test q12.xsl (JIT=true):" << endl;

    proc->setJustInTimeCompilation(true);

    XdmValue *result = proc->transformFileToValue("../data/xmark100k.xml", "../data/q12.xsl");
    if (result != nullptr && !proc->exceptionOccurred()) {
        cout << "XdmNode returned" << endl;
        sresult->success++;
        delete result;

    } else {
        printf("result is null \nCheck For errors:");
        sresult->failure++;
        sresult->failureList.push_back("xmarkTest2");
        if (proc->exceptionOccurred()) {
            SaxonApiException * exception = proc->getException();
            if(exception != nullptr) {
                const char *message = exception->getMessage();
                if (message != nullptr) {
                    cerr << message << endl;

                }


            }
            proc->exceptionClear();
        }
    }

    proc->setJustInTimeCompilation(false);

}

/* XMarkbench mark test q12.xsl with just-in-time=true*/
void exampleSimple_xmark(Xslt30Processor *proc, sResultCount *sresult) {
    cout << "exampleSimple_xmark test - test q12.xsl:" << endl;

    proc->setJustInTimeCompilation(true);

    XdmValue *result = proc->transformFileToValue("../data/xmark100k.xml", "../data/q12.xsl");

    if (result != nullptr && !proc->exceptionOccurred()) {
        cout << "XdmNode returned" << endl;
        sresult->success++;
        delete result;
    } else {
        printf("Result is null \nCheck For errors:");
        if (proc->exceptionOccurred()) {
            const char * message = proc->getErrorMessage();
            if(message != nullptr) {
                cout << proc->getErrorMessage() << endl;
            }
        }
        sresult->failure++;
        sresult->failureList.push_back("exampleSimple_xmark");
        proc->exceptionClear();
    }
    proc->clearParameters();
    proc->setJustInTimeCompilation(false);

}

/*
* Test saving nd loading a Xslt package
*/
void testPackage1(Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: testPackage1 - Saving and loading Packages:" << endl;
    trans->clearParameters();


    trans->compileFromFileAndSave("../data/test.xsl", "test1.sef");
    const char *output = trans->transformFileToString("../data/cat.xml", "test1.sef");

    if (output== nullptr) {
        printf("result is null \n");
        const char *message = trans->getErrorMessage();
        if (message != NULL) {
            cout << "Error message =" << message << endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("testPackage1");

    } else {
        printf("%s", output);
        printf("result is OK \n");
        sresult->success++;
    }
    fflush(stdout);
    delete output;
}


/*
* Test saving and loading a Xslt package
*/
void testPackage1a(Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: testPackage1a" << endl;
    trans->clearParameters();

    trans->compileFromFileAndSave("../data/test.xsl", "test1a.sef");

    if(trans->exceptionOccurred()) {
        const char *message = trans->getErrorMessage();
        if (message != nullptr) {
            cout << "Error message =" << message << endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("testPackage1a");
        trans->exceptionClear();
        return;
    }



    XsltExecutable * executable = trans->compileFromFile("test1.sef");

    if(executable == nullptr) {
	if(trans->exceptionOccurred()) {
          const char *message = trans->getErrorMessage();
          if (message != nullptr) {
            cout << "Error message =" << message << endl;
          }
          sresult->failure++;
          sresult->failureList.push_back("testPackage1a");
          trans->exceptionClear();
    	}
	return;


    }

    executable->setInitialMatchSelectionAsFile("../data/cat.xml");
    const char * output = executable->applyTemplatesReturningString();
    if (output== nullptr) {
        printf("result is null \n");
        const char *message = trans->getErrorMessage();
        if (message != NULL) {
            cout << "Error message =" << message << endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("testPackage1a");
        trans->exceptionClear();

    } else {
        printf("%s", output);
        printf("result is OK \n");
        sresult->success++;
        delete output;
    }
    fflush(stdout);
    delete executable;
}


/*
* Test saving and loading a Xslt package
*/
void testPackage2_Error(Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: testPackage2_Error:" << endl;

    const char *stylesheet = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='2.0'>       <xsl:param name='values' select='(2,3,4)' /><xsl:output method='xml' indent='yes' /><xsl:template match='*'><output><xsl:for-each select='$values' ><out><xsl:value-of select='. * 3'/></out><xsl:for-each></output></xsl:template><xsl:stylesheet>";

    trans->compileFromStringAndSave(stylesheet, "test2.sef");

    if(trans->exceptionOccurred() || trans->getException() != nullptr) {

        const char *message = trans->getErrorMessage();
        if (message != nullptr) {
            cout << "Error message =" << message << endl;
        }
        sresult->success++;
        trans->exceptionClear();
        return;

    } else {
        cout << "Test failed - no error message produced"  << endl;
    }

    trans->exceptionClear();
    sresult->failure++;
    sresult->failureList.push_back("testPackage2_Error");

}

void testCallFunction(SaxonProcessor *proc, Xslt30Processor *trans, sResultCount *sresult) {

    const char *source = "<?xml version='1.0'?> <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:f='http://localhost/' version='3.0'> <xsl:function name='f:add' visibility='public'>    <xsl:param name='a'/><xsl:param name='b'/> <xsl:sequence select='$a + $b'/></xsl:function></xsl:stylesheet>";
    cout << endl << "Test: testCallFunction:" << endl;
    XsltExecutable * executable = trans->compileFromString(source);

    if(executable == nullptr) {
        if(trans->exceptionOccurred()) {
            SaxonApiException * exception = trans->getException();
            if(exception != nullptr) {
                cerr << "Error: " << exception->getMessage() << endl;
                delete exception;
            }

        }
        sresult->failure++;
        sresult->failureList.push_back("testCallFunction");
        trans->exceptionClear();
        return;


    }

    XdmValue **valueArray = new XdmValue *[2];

    valueArray[0] = (XdmValue *) (proc->makeIntegerValue(2));
    valueArray[1] = (XdmValue *) (proc->makeIntegerValue(3));
    XdmValue *v = executable->callFunctionReturningValue("{http://localhost/}add", valueArray, 2);

    if (v != NULL && (v->getHead())->isAtomic() && ((XdmAtomicValue *) (v->getHead()))->getLongValue() == 5) {
        sresult->success++;
        delete v;
    } else {
        if (v != NULL && !(v->getHead())->isAtomic()) {
            cout << "Value in callFunction is not atomic - but expected as atomic value" << endl;
        }
        cout << "testCallFunction ======= FAIL ======" << endl;
        if(executable->exceptionOccurred()) {
            SaxonApiException * exception = executable->getException();
            if(exception != nullptr) {
                cerr << "Error: " << exception->getMessage() << endl;
                delete exception;
            }
        }
        sresult->failure++;
        sresult->failureList.push_back("testCallFunction");
        executable->exceptionClear();
        delete v;
    }
    delete valueArray[0];
    delete valueArray[1];
    delete [] valueArray;
    delete executable;
}

void testInitialTemplate(SaxonProcessor *proc, Xslt30Processor *trans, sResultCount *sresult) {


    const char *source = "<?xml version='1.0'?>  <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'  xmlns:xs='http://www.w3.org/2001/XMLSchema'  version='3.0'>  <xsl:template match='*'>     <xsl:param name='a' as='xs:double'/>     <xsl:param name='b' as='xs:float'/>     <xsl:sequence select='., $a + $b'/>  </xsl:template>  </xsl:stylesheet>";
    cout << endl << "Test:testInitialTemplate" << endl;
    XsltExecutable * executable = trans->compileFromString(source);
    if(executable == nullptr) {
        if(trans->exceptionOccurred()) {
            cout << "Error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }
    XdmNode * node = proc->parseXmlFromString("<e/>");
    executable->setResultAsRawValue(false);
    std::map<std::string, XdmValue *> parameterValues;

    XdmAtomicValue * a1 = proc->makeIntegerValue(12);
    XdmAtomicValue * a2 = proc->makeIntegerValue(5);;
    parameterValues["a"] = a1;
    parameterValues["b"] = a2;
    executable->setInitialTemplateParameters(parameterValues, false);
    executable->setInitialMatchSelection(node);
    XdmValue *result = executable->applyTemplatesReturningValue();
    if (result != nullptr) {
        sresult->success++;
        cout << "Result=" << result->getHead()->getStringValue() << endl;
        delete result;
    } else {
        sresult->failure++;
        sresult->failureList.push_back("testInitialTemplate");
    }
    delete executable;
    delete a1;
    delete a2;
    delete node;
    parameterValues.clear();

}


void  testResultDocumentAsMap(SaxonProcessor *proc, Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: testResultDocumentAsMap:" << endl;

    XdmNode * inputDoc = proc->parseXmlFromString("<a>b</a>");

    XsltExecutable * executable = trans->compileFromString("<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='a'><xsl:result-document href='out.xml'><e>f</e> </xsl:result-document> <xsl:result-document href='out2.xml'><e>hello</e> </xsl:result-document></xsl:template></xsl:stylesheet>");

    if(executable == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testResultDocumentAsMap");
        if(trans->exceptionOccurred()) {
            const char * message = trans->getErrorMessage();
            if(message != nullptr) {
                cout << "Error: " << trans->getErrorMessage() << endl;
            }
        }
        trans->exceptionClear();
        return;
    }
    executable->setCaptureResultDocuments(true);

        if(executable->exceptionOccurred()) {
            sresult->failureList.push_back("testResultDocumentAsMap");
            const char * message = executable->getErrorMessage();
            if(message != nullptr) {
                cout << "Error: " << executable->getErrorMessage() << endl;
            }
            executable->exceptionClear();
            delete executable;
            return;
        }

    sresult->success++;
    delete executable;
    return;




}

void testResolveUri(SaxonProcessor *proc, Xslt30Processor *trans, sResultCount *sresult) {
    cout << endl << "Test: testResolveUri:" << endl;

    XsltExecutable * executable = trans->compileFromString(
            "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:err='http://www.w3.org/2005/xqt-errors'><xsl:template name='go'><xsl:try><xsl:variable name='uri' as='xs:anyURI' select=\"resolve-uri('notice trailing space /out.xml')\"/> <xsl:message select='$uri'/><xsl:result-document href='{$uri}'><out/></xsl:result-document><xsl:catch><xsl:sequence select=\"'$err:code: ' || $err:code  || ', $err:description: ' || $err:description\"/></xsl:catch></xsl:try></xsl:template></xsl:stylesheet>");

    if(executable == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testResolveUri");
        if(trans->exceptionOccurred()) {
            const char * message = trans->getErrorMessage();
            if(message != nullptr) {
                cout << "Error: " << trans->getErrorMessage() << endl;
            }
        }
        trans->exceptionClear();
        return;
    }
    XdmValue *value = executable->callTemplateReturningValue("go");


    if (value== nullptr || executable->exceptionOccurred()) {
        if (executable->exceptionOccurred()) {
            SaxonApiException *exception = executable->getException();
            cerr << "Error: " << exception->getMessage() << endl;
            delete exception;
        }
        sresult->failure++;
        sresult->failureList.push_back("testResolveUri");
    } else {
        XdmItem * item1 = value->itemAt(0);
        if(item1 != nullptr) {
        const char *svalue = item1->getStringValue();
        if(svalue != nullptr) {
            cout << "testResolveUri = " << svalue << endl;
            delete svalue;
            svalue = nullptr;
            sresult->success++;

        }
        } else {
            cout << "Error: testResolveUri item is nullptr " << endl;
            sresult->failure++;
            sresult->failureList.push_back("testResolveUri");
        }


        delete value;
    }

    delete executable;
}

void testContextNotRoot(SaxonProcessor *proc, Xslt30Processor *trans, sResultCount *sresult) {
    cout << endl << "Test: testContextNotRoot" << endl;

    XdmNode *node = proc->parseXmlFromString("<doc><e>text</e></doc>");

    XsltExecutable *executable = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:variable name='x' select='.'/><xsl:template match='/'>errorA</xsl:template><xsl:template match='e'>[<xsl:value-of select='name($x)'/>]</xsl:template></xsl:stylesheet>");

    if (executable == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testContextNotRoot");
        if (trans->exceptionOccurred()) {
            cout << "Error: " << trans->getErrorMessage() << endl;
        }
        return;
    }

    executable->setGlobalContextItem(node);
    if (node != nullptr && node->getChildCount() > 0) {
        XdmNode **eNodeL1 = node->getChildren();
        std::cerr<<"testContextNotRoot cp 0"<<std::endl;
        if (eNodeL1 != nullptr) {
            XdmNode ** eNodeChildren = eNodeL1[0]->getChildren();
            if (eNodeChildren != nullptr) {
                XdmNode *eNode = eNodeChildren[0];
                cout << "Node content = " << eNode->toString() << endl;
                executable->setInitialMatchSelection(eNode);
                const char *result = executable->applyTemplatesReturningString();

                if (result == nullptr) {

                    cout << "testContextNotRoot ======= FAIL ======" << endl;
                    if (executable->exceptionOccurred()) {
                        SaxonApiException *exception = executable->getException();
                        cerr << "Error: " << exception->getMessage() << endl;
                        delete exception;
                    }
                    sresult->failure++;
                    sresult->failureList.push_back("testContextNotRoot");


                } else {

                    cout << "testContextNotRoot = " << result << endl;
                    sresult->success++;
                    delete result;

                }

            }
        }
    } else {
        sresult->failure++;
        sresult->failureList.push_back("testContextNotRoot");
    }

    delete executable;
    delete node;
}


void testContextNotRootNamedTemplate(SaxonProcessor *proc, Xslt30Processor *trans, sResultCount *sresult) {
    cout << endl << "Test: testContextNotRootNamedTemplate" << endl;

    XdmNode *node = proc->parseXmlFromString("<doc><e>text</e></doc>");

    if(node == nullptr) {
        if(proc->exceptionOccurred()) {
            const char * message = proc->getErrorMessage();
            if(message != nullptr) {
                cerr << "Error: " << message << endl;
            }
            proc->exceptionClear();
        }
        sresult->failure++;
        sresult->failureList.push_back("testContextNotRootNamedTemplate");
        return;
    }

    XsltExecutable * executable = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:variable name='x' select='.'/><xsl:template match='/'>errorA</xsl:template><xsl:template name='main'>[<xsl:value-of select='name($x)'/>]</xsl:template></xsl:stylesheet>");

    if(executable == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testContextNotRootNamedTemplate");
        if(trans->exceptionOccurred()) {
            cout << "Error: "<< trans->getErrorMessage() << endl;
        }
        delete node;
        return;
    }

    executable->setGlobalContextItem(node);
    const char *result = executable->callTemplateReturningString("main");

    if (result== nullptr) {

        cout << "testContextNotRootNameTemplate ======= FAIL ======" << endl;
        if(executable->exceptionOccurred()) {
            SaxonApiException * exception = executable->getException();
            cerr<<"Error: " << exception->getMessage() <<endl;
            delete exception;
        }
        sresult->failure++;
        sresult->failureList.push_back("testContextNotRootNamedTemplate");
    } else {

        cout << "testContextNotRoot = " << result << endl;
        sresult->success++;
        delete result;
    }
    delete node;
    delete executable;

}


void testContextNotRootNamedTemplateValue(SaxonProcessor *proc, Xslt30Processor *trans, sResultCount *sresult) {
    cout << endl << "Test: testContextNotRootNamedTemplateValue" << endl;

    XdmNode *node = proc->parseXmlFromString("<doc><e>text</e></doc>");

    if(node == nullptr) {
        if(proc->exceptionOccurred()) {
            const char * message = proc->getErrorMessage();
            if(message != nullptr) {
                cerr << "Error: " << message << endl;
            }
            proc->exceptionClear();
        }
        sresult->failure++;
        sresult->failureList.push_back("testContextNotRootNamedTemplateValue");
        return;
    }

    XsltExecutable * executable = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:variable name='x' select='.'/><xsl:template match='/'>errorA</xsl:template><xsl:template name='main'>[<xsl:value-of select='name($x)'/>]</xsl:template></xsl:stylesheet>");

    if(executable == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testContentNotRootNamedTemplateValue");
        if(trans->exceptionOccurred()) {
            cout << "Error: "<< trans->getErrorMessage() << endl;
        }
        delete node;
        return;
    }

    executable->setGlobalContextItem(node);
    XdmValue *result = executable->callTemplateReturningValue("main");

    if (result == nullptr) {

        cout << "testCallFunction ======= FAIL ======" << endl;
        if(executable->exceptionOccurred()) {
            SaxonApiException * exception = executable->getException();
            cerr<<"Error: " << exception->getMessage() <<endl;
            delete exception;
        }
        sresult->failure++;
        sresult->failureList.push_back("testContextNotRootNamedTemplateValue");
    } else {

        cout << "testContextNotRoot = " << result->getHead()->getStringValue() << endl;
        sresult->success++;
        delete result;
        result = nullptr;
    }

    delete node;
    delete executable;


}

void testCallSystemFunction(SaxonProcessor *proc, sResultCount *sresult) {

    XdmFunctionItem *fi = XdmFunctionItem::getSystemFunction(proc, "{http://www.w3.org/2005/xpath-functions}parse-json",
                                                            1);
    if (fi== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testCallSystemFunction");
        return;
    }

    XdmValue ** xdmValue = new XdmValue*[1];
    xdmValue[0] = new XdmValue();
    xdmValue[0]->addXdmItem(proc->makeStringValue("[1,2,3]"));
    XdmValue *result = fi->call(proc, xdmValue, 1);

    std::cerr << "Result = " << result->toString() << endl;
    if(result->size() == 3) {

        cout << "testCallSystemFunction = " << result->getHead()->getStringValue() << endl;
        sresult->success++;

    } else {
        sresult->failure++;
        sresult->failureList.push_back("testCallSystemFunction");

    }

}


void testPipeline(SaxonProcessor *proc, sResultCount *sresult) {
    cout << endl << "Test: testPipeline" << endl;

    Xslt30Processor * trans = proc->newXslt30Processor();
    if(trans == nullptr) {
        cout << "Error: Xslt30Processor is null - maybe unclean state of JNI" << endl;
        sresult->failure++;
        sresult->failureList.push_back("testPipeline");
        return;
    }
    XsltExecutable * stage1 = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template></xsl:stylesheet>");

    if(stage1 == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testPipeline");
        if(trans->exceptionOccurred()) {
            const char * message = trans->getErrorMessage();
            cout << "stage 1 Error: " << message << endl;

        }
        cout << "Stage 1 Error - exit method " << endl;
        delete trans;
        return;
    }

    XdmNode *inn = proc->parseXmlFromString("<z/>");

    XsltExecutable *stage2 = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template></xsl:stylesheet>");

    if(stage2 == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testPipeline");
        if(trans->exceptionOccurred()) {
            const char * message = trans->getErrorMessage();
            cout << "stage 2 Error: " << message << endl;

        }
        cout << "Stage 2 Error - exit method " << endl;
        delete stage1;
        delete trans;
        delete inn;
        return;
    }


    XsltExecutable *stage3 = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template></xsl:stylesheet>");

    if(stage3 == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testPipeline");
        if(trans->exceptionOccurred()) {
            const char * message = trans->getErrorMessage();
            cout << "stage 3 Error: " << message << endl;

        } else {
            cout << "Stage 3 Error: testPipeline failed with exception" << endl;
        }
        cout << "Stage 3 Error - exit method " << endl;
        delete stage1;
        delete stage2;
        delete trans;
        delete inn;
        return;
    }

    XsltExecutable *stage4 = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template></xsl:stylesheet>");

    if(stage4 == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testPipeline");
        if(trans->exceptionOccurred()) {
            const char * message = trans->getErrorMessage();
            cout << "stage 4 Error: " << message << endl;

        } else {
            cout << "Stage 4 Error: testPipeline failed with exception" << endl;
        }
        cout << "Stage 4 Error - exit method " << endl;
        delete stage1;
        delete stage2;
        delete stage3;
        delete trans;
        delete inn;
        return;
    }

    XsltExecutable *stage5 = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template></xsl:stylesheet>");

    if(stage5 == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testPipeline");
        if(trans->exceptionOccurred()) {
            const char * message = trans->getErrorMessage();
            cout << "stage 5 Error: " << message << endl;

        } else {
            cout << "Stage 5 Error: testPipeline failed with exception" << endl;
        }
        cout << "Stage 5     Error - exit method " << endl;
        delete stage1;
        delete stage2;
        delete stage3;
        delete stage4;
        delete trans;
        delete inn;
        return;
    }

    stage1->setProperty("!indent", "no");
    stage1->setInitialMatchSelection(inn);

    XdmValue *d1 = stage1->applyTemplatesReturningValue();
    if(d1 == nullptr) {
        if (stage1->exceptionOccurred()) {
            sresult->failure++;
            sresult->failureList.push_back("testPipeline");
            if (stage1->exceptionOccurred()) {
                SaxonApiException *exception = stage1->getException();
                if (exception != nullptr) {
                    cerr << "Error: " << exception->getMessage() << endl;
                    delete exception;
                    exception = nullptr;
                }
            }
            cout << "Stage d1 Error - exit method " << endl;
            delete stage1;
            delete stage2;
            delete stage3;
            delete stage4;
            delete stage5;
            delete trans;
            delete inn;
            return;
        }
    }

    XdmItem *d11 = d1->getHead();
    if (d11== nullptr) {

        cout << "d11 is NULL\n" << endl;
        sresult->failure++;
        sresult->failureList.push_back("testPipeline-1");
        delete stage1;
        delete stage2;
        delete stage3;
        delete stage4;
        delete stage5;
        delete d1;
        delete inn;
        delete trans;
        return;
    }
    const char *data = d11->toString();

    if (data != NULL) {
        cout << "d1 result=" << data << endl;
        delete data;
        data = nullptr;
    } else {

        cout << "d1 result Error - toString is NULL" << endl;
        delete stage1;
        delete stage2;
        delete stage3;
        delete stage4;
        delete stage5;
        delete d1;
        delete inn;
        delete trans;
        return;
    }
    stage2->setProperty("!indent", "no");
    stage2->setInitialMatchSelection(d11);
    XdmValue *d2 = stage2->applyTemplatesReturningValue();
    if (d2== nullptr) {
        cout << "ERROR-11\n" << endl;
        sresult->failure++;
        sresult->failureList.push_back("testPipeline-2");
        if (stage2->exceptionOccurred()) {
            SaxonApiException *exception = stage2->getException();
            if (exception != nullptr) {
                cerr << "Error: " << exception->getMessage() << endl;
                delete exception;
                exception = nullptr;
            }
        }
        cout << "Stage d2 Error - exit method " << endl;
        delete d1;
        delete inn;
        delete stage1;
        delete stage2;
        delete stage3;
        delete stage4;
        delete stage5;
        delete trans;
        return;
    } else {
        const char *data2 = d2->toString();
        cout << "d2 result=" << data2 << endl;
        delete data2;
        data2 = nullptr;
    }
    stage3->setProperty("!indent", "no");
    stage3->setInitialMatchSelection(d2);
    XdmValue * d3 = stage3->applyTemplatesReturningValue();
    if(d3 == nullptr){
        sresult->failure++;
        sresult->failureList.push_back("testPipeline-3");
        if (stage3->exceptionOccurred()) {
            SaxonApiException *exception = stage3->getException();
            if (exception != nullptr) {
                cerr << "Error: " << exception->getMessage() << endl;
                delete exception;
                exception = nullptr;
            }
        }
        cout << "Stage d3 Error - exit method " << endl;
        delete d1;
        delete d2;
        delete inn;

        delete stage1;
        delete stage2;
        delete stage3;
        delete stage4;
        delete stage5;
        delete trans;
        return;
      }
    cout << "after d2 result cp2"  << endl;
    stage4->setProperty("!indent", "no");
    stage4->setInitialMatchSelection(d3);
    XdmValue * d4 = stage4->applyTemplatesReturningValue();
    if(d4== nullptr){
        sresult->failure++;
        sresult->failureList.push_back("testPipeline-4");
        if (stage4->exceptionOccurred()) {
            SaxonApiException *exception = stage4->getException();
            if (exception != nullptr) {
                cerr << "Error: " << exception->getMessage() << endl;
                delete exception;
                exception = nullptr;
            }
        }
        cout << "Stage d4 Error - exit method " << endl;
        delete d3;
        delete d2;
        delete d1;
        delete inn;
        delete stage1;
        delete stage2;
        delete stage3;
        delete stage4;
        delete stage5;
        delete trans;
        return;
      }
    stage5->setProperty("!indent", "no");
    stage5->setInitialMatchSelection(d4);
    const char * sw = stage5->applyTemplatesReturningString();
    if(sw == nullptr){
        sresult->failure++;
        sresult->failureList.push_back("testPipeline-5");
        if (stage5->exceptionOccurred()) {
            SaxonApiException *exception = stage5->getException();
            if (exception != nullptr) {
                cerr << "Error: " << exception->getMessage() << endl;
                delete exception;
            }
        }
        cout << "Stage sw Error - exit method " << endl;
        delete stage1;
        delete stage2;
        delete stage3;
        delete stage4;
        delete stage5;
        delete trans;
        delete d4;
        delete d3;
        delete d2;
        delete d1;
        delete inn;
        return;
      }
    cout<<sw<<endl;
    cout << "testPipeline = " << sw << endl;
    sresult->success++;
    delete stage1;
    stage1 = nullptr;

    delete stage2;
    stage2 = nullptr;


    delete stage3;
    stage3 = nullptr;

    delete stage4;
    stage4 = nullptr;

    delete stage5;
    stage5 = nullptr;

    delete trans;
    trans = nullptr;

    delete sw;
    sw = nullptr;

    delete d4;
    d4 = nullptr;

    delete d3;
    d3 = nullptr;

    delete d2;
    d2 = nullptr;

    delete d1;
    d1 = nullptr;

    delete inn;
    inn = nullptr;

}


void testCatalog(const char * cwd, SaxonProcessor * proc, sResultCount *sresult) {

#ifdef MEM_DEBUG
    SaxonProcessor::getInfo();
#endif
    cout << endl << "Test: testCatalog" << endl;
    bool trace = false;
    proc->setcwd(cwd);
    proc->setCatalog("../data/catalog.xml");
    if (proc->exceptionOccurred()) {
        const char *message = proc->getErrorMessage();
        if(message != nullptr) {
            cerr << "exception-proc=" << message << endl;
        }
        proc->exceptionClear();
        return;

    }
    Xslt30Processor * trans = proc->newXslt30Processor();

    XsltExecutable  * executable = trans->compileFromFile("../data/example.xsl");

    if(executable == nullptr) {

        if (trans->exceptionOccurred()) {

            if(trans->getErrorMessage() != nullptr) {
                const char *message = trans->getErrorMessage();
                cerr << "exception11=" << message << endl;
            } else {
                const char *message = proc->getErrorMessage();
                if(message != nullptr) {
                    cerr << "exception-proc=" << message << endl;
                }
            }

        }

        trans->exceptionClear();
        sresult->failure++;
        sresult->failureList.push_back("testCatalog");
        delete trans;
        trans= nullptr;
        return;
    }

    executable->setInitialMatchSelectionAsFile("../data/example.xml");

    const char *result = executable->applyTemplatesReturningString();

    if(result != NULL) {
        std::cerr << "testCatalog result= " << result << std::endl;
        delete result;
        sresult->success++; // TODO - check the results more carefully
    } else {
        sresult->failure++;
        sresult->failureList.push_back("testCatalog");
        if (executable->exceptionOccurred()) {

                    if(executable->getErrorMessage() != nullptr) {
                        const char *message = executable->getErrorMessage();
                        cerr << "exception message=" << message << endl;
                    }
        }


     }
  


    delete executable;
    executable = nullptr;
    delete trans;
    trans = nullptr;

}



void testSaxSourceNoSystemId(SaxonProcessor * proc, DocumentBuilder * builder, sResultCount *sresult) {

     cout << endl << "Test: testSaxSourceNoSystemId" << endl;
     //DocumentBuilder * builder = proc->newDocumentBuilder();

     XdmNode * d = builder->parseXmlFromString("<doc/>");

     if(d != nullptr) {
                if(d->getBaseUri() != nullptr) {
                    std::cout<<"BaseUri:"<<d->getBaseUri()<<endl;
                }
                delete d;
                sresult->success++;
     } else {


         if (builder->exceptionOccurred()) {

             if(builder->getErrorMessage() != nullptr) {
                 const char *message = builder->getErrorMessage();
                 cerr << "exception=" << message << endl;
             }

         }
         builder->exceptionClear();



         sresult->failure++;
         sresult->failureList.push_back("testSaxSourceNoSystemId");

     }
     delete builder;

    }



    void testSaxSource(SaxonProcessor * proc, sResultCount *sresult) {

         cout << endl << "Test: testSaxSource" << endl;
         DocumentBuilder * builder = proc->newDocumentBuilder();
         builder->setBaseUri("https://www.example.com");
         XdmNode * d = builder->parseXmlFromString("<doc/>");
         if(d != nullptr) {
                    if(d->getBaseUri() != nullptr) {
                        std::cout<<"BaseUri:"<<d->getBaseUri()<<endl;
                    }
                    delete d;
                    sresult->success++;
         } else {
                    sresult->failure++;
                    sresult->failureList.push_back("testSaxSource");

         }

         delete builder;

        }


void testSaxSourceDTDValidation(SaxonProcessor * proc, sResultCount *sresult) {
        std::string validXML = "<!DOCTYPE a [<!ELEMENT a EMPTY>]><a/>";
        std::string invalidXML = "<!DOCTYPE a [<!ELEMENT a (b+)>]><a/>";
        cout << endl << "Test: testSaxSourceDTDValidation" << endl;
        DocumentBuilder * builder = proc->newDocumentBuilder();
        builder->setDTDValidation(true);

        XdmNode * d = builder->parseXmlFromString(validXML.c_str());
        if(d != nullptr) {
                delete d;

        } else {
                builder->exceptionClear();
                sresult->failure++;
                sresult->failureList.push_back("testSaxSourceDTDValidation-1");
                delete builder;
                return;
        }

        d = builder->parseXmlFromString(invalidXML.c_str());

        if(d == nullptr) {
            builder->exceptionClear(); //TODO check exception

        } else {
            sresult->failure++;
            sresult->failureList.push_back("testSaxSourceDTDValidation-2");
            delete d;
            delete builder;
            return;
        }

        builder->setDTDValidation(false);
        d = builder->parseXmlFromString(invalidXML.c_str());
        if(d != nullptr) {
            delete d;
            sresult->success++;

        } else {
            builder->exceptionClear(); // Check exception
            sresult->failure++;
            sresult->failureList.push_back("testSaxSourceDTDValidation-3");

        }

        delete builder;
        return;

    }


static int NUM_THREADS = 10;

void RunThread(XsltExecutable * executable, int tid, const std::string filename) {
    /*JavaVMAttachArgs att_arg;
    att_arg.version = JNI_VERSION_1_2;
    att_arg.name = NULL;
    att_arg.group = NULL;*/

    //SaxonProcessor::sxn_environ->jvm->AttachCurrentThread((void**)&SaxonProcessor::sxn_environ->env, NULL);
    cerr<<endl<<"RunThread cp0,  THEAD ID="<<tid<<endl;
    fflush(stderr);
    fflush(stdout);

    if(executable != nullptr) {
       executable->setInitialMatchSelectionAsFile(filename.c_str());

        cerr << "RunThread cp1" << endl;

       const char *result = nullptr;
        result = executable->applyTemplatesReturningString();
        if (result != nullptr) {
            cout << " Result from THREAD ID: " << tid << ", " << result << endl;
            delete result;
        } else {
            cerr << " ===== Failed in THREAD ID: " << tid << endl;
           /* if(executable->exceptionOccurred()) {
                SaxonApiException *exception = executable->getException();

                if (exception != nullptr) {
                    const char *message = exception->getMessage();
                    if (message != nullptr) {
                        cerr << "Error = " << message << endl;
                    }
                    delete exception;
                }
            } else {
            cerr << "No exception found - result is nullptr" << endl;
            }*/

        }
    delete executable;
    } else {
        cerr << "XsltExecutable is nullptr" << endl;
    }

  //  (   SaxonProcessor::sxn_environ->jvm)->DetachCurrentThread();

}

void testThreads (SaxonProcessor * processor, Xslt30Processor * trans, sResultCount *sresult) {

    std::vector<std::string> s = {
            "../data/xmark100k.xml",
            "../data/xmark1.xml",
            "../data/xmark4.xml",
            "../data/xmark10.xml"
    };

    std::vector<std::thread> threads;

    for (int i = 0; i < s.size(); i++) {

        XsltExecutable * executable = trans->compileFromFile("../data/q12.xsl");
        if(executable == nullptr) {
            continue;
        }
            threads.push_back(std::thread(RunThread, executable, i, s[i]));

    }
    cerr<<"testThreads cp0"<<endl;
    for (auto &th : threads) {
        th.join();
        cerr<<"testThreads cp1 - loop"<<endl;
    }
}



int main(int argc, char* argv[]) {


    const char * cwd = nullptr;
    if(argc > 0) {
	cwd = argv[1];
    }
    int check =0;
    //cin>>check;
    SaxonProcessor *processor = new SaxonProcessor(true);
    setDebugMode(SaxonProcessor::sxn_environ->thread, 1);
#if defined (__linux__)
    processor->setConfigurationProperty("http://saxon.sf.net/feature/licenseFileLocation", "/usr/lib/saxon-license.lic");
#elif  defined (__APPLE__) && defined(__MACH__)
    processor->setConfigurationProperty("http://saxon.sf.net/feature/licenseFileLocation", "/usr/local/lib/saxon-license.lic");
#else
    //TODO windows - processor->setConfigurationProperty("http://saxon.sf.net/feature/licenseFileLocation", "/usr/lib/saxon-license.lic");
#endif
    cout << "Test: Xslt30Processor with Saxon version=" << processor->version() << endl << endl;

    char buff[FILENAME_MAX]; //create string buffer to hold path
    GetCurrentDir( buff, FILENAME_MAX );
    cout<<"CWD = "<< buff<<endl;
    if(cwd != nullptr) {
   	 processor->setcwd(cwd);//"/Users/ond1/work/development/git/saxon-dev/saxondev/src/main/c/samples/cppTests"); //set to the current working directory
    } else {
        processor->setcwd(buff);
    }
    if (processor->isSchemaAwareProcessor()) {

        std::cerr << "Processor is Schema Aware" << std::endl;
    } else {
        std::cerr << "Processor is not Schema Aware" << std::endl;
    }

    sResultCount *sresult = new sResultCount();

    Xslt30Processor *trans = processor->newXslt30Processor();

    if( trans == nullptr) {
        if(processor->exceptionOccurred()) {
            cout<< "Error message: " <<processor->getErrorMessage()<<endl;

        }
        return -1;
    }
    //testValidation(trans,sresult);

    testInitialTemplate(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    exampleSimple1Err(trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    exampleSimple1(trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    exampleSimple_xmark(trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    exampleSimple2(trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    exampleTransformToFile(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    exampleTransformToString(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    exampleSimple3(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    exampleSimple3aError(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testApplyTemplatesString1(trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testApplyTemplatesString2(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;


    testApplyTemplates2a_Error(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testTransformToString4(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testCatalog(cwd, processor,  sresult); 

    cout<<endl<<"============================================================="<<endl<<endl;

    testTransformToString2b(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testTransformToString3(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testTransformFromstring(processor, trans, sresult);


    cout<<endl<<"============================================================="<<endl<<endl;

    testTransformFromstring2Err(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testTrackingOfValueReference(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testTrackingOfValueReferenceError(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testPackage1(trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;


    testPackage1a(trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testPackage2_Error(trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testXdmNodeOutput(trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    exampleParam(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    xmarkTest1(trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    xmarkTest2(trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testCallFunction(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testResolveUri(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testResultDocumentAsMap(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testContextNotRoot(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testContextNotRootNamedTemplate(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testContextNotRootNamedTemplateValue(processor, trans, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

   testPipeline(processor, sresult);


    cout<<endl<<"============================================================="<<endl<<endl;


    DocumentBuilder * builder = processor->newDocumentBuilder();
    testSaxSourceNoSystemId(processor, builder, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testSaxSource(processor, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;

    testSaxSourceDTDValidation(processor, sresult);

    cout<<endl<<"============================================================="<<endl<<endl;


    testContext2NotRootNamedTemplate(processor, trans, sresult);

    fflush(stdout);


    //Available in PE and EE
    testTransformToStringExtensionFunc(cwd, processor, trans, sresult);



    //processor->release();
    //return 0;

    fflush(stdout);


    //set to the current working directory
    if(cwd != nullptr) {
        processor->setcwd(cwd);
    } else {
        processor->setcwd(buff);
    }
    Xslt30Processor *trans2 = processor->newXslt30Processor();

    testApplyTemplatesString1(trans2, sresult);
    cout<<endl<<"============================================================="<<endl<<endl;



    //testThreads (processor, trans2, sresult);


    delete trans;
    delete trans2;
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
