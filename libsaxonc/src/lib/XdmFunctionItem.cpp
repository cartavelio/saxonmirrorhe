
//

#include "saxonc/XdmFunctionItem.h"

#include "saxonc/XdmArray.h"
#include "saxonc/XdmMap.h"

#ifdef MEM_DEBUG
#define new new (__FILE__, __LINE__)
#endif

XdmFunctionItem::XdmFunctionItem() : XdmItem(), fname(nullptr), arity(-1) {}

XdmFunctionItem::XdmFunctionItem(const XdmFunctionItem &aVal) : XdmItem(aVal) {
  arity = aVal.arity;
  fname = aVal.fname;
}

XdmFunctionItem::XdmFunctionItem(int64_t obj)
    : XdmItem(obj), fname(nullptr), arity(-1) {}


const char *XdmFunctionItem::getName() {
  if (fname == nullptr) {
    allocFn fn = operator new;
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    fname = j_getFunctionName(thread, (void *)fn,
                              (void *)value);
    return fname;
  } else {
    return fname;
  }
}

int XdmFunctionItem::getArity() {
  if (arity == -1) {
    if (value <= 0) {
      return -1;
    }
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    arity = j_xdmFunctionItem_getArity(thread,
                                       (void *)value);
    return arity;
  } else {
    return arity;
  }
}

XdmFunctionItem *XdmFunctionItem::getSystemFunction(SaxonProcessor *processor,
                                                    const char *name,
                                                    int arity) {
  if (processor == nullptr || name == nullptr) {
    throw SaxonApiException(
        "Error in getSystemFunction. Please make sure processor and "
        "name are not nullptr.");
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t result =
      j_getSystemFunction(thread,
                          (void *)processor->procRef, (char *)name, arity);
  if (result >= 0) {
    XdmFunctionItem *functionItem = new XdmFunctionItem(result);
    return functionItem;
  }

  return nullptr;
}

const char *XdmFunctionItem::getStringValue(const char *encoding) {
  throw SaxonApiException("The string value of a function is not defined",
                          "FOTY0014", nullptr, -1);
}

XdmValue *XdmFunctionItem::call(SaxonProcessor *processor, XdmValue **arguments,
                                int argument_length) {
  if ((argument_length > 0 && arguments == nullptr) || processor == nullptr) {
    throw SaxonApiException(
        "Error in XdmFunctionItem.call.  nullptr arguments found.");
  }

  int64_t argumentJArrayRef =
      SaxonProcessor::createJArray(arguments, argument_length);
  if (argumentJArrayRef == SXN_UNSET && argument_length > 0) {
    throw SaxonApiException("Error in XdmFunctionItem.call when converting "
                            "arguments -   nullptr arguments found.\"");
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t results = j_xdmFunctionItem_call(
      thread, (void *)processor->procRef,
      (void *)value,
      (argumentJArrayRef == SXN_UNSET ? (void *)nullptr
                                      : (void *)argumentJArrayRef));
  if (argumentJArrayRef != SXN_UNSET) {
    j_handles_destroy(thread,(void *)argumentJArrayRef);
    argumentJArrayRef = -1;
  }
  if (results == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
  return SaxonProcessor::makeXdmValueFromRef(results);
}
