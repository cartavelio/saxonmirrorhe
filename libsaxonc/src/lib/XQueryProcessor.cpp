#include "saxonc/XQueryProcessor.h"

#include <saxonc/XsdSchema.h>

#include "saxonc/XdmArray.h"
#include "saxonc/XdmAtomicValue.h"
#include "saxonc/XdmFunctionItem.h"
#include "saxonc/XdmItem.h"
#include "saxonc/XdmMap.h"
#include "saxonc/XdmNode.h"
#include "saxonc/XdmValue.h"

#ifdef MEM_DEBUG
#define new new (__FILE__, __LINE__)
#endif

XQueryProcessor::XQueryProcessor() {
  SaxonProcessor *p = new SaxonProcessor(false);
  XQueryProcessor(p, "");
}

XQueryProcessor::~XQueryProcessor() {
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "~XQueryProcessor destructor called: ob ref=" << (this)  << std::endl;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  clearProperties();
  clearParameters();
  if (cppXQ != SXN_UNSET) {
    j_handles_destroy(thread, (void *)cppXQ);
    cppXQ = SXN_UNSET;
  }
  cwdXQ.erase();
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

SaxonProcessor* XQueryProcessor::getSaxonProcessor() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  bool licensed = j_isLicensed(thread, (void *)procRef);
  int64_t newProcRef = j_copy_handles(thread, (void *)procRef);
  SaxonProcessor * processor = new SaxonProcessor(newProcRef, licensed);
  processor->setcwd(cwdXQ.c_str());
  return processor;
}

XQueryProcessor::XQueryProcessor(SaxonProcessor *p, std::string curr) {
  if (p == nullptr) {
    throw SaxonApiException("Processor object is null");
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  procRef = j_copy_handles(thread, (void *)p->procRef);
  schemaRef = SXN_UNSET;
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  cppXQ = j_createXQueryEngineWithProcessor(thread,(void *)procRef, &r);
  if (r.status != SAXON_OK) {
    j_handles_destroy(thread, (void *)procRef);
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }

  if (!(p->cwd.empty()) && curr.empty()) {
    cwdXQ = p->cwd;
  } else {
    cwdXQ = curr;
  }
}

XQueryProcessor::XQueryProcessor(const XQueryProcessor &other) {
  cwdXQ = other.cwdXQ;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  procRef = j_copy_handles(thread, (void *)other.procRef);
  cppXQ = j_copy_handles(thread, (void *)other.cppXQ);
  if (schemaRef != SXN_UNSET) {
    schemaRef = j_copy_handles(thread, (void *)other.schemaRef);
  } else {
    schemaRef = other.schemaRef;
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

XQueryProcessor *XQueryProcessor::clone() {
  return new XQueryProcessor(*this);
}

std::map<std::string, XdmValue *> &XQueryProcessor::getParameters() {
  std::map<std::string, XdmValue *> &ptr = parameters;
  return ptr;
}

std::map<std::string, std::string> &XQueryProcessor::getProperties() {
  std::map<std::string, std::string> &ptr = properties;
  return ptr;
}

// Set the source document for the query
void XQueryProcessor::setContextItem(XdmItem *value) {
    if (value == nullptr) {
        std::map<std::string, XdmValue *>::iterator it;
        it = parameters.find("node");
        if (it != parameters.end()) {
            XdmValue *valuei = it->second;
            if (valuei != nullptr) {
                parameters.erase("node");
            }
        }
    } else {
        setParameter("node", value, false);
    }
}

void XQueryProcessor::declareNamespace(const char *prefix, const char *uri) {
  if (prefix == nullptr || uri == nullptr) {
    return;
  } else {
    // setProperty("ns-prefix", uri);
    size_t s = properties.size();
    std::string skey = std::string("ns-prefix:") + prefix;
    properties.insert(
        std::pair<std::string, std::string>(skey, std::string(uri)));

    if (s == properties.size()) {
      std::map<std::string, std::string>::iterator it;
      it = properties.find(skey);
      if (it != properties.end()) {
        properties.erase(skey);
        properties[skey] = std::string(uri);
      }
    }
  }
}

// Set the source document for the query
void XQueryProcessor::setContextItemFromFile(const char *ifile) {
  setProperty("s", ifile);
}

// Set the output file where the result is sent
void XQueryProcessor::setOutputFile(const char *ofile) {
  setProperty("o", ofile);
}

XdmValue *XQueryProcessor::getParameter(const char *name, bool withParam) {
  std::map<std::string, XdmValue *>::iterator it;
  it = parameters.find((withParam ? "param:" : "") + std::string(name));
  if (it != parameters.end())
    return it->second;

  return nullptr;
}

// Set a parameter value used in the query
void XQueryProcessor::setParameter(const char *name, XdmValue *value, bool withParam) {
  if (value != nullptr && name != nullptr) {
    std::string skey = (withParam ? "param:" : "") + std::string(name);
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

// Remove a parameter (name, value) pair
bool XQueryProcessor::removeParameter(const char *name) {
  return (bool)(parameters.erase("param:" + std::string(name)));
}

const char * XQueryProcessor::getProperty(const char *name) {
  std::map<std::string, std::string>::iterator it;
  it = properties.find(std::string(name));
  if (it != properties.end())
    return it->second.c_str();
  return nullptr;
}

// Set a property.
void XQueryProcessor::setProperty(const char *name, const char *value) {
#ifdef DEBUG
  if (value == nullptr) {
    std::cerr << "XQueryProc setProperty is nullptr" << std::endl;
  }
#endif
  if (name != nullptr) {
    properties.erase(name);
    std::string skey = std::string(name);
    properties[skey] = std::string((value == nullptr ? "" : value));
  }
}


void XQueryProcessor::clearParameters(bool delValues) {
    if (delValues ) {
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


void XQueryProcessor::useSchema(XsdSchema * schema) {
  if (schema != nullptr) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    schemaRef = j_copy_handles(thread, (void *)schema->getUnderlyingSchema());
    setSchemaAware(true);
  }
}

void XQueryProcessor::setSchemaAware(bool schemaAware) {
  std::map<std::string, std::string>::iterator it;
  it = properties.find("sa:");
  if (it != properties.end()) {
    parameters.erase("sa:");
  }
  if (schemaAware) {
    setProperty("sa:", "true");
  } else {
    setProperty("sa:", "false");
    if (schemaRef != SXN_UNSET) {
      graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
      j_handles_destroy(thread,(void *)schemaRef);
      schemaRef = SXN_UNSET;
    }
  }
}

bool XQueryProcessor::isSchemaAware() {
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

void XQueryProcessor::clearProperties() {
  properties.clear();
  // outputfile1.clear();
}

void XQueryProcessor::setcwd(const char *dir) {
  if (dir != nullptr) {
    cwdXQ = std::string(dir);
  }
}

const char * XQueryProcessor::getcwd() {
  return cwdXQ.c_str();
}

void XQueryProcessor::setQueryBaseURI(const char *baseURI) {
  setProperty("base", baseURI);
}

const char * XQueryProcessor::getQueryBaseURI() {
  return getProperty("base");
}

void XQueryProcessor::setUpdating(bool updating) {
  if (updating) {
    setProperty("updating", "on");
  } else {
    setProperty("updating", "off");
  }
}

void XQueryProcessor::executeQueryToFile(const char *infilename,
                                         const char *ofilename,
                                         const char *query,
                                         const char *encoding) {
  if (infilename != nullptr) {
    setProperty("s", infilename);
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;

  int addition = 0;
  if (schemaRef > SXN_UNSET) {
    addition += 1;
  }

  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, addition);

  if (schemaRef != SXN_UNSET) {
    std::string schemaOptionStr = "-use-schema:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)schemaOptionStr.c_str(), (void *)schemaRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }


  j_executeQueryToFile(thread, (void *)procRef,
      (char *)cwdXQ.c_str(), (char *)query, (char *)ofilename, (char *)encoding,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef > 0) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r);
  }

}

XdmValue *XQueryProcessor::executeQueryToValue(const char *infilename,
                                               const char *query,
                                               const char *encoding) {
  if (infilename != nullptr) {
    setProperty("s", infilename);
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;

  int addition = 0;
  if (schemaRef > SXN_UNSET) {
    addition += 1;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, addition);

  if (schemaRef > SXN_UNSET) {

    std::string schemaOptionStr = "-use-schema:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)schemaOptionStr.c_str(), (void *)schemaRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }

  int64_t result = j_executeQueryToValue(thread, (void *)procRef,
      (char *)cwdXQ.c_str(), (char *)query, (char *)encoding,
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
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }

  return nullptr;
}

const char *XQueryProcessor::executeQueryToString(const char *infilename,
                                                  const char *query,
                                                  const char *encoding) {
  if (infilename != nullptr) {
    setProperty("s", infilename);
  }
  allocFn fn = operator new;
  saxon_error_t r = SAXON_ERROR_DEFAULT;

  int addition = 0;
  if (schemaRef > SXN_UNSET) {
    addition += 1;
  }

  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, addition);


  if (schemaRef != SXN_UNSET) {
    std::string schemaOptionStr = "-use-schema:";
    addProcessorDataPair(thread, (void *)processorDataRef,
        (char *)schemaOptionStr.c_str(), (void *)schemaRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }

  char *result = j_executeQueryToString(thread, (void *)fn, (void *)procRef,
      (char *)cwdXQ.c_str(), (char *)query, (char *)encoding,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef > 0) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return result;
}

const char *XQueryProcessor::runQueryToString() {
  return executeQueryToString(nullptr, nullptr);
}

XdmValue *XQueryProcessor::runQueryToValue() {
  return executeQueryToValue(nullptr, nullptr);
}

void XQueryProcessor::runQueryToFile() {
  executeQueryToFile(nullptr, nullptr, nullptr);
}

void XQueryProcessor::setQueryFile(const char *ofile) {
  // outputfile1 = std::string(ofile);
  setProperty("q", ofile);
}

void XQueryProcessor::setStreaming(bool option) {
  streaming = option;
  if (option) {
    setProperty("-streaming", "1");
  } else {
    setProperty("-streaming", "0");
  }
}

bool XQueryProcessor::isStreaming() const { return streaming; }

void XQueryProcessor::setLanguageVersion(const char *version) {
  setProperty("-lang-version", version);
}

void XQueryProcessor::setQueryContent(const char *content) {
  // outputfile1 = std::string(content);
  setProperty("qs", content);
}
