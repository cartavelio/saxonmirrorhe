////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) 2022 Saxonica Limited.
// This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
// If a copy of the MPL was not distributed with this file, You can obtain one at http://mozilla.org/MPL/2.0/.
// This Source Code Form is "Incompatible With Secondary Licenses", as defined by the Mozilla Public License, v. 2.0.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#if defined __linux__ || defined __APPLE__
    #include <stdlib.h>
    #include <string.h>
    #include <dlfcn.h>
    #include <stdio.h>  
    #define HANDLE void*
    #define LoadLibrary(x) dlopen(x, RTLD_LAZY)
//    #define FreeLibrary(x) dlclose(x, RTLD_LAZY)
    #define GetProcAddress(x,y) dlsym(x,y)
#else
    #include <windows.h>
#endif

#include "../Saxon.C.API/SaxonCGlue.h"
//#include "../Saxon.C.API/SaxonApiException.h"

const char * failure;




int transform(sxnc_environment * environi, int argc, const char* argv[]) {



    if(argc < 2) {
        printf("\nError: Not enough arguments in Transform");
        return 0;
    }

    int64_t processorDataRef = createProcessorDataWithCapacity(environi->thread, argc);
    for (int i =1; i < argc; i++) {
        addProcessorProperty(environi->thread, (void *)processorDataRef, (char *)argv[i]);
    }

     j_run_transform(environi->thread, 0, (void *)processorDataRef);
	return 0;
}




int main( int argc, const char* argv[] )
{
    sxnc_environment * environi;
    environi  =  (sxnc_environment *)malloc(sizeof(sxnc_environment));
    create_graalvm_isolate(environi);
#ifdef DEBUG
    setDebugMode(environi->thread, 1);
#endif
    transform(environi, argc, argv);

  
    fflush(stdout);
    if (graal_detach_thread(environi->thread) != 0) {
        fprintf(stderr, "graal_detach_thread error\n");
        return -1;
    }

    return 0;
}
