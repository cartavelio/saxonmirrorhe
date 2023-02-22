
#include <stdio.h>  /* defines FILENAME_MAX */
#ifdef WINDOWS
#include <direct.h>
#define GetCurrentDir _getcwd
#else
#include <unistd.h>
#define GetCurrentDir getcwd
#endif

#include "../../Saxon.C.API/SaxonCProcessor.h"
/*#include "../../Saxon.C.API/SaxonCXPath.h" */
//#include "../../Saxon.C.API/SaxonCGlue.h"

int main()
{

    char cwd[FILENAME_MAX]; //create string buffer to hold path
    GetCurrentDir( cwd, FILENAME_MAX );
    printf("CWD = %s\n",cwd);
    int cap = 10;
    sxnc_parameter * parameters;
    parameters;
    int parLen=0, parCap;
    parCap = cap;
    sxnc_property * properties;
    properties;
    int propLen=0;
    parCap = cap;
    sxnc_environment * environi;
    sxnc_processor * processor;

    initSaxonc(&environi, &processor, &parameters, &properties, parCap, parCap);

     /* graal_isolate_t *isolate = NULL;
  graal_isolatethread_t *thread = NULL;

  if (graal_create_isolate(NULL, &isolate, &thread) != 0) {
    fprintf(stderr, "initialization error\n");
    return 1;
  }*/

    //environi  =  (sxnc_environment *)malloc(sizeof(sxnc_environment));
    create_graalvm_isolate(environi);


    c_createSaxonProcessor((environi->thread), processor, 0);

    char * verCh  = getProductVariantAndVersion(environi, processor);
    printf("XSLT Tests\n\nSaxon version: %s \n", verCh);

    const char *result = xsltApplyStylesheet(environi, processor, cwd, "../data/cat.xml","../data/test.xsl", 0 ,0, 0, 0);

    if(!result) {
      printf("result is null \n");
      const char * errorMessage = c_getErrorMessage(environi);
      if(errorMessage) {
        printf("Error message %s \n", errorMessage);
      }
    }else {
      printf("%s", result);
    }
      fflush(stdout);


    graal_tear_down (environi->thread);
    return 0;
}
