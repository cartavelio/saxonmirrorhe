#include "saxonc/SchemaValidator.h"
#include "saxonc/XdmItem.h"
#include "saxonc/XdmNode.h"
#include "saxonc/XdmValue.h"

//#define DEBUG

#ifdef MEM_DEBUG
#define new new (__FILE__, __LINE__)
#endif

SchemaValidator::SchemaValidator() {
  SaxonProcessor *p = new SaxonProcessor(true);
  SchemaValidator(p, "");
}

SchemaValidator::SchemaValidator(std::string curr, int64_t p ,int64_t schemaValidatorObject) {
  cppV = schemaValidatorObject;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  procRef = j_copy_handles(thread, (void *)p);
  if (!(curr.empty())) {
    cwdV = curr;
  }
  lax = false; // default is strict (i.e. false)


}

SchemaValidator::SchemaValidator(SaxonProcessor *p, std::string curr) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  procRef = j_copy_handles(thread, (void *)p->procRef);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  cppV = SXN_UNSET;

  if (r.status == SXN_EXCEPTION) {
    j_handles_destroy(thread, (void *)procRef);
    throw SaxonApiException(r);
  }

  if (!(p->cwd.empty()) && curr.empty()) {
    cwdV = p->cwd;
  } else {
    cwdV = curr;
  }

  lax = false;
}

SchemaValidator::SchemaValidator(const SchemaValidator &other) {

  lax = other.lax;
  cwdV = other.cwdV;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (cppV != SXN_UNSET) {
    cppV = j_copy_handles(thread, (void *)other.cppV);
  }
  procRef = j_copy_handles(thread, (void *)other.procRef);

  outputFile = other.outputFile;

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

SchemaValidator::~SchemaValidator() {
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "~SchemaValidator destructor called: ob ref=" << (this)  <<" handle ref="<<(int)cppV<< std::endl;
  }
  clearProperties();
  clearParameters();
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (cppV > SXN_UNSET) {
    j_handles_destroy(thread, (void *)cppV);
  }


  if (procRef != SXN_UNSET) {
    if (getenv("SAXONC_DEBUG_FLAG")) {
      std::cerr << "~SchemaValidator destructor destroyed procRef="<< ((int)procRef) << std::endl;
    }
    j_handles_destroy(thread, (void *)procRef);
    procRef = SXN_UNSET;
  }
}

void SchemaValidator::setcwd(const char *dir) {
  if (dir != nullptr) {
    cwdV = std::string(dir);
  }
}

const char * SchemaValidator::getcwd() {
  return cwdV.c_str();
}

void SchemaValidator::setOutputFile(const char *sourceFile) {
  if (sourceFile != nullptr) {
    outputFile = std::string(sourceFile);
  }
}

const char * SchemaValidator::getOutputFile() {
  return outputFile.c_str();
}

void SchemaValidator::setLax(bool l) { lax = l; }

XdmNode *SchemaValidator::getValidationReport() {
  if (cppV == SXN_UNSET) {
    return nullptr;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t result =
      j_getValidationReport(thread, (void *)cppV, &r);

  if (result > SXN_UNSET) {
    XdmNode *node = new XdmNode(result);
    return node;
  } else if (r.status == SXN_EXCEPTION) {
    throw SaxonApiException(r);
  }
  return nullptr;
}

SaxonProcessor* SchemaValidator::getSaxonProcessor() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  bool licensed = j_isLicensed(thread, (void *)procRef);
  int64_t newProcRef = j_copy_handles(thread, (void *)procRef);
  SaxonProcessor * processor = new SaxonProcessor(newProcRef, licensed);
  processor->setcwd(cwdV.c_str());
  return processor;
}

void SchemaValidator::registerSchemaFromNode(XdmNode *node) {

  if (node == nullptr) {
    throw SaxonApiException("Error:: XdmNode XSD is NULL");
  }

  if (cppV > SXN_UNSET) {
    throw SaxonApiException("The registration of the schema method has already been called.");
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;


  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties);
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

  cppV = j_registerSchemaNode(thread, (void *)procRef,
      (void *)node->getUnderlyingValue(),
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef > 0) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (r.status == SXN_EXCEPTION) {
    throw SaxonApiException(r);
  }

}

void SchemaValidator::registerSchemaFromFile(const char *sourceFile) {
  if (sourceFile == nullptr) {
    throw SaxonApiException("Error: sourceFile string cannot be empty or NULL");
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties);
  if (cppV > SXN_UNSET) {
    throw SaxonApiException("The registration of the schema method has already been called.");
  }
  cppV = j_registerSchema(thread, (void *)procRef,
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

}

void SchemaValidator::exportSchema(const char *fileName) {
  if (fileName == nullptr) {
    throw SaxonApiException(
        "Error: fileName string cannot be empty or nullptr");
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  j_exportSchema(thread, (void *)procRef,
                     (char *)cwdV.c_str(), (char *)fileName, &r);
  if (r.status == SXN_EXCEPTION) {
    throw SaxonApiException(r);
  }
}

void SchemaValidator::registerSchemaFromString(const char *sourceStr,
                                               const char *systemID) {
  if (sourceStr == nullptr) {
    throw SaxonApiException(
        "Error:: Schema string cannot be empty or nullptr");
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties);

  if (cppV > SXN_UNSET) {
    throw SaxonApiException("The registration of the schema method has already been called.");
  }

  cppV = j_registerSchemaString(thread, (void *)procRef,
      (char *)(systemID == nullptr ? cwdV.c_str() : systemID),
      (char *)sourceStr,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef > 0) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (r.status == SXN_EXCEPTION) {
    throw SaxonApiException(r);
  }
}

void SchemaValidator::validateFromString(const char *content) {
  if (cppV == SXN_UNSET) {
    throw SaxonApiException("SchemaValidator is invalid");
  }
  if (content == nullptr) {
    throw SaxonApiException("Source content name is NULL");
  }
  if (lax) {
    setProperty("lax", "1");
  }
  if(!outputFile.empty()) {
    setProperty("o", outputFile.c_str());
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties);
  j_validateFromString(thread, (void *)procRef, (void *)cppV,
      (char *)cwdV.c_str(), (char *)content, (char *)nullptr,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);
  if (processorDataRef > 0) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (r.status == SXN_EXCEPTION) {
    throw SaxonApiException(r);
  }

}

void SchemaValidator::validate(const char *sourceFile) {

  if (cppV == SXN_UNSET) {
    throw SaxonApiException("SchemaValidator is invalid");
  }
  if (lax) {
    setProperty("lax", "1");
  }
  if(!outputFile.empty()) {
    setProperty("o", outputFile.c_str());
  }

  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties);
  j_validate(thread, (void *)procRef, (void *)cppV,
      (char *)cwdV.c_str(), (char *)sourceFile, (char *)nullptr,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);
  if (processorDataRef > 0) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (r.status == SXN_EXCEPTION) {
    throw SaxonApiException(r);
  }
}

XdmNode *SchemaValidator::validateToNode(const char *sourceFile) {
  if (cppV == SXN_UNSET) {
    throw SaxonApiException("SchemaValidator is invalid");
  }
  if (lax) {
    setProperty("lax", "1");
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties);
  int64_t result = j_validateToNode(thread, (void *)procRef, (void *)cppV,
      (char *)cwdV.c_str(), (char *)sourceFile,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);
  if (processorDataRef > 0) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (result > 0) {
    XdmNode *node = new XdmNode(result);
    return node;
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r);
  }

  return nullptr;
}


void SchemaValidator::setSourceNode(XdmNode *value) {
  if (value != nullptr) {
    setParameter("node", value, false);
  }
}

XdmValue *SchemaValidator::getParameter(const char *name, bool withParam) {
  std::map<std::string, XdmValue *>::iterator it;
  it = parameters.find((withParam ? "param:" : "") + std::string(name));
  if (it != parameters.end())
    return it->second;

  return nullptr;
}

void SchemaValidator::setParameter(const char *name, XdmValue *value,  bool withParam) {
  if (value != nullptr) {
    std::string skey = ((withParam ? "param:" : "") + std::string(name));
    std::map<std::string, XdmValue *>::iterator it = parameters.find(skey);
    if (it != parameters.end()) {
      XdmValue *valuei = it->second;
      if (valuei != nullptr) {
          parameters.erase(skey);
      }
    }
    parameters[skey] = value;
  }
}

bool SchemaValidator::removeParameter(const char *name) {
  return (bool)(parameters.erase("param:" + std::string(name)));
}

void SchemaValidator::setProperty(const char *name, const char *value) {
#ifdef DEBUG
  if (value == nullptr) {
    std::cerr << "Validator setProperty is nullptr" << std::endl;
  }
#endif
  properties.insert(std::pair<std::string, std::string>(
      std::string(name), std::string((value == nullptr ? "" : value))));
}

void SchemaValidator::clearParameters(bool delValues) {
    if (delValues ) {
        for (std::map<std::string, XdmValue *>::iterator itr = parameters.begin();
             itr != parameters.end(); itr++) {
            XdmValue *value = itr->second;
            if (delValues && value != nullptr) {
              if (getenv("SAXONC_DEBUG_FLAG")) {
                std::cerr << "SchemaValidator.clearParameter() - XdmValue refCount="
                  << value->getRefCount() << std::endl;
              }
              delete value;
              value = nullptr;
            }
        }
    }
    parameters.clear();
}

void SchemaValidator::clearProperties() { properties.clear(); }

std::map<std::string, XdmValue *> &SchemaValidator::getParameters() {
  std::map<std::string, XdmValue *> &ptr = parameters;
  return ptr;
}

std::map<std::string, std::string> &SchemaValidator::getProperties() {
  std::map<std::string, std::string> &ptr = properties;
  return ptr;
}
