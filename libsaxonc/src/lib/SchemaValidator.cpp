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

SchemaValidator::SchemaValidator(SaxonProcessor *p, std::string curr) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  procRef = j_copy_handles(thread, (void *)p->procRef);
  cppV = createSchemaValidatorWithProcessor(thread,(void *)procRef);
  exception = nullptr;

  if (cppV < 0) {
    throw SaxonApiException();
  }

  if (!(p->cwd.empty()) && curr.empty()) {
    cwdV = p->cwd;
  } else {
    cwdV = curr;
  }

  lax = false;
}

SchemaValidator::SchemaValidator(const SchemaValidator &other) {

  exception = nullptr;
  lax = other.lax;
  cwdV = other.cwdV;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  cppV = j_copy_handles(thread, (void *)other.cppV);
  procRef = j_copy_handles(thread, (void *)other.procRef);
  if(cppV == SXN_EXCEPTION) {
    throw SaxonApiException();
  }

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
  clearProperties();
  clearParameters();
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (cppV > 0) {
    j_handles_destroy(thread, (void *)cppV);
  }

  if (procRef != SXN_UNSET) {
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
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t result =
      j_getValidationReport(thread, (void *)cppV);

  if (result > 0) {
    XdmNode *node = new XdmNode(result);
    return node;
  } else if (result == SXN_EXCEPTION) {
    throw SaxonApiException();
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
    exception = new SaxonApiException("Error:: XdmNode XSD is NULL");
    return;
  }

  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties);
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t status = j_registerSchemaNode(thread, (void *)procRef,
      (void *)node->getUnderlyingValue(),
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef));

  if (processorDataRef > 0) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (status == SXN_EXCEPTION) {
    throw SaxonApiException();
  }

}

void SchemaValidator::registerSchemaFromFile(const char *sourceFile) {
  if (sourceFile == nullptr) {
    throw SaxonApiException("Error: sourceFile string cannot be empty or NULL");
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties);

  int64_t status = j_registerSchema(thread, (void *)procRef,
      (char *)cwdV.c_str(), (char *)sourceFile,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef));

  if (processorDataRef > 0) {
    j_handles_destroy(thread,
                      (void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (status == SXN_EXCEPTION) {
    throw SaxonApiException();
  }

}

void SchemaValidator::exportSchema(const char *fileName) {
  if (fileName == nullptr) {
    exception = new SaxonApiException(
        "Error: fileName string cannot be empty or nullptr");
    return;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t status =
      j_exportSchema(thread, (void *)procRef,
                     (char *)cwdV.c_str(), (char *)fileName);
  if (status == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
}

void SchemaValidator::registerSchemaFromString(const char *sourceStr,
                                               const char *systemID) {
  if (sourceStr == nullptr) {
    exception = new SaxonApiException(
        "Error:: Schema string cannot be empty or nullptr");
    return;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties);

  int64_t status = j_registerSchemaString(thread, (void *)procRef,
      (char *)(systemID == nullptr ? cwdV.c_str() : systemID),
      (char *)sourceStr,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef));

  if (processorDataRef > 0) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  
  if (status == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
}

void SchemaValidator::validate(const char *sourceFile) {
  if (lax) {
    setProperty("lax", "1");
  }
  if(!outputFile.empty()) {
    setProperty("o", outputFile.c_str());
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties);
  int64_t status = j_validate(thread, (void *)procRef, (void *)cppV,
      (char *)cwdV.c_str(), (char *)sourceFile, (char *)nullptr,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef));
  if (processorDataRef > 0) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (status == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
}

XdmNode *SchemaValidator::validateToNode(const char *sourceFile) {
  if (lax) {
    setProperty("lax", "1");
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties);
  int64_t result = j_validateToNode(thread, (void *)procRef, (void *)cppV,
      (char *)cwdV.c_str(), (char *)sourceFile,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef));
  if (processorDataRef > 0) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (result > 0) {
    XdmNode *node = new XdmNode(result);
    return node;
  } else if (result == SXN_EXCEPTION) {
    throw SaxonApiException();
  }

  return nullptr;
}

void SchemaValidator::exceptionClear() {
  if (exception != nullptr) {
    delete exception;
    exception = nullptr;
  }
}

const char *SchemaValidator::getErrorCode() {
  return nullptr;
}

const char *SchemaValidator::getErrorMessage() {
  return nullptr;
}

bool SchemaValidator::exceptionOccurred() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  bool exceptionOccurred = (bool)j_checkForException(thread);
  return exceptionOccurred || exception != nullptr;
}

const char *SchemaValidator::checkException() { return nullptr; }


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
