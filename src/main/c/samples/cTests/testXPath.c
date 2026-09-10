#include "../../Saxon.C.API/SaxonCXPath.h"
#include <stdio.h>  /* defines FILENAME_MAX */
#ifdef WINDOWS
#include <direct.h>
#define GetCurrentDir _getcwd
#else
#include <unistd.h>
#define GetCurrentDir getcwd
#endif

int main()
{

    char cwd[FILENAME_MAX]; //create string buffer to hold path
    GetCurrentDir( cwd, FILENAME_MAX );
    printf("CWD = %s\n",cwd);
    int cap = 10;
    sxnc_parameter * parameters; /*!< array of paramaters used for the transformation as (string, value) pairs */
    parameters;
    int parLen=0, parCap;
    parCap = cap;
    sxnc_property * properties; /*!< array of properties used for the transformation as (string, string) pairs */
    properties;
    int propLen=0, propCap;
    propCap = cap;
    sxnc_environment * environi;
    sxnc_processor * processor;

    initSaxonc(&environi, &processor, &parameters, &properties, parCap, propCap);

    /*
     * Initialize Graalvm run-time.
     * The handle of loaded component is used to retrieve Invocation API.
     */

    create_graalvm_isolate(environi);

    c_createSaxonProcessor((environi->thread), processor, 0);

    const char *verCh = version(environi, processor);
    printf("XPath Tests\n\nSaxon version: %s \n", verCh);
    setProperty(&properties, &propLen, &propCap, "s", "../data/cat.xml");
    

    sxnc_value * result = evaluate(environi,&processor, NULL, "/out/person", 0, properties, 0, propLen);

 bool resultBool = effectiveBooleanValue(environi,&processor, NULL, "count(/out/person)>0", 0, properties, 0, propLen);
 
    if(!result) {
      printf("result is null");
    }else {
      printf("%s", getStringValue(environi, *result));
    }

   if(resultBool==1) {
      printf("Boolean result is as expected: true");
    }else {
      printf("Boolean result is incorrectly: false");
      const char * errorMessage = c_getErrorMessage(environi);
      if(errorMessage) {
        printf("Error message %s \n", errorMessage);
      }
    }
      fflush(stdout);   

    graal_tear_down (environi->thread);
    freeSaxonc(&environi, &processor, &parameters, &properties);
    return 0;

}
