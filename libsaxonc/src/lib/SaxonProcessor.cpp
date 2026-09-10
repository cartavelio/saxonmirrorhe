#ifndef __linux__
#ifndef __APPLE__
//#include "stdafx.h"
#include <Tchar.h>
#endif
#endif

#include "saxonc/SaxonProcessor.h"
#include "saxonc/XdmArray.h"
#include "saxonc/XdmAtomicValue.h"
#include "saxonc/XdmFunctionItem.h"
#include "saxonc/XdmItem.h"
#include "saxonc/XdmMap.h"
#include "saxonc/XdmNode.h"
#include "saxonc/XdmValue.h"
#include "saxonc/UserFunctionHandler.h"
#include "saxonc/XsdCompiler.h"

//#define DEBUG

#ifdef DEBUG
#include <signal.h>
#endif

#include <stdio.h>


#if defined MEM_DEBUG

void *newImpl(std::size_t sz, char const *file, int line) {
  static int counter{};
  void *ptr = std::malloc(sz);
  std::cerr << "SaxonC Mem test: " << file << ": " << line << " " << ptr
            << std::endl;
  myAlloc.push_back(ptr);
  return ptr;
}

void *operator new(std::size_t sz, char const *file, int line) {
  return newImpl(sz, file, line);
}

void *operator new[](std::size_t sz, char const *file, int line) {
  return newImpl(sz, file, line);
}

void operator delete(void *ptr) noexcept {
  auto ind = std::distance(myAlloc.begin(),
                           std::find(myAlloc.begin(), myAlloc.end(), ptr));
  myAlloc[ind] = nullptr;
  std::free(ptr);
}

void operator delete(void *ptr, std::size_t) noexcept {
  auto ind = std::distance(myAlloc.begin(),
                           std::find(myAlloc.begin(), myAlloc.end(), ptr));
  std::cerr << "SaxonC Mem test deleting: " << ptr << std::endl;
  myAlloc[ind] = nullptr;
  std::free(ptr);
}

#define new new (__FILE__, __LINE__)




void SaxonProcessor::getInfo() {

  std::cout << std::endl;

  std::cout << "Allocation: " << std::endl;
  for (auto i : myAlloc) {
    if (i != nullptr)
      std::cout << " " << i << std::endl;
  }

  std::cout << std::endl;
}

#endif

extern "C" void * c_nativeCall(void *thread, char * functionName, void * proc, void * pythonFunc, void * func, int langFlag, int useNativeTypes, int numOfArguments, void ** arguments) {
  if (getenv("SAXONC_DEBUG_FLAG")) {
    fprintf(stdout, "We are in a c function. calling function name= %s\n", functionName);
  }

  void * userFuncResult = nullptr;

  if (langFlag == SXN_PHP) {
    //PHP logic
    if (getenv("SAXONC_DEBUG_FLAG")) {
      fprintf(stdout, "We are in a c function. calling PHP logic ");
    }
    phpExtensionFunction func1 = phpExtensionFunction(func);

    userFuncResult = func1(functionName, proc, func, useNativeTypes, numOfArguments, arguments);   // call of the user function

    for(int i =0; i< numOfArguments; i++) {
      j_handles_destroy((graal_isolatethread_t *)thread, arguments[i]);
    }

    operator delete(arguments);



  } else if (langFlag == SXN_PYTHON) {
    //Python logic
    if (getenv("SAXONC_DEBUG_FLAG")) {
      fprintf(stdout, "We are in a c function. calling Python logic ");
    }

    pythonExtensionFunction func1 = pythonExtensionFunction(pythonFunc);

    userFuncResult = func1(proc, func, useNativeTypes, numOfArguments, arguments);

  } else {
    if (getenv("SAXONC_DEBUG_FLAG")) {
      fprintf(stdout, "We are in a c function. calling C++ logic ");
    }
    XdmValue ** arguments1 = new XdmValue*[numOfArguments];
    for(int i =0; i< numOfArguments; i++) {
      if (getenv("SAXONC_DEBUG_FLAG")) {
        fprintf(stdout, "Making XdmValue YYYIII ref=%ld.\n", (long)arguments[i]);
      }
      arguments1[i] = SaxonProcessor::makeXdmValueFromRef((int64_t)arguments[i]);
    }
    userFuncResult  = userFunctionNativeCall(functionName, proc, func, useNativeTypes, numOfArguments, arguments1);

    for(int i =0; i< numOfArguments; i++) {

        delete arguments1[i];
    }

    //delete [] arguments; // TODO check where allocation is deleted.
    //arguments = nullptr;
    operator delete(arguments1);
    arguments1 = nullptr;
  }

  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr<<"after user function call - c_nativeCall ref="<<((long)userFuncResult)<<std::endl;
  }

  return userFuncResult;
}

// jobject cpp;
const char *failure;
sxnc_environment *SaxonProcessor::sxn_environ = 0;
int SaxonProcessor::jvmCreatedCPP = 0;

bool SaxonProcessor::exceptionOccurred() {
  return (bool)j_checkForException(sxn_environ->thread);
}

const char *SaxonProcessor::checkException() {
  const char *message = nullptr;
  message = checkForException(sxn_environ);
  return message;
}

/*SaxonApiException *SaxonProcessor::checkAndCreateException() {
    SaxonProcessor::attachCurrentThread();
    if (SaxonProcessor::sxn_environ->env->ExceptionCheck()) {
        SaxonApiException *exception =
checkForExceptionCPP(SaxonProcessor::sxn_environ->env, cppClass, nullptr);
#ifdef DEBUG
        SaxonProcessor::sxn_environ->env->ExceptionDescribe();
#endif
        return exception;
    }
    return nullptr;
}
*/
const char *SaxonProcessor::getErrorMessage() {
  if (exception == nullptr) {
    return nullptr;
  }
  return exception->getMessage();
}

void SaxonProcessor::exceptionClear() {

  if (exception != nullptr) {
    delete exception;
    exception = nullptr;
  }
  j_clearException(sxn_environ->thread);
}

/*SaxonProcessor::SaxonProcessor():  {
    licensei = false;
    SaxonProcessor(false);
}*/

SaxonApiException *SaxonProcessor::checkForExceptionCPP() {
  return nullptr;
}

SaxonProcessor::SaxonProcessor(int64_t proc, bool l) {
  cwd = "";
  licensei = l;
  exception = nullptr;
  procRef = proc;
}

SaxonProcessor::SaxonProcessor() { initialize(false); }

SaxonProcessor::SaxonProcessor(bool l) { initialize(l); }

void SaxonProcessor::initialize(bool l) {
  cwd = "";
  licensei = l;
  exception = nullptr;
  procRef = SXN_UNSET;
  if (SaxonProcessor::jvmCreatedCPP == 0) {
    SaxonProcessor::jvmCreatedCPP = 1;
    SaxonProcessor::sxn_environ =
        new sxnc_environment;

    SaxonProcessor::sxn_environ->isolate = NULL;
	SaxonProcessor::sxn_environ->mainthread = NULL;
	SaxonProcessor::sxn_environ->thread = NULL;


    /*
     * InitializeGraalvm run-time.
     */
    create_graalvm_isolate(SaxonProcessor::sxn_environ);

    nativeFn fn1 = c_nativeCall;
    j_setIndirectFunctionForNativeCall(SaxonProcessor::sxn_environ->thread, (void *)fn1);
#ifdef DEBUG
    setDebugMode(SaxonProcessor::sxn_environ->thread, 1);
#endif
  } else {
#ifdef DEBUG
    std::cerr << "SaxonProc constructor: jvm exists! jvmCreatedCPP="
              << jvmCreatedCPP << std::endl;
#endif
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  procRef =
      createSaxonProcessor2(thread, (int)l);

}

void SaxonProcessor::attachCurrentThread() {
#ifdef DEBUG
  std::cerr << "attachCurrentThread called" << std::endl;
#endif
  int result = attach_graalvm_thread(SaxonProcessor::sxn_environ);
#ifdef DEBUG
  if (result != 0) {
    std::cerr << "attachCurrentThread failed or already in the current thread"
              << std::endl;
  }
#endif
}

void SaxonProcessor::detachCurrentThread() {

#ifdef DEBUG
  std::cerr << "detachCurrentThread called" << std::endl;
  int result =
#endif
  detach_graalvm_thread(SaxonProcessor::sxn_environ);

#ifdef DEBUG
  if (result != 0) {
    std::cerr << "detachCurrentThread failed" << std::endl;
  }
#endif
}

graal_isolatethread_t * SaxonProcessor::getOrAttachCurrentThread(sxnc_environment * sxn) {

  graal_isolatethread_t * thread = get_or_attach_thread(sxn);
  if (thread == nullptr) {
    if (getenv("SAXONC_DEBUG_FLAG")) {
      fprintf(stdout, "getOrAttachCurrentThread error - returned main thread\n");
      fflush(stdout);
    }
    return SaxonProcessor::sxn_environ->mainthread;//throw SaxonApiException("Could not attach to the current thread");
  }
  return thread;

}



void SaxonProcessor::attachCurrentThread(sxnc_environment * sxn) {
#ifdef DEBUG
  std::cerr << "detachCurrentThread called" << std::endl;
  int result =
#endif
  attach_graalvm_thread(sxn);
  if (!sxn->mainthread) {
    sxn->mainthread = SaxonProcessor::sxn_environ->mainthread;
  }
  if (!sxn->thread) {
    sxn->thread = SaxonProcessor::sxn_environ->thread;
  }
#ifdef DEBUG
  if (result != 0) {
    std::cerr << "attachCurrentThread failed or already in the current thread"
              << std::endl;
  }
#endif

}

/**
 * Detach JVM from the current thread
 */
void SaxonProcessor::detachCurrentThread(sxnc_environment * sxn) {
  if (getenv("SAXONC_DEBUG_FLAG")) {
    fprintf(stdout, "detach: Cur thread pointer %p main thread=%p\n", (void *)sxn->thread, (void *)sxn->mainthread);
    fflush(stdout);
  }
  if (((void *)(sxn)->thread) == ((void *)(SaxonProcessor::sxn_environ)->mainthread)) {
    if (getenv("SAXONC_DEBUG_FLAG")) {
      fprintf(stdout, "graal_detach_thread error - already in main thread\n");
      fflush(stdout);
    }
    return;
  }
  if (graal_detach_thread(sxn->thread) != 0) {
    // if (getenv("SAXONC_DEBUG_FLAG")) {
    fprintf(stdout, "graal_detach_thread error\n");
    fflush(stdout);
    //}

  }

}



SaxonProcessor::SaxonProcessor(const char *configFile) {
  cwd = "";

  licensei = true;
  exception = nullptr;
  procRef = SXN_UNSET;
  if (SaxonProcessor::jvmCreatedCPP == 0) {
    SaxonProcessor::jvmCreatedCPP = 1;
    SaxonProcessor::sxn_environ =
        new sxnc_environment; //(sxnc_environment
                              //*)malloc(sizeof(sxnc_environment));

    /*
     * InitializeGraalvm run-time.
     */

    create_graalvm_isolate(SaxonProcessor::sxn_environ);
  } else {
#ifdef DEBUG
    std::cerr << "SaxonProc constructor: jvm exists! jvmCreatedCPP="
              << jvmCreatedCPP << std::endl;
#endif
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  procRef = createSaxonProcessorWithConfigurationFile(thread, (char *)configFile, &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }

#ifdef DEBUG
  setDebugMode(thread, 1);
#endif
}

SaxonProcessor::~SaxonProcessor() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  clearConfigurationProperties();
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "~SaxonProcessor destructor:"
              << " proc ref=" << ((long)procRef) << std::endl;
  }
  if (procRef != SXN_UNSET) {
    if (getenv("SAXONC_DEBUG_FLAG")) {
      std::cerr << "~SaxonProcessor destructor called with destroy:"
                << " ob ref=" << (this) << std::endl;
    }
    // SaxonProcessor::sxn_environ->env->DeleteGlobalRef(proc);
    j_handles_destroy(thread, (void *)procRef);
    procRef = SXN_UNSET;
  }
  if (!versionStr.empty()) {

    versionStr.clear();
  }
}

bool SaxonProcessor::isLicensed() {
  if (!licensei) {
    return false;
  }
  if (procRef < 0) {
      return false;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  return j_isLicensed(thread, (void *)procRef);
}


const char * SaxonProcessor::getSaxonEdition() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  return j_c_getSaxonEdition(thread, (void *)procRef);
}

void SaxonProcessor::registerUserFunctionWithProcessor(void * python_procPtr, const char * ns1, void * func, const char * signature, bool useNativeTypes) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  XdmValue * signatureValue = parseJsonFromString(signature);

  if(signatureValue == nullptr) {
    throw new SaxonApiException("Failed to create function signature from JSON");
  }


  allocFn alloc = operator new;

  if(getenv("SAXONC_DEBUG_FLAG") && useNativeTypes) {
    std::cerr<<"useNativeTypes is True in C++"<<std::endl;
  }

  saxon_error_t r = SAXON_ERROR_DEFAULT;
  long status = j_registerFunctionDefinition(thread,(void *)procRef, (char *) ns1,
                                             (void *)signatureValue->getUnderlyingValue(), python_procPtr, func, (void *) alloc, (int)useNativeTypes, &r);


  if(signatureValue != nullptr) {
    delete signatureValue;
  }
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
}

void SaxonProcessor::registerUserFunction(const char * ns1, XdmValue * (*func)(int, XdmValue **), const char * signature, bool isNativeTypes)
{

    registerUserFunctionWithProcessor(nullptr, ns1, (void *)func,signature, isNativeTypes);

}

bool SaxonProcessor::isSchemaAwareProcessor() {
  if (!licensei) {
    return false;
  } else {

    if (procRef < 0) {
      return false;
    }
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    licensei = j_isSchemaAware(thread, (void *)procRef, &r);
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }
    return licensei;
  }
}

void SaxonProcessor::applyConfigurationProperties() {

 /* if (configProperties.size() > 0) {
    int size = configProperties.size();
    int64_t processorDataRef = createProcessorDataWithCapacity(
        SaxonProcessor::sxn_environ->thread, size);

    int i = 0;
    std::map<std::string, std::string>::iterator iter =
        configProperties.begin();
    for (iter = configProperties.begin(); iter != configProperties.end();
         ++iter, i++) {
      addProcessorPropertyPair(
          SaxonProcessor::sxn_environ->thread, (void *)processorDataRef,
          (char *)(iter->first).c_str(), (char *)(iter->second).c_str());
    }

    int resultsi = j_applyPropertiesToConfiguration(SaxonProcessor::sxn_environ->thread, (void *)procRef,
                           (void *)processorDataRef);



  }*/
}

XdmValue *SaxonProcessor::makeXdmValueFromRef(int64_t results) {
  if (results > 0) {
    if (getenv("SAXONC_DEBUG_FLAG")) {
      std::cerr << "C++ makeXdmValueFromRef - ref="<<((long)results)<< std::endl;
    }
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    XdmValue *value = nullptr;
    int typeRef = j_getXdmObjectType(thread, (void *)results);
//#ifdef DEBUG
    if (getenv("SAXONC_DEBUG_FLAG")) {
      std::cerr << "C++ makeXdmValueFromRef - ref="<<((long)results)<<", typeRef= " << typeRef<< std::endl;
    }
//#endif

    XDM_TYPE xdm_type = static_cast<XDM_TYPE>(typeRef);
    if (xdm_type == XDM_ATOMIC_VALUE) {
      value = new XdmValue();
      value->addXdmItemFromUnderlyingValue(new XdmAtomicValue(results));

    } else if (xdm_type == XDM_NODE) {
      value = new XdmValue();
      value->addXdmItemFromUnderlyingValue(new XdmNode(results));

    } else if (xdm_type == XDM_MAP) {
      value = new XdmValue();
      value->addXdmItemFromUnderlyingValue(new XdmMap(results));

    } else if (xdm_type == XDM_ARRAY) {
      value = new XdmValue();
      value->addXdmItemFromUnderlyingValue(new XdmArray(results));

    } else if (xdm_type == XDM_FUNCTION_ITEM) {
      value = new XdmValue();
      value->addXdmItemFromUnderlyingValue(new XdmFunctionItem(results));
    } else if (typeRef == XDM_EMPTY) {
      // Empty Sequence detected.
      // TODO Maybe we should create an EmptySequence object
      j_handles_destroy(thread, (void *)results);
      results = SXN_UNSET;
      return nullptr;

    } else {
      value = new XdmValue(results, true);
      j_handles_destroy(thread, (void *)results);
      results = SXN_UNSET;
    }

    return value;
  } else if (results == SXN_EXCEPTION) {
    throw SaxonApiException("Internal error in making XdmValue");
  }
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "C++ makeXdmValueFromRef null found"<< std::endl;
  }
  return nullptr;
}

 void SaxonProcessor::destroyHandle(int64_t handleRef){

  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
   j_handles_destroy(thread,(void *)handleRef);
 }

XdmItem *SaxonProcessor::makeXdmItemFromRef(int64_t results) {
  if (results > 0) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    int typeRef = j_getXdmObjectType(thread, (void *)results);
    if (getenv("SAXONC_DEBUG_FLAG")) {
      std::cerr << "C++ makeXdmItemFromRef - ref="<<((long)results)<<", typeRef= " << typeRef<< std::endl;
    }

    XDM_TYPE xdm_type = static_cast<XDM_TYPE>(typeRef);
    XdmItem *xdmItem = nullptr;

    if (xdm_type == XDM_ATOMIC_VALUE) {
      xdmItem = new XdmAtomicValue(results);

    } else if (xdm_type == XDM_NODE) {
      xdmItem = new XdmNode(results);

    } else if (xdm_type == XDM_MAP) {
      xdmItem = new XdmMap(results);

    } else if (xdm_type == XDM_ARRAY) {
      xdmItem = new XdmArray(results);

    } else if (xdm_type == XDM_FUNCTION_ITEM) {
      xdmItem = new XdmFunctionItem(results);
    } else if (xdm_type == XDM_EMPTY) {
      // Empty Sequence detected.
      // TODO Maybe we should create an EmptySequence object
      j_handles_destroy(thread, (void *)results);
      results = SXN_UNSET;
      return nullptr;

    } else {
      throw SaxonApiException(
          "Internal Error: Cannot create XdmItem from reference");
    }

    return xdmItem;
  } else if (results == SXN_EXCEPTION) {
    throw SaxonApiException("Internal error in making XdmItem");
  }

  return nullptr;
}

int64_t SaxonProcessor::createJArray(XdmValue **values, int length) {
  if (length > 0) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    int64_t processorDataRef = createProcessorDataWithCapacity(thread, length);

    for (int i = 0; i < length; i++) {

      addProcessorValue(thread,(void *)processorDataRef, (void *)(values[i])->getUnderlyingValue());
    }

    return processorDataRef;

  } else {
    return SXN_UNSET;
  }
}

int64_t SaxonProcessor::createParameterJArray(
    std::map<std::string, XdmValue *> parameters,
    std::map<std::string, std::string> properties, int additions) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int size = (int) parameters.size() + properties.size() + additions;
  saxon_error_t r = SAXON_ERROR_DEFAULT;
#ifdef DEBUG
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "Properties size: " << properties.size() << std::endl;
    std::cerr << "Parameter size: " << parameters.size() << std::endl;
  }
#endif

  if (size > 0) {
    int64_t processorDataRef = createProcessorDataWithCapacity(thread, size);

    int i = 0;
    for (std::map<std::string, XdmValue *>::iterator iter = parameters.begin();
         iter != parameters.end(); ++iter, i++) {

#ifdef DEBUG
      std::cerr << "map 1" << std::endl;
      std::cerr << "iter->first" << (iter->first).c_str() << std::endl;
#endif

      /*#ifdef DEBUG
                  std::string s1 = typeid(iter->second).name();
                  std::cerr<<"Type of itr:"<<s1<<std::endl;

                  if((iter->second) == nullptr) {std::cerr<<"iter->second is
      nullptr"<<std::endl; } else { std::cerr<<"getting underlying
      value"<<std::endl; int64_t xx = (iter->second)->getUnderlyingValue();

                  if(xx == nullptr) {
                      std::cerr<<"value failed"<<std::endl;
                  } else {

                      std::cerr<<"Type of value:"<<((long)xx)<<std::endl;
                  }
                  if((iter->second)->getUnderlyingValue() == SXN_UNSET) {
                      std::cerr<<"(iter->second)->getUnderlyingValue() is
      nullptr"<<std::endl;
                  }}
      #endif  */

      if ((iter->second) == nullptr) {
        continue;
      }

      addProcessorDataPair(thread,
                           (void *)processorDataRef,
                           (char *)(iter->first).c_str(),
                           ((iter->second)->getUnderlyingValue() == SXN_UNSET ? (void *)nullptr : (void *)(iter->second)->getUnderlyingValue()), &r);

      if (r.status != SAXON_OK) {
        throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
      }
    }

    for (std::map<std::string, std::string>::iterator iter = properties.begin();
         iter != properties.end(); ++iter, i++) {
      addProcessorPropertyPair(thread, (void *)processorDataRef,
          (char *)(iter->first).c_str(), (char *)(iter->second).c_str(), &r);
      if (r.status != SAXON_OK) {
        throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
      }
    }

    return processorDataRef;

  } else {
    return SXN_UNSET;
  }
}

int64_t SaxonProcessor::createParameterJArray2(std::map<std::string, XdmValue *> parameters) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int size = (int) parameters.size();
  int64_t processorDataRef = createProcessorDataWithCapacity(thread, size);

#ifdef DEBUG
  std::cerr << "Parameter size: " << parameters.size() << std::endl;
#endif
  if (size > 0) {

    int i = 0;
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    for (std::map<std::string, XdmValue *>::iterator iter = parameters.begin();
         iter != parameters.end(); ++iter, i++) {

#ifdef DEBUG
      std::cerr << "map 1" << std::endl;
      std::cerr << "iter->first" << (iter->first).c_str() << std::endl;

#endif

      addProcessorDataPair(thread,(void *)processorDataRef,
                           (char *)(iter->first).c_str(),
                           (void *)(iter->second)->getUnderlyingValue(), &r);
      if (r.status != SAXON_OK) {
        throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
      }
    }

    return processorDataRef;

  } else {
    return SXN_UNSET;
  }
}

SaxonProcessor &SaxonProcessor::operator=(const SaxonProcessor &other) {
  procRef = other.procRef;
  cwd = other.cwd;

  parameters = other.parameters;
  //configProperties = other.configProperties;
  licensei = other.licensei;
  exception = other.exception;
  return *this;
}

SaxonProcessor::SaxonProcessor(const SaxonProcessor &other) {
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "SaxonProc: copy constructor called: " << std::endl;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  procRef = j_copy_handles(thread, (void *)other.procRef);
  cwd = other.cwd;

  //configProperties = other.configProperties;
  licensei = other.licensei;
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
}

DocumentBuilder *SaxonProcessor::newDocumentBuilder() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t docBuilderRef = createDocumentBuilder(thread, (void *)procRef);

  DocumentBuilder *builder = new DocumentBuilder(this, docBuilderRef, cwd);

  return builder;
}

Xslt30Processor *SaxonProcessor::newXslt30Processor() {
  return (new Xslt30Processor(this, cwd));
}

XQueryProcessor *SaxonProcessor::newXQueryProcessor() {
  return (new XQueryProcessor(this, cwd));
}

XPathProcessor *SaxonProcessor::newXPathProcessor() {
  return (new XPathProcessor(this, cwd));
}

SchemaValidator *SaxonProcessor::newSchemaValidator() {
  if (licensei) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    return (new SchemaValidator(this, cwd));
  } else {
    std::cerr << "\nError: Processor is not licensed for schema processing!!"
              << std::endl;
    return nullptr;
  }
}

XsdCompiler * SaxonProcessor::newXsdCompiler() {
  if (licensei) {
    return (new XsdCompiler(this, cwd));
  } else {
    std::cerr << "\nError: Processor is not licensed for schema processing!!"
              << std::endl;
    return nullptr;
  }
}


const char *SaxonProcessor::version() {

  if (versionStr.empty()) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    versionStr = std::string(j_c_getProductVersion(thread, (void *)procRef));
  }
  return versionStr.c_str();
}

void SaxonProcessor::setcwd(const char *dir) {
  if (dir != nullptr) {
    cwd = std::string(dir);
  }
}

const char *SaxonProcessor::getcwd() { return cwd.c_str(); }

const char *SaxonProcessor::getResourcesDirectory() {
  return ""; //_getResourceDirectory();
}

void SaxonProcessor::setResourcesDirectory(const char *dir) {
  // memset(&resources_dir[0], 0, sizeof(resources_dir));
#if defined(__linux__) || defined(__APPLE__)
  strncat(_getResourceDirectory(), dir, strlen(dir));
#else
  int destSize = strlen(dir) + strlen(dir);
  strncat_s(_getResourceDirectory(), destSize, dir, strlen(dir));

#endif
}

void SaxonProcessor::setCatalog(const char *catalogFile) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

  if (catalogFile == nullptr) {

    return;
  }

  if (procRef == SXN_UNSET) {
    throw SaxonApiException(
        "Processor is null in SaxonProcessor.setCatalogFiles");
    return;
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t processorDataRef =
      createProcessorDataWithCapacity(thread, 1);

  addProcessorProperty(thread, (void *)processorDataRef,
                       (char *)catalogFile, &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }

  j_setCatalogFiles(thread, (void *)procRef,
                        (char *)cwd.c_str(), (void *)processorDataRef, &r);
  j_handles_destroy(thread, (void *)processorDataRef);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
}

void SaxonProcessor::setCatalogFiles(const char **catalogFiles, int length) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

  if (catalogFiles == nullptr && length > 0) {

    return;
  }

  if (procRef == SXN_UNSET) {
    throw SaxonApiException(
        "Processor is null in SaxonProcessor.setCatalogFiles");
    return;
  }

  int64_t processorDataRef =
      createProcessorDataWithCapacity(thread, length);

  saxon_error_t r = SAXON_ERROR_DEFAULT;

  for (int i = 0; i < length; i++) {

    if (catalogFiles[i] != nullptr) {
      char* catFile = (char *)catalogFiles[i];
      addProcessorProperty(thread, (void *)processorDataRef,
                           catFile, &r);

      if (r.status != SAXON_OK) {
        throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
      }
    }
  }

  long result =
      j_setCatalogFiles(thread, (void *)procRef,
                        (char *)cwd.c_str(), (void *)processorDataRef, &r);
  j_handles_destroy(thread, (void *)processorDataRef);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }

  if (result == SXN_UNSET) {
    throw SaxonApiException("setCatalogFile failed");
  }
}

XdmValue *SaxonProcessor::parseJsonFromString(const char *source,
                                              const char *encoding) {

  if (source == nullptr) {
    throw SaxonApiException("JSON source string is null");
  }

  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "SaxonProc: JSON string: "<< source << std::endl;
  }

  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t xdmValueRef = parseJsonString(thread, (void *)procRef,
                                        (char *)source, (char *)encoding, &r);
  if (xdmValueRef >= 0) {
    XdmValue *value = makeXdmValueFromRef(xdmValueRef);

    return value;
  }
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
  return nullptr;
}

XdmValue *SaxonProcessor::parseJsonFromFile(const char *source) {

  if (source == nullptr) {
    throw SaxonApiException("JSON source file name is null");
  }

  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t xdmValueRef = parseJsonFile(thread, (void *)procRef,
                                      (char *)cwd.c_str(), (char *)source, &r);
  if (xdmValueRef > 0) {
    XdmValue *value = makeXdmValueFromRef(xdmValueRef);

    return value;
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
  return nullptr;
}

XdmNode *SaxonProcessor::parseXmlFromString(const char *source,
                                            const char *encoding,
                                            SchemaValidator *validator) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t xdmNodeRef = parseXmlStringWithValidator(
      thread, (void *)procRef, (char *)encoding,
      (validator != nullptr ? (void *)validator->getUnderlyingValidator()
                            : (void *)NULL),
      (char *)source, &r);
  if (xdmNodeRef > 0) {
    XdmNode *value = new XdmNode(xdmNodeRef);

    return value;
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return nullptr;
}

int SaxonProcessor::getNodeKind(int64_t nodeRef) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int nodeKind = j_getNodeKind(thread, (void *)nodeRef);

  return nodeKind;
}

XdmNode *SaxonProcessor::parseXmlFromFile(const char *filename,
                                          SchemaValidator *validator) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t xdmNodeRef = parseXmlFileWithValidator(
      thread, (char *)cwd.c_str(), (void *)procRef,
      (validator != nullptr ? (void *)validator->getUnderlyingValidator()
                            : (void *)nullptr),
      (char *)filename, &r);
  if (xdmNodeRef > 0) {
    XdmNode *value = nullptr;
    value = new XdmNode(xdmNodeRef);

    return value;
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return nullptr;
}

XdmNode *SaxonProcessor::parseXmlFromUri(const char *uri,
                                         SchemaValidator *validator) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t xdmNodeRef = parseXmlFileWithValidator(
      thread, (char *)cwd.c_str(), (void *)procRef,
      (validator != nullptr ? (void *)validator->getUnderlyingValidator()
                            : (void *)nullptr),
      (char *)uri, &r);
  if (xdmNodeRef >= 0) {
    XdmNode *value = nullptr;
    value = new XdmNode(xdmNodeRef);

    return value;
  }
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return nullptr;
}


// Set a configuration property.
void SaxonProcessor::setConfigurationProperty(const char *name,
                                              const char *value) {
  if (name != nullptr && value != nullptr) {
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    j_applyPropertyToConfiguration(thread, (void *)procRef,
                           (char *)name, (char *)value, &r);

    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }
  }

}

void SaxonProcessor::clearConfigurationProperties() {

}

void SaxonProcessor::createHeapDump(bool live) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  j_createHeapDump(thread, (int)live);
}

void SaxonProcessor::release() {
  if (SaxonProcessor::jvmCreatedCPP != 0) {
    SaxonProcessor::jvmCreatedCPP = 0;
    if (getenv("SAXONC_DEBUG_FLAG")) {
      std::cerr << "SaxonProc: JVM finalized calling !" << std::endl;
      j_printObjectHandlesPoolInfo(sxn_environ->thread);
    }

    //SaxonProcessor::createHeapDump(true);

    // finalizeJavaRT(SaxonProcessor::sxn_environ->jvm);

    // delete SaxonProcessor::sxn_environ;

    if (graal_detach_thread(sxn_environ->thread) != 0) {
      fprintf(stderr, "graal_detach_thread error\n");
      return;
    }

  } else {
    if (getenv("SAXONC_DEBUG_FLAG")) {
      std::cerr << "SaxonProc: JVM finalize not called!" << std::endl;
    }
  }
}

void SaxonProcessor::deleteXdmValueArray(XdmValue **arr, int len) {
  if (arr != nullptr) {
    delete[] arr;
  }
}

void SaxonProcessor::deleteXdmAtomicValueArray(XdmAtomicValue **arr, int len) {
  if (arr != nullptr) {
    delete[] arr;
  }
}

const char * SaxonProcessor::encodeString(const char * stringValue, const char * toCharSetName) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  allocFn fn = operator new;
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  const char * result = j_cpp_encodeString(thread, (char *)stringValue, (char *)toCharSetName, (void *)fn, &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
  return result;
}

/* ========= Factory method for Xdm ======== */

XdmAtomicValue *SaxonProcessor::makeStringValue(const char *str,
                                                const char *encoding) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t stringValueRef =
      j_makeStringValue(thread, (char *)str, (char *)encoding, &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
  XdmAtomicValue *value = new XdmAtomicValue(stringValueRef, "xs:string");
  return value;
}

XdmAtomicValue *SaxonProcessor::makeStringValue(std::string str,
                                                const char *encoding) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t stringValueRef = j_makeStringValue(
      thread, (char *)str.c_str(), (char *)encoding, &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
  XdmAtomicValue *value = new XdmAtomicValue(stringValueRef, "xs:string");
  return value;
}

XdmAtomicValue *SaxonProcessor::makeIntegerValue(int i) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t intValueRef = j_makeIntegerValue(thread, i);
  XdmAtomicValue *value = new XdmAtomicValue(
      intValueRef, "Q{http://www.w3.org/2001/XMLSchema}integer");
  return value;
}

XdmAtomicValue *SaxonProcessor::makeDoubleValue(double d) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t doubleValueRef = j_makeDoubleValue(thread, d);
  XdmAtomicValue *value = new XdmAtomicValue(
      doubleValueRef, "Q{http://www.w3.org/2001/XMLSchema}double");
  return value;
}

XdmAtomicValue *SaxonProcessor::makeFloatValue(float d) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t floatValueRef = j_makeFloatValue(thread, d);
  XdmAtomicValue *value = new XdmAtomicValue(
      floatValueRef, "Q{http://www.w3.org/2001/XMLSchema}float");
  return value;
}

XdmAtomicValue *SaxonProcessor::makeLongValue(long l) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t longValueRef = j_makeLongValue(thread, l);
  XdmAtomicValue *value = new XdmAtomicValue(
      longValueRef, "Q{http://www.w3.org/2001/XMLSchema}long");
  return value;
}

XdmAtomicValue *SaxonProcessor::makeBooleanValue(bool b) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t boolValueRef = j_makeBooleanValue(thread, (int)b);
  XdmAtomicValue *value = new XdmAtomicValue(
      boolValueRef, "Q{http://www.w3.org/2001/XMLSchema}boolean");
  return value;
}

XdmAtomicValue *SaxonProcessor::makeQNameValue(const char *str) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t qnameValueRef = j_makeQNameValue(thread, (char *)str);
  XdmAtomicValue *value = new XdmAtomicValue(qnameValueRef, "QName");
  return value;
}

XdmAtomicValue *SaxonProcessor::makeAtomicValue(const char *typei,
                                                const char *strValue) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if(typei == nullptr || strValue == nullptr) {
    return nullptr;
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t obj = j_makeAtomicValue(thread, (char *)typei, (char *)strValue, &r);
  if(obj == -1) {
    return nullptr;
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
  XdmAtomicValue *value = new XdmAtomicValue(obj, typei);
  return value;
}

const char *SaxonProcessor::clarkNameToEQName(const char *name) {
  allocFn fn = operator new;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  const char *result = j_clarkNameToEQName(thread,
                                           (void *)fn, (char *)name, &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
  return result;
}

const char *SaxonProcessor::EQNameToClarkName(const char *name) {
  allocFn fn = operator new;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  const char *result = j_EQNameToClarkName(thread,(void *)fn, (char *)name, &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }

  return result;
}

const char *SaxonProcessor::getStringValue(XdmItem *item) {
  allocFn fn = operator new;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  const char *result =
      j_cpp_getStringValue(thread, (void *)item->getUnderlyingValue(), (void *)fn);
#ifdef DEBUG
  if (result == nullptr) {
    std::cerr << "getStringValue of XdmItem is nullptr" << std::endl;
  } else {
    std::cerr << "getStringValue of XdmItem is OK" << std::endl;
  }
#endif

  return result;
}


XdmArray *SaxonProcessor::makeArray(short *input, int length) {
  if (input == nullptr) {
    /*std::cerr << "Error found when converting pointer array of short values to "
                 "XdmArray"; */
    return nullptr;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t xdmArrayRef =
      j_makeXdmArrayFromShort(thread, input, length);

  XdmArray *newArray = new XdmArray(xdmArrayRef, length);
  return newArray;
}

XdmArray *SaxonProcessor::makeArray(XdmValue **input, int length) {
  int64_t processorDataRef = SXN_UNSET;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (length == 0) {
    processorDataRef = createProcessorData(thread);
  } else {
    processorDataRef =
        createProcessorDataWithCapacity(thread, length);
  }

  for (int i = 0; i < length; i++) {
    if (input[i] == nullptr || input[i]->getUnderlyingValue() <= SXN_UNSET) {

      return nullptr;
    }
    addProcessorValue(thread, (void *)processorDataRef,
                      (void *)input[i]->getUnderlyingValue());
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;

  int64_t xdmArrayRef =
      j_makeXdmArray(thread, (void *)processorDataRef, &r);

  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr<<"DEBUG makeArray ref= "<<((long)xdmArrayRef)<<std::endl;
  }

  j_handles_destroy(thread, (void *)processorDataRef);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }

  auto *newArray = new XdmArray(xdmArrayRef, length);
  return newArray;
}

void * SaxonProcessor::makeInternalArray(void ** inputs, int length) {
  SaxonProcessor::attachCurrentThread();
  int64_t processorDataRef = SXN_UNSET;

  if (length == 0) {
    processorDataRef = createProcessorData(sxn_environ->thread);

  } else {
    processorDataRef =
        createProcessorDataWithCapacity(sxn_environ->thread, length);
  }

  for (int i = 0; i < length; i++) {
    addProcessorValue(sxn_environ->thread, (void *)processorDataRef,
                      inputs[i]);
  }

  saxon_error_t r = SAXON_ERROR_DEFAULT;

  int64_t xdmArrayRef =
      j_makeXdmArray(sxn_environ->thread, (void *)processorDataRef, &r);

  j_handles_destroy(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef);

  if (xdmArrayRef <= 0) {
    /*std::cerr
        << "Error found when converting array of XdmValue to XdmArray - Ref="
        << ((long)xdmArrayRef) << std::endl;*/
    return nullptr;
  }

  return (void *)(xdmArrayRef);
}


XdmArray *SaxonProcessor::makeArray(int *input, int length) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (length < 0) {
    throw SaxonApiException("Error found when converting pointer array of int values to XdmArray");
  }
  int64_t xdmArrayRef =
      j_makeXdmArrayFromInt(thread, input, length);

  auto *newArray = new XdmArray(xdmArrayRef, length);
  return newArray;
}

XdmArray *SaxonProcessor::makeArray(long long *input, int length) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (length < 0) {
    throw SaxonApiException("Error found when converting pointer array of long values to XdmArray");
  }
  int64_t xdmArrayRef =
      j_makeXdmArrayFromLong(thread, input, length);

  XdmArray *newArray = new XdmArray(xdmArrayRef, length);
  return newArray;
}

XdmArray *SaxonProcessor::makeArray(bool *input, int length) {

  if (input == nullptr || length < 0) {
    throw SaxonApiException("Error found when converting pointer array of bool values to XdmArray");
  }
  int *inputAsInts = new int[length];

  for (int i = 0; i < length; i++) {
    inputAsInts[i] = (int)input[i];
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

  int64_t xdmArrayRef =
      j_makeXdmArrayFromBool(thread, inputAsInts, length);
  XdmArray *newArray = new XdmArray(xdmArrayRef, length);
  return newArray;
}

XdmArray *SaxonProcessor::makeArray(char **input, int length) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if(length < 0) {
    throw SaxonApiException("Error creating XdmArray - length < 0");
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t  xdmArrayRef = j_makeXdmArrayFromStrings(thread,
    input, length, &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }

  auto * newArray = new XdmArray(xdmArrayRef, length);
  return newArray;
}

XdmMap *
SaxonProcessor::makeMap(std::map<XdmAtomicValue *, XdmValue *> dataMap) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t mapDataRef = SXN_UNSET;

  mapDataRef =
      j_create_mapDataWithCapacity(thread, (int) dataMap.size());

  for (std::map<XdmAtomicValue *, XdmValue *>::iterator iter = dataMap.begin();
       iter != dataMap.end(); ++iter) {
    if ((iter->first)->getUnderlyingValue() <= SXN_UNSET ||
        (iter->second)->getUnderlyingValue() <= SXN_UNSET) {
        throw SaxonApiException("Error found when converting array of XdmValue to XdmArray");
    }
    j_addMapPair(thread, (void *)mapDataRef,
                 (void *)(iter->first)->getUnderlyingValue(),
                 (void *)(iter->second)->getUnderlyingValue());
  }

  int64_t xdmmapRef = j_makeXdmMap(thread, (void *)mapDataRef);

  j_handles_destroy(thread, (void *)mapDataRef);
  XdmMap *newArray = new XdmMap(xdmmapRef);
  return newArray;
}

XdmMap *SaxonProcessor::makeMap2(std::map<std::string, XdmValue *> dataMap) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t mapDataRef = SXN_UNSET;

  mapDataRef =
      j_create_mapDataWithCapacity(thread, (int) dataMap.size());

  saxon_error_t r = SAXON_ERROR_DEFAULT;

  for (std::map<std::string, XdmValue *>::iterator iter = dataMap.begin();
       iter != dataMap.end(); ++iter) {
    if ((iter->first).empty() ||
        (iter->second)->getUnderlyingValue() <= SXN_UNSET) {
        throw SaxonApiException("Error found when converting array of XdmValue to XdmArray");
    }

    int64_t stringValueRef = j_makeStringValue(
        thread, (char *)(iter->first).c_str(), (char *)nullptr, &r);
    XdmAtomicValue *stringValue =
        new XdmAtomicValue(stringValueRef, "xs:string");
    j_addMapPair(thread, (void *)mapDataRef,
                 (void *)stringValue->getUnderlyingValue(),
                 (void *)(iter->second)->getUnderlyingValue());
    delete stringValue;
    if (r.status != SAXON_OK) {
      break;
    }
  }

  int64_t xdmmapRef = j_makeXdmMap(thread, (void *)mapDataRef);

  j_handles_destroy(thread, (void *)mapDataRef);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }



  XdmMap *newArray = new XdmMap(xdmmapRef);
  return newArray;
}

XdmMap *SaxonProcessor::makeMap3(XdmAtomicValue **keys, XdmValue **values,
                                 int size) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t mapDataRef = SXN_UNSET;

  mapDataRef = j_create_mapDataWithCapacity(thread, size);

  for (int i = 0; i < size; i++) {
    if (keys[i]->getUnderlyingValue() <= SXN_UNSET ||
        values[i]->getUnderlyingValue() <= SXN_UNSET) {
      throw SaxonApiException("Error found when converting array of XdmValue to XdmArray");
    }
    j_addMapPair(thread, (void *)mapDataRef,
                 (void *)keys[i]->getUnderlyingValue(),
                 (void *)values[i]->getUnderlyingValue());
  }

  int64_t xdmmapRef = j_makeXdmMap(thread, (void *)mapDataRef);
  
  j_handles_destroy(thread, (void *)mapDataRef);

  if (xdmmapRef == SXN_EXCEPTION) {
    throw SaxonApiException("Error found when converting std:map of pair (XdmAtomicValue, XdmValue)");
  }

  auto *newArray = new XdmMap(xdmmapRef);
  return newArray;
}

