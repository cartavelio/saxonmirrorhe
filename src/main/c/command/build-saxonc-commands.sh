#!/bin/sh

#Build file for SaxonC on C++

library_dir=${1-$(pwd -P)}/../@@libdir@@

gcc @@gccopts@@ -fPIC -I../Saxon.C.API/graalvm  -c ../Saxon.C.API/SaxonCGlue.c -o SaxonCGlue.o $1

gcc @@gccopts@@ -fPIC -I../Saxon.C.API/graalvm  Transform.c  -o transform -ldl -lc SaxonCGlue.o -L$library_dir -lsaxon-@@edition@@c-@@VERSION@@ $1

gcc @@gccopts@@ -fPIC -I../Saxon.C.API/graalvm Query.c -o query -ldl -lc SaxonCGlue.o -L$library_dir -lsaxon-@@edition@@c-@@VERSION@@ $1

gcc @@gccopts@@ -fPIC -I../Saxon.C.API/graalvm Gizmo.c -o gizmo -ldl -lc SaxonCGlue.o -L$library_dir -lsaxon-@@edition@@c-@@VERSION@@ $1

if [ -f Validate.c ]; then
    gcc @@gccopts@@ -fPIC -I../Saxon.C.API/graalvm Validate.c -o validate -ldl -lc SaxonCGlue.o -L$library_dir -lsaxon-@@edition@@c-@@VERSION@@ $1
fi

