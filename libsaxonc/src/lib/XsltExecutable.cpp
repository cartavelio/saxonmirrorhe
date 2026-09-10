// XsltExecutable.cpp : Defines the exported functions for the DLL application.
//

#include "saxonc/XsltExecutable.h"
#include "saxonc/XdmArray.h"
#include "saxonc/XdmAtomicValue.h"
#include "saxonc/XdmFunctionItem.h"
#include "saxonc/XdmItem.h"
#include "saxonc/XdmMap.h"
#include "saxonc/XdmNode.h"
#include "saxonc/XdmValue.h"

#ifdef DEBUG
#include <typeinfo> //used for testing only
#endif

#ifdef MEM_DEBUG
#define new new (__FILE__, __LINE__)
#endif

XsltExecutable::XsltExecutable(int64_t procRefi, int64_t exRef,
                               std::string curr) {

#ifdef DEBUG

#endif
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  rawResultsFlag = false;
  selectionRef = SXN_UNSET;
  saxonMessageHandlerRef = SXN_UNSET;
  saxonResultDocRef = SXN_UNSET;
  executableRef = exRef;
  procRef = j_copy_handles(thread, (void *)procRefi);
  cwdXE = curr;
  selectionStringFlag = false;
}

XsltExecutable::~XsltExecutable() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

  clearProperties();
  clearParameters();

  if (saxonMessageHandlerRef != SXN_UNSET) {
    j_handles_destroy(thread,
                      (void *)saxonMessageHandlerRef);
    saxonMessageHandlerRef = SXN_UNSET;
  }
  if (saxonResultDocRef != SXN_UNSET) {
    j_handles_destroy(thread,
                      (void *)saxonResultDocRef);
    saxonResultDocRef = SXN_UNSET;
  }
  if (executableRef != SXN_UNSET) {
    j_handles_destroy(thread,
                      (void *)executableRef);
    executableRef = SXN_UNSET;
  }
  if (procRef != SXN_UNSET) {
    j_handles_destroy(thread,
                      (void *)procRef);
    procRef = SXN_UNSET;
  }

}

XsltExecutable::XsltExecutable(const XsltExecutable &other) {
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "XsltExecutable. copy constructor" << std::endl;
  }

  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

  executableRef = j_copy_handles(thread, (void *)other.executableRef);
  if(executableRef < SXN_UNSET) {
    throw SaxonApiException("Failed to copy XsltExecutable");
  }

  procRef = j_copy_handles(thread, (void *)other.procRef);
  cwdXE = other.cwdXE;
  selectionRef = other.selectionRef;
  rawResultsFlag = other.rawResultsFlag;
  saxonMessageHandlerRef = SXN_UNSET;
  selectionStringFlag = other.selectionStringFlag;
  saxonResultDocRef = SXN_UNSET;

  std::map<std::string, XdmValue *>::const_iterator paramIter =
      other.parameters.begin();
  while (paramIter != other.parameters.end()) {

    XdmValue *valuei = paramIter->second;
    if (valuei == nullptr) {
      /*throw SaxonApiException("Error in XsltExecutable copy constructor -
         parameter value " "is NULL");*/
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


SaxonProcessor* XsltExecutable::getSaxonProcessor() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  bool licensed = j_isLicensed(thread, (void *)procRef);
  int64_t newProcRef = j_copy_handles(thread, (void *)procRef);
  SaxonProcessor * processor = new SaxonProcessor(newProcRef, licensed);
  processor->setcwd(cwdXE.c_str());
  return processor;
}

void XsltExecutable::deleteXdmValueArray(XdmValue **arr, int len) {
  if (arr != nullptr && len > 0) {
    delete[] arr;
  }
}

void XsltExecutable::setGlobalContextItem(XdmItem *value) {
  if (value != nullptr) {
    const char * ifile = getProperty("node");
    if(ifile != nullptr) {
      properties.erase("s");
    }
    setParameter("node", (XdmValue*)value, false);
  }
}

void XsltExecutable::setGlobalContextFromFile(const char *ifile) {
  if (ifile != nullptr) {
    XdmValue * valuei = getParameter("node", false);
    if(valuei != nullptr) {
      parameters.erase("node");
    }
    setProperty("s", ifile);
  }
}

void XsltExecutable::setInitialMatchSelection(XdmValue *_selection) {
    if (_selection == nullptr) {
        selectionRef = SXN_UNSET;
    } else {
        setParameter("_i_matches", _selection, false);
        selectionStringFlag = false;
    }
}

void XsltExecutable::setInitialMatchSelectionAsFile(const char *filename) {
    if (!selectionStringFlag) {
        XdmValue * valuei = getParameter("_i_matches", false);
            if(valuei != nullptr) {
              parameters.erase("_i_matches");
            }
    }

    selectionStringFlag = true;

    setProperty("_i_file_matches", filename);

}

void XsltExecutable::setOutputFile(const char *ofile) {
  setProperty("o", ofile);
}

void XsltExecutable::setBaseOutputURI(const char *baseURI) {
  if (baseURI != nullptr) {
    setProperty("baseoutput", baseURI);
  }
}

void XsltExecutable::setInitialMode(const char *modeName) {
  if (modeName != nullptr) {
    setProperty("im", modeName);
  }
}

void XsltExecutable::setParameter(const char *name, XdmValue *value) {
  if (value != nullptr && name != nullptr) {
    if (getenv("SAXONC_DEBUG_FLAG")) {
      std::cerr << "XsltExecutable.setParameter() - XdmValue ref="
        << ((long)value->getUnderlyingValue()) << std::endl;
    }
    std::string skey = ("param:" + std::string(name));
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

void XsltExecutable::setParameter(const char *name, XdmValue *value,
                                  bool withParam) {
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

void XsltExecutable::setInitialTemplateParameters(
    std::map<std::string, XdmValue *> _itparameters, bool _tunnel) {
  for (std::map<std::string, XdmValue *>::iterator itr = _itparameters.begin();
       itr != _itparameters.end(); itr++) {

    //parameters["itparam:" + std::string(itr->first)] = itr->second;
    std::string paramStr = (_tunnel ?"tnparam:":"itparam:") + std::string(itr->first);
    setParameter(paramStr.c_str(), itr->second, false);
  }
}

void XsltExecutable::setCaptureResultDocuments(bool flag, bool rawResults) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (flag) {
    resultDocumentMap.clear();
    if (saxonResultDocRef != SXN_UNSET) {
      j_handles_destroy(thread,
                  (void *)saxonResultDocRef);
      saxonResultDocRef = SXN_UNSET;
    }

    rawResultsFlag = rawResults || rawResultsFlag;

    saxonResultDocRef = createSaxonCResultDocumentHandler(
        thread, (int)rawResultsFlag);

  } else {
    setProperty("rd", "off");

    if (saxonResultDocRef > 0) {
      j_handles_destroy(thread,(void *)saxonResultDocRef);
      saxonResultDocRef = SXN_UNSET;
    }

    while (!resultDocumentMap.empty()) {
      delete resultDocumentMap.begin()->second;
      resultDocumentMap.erase(resultDocumentMap.begin());
    }
  }
}

std::map<std::string, XdmValue *> &XsltExecutable::getResultDocuments() {
  
  if (saxonResultDocRef > 0) {
    allocFn fn = operator new;
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    long long *results =
        j_getResultDocuments(thread, (void *)fn,
                             (void *)saxonResultDocRef);
    int64_t resultUrisRef = j_getResultDocumentURIs(thread, (void *)saxonResultDocRef);
    if (results != nullptr) {
      int sizex = (int) results[0];
      // std::cerr<<"getResultDocuments cp0 size= "<<sizex<<std::endl;
      for (int i = 1; i <= sizex; i++) {
        XdmValue *value = nullptr;
        char *str = j_getProcessorDataProperty(thread, (void *)fn,
            (void *)resultUrisRef, i - 1);

        value = SaxonProcessor::makeXdmValueFromRef(results[i]);
        if (value != nullptr && str != nullptr) {
          std::string docUriString = std::string(str);
          resultDocumentMap[docUriString] = value;
          operator delete(str);
        } else {
          if (resultUrisRef != SXN_UNSET) {
            j_handles_destroy(thread,
                  (void *)resultUrisRef);
          }
          throw SaxonApiException("getResultDocuments failed to create value");
        }
      }
      operator delete(results);
      if (resultUrisRef != SXN_UNSET) {
        j_handles_destroy(thread,
                          (void *)resultUrisRef);
      }
    }
  }

  return resultDocumentMap;
}

XdmValue *XsltExecutable::getParameter(const char *name, bool withParam) {
  std::map<std::string, XdmValue *>::iterator it;
  it = parameters.find((withParam ? "param:" : "") + std::string(name));
  if (it != parameters.end())
    return it->second;
  else {
    it = parameters.find((withParam ? "sparam:" : "")  + std::string(name));
    if (it != parameters.end())
      return it->second;
  }
  return nullptr;
}

bool XsltExecutable::removeParameter(const char *name) {
  return (bool)(parameters.erase("param:" + std::string(name)));
}

void XsltExecutable::setResultAsRawValue(bool option) {
  if (option) {
    setProperty("outvalue", "yes");
    rawResultsFlag = true;
  } else {
    removeProperty("outvalue");
    rawResultsFlag = false;
  }
}

XsltExecutable *XsltExecutable::clone() {
  XsltExecutable *executable = new XsltExecutable(*this);
  return executable;
}

void XsltExecutable::setProperty(const char *name, const char *value) {
#ifdef DEBUG
  if (value == nullptr) {
    std::cerr << "XsltExecutable setProperty is nullptr" << std::endl;
  }
#endif
  if (name != nullptr) {
    properties.erase(name);
    std::string skey = std::string(name);
    properties[skey] = std::string((value == nullptr ? "" : value));
  }
}

bool XsltExecutable::removeProperty(const char *name) {
  return (bool)(properties.erase(std::string(name)));
}

const char *XsltExecutable::getProperty(const char *name) {
  std::map<std::string, std::string>::iterator it;
  it = properties.find(std::string(name));
  if (it != properties.end())
    return it->second.c_str();
  return nullptr;
}


void XsltExecutable::clearParameters(bool delValues) {
  
    if (delValues) {
        for (std::map<std::string, XdmValue *>::iterator itr = parameters.begin();
             itr != parameters.end(); itr++) {
            XdmValue *value = itr->second;
            if (delValues && value != nullptr) {
                if (getenv("SAXONC_DEBUG_FLAG")) {
                    std::cerr << "XsltExecutable.clearParameter() - XdmValue refCount="
                      << value->getRefCount() << std::endl;
                }
                delete value;
                value = nullptr;
            }
        }
    }
    parameters.clear();
}

void XsltExecutable::clearProperties() {
  
  properties.clear();
  /*if (selectionRef != SXN_UNSET && selectionStringFlag) {
    j_handles_destroy(thread,
                      (void *)selectionRef);
    selectionRef = SXN_UNSET;
  } */
}

std::map<std::string, XdmValue *> &XsltExecutable::getParameters() {
  std::map<std::string, XdmValue *> &ptr = parameters;
  return ptr;
}

std::map<std::string, std::string> &XsltExecutable::getProperties() {
  std::map<std::string, std::string> &ptr = properties;
  return ptr;
}

void XsltExecutable::setcwd(const char *dir) {
  if (dir != nullptr) {
    cwdXE = std::string(dir);
  }
}

const char * XsltExecutable::getcwd() {
  return cwdXE.c_str();
}

void XsltExecutable::exportStylesheet(const char *filename) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  long status =
      j_save(thread, (char *)cwdXE.c_str(),
             (void *)executableRef, (char *)filename, &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
}

void XsltExecutable::applyTemplatesReturningFile(const char *output_filename) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);


  int additions = 0;
  if (saxonMessageHandlerRef != SXN_UNSET) {
    additions++;
  }
  if (saxonResultDocRef != SXN_UNSET) {
    additions++;
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  /*if(saxonMessageListenerRef != SXN_UNSET ) {
      additions++;
  }
  if(saxonResultDocRef != SXN_UNSET) {
      additions++;
  }*/
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, additions);

  std::string optionStr;
  // TODO change map to set values as int64_t then we can add
  // saxonMessageHandlerRef directly
  if (saxonMessageHandlerRef != SXN_UNSET) {
    optionStr = "m";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonMessageHandlerRef, &r);
  }
  if (saxonResultDocRef > 0) {
    optionStr = "rd";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonResultDocRef, &r);
  }

  long status = j_applyTemplatesReturningFile(
      thread, (char *)cwdXE.c_str(),
      (void *)executableRef,
      (void *)(selectionRef == SXN_UNSET ? (void *)nullptr
                                         : (void *)selectionRef),
      (char *)output_filename,
      (void *)(processorDataRef == SXN_UNSET ? (void *)nullptr
                                             : (void *)processorDataRef), &r);

  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }


}

const char *XsltExecutable::applyTemplatesReturningString() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);//get_or_attach_thread(SaxonProcessor::sxn_environ);


  int additions = 0;

  if (saxonMessageHandlerRef != SXN_UNSET) {
    additions++;
  }
  if (saxonResultDocRef != SXN_UNSET) {
    additions++;
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, additions);
  std::string optionStr;
  if (saxonMessageHandlerRef != SXN_UNSET ) {
    optionStr = "m";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonMessageHandlerRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }
  if (saxonResultDocRef > 0) {
    optionStr = "rd";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonResultDocRef, &r);
  }
  allocFn fn = operator new;

  char *result = j_applyTemplatesReturningString(
      thread, (void *)fn, (char *)cwdXE.c_str(),
      (void *)executableRef,
      (selectionRef == SXN_UNSET ? (void *)NULL : (void *)selectionRef),
      (processorDataRef == SXN_UNSET ? (void *)NULL
                                     : (void *)processorDataRef), &r);

  if (processorDataRef != SXN_UNSET) {
    
    j_handles_destroy(thread,
                      (void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }

  return result;
}

XdmValue *XsltExecutable::applyTemplatesReturningValue() {
  


  int additions = 0;

  if(saxonMessageHandlerRef != SXN_UNSET ) {
      additions++;
  }
  if(saxonResultDocRef != SXN_UNSET) {
      additions++;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, additions);

  // TODO change map to set values as int64_t then we can add
  // saxonMessageHandlerRef directly
  std::string optionStr;
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  if (saxonMessageHandlerRef != SXN_UNSET) {

    optionStr = "m";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonMessageHandlerRef, &r);
    if (r.status != SAXON_OK) {
      
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }
  }
  if (saxonResultDocRef > 0) {
    optionStr = "rd";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonResultDocRef, &r);
    if (r.status != SAXON_OK) {
      
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }
  }

  int64_t result = j_applyTemplatesReturningValue(
      thread, (char *)cwdXE.c_str(),
      (void *)executableRef,
      (void *)(selectionRef == SXN_UNSET ? (void *)NULL : (void *)selectionRef),
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

void XsltExecutable::callFunctionReturningFile(const char *functionName,
                                               XdmValue **arguments,
                                               int argument_length,
                                               const char *outfile) {

  


  int additions = 0;
  if(saxonMessageHandlerRef != SXN_UNSET ) {
      additions++;
  }
  if(saxonResultDocRef > 0) {
      additions++;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, additions);

  std::string optionStr;
  // TODO change map to set values as int64_t then we can add
  // saxonMessageHandlerRef directly
  if (saxonMessageHandlerRef != SXN_UNSET) {
    optionStr = "m";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonMessageHandlerRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }
  }
  if (saxonResultDocRef > 0) {
    optionStr = "rd";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonResultDocRef, &r);
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }
  }

  int64_t argumentValuesRef = createProcessorDataWithCapacity(thread, argument_length);

  for (int i = 0; i < argument_length; i++) {
    addProcessorValue(thread,
                      (void *)argumentValuesRef,
                      (void *)arguments[i]->getUnderlyingValue());
  }

  long status = j_callFunctionReturningFile(
      thread, (char *)cwdXE.c_str(),
      (void *)executableRef, (char *)functionName, (char *)outfile,
      (argumentValuesRef == SXN_UNSET ? (void *)NULL
                                      : (void *)argumentValuesRef),
      (processorDataRef == SXN_UNSET ? (void *)NULL
                                     : (void *)processorDataRef), &r);
  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (argumentValuesRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)argumentValuesRef);
    argumentValuesRef = SXN_UNSET;
  }

  if (status == SXN_EXCEPTION) {
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }
  }
}

const char *XsltExecutable::callFunctionReturningString(
    const char *functionName, XdmValue **arguments, int argument_length) {

  


  int additions = 0;

  if (saxonMessageHandlerRef != SXN_UNSET) {
    additions++;
  }

  if (saxonResultDocRef != SXN_UNSET) {
    additions++;
  }

  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, additions);

  std::string optionStr;
  // TODO change map to set values as int64_t then we can add
  // saxonMessageHandlerRef directly
  if (saxonMessageHandlerRef != SXN_UNSET) {
    optionStr = "m";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonMessageHandlerRef, &r);
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }
  if (saxonResultDocRef != SXN_UNSET) {
    optionStr = "rd";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonResultDocRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }

  int64_t argumentValuesRef = createProcessorDataWithCapacity(thread, argument_length);

  for (int i = 0; i < argument_length; i++) {
    addProcessorValue(thread,
                      (void *)argumentValuesRef,
                      (void *)arguments[i]->getUnderlyingValue());
  }
  allocFn fn = operator new;
  char *result = j_callFunctionReturningString(
      thread, (void *)fn, (char *)cwdXE.c_str(),
      (void *)executableRef, (char *)functionName,
      (void *)(argumentValuesRef == SXN_UNSET ? (void *)nullptr
                                              : (void *)argumentValuesRef),
      (void *)(processorDataRef == SXN_UNSET ? (void *)nullptr
                                             : (void *)processorDataRef), &r);
  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (argumentValuesRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)argumentValuesRef);
    argumentValuesRef = SXN_UNSET;
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }

  return result;
}

XdmValue *XsltExecutable::callFunctionReturningValue(const char *functionName,
                                                     XdmValue **arguments,
                                                     int argument_length) {

  


  int additions = 0;

  if(saxonMessageHandlerRef != SXN_UNSET ) {
      additions++;
  }
  if(saxonResultDocRef > 0) {
      additions++;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, additions);

  std::string optionStr;
  // TODO change map to set values as int64_t then we can add
  // saxonMessageHandlerRef directly
  if (saxonMessageHandlerRef != SXN_UNSET) {
    optionStr = "m";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonMessageHandlerRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }
  if (saxonResultDocRef > 0) {
    optionStr = "rd";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonResultDocRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r);
    }
  }

  int64_t argumentValuesRef = createProcessorDataWithCapacity(
      thread, argument_length);

  for (int i = 0; i < argument_length; i++) {
    addProcessorValue(thread,
                      (void *)argumentValuesRef,
                      (void *)arguments[i]->getUnderlyingValue());
  }

  int64_t result = j_callFunctionReturningValue(
      thread, (char *)cwdXE.c_str(),
      (void *)executableRef, (char *)functionName, (void *)argumentValuesRef,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);
  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (argumentValuesRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)argumentValuesRef);
    argumentValuesRef = SXN_UNSET;
  }

  if (result > 0) {
    return SaxonProcessor::makeXdmValueFromRef(result);
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r);
  }

  return nullptr;
}

void XsltExecutable::callTemplateReturningFile(const char *templateName,
                                               const char *outfile) {

  int additions = 0;

  if (saxonMessageHandlerRef != SXN_UNSET) {
    additions++;
  }
  if (saxonResultDocRef > 0) {
    additions++;
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, additions);

  // TODO change map to set values as int64_t then we can add
  // saxonMessageHandlerRef directly
  std::string optionStr;
  if (saxonMessageHandlerRef != SXN_UNSET) {
    optionStr = "m";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonMessageHandlerRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }
  if (saxonResultDocRef > 0) {
    optionStr = "rd";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonResultDocRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }

  long status = j_callTemplateReturningFile(
      thread, (char *)cwdXE.c_str(),
      (void *)executableRef, (char *)templateName, (char *)outfile,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);
  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
}

const char *
XsltExecutable::callTemplateReturningString(const char *templateName) {
  


  int additions = 0;
  if (saxonMessageHandlerRef != SXN_UNSET) {
    additions++;
  }
  if (saxonResultDocRef != SXN_UNSET) {
    additions++;
  }

  saxon_error_t r = SAXON_ERROR_DEFAULT;
  std::string optionStr;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, additions);

  // TODO change map to set values as int64_t then we can add
  // saxonMessageHandlerRef directly
  if (saxonMessageHandlerRef != SXN_UNSET) {
    optionStr = "m";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonMessageHandlerRef, &r);
  }
  if (saxonResultDocRef > 0) {
    optionStr = "rd";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonResultDocRef, &r);
  }
  allocFn fn = operator new;
  char *result = j_callTemplateReturningString(
      thread, (void *)fn, (char *)cwdXE.c_str(),
      (void *)executableRef, (char *)templateName,
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

XdmValue *XsltExecutable::callTemplateReturningValue(const char *templateName) {
  


  int additions = 0;

  if(saxonMessageHandlerRef != SXN_UNSET ) {
      additions++;
  }
  if(saxonResultDocRef > 0) {
      additions++;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, additions);

  saxon_error_t r = SAXON_ERROR_DEFAULT;

  std::string optionStr;
  // TODO change map to set values as int64_t then we can add
  // saxonMessageHandlerRef directly
  if (saxonMessageHandlerRef != SXN_UNSET) {
    optionStr = "m";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonMessageHandlerRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }
  if (saxonResultDocRef > 0) {
    optionStr = "rd";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonResultDocRef, &r);
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }

  int64_t result = j_callTemplateReturningValue(
      thread, (char *)cwdXE.c_str(),
      (void *)executableRef, (char *)templateName,
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

XdmValue *XsltExecutable::transformFileToValue(const char *sourcefile) {




  int additions = 0;

  if(saxonMessageHandlerRef != SXN_UNSET ) {
      additions++;
  }
  if(saxonResultDocRef > 0) {
      additions++;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, additions);

  std::string optionStr;
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  // TODO change map to set values as int64_t then we can add
  // saxonMessageHandlerRef directly
  if (saxonMessageHandlerRef != SXN_UNSET) {
    optionStr = "m";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonMessageHandlerRef, &r);
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }
  if (saxonResultDocRef > 0) {
    optionStr = "rd";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonResultDocRef, &r);
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }
    if(selectionStringFlag == true && selectionRef != SXN_UNSET) {
        optionStr = "s";
        addProcessorDataPair(thread,
                             (char *)processorDataRef, (char *)optionStr.c_str(),
                             (void *)selectionRef, &r);

      if (r.status != SAXON_OK) {
        throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
      }
    }
  char *cstylesheet = nullptr;
  int64_t result = j_transformToValue(
      thread, (char *)cwdXE.c_str(),
      (void *)nullptr, (void *)executableRef, (char *)sourcefile, cstylesheet,
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

void XsltExecutable::transformFileToFile(const char *source,
                                         const char *outputfile) {



  int additions = 0;

  if(saxonMessageHandlerRef != SXN_UNSET ) {
      additions++;
  }
  if(saxonResultDocRef > 0) {
      additions++;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, additions);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  std::string optionStr;
  // TODO change map to set values as int64_t then we can add
  // saxonMessageHandlerRef directly
  if (saxonMessageHandlerRef != SXN_UNSET) {
    optionStr = "m";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonMessageHandlerRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }
  if (saxonResultDocRef > 0) {
    optionStr = "rd";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonResultDocRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }
    if(selectionStringFlag == true && selectionRef != SXN_UNSET) {
        optionStr = "s";
        addProcessorDataPair(thread,
                             (char *)processorDataRef, (char *)optionStr.c_str(),
                             (void *)selectionRef, &r);

      if (r.status != SAXON_OK) {
        throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
      }
    }
  char *cstylesheet = nullptr;
  j_transformToFile(
      thread, (char *)cwdXE.c_str(),
      (void *)nullptr, (void *)executableRef, (char *)source, cstylesheet,
      (char *)outputfile,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef),&r);
  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
}

void XsltExecutable::setSaveXslMessage(bool create, const char *filename) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  if (create) {
    if (filename == nullptr) {
      if (saxonMessageHandlerRef != SXN_UNSET) {
        j_handles_destroy(thread,
                  (void *)saxonMessageHandlerRef);
        saxonMessageHandlerRef = SXN_UNSET;
      }
      saxonMessageHandlerRef = j_xsltExecutable_messageHandler_store(thread, &r);
    } else {
      saxonMessageHandlerRef = j_xsltExecutable_messageHandler_file(thread, (void *)procRef,
          (char *)cwdXE.c_str(), (char *)filename, &r);
    }
  } else {
    if (saxonMessageHandlerRef != SXN_UNSET) {
      j_handles_destroy(thread,
                        (void *)saxonMessageHandlerRef);
    }
    saxonMessageHandlerRef = j_xsltExecutable_messageHandler_null(thread, &r);;
  }

  if (r.status != SAXON_OK) {

    saxonMessageHandlerRef = SXN_UNSET;
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number, r.num_of_errors);
  }
}

XdmValue *XsltExecutable::getXslMessages() {
  if (saxonMessageHandlerRef != SXN_UNSET) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    allocFn fn = operator new;
    long long *results =
        j_getXslMessages(thread, (void *)fn,
                         (void *)saxonMessageHandlerRef);

    if (results != nullptr) {
      int sizex = (int) results[0];

      XdmValue *value = new XdmValue();
      for (int i = 1; i <= sizex; i++) {
        XdmItem *item = SaxonProcessor::makeXdmItemFromRef(results[i]);
        if (item != nullptr) {
          value->addXdmItemFromUnderlyingValue(item);
        } else {
          throw SaxonApiException("getXslMessages failed to create value");
        }
      }
      operator delete(results);
      return value;
    }
  }
  return nullptr;
}


void XsltExecutable::clearXslMessages() {
  if (saxonMessageHandlerRef != SXN_UNSET) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        j_clearXslMessages(thread,
                         (void *)saxonMessageHandlerRef);

  }
}

const char *XsltExecutable::transformFileToString(const char *source) {



  int additions = 0;

  if(saxonMessageHandlerRef != SXN_UNSET ) {
      additions++;
  }
  if(saxonResultDocRef > 0) {
      additions++;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t processorDataRef =
      SaxonProcessor::createParameterJArray(parameters, properties, additions);

  // TODO change map to set values as int64_t then we can add
  // saxonMessageHandlerRef directly
  std::string optionStr;
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  if (saxonMessageHandlerRef > 0) {
    optionStr = "m";
    addProcessorDataPair(thread,
                         (void *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonMessageHandlerRef, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }
  if (saxonResultDocRef > 0) {
    optionStr = "rd";
    addProcessorDataPair(thread,
                         (char *)processorDataRef, (char *)optionStr.c_str(),
                         (void *)saxonResultDocRef, &r);
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }
  if(selectionStringFlag == true && selectionRef != SXN_UNSET) {
      optionStr = "s";
      addProcessorDataPair(thread,
                           (char *)processorDataRef, (char *)optionStr.c_str(),
                           (void *)selectionRef, &r);
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
    }
  }

  char *cstylesheet = nullptr;
  allocFn fn = operator new;

  char *result = j_transformToString(
      thread, (void *)fn, (char *)cwdXE.c_str(),
      (void *)nullptr, (void *)executableRef, (char *)source, cstylesheet,
      (void *)(processorDataRef == SXN_UNSET ? (void *)NULL
                                             : (void *)processorDataRef), &r);

  if (processorDataRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)processorDataRef);
    processorDataRef = SXN_UNSET;
  }
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r);
  }

  return result;
}

const char *XsltExecutable::transformToString(XdmNode *source) {

  if (source != nullptr) {
      setParameter("node", (XdmValue *)source, false);
  }
  return transformFileToString(nullptr);
}

XdmValue *XsltExecutable::transformToValue(XdmNode *source) {

  if (source != nullptr) {
    setParameter("node", (XdmValue *)source, false);
  }
  return transformFileToValue(nullptr);
}

void XsltExecutable::transformToFile(XdmNode *source) {

  if (source != nullptr) {
      setParameter("node", (XdmValue *)source, false);
  }
  transformFileToFile(nullptr, nullptr);
}
