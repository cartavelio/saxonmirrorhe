#!/bin/sh

#Build file for SaxonC on C++

library_dir="../@@libdir@@"

gcc @@gccopts@@ -std=c99 -fPIC -I../Saxon.C.API/graalvm  -c ../Saxon.C.API/SaxonCGlue.c -o SaxonCGlue.o $@

gcc @@gccopts@@ -std=c99 -fPIC -I../Saxon.C.API/graalvm  Transform.c  -o transform -ldl -lc SaxonCGlue.o -L$library_dir -lsaxon-@@edition@@c-@@VERSION@@ -L$library_dir -Wl,-rpath,@executable_path/$library_dir $@

gcc @@gccopts@@ -std=c99 -fPIC -I../Saxon.C.API/graalvm Query.c -o query -ldl -lc SaxonCGlue.o -L$library_dir -lsaxon-@@edition@@c-@@VERSION@@ -L$library_dir -Wl,-rpath,@executable_path/$library_dir $@

if [ -f Validate.c ]; then
    gcc @@gccopts@@ -std=c99 -fPIC -I../Saxon.C.API/graalvm Validate.c -o validate -ldl -lc SaxonCGlue.o -L$library_dir -lsaxon-@@edition@@c-@@VERSION@@ -L$library_dir -Wl,-rpath,@executable_path/$library_dir $@
fi

