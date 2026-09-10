#include "saxonc/XdmMap.h"
#include "saxonc/XdmAtomicValue.h"
#include <map>

#ifdef MEM_DEBUG
#define new new (__FILE__, __LINE__)
#endif

XdmMap::XdmMap() : XdmFunctionItem() {}

XdmMap::XdmMap(const XdmMap &d): XdmFunctionItem(d) {
}

XdmMap::XdmMap(int64_t obj) : XdmFunctionItem(obj) { }


int64_t XdmMap::createEmptyUnderlyingXdmMap(graal_isolatethread_t *thread) {
  int64_t value  = j_makeXdmMap(thread, (void *)nullptr);
  return value;
}

const char *XdmMap::toString(const char *encoding) {

  allocFn fn = operator new;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  if (value == SXN_UNSET) {
    value = XdmMap::createEmptyUnderlyingXdmMap(thread);
  }

  const char *str = xdmMapToString(thread,
                                   (void *)fn, (void *)value, (char *)encoding, &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }

  return str;
}

int XdmMap::mapSize() {
  if (value == SXN_UNSET) {
    return 0;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  return j_xdmMap_size(thread, (void *)value);
}

XdmValue *XdmMap::get(XdmAtomicValue *key) {
  if (key == nullptr || value == SXN_UNSET) {
    return nullptr;
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t result =
      j_xdmMap_get(thread, (void *)value, (void *)key->getUnderlyingValue(), &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }

  return SaxonProcessor::makeXdmValueFromRef(result);
}

XdmValue *XdmMap::get(const char *key) {
  if (key == nullptr || value == SXN_UNSET) {
    return nullptr;
  }

  saxon_error_t r = SAXON_ERROR_DEFAULT;

  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t result = j_xdmMap_get_with_key_as_string(thread, (void *)value, (char *)key, &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }

  return SaxonProcessor::makeXdmValueFromRef(result);
}

XdmValue *XdmMap::get(int key) {
  if (key < 0 || value == SXN_UNSET) {
    return nullptr;
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t result = j_xdmMap_get_with_key_as_int(thread, (void *)value, key, &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return SaxonProcessor::makeXdmValueFromRef(result);
}

XdmValue *XdmMap::get(double key) {
  if (key < 0 || value == SXN_UNSET) {
    return nullptr;
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t result = j_xdmMap_get_with_key_as_double(thread, (void *)value, key, &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return SaxonProcessor::makeXdmValueFromRef(result);
}

XdmValue *XdmMap::get(long key) {
  if (key < 0 || value == SXN_UNSET) {
    return nullptr;
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t result = j_xdmMap_get_with_key_as_long(thread, (void *)value, key, &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return SaxonProcessor::makeXdmValueFromRef(result);
}

// TODO test this method
XdmMap *XdmMap::put(XdmAtomicValue *key, XdmValue *valuei) {
  //TODO if the value is unset we need to create it first
  if (key == nullptr || valuei == nullptr) {
    return nullptr;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (value == SXN_UNSET) {
    value = XdmMap::createEmptyUnderlyingXdmMap(thread);
  }
  int64_t result = j_xdmMap_put(thread, (void *)value,
      (void *)key->getUnderlyingValue(), (void *)valuei->getUnderlyingValue());


  if(result == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
  return new XdmMap(result);
}

XdmMap *XdmMap::remove(XdmAtomicValue *key) {
  if (key == nullptr || value == SXN_UNSET) {
    return nullptr;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (value == SXN_UNSET) {
    value = XdmMap::createEmptyUnderlyingXdmMap(thread);
  }
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  int64_t result =
      j_xdmMap_remove(thread, (void *)value,
                      (void *)key->getUnderlyingValue(), &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return new XdmMap(result);
}

std::set<XdmAtomicValue *> XdmMap::keySet() {
  std::set<XdmAtomicValue *> myset;
  allocFn fn = operator new;
  int size = mapSize();
  if (size == 0) {
    return myset;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  long long *results = j_xdmMap_keys(thread,(void *)fn, (void *)value);

  XdmAtomicValue *tempValue = nullptr;
  for (int p = 0; p < size; ++p) {
    tempValue = new XdmAtomicValue(results[p]);
    myset.insert(tempValue);
  }
  operator delete(results);

  return myset;
}

XdmAtomicValue **XdmMap::keys() {

  allocFn fn = operator new;
  int size = mapSize();
  if (size == 0) {
    return nullptr;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  long long *results = j_xdmMap_keys(thread,(void *)fn, (void *)value);

  XdmAtomicValue **valuesi = nullptr;

  valuesi = new XdmAtomicValue *[size];
  XdmAtomicValue *tempValue = nullptr;
  for (int p = 0; p < size; ++p) {
    tempValue = new XdmAtomicValue(results[p]);
    valuesi[p] = tempValue;
  }
  operator delete(results);
  return valuesi;
}

// std::map<XdmAtomicValue*, XdmValue*> XdmMap::asMap();

bool XdmMap::isEmpty() {
  if(value == SXN_UNSET) {
    return false;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  char result = j_xdmMap_isEmpty(thread, (void *)value);
  return result != 0;
}

bool XdmMap::containsKey(XdmAtomicValue *key) {
  if(key==nullptr && value == SXN_UNSET) {
    return false;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  char result =
      j_xdmMap_containsKey(thread, (void *)value, (void *)key->getUnderlyingValue());
  return result != 0;
}

std::list<XdmValue *> XdmMap::valuesAsList() {
  std::list<XdmValue *> mylist;
  allocFn fn = operator new;
  int sizex = mapSize();
  long long *results = nullptr;

  if(sizex > 0 || value != SXN_UNSET) {
    saxon_error_t r = SAXON_ERROR_DEFAULT;
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  	results = j_xdmMap_values(thread,(void *)fn, (void *)value, &r);
  }

  if (results != nullptr) {

    XdmValue *tempValue = nullptr;
    for (int p = 0; p < sizex; ++p) {
      tempValue = SaxonProcessor::makeXdmValueFromRef(results[p]);
      mylist.push_back(tempValue);
    }
    delete results;
  }

  return mylist;
}

XdmValue **XdmMap::values() {
  if(value == SXN_UNSET) {
    return nullptr;
  }

  allocFn fn = operator new;
  int size = mapSize();
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  long long *results = j_xdmMap_values(thread,
                                       (void *)fn, (void *)value, &r);

  XdmValue **valuesi = nullptr;

  valuesi = new XdmValue *[size];
  XdmValue *tempValue = nullptr;
  for (int p = 0; p < size; ++p) {
    tempValue = SaxonProcessor::makeXdmValueFromRef(results[p]);
    valuesi[p] = tempValue;
  }
  delete results;
  return valuesi;
}
