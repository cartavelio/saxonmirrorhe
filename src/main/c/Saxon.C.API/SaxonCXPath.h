#ifndef SAXONCXPATH_H 
#define SAXONCXPATH_H

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2022 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#include "SaxonCProcessor.h"


//===============================================================================================//



EXTERN_SAXONC


/*long long int j_makeStringValue(graal_isolatethread_t*, char * str);


long long int j_makeIntegerValue(graal_isolatethread_t*, int i);



long long int j_makeDoubleValue(graal_isolatethread_t*, double d);


long long int j_makeFloatValue(graal_isolatethread_t*, float);


long long int j_makeLongValue(graal_isolatethread_t*, long l);


long long int j_makeBooleanValue(graal_isolatethread_t*, bool b);

long long int j_makeQNameValue(graal_isolatethread_t*, char * itype, char *str);

char * getStringValueForXdmItem(graal_isolatethread_t*, int64_t itemRef);

char * getStringValueForXdmValue(graal_isolatethread_t*, int64_t valueRef);

char * xdmItemToString(graal_isolatethread_t*, int64_t valueRef);

char * xdmNodeToString(graal_isolatethread_t*, int64_t valueRef);

char * j_getPrimitiveTypeName(graal_isolatethread_t*, int64_t valueRef);

long j_getLongValue(graal_isolatethread_t*, int64_t valueRef);

int j_getHashCode(graal_isolatethread_t*, int64_t valueRef);

double j_getDoubleValue(graal_isolatethread_t*, int64_t valueRef);

double j_getBooleanValue(graal_isolatethread_t*, int64_t valueRef);

int j_getNodeKind(graal_isolatethread_t*, int64_t nodeRef);

char * j_getLocalName(graal_isolatethread_t*, int64_t nodeRef);

char * j_getNodeName(graal_isolatethread_t*, int64_t nodeRef);

long j_getTypedValue(graal_isolatethread_t*, int64_t nodeRef);

char * getBaseURIForXdmNode(graal_isolatethread_t*, int64_t nodeRef);

long getParentForXdmNode(graal_isolatethread_t*, int64_t nodeRef);

char * j_getAttributeValue(graal_isolatethread_t*, int64_t nodeRef, char * eqname); */




    /**
     * Create boxed Boolean value
     * @param val - boolean value
     */

    int64_t booleanValue(sxnc_environment* environi, bool);


    /**
     * Create an boxed Integer value
     * @param val - int value
     */

    int64_t integerValue(sxnc_environment *environi, int i);


    /**
     * Create an boxed Double value
     * @param val - double value
     */

    int64_t doubleValue(sxnc_environment* environi, double d);


    /**
     * Create an boxed Float value
     * @param val - float value
     */

    int64_t floatValue(sxnc_environment *environi, float f);


    /**
     * Create an boxed Long value
     * @param val - Long value
     */

    int64_t longValue(sxnc_environment *environi, long l);


    /**
     * Create an boxed String value
     * @param val - as char array value
     */

    int64_t getJavaStringValue(sxnc_environment *environi, const char *str);

    /**
     * A Constructor. Create a XdmValue based on the target type. Conversion is applied if possible
     * @param type - specify target type of the value  
     * @param val - Value to convert
     */

    int64_t xdmValueAsObj(sxnc_environment *environi, const char* type, const char* str);

    /**
     * A Constructor. Create a XdmValue based on the target type. Conversion is applied if possible
     * @param type - specify target type of the value  
     * @param val - Value to convert
     */

sxnc_value * xdmValue(sxnc_environment *environi, const char* type, const char* str);


    /**
     * Compile and evaluate an XPath expression, supplied as a character string, with properties and parameters required
     * by the XPath expression
     * @param cwd  - Current working directory
     * @param xpathStr  - A string containing the source text of the XPath expression
     * @param params - Parameters and properties names required by the XPath expression. This could contain the context node , source as string or file name, etc
     * @param values -  The values for the parameters and properties required by the XPath expression
    **/

sxnc_value * evaluate(sxnc_environment *environi, sxnc_processor ** proc, char * cwd, char * xpathStr, sxnc_parameter *parameters, sxnc_property * properties, int parLen, int propLen);


    /**
     * Evaluate the XPath expression, returning the effective boolean value of the result.
     *
     * @param cwd  - Current working directory
     * @param xpathStr  - A string containing the source text of the XPath expression
     * @param params - Parameters and properties names required by the XPath expression. This could contain the context node , source as string or file name, etc
     * @param values -  The values for the parameters and properties required by the XPath expression
     *
     *
     **/

bool effectiveBooleanValue(sxnc_environment* environi, sxnc_processor ** proc, char * cwd, char * xpathStr, sxnc_parameter *parameters, sxnc_property * properties, int parLen, int propLen);


    /**
     * Determine whether the item is an atomic value or a node
     *
     * @return true if the item is an atomic value, false if it is a node
     */

bool isAtomicValue(sxnc_environment *environi, sxnc_value value);


    /**
     * Get the number of items in the sequence
     *
     * @return the number of items in the value - always one
     */


int size(sxnc_environment *environi, sxnc_value val);


    /**
     * Get the n'th item in the value, counting from zero.
     *
     * @param n the item that is required, counting the first item in the sequence as item zero
     * @return the n'th item in the sequence making up the value, counting from zero
     */

sxnc_value * itemAt(sxnc_environment *environi, sxnc_value, int i);



int64_t getvalue(sxnc_environment *environi, sxnc_value);


   /**
     * Get the string value of the item. For a node, this gets the string value
     * of the node. For an atomic value, it has the same effect as casting the value
     * to a string. In all cases the result is the same as applying the XPath string()
     * function.
    **/

const char * getStringValue(sxnc_environment *environi, sxnc_value value);


    /** 
    *Get the integer value of the item. if the XdmItem is not castable to an integer then
     * @param sxnc_value - Value to convert to integer
     * @failure_value - If the Value is not an integer then we can specify the value to return. Default is zero.
     *
    **/

int getIntegerValue(sxnc_environment *environi, sxnc_value value, int failure_value);



bool getBooleanValue(sxnc_environment *environi, sxnc_value value);



long getLongValue(sxnc_environment *environi, sxnc_value value,  long failureVal);



float getFloatValue(sxnc_environment *environi, sxnc_value value, float failureVal);



double getDoubleValue(sxnc_environment *environi, sxnc_value value, double failureVal);


EXTERN_SAXONC_END



#endif 
