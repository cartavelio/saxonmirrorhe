

#include "XdmItem.h"

XdmItem::XdmItem(): XdmValue(){
	value = nullptr;
}

    XdmItem::XdmItem(const XdmItem &other): XdmValue(other){
        value = other.value;
	    xdmSize =1;
	    refCount = other.refCount;
	    stringValue = other.stringValue;
    }


XdmItem::XdmItem(int64_t objRef){
	value = objRef;
	xdmSize =1;
	refCount =0;
}

bool XdmItem::isAtomic(){
	return false;
}



bool XdmItem::isNode(){
    return false;

}

bool XdmItem::isFunction(){
   return false;

}

bool XdmItem::isMap(){
   return false;

}

bool XdmItem::isArray(){
   return false;

}


XdmItem * XdmItem::getHead(){
return this;}

  XdmItem * XdmItem::itemAt(int n){
	if (n < 0 || n >= size()) {
		return nullptr;	
	}
	return this;
  }



 int XdmItem::size(){
	return 1;	
   }

int64_t XdmItem::getUnderlyingValue(){
#ifdef DEBUG
	std::cerr<<std::endl<<"XdmItem-getUnderlyingValue:"<<std::endl; 
#endif
	if(!value) {
#ifdef DEBUG
	     std::cerr<<std::endl<<"XdmItem-getUnderlyingValue - nullptr:"<<std::endl;
#endif
		return nullptr;	
	}
	return value;
}

    const char * XdmItem:: getStringValue(){
        if(stringValue.empty()) {

            const char * str = getStringValueForXdmItem(SaxonProcessor::sxn_environ->thread, value);
            if(str == nullptr) {
                return nullptr;
            }
    		stringValue = std::string(str);

    		return stringValue.c_str();

    	} else {
    		return stringValue.c_str();
    	}
   }


const char * XdmItem::toString(){
  const char * str = xdmItemToString(SaxonProcessor::sxn_environ->thread, value);
  if(str == nullptr) {
    return nullptr;
  }
  return str;

}

	/**
	* Get the type of the object
	*/
	XDM_TYPE XdmItem::getType(){
		return XDM_ITEM;
	}

XdmItem::~XdmItem() {
   value = -1;
   /*else {
        decrementRefCount();
    }*/
    if(stringValue.empty()) {
        stringValue.clear();
    }
}

void XdmItem::incrementRefCount() {
    refCount++;
   // std::cerr<<"after refCount-inc-xdmItem="<<refCount<<" ob ref="<<(this)<<std::endl;
}

void XdmItem::decrementRefCount() {
    if (refCount > 0)
        refCount--;
   // std::cerr<<"after refCount-dec-xdmItem="<<refCount<<" ob ref="<<(this)<<std::endl;
}
