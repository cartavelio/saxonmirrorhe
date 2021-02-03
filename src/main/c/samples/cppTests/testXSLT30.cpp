#include <sstream>
#include <stdio.h>
#include "../../Saxon.C.API/SaxonProcessor.h"
#include "../../Saxon.C.API/XdmValue.h"
#include "../../Saxon.C.API/XdmItem.h"
#include "../../Saxon.C.API/XdmNode.h"
#include "../../Saxon.C.API/XdmFunctionItem.h"
#include "cppExtensionFunction.h"
#include "CppTestUtils.h"
#include <string>

// TODO: write test case for checking parameters which are null


using namespace std;
char fname[] = "_nativeCall";
char funcParameters[] = "(Ljava/lang/String;[Ljava/lang/Object;[Ljava/lang/String;)Ljava/lang/Object;";

JNINativeMethod cppMethods[] =
        {
                {
                        fname,
                        funcParameters,
                        (void *) &cppExtensionFunction::cppNativeCall
                }
        };


/*
* Test transform to String. Source and stylesheet supplied as arguments
*/
void testApplyTemplatesString1(Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: testApplyTemplatesString1:" << endl;
//    trans->setupXslMessage(false);

    XsltExecutable *executable = trans->compileFromFile("test.xsl");

    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testApplyTemplatesString1");
        cerr << "testApplyTemplatesString1 NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testApplyTemplatesString1 error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }
    executable->setInitialMatchSelectionAsFile("cat.xml");

    const char *output = executable->applyTemplatesReturningString();
    if (output == nullptr) {
        printf("result is null ====== FAIL ====== \n");
        sresult->failure++;
        fflush(stdout);
        sresult->failureList.push_back("testApplyTemplatesString1-0");
        return;
    } else if (string(output).find(string("<out>text2</out>")) != std::string::npos) {
        printf("%s", output);
        printf("result is OK \n");
        sresult->success++;
    } else {
        printf("result is null ====== FAIL ====== \n");
        sresult->failure++;
        sresult->failureList.push_back("testApplyTemplatesString1-1");
//	std::cout<<"output="<<output<<std::endl;
    }
    fflush(stdout);
    cout<<"checkout XXX0"<<endl;
    delete output;
    cout<<"checkout XXX1"<<endl;
    delete executable;

}

/*
* Test transform to String. Source and stylesheet supplied as arguments
*/
void testTransformToStringExtensionFunc(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: TransformToStringExtensionFunc:" << endl;
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

    executable->setProperty("extc", "home/ond1/work/svn/latest9.9-saxonc/samples/cppTests/cppExtensionFunction");

    if (nativeFound) {
        const char *output = executable->transformFileToString("cat.xml");

        if (output == nullptr) {
            SaxonProcessor::sxn_environ->env->ExceptionDescribe();
            printf("result is null ====== FAIL ======  \n");
            sresult->failureList.push_back("testTransformToStringExnteionFunc");
        } else {
            //printf("%s", output);
            printf("result is OK \n");
        }
        fflush(stdout);
        delete output;
    } else {
        printf("native Class not foun ====== FAIL ====== ");
        sresult->failureList.push_back("testTransformToStringExtensionFunc");
    }
    delete executable;
}


/*
* Test transform to String. stylesheet supplied as argument. Source supplied as XdmNode
*/
void testApplyTemplatesString2(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: testApplyTemplatesString2:" << endl;
    trans->clearParameters(true);

    XdmNode *input = processor->parseXmlFromFile("cat.xml");

    if (input== nullptr) {
        cout << "Source document is null." << endl;

    }
    XsltExecutable * executable = trans->compileFromFile("test.xsl");
    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testApplyTemplatesString2");
        cerr << "testApplyTemplatesString2 NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testApplyTemplatesString2 error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }
    executable->setInitialMatchSelection((XdmValue *) input);
    const char *output = executable->applyTemplatesReturningString();
    if (output== nullptr) {
        printf("result is null ====== FAIL ======  \n");
        sresult->failureList.push_back("testApplyTemplatesString2");
    } else {
        printf("%s", output);
        printf("result is OK \n");
    }
    fflush(stdout);
    delete output;
    delete executable;
}

/*
* Test transform to String. stylesheet supplied as argument. Source supplied as XdmNode
Should be error. Stylesheet file does not exist
*/
void testApplyTemplates2a(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: TransformToString2a:" << endl;
    trans->clearParameters(true);
    XdmNode *input = processor->parseXmlFromFile("cat.xml");

    if (input== nullptr) {
        cout << "Source document is null. ====== FAIL ======" << endl;
        sresult->failure++;
        sresult->failureList.push_back("testApplyTemplates2a");
        return;
    }
    XsltExecutable * executable = trans->compileFromFile("test-error.xsl");

    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testApplyTemplates2a");
        cerr << "testApplyTemplates2a NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testApplyTemplates2a error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }

    executable->setInitialMatchSelection((XdmValue *) input);
    const char *output = executable->applyTemplatesReturningString();
    if (output== nullptr) {
        printf("result is null \n");
        sresult->success++;
    } else {
        printf("%s", output);
        printf("result is OK - ======= FAIL ======= \n");
        sresult->failure++;
        sresult->failureList.push_back("testApplyTemplates2a");
    }
    fflush(stdout);
    delete output;
    delete executable;


}

/*
* Test transform to String. stylesheet supplied as argument. Source supplied as XdmNode
Should be error. Source file does not exist
*/
void testTransformToString2b(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: TransformToString2b:" << endl;
    trans->clearParameters(true);

    XdmNode *input = processor->parseXmlFromFile("cat-error.xml");

    if (input== nullptr) {
        cout << "Source document is null. ====== FAIL ====== " << endl;
        sresult->failureList.push_back("testTransformToString2b");
        return;
    }
    XsltExecutable * executable = trans->compileFromString("test-error.xsl");

    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testTransformToString2b");
        cerr << "testTransformToString2b NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testTransformToString2b error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }

    executable->setInitialMatchSelection((XdmNode *) input);
    const char *output = executable->transformFileToString(NULL);
    if (output== nullptr) {
        printf("result is null ====== FAIL ======  \nCheck For errors:");
        sresult->failureList.push_back("testTransformToString2b");
        if (executable->exceptionOccurred()) {
            cout << trans->getErrorMessage() << endl;
        }
    } else {
        printf("%s", output);
        printf("result is OK \n");
    }
    fflush(stdout);
    delete output;

    trans->exceptionClear();
    delete executable;

}


/*
* Test transform to String. stylesheet supplied as argument. Source supplied as xml string
and integer parmater created and supplied
*/
void testTransformToString3(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: TransformToString3:" << endl;
    trans->clearParameters(true);

    XdmNode *inputi = processor->parseXmlFromString(
            "<out><person>text1</person><person>text2</person><person>text3</person></out>");

    XdmAtomicValue *value1 = processor->makeIntegerValue(10);

    trans->setParameter("numParam", (XdmValue *) value1);

    if (inputi== nullptr) {
        cout << "Source document inputi is null. ====== FAIL ====== " << endl;
        sresult->failureList.push_back("testTransformToString3");
        return;
    }

    XsltExecutable * executable = trans->compileFromFile("test.xsl");

    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testTransformToString3");
        cerr << "testTransformToString3 NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testTransformToString3 error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }

    executable->setInitialMatchSelection((XdmNode *) inputi);
    const char *output = executable->applyTemplatesReturningString();
    if (output== nullptr) {
        printf("result is null ====== FAIL ====== \n");
        sresult->failureList.push_back("testTransformToString3");
    } else {
        printf("%s", output);
        printf("result is OK \n");
    }
    fflush(stdout);
    delete output;
    delete value1;
    delete executable;

}

/*
* Test transform to String. stylesheet supplied as argument. Source supplied as xml string
and integer parmater created and supplied
*/
void testTransformToString4(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: TransformToString4:" << endl;
    trans->clearParameters(true);

    XdmNode *input = processor->parseXmlFromString(
            "<out><person>text1</person><person>text2</person><person>text3</person></out>");

    XdmValue *values = new XdmValue();
    values->addXdmItem((XdmItem *) processor->makeIntegerValue(10));
    values->addXdmItem((XdmItem *) processor->makeIntegerValue(5));
    values->addXdmItem((XdmItem *) processor->makeIntegerValue(6));
    values->addXdmItem((XdmItem *) processor->makeIntegerValue(7));


    if (input== nullptr) {
        cout << "Source document is null. ====== FAIL ====== " << endl;
        sresult->failureList.push_back("testTransformToString4");
    }
    XdmNode *sheet = processor->parseXmlFromFile("test2.xsl");

    XsltExecutable *executable = trans->compileFromXdmNode(sheet);

    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testTransformToString4");
        cerr << "testTransformToString4 NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testTransformToString4 error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }

    executable->setParameter("values", (XdmValue *) values);
    executable->setInitialMatchSelection((XdmNode *) input);
    const char *output = executable->applyTemplatesReturningString(/*"test2.xsl"*/);
    if (output== nullptr) {
        printf("result is null \n");
        sresult->failureList.push_back("testTransformToString4");
    } else {
        printf("%s", output);
        printf("result is OK \n");
    }
    fflush(stdout);
    delete output;
    delete executable;
    delete input;
    delete values;

}

void testTransformFromstring(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {
    cout << endl << "Test: testTransfromFromstring:" << endl;
    trans->clearParameters(true);

    XdmNode *input = processor->parseXmlFromString(
            "<out><person>text1</person><person>text2</person><person>text3</person></out>");

    XsltExecutable * executable = trans->compileFromString(
            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='2.0'>       <xsl:param name='values' select='(2,3,4)' /><xsl:output method='xml' indent='yes' /><xsl:template match='*'><output><xsl:for-each select='$values' ><out><xsl:value-of select='. * 3'/></out></xsl:for-each></output></xsl:template></xsl:stylesheet>");

    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testTransformFromstring");
        cerr << "testTransformFromstring NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testTransformFromstring error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }

    const char *output = executable->transformToString((XdmNode *) input);
    if (output== nullptr) {
        printf("result is null ====== FAIL ====== \n");
        sresult->failureList.push_back("testTransformFromString");
    } else {
        printf("%s", output);
        printf("result is OK \n");

    }
    fflush(stdout);
    delete output;
    delete input;
    delete executable;


}

//Test case has error in the stylesheet
void testTransformFromstring2Err(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {
    cout << endl << "Test: testTransfromFromstring2-Error:" << endl;
    trans->clearParameters(true);

    XdmNode *input = processor->parseXmlFromString(
            "<out><person>text1</person><person>text2</person><person>text3</person></out>");

    XsltExecutable * executable = trans->compileFromString(
            "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='2.0'>       <xsl:param name='values' select='(2,3,4)' /><xsl:output method='xml' indent='yes' /><xsl:template match='*'><output><xsl:for-each select='$values' ><out><xsl:value-of select='. * 3'/></out><xsl:for-each></output></xsl:template><xsl:stylesheet>");

    if(executable== nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testTransformFromstring2Err");
        cerr << "testTransformFromstring2Err NULL found" << endl;
        if (trans->exceptionOccurred()) {
            cerr <<"testTransformFromstring2Err error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }

    executable->setInitialMatchSelection((XdmValue *) input);
    const char *output = executable->applyTemplatesReturningString();
    if (output== nullptr) {
        printf("result is null ====== FAIL ======  \n\nCheck For errors:\n");
        sresult->failureList.push_back("testTransformFromstring2Err");
        if (executable->exceptionOccurred()) {
            cout << "Error message=" << executable->getErrorMessage() << endl;
        }
    } else {
        printf("%s", output);
        printf("result is OK \n");
    }
    fflush(stdout);
    delete output;

    trans->exceptionClear();
    delete input;
    delete executable;

}

void testTrackingOfValueReference(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {
    trans->clearParameters(true);

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
    }

    std::map<std::string, XdmValue *> parMap = trans->getParameters();
    if (parMap.size() > 0) {
        //cout << "Parameter size: " << parMap.size() << endl;
        //cout << "Parameter size: " << parMap.size()<< endl;//", Value:"<<trans->getParameters()["value0"]->getHead()->getStringValue()<<endl;
        ostringstream name1;
        for (int i = 0; i < 10; i++) {
            name1 << "param:value" << i;
            cout << " i:" << i << " Map size:" << parMap.size() << ", ";
            XdmValue *valuei = parMap[name1.str()];
            if (valuei != NULL) {
                cout << name1.str();
                if (valuei->itemAt(0) != NULL)
                    cout << "= " << valuei->itemAt(0)->getStringValue();
                cout << endl;
            } else {
                sresult->failure++;
                std::cerr << "trackingValueReference ====== FAIL ======" << std::endl;
                sresult->failureList.push_back("testTrackingOfValueReference");
            }
            name1.str("");
        }
    }
    sresult->success++;
}

/*Test case should be error.*/
void testTrackingOfValueReferenceError(SaxonProcessor *processor, Xslt30Processor *trans, sResultCount *sresult) {
    trans->clearParameters(true);

    cout << endl << "Test: TrackingOfValueReference-Error:" << endl;
    cout << "Parameter Map size: " << (trans->getParameters().size()) << endl;
    ostringstream test;
    ostringstream valStr;
    ostringstream name;
    for (int i = 0; i < 10; i++) {
        test << "v" << i;
        valStr << "<out><person>text1</person><person>text2</person><person>text3</person><value>" << test.str()
               << "<value></out>";
        name << "value" << i;

        XdmValue *values = (XdmValue *) processor->parseXmlFromString(valStr.str().c_str());
        trans->setParameter(name.str().c_str(), values);
        test.str("");
        valStr.str("");
        name.str("");
    }
    std::map<std::string, XdmValue *> parMap = trans->getParameters();
    cout << "Parameter Map size: " << parMap.size() << endl;

    ostringstream name1;
    bool errorFound = false;
    for (int i = 0; i < 10; i++) {
        name1 << "param:value" << i;
        cout << " i:" << i << " Map size:" << parMap.size() << ", ";
        try {
            XdmValue *valuei = parMap.at(name1.str());
            if (valuei != NULL) {
                cout << name1.str();
                if (valuei->itemAt(0) != NULL)
                    cout << "= " << valuei->itemAt(0)->getStringValue();
                cout << endl;
            }
        } catch (const std::out_of_range &oor) {
            cout << "Out of range exception occurred. Exception no. " << endl;
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
    trans->clearParameters(true);



    XsltExecutable * executable = trans->compileFromString(
            "<?xml version='1.0'?><xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform'                 xmlns:xs='http://www.w3.org/2001/XMLSchema' version='3.0' exclude-result-prefixes='#all'>     <xsl:import-schema><xs:schema><xs:element name='x' type='xs:int'/></xs:schema></xsl:import-schema> <xsl:template name='main'>          <xsl:result-document validation='strict'> <x>3</x>   </xsl:result-document>    </xsl:template>    </xsl:stylesheet>");

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


void testXdmNodeOutput(Xslt30Processor *trans, sResultCount *sresult) {
    trans->clearParameters(true);


    std::cout << "testXdmNodeOutput" << std::endl;
    XsltExecutable * executable = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template name='go'><a/></xsl:template></xsl:stylesheet>");
    XdmValue *rootValue = executable->callTemplateReturningValue("go");
    if (rootValue== nullptr) {
        cout << "Result is null ====== FAIL ====== " << endl;
        sresult->failure++;
        sresult->failureList.push_back("testXdmNodeOutput-0.0");
        return;
    }
    XdmItem *rootItem = rootValue->getHead();
    if (rootItem== nullptr) {

        cout << "Result is null ====== FAIL ====== " << endl;
        sresult->failure++;
        sresult->failureList.push_back("testXdmNodeOutput-0");
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
    } else {
        //TODO - this test case prints the XML declaration. Check if this correct
        sresult->failure++;
        cout << "testXdmNodeOutputAndString ======= FAIL========" << endl;
        sresult->failureList.push_back("testXdmNodeOutput");
    }
    delete executable;
    delete result;

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
    } else {
        cout << "Result is null ====== FAIL ====== " << endl;
        sresult->failure++;
    }
    proc->clearParameters(true);
    delete executable;

}

void exampleSimple1Err(Xslt30Processor *proc, sResultCount *sresult) {
    cout << "ExampleSimple1Err taken from PHP:" << endl;

    XsltExecutable * executable = proc->compileFromFile("err.xsl");
    if(proc->exceptionOccurred()) {
        cout << "Result expected as null " << endl;
        sresult->success++;
    } else {

        sresult->failure++;
        sresult->failureList.push_back("exampleSimple1Err");

    }
    proc->clearParameters(true);
    delete executable;
}


void exampleSimple2(Xslt30Processor *proc, sResultCount *sresult) {
    cout << "<b>exampleSimple2:</b><br/>" << endl;
    proc->setcwd("/Users/ond1/work/development/git/saxon-dev/saxondev/src/main/c/samples/cppTests");

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
    cout << "exampleSimple2 cp0" << endl;
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
    proc->clearParameters(true);
    executable->clearParameters(true);
    delete executable;

}

void exampleSimple3(SaxonProcessor *saxonProc, Xslt30Processor *proc, sResultCount *sresult) {
    cout << "<b>exampleSimple3:</b><br/>" << endl;
    proc->clearParameters(true);

    XsltExecutable * executable = proc->compileFromFile("../php/xsl/foo.xsl");

   if(executable== nullptr) {
	cout << "executable is NULL" <<endl;
	if(proc->exceptionOccurred()) {
		cout << proc->getErrorMessage() <<endl;
		
	}
	return;

   }

    XdmNode *xdmNode = saxonProc->parseXmlFromString("<doc><b>text value of out</b></doc>");
    if (xdmNode== nullptr) {
        cout << "Error: xdmNode is null'" << endl;
        sresult->failure++;
        sresult->failureList.push_back("exampleSimple3");
        return;
    }
    executable->setInitialMatchSelection((XdmNode *) xdmNode);
    cout << "end of exampleSimple3" << endl;
    executable->clearParameters(true);

    sresult->success++;

}

void exampleSimple3a(SaxonProcessor *saxonProc, Xslt30Processor *proc, sResultCount *sresult) {
    cout << "<b>exampleSimple3:</b><br/>" << endl;
    proc->clearParameters(true);

    XsltExecutable * executable = proc->compileFromFile(nullptr);


    if (executable == nullptr) {


        cout << "end of exampleSimple3a" << endl;
        proc->clearParameters(true);

        sresult->success++;
        return;
    }

    cout << "Error: executable is not nullptr'" << endl;
    sresult->failure++;
    sresult->failureList.push_back("exampleSimple3a");
    return;


}

void exampleParam(SaxonProcessor *saxonProc, Xslt30Processor *proc, sResultCount *sresult) {
    cout << "\nExampleParam:</b><br/>" << endl;

    XsltExecutable * executable = proc->compileFromFile("../php/xsl/foo.xsl");

    executable->setInitialMatchSelectionAsFile("../php/xml/foo.xml");

    XdmAtomicValue *xdmvalue = saxonProc->makeStringValue("Hello to you");
    if (xdmvalue != NULL) {

        executable->setParameter("a-param", (XdmValue *) xdmvalue);

    } else {
        cout << "Xdmvalue is null - ====== FAIL =====" << endl;
        sresult->failure++;
        sresult->failureList.push_back("exampleParam-1");
    }
    const char *result = executable->applyTemplatesReturningString();
    if (result != NULL) {
        cout << "Output:" << result << endl;
        sresult->success++;
    } else {
        cout << "Result is NULL<br/>  ======= fail =====" << endl;
        sresult->failure++;
        sresult->failureList.push_back("exampleParam-2");
    }

    //proc->clearParameters();
    //unset($result);
    //echo 'again with a no parameter value<br/>';

    executable->setProperty("!indent", "yes");
    const char *result2 = executable->applyTemplatesReturningString();


    if (result2 != NULL) {
        cout << result2 << endl;
        sresult->success++;
    }

    //  unset($result);
    // echo 'again with no parameter and no properties value set. This should fail as no contextItem set<br/>';
    XdmAtomicValue *xdmValue2 = saxonProc->makeStringValue("goodbye to you");
    executable->setParameter("a-param", (XdmValue *) xdmValue2);

    const char *result3 = executable->applyTemplatesReturningString();
    if (result3 != NULL) {
        cout << "Output =" << result3 << endl;
        sresult->success++;
    } else {
        cout << "Error in result ===== FAIL =======" << endl;
        sresult->failure++;
        sresult->failureList.push_back("exampleParam");
    }
    proc->clearParameters();


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

    if(result2 != NULL) {
        cout<<result2<<endl;
        sresult->success++;

    }

    //  unset($result);
    // echo 'again with no parameter and no properties value set. This should fail as no contextItem set<br/>';
    XdmAtomicValue *xdmValue2 = saxonProc->makeStringValue("goodbye to you");
    executable->setParameter("a-param", (XdmValue*)xdmValue2);

    const char *result3 = executable->applyTemplatesReturningString();
    if(result3 != NULL) {
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
    cout << "XMarkbench mark test q12.xsl (JIT=true):" << endl;

    proc->setJustInTimeCompilation(true);

    XdmValue *result = proc->transformFileToValue("xmark100k.xml", "q12.xsl");
    if (result != NULL) {
        cout << "XdmNode returned" << endl;
        sresult->success++;
    } else {
        printf("result is null \nCheck For errors:");
        sresult->failure++;
        sresult->failureList.push_back("xmarkTest1");
        if (proc->exceptionOccurred() > 0) {
            cout << proc->getErrorMessage() << endl;
        }
    }
    proc->clearParameters(true);


}


/* XMarkbench mark test q12.xsl with just-in-time=true*/
void xmarkTest2(Xslt30Processor *proc, sResultCount *sresult) {
    cout << "XMarkbench mark test q12.xsl (JIT=true):" << endl;


    XdmValue *result = proc->transformFileToValue("xmark100k.xml", "q12.xsl");
    if (result != NULL) {
        cout << "XdmNode returned" << endl;
    } else {
        printf("result is null \nCheck For errors:");
        sresult->failure++;
        sresult->failureList.push_back("xmarkTest2");
        if (proc->exceptionOccurred() > 0) {
            cout << proc->getErrorMessage() << endl;
        }
    }
    proc->clearParameters(true);


}

/* XMarkbench mark test q12.xsl with just-in-time=true*/
void exampleSimple_xmark(Xslt30Processor *proc, sResultCount *sresult) {
    cout << "XMarkbench mark test q12.xsl:" << endl;

    proc->setJustInTimeCompilation(true);

    XdmValue *result = proc->transformFileToValue("xmark100k.xml", "q12.xsl");
    if (result != NULL) {
        cout << "XdmNode returned" << endl;
    } else {
        printf("result is null \nCheck For errors:");
        if (proc->exceptionOccurred() > 0) {
            cout << proc->getErrorMessage() << endl;
        }
    }
    proc->clearParameters(true);

}


/*
* Test saving nd loading a Xslt package
*/
void testPackage1(Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: Saving and loading Packages:" << endl;
    trans->clearParameters(true);


    trans->compileFromFileAndSave("test.xsl", "test1.sef");
    const char *output = trans->transformFileToString("cat.xml", "test1.sef");
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
void testPackage2(Xslt30Processor *trans, sResultCount *sresult) {

    cout << endl << "Test: Saving and loading Packages2 - Error:" << endl;
    trans->clearParameters(true);


    const char *stylesheet = "<xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' version='2.0'>       <xsl:param name='values' select='(2,3,4)' /><xsl:output method='xml' indent='yes' /><xsl:template match='*'><output><xsl:for-each select='$values' ><out><xsl:value-of select='. * 3'/></out><xsl:for-each></output></xsl:template><xsl:stylesheet>";

    trans->compileFromStringAndSave(stylesheet, "test2.sef");
    const char *output = trans->transformFileToString("cat.xml", "test2.sef");
    if (output== nullptr) {
        printf("result is null as expected \n");
        const char *message = trans->getErrorMessage();
        if (message != NULL) {
            cout << "Error message =" << message << endl;
        }
        sresult->success++;
    } else {
        printf("%s", output);
        printf("result is OK \n");
        sresult->failure++;
        sresult->failureList.push_back("testPackage2");
    }
    fflush(stdout);
    delete output;
}

void testCallFunction(SaxonProcessor *proc, Xslt30Processor *trans, sResultCount *sresult) {

    const char *source = "<?xml version='1.0'?> <xsl:stylesheet xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:f='http://localhost/' version='3.0'> <xsl:function name='f:add' visibility='public'>    <xsl:param name='a'/><xsl:param name='b'/> <xsl:sequence select='$a + $b'/></xsl:function></xsl:stylesheet>";
    cout << endl << "Test: testCallFunction:" << endl;
    XsltExecutable * executable = trans->compileFromString(source);
    XdmValue **valueArray = new XdmValue *[2];

    valueArray[0] = (XdmValue *) (proc->makeIntegerValue(2));
    valueArray[1] = (XdmValue *) (proc->makeIntegerValue(3));
    XdmValue *v = executable->callFunctionReturningValue("{http://localhost/}add", valueArray, 2);

    if (v != NULL && (v->getHead())->isAtomic() && ((XdmAtomicValue *) (v->getHead()))->getLongValue() == 5) {
        sresult->success++;
    } else {
        if (v != NULL && !(v->getHead())->isAtomic()) {
            cout << "Value in callFunction is not atomic - but expected as atomic value" << endl;
        }
        cout << "testCallFunction ======= FAIL ======" << endl;
        const char *message = executable->getErrorMessage();
        if (message != NULL) {
            cout << "Error message =" << message << endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("testCallFunction");
    }
    delete valueArray[0];
    delete valueArray[1];
    delete [] valueArray;
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
    XdmNode *node = proc->parseXmlFromString("<e/>");

    executable->setResultAsRawValue(false);
    std::map<std::string, XdmValue *> parameterValues;
    parameterValues["a"] = proc->makeIntegerValue(12);
    parameterValues["b"] = proc->makeIntegerValue(5);
    executable->setInitialTemplateParameters(parameterValues, false);
    executable->setInitialMatchSelection(node);
    XdmValue *result = executable->applyTemplatesReturningValue();
    if (result != NULL) {
        sresult->success++;
        cout << "Result=" << result->getHead()->getStringValue() << endl;
    } else {
        sresult->failure++;
    }

}

void testResolveUri(SaxonProcessor *proc, Xslt30Processor *trans, sResultCount *sresult) {
    cout << endl << "Test: testResolveUri:" << endl;

    XsltExecutable * executable = trans->compileFromString(
            "<xsl:stylesheet version='3.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform' xmlns:xs='http://www.w3.org/2001/XMLSchema' xmlns:err='http://www.w3.org/2005/xqt-errors'><xsl:template name='go'><xsl:try><xsl:variable name='uri' as='xs:anyURI' select=\"resolve-uri('notice trailing space /out.xml')\"/> <xsl:message select='$uri'/><xsl:result-document href='{$uri}'><out/></xsl:result-document><xsl:catch><xsl:sequence select=\"'$err:code: ' || $err:code  || ', $err:description: ' || $err:description\"/></xsl:catch></xsl:try></xsl:template></xsl:stylesheet>");

    if(executable == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testResolveUri");
        if(trans->exceptionOccurred()) {
            cout << "Error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }
    XdmValue *value = executable->callTemplateReturningValue("go");


    if (value== nullptr) {

        sresult->failure++;
        sresult->failureList.push_back("testResolveUri");
    } else {

        const char *svalue = value->itemAt(0)->getStringValue();
        cout << "testResolveUri = " << svalue << endl;
        sresult->success++;
    }


}

void testContextNotRoot(SaxonProcessor *proc, Xslt30Processor *trans, sResultCount *sresult) {
    cout << endl << "Test: testContextNotRoot" << endl;

    XdmNode *node = proc->parseXmlFromString("<doc><e>text</e></doc>");

    XsltExecutable * executable = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:variable name='x' select='.'/><xsl:template match='/'>errorA</xsl:template><xsl:template match='e'>[<xsl:value-of select='name($x)'/>]</xsl:template></xsl:stylesheet>");

    if(executable == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testContextNotRoot");
        if(trans->exceptionOccurred()) {
            cout << "Error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }

    executable->setGlobalContextItem(node);
    if (node->getChildCount() > 0) {
        XdmNode *eNode = node->getChildren()[0]->getChildren()[0];
        cout << "Node content = " << eNode->toString() << endl;
        executable->setInitialMatchSelection(eNode);
        const char *result = executable->applyTemplatesReturningString();

        if (result== nullptr) {

            cout << "testCallFunction ======= FAIL ======" << endl;
            const char *message = executable->getErrorMessage();
            if (message != NULL) {
                cout << "Error message =" << message << endl;
            }
            sresult->failure++;
            sresult->failureList.push_back("testContextNotRoot");
        } else {

            cout << "testContextNotRoot = " << result << endl;
            sresult->success++;
        }
    }
}


void testContextNotRootNamedTemplate(SaxonProcessor *proc, Xslt30Processor *trans, sResultCount *sresult) {
    cout << endl << "Test: testContextNotRootNamedTemplate" << endl;

    XdmNode *node = proc->parseXmlFromString("<doc><e>text</e></doc>");

    XsltExecutable * executable = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:variable name='x' select='.'/><xsl:template match='/'>errorA</xsl:template><xsl:template name='main'>[<xsl:value-of select='name($x)'/>]</xsl:template></xsl:stylesheet>");

    if(executable == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testContextNotRootNamedTemplate");
        if(trans->exceptionOccurred()) {
            cout << "Error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }

    executable->setGlobalContextItem(node);
    const char *result = executable->callTemplateReturningString("main");

    if (result== nullptr) {

        cout << "testCallFunction ======= FAIL ======" << endl;
        const char *message = executable->getErrorMessage();
        if (message != NULL) {
            cout << "Error message =" << message << endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("testContextNotRootNamedTemplate");
    } else {

        cout << "testContextNotRoot = " << result << endl;
        sresult->success++;
    }

}


void testContextNotRootNamedTemplateValue(SaxonProcessor *proc, Xslt30Processor *trans, sResultCount *sresult) {
    cout << endl << "Test: testContextNotRootNamedTemplateValue" << endl;

    XdmNode *node = proc->parseXmlFromString("<doc><e>text</e></doc>");

    XsltExecutable * executable = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:variable name='x' select='.'/><xsl:template match='/'>errorA</xsl:template><xsl:template name='main'>[<xsl:value-of select='name($x)'/>]</xsl:template></xsl:stylesheet>");

    if(executable == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testContentNotRootNamedTemplateValue");
        if(trans->exceptionOccurred()) {
            cout << "Error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }

    executable->setGlobalContextItem(node);
    XdmValue *result = executable->callTemplateReturningValue("main");

    if (result== nullptr) {

        cout << "testCallFunction ======= FAIL ======" << endl;
        const char *message = executable->getErrorMessage();
        if (message != NULL) {
            cout << "Error message =" << message << endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("testContextNotRootNamedTemplateValue");
    } else {

        cout << "testContextNotRoot = " << result->getHead()->getStringValue() << endl;
        sresult->success++;
    }

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
    XdmValue *result = fi->call(xdmValue, 1);

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
    XsltExecutable * stage1 = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template></xsl:stylesheet>");

    if(stage1 == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testPipeline");
        if(trans->exceptionOccurred()) {
            cout << "stage 1 Error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }

    XdmNode *inn = proc->parseXmlFromString("<z/>");

    XsltExecutable *stage2 = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template></xsl:stylesheet>");

    if(stage2 == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testPipeline");
        if(trans->exceptionOccurred()) {
            cout << "stage 2 Error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }
    XsltExecutable *stage3 = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template></xsl:stylesheet>");

    if(stage3 == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testPipeline");
        if(trans->exceptionOccurred()) {
            cout << "stage 3 Error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }
    XsltExecutable *stage4 = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template></xsl:stylesheet>");

    if(stage3 == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testPipeline");
        if(trans->exceptionOccurred()) {
            cout << "stage 4 Error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }

    XsltExecutable *stage5 = trans->compileFromString(
            "<xsl:stylesheet version='2.0' xmlns:xsl='http://www.w3.org/1999/XSL/Transform'><xsl:template match='/'><a><xsl:copy-of select='.'/></a></xsl:template></xsl:stylesheet>");

    if(stage5 == nullptr) {
        sresult->failure++;
        sresult->failureList.push_back("testPipeline");
        if(trans->exceptionOccurred()) {
            cout << "stage 5 Error: "<< trans->getErrorMessage() << endl;
        }
        return;
    }

    stage1->setProperty("!indent", "no");
    stage1->setInitialMatchSelection(inn);

    XdmValue *d1 = stage1->applyTemplatesReturningValue();
    if (stage1->exceptionOccurred()) {
        sresult->failure++;
        sresult->failureList.push_back("testPipeline");
        cout << "Exception message=" << (stage1->getErrorMessage()) << endl;
        return;
    }

    if (d1 == nullptr) {
        cout << "ERROR1 :"<< stage1->getErrorMessage() << endl;;
        sresult->failure++;
        sresult->failureList.push_back("testPipeline-0");
        return;
    }
    XdmItem *d11 = d1->getHead();
    if (d11== nullptr) {

        cout << "d11 is NULL\n" << endl;
    }
    const char *data = d1->getHead()->toString();

    if (data != NULL) {
        cout << "d1 result=" << data << endl;
    } else {
        sresult->failure++;
        sresult->failureList.push_back("testPipeline-1");

        return;
    }
    stage2->setProperty("!indent", "no");
    stage2->setInitialMatchSelection(d1->getHead());
    XdmValue *d2 = stage2->applyTemplatesReturningValue();
    if (d2== nullptr) {
        cout << "ERROR-11\n" << endl;
        if (stage2->exceptionOccurred()) {
            const char *message = stage2->getErrorMessage();
            cout << "exception=" << message << endl;
        }
        sresult->failure++;
        sresult->failureList.push_back("testPipeline-2");
        return;
    }
    stage3->setProperty("!indent", "no");
    stage3->setInitialMatchSelection(d2);
    XdmValue * d3 = stage3->applyTemplatesReturningValue();
    if(d3 == nullptr){
        sresult->failure++;
        sresult->failureList.push_back("testPipeline-3");
        if (stage3->exceptionOccurred()) {
            const char *message = stage3->getErrorMessage();
            cout << "exception=" << message << endl;
        }
      }
    stage4->setProperty("!indent", "no");
    stage4->setInitialMatchSelection(d3);
    XdmValue * d4 = stage4->applyTemplatesReturningValue();
    if(d3== nullptr){
        sresult->failure++;
        sresult->failureList.push_back("testPipeline-1");
        if (stage4->exceptionOccurred()) {
            const char *message = stage4->getErrorMessage();
            cout << "exception=" << message << endl;
        }
        return;
      }
    stage5->setProperty("!indent", "no");
    stage5->setInitialMatchSelection(d4);
    const char * sw = stage5->applyTemplatesReturningString();
    if(sw==NULL){
        sresult->failure++;
        sresult->failureList.push_back("testPipeline-1");
        if (stage5->exceptionOccurred()) {
            const char *message = stage5->getErrorMessage();
            cout << "exception=" << message << endl;
        }
        return;
      }
    cout<<sw<<endl;
    cout << "testPipeline = " << sw << endl;
    sresult->success++;


}


void testCatalog(const char * cwd, SaxonProcessor * proc, sResultCount *sresult) {


    bool trace = false;
    proc->setcwd(cwd);
    proc->setCatalog("../php/catalog-test/catalog.xml", trace);

    Xslt30Processor * trans = proc->newXslt30Processor();


    XsltExecutable  * executable = trans->compileFromFile("../php/catalog-test/test1.xsl");

    if(executable == nullptr) {
        if (trans->exceptionOccurred()) {
            const char *message = trans->getErrorMessage();
            cout << "exception=" << message << endl;
        }
    }
    //trans->setSourceFromFile("/home/ond1/work/svn/latest9.9-saxonc/samples/php/catalog-test/example.xml");
    executable->setInitialMatchSelectionAsFile("../php/catalog-test/example.xml");
//trans->compileFromFile("/home/ond1/work/svn/latest9.9-saxonc/samples/php/catalog-test/test1.xsl");
    const char *result = executable->applyTemplatesReturningString();

    if(result != NULL) {
        std::cerr << "testCatalog result= " << result << std::endl;
    }

}


/*
 *
 *void *RunThread(void *args) {

    struct arg_struct *argsi = (struct arg_struct *)args;
    int threadid = argsi->id;
    Xslt30Processor * trans = argsi->trans;
    long tid;
    tid = (long)threadid;

    trans->attachThread();

   trans->setInitialMatchSelectionAsFile("../php/xml/foo.xml");

    const char *result = trans->applyTemplatesReturningString();
    cout<<" Result from THREAD ID: "<< tid << ", " << result<<endl;
    delete result;
    //pthread_exit(NULL);
    trans->detachThread();
}

void testThreads (SaxonProcessor * processor) {
    pthread_t threads[NUM_THREADS];
    int rc;
    int i;

    Xslt30Processor *  trans = processor->newXslt30Processor();

    trans->compileFromFile("../php/xsl/foo.xsl");
    struct arg_struct args;
    args.trans = trans;

    for( i = 0; i < NUM_THREADS; i++ ) {
        cout << "main() : creating thread, " << i << endl;
        args.id = i;
        rc = pthread_create(&threads[i], NULL, RunThread, (void *)&args);

        if (rc) {
            cout << "Error:unable to create thread," << rc << endl;
            exit(-1);
        }
	(void) pthread_join(threads[i], NULL);
    }
  //  pthread_exit(NULL);
}

 *
 */


int main(int argc, char* argv[]) {


    const char * cwd = nullptr;
    if(argc > 0) {
	cwd = argv[1];
    }

    SaxonProcessor *processor = new SaxonProcessor(true);
    processor->setConfigurationProperty("http://saxon.sf.net/feature/licenseFileLocation", "/usr/local/lib/saxon-license.lic");
    cout << "Test: Xslt30Processor with Saxon version=" << processor->version() << endl << endl;
    
    char buff[FILENAME_MAX]; //create string buffer to hold path
    GetCurrentDir( buff, FILENAME_MAX );
    cout<<"CWD = "<< buff<<endl;
    if(cwd != nullptr) {
   	 processor->setcwd(cwd);//"/Users/ond1/work/development/git/saxon-dev/saxondev/src/main/c/samples/cppTests"); //set to the current working directory
    }
    if (processor->isSchemaAwareProcessor()) {

        std::cerr << "Processor is Schema Aware" << std::endl;
    } else {
        std::cerr << "Processor is not Schema Aware" << std::endl;
    }

    sResultCount *sresult = new sResultCount();
    Xslt30Processor *trans = processor->newXslt30Processor();

    if( trans == nullptr) {
        cout<< "Error creating Xslt30Processor"<<endl;
        if(processor->exceptionOccurred()) {
            cout<< "Error message: " <<processor->getErrorMessage()<<endl;

        }
        return -1;
    }
    //testValidation(trans,sresult);
    std::cerr << "cp0" << std::endl;
    testInitialTemplate(processor, trans, sresult);
    std::cerr << "cp1" << std::endl;
    exampleSimple1Err(trans, sresult);
    std::cerr << "cp2" << std::endl;
    exampleSimple1(trans, sresult);
    std::cerr << "cp3" << std::endl;

    exampleSimple_xmark(trans, sresult);
    std::cerr << "cp4" << std::endl;
    exampleSimple2(trans, sresult);
    std::cerr << "cp5" << std::endl;
    exampleSimple3(processor, trans, sresult);
    std::cerr << "cp6" << std::endl;
    exampleSimple3a(processor, trans, sresult);
    std::cerr << "cp7" << std::endl;
    testApplyTemplatesString1(trans, sresult);
    std::cerr << "cp8" << std::endl;
    testApplyTemplatesString2(processor, trans, sresult);

    testApplyTemplates2a(processor, trans, sresult);

    testTransformToString4(processor, trans, sresult);

    testCatalog(cwd, processor,  sresult);

    /*testTransformToString2b(processor, trans, sresult);

    testTransformToString3(processor, trans, sresult);
	
    testTransformFromstring(processor, trans, sresult);

    testTransformFromstring2Err(processor, trans, sresult);      */

    testTrackingOfValueReference(processor, trans, sresult);

    testTrackingOfValueReferenceError(processor, trans, sresult);

    testPackage1(trans, sresult);

    testPackage2(trans, sresult);

    testXdmNodeOutput(trans, sresult);

    exampleParam(processor, trans, sresult);

    xmarkTest1(trans, sresult);

    xmarkTest2(trans, sresult);
    testCallFunction(processor, trans, sresult);
    testResolveUri(processor, trans, sresult);
    testContextNotRoot(processor, trans, sresult);
    testContextNotRootNamedTemplate(processor, trans, sresult);
    testContextNotRootNamedTemplateValue(processor, trans, sresult);
    testPipeline(processor, sresult);

    //Available in PE and EE
    //testTransformToStringExtensionFunc(processor, trans);

    delete trans;
    delete processor;
    // processor->release();


    SaxonProcessor *processor2 = new SaxonProcessor(true);

    Xslt30Processor *trans2 = processor2->newXslt30Processor();
    testApplyTemplatesString1(trans2, sresult);
    delete trans2;
    processor2->release();

    std::cout << "\nTest Results - Number of tests= " << (sresult->success + sresult->failure) << ", Successes = "
              << sresult->success << ",  Failures= " << sresult->failure << std::endl;

    std::list<std::string>::iterator it;
    std::cout << "Failed tests:" << std::endl;
// Make iterate point to begining and incerement it one by one till it reaches the end of list.
    for (it = sresult->failureList.begin(); it != sresult->failureList.end(); it++) {
        //Print the contents
        std::cout << it->c_str() << std::endl;

    }


    return 0;
}
