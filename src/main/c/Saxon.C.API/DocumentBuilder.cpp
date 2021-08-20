// DocumentBuilder.cpp : Defines the exported functions for the DLL application.
//

#include "DocumentBuilder.h"





    DocumentBuilder::DocumentBuilder(jobject docObject, std::string cwd) {
           /*
             * Look for class.
             */
            docBuilderClass = lookForClass(SaxonProcessor::sxn_environ->env,
                                    "net/sf/saxon/s9api/DocumentBuilder");


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




    XdmNode* DocumentBuilder::parseXmlFromString(const char * content){ return nullptr;}



   XdmNode* parseXmlFromFile(const char * filename){ return nullptr;}
