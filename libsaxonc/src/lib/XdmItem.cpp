

#include "saxonc/XdmItem.h"

XdmItem::XdmItem() : XdmValue() {
  value = SXN_UNSET;
  stringValue = nullptr;
  itemToString = nullptr;
}

XdmItem::XdmItem(const XdmItem &other) : XdmValue(other) {
  value = j_copy_handles(SaxonProcessor::sxn_environ->thread, (void *)other.value);
  if(value == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
  xdmSize = 1;
  refCount = other.refCount;
  stringValue = other.stringValue;
  stringValue = other.stringValue;
  itemToString = other.itemToString;
}

XdmItem::XdmItem(int64_t objRef): XdmValue() {
  value = objRef;
  xdmSize = 1;
  refCount = 0;
  stringValue = nullptr;
  itemToString = nullptr;
}

bool XdmItem::isAtomic() {
    return XDM_ATOMIC_VALUE == j_getXdmObjectType(SaxonProcessor::sxn_environ->thread,
                                 (void *)value);
}

bool XdmItem::isNode() {
    return XDM_NODE == j_getXdmObjectType(SaxonProcessor::sxn_environ->thread,
                                                                           (void *)value);
}

bool XdmItem::isFunction() {
    return XDM_FUNCTION_ITEM == j_getXdmObjectType(SaxonProcessor::sxn_environ->thread,
                                                  (void *)value);
}

bool XdmItem::isMap() {
    return XDM_MAP == j_getXdmObjectType(SaxonProcessor::sxn_environ->thread,
                                                  (void *)value);
}

bool XdmItem::isArray() {
    return XDM_ARRAY == j_getXdmObjectType(SaxonProcessor::sxn_environ->thread,
                                                  (void *)value);
}

XdmItem *XdmItem::getHead() { return this; }

XdmItem *XdmItem::itemAt(int n) {
  if (n < 0 || n >= size()) {
    return nullptr;
  }
  return this;
}

int XdmItem::size() { return 1; }

int64_t XdmItem::getUnderlyingValue() {
  return value;
}

const char *XdmItem::getStringValue(const char *encoding) {
  const char *stringValuei = nullptr;

  if (value < 0) {
    return nullptr;
  }
  allocFn fn = operator new;
  stringValuei = getStringValueForXdmItem(SaxonProcessor::sxn_environ->thread,
                                          (void *)fn, (void *)value, (char *)encoding);
  if(stringValuei == nullptr && (bool)j_checkForException(SaxonProcessor::sxn_environ->thread)) {
    throw SaxonApiException();
  }
  return stringValuei;
}

const char *XdmItem::toString(const char *encoding) {
  const char *itemToStringi = nullptr;
  // if (itemToString == nullptr) {
  allocFn fn = operator new;
  itemToStringi = xdmItemToString(SaxonProcessor::sxn_environ->thread,
                                  (void *)fn, (void *)value, (char *)encoding);

  if(itemToStringi == nullptr && (bool)j_checkForException(SaxonProcessor::sxn_environ->thread)) {
    throw SaxonApiException();
  }
  return itemToStringi;
}


XDM_TYPE XdmItem::getType() { return XDM_ITEM; }

XdmItem::~XdmItem() {
  SaxonProcessor::attachCurrentThread();
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "~XdmItem destructor called:" << refCount
              << " ob ref=" << (this) << " value =" << ((long)value)
              << std::endl;
  }
  if (value > 0) {
    j_handles_destroy(SaxonProcessor::sxn_environ->thread, (void *)value);
  }
  value = SXN_UNSET;

}

void XdmItem::incrementRefCount() {
  refCount++;
#ifdef DEBUG
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "after refCount-inc-xdmItem=" << refCount
              << " ob ref=" << (this) << std::endl;
  }
#endif
}

void XdmItem::decrementRefCount() {
    if (refCount > 0)
        refCount--;
#ifdef DEBUG
    if (getenv("SAXONC_DEBUG_FLAG")) {
        std::cerr << "after refCount-dec-xdmItem=" << refCount
                  << " ob ref=" << (this) << " ref=" << ((long)value) << std::endl;
    }
#endif
}
