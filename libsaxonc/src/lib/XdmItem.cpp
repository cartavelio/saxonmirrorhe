

#include "saxonc/XdmItem.h"

XdmItem::XdmItem() : XdmValue() {
  value = SXN_UNSET;
  stringValue = nullptr;
  itemToString = nullptr;
}

XdmItem::XdmItem(const XdmItem &other) : XdmValue(other) {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  value = j_copy_handles(thread, (void *)other.value);
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

bool XdmItem::isAtomic() { return false; }

bool XdmItem::isNode() { return false; }

bool XdmItem::isFunction() { return false; }

bool XdmItem::isMap() { return false; }

bool XdmItem::isArray() { return false; }

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
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  stringValuei = getStringValueForXdmItem(thread,
                                          (void *)fn, (void *)value, (char *)encoding, &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return stringValuei;
}

const char *XdmItem::toString(const char *encoding) {
  const char *itemToStringi = nullptr;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  allocFn fn = operator new;
  itemToStringi = xdmItemToString(thread,
                                  (void *)fn, (void *)value, (char *)encoding, &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return itemToStringi;
}


XDM_TYPE XdmItem::getType() { return XDM_ITEM; }

XdmItem::~XdmItem() {
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "~XdmItem destructor called:" << refCount
              << " ob ref=" << (this) << " value =" << ((long)value)
              << std::endl;
  }

  if (value > 0) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    j_handles_destroy(thread, (void *)value);
  }
  value = SXN_UNSET;

}

void XdmItem::incrementRefCount() {
  refCount++;
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "after refCount-inc-xdmItem=" << refCount
              << " ob ref=" << (this) << std::endl;
  }
}

void XdmItem::decrementRefCount() {
    if (refCount > 0)
        refCount--;
#ifdef DEBUG
    if (getenv("SAXONC_DEBUG_FLAG")) {
        std::cerr << "after refCount-dec-xdmItem=" << refCount
                  << " ob ref=" << (this) << std::endl;
    }
#endif
}
