#!/bin/sh

jdkdir=../../Saxon.C.API/graalvm

gcc -std=c99 -m64 -I$jdkdir ../../Saxon.C.API/SaxonCGlue.c ../../Saxon.C.API/SaxonCProcessor.c ../../Saxon.C.API/SaxonCXPath.c  testXSLT.c -o testXSLT -ldl -lsaxon-@@edition@@c-@@VERSION@@  $1

gcc -std=c99 -m64 -I$jdkdir  ../../Saxon.C.API/SaxonCGlue.c ../../Saxon.C.API/SaxonCProcessor.c  testXQuery.c -o testXQuery -ldl -lsaxon-@@edition@@c-@@VERSION@@ $1

gcc -std=c99 -m64 -I$jdkdir ../../Saxon.C.API/SaxonCGlue.c ../../Saxon.C.API/SaxonCProcessor.c ../../Saxon.C.API/SaxonCXPath.c testXPath.c -o testXPath -ldl -lsaxon-@@edition@@c-@@VERSION@@ $1

