#include "saxonc/XdmArray.h"

#include "saxonc/XdmMap.h"

XdmArray::XdmArray() : XdmFunctionItem(), arrayLen(0) {}

//XdmArray::XdmArray(bool): XdmFunctionItem(), arrayLen(0) {}

XdmArray::XdmArray(const XdmArray &d) : XdmFunctionItem(d) {
  arrayLen = d.arrayLen;
}

// TODO arrayLen should be set upfront
XdmArray::XdmArray(int64_t obj) : XdmFunctionItem(obj), arrayLen(-1) {}

XdmArray::XdmArray(int64_t obj, int aLen)
    : XdmFunctionItem(obj), arrayLen(aLen) {}


int XdmArray::arrayLength() {
  if (value == SXN_UNSET) {
    return 0;
  }
  if (arrayLen == -1) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    arrayLen = j_xdmArray_arrayLength(thread,(void *)value);
  }
  return arrayLen;
}

XdmValue *XdmArray::get(int n) {
  if (n < 0 || value == SXN_UNSET) {
    throw SaxonApiException("Array index out of bounds.");
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t result =
      j_xdmArray_get(SaxonProcessor::sxn_environ->thread, (void *)value, n, &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return SaxonProcessor::makeXdmValueFromRef(result);
}

const char *XdmArray::toString(const char *encoding) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  if (value == SXN_UNSET) {
    value = j_makeXdmArray(thread, (void *) NULL, &r);
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }
  }
  allocFn fn = operator new;

  const char *str = j_xdmArray_toString(SaxonProcessor::sxn_environ->thread,
                                        (void *)fn, (void *)value);
  if(str == nullptr) {
    throw SaxonApiException("Array object is null");
  }

  return str;
}

const char *XdmArray::getStringValue(const char *encoding) {
  throw SaxonApiException("An array has no string value", "FOTY0014", nullptr,
                          -1);
}

XdmArray *XdmArray::put(int n, XdmValue *valuei) {
  if (n < 0) {
    throw SaxonApiException("XdmArray::put: Negative number supplied for the arguments");
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (value == SXN_UNSET) {
    value = j_makeXdmArray(thread, (void *) NULL, &r);
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }
  }

  int64_t result =
      j_xdmArray_put(thread, (void *)value, n,
                     (void *)valuei->getUnderlyingValue(), &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  auto *newValue = new XdmArray(result);
  return newValue;
}

XdmArray *XdmArray::addMember(XdmValue *valuei) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  if (value == SXN_UNSET) {
    value = j_makeXdmArray(thread, (void *) NULL, &r);
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }
  }
  int64_t result =
      j_xdmArray_addMember(thread, (void *)value,
                           (void *)valuei->getUnderlyingValue(), &r);
  if(result == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
  auto *newValue = new XdmArray(result);
  return newValue;
}

XdmArray *XdmArray::concat(XdmArray *valuei) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  if (value == SXN_UNSET) {
    value = j_makeXdmArray(thread, (void *) NULL, &r);
    if (r.status != SAXON_OK) {
      throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
    }
  }
  int64_t result =
      j_xdmArray_concat(thread, (void *)value,
                        (void *)valuei->getUnderlyingValue(), &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  auto *newValue = new XdmArray(result);
  return newValue;
}

std::list<XdmValue *> XdmArray::asList() {
  std::list<XdmValue *> arr;
  int sizex = arrayLength();
  if (sizex == 0 || value == SXN_UNSET) {
    return arr;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  allocFn fn = operator new;
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  long long *results = j_convertXdmArrayToArrayObject(thread, (void *)value, (void *)fn, &r);

  if (results) {

    XdmValue *tempValue;

    for (int p = 0; p < sizex; ++p) {

      tempValue = SaxonProcessor::makeXdmValueFromRef(results[p]);

      arr.push_back(tempValue);
    }
    operator delete(results);
  }

  return arr;
}

XdmValue **XdmArray::values() {
  XdmValue **arr = nullptr;

  int sizex = arrayLength();
  if (sizex == 0 || value == SXN_UNSET) {
    return arr;
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  allocFn fn = operator new;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  long long *results = j_convertXdmArrayToArrayObject(thread, (void *)value, (void *)fn, &r);

  if (results == nullptr) {
    return nullptr;
  }

  arr = new XdmValue *[sizex];
  XdmValue *tempValue = nullptr;

  for (int p = 0; p < sizex; ++p) {

    tempValue = SaxonProcessor::makeXdmValueFromRef(results[p]);

    arr[p] = tempValue;
  }
  operator delete(results);
  return arr;
}
