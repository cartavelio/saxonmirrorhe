#include "SaxonCGlue2.h"

/*jobject cpp;
char * dllname;
char * resources_dir;
int jvmCreated = 0;
char * tempDllname =
#if defined (__linux__)
"/libsaxon@@release@@c.so";
#elif  defined (__APPLE__) && defined(__MACH__)
"/libsaxon@@release@@c.dylib";
#else
"\\libsaxon@@release@@c.dll";
#endif

char * tempResources_dir =
#ifdef __linux__
"/saxon-data";
#elif  defined (__APPLE__) && defined(__MACH__)
"/saxon-data";
#else
"\\saxon-data";
#endif

char * dllPath =
#if defined (__linux__)
"/usr/lib";
#elif  defined (__APPLE__) && defined(__MACH__)
"/usr/local/lib";
#else
"C:\\Program Files\\Saxonica\\Saxon@@RELEASE@@C @@VERSION@@";
#endif*/

/*

char * getResourceDirectory() {
	return resources_dir;
}

char * _getResourceDirectory() {
	return resources_dir;
}*/


/*
 * Load dll.
 */
int create_graalvm_isolate(sxnc_environment * env)
{


    if (graal_create_isolate(NULL, &((env)->isolate), &((env)->thread)) != 0) {
        fprintf(stderr, "initialization error\n");
        return 1;
    }
    return 0;
}






/*
 * Create the Java SaxonProcessor
 */
int64_t createSaxonProcessor2(graal_isolatethread_t *thread, int license)
{
    return createSaxonProcessor(thread, license);
}







/*
 * Callback to check for exceptions. When called it returns the exception as a string
 */
const char * checkForException(sxnc_environment *environii) {


    return 0;
}


/*
 * Clean up and destroy Java VM to release memory used.
 */
void graal_tear_down (graal_isolatethread_t *thread)
{

    //if (jvmCreated != 0) {
    graal_tear_down_isolate(thread);
    //	jvmCreated = 0;
//	}
}






/*
 * Get a property from list
 */
char* getProperty(sxnc_property * properties, int propLen, const char* namespacei, const char * name) {


    return 0;
}


/*
 * set a parameter
 */
void setParameter(sxnc_parameter **parameters, int * parLen, int * parCap, const char * namespacei, const char * name, int64_t value) {


}


/*
 * set a property
 */
void setProperty(sxnc_property ** properties, int *propLen, int *propCap, const char* name, const char* value) {

}

/*
 * clear parameter
 */
void clearSettings(sxnc_parameter **parameters, int *parLen, sxnc_property ** properties, int *propLen) {

}







