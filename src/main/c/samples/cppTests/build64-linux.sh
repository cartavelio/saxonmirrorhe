#!/bin/sh

#Build file for Saxon/C on C++

#jdkdir=/usr/lib/jvm/java-7-oracle/include

# $jdkdir/bin/javac MyClassInDll.java

#jc =p MyDll.prj
#rm -rf MyDll_jetpdb
export JET_HOME=/usr/lib/rt
export PATH=$JET_HOME/bin:$PATH
export LD_LIBRARY_PATH=$JET_HOME/lib/amd64/shared:$LD_LIBRARY_PATH
export SAXON_ERRORS="-Wall -Werror -Wextra"


export TURN_ERRORS_TO_WARNINGS="-Wno-error=sizeof-pointer-memaccess -Wno-error=unused-value -Wno-error=extra -Wno-error=reorder -Wno-error=sign-compare -Wno-error=unused-variable -Wno-error=unused-parameter -Wno-error=return-type -Wno-error=unused-but-set-variable"

rm -r bin
mkdir -p bin

gcc -m64  -fPIC -c ../../Saxon.C.API/SaxonCGlue.c -o bin/SaxonCGlue.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

gcc -m64 -fPIC  -c ../../Saxon.C.API/SaxonCXPath.c -o bin/SaxonCXPath.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -fPIC -m64 -c cppExtensionFunction.cpp -o bin/cppExtensionFunction.o $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -m64  -fPIC -c ../../Saxon.C.API/XdmValue.cpp -o bin/XdmValue.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -m64 -fPIC -c ../../Saxon.C.API/XdmItem.cpp -o bin/XdmItem.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -m64 -fPIC -c ../../Saxon.C.API/XdmNode.cpp -o bin/XdmNode.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++ -std=c++14 -m64 -fPIC  -c ../../Saxon.C.API/XdmMap.cpp -o bin/XdmMap.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++ -std=c++14 -m64 -fPIC  -c ../../Saxon.C.API/XdmArray.cpp -o bin/XdmArray.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++ -std=c++14 -m64  -fPIC -c ../../Saxon.C.API/XdmFunctionItem.cpp -o bin/XdmFunctionItem.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -m64 -fPIC -c ../../Saxon.C.API/XdmAtomicValue.cpp -o bin/XdmAtomicValue.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -m64 -fPIC -c ../../Saxon.C.API/DocumentBuilder.cpp -o bin/DocumentBuilder.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -m64 -fPIC -c ../../Saxon.C.API/SaxonProcessor.cpp -o bin/SaxonProcessor.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++ -std=c++14 -m64 -fPIC  -c ../../Saxon.C.API/SaxonApiException.cpp -o bin/SaxonApiException.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -m64 -fPIC -c ../../Saxon.C.API/Xslt30Processor.cpp -o bin/Xslt30Processor.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++ -std=c++14 -m64  -fPIC -c ../../Saxon.C.API/XsltExecutable.cpp -o bin/XsltExecutable.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -m64 -fPIC -c ../../Saxon.C.API/XQueryProcessor.cpp -o bin/XQueryProcessor.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -m64 -fPIC -c ../../Saxon.C.API/XPathProcessor.cpp -o bin/XPathProcessor.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -m64 -fPIC -c ../../Saxon.C.API/SchemaValidator.cpp -o bin/SchemaValidator.o -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -m64 -fPIC -pthread bin/SaxonCGlue.o bin/SaxonCXPath.o bin/SaxonProcessor.o bin/XQueryProcessor.o bin/XsltExecutable.o bin/Xslt30Processor.o bin/XPathProcessor.o bin/DocumentBuilder.o bin/XdmValue.o bin/XdmItem.o bin/XdmNode.o bin/XdmAtomicValue.o bin/XdmArray.o bin/XdmMap.o bin/XdmFunctionItem.o bin/SchemaValidator.o bin/SaxonApiException.o bin/cppExtensionFunction.o testXSLT30.cpp -o testXSLT30 -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -m64 -fPIC bin/SaxonCGlue.o bin/SaxonCXPath.o bin/SaxonProcessor.o bin/XQueryProcessor.o bin/Xslt30Processor.o bin/XsltExecutable.o bin/XPathProcessor.o bin/DocumentBuilder.o bin/XdmValue.o bin/XdmItem.o bin/XdmNode.o bin/XdmAtomicValue.o bin/XdmArray.o bin/XdmFunctionItem.o bin/XdmMap.o bin/SchemaValidator.o bin/SaxonApiException.o testXQuery.cpp -o testXQuery -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2


g++  -std=c++14 -m64 -fPIC bin/SaxonCGlue.o bin/SaxonCXPath.o bin/SaxonProcessor.o bin/XQueryProcessor.o  bin/Xslt30Processor.o bin/XsltExecutable.o bin/XPathProcessor.o bin/DocumentBuilder.o bin/XdmValue.o bin/XdmItem.o bin/XdmNode.o bin/XdmAtomicValue.o bin/XdmArray.o bin/XdmFunctionItem.o bin/XdmMap.o bin/SchemaValidator.o bin/SaxonApiException.o testXPath.cpp -o testXPath -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -m64 -fPIC bin/SaxonCGlue.o bin/SaxonCXPath.o bin/SaxonProcessor.o bin/XQueryProcessor.o bin/Xslt30Processor.o bin/XsltExecutable.o bin/XPathProcessor.o bin/DocumentBuilder.o bin/XdmValue.o bin/XdmItem.o bin/XdmNode.o bin/XdmAtomicValue.o bin/XdmArray.o bin/XdmFunctionItem.o bin/XdmMap.o bin/SchemaValidator.o bin/SaxonApiException.o testValidator.cpp -o testValidator -ldl -lc $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2

g++  -std=c++14 -fPIC -m64 -shared -Wl,-soname,cppExtensionFunction.so -o  cppExtensionFunction.so bin/cppExtensionFunction.o $TURN_ERRORS_TO_WARNINGS $SAXON_ERRORS $1 $2
