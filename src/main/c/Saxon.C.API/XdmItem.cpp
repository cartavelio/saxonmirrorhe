

#include "XdmItem.h"

XdmItem::XdmItem(): XdmValue(){
    value = -1;
    stringValue = nullptr;
}

XdmItem::XdmItem(const XdmItem &other): XdmValue(other){
    value = other.value;
    xdmSize =1;
    refCount = other.refCount;
    stringValue = other.stringValue;
    stringValue = other.stringValue;
}


XdmItem::XdmItem(int64_t objRef){
    value = objRef;
    xdmSize =1;
    refCount =0;
    stringValue = nullptr;
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
        return -1;
    }
    return value;
}

const char * XdmItem:: getStringValue(){
    if(stringValue == nullptr) {
        if(value < 0) {
            return nullptr;
        }
        allocFn fn = operator new;
        stringValue = getStringValueForXdmItem(SaxonProcessor::sxn_environ->thread, (void *)fn, (void *)value);
        if(stringValue == nullptr) {
            return nullptr;
        }
        return stringValue;

    } else {
        return stringValue;
    }
}


const char * XdmItem::toString(){
    allocFn fn = operator new;
    const char * str = xdmItemToString(SaxonProcessor::sxn_environ->thread, (void *)fn, (void *)value);
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
    std::cerr<<"xdmITem destructor"<<std::endl;
    value = -1;
    /*else {
         decrementRefCount();
     }*/
    if(stringValue != nullptr) {
        std::cerr<<"xdmITem deleting string"<<std::endl;
        delete stringValue;
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
