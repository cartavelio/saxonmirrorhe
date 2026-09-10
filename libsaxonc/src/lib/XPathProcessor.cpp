#include "saxonc/XPathProcessor.h"
#include "saxonc/XdmArray.h"
#include "saxonc/XdmAtomicValue.h"
#include "saxonc/XdmFunctionItem.h"
#include "saxonc/XdmItem.h"
#include "saxonc/XdmMap.h"
#include "saxonc/XdmNode.h"
#include "saxonc/XdmValue.h"

#include <fstream>
#include <iostream>

#ifdef MEM_DEBUG
#define new new (__FILE__, __LINE__)
#endif

XPathProcessor::XPathProcessor() {
    SaxonProcessor *p = new SaxonProcessor(false);
    XPathProcessor(p, "");
}

XPathProcessor::XPathProcessor(SaxonProcessor *p, std::string curr) {
    xp_baseURI = nullptr;
    unprefixedElementPolicy = UnprefixedElementMatchingPolicy::DEFAULT_NAMESPACE;
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

    procRef = j_copy_handles(thread, (void *)p->procRef);

    cppXP = createXPathProcessorWithProcessor(thread, (void *) p->procRef);

    if (cppXP < SXN_UNSET) {
        throw SaxonApiException(
                "Error: Failed to create the XPathProcessor internal object");

    }

#ifdef DEBUG
    /*jmethodID debugMID =
    SaxonProcessor::sxn_environ->env->GetStaticMethodID(cppClass, "setDebugMode",
    "(Z)V"); SaxonProcessor::sxn_environ->env->CallStaticVoidMethod(cppClass,
    debugMID, (jboolean)true);*/
#endif

    if (!(p->cwd.empty()) && curr.empty()) {
        cwdXP = p->cwd;
    } else {
        cwdXP = curr;
    }
}

XPathProcessor::XPathProcessor(const XPathProcessor &other) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    cwdXP = other.cwdXP;
    procRef = j_copy_handles(thread, (void *) other.procRef);
    cppXP = j_copy_handles(thread, (void *)other.cppXP);
    xp_baseURI = other.xp_baseURI;
    unprefixedElementPolicy = other.unprefixedElementPolicy;
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

XPathProcessor::~XPathProcessor() {
    clearProperties();
    clearParameters();
    if (xp_baseURI != nullptr) {
        operator delete(xp_baseURI);
    }
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    if (cppXP != SXN_UNSET) {
        j_handles_destroy(thread, (void *) cppXP);
        cppXP = SXN_UNSET;
    }

    if (procRef != SXN_UNSET) {
        j_handles_destroy(thread,
                          (void *)procRef);
        procRef = SXN_UNSET;
    }
}

XdmValue *XPathProcessor::evaluate(const char *xpathStr, const char *encoding) {
    if (xpathStr == nullptr) {
        throw SaxonApiException("Error:: XPath string cannot be empty or nullptr");
    }
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    int64_t processorDataRef =
            SaxonProcessor::createParameterJArray(parameters, properties, 0);
    int64_t results = j_evaluate(thread, (void *) cppXP, (char *) cwdXP.c_str(),
            (char *) xpathStr, (char *) encoding,
            (void *) (processorDataRef == SXN_UNSET ? (void *) NULL
                                                    : (void *) processorDataRef), &r);
    if (processorDataRef > 0) {
        j_handles_destroy(thread,
                          (void *) processorDataRef);
        processorDataRef = SXN_UNSET;
    }
    if (results > 0) {
        return SaxonProcessor::makeXdmValueFromRef(results);
    } else if (r.status != SAXON_OK) {
        throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }

    return nullptr;
}

XdmItem *XPathProcessor::evaluateSingle(const char *xpathStr,
                                        const char *encoding) {

    if (xpathStr == nullptr) {
        throw SaxonApiException("Error:: XPath string cannot be empty or nullptr");
    }

    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

    saxon_error_t r = SAXON_ERROR_DEFAULT;
    int64_t processorDataRef =
            SaxonProcessor::createParameterJArray(parameters, properties);
    int64_t result = j_evaluateSingle(thread, (void *) cppXP, (char *) cwdXP.c_str(),
            (char *) xpathStr, (char *) encoding,
            (void *) (processorDataRef == SXN_UNSET ? (void *) NULL
                                                    : (void *) processorDataRef), &r);
    if (processorDataRef > 0) {
        j_handles_destroy(thread,(void *) processorDataRef);
        processorDataRef = SXN_UNSET;
    }
    if (result > 0) {
        int typeRef =
                j_getXdmObjectType(thread, (void *) result);
        XdmItem *xdmItem = nullptr;

        if (typeRef == XDM_ATOMIC_VALUE) {
            xdmItem = new XdmAtomicValue(result);
            return xdmItem;

        } else if (typeRef == XDM_NODE) {
            xdmItem = new XdmNode(result);
            return xdmItem;

        } else if (typeRef == XDM_MAP) {
            xdmItem = new XdmMap(result);
            return xdmItem;

        } else if (typeRef == XDM_ARRAY) {
            xdmItem = new XdmArray(result);
            return xdmItem;

        } else if (typeRef == XDM_FUNCTION_ITEM) {
            xdmItem = new XdmFunctionItem(result);
            return xdmItem;
        }
        xdmItem = new XdmItem(result);
        return xdmItem;
    } else
        if (r.status != SAXON_OK) {
            throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
        }

    return nullptr;
}

void XPathProcessor::setContextItem(XdmItem *item) {
    if (item == nullptr) {
        std::map<std::string, XdmValue *>::iterator it;
        it = parameters.find("node");
        if (it != parameters.end()) {
            XdmValue *valuei = it->second;
            if (valuei != nullptr) {
                parameters.erase("node");
            }
        }
    } else {
        setParameter("node", item, false);
    }
}

void XPathProcessor::setContextFile(const char *filename) {

    if (filename != nullptr) {
        setProperty("s", filename);
    } else {
        properties.erase("s");
    }
}

void XPathProcessor::setUnprefixedElementMatchingPolicy(
        UnprefixedElementMatchingPolicy policy) {
    std::map<std::string, std::string>::iterator it;
    it = properties.find("uemp:");
    if (it != properties.end()) {
        std::string valuei = it->second;
        parameters.erase("uemp:");
    }
    setProperty("uemp:", std::to_string(int(policy)).c_str());
}

UnprefixedElementMatchingPolicy
XPathProcessor::getUnprefixedElementMatchingPolicy() {
    std::map<std::string, std::string>::iterator it;
    it = properties.find("uemp:");
    if (it != properties.end()) {
        int valuei = stoi(it->second);
        return UnprefixedElementMatchingPolicy(valuei);
    }
    return UnprefixedElementMatchingPolicy::DEFAULT_NAMESPACE;
}

void XPathProcessor::declareNamespace(const char *prefix, const char *uri) {

    if (prefix == nullptr) {
        throw SaxonApiException("Null prefix supplied to declareNamespace()");
    }
    if (uri == nullptr) {
        throw SaxonApiException(
                "Null namespace URI supplied to declareNamespace()");
    }
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    j_xp_declareNamespace(thread, (void *) cppXP,
                          (char *) prefix, (char *) uri, &r);
}

void XPathProcessor::declareVariable(const char *name) {
    if (name == nullptr) {
        return;
    }
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    j_xp_declareVariable(thread, (void *) cppXP, (char *) name, &r);
}

void XPathProcessor::setBackwardsCompatible(bool option) {
    if (option) {
        setProperty("backwardsCom:", "true");
    } else {
        properties.erase("backwardsCom:");
    }
}

void XPathProcessor::setLanguageVersion(const char *version) {
    if (version) {
        setProperty("lang:", version);
    } else {
        properties.erase("lang:");
    }
}

void XPathProcessor::setCaching(bool caching) {
    if (caching) {
        setProperty("caching:", "true");
    } else {
        properties.erase("caching:");
    }
}

void XPathProcessor::importSchemaNamespace(const char *uri) {
    if (uri != nullptr) {
        setProperty("importSN", uri);
    }
}

void XPathProcessor::setBaseURI(const char *uriStr) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    int64_t status = j_xp_setBaseURI(thread, (void *) cppXP,(char *) uriStr, &r);
    if (xp_baseURI != nullptr) {
        operator delete(xp_baseURI);
        xp_baseURI = nullptr;
    }

    if (status == SXN_EXCEPTION) {
        throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }
}

const char *XPathProcessor::getBaseURI() {
    if (xp_baseURI == nullptr) {
        allocFn fn = operator new;
        graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
        xp_baseURI = j_xp_getBaseURI(thread,(void *) cppXP, (void *) fn);
    }
    return (const char *) xp_baseURI;
}

bool XPathProcessor::effectiveBooleanValue(const char *xpathStr,
                                           const char *encoding) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    int64_t processorDataRef =
            SaxonProcessor::createParameterJArray(parameters, properties);
    int resultsi = j_effectiveBooleanValue(thread, (void *) cppXP, (char *) cwdXP.c_str(),
            (char *) xpathStr, (char *) encoding,
            (void *) (processorDataRef == SXN_UNSET ? (void *) NULL
                                                    : (void *) processorDataRef), &r);
    if (processorDataRef > 0) {
        j_handles_destroy(thread,(void *) processorDataRef);
        processorDataRef = SXN_UNSET;
    }
    if (r.status != SAXON_OK) {
        throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }

    return (bool) resultsi;
}


XdmValue * XPathProcessor::getParameter(const char *name, bool withParam) {
  std::map<std::string, XdmValue *>::iterator it;
  it = parameters.find((withParam ? "param:" : "") + std::string(name));
  if (it != parameters.end())
    return it->second;

  return nullptr;
}

void XPathProcessor::setParameter(const char *name, XdmValue *value,
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


bool XPathProcessor::removeParameter(const char *name) {
    return (bool) (parameters.erase("param:" + std::string(name)));
}

const char * XPathProcessor::getProperty(const char *name) {
    std::map<std::string, std::string>::iterator it;
    it = properties.find(std::string(name));
    if (it != properties.end())
        return it->second.c_str();
    return nullptr;
}

void XPathProcessor::setProperty(const char *name, const char *value) {
    if (name != nullptr) {
        size_t s = properties.size();
        std::string skey = std::string(name);
        properties.insert(std::pair<std::string, std::string>(
                skey, std::string((value == nullptr ? "" : value))));

        if (s == properties.size()) {
            std::map<std::string, std::string>::iterator it;
            it = properties.find(skey);
            if (it != properties.end()) {
                properties.erase(skey);
                properties[skey] = std::string((value == nullptr ? "" : value));
            }
        }
    }
}


void XPathProcessor::clearParameters(bool delValues) {
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

void XPathProcessor::clearProperties() { properties.clear(); }

void XPathProcessor::setcwd(const char *dir) {
    if(dir != nullptr) {
        cwdXP = std::string(dir);
    }
}


const char * XPathProcessor::getcwd() {
    return cwdXP.c_str();
}


std::map<std::string, XdmValue *> &XPathProcessor::getParameters() {
    std::map < std::string, XdmValue * > &ptr = parameters;
    return ptr;
}

std::map <std::string, std::string> &XPathProcessor::getProperties() {
    std::map <std::string, std::string> &ptr = properties;
    return ptr;
}

SaxonProcessor* XPathProcessor::getSaxonProcessor() {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    bool licensed = j_isLicensed(thread, (void *)procRef);
    int64_t newProcRef = j_copy_handles(thread, (void *)procRef);
    SaxonProcessor * processor = new SaxonProcessor(newProcRef, licensed);
    processor->setcwd(cwdXP.c_str());
    return processor;
}
