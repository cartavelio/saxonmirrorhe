// Xslt30Processor.cpp : Defines the exported functions for the DLL application.
//

#include "saxonc/Xslt30Processor.h"
#include "saxonc/XdmArray.h"
#include "saxonc/XdmAtomicValue.h"
#include "saxonc/XdmFunctionItem.h"
#include "saxonc/XdmItem.h"
#include "saxonc/XdmMap.h"
#include "saxonc/XdmNode.h"
#include "saxonc/XdmValue.h"
//#include "XsltExecutable.h"
#ifdef DEBUG
#include <typeinfo> //used for testing only
#endif

#ifdef MEM_DEBUG
#define new new (__FILE__, __LINE__)
#endif

Xslt30Processor::Xslt30Processor() {

  SaxonProcessor *p = new SaxonProcessor(false);
  Xslt30Processor(p, "");
}

Xslt30Processor::Xslt30Processor(SaxonProcessor *proc, std::string curr) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

  procRef = j_copy_handles(thread, (void *)proc->procRef);
  jitCompilation = false;
  importPackageValue = SXN_UNSET;
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  cppXT = createXslt30WithProcessor(thread,
                                    (void *)proc->procRef, &r);

#ifdef DEBUG
  // jmethodID debugMID =
  // SaxonProcessor::sxn_environ->env->GetStaticMethodID(cppClass,
  // "setDebugMode", "(Z)V");
  // SaxonProcessor::sxn_environ->env->CallStaticVoidMethod(cppClass, debugMID,
  // (jboolean)true);

#endif

  if (r.status != SAXON_OK) {
    j_handles_destroy(thread, (void *)procRef);
    throw SaxonApiException(r);
  }

  if (!(proc->cwd.empty()) && curr.empty()) {
    cwdXT = proc->cwd;
  } else if (!curr.empty()) {
    cwdXT = curr;
  }
  schemaRef = SXN_UNSET;
}

Xslt30Processor::~Xslt30Processor() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  clearParameters();
  clearProperties();
  if (cppXT != SXN_UNSET) {
    j_handles_destroy(thread, (void *)cppXT);
    cppXT = SXN_UNSET;
  }
  cwdXT.erase();
  if (importPackageValue != SXN_UNSET) {
    j_handles_destroy(thread,
                      (void *)importPackageValue);
    importPackageValue = SXN_UNSET;
  }
  if (procRef != SXN_UNSET) {
    j_handles_destroy(thread,
                      (void *)procRef);
    procRef = SXN_UNSET;
  }
  if (schemaRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)schemaRef);
    schemaRef = SXN_UNSET;
  }
}

Xslt30Processor::Xslt30Processor(const Xslt30Processor &other) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  procRef = j_copy_handles(thread, (void *)other.procRef);
  cppXT = j_copy_handles(thread, (void *)other.cppXT);
  if (cppXT == SXN_UNSET) {
    throw SaxonApiException("Failed to copy internal SaxonProcessor");
  }

  cwdXT = other.cwdXT;
  importPackageValue = other.importPackageValue;
  std::map<std::string, XdmValue *>::const_iterator paramIter =
      other.parameters.begin();
  while (paramIter != other.parameters.end()) {

    XdmValue *valuei = paramIter->second;
    if (valuei == nullptr) {
      // std::cerr<<"Error in Xslt30Processor copy constructor"<<std::endl;
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

  jitCompilation = other.jitCompilation;
}

void Xslt30Processor::setProperty(const char *name, const char *value) {
  if (name != nullptr) {
    properties.erase(name);
    std::string skey = std::string(name);
    properties[skey] = std::string((value == nullptr ? "" : value));
  }
}

std::map<std::string, std::string> &Xslt30Processor::getProperties() { return properties; }


void Xslt30Processor::clearProperties() { properties.clear(); }

void Xslt30Processor::setBaseOutputURI(const char *baseURI) {
  if (baseURI != nullptr) {
    setProperty("baseoutput", baseURI);
  }
}

void Xslt30Processor::setParameter(const char *name, XdmValue *value) {
  if (value != nullptr && name != nullptr) {
    std::string skey = ("sparam:" + std::string(name));
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

XdmValue *Xslt30Processor::getParameter(const char *name) {
  std::map<std::string, XdmValue *>::iterator it;
  it = parameters.find("sparam:" + std::string(name));
  if (it != parameters.end())
    return it->second;
  return nullptr;
}

bool Xslt30Processor::removeParameter(const char *name) {
  return (bool)(parameters.erase("sparam:" + std::string(name)));
}

void Xslt30Processor::setJustInTimeCompilation(bool jit) {
  jitCompilation = jit;
}

bool Xslt30Processor::getJustInTimeCompilation() {

  return jitCompilation;
}

void Xslt30Processor::setFastCompilation(bool fast) {
  if (fast) {
    setProperty("-fastCompile", "1");
  } else {
    setProperty("-fastCompile", "0");
  }
}

void Xslt30Processor::setRelocatable(bool flag) {
  if (flag) {
    setProperty("-relocate", "1");
  } else {
    setProperty("-relocate", "0");
  }
}

void Xslt30Processor::setXsltLanguageVersion(const char *version) {
  if (version != nullptr) {
    setProperty("-lang", version);
  }
}

void Xslt30Processor::setTargetEdition(const char *edition) {
  if (edition != nullptr) {
    setProperty("-target", edition);
  }
}

void Xslt30Processor::clearParameters(bool delValues) {
    if (delValues) {
        for (std::map<std::string, XdmValue *>::iterator itr = parameters.begin();
             itr != parameters.end(); itr++) {
            XdmValue *value = itr->second;
            if (delValues && value != nullptr) {
#ifdef DEBUG
                std::cerr << "XQueryProc.clearParameter() - XdmValue refCount="
                  << value->getRefCount() << std::endl;
#endif
                delete value;
                value = nullptr;
            }
        }
    }
    parameters.clear();
}

std::map<std::string, XdmValue *> &Xslt30Processor::getParameters() {
  std::map<std::string, XdmValue *> &ptr = parameters;
  return ptr;
}

void Xslt30Processor::setcwd(const char *dir) {
  if (dir != nullptr) {
    cwdXT = std::string(dir);
  }
}

const char * Xslt30Processor::getcwd() {
  return cwdXT.c_str();
}

SaxonProcessor *Xslt30Processor::getSaxonProcessor() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  bool licensed = j_isLicensed(thread, (void *)procRef);
  int64_t newProcRef = j_copy_handles(thread, (void *)procRef);
  SaxonProcessor * processor = new SaxonProcessor(newProcRef, licensed);
  processor->setcwd(cwdXT.c_str());
  return processor;
}


void Xslt30Processor::useSchema(XsdSchema * schema, const char * role) {
  if (schema != nullptr) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    schemaRef = j_copy_handles(thread, (void *)schema->getUnderlyingSchema());
    setSchemaAware(true);
    if (role != nullptr) {
      setProperty("-schema-role:", role);
    }
  }

}


void Xslt30Processor::setSchemaAware(bool schemaAware) {
  std::map<std::string, std::string>::iterator it;
  it = properties.find("sa:");
  if (it != properties.end()) {
    parameters.erase("sa:");
  }
  if (schemaAware) {
    setProperty("sa:", "true");
  } else {
    setProperty("sa:", "false");
  }
}

bool Xslt30Processor::isSchemaAware() {
  std::map<std::string, std::string>::iterator it;
  it = properties.find("sa:");
  if (it != properties.end()) {
    std::string valuei = it->second;
    if (valuei == "true" || valuei == "on") {
      return true;
    }
  }
  return false;
}

void Xslt30Processor::compileFromXdmNodeAndSave(XdmNode *node,
                                                const char *filename) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int addition = 0;
  if (schemaRef > SXN_UNSET) {
    addition += 1;
  }
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, addition);

  if (schemaRef > SXN_UNSET) {

    std::string packageOptionStr = "-use-schema:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)schemaRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }

  j_compileFromXdmNodeAndSave(thread, (void *)cppXT,
      (void *)node->getUnderlyingValue(), (char *)cwdXT.c_str(),
      (char *)filename,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r);
  }
}

void Xslt30Processor::compileFromStringAndSave(const char *stylesheetStr,
                                               const char *filename,
                                               const char *encoding) {
  if (encoding != nullptr) {
    setProperty("!input-encoding", encoding);
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int addition = 0;
  if (schemaRef > SXN_UNSET) {
    addition += 1;
  }
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, addition);

  if (schemaRef > SXN_UNSET) {

    std::string packageOptionStr = "-use-schema:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)schemaRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }

  j_compileFromStringAndSave(thread, (void *)cppXT, (char *)cwdXT.c_str(),
      (char *)stylesheetStr, (char *)filename,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r);
  }
}

void Xslt30Processor::importPackage(const char *packageFile) {

  if (packageFile == nullptr) {
    throw SaxonApiException("ImportPackage file name is NULL");
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (importPackageValue == SXN_UNSET) {
    importPackageValue =
        createProcessorData(thread);
  }

  saxon_error_t r = SAXON_ERROR_DEFAULT;

  addProcessorProperty(thread,(void *)importPackageValue, (char *)packageFile, &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r);
  }
}

void Xslt30Processor::compileFromFileAndSave(const char *xslFilename,
                                             const char *filename) {
  if (xslFilename == nullptr) {
    throw SaxonApiException("XSL filename is null");
  }
  if (filename == nullptr) {
    throw SaxonApiException("Output filename is null");
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

  int addition = 0;
  if (schemaRef > SXN_UNSET) {
    addition += 1;
  }
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, addition);

  if (schemaRef > SXN_UNSET) {

    std::string packageOptionStr = "-use-schema:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)schemaRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }

  j_compileFromFileAndSave(thread, (void *)cppXT, (char *)cwdXT.c_str(),
      (char *)xslFilename, (char *)filename,
      (processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r);
  }
}

XsltExecutable *Xslt30Processor::compileFromString(const char *stylesheetStr,
                                                   const char *encoding) {
  if (stylesheetStr == nullptr) {
    throw SaxonApiException("XSL string is null");
  }
  if (encoding != nullptr) {
    setProperty("!input-encoding", encoding);
  }
  int addition = 0;
  if (importPackageValue != SXN_UNSET) {
    addition += 1;
  }
  if (schemaRef > SXN_UNSET) {
    addition += 1;
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, addition);

  if (importPackageValue > SXN_UNSET) {

    std::string packageOptionStr = "importPack:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)importPackageValue, &r);
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }

  if (schemaRef > SXN_UNSET) {

    std::string packageOptionStr = "-use-schema:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)schemaRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }

  int64_t objRef = j_compileFromString(thread, (void *)cppXT, (char *)cwdXT.c_str(),
      (char *)stylesheetStr, (int)jitCompilation,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r);
  }
  XsltExecutable *executable =
      new XsltExecutable(procRef, objRef, cwdXT);

  return executable;
}

XsltExecutable *Xslt30Processor::compileFromXdmNode(XdmNode *node) {
  if (node == nullptr) {
    throw SaxonApiException("XSL node is null");
  }

  int addition = 0;
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  if (importPackageValue > SXN_UNSET) {
    addition += 1;
  }
  if (schemaRef > SXN_UNSET) {
    addition += 1;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, addition);

  if (importPackageValue > SXN_UNSET) {

    std::string packageOptionStr = "importPack:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)importPackageValue, &r);
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r);
  }

  if (schemaRef > SXN_UNSET) {

    std::string packageOptionStr = "-use-schema:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)schemaRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }

  int64_t objRef = j_compileFromXdmNode(thread, (void *)cppXT, (char *)cwdXT.c_str(),
      (void *)node->getUnderlyingValue(), (int)jitCompilation,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (r.status != SAXON_OK) {
    //This should be a static error
    throw SaxonApiException(r);
  }
  XsltExecutable *executable =
      new XsltExecutable(procRef, objRef, cwdXT);

  return executable;
}

XsltExecutable *Xslt30Processor::compileFromAssociatedFile(const char *source) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (source == nullptr) {
    throw SaxonApiException("XSL filename is null");
  }

  int addition = 0;
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  if (importPackageValue > SXN_UNSET) {
    addition += 1;
  }
  if (schemaRef > SXN_UNSET) {
    addition += 1;
  }

  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, addition);

  if (importPackageValue > SXN_UNSET) {

    std::string packageOptionStr = "importPack:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)importPackageValue, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }

  if (schemaRef > SXN_UNSET) {

    std::string packageOptionStr = "-use-schema:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)schemaRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }

  int64_t objRef = j_compileFromAssociatedFile(thread, (void *)cppXT, (char *)cwdXT.c_str(),
      (char *)source, (int)jitCompilation,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (r.status != SAXON_OK) {
    //Static error will be thrown
    throw SaxonApiException(r);
  }
  XsltExecutable *executable =
      new XsltExecutable(procRef, objRef, cwdXT);
  return executable;
}

XsltExecutable *Xslt30Processor::compileFromFile(const char *stylesheet) {

  if (stylesheet == nullptr) {
    throw SaxonApiException("XSL filename is null");
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int addition = 0;
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  if (importPackageValue > SXN_UNSET) {
    addition += 1;
  }
  if (schemaRef > SXN_UNSET) {
    addition += 1;
  }

  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, addition);

  if (importPackageValue > SXN_UNSET) {

    std::string packageOptionStr = "importPack:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)importPackageValue, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }

  if (schemaRef > SXN_UNSET) {

    std::string packageOptionStr = "-use-schema:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)schemaRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }

  int64_t objRef = j_compileFromFile(thread, (void *)cppXT, (char *)cwdXT.c_str(),
      (char *)stylesheet, (int)jitCompilation,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r);
  }
  XsltExecutable *executable =
      new XsltExecutable(procRef, objRef, cwdXT);

  return executable;
}

XdmValue *Xslt30Processor::transformFileToValue(const char *sourcefile,
                                                const char *stylesheetfile) {


  if (sourcefile == nullptr && stylesheetfile == nullptr) {
    throw SaxonApiException("Found null file name");
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int addition = 0;
  if (importPackageValue > SXN_UNSET) {
    addition += 1;
  }

  saxon_error_t r = SAXON_ERROR_DEFAULT;

  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, addition);

  if (importPackageValue > SXN_UNSET) {

    std::string packageOptionStr = "importPack:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)importPackageValue, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }

  int64_t result = j_transformToValue(thread, (char *)cwdXT.c_str(), (void *)cppXT,
      (void *)nullptr, (char *)sourcefile, (char *)stylesheetfile,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);
  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (result > 0) {
    return SaxonProcessor::makeXdmValueFromRef(result);
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
  return nullptr;
}

void Xslt30Processor::transformFileToFile(const char *source,
                                          const char *stylesheet,
                                          const char *outputfile) {

  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (stylesheet == nullptr) {
    throw SaxonApiException("Error: stylesheet has not been set.");
  }

  int addition = 0;
  if (importPackageValue > SXN_UNSET) {
    addition += 1;
  }

  saxon_error_t r = SAXON_ERROR_DEFAULT;

  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, addition);

  if (importPackageValue > SXN_UNSET) {

    std::string packageOptionStr = "importPack:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)importPackageValue, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }

  long status = j_transformToFile(thread, (char *)cwdXT.c_str(), (void *)cppXT,
      (void *)nullptr, (char *)source, (char *)stylesheet, (char *)outputfile,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef > 0) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
}

const char *Xslt30Processor::transformFileToString(const char *source,
                                                   const char *stylesheet) {

  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (source == nullptr && stylesheet == nullptr) {
    throw SaxonApiException(
        "Error: nullptr file name found in transformFileToString.");
  }
  allocFn fn = operator new;

  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int addition = 0;
  if (importPackageValue > SXN_UNSET) {
    addition += 1;
  }

  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, addition);

  if (importPackageValue > SXN_UNSET) {

    std::string packageOptionStr = "importPack:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)packageOptionStr.c_str(), (void *)importPackageValue, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }

  char *result = j_transformToString(thread, (void *)fn, (char *)cwdXT.c_str(),
      (void *)cppXT, (void *)nullptr, (char *)source, (char *)stylesheet,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }

  return result;
}