

#include "XdmNode.h"

#ifdef MEM_DEBUG
#define new new(__FILE__, __LINE__)
#endif

XdmNode::XdmNode(int64_t obj): XdmItem(obj), baseURI(nullptr), nodeName(nullptr),localName(nullptr), childCount(-1), children(nullptr),parent(nullptr), typedValue(nullptr), attrValues(nullptr), attrCount(-1), nodeKind(UNKNOWN){

}

XdmNode::XdmNode(XdmNode * p, int64_t obj, XDM_NODE_KIND kind): XdmItem(obj), baseURI(nullptr), nodeName(nullptr), localName(nullptr),  childCount(-1), children(nullptr), parent(p), typedValue(nullptr), attrValues(nullptr),  attrCount(-1), nodeKind(kind){}

bool XdmNode::isAtomic() {
	return false;
} 
    
    XDM_NODE_KIND XdmNode::getNodeKind(){
	if(nodeKind == UNKNOWN) {
	    int kvalue = (int) j_getNodeKind(SaxonProcessor::sxn_environ->thread, value);
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

	switch (kind) {
            case DOCUMENT:
            case TEXT:
            case COMMENT:
                return nullptr;
            case PROCESSING_INSTRUCTION:
            case NAMESPACE:
            case ELEMENT:
            case ATTRIBUTE:
                localName = j_getLocalName(SaxonProcessor::sxn_environ->thread, value);

                return localName;



            default:
                return nullptr;
        }

        

    }

    const char * XdmNode::getNodeName(){
	
	if(nodeName != nullptr) {
		return nodeName;
	}
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
                nodeName = j_getNodeName(SaxonProcessor::sxn_environ->thread, value);
               

            default:
                return nullptr;
        }
	

    }

    //TODO we still don't know if the result is an XdmAtomicValue, EmptySequence or XdmValue
    XdmValue * XdmNode::getTypedValue(){
    	if(typedValue == nullptr) {
            int64_t  result = j_getTypedValue(SaxonProcessor::sxn_environ->thread, value);
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
        const char * str = xdmNodeToString(SaxonProcessor::sxn_environ->thread, value);
        if(str == nullptr) {
            return nullptr;
        }
        return str;
        	
    }

    
    const char* XdmNode::getBaseUri(){

	    if(baseURI != nullptr) {
		    return baseURI;
	    }

        baseURI = getBaseURIForXdmNode(SaxonProcessor::sxn_environ->thread, value);
	    return baseURI;

    }
    
    




    XdmNode* XdmNode::getParent(){
	if(parent == nullptr) {
		int64_t nodeRef = getParentForXdmNode(SaxonProcessor::sxn_environ->thread, value);
			if(nodeObj) {
				parent = new XdmNode(nullptr, nodeRef, UNKNOWN);
				//parent->incrementRefCount();
				return parent;
			}
			return nullptr;
		}
	} else {
		return parent;
	}
	
    }
    
    const char* XdmNode::getAttributeValue(const char *str){

	if(str == nullptr) { return nullptr;}

		const char * stri = j_getAttributeValue(SaxonProcessor::sxn_environ->thread, value, (char)str);

		return stri;

    }

    XdmNode** XdmNode::getAttributeNodes(bool cached){
    if (cached && attrValues != nullptr) {
        return attrValues;
    }  else {

		jclass xdmUtilsClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/option/cpp/XdmUtils");
		jmethodID xmID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(xdmUtilsClass,"getAttributeNodes",
					"(Lnet/sf/saxon/s9api/XdmNode;)[Lnet/sf/saxon/s9api/XdmNode;");
		jobjectArray results = (jobjectArray)(SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(xdmUtilsClass, xmID, 
		value));
		if(results == nullptr) {
			return nullptr;
		}
		XdmNode ** attrValuesi = nullptr;
		int sizex = SaxonProcessor::sxn_environ->env->GetArrayLength(results);
		attrCount = sizex;
		if(sizex>0) {	
			attrValuesi =  new XdmNode*[sizex];
			XdmNode * tempNode =nullptr;
			for (int p=0; p < sizex; ++p){
				jobject resulti = SaxonProcessor::sxn_environ->env->GetObjectArrayElement(results, p);
				tempNode = new XdmNode(this, resulti, ATTRIBUTE);
				this->incrementRefCount();
				attrValuesi[p] = tempNode;
			}
		}
		if(cached) {
		    attrValues = attrValuesi;
		}
		return attrValuesi;
	} 

    }


    int XdmNode::getAttributeCount(){
	if(attrCount == -1) {
		jclass xdmUtilsClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/option/cpp/XdmUtils");
		jmethodID xmID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(xdmUtilsClass,"getAttributeCount",
					"(Lnet/sf/saxon/s9api/XdmNode;)I");
		
		if (!xmID) {
			std::cerr << "Error: SaxonDll." << "getAttributeCount"
				<< " not found\n" << std::endl;
			return 0;
		}
		jint result = (jlong)(SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(xdmUtilsClass, xmID,
		value));

		attrCount =(int)result;
	}
	return attrCount;
    }

    int XdmNode::getChildCount(){
	if(childCount == -1) {
		jclass xdmUtilsClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/option/cpp/XdmUtils");
		jmethodID xmID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(xdmUtilsClass,"getChildCount",
					"(Lnet/sf/saxon/s9api/XdmNode;)I");
		
		if (!xmID) {
			std::cerr << "Error: SaxonDll." << "getchildCount"
				<< " not found\n" << std::endl;
			return 0;
		}
		jint result = (jlong)(SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(xdmUtilsClass, xmID,
		value));

		childCount =(int)result;
	}
	return childCount;
    }
    
    XdmNode** XdmNode::getChildren(bool cached){

    if (cached && children != nullptr) {
        return children;
    }  else {

    XdmNode ** childreni = nullptr;

	//if(children == nullptr) {
		jclass xdmUtilsClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/option/cpp/XdmUtils");
		jmethodID xmID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(xdmUtilsClass,"getChildren",
					"(Lnet/sf/saxon/s9api/XdmNode;)[Lnet/sf/saxon/s9api/XdmNode;");
		
		if (!xmID) {
			std::cerr << "Error: SaxonDll." << "getchildren"
				<< " not found\n" << std::endl;
			return nullptr;
		}
		jobjectArray results = (jobjectArray)(SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(xdmUtilsClass, xmID, 
		value));
		/*std::cerr<<"xdmNode c++ - cp1"<<std::endl;
		if(!results) {
		            std::cerr<<"xdmNode c++ - cp2 - null found"<<std::endl;
		}  */
		int sizex = SaxonProcessor::sxn_environ->env->GetArrayLength(results);
		childCount = sizex;	
		childreni =  new XdmNode*[sizex];
		XdmNode * tempNode = nullptr;
		for (int p=0; p < sizex; ++p){
			jobject resulti = SaxonProcessor::sxn_environ->env->GetObjectArrayElement(results, p);
			tempNode = new XdmNode(this, resulti, UNKNOWN);
			//tempNode->incrementRefCount();
			childreni[p] = tempNode;
		}
		SaxonProcessor::sxn_environ->env->DeleteLocalRef(results);
	//}
    if (cached) {
        children = childreni;
    }
	return childreni;

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

		jclass xdmUtilsClass = lookForClass(SaxonProcessor::sxn_environ->env, "net/sf/saxon/option/cpp/XdmUtils");
		jmethodID xmID = (jmethodID) SaxonProcessor::sxn_environ->env->GetStaticMethodID(xdmUtilsClass,"getChild",
					"(Lnet/sf/saxon/s9api/XdmNode;I)Lnet/sf/saxon/s9api/XdmNode;");

		if (!xmID) {
			std::cerr << "Error: SaxonDll." << "getchild"
				<< " not found\n" << std::endl;
			return nullptr;
		}
		jobject results = SaxonProcessor::sxn_environ->env->CallStaticObjectMethod(xdmUtilsClass, xmID,
		value,i);

	    child = new XdmNode(this, results, UNKNOWN);


	    return child;

        }
    }
  
