#ifndef SAXONDEV_PHPUSERFUNCTIONHANDLER_H
#define SAXONDEV_PHPUSERFUNCTIONHANDLER_H
class XdmValue;

void * phpNativeCall(char * functionName, void * proc, int useNativeTypes, int numOfArguments,  XdmValue ** arguments);

#endif //SAXONDEV_PHPUSERFUNCTIONHANDLER_H
