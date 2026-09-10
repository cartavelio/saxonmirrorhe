#include "saxonc/UserFunctionHandler.h"
#include "saxonc/XdmValue.h"

void * userFunctionNativeCall(char * functionName, void * proc, void * func, int useNativeTypes, int numOfArguments,  XdmValue ** arguments){
    //TODO move this code to a C++ specific method called userFunctionNative
    XdmValue * result = nullptr;
    if (getenv("SAXONC_DEBUG_FLAG")) {
        fprintf(stdout, "We are in a c function name= %s\n", functionName);

        fprintf(stdout, "We are in a c function pointer= %p\n", func);
        fflush(stdout);
    }
    sxnExtensionFunction func1 = sxnExtensionFunction(func);

    result = func1(numOfArguments, arguments);   // call of the user function

    return (void *)result->getUnderlyingValue();

}
