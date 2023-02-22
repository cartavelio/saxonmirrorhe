

#include "XdmNode.h"

#ifdef MEM_DEBUG
#define new new(__FILE__, __LINE__)
#endif

XdmNode::XdmNode(int64_t obj): XdmItem(obj), baseURI(nullptr), nodeName(nullptr),localName(nullptr), childCount(-1), children(nullptr),parent(nullptr), typedValue(nullptr), attrValues(nullptr), attrCount(-1), nodeKind(UNKNOWN){

}

XdmNode::XdmNode(XdmNode * p, int64_t obj, XDM_NODE_KIND kind): XdmItem(obj), baseURI(nullptr), nodeName(nullptr), localName(nullptr),  childCount(-1), children(nullptr), parent(p), typedValue(nullptr), attrValues(nullptr),  attrCount(-1), nodeKind(kind){}


XdmNode::XdmNode(const XdmNode &other) {
    baseURI = other.baseURI;
    value = other.value;
    nodeName = other.nodeName;
    localName = other.localName;
    childCount = other.childCount;
    parent = other.parent;
    typedValue = other.typedValue;
    attrCount = other.attrCount;
    nodeKind = other.nodeKind;
}

bool XdmNode::isAtomic() {
	return false;
} 
    
    XDM_NODE_KIND XdmNode::getNodeKind(){
	if(nodeKind == UNKNOWN) {
	    int kvalue = (int) j_getNodeKind(SaxonProcessor::sxn_environ->thread, (void *)value);
		nodeKind = static_cast<XDM_NODE_KIND>(kvalue);
	} 
	return nodeKind;

    }

    XdmNode::~XdmNode() {
       // if(getRefCount() <= 1) {
       if(baseURI != nullptr) {
        delete baseURI;
        baseURI = nullptr;
       }
       if(nodeName != nullptr) {
         delete nodeName;
         nodeName = nullptr;
       }
       if(localName != nullptr) {
         delete localName;
         localName = nullptr;
       }

           /* if (children != nullptr) {

                for (int p = 0; p < childCount; ++p) {
                    delete children[p];

                }
                delete[] children;
            }
            if (attrCount > 0 && attrValues != nullptr) {
                for (int p = 0; p < attrCount; ++p) {
                    delete attrValues[p];

                }
                delete[] attrValues;
            }     */

        //}
    }

                                            
    const char * XdmNode::getLocalName(){
	if(localName != nullptr) {
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
                localName = j_getLocalName(SaxonProcessor::sxn_environ->thread, (void *)fn, (void *)value);

                return localName;



            default:
                return nullptr;
        }

        

    }

    const char * XdmNode::getNodeName(){
	std::cerr<<"DEBUG C++: cp0"<<std::endl;
        std::cerr<<"DEBUG C++: value="<<(long)value<<std::endl;
	if(nodeName != nullptr) {
		return nodeName;
	}
        std::cerr<<"DEBUG C++: cp01"<<std::endl;
	allocFn fn = operator new;
	XDM_NODE_KIND kind = getNodeKind();
        std::cerr<<"DEBUG C++: cp02"<<std::endl;
	switch (kind) {
            case DOCUMENT:
            case TEXT:
            case COMMENT:
                return nullptr;
            case PROCESSING_INSTRUCTION:
            case NAMESPACE:
            case ELEMENT:
            case ATTRIBUTE:
                std::cerr<<"DEBUG C++: cp03"<<std::endl;
                nodeName = j_getNodeName(SaxonProcessor::sxn_environ->thread, (void *)fn, (void *)value);
            std::cerr<<"DEBUG C++: cp04"<<std::endl;
                return nodeName;
            default:
                return nullptr;
        }
	

    }

    //TODO we still don't know if the result is an XdmAtomicValue, EmptySequence or XdmValue
    XdmValue * XdmNode::getTypedValue(){
    	if(typedValue == nullptr) {
            int64_t  result = j_getTypedValue(SaxonProcessor::sxn_environ->thread, (void *)value);
            if(result > 0) {
                typedValue = new XdmValue();
                typedValue->addUnderlyingValue(result);
                return typedValue;
            }
    			return nullptr;

    	} else {
    		return typedValue;
    	}


    }


XdmItem * XdmNode::getHead(){
return this;}

    const char * XdmNode::getStringValue(){
   		return XdmItem::getStringValue();
    }

    const char * XdmNode::toString(){
        allocFn fn = operator new;
        const char * str = xdmNodeToString(SaxonProcessor::sxn_environ->thread, (void *)fn,(void *)value);
        if(str == nullptr) {
            return nullptr;
        }
        return str;
        	
    }

    
    const char* XdmNode::getBaseUri(){

	    if(baseURI != nullptr) {
		    return baseURI;
	    }
        allocFn fn = operator new;
        baseURI = getBaseURIForXdmNode(SaxonProcessor::sxn_environ->thread, (void *)fn, (void *)value);
	    return baseURI;

    }
    
    




    XdmNode* XdmNode::getParent(){
	if(parent == nullptr) {
		int64_t nodeRef = getParentForXdmNode(SaxonProcessor::sxn_environ->thread, (void *)value);
			if(nodeRef > -1) {
				parent = new XdmNode(nullptr, nodeRef, UNKNOWN);
				//parent->incrementRefCount();
				return parent;
			}
			return nullptr;

	} else {
		return parent;
	}
	
    }
    
    const char* XdmNode::getAttributeValue(const char *str){

	if(str == nullptr) { return nullptr;}

		const char * stri = j_getAttributeValue(SaxonProcessor::sxn_environ->thread, (void *)value, (char *)str);

		return stri;

    }



    XdmNode** XdmNode::getAttributeNodes(bool cached){
    if (cached && attrValues != nullptr) {
        return attrValues;
    }  else {
        getAttributeCount();
        allocFn fn = operator new;
        long long * results = j_getAttributeNodes(SaxonProcessor::sxn_environ->thread, (void *)fn, (void *)value);


		if(attrCount == 0 ) {
			return nullptr;
		}
		XdmNode ** attrValuesi = nullptr;


		attrValuesi =  new XdmNode*[attrCount];
		XdmNode * tempNode =nullptr;
		for (int p=0; p < attrCount; ++p){
		    tempNode = new XdmNode(this, results[p], ATTRIBUTE);
		    //this->incrementRefCount();
		    attrValuesi[p] = tempNode;
		}

		if(cached) {
		    attrValues = attrValuesi;
		}
		return attrValuesi;
	} 

    }


    int XdmNode::getAttributeCount(){
	if(attrCount == -1) {

		attrCount = j_getAttributeCount(SaxonProcessor::sxn_environ->thread, (void *)value);
	}
	return attrCount;
    }

    int XdmNode::getChildCount(){
	if(childCount == -1) {
        std::cerr<<"getChildCount count="<<((int)childCount)<< " value="<<value<<std::endl;
		childCount = j_getChildCount(SaxonProcessor::sxn_environ->thread, (void *)value);
        std::cerr<<"getChildCount count="<<childCount<<std::endl;

	}
	return childCount;
    }
    
    XdmNode** XdmNode::getChildren(bool cached){
        std::cerr<<"getChildren start"<<std::endl;
        if (cached && children != nullptr) {
            return children;
        }   else {
            if(childCount < 0) {
                getChildCount();
            }

            if(childCount == 0) {
                return nullptr;
            }
            std::cerr<<"getChildren, value="<<value<<std::endl;
            allocFn fn = operator new;
            long long * childNodes = j_getChildren(SaxonProcessor::sxn_environ->thread, (void*) fn, (void *)value);
            if(childNodes == nullptr) {
                std::cerr<<"getChildren, null returned"<<std::endl;
                return nullptr;
            }
            std::cerr<<"getChildren, cp0 childCount="<<childCount<<std::endl;
        children =   new XdmNode*[childCount];
		for (int p=0; p < childCount; ++p){
            std::cerr<<"DEBUG C++ loop getChildren, cp1 child value="<<childNodes[p]<<std::endl;
            children[p] = new XdmNode(this, (int64_t)childNodes[p], UNKNOWN);
            std::cerr<<"getChildren cp2"<<std::endl;

		}
            operator delete (childNodes);
            std::cerr<<"getChildren, cp3"<<std::endl;
	    return children;

    }
}

    XdmNode* XdmNode::getChild(int i, bool cached) {

       if (cached ) {

            if(children == nullptr) {
                getChildren(cached);


            }

            if(i < childCount && children[i] != nullptr) {
                return children[i];


                } else {
                    return nullptr;

                }
       }  else {
        XdmNode * child = nullptr;

		int64_t childRef = j_getChild(SaxonProcessor::sxn_environ->thread,(void *)value, i);

	    child = new XdmNode(this,childRef, UNKNOWN);


	    return child;

        }
    }
  
