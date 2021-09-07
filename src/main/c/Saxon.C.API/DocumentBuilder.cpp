// DocumentBuilder.cpp : Defines the exported functions for the DLL application.
//

#include "DocumentBuilder.h"





    DocumentBuilder::DocumentBuilder(jobject docObject, std::string cwd) {
           /*
             * Look for class.
             */
            docBuilderClass = lookForClass(SaxonProcessor::sxn_environ->env,
                                    "net/sf/saxon/s9api/DocumentBuilder");
            saxonCAPIClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/option/cpp/SaxonCAPI");


           docBuilderObject = SaxonProcessor::sxn_environ->env->NewGlobalRef(docObject);
           lineNumbering = false;
           dtdVal = false;
           schemaValidator = nullptr;

    }


    DocumentBuilder::DocumentBuilder(const DocumentBuilder &other) {
            docBuilderClass = lookForClass(SaxonProcessor::sxn_environ->env,
                                    "net/sf/saxon/s9api/DocumentBuilder");



        docBuilderObject = SaxonProcessor::sxn_environ->env->NewGlobalRef(other.docBuilderObject);
        lineNumbering = other.lineNumbering;
        schemaValidator = other.schemaValidator;
        dtdVal = other.dtdVal;
    }

    void DocumentBuilder::setLineNumbering(bool option){
        static jmethodID db_lineID = nullptr;

			db_lineID = (jmethodID) SaxonProcessor::sxn_environ->env->GetMethodID(docBuilderClass,
					"setLineNumbering",
					"(Z)");
        lineNumbering = option;
        SaxonProcessor::sxn_environ->env->CallVoidMethod(docBuilderObject, db_lineID, option);


    }


    bool DocumentBuilder::isLineNumbering(){
        return lineNumbering;


    }



    void DocumentBuilder::setSchemaValidator(SchemaValidator * validator){
        static jmethodID db_schemaID = nullptr;
	    if (!db_schemaID || validator == nullptr) {

	        return;
	    }
	    schemaValidator = validator;
		db_schemaID = (jmethodID) SaxonProcessor::sxn_environ->env->GetMethodID(docBuilderClass,
					"setSchemaValidator",
					"(Lnet/sf/saxon/s9api/SchemaValidator;)");
        SaxonProcessor::sxn_environ->env->CallVoidMethod(docBuilderObject, db_schemaID, validator->getUnderlyingValidator());


    }


    SchemaValidator * DocumentBuilder::getSchemaValidator() {
        return schemaValidator;
    }



    void DocumentBuilder::setDTDValidation(bool option) {
        static jmethodID db_dtdID = nullptr;
	    if (!db_dtdID ) {

	        return;
	    }
		db_dtdID = (jmethodID) SaxonProcessor::sxn_environ->env->GetMethodID(docBuilderClass,
					"setDTDValidation",
					"(Z)");
        dtdVal= option;
        SaxonProcessor::sxn_environ->env->CallVoidMethod(docBuilderObject, db_dtdID, dtdVal);




    }



    bool DocumentBuilder::isDTDValidation() {
        return dtdVal;
    }




    void DocumentBuilder::setBaseURI(const char* uri) {
        static jmethodID db_dtdID = nullptr;
	    if (!db_dtdID ) {

	        return;
	    }
		db_dtdID = (jmethodID) SaxonProcessor::sxn_environ->env->GetMethodID(docBuilderClass,
					"setBaseURI",
					"(Z)");
        baseURI= std::string(uri);
        SaxonProcessor::sxn_environ->env->CallVoidMethod(docBuilderObject, db_dtdID, dtdVal);

    }



    const char * DocumentBuilder::getBaseURI(){ return baseURI.c_str();}




    XdmNode* DocumentBuilder::parseXmlFromString(const char * source){
    jmethodID mID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(saxonCAPIClass, "parseXmlString",
                                                                                    "(Ljava/lang/String;Lnet/sf/saxon/s9api/Processor;Lnet/sf/saxon/s9api/DocumentBuilder;Lnet/sf/saxon/s9api/SchemaValidator;Ljava/lang/String;)Lnet/sf/saxon/s9api/XdmNode;");
    if (!mID) {
        std::cerr << "\nError: Saxonc." << "parseXmlString()" << " not found" << std::endl;
        return nullptr;
    }
//TODO SchemaValidator

    if(source == nullptr) {
        createException("Source string is NULL");
        return nullptr;
    }

    jobject xdmNodei = SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(saxonCAPIClass, mID,
                                                                                SaxonProcessor::sxn_environ->env->NewStringUTF(
                                                                                        cwd.c_str()), proc, docBuilderObject, (validator != nullptr ? validator->getUnderlyingValidator(): nullptr),
                                                                                SaxonProcessor::sxn_environ->env->NewStringUTF(
                                                                                        source));
    if (xdmNodei) {
        XdmNode *value = nullptr;
        value = new XdmNode(xdmNodei);
        SaxonProcessor::sxn_environ->env->DeleteLocalRef(xdmNodei);
        return value;
    } else if (exceptionOccurred()) {
        createException();
    }

#ifdef DEBUG
    SaxonProcessor::sxn_environ->env->ExceptionDescribe();
#endif

    return nullptr;


    }



   XdmNode* parseXmlFromFile(const char * filename){
    jmethodID mID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(saxonCAPIClass, "parseXmlFile",
                                                                                    "(Lnet/sf/saxon/s9api/Processor;Ljava/lang/String;Lnet/sf/saxon/s9api/DocumentBuilder;Lnet/sf/saxon/s9api/SchemaValidator;Ljava/lang/String;)Lnet/sf/saxon/s9api/XdmNode;");
    if (!mID) {
        std::cerr << "\nError: Saxonc.Dll " << "parseXmlFromFile()" << " not found" << std::endl;
        return nullptr;
    }
    jobject xdmNodei = SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(saxonCAPIClass, mID, proc,
                                                                                SaxonProcessor::sxn_environ->env->NewStringUTF(
                                                                                        ""),  docBuilderObject, (validator != nullptr ? validator->getUnderlyingValidator(): nullptr),
                                                                                SaxonProcessor::sxn_environ->env->NewStringUTF(
                                                                                        filename));
    if (xdmNodei) {
        XdmNode *value = nullptr;
        value = new XdmNode(xdmNodei);
        SaxonProcessor::sxn_environ->env->DeleteLocalRef(xdmNodei);
        return value;
    } else if (exceptionOccurred()) {
        createException();
    }
  }


   XdmNode* parseXmlFromUri(const char * uri){
    jmethodID mID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(saxonCAPIClass, "parseXmlFile",
                                                                                    "(Lnet/sf/saxon/s9api/Processor;Ljava/lang/String;Lnet/sf/saxon/s9api/DocumentBuilder;Lnet/sf/saxon/s9api/SchemaValidator;Ljava/lang/String;)Lnet/sf/saxon/s9api/XdmNode;");
    if (!mID) {
        std::cerr << "\nError: Saxonc.Dll " << "parseXmlFromFile()" << " not found" << std::endl;
        return nullptr;
    }
    jobject xdmNodei = SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(saxonCAPIClass, mID, proc,
                                                                                SaxonProcessor::sxn_environ->env->NewStringUTF(
                                                                                        ""),  docBuilderObject, (validator != nullptr ? validator->getUnderlyingValidator(): nullptr),
                                                                                SaxonProcessor::sxn_environ->env->NewStringUTF(
                                                                                        uri));
    if (xdmNodei) {
        XdmNode *value = nullptr;
        value = new XdmNode(xdmNodei);
        SaxonProcessor::sxn_environ->env->DeleteLocalRef(xdmNodei);
        return value;
    } else if (exceptionOccurred()) {
        createException();
    }



   }
