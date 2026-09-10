// QName.cpp : Defines the exported functions for the DLL application.
//

#include "saxonc/QNamee.h"
#include "saxonc/XdmNode.h"

#ifdef DEBUG
#include <typeinfo> //used for testing only
#endif

#ifdef MEM_DEBUG
#define new new (__FILE__, __LINE__)
#endif

QName::QName(int64_t obj){
    qnameObject = obj;
}

QName::QName(const char * prefix, const char * uri, const char * localName){

    if(localName == nullptr){
        throw SaxonApiException("Local name cannot be NULL");
    }
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    qnameObj = j_getQName(thread, (char *) uri, (char *) prefix, (char *) localName, &r);

    if(r.status != SXN_EXCEPTION){
        throw SaxonApiException(r.status);
    }

}

QName::QName(const char * uri, const char * lexical){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    qnameObj = j_getQNameFromLexical(thread, (char *) uri, (char *) lexical, &r);

    if(r.status != SXN_EXCEPTION){
        throw SaxonApiException(r.status);
    }
}

QName::QName(const char * localName){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    qnameObj = j_getQNameFromLocal(thread, (char *)localName, &r);

    if(r.status != SXN_EXCEPTION){
        throw SaxonApiException(r.status);
    }
}

QName::QName(const char * lexicalQName, XdmNode * element){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    if(element == nullptr){
        throw SaxonException("Cannot create QName - XdmNode is NULL");
    }
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    qnameObj = j_getQNameFromNode(thread, (char *)lexicalQName, (void *)element->getUnderlyingValue(), &r);
    if(r.status != SXN_EXCEPTION){
        throw SaxonApiException(r.status);
    }
}

QName::QName(const QName &other){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    qnameObj = j_copy_handles(thread, (void *)other.qnameObj);
}

QName::~QName(){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    if (qnameObj != SXN_UNSET) {
        j_handles_destroy(thread,(void *)qnameObj);
    }
}

bool operator QName::==(const QName &other) const{
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    char result = j_isQNameEqual(thread, (void *)qnameObj, (void *)other.underlyingValue());

    return result != 0;

}

int64_t QName::getUnderlyingValue(){
    return qnameObj;
}

QName * QName::fromClarkName(const char * expandedName){
    if(expandedName == nullptr){
        throw SaxonException("Cannot create QName - expanded name is NULL");
    }
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    int64_t qnameObj = j_fromClarkName(thread, (char *)expandedName, &r);

    if(r.status != SXN_EXCEPTION){
        throw SaxonApiException(r.status);
    }

    return new QName(qnameObj);
}

QName * QName::fromEQName(const char * expandedName){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    if(expandedName == nullptr){
        throw SaxonException("Cannot create QName - expanded name is NULL");
    }
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    int64_t qnameObj = j_fromEQName(thread, (char *)expandedName, &r);

    if(r.status != SXN_EXCEPTION){
        throw SaxonApiException(r.status);
    }

    return new QName(qnameObj);
}

bool QName::isValid(){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    char result = j_isQNameValid(thread, (void *)qnameObj);

    return result != 0;

}

const char * QName::getPrefix(){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    allocFn fn = operator new;
    char *result = j_getPrefixFromQName(thread,(void *)fn, (void *)qnameObj);

    if (result != nullptr) {
        std::string resultStr = std::string(result);
        operator delete(result);
        return resultStr.c_str();
    }

    return nullptr;
}

const char * QName::getNamespace(){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    allocFn fn = operator new;
    char *result = j_getNamespaceFromQName(thread,
                                          (void *)fn, (void *)qnameObj);

    if (result != nullptr) {
        std::string resultStr = std::string(result);
        operator delete(result);
        return resultStr.c_str();
    }

    return nullptr;
}

const char * QName::getNamespaceUri(){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    allocFn fn = operator new;
    char *result = j_getNamespaceUriFromQName(thread,
                                          (void *)fn, (void *)qnameObj);

    if (result != nullptr) {
        std::string resultStr = std::string(result);
        operator delete(result);
        return resultStr.c_str();
    }

    return nullptr;
}

const char * QName::getLocalName(){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    allocFn fn = operator new;
    char *result = j_getLocalNameFromQName(thread,
                                          (void *)fn, (void *)qnameObj);

    if (result != nullptr) {
        std::string resultStr = std::string(result);
        operator delete(result);
        return resultStr.c_str();
    }

    return nullptr;
}

const char * QName::getClarkName(){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    allocFn fn = operator new;
    char *result = j_getClarkNameFromQName(thread,
                                          (void *)fn, (void *)qnameObj);

    if (result != nullptr) {
        std::string resultStr = std::string(result);
        operator delete(result);
        return resultStr.c_str();
    }

    return nullptr;
}

const char * QName::getEQName(){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    allocFn fn = operator new;
    char *result = j_getEQNameFromQName(thread,
                                          (void *)fn, (void *)qnameObj);

    if (result != nullptr) {
        std::string resultStr = std::string(result);
        operator delete(result);
        return resultStr.c_str();
    }

    return nullptr;
}

const char * QName::toString(){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    allocFn fn = operator new;
    char *result = j_toStringQName(thread,
                                          (void *)fn, (void *)qnameObj);

    if (result != nullptr) {
        std::string resultStr = std::string(result);
        operator delete(result);
        return resultStr.c_str();
    }

    return nullptr;

}