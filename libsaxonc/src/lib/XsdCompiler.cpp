#include "saxonc/XsdCompiler.h"
#include "saxonc/XsdSchema.h"
#include "saxonc/XdmMap.h"
#include "saxonc/XdmNode.h"
#include "saxonc/XdmValue.h"

#include <fstream>
#include <iostream>

#ifdef MEM_DEBUG
#define new new (__FILE__, __LINE__)
#endif


    XsdCompiler::XsdCompiler(){
        SaxonProcessor *p = new SaxonProcessor(true);
        XsdCompiler(p, "");
    }


    XsdCompiler::XsdCompiler(SaxonProcessor *p, std::string curr){
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        procRef = j_copy_handles(thread, (void *)p->procRef);
        saxon_error_t r = SAXON_ERROR_DEFAULT;
        xsdCompilerObj = j_create_sxn_XsdCompiler(thread,
                                                  (void *)procRef, &r);

        if (r.status == SXN_EXCEPTION) {
            throw SaxonApiException(r);
        }

        if (!(p->cwd.empty()) && curr.empty()) {
            cwdV = p->cwd;
        } else {
            cwdV = curr;
        }

    }

    XsdCompiler::XsdCompiler(const XsdCompiler &other){
        cwdV = other.cwdV;
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        xsdCompilerObj = j_copy_handles(thread, (void *)other.xsdCompilerObj);
        procRef = j_copy_handles(thread, (void *)other.procRef);
        if(xsdCompilerObj == SXN_EXCEPTION) {
            throw SaxonApiException("Failed to copy XsdCompiler");
        }

        std::map<std::string, XdmValue *>::const_iterator paramIter =
            other.parameters.begin();
        while (paramIter != other.parameters.end()) {

            XdmValue *valuei = paramIter->second;
            if (valuei == nullptr) {

            } else {
                parameters[paramIter->first] = new XdmValue(*(valuei));
            }
            paramIter++;
        }

        std::map<std::string, std::string>::const_iterator propIter =
            other.properties.begin();
        while (propIter != other.properties.end()) {
            properties[propIter->first] = propIter->second;
            propIter++;
        }



    }

    XsdCompiler::~XsdCompiler(){
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        if (xsdCompilerObj > 0) {
            j_handles_destroy(thread, (void *)xsdCompilerObj);
        }

        if (procRef > 0) {
            j_handles_destroy(thread, (void *)procRef);
        }

    }


    void XsdCompiler::setcwd(const char *dir){
        if (dir != nullptr) {
            cwdV = std::string(dir);
        }

    }


    const char * XsdCompiler::getcwd(){
        return cwdV.c_str();
    }



    void XsdCompiler::setXsdVersion(const char * version){
        if (version != nullptr) {
            xsdVersion = std::string(version);
            setProperty("xsdVersion", xsdVersion.c_str());
        }
    }


    const char * XsdCompiler::getXsdVersion(){
        return xsdVersion.c_str();
    }



    XsdSchema * XsdCompiler::compileFromString(const char * source, const char *encoding){
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        if (source == nullptr) {
            throw SaxonApiException("Error: source content string cannot be empty or NULL");
        }

        if (encoding != nullptr) {
            setProperty("!input-encoding", encoding);
        }


        int64_t processorDataRef =
            SaxonProcessor::createParameterJArray(parameters, properties);

        saxon_error_t r = SAXON_ERROR_DEFAULT;

        int64_t xsdSchemaObj = j_xsdCompileFromString(
            thread, (void *)xsdCompilerObj,
            (char *)cwdV.c_str(), (char *)source,
            (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                                   : (void *)processorDataRef), &r);

        if (processorDataRef > 0) {
            j_handles_destroy(thread,
                              (void *)processorDataRef);
            processorDataRef = SXN_UNSET;
        }

        if (r.status == SXN_EXCEPTION) {
            throw SaxonApiException(r);
        }

        XsdSchema * schema = new XsdSchema(cwdV, procRef, xsdSchemaObj);
        return schema;
    }

    XsdSchema * XsdCompiler::compileFromNode(XdmNode *node){

        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        if (node == nullptr) {
            throw SaxonApiException("Error: sourceFile string cannot be empty or NULL");
        }

        int64_t processorDataRef =
            SaxonProcessor::createParameterJArray(parameters, properties);

        saxon_error_t r = SAXON_ERROR_DEFAULT;

        int64_t xsdSchemaObj = j_xsdCompileFromNode(
            thread, (void *)xsdCompilerObj,
            (void *)node->getUnderlyingValue(),
            (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                                   : (void *)processorDataRef), &r);

        if (processorDataRef > 0) {
            j_handles_destroy(thread,
                              (void *)processorDataRef);
            processorDataRef = SXN_UNSET;
        }

        if (r.status == SXN_EXCEPTION) {
            throw SaxonApiException(r);
        }

        XsdSchema * schema = new XsdSchema(cwdV, procRef,     xsdSchemaObj);
        return schema;
    }

    XsdSchema * XsdCompiler::compileFromFiles(const char ** filenames, int count) {
        if (filenames == nullptr || count == 0) {
            throw SaxonApiException("Error: filenames cannot be NULL");
        }
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        int64_t schemaFilesRef = SXN_UNSET;

        saxon_error_t r = SAXON_ERROR_DEFAULT;

        schemaFilesRef = createProcessorDataWithCapacity(thread, (int) count);

        for (int i=0; i<count; i++) {
            if (filenames[i] != nullptr) {
                addProcessorProperty(thread, (void *)schemaFilesRef, (char *)filenames[i], &r);
            } else {
                throw SaxonApiException("Error: filename cannot be NULL");
            }
        }

        if (r.status == SXN_EXCEPTION) {
            throw SaxonApiException(r);
        }

        int64_t processorDataRef =
           SaxonProcessor::createParameterJArray(parameters, properties);

        int64_t xsdSchemaObj = j_xsdCompileFromProcessorData(
            thread, (void *)procRef, (void *)xsdCompilerObj,
            (char *)cwdV.c_str(), (void *)schemaFilesRef,
            (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                                   : (void *)processorDataRef), &r);

        if (processorDataRef > 0) {
            j_handles_destroy(thread,
                              (void *)processorDataRef);
            processorDataRef = SXN_UNSET;
        }

        if (schemaFilesRef != SXN_UNSET) {
            j_handles_destroy(thread,(void *)schemaFilesRef);
            schemaFilesRef = SXN_UNSET;
        }

        if (r.status == SXN_EXCEPTION) {
            throw SaxonApiException(r);
        }

        XsdSchema * schema = new XsdSchema(cwdV, procRef, xsdSchemaObj);
        return schema;

    }

    XsdSchema * XsdCompiler::compileFromFiles(const std::vector<std::string>&filenames) {

        if (filenames.size() == 0) {
            throw SaxonApiException("Error: filenames cannot be empty or NULL");
        }

        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        int64_t schemaFilesRef = SXN_UNSET;

        int size = filenames.size();
        schemaFilesRef = createProcessorDataWithCapacity(thread, size);
        saxon_error_t r = SAXON_ERROR_DEFAULT;

        for (const auto& s : filenames) {
                const char *filename = s.c_str();
                if (filename == nullptr) {
                    throw SaxonApiException("Error: filename cannot be NULL");
                }
                addProcessorProperty(thread, (void *)schemaFilesRef, (char *)filename, &r);
        }

        if (r.status == SXN_EXCEPTION) {
            throw SaxonApiException(r);
        }

        int64_t processorDataRef =
            SaxonProcessor::createParameterJArray(parameters, properties);



        int64_t xsdSchemaObj = j_xsdCompileFromProcessorData(
            thread, (void *)procRef, (void *)xsdCompilerObj,
            (char *)cwdV.c_str(), (char *)schemaFilesRef,
            (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                                   : (void *)processorDataRef), &r);

        if (processorDataRef > 0) {
            j_handles_destroy(thread,
                              (void *)processorDataRef);
            processorDataRef = SXN_UNSET;
        }

        if (schemaFilesRef != SXN_UNSET) {
            j_handles_destroy(thread,(void *)schemaFilesRef);
            schemaFilesRef = SXN_UNSET;
        }

        if (r.status == SXN_EXCEPTION) {
            throw SaxonApiException(r);
        }

        XsdSchema * schema = new XsdSchema(cwdV, procRef, xsdSchemaObj);
        return schema;
    }

    XsdSchema * XsdCompiler::compileFromFiles(std::initializer_list<std::string> filenamesList) {
        return compileFromFiles(std::vector<std::string>(filenamesList));
    }


    XsdSchema * XsdCompiler::compileFromFile(const char * sourceFile){
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        if (sourceFile == nullptr) {
            throw SaxonApiException("Error: sourceFile string cannot be empty or NULL");
        }


        int64_t processorDataRef =
            SaxonProcessor::createParameterJArray(parameters, properties);

        saxon_error_t r = SAXON_ERROR_DEFAULT;

        int64_t xsdSchemaObj = j_xsdCompileFromFile(
            thread, (void *)procRef, (void *)xsdCompilerObj,
            (char *)cwdV.c_str(), (char *)sourceFile,
            (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                                   : (void *)processorDataRef), &r);

        if (processorDataRef > 0) {
            j_handles_destroy(thread,
                              (void *)processorDataRef);
            processorDataRef = SXN_UNSET;
        }

        if (r.status == SXN_EXCEPTION) {
            throw SaxonApiException(r);
        }

        XsdSchema * schema = new XsdSchema(cwdV, procRef, xsdSchemaObj);
        return schema;
    }

void XsdCompiler::setProperty(const char *name, const char *value) {
#ifdef DEBUG
        if (value == nullptr) {
            std::cerr << "Validator setProperty is nullptr" << std::endl;
        }
#endif
        properties.insert(std::pair<std::string, std::string>(
            std::string(name), std::string((value == nullptr ? "" : value))));
    }


XsdSchema * XsdCompiler::combine(XsdSchema ** schemas, int num_schemas){
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);


        int64_t schemaRefs = createProcessorDataWithCapacity(
            thread, num_schemas);

        for (int i=0; i<num_schemas; i++) {
            addProcessorValue(thread,
                      (void *)schemaRefs,
                      (void *)schemas[i]->getUnderlyingSchema());
        }



        saxon_error_t r = SAXON_ERROR_DEFAULT;

        int64_t xsdSchemaObj = j_xsdcombine(
            thread, (void *)xsdCompilerObj, (void *)schemaRefs,&r);
        

        if (r.status == SXN_EXCEPTION) {
            throw SaxonApiException(r);
        }
        XsdSchema * schema = new XsdSchema(cwdV, procRef, xsdSchemaObj);
        return schema;

    }

    /**
     * Return an empty schema. This may be useful for validating instance documents when all
     * the actual schema definitions are linked from the instance document using <code>xsi:schemaLocation</code>
     * and <code>xsi:noNamespaceSchemaLocation</code> attributes
     * @return an empty schema
     */
    XsdSchema * XsdCompiler::emptySchema(){
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

        int64_t xsdSchemaObj = j_xsdempty(thread, (void *)xsdCompilerObj);

        XsdSchema * schema = new XsdSchema(cwdV, procRef, xsdSchemaObj);
        return schema;


}

    XsdSchema * XsdCompiler::importComponentsFromNode(XdmNode * node) {
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        if (node == nullptr) {
            throw SaxonApiException("Error: source node cannot be empty or NULL");
        }

        saxon_error_t r = SAXON_ERROR_DEFAULT;

        int64_t xsdSchemaObj = j_xsdImportComponentsFromNode(
            thread, (void *)procRef, (void *)xsdCompilerObj,
            (void *)node->getUnderlyingValue(), &r);

        if (r.status == SXN_EXCEPTION) {
            throw SaxonApiException(r);
        }

        XsdSchema * schema = new XsdSchema(cwdV, procRef, xsdSchemaObj);
        return schema;

    }


    XsdSchema * XsdCompiler::importComponents(const char * sourceFile){
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        if (sourceFile == nullptr) {
            throw SaxonApiException("Error: sourceFile string cannot be empty or NULL");
        }

        saxon_error_t r = SAXON_ERROR_DEFAULT;
        int64_t processorDataRef =
            SaxonProcessor::createParameterJArray(parameters, properties);

        int64_t xsdSchemaObj = j_xsdImportComponents(thread, (void *)procRef, (void *)xsdCompilerObj,
            (char *)cwdV.c_str(), (char *)sourceFile,
            (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                                   : (void *)processorDataRef), &r);

        if (processorDataRef > 0) {
            j_handles_destroy(thread,
                              (void *)processorDataRef);
            processorDataRef = SXN_UNSET;
        }

        if (r.status == SXN_EXCEPTION) {
            throw SaxonApiException(r);
        }

        XsdSchema * schema = new XsdSchema(cwdV, procRef, xsdSchemaObj);
        return schema;


    }
