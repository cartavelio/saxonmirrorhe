#include "saxonc/XdmValue.h"
#include "saxonc/XdmArray.h"
#include "saxonc/XdmAtomicValue.h"
#include "saxonc/XdmFunctionItem.h"
#include "saxonc/XdmItem.h"
#include "saxonc/XdmMap.h"
#include "saxonc/XdmNode.h"

#ifdef MEM_DEBUG
#define new new (__FILE__, __LINE__)
#endif

XdmValue::XdmValue(const XdmValue &other) {
  // SaxonProcessor *proc = other.proc; //TODO
  valueType = other.valueType;
  refCount = 0;
  xdmSize = other.xdmSize;
  if(other.jValues != SXN_UNSET) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    jValues = j_copy_handles(thread, (void *)other.jValues);
    if(jValues == SXN_EXCEPTION) {
      throw SaxonApiException();
    }
  } else {
    jValues = SXN_UNSET;
  }
  if(other.emptyValue != SXN_UNSET) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    emptyValue = j_copy_handles(thread, (void *)other.emptyValue);
    if(emptyValue == SXN_EXCEPTION) {
      throw SaxonApiException();
    }
  } else {
    emptyValue = SXN_UNSET;
  }
  toStringValue = nullptr; // No longer used
  // values.resize(0);
  values_cap = other.values_cap;
  relinquished_values = new char[values_cap];
  memset(relinquished_values, 0, sizeof(*relinquished_values) * values_cap);
  if (other.values) {
  	for (int i = 0; i < xdmSize; i++) {
    	addXdmItem(other.values[i]);
  	}
  } else {
    //values = new XdmItem *[values_cap];
    values = nullptr;

  }
}

const char *XdmValue::toString(const char *encoding) {
  int count = size();
  allocFn fn = operator new;
  char *toStringValuei = nullptr;
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);

  if (count == 0) {
    toStringValuei =
        xdmItemToString(thread, (void *)fn, nullptr, nullptr, &r);


  } else if (count == 1) {
    toStringValuei =
        xdmItemToString(thread, (void *)fn, (void *)(values[0]->getUnderlyingValue()), (char *) encoding, &r);
  } else {
    if (jValues == -1) {
      jValues = getUnderlyingValue();
    }

    toStringValuei = xdmValueArrayToString(thread,(void *)fn, (void *)jValues, (char *) encoding, &r);
  }

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id,r.line_number,r.num_of_errors);
  }
  return toStringValuei;
}

int XdmValue::size() { return xdmSize; }

XdmValue::XdmValue(int64_t valRef) {
  initialize();
  XdmItem *xdm_item = new XdmItem(valRef);
  values_cap = 1;
  values = new XdmItem *[values_cap];
  relinquished_values = new char[values_cap];
  relinquished_values[0] = 0;
  addXdmItem(xdm_item);
  jValues = SXN_UNSET;
  valueType = nullptr;
  toStringValue = nullptr;
}

XdmValue::XdmValue(int64_t val, bool arr) {
  initialize();
  allocFn fn = operator new;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  long long *results = makeArrayFromXdmValue2(
      thread, (void *)fn, (void *)val);
  if (results == nullptr) {
    return;
  }

  int xdmSizei = (int) results[0]; // first value represents the size of the results array
  values_cap = xdmSizei;
  values = new XdmItem *[values_cap];
  relinquished_values = new char[values_cap];
  memset(relinquished_values, 0, sizeof(*relinquished_values) * values_cap);
  int typeRef = 0;

  for (int p = 1; p <= xdmSizei; ++p) {
    typeRef = j_getXdmObjectType(thread, (void *)results[p]);
//#ifdef DEBUG
    if (getenv("SAXONC_DEBUG_FLAG")) {
      std::cerr << "C++ XdmValue - typeRef= " << typeRef << std::endl;
    }
//#endif
    if (typeRef == -2) {
      std::cerr << "Exception thrown while creating XdmValue from array"
                << std::endl;
      return;
    }
    if (typeRef == XDM_ATOMIC_VALUE) {

      addXdmItemFromUnderlyingValue(new XdmAtomicValue(results[p]));

    } else if (typeRef == XDM_NODE) {

      addXdmItemFromUnderlyingValue(new XdmNode(results[p]));

    } else if (typeRef == XDM_ARRAY) {
      addXdmItemFromUnderlyingValue(new XdmArray(results[p]));
    } else if (typeRef == XDM_MAP) {
      addXdmItemFromUnderlyingValue(new XdmMap(results[p]));

    } else if (typeRef == XDM_FUNCTION_ITEM) {
      addXdmItemFromUnderlyingValue(new XdmFunctionItem(results[p]));
    } else {
      throw SaxonApiException("Error creating XdmValue from array");
    }
  }
  operator delete(results);
}

void XdmValue::resetRelinquishedItems(){
#ifdef DEBUG
    if (getenv("SAXONC_DEBUG_FLAG")) {
        std::cerr << "resetRelinquishedItems called:" << " ob ref=" << (this)  << std::endl;
    }
#endif
    for (int i = 0; i < xdmSize; i++) {
        if(values[i] != nullptr) {
            if (relinquished_values[i] == (char) 1) {
                values[i]->decrementRefCount();
                if (values[i]->getRefCount() < 1) {
                    relinquished_values[i] = (char) 0;  //reset
                }
            } else {
                values[i]->decrementRefCount();
            }
        }
    }
}

XdmValue::~XdmValue() {
#ifdef DEBUG
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "~XdmValue destructor called:" << refCount
              << " ob ref=" << (this)  << std::endl;
  }
#endif

  if (xdmSize > 0 && values != nullptr) {
      if (getenv("SAXONC_DEBUG_FLAG")) {
        std::cerr << "xdmSize: " << xdmSize << std::endl;
      }

    for (int i = 0; i < xdmSize; i++) {
      if (relinquished_values[i] == (char)0 && values[i] != nullptr) {
        delete values[i];
        values[i] = nullptr;
      }
    }
    delete[] values;
    delete[] relinquished_values;
    values = nullptr;
    relinquished_values = nullptr;
    values_cap = 0;
  }
  if (valueType != nullptr) {
      if (getenv("SAXONC_DEBUG_FLAG")) {
        std::cerr << "deleting valueType" << std::endl;
      }
    delete valueType;
  }
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  if (jValues > 0) {
        if (getenv("SAXONC_DEBUG_FLAG")) {
          std::cerr << "XdmValue on jValues j_handles_destroy" << std::endl;
        }

    j_handles_destroy(thread, (void *)jValues);
    jValues = SXN_UNSET;
  }

  if (emptyValue != SXN_UNSET) {
    j_handles_destroy(thread, (void *)emptyValue);
    emptyValue = SXN_UNSET;
  }

  xdmSize = 0;

  /*if (toStringValue != nullptr) {
    delete[] toStringValue;
    toStringValue = nullptr;
  }   */
}

void XdmValue::addXdmItem(XdmItem *val) {
  if (val != nullptr) {
    int xdmItemIndex = addXdmItemToValue(val);
    relinquished_values[xdmItemIndex] = (char)1;
  }
}

int XdmValue::addXdmItemToValue(XdmItem *val) {
    if (values == nullptr) {
      values_cap = 1;
      values = new XdmItem *[values_cap];
      relinquished_values = new char[values_cap];
      relinquished_values[0] = 0;
    }

    if (xdmSize == values_cap) {
      // resizing values pointer array
      XdmItem **newValues = new XdmItem *[values_cap * 2];
      char *new_relinquish_val = new char[values_cap * 2];
      memset(new_relinquish_val, 0,
             sizeof(*new_relinquish_val) * values_cap * 2);
      for (int i = 0; i < values_cap; i++) {
        newValues[i] = values[i];
        new_relinquish_val[i] = relinquished_values[i];
      }
      delete[] values;
      delete[] relinquished_values;
      values = newValues;
      relinquished_values = new_relinquish_val;
      values_cap *= 2;
    }

    values[xdmSize] = val;
    int addedItemIndex = xdmSize;
    xdmSize++;
    if (jValues != SXN_UNSET || emptyValue != SXN_UNSET) {
      graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
      if (jValues != SXN_UNSET) {
        if (getenv("SAXONC_DEBUG_FLAG")) {
          std::cerr << "XdmValue j_handles_destroy" << std::endl;
        }
        j_handles_destroy(thread, (void *)jValues);
        jValues = SXN_UNSET;
      }
      if (emptyValue != SXN_UNSET) {
        j_handles_destroy(thread, (void *)emptyValue);
        emptyValue = SXN_UNSET;
      }
    }
    if (toStringValue != nullptr) {
      operator delete(toStringValue);
      toStringValue = nullptr;
    }
    return addedItemIndex;
}

void XdmValue::addXdmItemFromUnderlyingValue(XdmItem *val) {
    if (val != nullptr) {
        addXdmItemToValue(val);
    }
}

void XdmValue::addUnderlyingValue(int64_t val) {
  XdmItem *valuei = new XdmItem(val);
  valuei->incrementRefCount();
  if (xdmSize == values_cap) {
    // resizing values pointer array
    XdmItem **newValues = new XdmItem *[values_cap * 2];
    char *new_relinquish_val = new char[values_cap * 2];
    memset(new_relinquish_val, 0,
             sizeof(*new_relinquish_val) * values_cap * 2);
    for (int i = 0; i < values_cap; i++) {
      newValues[i] = values[i];
      new_relinquish_val[i] = relinquished_values[i];
    }
    delete values;
    delete [] relinquished_values;
    values = newValues;
    relinquished_values = new_relinquish_val;
    values_cap *= 2;
  }
  values[xdmSize] = valuei;
  xdmSize++;
  jValues = SXN_UNSET; // TODO clear Graalvm reference from ObjectHandles pool or block user add item if set via jValues
}

void XdmValue::incrementRefCount() {
  refCount++;
  /*if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "refCount-inc-xdmVal=" << refCount << " ob ref=" << (this)
              << std::endl;
  }*/
}

void XdmValue::incrementRefCountForRelinquishedValue(int i){
  if (values!= nullptr && xdmSize > 0 && relinquished_values[i] == (char)0){
    values[i]->incrementRefCount();
    /*if (getenv("SAXONC_DEBUG_FLAG")) {
      std::cerr << "refCount-inc-relinquished-xdmVal=" << refCount << " ob ref=" << (this)
                << std::endl;
    }*/
  }
}

void XdmValue::decrementRefCount() {
  if (refCount > 0)
    refCount--;
  /*if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "refCount-dec-xdmVal=" << refCount << " ob ref=" << (this)
              << std::endl;
  }*/

}

XdmItem *XdmValue::getHead() {
  if (xdmSize > 0) {
    relinquished_values[0] = (char)1;
    //TODO handle the case where values[0] is null because we have called getHead() before
    return values[0];
  } else {
    return nullptr;
  }
}

int64_t XdmValue::getUnderlyingValue() {

  if (jValues == SXN_UNSET) {
    int i;
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    int count = xdmSize;
    if (count == 0) {
        //Given that count =0 we return empty sequence
        if (emptyValue == SXN_UNSET) {
          emptyValue =  j_getXdmEmptySequence(thread);
        }
        return emptyValue;
    }

    if (count == 1) {
      if (getenv("SAXONC_DEBUG_FLAG")) {
        std::cerr << "XdmValue.getUnderlyingValue() - XdmValue ref-values="
          << ((long)values[0]->getUnderlyingValue()) << std::endl;
      }
      return values[0]->getUnderlyingValue();
    }

    if(values == nullptr) {
      return SXN_UNSET;
    }
    jValues = createProcessorDataWithCapacity(thread, count);

    for (i = 0; i < count; i++) {
      int64_t ivalue = values[i]->getUnderlyingValue();
      addProcessorValue(thread, (void *)jValues,
                        (void *)ivalue);
    }
  }
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "XdmValue.getUnderlyingValue() - XdmValue ref-jvalues="
      << ((long)jValues) << std::endl;
  }
  return jValues;
}

void XdmValue::releaseXdmValue() {
//  for (int i = 0; i < xdmSize; i++) {
//    if (values[i] != nullptr) {
//      delete values[i];
//    }
//  }
//
//  jValues = SXN_UNSET;
}

XdmItem *XdmValue::itemAt(int n) {
  if (values!= nullptr && n >= 0 && n < xdmSize) {
    relinquished_values[n] = (char)1;
    return values[n];
  }
  return nullptr;
}


XDM_TYPE XdmValue::getType() { return XDM_VALUE; }
