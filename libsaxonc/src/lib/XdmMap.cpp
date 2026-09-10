#include "saxonc/XdmMap.h"
#include "saxonc/XdmAtomicValue.h"
#include <map>

#ifdef MEM_DEBUG
#define new new (__FILE__, __LINE__)
#endif

XdmMap::XdmMap() : XdmFunctionItem() {
  int64_t mapDataRef = j_create_mapData(SaxonProcessor::sxn_environ->thread);
  value  = j_makeXdmMap(SaxonProcessor::sxn_environ->thread, (void *)mapDataRef);
  j_handles_destroy(SaxonProcessor::sxn_environ->thread, (void *)mapDataRef);

}

XdmMap::XdmMap(const XdmMap &d): XdmFunctionItem(d) {
}

XdmMap::XdmMap(int64_t obj) : XdmFunctionItem(obj) { }

const char *XdmMap::toString(const char *encoding) {
  if (value == SXN_UNSET) {
    return nullptr;
  }
  allocFn fn = operator new;
  const char *str = xdmMapToString(SaxonProcessor::sxn_environ->thread,
                                   (void *)fn, (void *)value, (char *)encoding);

  if(str == nullptr && (bool)j_checkForException(SaxonProcessor::sxn_environ->thread)) {
    throw SaxonApiException();
  }

  return str;
}

int XdmMap::mapSize() {
  if (value == SXN_UNSET) {
    return 0;
  }
  return j_xdmMap_size(SaxonProcessor::sxn_environ->thread, (void *)value);
}

XdmValue *XdmMap::get(XdmAtomicValue *key) {
  if (key == nullptr || value == SXN_UNSET) {
    return nullptr;
  }
  int64_t result =
      j_xdmMap_get(SaxonProcessor::sxn_environ->thread, (void *)value,
                   (void *)key->getUnderlyingValue());

  if(result == SXN_EXCEPTION) {
    throw SaxonApiException();
  }

  return getXdmValueSubClass(result);
}

XdmValue *XdmMap::get(const char *key) {
  if (key == nullptr || value == SXN_UNSET) {
    return nullptr;
  }
  int64_t result = j_xdmMap_get_with_key_as_string(
      SaxonProcessor::sxn_environ->thread, (void *)value, (char *)key);

  if(result == SXN_EXCEPTION) {
    throw SaxonApiException();
  }

  return getXdmValueSubClass(result);
}

XdmValue *XdmMap::get(int key) {
  if (key < 0 || value == SXN_UNSET) {
    return nullptr;
  }
  int64_t result = j_xdmMap_get_with_key_as_int(
      SaxonProcessor::sxn_environ->thread, (void *)value, key);

  if(result == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
  return getXdmValueSubClass(result);
}

XdmValue *XdmMap::get(double key) {
  if (key < 0 || value == SXN_UNSET) {
    return nullptr;
  }
  int64_t result = j_xdmMap_get_with_key_as_double(
      SaxonProcessor::sxn_environ->thread, (void *)value, key);

  if(result == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
  return getXdmValueSubClass(result);
}

XdmValue *XdmMap::get(long key) {
  if (key < 0 || value == SXN_UNSET) {
    return nullptr;
  }
  int64_t result = j_xdmMap_get_with_key_as_long(
      SaxonProcessor::sxn_environ->thread, (void *)value, key);

  if(result == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
  return getXdmValueSubClass(result);
}

// TODO test this method
XdmMap *XdmMap::put(XdmAtomicValue *key, XdmValue *valuei) {
  //TODO if the value is unset we need to create it first
  if (key == nullptr || valuei == nullptr) {
    return nullptr;
  }
  int64_t result = j_xdmMap_put(
      SaxonProcessor::sxn_environ->thread, (void *)value,
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
  int64_t result =
      j_xdmMap_remove(SaxonProcessor::sxn_environ->thread, (void *)value,
                      (void *)key->getUnderlyingValue());

  if(result == SXN_EXCEPTION) {
    throw SaxonApiException();
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
  long long *results = j_xdmMap_keys(SaxonProcessor::sxn_environ->thread,
                                     (void *)fn, (void *)value);

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
  long long *results = j_xdmMap_keys(SaxonProcessor::sxn_environ->thread,
                                     (void *)fn, (void *)value);

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
  char result =
      j_xdmMap_isEmpty(SaxonProcessor::sxn_environ->thread, (void *)value);
  return result != 0;
}

bool XdmMap::containsKey(XdmAtomicValue *key) {
  if(value == SXN_UNSET) {
    return false;
  }
  char result =
      j_xdmMap_containsKey(SaxonProcessor::sxn_environ->thread, (void *)value,
                           (void *)key->getUnderlyingValue());
  return result != 0;
}

std::list<XdmValue *> XdmMap::valuesAsList() {
  std::list<XdmValue *> mylist;
  allocFn fn = operator new;
  int sizex = mapSize();
  long long *results = nullptr;

  if(sizex > 0 || value != SXN_UNSET) {

  	results = j_xdmMap_values(SaxonProcessor::sxn_environ->thread,
                                       (void *)fn, (void *)value);
  }

  if (results != nullptr) {

    XdmValue *tempValue = nullptr;
    for (int p = 0; p < sizex; ++p) {
      tempValue = getXdmValueSubClass(results[p]);
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
  long long *results = j_xdmMap_values(SaxonProcessor::sxn_environ->thread,
                                       (void *)fn, (void *)value);

  XdmValue **valuesi = nullptr;

  valuesi = new XdmValue *[size];
  XdmValue *tempValue = nullptr;
  for (int p = 0; p < size; ++p) {
    tempValue = getXdmValueSubClass(results[p]);
    valuesi[p] = tempValue;
  }
  delete results;
  return valuesi;
}
