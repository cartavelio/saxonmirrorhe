#include "../../Saxon.C.API/SaxonCProcessor.h"
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
    int parLen, parCap;
     parLen = 0;
    parCap = cap;
    sxnc_property * properties; /*!< array of properties used for the transformation as (string, string) pairs */
    properties;
    int propLen, propCap;
    propCap = cap;
    propLen =0;
    sxnc_environment * environi;
    sxnc_processor * processor;


    initSaxonc(&environi, &processor, &parameters, &properties, parCap, propCap);

    /*
     * Initialize Graalvm run-time.
     * The handle of loaded component is used to retrieve Invocation API.
     */

    create_graalvm_isolate(environi);
    c_createSaxonProcessor((environi->thread), processor, 0);

    char * verCh  = getProductVariantAndVersion(environi, processor);
    printf("XQuery Tests\n\nSaxon version: %s \n", verCh);
    setProperty(&properties, &propLen, &propCap, "s", "../data/cat.xml");

    setProperty(&properties, &propLen, &propCap, "cwd", cwd);

    setProperty(&properties, &propLen, &propCap, "qs", "<out>{count(/out/person)}</out>");

    const char * result = executeQueryToString(environi, &processor, NULL, 0,properties,0,propLen);

    if(!result) {
      printf("result is null\n");
    }else {
      printf("%s",result);
    }
    fflush(stdout);

    graal_tear_down (environi->thread);
    freeSaxonc(&environi, &processor, &parameters, &properties);

    return 0;
}
