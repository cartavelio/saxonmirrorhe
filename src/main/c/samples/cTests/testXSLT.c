#include "../../Saxon.C.API/SaxonCProcessor.h"
/*#include "../../Saxon.C.API/SaxonCXPath.h" */
#include "../../Saxon.C.API/SaxonCGlue2.h"

int main()
{
   /* HANDLE myDllHandle;
    jclass  myClassInDll;
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

    environi->myDllHandle = loadDefaultDll ();*/



     /* graal_isolate_t *isolate = NULL;
  graal_isolatethread_t *thread = NULL;

  if (graal_create_isolate(NULL, &isolate, &thread) != 0) {
    fprintf(stderr, "initialization error\n");
    return 1;
  }*/
    sxnc_environment * environi;
    environi  =  (sxnc_environment *)malloc(sizeof(sxnc_environment));
    create_graalvm_isolate(environi);


/*
    graal_isolate_t *isolate = NULL;
    graal_isolatethread_t *thread = NULL;
    sxnc_environment * environi;
    environi = malloc(sizeof(sxnc_environment));


    create_graalvm_isolate(environi);*/

    /*initJavaRT (environi->myDllHandle, &(environi->jvm), &(environi->env));
    const char *verCh = version(environi);
    printf("XSLT Tests\n\nSaxon version: %s \n", verCh);	
    jobject num = integerValue(environi, 5);
    setParameter(&parameters, &parLen, &parCap,"", "numParam", num);*/


    //const char *result = xsltApplyStylesheet(environi, &processor, NULL, "../data/cat.xml","../data/test.xsl", 0 ,0, 0, 0);
    int64_t procRef = createSaxonProcessor2((environi->thread), 0);
    printf("cp1\n");
    printf("%ld", (long)procRef);

    const char *result = xsltApplyStylesheet1(environi, procRef, "/home/ond1/work/repository/saxondev-11/src/main/c/samples/cTests", "../data/cat.xml","../data/test.xsl", 0 ,0, 0, 0);

    if(!result) {
      printf("result is null \n");
    }else {
      printf("%s", result);
    }
      fflush(stdout);

    /*
     * Finalize JET run-time.
     */
    /*finalizeJavaRT (environi->jvm);
    freeSaxonc(&environi, &processor, &parameters, &properties);*/

    graal_tear_down (environi->thread);
    return 0;
}
