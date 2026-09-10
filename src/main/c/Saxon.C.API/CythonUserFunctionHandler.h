#include "Python.h"


#ifndef SAXONDEV_CYTHONUSERFUNCTIONHANDLER_H
#define SAXONDEV_CYTHONUSERFUNCTIONHANDLER_H
class XdmValue;

void * python_nativeFunctionCall(char * functionName, void * proc, void * func, int useNativeTypes, int numOfArguments,  XdmValue ** arguments);

#endif //SAXONDEV_CYTHONUSERFUNCTIONHANDLER_H
