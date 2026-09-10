
//

#include "saxonc/XdmAtomicValue.h"

XdmAtomicValue::XdmAtomicValue() : XdmItem() {}

XdmAtomicValue::XdmAtomicValue(const XdmAtomicValue &aVal) : XdmItem(aVal) {
  valType = aVal.valType;
}

XdmItem *XdmAtomicValue::getHead() { return this; }

XdmAtomicValue::XdmAtomicValue(int64_t obj) : XdmItem(obj) {}

XdmAtomicValue::XdmAtomicValue(int64_t obj, const char *ty) : XdmItem(obj) {
  valType = std::string(ty);
}

XdmAtomicValue::~XdmAtomicValue() {
#ifdef DEBUG
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "~XdmAtomicValue destructor called:" << refCount
              << " ob ref=" << (this) << "value =" << ((long)value)
              << std::endl;
  }
#endif
  if (!valType.empty()) {
      valType.clear();
  }
}

void XdmAtomicValue::setType(const char *ty) { valType = std::string(ty); }

const char *XdmAtomicValue::getPrimitiveTypeName() {
  if (!valType.empty()) {
    return valType.c_str();
  }
  allocFn fn = operator new;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  char *result = j_getPrimitiveTypeName(thread,
                                        (void *)fn, (void *)value);

  if (result != nullptr) {
    valType = std::string(result);
    operator delete(result);
    return valType.c_str();
  }

  return "Q{http://www.w3.org/2001/XMLSchema}anyAtomicType";
}

bool XdmAtomicValue::getBooleanValue() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  char result =
      j_getBooleanValue(thread, (void *)value, &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return result != 0;
}

int XdmAtomicValue::getHashCode() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int result =
      j_getHashCode(thread, (void *)value);
  return result;
}

double XdmAtomicValue::getDoubleValue() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  double result =
      j_getDoubleValue(thread, (void *)value, &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return result;
}

long XdmAtomicValue::getLongValue() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  long result =
        j_getLongValue(thread, (void *)value, &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return result;
}
