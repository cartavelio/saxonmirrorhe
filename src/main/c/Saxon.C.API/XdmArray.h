////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2019 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef SAXON_XDM_ARRAY_h
#define SAXON_XDM_ARRAY_h


#include "XdmFunctionItem.h"
#include <string>

#include <stdlib.h>
#include <string.h>
#include <set>

class XdmArray : public XdmFunctionItem {

public:

    XdmArray();

    XdmArray(const XdmArray &d);


    virtual ~XdmArray(){
	//std::cerr<<"destructor called fpr XdmFunctionItem"<<std::endl;

    }

    XdmArray(jobject);

    int arrayLength() const;

    XdmValue* get(int n);

    XdmArray* put(int n, XdmValue * value);

    XdmArray* addMember(XdmValue* value);

    XdmArray* concat(XdmArray * value);

    std::list<XdmValue *> asList();

    /**
    * Utility method for working with Saxon/C on Python
    */
    XdmValue ** createXdmValueArray(int len){
	   return (new XdmValue*[len]);
    }


    XdmValue** values();

    int getArity(){return 1;}

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
		return XDM_ARRAY;
	}


private:


    std::string fname;
    int arity;

};




#endif
