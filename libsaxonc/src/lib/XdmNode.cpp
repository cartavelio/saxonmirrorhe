

#include "saxonc/XdmNode.h"

#ifdef MEM_DEBUG
#define new new (__FILE__, __LINE__)
#endif

XdmNode::XdmNode(int64_t obj)
    : XdmItem(obj), baseURI(nullptr), nodeName(nullptr), localName(nullptr),
      children(nullptr), childCount(-1), axisCount(0),
      attrValues(nullptr), attrCount(-1), nodeKind(UNKNOWN) {
}

XdmNode::XdmNode(XdmNode *p, int64_t obj, XDM_NODE_KIND kind)
    : XdmItem(obj), baseURI(nullptr), nodeName(nullptr), localName(nullptr),
      children(nullptr), childCount(-1), axisCount(0),
      attrValues(nullptr), attrCount(-1), nodeKind(kind) {}

XdmNode::XdmNode(const XdmNode &other): XdmItem(other) {
  baseURI = nullptr;
  nodeName = nullptr;
  localName = nullptr;
  childCount = other.childCount;
  attrCount = other.attrCount;
  nodeKind = other.nodeKind;
  axisCount = other.axisCount;
  //children = other.children;
}

bool XdmNode::operator==(const XdmNode& other) const
{
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  char result = j_xdmNodeEquals(SaxonProcessor::sxn_environ->thread, (void *)value, (void *)other.value, &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return result != 0;
}

bool XdmNode::equals(XdmNode * other) {
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  char result = j_xdmNodeEquals(SaxonProcessor::sxn_environ->thread, (void *)value, (void *)other->value, &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  return result != 0;
}

bool XdmNode::isAtomic() { return false; }

XDM_NODE_KIND XdmNode::getNodeKind() {
  if (nodeKind == UNKNOWN) {
    int kvalue =
        (int)j_getNodeKind(SaxonProcessor::sxn_environ->thread, (void *)value);
    nodeKind = static_cast<XDM_NODE_KIND>(kvalue);
  }
  return nodeKind;
}

XdmNode::~XdmNode() {
#ifdef DEBUG
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "~XdmNode destructor called:" << refCount
              << " ob ref=" << (this) << std::endl;
  }
#endif
  // if(getRefCount() <= 1) {
  if (baseURI != nullptr) {
    operator delete((char *)baseURI);
    baseURI = nullptr;
  }
  if (nodeName != nullptr) {
    operator delete((char *)nodeName);
    nodeName = nullptr;
  }
  if (localName != nullptr) {
    operator delete((char *)localName);
    localName = nullptr;
  }

    /*if (children != nullptr) {

        for (int p = 0; p < childCount; ++p) {
            if(children[p]->getRefCount() < 1) {
                delete children[p];
            }
        }
        delete[] children;
    }*/

  /*
   if (attrCount > 0 && attrValues != nullptr) {
       for (int p = 0; p < attrCount; ++p) {
           delete attrValues[p];

       }
       delete[] attrValues;
   }     */

  //}

}

void XdmNode::incrementRefCountForRelinquishedChildren() {
    if (childCount > 0) {
#ifdef DEBUG
        if (getenv("SAXONC_DEBUG_FLAG")) {
            std::cerr << "refCount-inc-relinquished-xdmNode=" << refCount << " ob ref=" << (this)
                      << std::endl;
        }
#endif
        for(int i=0; i<childCount; i++) {
            children[i]->incrementRefCount();
        }
    }
}

bool XdmNode::hasRelinquishedChildren(){
    return children != nullptr;
}

void XdmNode::resetRelinquishedChildren(){
    if (childCount > 0 && children != nullptr) {
#ifdef DEBUG
        if (getenv("SAXONC_DEBUG_FLAG")) {
            std::cerr << "XdmNode-resetRelinquishedChildren =" << refCount << " ob ref=" << (this)
                      << std::endl;
        }
#endif
        for(int i=0; i<childCount; i++) {
            if(children[i] != nullptr) {
                children[i]->decrementRefCount();
            }
        }
    }

}


const char *XdmNode::getLocalName() {
  if (localName != nullptr) {
    return localName;
  }
  XDM_NODE_KIND kind = getNodeKind();
  allocFn fn = operator new;
  switch (kind) {
  case DOCUMENT:
  case TEXT:
  case COMMENT:
    return nullptr;
  case PROCESSING_INSTRUCTION:
  case NAMESPACE:
  case ELEMENT:
  case ATTRIBUTE:
    localName = j_getLocalName(SaxonProcessor::sxn_environ->thread, (void *)fn,
                               (void *)value);

    return localName;

  default:
    return nullptr;
  }
}

const char *XdmNode::getNodeName() {
  if (nodeName != nullptr) {
    return nodeName;
  }
  allocFn fn = operator new;
  graal_isolatethread_t *thread = nullptr;
  XDM_NODE_KIND kind = getNodeKind();
  switch (kind) {
  case DOCUMENT:
  case TEXT:
  case COMMENT:
    return nullptr;
  case PROCESSING_INSTRUCTION:
  case NAMESPACE:
  case ELEMENT:
  case ATTRIBUTE:
      thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
      nodeName = j_getNodeName(thread, (void *)fn, (void *)value);
    return nodeName;
  default:
    return nullptr;
  }
}

// TODO we still don't know if the result is an XdmAtomicValue, EmptySequence or
// XdmValue
XdmValue *XdmNode::getTypedValue() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  return SaxonProcessor::makeXdmValueFromRef(j_getTypedValue(thread, (void *)value));
}

int XdmNode::getLineNumber(){
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    return j_getLineNumber1(thread, (void *)value);
}

int XdmNode::getColumnNumber() {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    return j_getColumnNumber1(thread, (void *)value);
}

XdmItem *XdmNode::getHead() { return this; }

const char *XdmNode::getStringValue(const char *encoding) { return XdmItem::getStringValue(encoding); }

const char *XdmNode::toString(const char *encoding) {
  const char *nodeToStringi = nullptr;
  // if (nodeToString == nullptr) {
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  allocFn fn = operator new;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  nodeToStringi = xdmNodeToString(thread,(void *)fn, (void *)value, (char *)encoding, &r);
  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }
  //}
  return nodeToStringi;
}

const char *XdmNode::getBaseUri() {
  if (baseURI != nullptr) {
    return baseURI;
  }
  allocFn fn = operator new;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  baseURI = getBaseURIForXdmNode(thread, (void *)fn, (void *)value);
  return baseURI;
}

XdmNode *XdmNode::getParent() {
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    int64_t nodeRef = getParentForXdmNode(thread, (void *)value);
    if (nodeRef > SXN_UNSET) {
      return new XdmNode(nullptr, nodeRef, UNKNOWN);
    }
    return nullptr;
}

const char *XdmNode::getAttributeValue(const char *str) {
  if (str == nullptr) {
    return nullptr;
  }
  allocFn fn = operator new;
  saxon_error_t r = SAXON_ERROR_DEFAULT;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  const char *stri =
      j_getAttributeValue(thread, (void *)value, (char *)str, (void *)fn, &r);

  if (r.status != SAXON_OK) {
    throw SaxonApiException(r.status, r.message, r.error_code, r.system_id, r.line_number,r.num_of_errors);
  }

  return stri;
}

XdmNode **XdmNode::getAttributeNodes(bool cached) {
  if (cached && attrValues != nullptr) {
    return attrValues;
  } else {
    getAttributeCount();
    allocFn fn = operator new;
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    long long *results = j_getAttributeNodes(thread, (void *)fn, (void *)value);

    if (attrCount == 0) {
      return nullptr;
    }
    XdmNode **attrValuesi = nullptr;

    attrValuesi = new XdmNode *[attrCount];
    XdmNode *tempNode = nullptr;
    for (int p = 0; p < attrCount; ++p) {
      tempNode = new XdmNode(this, results[p], ATTRIBUTE);
      attrValuesi[p] = tempNode;
    }

    if (cached) {
      attrValues = attrValuesi;
    }
    operator delete(results);
    return attrValuesi;
  }
}

int XdmNode::axisNodeCount() {return axisCount; }

XdmNode **XdmNode::axisNodes(EnumXdmAxis axis) {
  allocFn fn = operator new;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  long long *results = j_axisIterator(thread, (void *)fn, (void *)value, int(axis));
  if (results == nullptr) {
    axisCount = 0;
    return nullptr;
  }
  axisCount = (int) results[0];
  if (getenv("SAXONC_DEBUG_FLAG")) {
    std::cerr << "axisNodes count=" << axisCount << std::endl;
  }
  if (axisCount == 0) {
    return nullptr;
  }
  XdmNode **valuesi = nullptr;

  valuesi = new XdmNode *[axisCount];
  XdmNode *tempNode = nullptr;
  for (int p = 1; p < axisCount + 1; ++p) {
    tempNode = new XdmNode(this, results[p], UNKNOWN);
    valuesi[p - 1] = tempNode;
  }

  operator delete(results);

  return valuesi;
}

int XdmNode::getAttributeCount() {
  if (attrCount == -1) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    attrCount = j_getAttributeCount(thread, (void *)value);
  }
  return attrCount;
}

int XdmNode::getChildCount() {
  if (childCount == -1) {
    graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
    childCount = j_getChildCount(thread, (void *)value);
  }
  return childCount;
}

XdmNode **XdmNode::getChildren(bool cached) {
  if (childCount < 0) {
    getChildCount();
  }

  if (childCount == 0) {
    return nullptr;
  }

  allocFn fn = operator new;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  long long *childNodes = j_getChildren(thread, (void *)fn, (void *)value);
  if (childNodes == nullptr) {
    return nullptr;
  }
  XdmNode **fetchedChildren = new XdmNode *[childCount];
  for (int p = 0; p < childCount; ++p) {
    fetchedChildren[p] = new XdmNode(this, (int64_t)childNodes[p], UNKNOWN);
  }
  operator delete(childNodes);
  return fetchedChildren;
}

XdmNode *XdmNode::getChild(int i, bool cached) {
  if (i < 0) {
    return nullptr;
  }

  XdmNode *child = nullptr;
  graal_isolatethread_t *thread = SaxonProcessor::getOrAttachCurrentThread(SaxonProcessor::sxn_environ);
  int64_t childRef = j_getChild(thread, (void *)value, i);

  if (childRef == SXN_UNSET) {
    return nullptr;
  }

  child = new XdmNode(this, childRef, UNKNOWN);

  return child;
}
