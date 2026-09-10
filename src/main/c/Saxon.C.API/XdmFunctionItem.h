////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2021 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef SAXON_XDMFUNCTIONITEM_h
#define SAXON_XDMFUNCTIONITEM_h


#include "XdmItem.h"
#include "XdmAtomicValue.h"
#include "XdmNode.h"
#include <string>

#include <stdlib.h>
#include <string.h>

/**
 * The class XdmFunctionItem represents a function item
 */
class XdmFunctionItem : public XdmItem {

public:

    //! Default constructor for XdmFunctionItem.
    /**
     * Create an empty XdmFunctionItem
     */
    XdmFunctionItem();

    //! XdmFunctionItem constructor created from a Java object - internal use
    /**
     * XdmFunctionItem constructor created from a JNI Java object. This is for internal use only
     * @param obj - The internal Java object for the XdmFunctionItem is wrapped here in the C++ XdmArray
     */
    XdmFunctionItem(jobject obj);

    //! Copy constructor.
    /**
     * Copy constructor for XdmFunctionItem
     */
    XdmFunctionItem(const XdmFunctionItem &d);


    //! Destructor.
    /**
     * Destructor for XdmFunctionItem
     */
    virtual ~XdmFunctionItem(){
        if(!fname.empty()) {
            fname.clear();
        }
    }


    /**
     * Get the name of the function
     *
     * @return the function name, as a QName, or null for an anonymous inline function item
     */
    const char* getName();

    /**
     * Get the arity of the function
     *
     * @return the arity of the function, that is, the number of arguments in the function's signature
     */
    virtual int getArity();

    //! Get a system function
    /**
     * Get a system function. This can be any function defined in XPath 3.1 functions and operators,
     * including functions in the math, map, and array namespaces. It can also be a Saxon extension
     * function, provided a licensed Processor is used.
     * @return the requested function, or null if there is no such function. Note that some functions
     * (those with particular context dependencies) may be unsuitable for dynamic calling.
     */
    static XdmFunctionItem * getSystemFunction(SaxonProcessor * processor, const char * name, int arity);



    /**
    * Utility method for working with SaxonC on Python
    */
    XdmValue ** createXdmValueArray(int len){
        return (new XdmValue*[len]);
    }


    /**
     * Call the function
     *
     * @param arguments the values to be supplied as arguments to the function. The "function
     *                  conversion rules" will be applied to convert the arguments to the required
     *                  type when necessary.
     * @return the result of calling the function
     */
    XdmValue * call(SaxonProcessor * processor, XdmValue ** arguments, int argument_length);

    /**
     * Check if this XdmItem is atomic
     * @return false
     */
    bool isAtomic(){
        return false;
    }

    /**
    * Get the type of the object
    */
    XDM_TYPE getType() {
        return XDM_FUNCTION_ITEM;
    }




private:

    std::string fname; /*!< The name of the function item */
    int arity;/*!< The arity of this function */

};




#endif
