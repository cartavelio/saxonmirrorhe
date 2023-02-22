////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2020 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef SAXON_XDM_MAP_h
#define SAXON_XDM_MAP_h


#include "XdmFunctionItem.h"
#include "XdmArray.h"
#include <string>

#include <stdlib.h>
#include <string.h>
#include <map>
#include <set>

class XdmMap : public XdmFunctionItem {

public:

    XdmMap();

    XdmMap(const XdmMap &d);


    virtual ~XdmMap(){
	//std::cerr<<"destructor called for XdmMap"<<std::endl;

    }

    XdmMap(jobject);

    XdmMap(std::map<XdmAtomicValue *, XdmValue*> value);

    int mapSize();

    XdmValue * get(XdmAtomicValue* key);

    XdmValue * get(const char * key);

    XdmValue * get(int key);

    XdmValue * get(double key);

    XdmValue *  get(long key);

    XdmMap * put(XdmAtomicValue * key, XdmValue * value);

    XdmMap * remove(XdmAtomicValue* key);

    std::set<XdmAtomicValue*> keySet();

    XdmAtomicValue ** keys();

    //std::map<XdmAtomicValue*, XdmValue*> asMap();

    bool isEmpty();

    bool containsKey(XdmAtomicValue* key);

    std::list<XdmValue*> valuesAsList();

    XdmValue** values();

    /**
     * Determine whether the item is a function or some other type of item
     *
     * @return true if the item is a Xdm function item, false if it is an atomic value or a function (including maps and arrays)
     */
    bool isFunction(){
        return true;
    }

	/**
	* Get the type of the object
	*/
	XDM_TYPE getType() {
		return XDM_MAP;
	}


private:


    std::string fname;
    std::map<XdmAtomicValue *, XdmValue*> data;
    int arity, map_size;

};




#endif
