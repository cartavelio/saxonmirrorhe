cmake_minimum_required(VERSION 3.17)
project(cppTests)

set(CMAKE_CXX_STANDARD 14)

add_executable(cppTests testXPath.cpp
        testXQuery.cpp
        testXSLT.cpp
        testXSLT30.cpp
        testValidator.cpp
        cppExtensionFunction.cpp
        ../../Saxon.C.API/jni/unix/jni_md.h
        ../../Saxon.C.API/jni/jni.h
        ../../Saxon.C.API/SaxonApiException.cpp
        ../../Saxon.C.API/SaxonApiException.h
        ../../Saxon.C.API/SaxonCGlue.c
        ../../Saxon.C.API/SaxonCGlue.h
        ../../Saxon.C.API/SaxonCProcessor.c
        ../../Saxon.C.API/SaxonCProcessor.h
        ../../Saxon.C.API/SaxonCXPath.c
        ../../Saxon.C.API/SaxonCXPath.h
        ../../Saxon.C.API/SaxonProcessor.cpp
        ../../Saxon.C.API/SaxonProcessor.h
        ../../Saxon.C.API/SchemaValidator.cpp
        ../../Saxon.C.API/SchemaValidator.h
        ../../Saxon.C.API/XdmArray.h
        ../../Saxon.C.API/XdmAtomicValue.cpp
        ../../Saxon.C.API/XdmAtomicValue.h
        ../../Saxon.C.API/XdmFunctionItem.cpp
        ../../Saxon.C.API/XdmFunctionItem.h
        ../../Saxon.C.API/XdmArray.cpp
        ../../Saxon.C.API/XdmArray.h
        ../../Saxon.C.API/XdmItem.cpp
        ../../Saxon.C.API/XdmItem.h
        ../../Saxon.C.API/XdmMap.cpp
        ../../Saxon.C.API/XdmMap.h
        ../../Saxon.C.API/XdmNode.cpp
        ../../Saxon.C.API/XdmNode.h
        ../../Saxon.C.API/XdmValue.cpp
        ../../Saxon.C.API/XdmValue.h
        ../../Saxon.C.API/XPathProcessor.cpp
        ../../Saxon.C.API/XPathProcessor.h
        ../../Saxon.C.API/XQueryProcessor.cpp
        ../../Saxon.C.API/XQueryProcessor.h
        ../../Saxon.C.API/Xslt30Processor.cpp
        ../../Saxon.C.API/Xslt30Processor.h
        ../../Saxon.C.API/XsltExecutable.cpp
        ../../Saxon.C.API/XsltExecutable.h
        ../../Saxon.C.API/XsltProcessor.cpp
        ../../Saxon.C.API/XsltProcessor.h cppExtensionFunction.h CppTestUtils.h)