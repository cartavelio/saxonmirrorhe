#ifndef SAXONDEV_USERFUNCTIONHANDLER_H
#define SAXONDEV_USERFUNCTIONHANDLER_H
class XdmValue;

typedef XdmValue *(*sxnExtensionFunction)(int, XdmValue **);

void * userFunctionNativeCall(char * functionName, void * proc, void * func, int useNativeTypes, int numOfArguments,  XdmValue ** arguments);

#endif //SAXONDEV_USERFUNCTIONHANDLER_H
