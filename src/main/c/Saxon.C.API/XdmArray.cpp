#include "XdmArray.h"

#include "XdmMap.h"

XdmArray::XdmArray() : XdmFunctionItem(), arrayLen(0) {
  int64_t processorDataRef = createProcessorData(SaxonProcessor::sxn_environ->thread);
  value = j_makeXdmArray(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef);

  j_handles_destroy(SaxonProcessor::sxn_environ->thread, (void *)processorDataRef);

}

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
    arrayLen = j_xdmArray_arrayLength(SaxonProcessor::sxn_environ->thread,
                                      (void *)value);
  }
  return arrayLen;
}

XdmValue *XdmArray::get(int n) {
  if (n < 0 || value == SXN_UNSET) {
    return nullptr;
  }
  int64_t result =
      j_xdmArray_get(SaxonProcessor::sxn_environ->thread, (void *)value, n);
  if(result == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
  return getXdmValueSubClass(result);
}

const char *XdmArray::toString(const char *encoding) {
  if (value == SXN_UNSET) {
    return nullptr;
  }
  allocFn fn = operator new;
  const char *str = j_xdmArray_toString(SaxonProcessor::sxn_environ->thread,
                                        (void *)fn, (void *)value);
  if(str == nullptr) {
    throw SaxonApiException();
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
  int64_t result =
      j_xdmArray_put(SaxonProcessor::sxn_environ->thread, (void *)value, n,
                     (void *)valuei->getUnderlyingValue());
  if(result == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
  auto *newValue = new XdmArray(result);
  return newValue;
}

XdmArray *XdmArray::addMember(XdmValue *valuei) {
  int64_t result =
      j_xdmArray_addMember(SaxonProcessor::sxn_environ->thread, (void *)value,
                           (void *)valuei->getUnderlyingValue());
  if(result == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
  auto *newValue = new XdmArray(result);
  return newValue;
}

XdmArray *XdmArray::concat(XdmArray *valuei) {
  int64_t result =
      j_xdmArray_concat(SaxonProcessor::sxn_environ->thread, (void *)value,
                        (void *)valuei->getUnderlyingValue());
  if(result == SXN_EXCEPTION) {
    throw SaxonApiException();
  }
  auto *newValue = new XdmArray(result);
  return newValue;
}

std::list<XdmValue *> XdmArray::asList() {
  std::list<XdmValue *> arr;
  int sizex = arrayLength();
  if (sizex == 0) {
    return arr;
  }
  allocFn fn = operator new;
  long long *results = j_convertXdmArrayToArrayObject(
      SaxonProcessor::sxn_environ->thread, (void *)value, (void *)fn);

  if (results) {

    XdmValue *tempValue;

    for (int p = 0; p < sizex; ++p) {

      int typeRef = j_getXdmObjectType(SaxonProcessor::sxn_environ->thread,
                                       (void *)results[p]);
      XdmItem *xdmItem = nullptr;
      if (typeRef == -2) {
        // exception = new SaxonApiException();
        return arr;
      }
      if (typeRef == 1) {
        tempValue = new XdmValue();
        xdmItem = new XdmAtomicValue(results[p]);
        tempValue->addXdmItem(xdmItem);

      } else if (typeRef == 2) {
        tempValue = new XdmValue();
        xdmItem = new XdmNode(results[p]);
        tempValue->addXdmItem(xdmItem);

      } else if (typeRef == 4) {
        tempValue = new XdmValue();
        xdmItem = new XdmMap(results[p]);
        tempValue->addXdmItem(xdmItem);

      } else if (typeRef == 3) {
        tempValue = new XdmValue();
        xdmItem = new XdmArray(results[p]);
        tempValue->addXdmItem(xdmItem);

      } else if (typeRef == 5) {
        tempValue = new XdmValue();
        xdmItem = new XdmFunctionItem(results[p]);
        tempValue->addXdmItem(xdmItem);
      } else if (typeRef == 6) {
        // Empty Sequence detected.
        // TODO Maybe we should create an EmptySequence object
        j_handles_destroy(SaxonProcessor::sxn_environ->thread,
                          (void *)results[p]);

      } else {
        tempValue = new XdmValue(results[p], true);
      }

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
  allocFn fn = operator new;
  long long *results = j_convertXdmArrayToArrayObject(
      SaxonProcessor::sxn_environ->thread, (void *)value, (void *)fn);

  if (results == nullptr) {
    return nullptr;
  }

  arr = new XdmValue *[sizex];
  XdmValue *tempValue = nullptr;

  for (int p = 0; p < sizex; ++p) {

    int typeRef = j_getXdmObjectType(SaxonProcessor::sxn_environ->thread,
                                     (void *)results[p]);
    XdmItem *xdmItem = nullptr;
    if (typeRef == -2) {
      // exception = new SaxonApiException();
      std::cerr << "XdmArray.values() failed with exception" << std::endl;

      SaxonApiException *exception = new SaxonApiException();
      std::cerr << "Exception message = " << exception->getMessage()
                << std::endl;
      delete exception;
      delete [] arr;
      return nullptr;
    }
    if (typeRef == 1) {
      tempValue = new XdmValue();
      xdmItem = new XdmAtomicValue(results[p]);
      tempValue->addXdmItem(xdmItem);

    } else if (typeRef == 2) {
      tempValue = new XdmValue();
      xdmItem = new XdmNode(results[p]);
      tempValue->addXdmItem(xdmItem);

    } else if (typeRef == 4) {
      tempValue = new XdmValue();
      xdmItem = new XdmMap(results[p]);
      tempValue->addXdmItem(xdmItem);

    } else if (typeRef == 3) {
      tempValue = new XdmValue();
      xdmItem = new XdmArray(results[p]);
      tempValue->addXdmItem(xdmItem);

    } else if (typeRef == 5) {
      tempValue = new XdmValue();
      xdmItem = new XdmFunctionItem(results[p]);
      tempValue->addXdmItem(xdmItem);
    } else if (typeRef == 6) {
      // Empty Sequence detected.
      // TODO Maybe we should create an EmptySequence object
      std::cerr << "XdmArray.values() failed - EmptySequence not handled"
                << std::endl;
      j_handles_destroy(SaxonProcessor::sxn_environ->thread,
                        (void *)results[p]);

    } else {
      tempValue = new XdmValue((int64_t)results[p], true);
    }

    arr[p] = tempValue;
  }
  operator delete(results);
  return arr;
}
