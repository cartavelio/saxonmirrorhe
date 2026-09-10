#include "saxonc/XsdSchema.h"
#include "saxonc/SchemaValidator.h"

#include <fstream>
#include <iostream>

#ifdef MEM_DEBUG
#define new new (__FILE__, __LINE__)
#endif



    XsdSchema::XsdSchema(const XsdSchema &other){
        cwdS = other.cwdS;
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        xsdSchemaObj = j_copy_handles(thread, (void *)other.xsdSchemaObj);
        if(xsdSchemaObj == SXN_EXCEPTION) {
            throw SaxonApiException();
        }

    }

    XsdSchema::XsdSchema(std::string cwd, int64_t p, int64_t exObject) {
        xsdSchemaObj = exObject;
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        procRef = j_copy_handles(thread, (void *)p);
        if (!(cwd.empty()) ) {
            cwdS = cwd;
        }
    }


    XsdSchema::~XsdSchema() {
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        if (xsdSchemaObj  > SXN_UNSET) {
            j_handles_destroy(thread, (void *)xsdSchemaObj);
        }

        if (procRef  > SXN_UNSET) {
            j_handles_destroy(thread, (void *)procRef);
        }
    }

    SchemaValidator * XsdSchema::newValidator() {
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        int64_t validatorObj =  j_newSchemaValidator(thread, (void *)xsdSchemaObj);

        return new SchemaValidator(cwdS, procRef, validatorObj);

    }

    XsdSchema * XsdSchema::combine(XsdSchema * other) {
        if (other == nullptr) {
            throw SaxonApiException("XsdSchema::combine() called with null XsdSchema");
        }
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        saxon_error_t r = SAXON_ERROR_DEFAULT;
        int64_t newXsdSchema =  j_xsd_schema_combine(thread, (void *)xsdSchemaObj, (void *)other->getUnderlyingSchema(), &r);

        if (r.status == SXN_EXCEPTION) {
            throw SaxonApiException(r);
        }

        return new XsdSchema(cwdS, procRef, newXsdSchema);


    }

    int64_t XsdSchema::getUnderlyingSchema() {
        return xsdSchemaObj;
    }

    void XsdSchema::exportComponents(const char * filename) {
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        saxon_error_t r = SAXON_ERROR_DEFAULT;
        //TODO we need to sort out the base URI part (i.e systemID). At the moment a full path must be supplied
        j_exportComponentsToFile(thread, (void *)xsdSchemaObj, (char *)filename, &r);

        if (r.status == SXN_EXCEPTION) {
            throw SaxonApiException(r);
        }
    }