////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2022 Saxonica Limited.
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

/**
 * An array in the XDM data model. An array is a list of zero or more members, each of which
 * is an arbitrary XDM value. The array itself is an XDM item.
 * <p>An XdmArray is immutable.</p>
 *
 */
class XdmArray : public XdmFunctionItem {

public:

    //! XdmArray default constructor.
    /**
     * Create an empty XdmArray
     */
    XdmArray();

    //! Copy constructor.
    /**
     * Copy constructor for XdmArray
     */
    XdmArray(const XdmArray &d);

    //! Destructor.
    /**
     * Destructor for XdmArray
     */
    virtual ~XdmArray(){
	    fname.clear();

    }

    //! XdmArray constructor which is used a wrapper class for the Java XdmArray object - internal use
    /**
     * @param obj - The internal Java object for the XdmArray is wrapped here in the C++ XdmArray
     */
    XdmArray(jobject obj);



    //! XdmArray constructor which is used a wrapper class for the Java XdmArray object - internal use
    /**
     * @param obj - The internal Java object for the XdmArray is wrapped here in the C++ XdmArray
     * @param len - The length of the array if known
     */
    XdmArray(jobject obj, int len);


    //! Get the number of members in the array
    /**
     *
     * @return the number of members in the array. (Note that the <code>size()</code> method returns 1 (one),
     * because an XDM array is an item.)
     */
    int arrayLength();

    /**
     * Get the n'th member in the array, counting from zero.
     *
     * @param n the member that is required, counting the first member in the array as member zero
     * @return the n'th member in the sequence making up the array, counting from zero
     * @remark  if n is less than zero or greater than or equal to the number
     *                                    of members in the array we return null. User should clear any exception thrown
     */
    XdmValue* get(int n);

    /**
     * Create a new array in which one member is replaced with a new value.
     *
     * @param n the position of the member that is to be replaced, counting the first member
     *          in the array as member zero
     * @param value - is the new member for the the new array
     * @return a new array, the same length as the original, with one member replaced
     * by a new value
     * @remark if n is less than zero or greater than or equal to the number
     *                                   of members in the array then return null
     */
    XdmArray* put(int n, XdmValue * value);

    /**
     * Append a new member to an array
     * @param value - the new member
     * @return a new array, one item longer than the original
     * @remark if the value is lazily evaluated, and evaluation fails then return null
     */
    XdmArray* addMember(XdmValue* value);

    /**
     * Concatenate another array
     *
     * @param value - the other array
     * @return a new array, containing the members of this array followed by the members of the
     * other array
     */
    XdmArray* concat(XdmArray * value);

    /**
     * Get the members of the array in the form of a list.
     * @return a std::list of the members of this array.
     */
    std::list<XdmValue *> asList();

    /**
    * Utility method for working with SaxonC on Python
     * Create an array of xdmValue objects
     * @param len - the length of the array to create
     * @return an empty array of XdmValue of length len
    */
    XdmValue ** createXdmValueArray(int len){
	   return (new XdmValue*[len]);
    }


    /**
     * Get the members of the array in the form of an array
     * @return an array of the members of this XdmArray.
     */
    XdmValue** values();

    /**
     * Get the arity of the function
     *
     * @return the arity of the function, that is, the number of arguments in the function's signature
     */
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

    //! Determine whether the item is an array or some other type of item
    /**
     *
     * @return true if the item is a Xdm array item, false if it is some other type
     */
    bool isArray(){
        return true;
    }


private:
    int arrayLen;



};




#endif
