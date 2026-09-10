////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2022 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef SAXONCGLUE2_H
#define SAXONCGLUE2_H



#include <libsaxon-eec-12.1.h>


#if defined __linux__ || defined __APPLE__
#include <stdlib.h>
#include <string.h>
#include <dlfcn.h>
#include <stdio.h>




#ifdef __cplusplus
#define EXTERN_SAXONC extern "C" {
#define EXTERN_SAXONC_END }
#else
#define EXTERN_SAXONC
#define EXTERN_SAXONC_END
#endif
#endif


#ifndef __cplusplus
#ifndef _BOOL
#include <stdbool.h>
/*typedef unsigned char sxn_bool;
static const sxn_bool sxn_false = 0;
static const sxn_bool sxn_true = 1;*/

#else
//typedef bool sxn_bool;
static const bool false = 0;
static const bool true = 1;
#endif
#endif



#ifndef __cplusplus
#if defined(LICENSE)
#define sxn_license 1
#else
#define sxn_license 0
#endif
#endif


//#define DEBUG


EXTERN_SAXONC


extern char * dllname;

extern char * resources_dir;

// Static variable used to track when jvm has been created. Used to prevent creation more than once.
extern int jvmCreated; //TODO this might not be required in graalvm - remove
//graal_isolate_t *isolate;
//graal_isolatethread_t *thread;

//===============================================================================================//
/*! <code>Environment</code>. This struct captures the Graalvm properties assocated with the cross compiled SaxonC library.
 * <p/>
 */
typedef struct {
    graal_isolate_t *isolate;
    graal_isolatethread_t *thread;
} sxnc_environment;


//===============================================================================================//

/*! <code>sxnc_parameter</code>. This struct captures details of paramaters used for the transformation as (string, value) pairs.
 * <p/>
 */
typedef struct {
    int64_t parameterDataRef; /*!< Internal use only. The value of the parameter points to the Java object. */

} sxnc_parameter;

//===============================================================================================//

/*! <code>sxnc_property</code>. This struct captures details of properties used for the transformation as (string, string) pairs.
 * <p/>
 */
typedef struct {
    int64_t parameterDataRef; /*!< Internal use only. The value of the parameter points to the Java object. */
} sxnc_property;

/*! <code>sxnc_value</code>. This struct is used to capture the xdmvalue and its type information.
 * <p/>
 */
typedef struct {
    int64_t xdmvalue; /*!< Internal use only. The value given as a Java object. */
} sxnc_value;

extern int64_t cpp;



extern const char * failure;


// Graalvm entrypoint methods
/*void vmLocatorSymbol(graal_isolatethread_t* thread);
long long int  createSaxonProcessor(graal_isolatethread_t*, int license);

long long int createProcessorData(graal_isolatethread_t*);

long long int createProcessorDataWithCapacity(graal_isolatethread_t*, int cap);

char * getProductVersion(graal_isolatethread_t*, int64_t processorRef);

long long int createDocumentBuilder(graal_isolatethread_t*);

void addProcessorDataPair(graal_isolatethread_t*, int64_t processorDataRef, char * name, int64_t valueRef);

void addProcessorPropertyPair(graal_isolatethread_t*, int64_t processorDataRef, char * name, char * dataRef);

void addProcessorValue(graal_isolatethread_t*,  int64_t processorDataRef, int64_t valueRef);

void setCatalogFile(graal_isolatethread_t*, char * filename); */



//void initSaxonc(sxnc_environment ** environi);


/*
* Get resources directory.
*/

char * getResourceDirectory();

/*
* Get resources directory. This is to replace getResourceDirectory().
*/

char * _getResourceDirectory();


/*
 * Load dll using the default setting in SaxonC
 * Recommended method to use to load library
 */
int create_graalvm_isolate(sxnc_environment * env);


int64_t createSaxonProcessor2(graal_isolatethread_t *thread, int license);


/*
 * Callback to check for exceptions. When called it returns the exception as a string
 */
const char * checkForException(sxnc_environment *environi);

/*
 * Clean up and destroy GraalVM to release memory used.
 */
void graal_tear_down (graal_isolatethread_t *thread);


/*
 * Get a parameter from list
 */
int64_t getParameter(sxnc_parameter *parameters,  int parLen, const char* namespacei, int64_t value);

/*
 * Get a property from list
 */
//char* getProperty(sxnc_property * properties, const char * name, const char* value);


/*
 * set a parameter
 */
//void setParameter(sxnc_parameter *parameters, const char * name, sxnc_value * value);


/*
 * set a property
 */
void setProperty(sxnc_property ** properties, int *propLen, int *propCap, const char* name, const char* value);

/*
 * clear parameter
 */
void clearSettings(sxnc_parameter **parameters, int *parLen, sxnc_property ** properties, int *propLen);





EXTERN_SAXONC_END


#endif //SAXONCGLUE_H
